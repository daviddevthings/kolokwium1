using System.Data.Common;
using APBD_example_test1_2025.Exceptions;
using Kolokwium1.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace Kolokwium1.Services;

public class DbService : IDbService
{
    private readonly string _connectionString;

    public DbService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default") ?? string.Empty;
    }

    public async Task<AppointmentDTO> GetAppointmentById(int id)
    {
        string query =
            @"SELECT A.date, P.first_name, P.last_name, P.date_of_birth, D.doctor_id, D.PWZ,S.name,S.base_fee from s30780.Patient P     
            join s30780.Appointment A on P.patient_id = A.patient_id    
            join s30780.Doctor D on A.doctor_id = D.doctor_id     
            join s30780.Appointment_Service AService on A.appoitment_id = AService.appoitment_id   
            join s30780.Service S on AService.service_id = S.service_id
            where A.appoitment_id = @ID;";
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        await connection.OpenAsync();

        command.Parameters.AddWithValue("@ID", id);

        var reader = await command.ExecuteReaderAsync();
        AppointmentDTO? result = null;
        while (await reader.ReadAsync())
        {
            if (reader.IsDBNull(0))
            {
                return null;
            }

            if (result is null)
            {
                result = new AppointmentDTO
                {
                    Date = reader.GetDateTime(0),
                    Patient = new PatientDTO
                    {
                        firstName = reader.GetString(1),
                        lastName = reader.GetString(2),
                        dateOfBirth = reader.GetDateTime(3)
                    },
                    Doctor = new DoctorDTO
                    {
                        doctorId = reader.GetInt32(4),
                        pwz = reader.GetString(5)
                    },
                    AppointmentServices = new List<AppointmentServices>()
                };
            }

            result.AppointmentServices.Add(
                new AppointmentServices
                {
                    name = reader.GetString(6),
                    serviceFee = reader.GetDecimal(7)
                });
        }

        return result;
    }

    public async Task CreateAppointment(AddApointmentDTO appointment)
    {
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        await connection.OpenAsync();

        DbTransaction transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;

        try
        {
            command.Parameters.Clear();
            command.CommandText = "SELECT 1 FROM Appointment WHERE appoitment_id = @ID";
            command.Parameters.AddWithValue("@ID", appointment.AppointmentId);
            var appointmentIdRes = await command.ExecuteScalarAsync();
            Console.Out.WriteLine(appointmentIdRes);
            if (appointmentIdRes is not null)
                throw new ConflictException($"Appointment with ID - {appointment.AppointmentId} - already exists.");

            command.Parameters.Clear();

            command.CommandText = "SELECT 1 FROM Patient WHERE patient_id = @ID";
            command.Parameters.AddWithValue("@ID", appointment.PatientId);
            var patientIdRes = await command.ExecuteScalarAsync();
            if (patientIdRes is null)
                throw new NotFoundException($"Patient with ID - {appointment.PatientId} - not found.");

            command.Parameters.Clear();

            command.CommandText = "SELECT 1 FROM Doctor WHERE pwz = @ID";
            command.Parameters.AddWithValue("@ID", appointment.Pwz);
            var doctorIdRes = await command.ExecuteScalarAsync();
            if (doctorIdRes is null)
                throw new NotFoundException($"Doctor with ID - {appointment.Pwz} - not found.");
            command.Parameters.Clear();


            command.CommandText = "SELECT doctor_id from Doctor where PWZ=@PWZ SELECT CAST(SCOPE_IDENTITY() as int)";
            command.Parameters.AddWithValue("@PWZ", appointment.Pwz);

            var doctorID = (int)await command.ExecuteScalarAsync();
            command.Parameters.Clear();

            command.CommandText =
                "INSERT INTO Appointment (appoitment_id, patient_id, doctor_id,date) VALUES (@AID, @PID, @DID, @DATE)";


            command.Parameters.AddWithValue("@AID", appointment.AppointmentId);
            command.Parameters.AddWithValue("@PID", appointment.PatientId);
            command.Parameters.AddWithValue("@DID", doctorID);
            command.Parameters.AddWithValue("@DATE", DateTime.Now);
            await command.ExecuteNonQueryAsync();

            command.Parameters.Clear();

            foreach (var a in appointment.Services)
            {
                command.CommandText = "SELECT service_id FROM Service WHERE name = @NAME";
                command.Parameters.AddWithValue("@NAME", a.ServiceName);
                var serviceIdRes = await command.ExecuteScalarAsync();
                if (serviceIdRes is null)
                    throw new NotFoundException($"Service with Name - {a.ServiceName} - not found.");
                command.Parameters.Clear();


                command.CommandText =
                    "INSERT INTO Appointment_Service (appoitment_id, service_id, service_fee) VALUES (@AID, @SID, @SFee)";


                command.Parameters.AddWithValue("@AID", appointment.AppointmentId);
                command.Parameters.AddWithValue("@SID", (int)serviceIdRes);
                command.Parameters.AddWithValue("@SFee", a.ServiceFee);
                await command.ExecuteNonQueryAsync();
                command.Parameters.Clear();
            }

            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}