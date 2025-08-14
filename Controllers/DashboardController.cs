using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using consultancysolution.Models;
using consultancysolution.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace consultancysolution.Controllers;

public class DashboardController : Controller
{
    private readonly ILogger<DashboardController> _logger;
    private readonly ApplicationDbContext _context;

    public DashboardController(ILogger<DashboardController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }
    [HttpGet]
    public async Task<IActionResult> GetGenderCounts()
    {
        try
        {
            var genderCounts = await _context.Students
                .GroupBy(s => s.Gender ?? "Other")
                .Select(g => new { label = g.Key, value = g.Count() })
                .ToListAsync();
            return Json(genderCounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching gender counts");
            return StatusCode(500, new { error = "Failed to fetch gender counts" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetCourseCounts()
    {
        try
        {
            var studentsPerCourse = await _context.StudentCourses
                .GroupBy(sc => sc.Course.Name)
                .Select(g => new { label = g.Key ?? "Unknown", value = g.Count() })
                .ToListAsync();
            return Json(studentsPerCourse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching course counts");
            return StatusCode(500, new { error = "Failed to fetch course counts" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetMonthlyEnrollment()
    {
        try
        {
            var monthlyEnrollment = await _context.Students
                .GroupBy(s => new { s.AdmissionDate.Year, s.AdmissionDate.Month })
                .Select(g => new
                {
                    month = g.Key.Month,
                    year = g.Key.Year,
                    count = g.Count()
                })
                .OrderBy(g => g.year)
                .ThenBy(g => g.month)
                .ToListAsync();

            // Convert month number to month name after fetching
            var monthlyEnrollmentFormatted = monthlyEnrollment
                .Select(x => new
                {
                    label = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(x.month)} {x.year}",
                    value = x.count
                })
                .ToList();

            return Json(monthlyEnrollment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching monthly enrollment");
            return StatusCode(500, new { error = "Failed to fetch monthly enrollment" });
        }
    }
}
