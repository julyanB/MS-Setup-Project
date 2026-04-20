using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using BankingOperationsService.Application.Exceptions;
using BankingOperationsService.Domain.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BankingOperationsService.Web.Middleware;

public class CustomExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CustomExceptionHandlerMiddleware> _logger;

    public CustomExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<CustomExceptionHandlerMiddleware> logger)
    {
        _logger = logger;
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ModelValidationException validationException)
        {
            var errors = validationException.Errors
                .SelectMany(e => e.Value)
                .ToArray();

            _logger.LogInformation(validationException, "Request failed validation: {Errors}", string.Join("; ", errors));
            await MapResponse(context, ErrorCodes.ValidationError, messages: errors);
        }
        catch (NotFoundException notFoundException)
        {
            _logger.LogInformation(notFoundException, "Resource not found: {Message}", notFoundException.Message);
            await MapResponse(context, ErrorCodes.NotFoundError, notFoundException.Message);
        }
        catch (DatabaseException databaseException)
        {
            _logger.LogError(databaseException, "A Database exception occurred.");
            await MapResponse(context, databaseException.Code, databaseException.Message);
        }
        catch (BaseDomainException exception)
        {
            _logger.LogWarning(exception, "Domain rule violated: {ErrorCode} - {Message}", exception.ErrorCode, exception.Message);
            await MapResponse(context, exception.ErrorCode, exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An api handler threw an error.");
            await MapResponse(context, ErrorCodes.GenericError, exception.Message);
        }
    }

    private static async Task MapResponse(HttpContext context, ErrorCodes errorCode, string? message = null, IEnumerable<string>? messages = null)
    {
        var response = new ErrorResponse(errorCode, message, messages);
        await WriteResponse(context, response, (int)ToHttpStatusCode(errorCode));
    }

    private static async Task WriteResponse<TResponse>(HttpContext context, TResponse response, int statusCode)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        JsonSerializerOptions options = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        var responseAsString = JsonSerializer.Serialize(response, options);
        await context.Response.WriteAsync(responseAsString);
    }

    private static HttpStatusCode ToHttpStatusCode(ErrorCodes errorCode) =>
        errorCode switch
        {
            ErrorCodes.ValidationError or
            ErrorCodes.ConcurrencyError or
            ErrorCodes.DuplicateKeyError => HttpStatusCode.BadRequest,
            ErrorCodes.NotFoundError => HttpStatusCode.NotFound,
            _ => HttpStatusCode.InternalServerError
        };
}

public static class CustomExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder builder)
        => builder.UseMiddleware<CustomExceptionHandlerMiddleware>();
}
