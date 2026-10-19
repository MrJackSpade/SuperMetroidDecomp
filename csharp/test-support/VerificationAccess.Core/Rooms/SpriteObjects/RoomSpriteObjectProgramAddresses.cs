namespace SuperMetroid.Core.Rooms.SpriteObjects;

/// <summary>The bank-$B4 address of each room sprite-object program's native instruction list.</summary>
internal static class RoomSpriteObjectProgramAddresses
{
    /// <summary>Each program's list address.</summary>
    internal static IReadOnlyDictionary<RoomSpriteObjectProgram, ushort> Entries { get; } = new Dictionary<RoomSpriteObjectProgram, ushort>
    {
        [RoomSpriteObjectProgram.DraygonFoamingAtTheMouth] = 0xbe24,
        [RoomSpriteObjectProgram.SmallExplosion] = 0xbea4,
        [RoomSpriteObjectProgram.DudShot] = 0xbeea,
        [RoomSpriteObjectProgram.SmallDudShot] = 0xbf1c,
        [RoomSpriteObjectProgram.SpacePirateLandingDustCloud] = 0xbf32,
        [RoomSpriteObjectProgram.Smoke] = 0xbf56,
        [RoomSpriteObjectProgram.BigExplosion] = 0xbf74,
        [RoomSpriteObjectProgram.ShortBigDustCloud] = 0xc014,
        [RoomSpriteObjectProgram.BigDustCloud] = 0xc05e,
        [RoomSpriteObjectProgram.ShortDraygonBreathBubbles] = 0xc10c,
        [RoomSpriteObjectProgram.PuromiBody] = 0xc30a,
        [RoomSpriteObjectProgram.PuromiRightExplosion] = 0xc33e,
        [RoomSpriteObjectProgram.PuromiLeftExplosion] = 0xc35c,
        [RoomSpriteObjectProgram.PuromiSplash] = 0xc37a,
        [RoomSpriteObjectProgram.FallingSparkTrail] = 0xc390,
        [RoomSpriteObjectProgram.MetroidElectricity] = 0xc3ba,
        [RoomSpriteObjectProgram.Unused33] = 0xc436,
        [RoomSpriteObjectProgram.MetroidShell] = 0xc4b6,
        [RoomSpriteObjectProgram.Unused35] = 0xc536,
        [RoomSpriteObjectProgram.YappingMawBaseFacingDown] = 0xc5d8,
        [RoomSpriteObjectProgram.YappingMawBaseFacingUp] = 0xc5de,
        [RoomSpriteObjectProgram.EvirFacingLeft] = 0xc608,
        [RoomSpriteObjectProgram.EvirFacingRight] = 0xc61c,
    };
}
