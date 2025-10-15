    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using HospitalSanVicente.Data;
    using HospitalSanVicente.Models;

    namespace HospitalSanVicente.Controllers
    {
        public class PatientsController : Controller
        {
            private readonly ApplicationDbContext _context;

            public PatientsController(ApplicationDbContext context)
            {
                _context = context;
            }

            // LISTADO DE PACIENTES ACTIVOS
            public async Task<IActionResult> Index()
            {
                var pacientesActivos = await _context.Patients
                    .Where(p => p.IsActive)
                    .ToListAsync();

                ViewBag.ShowingActive = true;
                return View(pacientesActivos);
            }

            // LISTADO DE PACIENTES INACTIVOS
            public async Task<IActionResult> Inactivos()
            {
                var pacientesInactivos = await _context.Patients
                    .Where(p => !p.IsActive)
                    .ToListAsync();

                ViewBag.ShowingActive = false;
                return View("Index", pacientesInactivos); // reutiliza la misma vista Index
            }

            // CREAR PACIENTE
            public IActionResult Create()
            {
                return View();
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Create(Patient patient)
            {
                // Validar documento único
                if (await _context.Patients.AnyAsync(p => p.DocumentNumber == patient.DocumentNumber))
                {
                    TempData["error"] = "El documento de identidad ya esta registrado.";
                    return View(patient);
                }

                if (ModelState.IsValid)
                {
                    patient.IsActive = true;
                    _context.Add(patient);
                    await _context.SaveChangesAsync();
                    TempData["message"] = "Paciente registrado correctamente.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["error"] = "Error al registrar el paciente. Verifique los datos.";
                return View(patient);
            }

            //EDITAR PACIENTE
            public async Task<IActionResult> Edit(int? id)
            {
                if (id == null)
                    return NotFound();

                var patient = await _context.Patients.FindAsync(id);
                if (patient == null)
                    return NotFound();

                return View(patient);
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(int id, Patient patient)
            {
                if (id != patient.PatientId)
                    return NotFound();

                // Validar documento único (excluyendo el propio registro)
                if (await _context.Patients.AnyAsync(p => p.DocumentNumber == patient.DocumentNumber && p.PatientId != patient.PatientId))
                {
                    TempData["error"] = "Ya existe un paciente con ese documento.";
                    return View(patient);
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(patient);
                        await _context.SaveChangesAsync();
                        TempData["message"] = "Paciente actualizado correctamente.";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!_context.Patients.Any(e => e.PatientId == patient.PatientId))
                            return NotFound();
                        else
                            throw;
                    }
                }

                TempData["error"] = "Error al actualizar el paciente. Verifique los datos.";
                return View(patient);
            }

            //  CAMBIAR ESTADO (ACTIVO / INACTIVO)
            [HttpPost]
            [ValidateAntiForgeryToken]
            [Route("Patients/CambiarEstado/{id}")]
            public async Task<IActionResult> CambiarEstado(int id)
            {
                var patient = await _context.Patients.FindAsync(id);
                if (patient == null)
                {
                    TempData["error"] = "El paciente no existe.";
                    return RedirectToAction(nameof(Index));
                }

                patient.IsActive = !patient.IsActive; // alterna el estado
                await _context.SaveChangesAsync();

                TempData["message"] = patient.IsActive
                    ? "Paciente activado correctamente."
                    : "Paciente desactivado correctamente.";

                return RedirectToAction(nameof(Index));
            }
        }
    }
