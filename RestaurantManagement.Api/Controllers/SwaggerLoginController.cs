using Microsoft.AspNetCore.Mvc;

namespace RestaurantManagement.Api.Controllers
{
    [Route("[controller]")]
    public class SwaggerLoginController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var html = @"
                <html>
                    <head>
                        <title>Swagger Login</title>
                        <style>
                            body { font-family: Arial; margin: 50px; background: #f0f0f0; }
                            form { background: white; padding: 20px; border-radius: 8px; width: 300px; margin: auto; box-shadow: 0 0 10px rgba(0,0,0,0.1); }
                            input { display: block; margin-bottom: 10px; width: 100%; padding: 8px; }
                            button { padding: 8px 16px; background: #007bff; color: white; border: none; cursor: pointer; }
                        </style>
                    </head>
                    <body>
                        <form method='post'>
                            <h3>Swagger Login</h3>
                            <label>Username:</label>
                            <input type='text' name='username' required />
                            <label>Password:</label>
                            <input type='password' name='password' required />
                            <button type='submit'>Login</button>
                        </form>
                    </body>
                </html>";
            return Content(html, "text/html");
        }

        [HttpPost]
        public IActionResult Index([FromForm] string username, [FromForm] string password)
        {
            // Replace this check with actual secure authentication in real cases
            if (username == "admin" && password == "pizzadaay123")
            {
                Response.Cookies.Append("SwaggerAccess", "true", new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(30)
                });

                return Redirect("/swagger");
            }

            return Unauthorized("Sai thông tin đăng nhập.");
        }
    }
}
