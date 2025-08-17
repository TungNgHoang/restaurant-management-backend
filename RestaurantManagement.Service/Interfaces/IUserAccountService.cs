namespace RestaurantManagement.DataAccess.Interfaces
{
    public interface IUserAccountService 
    {
        Task<UserAccountDto> CreateUserAccountAsync(UserAccountDto userAccountDto);
        Task<UserAccountDto> GetUserAccountByIdAsync(Guid id);
        Task<string> LoginAsync(LoginRequestDto loginRequest);
        Task ChangePasswordAsync(ChangePasswordDto dto);

    }
}
