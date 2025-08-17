using Microsoft.AspNetCore.Mvc;
using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;
using System.Text;
using RestaurantManagement.Core.Models;

namespace RestaurantManagement.Api.Filters
{
    public class LogActionFilter : IAsyncActionFilter
    {
        private readonly ElasticsearchClient _esClient;

        public LogActionFilter(ElasticsearchClient esClient)
        {
            _esClient = esClient;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var stopwatch = Stopwatch.StartNew();

            // Log trước khi execute action
            var log = new ApiLog
            {
                Controller = context.ActionDescriptor.RouteValues["controller"],
                Action = context.ActionDescriptor.RouteValues["action"],
                HttpMethod = context.HttpContext.Request.Method,
                Path = context.HttpContext.Request.Path,
                QueryString = context.HttpContext.Request.QueryString.ToString(),
                UserId = context.HttpContext.User?.FindFirst("sub")?.Value ?? "Anonymous" // Giả sử dùng JWT, thay bằng claim của bạn
            };

            // Đọc request body nếu cần (chỉ cho POST/PUT, và rewind stream)
            if (context.HttpContext.Request.ContentLength > 0)
            {
                context.HttpContext.Request.EnableBuffering();
                using var reader = new StreamReader(context.HttpContext.Request.Body, Encoding.UTF8, true, 1024, true);
                log.RequestBody = await reader.ReadToEndAsync();
                context.HttpContext.Request.Body.Position = 0; // Rewind để controller đọc lại
            }

            // Execute action
            var executed = await next();

            // Log sau khi execute
            stopwatch.Stop();
            log.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
            log.StatusCode = executed.HttpContext.Response.StatusCode;

            if (executed.Exception != null)
            {
                log.Exception = executed.Exception.Message;
                executed.ExceptionHandled = true; // Optional: Handle exception nếu cần
            }

            // Index log vào Elasticsearch asynchronously
            _ = _esClient.IndexAsync(log); // Fire-and-forget để không block
        }
    }
}