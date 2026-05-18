using System.Net;
using System.Text.Json;
using FluentValidation;

namespace ECommerce.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
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

            var result = JsonSerializer.Serialize(new
            {
                Fail = true,
                StatusCode = (int)code,
                Errors = errors ?? (object)new
                {
                    //develop stage 
                    //Message = exception.Message,
                    //Detail = exception.StackTrace
                    // user stage: only show generic error message for unhandled errors
                    Message = "An unexpected error occurred. Please try again later."
                }
            });

            return context.Response.WriteAsync(result);
        }
    }
}
