using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>Samus as room PLMs see her (#633): absent until the room's Samus exists.</summary>
public interface IRoomSamusAccess
{
    /// <summary>The current Samus, or none.</summary>
    SamusState? Current { get; }
}
