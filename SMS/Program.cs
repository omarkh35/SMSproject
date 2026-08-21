using BLL;
using BLL.Interfaces;
using BLL.Services;
using DAL.Context;
using DAL.Interfaces;
using DAL.Repositries;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BLL.Notifications.Interfaces;
using Microsoft.Extensions.Logging;
using SMS.Hubs;
using SMS.Services;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNgrok",
        policy =>
        {

            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();

            //policy.AllowAnyOrigin()   
            //      .AllowAnyMethod()   
            //      .AllowAnyHeader();  
        });
});

builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddSignalR();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{

    // 1. Define the Security Scheme (How the token is packed)
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token in this exact format: Bearer {your_token_here}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .LogTo(Console.WriteLine, LogLevel.Information)
           .EnableSensitiveDataLogging());


// Example using Scoped lifetime
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped(typeof(IBaseRepositories<>), typeof(BaseRepositry<>));
builder.Services.AddScoped<BLL.Interfaces.IAuthService, BLL.Services.AuthService>();
builder.Services.AddScoped<BLL.Interfaces.IJwtService, BLL.Services.JwtService>();
builder.Services.AddScoped<IParentService, ParentService>();
builder.Services.AddScoped<ISchoolAdminService, SchoolAdminService>();
builder.Services.AddScoped<IDepartmentManagerService, DepartmentManagerService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<ISupervisorService, SupervisorService>();
builder.Services.AddScoped<IAccountantService, AccountantService>();
builder.Services.AddScoped<IAdminAuthService, AdminAuthService>();
builder.Services.AddScoped<ISchoolSettingService, SchoolSettingService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<IResetPassService, ResetPassService>();

builder.Services.AddBusinessLayer();

// «” Œœ«„ SignalRParentNotificationDispatcher ·≈—”«· «·≈‘⁄«—«  «··ÕŸÌ… In-App ⁄»— WebSockets
builder.Services.AddSingleton<IParentPushNotificationDispatcher, SignalRParentNotificationDispatcher>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // TokenValidationParameters define how incoming JWTs will be validated.
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Ensures the token was issued by a trusted issuer.
            ValidateIssuer = true,


            // Ensures the token is intended for this API (audience check).
            ValidateAudience = true,


            // Ensures the token has not expired.
            ValidateLifetime = true,


            // Ensures the token signature is valid and was signed by the API.
            ValidateIssuerSigningKey = true,


            // The expected issuer value (must match the issuer used when creating the JWT).
            ValidIssuer = "SchoolApi",


            // The expected audience value (must match the audience used when creating the JWT).
            ValidAudience = "SchoolApiUsers",


            // The secret key used to validate the JWT signature.
            // This must be the same key used when generating the token.
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("THIS_IS_A_VERY_SECRET_KEY_123456"))
        };

        //  „ﬂÌ‰ ﬁ—«¡… «·‹ Token „‰ «·‹ Query String ·« ’«·«  SignalR (WebSockets)
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/notifications"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };


    });


// ===============================
// Authorization Configuration
// ===============================


// Register authorization services.
// This enables attributes like [Authorize] and role-based authorization.
builder.Services.AddAuthorization();

// Add services to the container.


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowNgrok");

app.UseStaticFiles();

app.UseAuthentication();

app.UseAuthorization();

// —»ÿ ﬁ‰«… «·≈‘⁄«—«  «··ÕŸÌ… · ÿ»Ìﬁ «·√Â· (SignalR Hub)
app.MapHub<NotificationHub>("/hubs/notifications");

app.MapControllers();

app.Run();
