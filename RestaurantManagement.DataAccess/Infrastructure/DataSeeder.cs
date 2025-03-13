using Microsoft.Extensions.DependencyInjection;
using RestaurantManagement.DataAccess.Models;
using RestaurantManagement.DataAccess.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.DataAccess.Infrastructure
{
    public static class DataSeeder
    {
        public static async Task SeedDataAsync(IServiceProvider serviceProvider)
        {
            // Tạo scope mới để lấy DbContext
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<RestaurantDBContext>();

            // Đảm bảo DB đã được tạo (nếu dùng EnsureCreated)
            // hoặc bạn có thể dùng context.Database.Migrate() nếu sử dụng Migration
            context.Database.EnsureCreated();

            // Kiểm tra xem bảng TblUserAccount đã có dữ liệu chưa
            if (!context.TblUserAccounts.Any())
            {
                // Nếu chưa có, thêm 2 tài khoản (Admin, Manager)
                var adminAccount = new TblUserAccount
                {
                    UacEmail = "admin@example.com",
                    UacPassword = "admin123", // Thực tế nên băm mật khẩu
                    UacRole = "Admin"
                };

                var managerAccount = new TblUserAccount
                {
                    UacEmail = "manager@example.com",
                    UacPassword = "manager123", // Thực tế nên băm mật khẩu
                    UacRole = "Manager"
                };

                // Thêm vào DbSet
                context.TblUserAccounts.AddRange(adminAccount, managerAccount);

                // Lưu thay đổi vào DB
                await context.SaveChangesAsync();
            }
        }
    }
}
