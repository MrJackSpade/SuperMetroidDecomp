namespace SuperMetroid.Core.Enemies;

/// <summary>The bank-$B4 graphics sets of the Maridia room-header region, in native table order.</summary>
internal static class MaridiaRoomEnemyGraphicsSetDefinitions
{
    /// <summary>The region's lists, in native table order.</summary>
    internal static RoomEnemyGraphicsSetDefinition[] Build() =>
    [
        new(nameof(RoomEnemyGraphicsSetDefinitions.MtEverest), [
            new(EnemyDefinitionId.Powamp, ColourStore: new(1)),
            new(EnemyDefinitionId.Sciser, ColourStore: new(3)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.HalfieClimb), [
            new(EnemyDefinitionId.Oum, ColourStore: new(1)),
            new(EnemyDefinitionId.Mochtroid, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WateringHole), [
            new(EnemyDefinitionId.Choot, ColourStore: new(1)),
            new(EnemyDefinitionId.Zeb, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrabShaft), [
            new(EnemyDefinitionId.Sciser, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MaridiaElev), [
            new(EnemyDefinitionId.Ripper, ColourStore: new(3)),
            new(EnemyDefinitionId.Owtch, ColourStore: new(7)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Colosseum), [
            new(EnemyDefinitionId.Mochtroid, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.AqueductSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.ThePrecious), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BotwoonETank), [
            new(EnemyDefinitionId.Puyo, ColourStore: new(1)),
            new(EnemyDefinitionId.Sciser, ColourStore: new(1)),
            new(EnemyDefinitionId.Zoa, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.DraygonSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MaridiaMissileRefill), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PlasmaBeachQuicksand), [
            new(EnemyDefinitionId.Bull, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BotwoonQuicksand), [
            new(EnemyDefinitionId.Bull, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Shaktool), [
            new(EnemyDefinitionId.Shaktool, ColourStore: new(1)),
            new(EnemyDefinitionId.Yard, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Draygon0), [
            new(EnemyDefinitionId.DraygonBody, ColourStore: new(7)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Aqueduct), [
            new(EnemyDefinitionId.Yard, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.RedFish), [
            new(EnemyDefinitionId.Skultera, ColourStore: new(1)),
            new(EnemyDefinitionId.Sciser, ColourStore: new(1)),
            new(EnemyDefinitionId.Zebbo, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Plasma), [
            new(EnemyDefinitionId.PirateMagentaWalking, ColourStore: new(1)),
            new(EnemyDefinitionId.PirateMagentaWall, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.SpaceJump), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Pants), [
            new(EnemyDefinitionId.Menu, ColourStore: new(1)),
            new(EnemyDefinitionId.Puyo, ColourStore: new(2)),
            new(EnemyDefinitionId.Tripper, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GlassTunnelSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GlassTunnel), [
            new(EnemyDefinitionId.NoobTubeCrack, ColourStore: new(7)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WestTunnel), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.EastTunnel), [
            new(EnemyDefinitionId.Boyon, ColourStore: new(1)),
            new(EnemyDefinitionId.Zebbo, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MamaTurtle), [
            new(EnemyDefinitionId.MamaTurtle, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrabTunnel), [
            new(EnemyDefinitionId.Sciser, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.NWestMaridiaBug), [
            new(EnemyDefinitionId.Owtch, ColourStore: new(1)),
            new(EnemyDefinitionId.Menu, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PseudoPlasmaSpark), [
            new(EnemyDefinitionId.Owtch, ColourStore: new(1)),
            new(EnemyDefinitionId.Choot, ColourStore: new(2)),
            new(EnemyDefinitionId.Skultera, ColourStore: new(3)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WestSandHallTunnel), [
            new(EnemyDefinitionId.Sciser, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PlasmaTutorial), [
            new(EnemyDefinitionId.Puyo, ColourStore: new(1)),
            new(EnemyDefinitionId.MZoomer, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.ThreadTheNeedle), [
            new(EnemyDefinitionId.Puyo, ColourStore: new(1)),
            new(EnemyDefinitionId.Boyon, ColourStore: new(1)),
            new(EnemyDefinitionId.Choot, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PlasmaSpark), [
            new(EnemyDefinitionId.Owtch, ColourStore: new(1)),
            new(EnemyDefinitionId.Choot, ColourStore: new(2)),
            new(EnemyDefinitionId.Skultera, ColourStore: new(3)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Toilet), [
            new(EnemyDefinitionId.Mochtroid, ColourStore: new(1)),
            new(EnemyDefinitionId.Puyo, ColourStore: new(2)),
            new(EnemyDefinitionId.Sciser, ColourStore: new(2)),
            new(EnemyDefinitionId.Yard, ColourStore: new(3)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WestSandHall), [
            new(EnemyDefinitionId.Evir, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Oasis), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.EastSandHall), [
            new(EnemyDefinitionId.Evir, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WestAqueductQuicksand), [
            new(EnemyDefinitionId.Bull, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.EastAqueductQuicksand), [
            new(EnemyDefinitionId.Bull, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Butterfly), [
            new(EnemyDefinitionId.Zoa, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BotwoonHallway), [
            new(EnemyDefinitionId.Mochtroid, ColourStore: new(1)),
            new(EnemyDefinitionId.Puyo, ColourStore: new(2)),
            new(EnemyDefinitionId.Sciser, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.EastPants), [
            new(EnemyDefinitionId.Puyo, ColourStore: new(1)),
            new(EnemyDefinitionId.Tripper, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Springball), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BelowBotwoonETank), [
            new(EnemyDefinitionId.Zoa, ColourStore: new(1)),
            new(EnemyDefinitionId.Owtch, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Kassiuz), [
            new(EnemyDefinitionId.Choot, ColourStore: new(1)),
            new(EnemyDefinitionId.Puyo, ColourStore: new(2)),
            new(EnemyDefinitionId.Boyon, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.ForgottenHighwaySave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MaridiaEnergyRefill), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MaridiaMap), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrabHole), [
            new(EnemyDefinitionId.Sciser, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Botwoon), [
            new(EnemyDefinitionId.Owtch, ColourStore: new(1)),
            new(EnemyDefinitionId.Botwoon, ColourStore: new(7)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MainStreet), [
            new(EnemyDefinitionId.Skultera, ColourStore: new(1)),
            new(EnemyDefinitionId.Sciser, ColourStore: new(2)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BugSandHole), [
            new(EnemyDefinitionId.YappingMaw, ColourStore: new(1)),
            new(EnemyDefinitionId.Zoa, ColourStore: new(7)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.EastSandHole), [
            new(EnemyDefinitionId.Boulder, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WestSandHole), [
            new(EnemyDefinitionId.Boulder, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WestCactusAlley), [
            new(EnemyDefinitionId.Cacatac, ColourStore: new(7)),
            new(EnemyDefinitionId.Zoa, ColourStore: new(7)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.EastCactusAlley), [
            new(EnemyDefinitionId.Cacatac, ColourStore: new(1)),
            new(EnemyDefinitionId.Zoa, ColourStore: new(1)),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.FishTank), [
            new(EnemyDefinitionId.Skultera, ColourStore: new(1)),
            new(EnemyDefinitionId.PirateMagentaWalking, ColourStore: new(2)),
        ]),
    ];
}
