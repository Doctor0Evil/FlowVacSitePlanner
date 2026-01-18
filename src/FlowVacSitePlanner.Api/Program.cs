using System.Text.Json;
using System.Text.Json.Serialization;
using FlowVacSitePlanner.Api.Geo;
using FlowVacSitePlanner.Api.Services.Implementations;
using FlowVacSitePlanner.Api.Services.Interfaces;
using FlowVacSitePlanner.Domain.Planning;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Geo / NTS
builder.Services.AddSingleton(GeometryFactoryProvider.CreateGeometryFactory());

// Domain services
builder.Services.AddScoped<IShardValidationService, ShardValidationService>();
builder.Services.AddScoped<IEcoImpactScoringService, EcoImpactScoringService>();
builder.Services.AddScoped<IPlanningService, PlanningService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

// Constrained, shard-backed planning endpoint surface.
app.MapPost("/api/planning/sites",
    async ([FromBody] Models.Requests.SitePlanningRequest request,
           [FromServices] IPlanningService planningService,
           CancellationToken cancellationToken) =>
    {
        var result = await planningService.PlanAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return Results.BadRequest(new
            {
                error = result.ErrorCode,
                message = result.ErrorMessage
            });
        }

        var response = Models.Responses.SitePlanningResponse.FromDomain(result);
        return Results.Ok(response);
    });

app.Run();
