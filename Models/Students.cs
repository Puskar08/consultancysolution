namespace consultancysolution.Models
{
    public class Students
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string? ProfilePhotoUrl { get; set; }
        public string? PassportUrl { get; set; }
        //Admission Details
        public DateTime AdmissionDate { get; set; } = DateTime.Now;
        public decimal AdmissionCost { get; set; }
        public decimal Discount { get; set; }
        public decimal ModifiedAdmissionCost => AdmissionCost - Discount;
        public decimal PaidAdmissionAmount { get; set; }
        public decimal DueAdmissionAmount => ModifiedAdmissionCost - PaidAdmissionAmount;
        //nvaigation property for the relationship
        public ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();

        //calculate the total paid amount for all courses
        public decimal GetTotalPaidAmount()
        {
            return StudentCourses.Sum(sc => sc.PaidAmount);
        }
        //calculate the total due amount for all courses
        public decimal GetTotalDueAmount()
        {
            return StudentCourses.Sum(sc => sc.DueAmount);
        }
    }
}