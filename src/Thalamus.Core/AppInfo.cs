namespace Thalamus.Core;

public static class AppInfo
{
    public const string Version = "0.2.0";

    public static string BuildInfo()
    {
        var asm = typeof(AppInfo).Assembly;
        return $"Thalamus {Version} - {asm.GetName().Name}";
    }
}