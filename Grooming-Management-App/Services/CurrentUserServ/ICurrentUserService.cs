using System.Security.Claims;
using Grooming_Management_App.Enums;

namespace Grooming_Management_App.Services.CurrentUserServ;

public interface ICurrentUserService
{
    int SalonId { get; }
    int UserId { get; }
    RoleEnum Role { get; }
}