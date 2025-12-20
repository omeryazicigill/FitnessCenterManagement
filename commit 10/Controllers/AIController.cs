using FitnessCenterManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace FitnessCenterManagement.Controllers
{
    [Authorize]
    public class AIController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AIController> _logger;
        private const string ApiBaseUrl = "https://generativelanguage.googleapis.com/v1beta";
        private string? _cachedModelName;

        public AIController(IConfiguration configuration, ILogger<AIController> logger)
        {
            _configuration = configuration;
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

            // API Key kontrolü
            string? apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_GEMINI_API_KEY_HERE")
            {
                model.ExerciseRecommendation = "<div class='alert alert-danger'>Hata: API Key 'appsettings.json' içinde bulunamadı veya yanlış yapılandırılmış.</div>";
                return View("Recommendation", model);
            }

            try
            {
                // Model seçimi
                string modelId = await GetAvailableModelAsync(apiKey);

                // BMI hesaplama
                double heightInMeters = model.Height / 100.0;
                double bmi = (double)model.Weight / (heightInMeters * heightInMeters);
                string bmiCategory = GetBMICategory(bmi);

                // Prompt oluştur
                string prompt = $@"Sen deneyimli bir fitness antrenörüsün. Aşağıdaki kullanıcı bilgilerine dayanarak, kişiselleştirilmiş ve uygulanabilir bir haftalık antrenman programı hazırla.

KULLANICI BİLGİLERİ:
• Yaş: {model.Age}
• Kilo: {model.Weight} kg
• Boy: {model.Height} cm
• BMI: {bmi:F1} ({bmiCategory})
• Vücut Tipi: {model.BodyType}
• Fitness Hedefi: {model.Goal}

PROGRAM GEREKSİNİMLERİ:
1. Kullanıcının yaş ({model.Age}), kilo ({model.Weight}kg), boy ({model.Height}cm) ve hedefine ({model.Goal}) uygun bir program tasarla
2. BMI ({bmi:F1}) değerine göre antrenman yoğunluğunu ayarla
3. {model.BodyType} vücut tipine uygun egzersiz seçimleri yap
4. Her antrenman günü için farklı ve çeşitli egzersizler planla
5. Yaş ve fitness seviyesine uygun gerçekçi beklentiler oluştur

HAFTALIK PROGRAM YAPISI:
• Her gün için 4-6 egzersiz öner
• Her egzersiz için: set sayısı, tekrar sayısı (veya süre), dinlenme aralıkları
• Haftanın 7 gününü kapsayan detaylı plan
• İlerleme stratejisi ve güvenlik önerileri

GÖRSELLER:
• Her antrenman günü için o gün çalıştırılan ANA KAS GRUBUNUN estetik ve gelişmiş bir fotoğrafını göster
• Görsel URL formatı: <img src='https://image.pollinations.ai/prompt/aesthetic fitness model with developed [KAS_GRUBU_INGILIZCE] muscles, gym lighting, hyper realistic?width=600&height=300&nologo=true' style='width:100%; border-radius:10px; margin-bottom:15px; box-shadow: 0 5px 15px rgba(0,0,0,0.5);' alt='Hedef Vücut' />

YANIT FORMATI: Türkçe, HTML formatında (<h4>, <h5>, <ul>, <li>, <p>, <strong> kullan). Detaylı, motive edici ve profesyonel bir program sun.";

                model.ExerciseRecommendation = await CallGeminiAPIAsync(prompt, apiKey, modelId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Egzersiz önerisi alınırken hata oluştu");
                model.ExerciseRecommendation = $"<div class='alert alert-danger'>Hata oluştu: {ex.Message}</div>";
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

            // API Key kontrolü
            string? apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_GEMINI_API_KEY_HERE")
            {
                model.DietPlan = "<div class='alert alert-danger'>Hata: API Key 'appsettings.json' içinde bulunamadı.</div>";
                return View("Recommendation", model);
            }

            try
            {
                // Model seçimi
                string modelId = await GetAvailableModelAsync(apiKey);

                // BMI ve kalori hesaplama
                double heightInMeters = model.Height / 100.0;
                double bmi = (double)model.Weight / (heightInMeters * heightInMeters);
                string bmiCategory = GetBMICategory(bmi);
                double bmr = 88.362 + (13.397 * (double)model.Weight) + (4.799 * model.Height) - (5.677 * model.Age);
                int dailyCalories = model.Goal switch
                {
                    "Kilo vermek" => (int)(bmr * 1.2) - 500,
                    "Kas geliştirmek" => (int)(bmr * 1.5) + 300,
                    _ => (int)(bmr * 1.3)
                };

                // Prompt oluştur
                string prompt = $@"Sen uzman bir beslenme danışmanısın. Aşağıdaki kullanıcı profiline göre, kişiselleştirilmiş ve sürdürülebilir bir beslenme planı hazırla.

KULLANICI PROFİLİ:
• Yaş: {model.Age}
• Kilo: {model.Weight} kg
• Boy: {model.Height} cm
• BMI: {bmi:F1} ({bmiCategory})
• Vücut Tipi: {model.BodyType}
• Hedef: {model.Goal}
• Önerilen Günlük Kalori: {dailyCalories} kcal

PLAN GEREKSİNİMLERİ:
1. Kullanıcının fiziksel özelliklerine ({model.Age} yaş, {model.Weight}kg, {model.Height}cm) ve hedefine ({model.Goal}) uygun bir plan oluştur
2. BMI ({bmi:F1}) değerine göre kalori ve makro besin dağılımını belirle
3. {model.BodyType} vücut tipine uygun beslenme yaklaşımı uygula
4. Türk mutfağına uygun, pratik ve lezzetli öğünler öner
5. Her öğün için çeşitli ve dengeli seçenekler sun

GÜNLÜK BESLENME İÇERİĞİ:
1. Kalori hedefi ve makro besin dağılımı (protein, karbonhidrat, yağ - gram ve yüzde olarak)
2. Kahvaltı, öğle, akşam ve ara öğünler için detaylı menü önerileri
3. Porsiyon miktarları ve kalori değerleri
4. Su tüketimi önerisi
5. Beslenme ipuçları ve öneriler

YANIT FORMATI: Türkçe, HTML formatında (<h4>, <h5>, <ul>, <li>, <p>, <strong> kullan). Kapsamlı, uygulanabilir ve motivasyonel bir beslenme planı sun.";

                model.DietPlan = await CallGeminiAPIAsync(prompt, apiKey, modelId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Diyet planı alınırken hata oluştu");
                model.DietPlan = $"<div class='alert alert-danger'>Hata oluştu: {ex.Message}</div>";
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

            string? apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_GEMINI_API_KEY_HERE")
            {
                model.AnalysisResult = "<div class='alert alert-danger'>Hata: API Key 'appsettings.json' içinde bulunamadı.</div>";
                return View("ImageAnalysis", model);
            }

            try
            {
                using var memoryStream = new MemoryStream();
                await model.Image.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();
                var base64Image = Convert.ToBase64String(imageBytes);

                string modelId = await GetAvailableModelAsync(apiKey);
                string prompt = @"Bu fotoğrafı bir fitness ve beslenme uzmanı gözüyle analiz et. Aşağıdaki bilgileri Türkçe ve HTML formatında sun:

1. VÜCUT TİPİ TESPİTİ:
   • Vücut tipi kategorisi (Ektomorf/Mezomorf/Endomorf)
   • Fiziksel özellikler

2. VÜCUT KOMPOZİSYONU:
   • Tahmini vücut yağ yüzdesi
   • Kas kütlesi değerlendirmesi
   • Genel fiziksel durum

3. ÖNERİLEN EGZERSİZLER:
   • Uygun antrenman türleri
   • Önerilen antrenman yoğunluğu
   • Önemli notlar

4. BESLENME TAVSİYELERİ:
   • Beslenme stratejisi
   • Makro besin önerileri
   • Özel öneriler

5. GENEL DEĞERLENDİRME:
   • Mevcut durum analizi
   • Potansiyel hedefler
   • Motivasyonel mesaj

HTML formatında düzenle (<h4>, <h5>, <ul>, <li>, <p>, <strong> kullan).";

                model.AnalysisResult = await CallGeminiVisionAPIAsync(prompt, base64Image, apiKey, modelId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Görsel analizi yapılırken hata oluştu");
                model.AnalysisResult = $"<div class='alert alert-danger'>Hata oluştu: {ex.Message}</div>";
            }

            return View("ImageAnalysis", model);
        }

        public IActionResult TargetBodyImage()
        {
            return View(new TargetBodyImageViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetTargetBodyImage(TargetBodyImageViewModel model)
        {
            if (string.IsNullOrEmpty(model.MuscleGroup))
            {
                ModelState.AddModelError("MuscleGroup", "Lütfen bir kas grubu seçin.");
                return View("TargetBodyImage", model);
            }

            string? apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_GEMINI_API_KEY_HERE")
            {
                model.TargetBodyResult = "<div class='alert alert-danger'>Hata: API Key 'appsettings.json' içinde bulunamadı.</div>";
                return View("TargetBodyImage", model);
            }

            string? base64Image = null;

            // Fotoğraf varsa işle
            if (model.Image != null && model.Image.Length > 0)
            {
                // Check file size (max 5MB)
                if (model.Image.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("Image", "Dosya boyutu 5MB'dan küçük olmalıdır.");
                    return View("TargetBodyImage", model);
                }

                // Check file type
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(model.Image.ContentType.ToLower()))
                {
                    ModelState.AddModelError("Image", "Sadece JPEG, PNG veya GIF formatında dosya yükleyebilirsiniz.");
                    return View("TargetBodyImage", model);
                }

                try
                {
                    using var memoryStream = new MemoryStream();
                    await model.Image.CopyToAsync(memoryStream);
                    var imageBytes = memoryStream.ToArray();
                    base64Image = Convert.ToBase64String(imageBytes);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fotoğraf işlenirken hata oluştu");
                    ModelState.AddModelError("Image", "Fotoğraf işlenirken bir hata oluştu.");
                    return View("TargetBodyImage", model);
                }
            }

            try
            {
                string modelId = await GetAvailableModelAsync(apiKey);
                var muscleDescription = GetMuscleGroupDescription(model.MuscleGroup);
                var muscleGroupEnglish = GetMuscleGroupEnglish(model.MuscleGroup);

                string prompt;
                if (base64Image == null)
                {
                    // Fotoğraf yoksa tahmini görsel URL'i ekle
                    prompt = $@"{muscleDescription} kaslarını düzenli ve etkili şekilde çalıştırdıktan sonra vücudun nasıl görüneceğini açık bir şekilde açıkla.

ÖNEMLİ: Cevabının başına şu görseli ekle (HTML formatında):
<img src='https://image.pollinations.ai/prompt/aesthetic fitness model with developed {muscleGroupEnglish} muscles, gym lighting, hyper realistic, professional photography?width=600&height=400&nologo=true' style='width:100%; border-radius:10px; margin-bottom:20px; box-shadow: 0 5px 15px rgba(0,0,0,0.5);' alt='Hedef Vücut Görünümü' />

Aşağıdaki bilgileri Türkçe ve HTML formatında ver:

1. HEDEF GÖRÜNÜM TANIMI:
   • Antrenman sonrası beklenen görsel değişiklikler
   • Kas şekli, boyutu ve definisyonundaki gelişim
   • Vücut proporsiyonlarındaki iyileşmeler
   • Genel estetik gelişim

2. ÖNERİLEN ANTREMAN PROGRAMI:
   • {muscleDescription} için özel egzersizler
   • Set ve tekrar önerileri
   • Haftalık antrenman sıklığı
   • İlerleme planı

3. ZAMAN ÇİZELGESİ:
   • Gerçekçi beklentiler (ay/hafta)
   • Aşamalı gelişim süreci
   • Önemli notlar

4. MOTİVASYON VE YOL HARİTASI:
   • Kişiselleştirilmiş motivasyon
   • Başarı ipuçları
   • Öneriler

HTML formatında düzenle (<h4>, <h5>, <ul>, <li>, <p>, <strong> kullan). Detaylı, gerçekçi ve motive edici bir açıklama yap.";
                }
                else
                {
                    // Fotoğraf varsa mevcut durumu analiz et
                    prompt = $@"Bu fotoğrafı analiz et ve kullanıcıya şunu açık bir şekilde göster: {muscleDescription} kaslarını düzenli ve etkili şekilde çalıştırdıktan sonra vücudu nasıl görünecek?

Aşağıdaki bilgileri Türkçe ve HTML formatında ver:

1. MEVCUT DURUM:
   • {muscleDescription} kaslarının mevcut durumu
   • Güçlü yönler ve gelişim alanları
   • Vücut simetrisi analizi

2. HEDEF GÖRÜNÜM TANIMI:
   • Antrenman sonrası beklenen görsel değişiklikler
   • Kas şekli, boyutu ve definisyonundaki gelişim
   • Vücut proporsiyonlarındaki iyileşmeler
   • Genel estetik gelişim

3. ÖNERİLEN ANTREMAN PROGRAMI:
   • {muscleDescription} için özel egzersizler
   • Set ve tekrar önerileri
   • Haftalık antrenman sıklığı
   • İlerleme planı

4. ZAMAN ÇİZELGESİ:
   • Gerçekçi beklentiler (ay/hafta)
   • Aşamalı gelişim süreci
   • Önemli notlar

5. MOTİVASYON VE YOL HARİTASI:
   • Kişiselleştirilmiş motivasyon
   • Başarı ipuçları
   • Öneriler

HTML formatında düzenle (<h4>, <h5>, <ul>, <li>, <p>, <strong> kullan). Detaylı, gerçekçi ve motive edici bir açıklama yap.";
                }

                // Fotoğraf varsa vision API, yoksa normal API kullan
                if (base64Image != null)
                {
                    model.TargetBodyResult = await CallGeminiVisionAPIAsync(prompt, base64Image, apiKey, modelId);
                }
                else
                {
                    model.TargetBodyResult = await CallGeminiAPIAsync(prompt, apiKey, modelId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hedef görsel analizi yapılırken hata oluştu");
                model.TargetBodyResult = $"<div class='alert alert-danger'>Hata oluştu: {ex.Message}</div>";
            }

            return View("TargetBodyImage", model);
        }

        // Helper Methods
        private async Task<string> GetAvailableModelAsync(string apiKey)
        {
            // Cache'lenmiş model varsa direkt dön
            if (!string.IsNullOrEmpty(_cachedModelName))
            {
                return _cachedModelName;
            }

            // Gemini API dokümantasyonuna göre: gemini-2.5-flash veya gemini-1.5-flash kullanılabilir
            var defaultModel = "gemini-2.5-flash";
            _cachedModelName = defaultModel;
            _logger.LogInformation("Varsayılan model kullanılıyor: {Model}", defaultModel);
            return defaultModel;
        }

        private async Task<List<string>> GetAvailableModelNamesAsync(string apiKey)
        {
            var list = new List<string>();
            string url = $"{ApiBaseUrl}/models?key={apiKey}";

            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        using var jsonDoc = JsonDocument.Parse(json);
                        
                        if (jsonDoc.RootElement.TryGetProperty("models", out var modelsArray))
                        {
                            foreach (var m in modelsArray.EnumerateArray())
                            {
                                if (m.TryGetProperty("name", out var nameElement))
                                {
                                    string name = nameElement.GetString() ?? "";
                                    if (m.TryGetProperty("supportedGenerationMethods", out var methodsArray))
                                    {
                                        var methods = methodsArray.EnumerateArray()
                                            .Select(m => m.GetString())
                                            .Where(m => !string.IsNullOrEmpty(m))
                                            .ToList();

                                        if (methods.Contains("generateContent") && !string.IsNullOrEmpty(name))
                                        {
                                            list.Add(name);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // Hata olursa boş liste döner
                }
            }

            return list;
        }

        private async Task<string> CallGeminiAPIAsync(string prompt, string apiKey, string modelId)
        {
            const int maxRetries = 3;
            const int delayMs = 3000; // 3 saniye bekleme

            // Alternatif modeller - 404 veya 429 hatası alınırsa denenecek
            // Gemini API dokümantasyonuna göre: gemini-2.5-flash önerilen model
            var alternativeModels = new[] { modelId, "gemini-2.5-flash", "gemini-1.5-flash" };

            foreach (var modelToTry in alternativeModels.Distinct())
            {
                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    // Gemini API dokümantasyonuna göre: API key header'da gönderilmeli, query parameter değil
                    string apiUrl = $"{ApiBaseUrl}/models/{modelToTry}:generateContent";

                    var requestBody = new
                    {
                        contents = new[]
                        {
                            new
                            {
                                parts = new[] { new { text = prompt } }
                            }
                        },
                        generationConfig = new
                        {
                            temperature = 1.0,
                            topK = 40,
                            topP = 0.95,
                            maxOutputTokens = 4096
                        }
                    };

                    using (var client = new HttpClient())
                    {
                        // Gemini API dokümantasyonuna göre: API key header'da gönderilmeli
                        client.DefaultRequestHeaders.Clear();
                        client.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);
                        
                        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                        try
                        {
                            _logger.LogInformation("API çağrısı - Model: {Model}, Deneme: {Attempt}/{MaxRetries}", modelToTry, attempt, maxRetries);
                            var response = await client.PostAsync(apiUrl, content);

                            // 429 hatası alınırsa retry yap
                            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                            {
                                if (attempt < maxRetries)
                                {
                                    _logger.LogWarning("429 (Too Many Requests) hatası alındı. {DelayMs}ms sonra tekrar deneniyor...", delayMs * attempt);
                                    await Task.Delay(delayMs * attempt);
                                    continue;
                                }
                                else
                                {
                                    // Son deneme de 429 aldıysa bir sonraki modeli dene
                                    break;
                                }
                            }

                            // 404 hatası alınırsa bir sonraki modeli dene
                            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                            {
                                _logger.LogWarning("404 (Not Found) hatası - Model '{Model}' bulunamadı, bir sonraki model deneniyor...", modelToTry);
                                break;
                            }

                            if (response.IsSuccessStatusCode)
                            {
                                var responseString = await response.Content.ReadAsStringAsync();
                                using var jsonDoc = JsonDocument.Parse(responseString);

                                if (jsonDoc.RootElement.TryGetProperty("candidates", out var candidates) &&
                                    candidates.GetArrayLength() > 0)
                                {
                                    var firstCandidate = candidates[0];
                                    if (firstCandidate.TryGetProperty("content", out var contentObj) &&
                                        contentObj.TryGetProperty("parts", out var parts) &&
                                        parts.GetArrayLength() > 0)
                                    {
                                        var firstPart = parts[0];
                                        if (firstPart.TryGetProperty("text", out var textElement))
                                        {
                                            string aiText = textElement.GetString() ?? "";
                                            // Markdown temizliği
                                            aiText = aiText.Replace("```html", "").Replace("```", "");
                                            // Başarılı modeli cache'le
                                            if (modelToTry != _cachedModelName)
                                            {
                                                _cachedModelName = modelToTry;
                                                _logger.LogInformation("Başarılı! Model '{Model}' cache'lendi.", modelToTry);
                                            }
                                            return aiText;
                                        }
                                    }
                                }

                                return "<div class='alert alert-warning'>Yapay zeka boş cevap döndü. Lütfen tekrar deneyin.</div>";
                            }
                            else
                            {
                                var errorContent = await response.Content.ReadAsStringAsync();
                                _logger.LogWarning("API Hatası: {StatusCode} - {Error}", response.StatusCode, errorContent);
                                
                                // Son model ve son deneme ise hata döndür
                                if (modelToTry == alternativeModels.Last() && attempt == maxRetries)
                                {
                                    return $"<div class='alert alert-danger'>Hata oluştu. Kod: {response.StatusCode}. Lütfen birkaç dakika sonra tekrar deneyin.</div>";
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Bağlantı hatası - Model: {Model}, Deneme: {Attempt}", modelToTry, attempt);
                            
                            // Son model ve son deneme ise hata döndür
                            if (modelToTry == alternativeModels.Last() && attempt == maxRetries)
                            {
                                return $"<div class='alert alert-danger'>Bağlantı hatası: {ex.Message}. Lütfen daha sonra tekrar deneyin.</div>";
                            }
                            
                            if (attempt < maxRetries)
                            {
                                await Task.Delay(delayMs * attempt);
                            }
                        }
                    }
                }
            }

            return "<div class='alert alert-danger'>Tüm modeller denenmesine rağmen AI servisi ile iletişim kurulamadı. Lütfen birkaç dakika sonra tekrar deneyin.</div>";
        }

        private async Task<string> CallGeminiVisionAPIAsync(string prompt, string imageBase64, string apiKey, string modelId)
        {
            const int maxRetries = 3;
            const int delayMs = 3000;

            // Vision için uygun modeller (gemini-2.5-flash ve gemini-1.5-flash görsel destekliyor)
            var alternativeModels = new[] { modelId, "gemini-2.5-flash", "gemini-1.5-flash" };

            foreach (var modelToTry in alternativeModels.Distinct())
            {
                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    // Gemini API dokümantasyonuna göre: API key header'da gönderilmeli, query parameter değil
                    string apiUrl = $"{ApiBaseUrl}/models/{modelToTry}:generateContent";

                    var parts = new List<object> { new { text = prompt } };
                    
                    // Görsel için base64 data ekle
                    string mimeType = "image/jpeg";
                    parts.Add(new { inline_data = new { mime_type = mimeType, data = imageBase64 } });

                    var requestBody = new
                    {
                        contents = new[]
                        {
                            new
                            {
                                parts = parts.ToArray()
                            }
                        },
                        generationConfig = new
                        {
                            temperature = 1.0,
                            topK = 40,
                            topP = 0.95,
                            maxOutputTokens = 4096
                        }
                    };

                    using (var client = new HttpClient())
                    {
                        // Gemini API dokümantasyonuna göre: API key header'da gönderilmeli
                        client.DefaultRequestHeaders.Clear();
                        client.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);
                        
                        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                        try
                        {
                            _logger.LogInformation("Vision API çağrısı - Model: {Model}, Deneme: {Attempt}/{MaxRetries}", modelToTry, attempt, maxRetries);
                            var response = await client.PostAsync(apiUrl, content);

                            // 429 hatası alınırsa retry yap
                            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                            {
                                if (attempt < maxRetries)
                                {
                                    _logger.LogWarning("429 (Too Many Requests) hatası alındı. {DelayMs}ms sonra tekrar deneniyor...", delayMs * attempt);
                                    await Task.Delay(delayMs * attempt);
                                    continue;
                                }
                                else
                                {
                                    break;
                                }
                            }

                            // 404 hatası alınırsa bir sonraki modeli dene
                            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                            {
                                _logger.LogWarning("404 (Not Found) hatası - Model '{Model}' bulunamadı, bir sonraki model deneniyor...", modelToTry);
                                break;
                            }

                            if (response.IsSuccessStatusCode)
                            {
                                var responseString = await response.Content.ReadAsStringAsync();
                                using var jsonDoc = JsonDocument.Parse(responseString);

                                if (jsonDoc.RootElement.TryGetProperty("candidates", out var candidates) &&
                                    candidates.GetArrayLength() > 0)
                                {
                                    var firstCandidate = candidates[0];
                                    if (firstCandidate.TryGetProperty("content", out var contentObj) &&
                                        contentObj.TryGetProperty("parts", out var partsArray) &&
                                        partsArray.GetArrayLength() > 0)
                                    {
                                        var firstPart = partsArray[0];
                                        if (firstPart.TryGetProperty("text", out var textElement))
                                        {
                                            string aiText = textElement.GetString() ?? "";
                                            // Markdown temizliği
                                            aiText = aiText.Replace("```html", "").Replace("```", "");
                                            return aiText;
                                        }
                                    }
                                }

                                return "<div class='alert alert-warning'>Yapay zeka boş cevap döndü. Lütfen tekrar deneyin.</div>";
                            }
                            else
                            {
                                var errorContent = await response.Content.ReadAsStringAsync();
                                _logger.LogWarning("Vision API Hatası: {StatusCode} - {Error}", response.StatusCode, errorContent);
                                
                                if (modelToTry == alternativeModels.Last() && attempt == maxRetries)
                                {
                                    return $"<div class='alert alert-danger'>Hata oluştu. Kod: {response.StatusCode}. Lütfen birkaç dakika sonra tekrar deneyin.</div>";
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Vision bağlantı hatası - Model: {Model}, Deneme: {Attempt}", modelToTry, attempt);
                            
                            if (modelToTry == alternativeModels.Last() && attempt == maxRetries)
                            {
                                return $"<div class='alert alert-danger'>Bağlantı hatası: {ex.Message}. Lütfen daha sonra tekrar deneyin.</div>";
                            }
                            
                            if (attempt < maxRetries)
                            {
                                await Task.Delay(delayMs * attempt);
                            }
                        }
                    }
                }
            }

            return "<div class='alert alert-danger'>Tüm modeller denenmesine rağmen görsel analiz servisi ile iletişim kurulamadı. Lütfen birkaç dakika sonra tekrar deneyin.</div>";
        }

        private string GetBMICategory(double bmi)
        {
            return bmi switch
            {
                < 18.5 => "Zayıf",
                < 25 => "Normal",
                < 30 => "Fazla Kilolu",
                _ => "Obez"
            };
        }

        private string GetMuscleGroupDescription(string muscleGroup)
        {
            var descriptions = new Dictionary<string, string>
            {
                { "Üst Gövde", "göğüs, omuz, sırt, kollar ve karın" },
                { "Alt Gövde", "kalça, uyluk, bacak ve baldır" },
                { "Göğüs", "pektoral (göğüs)" },
                { "Sırt", "latissimus dorsi, rhomboid ve trapez (sırt)" },
                { "Omuz", "deltoid (omuz)" },
                { "Kollar", "biceps ve triceps (kol)" },
                { "Karın", "abdominal (karın)" },
                { "Bacaklar", "quadriceps, hamstring ve baldır (bacak)" },
                { "Kalça", "gluteal (kalça)" }
            };

            return descriptions.ContainsKey(muscleGroup) ? descriptions[muscleGroup] : muscleGroup.ToLower();
        }

        private string GetMuscleGroupEnglish(string muscleGroup)
        {
            var englishNames = new Dictionary<string, string>
            {
                { "Üst Gövde", "chest, shoulders, back, arms and abs" },
                { "Alt Gövde", "glutes, thighs, legs and calves" },
                { "Göğüs", "chest muscles" },
                { "Sırt", "back muscles" },
                { "Omuz", "shoulder muscles" },
                { "Kollar", "arm muscles" },
                { "Karın", "abdominal muscles" },
                { "Bacaklar", "leg muscles" },
                { "Kalça", "gluteal muscles" }
            };

            return englishNames.ContainsKey(muscleGroup) ? englishNames[muscleGroup] : muscleGroup.ToLower();
        }
    }
}
