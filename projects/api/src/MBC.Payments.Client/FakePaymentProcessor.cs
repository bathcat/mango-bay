using MBC.Core.Models.Payment;
using MBC.Core.Services;

namespace MBC.Payments.Client;

public class FakePaymentProcessor : IPaymentProcessor
{
    public Task<PaymentResult> ProcessPayment(PaymentRequest request)
    {
        var cardNumber = request.CreditCard.CardNumber;
        var expiration = request.CreditCard.Expiration;
        var cvc = request.CreditCard.Cvc;
        var amount = request.Amount;

        if (string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length != 16 || !cardNumber.All(char.IsDigit))
        {
            return Task.FromResult(new PaymentResult
            {
                Success = false,
                MerchantReference = request.MerchantReference,
                TransactionId = string.Empty,
                ErrorMessage = $"Invalid card number format. Must be 16 digits. Received: {cardNumber}"
            });
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (expiration < today)
        {
            return Task.FromResult(new PaymentResult
            {
                Success = false,
                MerchantReference = request.MerchantReference,
                TransactionId = string.Empty,
                ErrorMessage = "Card has expired."
            });
        }

        if (string.IsNullOrWhiteSpace(cvc) || cvc.Length < 3 || cvc.Length > 4 || !cvc.All(char.IsDigit))
        {
            return Task.FromResult(new PaymentResult
            {
                Success = false,
                MerchantReference = request.MerchantReference,
                TransactionId = string.Empty,
                ErrorMessage = "Invalid CVC format. Must be 3 or 4 digits."
            });
        }

        if (cvc == "999")
        {
            return Task.FromResult(new PaymentResult
            {
                Success = false,
                MerchantReference = request.MerchantReference,
                TransactionId = string.Empty,
                ErrorMessage = "Security code mismatch."
            });
        }

        if (amount > 10000)
        {
            return Task.FromResult(new PaymentResult
            {
                Success = false,
                MerchantReference = request.MerchantReference,
                TransactionId = string.Empty,
                ErrorMessage = "Amount exceeds transaction limit of $10,000."
            });
        }

        if (cardNumber.StartsWith("12"))
        {
            var transactionId = GenerateTransactionId();
            return Task.FromResult(new PaymentResult
            {
                Success = true,
                MerchantReference = request.MerchantReference,
                TransactionId = transactionId,
                ErrorMessage = null
            });
        }

        if (cardNumber.StartsWith("13"))
        {
            return Task.FromResult(new PaymentResult
            {
                Success = false,
                MerchantReference = request.MerchantReference,
                TransactionId = string.Empty,
                ErrorMessage = "Insufficient funds."
            });
        }

        if (cardNumber.StartsWith("14"))
        {
            return Task.FromResult(new PaymentResult
            {
                Success = false,
                MerchantReference = request.MerchantReference,
                TransactionId = string.Empty,
                ErrorMessage = "Invalid card."
            });
        }

        return Task.FromResult(new PaymentResult
        {
            Success = false,
            MerchantReference = request.MerchantReference,
            TransactionId = string.Empty,
            ErrorMessage = "Payment declined."
        });
    }

    public Task<RefundResult> RefundPayment(string transactionId, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
        {
            return Task.FromResult(new RefundResult
            {
                Success = false,
                ErrorMessage = "Transaction ID is required."
            });
        }

        if (transactionId.StartsWith("TXN-"))
        {
            return Task.FromResult(new RefundResult
            {
                Success = true,
                ErrorMessage = null
            });
        }

        return Task.FromResult(new RefundResult
        {
            Success = false,
            ErrorMessage = "Transaction not found."
        });
    }

    private static string GenerateTransactionId()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = Guid.NewGuid().ToString("N")[..5].ToUpper();
        return $"TXN-{timestamp}-{random}";
    }
}

