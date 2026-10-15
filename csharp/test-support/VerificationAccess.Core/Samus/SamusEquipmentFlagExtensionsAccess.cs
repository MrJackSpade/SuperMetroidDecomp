namespace SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;

/// <summary>Verification access to <see cref="SamusEquipmentFlagExtensions"/> members production does not use.</summary>
internal static class SamusEquipmentFlagExtensionsAccess
{
    /// <summary>Tests that every requested equipment bit is present in the native word.</summary>
    internal static bool HasAll(this ushort word, SamusEquipmentFlags flags) =>
        ((SamusEquipmentFlags)word & flags) == flags;

    /// <summary>Tests that every requested beam bit is present in the native word.</summary>
    internal static bool HasAll(this ushort word, SamusBeamFlags flags) =>
        ((SamusBeamFlags)word & flags) == flags;

    /// <summary>
    /// Returns a native equipment word with the requested, independently combinable bits
    /// enabled. Keeping this operation here prevents call sites from reintroducing casts and
    /// hexadecimal masks merely because the WRAM-facing property remains a <see cref="ushort"/>.
    /// </summary>
    internal static ushort With(this ushort word, SamusEquipmentFlags flags) =>
        (ushort)(word | (ushort)flags);

    /// <summary>Returns a native equipment word with the requested bits disabled.</summary>
    internal static ushort Without(this ushort word, SamusEquipmentFlags flags) =>
        (ushort)(word & ~(ushort)flags);

    /// <summary>Returns a native beam word with the requested equipment bits enabled.</summary>
    internal static ushort With(this ushort word, SamusBeamFlags flags) =>
        (ushort)(word | (ushort)flags);

    /// <summary>Returns a native beam word with the requested equipment bits disabled.</summary>
    internal static ushort Without(this ushort word, SamusBeamFlags flags) =>
        (ushort)(word & ~(ushort)flags);

    /// <summary>Preserves the selected equipment bits as a WRAM-compatible unsigned word.</summary>
    internal static ushort ToNativeWord(this SamusEquipmentFlags flags) => (ushort)flags;
}
