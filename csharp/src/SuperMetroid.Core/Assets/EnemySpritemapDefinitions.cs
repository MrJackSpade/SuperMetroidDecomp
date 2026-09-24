using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>One named visual frame selected by an otherwise compiled enemy program.</summary>
internal readonly record struct EnemySpritemapDefinition(byte Bank, ushort Pointer, string Name);

/// <summary>
/// Stock identities for installed enemy compositions. These are visual frame selections,
/// not editable instruction timers, AI callbacks, collision or hitbox definitions.
/// </summary>
internal static class EnemySpritemapDefinitions
{
    internal const int Version = 6;
    internal const int PreviousVersion = 5;
    internal const int PreviousFrameCount = 69;
    internal const int LegacyVersion = 4;
    internal const int LegacyFrameCount = 47;
    internal const string FileName = "enemy-compositions.json";
    internal const byte BoyonBank = 0xa2;
    internal const byte SkulteraBank = 0xa3;
    internal const byte WaverBank = 0xa3;
    internal const byte BoulderBank = 0xa6;
    internal const byte AtomicBank = 0xa8;
    internal const int MaximumParts = 128;
    internal const int TileColumns = 16;
    internal const int TileRows = 32;

    private static readonly EnemySpritemapDefinition[] FrameDefinitions =
    [
        new(BoyonBank, 0x88da, "boyon_idle_0"),
        new(BoyonBank, 0x88e1, "boyon_idle_1"),
        new(BoyonBank, 0x88e8, "boyon_idle_2"),
        new(BoyonBank, 0x88ef, "boyon_bounce_0"),
        new(BoyonBank, 0x88f6, "boyon_bounce_1"),
        new(BoyonBank, 0x88fd, "boyon_bounce_2"),
        new(BoyonBank, 0x8904, "boyon_bounce_3"),
        new(BoyonBank, 0xa0bb, "cacatac_upright_idle_0"),
        new(BoyonBank, 0xa0db, "cacatac_upright_idle_1"),
        new(BoyonBank, 0xa0fb, "cacatac_upright_idle_2"),
        new(BoyonBank, 0xa11b, "cacatac_upright_idle_3"),
        new(BoyonBank, 0xa13b, "cacatac_upright_idle_4"),
        new(BoyonBank, 0xa15b, "cacatac_upright_idle_5"),
        new(BoyonBank, 0xa17b, "cacatac_upright_idle_6"),
        new(BoyonBank, 0xa19b, "cacatac_upright_idle_7"),
        new(BoyonBank, 0xa1bb, "cacatac_upright_attack_1"),
        new(BoyonBank, 0xa1ef, "cacatac_upright_attack_2"),
        new(BoyonBank, 0xa223, "cacatac_inverted_idle_0"),
        new(BoyonBank, 0xa243, "cacatac_inverted_idle_1"),
        new(BoyonBank, 0xa263, "cacatac_inverted_idle_2"),
        new(BoyonBank, 0xa283, "cacatac_inverted_idle_3"),
        new(BoyonBank, 0xa2a3, "cacatac_inverted_idle_4"),
        new(BoyonBank, 0xa2c3, "cacatac_inverted_idle_5"),
        new(BoyonBank, 0xa2e3, "cacatac_inverted_idle_6"),
        new(BoyonBank, 0xa303, "cacatac_inverted_idle_7"),
        new(BoyonBank, 0xa323, "cacatac_inverted_attack_1"),
        new(BoyonBank, 0xa357, "cacatac_inverted_attack_2"),
        new(BoulderBank, 0x8a59, "boulder_roll_0"),
        new(BoulderBank, 0x8a6f, "boulder_roll_1"),
        new(BoulderBank, 0x8a85, "boulder_roll_2"),
        new(BoulderBank, 0x8a9b, "boulder_roll_3"),
        new(BoulderBank, 0x8ab1, "boulder_roll_4"),
        new(BoulderBank, 0x8ac7, "boulder_roll_5"),
        new(BoulderBank, 0x8add, "boulder_roll_6"),
        new(BoulderBank, 0x8af3, "boulder_roll_7"),
        new(AtomicBank, 0xe489, "atomic_up_right_0"),
        new(AtomicBank, 0xe49f, "atomic_up_right_1"),
        new(AtomicBank, 0xe4b5, "atomic_up_right_2"),
        new(AtomicBank, 0xe4cb, "atomic_up_right_3"),
        new(AtomicBank, 0xe4e1, "atomic_up_right_4"),
        new(AtomicBank, 0xe4f2, "atomic_up_right_5"),
        new(AtomicBank, 0xe508, "atomic_up_left_0"),
        new(AtomicBank, 0xe51e, "atomic_up_left_1"),
        new(AtomicBank, 0xe534, "atomic_up_left_2"),
        new(AtomicBank, 0xe54a, "atomic_up_left_3"),
        new(AtomicBank, 0xe560, "atomic_up_left_4"),
        new(AtomicBank, 0xe571, "atomic_up_left_5"),
        new(SkulteraBank, 0x928a, "skultera_swim_left_0"),
        new(SkulteraBank, 0x92a5, "skultera_swim_left_1"),
        new(SkulteraBank, 0x92c0, "skultera_swim_left_2"),
        new(SkulteraBank, 0x92db, "skultera_turn_right_0"),
        new(SkulteraBank, 0x92f6, "skultera_turn_right_1"),
        new(SkulteraBank, 0x9311, "skultera_turn_right_2"),
        new(SkulteraBank, 0x9327, "skultera_turn_right_3"),
        new(SkulteraBank, 0x933d, "skultera_turn_right_4"),
        new(SkulteraBank, 0x934e, "skultera_turn_right_5"),
        new(SkulteraBank, 0x9364, "skultera_turn_right_6"),
        new(SkulteraBank, 0x937f, "skultera_turn_right_7"),
        new(SkulteraBank, 0x939a, "skultera_swim_right_0"),
        new(SkulteraBank, 0x93b5, "skultera_swim_right_1"),
        new(SkulteraBank, 0x93d0, "skultera_swim_right_2"),
        new(SkulteraBank, 0x93eb, "skultera_turn_left_0"),
        new(SkulteraBank, 0x9406, "skultera_turn_left_1"),
        new(SkulteraBank, 0x9421, "skultera_turn_left_2"),
        new(SkulteraBank, 0x9437, "skultera_turn_left_3"),
        new(SkulteraBank, 0x944d, "skultera_turn_left_4"),
        new(SkulteraBank, 0x945e, "skultera_turn_left_5"),
        new(SkulteraBank, 0x9474, "skultera_turn_left_6"),
        new(SkulteraBank, 0x948f, "skultera_turn_left_7"),
        new(WaverBank, 0x884a, "waver_steady_left"),
        new(WaverBank, 0x88b3, "waver_steady_right"),
        new(WaverBank, 0x885b, "waver_spin_left_0"),
        new(WaverBank, 0x8871, "waver_spin_left_1"),
        new(WaverBank, 0x881e, "waver_spin_left_2"),
        new(WaverBank, 0x8834, "waver_spin_left_3"),
        new(WaverBank, 0x88c4, "waver_spin_right_0"),
        new(WaverBank, 0x88da, "waver_spin_right_1"),
        new(WaverBank, 0x8887, "waver_spin_right_2"),
        new(WaverBank, 0x889d, "waver_spin_right_3"),
    ];

