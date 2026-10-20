namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One palette-FX program command with its typed operand.</summary>
public abstract record PaletteFxCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private PaletteFxCue()
    {
    }

    /// <summary><c>$8D:C673</c>: queues a library-2 sound.</summary>
    /// <param name="Sound">The library-2 sound a <see cref="QueueSoundLibrary2"/> cue queues.</param>
    public sealed record QueueSoundLibrary2(SoundEffectId? Sound) : PaletteFxCue;

    /// <summary><c>$8D:C655</c>: later records write from this CGRAM byte index.</summary>
    /// <param name="ColorIndex">The CGRAM colour a <see cref="SetColorIndex"/> cue makes the object's next record start at.</param>
    public sealed record SetColorIndex(int ColorIndex) : PaletteFxCue;

    /// <summary><c>$8D:F1C6</c>: publishes the Norfair heat palette index.</summary>
    /// <param name="HeatPhase">The Norfair heat animation phase a <see cref="SetHeatPaletteIndex"/> cue publishes.</param>
    public sealed record SetHeatPaletteIndex(int HeatPhase) : PaletteFxCue;

    /// <summary><c>$8D:C5D4</c>: installs a pre-instruction.</summary>
    /// <param name="PreInstruction">The pre-instruction a <see cref="SetPreInstruction"/> cue installs.</param>
    public sealed record SetPreInstruction(PaletteFxPreInstruction PreInstruction) : PaletteFxCue;

    /// <summary>A cue that carries no operand.</summary>
    /// <param name="Kind">What the cue does.</param>
    public sealed record Command(PaletteFxCueKind Kind) : PaletteFxCue;
}
