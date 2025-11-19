public class DateTimeHelper
{
    public static void OutputDatetime(int result)
    {
        DateTime utcNow = DateTime.UtcNow;
        TimeZoneInfo chinaZone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
        DateTime beijingTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, chinaZone);
        Console.WriteLine($"-------------------- [{beijingTime:yyyy-MM-dd HH:mm:ss}] 已成功插入{result}条 --------------------");
    }
}