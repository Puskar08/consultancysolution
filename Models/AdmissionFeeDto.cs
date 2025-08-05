namespace consultancysolution.Models
{
    public class AdmissionFeeDto
    {
        public decimal AdmissionFee { get; set; } = 1000;
        public decimal Discount { get; set; }
        public decimal ModifiedAdmissionFee { get; set; }
        public decimal Paid { get; set; }
        public decimal Due { get; set; }
    }
}