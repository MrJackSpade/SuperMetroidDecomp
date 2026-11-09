namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One named step of a colour effect's program that shows nothing, with its typed operand.</summary>
public abstract record PaletteFxCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private PaletteFxCue()
    {
    }

    /// <summary>Queues a sound of the enemies' library.</summary>
    /// <param name="Sound">The enemies'-library sound a <see cref="QueueSound"/> cue queues.</param>
    public sealed record QueueSound(SoundEffectId? Sound) : PaletteFxCue;

    /// <summary>Later records show in these colours.</summary>
    /// <param name="Target">The colours a <see cref="SetColours"/> cue makes the effect's later records show in.</param>
    public sealed record SetColours(PaletteFxColours Target) : PaletteFxCue;

    /// <summary>Publishes the Norfair heat phase Samus's heat shimmer follows.</summary>
    /// <param name="HeatPhase">The Norfair heat animation phase a <see cref="SetHeatPaletteIndex"/> cue publishes.</param>
    public sealed record SetHeatPaletteIndex(int HeatPhase) : PaletteFxCue;

    /// <summary>The effect watches <see cref="Condition"/> from the next update on.</summary>
    /// <param name="Condition">The condition a <see cref="Watch"/> cue makes the effect watch.</param>
    public sealed record Watch(ColourEffectCondition Condition) : PaletteFxCue;
}
