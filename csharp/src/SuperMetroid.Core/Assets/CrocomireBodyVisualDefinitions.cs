using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// The fifty distinct fight-body extended-frame roots selected by Crocomire's
/// compiled bank-$A4 instruction operands. Forty-two have both ordinary OAM
/// components and BG2 streams; the final eight death-transition roots have
/// only ordinary OAM components. Skeleton poses use a separate later range.
/// </summary>
internal static class CrocomireBodyVisualDefinitions
{
    /// <summary>Native Crocomire body visual bank $A4.</summary>
    internal const byte Bank = 0xa4;
    /// <summary>First pure-OAM transition root at $A4:CA7E.</summary>
    internal const ushort FirstPureOamFrame = 0xca7e;
    /// <summary>First skeleton extended frame at $A4:E1FE.</summary>
    internal const ushort FirstSkeletonFrame = 0xe1fe;
    internal const int BodyFrameCount = 50;
    internal const int MixedBg2FrameCount = 42;

    private static readonly ushort[] BodyPointers = BuildBodyPointers();

    internal static ReadOnlySpan<ushort> Frames => BodyPointers;

    internal static bool HasBg2(ushort pointer) =>
        pointer < FirstPureOamFrame && Array.BinarySearch(BodyPointers, pointer) >= 0;

    private static ushort[] BuildBodyPointers()
    {
        var pointers = new SortedSet<ushort>();
        for (int index = 0;
             index < CrocomireInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = CrocomireInstructionProgramDefinitions
                .PresentationWordAddress(index);
            if (!CompiledEnemyVisualSelectors.TryGet(Bank, operand, out ushort pointer))
                throw new InvalidDataException(
                    $"Crocomire visual selector $A4:{operand:X4} is not compiled.");
            if (pointer < FirstSkeletonFrame)
                pointers.Add(pointer);
        }
        ushort[] selected = [.. pointers];
        if (selected.Length != BodyFrameCount ||
            selected.Count(pointer => pointer < FirstPureOamFrame) != MixedBg2FrameCount)
            throw new InvalidDataException(
                "Crocomire body visual selector inventory changed.");
        return selected;
    }
}
