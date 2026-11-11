using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ApiEcommerce.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize(Roles = "Admin")]
    [ApiVersionNeutral]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        private readonly IMapper _mapper;

        public UserController(IUserRepository userRepository, IMapper mapper)
        {
            this._userRepository = userRepository;
            this._mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult GetUsers()
        {
            var users = _userRepository.GetUsers();

            var usersDto = _mapper.Map<List<UserDto>>(users);

            return Ok(usersDto);
        }


        [HttpGet("{id}", Name = "GetUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult GetUser(string id)
        {
            var user = _userRepository.GetUser(id);

            if (user is null)
            {
                return NotFound($"User with ID {id} not found");
            }

            var userDto = _mapper.Map<UserDto>(user);

            return Ok(userDto);
        }
        
        [AllowAnonymous]
        [HttpPost(Name = "RegisterUser")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegisterUser([FromBody] CreateUserDto createUserDto)
        {

            if(createUserDto is null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(createUserDto.Username))
            {
                return BadRequest("Username is required");
            }

            if (!_userRepository.IsUniqueUser(createUserDto.Username))
            {
                return BadRequest("User already exists.");
            }


            var result = await _userRepository.Register(createUserDto);

            if (result == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred when user was processed");
            }

            return CreatedAtRoute(
                routeName: "GetUser", 
                routeValues: new { id = result.Id }, 
                value: result
                );

        }


        [AllowAnonymous]
        [HttpPost("Login", Name = "LoginUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> LoginUser([FromBody] UserLoginDto userLoginDto)
        {

            if (userLoginDto is null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userRepository.Login(userLoginDto);

            if (user == null)
            {
                return Unauthorized();
            }

            return Ok(user);

        }
    }
}