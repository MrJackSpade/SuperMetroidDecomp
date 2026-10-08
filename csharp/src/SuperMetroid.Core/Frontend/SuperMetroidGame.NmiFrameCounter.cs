namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    // Before a room runtime exists, the frontend owns the accepted-NMI counters $05B5/$05B6.
    // As in SuperMetroidRuntime.StepFrame, each update begins with the accepted NMI that
    // delivered its input. CommonBootSection clears bank $7E after the reset update's NMI,
    // so the reset dispatch zeroes them after that count.
    private ushort menuNmiFrameCounter;
    private byte menuNmiFrameCounter8;

    /// <summary>Clears the counters as the reset vector's bank-$7E wipe does.</summary>
    private void ResetMenuNmiFrameCounters()
    {
        menuNmiFrameCounter = 0;
        menuNmiFrameCounter8 = 0;
    }

    /// <summary>Counts the accepted NMI that began this update while no runtime owns the counters.</summary>
    private void AdvanceMenuNmiFrameCounters()
    {
        if (runtime is not null)
            return;
        menuNmiFrameCounter = unchecked((ushort)(menuNmiFrameCounter + 1));
        menuNmiFrameCounter8 = unchecked((byte)(menuNmiFrameCounter8 + 1));
    }

    /// <summary>Hands the frontend's counters to a newly allocated runtime.</summary>
    private void PublishMenuNmiFrameCounters() =>
        runtime!.AdoptNmiFrameCounters(menuNmiFrameCounter8, menuNmiFrameCounter);

    /// <summary>Takes the counters back from a runtime that is being released.</summary>
    private void RetainRuntimeNmiFrameCounters()
    {
        menuNmiFrameCounter = runtime!.NmiFrameCounter;
        menuNmiFrameCounter8 = runtime.NmiFrameCounter8;
    }
}
