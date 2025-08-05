using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace consultancysolution.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Duration { get; set; }
        public decimal Price { get; set; }
        public string? Image { get; set; }
        public string? Category { get; set; }
        public ICollection<StudentCourse> studentCourses { get; set; } = new List<StudentCourse>();

    }
}