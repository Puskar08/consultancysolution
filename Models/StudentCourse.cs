namespace consultancysolution.Models
{
    public class StudentCourse
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public decimal ModifiedCoursePrice { get; set; }// This property is used to store the modified course price if different for different students
        public DateTime EnrollmentDate { get; set; }
        public string? Grade { get; set; }

        // Navigation properties
        public virtual Students Student { get; set; }
        public virtual Course Course { get; set; }
        //payrment details
        public decimal PaidAmount { get; set; }
        public decimal DueAmount { get; set; }
        // Method to update the due amount
        // public void UpdateDueAmount()
        // {
        //     if (Course != null && decimal.TryParse(Course.Price, out var coursePrice))
        //     {
        //         DueAmount = coursePrice - PaidAmount;
        //     }
        // }
    }
}