namespace RestaurantManagement.Service.Dtos.AuthDto
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password là bắt buộc.")]
        public string Password { get; set; }

    }
}
