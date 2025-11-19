using Microsoft.Playwright;
using SqlSugar;

public class CrawlerEngine
{
    private readonly IPage _page;
    private readonly SqlSugarClient _db;

    public CrawlerEngine(IPage page, SqlSugarClient db)
    {
        _page = page;
        _db = db;
    }

    public async Task ExecuteAsync(List<TaskAction> actions)
    {
        foreach (var action in actions)
        {
            switch (action)
            {
                case NavigateAction navigate:
                    await GotoAsync(navigate.Url);
                    break;
                case FillAction fill:
                    await FillAsync(fill.Selector, fill.Value);
                    break;
                case ClickAction click:
                    await ClickAsync(click.Selector);
                    break;
                case WaitAction wait:
                    await WaitAsync(wait.Selector, wait.Timeout);
                    break;
                case TableAction table:
                    await TableAsync(table);
                    break;
                case ListAction list:
                    await ListAsync(list.Selector);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
    }

    private async Task GotoAsync(string url)
    {
        await _page.GotoAsync(url);
    }

    private async Task FillAsync(string selector, string value)
    {
        await _page.Locator(selector).FillAsync(value);
    }

    private async Task ClickAsync(string selector)
    {
        await _page.Locator(selector).ClickAsync();
    }

    private async Task WaitAsync(string selector, float? timeout)
    {
        if (timeout is null)
            await _page.WaitForSelectorAsync(selector);
        else
            await _page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions { Timeout = timeout });
    }

    private async Task TableAsync(TableAction table)
    {
        await _page.WaitForSelectorAsync(table.Selector);
        var rows = await _page.QuerySelectorAllAsync(table.Selector);
        List<List<string>> scrapedRows = new List<List<string>>();
        foreach (var row in rows)
        {
            var cells = await row.QuerySelectorAllAsync("td");
            var allTexts = await Task.WhenAll(cells.Select(c => c.InnerTextAsync()));
            var scrapedRow = table.Columns.Where(i => i < allTexts.Length).Select(i => allTexts[i]?.Trim() ?? "").ToList();
            scrapedRows.Add(scrapedRow);
            Console.WriteLine(string.Join(" | ", scrapedRow));
        }

        if (table.SaveToDb)
        {
            var dbHelper = new DatabaseHelper(_db);
            if (!dbHelper.IsAnyTable(table.Table.Name))
            {
                dbHelper.InitTables(table.Table);
            }

            var dataList = dbHelper.BuildInsertDataList(scrapedRows, table.Table.Columns);
            await dbHelper.SaveToDb(dataList, table.Table.Name);
        }
    }

    private async Task ListAsync(string selector)
    {
        await _page.WaitForSelectorAsync(selector);
        var list = await _page.QuerySelectorAllAsync(selector);
        var allTexts = await Task.WhenAll(list.Select(r => r.InnerTextAsync()));
        allTexts.Select(t => t.Trim()).ToList().ForEach(t => Console.WriteLine(t));
    }
}