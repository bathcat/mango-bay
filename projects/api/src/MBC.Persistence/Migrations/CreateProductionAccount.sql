-- Create login if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'MBC_Production')
    CREATE LOGIN [MBC_Production] WITH PASSWORD = 'StrongPassword123!';
GO

-- Create database user
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'MBC_Production')
    CREATE USER [MBC_Production] FOR LOGIN [MBC_Production];
GO

-- Grant table-specific permissions
GRANT SELECT, INSERT, UPDATE ON [Customers] TO [MBC_Production];
GRANT SELECT ON [Pilots] TO [MBC_Production];
GRANT SELECT, INSERT, UPDATE, DELETE ON [Sites] TO [MBC_Production];
GRANT SELECT, INSERT, UPDATE ON [Deliveries] TO [MBC_Production];
GRANT SELECT, INSERT, UPDATE ON [Payments] TO [MBC_Production];
GRANT SELECT, INSERT ON [DeliveryReviews] TO [MBC_Production];
GRANT SELECT, INSERT ON [DeliveryProofs] TO [MBC_Production];
GRANT SELECT, INSERT, UPDATE ON [AspNetUsers] TO [MBC_Production];
GRANT SELECT ON [AspNetRoles] TO [MBC_Production];
GRANT SELECT, INSERT, DELETE ON [AspNetUserRoles] TO [MBC_Production];
GRANT SELECT, INSERT, DELETE ON [AspNetUserClaims] TO [MBC_Production];
GRANT SELECT, INSERT, DELETE ON [AspNetUserLogins] TO [MBC_Production];
GRANT SELECT, INSERT, DELETE ON [AspNetUserTokens] TO [MBC_Production];
GO
