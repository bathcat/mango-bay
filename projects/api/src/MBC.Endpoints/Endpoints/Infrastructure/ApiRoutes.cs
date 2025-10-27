namespace MBC.Endpoints.Endpoints.Infrastructure;

public static class ApiRoutes
{
    private const string Prefix = "/api/v1";

    public static string Auth => $"{Prefix}/auth";
    public static string Deliveries => $"{Prefix}/deliveries";
    public static string Customers => $"{Prefix}/customers";
    public static string Payments => $"{Prefix}/payments";
    public static string Pilots => $"{Prefix}/pilots";
    public static string Proofs => $"{Prefix}/proofs";
    public static string Reviews => $"{Prefix}/reviews";
    public static string Sites => $"{Prefix}/sites";
}

