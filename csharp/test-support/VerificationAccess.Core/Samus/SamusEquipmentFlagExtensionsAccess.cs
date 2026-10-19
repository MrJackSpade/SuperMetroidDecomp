namespace SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Samus.Projectiles;

/// <summary>Verification access to equipment-flag queries production does not use.</summary>
internal static class SamusEquipmentFlagExtensionsAccess
{
    /// <summary>Tests that every requested equipment bit is present.</summary>
    internal static bool HasAll(this SamusEquipmentFlags items, SamusEquipmentFlags flags) => (items & flags) == flags;

    /// <summary>Tests that every requested beam bit is present.</summary>
    internal static bool HasAll(this SamusBeamFlags beams, SamusBeamFlags flags) => (beams & flags) == flags;

    /// <summary>The native mask-table word of a pause equipment control's bit.</summary>
    internal static ushort ToNativeWord(this PauseEquipmentBit bit) => (ushort)(bit.Beam.ToNativeWord() | bit.Item.ToNativeWord());
}
