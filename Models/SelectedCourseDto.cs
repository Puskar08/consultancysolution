namespace consultancysolution.Models
{

    public class SelectedCourseDto
    {
        public int Id { get; set; }
        public string CourseName { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public decimal ModifiedCoursePrice { get; set; }
    }
}