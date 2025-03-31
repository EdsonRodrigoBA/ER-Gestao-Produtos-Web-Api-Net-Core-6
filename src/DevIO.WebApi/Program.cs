using DevIO.Api.Extensions;
using DevIO.Data.Context;
using DevIO.WebApi.Configurations;
using DevIO.WebApi.Extensions;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<MeuDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddIdentityConfiguration(builder.Configuration);

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddWebApiConfig();

builder.Services.ResolverDependecies();
builder.Services.AddLogConfiguration(builder.Configuration);



var app = builder.Build();
app.UseAuthentication();
app.UseMiddleware<ExceptionMiddleware>();
app.UseWebApiConfig();
app.UseLogConfiguration();

app.Run();
