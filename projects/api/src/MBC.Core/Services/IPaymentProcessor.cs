using System.Threading.Tasks;
using MBC.Core.Models.Payment;

namespace MBC.Core.Services;

public interface IPaymentProcessor
{
    Task<PaymentResult> ProcessPayment(PaymentRequest request);
    Task<RefundResult> RefundPayment(string transactionId, decimal amount);
}

