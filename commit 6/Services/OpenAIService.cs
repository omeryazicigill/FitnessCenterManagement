using System.Text;
using System.Text.Json;

namespace FitnessCenterManagement.Services
{
    public class OpenAIService : IAIService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ILogger<OpenAIService> _logger;

        public OpenAIService(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<OpenAIService> logger)
        {
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
        }

        public async Task<string> GetExerciseRecommendationAsync(int age, decimal weight, int height, string bodyType, string goal)
        {
            var prompt = $@"Sen bir profesyonel fitness antrenörüsün. Aşağıdaki bilgilere göre kişiye özel egzersiz programı öner:

Yaş: {age}
Kilo: {weight} kg
Boy: {height} cm
Vücut Tipi: {bodyType}
Hedef: {goal}

Lütfen haftalık bir egzersiz programı oluştur. Program şunları içersin:
1. Haftanın her günü için egzersizler
2. Set ve tekrar sayıları
3. Dinlenme süreleri
4. Önemli ipuçları

Yanıtı Türkçe olarak ver ve HTML formatında düzenle (<h4>, <ul>, <li>, <p>, <strong> etiketleri kullanabilirsin).";

            return await SendChatRequestAsync(prompt);
        }

        public async Task<string> GetDietPlanAsync(int age, decimal weight, int height, string bodyType, string goal)
        {
            var prompt = $@"Sen bir profesyonel beslenme uzmanısın. Aşağıdaki bilgilere göre kişiye özel beslenme planı oluştur:

Yaş: {age}
Kilo: {weight} kg
Boy: {height} cm
Vücut Tipi: {bodyType}
Hedef: {goal}

Lütfen günlük bir beslenme planı oluştur. Plan şunları içersin:
1. Günlük kalori ihtiyacı
2. Makro besin dağılımı (protein, karbonhidrat, yağ)
3. Öğünler ve örnek yemekler
4. Su tüketimi önerisi
5. Beslenme ipuçları

Yanıtı Türkçe olarak ver ve HTML formatında düzenle (<h4>, <ul>, <li>, <p>, <strong> etiketleri kullanabilirsin).";

            return await SendChatRequestAsync(prompt);
        }

        public async Task<string> AnalyzeImageAsync(string imageBase64)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            
            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_OPENAI_API_KEY_HERE")
            {
                return GetMockImageAnalysis();
            }

