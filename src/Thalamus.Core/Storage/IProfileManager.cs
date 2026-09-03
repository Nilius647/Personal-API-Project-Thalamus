using Thalamus.Core.Models;

namespace Thalamus.Core.Storage;

public interface IProfileManager
{
    Task<Profile> CreateProfileAsync(string name);
    Task DeleteProfileAsync(Guid profileId);
    Task ModifyProfileAsync(Guid profileId, string newName);
    Task<IReadOnlyList<Profile>> ProfileListAsync();
    Task<Profile> GetCurrentProfileAsync();
    Task SetCurrentProfileAsync(Guid profileId);
    Task<IRepository> GetProfileRepositoryAsync(Guid profileId);
}