namespace SuperMetroid.Core.Game;

/// <summary>Compiled ceiling-break projectile placement data.</summary>
public static class KraidCeilingRockPositions
{
    /// <summary>
    /// $A7:ACB3 nine X words, followed by ACC5's AD03 callback pointer.
    /// Native authored selectors are even byte offsets. Retaining the following
    /// word also preserves the existing reader's overlapping odd-offset result;
    /// this does not define native execution of a malformed callback pointer.
    /// </summary>
    public static ushort AtByteOffset(int offset)
    {
        ReadOnlySpan<ushort> words = [0x68, 0xd8, 0x28, 0xa8, 0x58, 0xc8, 0x38, 0xb8, 0x48, 0xad03];
        int index = offset >> 1;
        return (offset & 1) == 0 ? words[index]
            : unchecked((ushort)((words[index] >> 8) | (words[index + 1] << 8)));
    }
}
