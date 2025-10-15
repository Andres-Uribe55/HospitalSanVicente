using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace HospitalSanVicente.Models
{
    [Index(nameof(DocumentNumber), IsUnique = true)]
    public class Patient
    {
        [Key]
        public int PatientId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(100)]
        public string LastName { get; set; }

        // --- LÍNEA AÑADIDA ---
        // Esta propiedad combina nombre y apellido y no se guarda en la base de datos.
        public string FullName => $"{FirstName} {LastName}";
        // ---------------------

        [Required(ErrorMessage = "El tipo de documento es obligatorio")]
        [StringLength(20)]
        public string DocumentType { get; set; }

        [Required(ErrorMessage = "El número de documento es obligatorio")]
        [StringLength(20)]
        public string DocumentNumber { get; set; }

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

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