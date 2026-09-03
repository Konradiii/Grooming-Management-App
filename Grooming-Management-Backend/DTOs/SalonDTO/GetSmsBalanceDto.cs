namespace Grooming_Management_App.DTOs.SalonDTO;

public class GetSmsBalanceDto
{
    public int Remaining { get; set; }
    public DateOnly ResetDate { get; set; }
}