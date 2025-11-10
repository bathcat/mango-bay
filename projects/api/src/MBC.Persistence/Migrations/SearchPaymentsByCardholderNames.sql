IF TYPE_ID(N'StringListType') IS NULL
BEGIN
    CREATE TYPE StringListType AS TABLE
    (
        Value NVARCHAR(MAX)
    );
END
GO

IF OBJECT_ID('SearchPaymentsByCardholderNames', 'P') IS NOT NULL
    DROP PROCEDURE SearchPaymentsByCardholderNames;
GO

CREATE PROCEDURE SearchPaymentsByCardholderNames
    @CustomerId UNIQUEIDENTIFIER,
    @Names StringListType READONLY
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.Id,
        p.DeliveryId,
        p.Amount,
        p.Status,
        p.MerchantReference,
        p.TransactionId,
        p.CreditCard_CardNumber,
        p.CreditCard_Expiration,
        p.CreditCard_Cvc,
        p.CreditCard_CardholderName,
        p.CreatedAt,
        p.UpdatedAt
    FROM Payments p
    INNER JOIN Deliveries d ON p.DeliveryId = d.Id
    WHERE d.CustomerId = @CustomerId
        AND p.CreditCard_CardholderName IN (SELECT Value FROM @Names)
    ORDER BY p.CreatedAt DESC;
END
GO

