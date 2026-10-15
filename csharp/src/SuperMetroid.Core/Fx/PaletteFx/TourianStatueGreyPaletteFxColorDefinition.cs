using SuperMetroid.Core.Areas.Tourian;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One boss-specific entry into the shared Tourian statue grey-out program.</summary>
public sealed class TourianStatueGreyPaletteFxColorDefinition
{
    /// <summary>Binds one statue's palette destination and branch form to its native pointers.</summary>
    internal TourianStatueGreyPaletteFxColorDefinition(
        TourianStatueBoss boss,
        ushort definitionPointer,
        ushort programStart,
        ushort colorByteIndex,
        bool usesGoto)
    {
        Boss = boss;
        DefinitionPointer = definitionPointer;
        ProgramStart = programStart;
        ColorByteIndex = colorByteIndex;
        UsesGoto = usesGoto;
    }

    /// <summary>The statue boss represented by this entry.</summary>
    public TourianStatueBoss Boss { get; }

    /// <summary>The boss-specific <c>PalFxDef</c> identity in bank $8D.</summary>
    public ushort DefinitionPointer { get; }

    /// <summary>The boss-specific entry instruction-list pointer.</summary>
    public ushort ProgramStart { get; }

    /// <summary>The byte index of the statue's first CGRAM color.</summary>
    public ushort ColorByteIndex { get; }

    /// <summary>Whether this entry uses an explicit branch rather than falling through.</summary>
    public bool UsesGoto { get; }
}
