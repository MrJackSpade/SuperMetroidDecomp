using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Pipe Bug visual selection from flight phase and single-part OAM record geometry.</summary>
internal static class PipeBugVisualDefinitions
{
    /// <summary>Spritemaps_Zeb_0 at $B3:89B7; five left poses followed by five right poses.</summary>
    private const ushort BrinstarFrames = 0x89b7;
    /// <summary>Spritemaps_Zebbo_0 at $B3:8A6D; three shooting poses then three rising poses per facing.</summary>
    private const ushort StrongBrinstarFrames = 0x8a6d;
    /// <summary>Spritemaps_Gamet_0 at $B3:8E96; five poses per facing.</summary>
    private const ushort NorfairFrames = 0x8e96;
    /// <summary>Spritemaps_Geega_0 at $B3:92AD; straight then arcing three-pose groups per facing.</summary>
    private const ushort YellowFrames = 0x92ad;
    /// <summary>Each native Pipe Bug OAM composition has a two-byte count and one five-byte part.</summary>
    private const int FrameBytes = 2 + 5;

    internal static ushort FrameAt(ushort enemyDefinition, ushort address)
    {
        switch (enemyDefinition)
        {
            case PipeBugDefinitions.BrinstarEnemyDefinition:
                return FivePoseFrame(address, BrinstarPipeBugInstructionProgramDefinitions.NormalRisingLeft, BrinstarFrames);
            case PipeBugDefinitions.NorfairEnemyDefinition:
                return FivePoseFrame(address, NorfairPipeBugInstructionProgramDefinitions.RisingLeft, NorfairFrames);
            case PipeBugDefinitions.StrongBrinstarEnemyDefinition:
                return ThreePoseFrame(address, BrinstarPipeBugInstructionProgramDefinitions.StrongRisingLeft, StrongBrinstarFrames, risingFirst: true);
            case PipeBugDefinitions.YellowEnemyDefinition:
                return ThreePoseFrame(address, YellowPipeBugInstructionProgramDefinitions.FlyingLeft, YellowFrames, risingFirst: false);
            default:
                throw new InvalidDataException($"Enemy ${enemyDefinition:X4} has no compiled Pipe Bug visuals.");
        }
    }

    /// <summary>
    /// Normal Zeb/Gamet halves contain eight rising records, goto, six flight records, goto.
    /// Rising opens and closes all five poses; faster flight omits intermediate pose2.
    /// Native $B3:87AB-882A and $B3:8AE1-8B60 share this structure.
    /// </summary>
    private static ushort FivePoseFrame(ushort address, ushort start, ushort frames)
    {
        int relative = address - start;
        if ((uint)relative >= 128) return Invalid(address);
        int facing = relative / 64;
        int offset = relative % 64;
        bool flying = offset >= 36;
        if (flying) offset -= 36;
        int count = flying ? 6 : 8;
        int record = (offset - 2) / 4;
        if (offset < 2 || (offset - 2) % 4 != 0 || record >= count) return Invalid(address);
        int phase = Math.Min(record, count - record);
        if (flying && phase >= 2) phase++;
        return (ushort)(frames + FrameBytes * (facing * 5 + phase));
    }

    /// <summary>
    /// Strong Zebbo and Geega each use four timed records and a goto per role/facing.
    /// Each three-pose sequence opens then closes. Zebbo's native OAM groups place
    /// shooting before rising, opposite its program order; Geega keeps straight then arc.
    /// </summary>
    private static ushort ThreePoseFrame(ushort address, ushort start, ushort frames, bool risingFirst)
    {
        int relative = address - start;
        if ((uint)relative >= 80) return Invalid(address);
        int program = relative / 20;
        int offset = relative % 20;
        int record = (offset - 2) / 4;
        if (offset < 2 || (offset - 2) % 4 != 0 || record >= 4) return Invalid(address);
        int group = risingFirst ? program ^ 1 : program;
        return (ushort)(frames + FrameBytes * (group * 3 + Math.Min(record, 4 - record)));
    }

    private static ushort Invalid(ushort address) => throw new InvalidDataException(
        $"Pipe Bug visual operand $B3:{address:X4} is not compiled.");
}
