using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OAuthDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(ILogger<OrdersController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Authorize(policy: "OrdersRead")]
        public IActionResult Get()
        {
            return Ok(new[]
            {
               new { Id = 2, Amount = 250 },
               new { Id = 1, Amount = 100 },
            });
        }
    }
}
