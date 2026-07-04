using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wheelsell.BusinessLogic.DTOs.Adverts;
using Wheelsell.BusinessLogic.DTOs.Common;
using Wheelsell.BusinessLogic.Services;
using Wheelsell.DataAccess.Enums;

namespace Wheelsell.Control.Controllers;

[Route("api/adverts")]
public class AdvertsController : ApiControllerBase
{
    private readonly IAdvertService _advertService;

    public AdvertsController(IAdvertService advertService)
    {
        _advertService = advertService;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] AdvertSearchRequest request)
    {
        var result = await _advertService.SearchAsync(request, CurrentUserIdOrNull);
        return HandleResult(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _advertService.GetByIdAsync(id, CurrentUserIdOrNull);
        return HandleResult(result);
    }

    [HttpGet("user/{sellerId:int}")]
    public async Task<IActionResult> GetUserAdverts(int sellerId, [FromQuery] AdvertStatus? status, [FromQuery] PagedRequest request)
    {
        var result = await _advertService.GetUserAdvertsAsync(sellerId, status, request, CurrentUserIdOrNull);
        return HandleResult(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(CreateAdvertRequest request)
    {
        var result = await _advertService.CreateAsync(CurrentUserId, request);
        return HandleResult(result);
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateAdvertRequest request)
    {
        var result = await _advertService.UpdateAsync(id, CurrentUserId, request);
        return HandleResult(result);
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _advertService.DeleteAsync(id, CurrentUserId);
        return HandleResult(result);
    }

    [Authorize]
    [HttpPost("{id:int}/off-sale")]
    public async Task<IActionResult> SetOffSale(int id)
    {
        var result = await _advertService.SetOffSaleAsync(id, CurrentUserId);
        return HandleResult(result);
    }

    [Authorize]
    [HttpPost("{id:int}/activate")]
    public async Task<IActionResult> SetActive(int id)
    {
        var result = await _advertService.SetActiveAsync(id, CurrentUserId);
        return HandleResult(result);
    }

    [Authorize]
    [HttpPost("{id:int}/mark-sold")]
    public async Task<IActionResult> MarkSold(int id, MarkAdvertSoldRequest request)
    {
        var result = await _advertService.MarkSoldAsync(id, CurrentUserId, request);
        return HandleResult(result);
    }

    [Authorize]
    [HttpPost("{id:int}/images")]
    public async Task<IActionResult> AddImages(int id, [FromForm] List<IFormFile> files)
    {
        var result = await _advertService.AddImagesAsync(id, CurrentUserId, files);
        return HandleResult(result);
    }

    [Authorize]
    [HttpPost("{id:int}/videos")]
    public async Task<IActionResult> AddVideos(int id, [FromForm] List<IFormFile> files)
    {
        var result = await _advertService.AddVideosAsync(id, CurrentUserId, files);
        return HandleResult(result);
    }

    [Authorize]
    [HttpDelete("{id:int}/images/{imageId:int}")]
    public async Task<IActionResult> DeleteImage(int id, int imageId)
    {
        var result = await _advertService.DeleteImageAsync(id, CurrentUserId, imageId);
        return HandleResult(result);
    }

    [Authorize]
    [HttpDelete("{id:int}/videos/{videoId:int}")]
    public async Task<IActionResult> DeleteVideo(int id, int videoId)
    {
        var result = await _advertService.DeleteVideoAsync(id, CurrentUserId, videoId);
        return HandleResult(result);
    }
}
