public class CreateVisitWithNewDogDto
{
    // właściciel
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Phone { get; set; }

    // pies
    public string DogName { get; set; }
    public int AgeInMonths { get; set; }
    public int BreedId { get; set; }
    public string? DogNotes { get; set; }

    // wizyta
    public int GroomerId { get; set; }
    public int ServiceBreedId { get; set; }
    public DateTime Date { get; set; }
    public int? DurationMinutes { get; set; }
    public string? Notes { get; set; }
}