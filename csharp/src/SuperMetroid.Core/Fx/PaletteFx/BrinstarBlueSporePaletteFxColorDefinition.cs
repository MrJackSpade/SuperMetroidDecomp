namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One room-specific entry into the fourteen-frame blue-spore palette loop.</summary>
public sealed class BrinstarBlueSporePaletteFxColorDefinition
{
    /// <summary>Builds the definition from its owner's program values.</summary>
    /// <param name="owner">The room family the entry serves.</param>
    /// <param name="program">The program whose records the entry shows.</param>
    internal BrinstarBlueSporePaletteFxColorDefinition(BrinstarBlueSporePaletteOwner owner, PaletteFxProgram program)
    {
        Owner = owner;
        Rows = PaletteFxColorRows.Of(program, BrinstarBlueSporePaletteFxColorDefinitions.ColorsPerFrame);
    }

    /// <summary>The room family represented by this program.</summary>
    public BrinstarBlueSporePaletteOwner Owner { get; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public PaletteFxColorRows Rows { get; }
}
