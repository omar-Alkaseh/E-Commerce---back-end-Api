using Domain.Enums;

namespace Application.Features.Payments.DTOs
{
    public class CreatePaymentDto
    {
        public PaymentMethod.EnPaymentMethod PaymentMethod { get; set; }
    }
}
