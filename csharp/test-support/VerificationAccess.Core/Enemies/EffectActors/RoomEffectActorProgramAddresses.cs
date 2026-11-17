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
    /// Each segment's list address (#2275): a kind's own run starts at the kind's list, and a Metroid's electricity's and
    /// shell's cycles at the records their first pass runs on into.
    /// </summary>
    internal static IReadOnlyDictionary<RoomEffectActorSegment, ushort> SegmentEntries { get; } =
        new Dictionary<RoomEffectActorSegment, ushort>(Entries.Select(entry =>
            KeyValuePair.Create(Enum.Parse<RoomEffectActorSegment>(entry.Key.ToString()), entry.Value)))
        {
            [RoomEffectActorSegment.MetroidElectricityCycle] = 0xc436,
            [RoomEffectActorSegment.MetroidShellCycle] = 0xc536,
        };

    /// <summary>
    /// The native records beat <paramref name="beat"/> of <paramref name="segment"/> stands for: one, except the splash's
    /// first frame, which the list shows in two records of two updates each and the port as one frame of four (#2275).
    /// </summary>
    internal static int RecordsOf(RoomEffectActorSegment segment, int beat) =>
        segment == RoomEffectActorSegment.NuclearWaffleSplash && beat == 0 ? 2 : 1;
}
