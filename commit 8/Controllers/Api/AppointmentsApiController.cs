using FitnessCenterManagement.Data;
using FitnessCenterManagement.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterManagement.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tüm randevuları listeler (filtreleme destekli)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAppointments(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int? trainerId = null,
            [FromQuery] AppointmentStatus? status = null)
        {
            var query = _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .AsQueryable();

            // Apply LINQ filters
            if (startDate.HasValue)
            {
                query = query.Where(a => a.AppointmentDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(a => a.AppointmentDate <= endDate.Value);
            }

            if (trainerId.HasValue)
            {
                query = query.Where(a => a.TrainerId == trainerId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            var appointments = await query
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.StartTime)
                .Select(a => new
                {
                    a.Id,
                    a.AppointmentDate,
                    StartTime = a.StartTime.ToString(@"hh\:mm"),
                    EndTime = a.EndTime.ToString(@"hh\:mm"),
                    TrainerName = a.Trainer != null ? a.Trainer.FirstName + " " + a.Trainer.LastName : null,
                    ServiceName = a.Service != null ? a.Service.Name : null,
                    MemberName = a.User != null ? a.User.FirstName + " " + a.User.LastName : null,
                    a.Price,
                    Status = a.Status.ToString(),
                    a.StatusName,
                    a.StatusColor,
                    a.Notes,
                    a.CreatedAt
                })
                .ToListAsync();

            return Ok(appointments);
        }

        /// <summary>
        /// ID'ye göre randevu getirir
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetAppointment(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .Where(a => a.Id == id)
                .Select(a => new
                {
                    a.Id,
                    a.AppointmentDate,
                    StartTime = a.StartTime.ToString(@"hh\:mm"),
                    EndTime = a.EndTime.ToString(@"hh\:mm"),
                    Trainer = a.Trainer != null ? new
                    {
                        a.Trainer.Id,
                        a.Trainer.FirstName,
                        a.Trainer.LastName,
                        FullName = a.Trainer.FirstName + " " + a.Trainer.LastName
                    } : null,
                    Service = a.Service != null ? new
                    {
                        a.Service.Id,
                        a.Service.Name,
                        a.Service.DurationMinutes
                    } : null,
                    Member = a.User != null ? new
                    {
                        a.User.Id,
                        a.User.FirstName,
                        a.User.LastName,
                        FullName = a.User.FirstName + " " + a.User.LastName,
                        a.User.Email
                    } : null,
                    a.Price,
                    Status = a.Status.ToString(),
                    a.StatusName,
                    a.Notes,
                    a.CreatedAt,
                    a.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (appointment == null)
            {
                return NotFound(new { message = "Randevu bulunamadı" });
            }

            return Ok(appointment);
        }

        /// <summary>
        /// Üyenin randevularını getirir
        /// </summary>
        [HttpGet("member/{userId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetMemberAppointments(
            string userId,
            [FromQuery] AppointmentStatus? status = null)
        {
            var query = _context.Appointments
                .Where(a => a.UserId == userId)
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
                .Select(a => new
                {
                    a.Id,
                    a.AppointmentDate,
                    StartTime = a.StartTime.ToString(@"hh\:mm"),
                    EndTime = a.EndTime.ToString(@"hh\:mm"),
                    TrainerName = a.Trainer != null ? a.Trainer.FirstName + " " + a.Trainer.LastName : null,
                    ServiceName = a.Service != null ? a.Service.Name : null,
                    a.Price,
                    Status = a.Status.ToString(),
                    a.StatusName
                })
                .ToListAsync();

            return Ok(appointments);
        }

        /// <summary>
        /// Randevu istatistiklerini getirir
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<object>> GetStatistics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var query = _context.Appointments.AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(a => a.AppointmentDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(a => a.AppointmentDate <= endDate.Value);
            }

            var appointments = await query.ToListAsync();

            var statistics = new
            {
                TotalAppointments = appointments.Count,
                PendingCount = appointments.Count(a => a.Status == AppointmentStatus.Pending),
                ApprovedCount = appointments.Count(a => a.Status == AppointmentStatus.Approved),
                CompletedCount = appointments.Count(a => a.Status == AppointmentStatus.Completed),
                CancelledCount = appointments.Count(a => a.Status == AppointmentStatus.Cancelled),
                RejectedCount = appointments.Count(a => a.Status == AppointmentStatus.Rejected),
                TotalRevenue = appointments.Where(a => a.Status == AppointmentStatus.Completed).Sum(a => a.Price),
                AveragePrice = appointments.Any() ? appointments.Average(a => a.Price) : 0,
                ByTrainer = appointments
                    .GroupBy(a => a.TrainerId)
                    .Select(g => new
                    {
                        TrainerId = g.Key,
                        Count = g.Count(),
                        Revenue = g.Where(a => a.Status == AppointmentStatus.Completed).Sum(a => a.Price)
                    })
                    .OrderByDescending(x => x.Count)
                    .Take(5),
                ByService = appointments
                    .GroupBy(a => a.ServiceId)
                    .Select(g => new
                    {
                        ServiceId = g.Key,
                        Count = g.Count(),
                        Revenue = g.Where(a => a.Status == AppointmentStatus.Completed).Sum(a => a.Price)
                    })
                    .OrderByDescending(x => x.Count)
                    .Take(5)
            };

            return Ok(statistics);
        }

        /// <summary>
        /// Bugünün randevularını getirir
        /// </summary>
        [HttpGet("today")]
        public async Task<ActionResult<IEnumerable<object>>> GetTodayAppointments()
        {
            var today = DateTime.Today;

            var appointments = await _context.Appointments
                .Where(a => a.AppointmentDate.Date == today)
                .Include(a => a.User)
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .OrderBy(a => a.StartTime)
                .Select(a => new
                {
                    a.Id,
                    StartTime = a.StartTime.ToString(@"hh\:mm"),
                    EndTime = a.EndTime.ToString(@"hh\:mm"),
                    TrainerName = a.Trainer != null ? a.Trainer.FirstName + " " + a.Trainer.LastName : null,
                    ServiceName = a.Service != null ? a.Service.Name : null,
                    MemberName = a.User != null ? a.User.FirstName + " " + a.User.LastName : null,
                    Status = a.Status.ToString(),
                    a.StatusName,
                    a.StatusColor
                })
                .ToListAsync();

            return Ok(new
            {
                date = today.ToString("yyyy-MM-dd"),
                count = appointments.Count,
                appointments
            });
        }
    }
}




