public abstract class TaskAction
{
    public TaskType Type { get; set; }

    public int Order { get; set; }
}

public class NavigateAction : TaskAction
{
    public string Url { get; set; } = string.Empty;
}

public class FillAction : TaskAction
{
    public string Selector { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}

public class ClickAction : TaskAction
{
    public string Selector { get; set; } = string.Empty;
}

public class WaitAction : TaskAction
{
    public string Selector { get; set; } = string.Empty;

    public float? Timeout { get; set; }
}

public class TableAction : TaskAction
{
    public string Selector { get; set; } = string.Empty;

    public List<int> Columns { get; set; } = new List<int>();

    public bool SaveToDb { get; set; }

    public Table Table { get; set; } = new Table();
}

public class Table
{
    public string Name { get; set; } = string.Empty;

    public List<Column> Columns { get; set; } = new List<Column>();
}

public class Column
{
    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = "string";

    public int? Length { get; set; }

    public bool IsPrimaryKey { get; set; }

    public bool IsIdentity { get; set; }
}

public class ListAction : TaskAction
{
    public string Selector { get; set; } = string.Empty;

    public bool SaveToDb { get; set; }

    public Table Table { get; set; } = new Table();
}