using Microsoft.Build.Framework;

namespace Kolokwium1.Models.DTOs;

public class AddApointmentDTO
{
    [Required] public int AppointmentId { get; set; }
    [Required] public int PatientId { get; set; }
    [Required] public string Pwz { get; set; }
    public List<AddAppointmentServices> Services { get; set; }
}

public class AddAppointmentServices
{
    public string ServiceName { get; set; }
    public decimal ServiceFee { get; set; }
}

