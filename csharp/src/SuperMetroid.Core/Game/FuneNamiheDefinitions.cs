namespace SuperMetroid.Core.Game;

/// <summary>Compiled cartridge identities and instruction selectors for Fune/Namihe.</summary>
internal static class FuneNamiheDefinitions
{

    /// <summary>$A8:96D7, Fune idle-left selector cursor.</summary>
    internal const ushort FuneIdleLeftCursor = 0x96d7;

    /// <summary>$A8:96DF, Namihe idle-left selector cursor.</summary>
    internal const ushort NamiheIdleLeftCursor = 0x96df;

    internal const ushort ActiveCursorDelta = 4;
    internal const ushort FacingRightCursorDelta = 2;

    /// <summary>Sound effect $1F in library two, queued when either actor spits.</summary>
    internal const ushort SpitSoundEffect = 0x001f;

    /// <summary>$A8:96D3-$A8:96E2 selects species, activity and facing.
    /// The cursor advances by two for right-facing and four for idle programs.</summary>
    internal static ushort InstructionList(ushort cursor)
    {
        int offset = cursor - (FuneIdleLeftCursor - ActiveCursorDelta);
        if ((uint)offset >= 16 || (offset & 1) != 0)
            throw new InvalidDataException(
                $"Fune/Namihe instruction selector $A8:{cursor:X4} is not authored.");
        bool namihe = offset >= 8;
        bool idle = (offset & ActiveCursorDelta) != 0;
        bool right = (offset & FacingRightCursorDelta) != 0;
        return (namihe, idle, right) switch
        {
            (false, false, false) => FuneNamiheInstructionProgramDefinitions.FuneActiveLeft,
            (false, false, true) => FuneNamiheInstructionProgramDefinitions.FuneActiveRight,
            (false, true, false) => FuneNamiheInstructionProgramDefinitions.FuneIdleLeft,
            (false, true, true) => FuneNamiheInstructionProgramDefinitions.FuneIdleRight,
            (true, false, false) => FuneNamiheInstructionProgramDefinitions.NamiheActiveLeft,
            (true, false, true) => FuneNamiheInstructionProgramDefinitions.NamiheActiveRight,
            (true, true, false) => FuneNamiheInstructionProgramDefinitions.NamiheIdleLeft,
            (true, true, true) => FuneNamiheInstructionProgramDefinitions.NamiheIdleRight,
        };
    }
}
