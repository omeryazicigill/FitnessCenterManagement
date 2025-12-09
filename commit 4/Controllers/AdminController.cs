using FitnessCenterManagement.Data;
using FitnessCenterManagement.Models.Entities;
using FitnessCenterManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var totalMembers = await _userManager.GetUsersInRoleAsync("Member");
            var appointments = await _context.Appointments.ToListAsync();
            
            var dashboard = new DashboardViewModel
            {
                TotalMembers = totalMembers.Count,
                TotalTrainers = await _context.Trainers.CountAsync(t => t.IsActive),
                TotalServices = await _context.Services.CountAsync(s => s.IsActive),
                TotalAppointments = appointments.Count,
                PendingAppointments = appointments.Count(a => a.Status == AppointmentStatus.Pending),
                TodayAppointments = appointments.Count(a => a.AppointmentDate.Date == DateTime.Today),
                TotalRevenue = appointments.Where(a => a.Status == AppointmentStatus.Completed).Sum(a => a.Price),
                MonthlyRevenue = appointments.Where(a => a.Status == AppointmentStatus.Completed && 
                    a.AppointmentDate.Month == DateTime.Now.Month && 
                    a.AppointmentDate.Year == DateTime.Now.Year).Sum(a => a.Price),
                RecentAppointments = await _context.Appointments
                    .Include(a => a.User)
                    .Include(a => a.Trainer)
                    .Include(a => a.Service)
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(10)
                    .Select(a => new AppointmentListViewModel
                    {
                        Id = a.Id,
                        TrainerName = a.Trainer != null ? a.Trainer.FirstName + " " + a.Trainer.LastName : "",
                        ServiceName = a.Service != null ? a.Service.Name : "",
                        AppointmentDate = a.AppointmentDate,
                        StartTime = a.StartTime,
                        EndTime = a.EndTime,
                        Price = a.Price,
                        Status = a.Status,
                        StatusName = a.StatusName,
                        StatusColor = a.StatusColor,
                        UserName = a.User != null ? a.User.FirstName + " " + a.User.LastName : ""
                    })
                    .ToListAsync()
            };

            return View(dashboard);
        }

        #region Gym Management
        public async Task<IActionResult> Gyms()
        {
            var gyms = await _context.Gyms.ToListAsync();
            return View(gyms);
        }

        public IActionResult CreateGym()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGym(Gym gym)
        {
            if (ModelState.IsValid)
            {
                gym.CreatedAt = DateTime.Now;
                _context.Gyms.Add(gym);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Spor salonu başarıyla eklendi.";
                return RedirectToAction(nameof(Gyms));
            }
            return View(gym);
        }

        public async Task<IActionResult> EditGym(int id)
        {
            var gym = await _context.Gyms.FindAsync(id);
            if (gym == null) return NotFound();
            return View(gym);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGym(int id, Gym gym)
        {
            if (id != gym.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(gym);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Spor salonu başarıyla güncellendi.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Gyms.AnyAsync(g => g.Id == id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Gyms));
            }
            return View(gym);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGym(int id)
        {
            var gym = await _context.Gyms.FindAsync(id);
            if (gym != null)
            {
                _context.Gyms.Remove(gym);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Spor salonu başarıyla silindi.";
            }
            return RedirectToAction(nameof(Gyms));
        }
        #endregion

        #region Service Management
        public async Task<IActionResult> Services()
        {
            var services = await _context.Services.Include(s => s.Gym).ToListAsync();
            return View(services);
        }

        public async Task<IActionResult> CreateService()
        {
            ViewBag.Gyms = await _context.Gyms.Where(g => g.IsActive).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateService(Service service)
        {
            if (ModelState.IsValid)
            {
                service.CreatedAt = DateTime.Now;
                _context.Services.Add(service);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Hizmet başarıyla eklendi.";
                return RedirectToAction(nameof(Services));
            }
            ViewBag.Gyms = await _context.Gyms.Where(g => g.IsActive).ToListAsync();
            return View(service);
        }

        public async Task<IActionResult> EditService(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) return NotFound();
            ViewBag.Gyms = await _context.Gyms.Where(g => g.IsActive).ToListAsync();
            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditService(int id, Service service)
        {
            if (id != service.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(service);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Hizmet başarıyla güncellendi.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Services.AnyAsync(s => s.Id == id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Services));
            }
            ViewBag.Gyms = await _context.Gyms.Where(g => g.IsActive).ToListAsync();
            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteService(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Hizmet başarıyla silindi.";
            }
            return RedirectToAction(nameof(Services));
        }
        #endregion

        #region Trainer Management
        public async Task<IActionResult> Trainers()
        {
            var trainers = await _context.Trainers.Include(t => t.Gym).ToListAsync();
            return View(trainers);
        }

        public async Task<IActionResult> CreateTrainer()
        {
            ViewBag.Gyms = await _context.Gyms.Where(g => g.IsActive).ToListAsync();
            ViewBag.Services = await _context.Services.Where(s => s.IsActive).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTrainer(Trainer trainer, int[] selectedServices)
        {
            if (ModelState.IsValid)
            {
                trainer.CreatedAt = DateTime.Now;
                _context.Trainers.Add(trainer);
                await _context.SaveChangesAsync();

                // Add selected services
                if (selectedServices != null)
                {
                    foreach (var serviceId in selectedServices)
                    {
                        _context.TrainerServices.Add(new TrainerService
                        {
                            TrainerId = trainer.Id,
                            ServiceId = serviceId
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Antrenör başarıyla eklendi.";
                return RedirectToAction(nameof(Trainers));
            }
            ViewBag.Gyms = await _context.Gyms.Where(g => g.IsActive).ToListAsync();
            ViewBag.Services = await _context.Services.Where(s => s.IsActive).ToListAsync();
            return View(trainer);
        }

        public async Task<IActionResult> EditTrainer(int id)
        {
            var trainer = await _context.Trainers
                .Include(t => t.TrainerServices)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (trainer == null) return NotFound();
            
            ViewBag.Gyms = await _context.Gyms.Where(g => g.IsActive).ToListAsync();
            ViewBag.Services = await _context.Services.Where(s => s.IsActive).ToListAsync();
            ViewBag.SelectedServices = trainer.TrainerServices.Select(ts => ts.ServiceId).ToList();
            return View(trainer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTrainer(int id, Trainer trainer, int[] selectedServices)
        {
            if (id != trainer.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(trainer);
                    
                    // Update services
                    var existingServices = await _context.TrainerServices
                        .Where(ts => ts.TrainerId == id).ToListAsync();
                    _context.TrainerServices.RemoveRange(existingServices);
                    
                    if (selectedServices != null)
                    {
                        foreach (var serviceId in selectedServices)
                        {
                            _context.TrainerServices.Add(new TrainerService
                            {
                                TrainerId = trainer.Id,
                                ServiceId = serviceId
                            });
                        }
                    }
                    
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Antrenör başarıyla güncellendi.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Trainers.AnyAsync(t => t.Id == id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Trainers));
            }
            ViewBag.Gyms = await _context.Gyms.Where(g => g.IsActive).ToListAsync();
            ViewBag.Services = await _context.Services.Where(s => s.IsActive).ToListAsync();
            return View(trainer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTrainer(int id)
        {
            var trainer = await _context.Trainers.FindAsync(id);
            if (trainer != null)
            {
                _context.Trainers.Remove(trainer);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Antrenör başarıyla silindi.";
            }
            return RedirectToAction(nameof(Trainers));
        }

        public async Task<IActionResult> TrainerAvailability(int id)
        {
            var trainer = await _context.Trainers
                .Include(t => t.Availabilities)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (trainer == null) return NotFound();
            return View(trainer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAvailability(int trainerId, DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime)
        {
            var availability = new TrainerAvailability
            {
                TrainerId = trainerId,
                DayOfWeek = dayOfWeek,
                StartTime = startTime,
                EndTime = endTime,
                IsActive = true
            };
            _context.TrainerAvailabilities.Add(availability);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Müsaitlik saati eklendi.";
            return RedirectToAction(nameof(TrainerAvailability), new { id = trainerId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAvailability(int id, int trainerId)
        {
            var availability = await _context.TrainerAvailabilities.FindAsync(id);
            if (availability != null)
            {
                _context.TrainerAvailabilities.Remove(availability);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Müsaitlik saati silindi.";
            }
            return RedirectToAction(nameof(TrainerAvailability), new { id = trainerId });
        }
        #endregion

        #region Appointment Management
        public async Task<IActionResult> Appointments(AppointmentStatus? status = null)
        {
            var query = _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            var appointments = await query
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.StartTime)
                .Select(a => new AppointmentListViewModel
                {
                    Id = a.Id,
                    TrainerName = a.Trainer != null ? a.Trainer.FirstName + " " + a.Trainer.LastName : "",
                    ServiceName = a.Service != null ? a.Service.Name : "",
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    Price = a.Price,
                    Status = a.Status,
                    StatusName = a.StatusName,
                    StatusColor = a.StatusColor,
                    Notes = a.Notes,
                    UserName = a.User != null ? a.User.FirstName + " " + a.User.LastName : ""
                })
                .ToListAsync();

            ViewBag.CurrentStatus = status;
            return View(appointments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAppointmentStatus(int id, AppointmentStatus status)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.Status = status;
            appointment.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Randevu durumu güncellendi.";
            return RedirectToAction(nameof(Appointments));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Randevu başarıyla silindi.";
            }
            return RedirectToAction(nameof(Appointments));
        }
        #endregion

        #region Member Management
        public async Task<IActionResult> Members()
        {
            var members = await _userManager.GetUsersInRoleAsync("Member");
            return View(members.ToList());
        }

        public async Task<IActionResult> MemberDetails(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var appointments = await _context.Appointments
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .Where(a => a.UserId == id)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            ViewBag.Appointments = appointments;
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMember(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
                TempData["SuccessMessage"] = "Üye başarıyla silindi.";
            }
            return RedirectToAction(nameof(Members));
        }
        #endregion
    }
}




