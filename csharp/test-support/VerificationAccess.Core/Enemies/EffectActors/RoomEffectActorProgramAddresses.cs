namespace SuperMetroid.Core.Enemies.EffectActors;

/// <summary>The bank-$B4 address of each room sprite-object program's native instruction list.</summary>
internal static class RoomEffectActorProgramAddresses
{
    /// <summary>Each program's list address.</summary>
    internal static IReadOnlyDictionary<RoomEffectActorSegment, ushort> Entries { get; } = new Dictionary<RoomEffectActorSegment, ushort>
    {
        [RoomEffectActorSegment.DraygonFoamingAtTheMouth] = 0xbe24,
        [RoomEffectActorSegment.SmallExplosion] = 0xbea4,
        [RoomEffectActorSegment.DudShot] = 0xbeea,
        [RoomEffectActorSegment.SmallDudShot] = 0xbf1c,
        [RoomEffectActorSegment.SpacePirateLandingDustCloud] = 0xbf32,
        [RoomEffectActorSegment.Smoke] = 0xbf56,
        [RoomEffectActorSegment.BigExplosion] = 0xbf74,
        [RoomEffectActorSegment.ShortBigDustCloud] = 0xc014,
        [RoomEffectActorSegment.BigDustCloud] = 0xc05e,
        [RoomEffectActorSegment.ShortDraygonBreathBubbles] = 0xc10c,
        [RoomEffectActorSegment.PuromiBody] = 0xc30a,
        [RoomEffectActorSegment.PuromiRightExplosion] = 0xc33e,
        [RoomEffectActorSegment.PuromiLeftExplosion] = 0xc35c,
        [RoomEffectActorSegment.PuromiSplash] = 0xc37a,
        [RoomEffectActorSegment.FallingSparkTrail] = 0xc390,
        [RoomEffectActorSegment.MetroidElectricity] = 0xc3ba,
        [RoomEffectActorSegment.MetroidElectricityLoop] = 0xc436,
        [RoomEffectActorSegment.MetroidShell] = 0xc4b6,
        [RoomEffectActorSegment.MetroidShellLoop] = 0xc536,
        [RoomEffectActorSegment.YappingMawBaseFacingDown] = 0xc5d8,
        [RoomEffectActorSegment.YappingMawBaseFacingUp] = 0xc5de,
        [RoomEffectActorSegment.EvirFacingLeft] = 0xc608,
        [RoomEffectActorSegment.EvirFacingRight] = 0xc61c,
        [RoomEffectActorSegment.EnemyHit] = 0xc5c6,
    };
}
