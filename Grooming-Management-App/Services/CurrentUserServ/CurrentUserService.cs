using System.Security.Claims;
using Grooming_Management_App.Enums;

namespace Grooming_Management_App.Services.CurrentUserServ;

public class CurrentUserService(IHttpContextAccessor acs) : ICurrentUserService
{
    
    public int SalonId => int.Parse(acs.HttpContext.User.FindFirst("salonId").Value);
    public int UserId => int.Parse(acs.HttpContext.User.FindFirst("userId").Value);
    public RoleEnum Role => Enum.Parse<RoleEnum>(acs.HttpContext.User.FindFirst(ClaimTypes.Role).Value);
    
}