using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>
/// The camera position the frontend holds while no room runtime exists. The frame counters live on the session's
/// <see cref="FrameClock"/>, which every runtime shares.
/// </summary>
internal sealed class MenuCameraRetention
{
    /// <summary>Preserves the camera's fractions across runtime replacement; only camera movement,
    /// scroll clamps, or the reset vector's wipe change them. The next runtime consumes the fractions.</summary>
    private FixedPosition<RoomPixels> retainedCameraPosition;

    /// <summary>Clears the position as the reset's memory wipe does.</summary>
    internal void Reset() => retainedCameraPosition = FixedPosition<RoomPixels>.Zero;

    /// <summary>Hands the retained camera fractions to a new runtime.</summary>
    internal void PublishTo(SuperMetroidRuntime runtime) => runtime.BackgroundPhase.AdoptCameraPosition(retainedCameraPosition);

    /// <summary>Takes the camera's whole position back before runtime release.</summary>
    internal void RetainFrom(SuperMetroidRuntime runtime) =>
        retainedCameraPosition = runtime.Room.Camera?.Position ?? runtime.Room.InheritedCameraPosition;
}
