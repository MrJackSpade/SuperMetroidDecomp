using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.SpriteObjects;

/// <summary>
/// Verification's native view of <see cref="RoomSpriteObjectKind"/> (#1481): the object number the cartridge passes to
/// its sprite-object spawn routine, an index into its object table. Core's values are arbitrary.
/// </summary>
internal static class RoomSpriteObjectNativeWords
{
    /// <summary>The object number naming each spawned <see cref="RoomSpriteObjectKind"/>.</summary>
    internal static readonly NativeWordIdentities<RoomSpriteObjectKind> Kinds = new(new Dictionary<RoomSpriteObjectKind, ushort>
    {
        [RoomSpriteObjectKind.SporeSpawnDyingExplosion] = 0x03,
        [RoomSpriteObjectKind.Smoke] = 0x0c,
        [RoomSpriteObjectKind.EnemyProjectileDud] = 0x06,
        [RoomSpriteObjectKind.NinjaPirateLandingDust] = 0x0a,
        [RoomSpriteObjectKind.BotwoonSmallExplosion] = 0x09,
        [RoomSpriteObjectKind.BotwoonLargeExplosion] = 0x1d,
        [RoomSpriteObjectKind.ShortBigDustCloud] = 0x12,
        [RoomSpriteObjectKind.DustCloud] = 0x15,
        [RoomSpriteObjectKind.DraygonBreathBubble] = 0x18,
        [RoomSpriteObjectKind.NuclearWaffleBody] = 0x2b,
        [RoomSpriteObjectKind.NuclearWaffleTurnClockwise] = 0x2c,
        [RoomSpriteObjectKind.NuclearWaffleTurnCounterClockwise] = 0x2d,
        [RoomSpriteObjectKind.NuclearWaffleTurnOverlay] = 0x2e,
        [RoomSpriteObjectKind.FallingSparkTrail] = 0x30,
        [RoomSpriteObjectKind.MetroidOuterBodyA] = 0x32,
        [RoomSpriteObjectKind.MetroidOuterBodyB] = 0x34,
        [RoomSpriteObjectKind.YappingMawRootVariantZero] = 0x38,
        [RoomSpriteObjectKind.YappingMawRootVariantOne] = 0x39,
        [RoomSpriteObjectKind.DraygonIntroEvir] = 0x3b,
        [RoomSpriteObjectKind.DraygonDeathEvirFacingRight] = 0x3c,
        [RoomSpriteObjectKind.DraygonSpiralFoam] = 0x3d,
        [RoomSpriteObjectKind.EnemyHit] = 0x37,
    });
}
