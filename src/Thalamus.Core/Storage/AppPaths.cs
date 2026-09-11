namespace Thalamus.Core.Storage;

public static class AppPaths
{
    public static string BaseFolder =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Thalamus");
    public static string ProfilesFile =>
        Path.Combine(BaseFolder, "profiles.json");
    public static string DatabaseFor(Guid profileId)
    {
        return Path.Combine(BaseFolder, "profiles", $"{profileId}.db");
    }
}