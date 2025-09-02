using BoldReports.Data.WebData;
using BoldReports.Web;
using BoldReports.Web.ReportViewer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace ReportWithRealTime;

[Route("api/{controller}/{action}/{id?}")]
public class BoldReportsAPIController : Controller, IReportController
{
    // The Report Viewer requires a memory cache to store information from consecutive client requests and
    // the rendered report viewer in the server.
    private IMemoryCache _cache;

    // The IWebHostEnvironment is used within the sample to retrieve application data from the wwwroot.
    private IWebHostEnvironment _hostingEnvironment;

    private Dictionary<string, object> _jsonResult;

    public BoldReportsAPIController(IMemoryCache memoryCache, IWebHostEnvironment hostingEnvironment)
    {
        _cache = memoryCache;
        _hostingEnvironment = hostingEnvironment;
        _jsonResult = new();
    }

    //Get action for getting resources from the report
    [ActionName("GetResource")]
    [AcceptVerbs("GET")]
    // Method will be called from Report Viewer client to get the image src for Image report item.
    public object GetResource(ReportResource resource)
    {
        return ReportHelper.GetResource(resource, this, _cache);
    }

    [NonAction]
    public void OnInitReportOptions(ReportViewerOptions reportOption)
    {
        string basePath = _hostingEnvironment.WebRootPath;
        // Here, we have loaded the sales-order-detail.rdl report from the application folder wwwrootResources. sales-order-detail.rdl should be located in the wwwroot\Resources application folder.
        System.IO.FileStream inputStream = new System.IO.FileStream(basePath + @"\Resources\" + reportOption.ReportModel.ReportPath + ".rdl", System.IO.FileMode.Open, System.IO.FileAccess.Read);
        MemoryStream reportStream = new MemoryStream();
        inputStream.CopyTo(reportStream);
        reportStream.Position = 0;
        inputStream.Close();
        reportOption.ReportModel.Stream = reportStream;
    }

    [NonAction]
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

        // Assign all credentials at once
        reportOption.ReportModel.DataSourceCredentials = credentialsList;
    }

    [HttpPost]
    public object PostFormReportAction()
    {
        return ReportHelper.ProcessReport(null, this, _cache);
    }

    [HttpPost]
    public object PostReportAction([FromBody] Dictionary<string, object> jsonArray)
    {
        _jsonResult = jsonArray;
        return ReportHelper.ProcessReport(jsonArray, this, this._cache);
    }
}
