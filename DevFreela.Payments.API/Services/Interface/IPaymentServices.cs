using DevFreela.Payments.API.Model;
using System.Threading.Tasks;

namespace DevFreela.Payments.API.Services.Interface
{
    public interface IPaymentServices
    {
        Task<bool>Process(PaymentsInfoInputModel paymentsInfoInputModel);
    }
}
