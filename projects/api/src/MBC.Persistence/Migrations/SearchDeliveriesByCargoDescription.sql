CREATE PROCEDURE [dbo].[SearchDeliveriesByCargoDescription]
    @CustomerId UNIQUEIDENTIFIER,
    @SearchTerm NVARCHAR(MAX),
    @Skip INT,
    @Take INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(*) AS TotalCount
    FROM Deliveries d
    WHERE d.CustomerId = @CustomerId
        AND d.Details_CargoDescription LIKE '%' + @SearchTerm + '%';

    SELECT
        d.Id,
        d.CustomerId,
        d.PilotId,
        d.PaymentId,
        d.Details_OriginId AS OriginId,
        d.Details_DestinationId AS DestinationId,
        d.Details_CargoDescription AS CargoDescription,
        d.Details_CargoWeightKg AS CargoWeightKg,
        d.Details_ScheduledFor AS ScheduledFor,
        d.CompletedOn,
        d.Status,
        d.CreatedAt,
        d.UpdatedAt
    FROM Deliveries d
    WHERE d.CustomerId = @CustomerId
        AND d.Details_CargoDescription LIKE '%' + @SearchTerm + '%'
    ORDER BY d.CreatedAt DESC
    OFFSET @Skip ROWS
    FETCH NEXT @Take ROWS ONLY;
END
GO

