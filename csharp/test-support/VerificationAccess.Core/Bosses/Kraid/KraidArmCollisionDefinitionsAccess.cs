
using System.Collections.Immutable;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Verification access to <see cref="KraidArmCollisionDefinitions"/> members production does not use.</summary>
internal static class KraidArmCollisionDefinitionsAccess
{
    /// <summary>A single-component frame: a two-byte count and one eight-byte hitbox.</summary>
    private const int SingleComponentFrameBytes = 2 + 8;

    /// <summary>Bank $A7, holding Kraid's arm frames and hitbox lists.</summary>
    private const byte NativeBank = 0xa7;

    /// <summary>Each hitbox list's bank-$A7 address, in <see cref="KraidArmHitboxList"/> order.</summary>
    private static readonly ushort[] HitboxListAddresses =
        [0x92d1, 0x92eb, 0x92f9, 0x9313, 0x9321, 0x933b, 0x9349, 0x9371, 0x937f, 0x9399, 0x93f7, 0x9411, 0x941f, 0x9439, 0x946f, 0x947d];

    /// <summary>The bank-$A7 address of hitbox list <paramref name="list"/>.</summary>
    internal static ushort AddressOf(KraidArmHitboxList list) => HitboxListAddresses[(int)list];

    /// <summary>The hitbox list stored at bank-$A7 address <paramref name="pointer"/>.</summary>
    internal static KraidArmHitboxList HitboxListAt(ushort pointer) =>
        Array.IndexOf(HitboxListAddresses, pointer) is var index and >= 0
            ? (KraidArmHitboxList)index
            : throw new InvalidDataException($"Kraid-arm hitbox list $A7:{pointer:X4} is not compiled.");

    /// <summary>Calculates Kraid arm collision frame roots from the native record stride.</summary>
    extension(KraidArmCollisionDefinitions)
    {
        /// <summary>The hitboxes of the list at bank-$A7 address <paramref name="pointer"/>.</summary>
        internal static ImmutableArray<KraidCollisionHitbox> HitboxesAt(ushort pointer) =>
            KraidArmCollisionDefinitions.HitboxesAt(HitboxListAt(pointer));

        /// <summary>The components of the arm frame at native address <paramref name="pointer"/>, when it is one.</summary>
        internal static bool TryGetComponents(ushort pointer, out ImmutableArray<KraidCollisionComponent> components) =>
            KraidArmCollisionDefinitions.TryGetComponents(RoomEnemySlotFrameAccess.TryFrameAt(NativeBank, pointer), out components);

        /// <summary>Calculates the ordered physical frame roots from native component-record sizes.</summary>
        /// <remarks>Independently reviewed for #1165 against bank_A7.asm and every original
        /// frame count. General and rising/sinking groups have ten five-component frames
        /// each; the final two frames have one component. No pointer roster is stored.</remarks>
        internal static ushort FramePointer(int index)
        {
            if ((uint)index >= KraidArmCollisionDefinitions.FrameCount) throw new ArgumentOutOfRangeException(nameof(index));
            return (ushort)(index < 20 ? KraidArmCollisionDefinitionsConstants.FirstGeneralFrame + KraidArmCollisionDefinitionsConstants.GeneralFrameBytes * index
                : KraidArmCollisionDefinitionsConstants.FirstSingleComponentFrame + SingleComponentFrameBytes * (index - 20));
        }
    }
}
