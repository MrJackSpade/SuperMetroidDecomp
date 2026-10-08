namespace SuperMetroid.Core.Assets;

/// <summary>Kraid's independently animated extended foot compositions, not his BG2 body.</summary>
internal static class KraidFootVisualDefinitions
{
    /// <summary>Native Kraid instruction and artwork bank $A7.</summary>
    internal const byte Bank = 0xa7;
    /// <summary>Thirty-five selected roots: initial $A565 and walking/lunging/backwards $8CE3..8F47.</summary>
    internal const int FrameCount = 35;

    /// <summary>$A7:8CE3, ExtendedSpritemap_KraidFoot_0; each root has two components.</summary>
    private const ushort MovingStart = 0x8ce3;
    /// <summary>$A7:A565, ExtendedSpritemap_KraidFoot_Initial, sorted after moving roots.</summary>
    private const ushort InitialFrame = 0xa565;
    internal static KraidFootFrameSequence Frames => new(FrameCount);

    /// <summary>Selected moving roots use eighteen-byte records (two count bytes
    /// plus two eight-byte components). Native ordinal $21 is never selected by
    /// the compiled programs; skip it and append the separate initial root.</summary>
    internal static EnemyExtendedFrameDefinition Frame(int index)
    {
        if ((uint)index >= FrameCount) throw new IndexOutOfRangeException();
        ushort pointer = index == FrameCount - 1 ? InitialFrame
            : (ushort)(MovingStart + 18 * (index < 0x21 ? index : index + 1));
        return new(Bank, pointer, $"kraid_foot_oam_{pointer:X4}");
    }
}

/// <summary>Calculated foot-frame enumeration without a stored definition lookup.</summary>
internal readonly record struct KraidFootFrameSequence(int Length)
{
    internal EnemyExtendedFrameDefinition this[int index] => KraidFootVisualDefinitions.Frame(index);
    internal EnemyExtendedFrameDefinition[] ToArray()
    {
        var result = new EnemyExtendedFrameDefinition[Length];
        for (int index = 0; index < result.Length; index++) result[index] = this[index];
        return result;
    }
}
