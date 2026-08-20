namespace Grooming_Management_App.DTOs.EarningDTO;

public class GetEarningForPeriodDto
{
    public decimal Earnings { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}