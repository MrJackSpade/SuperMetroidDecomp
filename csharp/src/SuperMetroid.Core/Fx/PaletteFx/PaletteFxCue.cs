namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One palette-FX program command with its typed operand.</summary>
public abstract record PaletteFxCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private PaletteFxCue()
    {
    }

    /// <summary>Queues a sound of the enemies' library.</summary>
    /// <param name="Sound">The enemies'-library sound a <see cref="QueueSound"/> cue queues.</param>
    public sealed record QueueSound(SoundEffectId? Sound) : PaletteFxCue;

    /// <summary>Later records write from these colours.</summary>
    /// <param name="Target">The colours a <see cref="SetColours"/> cue makes the object's later records colour.</param>
    public sealed record SetColours(PaletteFxColours Target) : PaletteFxCue;

    /// <summary>Publishes the Norfair heat palette index.</summary>
    /// <param name="HeatPhase">The Norfair heat animation phase a <see cref="SetHeatPaletteIndex"/> cue publishes.</param>
    public sealed record SetHeatPaletteIndex(int HeatPhase) : PaletteFxCue;

    /// <summary>Installs a frame behaviour.</summary>
    /// <param name="FrameBehavior">The frame behaviour a <see cref="SetFrameBehavior"/> cue installs.</param>
    public sealed record SetFrameBehavior(PaletteFxFrameBehavior FrameBehavior) : PaletteFxCue;

    /// <summary>A cue that carries no operand.</summary>
    /// <param name="Kind">What the cue does.</param>
    public sealed record Command(PaletteFxCueKind Kind) : PaletteFxCue;
}
