using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>Compiled application-owned state-selection programs for all retail rooms.</summary>
public static class RoomStateSelectionDefinitions
{
    /// <summary>The 54 room programs with conditional states; all other retail rooms finish immediately.</summary>
    private static readonly RoomStateProgram[] conditionalPrograms =
    [
        new(0x91f8, 0x9213, [new(RoomStateCondition.EventHasBeenSet, 0x0e, 0x9261), new(RoomStateCondition.PowerBombs, 0x00, 0x9247), new(RoomStateCondition.EventHasBeenSet, 0x00, 0x922d)]),
        new(0x92b3, 0x92c5, [new(RoomStateCondition.EventHasBeenSet, 0x00, 0x92df)]),
        new(0x92fd, 0x9314, [new(RoomStateCondition.EventHasBeenSet, 0x0e, 0x9348), new(RoomStateCondition.EventHasBeenSet, 0x00, 0x932e)]),
        new(0x96ba, 0x96d1, [new(RoomStateCondition.EventHasBeenSet, 0x0e, 0x9705), new(RoomStateCondition.EventHasBeenSet, 0x00, 0x96eb)]),
        new(0x975c, 0x976d, [new(RoomStateCondition.MorphBallAndMissiles, 0x00, 0x9787)]),
        new(0x97b5, 0x97c6, [new(RoomStateCondition.MorphBallAndMissiles, 0x00, 0x97e0)]),
        new(0x9804, 0x981b, [new(RoomStateCondition.EventHasBeenSet, 0x0e, 0x984f), new(RoomStateCondition.BossIsDead, 0x04, 0x9835)]),
        new(0x9879, 0x9890, [new(RoomStateCondition.EventHasBeenSet, 0x0e, 0x98c4), new(RoomStateCondition.BossIsDead, 0x04, 0x98aa)]),
        new(0x9a44, 0x9a56, [new(RoomStateCondition.EventHasBeenSet, 0x00, 0x9a70)]),
        new(0x9a90, 0x9aa2, [new(RoomStateCondition.EventHasBeenSet, 0x00, 0x9abc)]),
        new(0x9dc7, 0x9dd9, [new(RoomStateCondition.BossIsDead, 0x02, 0x9df3)]),
        new(0x9e9f, 0x9eb1, [new(RoomStateCondition.EventHasBeenSet, 0x00, 0x9ecb)]),
        new(0x9f11, 0x9f23, [new(RoomStateCondition.EventHasBeenSet, 0x00, 0x9f3d)]),
        new(0x9f64, 0x9f76, [new(RoomStateCondition.EventHasBeenSet, 0x00, 0x9f90)]),
        new(0xa521, 0xa533, [new(RoomStateCondition.BossIsDead, 0x01, 0xa54d)]),
        new(0xa59f, 0xa5b1, [new(RoomStateCondition.BossIsDead, 0x01, 0xa5cb)]),
        new(0xa98d, 0xa99f, [new(RoomStateCondition.BossIsDead, 0x02, 0xa9b9)]),
        new(0xb283, 0xb295, [new(RoomStateCondition.BossIsDead, 0x04, 0xb2af)]),
        new(0xb32e, 0xb340, [new(RoomStateCondition.BossIsDead, 0x01, 0xb35a)]),
        new(0xc98e, 0xc9a0, [new(RoomStateCondition.BossIsDead, 0x01, 0xc9ba)]),
        new(0xca08, 0xca1a, [new(RoomStateCondition.BossIsDead, 0x01, 0xca34)]),
        new(0xca52, 0xca64, [new(RoomStateCondition.BossIsDead, 0x01, 0xca7e)]),
        new(0xcaae, 0xcac0, [new(RoomStateCondition.BossIsDead, 0x01, 0xcada)]),
        new(0xcaf6, 0xcb08, [new(RoomStateCondition.BossIsDead, 0x01, 0xcb22)]),
        new(0xcb8b, 0xcb9d, [new(RoomStateCondition.BossIsDead, 0x01, 0xcbb7)]),
        new(0xcbd5, 0xcbe7, [new(RoomStateCondition.BossIsDead, 0x01, 0xcc01)]),
        new(0xcc27, 0xcc39, [new(RoomStateCondition.BossIsDead, 0x01, 0xcc53)]),
        new(0xcc6f, 0xcc81, [new(RoomStateCondition.BossIsDead, 0x01, 0xcc9b)]),
        new(0xcccb, 0xccdd, [new(RoomStateCondition.BossIsDead, 0x01, 0xccf7)]),
        new(0xcd13, 0xcd25, [new(RoomStateCondition.BossIsDead, 0x01, 0xcd3f)]),
        new(0xcd5c, 0xcd6e, [new(RoomStateCondition.BossIsDead, 0x01, 0xcd88)]),
        new(0xcda8, 0xcdba, [new(RoomStateCondition.BossIsDead, 0x01, 0xcdd4)]),
        new(0xcdf1, 0xce03, [new(RoomStateCondition.BossIsDead, 0x01, 0xce1d)]),
        new(0xce40, 0xce52, [new(RoomStateCondition.BossIsDead, 0x01, 0xce6c)]),
        new(0xce8a, 0xce9c, [new(RoomStateCondition.BossIsDead, 0x01, 0xceb6)]),
        new(0xcefb, 0xcf0d, [new(RoomStateCondition.EventHasBeenSet, 0x0b, 0xcf27)]),
        new(0xd78f, 0xd7a1, [new(RoomStateCondition.BossIsDead, 0x01, 0xd7bb)]),
        new(0xd8c5, 0xd8d7, [new(RoomStateCondition.EventHasBeenSet, 0x0d, 0xd8f1)]),
        new(0xd95e, 0xd970, [new(RoomStateCondition.BossIsDead, 0x02, 0xd98a)]),
        new(0xda60, 0xda72, [new(RoomStateCondition.BossIsDead, 0x01, 0xda8c)]),
        new(0xdae1, 0xdaf3, [new(RoomStateCondition.EventHasBeenSet, 0x10, 0xdb0d)]),
        new(0xdb31, 0xdb43, [new(RoomStateCondition.EventHasBeenSet, 0x11, 0xdb5d)]),
        new(0xdb7d, 0xdb8f, [new(RoomStateCondition.EventHasBeenSet, 0x12, 0xdba9)]),
        new(0xdbcd, 0xdbdf, [new(RoomStateCondition.EventHasBeenSet, 0x13, 0xdbf9)]),
        new(0xdc19, 0xdc2b, [new(RoomStateCondition.EventHasBeenSet, 0x14, 0xdc45)]),
        new(0xdc65, 0xdc77, [new(RoomStateCondition.EventHasBeenSet, 0x14, 0xdc91)]),
        new(0xdcb1, 0xdcc3, [new(RoomStateCondition.EventHasBeenSet, 0x14, 0xdcdd)]),
        new(0xdd58, 0xdd6e, [new(RoomStateCondition.MainAreaBossIsDead, 0x00, 0xdda2), new(RoomStateCondition.EventHasBeenSet, 0x02, 0xdd88)]),
        new(0xdf45, 0xdf57, [new(RoomStateCondition.BossIsDead, 0x01, 0xdf71)]),
        new(0xdf8d, 0xdf9f, [new(RoomStateCondition.BossIsDead, 0x01, 0xdfb9)]),
        new(0xdfd7, 0xdfe9, [new(RoomStateCondition.BossIsDead, 0x01, 0xe003)]),
        new(0xe021, 0xe033, [new(RoomStateCondition.BossIsDead, 0x01, 0xe04d)]),
        new(0xe06b, 0xe07d, [new(RoomStateCondition.BossIsDead, 0x01, 0xe097)]),
        new(0xe0b5, 0xe0c7, [new(RoomStateCondition.BossIsDead, 0x01, 0xe0e1)]),
    ];

