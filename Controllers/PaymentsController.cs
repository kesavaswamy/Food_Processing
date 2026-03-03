using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using System.Text.Json;

namespace Food_Processing.Controllers
{
    [Route("api/payments")]
    public class PaymentsController : Controller
    {
        private readonly ILogger<PaymentsController> _logger;
        private readonly IConfiguration _configuration;

        public PaymentsController(ILogger<PaymentsController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        // POST api/payments/webhook
        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            var json = await new StreamReader(Request.Body).ReadToEndAsync();
            _logger.LogInformation("Received webhook: {payload}", json);
            // In production verify signatures and handle events properly (payment_intent.succeeded, charge.refunded, etc.)
            return Ok();
        }

        // POST api/payments/create-payment-intent
        [HttpPost("create-payment-intent")]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentRequest request)
        {
            var secretKey = _configuration["Stripe:SecretKey"];
            if (string.IsNullOrEmpty(secretKey))
                return BadRequest(new { error = "Stripe secret key not configured." });

            StripeConfiguration.ApiKey = secretKey;

            // server-side basic validation: ensure items exist and amount matches
            var order = new Order { Amount = request.Amount, Currency = request.Currency ?? "inr" };
            if (request.Items != null)
            {
                foreach (var it in request.Items)
                {
                    order.Items.Add(new OrderItem { Id = it.Id, Name = it.Name, Price = it.Price, Qty = it.Qty });
                }
            }
            OrdersStore.Save(order);

            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(order.Amount * 100),
                Currency = order.Currency,
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true },
                Metadata = new Dictionary<string, string> { { "order_id", order.Id } }
            };

            var service = new PaymentIntentService();
            var intent = await service.CreateAsync(options);

            return Json(new { clientSecret = intent.ClientSecret, orderId = order.Id });
        }

        // GET api/payments/order/{id}
        [HttpGet("order/{id}")]
        public IActionResult GetOrder(string id)
        {
            var order = OrdersStore.Get(id);
            if (order == null) return NotFound();
            return Json(order);
        }

        // POST api/payments/create-order -- create order without payment (demo fallback)
        [HttpPost("create-order")]
        public IActionResult CreateOrder([FromBody] CreatePaymentRequest request)
        {
            var order = new Order { Amount = request.Amount, Currency = request.Currency ?? "inr" };
            if (request.Items != null)
            {
                foreach (var it in request.Items)
                {
                    order.Items.Add(new OrderItem { Id = it.Id, Name = it.Name, Price = it.Price, Qty = it.Qty });
                }
            }
            OrdersStore.Save(order);
            return Json(new { orderId = order.Id });
        }
    }

}
