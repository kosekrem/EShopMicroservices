using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
{
    // Reverse Proxy
    builder.Services.AddReverseProxy()
        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
    
    // Rate Limiter
    builder.Services.AddRateLimiter(rateLimiterOptions =>
    {
        rateLimiterOptions.AddFixedWindowLimiter("fixed", options =>
        {
            options.Window = TimeSpan.FromSeconds(10);
            options.PermitLimit = 5;
        });
    });
}

var app = builder.Build();
{
    app.UseRateLimiter();

    app.MapReverseProxy();
    
    app.Run();
}
