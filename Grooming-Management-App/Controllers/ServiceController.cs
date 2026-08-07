using Grooming_Management_App.DTOs.ServiceDTO;
using Grooming_Management_App.Enums;
using Grooming_Management_App.Services.CurrentUserServ;
using Grooming_Management_App.Services.ServiceServ;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grooming_Management_App.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Services")]
public class ServiceController(IServiceService service, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<List<GetServiceDto>> GetAllServices(ActiveStatusEnum? status, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        var services = await service.GetAllServicesAsync(salonId, status, ct);
        return services;
    }

    [HttpGet("{serviceId:int}")]
    [Authorize]
    public async Task<GetServiceDto> GetService(int serviceId , CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        var thatOneService = await service.GetServiceAsync(salonId, serviceId, ct);
        return thatOneService;
    }

    [HttpPut("{serviceId:int}/ActivateService")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> ActivateService(int serviceId, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        await service.ActivateServiceAsync(salonId, serviceId, ct);
        return NoContent();
    }
    [HttpPut("{serviceId:int}/DeactivateService")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> DeactivateService(int serviceId, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        await service.DeactivateServiceAsync(salonId, serviceId, ct);
        return NoContent();
    }

    [HttpPost]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> AddService(string newName, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        var newServiceId = await service.AddServiceAsync(salonId, newName, ct);
        return Created($"api/Service/{newServiceId}" ,null);
    }
    [HttpPut("{serviceId:int}/EditServiceName")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> EditNameService(int serviceId, string newName, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        await service.EditNameServiceAsync(salonId, serviceId, newName, ct);
        return NoContent();
    }
    
    
    
}
