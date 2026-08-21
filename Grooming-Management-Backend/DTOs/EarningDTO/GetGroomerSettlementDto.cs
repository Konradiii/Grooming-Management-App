namespace Grooming_Management_App.DTOs.EarningDTO;

public class GetGroomerSettlementDto
{
    public int GroomerId { get; set; }
    public string GroomerFullName { get; set; }
    public int VisitsCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal Settlement { get; set; }
}