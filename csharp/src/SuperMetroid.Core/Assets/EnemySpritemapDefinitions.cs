namespace SuperMetroid.Core.Assets;

/// <summary>One named visual frame selected by an otherwise compiled enemy program.</summary>
internal readonly record struct EnemySpritemapDefinition(byte Bank, ushort Pointer, string Name);

/// <summary>
/// Stock identities for installed enemy compositions. These are visual frame selections,
/// not editable instruction timers, AI callbacks, collision or hitbox definitions.
/// </summary>
internal static class EnemySpritemapDefinitions
{
    internal const int Version = 3;
    internal const string FileName = "enemy-compositions.json";
    internal const byte BoyonBank = 0xa2;
    internal const byte BoulderBank = 0xa6;
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
    ];

    internal static ReadOnlySpan<EnemySpritemapDefinition> Frames => FrameDefinitions;

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
}
