using api.DTOs;
using infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/containers")]
    public class ContainersController : ControllerBase
    {
        private readonly IDockerService _dockerService;

        public ContainersController(IDockerService dockerService)
        {
            _dockerService = dockerService;
        }

        // POST /api/containers
        [HttpPost]
        public async Task<IActionResult> CreateContainer([FromBody] CreateContainerRequest request)
        {
            try
            {
                var container = await _dockerService.CreateContainerAsync(request.Image, request.DatabaseType, request.Credentials);
                return CreatedAtAction(nameof(GetContainer), new { id = container.Id }, container);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // POST /api/containers/{id}/start
        [HttpPost("{id}/start")]
        public async Task<IActionResult> StartContainer(string id)
        {
            try
            {
                await _dockerService.StartContainerAsync(id);
                return Ok(new { message = $"Container {id} started successfully." });
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        // POST /api/containers/{id}/stop
        [HttpPost("{id}/stop")]
        public async Task<IActionResult> StopContainer(string id)
        {
            try
            {
                await _dockerService.StopContainerAsync(id);
                return Ok(new { message = $"Container {id} stopped successfully." });
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        // DELETE /api/containers/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContainer(string id)
        {
            try
            {
                await _dockerService.DeleteContainerAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        // GET /api/containers/{id} - Endpoint adicional para verificar el estado
        [HttpGet("{id}")]
        public async Task<IActionResult> GetContainer(string id)
        {
            try
            {
                var container = await _dockerService.GetContainerStatusAsync(id);
                return Ok(container);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }
    }
}
