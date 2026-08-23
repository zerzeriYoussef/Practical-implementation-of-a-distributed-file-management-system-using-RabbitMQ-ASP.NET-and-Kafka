using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvP.Application.DTOs;
using MvP.Application.Services.Teams;
using System.Security.Claims;

namespace MvP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TeamController : ControllerBase
{
    private readonly TeamService _teamService;

    public TeamController(TeamService teamService)
    {
        _teamService = teamService;
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
}
