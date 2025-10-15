using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace HospitalSanVicente.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un paciente")]
        public int PatientId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un médico")]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Debe indicar la fecha y hora de la cita")]
        public DateTime AppointmentDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Programada";
        
        [StringLength(500)]
        public string? Notes { get; set; } 
        
        [Required]
        public bool EmailSent { get; set; } = false;

        
        [ForeignKey(nameof(PatientId))]
        [ValidateNever]
        public Patient? Patient { get; set; } 

        [ForeignKey(nameof(DoctorId))]
        [ValidateNever]
        public Doctor? Doctor { get; set; } 
    }
}