            try
            {
                var requestBody = new
                {
                    model = "gpt-4o",
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = new object[]
                            {
                                new { type = "text", text = @"Bu fotoğrafı analiz et ve kişinin vücut tipini belirle. 
                                Aşağıdaki bilgileri Türkçe olarak ver:
                                1. Tahmini vücut tipi (ektomorf, mezomorf, endomorf)
                                2. Tahmini vücut yağ oranı
                                3. Önerilen egzersiz türleri
                                4. Beslenme önerileri
                                
                                Yanıtı HTML formatında düzenle." },
                                new { type = "image_url", image_url = new { url = $"data:image/jpeg;base64,{imageBase64}" } }
                            }
                        }
                    },
                    max_tokens = 1000
                };

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var response = await _httpClient.PostAsync(
                    "https://api.openai.com/v1/chat/completions",
                    new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
                );

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var jsonDoc = JsonDocument.Parse(responseContent);
                    return jsonDoc.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString() ?? GetMockImageAnalysis();
                }
                else
                {
                    _logger.LogWarning("OpenAI API hatası: {StatusCode}", response.StatusCode);
                    return GetMockImageAnalysis();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Görsel analiz sırasında hata oluştu");
                return GetMockImageAnalysis();
            }
        }

        private async Task<string> SendChatRequestAsync(string prompt)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            
            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_OPENAI_API_KEY_HERE")
            {
                return GetMockResponse(prompt);
            }

            try
            {
                var requestBody = new
                {
                    model = "gpt-4o-mini",
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    },
                    max_tokens = 2000,
                    temperature = 0.7
                };

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var response = await _httpClient.PostAsync(
                    "https://api.openai.com/v1/chat/completions",
                    new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
                );

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var jsonDoc = JsonDocument.Parse(responseContent);
                    return jsonDoc.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString() ?? GetMockResponse(prompt);
                }
                else
                {
                    _logger.LogWarning("OpenAI API hatası: {StatusCode}", response.StatusCode);
                    return GetMockResponse(prompt);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI isteği sırasında hata oluştu");
                return GetMockResponse(prompt);
            }
        }

        private string GetMockResponse(string prompt)
        {
            if (prompt.Contains("egzersiz"))
            {
                return @"<h4>🏋️ Kişisel Egzersiz Programınız</h4>
                <p><strong>Haftalık Program Özeti</strong></p>
                
                <h5>📅 Pazartesi - Göğüs & Triceps</h5>
                <ul>
                    <li>Bench Press: 4 set x 10 tekrar</li>
                    <li>İncline Dumbbell Press: 3 set x 12 tekrar</li>
                    <li>Cable Fly: 3 set x 15 tekrar</li>
                    <li>Triceps Pushdown: 3 set x 12 tekrar</li>
                    <li>Overhead Triceps Extension: 3 set x 12 tekrar</li>
                </ul>
                
                <h5>📅 Salı - Sırt & Biceps</h5>
                <ul>
                    <li>Lat Pulldown: 4 set x 10 tekrar</li>
                    <li>Barbell Row: 4 set x 10 tekrar</li>
                    <li>Seated Cable Row: 3 set x 12 tekrar</li>
                    <li>Barbell Curl: 3 set x 12 tekrar</li>
                    <li>Hammer Curl: 3 set x 12 tekrar</li>
                </ul>
                
                <h5>📅 Çarşamba - Dinlenme veya Kardiyo</h5>
                <ul>
                    <li>30 dakika orta tempo koşu veya yürüyüş</li>
                    <li>Esneme hareketleri</li>
                </ul>
                
                <h5>📅 Perşembe - Omuz & Karın</h5>
                <ul>
                    <li>Military Press: 4 set x 10 tekrar</li>
                    <li>Lateral Raise: 3 set x 15 tekrar</li>
                    <li>Front Raise: 3 set x 12 tekrar</li>
                    <li>Plank: 3 set x 45 saniye</li>
                    <li>Crunch: 3 set x 20 tekrar</li>
                </ul>
                
                <h5>📅 Cuma - Bacak</h5>
                <ul>
                    <li>Squat: 4 set x 10 tekrar</li>
                    <li>Leg Press: 4 set x 12 tekrar</li>
                    <li>Romanian Deadlift: 3 set x 10 tekrar</li>
                    <li>Leg Curl: 3 set x 12 tekrar</li>
                    <li>Calf Raise: 4 set x 15 tekrar</li>
                </ul>
                
                <h5>💡 Önemli İpuçları</h5>
                <ul>
                    <li>Her egzersiz öncesi 5-10 dakika ısınma yapın</li>
                    <li>Setler arası 60-90 saniye dinlenin</li>
                    <li>Haftada en az 2 gün dinlenin</li>
                    <li>Bol su için ve düzenli uyuyun</li>
                </ul>";
            }
            else
            {
                return @"<h4>🥗 Kişisel Beslenme Planınız</h4>
                
                <h5>📊 Günlük Hedefler</h5>
                <ul>
                    <li><strong>Kalori:</strong> 2000-2200 kcal</li>
                    <li><strong>Protein:</strong> 150g (Toplam kalorinin %30)</li>
                    <li><strong>Karbonhidrat:</strong> 200g (Toplam kalorinin %40)</li>
                    <li><strong>Yağ:</strong> 65g (Toplam kalorinin %30)</li>
                    <li><strong>Su:</strong> Günde en az 2.5-3 litre</li>
                </ul>
                
                <h5>🌅 Kahvaltı (07:00-08:00)</h5>
                <ul>
                    <li>3 yumurta (haşlanmış veya omlet)</li>
                    <li>2 dilim tam buğday ekmeği</li>
                    <li>1 avuç ceviz veya badem</li>
                    <li>1 porsiyon meyve</li>
                </ul>
                
                <h5>🍎 Ara Öğün (10:00-10:30)</h5>
                <ul>
                    <li>1 porsiyon yoğurt</li>
                    <li>1 muz veya elma</li>
                </ul>
                
                <h5>🍽️ Öğle Yemeği (12:30-13:30)</h5>
                <ul>
                    <li>150g ızgara tavuk veya balık</li>
                    <li>1 porsiyon pilav veya makarna</li>
                    <li>Bol yeşil salata</li>
                    <li>1 yemek kaşığı zeytinyağı</li>
                </ul>
                
                <h5>🥜 Ara Öğün (15:30-16:00)</h5>
                <ul>
                    <li>1 scoop protein tozu (opsiyonel)</li>
                    <li>1 avuç kuruyemiş</li>
                </ul>
                
                <h5>🍲 Akşam Yemeği (19:00-20:00)</h5>
                <ul>
                    <li>150g kırmızı et veya balık</li>
                    <li>Sebze yemeği</li>
                    <li>1 kase yoğurt</li>
                </ul>
                
                <h5>💡 Beslenme İpuçları</h5>
                <ul>
                    <li>Öğünlerinizi düzenli saatlerde yiyin</li>
                    <li>İşlenmiş gıdalardan kaçının</li>
                    <li>Şekerli içecekleri su ile değiştirin</li>
                    <li>Yemekleri yavaş yiyin</li>
                </ul>";
            }
        }

        private string GetMockImageAnalysis()
        {
            return @"<h4>📸 Görsel Analiz Sonuçları</h4>
            
            <h5>🎯 Vücut Tipi Tahmini</h5>
            <p><strong>Mezomorf</strong> - Orta yapılı, atletik vücut tipi</p>
            
            <h5>📊 Tahmini Değerler</h5>
            <ul>
                <li><strong>Vücut Yağ Oranı:</strong> %18-22 (Normal aralıkta)</li>
                <li><strong>Kas Kütlesi:</strong> Orta seviye</li>
                <li><strong>Genel Durum:</strong> Sağlıklı</li>
            </ul>
            
            <h5>🏋️ Önerilen Egzersiz Türleri</h5>
            <ul>
                <li>Ağırlık antrenmanı (haftada 3-4 gün)</li>
                <li>HIIT kardiyo (haftada 2 gün)</li>
                <li>Esneklik çalışmaları</li>
            </ul>
            
            <h5>🥗 Beslenme Önerileri</h5>
            <ul>
                <li>Yüksek proteinli beslenme</li>
                <li>Kompleks karbonhidratlar tercih edin</li>
                <li>Sağlıklı yağları ihmal etmeyin</li>
                <li>Bol su tüketin</li>
            </ul>
            
            <p class='text-muted mt-3'><em>Not: Bu analiz yapay zeka tarafından yapılan tahmini bir değerlendirmedir. Kesin sonuçlar için bir sağlık uzmanına danışınız.</em></p>";
        }
    }
}




