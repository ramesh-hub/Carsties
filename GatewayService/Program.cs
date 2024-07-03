using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        //Authority is Identity Server URL. It tells our AuctionService who the Token issuer is aka Identity server.
        //All the activities are invisible to us. The Bearer token from the requests Authorization header is verified 
        //automatically for us by the framework against the Token authority/Issuer.
        options.Authority = builder.Configuration["IdentityServerUrl"];
        options.RequireHttpsMetadata = false; //As our IdentityServer is running local on port 5000
        //now can specify what parts you want to validate on the token
        options.TokenValidationParameters.ValidateAudience = false;
        options.TokenValidationParameters.NameClaimType = "username";//This is available for us to get Username as Name property
        //on User.Identity.Name

    });

var app = builder.Build();

app.MapReverseProxy();

app.UseAuthentication();
app.UseAuthorization();

//app.MapGet("/", () => "Hello World!");

app.Run();
