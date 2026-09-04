
namespace OrderConsumer.Services
{
    public interface IOrderService
    {
        Task<List<OrderResponse>> GetOrdersAsync(string accessToken);
    }
}