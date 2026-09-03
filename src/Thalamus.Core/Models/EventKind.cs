namespace Thalamus.Core.Models;

public enum EventKind   //Don't change the order
{
    Boot = 1,
    Shutdown = 2,
    Lock = 3,
    Unlock = 4,
    Sleep = 5,
    Wake = 6
}