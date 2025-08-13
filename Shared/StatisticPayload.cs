using System;
using Shared.Models;

namespace Shared;

public class StatisticPayload
{
    public List<CustomersByCountry> CustomersByCountry { get; set; } = new();
    public List<ProductsByCategory> ProductsByCategory { get; set; } = new();
    public List<PurchasesByCustomers> PurchasesByCustomers { get; set; } = new();
    public List<SalesByEmployee> SalesByEmployee { get; set; } = new();
}
