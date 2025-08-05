using consultancysolution.Data;
using consultancysolution.Models;
using CourseManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace consultancysolution.Controllers;

public class StudentsController : Controller
{
    private readonly ApplicationDbContext _context;
    public StudentsController(ApplicationDbContext context)
    {
        _context = context;
    }
    public IActionResult Index()
    {
        var students = _context.Students.ToList();
        return View(students);
    }
    // GET: Students/Details/5
    public IActionResult Details(int id)
    {
        var student = _context.Students.FirstOrDefault(s => s.Id == id);
        if (student == null)
        {
            return NotFound();
        }
        return View(student);
    }
    // GET: Students/Create
    public IActionResult Create()
    {
        // Get the list of courses to populate the dropdown
        var courses = _context.Courses.ToList();
        ViewBag.Courses = courses;
        ViewBag.FromButtom = "Create";
        //return View("CreateStudent");
        return View("CreateEditStudent");
    }
    // POST: Students/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Students student, List<StudentCourse> studentCourses)
    {
        if (ModelState.IsValid)
        {
            // Add the student to the database
            _context.Students.Add(student);
            _context.SaveChanges();

            // Add the selected courses to the student's course list
            foreach (var courses in studentCourses)
            {
                courses.StudentId = student.Id;
                courses.CourseId = courses.CourseId;
                courses.EnrollmentDate = DateTime.Now;
                courses.ModifiedCoursePrice = courses.ModifiedCoursePrice;
                courses.PaidAmount = 0;
                courses.DueAmount = 0;
                _context.StudentCourses.Add(courses);
            }
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        // If model state is invalid, repopulate the courses
        ViewBag.Courses = _context.Courses.ToList();
        return View(student);
    }
    [HttpPost]
    [Route("Students/CreateStudent")]
    public IActionResult CreateStudent([FromBody] Students student)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(new { errors });
        }

        _context.Students.Add(student);
        _context.SaveChanges();

        var courses = _context.Courses.Select(c => new
        {
            id = c.Id,
            name = c.Name,
            fee = c.Price
        }).ToList();

