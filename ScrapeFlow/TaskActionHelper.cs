using Microsoft.Extensions.Configuration;

public class TaskActionHelper
{
    public static List<TaskAction> ParseActions(IConfigurationSection tasksSection)
    {
        if (tasksSection == null)
            throw new ArgumentNullException(nameof(tasksSection));

        var actions = new List<TaskAction>();

        foreach (var task in tasksSection.GetChildren())
        {
            var typeStr = task["Type"];
            if (string.IsNullOrWhiteSpace(typeStr))
                throw new InvalidOperationException("动作配置缺少 'Type' 字段。");

            if (!Enum.TryParse<TaskType>(typeStr, true, out var type))
                throw new NotSupportedException($"未知或无效的动作类型: {typeStr}");

            TaskAction? action = type switch
            {
                TaskType.Navigate => task.Get<NavigateAction>(),
                TaskType.Fill => task.Get<FillAction>(),
                TaskType.Click => task.Get<ClickAction>(),
                TaskType.Wait => task.Get<WaitAction>(),
                TaskType.Table => task.Get<TableAction>(),
                TaskType.List => task.Get<ListAction>(),
                _ => throw new NotSupportedException($"未实现的动作类型: {type}")
            };

            if (action is null)
                throw new InvalidOperationException($"无法从配置中反序列化动作类型: {type}");

            actions.Add(action);
        }

        return actions;
    }
}