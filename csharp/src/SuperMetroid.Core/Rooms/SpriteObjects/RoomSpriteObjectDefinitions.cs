namespace SuperMetroid.Core.Rooms.SpriteObjects;

/// <summary>The program each room sprite-object kind starts, as <c>CreateSpriteAtPos</c> at $B4:BC26 selects it.</summary>
internal static class RoomSpriteObjectDefinitions
{
    /// <summary>The program <paramref name="kind"/> starts.</summary>
    internal static RoomSpriteObjectProgram Program(RoomSpriteObjectKind kind) => kind switch
    {
        RoomSpriteObjectKind.SporeSpawnDyingExplosion => RoomSpriteObjectProgram.SmallExplosion,
        RoomSpriteObjectKind.EnemyProjectileDud => RoomSpriteObjectProgram.DudShot,
        RoomSpriteObjectKind.BotwoonSmallExplosion => RoomSpriteObjectProgram.SmallDudShot,
        RoomSpriteObjectKind.NinjaPirateLandingDust => RoomSpriteObjectProgram.SpacePirateLandingDustCloud,
        RoomSpriteObjectKind.Smoke => RoomSpriteObjectProgram.Smoke,
        RoomSpriteObjectKind.ShortBigDustCloud => RoomSpriteObjectProgram.ShortBigDustCloud,
        RoomSpriteObjectKind.DustCloud => RoomSpriteObjectProgram.BigDustCloud,
        RoomSpriteObjectKind.DraygonBreathBubble => RoomSpriteObjectProgram.ShortDraygonBreathBubbles,
        RoomSpriteObjectKind.BotwoonLargeExplosion => RoomSpriteObjectProgram.BigExplosion,
        RoomSpriteObjectKind.NuclearWaffleBody => RoomSpriteObjectProgram.PuromiBody,
        RoomSpriteObjectKind.NuclearWaffleTurnClockwise => RoomSpriteObjectProgram.PuromiRightExplosion,
        RoomSpriteObjectKind.NuclearWaffleTurnCounterClockwise => RoomSpriteObjectProgram.PuromiLeftExplosion,
        RoomSpriteObjectKind.NuclearWaffleTurnOverlay => RoomSpriteObjectProgram.PuromiSplash,
        RoomSpriteObjectKind.FallingSparkTrail => RoomSpriteObjectProgram.FallingSparkTrail,
        RoomSpriteObjectKind.MetroidOuterBodyA => RoomSpriteObjectProgram.MetroidElectricity,
        RoomSpriteObjectKind.MetroidOuterBodyB => RoomSpriteObjectProgram.MetroidShell,
        RoomSpriteObjectKind.YappingMawRootVariantZero => RoomSpriteObjectProgram.YappingMawBaseFacingDown,
        RoomSpriteObjectKind.YappingMawRootVariantOne => RoomSpriteObjectProgram.YappingMawBaseFacingUp,
        RoomSpriteObjectKind.DraygonIntroEvir => RoomSpriteObjectProgram.EvirFacingLeft,
        RoomSpriteObjectKind.DraygonDeathEvirFacingRight => RoomSpriteObjectProgram.EvirFacingRight,
        RoomSpriteObjectKind.DraygonSpiralFoam => RoomSpriteObjectProgram.DraygonFoamingAtTheMouth,
        RoomSpriteObjectKind.None => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Not a room sprite-object kind any owner spawns."),
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Not a room sprite-object kind any owner spawns."),
    };
}
