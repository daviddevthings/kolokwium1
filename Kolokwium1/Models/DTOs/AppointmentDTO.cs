namespace Kolokwium1.Models.DTOs;

public class AppointmentDTO
{
    public DateTime Date { get; set; }
    public PatientDTO Patient { get; set; }
    public DoctorDTO Doctor { get; set; }
    public List<AppointmentServices> AppointmentServices { get; set; }
}

public class PatientDTO
{
    public string firstName { get; set; }
    public string lastName { get; set; }
    public DateTime dateOfBirth { get; set; }
}

public class DoctorDTO
{
    public int doctorId { get; set; }
    public string pwz { get; set; }
}

public class AppointmentServices
{
    public string name { get; set; }
    public decimal serviceFee { get; set; }
}