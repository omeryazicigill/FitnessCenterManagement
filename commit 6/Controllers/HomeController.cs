using FitnessCenterManagement.Data;
using FitnessCenterManagement.Models;
using FitnessCenterManagement.Models.Entities;
using FitnessCenterManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace FitnessCenterManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var gym = await _context.Gyms.FirstOrDefaultAsync();
            var services = await _context.Services.Where(s => s.IsActive).Take(6).ToListAsync();
            var trainers = await _context.Trainers.Where(t => t.IsActive).Take(4).ToListAsync();

            ViewBag.Gym = gym;
            ViewBag.Services = services;
            ViewBag.Trainers = trainers;

            return View();
        }

        public async Task<IActionResult> Services()
        {
            var services = await _context.Services
                .Where(s => s.IsActive)
                .Include(s => s.Gym)
                .ToListAsync();

            return View(services);
        }

        public async Task<IActionResult> Trainers()
        {
            var trainers = await _context.Trainers
                .Where(t => t.IsActive)
                .Include(t => t.Gym)
                .Include(t => t.TrainerServices)
                    .ThenInclude(ts => ts.Service)
                .ToListAsync();

            return View(trainers);
        }

        public async Task<IActionResult> TrainerDetails(int id)
        {
            var trainer = await _context.Trainers
                .Include(t => t.Gym)
                .Include(t => t.TrainerServices)
                    .ThenInclude(ts => ts.Service)
                .Include(t => t.Availabilities)
                .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);

            if (trainer == null)
            {
                return NotFound();
            }

            return View(trainer);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            var userId = _userManager.GetUserId(User);

            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }

            var appointments = await _context.Appointments
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .Where(a => a.UserId == userId)
                .ToListAsync();

            var dashboard = new MemberDashboardViewModel
            {
                TotalAppointments = appointments.Count,
                CompletedAppointments = appointments.Count(a => a.Status == AppointmentStatus.Completed),
                UpcomingAppointments = appointments.Count(a => a.AppointmentDate >= DateTime.Today && 
                    (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Approved)),
                UpcomingAppointmentsList = appointments
                    .Where(a => a.AppointmentDate >= DateTime.Today && 
                        (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Approved))
                    .OrderBy(a => a.AppointmentDate)
                    .ThenBy(a => a.StartTime)
                    .Take(5)
                    .Select(a => new AppointmentListViewModel
                    {
                        Id = a.Id,
                        TrainerName = a.Trainer != null ? a.Trainer.FullName : "",
                        ServiceName = a.Service != null ? a.Service.Name : "",
                        AppointmentDate = a.AppointmentDate,
                        StartTime = a.StartTime,
                        EndTime = a.EndTime,
                        Price = a.Price,
                        Status = a.Status,
                        StatusName = a.StatusName,
                        StatusColor = a.StatusColor
                    })
                    .ToList(),
                RecentAppointments = appointments
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(5)
                    .Select(a => new AppointmentListViewModel
                    {
                        Id = a.Id,
                        TrainerName = a.Trainer != null ? a.Trainer.FullName : "",
                        ServiceName = a.Service != null ? a.Service.Name : "",
                        AppointmentDate = a.AppointmentDate,
                        StartTime = a.StartTime,
                        EndTime = a.EndTime,
                        Price = a.Price,
                        Status = a.Status,
                        StatusName = a.StatusName,
                        StatusColor = a.StatusColor
                    })
                    .ToList()
            };

            return View(dashboard);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
