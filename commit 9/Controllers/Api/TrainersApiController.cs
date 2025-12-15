using FitnessCenterManagement.Data;
using FitnessCenterManagement.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterManagement.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TrainersApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tüm aktif antrenörleri listeler
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetTrainers()
        {
            var trainers = await _context.Trainers
                .Where(t => t.IsActive)
                .Include(t => t.Gym)
                .Include(t => t.TrainerServices)
                    .ThenInclude(ts => ts.Service)
                .Select(t => new
                {
                    t.Id,
                    t.FirstName,
                    t.LastName,
                    FullName = t.FirstName + " " + t.LastName,
                    t.Email,
                    t.Phone,
                    t.Biography,
                    t.Specializations,
                    t.ExperienceYears,
                    t.ImageUrl,
                    GymName = t.Gym != null ? t.Gym.Name : null,
                    Services = t.TrainerServices
                        .Where(ts => ts.Service != null)
                        .Select(ts => new
                        {
                            ts.Service!.Id,
                            ts.Service.Name,
                            ts.Service.Price,
                            ts.Service.DurationMinutes
                        })
                })
                .ToListAsync();

            return Ok(trainers);
        }

        /// <summary>
        /// ID'ye göre antrenör getirir
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetTrainer(int id)
        {
            var trainer = await _context.Trainers
                .Where(t => t.Id == id && t.IsActive)
                .Include(t => t.Gym)
                .Include(t => t.TrainerServices)
                    .ThenInclude(ts => ts.Service)
                .Include(t => t.Availabilities)
                .Select(t => new
                {
                    t.Id,
                    t.FirstName,
                    t.LastName,
                    FullName = t.FirstName + " " + t.LastName,
                    t.Email,
                    t.Phone,
                    t.Biography,
                    t.Specializations,
                    t.ExperienceYears,
                    t.ImageUrl,
                    GymName = t.Gym != null ? t.Gym.Name : null,
                    Services = t.TrainerServices
                        .Where(ts => ts.Service != null)
                        .Select(ts => new
                        {
                            ts.Service!.Id,
                            ts.Service.Name,
                            ts.Service.Price,
                            ts.Service.DurationMinutes
                        }),
                    Availabilities = t.Availabilities
                        .Where(a => a.IsActive)
                        .Select(a => new
                        {
                            a.DayOfWeek,
                            DayName = a.DayName,
                            StartTime = a.StartTime.ToString(@"hh\:mm"),
                            EndTime = a.EndTime.ToString(@"hh\:mm")
                        })
                })
                .FirstOrDefaultAsync();

            if (trainer == null)
            {
                return NotFound(new { message = "Antrenör bulunamadı" });
            }

            return Ok(trainer);
        }

        /// <summary>
        /// Belirli bir tarihte uygun antrenörleri getirir
        /// </summary>
        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<object>>> GetAvailableTrainers(
            [FromQuery] DateTime date,
            [FromQuery] int? serviceId = null)
        {
            var dayOfWeek = date.DayOfWeek;

            var query = _context.Trainers
                .Where(t => t.IsActive)
                .Include(t => t.Availabilities)
                .Include(t => t.TrainerServices)
                .Where(t => t.Availabilities.Any(a => a.DayOfWeek == dayOfWeek && a.IsActive));

            // Filter by service if provided
            if (serviceId.HasValue)
            {
                query = query.Where(t => t.TrainerServices.Any(ts => ts.ServiceId == serviceId.Value));
            }

            var trainers = await query
                .Select(t => new
                {
                    t.Id,
                    t.FirstName,
                    t.LastName,
                    FullName = t.FirstName + " " + t.LastName,
                    t.Specializations,
                    t.ExperienceYears,
                    t.ImageUrl,
                    Availability = t.Availabilities
                        .Where(a => a.DayOfWeek == dayOfWeek && a.IsActive)
                        .Select(a => new
                        {
                            StartTime = a.StartTime.ToString(@"hh\:mm"),
                            EndTime = a.EndTime.ToString(@"hh\:mm")
                        })
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(new
            {
                date = date.ToString("yyyy-MM-dd"),
                dayOfWeek = dayOfWeek.ToString(),
                trainers
            });
        }

        /// <summary>
        /// Uzmanlık alanına göre antrenörleri filtreler
        /// </summary>
        [HttpGet("specialization/{specialization}")]
        public async Task<ActionResult<IEnumerable<object>>> GetTrainersBySpecialization(string specialization)
        {
            var trainers = await _context.Trainers
                .Where(t => t.IsActive && t.Specializations != null && 
                           t.Specializations.ToLower().Contains(specialization.ToLower()))
                .Select(t => new
                {
                    t.Id,
                    t.FirstName,
                    t.LastName,
                    FullName = t.FirstName + " " + t.LastName,
                    t.Specializations,
                    t.ExperienceYears,
                    t.ImageUrl
                })
                .ToListAsync();

            return Ok(trainers);
        }

        /// <summary>
        /// Antrenörün randevularını getirir
        /// </summary>
        [HttpGet("{id}/appointments")]
        public async Task<ActionResult<IEnumerable<object>>> GetTrainerAppointments(
            int id,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] AppointmentStatus? status = null)
        {
            var trainer = await _context.Trainers.FindAsync(id);
            if (trainer == null)
            {
                return NotFound(new { message = "Antrenör bulunamadı" });
            }

            var query = _context.Appointments
                .Where(a => a.TrainerId == id)
                .Include(a => a.Service)
                .AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(a => a.AppointmentDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(a => a.AppointmentDate <= endDate.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            var appointments = await query
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .Select(a => new
                {
                    a.Id,
                    a.AppointmentDate,
                    StartTime = a.StartTime.ToString(@"hh\:mm"),
                    EndTime = a.EndTime.ToString(@"hh\:mm"),
                    ServiceName = a.Service != null ? a.Service.Name : null,
                    a.Price,
                    Status = a.Status.ToString(),
                    a.StatusName
                })
                .ToListAsync();

            return Ok(appointments);
        }
    }
}




