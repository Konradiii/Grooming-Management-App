using Grooming_Management_App.DataInfrastructure;
using Grooming_Management_App.Enums;
using Grooming_Management_App.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Grooming_Management_App.Middleware;

public class SubscriptionMiddleware(RequestDelegate next)
{

    public async Task InvokeAsync(HttpContext context, GroomingDbContext ctx)
    {

        if (context.Request.Method != HttpMethods.Post)
        {
            await next(context);
            return;
        }

        if (context.Request.Path.StartsWithSegments("/api/Auth"))
        {
            await next(context);
            return;        
        }


        if (context.User.Identity?.IsAuthenticated != true)
        {
            await next(context);
            return;
        }
        
        var salonIdClaim = context.User.FindFirst("salonId")?.Value;
        if (!int.TryParse(salonIdClaim, out var salonId))
        {
            await next(context);
            return;
        }
        
        var status = await ctx.Salons
            .Where(s => s.Id == salonId)
            .Select(s=>s.SubscriptionStatus)
            .FirstOrDefaultAsync();
        
        if (status == SubscriptionStatusEnum.Suspended)
        {
            context.Response.StatusCode = StatusCodes.Status402PaymentRequired;
            await context.Response.WriteAsJsonAsync(new
            {
                title = "Subscription expired. Please renew to continue.",
                status = 402
            });
            return;
        }

        await next(context);
    }
    
}