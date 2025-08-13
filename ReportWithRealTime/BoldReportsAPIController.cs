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

        foreach (DataSourceInfo item in datasources)
        {
            string jsonValue = System.IO.File.ReadAllText(basePath + $@"\Resources\JsonDataSources\{item.DataSourceName}.json");

            FileDataModel model = new FileDataModel();
            model.DataMode = "inline";
            model.Data = jsonValue;
            item.DataProvider = "JSON";

            DataSourceCredentials DataSourceCredentials = new DataSourceCredentials();
            DataSourceCredentials.Name = item.DataSourceName;
            DataSourceCredentials.UserId = null;
            DataSourceCredentials.Password = null;
            DataSourceCredentials.ConnectionString = JsonConvert.SerializeObject(model);
            DataSourceCredentials.IntegratedSecurity = false;
            reportOption.ReportModel.DataSourceCredentials = new List<DataSourceCredentials>
                        {
                                DataSourceCredentials
                        };
        }
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
