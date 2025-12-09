using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterManagement.Models.Entities
{
    public class Service
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Hizmet adı zorunludur")]
        [StringLength(100, ErrorMessage = "Hizmet adı en fazla 100 karakter olabilir")]
        [Display(Name = "Hizmet Adı")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Açıklama")]
        [StringLength(1000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Süre zorunludur")]
        [Range(15, 180, ErrorMessage = "Süre 15-180 dakika arasında olmalıdır")]
        [Display(Name = "Süre (Dakika)")]
        public int DurationMinutes { get; set; }

        [Required(ErrorMessage = "Ücret zorunludur")]
        [Range(0, 10000, ErrorMessage = "Ücret 0-10000 TL arasında olmalıdır")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Ücret (TL)")]
        public decimal Price { get; set; }

        [Display(Name = "Kategori")]
        public ServiceCategory Category { get; set; }

        [Display(Name = "Fotoğraf URL")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Foreign Key
        [Display(Name = "Spor Salonu")]
        public int GymId { get; set; }

        // Navigation Properties
        [ForeignKey("GymId")]
        public virtual Gym? Gym { get; set; }

        public virtual ICollection<TrainerService> TrainerServices { get; set; } = new List<TrainerService>();
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }

    public enum ServiceCategory
    {
        [Display(Name = "Fitness")]
        Fitness,
        [Display(Name = "Yoga")]
        Yoga,
        [Display(Name = "Pilates")]
        Pilates,
        [Display(Name = "Kardiyo")]
        Cardio,
        [Display(Name = "Kişisel Antrenman")]
        PersonalTraining,
        [Display(Name = "Grup Dersleri")]
        GroupClasses,
        [Display(Name = "Yüzme")]
        Swimming,
        [Display(Name = "Boks")]
        Boxing,
        [Display(Name = "CrossFit")]
        CrossFit,
        [Display(Name = "Diğer")]
        Other
    }
}




