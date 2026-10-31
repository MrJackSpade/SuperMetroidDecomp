using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>
/// The layer-one position the frontend holds while no room runtime exists. The frame counters live on the session's
/// <see cref="FrameClock"/>, which every runtime shares.
/// </summary>
internal sealed class MenuLayer1Retention
{
    /// <summary>Preserves layer-one fractions $090F/$0913 across runtime replacement; only camera movement,
    /// scroll clamps, or the reset vector's wipe change them. The next runtime consumes the fractions.</summary>
    private FixedPosition<RoomPixels> retainedLayer1Position;

    /// <summary>Clears the position as the reset vector's bank-$7E wipe does.</summary>
    internal void Reset() => retainedLayer1Position = FixedPosition<RoomPixels>.Zero;

    /// <summary>Hands the retained layer-one fractions to a new runtime.</summary>
    internal void PublishTo(SuperMetroidRuntime runtime) => runtime.BackgroundPhase.AdoptLayer1Position(retainedLayer1Position);

    /// <summary>Takes the camera's whole layer-one position back before runtime release.</summary>
    internal void RetainFrom(SuperMetroidRuntime runtime) =>
        retainedLayer1Position = runtime.Room.Camera?.Position ?? runtime.Room.InheritedLayer1Position;
}
