using Domain.Enums;

namespace Contracts.Requests
{
    public class CreatePaymentRequest
    {
        public PaymentMethod.EnPaymentMethod PaymentMethod { get; set; }
    }
}
