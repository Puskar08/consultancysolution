
namespace CourseManagementSystem.Models
{
    public class CoursePaymentDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public decimal Price { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DueAmount { get; set; }
    }
}