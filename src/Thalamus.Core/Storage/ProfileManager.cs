using Thalamus.Core.Models;
using System.Text.Json;

namespace Thalamus.Core.Storage;

public class ProfileManager : IProfileManager
{
    private readonly string _baseFolder;
    public ProfileManager(string baseFolder)
    {
        _baseFolder = baseFolder;
    }
    private string ProfilesFile => Path.Combine(_baseFolder, "profiles.json");
    private string DatabaseFor(Guid profileId) =>
        Path.Combine(_baseFolder, "profiles", $"{profileId}.db");
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private async Task<ProfileStore> LoadAsync()
    {
        if (!File.Exists(ProfilesFile))
            return new ProfileStore();
        var json = await File.ReadAllTextAsync(ProfilesFile);
        return JsonSerializer.Deserialize<ProfileStore>(json) ?? new ProfileStore();
        
    }
    private async Task SaveAsync(ProfileStore store)
    {
        Directory.CreateDirectory(_baseFolder);
        var json = JsonSerializer.Serialize(store, JsonOptions);
        await File.WriteAllTextAsync(ProfilesFile, json);
    }
    public async Task<Profile> CreateProfileAsync(string name)
    {
        Guid profileId = Guid.NewGuid();
        var newProfile = new Profile(profileId, name, DateTime.UtcNow);
        var store = await LoadAsync();
        store.Profiles.Add(newProfile);
        if(store.CurrentProfileId == null)
            store.CurrentProfileId = profileId;
        await SaveAsync(store);
        return newProfile;
    }
    public async Task DeleteProfileAsync(Guid profileId)
    {
        var store = await LoadAsync();
        if(store.CurrentProfileId == profileId)
            store.CurrentProfileId = null;
        store.Profiles.RemoveAll(p => p.Id == profileId);
        var path = DatabaseFor(profileId);
        if (File.Exists(path))
            File.Delete(path);
        await SaveAsync(store);
    }
    public async Task ModifyProfileAsync(Guid profileId, string newName)
    {
        var store = await LoadAsync();
        var i = store.Profiles.FindIndex(p => p.Id == profileId);
        if (i >= 0)
            store.Profiles[i] = store.Profiles[i] with { Name = newName };
        await SaveAsync(store);
    }
    public async Task<Profile?> GetCurrentProfileAsync()
    {
        var store = await LoadAsync();
        return store.Profiles.Find(p => p.Id == store.CurrentProfileId);
    }
    public async Task<IReadOnlyList<Profile>> ProfileListAsync()
    {
        var store = await LoadAsync();
        return store.Profiles;
    }
    public async Task SetCurrentProfileAsync(Guid profileId)
    {
        var store = await LoadAsync();
        if(store.Profiles.Exists(p => p.Id == profileId))
            store.CurrentProfileId = profileId;
        else
            throw new ArgumentException("Profile not found", nameof(profileId));
        await SaveAsync(store);
    }
    public async Task<IRepository> GetProfileRepositoryAsync(Guid profileId)
    {
        var store = await LoadAsync();
        if(!store.Profiles.Exists(p => p.Id == profileId))
            throw new ArgumentException("Profile not found", nameof(profileId));
        var factory = new DbConnectionFactory(DatabaseFor(profileId));
        return new SqliteRepository(factory);
    }
}