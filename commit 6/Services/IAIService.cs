namespace FitnessCenterManagement.Services
{
    public interface IAIService
    {
        Task<string> GetExerciseRecommendationAsync(int age, decimal weight, int height, string bodyType, string goal);
        Task<string> GetDietPlanAsync(int age, decimal weight, int height, string bodyType, string goal);
        Task<string> AnalyzeImageAsync(string imageBase64);
    }
}




