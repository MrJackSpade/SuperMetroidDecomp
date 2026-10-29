using SuperMetroid.Core.Areas.Tourian;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One boss-specific entry into the shared Tourian statue grey-out program.</summary>
public sealed class TourianStatueGreyPaletteFxColorDefinition
{
    /// <summary>Binds one statue's branch form to its native pointers.</summary>
    internal TourianStatueGreyPaletteFxColorDefinition(
        TourianStatueBoss boss,
        ushort definitionPointer,
        ushort programStart,
        bool usesGoto)
    {
        Boss = boss;
        DefinitionPointer = definitionPointer;
        ProgramStart = programStart;
        UsesGoto = usesGoto;
    }

    /// <summary>The statue boss represented by this entry.</summary>
    public TourianStatueBoss Boss { get; }

    /// <summary>The boss-specific <c>PalFxDef</c> identity in bank $8D.</summary>
    public ushort DefinitionPointer { get; }

    /// <summary>The boss-specific entry instruction-list pointer.</summary>
    public ushort ProgramStart { get; }

    /// <summary>Whether this entry uses an explicit branch rather than falling through.</summary>
    public bool UsesGoto { get; }
}
