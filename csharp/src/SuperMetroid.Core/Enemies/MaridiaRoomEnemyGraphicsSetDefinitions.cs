namespace SuperMetroid.Core.Enemies;

/// <summary>The bank-$B4 graphics sets of the Maridia room-header region, in native table order.</summary>
internal static class MaridiaRoomEnemyGraphicsSetDefinitions
{
    /// <summary>The region's lists, in native table order.</summary>
    internal static RoomEnemyGraphicsSetDefinition[] Build() =>
    [
        new(nameof(RoomEnemyGraphicsSetDefinitions.MtEverest), [
            new(EnemyDefinitionId.Powamp, PaletteRow: 1),
            new(EnemyDefinitionId.Sciser, PaletteRow: 3),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.HalfieClimb), [
            new(EnemyDefinitionId.Oum, PaletteRow: 1),
            new(EnemyDefinitionId.Mochtroid, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WateringHole), [
            new(EnemyDefinitionId.Choot, PaletteRow: 1),
            new(EnemyDefinitionId.Zeb, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrabShaft), [
            new(EnemyDefinitionId.Sciser, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MaridiaElev), [
            new(EnemyDefinitionId.Ripper, PaletteRow: 3),
            new(EnemyDefinitionId.Owtch, PaletteRow: 7),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Colosseum), [
            new(EnemyDefinitionId.Mochtroid, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.AqueductSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.ThePrecious), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BotwoonETank), [
            new(EnemyDefinitionId.Puyo, PaletteRow: 1),
            new(EnemyDefinitionId.Sciser, PaletteRow: 1),
            new(EnemyDefinitionId.Zoa, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.DraygonSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MaridiaMissileRefill), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PlasmaBeachQuicksand), [
            new(EnemyDefinitionId.Bull, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BotwoonQuicksand), [
            new(EnemyDefinitionId.Bull, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Shaktool), [
            new(EnemyDefinitionId.Shaktool, PaletteRow: 1),
            new(EnemyDefinitionId.Yard, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Draygon0), [
            new(EnemyDefinitionId.DraygonBody, PaletteRow: 7),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Aqueduct), [
            new(EnemyDefinitionId.Yard, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.RedFish), [
            new(EnemyDefinitionId.Skultera, PaletteRow: 1),
            new(EnemyDefinitionId.Sciser, PaletteRow: 1),
            new(EnemyDefinitionId.Zebbo, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Plasma), [
            new(EnemyDefinitionId.PirateMagentaWalking, PaletteRow: 1),
            new(EnemyDefinitionId.PirateMagentaWall, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.SpaceJump), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Pants), [
            new(EnemyDefinitionId.Menu, PaletteRow: 1),
            new(EnemyDefinitionId.Puyo, PaletteRow: 2),
            new(EnemyDefinitionId.Tripper, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GlassTunnelSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GlassTunnel), [
            new(EnemyDefinitionId.NoobTubeCrack, PaletteRow: 7),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WestTunnel), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.EastTunnel), [
            new(EnemyDefinitionId.Boyon, PaletteRow: 1),
            new(EnemyDefinitionId.Zebbo, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MamaTurtle), [
            new(EnemyDefinitionId.MamaTurtle, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrabTunnel), [
            new(EnemyDefinitionId.Sciser, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.NWestMaridiaBug), [
            new(EnemyDefinitionId.Owtch, PaletteRow: 1),
            new(EnemyDefinitionId.Menu, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PseudoPlasmaSpark), [
            new(EnemyDefinitionId.Owtch, PaletteRow: 1),
            new(EnemyDefinitionId.Choot, PaletteRow: 2),
            new(EnemyDefinitionId.Skultera, PaletteRow: 3),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WestSandHallTunnel), [
            new(EnemyDefinitionId.Sciser, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PlasmaTutorial), [
            new(EnemyDefinitionId.Puyo, PaletteRow: 1),
            new(EnemyDefinitionId.MZoomer, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.ThreadTheNeedle), [
            new(EnemyDefinitionId.Puyo, PaletteRow: 1),
            new(EnemyDefinitionId.Boyon, PaletteRow: 1),
            new(EnemyDefinitionId.Choot, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PlasmaSpark), [
            new(EnemyDefinitionId.Owtch, PaletteRow: 1),
            new(EnemyDefinitionId.Choot, PaletteRow: 2),
            new(EnemyDefinitionId.Skultera, PaletteRow: 3),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Toilet), [
            new(EnemyDefinitionId.Mochtroid, PaletteRow: 1),
            new(EnemyDefinitionId.Puyo, PaletteRow: 2),
            new(EnemyDefinitionId.Sciser, PaletteRow: 2),
            new(EnemyDefinitionId.Yard, PaletteRow: 3),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WestSandHall), [
            new(EnemyDefinitionId.Evir, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Oasis), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.EastSandHall), [
            new(EnemyDefinitionId.Evir, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WestAqueductQuicksand), [
            new(EnemyDefinitionId.Bull, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.EastAqueductQuicksand), [
            new(EnemyDefinitionId.Bull, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Butterfly), [
            new(EnemyDefinitionId.Zoa, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BotwoonHallway), [
            new(EnemyDefinitionId.Mochtroid, PaletteRow: 1),
            new(EnemyDefinitionId.Puyo, PaletteRow: 2),
            new(EnemyDefinitionId.Sciser, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.EastPants), [
            new(EnemyDefinitionId.Puyo, PaletteRow: 1),
            new(EnemyDefinitionId.Tripper, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Springball), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BelowBotwoonETank), [
            new(EnemyDefinitionId.Zoa, PaletteRow: 1),
            new(EnemyDefinitionId.Owtch, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Kassiuz), [
            new(EnemyDefinitionId.Choot, PaletteRow: 1),
            new(EnemyDefinitionId.Puyo, PaletteRow: 2),
            new(EnemyDefinitionId.Boyon, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.ForgottenHighwaySave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MaridiaEnergyRefill), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MaridiaMap), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CrabHole), [
            new(EnemyDefinitionId.Sciser, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Botwoon), [
            new(EnemyDefinitionId.Owtch, PaletteRow: 1),
            new(EnemyDefinitionId.Botwoon, PaletteRow: 7),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MainStreet), [
            new(EnemyDefinitionId.Skultera, PaletteRow: 1),
            new(EnemyDefinitionId.Sciser, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BugSandHole), [
            new(EnemyDefinitionId.YappingMaw, PaletteRow: 1),
            new(EnemyDefinitionId.Zoa, PaletteRow: 7),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.EastSandHole), [
            new(EnemyDefinitionId.Boulder, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WestSandHole), [
            new(EnemyDefinitionId.Boulder, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WestCactusAlley), [
            new(EnemyDefinitionId.Cacatac, PaletteRow: 7),
            new(EnemyDefinitionId.Zoa, PaletteRow: 7),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.EastCactusAlley), [
            new(EnemyDefinitionId.Cacatac, PaletteRow: 1),
            new(EnemyDefinitionId.Zoa, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.FishTank), [
            new(EnemyDefinitionId.Skultera, PaletteRow: 1),
            new(EnemyDefinitionId.PirateMagentaWalking, PaletteRow: 2),
        ]),
    ];
}
