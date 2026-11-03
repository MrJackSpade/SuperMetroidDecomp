namespace SuperMetroid.Core.Enemies;

/// <summary>The graphics sets of the Maridia room-header region, in table order.</summary>
internal static class MaridiaRoomEnemyGraphicsSetDefinitions
{
    /// <summary>The region's lists, in table order.</summary>
    internal static RoomEnemyGraphicsSetDefinition[] Build() =>
    [
        new(nameof(RoomEnemyGraphicsSetDefinitions.MtEverest), [
            new(EnemyDefinitionId.Powamp),
            new(EnemyDefinitionId.Sciser),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.HalfieClimb), [
            new(EnemyDefinitionId.Oum),
            new(EnemyDefinitionId.Mochtroid),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WateringHole), [
            new(EnemyDefinitionId.Choot),
            new(EnemyDefinitionId.Zeb),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrabShaft), [
            new(EnemyDefinitionId.Sciser),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MaridiaElev), [
            new(EnemyDefinitionId.Ripper),
            new(EnemyDefinitionId.Owtch),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Colosseum), [
            new(EnemyDefinitionId.Mochtroid),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.AqueductSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.ThePrecious), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BotwoonETank), [
            new(EnemyDefinitionId.Puyo),
            new(EnemyDefinitionId.Sciser),
            new(EnemyDefinitionId.Zoa),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.DraygonSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MaridiaMissileRefill), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PlasmaBeachQuicksand), [
            new(EnemyDefinitionId.Bull),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BotwoonQuicksand), [
            new(EnemyDefinitionId.Bull),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Shaktool), [
            new(EnemyDefinitionId.Shaktool),
            new(EnemyDefinitionId.Yard),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Draygon0), [
            new(EnemyDefinitionId.DraygonBody),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Aqueduct), [
            new(EnemyDefinitionId.Yard),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.RedFish), [
            new(EnemyDefinitionId.Skultera),
            new(EnemyDefinitionId.Sciser),
            new(EnemyDefinitionId.Zebbo),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Plasma), [
            new(EnemyDefinitionId.PirateMagentaWalking),
            new(EnemyDefinitionId.PirateMagentaWall),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.SpaceJump), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Pants), [
            new(EnemyDefinitionId.Menu),
            new(EnemyDefinitionId.Puyo),
            new(EnemyDefinitionId.Tripper),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GlassTunnelSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GlassTunnel), [
            new(EnemyDefinitionId.NoobTubeCrack),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WestTunnel), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.EastTunnel), [
            new(EnemyDefinitionId.Boyon),
            new(EnemyDefinitionId.Zebbo),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MamaTurtle), [
            new(EnemyDefinitionId.MamaTurtle),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrabTunnel), [
            new(EnemyDefinitionId.Sciser),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.NWestMaridiaBug), [
            new(EnemyDefinitionId.Owtch),
            new(EnemyDefinitionId.Menu),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PseudoPlasmaSpark), [
            new(EnemyDefinitionId.Owtch),
            new(EnemyDefinitionId.Choot),
            new(EnemyDefinitionId.Skultera),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WestSandHallTunnel), [
            new(EnemyDefinitionId.Sciser),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PlasmaTutorial), [
            new(EnemyDefinitionId.Puyo),
            new(EnemyDefinitionId.MZoomer),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.ThreadTheNeedle), [
            new(EnemyDefinitionId.Puyo),
            new(EnemyDefinitionId.Boyon),
            new(EnemyDefinitionId.Choot),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PlasmaSpark), [
            new(EnemyDefinitionId.Owtch),
            new(EnemyDefinitionId.Choot),
            new(EnemyDefinitionId.Skultera),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Toilet), [
            new(EnemyDefinitionId.Mochtroid),
            new(EnemyDefinitionId.Puyo),
            new(EnemyDefinitionId.Sciser),
            new(EnemyDefinitionId.Yard),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WestSandHall), [
            new(EnemyDefinitionId.Evir),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Oasis), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.EastSandHall), [
            new(EnemyDefinitionId.Evir),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WestAqueductQuicksand), [
            new(EnemyDefinitionId.Bull),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.EastAqueductQuicksand), [
            new(EnemyDefinitionId.Bull),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Butterfly), [
            new(EnemyDefinitionId.Zoa),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BotwoonHallway), [
            new(EnemyDefinitionId.Mochtroid),
            new(EnemyDefinitionId.Puyo),
            new(EnemyDefinitionId.Sciser),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.EastPants), [
            new(EnemyDefinitionId.Puyo),
            new(EnemyDefinitionId.Tripper),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Springball), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BelowBotwoonETank), [
            new(EnemyDefinitionId.Zoa),
            new(EnemyDefinitionId.Owtch),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Kassiuz), [
            new(EnemyDefinitionId.Choot),
            new(EnemyDefinitionId.Puyo),
            new(EnemyDefinitionId.Boyon),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.ForgottenHighwaySave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MaridiaEnergyRefill), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MaridiaMap), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrabHole), [
            new(EnemyDefinitionId.Sciser),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Botwoon), [
            new(EnemyDefinitionId.Owtch),
            new(EnemyDefinitionId.Botwoon),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MainStreet), [
            new(EnemyDefinitionId.Skultera),
            new(EnemyDefinitionId.Sciser),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BugSandHole), [
            new(EnemyDefinitionId.YappingMaw),
            new(EnemyDefinitionId.Zoa),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.EastSandHole), [
            new(EnemyDefinitionId.Boulder),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WestSandHole), [
            new(EnemyDefinitionId.Boulder),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WestCactusAlley), [
            new(EnemyDefinitionId.Cacatac),
            new(EnemyDefinitionId.Zoa),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.EastCactusAlley), [
            new(EnemyDefinitionId.Cacatac),
            new(EnemyDefinitionId.Zoa),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.FishTank), [
            new(EnemyDefinitionId.Skultera),
            new(EnemyDefinitionId.PirateMagentaWalking),
        ]),
    ];
}
