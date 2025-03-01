using EventMgmt;
using Microsoft.AspNetCore.Connections;
using System;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<AppDbcontext>();
builder.Services.AddControllers();
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
//builder.Services.AddSingleton<IConnectionFactory>(new ConnectionFactory(builder.Configuration.GetConnectionString("DefaultConnection")));
//builder.Services.AddControllers();
//var app = builder.Build();
// Configure the HTTP request pipeline.
//app.MapControllers();
//app.Run();


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
