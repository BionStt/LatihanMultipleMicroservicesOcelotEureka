using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using Microsoft.Extensions.Logging;
using System;

namespace ServiceOne.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        //public ValuesController(string serviceName, ILogger<ValuesController> logger)
        //{
        //    _service_name = Assembly.GetExecutingAssembly().GetName().Name;
        //    _logger = logger;
        //}

        //private readonly string _service_name;
        private readonly ILogger<ValuesController> _logger;

        public ValuesController(ILogger<ValuesController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("badcode")]
        public string BadCode()
        {
 
            var msg = $"{Assembly.GetExecutingAssembly().GetName().Name} - {Assembly.GetExecutingAssembly().HostContext} - {Assembly.GetExecutingAssembly().Location} -> Some bad code was executed!";
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
