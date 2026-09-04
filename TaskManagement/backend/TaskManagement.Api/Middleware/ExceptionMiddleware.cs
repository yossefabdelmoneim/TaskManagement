using TaskManagement.Api.Responses;
using System.Text.Json;
using TaskManagement.Api.Exceptions;

namespace TaskManagement.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

     public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);

            context.Response.ContentType = "application/json";

            ErrorResponse response;

            switch (ex)
            {
                case NotFoundException:
                    context.Response.StatusCode = StatusCodes.Status404NotFound;

                    response = new ErrorResponse
                    {
                        StatusCode = 404,
                        Message = ex.Message
                    };
                    break;

                case UnauthorizedException:
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                    response = new ErrorResponse
                    {
                        StatusCode = 401,
                        Message = ex.Message
                    };
                    break;

                default:
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                    response = new ErrorResponse
                    {
                        StatusCode = 500,
                        Message = "An unexpected error occurred."
                    };
                    break;
            }

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        }
            }
}