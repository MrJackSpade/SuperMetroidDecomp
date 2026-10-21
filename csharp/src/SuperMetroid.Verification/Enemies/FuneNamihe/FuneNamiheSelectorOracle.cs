using SuperMetroid.Core.Enemies.FuneNamihe;
namespace SuperMetroid.Verification.Enemies.FuneNamihe;

/// <summary>
/// The cartridge's Fune/Namihe animation selector table at $A8:96D3-$A8:96E2, as the verification oracle and the
/// snapshot codec read it: the actor's selection word is a cursor into this table, two bytes further for right-facing
/// entries and four bytes further for idle ones, with Namihe's four entries after Fune's.
/// </summary>
internal static class FuneNamiheSelectorOracle
{
    /// <summary>$A8:96D7, Fune idle-left selector cursor.</summary>
    internal const ushort FuneIdleLeftCursor = 0x96d7;

    /// <summary>$A8:96DF, Namihe idle-left selector cursor.</summary>
    internal const ushort NamiheIdleLeftCursor = 0x96df;

    /// <summary>Cursor distance from an active selector to the idle selector of the same species and facing.</summary>
    internal const ushort ActiveCursorDelta = 4;

    /// <summary>Cursor distance between left- and right-facing selectors of one species and activity.</summary>
    internal const ushort FacingRightCursorDelta = 2;

    /// <summary>Decodes a selector cursor into its species, activity and facing, rejecting any cursor outside the table.</summary>
    /// <param name="cursor">The selector cursor.</param>
    internal static DecodeResult Decode(ushort cursor)
    {
        int offset = cursor - (FuneIdleLeftCursor - ActiveCursorDelta);
        if ((uint)offset >= 16 || (offset & 1) != 0)
            throw new InvalidDataException($"Fune/Namihe instruction selector $A8:{cursor:X4} is not authored.");
        return new DecodeResult(offset >= 8, (offset & ActiveCursorDelta) == 0, (offset & FacingRightCursorDelta) != 0);
    }

    /// <summary>The animation the selector at <paramref name="cursor"/> names.</summary>
    /// <param name="cursor">The selector cursor.</param>
    internal static FuneNamiheProgram ProgramAt(ushort cursor) => Decode(cursor) switch
    {
        (false, true, false) => FuneNamiheProgram.FuneActiveLeft,
        (false, true, true) => FuneNamiheProgram.FuneActiveRight,
        (false, false, false) => FuneNamiheProgram.FuneIdleLeft,
        (false, false, true) => FuneNamiheProgram.FuneIdleRight,
        (true, true, false) => FuneNamiheProgram.NamiheActiveLeft,
        (true, true, true) => FuneNamiheProgram.NamiheActiveRight,
        (true, false, false) => FuneNamiheProgram.NamiheIdleLeft,
        (true, false, true) => FuneNamiheProgram.NamiheIdleRight,
    };
}
