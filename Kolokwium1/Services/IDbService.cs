using Kolokwium1.Models.DTOs;

namespace Kolokwium1.Services;

public interface IDbService
{
    public Task<AppointmentDTO> GetAppointmentById(int id);
    public Task CreateAppointment(AddApointmentDTO appointment);
}