var assembly = typeof(Program).Assembly;

var builder = WebApplication.CreateBuilder(args);
{
    // ----------Add Services--------------
    
    // Mediatr
    builder.Services.AddMediatR(config =>
    {
        config.RegisterServicesFromAssembly(assembly);
        config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        config.AddOpenBehavior(typeof(LoggingBehavior<,>));
    });
    
    // Custom validations by FluentValidation
    builder.Services.AddValidatorsFromAssembly(assembly);
    
    // Carter
    builder.Services.AddCarter(new DependencyContextAssemblyCatalog(assemblies: assembly));
    
    // Marten
    builder.Services.AddMarten(opts =>
    {
        opts.Connection(builder.Configuration.GetConnectionString("Database")!);
    }).UseLightweightSessions();

    if (builder.Environment.IsDevelopment())
        builder.Services.InitializeMartenWith<CatalogInitialData>();

    // Custom Exception Handler
    builder.Services.AddExceptionHandler<CustomExceptionHandler>();
    
    // Health Checks
    builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("Database")!);
}

var app = builder.Build();
{
    // ------------- Configure the HTTP request pipeline. ---------------

    app.MapCarter();

    app.UseExceptionHandler(options => { });

    app.UseHealthChecks("/health", new HealthCheckOptions()
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
    
    app.Run();
}