        return Ok(new
        {
            id = student.Id,
            courses
        });
    }
    // [HttpPost]
    // [Route("Students/SaveCourses")]
    // public IActionResult SaveCoursesprev([FromBody] List<StudentCourseDto> courses)
    // {
    //     if (courses == null || courses.Count == 0)
    //         return BadRequest("No courses provided.");

    //     var savedCourses = new List<object>();

    //     foreach (var course in courses)
    //     {
    //         var studentCourse = new StudentCourse
    //         {
    //             StudentId = course.StudentId,
    //             CourseId = course.CourseId,
    //             ModifiedCoursePrice = course.ModifiedFee,
    //             PaidAmount = 0,
    //             DueAmount = course.ModifiedFee,
    //             EnrollmentDate = DateTime.Now
    //         };

    //         _context.StudentCourses.Add(studentCourse);

    //         var courseInfo = _context.Courses.FirstOrDefault(c => c.Id == course.CourseId);
    //         if (courseInfo != null)
    //         {
    //             savedCourses.Add(new
    //             {
    //                 name = courseInfo.Name,
    //                 modifiedFee = course.ModifiedFee
    //             });
    //         }
    //     }

    //     _context.SaveChanges();

    //     return Ok(new { message = "Courses saved successfully.", courses = savedCourses });
    // }
    // [HttpPost]
    // public IActionResult SaveAdmissionFee([FromBody] AdmissionFeeDto data)
    // {
    //     var student = _context.Students.FirstOrDefault(s => s.Id == data.StudentId);
    //     if (student == null)
    //         return NotFound("Student not found");

    //     student.AdmissionCost = data.AdmissionFee;
    //     student.PaidAdmissionAmount = data.Paid;
    //     _context.SaveChanges();

    //     return Ok();
    // }
    // [HttpPost]
    // public IActionResult SaveCoursePayments([FromBody] List<CoursePaymentDto> payments)
    // {
    //     foreach (var payment in payments)
    //     {
    //         var studentCourse = _context.StudentCourses
    //             .FirstOrDefault(sc => sc.StudentId == payment.StudentId && sc.CourseId == payment.CourseId);

    //         if (studentCourse != null)
    //         {
    //             studentCourse.PaidAmount = payment.PaidAmount;
    //             studentCourse.DueAmount = payment.DueAmount;
    //         }
    //     }

    //     _context.SaveChanges();
    //     return Ok();
    // }

    // GET: Students/Edit/5
    public IActionResult Edit(int id)
    {
        var student = _context.Students.FirstOrDefault(s => s.Id == id);
        if (student == null)
        {
            return NotFound();
        }
        // Get the list of courses to populate the dropdown
        var courses = _context.Courses.ToList();
        ViewBag.Courses = courses;
        return View(student);
    }
    // POST: Students/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Students student, int[] selectedCourses)
    {
        if (id != student.Id)
        {
            return BadRequest();
        }
        if (ModelState.IsValid)
        {
            // Update the student details
            _context.Update(student);
            _context.SaveChanges();

            // Update the selected courses for the student
            var existingCourses = _context.StudentCourses.Where(sc => sc.StudentId == student.Id).ToList();
            _context.StudentCourses.RemoveRange(existingCourses);
            foreach (var courseId in selectedCourses)
            {
                var studentCourse = new StudentCourse
                {
                    StudentId = student.Id,
                    CourseId = courseId,
                    PaidAmount = 0,
                    DueAmount = 0
                };
                _context.StudentCourses.Add(studentCourse);
            }
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        // If model state is invalid, repopulate the courses
        ViewBag.Courses = _context.Courses.ToList();
        return View(student);
    }
    // GET: Students/Delete/5
    public IActionResult Delete(int id)
    {
        var student = _context.Students.FirstOrDefault(s => s.Id == id);
        if (student == null)
        {
            return NotFound();
        }
        return View(student);
    }
    // POST: Students/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        var student = _context.Students.FirstOrDefault(s => s.Id == id);
        if (student != null)
        {
            _context.Students.Remove(student);
            _context.SaveChanges();
        }
        return RedirectToAction(nameof(Index));
    }
    // GET: Students/EnrollCourse/5
    public IActionResult EnrollCourse(int id)
    {
        var student = _context.Students.FirstOrDefault(s => s.Id == id);
        if (student == null)
        {
            return NotFound();
        }
        // Get the list of courses to populate the dropdown
        var courses = _context.Courses.ToList();
        ViewBag.Courses = courses;
        return View(student);
    }
    // POST: Students/EnrollCourse/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EnrollCourse(int id, int[] selectedCourses)
    {
        var student = _context.Students.FirstOrDefault(s => s.Id == id);
        if (student == null)
        {
            return NotFound();
        }
        if (ModelState.IsValid)
        {
            // Add the selected courses to the student's course list
            foreach (var courseId in selectedCourses)
            {
                var studentCourse = new StudentCourse
                {
                    StudentId = student.Id,
                    CourseId = courseId,
                    PaidAmount = 0,
                    DueAmount = 0
                };
                _context.StudentCourses.Add(studentCourse);
            }
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        // If model state is invalid, repopulate the courses
        ViewBag.Courses = _context.Courses.ToList();
        return View(student);
    }


    //here starts the actual code 
    [HttpGet]
    public IActionResult GetTabContent(string tab, string editType, int studentId)
    {
        switch (tab)
        {
            case "basic-info":
                ViewBag.EditMode = editType;
                if (editType == "Edit" && studentId > 0)
                {
                    var student = _context.Students.FirstOrDefault(s => s.Id == studentId);
                    if (student != null)
                    {
                        return PartialView("_BasicInfo", student);
                    }
                    return NotFound("Student not found");
                }
                return PartialView("_BasicInfo", new Students());
            case "course-selection":
                ViewBag.Courses = _context.Courses.ToList();
                ViewBag.EditMode = editType;
                //ViewBag.studentId = studentId;
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
                    return PartialView("_CourseSelection", studentCourses);
                }
                return PartialView("_CourseSelection", new List<SelectedCourseDto>());
            case "account":
                ViewBag.EditMode = editType;
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

                    var accountDto = new AccountDto
                    {
                        StudentId = studentId,
                        Admission = admission ?? new AdmissionFeeDto(),
                        CoursePayments = coursePayments
                    };
                    return PartialView("_Account", accountDto);
                }
                return PartialView("_Account", new AccountDto());
            default:
                return BadRequest("Invalid Tab");
        }
    }
    [HttpGet]
    public IActionResult GetBaiscInfo(int id)
    {
        var student = _context.Students.FirstOrDefault(s => s.Id == id);
        if (student != null)
        {
            ViewBag.EditMode = "Edit";
            return PartialView("_BasicInfo", student);
        }
        return BadRequest("Invalid Tab");
    }
    [HttpPost]
    public IActionResult CreateStudentOnly(Students student)
    {
        if (ModelState.IsValid)
        {
            // Add the student to the database
            _context.Students.Add(student);
            _context.SaveChanges();
            ViewBag.EditMode = "Edit";
            return PartialView("_BasicInfo", _context.Students.FirstOrDefault(s => s.Id == student.Id));
        }
        return BadRequest("Failed to save student");
    }
    [HttpPost]
    public IActionResult ModifyStudent(Students student)
    {
        if (ModelState.IsValid)
        {
            _context.Update(student);
            _context.SaveChanges();
            ViewBag.EditMode = "Edit";
            return PartialView("_BasicInfo", _context.Students.FirstOrDefault(s => s.Id == student.Id));
        }
        return BadRequest("Failed to save student");
    }

    [HttpPost]
    [Route("Students/SaveCourses")]
    public IActionResult SaveCourses(int studentid, [FromBody] List<SelectedCourseDto> selectedCourses)
    {
        if (selectedCourses == null || !selectedCourses.Any())
            return BadRequest("No courses submitted.");
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

        ViewBag.EditMode = "Edit";
        ViewBag.Courses = _context.Courses.ToList();
        return PartialView("_CourseSelection", studentCourseList);
        //return Ok("Courses saved successfully.");
    }
    [HttpPost]
    public IActionResult SaveAdmissionFee(int studentId, [FromBody] AccountDto admission)
    {
        if (admission == null || studentId == 0)
            return BadRequest("Invalid data");

        var student = _context.Students.Find(studentId);
        if (student == null)
            return NotFound("Student not found");

        student.AdmissionCost = admission.Admission.AdmissionFee;
        student.Discount = admission.Admission.Discount;
        // student.ModifiedAdmissionCost = admission.ModifiedAdmissionFee;
        student.PaidAdmissionAmount = admission.Admission.Paid;
        // student.DueAdmissionAmount = admission.Due;

        _context.SaveChanges();
        return PartialView("_Account", admission);
    }

    [HttpPost]
    public IActionResult SaveCoursePayments(int studentId, [FromBody] List<CoursePaymentDto> coursePayment)
    {
        if (coursePayment == null || studentId == 0)
            return BadRequest("Invalid data");

        var courseIds = coursePayment.Select(cp => cp.CourseId).ToList();
        var existingCourses = _context.StudentCourses
                                .Where(sc => sc.StudentId == studentId && courseIds.Contains(sc.CourseId))
                                .ToList();
        if (existingCourses.Count == 0)
            return NotFound("No courses found for the student");
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
        var accountDto = new AccountDto
        {
            StudentId = studentId,
            Admission = admission ?? new AdmissionFeeDto(),
            CoursePayments = coursePayments
        };
        return PartialView("_Account", accountDto);
    }

}