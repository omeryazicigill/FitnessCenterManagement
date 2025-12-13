using FitnessCenterManagement.Data;
using FitnessCenterManagement.Models.Entities;
using FitnessCenterManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterManagement.Controllers
{
    [Authorize]
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            
            // Geçmiş tarihli ve onaylanmış randevuları otomatik olarak "Tamamlandı" yap
            var now = DateTime.Now;
            var pastAppointments = await _context.Appointments
                .Where(a => a.UserId == userId && 
                           a.Status == AppointmentStatus.Approved &&
                           (a.AppointmentDate.Date < now.Date || 
                            (a.AppointmentDate.Date == now.Date && a.EndTime <= now.TimeOfDay)))
                .ToListAsync();

            if (pastAppointments.Any())
            {
                foreach (var appointment in pastAppointments)
                {
                    appointment.Status = AppointmentStatus.Completed;
                    appointment.UpdatedAt = DateTime.Now;
                }
                await _context.SaveChangesAsync();
            }
            
            var appointments = await _context.Appointments
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .Where(a => a.UserId == userId)
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
                    Notes = a.Notes
                })
                .ToListAsync();

            return View(appointments);
        }

        public async Task<IActionResult> Create()
        {
            var viewModel = new AppointmentViewModel
            {
                Trainers = await _context.Trainers.Where(t => t.IsActive).ToListAsync(),
                Services = await _context.Services.Where(s => s.IsActive).ToListAsync(),
                AppointmentDate = DateTime.Today
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var service = await _context.Services.FindAsync(model.ServiceId);
                
                if (service == null)
                {
                    ModelState.AddModelError("", "Hizmet bulunamadı.");
                    return await ReloadCreateView(model);
                }

                // Parse start time
                if (!TimeSpan.TryParse(model.StartTime, out var startTime))
                {
                    ModelState.AddModelError("StartTime", "Geçersiz saat formatı.");
                    return await ReloadCreateView(model);
                }

                var endTime = startTime.Add(TimeSpan.FromMinutes(service.DurationMinutes));

                // Check for conflicting appointments
                var hasConflict = await _context.Appointments
                    .AnyAsync(a => a.TrainerId == model.TrainerId &&
                                   a.AppointmentDate.Date == model.AppointmentDate.Date &&
                                   a.Status != AppointmentStatus.Cancelled &&
                                   a.Status != AppointmentStatus.Rejected &&
                                   ((startTime >= a.StartTime && startTime < a.EndTime) ||
                                    (endTime > a.StartTime && endTime <= a.EndTime) ||
                                    (startTime <= a.StartTime && endTime >= a.EndTime)));

                if (hasConflict)
                {
                    ModelState.AddModelError("", "Bu saat diliminde antrenörün başka bir randevusu bulunmaktadır.");
                    return await ReloadCreateView(model);
                }

                // Check trainer availability
                var dayOfWeek = model.AppointmentDate.DayOfWeek;
                var isAvailable = await _context.TrainerAvailabilities
                    .AnyAsync(ta => ta.TrainerId == model.TrainerId &&
                                    ta.DayOfWeek == dayOfWeek &&
                                    ta.IsActive &&
                                    ta.StartTime <= startTime &&
                                    ta.EndTime >= endTime);

                if (!isAvailable)
                {
                    ModelState.AddModelError("", "Antrenör bu gün ve saatte müsait değildir.");
                    return await ReloadCreateView(model);
                }

                var appointment = new Appointment
                {
                    UserId = userId!,
                    TrainerId = model.TrainerId,
                    ServiceId = model.ServiceId,
                    AppointmentDate = model.AppointmentDate,
                    StartTime = startTime,
                    EndTime = endTime,
                    Price = service.Price,
                    Status = AppointmentStatus.Pending,
                    Notes = model.Notes,
                    CreatedAt = DateTime.Now
                };

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Randevunuz başarıyla oluşturuldu. Onay bekleniyor.";
                return RedirectToAction(nameof(Index));
            }

            return await ReloadCreateView(model);
        }

        private async Task<IActionResult> ReloadCreateView(AppointmentViewModel model)
        {
            model.Trainers = await _context.Trainers.Where(t => t.IsActive).ToListAsync();
            model.Services = await _context.Services.Where(s => s.IsActive).ToListAsync();
            return View("Create", model);
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableTimes(int trainerId, DateTime date)
        {
            var dayOfWeek = date.DayOfWeek;
            
            // Get trainer availability for the day
            var availability = await _context.TrainerAvailabilities
                .Where(ta => ta.TrainerId == trainerId && ta.DayOfWeek == dayOfWeek && ta.IsActive)
                .FirstOrDefaultAsync();

            if (availability == null)
            {
                return Json(new { success = false, message = "Antrenör bu gün müsait değil." });
            }

            // Get existing appointments for the day
            var existingAppointments = await _context.Appointments
                .Where(a => a.TrainerId == trainerId &&
                           a.AppointmentDate.Date == date.Date &&
                           a.Status != AppointmentStatus.Cancelled &&
                           a.Status != AppointmentStatus.Rejected)
                .Select(a => new { a.StartTime, a.EndTime })
                .ToListAsync();

            // Generate available time slots (every 30 minutes)
            var availableTimes = new List<string>();
            var currentTime = availability.StartTime;
            var endTime = availability.EndTime.Subtract(TimeSpan.FromMinutes(30));

            while (currentTime <= endTime)
            {
                var slotEnd = currentTime.Add(TimeSpan.FromMinutes(60));
                
                var isSlotTaken = existingAppointments.Any(a =>
                    (currentTime >= a.StartTime && currentTime < a.EndTime) ||
                    (slotEnd > a.StartTime && slotEnd <= a.EndTime));

                if (!isSlotTaken && (date.Date > DateTime.Today || 
                    (date.Date == DateTime.Today && currentTime > DateTime.Now.TimeOfDay)))
                {
                    availableTimes.Add(currentTime.ToString(@"hh\:mm"));
                }

                currentTime = currentTime.Add(TimeSpan.FromMinutes(30));
            }

            return Json(new { success = true, times = availableTimes });
        }

        [HttpGet]
        public async Task<IActionResult> GetServicesByTrainer(int trainerId)
        {
            var services = await _context.TrainerServices
                .Where(ts => ts.TrainerId == trainerId)
                .Include(ts => ts.Service)
                .Where(ts => ts.Service != null && ts.Service.IsActive)
                .Select(ts => new { ts.Service!.Id, ts.Service.Name, ts.Service.Price, ts.Service.DurationMinutes })
                .ToListAsync();

            return Json(services);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = _userManager.GetUserId(User);
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (appointment == null)
            {
                return NotFound();
            }

            if (appointment.Status == AppointmentStatus.Completed)
            {
                TempData["ErrorMessage"] = "Tamamlanmış randevular iptal edilemez.";
                return RedirectToAction(nameof(Index));
            }

            appointment.Status = AppointmentStatus.Cancelled;
            appointment.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Randevunuz iptal edildi.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");

            var appointment = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.Id == id && (a.UserId == userId || isAdmin));

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }
    }
}




