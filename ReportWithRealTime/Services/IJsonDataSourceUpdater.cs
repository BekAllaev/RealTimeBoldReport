using System;
using Shared;

namespace ReportWithRealTime.Services;

public interface IJsonDataSourceUpdater
{
    Task UpdateDataSourceAsync(StatisticPayload statisticPayload);
}
