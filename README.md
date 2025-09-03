# Bold report + Blazor + SignalR

## How to launch?
Download repo and hit F5 from VS or hit "Run all" from menu "Run and debug" if you are in VS Code.

## References
Syncfusion Bold Report - https://www.boldreports.com/  
Blazor - https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor  
SignalR - https://learn.microsoft.com/en-us/aspnet/core/signalr/introduction?view=aspnetcore-9.0  
How to add Report Web viewer into Blazor - https://help.boldreports.com/report-viewer-sdk/blazor-reporting/report-viewer/add-report-viewer-to-a-blazor-application/

A lot of information while writing this repo was extracted from the discussion with Syncfusion Bold Report support(to whom I would like to express my deepest thanks).   
Here are they:
- Setting data for pie chart - https://support.boldreports.com/support/tickets/748970
- Setting data for several charts on the report - https://support.boldreports.com/support/tickets/750984
- How to configure single serie for column charts - [https://support.boldreports.com/support/tickets/752089](https://support.boldreports.com/support/tickets/752089)
- How to troubleshoot problem with CSV library - https://support.boldreports.com/support/tickets/757167
- How to update report with new data in real-time - https://support.boldreports.com/support/tickets/760479 

## Description 
In this app you will see only two pages. First one is Home page where  report viewer is, second one is UpdateStatistics page, there is only button that has only one thing to do - it sends POST request to backend. Backend generates some fake data and sends it back with SignlaR request, after this Blazor updates report on the Home page.

## Remarks 
In this [link](https://help.boldreports.com/report-viewer-sdk/blazor-reporting/report-viewer/add-report-viewer-to-a-blazor-application/) you will see instruction on how to add Report Web viewer into Blazor. The only thing I want to mention - when you will add refernces to the `boldreports-interop.js` script do it that way:
```
<!-- Blazor interop file -->
<script src="scripts/boldreports-interop.js"></script>
```

## How to make Report Web viewer read data from JSON?

First of all edit `PostReportAction` like this
```
    private Dictionary<string, object> _jsonResult = new();
    public object PostReportAction([FromBody] Dictionary<string, object> jsonArray)
    {
        _jsonResult = jsonArray;
        return ReportHelper.ProcessReport(jsonArray, this, this._cache);
    }
```

> We will use `_jsonResult` later 

You will need to edit `OnReportLoaded` method in `BoldReportApiController` like this:
```
    public void OnReportLoaded(ReportViewerOptions reportOption)
    {
        string basePath = _hostingEnvironment.WebRootPath;

        List<DataSourceInfo> datasources = ReportHelper.GetDataSources(_jsonResult, this, _cache);

        // List to hold all credentials
        List<DataSourceCredentials> credentialsList = new List<DataSourceCredentials>();

        foreach (DataSourceInfo item in datasources)
        {
            string jsonFilePath = Path.Combine(basePath, "Resources", "JsonDataSources", $"{item.DataSourceName}.json");

            if (System.IO.File.Exists(jsonFilePath))
            {
                string jsonValue = System.IO.File.ReadAllText(jsonFilePath);

                FileDataModel model = new FileDataModel
                {
                    DataMode = "inline",
                    Data = jsonValue
                };

                item.DataProvider = "JSON";

                DataSourceCredentials dataSourceCredentials = new DataSourceCredentials
                {
                    Name = item.DataSourceName,
                    UserId = null,
                    Password = null,
                    ConnectionString = JsonConvert.SerializeObject(model),
                    IntegratedSecurity = false
                };

                credentialsList.Add(dataSourceCredentials);
            }
            else
            {
                // Optional: log or handle missing file
                Console.WriteLine($"JSON file not found for data source: {item.DataSourceName}");
            }
        }
```

This line extract all JSON data sources from report
```
List<DataSourceInfo> datasources = ReportHelper.GetDataSources(_jsonResult, this, _cache);
```

I have JSON files in `wwwroot/Resources/JsonDataSources` folder where name of these files matches name of JSON data sources in the report. So these code fills data sources from report with content
```
FileDataModel model = new FileDataModel
{
    DataMode = "inline",
    Data = jsonValue
};
```
Where `jsonValue` contains data that were read from JSON files

## How report real-time update happens?
I send POST request from Blazor app, then I send SignalR request back to Blazor with new JSON data. On Blazor I just update JSON files, after this I update the page and when page is updated Report viewer reads already updated data.
