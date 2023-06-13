using DevFreela.Payments.API.Model;
using DevFreela.Payments.API.Services.Interface;
using System.Threading.Tasks;

namespace DevFreela.Payments.API.Services.Service
{
    public class PaymentServices : IPaymentServices
    {
        public Task<bool> Process(PaymentsInfoInputModel paymentsInfoInputModel)
        {
            return Task.FromResult(true);
        }
    }
}
