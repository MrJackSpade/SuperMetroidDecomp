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
        [RoomEffectActorKind.SmallExplosion] = 0x03,
        [RoomEffectActorKind.Smoke] = 0x0c,
        [RoomEffectActorKind.DudShot] = 0x06,
        [RoomEffectActorKind.RisingDustPuff] = 0x0a,
        [RoomEffectActorKind.SmokePuff] = 0x09,
        [RoomEffectActorKind.BigExplosion] = 0x1d,
        [RoomEffectActorKind.ShortBigDustCloud] = 0x12,
        [RoomEffectActorKind.BigDustCloud] = 0x15,
        [RoomEffectActorKind.DraygonBreathBubbles] = 0x18,
        [RoomEffectActorKind.NuclearWaffleBody] = 0x2b,
        [RoomEffectActorKind.NuclearWaffleRightExplosion] = 0x2c,
        [RoomEffectActorKind.NuclearWaffleLeftExplosion] = 0x2d,
        [RoomEffectActorKind.NuclearWaffleSplash] = 0x2e,
        [RoomEffectActorKind.FallingSparkTrail] = 0x30,
        [RoomEffectActorKind.MetroidElectricity] = 0x32,
        [RoomEffectActorKind.MetroidShell] = 0x34,
        [RoomEffectActorKind.YappingMawBaseFacingDown] = 0x38,
        [RoomEffectActorKind.YappingMawBaseFacingUp] = 0x39,
        [RoomEffectActorKind.EvirFacingLeft] = 0x3b,
        [RoomEffectActorKind.EvirFacingRight] = 0x3c,
        [RoomEffectActorKind.DraygonFoam] = 0x3d,
        [RoomEffectActorKind.HitBurst] = 0x37,
    });
}
