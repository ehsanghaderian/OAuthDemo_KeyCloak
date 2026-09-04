
namespace OrderConsumer.Services
{
    public interface IOAuthTokenService
    {
        Task<string> GetAccessTokenAsync();
    }
}