using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalSanVicente.Data;
using HospitalSanVicente.Models;

namespace HospitalSanVicente.Controllers
{
    public class DoctorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DoctorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // LISTADO DE DOCTORES ACTIVOS
        public async Task<IActionResult> Index()
        {
            var doctoresActivos = await _context.Doctors
                .Where(d => d.IsActive)
                .ToListAsync();

            ViewBag.ShowingActive = true;
            return View(doctoresActivos);
        }

        // LISTADO DE DOCTORES INACTIVOS
        public async Task<IActionResult> Inactivos()
        {
            var doctoresInactivos = await _context.Doctors
                .Where(d => !d.IsActive)
                .ToListAsync();

            ViewBag.ShowingActive = false;
            return View("Index", doctoresInactivos); // reutiliza la vista Index
        }

        // CREAR DOCTOR
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Doctor doctor)
        {
            // Validar número de documento único
            if (await _context.Doctors.AnyAsync(d => d.DocumentNumber == doctor.DocumentNumber))
            {
                TempData["error"] = "El número de documento ya esta registrado.";
                return View(doctor);
            }

            // Validar combinación única de nombre + apellido + especialidad
            if (await _context.Doctors.AnyAsync(d =>
                d.FirstName == doctor.FirstName &&
                d.LastName == doctor.LastName &&
                d.Specialty == doctor.Specialty))
            {
                TempData["error"] = "Ya existe un doctor con el mismo nombre, apellido y especialidad.";
                return View(doctor);
            }

            if (ModelState.IsValid)
            {
                doctor.IsActive = true;
                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();

                TempData["message"] = "Doctor registrado correctamente.";
                return RedirectToAction(nameof(Index));
            }

            TempData["error"] = "Error al registrar el doctor. Verifique los datos.";
            return View(doctor);
        }

        // EDITAR DOCTOR
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
                return NotFound();

            return View(doctor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Doctor doctor)
        {
            if (id != doctor.DoctorId)
                return NotFound();

            // Validar documento único (excepto el propio)
            if (await _context.Doctors.AnyAsync(d => 
                d.DocumentNumber == doctor.DocumentNumber && d.DoctorId != doctor.DoctorId))
            {
                TempData["error"] = "Ya existe un doctor con ese número de documento.";
                return View(doctor);
            }

            // Validar combinación única de nombre + apellido + especialidad (excepto el propio)
            if (await _context.Doctors.AnyAsync(d =>
                d.FirstName == doctor.FirstName &&
                d.LastName == doctor.LastName &&
                d.Specialty == doctor.Specialty &&
                d.DoctorId != doctor.DoctorId))
            {
                TempData["error"] = "Ya existe un doctor con el mismo nombre, apellido y especialidad.";
                return View(doctor);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(doctor);
                    await _context.SaveChangesAsync();
                    TempData["message"] = "Doctor actualizado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Doctors.Any(d => d.DoctorId == doctor.DoctorId))
                        return NotFound();
                    else
                        throw;
                }
            }

            TempData["error"] = "Error al actualizar el doctor. Verifique los datos.";
            return View(doctor);
        }

        // CAMBIAR ESTADO (ACTIVO / INACTIVO)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Doctors/CambiarEstado/{id}")]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
            {
                TempData["error"] = "El doctor no existe.";
                return RedirectToAction(nameof(Index));
            }

            doctor.IsActive = !doctor.IsActive;
            await _context.SaveChangesAsync();

            TempData["message"] = doctor.IsActive
                ? "Doctor activado correctamente."
                : "Doctor desactivado correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}
