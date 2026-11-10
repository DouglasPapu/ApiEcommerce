using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ApiEcommerce.Repository
{
    public class UserRepository : IUserRepository
    {

        private readonly ApplicationDbContext _db;

        private string? secretKey;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly IMapper _mapper;

        public UserRepository(ApplicationDbContext db, 
                              IConfiguration configuration, 
                              UserManager<ApplicationUser> userManager,
                              RoleManager<IdentityRole> roleManager,
                              IMapper mapper)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            secretKey = configuration.GetValue<string>("Apisettings:SecretKey");
        }

        public ApplicationUser? GetUser(string id)
        {
            return _db.ApplicationUser.FirstOrDefault<ApplicationUser>(p => p.Id == id);
        }

        public ICollection<ApplicationUser> GetUsers()
        {
           return _db.ApplicationUser.OrderBy(u => u.UserName).ToList();
        }

        public bool IsUniqueUser(string username)
        {
            return !_db.ApplicationUser.Any(u => u.UserName.ToLower().Trim() == username.ToLower().Trim());
        }

        public async Task<UserLoginResponseDto> Login(UserLoginDto userLoginDto)
        {
            if (string.IsNullOrEmpty(userLoginDto.Username))
            {
                return new UserLoginResponseDto()
                {
                  Token = "",
                  User =  null,
                  Message = "Username is required"
                };
            }

            var user = await _db.ApplicationUser.FirstOrDefaultAsync<ApplicationUser>(u => u.UserName != null && u.UserName.ToLower().Trim() == userLoginDto.Username.ToLower().Trim());

            if (user is null)
            {
                return new UserLoginResponseDto()
                {
                  Token = "",
                  User =  null,
                  Message = "Username not found"
                };
            }

            if (userLoginDto.Password is null)
            {
                return new UserLoginResponseDto()
                {
                  Token = "",
                  User =  null,
                  Message = "Password is required"
                };
            }   

            bool isValid = await _userManager.CheckPasswordAsync(user, userLoginDto.Password);

            if (!isValid)
            {
                return new UserLoginResponseDto()
                {
                  Token = "",
                  User =  null,
                  Message = "The credentials are invalid"
                };
            }   

            //JWT
            var handlerToken = new JwtSecurityTokenHandler();

            if (string.IsNullOrWhiteSpace(secretKey))
            {
                throw new InvalidOperationException("SecretKey is not configured");
            }


            var roles = await _userManager.GetRolesAsync(user);

            var key = Encoding.UTF8.GetBytes(secretKey);
            var tokenDescryptor = new SecurityTokenDescriptor()
            {
              Subject = new ClaimsIdentity(new[]
              {
                  new Claim("id", user.Id.ToString()),
                  new Claim("username", user.UserName ?? string.Empty),
                  new Claim(ClaimTypes.Role, roles.FirstOrDefault() ?? string.Empty)
              }),
              Expires = DateTime.UtcNow.AddHours(2),
              SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)  
            };

            var token = handlerToken.CreateToken(tokenDescryptor);

            return new UserLoginResponseDto()
            {
                Token = handlerToken.WriteToken(token),
                /*
                User = new UserRegisterDto()
                {
                    Username = user.Username,
                    Name = user.Name,
                    Role = user.Role,
                    Password = user.Password ?? ""
                },
                */
                User = _mapper.Map<UserDataDto>(user),
                Message = "User login sucessful"
            };
        
        }

        /*
        public async Task<User> Register(CreateUserDto createUserDto)
        {
            var encryptedPassword = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);

            User user = new User()
            {
                Username = createUserDto.Username ?? "No Username",
                Name = createUserDto.Name,
                Role = createUserDto.Role,
                Password = encryptedPassword
            };

            _db.Users.Add(user);

            await _db.SaveChangesAsync();
            return user;
        }
        */

        public async Task<UserDataDto> Register(CreateUserDto createUserDto)
        {
            if (string.IsNullOrEmpty(createUserDto.Username))
            {
                throw new ArgumentNullException("The Username is required");
            }

            if (createUserDto.Password is null)
            {
                throw new ArgumentNullException("The Password is required");
            }

             var user = new ApplicationUser()
             {
                 UserName = createUserDto.Username,
                 Email = createUserDto.Username,
                 NormalizedEmail = createUserDto.Username.ToUpper(),
                 Name = createUserDto.Name
             };

             var result = await _userManager.CreateAsync(user, createUserDto.Password);

            if (result.Succeeded)
            {
                var userRole = createUserDto.Role ?? "User";
                var roleExists = await _roleManager.RoleExistsAsync(userRole);

                if (!roleExists)
                {
                    var identityRole = new IdentityRole(userRole);
                    await _roleManager.CreateAsync(identityRole);
                }

                await _userManager.AddToRoleAsync(user, userRole);
                var createdUser = _db.ApplicationUser.FirstOrDefault(u => u.UserName == createUserDto.Username);

                return _mapper.Map<UserDataDto>(createdUser); 
            }
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new ApplicationException($"User couldn't be registered: { errors }");
        }


    }
}