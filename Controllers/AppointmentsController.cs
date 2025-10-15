using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HospitalSanVicente.Data;
using HospitalSanVicente.Models;
using HospitalSanVicente.Services;
using System.Linq;
using System.Threading.Tasks;

namespace HospitalSanVicente.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public AppointmentsController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: Appointments
        public async Task<IActionResult> Index(int? patientId, int? doctorId)
        {
            ViewBag.Patients = new SelectList(await _context.Patients.Where(p => p.IsActive).ToListAsync(), "PatientId", "FullName");
            ViewBag.Doctors = new SelectList(await _context.Doctors.Where(d => d.IsActive).ToListAsync(), "DoctorId", "FullName");
            
            var appointmentsQuery = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .AsQueryable();

            if (patientId.HasValue)
            {
                appointmentsQuery = appointmentsQuery.Where(a => a.PatientId == patientId.Value);
            }

            if (doctorId.HasValue)
            {
                appointmentsQuery = appointmentsQuery.Where(a => a.DoctorId == doctorId.Value);
            }

            var appointments = await appointmentsQuery.OrderByDescending(a => a.AppointmentDate).ToListAsync();
            
            return View(appointments);
        }

        // GET: Appointments/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Patients = new SelectList(await _context.Patients.Where(p => p.IsActive).ToListAsync(), "PatientId", "FullName");
            ViewBag.Doctors = new SelectList(await _context.Doctors.Where(d => d.IsActive).ToListAsync(), "DoctorId", "FullName");
            return View();
        }

        // POST: Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Appointment appointment)
        {
            // Validaciones personalizadas de lógica de negocio
            var doctorHasAppointment = await _context.Appointments.AnyAsync(a =>
                a.DoctorId == appointment.DoctorId &&
                a.AppointmentDate == appointment.AppointmentDate &&
                a.Status != "Cancelada");

            if (doctorHasAppointment)
            {
                ModelState.AddModelError("AppointmentDate", "El médico seleccionado ya tiene una cita programada a esta hora.");
            }

            var patientHasAppointment = await _context.Appointments.AnyAsync(a =>
                a.PatientId == appointment.PatientId &&
                a.AppointmentDate == appointment.AppointmentDate &&
                a.Status != "Cancelada");
            
            if (patientHasAppointment)
            {
                ModelState.AddModelError("AppointmentDate", "El paciente seleccionado ya tiene una cita programada a esta hora.");
            }

            // ===================================================================
            // INICIO: BLOQUE DE DEPURACIÓN TEMPORAL
            // Este bloque te ayudará a ver en la consola por qué ModelState.IsValid es falso.
            // ===================================================================
            if (!ModelState.IsValid)
            {
                // Busca todos los errores de validación del modelo
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                string errorMessages = string.Join(" | ", errors.Select(e => e.ErrorMessage));

                // Imprime los errores en la consola de tu aplicación
                System.Diagnostics.Debug.WriteLine("Errores de validación del modelo: " + errorMessages);
                
                // Opcional: muestra un mensaje de error genérico en la UI
                TempData["error"] = "Se encontraron errores de validación. Por favor, revisa los datos del formulario.";
            }
            // ===================================================================
            // FIN: BLOQUE DE DEPURACIÓN TEMPORAL
            // ===================================================================

            if (ModelState.IsValid)
            {
                appointment.Status = "Programada";
                appointment.EmailSent = false;

                _context.Add(appointment);
                await _context.SaveChangesAsync();

                var appointmentDetails = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .FirstOrDefaultAsync(a => a.AppointmentId == appointment.AppointmentId);

                if (appointmentDetails != null)
                {
                    bool emailFueEnviado = await _emailService.SendAppointmentConfirmationEmailAsync(appointmentDetails);
                    
                    if (emailFueEnviado)
                    {
                        appointment.EmailSent = true;
                        _context.Update(appointment);
                        await _context.SaveChangesAsync();
                        TempData["message"] = "Cita agendada y correo de confirmación enviado correctamente.";
                    }
                    else
                    {
                        TempData["message"] = "Cita agendada, pero NO se pudo enviar el correo de confirmación.";
                    }
                }
                
                return RedirectToAction(nameof(Index));
            }

            // Si el código llega aquí, es porque ModelState no era válido.
            // Recargamos los dropdowns y devolvemos la vista para que el usuario corrija los errores.
            ViewBag.Patients = new SelectList(await _context.Patients.Where(p => p.IsActive).ToListAsync(), "PatientId", "FullName", appointment.PatientId);
            ViewBag.Doctors = new SelectList(await _context.Doctors.Where(d => d.IsActive).ToListAsync(), "DoctorId", "FullName", appointment.DoctorId);
            
            return View(appointment);
        }

        // GET: Appointments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();
            
            ViewBag.Patients = new SelectList(await _context.Patients.Where(p => p.IsActive).ToListAsync(), "PatientId", "FullName", appointment.PatientId);
            ViewBag.Doctors = new SelectList(await _context.Doctors.Where(d => d.IsActive).ToListAsync(), "DoctorId", "FullName", appointment.DoctorId);
            
            return View(appointment);
        }

        // POST: Appointments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Appointment appointment)
        {
            if (id != appointment.AppointmentId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(appointment);
                    await _context.SaveChangesAsync();
                    TempData["message"] = "Cita actualizada correctamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Appointments.Any(e => e.AppointmentId == appointment.AppointmentId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Patients = new SelectList(await _context.Patients.Where(p => p.IsActive).ToListAsync(), "PatientId", "FullName", appointment.PatientId);
            ViewBag.Doctors = new SelectList(await _context.Doctors.Where(d => d.IsActive).ToListAsync(), "DoctorId", "FullName", appointment.DoctorId);
            TempData["error"] = "No se pudo actualizar la cita. Por favor, verifique los datos.";
            return View(appointment);
        }
        
        // POST: Appointments/ChangeStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, string newStatus)
        {
            if (string.IsNullOrEmpty(newStatus))
            {
                TempData["error"] = "Debe proporcionar un estado válido.";
                return RedirectToAction(nameof(Index));
            }
            
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();
            
            appointment.Status = newStatus;
            _context.Update(appointment);
            await _context.SaveChangesAsync();

            TempData["message"] = $"El estado de la cita se ha cambiado a '{newStatus}' correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}