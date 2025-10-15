using HospitalSanVicente.Models;
using System.Threading.Tasks;

namespace HospitalSanVicente.Services
{
    public interface IEmailService
    {
        Task<bool> SendAppointmentConfirmationEmailAsync(Appointment appointment);
    }
}

