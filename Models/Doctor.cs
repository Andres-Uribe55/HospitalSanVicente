using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace HospitalSanVicente.Models
{
    [Index(nameof(DocumentNumber), IsUnique = true)]
    [Index(nameof(FirstName), nameof(LastName), nameof(Specialty), IsUnique = true)]
    public class Doctor
    {
        [Key]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(100)]
        public string LastName { get; set; }

        // --- LÍNEA AÑADIDA ---
        public string FullName => $"{FirstName} {LastName}";
        // ---------------------

        [Required(ErrorMessage = "El tipo de documento es obligatorio")]
        [StringLength(20)]
        public string DocumentType { get; set; }

        [Required(ErrorMessage = "El número de documento es obligatorio")]
        [StringLength(20)]
        public string DocumentNumber { get; set; }

        [Required(ErrorMessage = "La especialidad es obligatoria")]
        [StringLength(100)]
        public string Specialty { get; set; }

        [Phone]
        [StringLength(20)]
        public string? Phone { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }
        
        public bool IsActive { get; set; } = true;
        public ICollection<Appointment>? Appointments { get; set; }
    }
}