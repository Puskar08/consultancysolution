using consultancysolution.Data;
using consultancysolution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;

namespace consultancysolution.Controllers;

public class StudentsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICompositeViewEngine _viewEngine;
    public StudentsController(ApplicationDbContext context, ICompositeViewEngine viewEngine)
    {
        _context = context;
        _viewEngine = viewEngine;
    }
    public IActionResult Index()
    {
        var studentsWithCourses = _context.Students
                        .Include(s => s.StudentCourses)
                        .ThenInclude(sc => sc.Course)
                        .ToList();
        return View(studentsWithCourses);
    }
    // GET: Students/Create
    public IActionResult Create()
    {
        return RedirectToAction("Form", new { mode = "Create", studentId = 0 });
    }
    public IActionResult Form(string mode, int studentId = 0)
    {
        // Load courses for dropdown
        var courses = _context.Courses.ToList();
        ViewBag.Courses = courses;

        if (mode == "Edit" && studentId > 0)
        {
            var student = _context.Students.Find(studentId);
            if (student == null)
                return NotFound();

            ViewBag.Student = student; // pass student data to view
        }
        // Pass mode and studentId to view using ViewData or ViewBag
        ViewData["Mode"] = mode;
        ViewData["StudentId"] = studentId;

        return View("CreateEditStudent");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var student = _context.Students.Find(id);
        if (student == null)
        {
            return Json(new
            {
                success = false,
                message = "Student not found."
            });
        }
        try
        {
            // Remove related student courses first
            var studentCourses = _context.StudentCourses.Where(sc => sc.StudentId == id);
            _context.StudentCourses.RemoveRange(studentCourses);

            // Remove the student
            _context.Students.Remove(student);

            // Save all changes at once
            _context.SaveChanges();

            return Json(new
            {
                success = true,
                message = "Student deleted successfully."
            });
        }
        catch (Exception ex)
        {
            return Json(new
            {
                success = false,
                message = "Error checking student courses: " + ex.Message
            });
        }
    }

    private string RenderPartialViewToString(string viewName, object model)
    {
        ViewData.Model = model;
        using (var sw = new StringWriter())
        {
            var viewResult = _viewEngine.FindView(ControllerContext, viewName, false);
            if (viewResult.Success)
            {
                var viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    sw,
                    new HtmlHelperOptions()
                );
                viewResult.View.RenderAsync(viewContext).GetAwaiter().GetResult();
                return sw.GetStringBuilder().ToString();
            }
            else
            {
                throw new ArgumentException($"View {viewName} not found");
            }
        }
    }

    [HttpGet]
    public IActionResult GetTabContent(string tab, string pageMode, int studentId)
    {
        var htmlContent = string.Empty;
        var message = string.Empty;
        switch (tab)
        {
            case "basic-info":
                ViewBag.BasicInfoMode = pageMode == "Create" && studentId == 0 ? "Add" : (pageMode == "Create" && studentId > 0 ? "Edit" : (pageMode == "Edit" ? "Edit" : "View"));
                if (studentId > 0)
                {
                    var student = _context.Students.FirstOrDefault(s => s.Id == studentId);
                    if (student != null)
                    {
                        htmlContent = RenderPartialViewToString("_BasicInfo", student);
                        break;
                    }
                }
                htmlContent = RenderPartialViewToString("_BasicInfo", new Students());
                break;
            case "course-selection":
                ViewBag.Courses = _context.Courses.ToList();
                if (studentId > 0)
                {
                    var studentCourses = (from sc in _context.StudentCourses
                                          join c in _context.Courses on sc.CourseId equals c.Id
                                          where sc.StudentId == studentId
                                          select new SelectedCourseDto
                                          {
                                              Id = sc.CourseId,
                                              CourseName = c.Name,
                                              Price = c.Price,
                                              Discount = c.Price - sc.ModifiedCoursePrice,
                                              ModifiedCoursePrice = sc.ModifiedCoursePrice
                                          }).ToList();
                    ViewBag.CourseSelectionMode = studentCourses.Any() ? "Edit" : "Add";
                    htmlContent = RenderPartialViewToString("_CourseSelection", studentCourses);
                    break;
                }
                htmlContent = RenderPartialViewToString("_CourseSelection", new List<SelectedCourseDto>());
                break;
            case "account":
                if (studentId > 0)
                {
                    var admission = _context.Students
                        .Where(s => s.Id == studentId)
                        .Select(s => new AdmissionFeeDto
                        {
                            AdmissionFee = s.AdmissionCost,
                            Discount = s.Discount,
                            ModifiedAdmissionFee = s.AdmissionCost - s.Discount,
                            Paid = s.PaidAdmissionAmount,
                            Due = s.AdmissionCost - s.Discount - s.PaidAdmissionAmount
                        }).FirstOrDefault();
                    ViewBag.AdmissionFormMode = admission?.Paid > 0 || admission?.Discount > 0 ? "Edit" : "Add";

                    var coursePayments = _context.StudentCourses
                        .Where(sc => sc.StudentId == studentId)
                        .Select(sc => new CoursePaymentDto
                        {
                            CourseId = sc.CourseId,
                            CourseName = _context.Courses.Where(c => c.Id == sc.CourseId).Select(c => c.Name).FirstOrDefault() ?? "Unknown Course",
                            Price = sc.ModifiedCoursePrice,
                            PaidAmount = sc.PaidAmount,
                            DueAmount = sc.DueAmount
                        }).ToList();
                    ViewBag.CoursePaymentFormMode = coursePayments.Any(cp => cp.PaidAmount > 0) ? "Edit" : "Add";

                    var accountDto = new AccountDto
                    {
                        StudentId = studentId,
                        Admission = admission ?? new AdmissionFeeDto(),
                        CoursePayments = coursePayments
                    };
                    htmlContent = RenderPartialViewToString("_Account", accountDto);
                    break;
                }
                htmlContent = RenderPartialViewToString("_Account", new AccountDto());
                break;
            default:
                message = "Invalid tab selected.";
                return Json(new
                {
                    success = false,
                    message = message,
                    html = htmlContent
                });
        }
        return Json(new
        {
            success = true,
            message = "Tab content loaded successfully.",
            html = htmlContent
        });
    }
    [HttpPost]
    public IActionResult CreateStudentOnly(Students student, IFormFile ProfilePhoto, IFormFile Passport)
    {
        if (!ModelState.IsValid || !new[] { "Male", "Female", "Other", null }.Contains(student.Gender))
        {
            return Json(new
            {
                success = false,
                message = "Invalid student data",
                html = string.Empty
            });
        }

        try
        {
            // Handle ProfilePhoto upload
            if (ProfilePhoto != null && ProfilePhoto.Length > 0)
            {
                if (!ProfilePhoto.ContentType.StartsWith("image/"))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Profile photo must be an image file",
                        html = string.Empty
                    });
                }
                var photoFileName = Guid.NewGuid().ToString() + Path.GetExtension(ProfilePhoto.FileName);
                var photoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", photoFileName);
                using (var stream = new FileStream(photoPath, FileMode.Create))
                {
                    ProfilePhoto.CopyTo(stream);
                }
                student.ProfilePhotoUrl = $"/uploads/{photoFileName}";
            }

            // Handle Passport upload
            if (Passport != null && Passport.Length > 0)
            {
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
                var extension = Path.GetExtension(Passport.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Passport must be a PDF, DOC, or DOCX file",
                        html = string.Empty
                    });
                }
                var passportFileName = Guid.NewGuid().ToString() + extension;
                var passportPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", passportFileName);
                using (var stream = new FileStream(passportPath, FileMode.Create))
                {
                    Passport.CopyTo(stream);
                }
                student.PassportUrl = $"/uploads/{passportFileName}";
            }

            // Set default values
            student.AdmissionCost = 1000;
            student.Discount = 0;
            student.PaidAdmissionAmount = 0;

            // Add to database
            _context.Students.Add(student);
            _context.SaveChanges();

            ViewBag.BasicInfoMode = "Edit";
            var htmlContent = RenderPartialViewToString("_BasicInfo", student);
            return Json(new
            {
                success = true,
                message = "Student created successfully.",
                studentId = student.Id,
                html = htmlContent
            });
        }
        catch (Exception ex)
        {
            return Json(new
            {
                success = false,
                message = "Error saving student: " + ex.Message,
                html = string.Empty
            });
        }
    }

    [HttpPost]
    public IActionResult ModifyStudent(Students student, IFormFile ProfilePhoto, IFormFile Passport)
    {
        if (!ModelState.IsValid || !new[] { "Male", "Female", "Other", null }.Contains(student.Gender))
        {
            return Json(new
            {
                success = false,
                message = "Invalid student data",
                studentId = student.Id,
                html = string.Empty
            });
        }

        try
        {
            var existingStudent = _context.Students.FirstOrDefault(s => s.Id == student.Id);
            if (existingStudent == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Student not found.",
                    html = string.Empty
                });
            }

            // Update properties
            existingStudent.Name = student.Name;
            existingStudent.Email = student.Email;
            existingStudent.Address = student.Address;
            existingStudent.Phone = student.Phone;
            existingStudent.DateOfBirth = student.DateOfBirth;
            existingStudent.AdmissionDate = student.AdmissionDate;
            existingStudent.Gender = student.Gender;

            // Handle ProfilePhoto upload
            if (ProfilePhoto != null && ProfilePhoto.Length > 0)
            {
                if (!ProfilePhoto.ContentType.StartsWith("image/"))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Profile photo must be an image file",
                        html = string.Empty
                    });
                }
                var photoFileName = Guid.NewGuid().ToString() + Path.GetExtension(ProfilePhoto.FileName);
                var photoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", photoFileName);
                using (var stream = new FileStream(photoPath, FileMode.Create))
                {
                    ProfilePhoto.CopyTo(stream);
                }
                // Delete old photo if it exists
                if (!string.IsNullOrEmpty(existingStudent.ProfilePhotoUrl))
                {
                    var oldPhotoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingStudent.ProfilePhotoUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPhotoPath))
                    {
                        System.IO.File.Delete(oldPhotoPath);
                    }
                }
                existingStudent.ProfilePhotoUrl = $"/uploads/{photoFileName}";
            }

            // Handle Passport upload
            if (Passport != null && Passport.Length > 0)
            {
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
                var extension = Path.GetExtension(Passport.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Passport must be a PDF, DOC, or DOCX file",
                        html = string.Empty
                    });
                }
                var passportFileName = Guid.NewGuid().ToString() + extension;
                var passportPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", passportFileName);
                using (var stream = new FileStream(passportPath, FileMode.Create))
                {
                    Passport.CopyTo(stream);
                }
                // Delete old passport if it exists
                if (!string.IsNullOrEmpty(existingStudent.PassportUrl))
                {
                    var oldPassportPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingStudent.PassportUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPassportPath))
                    {
                        System.IO.File.Delete(oldPassportPath);
                    }
                }
                existingStudent.PassportUrl = $"/uploads/{passportFileName}";
            }

            _context.SaveChanges();
            ViewBag.BasicInfoMode = "Edit";
            var htmlContent = RenderPartialViewToString("_BasicInfo", existingStudent);
            return Json(new
            {
                success = true,
                message = "Student updated successfully.",
                studentId = existingStudent.Id,
                html = htmlContent
            });
        }
        catch (Exception ex)
        {
            return Json(new
            {
                success = false,
                message = "Error updating student: " + ex.Message,
                html = string.Empty
            });
        }
    }

    [HttpPost]
    [Route("Students/SaveCourses")]
    public IActionResult SaveCourses(int studentid, [FromBody] List<SelectedCourseDto> selectedCourses)
    {
        if (selectedCourses == null || !selectedCourses.Any())
            return Json(new
            {
                success = false,
                message = "No courses selected."
            });
        _context.StudentCourses.RemoveRange(_context.StudentCourses.Where(sc => sc.StudentId == studentid));
        foreach (var course in selectedCourses)
        {
            var modifiedFee = Math.Max(0, course.Price - course.Discount);

            var studentCourse = new StudentCourse
            {
                StudentId = studentid,
                CourseId = course.Id,
                ModifiedCoursePrice = modifiedFee,
                EnrollmentDate = DateTime.UtcNow, // or DateTime.Now depending on your time handling
                Grade = null // Optional, can be updated later
            };

            _context.StudentCourses.Add(studentCourse); // Assuming _context is your DbContext
        }

        _context.SaveChanges();
        var studentCourseList = (from sc in _context.StudentCourses
                                 join c in _context.Courses on sc.CourseId equals c.Id
                                 where sc.StudentId == studentid
                                 select new SelectedCourseDto
                                 {
                                     Id = sc.CourseId,
                                     CourseName = c.Name,
                                     Price = c.Price,
                                     Discount = c.Price - sc.ModifiedCoursePrice,
                                     ModifiedCoursePrice = sc.ModifiedCoursePrice
                                 }).ToList();

        ViewBag.CourseSelectionMode = "Edit";
        ViewBag.Courses = _context.Courses.ToList();
        var htmlContent = RenderPartialViewToString("_CourseSelection", studentCourseList);
        return Json(new
        {
            success = true,
            message = "Courses saved successfully.",
            html = htmlContent,
        });
    }

    [HttpPost]
    public IActionResult SaveAdmissionFee(int studentId, [FromBody] AdmissionFeeDto admissionFeeDto)
    {
        if (admissionFeeDto == null || studentId == 0)
            return Json(new
            {
                success = false,
                message = "Invalid data"
            });

        var student = _context.Students.Find(studentId);
        if (student == null)
            return Json(new
            {
                success = false,
                message = "Student not found"
            });

        student.AdmissionCost = admissionFeeDto.AdmissionFee != 0 ? admissionFeeDto.AdmissionFee : student.AdmissionCost; // Default admission cost if not provided
        student.Discount = admissionFeeDto.Discount;
        student.PaidAdmissionAmount = admissionFeeDto.Paid;

        _context.SaveChanges();
        var admissionInfo = _context.Students
                        .Where(s => s.Id == studentId)
                        .Select(s => new AdmissionFeeDto
                        {
                            AdmissionFee = s.AdmissionCost,
                            Discount = s.Discount,
                            ModifiedAdmissionFee = s.AdmissionCost - s.Discount,
                            Paid = s.PaidAdmissionAmount,
                            Due = s.AdmissionCost - s.Discount - s.PaidAdmissionAmount
                        }).FirstOrDefault();
        ViewBag.AdmissionFormMode = admissionInfo?.Paid > 0 ? "Edit" : "Add";
        var htmlContent = RenderPartialViewToString("_AdmissionInfo", admissionInfo ?? new AdmissionFeeDto());
        return Json(new
        {
            success = true,
            message = "Admission fee saved successfully.",
            html = htmlContent
        });
    }

    [HttpPost]
    public IActionResult SaveCoursePayments(int studentId, [FromBody] List<CoursePaymentDto> coursePayment)
    {
        if (coursePayment == null || studentId == 0)
            return Json(new
            {
                success = false,
                message = "Invalid data"
            });

        var courseIds = coursePayment.Select(cp => cp.CourseId).ToList();
        var existingCourses = _context.StudentCourses
                                .Where(sc => sc.StudentId == studentId && courseIds.Contains(sc.CourseId))
                                .ToList();
        if (existingCourses.Count == 0)
            return Json(new
            {
                success = false,
                message = "No courses found for the student."
            });
        foreach (var course in existingCourses)
        {
            var payment = coursePayment.FirstOrDefault(cp => cp.CourseId == course.CourseId);
            if (payment != null)
            {
                course.PaidAmount = payment.PaidAmount;
                course.DueAmount = payment.DueAmount;
            }
        }
        _context.SaveChanges();

        var coursePayments = _context.StudentCourses
            .Where(sc => sc.StudentId == studentId)
            .Select(sc => new CoursePaymentDto
            {
                CourseId = sc.CourseId,
                CourseName = _context.Courses.Where(c => c.Id == sc.CourseId).Select(c => c.Name).FirstOrDefault() ?? "Unknown Course",
                Price = sc.ModifiedCoursePrice,
                PaidAmount = sc.PaidAmount,
                DueAmount = sc.DueAmount
            }).ToList();
        ViewBag.CoursePaymentFormMode = coursePayments.Any(cp => cp.PaidAmount > 0) ? "Edit" : "Add";
        var htmlContent = RenderPartialViewToString("_CoursePaymentInfo", coursePayments);
        return Json(new
        {
            success = true,
            message = "Course payments saved successfully.",
            html = htmlContent
        });
    }

}