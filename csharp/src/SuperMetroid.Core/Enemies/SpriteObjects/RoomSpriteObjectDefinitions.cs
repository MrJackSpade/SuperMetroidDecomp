namespace SuperMetroid.Core.Enemies.SpriteObjects;

/// <summary>The animation each room sprite-object kind starts, as the sprite-object spawn selects it.</summary>
internal static class RoomSpriteObjectDefinitions
{
    /// <summary>The animation each kind an owner spawns starts.</summary>
    internal static IReadOnlyDictionary<RoomSpriteObjectKind, RoomSpriteObjectSegment> Animations { get; } =
        new Dictionary<RoomSpriteObjectKind, RoomSpriteObjectSegment>
    {
        [RoomSpriteObjectKind.SporeSpawnDyingExplosion] = RoomSpriteObjectSegment.SmallExplosion,
        [RoomSpriteObjectKind.EnemyProjectileDud] = RoomSpriteObjectSegment.DudShot,
        [RoomSpriteObjectKind.BotwoonSmallExplosion] = RoomSpriteObjectSegment.SmallDudShot,
        [RoomSpriteObjectKind.NinjaPirateLandingDust] = RoomSpriteObjectSegment.SpacePirateLandingDustCloud,
        [RoomSpriteObjectKind.Smoke] = RoomSpriteObjectSegment.Smoke,
        [RoomSpriteObjectKind.ShortBigDustCloud] = RoomSpriteObjectSegment.ShortBigDustCloud,
        [RoomSpriteObjectKind.DustCloud] = RoomSpriteObjectSegment.BigDustCloud,
        [RoomSpriteObjectKind.DraygonBreathBubble] = RoomSpriteObjectSegment.ShortDraygonBreathBubbles,
        [RoomSpriteObjectKind.BotwoonLargeExplosion] = RoomSpriteObjectSegment.BigExplosion,
        [RoomSpriteObjectKind.NuclearWaffleBody] = RoomSpriteObjectSegment.PuromiBody,
        [RoomSpriteObjectKind.NuclearWaffleTurnClockwise] = RoomSpriteObjectSegment.PuromiRightExplosion,
        [RoomSpriteObjectKind.NuclearWaffleTurnCounterClockwise] = RoomSpriteObjectSegment.PuromiLeftExplosion,
        [RoomSpriteObjectKind.NuclearWaffleTurnOverlay] = RoomSpriteObjectSegment.PuromiSplash,
        [RoomSpriteObjectKind.FallingSparkTrail] = RoomSpriteObjectSegment.FallingSparkTrail,
        [RoomSpriteObjectKind.MetroidElectricity] = RoomSpriteObjectSegment.MetroidElectricity,
        [RoomSpriteObjectKind.MetroidShell] = RoomSpriteObjectSegment.MetroidShell,
        [RoomSpriteObjectKind.YappingMawRootFacingDown] = RoomSpriteObjectSegment.YappingMawBaseFacingDown,
        [RoomSpriteObjectKind.YappingMawRootFacingUp] = RoomSpriteObjectSegment.YappingMawBaseFacingUp,
        [RoomSpriteObjectKind.DraygonIntroEvir] = RoomSpriteObjectSegment.EvirFacingLeft,
        [RoomSpriteObjectKind.DraygonDeathEvirFacingRight] = RoomSpriteObjectSegment.EvirFacingRight,
        [RoomSpriteObjectKind.DraygonSpiralFoam] = RoomSpriteObjectSegment.DraygonFoamingAtTheMouth,
        [RoomSpriteObjectKind.EnemyHit] = RoomSpriteObjectSegment.EnemyHit,
    };
}
