using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterManagement.Models.Entities
{
    public class Appointment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Üye seçimi zorunludur")]
        [Display(Name = "Üye")]
        public string UserId { get; set; } = string.Empty;

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

        [Required(ErrorMessage = "Başlangıç saati zorunludur")]
        [Display(Name = "Başlangıç Saati")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Display(Name = "Bitiş Saati")]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        [Display(Name = "Durum")]
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        [Display(Name = "Ücret")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Display(Name = "Notlar")]
        [StringLength(500)]
        public string? Notes { get; set; }

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Güncellenme Tarihi")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [ForeignKey("TrainerId")]
        public virtual Trainer? Trainer { get; set; }

        [ForeignKey("ServiceId")]
        public virtual Service? Service { get; set; }

        [Display(Name = "Durum Adı")]
        public string StatusName => Status switch
        {
            AppointmentStatus.Pending => "Beklemede",
            AppointmentStatus.Approved => "Onaylandı",
            AppointmentStatus.Rejected => "Reddedildi",
            AppointmentStatus.Completed => "Tamamlandı",
            AppointmentStatus.Cancelled => "İptal Edildi",
            _ => ""
        };

        [Display(Name = "Durum Rengi")]
        public string StatusColor => Status switch
        {
            AppointmentStatus.Pending => "warning",
            AppointmentStatus.Approved => "success",
            AppointmentStatus.Rejected => "danger",
            AppointmentStatus.Completed => "info",
            AppointmentStatus.Cancelled => "secondary",
            _ => "secondary"
        };
    }

    public enum AppointmentStatus
    {
        [Display(Name = "Beklemede")]
        Pending,
        [Display(Name = "Onaylandı")]
        Approved,
        [Display(Name = "Reddedildi")]
        Rejected,
        [Display(Name = "Tamamlandı")]
        Completed,
        [Display(Name = "İptal Edildi")]
        Cancelled
    }
}




