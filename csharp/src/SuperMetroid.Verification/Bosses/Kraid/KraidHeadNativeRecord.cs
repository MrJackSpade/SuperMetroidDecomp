using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Programs;
namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>One native Kraid head-list record, decoded from the cartridge for the ROM oracle and the snapshot import.</summary>
/// <param name="Pointer">The record's bank-$A7 address.</param>
/// <param name="Cursor">The typed cursor that stands for the record.</param>
/// <param name="Opcode">The record's first word: a pose's duration, a cry's call target, or the terminator.</param>
/// <param name="Tilemap">A pose's head tilemap address; zero otherwise.</param>
/// <param name="OuterShape">A pose's outer (vulnerable) mouth-shape address; zero otherwise.</param>
/// <param name="InnerShape">A pose's inner (invulnerable) mouth-shape address, $FFFF for none; zero otherwise.</param>
/// <param name="Sound">A cry's library-two sound number; zero otherwise.</param>
internal readonly record struct KraidHeadNativeRecord(
    ushort Pointer, AnimationProgramCursor<KraidHeadProgram> Cursor, ushort Opcode, ushort Tilemap, ushort OuterShape,
    ushort InnerShape, ushort Sound)
{
    /// <summary>Whether the record is a timed pose.</summary>
    internal bool IsPose => Opcode < 0x8000;

    /// <summary>Whether the record ends its list.</summary>
    internal bool IsTerminator => Opcode == KraidHeadNativeCursors.Terminator;
}
