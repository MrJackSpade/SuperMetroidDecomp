using SuperMetroid.Core.Saves;

namespace SuperMetroid.Core.Frontend;

/// <summary>Where a host keeps a session's battery-backed saves between runs.</summary>
public interface ISessionSaveStorage
{
    /// <summary>Keeps <paramref name="saves"/>, which the game has just changed.</summary>
    /// <param name="saves">The session's saves.</param>
    void Persist(SaveSlotStore saves);
}
