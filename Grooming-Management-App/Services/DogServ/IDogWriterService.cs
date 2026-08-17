using Grooming_Management_App.DTOs.DogDTO;

namespace Grooming_Management_App.Services.DogServ;

public interface IDogWriterService
{
    
    
    Task<int> CreateDogAsync(int salonId, CreateDogDto dto, CancellationToken ct );
    
    Task UpdateDogAsync(int salonId, int dogId, UpdateDogDto dto, CancellationToken ct );




}