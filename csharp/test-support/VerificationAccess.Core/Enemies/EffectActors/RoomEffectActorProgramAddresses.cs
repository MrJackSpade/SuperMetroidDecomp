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

    /// <summary>
    /// Each segment's first record (#2276): a kind's own at its list's address, and the cycle a Metroid's electricity and shell
    /// loop at the record their opening runs on into.
    /// </summary>
    internal static IReadOnlyDictionary<RoomEffectActorSegment, ushort> SegmentEntries { get; } = new Dictionary<RoomEffectActorSegment, ushort>
    {
        [RoomEffectActorSegment.DraygonFoam] = Entries[RoomEffectActorKind.DraygonFoam],
        [RoomEffectActorSegment.SmallExplosion] = Entries[RoomEffectActorKind.SmallExplosion],
        [RoomEffectActorSegment.DudShot] = Entries[RoomEffectActorKind.DudShot],
        [RoomEffectActorSegment.SmokePuff] = Entries[RoomEffectActorKind.SmokePuff],
        [RoomEffectActorSegment.RisingDustPuff] = Entries[RoomEffectActorKind.RisingDustPuff],
        [RoomEffectActorSegment.Smoke] = Entries[RoomEffectActorKind.Smoke],
        [RoomEffectActorSegment.BigExplosion] = Entries[RoomEffectActorKind.BigExplosion],
        [RoomEffectActorSegment.ShortBigDustCloud] = Entries[RoomEffectActorKind.ShortBigDustCloud],
        [RoomEffectActorSegment.BigDustCloud] = Entries[RoomEffectActorKind.BigDustCloud],
        [RoomEffectActorSegment.DraygonBreathBubbles] = Entries[RoomEffectActorKind.DraygonBreathBubbles],
        [RoomEffectActorSegment.NuclearWaffleBody] = Entries[RoomEffectActorKind.NuclearWaffleBody],
        [RoomEffectActorSegment.NuclearWaffleRightExplosion] = Entries[RoomEffectActorKind.NuclearWaffleRightExplosion],
        [RoomEffectActorSegment.NuclearWaffleLeftExplosion] = Entries[RoomEffectActorKind.NuclearWaffleLeftExplosion],
        [RoomEffectActorSegment.NuclearWaffleSplash] = Entries[RoomEffectActorKind.NuclearWaffleSplash],
        [RoomEffectActorSegment.FallingSparkTrail] = Entries[RoomEffectActorKind.FallingSparkTrail],
        [RoomEffectActorSegment.MetroidElectricity] = Entries[RoomEffectActorKind.MetroidElectricity],
        [RoomEffectActorSegment.MetroidElectricityCycle] = 0xc436,
        [RoomEffectActorSegment.MetroidShell] = Entries[RoomEffectActorKind.MetroidShell],
        [RoomEffectActorSegment.MetroidShellCycle] = 0xc536,
        [RoomEffectActorSegment.YappingMawBaseFacingDown] = Entries[RoomEffectActorKind.YappingMawBaseFacingDown],
        [RoomEffectActorSegment.YappingMawBaseFacingUp] = Entries[RoomEffectActorKind.YappingMawBaseFacingUp],
        [RoomEffectActorSegment.EvirFacingLeft] = Entries[RoomEffectActorKind.EvirFacingLeft],
        [RoomEffectActorSegment.EvirFacingRight] = Entries[RoomEffectActorKind.EvirFacingRight],
        [RoomEffectActorSegment.HitBurst] = Entries[RoomEffectActorKind.HitBurst],
    };
}
