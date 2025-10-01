using System.Net;

using FluentValidation.Results;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Core.Feature.Payments.Services;
using PaymentGateway.Core.Feature.Security.Attributes;
using PaymentGateway.Models.Entities.Requests;
using PaymentGateway.Models.Entities.Responses;
using PaymentGateway.PersistantStorage.Dto;

namespace PaymentGateway.Core.Feature.Payments.Controllers;

[ApiController]
[ValidateMerchantApiKey]
public class PaymentsController(IPaymentService paymentService, PaymentValidator paymentValidator) : Controller
{
    [HttpGet()]
    [Route("api/payment/{id:guid}")]
    public async Task<ActionResult<PostPaymentResponse?>> GetPaymentAsync(Guid id)
    {
        var merchant = Request.HttpContext.Items["merchant"] as Merchant;
        if (merchant == null)
        {
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
        var payment = await paymentService.GetPaymentAsync(id, merchant.Id);
        if (payment.IsFailed)
        {
            return BadRequest();
        }

        return new OkObjectResult(payment.Value);
    }

    [HttpPost()]
    [Route("api/payment/process")]
    public async Task<ActionResult<PostPaymentResponse>> ProcessPaymentAsync(
        MerchantPaymentProcessRequest processRequest)
    {
        if (Request.HttpContext.Items["merchant"] is not Merchant merchant)
        {
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
        processRequest.MerchantId = merchant.Id;
        ValidationResult results = await paymentValidator.ValidateAsync(processRequest);

        if (!results.IsValid)
        {
            return new OkObjectResult(await paymentService.RejectPaymentAsync(processRequest));
        }

        var payment = await paymentService.ProcessPaymentAsync(processRequest);
        if (payment.IsFailed)
        {
            return BadRequest();
        }

        return new OkObjectResult(payment.Value);
    }
}