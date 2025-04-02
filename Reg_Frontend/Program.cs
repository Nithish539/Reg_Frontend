﻿var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseStaticFiles(); 
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.Run();
