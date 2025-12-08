using FitnessCenterManagement.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace FitnessCenterManagement.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Create roles
            string[] roles = { "Admin", "Member" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Create admin user
            var adminEmail = "b231210383@sakarya.edu.tr";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Ömer",
                    LastName = "Yazıcıgil",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.Now
                };

                var result = await userManager.CreateAsync(adminUser, "sau");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Seed Gym data if not exists
            if (!context.Gyms.Any())
            {
                var gym = new Gym
                {
                    Name = "FitLife Spor Merkezi",
                    Address = "Sakarya Üniversitesi Kampüsü, Esentepe, 54050 Serdivan/Sakarya",
                    Phone = "0264 123 45 67",
                    Email = "info@fitlife.com",
                    OpeningTime = new TimeSpan(6, 0, 0),
                    ClosingTime = new TimeSpan(23, 0, 0),
                    Description = "Modern ekipmanlar ve profesyonel eğitmenler ile spor deneyiminizi en üst seviyeye çıkarın.",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };
                context.Gyms.Add(gym);
                await context.SaveChangesAsync();

                // Seed Services
                var services = new List<Service>
                {
                    new Service
                    {
                        Name = "Kişisel Antrenman",
                        Description = "Birebir antrenör eşliğinde kişiselleştirilmiş egzersiz programı",
                        DurationMinutes = 60,
                        Price = 500,
                        Category = ServiceCategory.PersonalTraining,
                        GymId = gym.Id,
                        IsActive = true
                    },
                    new Service
                    {
                        Name = "Yoga Dersi",
                        Description = "Zihin ve beden uyumunu sağlayan yoga seansları",
                        DurationMinutes = 60,
                        Price = 200,
                        Category = ServiceCategory.Yoga,
                        GymId = gym.Id,
                        IsActive = true
                    },
                    new Service
                    {
                        Name = "Pilates",
                        Description = "Core kaslarını güçlendiren pilates dersleri",
                        DurationMinutes = 45,
                        Price = 250,
                        Category = ServiceCategory.Pilates,
                        GymId = gym.Id,
                        IsActive = true
                    },
                    new Service
                    {
                        Name = "Fitness (Grup)",
                        Description = "Grup halinde fitness aktiviteleri",
                        DurationMinutes = 60,
                        Price = 150,
                        Category = ServiceCategory.Fitness,
                        GymId = gym.Id,
                        IsActive = true
                    },
                    new Service
                    {
                        Name = "Kardiyo Programı",
                        Description = "Yağ yakımı ve dayanıklılık için kardiyo egzersizleri",
                        DurationMinutes = 45,
                        Price = 180,
                        Category = ServiceCategory.Cardio,
                        GymId = gym.Id,
                        IsActive = true
                    },
                    new Service
                    {
                        Name = "CrossFit",
                        Description = "Yüksek yoğunluklu fonksiyonel fitness programı",
                        DurationMinutes = 60,
                        Price = 300,
                        Category = ServiceCategory.CrossFit,
                        GymId = gym.Id,
                        IsActive = true
                    },
                    new Service
                    {
                        Name = "Boks Antrenmanı",
                        Description = "Boks teknikleri ve kondisyon geliştirme",
                        DurationMinutes = 60,
                        Price = 350,
                        Category = ServiceCategory.Boxing,
                        GymId = gym.Id,
                        IsActive = true
                    }
                };
                context.Services.AddRange(services);
                await context.SaveChangesAsync();

                // Seed Trainers
                var trainers = new List<Trainer>
                {
                    new Trainer
                    {
                        FirstName = "Ahmet",
                        LastName = "Yılmaz",
                        Email = "ahmet.yilmaz@fitlife.com",
                        Phone = "0532 111 22 33",
                        Biography = "10 yıllık deneyime sahip fitness ve vücut geliştirme uzmanı.",
                        Specializations = "Kas Geliştirme, Kilo Verme, Fitness",
                        ExperienceYears = 10,
                        GymId = gym.Id,
                        IsActive = true
                    },
                    new Trainer
                    {
                        FirstName = "Ayşe",
                        LastName = "Kaya",
                        Email = "ayse.kaya@fitlife.com",
                        Phone = "0533 222 33 44",
                        Biography = "Sertifikalı yoga eğitmeni ve wellness danışmanı.",
                        Specializations = "Yoga, Pilates, Meditasyon",
                        ExperienceYears = 8,
                        GymId = gym.Id,
                        IsActive = true
                    },
                    new Trainer
                    {
                        FirstName = "Mehmet",
                        LastName = "Demir",
                        Email = "mehmet.demir@fitlife.com",
                        Phone = "0534 333 44 55",
                        Biography = "Eski milli sporcu, CrossFit ve fonksiyonel antrenman uzmanı.",
                        Specializations = "CrossFit, Kardiyo, Fonksiyonel Antrenman",
                        ExperienceYears = 12,
                        GymId = gym.Id,
                        IsActive = true
                    },
                    new Trainer
                    {
                        FirstName = "Zeynep",
                        LastName = "Çelik",
                        Email = "zeynep.celik@fitlife.com",
                        Phone = "0535 444 55 66",
                        Biography = "Pilates ve rehabilitasyon uzmanı.",
                        Specializations = "Pilates, Rehabilitasyon, Esneklik",
                        ExperienceYears = 6,
                        GymId = gym.Id,
                        IsActive = true
                    },
                    new Trainer
                    {
                        FirstName = "Can",
                        LastName = "Öztürk",
                        Email = "can.ozturk@fitlife.com",
                        Phone = "0536 555 66 77",
                        Biography = "Profesyonel boks antrenörü ve kondisyoner.",
                        Specializations = "Boks, Kickboks, Kondisyon",
                        ExperienceYears = 15,
                        GymId = gym.Id,
                        IsActive = true
                    }
                };
                context.Trainers.AddRange(trainers);
                await context.SaveChangesAsync();

                // Seed Trainer Availabilities
                foreach (var trainer in trainers)
                {
                    var availabilities = new List<TrainerAvailability>
                    {
                        new TrainerAvailability { TrainerId = trainer.Id, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(17, 0, 0), IsActive = true },
                        new TrainerAvailability { TrainerId = trainer.Id, DayOfWeek = DayOfWeek.Tuesday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(17, 0, 0), IsActive = true },
                        new TrainerAvailability { TrainerId = trainer.Id, DayOfWeek = DayOfWeek.Wednesday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(17, 0, 0), IsActive = true },
                        new TrainerAvailability { TrainerId = trainer.Id, DayOfWeek = DayOfWeek.Thursday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(17, 0, 0), IsActive = true },
                        new TrainerAvailability { TrainerId = trainer.Id, DayOfWeek = DayOfWeek.Friday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(17, 0, 0), IsActive = true },
                    };
                    context.TrainerAvailabilities.AddRange(availabilities);
                }
                await context.SaveChangesAsync();

                // Seed TrainerServices
                var allTrainers = context.Trainers.ToList();
                var allServices = context.Services.ToList();
                
                // Ahmet - Fitness, Personal Training
                context.TrainerServices.Add(new TrainerService { TrainerId = allTrainers[0].Id, ServiceId = allServices.First(s => s.Category == ServiceCategory.PersonalTraining).Id });
                context.TrainerServices.Add(new TrainerService { TrainerId = allTrainers[0].Id, ServiceId = allServices.First(s => s.Category == ServiceCategory.Fitness).Id });
                
                // Ayşe - Yoga, Pilates
                context.TrainerServices.Add(new TrainerService { TrainerId = allTrainers[1].Id, ServiceId = allServices.First(s => s.Category == ServiceCategory.Yoga).Id });
                context.TrainerServices.Add(new TrainerService { TrainerId = allTrainers[1].Id, ServiceId = allServices.First(s => s.Category == ServiceCategory.Pilates).Id });
                
                // Mehmet - CrossFit, Cardio
                context.TrainerServices.Add(new TrainerService { TrainerId = allTrainers[2].Id, ServiceId = allServices.First(s => s.Category == ServiceCategory.CrossFit).Id });
                context.TrainerServices.Add(new TrainerService { TrainerId = allTrainers[2].Id, ServiceId = allServices.First(s => s.Category == ServiceCategory.Cardio).Id });
                
                // Zeynep - Pilates
                context.TrainerServices.Add(new TrainerService { TrainerId = allTrainers[3].Id, ServiceId = allServices.First(s => s.Category == ServiceCategory.Pilates).Id });
                
                // Can - Boxing
                context.TrainerServices.Add(new TrainerService { TrainerId = allTrainers[4].Id, ServiceId = allServices.First(s => s.Category == ServiceCategory.Boxing).Id });
                
                await context.SaveChangesAsync();
            }
        }
    }
}




