using Microsoft.EntityFrameworkCore;
using consultancysolution.Models;

namespace consultancysolution.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // DbSet for Courses
        public DbSet<Course> Courses { get; set; }
        public DbSet<Students> Students { get; set; }
        public DbSet<StudentCourse> StudentCourses { get; set; }
    }
}