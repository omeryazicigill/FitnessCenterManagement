using System.ComponentModel.DataAnnotations;
using FitnessCenterManagement.Models.Entities;

namespace FitnessCenterManagement.ViewModels
{
    public class AppointmentViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Antrenör seçimi zorunludur")]
        [Display(Name = "Antrenör")]
        public int TrainerId { get; set; }

        [Required(ErrorMessage = "Hizmet seçimi zorunludur")]
        [Display(Name = "Hizmet")]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Randevu tarihi zorunludur")]
        [Display(Name = "Randevu Tarihi")]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Randevu saati zorunludur")]
        [Display(Name = "Randevu Saati")]
        public string StartTime { get; set; } = string.Empty;

        [Display(Name = "Notlar")]
        [StringLength(500)]
        public string? Notes { get; set; }

        // For display purposes
        public List<Trainer>? Trainers { get; set; }
        public List<Service>? Services { get; set; }
        public List<string>? AvailableTimes { get; set; }
    }

    public class AppointmentListViewModel
    {
        public int Id { get; set; }
        public string TrainerName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal Price { get; set; }
        public AppointmentStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string StatusColor { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? UserName { get; set; }
    }
}




