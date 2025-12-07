using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterManagement.Models.Entities
{
    public class Trainer
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad zorunludur")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad zorunludur")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [Display(Name = "E-posta")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        [Display(Name = "Telefon")]
        public string? Phone { get; set; }

        [Display(Name = "Biyografi")]
        [StringLength(2000)]
        public string? Biography { get; set; }

        [Display(Name = "Uzmanlık Alanları")]
        public string? Specializations { get; set; }

        [Display(Name = "Deneyim (Yıl)")]
        [Range(0, 50, ErrorMessage = "Deneyim 0-50 yıl arasında olmalıdır")]
        public int? ExperienceYears { get; set; }

        [Display(Name = "Fotoğraf URL")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Foreign Key
        [Required(ErrorMessage = "Spor salonu seçimi zorunludur")]
        [Display(Name = "Spor Salonu")]
        public int GymId { get; set; }

        // Navigation Properties
        [ForeignKey("GymId")]
        public virtual Gym? Gym { get; set; }

        public virtual ICollection<TrainerService> TrainerServices { get; set; } = new List<TrainerService>();
        public virtual ICollection<TrainerAvailability> Availabilities { get; set; } = new List<TrainerAvailability>();
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        [Display(Name = "Ad Soyad")]
        public string FullName => $"{FirstName} {LastName}";
    }
}

