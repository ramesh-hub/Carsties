using AuctionService.Data;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using AuctionService.Consumers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<AuctionService.Data.AuctionDBContext>(options => 
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddMassTransit(x =>
{
    //add outbox incase the service bus is down so we can save message in our db.
    x.AddEntityFrameworkOutbox<AuctionDBContext>(o =>
    {
        o.QueryDelay = TimeSpan.FromSeconds(10);
        o.UsePostgres();
        o.UseBusOutbox();
    });

    x.AddConsumersFromNamespaceContaining<AuctionFinishedConsumer>();
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.UseRetry(r => 
        {
            r.Handle<RabbitMqConnectionException>();
            r.Interval(5, TimeSpan.FromSeconds(10));
        });
		
		//cfg.Host("localhost", "/", h =>
        //{
        //    h.Username("guest");
        //    h.Password("guest");
        //});

        //when containerized an auciton service, the container runs in its own machine and does not know what local host is, so be specific
       cfg.Host(builder.Configuration["RabbitMq:Host"], "/", host => 
	   {
            host.Username(builder.Configuration.GetValue("RabbitMq:Username", "guest"));
            host.Password(builder.Configuration.GetValue("RabbitMq:Password", "guest"));
        });

        cfg.ConfigureEndpoints(context);
    });
});

    

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        //Authority is Identity Server URL. It tells our AuctionService who the Token issuer is aka Identity server.
        //All the activities are invisible to us. The Bearer token from the requests Authorization header is verified 
        //automatically for us by the framework against the Token authority/Issuer.
        options.Authority = builder.Configuration["IdentityServiceUrl"];
        options.RequireHttpsMetadata = false; //As our IdentityServer is running local on port 5000
        //now can specify what parts you want to validate on the token
        options.TokenValidationParameters.ValidateAudience = false;
        options.TokenValidationParameters.NameClaimType = "username";//This is available for us to get Username as Name property
        //on User.Identity.Name

    });

var app = builder.Build();

//This is required before UserAuthorization()
app.UseAuthentication();

// Configure the HTTP request pipeline.
app.UseAuthorization();

app.MapControllers();

var retryPolicy = Policy
    .Handle<NpgsqlException>()
    .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(10));

retryPolicy.ExecuteAndCapture(() => DbInitializer.InitDb(app));

app.Run();
