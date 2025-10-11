using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;

namespace Lampersky.DynamicFields.Filters;

public class HeadScriptInjectorFilter(string scriptUrl) : ActionFilterAttribute
{
    public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        var response = context.HttpContext.Response;
        var originalBodyStream = response.Body;

        using var memoryStream = new MemoryStream();
        response.Body = memoryStream;

        var executedContext = await next();

        memoryStream.Position = 0;

        string html;
        using (var reader = new StreamReader(memoryStream, Encoding.UTF8))
        {
            html = await reader.ReadToEndAsync();
        }

        if (!string.IsNullOrEmpty(html))
        {
            html = html.Replace("<head>", $"<head><script src=\"{scriptUrl}\"></script>");
        }

        var modifiedBytes = Encoding.UTF8.GetBytes(html ?? string.Empty);
        await originalBodyStream.WriteAsync(modifiedBytes, 0, modifiedBytes.Length);

        response.Body = originalBodyStream;
    }
}