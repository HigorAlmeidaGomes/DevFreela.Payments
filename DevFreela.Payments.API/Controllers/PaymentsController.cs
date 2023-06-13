using DevFreela.Payments.API.Model;
using DevFreela.Payments.API.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

namespace DevFreela.Payments.API.Controllers
{
    [Route("api/payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentServices _paymentServices;
        public PaymentsController(IPaymentServices paymentServices)
        {
            _paymentServices = paymentServices;
        }

        [HttpPost]
        public async Task<IActionResult> PostPay([FromBody] PaymentsInfoInputModel paymentsInfoInputModel)
        {
            var result = await _paymentServices.Process(paymentsInfoInputModel);
            return result ? NoContent() : BadRequest(result);
        }
    }
}
