using Thalamus.Core.Models;

namespace Thalamus.Core.Storage;

internal class ProfileStore
{
    public List<Profile> Profiles {get; set;} = [];
    public Guid? CurrentProfileId {get; set;}
}