using GymFlow.Application.Modules.Members;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MembersController : ControllerBase
{
    private readonly IMemberService _memberService;

    public MembersController(IMemberService memberService)
    {
        _memberService = memberService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllMembers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] string? sortBy = "createdAt",
        [FromQuery] string? sortOrder = "desc")
    {
        var gymId = GetGymId();
        var result = await _memberService.GetAllAsync(gymId, page, pageSize, search, status, sortBy, sortOrder);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMemberById(Guid id)
    {
        var gymId = GetGymId();
        var member = await _memberService.GetByIdAsync(id, gymId);
        if (member == null) return NotFound();
        return Ok(member);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMember([FromBody] CreateMemberDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var gymId = GetGymId();
        var member = await _memberService.CreateAsync(dto, gymId);
        return CreatedAtAction(nameof(GetMemberById), new { id = member.Id }, member);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMember(Guid id, [FromBody] UpdateMemberDto dto)
    {
        var gymId = GetGymId();
        var member = await _memberService.UpdateAsync(id, dto, gymId);
        if (member == null) return NotFound();
        return Ok(member);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMember(Guid id)
    {
        var gymId = GetGymId();
        var success = await _memberService.DeleteAsync(id, gymId);
        if (!success) return NotFound();
        return NoContent();
    }

    [HttpPost("{id}/check-in")]
    public async Task<IActionResult> CheckIn(Guid id)
    {
        var gymId = GetGymId();
        await _memberService.CheckInAsync(id, gymId);
        return Ok(new { message = "Check-in recorded" });
    }

    [HttpGet("expiring")]
    public async Task<IActionResult> GetExpiringMembers([FromQuery] int days = 7)
    {
        var gymId = GetGymId();
        var members = await _memberService.GetExpiringAsync(gymId, days);
        return Ok(members);
    }

    private Guid GetGymId()
    {
        var gymIdClaim = User.FindFirst("gymId")?.Value;
        return gymIdClaim != null ? Guid.Parse(gymIdClaim) : Guid.Empty;
    }
}
