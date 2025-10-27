using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MBC.Core.Entities;
using MBC.Core.Models;
using MBC.Core.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MBC.Persistence.Stores;

public class DeliveryStore : IDeliveryStore
{
    private readonly MBCDbContext _context;
    private readonly ILogger<DeliveryStore> _logger;

    public DeliveryStore(MBCDbContext context, ILogger<DeliveryStore> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Delivery?> GetById(Guid id)
    {
        return await _context.Deliveries
            .Include(d => d.Customer)
            .Include(d => d.Pilot)
            .Include(d => d.Origin)
            .Include(d => d.Destination)
            .Include(d => d.Payment)
            .Include(d => d.Review)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<Page<Delivery>> GetPage(int skip, int take)
    {
        var query = _context.Deliveries
            .Include(d => d.Customer)
            .Include(d => d.Pilot)
            .Include(d => d.Origin)
            .Include(d => d.Destination)
            .Include(d => d.Payment)
            .Include(d => d.Review)
            .OrderByDescending(d => d.CreatedAt);

        return await query.ToPageAsync(skip, take);
    }

    public async Task<Delivery> Create(Delivery delivery)
    {
        _context.Deliveries.Add(delivery);
        await _context.SaveChangesAsync();
        return delivery;
    }

    public async Task<Delivery> Update(Delivery delivery)
    {
        var existing = await _context.Deliveries.FindAsync(delivery.Id);
        if (existing == null)
        {
            throw new InvalidOperationException($"Delivery with ID {delivery.Id} not found.");
        }

        _context.Entry(existing).CurrentValues.SetValues(delivery);
        _context.Entry(existing.Details).CurrentValues.SetValues(delivery.Details);
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> Delete(Guid id)
    {
        var delivery = await _context.Deliveries.FindAsync(id);
        if (delivery == null)
        {
            return false;
        }

        _context.Deliveries.Remove(delivery);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Page<Delivery>> GetByCustomerId(Guid customerId, int skip, int take)
    {
        var query = _context.Deliveries
            .Where(d => d.CustomerId == customerId)
            .Include(d => d.Customer)
            .Include(d => d.Pilot)
            .Include(d => d.Origin)
            .Include(d => d.Destination)
            .Include(d => d.Payment)
            .Include(d => d.Review)
            .OrderByDescending(d => d.CreatedAt);

        return await query.ToPageAsync(skip, take);
    }

    public async Task<Page<Delivery>> GetByPilotId(Guid pilotId, int skip, int take)
    {
        var query = _context.Deliveries
            .Where(d => d.PilotId == pilotId)
            .Include(d => d.Customer)
            .Include(d => d.Pilot)
            .Include(d => d.Origin)
            .Include(d => d.Destination)
            .Include(d => d.Payment)
            .Include(d => d.Review)
            .OrderByDescending(d => d.CreatedAt);

        return await query.ToPageAsync(skip, take);
    }

    public async Task<Page<Delivery>> SearchByCargoDescription(Guid customerId, string searchTerm, int skip, int take)
    {
        if (_context.Database.IsSqlServer())
        {
            return await SearchByCargoDescriptionSproc(customerId, searchTerm, skip, take);
        }

        return await SearchByCargoDescriptionEf(customerId, searchTerm, skip, take);
    }

    private async Task<Page<Delivery>> SearchByCargoDescriptionEf(Guid customerId, string searchTerm, int skip, int take)
    {
        var query = _context.Deliveries
            .Where(d => d.CustomerId == customerId)
            .Where(d => EF.Functions.Like(d.Details.CargoDescription, $"%{searchTerm ?? string.Empty}%"))
            .Include(d => d.Customer)
            .Include(d => d.Pilot)
            .Include(d => d.Origin)
            .Include(d => d.Destination)
            .Include(d => d.Payment)
            .Include(d => d.Review)
            .OrderByDescending(d => d.CreatedAt);

        return await query.ToPageAsync(skip, take);
    }

    private async Task<Page<Delivery>> SearchByCargoDescriptionSproc(Guid customerId, string searchTerm, int skip, int take)
    {
        _logger.LogInformation("Searching deliveries for customer {CustomerId} with search term: {SearchTerm} using sproc", customerId, searchTerm);
        var connection = _context.Database.GetDbConnection();

        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        using var command = connection.CreateCommand();
        command.CommandText = $"EXEC SearchDeliveriesByCargoDescription '{customerId}', '{searchTerm ?? string.Empty}', {skip}, {take}";

        using var reader = await command.ExecuteReaderAsync();

        await reader.ReadAsync();
        var totalCount = reader.GetInt32(0);

        await reader.NextResultAsync();
        var deliveries = new List<Delivery>();

        while (await reader.ReadAsync())
        {
            var delivery = new Delivery
            {
                Id = reader.GetGuid(0),
                CustomerId = reader.GetGuid(1),
                PilotId = reader.GetGuid(2),
                PaymentId = reader.GetGuid(3),
                Details = new JobDetails
                {
                    OriginId = reader.GetGuid(4),
                    DestinationId = reader.GetGuid(5),
                    CargoDescription = reader.GetString(6),
                    CargoWeightKg = reader.GetDecimal(7),
                    ScheduledFor = DateOnly.FromDateTime(reader.GetDateTime(8))
                },
                CompletedOn = reader.IsDBNull(9) ? null : reader.GetDateTime(9),
                Status = (DeliveryStatus)reader.GetInt32(10),
                CreatedAt = reader.GetDateTime(11),
                UpdatedAt = reader.GetDateTime(12)
            };
            deliveries.Add(delivery);
        }

        return Page.Create(
            items: deliveries,
            offset: skip,
            countRequested: take,
            totalCount: totalCount
        );
    }
}

