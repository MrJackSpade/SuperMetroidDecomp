namespace SuperMetroid.Core.Enemies.EffectActors;

/// <summary>The bank-$B4 address of each room sprite-object program's native instruction list.</summary>
internal static class RoomEffectActorProgramAddresses
{
    /// <summary>
    /// Each kind's list address. A Metroid's electricity and shell lists run straight on into the records they loop over,
    /// so those records follow the first pass's.
    /// </summary>
    internal static IReadOnlyDictionary<RoomEffectActorKind, ushort> Entries { get; } = new Dictionary<RoomEffectActorKind, ushort>
    {
        [RoomEffectActorKind.DraygonFoam] = 0xbe24,
        [RoomEffectActorKind.SmallExplosion] = 0xbea4,
        [RoomEffectActorKind.DudShot] = 0xbeea,
        [RoomEffectActorKind.SmokePuff] = 0xbf1c,
        [RoomEffectActorKind.RisingDustPuff] = 0xbf32,
        [RoomEffectActorKind.Smoke] = 0xbf56,
        [RoomEffectActorKind.BigExplosion] = 0xbf74,
        [RoomEffectActorKind.ShortBigDustCloud] = 0xc014,
        [RoomEffectActorKind.BigDustCloud] = 0xc05e,
        [RoomEffectActorKind.DraygonBreathBubbles] = 0xc10c,
        [RoomEffectActorKind.NuclearWaffleBody] = 0xc30a,
        [RoomEffectActorKind.NuclearWaffleRightExplosion] = 0xc33e,
        [RoomEffectActorKind.NuclearWaffleLeftExplosion] = 0xc35c,
        [RoomEffectActorKind.NuclearWaffleSplash] = 0xc37a,
        [RoomEffectActorKind.FallingSparkTrail] = 0xc390,
        [RoomEffectActorKind.MetroidElectricity] = 0xc3ba,
        [RoomEffectActorKind.MetroidShell] = 0xc4b6,
        [RoomEffectActorKind.YappingMawBaseFacingDown] = 0xc5d8,
        [RoomEffectActorKind.YappingMawBaseFacingUp] = 0xc5de,
        [RoomEffectActorKind.EvirFacingLeft] = 0xc608,
        [RoomEffectActorKind.EvirFacingRight] = 0xc61c,
        [RoomEffectActorKind.HitBurst] = 0xc5c6,
    };
}
