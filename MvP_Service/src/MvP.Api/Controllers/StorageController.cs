using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvP.Application.DTOs;
using MvP.Application.Services.Storage;

namespace MvP.Api.Controllers;

[ApiController]
[Route("api/storage")]
[Authorize]
public class StorageController : ControllerBase
{
    private readonly StorageService _storageService;

    public StorageController(StorageService storageService)
    {
        _storageService = storageService;
    }

    [HttpPost("teams/{teamId:guid}/files")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadFile(Guid teamId, IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is required.");
        }

        await using var stream = file.OpenReadStream();
        var response = await _storageService.UploadTeamFileAsync(
            teamId,
            file.FileName,
            file.ContentType,
            file.Length,
            stream,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("teams/{teamId:guid}/files")]
    public async Task<IActionResult> GetFiles(Guid teamId, CancellationToken cancellationToken)
    {
        var response = await _storageService.GetTeamFilesAsync(teamId, cancellationToken);
        return Ok(response);
    }

    [HttpGet("teams/{teamId:guid}/files/{fileId:guid}")]
    public async Task<IActionResult> GetFile(Guid teamId, Guid fileId, CancellationToken cancellationToken)
    {
        var response = await _storageService.GetTeamFileAsync(teamId, fileId, cancellationToken);
        return Ok(response);
    }

    [HttpGet("teams/{teamId:guid}/files/{fileId:guid}/download")]
    public async Task<IActionResult> GetDownloadUrl(Guid teamId, Guid fileId, CancellationToken cancellationToken)
    {
        var response = await _storageService.GetTeamFileDownloadAsync(teamId, fileId, cancellationToken);
        return Ok(response);
    }

    [HttpPatch("teams/{teamId:guid}/files/{fileId:guid}")]
    public async Task<IActionResult> RenameFile(
        Guid teamId,
        Guid fileId,
        [FromBody] RenameStoredFileRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _storageService.RenameTeamFileAsync(
            teamId,
            fileId,
            request.FileName,
            cancellationToken);

        return Ok(response);
    }
}
