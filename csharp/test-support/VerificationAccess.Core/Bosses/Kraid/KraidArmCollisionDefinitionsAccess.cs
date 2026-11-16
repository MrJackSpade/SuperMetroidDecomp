
using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Verification access to <see cref="KraidArmCollisionDefinitions"/> members production does not use.</summary>
internal static class KraidArmCollisionDefinitionsAccess
{
    /// <summary>A single-component frame: a two-byte count and one eight-byte hitbox.</summary>
    private const int SingleComponentFrameBytes = 2 + 8;

    /// <summary>Bank $A7, holding Kraid's arm frames and hitbox lists.</summary>
    private const byte NativeBank = 0xa7;

    /// <summary>Each hitbox list's bank-$A7 address.</summary>
    private static readonly NativeWordIdentities<KraidArmHitboxList> HitboxListAddresses = new(new Dictionary<KraidArmHitboxList, ushort>
    {
        [KraidArmHitboxList.LimbRaised] = 0x92d1,
        [KraidArmHitboxList.LimbRaisedOpen] = 0x92eb,
        [KraidArmHitboxList.LimbLowering] = 0x92f9,
        [KraidArmHitboxList.LimbLevel] = 0x9313,
        [KraidArmHitboxList.LimbHanging] = 0x9321,
        [KraidArmHitboxList.LimbTucked] = 0x933b,
        [KraidArmHitboxList.LimbFolded] = 0x9349,
        [KraidArmHitboxList.ConnectorLowered] = 0x9371,
        [KraidArmHitboxList.ConnectorHanging] = 0x937f,
        [KraidArmHitboxList.ConnectorTucked] = 0x9399,
        [KraidArmHitboxList.ConnectorRaised] = 0x93f7,
        [KraidArmHitboxList.BodyBehindLoweredArm] = 0x9411,
        [KraidArmHitboxList.BodyBehindLoweringArm] = 0x941f,
        [KraidArmHitboxList.BodyBehindRaisedArm] = 0x9439,
        [KraidArmHitboxList.DroopingDiagonal] = 0x946f,
        [KraidArmHitboxList.Outstretched] = 0x947d,
    });

    /// <summary>The hitbox list stored at bank-$A7 address <paramref name="pointer"/>.</summary>
    internal static KraidArmHitboxList HitboxListAt(ushort pointer) =>
        HitboxListAddresses.Decode(pointer, "compiled Kraid-arm hitbox list");

    /// <summary>Reads Kraid arm collision by native address.</summary>
    extension(KraidArmCollision)
    {
        /// <summary>The hitboxes of the list at bank-$A7 address <paramref name="pointer"/>.</summary>
        internal static ImmutableArray<EnemyCollisionHitbox> HitboxesAt(ushort pointer) =>
            KraidArmCollision.HitboxesAt(HitboxListAt(pointer));

        /// <summary>The components of the arm frame at native address <paramref name="pointer"/>, when it is one.</summary>
        internal static bool TryGetComponents(ushort pointer, out ImmutableArray<KraidCollisionComponent> components) =>
            KraidArmCollision.TryGetComponents(RoomEnemyPlaceFrameAccess.TryFrameAt(NativeBank, pointer), out components);
    }

    /// <summary>Calculates Kraid arm collision frame roots from the native record stride.</summary>
    extension(KraidArmCollisionDefinitions)
    {

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
