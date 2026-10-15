using SuperMetroid.Core.Areas.Tourian;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One boss-specific entry into the shared Tourian statue grey-out program.</summary>
public sealed class TourianStatueGreyPaletteFxProgramDefinition
{
    /// <summary>Binds one statue's palette destination and branch form to its native pointers.</summary>
    internal TourianStatueGreyPaletteFxProgramDefinition(
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

    /// <summary>Reads one boss-specific setup word.</summary>
    public bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        int offset = pointer - ProgramStart;
        ushort? setupWord = offset switch
        {
            0 => (ushort)PaletteFxInstruction.SetColorIndex,
            2 => ColorByteIndex,
            4 when UsesGoto => (ushort)PaletteFxInstruction.Goto,
            6 when UsesGoto =>
                TourianStatueGreyPaletteFxProgramMechanicsDefinitions.FirstFramePointer,
            _ => null,
        };
        if (setupWord.HasValue)
        {
            value = setupWord.Value;
            return true;
        }

        value = 0;
        return false;
    }
}
