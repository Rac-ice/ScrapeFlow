using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using SqlSugar;

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var dbConfig = config.GetSection("Database").Get<DatabaseConfig>() ?? throw new InvalidOperationException("Database config missing");
var browserConfig = config.GetSection("Browser").Get<BrowserConfig>() ?? throw new InvalidOperationException("Browser config missing");
var tasksSection = config.GetSection("Tasks") ?? throw new InvalidOperationException("Tasks config missing");

SqlSugarClient db = new SqlSugarClient(new ConnectionConfig()
{
    ConnectionString = dbConfig.ConnectionString,
    DbType = EnumHelper.ToEnum<DbType>(dbConfig.DbType),
    IsAutoCloseConnection = true
});

using var playwright = await Playwright.CreateAsync();

await using var browser = await playwright.Chromium.LaunchAsync(new()
{
    Headless = browserConfig.Headless
});

var context = await browser.NewContextAsync();
if (browserConfig.Timeout is not null)
{
    context.SetDefaultTimeout(browserConfig.Timeout.Value);
    context.SetDefaultNavigationTimeout(browserConfig.Timeout.Value);
}

var page = await context.NewPageAsync();

var actions = TaskActionHelper.ParseActions(tasksSection).OrderBy(a => a.Order).ToList();

var engine = new CrawlerEngine(page, db);
await engine.ExecuteAsync(actions);