    private static readonly ushort[] AtomicUpRightFrames =
        [0xe489, 0xe49f, 0xe4b5, 0xe4cb, 0xe4e1, 0xe4f2];
    private static readonly ushort[] AtomicUpLeftFrames =
        [0xe508, 0xe51e, 0xe534, 0xe54a, 0xe560, 0xe571];

    internal static ReadOnlySpan<EnemySpritemapDefinition> Frames => FrameDefinitions;

    /// <summary>
    /// Selects only families whose fixed instruction visual operands are compiled.
    /// Unknown families retain the existing cartridge route until separately migrated.
    /// Known families reject an unlisted operand rather than reading adjacent data.
    /// </summary>
    internal static bool TryFrameAt(ushort enemyDefinition, ushort operandAddress,
        out ushort frame)
    {
        frame = enemyDefinition switch
        {
            RoomEnemySystem.BoyonDefinition => BoyonFrameAt(operandAddress),
            RoomEnemySystem.CacatacDefinition => CacatacFrameAt(operandAddress),
            RoomEnemySystem.BoulderDefinition => BoulderFrameAt(operandAddress),
            RoomEnemySystem.AtomicDefinition => AtomicFrameAt(operandAddress),
            RoomEnemySystem.SkulteraDefinition => SkulteraFrameAt(operandAddress),
            RoomEnemySystem.WaverDefinition => WaverFrameAt(operandAddress),
            _ => 0,
        };
        return enemyDefinition is RoomEnemySystem.BoyonDefinition or
            RoomEnemySystem.CacatacDefinition or RoomEnemySystem.BoulderDefinition or
            RoomEnemySystem.AtomicDefinition or RoomEnemySystem.SkulteraDefinition or
            RoomEnemySystem.WaverDefinition;
    }

    /// <summary>
    /// Waver's two steady and eight spinning frames from the interleaved visual
    /// operands at $A3:86A9-$86D5. Spin completion and delays remain mechanics.
    /// </summary>
    internal static ushort WaverFrameAt(ushort operandAddress) => operandAddress switch
    {
        0x86a9 => 0x884a,
        0x86af => 0x88b3,
        0x86b5 => 0x885b,
        0x86b9 => 0x8871,
        0x86bd => 0x881e,
        0x86c1 => 0x8834,
        0x86c9 => 0x88c4,
        0x86cd => 0x88da,
        0x86d1 => 0x8887,
        0x86d5 => 0x889d,
        _ => throw new InvalidDataException(
            $"Waver visual operand $A3:{operandAddress:X4} is not compiled."),
    };

