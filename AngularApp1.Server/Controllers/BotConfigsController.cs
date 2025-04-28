using Microsoft.AspNetCore.Mvc;
using AngularApp1.Server.Models.Entities;
using AngularApp1.Server.Models.Requests;
using AngularApp1.Server.Models.Responses;
using AngularApp1.Server.Services.Interfaces;
using System.Net.Mime;

namespace AngularApp1.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    public class BotConfigsController : ControllerBase
    {
        private readonly ILogger<BotConfigsController> _logger;
        private readonly IBotConfigService _botConfigService;

        public BotConfigsController(
            IBotConfigService botConfigService,
            ILogger<BotConfigsController> logger)
        {
            _botConfigService = botConfigService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<BotConfigResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var configs = await _botConfigService.GetAsync();
                var response = configs.Select(c => new BotConfigResponse
                {
                    Id = c.Id!,
                    AppName = c.AppName,
                    Config1 = c.Config1,
                    Config2 = c.Config2,
                    Config3 = c.Config3,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bot configurations");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving configurations");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BotConfigResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var config = await _botConfigService.GetAsync(id);
                if (config is null)
                {
                    return NotFound();
                }

                var response = new BotConfigResponse
                {
                    Id = config.Id!,
                    AppName = config.AppName,
                    Config1 = config.Config1,
                    Config2 = config.Config2,
                    Config3 = config.Config3,
                    CreatedAt = config.CreatedAt,
                    UpdatedAt = config.UpdatedAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bot configuration with ID {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving configuration");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(BotConfigResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateBotConfigRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var newConfig = new BotConfig
                {
                    AppName = request.AppName,
                    Config1 = request.Config1,
                    Config2 = request.Config2,
                    Config3 = request.Config3
                };
                
                await _botConfigService.CreateAsync(newConfig);

                var response = new BotConfigResponse
                {
                    Id = newConfig.Id!,
                    AppName = newConfig.AppName,
                    Config1 = newConfig.Config1,
                    Config2 = newConfig.Config2,
                    Config3 = newConfig.Config3,
                    CreatedAt = newConfig.CreatedAt,
                    UpdatedAt = newConfig.UpdatedAt
                };

                return CreatedAtAction(nameof(GetById), new { id = newConfig.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bot configuration");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating configuration");
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(string id, [FromBody] CreateBotConfigRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingConfig = await _botConfigService.GetAsync(id);
                if (existingConfig is null)
                {
                    return NotFound();
                }

                existingConfig.AppName = request.AppName;
                existingConfig.Config1 = request.Config1;
                existingConfig.Config2 = request.Config2;
                existingConfig.Config3 = request.Config3;
                existingConfig.UpdatedAt = DateTime.UtcNow;

                await _botConfigService.UpdateAsync(id, existingConfig);

                return Ok(existingConfig);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating bot configuration with ID {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating configuration");
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var config = await _botConfigService.GetAsync(id);
                if (config is null)
                {
                    return NotFound();
                }

                await _botConfigService.RemoveAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting bot configuration with ID {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting configuration");
            }
        }
    }
}