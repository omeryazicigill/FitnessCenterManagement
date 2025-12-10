using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterManagement.Models.Entities
{
    // Many-to-Many relationship between Trainer and Service
    public class TrainerService
    {
        public int Id { get; set; }

        public int TrainerId { get; set; }

        public int ServiceId { get; set; }

        // Navigation Properties
        [ForeignKey("TrainerId")]
        public virtual Trainer? Trainer { get; set; }

        [ForeignKey("ServiceId")]
        public virtual Service? Service { get; set; }
    }
}




