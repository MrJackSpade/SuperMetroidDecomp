namespace SuperMetroid.Core.Game;

/// <summary>Compiled cartridge identities and instruction selectors for Fune/Namihe.</summary>
internal static class FuneNamiheDefinitions
{
    /// <summary>Enemy definition $E6FF (Fune) in bank $A0.</summary>
    internal const ushort FuneEnemyDefinition = 0xe6ff;

    /// <summary>Enemy definition $E73F (Namihe) in bank $A0.</summary>
    internal const ushort NamiheEnemyDefinition = 0xe73f;

    /// <summary>$A8:96D7, Fune idle-left selector cursor.</summary>
    internal const ushort FuneIdleLeftCursor = 0x96d7;

    /// <summary>$A8:96DF, Namihe idle-left selector cursor.</summary>
    internal const ushort NamiheIdleLeftCursor = 0x96df;

    internal const ushort ActiveCursorDelta = 4;
    internal const ushort FacingRightCursorDelta = 2;

    /// <summary>Sound effect $1F in library two, queued when either actor spits.</summary>
    internal const ushort SpitSoundEffect = 0x001f;

    /// <summary>$A8:96D3-$A8:96E2, active/idle and left/right instruction lists.</summary>
    private static ReadOnlySpan<ushort> InstructionLists =>
    [
        FuneNamiheInstructionProgramDefinitions.FuneActiveLeft,
        FuneNamiheInstructionProgramDefinitions.FuneActiveRight,
        FuneNamiheInstructionProgramDefinitions.FuneIdleLeft,
        FuneNamiheInstructionProgramDefinitions.FuneIdleRight,
        FuneNamiheInstructionProgramDefinitions.NamiheActiveLeft,
        FuneNamiheInstructionProgramDefinitions.NamiheActiveRight,
        FuneNamiheInstructionProgramDefinitions.NamiheIdleLeft,
        FuneNamiheInstructionProgramDefinitions.NamiheIdleRight,
    ];

    internal static ushort InstructionList(ushort cursor)
    {
        ushort offset = unchecked((ushort)(cursor - 0x96d3));
        if ((offset & 1) != 0 || offset >= InstructionLists.Length * 2)
        {
            throw new InvalidDataException(
                $"Fune/Namihe instruction selector $A8:{cursor:X4} is not authored.");
        }
        return InstructionLists[offset >> 1];
    }
}
