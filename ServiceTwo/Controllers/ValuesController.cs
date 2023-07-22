using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace ServiceTwo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ILogger<ValuesController> _logger;

        public ValuesController(ILogger<ValuesController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("badcode")]
        public string BadCode()
        {
            var msg = $"{Assembly.GetExecutingAssembly().GetName().Name} -> Some bad code was executed!";
            throw new Exception(msg);
        }

        [HttpGet]
        public IActionResult Get()
        {
            var msg = $"{Assembly.GetExecutingAssembly().GetName().Name} -> Value";
            _logger.LogInformation(msg);
            return Ok(msg);
        }

        [HttpGet("healthcheck")]
        public IActionResult Healthcheck()
        {
            var msg = $"{Assembly.GetExecutingAssembly().GetName().Name} is healthy";
            _logger.LogInformation(msg);
            return Ok(msg);
        }

        [HttpGet("status")]
        public IActionResult Status()
        {
            var msg = $"{Assembly.GetExecutingAssembly().GetName().Name}, running on {Request.Host}";
            _logger.LogInformation(msg);
            return Ok(msg);
        }





    }
}
