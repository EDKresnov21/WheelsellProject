using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wheelsell.BusinessLogic.DTOs.Lookups;
using Wheelsell.BusinessLogic.Services;

namespace Wheelsell.Control.Controllers;

[Route("api/lookups")]
public class LookupsController : ApiControllerBase
{
    private readonly ILookupService _lookupService;

    public LookupsController(ILookupService lookupService)
    {
        _lookupService = lookupService;
    }

    [HttpGet("brands")]
    public async Task<IActionResult> GetBrands() => Ok(await _lookupService.GetBrandsAsync());

    [HttpGet("models")]
    public async Task<IActionResult> GetModels([FromQuery] int? brandId) => Ok(await _lookupService.GetModelsAsync(brandId));

    [HttpGet("currencies")]
    public async Task<IActionResult> GetCurrencies() => Ok(await _lookupService.GetCurrenciesAsync());

    [HttpGet("feature-categories")]
    public async Task<IActionResult> GetFeatureCategories() => Ok(await _lookupService.GetFeatureCategoriesAsync());

    [Authorize(Policy = "ModeratorOrAdmin")]
    [HttpPost("brands")]
    public async Task<IActionResult> CreateBrand(CreateBrandRequest request)
    {
        var result = await _lookupService.CreateBrandAsync(request);
        return HandleResult(result);
    }

    [Authorize(Policy = "ModeratorOrAdmin")]
    [HttpDelete("brands/{id:int}")]
    public async Task<IActionResult> DeleteBrand(int id)
    {
        var result = await _lookupService.DeleteBrandAsync(id);
        return HandleResult(result);
    }

    [Authorize(Policy = "ModeratorOrAdmin")]
    [HttpPost("models")]
    public async Task<IActionResult> CreateModel(CreateCarModelRequest request)
    {
        var result = await _lookupService.CreateModelAsync(request);
        return HandleResult(result);
    }

    [Authorize(Policy = "ModeratorOrAdmin")]
    [HttpDelete("models/{id:int}")]
    public async Task<IActionResult> DeleteModel(int id)
    {
        var result = await _lookupService.DeleteModelAsync(id);
        return HandleResult(result);
    }

    [Authorize(Policy = "ModeratorOrAdmin")]
    [HttpPost("currencies")]
    public async Task<IActionResult> CreateCurrency(CreateCurrencyRequest request)
    {
        var result = await _lookupService.CreateCurrencyAsync(request);
        return HandleResult(result);
    }

    [Authorize(Policy = "ModeratorOrAdmin")]
    [HttpDelete("currencies/{id:int}")]
    public async Task<IActionResult> DeleteCurrency(int id)
    {
        var result = await _lookupService.DeleteCurrencyAsync(id);
        return HandleResult(result);
    }

    [Authorize(Policy = "ModeratorOrAdmin")]
    [HttpPost("feature-categories")]
    public async Task<IActionResult> CreateFeatureCategory(CreateFeatureCategoryRequest request)
    {
        var result = await _lookupService.CreateFeatureCategoryAsync(request);
        return HandleResult(result);
    }

    [Authorize(Policy = "ModeratorOrAdmin")]
    [HttpPost("features")]
    public async Task<IActionResult> CreateFeature(CreateFeatureRequest request)
    {
        var result = await _lookupService.CreateFeatureAsync(request);
        return HandleResult(result);
    }

    [Authorize(Policy = "ModeratorOrAdmin")]
    [HttpDelete("features/{id:int}")]
    public async Task<IActionResult> DeleteFeature(int id)
    {
        var result = await _lookupService.DeleteFeatureAsync(id);
        return HandleResult(result);
    }
}
