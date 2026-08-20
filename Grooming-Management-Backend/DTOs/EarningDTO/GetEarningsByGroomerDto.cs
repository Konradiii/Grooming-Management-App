namespace Grooming_Management_App.DTOs.EarningDTO;

public class GetEarningsByGroomerDto
{
    public int GroomerId { get; set; }
    public string GroomerFullName { get; set; }
    public decimal Earnings { get; set; }
}