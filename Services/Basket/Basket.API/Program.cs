using BuildingBlocks.Messaging.MassTransit;
using Discount.Grpc;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var assembly = typeof(Program).Assembly;

var builder = WebApplication.CreateBuilder(args);
{
    // Add services to the container

    // Carter
    builder.Services.AddCarter(new DependencyContextAssemblyCatalog(assemblies: assembly));
    
    // MediatR
    builder.Services.AddMediatR(config =>
    {
        config.RegisterServicesFromAssembly(assembly);
        config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        config.AddOpenBehavior(typeof(LoggingBehavior<,>));
    });
    
    // Marten
    builder.Services.AddMarten(opts =>
    {
        opts.Connection(builder.Configuration.GetConnectionString("Database")!);
        opts.Schema.For<ShoppingCart>().Identity(x => x.UserName);
    }).UseLightweightSessions();
    
    // Basket Repository & Cached Basket Repository with Scrutor
    builder.Services.AddScoped<IBasketRepository, BasketRepository>();
    builder.Services.Decorate<IBasketRepository, CachedBasketRepository>();
    
    // Custom Exception Handler
    builder.Services.AddExceptionHandler<CustomExceptionHandler>();
    
    // Redis 
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration.GetConnectionString("Redis");
        // options.InstanceName = "Basket";
    });
    
    // Healthchecks
    builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("Database")!)
        .AddRedis(builder.Configuration.GetConnectionString("Redis")!);
    
    // Grpc services
    builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(options =>
    {
        options.Address = new Uri(builder.Configuration["GrpcSettings:DiscountUrl"]!);
    }).ConfigurePrimaryHttpMessageHandler(() =>
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        return handler;
    });
    
    // RabbitMQ MassTransit (Async Communication Services)
    builder.Services.AddMessageBroker(builder.Configuration);


}
var app = builder.Build();
{
    // Configure the Http request pipeline
    
    app.MapCarter();

    app.UseExceptionHandler(options => { });

    app.UseHealthChecks("/health", 
        new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
    
    app.Run();
}

