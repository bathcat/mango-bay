using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MBC.Core.Entities;
using MBC.Core.Models;
using MBC.Core.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MBC.Persistence.Stores;

public class PaymentStore : IPaymentStore
{
    private readonly MBCDbContext _context;
    private readonly ILogger<PaymentStore> _logger;

    public PaymentStore(MBCDbContext context, ILogger<PaymentStore> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Payment?> GetById(Guid id)
    {
        return await _context.Payments
            .Include(p => p.Delivery)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Page<Payment>> GetPage(int skip, int take)
    {
        var query = _context.Payments
            .Include(p => p.Delivery)
            .OrderByDescending(p => p.CreatedAt);

        return await query.ToPageAsync(skip, take);
    }

    public async Task<Payment> Create(Payment payment)
    {
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task<Payment> Update(Payment payment)
    {
        var existing = await _context.Payments.FindAsync(payment.Id);
        if (existing == null)
        {
            throw new InvalidOperationException($"Payment with ID {payment.Id} not found.");
        }

        _context.Entry(existing).CurrentValues.SetValues(payment);
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<Payment?> GetByDeliveryId(Guid deliveryId)
    {
        return await _context.Payments
            .Include(p => p.Delivery)
            .FirstOrDefaultAsync(p => p.DeliveryId == deliveryId);
    }

    public async Task<IEnumerable<Payment>> SearchByCardholderNames(Guid customerId, string[] names)
    {
        if (_context.Database.IsSqlServer())
        {
            return await SearchByCardholderNamesSproc(customerId, names);
        }

        return await SearchByCardholderNamesEf(customerId, names);
    }

    private async Task<IEnumerable<Payment>> SearchByCardholderNamesEf(Guid customerId, string[] names)
    {
        var query = _context.Payments
            .Include(p => p.Delivery)
            .Where(p => p.Delivery != null && p.Delivery.CustomerId == customerId);

        if (names.Length > 0)
        {
            query = query.Where(p => names.Contains(p.CreditCard.CardholderName));
        }

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    private async Task<IEnumerable<Payment>> SearchByCardholderNamesSproc(Guid customerId, string[] names)
    {
        _logger.LogInformation("Searching payments for customer {CustomerId} with {Count} cardholder names using sproc", customerId, names.Length);
        var connection = _context.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        using var command = connection.CreateCommand();
        command.CommandText = "SearchPaymentsByCardholderNames";
        command.CommandType = CommandType.StoredProcedure;

        var customerIdParam = new SqlParameter("@CustomerId", SqlDbType.UniqueIdentifier)
        {
            Value = customerId
        };
        command.Parameters.Add(customerIdParam);

        var namesTable = new DataTable();
        namesTable.Columns.Add("Value", typeof(string));
        foreach (var name in names)
        {
            namesTable.Rows.Add(name);
        }

        var namesParam = new SqlParameter("@Names", SqlDbType.Structured)
        {
            TypeName = "StringListType",
            Value = namesTable
        };
        command.Parameters.Add(namesParam);

        using var reader = await command.ExecuteReaderAsync();
        var payments = new List<Payment>();

        while (await reader.ReadAsync())
        {
            var payment = new Payment
            {
                Id = reader.GetGuid(0),
                DeliveryId = reader.GetGuid(1),
                Amount = reader.GetDecimal(2),
                Status = (PaymentStatus)reader.GetInt32(3),
                MerchantReference = reader.GetString(4),
                TransactionId = reader.GetString(5),
                CreditCard = new CreditCardInfo
                {
                    CardNumber = reader.GetString(6),
                    Expiration = DateOnly.FromDateTime(reader.GetDateTime(7)),
                    Cvc = reader.GetString(8),
                    CardholderName = reader.GetString(9)
                },
                CreatedAt = reader.GetDateTime(10),
                UpdatedAt = reader.GetDateTime(11)
            };
            payments.Add(payment);
        }

        return payments;
    }
}

