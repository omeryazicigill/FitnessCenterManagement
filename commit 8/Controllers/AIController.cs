using FitnessCenterManagement.Services;
using FitnessCenterManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCenterManagement.Controllers
{
    [Authorize]
    public class AIController : Controller
    {
        private readonly IAIService _aiService;
        private readonly ILogger<AIController> _logger;

        public AIController(IAIService aiService, ILogger<AIController> logger)
        {
            _aiService = aiService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Recommendation()
        {
            return View(new AIRecommendationViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetExerciseRecommendation(AIRecommendationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Recommendation", model);
            }

            try
            {
                model.ExerciseRecommendation = await _aiService.GetExerciseRecommendationAsync(
                    model.Age, model.Weight, model.Height, model.BodyType, model.Goal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Egzersiz önerisi alınırken hata oluştu");
                ModelState.AddModelError("", "Öneri alınırken bir hata oluştu. Lütfen tekrar deneyin.");
            }

            return View("Recommendation", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetDietPlan(AIRecommendationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Recommendation", model);
            }

            try
            {
                model.DietPlan = await _aiService.GetDietPlanAsync(
                    model.Age, model.Weight, model.Height, model.BodyType, model.Goal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Diyet planı alınırken hata oluştu");
                ModelState.AddModelError("", "Plan alınırken bir hata oluştu. Lütfen tekrar deneyin.");
            }

            return View("Recommendation", model);
        }

        public IActionResult ImageAnalysis()
        {
            return View(new ImageAnalysisViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnalyzeImage(ImageAnalysisViewModel model)
        {
            if (model.Image == null || model.Image.Length == 0)
            {
                ModelState.AddModelError("Image", "Lütfen bir fotoğraf yükleyin.");
                return View("ImageAnalysis", model);
            }

            // Check file size (max 5MB)
            if (model.Image.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError("Image", "Dosya boyutu 5MB'dan küçük olmalıdır.");
                return View("ImageAnalysis", model);
            }

            // Check file type
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
            if (!allowedTypes.Contains(model.Image.ContentType.ToLower()))
            {
                ModelState.AddModelError("Image", "Sadece JPEG, PNG veya GIF formatında dosya yükleyebilirsiniz.");
                return View("ImageAnalysis", model);
            }

            try
            {
                using var memoryStream = new MemoryStream();
                await model.Image.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();
                var base64Image = Convert.ToBase64String(imageBytes);

                model.AnalysisResult = await _aiService.AnalyzeImageAsync(base64Image);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Görsel analizi yapılırken hata oluştu");
                ModelState.AddModelError("", "Analiz yapılırken bir hata oluştu. Lütfen tekrar deneyin.");
            }

            return View("ImageAnalysis", model);
        }
    }
}




