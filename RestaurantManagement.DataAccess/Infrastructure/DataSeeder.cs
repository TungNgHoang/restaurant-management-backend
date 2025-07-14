using Microsoft.Extensions.DependencyInjection;
using RestaurantManagement.DataAccess.Models;
using RestaurantManagement.DataAccess.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace RestaurantManagement.DataAccess.Infrastructure
{
    public static class DataSeeder
    {
        public static async Task SeedDataAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<RestaurantDBContext>();
            context.Database.EnsureCreated();

            if (!context.TblUserAccounts.Any())
            {
                var hasher = new PasswordHasher<TblUserAccount>();

                var adminAccount = new TblUserAccount
                {
                    UacId = Guid.NewGuid(),
                    UacEmail = "admin@example.com",
                    UacRole = "Admin",
                    CreatedAt = DateTime.Now,
                    IsDeleted = false
                };
                adminAccount.UacPassword = hasher.HashPassword(adminAccount, "admin123");

                var managerAccount = new TblUserAccount
                {
                    UacId = Guid.NewGuid(),
                    UacEmail = "manager@example.com",
                    UacRole = "Manager",
                    CreatedAt = DateTime.Now,
                    IsDeleted = false
                };
                managerAccount.UacPassword = hasher.HashPassword(managerAccount, "manager123");

                var cashierAccount = new TblUserAccount
                {
                    UacId = Guid.NewGuid(),
                    UacEmail = "cashier@example.com",
                    UacRole = "Cashier",
                    CreatedAt = DateTime.Now,
                    IsDeleted = false
                };
                cashierAccount.UacPassword = hasher.HashPassword(cashierAccount, "cashier123");

                var staffAccount = new TblUserAccount
                {
                    UacId = Guid.NewGuid(),
                    UacEmail = "staff@example.com",
                    UacRole = "Staff",
                    CreatedAt = DateTime.Now,
                    IsDeleted = false
                };
                staffAccount.UacPassword = hasher.HashPassword(staffAccount, "staff123");

                context.TblUserAccounts.AddRange(adminAccount, managerAccount, cashierAccount, staffAccount);
                await context.SaveChangesAsync();
            }
        }
    }
}
