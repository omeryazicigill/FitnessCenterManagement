using System.ComponentModel.DataAnnotations;

namespace FitnessCenterManagement.ViewModels
{
    public class AIRecommendationViewModel
    {
        [Required(ErrorMessage = "Yaş zorunludur")]
        [Range(10, 100, ErrorMessage = "Yaş 10-100 arasında olmalıdır")]
        [Display(Name = "Yaş")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Kilo zorunludur")]
        [Range(30, 300, ErrorMessage = "Kilo 30-300 kg arasında olmalıdır")]
        [Display(Name = "Kilo (kg)")]
        public decimal Weight { get; set; }

        [Required(ErrorMessage = "Boy zorunludur")]
        [Range(100, 250, ErrorMessage = "Boy 100-250 cm arasında olmalıdır")]
        [Display(Name = "Boy (cm)")]
        public int Height { get; set; }

        [Required(ErrorMessage = "Vücut tipi seçimi zorunludur")]
        [Display(Name = "Vücut Tipi")]
        public string BodyType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hedef seçimi zorunludur")]
        [Display(Name = "Hedefiniz")]
        public string Goal { get; set; } = string.Empty;

        // Results
        public string? ExerciseRecommendation { get; set; }
        public string? DietPlan { get; set; }
    }

    public class ImageAnalysisViewModel
    {
        [Display(Name = "Fotoğraf")]
        public IFormFile? Image { get; set; }

        public string? AnalysisResult { get; set; }
    }

    public class TargetBodyImageViewModel
    {
        [Display(Name = "Fotoğraf")]
        public IFormFile? Image { get; set; }

        [Required(ErrorMessage = "Kas grubu seçimi zorunludur")]
        [Display(Name = "Hedef Kas Grubu")]
        public string MuscleGroup { get; set; } = string.Empty;

        public string? TargetBodyResult { get; set; }
    }
}




