using Microsoft.Playwright;
using SqlSugar;

public class CrawlerEngine
{
    private readonly IPage _page;
    private readonly DatabaseHelper _dbHelper;

    public CrawlerEngine(IPage page, SqlSugarClient db)
    {
        _page = page;
        _dbHelper = new DatabaseHelper(db);
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
                    await ListAsync(list);
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
            if (!_dbHelper.IsAnyTable(table.Table.Name))
            {
                _dbHelper.InitTables(table.Table);
            }

            var dataList = _dbHelper.BuildInsertDataList(scrapedRows, table.Table.Columns);
            await _dbHelper.SaveToDb(dataList, table.Table.Name);
        }
    }

    private async Task ListAsync(ListAction list)
    {
        await _page.WaitForSelectorAsync(list.Selector);
        var rows = await _page.QuerySelectorAllAsync(list.Selector);
        var allTexts = await Task.WhenAll(rows.Select(r => r.InnerTextAsync()));
        var txts = allTexts.Select(t => t.Trim()).ToList();
        txts.ForEach(t => Console.WriteLine(t));

        if (list.SaveToDb)
        {
            if (!_dbHelper.IsAnyTable(list.Table.Name))
            {
                _dbHelper.InitTables(list.Table);
            }

            var dataList = _dbHelper.BuildInsertDataList(txts.Select(t => new List<string> { t }).ToList(), list.Table.Columns);
            await _dbHelper.SaveToDb(dataList, list.Table.Name);
        }
    }
}