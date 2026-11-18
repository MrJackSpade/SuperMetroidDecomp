using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Verification.Harness;

/// <summary>Samus as a room-object or colour-effect fixture supplies her (#633, #2391).</summary>
internal sealed class FixtureRoomSamus(Func<SamusState> current) : IRoomSamusAccess
{
    /// <summary>A Samus no check installed (#2391): reading her fails at the dependency the check reached.</summary>
    internal static FixtureRoomSamus Uninstalled { get; } =
        new(() => throw new InvalidOperationException("The fixture did not install Samus."));

    /// <summary>Reads the fixture's current Samus owner on demand.</summary>
    public IRoomSamus Current => current().RoomView;
}
