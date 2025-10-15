using HospitalSanVicente.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Threading.Tasks;

namespace HospitalSanVicente.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        // Inyectamos la configuración para poder usarla
        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task<bool> SendAppointmentConfirmationEmailAsync(Appointment appointment)
        {
            // Verificamos que el paciente tenga un correo electrónico
            if (string.IsNullOrEmpty(appointment.Patient?.Email))
            {
                // No se puede enviar si no hay email, pero no es un error del sistema.
                return false;
            }

            try
            {
                var email = new MimeMessage();
                email.Sender = new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail);
                email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                email.To.Add(new MailboxAddress(appointment.Patient.FullName, appointment.Patient.Email));
                email.Subject = "Confirmación de Cita Médica - Hospital San Vicente";

                var builder = new BodyBuilder();
                builder.HtmlBody = $@"
                    <h2>Confirmación de Cita</h2>
                    <p>Hola <strong>{appointment.Patient.FirstName}</strong>,</p>
                    <p>Tu cita ha sido agendada con éxito. Aquí están los detalles:</p>
                    <ul>
                        <li><strong>Médico:</strong> {appointment.Doctor.FullName}</li>
                        <li><strong>Especialidad:</strong> {appointment.Doctor.Specialty}</li>
                        <li><strong>Fecha y Hora:</strong> {appointment.AppointmentDate:dddd, dd MMMM yyyy 'a las' HH:mm}</li>
                    </ul>
                    <p>Gracias por confiar en el Hospital San Vicente.</p>";

                email.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                return true; // Correo enviado con éxito
            }
            catch (System.Exception ex)
            {
                // Aquí podrías registrar el error en un log
                // Console.WriteLine(ex.Message);
                return false; // Error al enviar el correo
            }
        }
    }
}