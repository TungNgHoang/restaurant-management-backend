using System.Net;
using System.Text;
using RestaurantManagement.Core.Exceptions;
using RestaurantManagement.Core.ApiModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace RestaurantManagement.Api.Middlewares
{
    public class ExceptionMiddleware
    {

        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            var originalResponseBodyStream = httpContext.Response.Body;
            using (var responseBodyStream = new MemoryStream())
            {
                httpContext.Response.Body = responseBodyStream;

                try
                {
                    // Log the incoming request
                    //await LogRequest(httpContext);

                    await _next(httpContext);

                    // Log the outgoing response
                    await LogResponse(httpContext);
                }
                catch (ErrorException ex)
                {
                    await HandleCustomExceptionAsync(httpContext, ex);
                }
                catch (UnauthorizedAccessException ex)
                {
                    await HandleUnauthorizedExceptionAsync(httpContext, ex);
                }
                catch (Exception ex)
                {
                    await HandleExceptionAsync(httpContext, ex);
                }
                finally
                {
                    // Ensure to copy the contents of the memory stream to the original response body stream
                    responseBodyStream.Seek(0, SeekOrigin.Begin);
                    await responseBodyStream.CopyToAsync(originalResponseBodyStream);
                }
            }
        }



        private async Task HandleCustomExceptionAsync(HttpContext context, ErrorException exception)
        {
            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            var apiResponse = new ApiResponseModel(exception.StatusCode, exception.Data);
            var jsonResponse = JsonConvert.SerializeObject(apiResponse, serializerSettings);

            var builder = new StringBuilder();
            builder.AppendLine($"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}");
            builder.AppendLine("Incoming Request:");
            builder.AppendLine($"Method: {context.Request.Method}");
            builder.AppendLine($"Path: {context.Request.Path}");

            foreach (var header in context.Request.Headers)
            {
                builder.AppendLine($"{header.Key}: {header.Value}");
            }

            if (context.Request.ContentLength > 0)
            {
                var buffer = new byte[(int)context.Request.ContentLength];
                await context.Request.Body.ReadAsync(buffer, 0, buffer.Length);
                var requestBody = Encoding.UTF8.GetString(buffer);
                builder.AppendLine($"Body: {requestBody}");

                //context.Request.Body.Position = 0;
            }
            builder.AppendLine("Outgoing Response:");
            builder.AppendLine($"StatusCode: {exception.StatusCode}");
            builder.AppendLine($"Message: {exception.Data}");

            var logMessage = builder.ToString();

            await context.Response.WriteAsync(jsonResponse);
        }

        private async Task HandleUnauthorizedExceptionAsync(HttpContext context, UnauthorizedAccessException exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

            await context.Response.WriteAsync(exception.Message);
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}");
            builder.AppendLine("Incoming Request:");
            builder.AppendLine($"Method: {context.Request.Method}");
            builder.AppendLine($"Path: {context.Request.Path}");

            foreach (var header in context.Request.Headers)
            {
                builder.AppendLine($"{header.Key}: {header.Value}");
            }

            if (context.Request.ContentLength > 0)
            {
                var buffer = new byte[(int)context.Request.ContentLength];
                await context.Request.Body.ReadAsync(buffer, 0, buffer.Length);
                var requestBody = Encoding.UTF8.GetString(buffer);
                builder.AppendLine($"Body: {requestBody}");

                // context.Request.Body.Position = 0;
            }
            builder.AppendLine("Outgoing Response:");
            builder.AppendLine($"Message: {exception.Message}");
            builder.AppendLine($"StackTrace: {exception.StackTrace?.Trim()}");

            var logMessage = builder.ToString();
            _logger.LogError(logMessage);

            Console.WriteLine(exception.Message);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;


            await context.Response.WriteAsync("An error occurred. Please contact AmericanBank support.");
            await context.Response.WriteAsync(exception.Message + exception.StackTrace);
        }

        private async Task LogRequest(HttpContext context)
        {
            context.Request.EnableBuffering();

            var builder = new StringBuilder();
            builder.AppendLine("Incoming Request:");
            builder.AppendLine($"Method: {context.Request.Method}");
            builder.AppendLine($"Path: {context.Request.Path}");

            foreach (var header in context.Request.Headers)
            {
                builder.AppendLine($"{header.Key}: {header.Value}");
            }

            if (context.Request.ContentLength > 0)
            {
                var buffer = new byte[(int)context.Request.ContentLength];
                await context.Request.Body.ReadAsync(buffer, 0, buffer.Length);
                var requestBody = Encoding.UTF8.GetString(buffer);
                builder.AppendLine($"Body: {requestBody}");

                context.Request.Body.Position = 0;
            }

            var logMessage = builder.ToString();
            _logger.LogInformation(logMessage);
        }

        private async Task LogResponse(HttpContext context)
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            var builder = new StringBuilder();
            builder.AppendLine($"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}");
            builder.AppendLine("Incoming Request:");
            builder.AppendLine($"Method: {context.Request.Method}");
            builder.AppendLine($"Path: {context.Request.Path}");

            foreach (var header in context.Request.Headers)
            {
                builder.AppendLine($"{header.Key}: {header.Value}");
            }

            if (context.Request.ContentLength > 0)
            {
                var buffer = new byte[(int)context.Request.ContentLength];
                await context.Request.Body.ReadAsync(buffer, 0, buffer.Length);
                var requestBody = Encoding.UTF8.GetString(buffer);
                builder.AppendLine($"Body: {requestBody}");

                //context.Request.Body.Position = 0;
            }
            builder.AppendLine("Outgoing Response:");
            builder.AppendLine($"Status Code: {context.Response.StatusCode}");
            builder.AppendLine($"Body: {responseBody}");

            var logMessage = builder.ToString();
            _logger.LogInformation(logMessage);
        }


    }
}
