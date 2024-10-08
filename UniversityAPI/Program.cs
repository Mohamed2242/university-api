using System.Data;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using UniversityAPI.Application.Repositories;
using UniversityAPI.Application.Services;
using UniversityAPI.Core.Interface;
using UniversityAPI.Core.Models;
using UniversityAPI.Data;
using UniversityAPI.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//builder.Services.AddLogging<Serilog.ILogger>(ConfigurationBinder=>)
builder.Services.AddControllers().AddJsonOptions(options =>
{
	options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
	options.JsonSerializerOptions.MaxDepth = 64; // Adjust max depth if necessary
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddCors(option =>
{
	option.AddPolicy("MyPolicy", builder =>
	{
		builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
	});
});
builder.Services.AddDbContext<AppDbContext>(option =>
			option.UseLazyLoadingProxies()
			.UseSqlServer(builder.Configuration.GetConnectionString("ConStr")));

//builder.Services.AddIdentityApiEndpoints<ApplicationUser>().AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
		.AddEntityFrameworkStores<AppDbContext>()
		.AddDefaultTokenProviders();

builder.Services.AddScoped<ITokenRepository, TokenRepository>();
builder.Services.AddScoped<IEmailRepository, EmailRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IAssistantRepository, AssistantRepository>();
builder.Services.AddScoped<IAssistantService, AssistantService>();


builder.Services.AddAuthentication(x =>
{
	x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
	x.RequireHttpsMetadata = false;
	x.SaveToken = true;
	x.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuerSigningKey = true,
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("veryveryverynewsecuresecretkeyformywebapp._._.")),
		ValidateAudience = false,
		ValidateIssuer = false
	};
});

builder.Services.AddAutoMapper(typeof(Program));

// Configure Serilog
Log.Logger = new LoggerConfiguration()
	.WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
	.CreateLogger();

// Use Serilog as the logging provider
builder.Host.UseSerilog();

var app = builder.Build();

// Add role creation during startup
using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	try
	{
		// Ensure roles are created
		await CreateRoles(services);
	}
	catch (Exception ex)
	{
		var logger = services.GetRequiredService<ILogger<Program>>();
		logger.LogError(ex, "An error occurred while creating roles.");
	}
}

/*app.UseCors(policy => policy.AllowAnyHeader()
.AllowAnyMethod()
.SetIsOriginAllowed(origin => true)
.AllowCredentials());
*/

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseCors("MyPolicy");

app.UseAuthentication();

app.UseAuthorization();
app.MapControllers();

app.Run();



// Role creation method
async Task CreateRoles(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

string[] roleNames = { "Admin", "Employee", "Student", "Doctor", "Assistant" };
IdentityResult roleResult;

foreach (var roleName in roleNames)
{
	var roleExist = await roleManager.RoleExistsAsync(roleName);
	if (!roleExist)
	{
		// Create the roles if they don't exist
		roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
	}
}
}