    /// <summary>Number of non-debug room headers in the pinned retail revision.</summary>
    public const int RetailRoomCount = RoomHeaderDefinitions.RetailRoomCount;

    /// <summary>Number of rooms whose state program contains at least one condition.</summary>
    public const int ConditionalProgramCount = 54;

    static RoomStateSelectionDefinitions()
    {
        if (conditionalPrograms.Length != ConditionalProgramCount ||
            conditionalPrograms.Select(program => program.RoomPointer).Distinct().Count() !=
                ConditionalProgramCount ||
            conditionalPrograms.Any(program =>
                !RoomHeaderDefinitions.Contains(program.RoomPointer)))
        {
            throw new InvalidOperationException(
                "Compiled room-state catalog cardinality or ownership is inconsistent.");
        }
    }

    /// <summary>Selects a room-state record using compiled native condition order.</summary>
    public static ushort Select(ushort roomPointer, RoomStateSelectionContext selection)
    {
        _ = RoomHeaderDefinitions.Get(roomPointer);

        foreach (RoomStateProgram program in conditionalPrograms)
        {
            if (program.RoomPointer != roomPointer)
                continue;
            foreach (RoomStateClause clause in program.Clauses)
            {
                if (Matches(clause, selection))
                    return clause.StatePointer;
            }
            return program.DefaultStatePointer;
        }

        return unchecked((ushort)(roomPointer + RoomHeaderRomData.FixedHeaderByteCount + 2));
    }

    private static bool Matches(RoomStateClause clause, RoomStateSelectionContext selection) =>
        clause.Condition switch
        {
            RoomStateCondition.MainAreaBossIsDead =>
                selection.IsBossDead(RoomStateSelectorOperands.MainAreaBoss),
            RoomStateCondition.EventHasBeenSet => selection.IsEventSet(clause.Operand),
            RoomStateCondition.BossIsDead => selection.IsBossDead(
                BossBitMasks.FromCartridge(clause.Operand,
                    $"Compiled room-state selector for state $8F:{clause.StatePointer:X4}")),
            RoomStateCondition.MorphBallAndMissiles => selection.HasMorphBallAndMissiles,
            RoomStateCondition.PowerBombs => selection.HasPowerBombs,
            _ => throw new InvalidOperationException(
                $"Unknown compiled room-state condition {clause.Condition}."),
        };

    private sealed record RoomStateProgram(
        ushort RoomPointer,
        ushort DefaultStatePointer,
        RoomStateClause[] Clauses);

    private readonly record struct RoomStateClause(
        RoomStateCondition Condition,
        byte Operand,
        ushort StatePointer);

    private enum RoomStateCondition : byte
    {
        MainAreaBossIsDead,
        EventHasBeenSet,
        BossIsDead,
        MorphBallAndMissiles,
        PowerBombs,
    }
}
