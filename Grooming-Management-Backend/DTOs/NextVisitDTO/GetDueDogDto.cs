namespace Grooming_Management_App.DTOs.NextVisitDTO;

public class GetDueDogDto
{
    public int DogId { get; set; }
    public string DogName { get; set; } = string.Empty;
    public string BreedName { get; set; } = string.Empty;

    public int DogOwnerId { get; set; }
    public string DogOwnerFullName { get; set; } = string.Empty;
    public string DogOwnerPhone { get; set; } = string.Empty;

    public DateOnly LastVisitDate { get; set; }
    public DateOnly PredictedDate { get; set; }
    public int TypicalIntervalDays { get; set; }
    public int ToleranceDays { get; set; }
    public int DaysOverdue { get; set; }
    public int VisitsCount { get; set; }
}