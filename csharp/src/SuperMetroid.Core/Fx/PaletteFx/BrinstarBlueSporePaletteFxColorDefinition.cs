namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One room-specific entry into the fourteen-frame blue-spore palette loop.</summary>
/// <param name="owner">The room family the entry serves.</param>
/// <param name="program">The program whose records the entry shows.</param>
public sealed class BrinstarBlueSporePaletteFxColorDefinition(BrinstarBlueSporePaletteOwner owner, PaletteFxProgram program)
{
    /// <summary>The room family represented by this program.</summary>
    public BrinstarBlueSporePaletteOwner Owner { get; } = owner;

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public PaletteFxColorRows Rows { get; } = PaletteFxColorRows.Of(program, BrinstarBlueSporePaletteFxColorDefinitions.ColorsPerFrame);
}
