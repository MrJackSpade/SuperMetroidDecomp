namespace SuperMetroid.Core.Enemies.EffectActors;

/// <summary>The animation each room effect-actor kind starts, as the effect-actor spawn selects it.</summary>
internal static class RoomEffectActorDefinitions
{
    /// <summary>The animation each kind an owner spawns starts.</summary>
    internal static IReadOnlyDictionary<RoomEffectActorKind, RoomEffectActorSegment> Animations { get; } =
        new Dictionary<RoomEffectActorKind, RoomEffectActorSegment>
    {
        [RoomEffectActorKind.SporeSpawnDyingExplosion] = RoomEffectActorSegment.SmallExplosion,
        [RoomEffectActorKind.EnemyProjectileDud] = RoomEffectActorSegment.DudShot,
        [RoomEffectActorKind.BotwoonSmallExplosion] = RoomEffectActorSegment.SmallDudShot,
        [RoomEffectActorKind.NinjaPirateLandingDust] = RoomEffectActorSegment.SpacePirateLandingDustCloud,
        [RoomEffectActorKind.Smoke] = RoomEffectActorSegment.Smoke,
        [RoomEffectActorKind.ShortBigDustCloud] = RoomEffectActorSegment.ShortBigDustCloud,
        [RoomEffectActorKind.DustCloud] = RoomEffectActorSegment.BigDustCloud,
        [RoomEffectActorKind.DraygonBreathBubble] = RoomEffectActorSegment.ShortDraygonBreathBubbles,
        [RoomEffectActorKind.BotwoonLargeExplosion] = RoomEffectActorSegment.BigExplosion,
        [RoomEffectActorKind.NuclearWaffleBody] = RoomEffectActorSegment.PuromiBody,
        [RoomEffectActorKind.NuclearWaffleTurnClockwise] = RoomEffectActorSegment.PuromiRightExplosion,
        [RoomEffectActorKind.NuclearWaffleTurnCounterClockwise] = RoomEffectActorSegment.PuromiLeftExplosion,
        [RoomEffectActorKind.NuclearWaffleTurnOverlay] = RoomEffectActorSegment.PuromiSplash,
        [RoomEffectActorKind.FallingSparkTrail] = RoomEffectActorSegment.FallingSparkTrail,
        [RoomEffectActorKind.MetroidElectricity] = RoomEffectActorSegment.MetroidElectricity,
        [RoomEffectActorKind.MetroidShell] = RoomEffectActorSegment.MetroidShell,
        [RoomEffectActorKind.YappingMawRootFacingDown] = RoomEffectActorSegment.YappingMawBaseFacingDown,
        [RoomEffectActorKind.YappingMawRootFacingUp] = RoomEffectActorSegment.YappingMawBaseFacingUp,
        [RoomEffectActorKind.DraygonIntroEvir] = RoomEffectActorSegment.EvirFacingLeft,
        [RoomEffectActorKind.DraygonDeathEvirFacingRight] = RoomEffectActorSegment.EvirFacingRight,
        [RoomEffectActorKind.DraygonSpiralFoam] = RoomEffectActorSegment.DraygonFoamingAtTheMouth,
        [RoomEffectActorKind.EnemyHit] = RoomEffectActorSegment.EnemyHit,
    };
}
