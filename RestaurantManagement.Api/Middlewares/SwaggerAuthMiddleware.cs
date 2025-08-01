﻿namespace RestaurantManagement.Api.Middlewares
{
    public class SwaggerAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public SwaggerAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // Nếu request là Swagger và chưa đăng nhập HOẶC không phải Admin/Manager
            if (context.Request.Path.StartsWithSegments("/swagger") &&
                (!context.User.Identity.IsAuthenticated ||
                 !(context.User.IsInRole("Admin") || context.User.IsInRole("Manager"))))
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Forbidden: Swagger is restricted.");
                return;
            }

            await _next(context);
        }
    }
}
