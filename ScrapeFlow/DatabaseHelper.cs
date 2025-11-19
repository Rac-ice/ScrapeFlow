using SqlSugar;

public class DatabaseHelper
{
    private readonly SqlSugarClient _db;

    public DatabaseHelper(SqlSugarClient db)
    {
        _db = db;
    }

    public bool IsAnyTable(string tableName)
    {
        return _db.DbMaintenance.IsAnyTable(tableName);
    }

    public void InitTables(Table table)
    {
        var typeBuilder = _db.DynamicBuilder().CreateClass(table.Name);
        foreach (var col in table.Columns)
        {
            if (col.IsPrimaryKey && col.IsIdentity)
            {
                typeBuilder.CreateProperty(col.Name, ParseType(col.Type), new SugarColumn { IsPrimaryKey = col.IsPrimaryKey, IsIdentity = col.IsIdentity });
            }
            else if (col.Length is not null)
            {
                typeBuilder.CreateProperty(col.Name, ParseType(col.Type), new SugarColumn { Length = col.Length.Value, IsNullable = true });
            }
            else
            {
                typeBuilder.CreateProperty(col.Name, ParseType(col.Type), new SugarColumn { IsNullable = true });
            }
        }
        var type = typeBuilder.BuilderType();
        _db.CodeFirst.InitTables(type);
    }

    public List<Dictionary<string, object>> BuildInsertDataList(List<List<string>> scrapedRows, List<Column> columns)
    {
        if (scrapedRows == null || !scrapedRows.Any())
            return new List<Dictionary<string, object>>();

        var insertColumns = columns.Where(c => !(c.IsPrimaryKey && c.IsIdentity)).ToList();

        var result = new List<Dictionary<string, object>>();

        for (int i = 0; i < scrapedRows.Count; i++)
        {
            var row = scrapedRows[i];

            var dict = new Dictionary<string, object>();
            for (int j = 0; j < insertColumns.Count; j++)
            {
                var col = insertColumns[j];
                var rawValue = row[j];
                var typedValue = ParseValue(rawValue, col.Type);
                dict[col.Name] = typedValue ?? DBNull.Value;
            }
            result.Add(dict);
        }

        return result;
    }

    public async Task SaveToDb(List<Dictionary<string, object>> dataList, string tableName)
    {
        if (dataList.Any())
        {
            _db.DbMaintenance.TruncateTable(tableName);
            var result = await _db.Insertable(dataList).AS(tableName).ExecuteCommandAsync();
            DateTimeHelper.OutputDatetime(result);
        }
    }

    private Type ParseType(string type)
    {
        return type switch
        {
            "string" => typeof(string),
            "int" => typeof(int),
            "long" => typeof(long),
            "float" => typeof(float),
            "double" => typeof(double),
            "decimal" => typeof(decimal),
            "datetime" => typeof(DateTime),
            "bool" => typeof(bool),
            _ => typeof(string)
        };
    }

    private object? ParseValue(string? value, string type)
    {
        if (string.IsNullOrEmpty(value)) return null;

        return type switch
        {
            "string" => value,
            "int" => int.TryParse(value, out var i) ? i : null,
            "long" => long.TryParse(value, out var l) ? l : null,
            "float" => float.TryParse(value, out var f) ? f : null,
            "double" => double.TryParse(value, out var dbl) ? dbl : null,
            "decimal" => decimal.TryParse(value.Replace("%", "").Replace(",", "").Trim(), out var d) ? d : null,
            "datetime" => DateTime.TryParse(value, out var dt) ? dt : null,
            "bool" => bool.TryParse(value, out var b) ? b : null,
            _ => value
        };
    }
}