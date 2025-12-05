using System.ComponentModel.DataAnnotations;

namespace FitnessCenterManagement.Models.Entities
{
    public class Gym
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Salon adı zorunludur")]
        [StringLength(100, ErrorMessage = "Salon adı en fazla 100 karakter olabilir")]
        [Display(Name = "Salon Adı")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Adres zorunludur")]
        [StringLength(500, ErrorMessage = "Adres en fazla 500 karakter olabilir")]
        [Display(Name = "Adres")]
        public string Address { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        [Display(Name = "Telefon")]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [Display(Name = "E-posta")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Açılış saati zorunludur")]
        [Display(Name = "Açılış Saati")]
        [DataType(DataType.Time)]
        public TimeSpan OpeningTime { get; set; }

        [Required(ErrorMessage = "Kapanış saati zorunludur")]
        [Display(Name = "Kapanış Saati")]
        [DataType(DataType.Time)]
        public TimeSpan ClosingTime { get; set; }

        [Display(Name = "Açıklama")]
        [StringLength(1000)]
        public string? Description { get; set; }

        [Display(Name = "Fotoğraf URL")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        public virtual ICollection<Service> Services { get; set; } = new List<Service>();
        public virtual ICollection<Trainer> Trainers { get; set; } = new List<Trainer>();
    }
}

