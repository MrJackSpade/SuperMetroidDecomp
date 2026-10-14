using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

/// <summary>Samus as a PLM fixture supplies her (#633).</summary>
internal sealed class FixtureRoomSamus(Func<SamusState?> current) : IRoomSamusAccess
{
    /// <summary>Reads the fixture's current Samus owner on demand.</summary>
    public SamusState? Current => current();
}
