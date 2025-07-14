namespace RestaurantManagement.Service.Interfaces
{
    public interface IPaymentService
    {
        Task CheckoutAndPayAsync(Guid resId, Guid ordId, string proCode, string payMethod);
    }
}
