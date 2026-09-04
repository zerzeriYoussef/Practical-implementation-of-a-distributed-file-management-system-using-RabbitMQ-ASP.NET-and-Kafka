using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvP.Application.DTOs;
using MvP.Application.Services.Storage;
using MvP.Application.Services.Teams;

namespace MvP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TeamController : ControllerBase
{
    private readonly TeamService _teamService;
    private readonly FileUploadService _fileUploadService;

    public TeamController(TeamService teamService, FileUploadService fileUploadService)
    {
        _teamService = teamService;
        _fileUploadService = fileUploadService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTeams()
    {
        var response = await _teamService.GetTeamsAsync();
        return Ok(response);
    }

    [HttpGet("{teamId:guid}/members")]
    public async Task<IActionResult> GetTeamMembers(Guid teamId)
    {
        var response = await _teamService.GetTeamMembersAsync(teamId);
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTeam([FromBody] CreateTeamRequest request)
    {
        var response = await _teamService.CreateTeamAsync(request);
        return Ok(response);
    }

    [HttpPost("join")]
    public async Task<IActionResult> JoinTeam([FromBody] JoinTeamRequest request)
    {
        var response = await _teamService.JoinTeamAsync(request);
        return Ok(response);
    }

    [HttpPost("{teamId:guid}/leave")]
    public async Task<IActionResult> LeaveTeam(Guid teamId)
    {
        await _teamService.LeaveTeamAsync(teamId);
        return NoContent();
    }

    [HttpDelete("{teamId:guid}/members/{userId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid teamId, Guid userId)
    {
        await _teamService.RemoveMemberAsync(teamId, userId);
        return NoContent();
    }

    [HttpPost("{teamId:guid}/files")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadFile(Guid teamId, IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is required.");
        }

        await using var stream = file.OpenReadStream();
        var response = await _fileUploadService.UploadTeamFileAsync(
            teamId,
            file.FileName,
            file.ContentType,
            file.Length,
            stream,
            cancellationToken);

        return Ok(response);
    }
}
