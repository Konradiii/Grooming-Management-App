
using Grooming_Management_App.DTOs.NextVisitDTO;
using Grooming_Management_App.Services.CurrentUserServ;
using Grooming_Management_App.Services.NextVisitServ;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grooming_Management_App.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Owner,Groomer")]
public class NextVisitController(
    INextVisitReaderService service,
    ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet("due")]
    public async Task<ActionResult<List<GetDueDogDto>>> GetDueDogs(CancellationToken ct)
    {
        var result = await service.GetDueDogsAsync(currentUser.SalonId, ct);
        return Ok(result);
    }
}