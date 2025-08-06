using consultancysolution.Models;

namespace consultancysolution.Models;

public class AccountDto
{
    public int StudentId { get; set; }
    public AdmissionFeeDto Admission { get; set; }
    public List<CoursePaymentDto> CoursePayments { get; set; } = new List<CoursePaymentDto>();
}
