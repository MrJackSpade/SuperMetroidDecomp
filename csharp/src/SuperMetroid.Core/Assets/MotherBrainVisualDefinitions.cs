namespace SuperMetroid.Core.Assets;

/// <summary>
/// Ordinary bank-$A9 OAM layouts selected by Mother Brain's head hook and
/// falling-tube programs. These identities name artwork, not encounter timing.
/// </summary>
internal static class MotherBrainVisualDefinitions
{
    /// <summary>$A9, the native bank containing Mother Brain's OAM maps.</summary>
    internal const byte Bank = 0xa9;

    /// <summary>
    /// Head and neck maps $A9:A586-$A789 and $A9:AD3E-$AD6D, followed by the
    /// five falling-tube maps $A9:ADA1-$AE5D. $A9:A694 is the private neck-joint
    /// layout, selected by the draw hook rather than an instruction word.
    /// </summary>
    private static ReadOnlySpan<ushort> NativePointers =>
    [
        0xa586, 0xa5bf, 0xa5f8, 0xa62c, 0xa660, 0xa694, 0xa69b,
        0xa6d9, 0xa717, 0xa750, 0xa789, 0xad3e, 0xad6d,
        0xada1, 0xadd5, 0xae09, 0xae33, 0xae5d,
    ];

    internal static EnemySpritemapDefinition[] Frames() =>
        [.. NativePointers.ToArray().Select(pointer =>
            new EnemySpritemapDefinition(Bank, pointer, $"mother_brain_a9_{pointer:x4}"))];
}
