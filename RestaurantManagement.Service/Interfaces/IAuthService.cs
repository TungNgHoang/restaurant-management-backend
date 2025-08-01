﻿namespace RestaurantManagement.Service.Interfaces
{
    public interface IAuthService
    {
        // Nhận đối tượng TblUserAccount và trả về JWT token dạng string
        Task<string> GenerateJwtTokenAsync(TblUserAccount user);
        Task<bool> LogoutAsync(string token); // Thêm dòng này để khai báo LogoutAsync

    }
}
