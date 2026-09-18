namespace SuperMetroid.Core.Game;

/// <summary>
/// Composable Zoa animation selector stored in variables C/D. Native AI uses bit zero
/// for rising versus shooting and bit one for right-facing versus left-facing.
/// </summary>
[Flags]
public enum ZoaAnimationSelector : ushort
{
    None = 0,
    Rising = 1,
    FacingRight = 2,
}

/// <summary>Compiled fixed animation-program selectors for Zoa.</summary>
internal static class ZoaAnimationDefinitions
{
    /// <summary>
    /// Left-shooting, left-rising, right-shooting, and right-rising instruction lists at
    /// <c>$A3:B40D-$A3:B414</c>, indexed by <see cref="ZoaAnimationSelector"/>.
    /// </summary>
    private static readonly ushort[] InstructionLists =
    [
        0xb3c1,
        0xb3d7,
        0xb3e7,
        0xb3fd,
    ];

    /// <summary>Returns the authored list for one facing/movement combination.</summary>
    internal static ushort InstructionList(ZoaAnimationSelector selector)
    {
        int index = (int)selector;
        if ((uint)index >= InstructionLists.Length)
        {
            throw new InvalidDataException(
                $"Zoa animation selector ${index:X4} exceeds its four-entry table.");
        }

        return InstructionLists[index];
    }
}
