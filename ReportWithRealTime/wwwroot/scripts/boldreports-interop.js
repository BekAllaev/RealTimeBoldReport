// Interop file to render the Bold Report Viewer component with properties.
window.BoldReports = {
    RenderViewer: function (elementID, reportViewerOptions) {
        $("#" + elementID).boldReportViewer({
            reportPath: reportViewerOptions.reportName,
            reportServiceUrl: reportViewerOptions.serviceURL
        });
    },

    Refresh: function (elementID, ts) {
        const $el = $("#" + elementID);
        const inst = $el.data("boldReportViewer"); // jQuery-инстанс Bold Reports

        debugger;

        if (!inst) return;

        inst.refreshReport(); // <- ключевая строка
    }
}