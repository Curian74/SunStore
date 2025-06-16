using SunStore.ViewModel;

namespace SunStore.Services
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(HttpContext context, VnPaymentRequestModel model);
        VnPaymentResponseModel PaymentExcute(IQueryCollection collections);
    }
}
