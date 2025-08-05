using consultancysolution.Data;
using consultancysolution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace consultancysolution.Controllers;

public class CourseController : Controller
{
    private readonly ApplicationDbContext _context;
    public CourseController(ApplicationDbContext context)
    {
        _context = context;
    }
    public IActionResult Index()
    {
        var courses = _context.Courses.ToList();
        return View(courses);
    }
    public IActionResult Details(int id)
    {
        var course = _context.Courses.FirstOrDefault(c => c.Id == id);
        if (course == null)
        {
            return NotFound();
        }
        return View(course);
    }
    // GET: Course/Create
    public IActionResult Create()
    {
        return View();
    }
    // POST: Course/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Course course, IFormFile ImageFile)
    {
        if (ModelState.IsValid)
        {
            // Check if an image file is uploaded
            if (ImageFile != null && ImageFile.Length > 0)
            {
                // Define the path to save the image
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/courses");
                var fileName = $"{course.Name.Replace(" ", "_")}.jpg";
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Ensure the directory exists
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Save the file to the server
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    ImageFile.CopyTo(stream);
                }

                // Set the image path in the course object
                course.Image = $"/images/courses/{fileName}";
            }
            _context.Courses.Add(course);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        return View(course);
    }

    //get: Course/Edit
    public IActionResult Edit(int id)
    {
        var course = _context.Courses.FirstOrDefault(c => c.Id == id);
        if (course == null)
        {
            return NotFound();
        }
        return View(course);
    }
    //post: Course/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Course course, IFormFile ImageFile)
    {
        if (id != course.Id)
        {
            return BadRequest();
        }
        ModelState.Remove("ImageFile"); // Remove the ImageFile property from ModelState to avoid validation errors
        // Check if the model state is valid
        if (ModelState.IsValid)
        {
            // Retrieve the existing course from the database
            var existingCourse = _context.Courses.FirstOrDefault(c => c.Id == id);
            if (existingCourse == null)
            {
                return NotFound();
            }

            // Update the course details
            existingCourse.Name = course.Name;
            existingCourse.Description = course.Description;
            existingCourse.Duration = course.Duration;
            existingCourse.Price = course.Price;
            existingCourse.Category = course.Category;

            // Handle image upload if a new file is provided
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/courses");
                var fileName = $"{course.Name.Replace(" ", "_")}.jpg";
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Delete the existing image if it exists
                if (!string.IsNullOrEmpty(existingCourse.Image))
                {
                    var existingFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingCourse.Image.TrimStart('/'));
                    if (System.IO.File.Exists(existingFilePath))
                    {
                        System.IO.File.Delete(existingFilePath);
                    }
                }

                // Ensure the directory exists
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Save the new image
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    ImageFile.CopyTo(stream);
                }

                // Update the image path with a cache-busting query string
                existingCourse.Image = $"/images/courses/{fileName}?v={DateTime.Now.Ticks}";
            }
            _context.Update(existingCourse);
            _context.SaveChanges();
            return RedirectToAction("Details", new { id = existingCourse.Id });
        }
        return View(course);
    }

    //POST: Course/Delete
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var course = _context.Courses.FirstOrDefault(c => c.Id == id);
        if (course == null)
        {
            return NotFound();
        }
        //Delete the associated image file if it exists
        if (!string.IsNullOrEmpty(course.Image))
        {
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", course.Image.TrimStart('/'));
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
        }
        // Delete the course
        _context.Courses.Remove(course);
        _context.SaveChanges();
        return RedirectToAction(nameof(Index));
    }

}

