using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Saves;

namespace SuperMetroid.Diagnostics.Saves;

/// <summary>A session's saves kept in a JSON save file, rewritten atomically whenever the game changes them.</summary>
/// <param name="Path">The JSON save file.</param>
public sealed record GameSaveFile(string Path) : ISessionSaveStorage
{
    /// <inheritdoc/>
    public void Persist(SaveSlotStore saves) => GameSaveFileStore.WriteAtomic(saves, Path);
}
