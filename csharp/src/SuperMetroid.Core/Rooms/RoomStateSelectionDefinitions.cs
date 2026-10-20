using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms;

/// <summary>Direct ordered state-selection branches for every retail room.</summary>
/// <remarks>Native bank-$8F inline programs test named events, boss flags and
/// equipment predicates in order; the first successful condition wins. The 54
/// conditional programs are expressed as branches without stored clause arrays.
/// Unconditional rooms name their single state. Only the 262 retail rooms are
/// accepted.</remarks>
public static class RoomStateSelectionDefinitions
{

    /// <summary>One room's ordered state-selection branches: each <paramref name="match"/> call offers a
    /// candidate under its condition, and the program returns the first match or its default state.</summary>
    private delegate CartridgeRoomState RoomStateProgram(RoomStateSelectionContext selection,
        Func<bool, CartridgeRoomState, bool> match);

    /// <summary>Each retail room's state-selection program, in native room order.</summary>
    private static readonly Dictionary<RoomHeaderDefinition, RoomStateProgram> Programs = new()
    {
        [RoomHeaderDefinitions.LandingSite] = static (selection, match) =>
        {
            if (match(selection.IsEventSet((byte)EventNumber.ZebesTimebombSet), RoomStateDefinitions.LandingSite3)) return RoomStateDefinitions.LandingSite3;
            if (match(selection.HasPowerBombs, RoomStateDefinitions.LandingSite2)) return RoomStateDefinitions.LandingSite2;
            if (match(selection.IsEventSet((byte)EventNumber.ZebesAwake), RoomStateDefinitions.LandingSite1)) return RoomStateDefinitions.LandingSite1;
            return RoomStateDefinitions.LandingSite0;
        },
        [RoomHeaderDefinitions.GauntletEntrance] = static (selection, match) =>
        {
            if (match(selection.IsEventSet((byte)EventNumber.ZebesAwake), RoomStateDefinitions.GauntletEntrance1)) return RoomStateDefinitions.GauntletEntrance1;
            return RoomStateDefinitions.GauntletEntrance0;
        },
        [RoomHeaderDefinitions.Parlor] = static (selection, match) =>
        {
            if (match(selection.IsEventSet((byte)EventNumber.ZebesTimebombSet), RoomStateDefinitions.Parlor2)) return RoomStateDefinitions.Parlor2;
            if (match(selection.IsEventSet((byte)EventNumber.ZebesAwake), RoomStateDefinitions.Parlor1)) return RoomStateDefinitions.Parlor1;
            return RoomStateDefinitions.Parlor0;
        },
        [RoomHeaderDefinitions.Climb] = static (selection, match) =>
        {
            if (match(selection.IsEventSet((byte)EventNumber.ZebesTimebombSet), RoomStateDefinitions.Climb2)) return RoomStateDefinitions.Climb2;
            if (match(selection.IsEventSet((byte)EventNumber.ZebesAwake), RoomStateDefinitions.Climb1)) return RoomStateDefinitions.Climb1;
            return RoomStateDefinitions.Climb0;
        },
        [RoomHeaderDefinitions.Pit] = static (selection, match) =>
        {
            if (match(selection.HasMorphBallAndMissiles, RoomStateDefinitions.Pit1)) return RoomStateDefinitions.Pit1;
            return RoomStateDefinitions.Pit0;
        },
        [RoomHeaderDefinitions.ElevToBlueBrinstar] = static (selection, match) =>
        {
            if (match(selection.HasMorphBallAndMissiles, RoomStateDefinitions.ElevToBlueBrinstar1)) return RoomStateDefinitions.ElevToBlueBrinstar1;
            return RoomStateDefinitions.ElevToBlueBrinstar0;
        },
        [RoomHeaderDefinitions.BombTorizo] = static (selection, match) =>
        {
            if (match(selection.IsEventSet((byte)EventNumber.ZebesTimebombSet), RoomStateDefinitions.BombTorizo2)) return RoomStateDefinitions.BombTorizo2;
            if (match(selection.IsBossDead(BossBits.AreaTorizo), RoomStateDefinitions.BombTorizo1)) return RoomStateDefinitions.BombTorizo1;
            return RoomStateDefinitions.BombTorizo0;
        },
        [RoomHeaderDefinitions.Flyway] = static (selection, match) =>
        {
            if (match(selection.IsEventSet((byte)EventNumber.ZebesTimebombSet), RoomStateDefinitions.Flyway2)) return RoomStateDefinitions.Flyway2;
            if (match(selection.IsBossDead(BossBits.AreaTorizo), RoomStateDefinitions.Flyway1)) return RoomStateDefinitions.Flyway1;
            return RoomStateDefinitions.Flyway0;
        },
        [RoomHeaderDefinitions.FinalMissileBombway] = static (selection, match) =>
        {
            if (match(selection.IsEventSet((byte)EventNumber.ZebesAwake), RoomStateDefinitions.FinalMissileBombway1)) return RoomStateDefinitions.FinalMissileBombway1;
            return RoomStateDefinitions.FinalMissileBombway0;
        },
        [RoomHeaderDefinitions.FinalMissile] = static (selection, match) =>
        {
            if (match(selection.IsEventSet((byte)EventNumber.ZebesAwake), RoomStateDefinitions.FinalMissile1)) return RoomStateDefinitions.FinalMissile1;
            return RoomStateDefinitions.FinalMissile0;
        },
        [RoomHeaderDefinitions.SporeSpawn] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaMiniBoss), RoomStateDefinitions.SporeSpawn1)) return RoomStateDefinitions.SporeSpawn1;
            return RoomStateDefinitions.SporeSpawn0;
        },
        [RoomHeaderDefinitions.MorphBall] = static (selection, match) =>
        {
            if (match(selection.IsEventSet((byte)EventNumber.ZebesAwake), RoomStateDefinitions.MorphBall1)) return RoomStateDefinitions.MorphBall1;
            return RoomStateDefinitions.MorphBall0;
        },
        [RoomHeaderDefinitions.ConstructionZone] = static (selection, match) =>
        {
            if (match(selection.IsEventSet((byte)EventNumber.ZebesAwake), RoomStateDefinitions.ConstructionZone1)) return RoomStateDefinitions.ConstructionZone1;
            return RoomStateDefinitions.ConstructionZone0;
        },
        [RoomHeaderDefinitions.BlueBrinstarETank] = static (selection, match) =>
        {
            if (match(selection.IsEventSet((byte)EventNumber.ZebesAwake), RoomStateDefinitions.BlueBrinstarETank1)) return RoomStateDefinitions.BlueBrinstarETank1;
            return RoomStateDefinitions.BlueBrinstarETank0;
        },
        [RoomHeaderDefinitions.MiniKraid] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.MiniKraid1)) return RoomStateDefinitions.MiniKraid1;
            return RoomStateDefinitions.MiniKraid0;
        },
        [RoomHeaderDefinitions.Kraid] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.Kraid1)) return RoomStateDefinitions.Kraid1;
            return RoomStateDefinitions.Kraid0;
        },
        [RoomHeaderDefinitions.Crocomire] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaMiniBoss), RoomStateDefinitions.Crocomire1)) return RoomStateDefinitions.Crocomire1;
            return RoomStateDefinitions.Crocomire0;
        },
        [RoomHeaderDefinitions.GoldenTorizo] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaTorizo), RoomStateDefinitions.GoldenTorizo1)) return RoomStateDefinitions.GoldenTorizo1;
            return RoomStateDefinitions.GoldenTorizo0;
        },
        [RoomHeaderDefinitions.Ridley] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.Ridley1)) return RoomStateDefinitions.Ridley1;
            return RoomStateDefinitions.Ridley0;
        },
        [RoomHeaderDefinitions.BowlingAlley] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.BowlingAlley1)) return RoomStateDefinitions.BowlingAlley1;
            return RoomStateDefinitions.BowlingAlley0;
        },
        [RoomHeaderDefinitions.WreckedShipEntrance] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.WreckedShipEntrance1)) return RoomStateDefinitions.WreckedShipEntrance1;
            return RoomStateDefinitions.WreckedShipEntrance0;
        },
        [RoomHeaderDefinitions.Attic] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.Attic1)) return RoomStateDefinitions.Attic1;
            return RoomStateDefinitions.Attic0;
        },
        [RoomHeaderDefinitions.AssemblyLine] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.AssemblyLine1)) return RoomStateDefinitions.AssemblyLine1;
            return RoomStateDefinitions.AssemblyLine0;
        },
        [RoomHeaderDefinitions.WreckedShipMainShaft] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.WreckedShipMainShaft1)) return RoomStateDefinitions.WreckedShipMainShaft1;
            return RoomStateDefinitions.WreckedShipMainShaft0;
        },
        [RoomHeaderDefinitions.SpikyDeath] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.SpikyDeath1)) return RoomStateDefinitions.SpikyDeath1;
            return RoomStateDefinitions.SpikyDeath0;
        },
        [RoomHeaderDefinitions.ElectricDeath] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.ElectricDeath1)) return RoomStateDefinitions.ElectricDeath1;
            return RoomStateDefinitions.ElectricDeath0;
        },
        [RoomHeaderDefinitions.WreckedShipETank] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.WreckedShipETank1)) return RoomStateDefinitions.WreckedShipETank1;
            return RoomStateDefinitions.WreckedShipETank0;
        },
        [RoomHeaderDefinitions.Basement] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.Basement1)) return RoomStateDefinitions.Basement1;
            return RoomStateDefinitions.Basement0;
        },
        [RoomHeaderDefinitions.WreckedShipMap] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.WreckedShipMap1)) return RoomStateDefinitions.WreckedShipMap1;
            return RoomStateDefinitions.WreckedShipMap0;
        },
        [RoomHeaderDefinitions.Phantoon] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.Phantoon1)) return RoomStateDefinitions.Phantoon1;
            return RoomStateDefinitions.Phantoon0;
        },
        [RoomHeaderDefinitions.SpongeBath] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.SpongeBath1)) return RoomStateDefinitions.SpongeBath1;
            return RoomStateDefinitions.SpongeBath0;
        },
        [RoomHeaderDefinitions.WSWestSuper] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.WSWestSuper1)) return RoomStateDefinitions.WSWestSuper1;
            return RoomStateDefinitions.WSWestSuper0;
        },
        [RoomHeaderDefinitions.WSEastSuper] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.WSEastSuper1)) return RoomStateDefinitions.WSEastSuper1;
            return RoomStateDefinitions.WSEastSuper0;
        },
        [RoomHeaderDefinitions.GravitySuit] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.GravitySuit1)) return RoomStateDefinitions.GravitySuit1;
            return RoomStateDefinitions.GravitySuit0;
        },
        [RoomHeaderDefinitions.WreckedShipSave] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.WreckedShipSave1)) return RoomStateDefinitions.WreckedShipSave1;
            return RoomStateDefinitions.WreckedShipSave0;
        },
        [RoomHeaderDefinitions.GlassTunnel] = static (selection, match) =>
        {
            if (match(selection.IsEventSet((byte)EventNumber.MaridiaNoobTubeBroken), RoomStateDefinitions.GlassTunnel1)) return RoomStateDefinitions.GlassTunnel1;
            return RoomStateDefinitions.GlassTunnel0;
        },
        [RoomHeaderDefinitions.ThePrecious] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.ThePrecious1)) return RoomStateDefinitions.ThePrecious1;
            return RoomStateDefinitions.ThePrecious0;
        },
        [RoomHeaderDefinitions.Shaktool] = static (selection, match) =>
        {
            if (match(selection.IsEventSet((byte)EventNumber.ShaktoolClearedPath), RoomStateDefinitions.Shaktool1)) return RoomStateDefinitions.Shaktool1;
            return RoomStateDefinitions.Shaktool0;
        },
        [RoomHeaderDefinitions.Botwoon] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaMiniBoss), RoomStateDefinitions.Botwoon1)) return RoomStateDefinitions.Botwoon1;
            return RoomStateDefinitions.Botwoon0;
        },
        [RoomHeaderDefinitions.Draygon] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.Draygon1)) return RoomStateDefinitions.Draygon1;
            return RoomStateDefinitions.Draygon0;
        },
        [RoomHeaderDefinitions.Metroids1] = static (selection, match) =>
        {
            if (match(selection.IsEventSet((byte)EventNumber.FirstMetroidHallCleared), RoomStateDefinitions.Metroids11)) return RoomStateDefinitions.Metroids11;
            return RoomStateDefinitions.Metroids10;
        },
        [RoomHeaderDefinitions.Metroids2] = static (selection, match) =>
        {
            if (match(selection.IsEventSet((byte)EventNumber.FirstMetroidShaftCleared), RoomStateDefinitions.Metroids21)) return RoomStateDefinitions.Metroids21;
            return RoomStateDefinitions.Metroids20;
        },
        [RoomHeaderDefinitions.Metroids3] = static (selection, match) =>
        {
            if (match(selection.IsEventSet((byte)EventNumber.SecondMetroidHallCleared), RoomStateDefinitions.Metroids31)) return RoomStateDefinitions.Metroids31;
            return RoomStateDefinitions.Metroids30;
        },
        [RoomHeaderDefinitions.Metroids4] = static (selection, match) =>
        {
            if (match(selection.IsEventSet((byte)EventNumber.SecondMetroidShaftCleared), RoomStateDefinitions.Metroids41)) return RoomStateDefinitions.Metroids41;
            return RoomStateDefinitions.Metroids40;
        },
        [RoomHeaderDefinitions.BlueHopper] = static (selection, match) =>
        {
            if (match(selection.IsEventSet((byte)EventNumber.Unused14), RoomStateDefinitions.BlueHopper1)) return RoomStateDefinitions.BlueHopper1;
            return RoomStateDefinitions.BlueHopper0;
        },
        [RoomHeaderDefinitions.DustTorizo] = static (selection, match) =>
        {
            if (match(selection.IsEventSet((byte)EventNumber.Unused14), RoomStateDefinitions.DustTorizo1)) return RoomStateDefinitions.DustTorizo1;
            return RoomStateDefinitions.DustTorizo0;
        },
        [RoomHeaderDefinitions.BigBoy] = static (selection, match) =>
        {
            if (match(selection.IsEventSet((byte)EventNumber.Unused14), RoomStateDefinitions.BigBoy1)) return RoomStateDefinitions.BigBoy1;
            return RoomStateDefinitions.BigBoy0;
        },
        [RoomHeaderDefinitions.MotherBrain] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(RoomStateSelectorOperands.MainAreaBoss), RoomStateDefinitions.MotherBrain2)) return RoomStateDefinitions.MotherBrain2;
            if (match(selection.IsEventSet((byte)EventNumber.MotherBrainGlassDestroyed), RoomStateDefinitions.MotherBrain1)) return RoomStateDefinitions.MotherBrain1;
            return RoomStateDefinitions.MotherBrain0;
        },
        [RoomHeaderDefinitions.CeresElev] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.CeresElev1)) return RoomStateDefinitions.CeresElev1;
            return RoomStateDefinitions.CeresElev0;
        },
        [RoomHeaderDefinitions.FallingTile] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.FallingTile1)) return RoomStateDefinitions.FallingTile1;
            return RoomStateDefinitions.FallingTile0;
        },
        [RoomHeaderDefinitions.MagnetStairs] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.MagnetStairs1)) return RoomStateDefinitions.MagnetStairs1;
            return RoomStateDefinitions.MagnetStairs0;
        },
        [RoomHeaderDefinitions.DeadScientist] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.DeadScientist1)) return RoomStateDefinitions.DeadScientist1;
            return RoomStateDefinitions.DeadScientist0;
        },
        [RoomHeaderDefinitions.Room58Escape] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.Room58Escape1)) return RoomStateDefinitions.Room58Escape1;
            return RoomStateDefinitions.Room58Escape0;
        },
        [RoomHeaderDefinitions.CeresRidley] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.CeresRidley1)) return RoomStateDefinitions.CeresRidley1;
            return RoomStateDefinitions.CeresRidley0;
        },
        [RoomHeaderDefinitions.CrateriaPowerBombs] = static (_, _) => RoomStateDefinitions.CrateriaPowerBombs,
        [RoomHeaderDefinitions.CrateriaSave] = static (_, _) => RoomStateDefinitions.CrateriaSave,
        [RoomHeaderDefinitions.WestOcean] = static (_, _) => RoomStateDefinitions.WestOcean,
        [RoomHeaderDefinitions.BowlingAlleyPath] = static (_, _) => RoomStateDefinitions.BowlingAlleyPath,
        [RoomHeaderDefinitions.CrateriaKihunter] = static (_, _) => RoomStateDefinitions.CrateriaKihunter,
        [RoomHeaderDefinitions.ForgottenHighwayElev] = static (_, _) => RoomStateDefinitions.ForgottenHighwayElev,
        [RoomHeaderDefinitions.EastOcean] = static (_, _) => RoomStateDefinitions.EastOcean,
        [RoomHeaderDefinitions.ForgottenHighwayKagos] = static (_, _) => RoomStateDefinitions.ForgottenHighwayKagos,
        [RoomHeaderDefinitions.CrabMaze] = static (_, _) => RoomStateDefinitions.CrabMaze,
        [RoomHeaderDefinitions.ForgottenHighwayElbow] = static (_, _) => RoomStateDefinitions.ForgottenHighwayElbow,
        [RoomHeaderDefinitions.CrateriaTube] = static (_, _) => RoomStateDefinitions.CrateriaTube,
        [RoomHeaderDefinitions.Moat] = static (_, _) => RoomStateDefinitions.Moat,
        [RoomHeaderDefinitions.RedBinstarElev] = static (_, _) => RoomStateDefinitions.RedBinstarElev,
        [RoomHeaderDefinitions.GauntletETank] = static (_, _) => RoomStateDefinitions.GauntletETank,
        [RoomHeaderDefinitions.PreBowling] = static (_, _) => RoomStateDefinitions.PreBowling,
        [RoomHeaderDefinitions.PreMapFlyway] = static (_, _) => RoomStateDefinitions.PreMapFlyway,
        [RoomHeaderDefinitions.Terminator] = static (_, _) => RoomStateDefinitions.Terminator,
        [RoomHeaderDefinitions.GreenBrinstarElev] = static (_, _) => RoomStateDefinitions.GreenBrinstarElev,
        [RoomHeaderDefinitions.LowerMushrooms] = static (_, _) => RoomStateDefinitions.LowerMushrooms,
        [RoomHeaderDefinitions.CrateriaMap] = static (_, _) => RoomStateDefinitions.CrateriaMap,
        [RoomHeaderDefinitions.GreenPiratesShaft] = static (_, _) => RoomStateDefinitions.GreenPiratesShaft,
        [RoomHeaderDefinitions.CrateriaSuper] = static (_, _) => RoomStateDefinitions.CrateriaSuper,
        [RoomHeaderDefinitions.GreenBrinstarMainShaft] = static (_, _) => RoomStateDefinitions.GreenBrinstarMainShaft,
        [RoomHeaderDefinitions.SporeSpawnSuper] = static (_, _) => RoomStateDefinitions.SporeSpawnSuper,
        [RoomHeaderDefinitions.BrinstarPreMap] = static (_, _) => RoomStateDefinitions.BrinstarPreMap,
        [RoomHeaderDefinitions.EarlySupers] = static (_, _) => RoomStateDefinitions.EarlySupers,
        [RoomHeaderDefinitions.BrinstarReserveTank] = static (_, _) => RoomStateDefinitions.BrinstarReserveTank,
        [RoomHeaderDefinitions.GreenBrinstarMap] = static (_, _) => RoomStateDefinitions.GreenBrinstarMap,
        [RoomHeaderDefinitions.GreenBrinstarFirefleas] = static (_, _) => RoomStateDefinitions.GreenBrinstarFirefleas,
        [RoomHeaderDefinitions.GreenBrinstarMissileRefill] = static (_, _) => RoomStateDefinitions.GreenBrinstarMissileRefill,
        [RoomHeaderDefinitions.Dachora] = static (_, _) => RoomStateDefinitions.Dachora,
        [RoomHeaderDefinitions.BigPink] = static (_, _) => RoomStateDefinitions.BigPink,
        [RoomHeaderDefinitions.SporeSpawnKihunters] = static (_, _) => RoomStateDefinitions.SporeSpawnKihunters,
        [RoomHeaderDefinitions.PinkBrinstarPowerBombs] = static (_, _) => RoomStateDefinitions.PinkBrinstarPowerBombs,
        [RoomHeaderDefinitions.GreenHillZone] = static (_, _) => RoomStateDefinitions.GreenHillZone,
        [RoomHeaderDefinitions.NoobBridge] = static (_, _) => RoomStateDefinitions.NoobBridge,
        [RoomHeaderDefinitions.GreenBrinstarBeetoms] = static (_, _) => RoomStateDefinitions.GreenBrinstarBeetoms,
        [RoomHeaderDefinitions.EtecoonETank] = static (_, _) => RoomStateDefinitions.EtecoonETank,
        [RoomHeaderDefinitions.EtecoonSuper] = static (_, _) => RoomStateDefinitions.EtecoonSuper,
        [RoomHeaderDefinitions.DachoraEnergyRefill] = static (_, _) => RoomStateDefinitions.DachoraEnergyRefill,
        [RoomHeaderDefinitions.SporeSpawnFarming] = static (_, _) => RoomStateDefinitions.SporeSpawnFarming,
        [RoomHeaderDefinitions.WaterwayETank] = static (_, _) => RoomStateDefinitions.WaterwayETank,
        [RoomHeaderDefinitions.FirstMissile] = static (_, _) => RoomStateDefinitions.FirstMissile,
        [RoomHeaderDefinitions.PinkBrinstarHoppers] = static (_, _) => RoomStateDefinitions.PinkBrinstarHoppers,
        [RoomHeaderDefinitions.HopperETank] = static (_, _) => RoomStateDefinitions.HopperETank,
        [RoomHeaderDefinitions.BigPinkSaveRoom] = static (_, _) => RoomStateDefinitions.BigPinkSaveRoom,
        [RoomHeaderDefinitions.BlueBrinstarBoulders] = static (_, _) => RoomStateDefinitions.BlueBrinstarBoulders,
        [RoomHeaderDefinitions.BillyMays] = static (_, _) => RoomStateDefinitions.BillyMays,
        [RoomHeaderDefinitions.GreenBrinstarSave] = static (_, _) => RoomStateDefinitions.GreenBrinstarSave,
        [RoomHeaderDefinitions.EtecoonSave] = static (_, _) => RoomStateDefinitions.EtecoonSave,
        [RoomHeaderDefinitions.RedTower] = static (_, _) => RoomStateDefinitions.RedTower,
        [RoomHeaderDefinitions.RedBrinstarFirefleas] = static (_, _) => RoomStateDefinitions.RedBrinstarFirefleas,
        [RoomHeaderDefinitions.XrayScope] = static (_, _) => RoomStateDefinitions.XrayScope,
        [RoomHeaderDefinitions.Hellway] = static (_, _) => RoomStateDefinitions.Hellway,
        [RoomHeaderDefinitions.Caterpillar] = static (_, _) => RoomStateDefinitions.Caterpillar,
        [RoomHeaderDefinitions.BetaPowerBomb] = static (_, _) => RoomStateDefinitions.BetaPowerBomb,
        [RoomHeaderDefinitions.AlphaPowerBomb] = static (_, _) => RoomStateDefinitions.AlphaPowerBomb,
        [RoomHeaderDefinitions.SkreeBoost] = static (_, _) => RoomStateDefinitions.SkreeBoost,
        [RoomHeaderDefinitions.BelowSpazer] = static (_, _) => RoomStateDefinitions.BelowSpazer,
        [RoomHeaderDefinitions.Spazer] = static (_, _) => RoomStateDefinitions.Spazer,
        [RoomHeaderDefinitions.WarehouseZeela] = static (_, _) => RoomStateDefinitions.WarehouseZeela,
        [RoomHeaderDefinitions.WarehouseETank] = static (_, _) => RoomStateDefinitions.WarehouseETank,
        [RoomHeaderDefinitions.WarehouseKihunter] = static (_, _) => RoomStateDefinitions.WarehouseKihunter,
        [RoomHeaderDefinitions.KraidEyeDoor] = static (_, _) => RoomStateDefinitions.KraidEyeDoor,
        [RoomHeaderDefinitions.StatuesHallway] = static (_, _) => RoomStateDefinitions.StatuesHallway,
        [RoomHeaderDefinitions.RedTowerRefill] = static (_, _) => RoomStateDefinitions.RedTowerRefill,
        [RoomHeaderDefinitions.KraidRefill] = static (_, _) => RoomStateDefinitions.KraidRefill,
        [RoomHeaderDefinitions.Statues] = static (_, _) => RoomStateDefinitions.Statues,
        [RoomHeaderDefinitions.WarehouseEntrance] = static (_, _) => RoomStateDefinitions.WarehouseEntrance,
        [RoomHeaderDefinitions.VariaSuit] = static (_, _) => RoomStateDefinitions.VariaSuit,
        [RoomHeaderDefinitions.WarehouseSave] = static (_, _) => RoomStateDefinitions.WarehouseSave,
        [RoomHeaderDefinitions.RedBrinstarSave] = static (_, _) => RoomStateDefinitions.RedBrinstarSave,
        [RoomHeaderDefinitions.IceBeamAcid] = static (_, _) => RoomStateDefinitions.IceBeamAcid,
        [RoomHeaderDefinitions.Cathedral] = static (_, _) => RoomStateDefinitions.Cathedral,
        [RoomHeaderDefinitions.CathedralEntrance] = static (_, _) => RoomStateDefinitions.CathedralEntrance,
        [RoomHeaderDefinitions.BusinessCenter] = static (_, _) => RoomStateDefinitions.BusinessCenter,
        [RoomHeaderDefinitions.IceBeamGate] = static (_, _) => RoomStateDefinitions.IceBeamGate,
        [RoomHeaderDefinitions.IceBeamTutorial] = static (_, _) => RoomStateDefinitions.IceBeamTutorial,
        [RoomHeaderDefinitions.IceBeam] = static (_, _) => RoomStateDefinitions.IceBeam,
        [RoomHeaderDefinitions.IceBeamSnake] = static (_, _) => RoomStateDefinitions.IceBeamSnake,
        [RoomHeaderDefinitions.CrumbleShaft] = static (_, _) => RoomStateDefinitions.CrumbleShaft,
        [RoomHeaderDefinitions.CrocomireSpeedway] = static (_, _) => RoomStateDefinitions.CrocomireSpeedway,
        [RoomHeaderDefinitions.HiJumpBoots] = static (_, _) => RoomStateDefinitions.HiJumpBoots,
        [RoomHeaderDefinitions.CrocomireEscape] = static (_, _) => RoomStateDefinitions.CrocomireEscape,
        [RoomHeaderDefinitions.HiJumpETank] = static (_, _) => RoomStateDefinitions.HiJumpETank,
        [RoomHeaderDefinitions.PostCrocFarming] = static (_, _) => RoomStateDefinitions.PostCrocFarming,
        [RoomHeaderDefinitions.PostCrocSave] = static (_, _) => RoomStateDefinitions.PostCrocSave,
        [RoomHeaderDefinitions.PostCrocPowerBombs] = static (_, _) => RoomStateDefinitions.PostCrocPowerBombs,
        [RoomHeaderDefinitions.PostCrocShaft] = static (_, _) => RoomStateDefinitions.PostCrocShaft,
        [RoomHeaderDefinitions.PostCrocMissile] = static (_, _) => RoomStateDefinitions.PostCrocMissile,
        [RoomHeaderDefinitions.GrappleTutorial3] = static (_, _) => RoomStateDefinitions.GrappleTutorial3,
        [RoomHeaderDefinitions.PostCrocJump] = static (_, _) => RoomStateDefinitions.PostCrocJump,
        [RoomHeaderDefinitions.GrappleTutorial2] = static (_, _) => RoomStateDefinitions.GrappleTutorial2,
        [RoomHeaderDefinitions.GrappleTutorial1] = static (_, _) => RoomStateDefinitions.GrappleTutorial1,
        [RoomHeaderDefinitions.GrappleBeam] = static (_, _) => RoomStateDefinitions.GrappleBeam,
        [RoomHeaderDefinitions.NorfairReserveTank] = static (_, _) => RoomStateDefinitions.NorfairReserveTank,
        [RoomHeaderDefinitions.GreenBubblesMissiles] = static (_, _) => RoomStateDefinitions.GreenBubblesMissiles,
        [RoomHeaderDefinitions.BubbleMountain] = static (_, _) => RoomStateDefinitions.BubbleMountain,
        [RoomHeaderDefinitions.SpeedBoosterHall] = static (_, _) => RoomStateDefinitions.SpeedBoosterHall,
        [RoomHeaderDefinitions.SpeedBooster] = static (_, _) => RoomStateDefinitions.SpeedBooster,
        [RoomHeaderDefinitions.SingleChamber] = static (_, _) => RoomStateDefinitions.SingleChamber,
        [RoomHeaderDefinitions.DoubleChamber] = static (_, _) => RoomStateDefinitions.DoubleChamber,
        [RoomHeaderDefinitions.WaveBeam] = static (_, _) => RoomStateDefinitions.WaveBeam,
        [RoomHeaderDefinitions.SpikyPlatformsTunnel] = static (_, _) => RoomStateDefinitions.SpikyPlatformsTunnel,
        [RoomHeaderDefinitions.Volcano] = static (_, _) => RoomStateDefinitions.Volcano,
        [RoomHeaderDefinitions.KronicBoost] = static (_, _) => RoomStateDefinitions.KronicBoost,
        [RoomHeaderDefinitions.MagdolliteTunnel] = static (_, _) => RoomStateDefinitions.MagdolliteTunnel,
        [RoomHeaderDefinitions.PurpleShaft] = static (_, _) => RoomStateDefinitions.PurpleShaft,
        [RoomHeaderDefinitions.LavaDive] = static (_, _) => RoomStateDefinitions.LavaDive,
        [RoomHeaderDefinitions.LowerNorfairElev] = static (_, _) => RoomStateDefinitions.LowerNorfairElev,
        [RoomHeaderDefinitions.UpperNorfairFarming] = static (_, _) => RoomStateDefinitions.UpperNorfairFarming,
        [RoomHeaderDefinitions.RisingTide] = static (_, _) => RoomStateDefinitions.RisingTide,
        [RoomHeaderDefinitions.AcidSnakesTunnel] = static (_, _) => RoomStateDefinitions.AcidSnakesTunnel,
        [RoomHeaderDefinitions.SpikyAcidSnakesTunnel] = static (_, _) => RoomStateDefinitions.SpikyAcidSnakesTunnel,
        [RoomHeaderDefinitions.UpperNorfairRefill] = static (_, _) => RoomStateDefinitions.UpperNorfairRefill,
        [RoomHeaderDefinitions.PurpleFarming] = static (_, _) => RoomStateDefinitions.PurpleFarming,
        [RoomHeaderDefinitions.BatCave] = static (_, _) => RoomStateDefinitions.BatCave,
        [RoomHeaderDefinitions.NorfairMap] = static (_, _) => RoomStateDefinitions.NorfairMap,
        [RoomHeaderDefinitions.BubbleMountainSave] = static (_, _) => RoomStateDefinitions.BubbleMountainSave,
        [RoomHeaderDefinitions.FrogSpeedway] = static (_, _) => RoomStateDefinitions.FrogSpeedway,
        [RoomHeaderDefinitions.RedPirateShaft] = static (_, _) => RoomStateDefinitions.RedPirateShaft,
        [RoomHeaderDefinitions.FrogSave] = static (_, _) => RoomStateDefinitions.FrogSave,
        [RoomHeaderDefinitions.CrocomireSave] = static (_, _) => RoomStateDefinitions.CrocomireSave,
        [RoomHeaderDefinitions.LNElevSave] = static (_, _) => RoomStateDefinitions.LNElevSave,
        [RoomHeaderDefinitions.AcidStatue] = static (_, _) => RoomStateDefinitions.AcidStatue,
        [RoomHeaderDefinitions.MainHall] = static (_, _) => RoomStateDefinitions.MainHall,
        [RoomHeaderDefinitions.FastRipper] = static (_, _) => RoomStateDefinitions.FastRipper,
        [RoomHeaderDefinitions.GTEnergyRefill] = static (_, _) => RoomStateDefinitions.GTEnergyRefill,
        [RoomHeaderDefinitions.LNFarming] = static (_, _) => RoomStateDefinitions.LNFarming,
        [RoomHeaderDefinitions.FastPillarsSetup] = static (_, _) => RoomStateDefinitions.FastPillarsSetup,
        [RoomHeaderDefinitions.UnusedRoom8FB3E1] = static (_, _) => RoomStateDefinitions.UnusedRoom8FB3EE,
        [RoomHeaderDefinitions.MickeyMouse] = static (_, _) => RoomStateDefinitions.MickeyMouse,
        [RoomHeaderDefinitions.Pillar] = static (_, _) => RoomStateDefinitions.Pillar,
        [RoomHeaderDefinitions.Plowerhouse] = static (_, _) => RoomStateDefinitions.Plowerhouse,
        [RoomHeaderDefinitions.WorstRoomInTheGame] = static (_, _) => RoomStateDefinitions.WorstRoomInTheGame,
        [RoomHeaderDefinitions.Amphitheatre] = static (_, _) => RoomStateDefinitions.Amphitheatre,
        [RoomHeaderDefinitions.LNSpringBallMaze] = static (_, _) => RoomStateDefinitions.LNSpringBallMaze,
        [RoomHeaderDefinitions.LNEscapePowerBombs] = static (_, _) => RoomStateDefinitions.LNEscapePowerBombs,
        [RoomHeaderDefinitions.RedKihunterShaft] = static (_, _) => RoomStateDefinitions.RedKihunterShaft,
        [RoomHeaderDefinitions.Wasteland] = static (_, _) => RoomStateDefinitions.Wasteland,
        [RoomHeaderDefinitions.MetalPirates] = static (_, _) => RoomStateDefinitions.MetalPirates,
        [RoomHeaderDefinitions.ThreeMusketeers] = static (_, _) => RoomStateDefinitions.ThreeMusketeers,
        [RoomHeaderDefinitions.RidleyETank] = static (_, _) => RoomStateDefinitions.RidleyETank,
        [RoomHeaderDefinitions.ScrewAttack] = static (_, _) => RoomStateDefinitions.ScrewAttack,
        [RoomHeaderDefinitions.LNFireflea] = static (_, _) => RoomStateDefinitions.LNFireflea,
        [RoomHeaderDefinitions.LNSave] = static (_, _) => RoomStateDefinitions.LNSave,
        [RoomHeaderDefinitions.GlassTunnelSave] = static (_, _) => RoomStateDefinitions.GlassTunnelSave,
        [RoomHeaderDefinitions.WestTunnel] = static (_, _) => RoomStateDefinitions.WestTunnel,
        [RoomHeaderDefinitions.EastTunnel] = static (_, _) => RoomStateDefinitions.EastTunnel,
        [RoomHeaderDefinitions.MainStreet] = static (_, _) => RoomStateDefinitions.MainStreet,
        [RoomHeaderDefinitions.FishTank] = static (_, _) => RoomStateDefinitions.FishTank,
        [RoomHeaderDefinitions.MamaTurtle] = static (_, _) => RoomStateDefinitions.MamaTurtle,
        [RoomHeaderDefinitions.CrabTunnel] = static (_, _) => RoomStateDefinitions.CrabTunnel,
        [RoomHeaderDefinitions.MtEverest] = static (_, _) => RoomStateDefinitions.MtEverest,
        [RoomHeaderDefinitions.RedFish] = static (_, _) => RoomStateDefinitions.RedFish,
        [RoomHeaderDefinitions.WateringHole] = static (_, _) => RoomStateDefinitions.WateringHole,
        [RoomHeaderDefinitions.NWestMaridiaBug] = static (_, _) => RoomStateDefinitions.NWestMaridiaBug,
        [RoomHeaderDefinitions.CrabShaft] = static (_, _) => RoomStateDefinitions.CrabShaft,
        [RoomHeaderDefinitions.PseudoPlasmaSpark] = static (_, _) => RoomStateDefinitions.PseudoPlasmaSpark,
        [RoomHeaderDefinitions.CrabHole] = static (_, _) => RoomStateDefinitions.CrabHole,
        [RoomHeaderDefinitions.WestSandHallTunnel] = static (_, _) => RoomStateDefinitions.WestSandHallTunnel,
        [RoomHeaderDefinitions.PlasmaTutorial] = static (_, _) => RoomStateDefinitions.PlasmaTutorial,
        [RoomHeaderDefinitions.Plasma] = static (_, _) => RoomStateDefinitions.Plasma,
        [RoomHeaderDefinitions.ThreadTheNeedle] = static (_, _) => RoomStateDefinitions.ThreadTheNeedle,
        [RoomHeaderDefinitions.MaridiaElev] = static (_, _) => RoomStateDefinitions.MaridiaElev,
        [RoomHeaderDefinitions.PlasmaSpark] = static (_, _) => RoomStateDefinitions.PlasmaSpark,
        [RoomHeaderDefinitions.Kassiuz] = static (_, _) => RoomStateDefinitions.Kassiuz,
        [RoomHeaderDefinitions.MaridiaMap] = static (_, _) => RoomStateDefinitions.MaridiaMap,
        [RoomHeaderDefinitions.ForgottenHighwaySave] = static (_, _) => RoomStateDefinitions.ForgottenHighwaySave,
        [RoomHeaderDefinitions.Toilet] = static (_, _) => RoomStateDefinitions.Toilet,
        [RoomHeaderDefinitions.BugSandHole] = static (_, _) => RoomStateDefinitions.BugSandHole,
        [RoomHeaderDefinitions.WestSandHall] = static (_, _) => RoomStateDefinitions.WestSandHall,
        [RoomHeaderDefinitions.Oasis] = static (_, _) => RoomStateDefinitions.Oasis,
        [RoomHeaderDefinitions.EastSandHall] = static (_, _) => RoomStateDefinitions.EastSandHall,
        [RoomHeaderDefinitions.WestSandHole] = static (_, _) => RoomStateDefinitions.WestSandHole,
        [RoomHeaderDefinitions.EastSandHole] = static (_, _) => RoomStateDefinitions.EastSandHole,
        [RoomHeaderDefinitions.WestAqueductQuicksand] = static (_, _) => RoomStateDefinitions.WestAqueductQuicksand,
        [RoomHeaderDefinitions.EastAqueductQuicksand] = static (_, _) => RoomStateDefinitions.EastAqueductQuicksand,
        [RoomHeaderDefinitions.Aqueduct] = static (_, _) => RoomStateDefinitions.Aqueduct,
        [RoomHeaderDefinitions.Butterfly] = static (_, _) => RoomStateDefinitions.Butterfly,
        [RoomHeaderDefinitions.BotwoonHallway] = static (_, _) => RoomStateDefinitions.BotwoonHallway,
        [RoomHeaderDefinitions.Pants] = static (_, _) => RoomStateDefinitions.Pants,
        [RoomHeaderDefinitions.EastPants] = static (_, _) => RoomStateDefinitions.EastPants,
        [RoomHeaderDefinitions.Springball] = static (_, _) => RoomStateDefinitions.Springball,
        [RoomHeaderDefinitions.BelowBotwoonETank] = static (_, _) => RoomStateDefinitions.BelowBotwoonETank,
        [RoomHeaderDefinitions.Colosseum] = static (_, _) => RoomStateDefinitions.Colosseum,
        [RoomHeaderDefinitions.AqueductSave] = static (_, _) => RoomStateDefinitions.AqueductSave,
        [RoomHeaderDefinitions.BotwoonETank] = static (_, _) => RoomStateDefinitions.BotwoonETank,
        [RoomHeaderDefinitions.DraygonSave] = static (_, _) => RoomStateDefinitions.DraygonSave,
        [RoomHeaderDefinitions.MaridiaMissileRefill] = static (_, _) => RoomStateDefinitions.MaridiaMissileRefill,
        [RoomHeaderDefinitions.PlasmaBeachQuicksand] = static (_, _) => RoomStateDefinitions.PlasmaBeachQuicksand,
        [RoomHeaderDefinitions.BotwoonQuicksand] = static (_, _) => RoomStateDefinitions.BotwoonQuicksand,
        [RoomHeaderDefinitions.HalfieClimb] = static (_, _) => RoomStateDefinitions.HalfieClimb,
        [RoomHeaderDefinitions.SpaceJump] = static (_, _) => RoomStateDefinitions.SpaceJump,
        [RoomHeaderDefinitions.MaridiaEnergyRefill] = static (_, _) => RoomStateDefinitions.MaridiaEnergyRefill,
        [RoomHeaderDefinitions.WestCactusAlley] = static (_, _) => RoomStateDefinitions.WestCactusAlley,
        [RoomHeaderDefinitions.EastCactusAlley] = static (_, _) => RoomStateDefinitions.EastCactusAlley,
        [RoomHeaderDefinitions.TourianFirst] = static (_, _) => RoomStateDefinitions.TourianFirst,
        [RoomHeaderDefinitions.Seaweed] = static (_, _) => RoomStateDefinitions.Seaweed,
        [RoomHeaderDefinitions.TourianRecharge] = static (_, _) => RoomStateDefinitions.TourianRecharge,
        [RoomHeaderDefinitions.TourianEyeDoor] = static (_, _) => RoomStateDefinitions.TourianEyeDoor,
        [RoomHeaderDefinitions.RinkaShaft] = static (_, _) => RoomStateDefinitions.RinkaShaft,
        [RoomHeaderDefinitions.MotherBrainSave] = static (_, _) => RoomStateDefinitions.MotherBrainSave,
        [RoomHeaderDefinitions.TourianEscape1] = static (_, _) => RoomStateDefinitions.TourianEscape1,
        [RoomHeaderDefinitions.TourianEscape2] = static (_, _) => RoomStateDefinitions.TourianEscape2,
        [RoomHeaderDefinitions.TourianEscape3] = static (_, _) => RoomStateDefinitions.TourianEscape3,
        [RoomHeaderDefinitions.TourianEscape4] = static (_, _) => RoomStateDefinitions.TourianEscape4,
        [RoomHeaderDefinitions.UpperTourianSave] = static (_, _) => RoomStateDefinitions.UpperTourianSave,
    };

    /// <summary>Selects a state with native first-match precedence.</summary>
    public static CartridgeRoomState Select(RoomHeaderDefinition room, RoomStateSelectionContext selection) =>
        Resolve(Ensure.NotNull(room), selection, null);

    /// <summary>Returns the default first, then every alternative in native order.
    /// The caller-owned view is constructed on demand from the same branches;
    /// it is never cached as a second program table.</summary>
    public static IReadOnlyList<CartridgeRoomState> GetStates(RoomHeaderDefinition room)
    {
        Ensure.NotNull(room);
        var variants = new List<CartridgeRoomState>();
        CartridgeRoomState fallback = Resolve(room, default, variants);
        variants.Insert(0, fallback);
        return variants.AsReadOnly();
    }

    /// <summary>Evaluates each room's native conditional branches in order, or records their alternatives before the default state.</summary>
    private static CartridgeRoomState Resolve(RoomHeaderDefinition room, RoomStateSelectionContext selection, List<CartridgeRoomState>? variants)
    {
        bool Match(bool condition, CartridgeRoomState candidate)
        {
            variants?.Add(candidate);
            // Enumeration records every branch and reaches the default. Runtime
            // selection returns immediately on its first satisfied condition.
            return variants is null && condition;
        }
        return Programs.TryGetValue(room, out RoomStateProgram? program) ? program(selection, Match)
            : throw new ArgumentOutOfRangeException(nameof(room), room.Name,
                "Room is not one of the 262 compiled retail rooms.");
    }
}