    /// <summary>
    /// The twenty-two Skultera frame operands are fixed visual identities; control
    /// durations, turn callbacks, and layer changes remain in the instruction catalog.
    /// </summary>
    internal static ushort SkulteraFrameAt(ushort operandAddress)
    {
        if (SkulteraInstructionProgramDefinitions.IsPresentationWord(operandAddress) &&
            CompiledEnemyVisualSelectors.TryGet(SkulteraBank, operandAddress,
                out ushort frame))
            return frame;
        throw new InvalidDataException(
            $"Skultera visual operand $A3:{operandAddress:X4} is not compiled.");
    }

    /// <summary>
    /// The ten fixed pointer operands interleaved with Boyon's compiled idle and bounce
    /// instructions. Repeated frames retain the cartridge's exact visual sequence.
    /// </summary>
    internal static ushort BoyonFrameAt(ushort operandAddress) => operandAddress switch
    {
        0x86ad => 0x88da,
        0x86b1 => 0x88e1,
        0x86b5 => 0x88e8,
        0x86b9 => 0x88e1,
        0x86c5 => 0x88ef,
        0x86c9 => 0x88f6,
        0x86cd => 0x88fd,
        0x86d1 => 0x8904,
        0x86d5 => 0x88fd,
        0x86d9 => 0x88f6,
        _ => throw new InvalidDataException(
            $"Boyon visual operand $A2:{operandAddress:X4} is not compiled."),
    };

    /// <summary>
    /// Four native Cacatac programs: eight idle frames and four attack selectors
    /// per orientation. Attack poses zero and three reuse idle zero and attack one.
    /// The lookup cannot alter attack timings or spike-spawn callbacks.
    /// </summary>
    internal static ushort CacatacFrameAt(ushort operandAddress)
    {
        if (operandAddress >= 0x9e8e && operandAddress <= 0x9eaa &&
            (operandAddress - 0x9e8e) % 4 == 0)
            return unchecked((ushort)(0xa0bb + (operandAddress - 0x9e8e) * 8));
        if (operandAddress >= 0x9ede && operandAddress <= 0x9efa &&
            (operandAddress - 0x9ede) % 4 == 0)
            return unchecked((ushort)(0xa223 + (operandAddress - 0x9ede) * 8));
        return operandAddress switch
        {
            0x9eb2 => 0xa0bb,
            0x9eb6 or 0x9ebe => 0xa1bb,
            0x9eba => 0xa1ef,
            0x9f02 => 0xa223,
            0x9f06 or 0x9f0e => 0xa323,
            0x9f0a => 0xa357,
            _ => throw new InvalidDataException(
                $"Cacatac visual operand $A2:{operandAddress:X4} is not compiled."),
        };
    }

    /// <summary>
    /// Boulder rolls through eight four-part frames. Its right-moving list plays the
    /// same eight frames in reverse after the first frame; direction and duration
    /// remain gameplay-owned.
    /// </summary>
    internal static ushort BoulderFrameAt(ushort operandAddress)
    {
        if (operandAddress >= 0x86a9 && operandAddress <= 0x86c5 &&
            (operandAddress - 0x86a9) % 4 == 0)
            return unchecked((ushort)(0x8a59 + (operandAddress - 0x86a9) / 4 * 0x16));
        if (operandAddress >= 0x86cd && operandAddress <= 0x86e9 &&
            (operandAddress - 0x86cd) % 4 == 0)
        {
            int index = (operandAddress - 0x86cd) / 4;
            return unchecked((ushort)(0x8a59 + (index == 0 ? 0 : 8 - index) * 0x16));
        }
        throw new InvalidDataException(
            $"Boulder visual operand $A6:{operandAddress:X4} is not compiled.");
    }

    /// <summary>
    /// Atomic has two authored six-frame spirals; the down-left/down-right programs
    /// replay the matching up spiral in reverse. Directional movement remains compiled.
    /// </summary>
    internal static ushort AtomicFrameAt(ushort operandAddress)
    {
        if (operandAddress >= 0xe312 && operandAddress <= 0xe326 &&
            (operandAddress - 0xe312) % 4 == 0)
            return AtomicUpRightFrames[(operandAddress - 0xe312) / 4];
        if (operandAddress >= 0xe32e && operandAddress <= 0xe342 &&
            (operandAddress - 0xe32e) % 4 == 0)
            return AtomicUpLeftFrames[(operandAddress - 0xe32e) / 4];
        if (operandAddress >= 0xe34a && operandAddress <= 0xe35e &&
            (operandAddress - 0xe34a) % 4 == 0)
            return AtomicUpRightFrames[5 - (operandAddress - 0xe34a) / 4];
        if (operandAddress >= 0xe366 && operandAddress <= 0xe37a &&
            (operandAddress - 0xe366) % 4 == 0)
            return AtomicUpLeftFrames[5 - (operandAddress - 0xe366) / 4];
        throw new InvalidDataException(
            $"Atomic visual operand $A8:{operandAddress:X4} is not compiled.");
    }
}
