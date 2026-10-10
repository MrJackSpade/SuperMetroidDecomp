using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// The fifteen distinct bank-$AA OAM compositions selected by Shaktool's seven
/// articulated enemy records. The native instruction programs own timing and
/// segment motion; these definitions only identify editable visual frames.
/// </summary>
internal static class ShaktoolVisualDefinitions
{
    /// <summary>Native Shaktool spritemap bank $AA.</summary>
    internal const byte Bank = 0xaa;

    /// <summary>$AA:DF5C: first of three single-part final-saw poses, immediately followed by the single-part arm at $DF71.</summary>
    private const ushort FirstFinalSaw = 0xdf5c;
    /// <summary>$AA:DF5C-DF71 and E028-E036: two-byte count plus one five-byte OAM part.</summary>
    private const int SinglePartRecordBytes = sizeof(ushort) + 5;
    /// <summary>$AA:DF78-E012: each of the eight compass-oriented head records declares four five-byte parts.</summary>
    private const int HeadRecordBytes = sizeof(ushort) + 4 * 5;
    /// <summary>$AA:DF78: head group immediately follows the three final-saw records and the arm record.</summary>
    private const ushort FirstHead = FirstFinalSaw + 4 * SinglePartRecordBytes;
    /// <summary>$AA:E028: first primary-saw record immediately follows the eight head directions.</summary>
    private const ushort FirstPrimarySaw = FirstHead + 8 * HeadRecordBytes;

    /// <summary>Enumerates the fifteen editable spritemap frames in native record order: final saw, arm, head directions, then primary saw.</summary>
    /// <returns>Named frame definitions for Shaktool's three saw poses, arm, eight head orientations, and three primary-saw poses.</returns>
    internal static IEnumerable<EnemySpritemapDefinition> Frames()
    {
        for (int pose = 0; pose < 3; pose++)
            yield return new(Bank, (ushort)(FirstFinalSaw + pose * SinglePartRecordBytes),
                $"shaktool_final_saw_{pose.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        yield return new(Bank, FirstFinalSaw + 3 * SinglePartRecordBytes, "shaktool_arm");
        for (int direction = 0; direction < 8; direction++)
            yield return new(Bank, (ushort)(FirstHead + direction * HeadRecordBytes),
                "shaktool_head_" + HeadDirectionName(direction));
        for (int pose = 0; pose < 3; pose++)
            yield return new(Bank, (ushort)(FirstPrimarySaw + pose * SinglePartRecordBytes),
                $"shaktool_primary_saw_{pose.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
    }

    /// <summary>$AA:DF78-E012: native head drawings progress left/up-left/up/up-right/right/down-right/down/down-left; names preserve the installed schema identities.</summary>
    private static string HeadDirectionName(int direction) => direction switch
    {
        0 => "left",
        1 => "up_left",
        2 => "up",
        3 => "up_right",
        4 => "right",
        5 => "down_right",
        6 => "down",
        7 => "down_left",
        _ => throw new ArgumentOutOfRangeException(nameof(direction)),
    };
    /// <summary>Resolves only presentation words in Shaktool's bank-$AA programs.</summary>
    internal static ushort FrameAt(ushort operandAddress)
    {
        if (ShaktoolInstructionProgramDefinitions.IsPresentationWord(operandAddress) &&
            CompiledEnemyVisualSelectors.TryGet(Bank, operandAddress, out ushort frame))
            return frame;
        throw new InvalidDataException(
            $"Shaktool visual operand $AA:{operandAddress:X4} is not compiled.");
    }
}
