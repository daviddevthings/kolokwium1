using APBD_example_test1_2025.Exceptions;
using Kolokwium1.Models.DTOs;
using Kolokwium1.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kolokwium1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly IDbService _dbService;

        public AppointmentsController(IDbService dbService)
        {
            _dbService = dbService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAppointment(int id)
        {
            var appointment = await _dbService.GetAppointmentById(id);
            if (appointment is null)
            {
                return NotFound(new { Message = "Appointment with given id not found" });
            }

            return Ok(appointment);
        }

        [HttpPost("/api/appointments")]
        public async Task<IActionResult> CreateAppointment([FromBody] AddApointmentDTO appointment)
        {
            if (appointment is null)
            {
                return BadRequest(new { Message = "Empty body" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Invalid appointment data" });
            }

            try
            {
                await _dbService.CreateAppointment(appointment);
            }
            catch (ConflictException e)
            {
                return Conflict(e.Message);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }

            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }

            return CreatedAtAction(nameof(GetAppointment), new { id = appointment.AppointmentId }, appointment);
        }
    }
}