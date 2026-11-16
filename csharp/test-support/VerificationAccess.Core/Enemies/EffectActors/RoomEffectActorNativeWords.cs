using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.EffectActors;

/// <summary>
/// Verification's native view of <see cref="RoomEffectActorKind"/> (#1481): the object number the cartridge passes to
/// its sprite-object spawn routine, an index into its object table. Core's values are arbitrary.
/// </summary>
internal static class RoomEffectActorNativeWords
{
    /// <summary>The object number naming each spawned <see cref="RoomEffectActorKind"/>.</summary>
    internal static readonly NativeWordIdentities<RoomEffectActorKind> Kinds = new(new Dictionary<RoomEffectActorKind, ushort>
    {
        [RoomEffectActorKind.SporeSpawnDyingExplosion] = 0x03,
        [RoomEffectActorKind.Smoke] = 0x0c,
        [RoomEffectActorKind.EnemyProjectileDud] = 0x06,
        [RoomEffectActorKind.NinjaPirateLandingDust] = 0x0a,
        [RoomEffectActorKind.BotwoonSmallExplosion] = 0x09,
        [RoomEffectActorKind.BotwoonLargeExplosion] = 0x1d,
        [RoomEffectActorKind.ShortBigDustCloud] = 0x12,
        [RoomEffectActorKind.DustCloud] = 0x15,
        [RoomEffectActorKind.DraygonBreathBubble] = 0x18,
        [RoomEffectActorKind.NuclearWaffleBody] = 0x2b,
        [RoomEffectActorKind.NuclearWaffleTurnClockwise] = 0x2c,
        [RoomEffectActorKind.NuclearWaffleTurnCounterClockwise] = 0x2d,
        [RoomEffectActorKind.NuclearWaffleTurnOverlay] = 0x2e,
        [RoomEffectActorKind.FallingSparkTrail] = 0x30,
        [RoomEffectActorKind.MetroidElectricity] = 0x32,
        [RoomEffectActorKind.MetroidShell] = 0x34,
        [RoomEffectActorKind.YappingMawRootFacingDown] = 0x38,
        [RoomEffectActorKind.YappingMawRootFacingUp] = 0x39,
        [RoomEffectActorKind.DraygonIntroEvir] = 0x3b,
        [RoomEffectActorKind.DraygonDeathEvirFacingRight] = 0x3c,
        [RoomEffectActorKind.DraygonSpiralFoam] = 0x3d,
        [RoomEffectActorKind.EnemyHit] = 0x37,
    });
}
