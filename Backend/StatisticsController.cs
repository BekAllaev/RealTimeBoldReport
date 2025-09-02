using Bogus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Shared;
using Shared.Models;

namespace Backend
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly Faker _faker;
        private readonly IHubContext<StatisticsHub> _hub;

        public StatisticsController(Faker faker, IHubContext<StatisticsHub> hub)
        {
            _faker = faker;
            _hub = hub;
        }

        // POST api/statistics/updateStatistics
        [HttpPost("updateStatistics")]
        public async Task<IActionResult> UpdateStatistics()
        {
            var customersByCountry = new Faker<CustomersByCountry>("en")
                .RuleFor(x => x.CountryName, f => f.Address.Country())
                .RuleFor(x => x.Count, f => f.Random.Number(1, 500))
                .Generate(10);

            var productsByCategory = new Faker<ProductsByCategory>("en")
                .RuleFor(x => x.CategoryName, f => f.Commerce.Categories(1)[0])
                .RuleFor(x => x.ProductsCount, f => f.Random.Number(1, 200))
                .Generate(10);

            var purchasesByCustomers = new Faker<PurchasesByCustomers>("en")
                .RuleFor(x => x.Customer, f => f.Name.FullName())
                .RuleFor(x => x.Purchases, f => Math.Round(f.Random.Double(50, 5000), 2))
                .Generate(10);

            var salesByEmployee = new Faker<SalesByEmployee>("en")
                .RuleFor(x => x.Id, f => f.IndexFaker + 1)
                .RuleFor(x => x.LastName, f => f.Name.LastName())
                .RuleFor(x => x.Sales, f => Math.Round(f.Random.Double(100, 20000), 2))
                .Generate(10);

            var payload = new StatisticPayload
            {
                CustomersByCountry = customersByCountry,
                ProductsByCategory = productsByCategory,
                PurchasesByCustomers = purchasesByCustomers,
                SalesByEmployee = salesByEmployee
            };

            await _hub.Clients.All.SendAsync("ReceiveAllStats", payload);

            return Accepted();
        }
    }
}
