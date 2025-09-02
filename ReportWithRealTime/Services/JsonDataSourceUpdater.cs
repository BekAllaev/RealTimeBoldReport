using System;
using Newtonsoft.Json;
using Shared;

namespace ReportWithRealTime.Services;

public class JsonDataSourceUpdater : IJsonDataSourceUpdater
{
    private IWebHostEnvironment _hostingEnvironment;

    public JsonDataSourceUpdater(IWebHostEnvironment hostingEnvironment)
    {
        _hostingEnvironment = hostingEnvironment;
    }

    public async Task UpdateDataSourceAsync(StatisticPayload statisticPayload)
    {
        await UpdateAsync(statisticPayload.CustomersByCountry, nameof(statisticPayload.CustomersByCountry));
        await UpdateAsync(statisticPayload.ProductsByCategory, nameof(statisticPayload.ProductsByCategory));
        await UpdateAsync(statisticPayload.PurchasesByCustomers, nameof(statisticPayload.PurchasesByCustomers));
        await UpdateAsync(statisticPayload.SalesByEmployee, nameof(statisticPayload.SalesByEmployee));
    }

    private async Task UpdateAsync<T>(IEnumerable<T> items, string statisticsName)
    {
        var filePath = _hostingEnvironment.WebRootPath + $@"\Resources\JsonDataSources\{statisticsName}.json";

        var json = JsonConvert.SerializeObject(items);

        await File.WriteAllTextAsync(filePath, json);
    }
}
