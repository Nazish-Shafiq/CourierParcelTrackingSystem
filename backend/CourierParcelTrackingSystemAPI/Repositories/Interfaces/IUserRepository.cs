using CourierParcelTrackingSystem.API.Entities;

namespace CourierParcelTrackingSystem.API.Repositories.Interfaces;

public interface IUserRepository
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
}
