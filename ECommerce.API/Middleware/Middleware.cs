using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace ECommerce.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unhandled exception. Method={Method}, Path={Path}, TraceId={TraceId}",
                    context.Request.Method,
                    context.Request.Path,
                    context.TraceIdentifier);

                // MVC pages should not return JSON; only API routes use this handler.
                if (!context.Request.Path.StartsWithSegments("/api"))
                    throw;

                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var code = HttpStatusCode.InternalServerError; // Default to 500
            object? errors = null;

            if (exception is ValidationException validationException)
            {
                code = HttpStatusCode.BadRequest;
                errors = validationException.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
            }
            else if (exception is KeyNotFoundException)
            {
                code = HttpStatusCode.NotFound;
                errors = new { Message = exception.Message };
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            const string genericMessage = "An unexpected error occurred. Please try again later.";
            var errorPayload = errors ?? (object)new { Message = genericMessage };
            var message = exception is KeyNotFoundException knf && !string.IsNullOrWhiteSpace(knf.Message)
                ? knf.Message
                : genericMessage;

            var result = JsonSerializer.Serialize(new
            {
                message,
                Fail = true,
                StatusCode = (int)code,
                Errors = errorPayload
            });

            return context.Response.WriteAsync(result);
        }
    }
}
