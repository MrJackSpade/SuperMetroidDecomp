using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Frontend;

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

    // Layer-one fractions $090F/$0913 also outlive a released runtime: only camera movement
    // and scroll clamps write them, never a load. Only the reset vector's wipe clears them.
    // The released camera's whole position is kept with them; the next runtime reads only
    // the fractions.
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

    /// <summary>Hands the counters to a newly allocated runtime.</summary>
    internal void PublishTo(SuperMetroidRuntime runtime)
    {
        runtime.NmiHandler.AdoptNmiFrameCounters(FrameCounter8, FrameCounter);
        runtime.BackgroundPhase.AdoptLayer1Position(retainedLayer1Position);
    }

    /// <summary>Takes the counters back from a runtime that is being released.</summary>
    internal void RetainFrom(SuperMetroidRuntime runtime)
    {
        FrameCounter = runtime.Nmi.NmiFrameCounter;
        FrameCounter8 = runtime.Nmi.NmiFrameCounter8;
        retainedLayer1Position = runtime.Room.Camera?.Position ?? runtime.Room.InheritedLayer1Position;
    }
}
