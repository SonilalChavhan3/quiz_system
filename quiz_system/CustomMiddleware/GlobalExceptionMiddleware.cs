using System.Text.Json;

namespace quiz_system.CustomMiddleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
                _logger.LogError(ex, "An unhandled exception occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "text/html";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;


            var htmlContent = $@"
            <!DOCTYPE html>
            <html lang=""en"">
            <head>
                <meta charset=""UTF-8"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                <title>Error</title>
                <style>
                    body {{ font-family: Arial, sans-serif; margin: 50px; color: #333; }}
                    .container {{ max-width: 600px; margin: auto; text-align: center; }}
                    h1 {{ color: #e74c3c; }}
                    p {{ font-size: 18px; }}
                    pre {{ background: #f4f4f4; padding: 10px; border-radius: 5px; overflow-x: auto; }}
                </style>
            </head>
            <body>
                <div class=""container"">
                    <h1>500 - Internal Server Error</h1>
                    <p>An unexpected error occurred. Please try again later.</p>
                    <h3>Error Details:</h3>
                    <pre>{exception.Message}</pre>
                </div>
            </body>
            </html>";
            return context.Response.WriteAsync(htmlContent);
        }
    }

}
