using System;

public static class EnumHelper
{
    /// <summary>
    /// 将字符串安全转换为枚举值，若失败则返回默认值（或可选抛异常）
    /// </summary>
    /// <typeparam name="T">枚举类型</typeparam>
    /// <param name="value">要转换的字符串</param>
    /// <param name="ignoreCase">是否忽略大小写（默认 true）</param>
    /// <param name="defaultValue">转换失败时的默认值（默认 default(T)）</param>
    /// <returns>转换后的枚举值</returns>
    public static T ToEnum<T>(string value, bool ignoreCase = true, T defaultValue = default!)
        where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return defaultValue;

        if (Enum.TryParse(value, ignoreCase, out T result))
            return result;

        return defaultValue;
    }

    /// <summary>
    /// 尝试将字符串转换为枚举（类似 TryParse，但泛型更友好）
    /// </summary>
    public static bool TryParse<T>(string value, bool ignoreCase, out T result)
        where T : struct, Enum
    {
        return Enum.TryParse(value, ignoreCase, out result);
    }
}