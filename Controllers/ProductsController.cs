using Microsoft.AspNetCore.Mvc;
using ApiEcommerce.Repository.IRepository;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Models;
using Mapster;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using ApiEcommerce.Models.Dtos.Responses;

namespace ApiEcommerce.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    //[ApiVersion("1.0")]
    //[ApiVersion("2.0")]
    [ApiVersionNeutral]
    [Authorize(Roles = "Admin")]  
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;

        private readonly ICategoryRepository _categoryRepository;

        public ProductsController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<IEnumerable<ProductDto>> GetProducts()
        {
            var products = _productRepository.GetAllProducts();;

            var productsDto = products.Adapt<List<ProductDto>>();

            return Ok(productsDto);
        }

        [AllowAnonymous]
        [HttpGet("{productId:int}", Name = "GetProduct")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult GetProduct(int productId)
        {
            var product = _productRepository.GetProduct(productId);

            if (product == null)
            {
                return NotFound($"Product with ID {productId} not found.");
            }

            var productDto = product.Adapt<ProductDto>();

            return Ok(productDto);
        }

        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult CreateProduct([FromForm] CreateProductDto createProductDto)
        {
            if (createProductDto == null)
            {
                return BadRequest(ModelState);
            }

            if (_productRepository.ProductExists(createProductDto.Name))
            {
                ModelState.AddModelError("CustomError", "Product already exists!");
                return StatusCode(400, ModelState);
            }

            //validate category foreing key
            if (!_categoryRepository.CategoryExists(createProductDto.CategoryId))
            {
              ModelState.AddModelError("CustomError", $"Category {createProductDto.CategoryId} doesn't exists");
              return BadRequest(ModelState);
            }

            var product = createProductDto.Adapt<Product>();

            //Add Image
            if(createProductDto.Image is not null)
            {
                UploadProductImage(createProductDto, product);
            }
            else
            {
                product.ImgUrl = "https://placehold.co/600x400";
            }

            if(!_productRepository.CreateProduct(product))
            {
                ModelState.AddModelError("CustomError", $"Something went wrong when saving the record {createProductDto.Name}");
                return StatusCode(500, ModelState);
            }

            var createdProduct = _productRepository.GetProduct(product.ProductId);   
            var productDto = createdProduct?.Adapt<ProductDto>();

            return CreatedAtRoute("GetProduct", new { productId = product.ProductId }, productDto);
        }

        [HttpGet("searchProductsByCategory/{categoryId:int}", Name = "GetProductsByCategory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult GetProductsByCategory(int categoryId)
        {

            if (!_categoryRepository.CategoryExists(categoryId))
            {
                return NotFound($"Category with ID {categoryId} doesn't exists");
            }

            var products = _productRepository.GetProductsByCategory(categoryId);

            if (products.Count <= 0)
            {
                return NotFound($"There are no products with Category {categoryId}");
            }

            //convert products to productDTO
            var productDto = products.Adapt<List<ProductDto>>();

            return Ok(productDto);
        }

        [HttpGet("searchProductsByNameDescription/{searchTerm}", Name = "GetProductsByNameDescription")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult GetProductsByNameDescription(string searchTerm)
        {

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest($"Name is empty");
            }

            var products = _productRepository.SearchProducts(searchTerm);

            if (products.Count <= 0)
            {
                return NotFound($"There are no products with the name {searchTerm}");
            }

            //convert products to productDTO
            var productDto = products.Adapt<List<ProductDto>>();

            return Ok(productDto);
        }


        [HttpPatch("buyProduct/{name}/{quantity:int}", Name = "BuyProduct")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult BuyProduct(string name, int quantity)
        {

            if (string.IsNullOrWhiteSpace(name) || quantity <= 0)
            {
                return BadRequest("The name or quantity is invalid");
            }

            var foundProduct = _productRepository.ProductExists(name);

            if (!foundProduct)
            {
                return NotFound($"The product with the name {name} doesn't exist");
            }

            if (!_productRepository.BuyProduct(name,quantity))
            {
                ModelState.AddModelError("CustomError", $"An error ocurred while purchasing the product, the quantity exceeds the amount in stock. Try it again.");
                return BadRequest(ModelState);
            }

             return Ok($"{quantity} { (quantity <= 1 ? " unit" : " units") }  of the product { (quantity <= 1 ? " was" : " were") }  purchased");
        }


        [HttpPut("/productId:int")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult UpdateProduct(int productId, [FromForm] UpdateProductDto updateProductDto)
        {
            if (updateProductDto == null)
            {
                return BadRequest(ModelState);
            }

            if (!_productRepository.ProductExists(productId))
            {
                ModelState.AddModelError("CustomError", "Product doesn't exist");
                return StatusCode(400, ModelState);
            }

            //validate category foreing key
            if (!_categoryRepository.CategoryExists(updateProductDto.CategoryId))
            {
              ModelState.AddModelError("CustomError", $"Category {updateProductDto.CategoryId} doesn't exists");
              return BadRequest(ModelState);
            }

            var product = updateProductDto.Adapt<Product>();
            product.ProductId = productId;
            

            //Add Image
            if(updateProductDto.Image is not null)
            {
                UploadProductImage(updateProductDto, product);
            }
            else
            {
                product.ImgUrl = "https://placehold.co/600x400";
            }


            if(!_productRepository.UpdateProduct(product))
            {
                ModelState.AddModelError("CustomError", $"Something went wrong when saving the record {product.Name}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }
              
        [HttpDelete("{productId:int}", Name = "DeleteProduct")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult DeleteProduct(int productId)
        {
            if (productId <= 0)
            {
                return BadRequest(ModelState);
            }

            if (!_productRepository.ProductExists(productId))
            {
                return NotFound();
            }

            var product = _productRepository.GetProduct(productId);

            if(product is null)
            {
                return NotFound($"The Product with ID { productId } doesn't exist");
            }

            var wasRemoved = _productRepository.DeleteProduct(product);

            if (!wasRemoved)
            {
                ModelState.AddModelError("CustomError", $"The product couldn't be deleted. Try it again.");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }


        [AllowAnonymous]
        [HttpGet("Paged", Name = "GetProductsInPage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult GetProductsInPage([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest("Paraneters of pagination are invalid");
            }

            var totalProducts = _productRepository.GetTotalProducts();
            var totalPages = (int) Math.Ceiling((double)totalProducts / pageSize);

            if (pageNumber > totalPages)
            {
                return NotFound("There aren't pages available.");
            }

            var products = _productRepository.GetProductsInPages(pageNumber, pageSize);
            var productDto = products.Adapt<List<ProductDto>>();

            var paginationResponse = new PaginationResponse<ProductDto>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
                Items = productDto
            };

            return Ok(paginationResponse);
        }



        private void UploadProductImage(dynamic productDto, Product product)
        {
            string fileName = product.ProductId + Guid.NewGuid().ToString() + Path.GetExtension(productDto.Image.FileName);
            var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwwroot", "ProductsImages");

            if (!Directory.Exists(imagesFolder))
            {
                Directory.CreateDirectory(imagesFolder);
            }

            var filePath = Path.Combine(imagesFolder, fileName);
            FileInfo file = new FileInfo(filePath);

            if (file.Exists)
            {
                file.Delete();
            }

            using var fileStream = new FileStream(filePath, FileMode.Create);
            productDto.Image.CopyTo(fileStream);
            var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
            product.ImgUrl = $"{baseUrl}/ProductsImages/{fileName}";
            product.ImgUrlLocal = filePath;
        }
    }
}