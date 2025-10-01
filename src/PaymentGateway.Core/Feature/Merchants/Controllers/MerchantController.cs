using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Core.Feature.Merchants.Services;
using PaymentGateway.Core.Feature.Security.Attributes;
using PaymentGateway.Models.Entities.Requests;
using PaymentGateway.PersistantStorage.Dto;

namespace PaymentGateway.Core.Feature.Merchants.Controllers;
[ApiController]
[Route("Api/merchant/[action]")]
[ValidateAdminApiKey]

public class MerchantController(IMerchantService paymentService) : Controller
{

    [HttpPost()]
    [Route("/api/merchant/create")]
    public async Task<ActionResult<Merchant>> CreateMerchant()
    {
        var payment = await paymentService.CreateMerchantAsync();
        if (payment.IsFailed)
        {
            return BadRequest();
        }
        return new OkObjectResult(payment.Value);
    }
}