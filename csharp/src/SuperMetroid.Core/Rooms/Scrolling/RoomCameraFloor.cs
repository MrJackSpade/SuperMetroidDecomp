namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>
/// The lowest Y the room's camera may take, which keeps it from looking above a fight's arena (#2243). Every room starts
/// with none; Spore Spawn's live fight holds his arena's top and releases it as he dies. The camera keeps to it once it has
/// tracked Samus on both axes, before anything reads where it stands.
/// </summary>
[RoomLifetime]
public sealed partial class RoomCameraFloor
{
    /// <summary>The lowest Y, in room pixels, the camera's top edge may take, or null while nothing holds the camera.</summary>
    public int? LowestY { get; private set; }

    /// <summary>Keeps the camera's top edge at or below <paramref name="lowestY"/> from its next tracking on.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="lowestY"/> lies above the room's top.</exception>
    internal void Hold(int lowestY) => LowestY = Ensure.AtLeastZero(lowestY);

    /// <summary>Frees the camera to look anywhere its scrolling allows.</summary>
    internal void Release() => LowestY = null;
}
