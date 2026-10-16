using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>
/// The accepted-NMI counters $05B5/$05B6 and the layer-one position the frontend holds while no room runtime
/// exists. As in SuperMetroidRuntime.StepFrame, each update begins with the accepted NMI that delivered its input;
/// CommonBootSection clears bank $7E after the reset update's NMI, so the reset dispatch zeroes them after that count.
/// </summary>
internal sealed class MenuNmiFrameCounters
{
    /// <summary>Accepted-NMI counter $05B6.</summary>
    internal ushort FrameCounter { get; private set; }

    /// <summary>Accepted-NMI counter $05B5.</summary>
    internal byte FrameCounter8 { get; private set; }

    /// <summary>Preserves layer-one fractions $090F/$0913 across runtime replacement; only camera movement,
    /// scroll clamps, or the reset vector's wipe change them. The next runtime consumes the fractions.</summary>
    private FixedPosition<RoomPixels> retainedLayer1Position;

    /// <summary>Clears the counters as the reset vector's bank-$7E wipe does.</summary>
    internal void Reset()
    {
        FrameCounter = 0;
        FrameCounter8 = 0;
        retainedLayer1Position = FixedPosition<RoomPixels>.Zero;
    }

    /// <summary>Counts the accepted NMI that began an update while no runtime owns the counters.</summary>
    internal void Advance()
    {
        FrameCounter = unchecked((ushort)(FrameCounter + 1));
        FrameCounter8 = unchecked((byte)(FrameCounter8 + 1));
    }

    /// <summary>Hands accepted-NMI counters and the retained layer-one fractions to a new runtime.</summary>
    internal void PublishTo(SuperMetroidRuntime runtime)
    {
        runtime.Clock.Import(FrameCounter8, FrameCounter);
        runtime.BackgroundPhase.AdoptLayer1Position(retainedLayer1Position);
    }

    /// <summary>Takes the counters and the camera's whole layer-one position back before runtime release.</summary>
    internal void RetainFrom(SuperMetroidRuntime runtime)
    {
        FrameCounter = runtime.Clock.Counter;
        FrameCounter8 = runtime.Clock.Counter8;
        retainedLayer1Position = runtime.Room.Camera?.Position ?? runtime.Room.InheritedLayer1Position;
    }
}
