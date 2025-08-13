using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using consultancysolution.Models;

namespace consultancysolution.Controllers;

public class DashboardController : Controller
{
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(ILogger<DashboardController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

}
