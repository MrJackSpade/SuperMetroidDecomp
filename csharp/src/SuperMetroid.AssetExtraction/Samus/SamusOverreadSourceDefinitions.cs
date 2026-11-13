using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Where on the cartridge, and how, Samus's glitch paths read past their data: extraction's private knowledge, which it
/// decodes once into the Samus overread services (#1474).
/// </summary>
[EncodingOwner("cartridge regions Samus's glitch paths read past their data")]
internal static class SamusOverreadSourceDefinitions
{
    /// <summary>Bank $80, whose upper half the SpaceTime palette copy reads through its bank-$00 mirror.</summary>
    internal const int SpacetimeCopySourceBank = 0x800000;

    /// <summary>The first bank offset the cartridge serves; the copy's reads below it would reach scratch memory.</summary>
    internal const int CartridgeHalfStart = 0x8000;

    /// <summary>
    /// The bytes from sprite palette six, where the interrupted <c>STA.l $7EC1C0,X</c> at $90:AD14 starts the copy, to the
    /// event bitset that starts the persistent-progress slice of the SRAM mirror.
    /// </summary>
    internal const int ProgressionSliceDistance = 0x1660;

    /// <summary>$90:AD1C compares the inherited read index against $0020 after advancing it.</summary>
    internal const int PaletteByteCount = 0x0020;

    /// <summary>$90:DD75, the byte the HUD's posture-transition admission reads for pose zero; the real flags start at $90:DDAA.</summary>
    internal const int HudPostureFlagsForPoseZero = 0x90dd75;

    /// <summary>$90:D4C6: the Shinespark crash's echo departure table, two angle bytes per pose from the first crash pose.</summary>
    internal const int CrashDepartureTable = 0x90d4c6;

    /// <summary>$90:C2D1: the beam speed table, a cardinal then a diagonal speed word per beam combination.</summary>
    internal const int BeamSpeedTable = 0x90c2d1;

    /// <summary>$90:C254: the uncharged cooldown row, a byte per beam combination.</summary>
    internal const int UnchargedCooldowns = 0x90c254;

    /// <summary>$90:C264: the charged cooldown row, a byte per beam combination.</summary>
    internal const int ChargedCooldowns = 0x90c264;

    /// <summary>$90:C283: the held-fire cooldown row, a byte per beam combination.</summary>
    internal const int HeldFireCooldowns = 0x90c283;

    /// <summary>$91:B010: the table of each pose's animation program, a bank-$91 word per pose number.</summary>
    internal const int SamusAnimationProgramTable = 0x91b010;

    /// <summary>Bank $91, which holds the animation programs the table's words name.</summary>
    internal const int SamusAnimationProgramBank = 0x910000;

    /// <summary>
    /// The lowest animation program byte the interpreter takes as an instruction (#1535); every byte below it is a delay.
    /// An instruction's low four bits select its handler, whatever its high bits.
    /// </summary>
    internal const int FirstAnimationInstructionByte = 0x80;

    /// <summary>
    /// The bytes the Power Bomb fuse adds to the live list pointer to switch to the fast animation. The Chainsaw glitch
    /// beam runs the fuse over its own list, so it applies this offset to a beam list.
    /// </summary>
    internal const int FastAnimationOffset = 28;
}
