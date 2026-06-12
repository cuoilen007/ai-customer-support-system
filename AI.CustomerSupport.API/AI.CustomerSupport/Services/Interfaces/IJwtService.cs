using AI.CustomerSupport.API.Models;

namespace AI.CustomerSupport.API.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
