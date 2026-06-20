using Application.Common.Exceptions;

namespace ECommerce.Api.Middlewares
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
            catch (Exception e)
            {
                await HandleExceptionAsync(context, e);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = exception switch
            {
                AppValidationException => StatusCodes.Status400BadRequest,

                BadRequestException => StatusCodes.Status400BadRequest,

                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,

                ForbiddenException => StatusCodes.Status403Forbidden,

                NotFoundException => StatusCodes.Status404NotFound,

                ConflictException => StatusCodes.Status409Conflict,

                TooManyRequestsException => StatusCodes.Status429TooManyRequests,

                _ => StatusCodes.Status500InternalServerError
            };

            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(exception, "Unhandled exception occurred.");
            }
            else
            {
                _logger.LogWarning(exception, "Handled exception occurred.");
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            object response = exception switch
            {
                AppValidationException validationException => new
                {
                    statusCode,
                    message = validationException.Message,
                    errors = validationException.Errors,
                    traceId = context.TraceIdentifier
                },

                _ => new
                {
                    statusCode,
                    message = statusCode == StatusCodes.Status500InternalServerError
                        ? "An unexpected error occurred."
                        : exception.Message,
                    traceId = context.TraceIdentifier
                }
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
