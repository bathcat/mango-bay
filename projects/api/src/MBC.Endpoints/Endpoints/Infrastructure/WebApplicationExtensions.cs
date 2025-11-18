using Microsoft.AspNetCore.Builder;

namespace MBC.Endpoints.Endpoints.Infrastructure;

public static class WebApplicationExtensions
{
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        app.MapAuthEndpoints();
        app.MapAuthWebEndpoints();
        app.MapDeliveryEndpoints();
        app.MapCustomerEndpoints();
        app.MapPaymentEndpoints();
        app.MapPilotEndpoints();
        app.MapDeliveryProofEndpoints();
        app.MapReviewEndpoints();
        app.MapSiteEndpoints();

        return app;
    }
}

