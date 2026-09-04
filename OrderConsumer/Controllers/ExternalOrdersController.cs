using Microsoft.AspNetCore.Mvc;
using OrderConsumer.Services;

namespace OrderConsumer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExternalOrdersController : ControllerBase
    {
        private readonly ILogger<ExternalOrdersController> _logger;
        private readonly IOAuthTokenService _authTokenService;
        private readonly IOrderService _orderService;
        public ExternalOrdersController(ILogger<ExternalOrdersController> logger, IOAuthTokenService authTokenService, IOrderService orderService)
        {
            _logger = logger;
            _authTokenService = authTokenService;
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var accessToken = await _authTokenService.GetAccessTokenAsync();
            var orders = await _orderService.GetOrdersAsync(accessToken);
            return Ok(orders);
        }
    }
}
