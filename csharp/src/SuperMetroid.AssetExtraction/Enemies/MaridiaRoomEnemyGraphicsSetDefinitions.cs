using SuperMetroid.Core.Enemies;
namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The graphics sets of the Maridia room-header region, in table order.</summary>
internal static partial class MaridiaRoomEnemyGraphicsSetDefinitions
{
    /// <summary>The members of every set here that loads Owtches, Choots and Skulteras, in record order.</summary>
    private static readonly RoomEnemyGraphicsSetMember[] OwtchChootAndSkultera =
    [
        new(EnemyDefinitionId.Owtch, row: 1), new(EnemyDefinitionId.Choot, row: 2), new(EnemyDefinitionId.Skultera, row: 3),
    ];

    /// <summary>The region's lists, in table order.</summary>
    internal static RoomEnemyGraphicsSetMembers[] Build() =>
    [
        new(RoomEnemyGraphicsSetDefinitions.MtEverest, [
            new(EnemyDefinitionId.Powamp, row: 1),
            new(EnemyDefinitionId.Sciser, row: 3),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.HalfieClimb, [
            new(EnemyDefinitionId.Oum, row: 1),
            new(EnemyDefinitionId.Mochtroid, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WateringHole, [
            new(EnemyDefinitionId.Choot, row: 1),
            new(EnemyDefinitionId.Zeb, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.CrabShaft, [
            new(EnemyDefinitionId.Sciser, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.MaridiaElevator, [
            new(EnemyDefinitionId.Ripper, row: 3),
            new(EnemyDefinitionId.Owtch, row: 7),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Colosseum, [
            new(EnemyDefinitionId.Mochtroid, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.AqueductSave, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.ThePrecious, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BotwoonEnergyTank, [
            new(EnemyDefinitionId.Puyo, row: 1),
            new(EnemyDefinitionId.Sciser, row: 1),
            new(EnemyDefinitionId.Zoa, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.DraygonSave, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.MaridiaMissileRefill, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.PlasmaBeachQuicksand, [
            new(EnemyDefinitionId.Bull, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BotwoonQuicksand, [
            new(EnemyDefinitionId.Bull, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Shaktool, [
            new(EnemyDefinitionId.Shaktool, row: 1),
            new(EnemyDefinitionId.Yard, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.DraygonDefault, [
            new(EnemyDefinitionId.DraygonBody, row: 7),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Aqueduct, [
            new(EnemyDefinitionId.Yard, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.RedFish, [
            new(EnemyDefinitionId.Skultera, row: 1),
            new(EnemyDefinitionId.Sciser, row: 1),
            new(EnemyDefinitionId.Zebbo, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Plasma, [
            new(EnemyDefinitionId.PirateMagentaWalking, row: 1),
            new(EnemyDefinitionId.PirateMagentaWall, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.SpaceJump, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Pants, [
            new(EnemyDefinitionId.MaridiaFly, row: 1),
            new(EnemyDefinitionId.Puyo, row: 2),
            new(EnemyDefinitionId.Tripper, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.GlassTunnelSave, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.GlassTunnel, [
            new(EnemyDefinitionId.NoobTubeCrack, row: 7),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WestTunnel, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.EastTunnel, [
            new(EnemyDefinitionId.Boyon, row: 1),
            new(EnemyDefinitionId.Zebbo, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.MamaTurtle, [
            new(EnemyDefinitionId.MamaTurtle, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.CrabTunnel, [
            new(EnemyDefinitionId.Sciser, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.NorthwestMaridiaBug, [
            new(EnemyDefinitionId.Owtch, row: 1),
            new(EnemyDefinitionId.MaridiaFly, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.PseudoPlasmaSpark, OwtchChootAndSkultera),
        new(RoomEnemyGraphicsSetDefinitions.WestSandHallTunnel, [
            new(EnemyDefinitionId.Sciser, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.PlasmaTutorial, [
            new(EnemyDefinitionId.Puyo, row: 1),
            new(EnemyDefinitionId.ArmoredZoomer, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.ThreadTheNeedle, [
            new(EnemyDefinitionId.Puyo, row: 1),
            new(EnemyDefinitionId.Boyon, row: 1),
            new(EnemyDefinitionId.Choot, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.PlasmaSpark, OwtchChootAndSkultera),
        new(RoomEnemyGraphicsSetDefinitions.Toilet, [
            new(EnemyDefinitionId.Mochtroid, row: 1),
            new(EnemyDefinitionId.Puyo, row: 2),
            new(EnemyDefinitionId.Sciser, row: 2),
            new(EnemyDefinitionId.Yard, row: 3),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WestSandHall, [
            new(EnemyDefinitionId.Evir, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Oasis, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.EastSandHall, [
            new(EnemyDefinitionId.Evir, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WestAqueductQuicksand, [
            new(EnemyDefinitionId.Bull, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.EastAqueductQuicksand, [
            new(EnemyDefinitionId.Bull, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Butterfly, [
            new(EnemyDefinitionId.Zoa, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BotwoonHallway, [
            new(EnemyDefinitionId.Mochtroid, row: 1),
            new(EnemyDefinitionId.Puyo, row: 2),
            new(EnemyDefinitionId.Sciser, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.EastPants, [
            new(EnemyDefinitionId.Puyo, row: 1),
            new(EnemyDefinitionId.Tripper, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Springball, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BelowBotwoonEnergyTank, [
            new(EnemyDefinitionId.Zoa, row: 1),
            new(EnemyDefinitionId.Owtch, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Kassiuz, [
            new(EnemyDefinitionId.Choot, row: 1),
            new(EnemyDefinitionId.Puyo, row: 2),
            new(EnemyDefinitionId.Boyon, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.ForgottenHighwaySave, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.MaridiaEnergyRefill, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.MaridiaMap, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.CrabHole, [
            new(EnemyDefinitionId.Sciser, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Botwoon, [
            new(EnemyDefinitionId.Owtch, row: 1),
            new(EnemyDefinitionId.Botwoon, row: 7),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.MainStreet, [
            new(EnemyDefinitionId.Skultera, row: 1),
            new(EnemyDefinitionId.Sciser, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BugSandHole, [
            new(EnemyDefinitionId.YappingMaw, row: 1),
            new(EnemyDefinitionId.Zoa, row: 7),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.EastSandHole, [
            new(EnemyDefinitionId.Boulder, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WestSandHole, [
            new(EnemyDefinitionId.Boulder, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WestCactusAlley, [
            new(EnemyDefinitionId.Cacatac, row: 7), new(EnemyDefinitionId.Zoa, row: 7),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.EastCactusAlley, [
            new(EnemyDefinitionId.Cacatac, row: 1), new(EnemyDefinitionId.Zoa, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.FishTank, [
            new(EnemyDefinitionId.Skultera, row: 1),
            new(EnemyDefinitionId.PirateMagentaWalking, row: 2),
        ]),
    ];
}
