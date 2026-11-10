using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ApiEcommerce.Constants;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;

namespace ApiEcommerce.Controllers.V2;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("2.0")]
[ApiController]
[EnableCors(PolicyNames.ALLOW_SPECIFIC_ORIGIN)]
[Authorize(Roles = "Admin")]
public class CategoryController : ControllerBase
{

    private readonly ICategoryRepository _categoryRepository;

    private readonly IMapper _mapper;

    public CategoryController(ICategoryRepository categoryRepository, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    [AllowAnonymous] //permite ver las categorias ignorando el autorize.
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    //[EnableCors(PolicyNames.ALLOW_SPECIFIC_ORIGIN)]
    public IActionResult GetCategoriesOrderById()
    {
        var categories = _categoryRepository.GetCategories().OrderBy(cat => cat.Id);

        var categoriesDto = new List<CategoryDto>();

        foreach (var category in categories)
        {
            categoriesDto.Add(_mapper.Map<CategoryDto>(category));
        }

        return Ok(categoriesDto);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}", Name = "GetCategory")]
    //[ResponseCache(Duration = 10)]
    [ResponseCache(CacheProfileName = CacheProfiles.DEFAULT_10)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetCategory(int id)
    {
        System.Console.WriteLine($"Category with ID {id} a las {DateTime.Now}");
        var category = _categoryRepository.GetCategory(id);
        System.Console.WriteLine($"Answer with ID {id}");

        if (category == null)
        {
            return NotFound($"Category with ID {id} not found.");
        }

        var categoryDto = _mapper.Map<CategoryDto>(category);

        return Ok(categoryDto);
    }

    
    [HttpPost()]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult CreateCategory([FromBody] CreateCategoryDto categoryDto)
    {
        if (categoryDto == null)
        {
            return BadRequest(ModelState);
        }

        if (_categoryRepository.CategoryExists(categoryDto.Name))
        {
            ModelState.AddModelError("CustomError", "Category already exists!");
            return StatusCode(400, ModelState);
        }

        var category = _mapper.Map<Category>(categoryDto);

        if(!_categoryRepository.CreateCategory(category))
        {
            ModelState.AddModelError("CustomError", $"Something went wrong when saving the record {category.Name}");
            return StatusCode(500, ModelState);
        }

        return CreatedAtRoute("GetCategory", new { id = category.Id }, category);
        //return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
    }

    [HttpPatch("{id:int}", Name = "UpdateCategory")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult UpdateCategory(int id, [FromBody] CreateCategoryDto updateCategoryDto)
    {

        if (!_categoryRepository.CategoryExists(id))
        {
            ModelState.AddModelError("CustomError", $"Category with ID {id} not found.");
            return NotFound($"Category with ID {id} not found.");
        }

        if (updateCategoryDto == null)
        {
            return BadRequest(ModelState);
        }

        if (_categoryRepository.CategoryExists(updateCategoryDto.Name))
        {
            ModelState.AddModelError("CustomError", "Category already exists!");
            return StatusCode(400, ModelState);
        }

        var category = _mapper.Map<Category>(updateCategoryDto);
        category.Id = id;

        if (!_categoryRepository.UpdateCategory(category))
        {
            ModelState.AddModelError("CustomError", $"Something went wrong when updating the record {category.Name}");
            return StatusCode(500, ModelState);
        }  

        return NoContent();
    }

    [HttpDelete("{id:int}", Name = "DeleteCategory")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult DeleteCategory(int id)
    {

        if (!_categoryRepository.CategoryExists(id))
        {
            return NotFound($"Category with ID {id} not found.");
        }

        var category = _categoryRepository.GetCategory(id);

        if (category == null)
        {
            return NotFound($"Category with ID {id} not found.");
        }

        if (!_categoryRepository.DeleteCategory(category))
        {
            ModelState.AddModelError("CustomError", $"Something went wrong when deleting the record {category.Name}");
            return StatusCode(500, ModelState);
        }

        return NoContent();
    }

}
