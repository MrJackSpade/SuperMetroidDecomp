namespace SuperMetroid.Core.Enemies.SpriteObjects;

/// <summary>The bank-$B4 address of each room sprite-object program's native instruction list.</summary>
internal static class RoomSpriteObjectProgramAddresses
{
    /// <summary>Each program's list address.</summary>
    internal static IReadOnlyDictionary<RoomSpriteObjectSegment, ushort> Entries { get; } = new Dictionary<RoomSpriteObjectSegment, ushort>
    {
        [RoomSpriteObjectSegment.DraygonFoamingAtTheMouth] = 0xbe24,
        [RoomSpriteObjectSegment.SmallExplosion] = 0xbea4,
        [RoomSpriteObjectSegment.DudShot] = 0xbeea,
        [RoomSpriteObjectSegment.SmallDudShot] = 0xbf1c,
        [RoomSpriteObjectSegment.SpacePirateLandingDustCloud] = 0xbf32,
        [RoomSpriteObjectSegment.Smoke] = 0xbf56,
        [RoomSpriteObjectSegment.BigExplosion] = 0xbf74,
        [RoomSpriteObjectSegment.ShortBigDustCloud] = 0xc014,
        [RoomSpriteObjectSegment.BigDustCloud] = 0xc05e,
        [RoomSpriteObjectSegment.ShortDraygonBreathBubbles] = 0xc10c,
        [RoomSpriteObjectSegment.PuromiBody] = 0xc30a,
        [RoomSpriteObjectSegment.PuromiRightExplosion] = 0xc33e,
        [RoomSpriteObjectSegment.PuromiLeftExplosion] = 0xc35c,
        [RoomSpriteObjectSegment.PuromiSplash] = 0xc37a,
        [RoomSpriteObjectSegment.FallingSparkTrail] = 0xc390,
        [RoomSpriteObjectSegment.MetroidElectricity] = 0xc3ba,
        [RoomSpriteObjectSegment.MetroidElectricityLoop] = 0xc436,
        [RoomSpriteObjectSegment.MetroidShell] = 0xc4b6,
        [RoomSpriteObjectSegment.MetroidShellLoop] = 0xc536,
        [RoomSpriteObjectSegment.YappingMawBaseFacingDown] = 0xc5d8,
        [RoomSpriteObjectSegment.YappingMawBaseFacingUp] = 0xc5de,
        [RoomSpriteObjectSegment.EvirFacingLeft] = 0xc608,
        [RoomSpriteObjectSegment.EvirFacingRight] = 0xc61c,
        [RoomSpriteObjectSegment.EnemyHit] = 0xc5c6,
    };
}
