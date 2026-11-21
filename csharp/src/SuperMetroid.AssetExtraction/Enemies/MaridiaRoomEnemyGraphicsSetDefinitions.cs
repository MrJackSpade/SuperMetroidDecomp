using SuperMetroid.Core.Enemies;
namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The graphics sets of the Maridia room-header region, in table order.</summary>
internal static class MaridiaRoomEnemyGraphicsSetDefinitions
{
    /// <summary>The members of every set here that loads Owtches, Choots and Skulteras, in record order.</summary>
    private static readonly RoomEnemyGraphicsSetMember[] OwtchChootAndSkultera =
    [
        new(EnemyDefinitionId.Owtch), new(EnemyDefinitionId.Choot), new(EnemyDefinitionId.Skultera),
    ];

    /// <summary>The members of every set here that loads Cacatacs and Zoas, in record order.</summary>
    private static readonly RoomEnemyGraphicsSetMember[] CacatacAndZoa =
    [
        new(EnemyDefinitionId.Cacatac), new(EnemyDefinitionId.Zoa),
    ];

    /// <summary>The region's lists, in table order.</summary>
    internal static RoomEnemyGraphicsSetMembers[] Build() =>
    [
        new(RoomEnemyGraphicsSetDefinitions.MtEverest, [
            new(EnemyDefinitionId.Powamp),
            new(EnemyDefinitionId.Sciser),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.HalfieClimb, [
            new(EnemyDefinitionId.Oum),
            new(EnemyDefinitionId.Mochtroid),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WateringHole, [
            new(EnemyDefinitionId.Choot),
            new(EnemyDefinitionId.Zeb),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.CrabShaft, [
            new(EnemyDefinitionId.Sciser),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.MaridiaElevator, [
            new(EnemyDefinitionId.Ripper),
            new(EnemyDefinitionId.Owtch),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Colosseum, [
            new(EnemyDefinitionId.Mochtroid),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.AqueductSave, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.ThePrecious, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BotwoonEnergyTank, [
            new(EnemyDefinitionId.Puyo),
            new(EnemyDefinitionId.Sciser),
            new(EnemyDefinitionId.Zoa),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.DraygonSave, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.MaridiaMissileRefill, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.PlasmaBeachQuicksand, [
            new(EnemyDefinitionId.Bull),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BotwoonQuicksand, [
            new(EnemyDefinitionId.Bull),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Shaktool, [
            new(EnemyDefinitionId.Shaktool),
            new(EnemyDefinitionId.Yard),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.DraygonDefault, [
            new(EnemyDefinitionId.DraygonBody),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Aqueduct, [
            new(EnemyDefinitionId.Yard),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.RedFish, [
            new(EnemyDefinitionId.Skultera),
            new(EnemyDefinitionId.Sciser),
            new(EnemyDefinitionId.Zebbo),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Plasma, [
            new(EnemyDefinitionId.PirateMagentaWalking),
            new(EnemyDefinitionId.PirateMagentaWall),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.SpaceJump, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Pants, [
            new(EnemyDefinitionId.MaridiaFly),
            new(EnemyDefinitionId.Puyo),
            new(EnemyDefinitionId.Tripper),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.GlassTunnelSave, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.GlassTunnel, [
            new(EnemyDefinitionId.NoobTubeCrack),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WestTunnel, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.EastTunnel, [
            new(EnemyDefinitionId.Boyon),
            new(EnemyDefinitionId.Zebbo),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.MamaTurtle, [
            new(EnemyDefinitionId.MamaTurtle),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.CrabTunnel, [
            new(EnemyDefinitionId.Sciser),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.NorthwestMaridiaBug, [
            new(EnemyDefinitionId.Owtch),
            new(EnemyDefinitionId.MaridiaFly),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.PseudoPlasmaSpark, OwtchChootAndSkultera),
        new(RoomEnemyGraphicsSetDefinitions.WestSandHallTunnel, [
            new(EnemyDefinitionId.Sciser),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.PlasmaTutorial, [
            new(EnemyDefinitionId.Puyo),
            new(EnemyDefinitionId.ArmoredZoomer),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.ThreadTheNeedle, [
            new(EnemyDefinitionId.Puyo),
            new(EnemyDefinitionId.Boyon),
            new(EnemyDefinitionId.Choot),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.PlasmaSpark, OwtchChootAndSkultera),
        new(RoomEnemyGraphicsSetDefinitions.Toilet, [
            new(EnemyDefinitionId.Mochtroid),
            new(EnemyDefinitionId.Puyo),
            new(EnemyDefinitionId.Sciser),
            new(EnemyDefinitionId.Yard),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WestSandHall, [
            new(EnemyDefinitionId.Evir),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Oasis, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.EastSandHall, [
            new(EnemyDefinitionId.Evir),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WestAqueductQuicksand, [
            new(EnemyDefinitionId.Bull),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.EastAqueductQuicksand, [
            new(EnemyDefinitionId.Bull),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Butterfly, [
            new(EnemyDefinitionId.Zoa),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BotwoonHallway, [
            new(EnemyDefinitionId.Mochtroid),
            new(EnemyDefinitionId.Puyo),
            new(EnemyDefinitionId.Sciser),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.EastPants, [
            new(EnemyDefinitionId.Puyo),
            new(EnemyDefinitionId.Tripper),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Springball, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BelowBotwoonEnergyTank, [
            new(EnemyDefinitionId.Zoa),
            new(EnemyDefinitionId.Owtch),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Kassiuz, [
            new(EnemyDefinitionId.Choot),
            new(EnemyDefinitionId.Puyo),
            new(EnemyDefinitionId.Boyon),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.ForgottenHighwaySave, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.MaridiaEnergyRefill, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.MaridiaMap, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.CrabHole, [
            new(EnemyDefinitionId.Sciser),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Botwoon, [
            new(EnemyDefinitionId.Owtch),
            new(EnemyDefinitionId.Botwoon),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.MainStreet, [
            new(EnemyDefinitionId.Skultera),
            new(EnemyDefinitionId.Sciser),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BugSandHole, [
            new(EnemyDefinitionId.YappingMaw),
            new(EnemyDefinitionId.Zoa),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.EastSandHole, [
            new(EnemyDefinitionId.Boulder),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WestSandHole, [
            new(EnemyDefinitionId.Boulder),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WestCactusAlley, CacatacAndZoa),
        new(RoomEnemyGraphicsSetDefinitions.EastCactusAlley, CacatacAndZoa),
        new(RoomEnemyGraphicsSetDefinitions.FishTank, [
            new(EnemyDefinitionId.Skultera),
            new(EnemyDefinitionId.PirateMagentaWalking),
        ]),
    ];
}
