using SuperMetroid.Core;
using System.Collections.Immutable;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>
/// The stock opening narration's script layout in the cartridge: bank-$8C page lists, their
/// bounding callbacks and the record shapes <see cref="IntroNarrationExtractor"/> checks.
/// Extraction-only data; the simulation never reads it.
/// </summary>
[EncodingOwner("bank-$8C opening narration script records")]
internal static class IntroNarrationScriptDefinitions
{
    /// <summary>Bank containing the intro background-object streams and glyph records.</summary>
    internal const byte ScriptBank = 0x8c;
    /// <summary>Interpreter command-bit mask distinguishing callbacks from timed records.</summary>
    internal const ushort InstructionCommandBit = 0x8000;
    /// <summary><c>CinematicBGObject_Instruction_Delete</c> at $8B:9698.</summary>
    internal const ushort DeleteOpcode = 0x9698;
    /// <summary><c>Instruction_SetCaretToBlink</c> at $8B:ADD4.</summary>
    internal const ushort SetCaretBlinkingOpcode = 0xadd4;
    /// <summary><c>IndirectInstructionFunction_DrawTextCharacter</c> at $8B:884D.</summary>
    internal const ushort DrawCharacterFunction = 0x884d;
    /// <summary><c>IndirectInstructionFunction_DoNothing</c> at $8B:8849.</summary>
    internal const ushort DoNothingFunction = 0x8849;
    /// <summary>$8C:D683, the no-op marker record shared by each page boundary.</summary>
    internal const ushort MarkerDataPointer = 0xd683;
    /// <summary>Packed tile position (1,1) used by the no-op marker.</summary>
    internal const ushort MarkerPackedPosition = 0x0101;
    /// <summary>One-column-by-one-row dimensions required by each narration glyph payload.</summary>
    internal const ushort SingleTileDimensions = 0x0101;
    /// <summary>Low-byte X component of each packed narration tile position.</summary>
    internal const ushort PackedPositionXMask = 0x00ff;
    /// <summary>Malformed-stream bound; retail's longest page is well below this count.</summary>
    internal const int MaximumRecords = 512;

    /// <summary>
    /// The six pages' stock scripts in narrative order, page one through page six: the lists at $8C:C383, $C797, $CB45,
    /// $CE33, $D15D and $D511, bounded by callbacks $AE43/$AE5B, $AE79/$AE91, $B074/$B08C, $B0B3/$B0CB, $B19B/$B1B3 and
    /// $B228/$B240.
    /// </summary>
    internal static ImmutableArray<IntroNarrationScriptPage> Pages { get; } =
    [
        new(IntroNarrationPageId.Page1, 0xc383, 0xae43, 0xae5b),
        new(IntroNarrationPageId.Page2, 0xc797, 0xae79, 0xae91),
        new(IntroNarrationPageId.Page3, 0xcb45, 0xb074, 0xb08c),
        new(IntroNarrationPageId.Page4, 0xce33, 0xb0b3, 0xb0cb),
        new(IntroNarrationPageId.Page5, 0xd15d, 0xb19b, 0xb1b3),
        new(IntroNarrationPageId.Page6, 0xd511, 0xb228, 0xb240),
    ];
}
