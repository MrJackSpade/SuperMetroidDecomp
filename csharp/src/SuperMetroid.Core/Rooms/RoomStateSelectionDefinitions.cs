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
            if (match(selection.IsEventSet((byte)EventNumber.ZebesTimebombSet), CrateriaRoomStateDefinitions.LandingSite3)) return CrateriaRoomStateDefinitions.LandingSite3;
            if (match(selection.HasPowerBombs, CrateriaRoomStateDefinitions.LandingSite2)) return CrateriaRoomStateDefinitions.LandingSite2;
            if (match(selection.IsEventSet((byte)EventNumber.ZebesAwake), CrateriaRoomStateDefinitions.LandingSite1)) return CrateriaRoomStateDefinitions.LandingSite1;
            return CrateriaRoomStateDefinitions.LandingSite0;
        },
        [RoomHeaderDefinitions.GauntletEntrance] = static (selection, match) =>
        {
            if (match(selection.IsEventSet((byte)EventNumber.ZebesAwake), CrateriaRoomStateDefinitions.GauntletEntrance1)) return CrateriaRoomStateDefinitions.GauntletEntrance1;
            return CrateriaRoomStateDefinitions.GauntletEntrance0;
        },
        [RoomHeaderDefinitions.Parlor] = static (selection, match) =>
        {
            if (match(selection.IsEventSet((byte)EventNumber.ZebesTimebombSet), CrateriaRoomStateDefinitions.Parlor2)) return CrateriaRoomStateDefinitions.Parlor2;
            if (match(selection.IsEventSet((byte)EventNumber.ZebesAwake), CrateriaRoomStateDefinitions.Parlor1)) return CrateriaRoomStateDefinitions.Parlor1;
            return CrateriaRoomStateDefinitions.Parlor0;
        },
        [RoomHeaderDefinitions.Climb] = static (selection, match) =>
        {
            if (match(selection.IsEventSet((byte)EventNumber.ZebesTimebombSet), CrateriaRoomStateDefinitions.Climb2)) return CrateriaRoomStateDefinitions.Climb2;
            if (match(selection.IsEventSet((byte)EventNumber.ZebesAwake), CrateriaRoomStateDefinitions.Climb1)) return CrateriaRoomStateDefinitions.Climb1;
            return CrateriaRoomStateDefinitions.Climb0;
        },
        [RoomHeaderDefinitions.Pit] = static (selection, match) =>
        {
            if (match(selection.HasMorphBallAndMissiles, CrateriaRoomStateDefinitions.Pit1)) return CrateriaRoomStateDefinitions.Pit1;
            return CrateriaRoomStateDefinitions.Pit0;
        },
        [RoomHeaderDefinitions.ElevToBlueBrinstar] = static (selection, match) =>
        {
            if (match(selection.HasMorphBallAndMissiles, CrateriaRoomStateDefinitions.ElevToBlueBrinstar1)) return CrateriaRoomStateDefinitions.ElevToBlueBrinstar1;
            return CrateriaRoomStateDefinitions.ElevToBlueBrinstar0;
        },
        [RoomHeaderDefinitions.BombTorizo] = static (selection, match) =>
        {
            if (match(selection.IsEventSet((byte)EventNumber.ZebesTimebombSet), CrateriaRoomStateDefinitions.BombTorizo2)) return CrateriaRoomStateDefinitions.BombTorizo2;
            if (match(selection.IsBossDead(BossBits.AreaTorizo), CrateriaRoomStateDefinitions.BombTorizo1)) return CrateriaRoomStateDefinitions.BombTorizo1;
            return CrateriaRoomStateDefinitions.BombTorizo0;
        },
        [RoomHeaderDefinitions.Flyway] = static (selection, match) =>
        {
            if (match(selection.IsEventSet((byte)EventNumber.ZebesTimebombSet), CrateriaRoomStateDefinitions.Flyway2)) return CrateriaRoomStateDefinitions.Flyway2;
            if (match(selection.IsBossDead(BossBits.AreaTorizo), CrateriaRoomStateDefinitions.Flyway1)) return CrateriaRoomStateDefinitions.Flyway1;
            return CrateriaRoomStateDefinitions.Flyway0;
        },
        [RoomHeaderDefinitions.FinalMissileBombway] = static (selection, match) =>
        {
            if (match(selection.IsEventSet((byte)EventNumber.ZebesAwake), CrateriaRoomStateDefinitions.FinalMissileBombway1)) return CrateriaRoomStateDefinitions.FinalMissileBombway1;
            return CrateriaRoomStateDefinitions.FinalMissileBombway0;
        },
        [RoomHeaderDefinitions.FinalMissile] = static (selection, match) =>
        {
            if (match(selection.IsEventSet((byte)EventNumber.ZebesAwake), CrateriaRoomStateDefinitions.FinalMissile1)) return CrateriaRoomStateDefinitions.FinalMissile1;
            return CrateriaRoomStateDefinitions.FinalMissile0;
        },
        [RoomHeaderDefinitions.SporeSpawn] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaMiniBoss), BrinstarRoomStateDefinitions.SporeSpawn1)) return BrinstarRoomStateDefinitions.SporeSpawn1;
            return BrinstarRoomStateDefinitions.SporeSpawn0;
        },
        [RoomHeaderDefinitions.MorphBall] = static (selection, match) =>
        {
            if (match(selection.IsEventSet((byte)EventNumber.ZebesAwake), BrinstarRoomStateDefinitions.MorphBall1)) return BrinstarRoomStateDefinitions.MorphBall1;
            return BrinstarRoomStateDefinitions.MorphBall0;
        },
        [RoomHeaderDefinitions.ConstructionZone] = static (selection, match) =>
        {
            if (match(selection.IsEventSet((byte)EventNumber.ZebesAwake), BrinstarRoomStateDefinitions.ConstructionZone1)) return BrinstarRoomStateDefinitions.ConstructionZone1;
            return BrinstarRoomStateDefinitions.ConstructionZone0;
        },
        [RoomHeaderDefinitions.BlueBrinstarETank] = static (selection, match) =>
        {
            if (match(selection.IsEventSet((byte)EventNumber.ZebesAwake), BrinstarRoomStateDefinitions.BlueBrinstarETank1)) return BrinstarRoomStateDefinitions.BlueBrinstarETank1;
            return BrinstarRoomStateDefinitions.BlueBrinstarETank0;
        },
        [RoomHeaderDefinitions.MiniKraid] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), BrinstarRoomStateDefinitions.MiniKraid1)) return BrinstarRoomStateDefinitions.MiniKraid1;
            return BrinstarRoomStateDefinitions.MiniKraid0;
        },
        [RoomHeaderDefinitions.Kraid] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), BrinstarRoomStateDefinitions.Kraid1)) return BrinstarRoomStateDefinitions.Kraid1;
            return BrinstarRoomStateDefinitions.Kraid0;
        },
        [RoomHeaderDefinitions.Crocomire] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaMiniBoss), NorfairRoomStateDefinitions.Crocomire1)) return NorfairRoomStateDefinitions.Crocomire1;
            return NorfairRoomStateDefinitions.Crocomire0;
        },
        [RoomHeaderDefinitions.GoldenTorizo] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaTorizo), NorfairRoomStateDefinitions.GoldenTorizo1)) return NorfairRoomStateDefinitions.GoldenTorizo1;
            return NorfairRoomStateDefinitions.GoldenTorizo0;
        },
        [RoomHeaderDefinitions.Ridley] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), NorfairRoomStateDefinitions.Ridley1)) return NorfairRoomStateDefinitions.Ridley1;
            return NorfairRoomStateDefinitions.Ridley0;
        },
        [RoomHeaderDefinitions.BowlingAlley] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), WreckedShipRoomStateDefinitions.BowlingAlley1)) return WreckedShipRoomStateDefinitions.BowlingAlley1;
            return WreckedShipRoomStateDefinitions.BowlingAlley0;
        },
        [RoomHeaderDefinitions.WreckedShipEntrance] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), WreckedShipRoomStateDefinitions.WreckedShipEntrance1)) return WreckedShipRoomStateDefinitions.WreckedShipEntrance1;
            return WreckedShipRoomStateDefinitions.WreckedShipEntrance0;
        },
        [RoomHeaderDefinitions.Attic] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), WreckedShipRoomStateDefinitions.Attic1)) return WreckedShipRoomStateDefinitions.Attic1;
            return WreckedShipRoomStateDefinitions.Attic0;
        },
        [RoomHeaderDefinitions.AssemblyLine] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), WreckedShipRoomStateDefinitions.AssemblyLine1)) return WreckedShipRoomStateDefinitions.AssemblyLine1;
            return WreckedShipRoomStateDefinitions.AssemblyLine0;
        },
        [RoomHeaderDefinitions.WreckedShipMainShaft] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), WreckedShipRoomStateDefinitions.WreckedShipMainShaft1)) return WreckedShipRoomStateDefinitions.WreckedShipMainShaft1;
            return WreckedShipRoomStateDefinitions.WreckedShipMainShaft0;
        },
        [RoomHeaderDefinitions.SpikyDeath] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), WreckedShipRoomStateDefinitions.SpikyDeath1)) return WreckedShipRoomStateDefinitions.SpikyDeath1;
            return WreckedShipRoomStateDefinitions.SpikyDeath0;
        },
        [RoomHeaderDefinitions.ElectricDeath] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), WreckedShipRoomStateDefinitions.ElectricDeath1)) return WreckedShipRoomStateDefinitions.ElectricDeath1;
            return WreckedShipRoomStateDefinitions.ElectricDeath0;
        },
        [RoomHeaderDefinitions.WreckedShipETank] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), WreckedShipRoomStateDefinitions.WreckedShipETank1)) return WreckedShipRoomStateDefinitions.WreckedShipETank1;
            return WreckedShipRoomStateDefinitions.WreckedShipETank0;
        },
        [RoomHeaderDefinitions.Basement] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), WreckedShipRoomStateDefinitions.Basement1)) return WreckedShipRoomStateDefinitions.Basement1;
            return WreckedShipRoomStateDefinitions.Basement0;
        },
        [RoomHeaderDefinitions.WreckedShipMap] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), WreckedShipRoomStateDefinitions.WreckedShipMap1)) return WreckedShipRoomStateDefinitions.WreckedShipMap1;
            return WreckedShipRoomStateDefinitions.WreckedShipMap0;
        },
        [RoomHeaderDefinitions.Phantoon] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), WreckedShipRoomStateDefinitions.Phantoon1)) return WreckedShipRoomStateDefinitions.Phantoon1;
            return WreckedShipRoomStateDefinitions.Phantoon0;
        },
        [RoomHeaderDefinitions.SpongeBath] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), WreckedShipRoomStateDefinitions.SpongeBath1)) return WreckedShipRoomStateDefinitions.SpongeBath1;
            return WreckedShipRoomStateDefinitions.SpongeBath0;
        },
        [RoomHeaderDefinitions.WSWestSuper] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), WreckedShipRoomStateDefinitions.WSWestSuper1)) return WreckedShipRoomStateDefinitions.WSWestSuper1;
            return WreckedShipRoomStateDefinitions.WSWestSuper0;
        },
        [RoomHeaderDefinitions.WSEastSuper] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), WreckedShipRoomStateDefinitions.WSEastSuper1)) return WreckedShipRoomStateDefinitions.WSEastSuper1;
            return WreckedShipRoomStateDefinitions.WSEastSuper0;
        },
        [RoomHeaderDefinitions.GravitySuit] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), WreckedShipRoomStateDefinitions.GravitySuit1)) return WreckedShipRoomStateDefinitions.GravitySuit1;
            return WreckedShipRoomStateDefinitions.GravitySuit0;
        },
        [RoomHeaderDefinitions.WreckedShipSave] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), WreckedShipRoomStateDefinitions.WreckedShipSave1)) return WreckedShipRoomStateDefinitions.WreckedShipSave1;
            return WreckedShipRoomStateDefinitions.WreckedShipSave0;
        },
        [RoomHeaderDefinitions.GlassTunnel] = static (selection, match) =>
        {
            if (match(selection.IsEventSet((byte)EventNumber.MaridiaNoobTubeBroken), MaridiaRoomStateDefinitions.GlassTunnel1)) return MaridiaRoomStateDefinitions.GlassTunnel1;
            return MaridiaRoomStateDefinitions.GlassTunnel0;
        },
        [RoomHeaderDefinitions.ThePrecious] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), MaridiaRoomStateDefinitions.ThePrecious1)) return MaridiaRoomStateDefinitions.ThePrecious1;
            return MaridiaRoomStateDefinitions.ThePrecious0;
        },
        [RoomHeaderDefinitions.Shaktool] = static (selection, match) =>
        {
            if (match(selection.IsEventSet((byte)EventNumber.ShaktoolClearedPath), MaridiaRoomStateDefinitions.Shaktool1)) return MaridiaRoomStateDefinitions.Shaktool1;
            return MaridiaRoomStateDefinitions.Shaktool0;
        },
        [RoomHeaderDefinitions.Botwoon] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaMiniBoss), MaridiaRoomStateDefinitions.Botwoon1)) return MaridiaRoomStateDefinitions.Botwoon1;
            return MaridiaRoomStateDefinitions.Botwoon0;
        },
        [RoomHeaderDefinitions.Draygon] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), MaridiaRoomStateDefinitions.Draygon1)) return MaridiaRoomStateDefinitions.Draygon1;
            return MaridiaRoomStateDefinitions.Draygon0;
        },
        [RoomHeaderDefinitions.Metroids1] = static (selection, match) =>
        {
            if (match(selection.IsEventSet((byte)EventNumber.FirstMetroidHallCleared), TourianRoomStateDefinitions.Metroids11)) return TourianRoomStateDefinitions.Metroids11;
            return TourianRoomStateDefinitions.Metroids10;
        },
        [RoomHeaderDefinitions.Metroids2] = static (selection, match) =>
        {
            if (match(selection.IsEventSet((byte)EventNumber.FirstMetroidShaftCleared), TourianRoomStateDefinitions.Metroids21)) return TourianRoomStateDefinitions.Metroids21;
            return TourianRoomStateDefinitions.Metroids20;
        },
        [RoomHeaderDefinitions.Metroids3] = static (selection, match) =>
        {
            if (match(selection.IsEventSet((byte)EventNumber.SecondMetroidHallCleared), TourianRoomStateDefinitions.Metroids31)) return TourianRoomStateDefinitions.Metroids31;
            return TourianRoomStateDefinitions.Metroids30;
        },
        [RoomHeaderDefinitions.Metroids4] = static (selection, match) =>
        {
            if (match(selection.IsEventSet((byte)EventNumber.SecondMetroidShaftCleared), TourianRoomStateDefinitions.Metroids41)) return TourianRoomStateDefinitions.Metroids41;
            return TourianRoomStateDefinitions.Metroids40;
        },
        [RoomHeaderDefinitions.BlueHopper] = static (selection, match) =>
        {
            if (match(selection.IsEventSet((byte)EventNumber.Unused14), TourianRoomStateDefinitions.BlueHopper1)) return TourianRoomStateDefinitions.BlueHopper1;
            return TourianRoomStateDefinitions.BlueHopper0;
        },
        [RoomHeaderDefinitions.DustTorizo] = static (selection, match) =>
        {
            if (match(selection.IsEventSet((byte)EventNumber.Unused14), TourianRoomStateDefinitions.DustTorizo1)) return TourianRoomStateDefinitions.DustTorizo1;
            return TourianRoomStateDefinitions.DustTorizo0;
        },
        [RoomHeaderDefinitions.BigBoy] = static (selection, match) =>
        {
            if (match(selection.IsEventSet((byte)EventNumber.Unused14), TourianRoomStateDefinitions.BigBoy1)) return TourianRoomStateDefinitions.BigBoy1;
            return TourianRoomStateDefinitions.BigBoy0;
        },
        [RoomHeaderDefinitions.MotherBrain] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(RoomStateSelectorOperands.MainAreaBoss), TourianRoomStateDefinitions.MotherBrain2)) return TourianRoomStateDefinitions.MotherBrain2;
            if (match(selection.IsEventSet((byte)EventNumber.MotherBrainGlassDestroyed), TourianRoomStateDefinitions.MotherBrain1)) return TourianRoomStateDefinitions.MotherBrain1;
            return TourianRoomStateDefinitions.MotherBrain0;
        },
        [RoomHeaderDefinitions.CeresElev] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), CeresRoomStateDefinitions.CeresElev1)) return CeresRoomStateDefinitions.CeresElev1;
            return CeresRoomStateDefinitions.CeresElev0;
        },
        [RoomHeaderDefinitions.FallingTile] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), CeresRoomStateDefinitions.FallingTile1)) return CeresRoomStateDefinitions.FallingTile1;
            return CeresRoomStateDefinitions.FallingTile0;
        },
        [RoomHeaderDefinitions.MagnetStairs] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), CeresRoomStateDefinitions.MagnetStairs1)) return CeresRoomStateDefinitions.MagnetStairs1;
            return CeresRoomStateDefinitions.MagnetStairs0;
        },
        [RoomHeaderDefinitions.DeadScientist] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), CeresRoomStateDefinitions.DeadScientist1)) return CeresRoomStateDefinitions.DeadScientist1;
            return CeresRoomStateDefinitions.DeadScientist0;
        },
        [RoomHeaderDefinitions.Room58Escape] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), CeresRoomStateDefinitions.Room58Escape1)) return CeresRoomStateDefinitions.Room58Escape1;
            return CeresRoomStateDefinitions.Room58Escape0;
        },
        [RoomHeaderDefinitions.CeresRidley] = static (selection, match) =>
        {
            if (match(selection.IsBossDead(BossBits.AreaBoss), CeresRoomStateDefinitions.CeresRidley1)) return CeresRoomStateDefinitions.CeresRidley1;
            return CeresRoomStateDefinitions.CeresRidley0;
        },
        [RoomHeaderDefinitions.CrateriaPowerBombs] = static (_, _) => CrateriaRoomStateDefinitions.CrateriaPowerBombs,
        [RoomHeaderDefinitions.CrateriaSave] = static (_, _) => CrateriaRoomStateDefinitions.CrateriaSave,
        [RoomHeaderDefinitions.WestOcean] = static (_, _) => CrateriaRoomStateDefinitions.WestOcean,
        [RoomHeaderDefinitions.BowlingAlleyPath] = static (_, _) => CrateriaRoomStateDefinitions.BowlingAlleyPath,
        [RoomHeaderDefinitions.CrateriaKihunter] = static (_, _) => CrateriaRoomStateDefinitions.CrateriaKihunter,
        [RoomHeaderDefinitions.ForgottenHighwayElev] = static (_, _) => CrateriaRoomStateDefinitions.ForgottenHighwayElev,
        [RoomHeaderDefinitions.EastOcean] = static (_, _) => CrateriaRoomStateDefinitions.EastOcean,
        [RoomHeaderDefinitions.ForgottenHighwayKagos] = static (_, _) => CrateriaRoomStateDefinitions.ForgottenHighwayKagos,
        [RoomHeaderDefinitions.CrabMaze] = static (_, _) => CrateriaRoomStateDefinitions.CrabMaze,
        [RoomHeaderDefinitions.ForgottenHighwayElbow] = static (_, _) => CrateriaRoomStateDefinitions.ForgottenHighwayElbow,
        [RoomHeaderDefinitions.CrateriaTube] = static (_, _) => CrateriaRoomStateDefinitions.CrateriaTube,
        [RoomHeaderDefinitions.Moat] = static (_, _) => CrateriaRoomStateDefinitions.Moat,
        [RoomHeaderDefinitions.RedBinstarElev] = static (_, _) => CrateriaRoomStateDefinitions.RedBinstarElev,
        [RoomHeaderDefinitions.GauntletETank] = static (_, _) => CrateriaRoomStateDefinitions.GauntletETank,
        [RoomHeaderDefinitions.PreBowling] = static (_, _) => CrateriaRoomStateDefinitions.PreBowling,
        [RoomHeaderDefinitions.PreMapFlyway] = static (_, _) => CrateriaRoomStateDefinitions.PreMapFlyway,
        [RoomHeaderDefinitions.Terminator] = static (_, _) => CrateriaRoomStateDefinitions.Terminator,
        [RoomHeaderDefinitions.GreenBrinstarElev] = static (_, _) => CrateriaRoomStateDefinitions.GreenBrinstarElev,
        [RoomHeaderDefinitions.LowerMushrooms] = static (_, _) => CrateriaRoomStateDefinitions.LowerMushrooms,
        [RoomHeaderDefinitions.CrateriaMap] = static (_, _) => CrateriaRoomStateDefinitions.CrateriaMap,
        [RoomHeaderDefinitions.GreenPiratesShaft] = static (_, _) => CrateriaRoomStateDefinitions.GreenPiratesShaft,
        [RoomHeaderDefinitions.CrateriaSuper] = static (_, _) => CrateriaRoomStateDefinitions.CrateriaSuper,
        [RoomHeaderDefinitions.GreenBrinstarMainShaft] = static (_, _) => BrinstarRoomStateDefinitions.GreenBrinstarMainShaft,
        [RoomHeaderDefinitions.SporeSpawnSuper] = static (_, _) => BrinstarRoomStateDefinitions.SporeSpawnSuper,
        [RoomHeaderDefinitions.BrinstarPreMap] = static (_, _) => BrinstarRoomStateDefinitions.BrinstarPreMap,
        [RoomHeaderDefinitions.EarlySupers] = static (_, _) => BrinstarRoomStateDefinitions.EarlySupers,
        [RoomHeaderDefinitions.BrinstarReserveTank] = static (_, _) => BrinstarRoomStateDefinitions.BrinstarReserveTank,
        [RoomHeaderDefinitions.GreenBrinstarMap] = static (_, _) => BrinstarRoomStateDefinitions.GreenBrinstarMap,
        [RoomHeaderDefinitions.GreenBrinstarFirefleas] = static (_, _) => BrinstarRoomStateDefinitions.GreenBrinstarFirefleas,
        [RoomHeaderDefinitions.GreenBrinstarMissileRefill] = static (_, _) => BrinstarRoomStateDefinitions.GreenBrinstarMissileRefill,
        [RoomHeaderDefinitions.Dachora] = static (_, _) => BrinstarRoomStateDefinitions.Dachora,
        [RoomHeaderDefinitions.BigPink] = static (_, _) => BrinstarRoomStateDefinitions.BigPink,
        [RoomHeaderDefinitions.SporeSpawnKihunters] = static (_, _) => BrinstarRoomStateDefinitions.SporeSpawnKihunters,
        [RoomHeaderDefinitions.PinkBrinstarPowerBombs] = static (_, _) => BrinstarRoomStateDefinitions.PinkBrinstarPowerBombs,
        [RoomHeaderDefinitions.GreenHillZone] = static (_, _) => BrinstarRoomStateDefinitions.GreenHillZone,
        [RoomHeaderDefinitions.NoobBridge] = static (_, _) => BrinstarRoomStateDefinitions.NoobBridge,
        [RoomHeaderDefinitions.GreenBrinstarBeetoms] = static (_, _) => BrinstarRoomStateDefinitions.GreenBrinstarBeetoms,
        [RoomHeaderDefinitions.EtecoonETank] = static (_, _) => BrinstarRoomStateDefinitions.EtecoonETank,
        [RoomHeaderDefinitions.EtecoonSuper] = static (_, _) => BrinstarRoomStateDefinitions.EtecoonSuper,
        [RoomHeaderDefinitions.DachoraEnergyRefill] = static (_, _) => BrinstarRoomStateDefinitions.DachoraEnergyRefill,
        [RoomHeaderDefinitions.SporeSpawnFarming] = static (_, _) => BrinstarRoomStateDefinitions.SporeSpawnFarming,
        [RoomHeaderDefinitions.WaterwayETank] = static (_, _) => BrinstarRoomStateDefinitions.WaterwayETank,
        [RoomHeaderDefinitions.FirstMissile] = static (_, _) => BrinstarRoomStateDefinitions.FirstMissile,
        [RoomHeaderDefinitions.PinkBrinstarHoppers] = static (_, _) => BrinstarRoomStateDefinitions.PinkBrinstarHoppers,
        [RoomHeaderDefinitions.HopperETank] = static (_, _) => BrinstarRoomStateDefinitions.HopperETank,
        [RoomHeaderDefinitions.BigPinkSaveRoom] = static (_, _) => BrinstarRoomStateDefinitions.BigPinkSaveRoom,
        [RoomHeaderDefinitions.BlueBrinstarBoulders] = static (_, _) => BrinstarRoomStateDefinitions.BlueBrinstarBoulders,
        [RoomHeaderDefinitions.BillyMays] = static (_, _) => BrinstarRoomStateDefinitions.BillyMays,
        [RoomHeaderDefinitions.GreenBrinstarSave] = static (_, _) => BrinstarRoomStateDefinitions.GreenBrinstarSave,
        [RoomHeaderDefinitions.EtecoonSave] = static (_, _) => BrinstarRoomStateDefinitions.EtecoonSave,
        [RoomHeaderDefinitions.RedTower] = static (_, _) => BrinstarRoomStateDefinitions.RedTower,
        [RoomHeaderDefinitions.RedBrinstarFirefleas] = static (_, _) => BrinstarRoomStateDefinitions.RedBrinstarFirefleas,
        [RoomHeaderDefinitions.XrayScope] = static (_, _) => BrinstarRoomStateDefinitions.XrayScope,
        [RoomHeaderDefinitions.Hellway] = static (_, _) => BrinstarRoomStateDefinitions.Hellway,
        [RoomHeaderDefinitions.Caterpillar] = static (_, _) => BrinstarRoomStateDefinitions.Caterpillar,
        [RoomHeaderDefinitions.BetaPowerBomb] = static (_, _) => BrinstarRoomStateDefinitions.BetaPowerBomb,
        [RoomHeaderDefinitions.AlphaPowerBomb] = static (_, _) => BrinstarRoomStateDefinitions.AlphaPowerBomb,
        [RoomHeaderDefinitions.SkreeBoost] = static (_, _) => BrinstarRoomStateDefinitions.SkreeBoost,
        [RoomHeaderDefinitions.BelowSpazer] = static (_, _) => BrinstarRoomStateDefinitions.BelowSpazer,
        [RoomHeaderDefinitions.Spazer] = static (_, _) => BrinstarRoomStateDefinitions.Spazer,
        [RoomHeaderDefinitions.WarehouseZeela] = static (_, _) => BrinstarRoomStateDefinitions.WarehouseZeela,
        [RoomHeaderDefinitions.WarehouseETank] = static (_, _) => BrinstarRoomStateDefinitions.WarehouseETank,
        [RoomHeaderDefinitions.WarehouseKihunter] = static (_, _) => BrinstarRoomStateDefinitions.WarehouseKihunter,
        [RoomHeaderDefinitions.KraidEyeDoor] = static (_, _) => BrinstarRoomStateDefinitions.KraidEyeDoor,
        [RoomHeaderDefinitions.StatuesHallway] = static (_, _) => CrateriaRoomStateDefinitions.StatuesHallway,
        [RoomHeaderDefinitions.RedTowerRefill] = static (_, _) => BrinstarRoomStateDefinitions.RedTowerRefill,
        [RoomHeaderDefinitions.KraidRefill] = static (_, _) => BrinstarRoomStateDefinitions.KraidRefill,
        [RoomHeaderDefinitions.Statues] = static (_, _) => CrateriaRoomStateDefinitions.Statues,
        [RoomHeaderDefinitions.WarehouseEntrance] = static (_, _) => BrinstarRoomStateDefinitions.WarehouseEntrance,
        [RoomHeaderDefinitions.VariaSuit] = static (_, _) => BrinstarRoomStateDefinitions.VariaSuit,
        [RoomHeaderDefinitions.WarehouseSave] = static (_, _) => BrinstarRoomStateDefinitions.WarehouseSave,
        [RoomHeaderDefinitions.RedBrinstarSave] = static (_, _) => BrinstarRoomStateDefinitions.RedBrinstarSave,
        [RoomHeaderDefinitions.IceBeamAcid] = static (_, _) => NorfairRoomStateDefinitions.IceBeamAcid,
        [RoomHeaderDefinitions.Cathedral] = static (_, _) => NorfairRoomStateDefinitions.Cathedral,
        [RoomHeaderDefinitions.CathedralEntrance] = static (_, _) => NorfairRoomStateDefinitions.CathedralEntrance,
        [RoomHeaderDefinitions.BusinessCenter] = static (_, _) => NorfairRoomStateDefinitions.BusinessCenter,
        [RoomHeaderDefinitions.IceBeamGate] = static (_, _) => NorfairRoomStateDefinitions.IceBeamGate,
        [RoomHeaderDefinitions.IceBeamTutorial] = static (_, _) => NorfairRoomStateDefinitions.IceBeamTutorial,
        [RoomHeaderDefinitions.IceBeam] = static (_, _) => NorfairRoomStateDefinitions.IceBeam,
        [RoomHeaderDefinitions.IceBeamSnake] = static (_, _) => NorfairRoomStateDefinitions.IceBeamSnake,
        [RoomHeaderDefinitions.CrumbleShaft] = static (_, _) => NorfairRoomStateDefinitions.CrumbleShaft,
        [RoomHeaderDefinitions.CrocomireSpeedway] = static (_, _) => NorfairRoomStateDefinitions.CrocomireSpeedway,
        [RoomHeaderDefinitions.HiJumpBoots] = static (_, _) => NorfairRoomStateDefinitions.HiJumpBoots,
        [RoomHeaderDefinitions.CrocomireEscape] = static (_, _) => NorfairRoomStateDefinitions.CrocomireEscape,
        [RoomHeaderDefinitions.HiJumpETank] = static (_, _) => NorfairRoomStateDefinitions.HiJumpETank,
        [RoomHeaderDefinitions.PostCrocFarming] = static (_, _) => NorfairRoomStateDefinitions.PostCrocFarming,
        [RoomHeaderDefinitions.PostCrocSave] = static (_, _) => NorfairRoomStateDefinitions.PostCrocSave,
        [RoomHeaderDefinitions.PostCrocPowerBombs] = static (_, _) => NorfairRoomStateDefinitions.PostCrocPowerBombs,
        [RoomHeaderDefinitions.PostCrocShaft] = static (_, _) => NorfairRoomStateDefinitions.PostCrocShaft,
        [RoomHeaderDefinitions.PostCrocMissile] = static (_, _) => NorfairRoomStateDefinitions.PostCrocMissile,
        [RoomHeaderDefinitions.GrappleTutorial3] = static (_, _) => NorfairRoomStateDefinitions.GrappleTutorial3,
        [RoomHeaderDefinitions.PostCrocJump] = static (_, _) => NorfairRoomStateDefinitions.PostCrocJump,
        [RoomHeaderDefinitions.GrappleTutorial2] = static (_, _) => NorfairRoomStateDefinitions.GrappleTutorial2,
        [RoomHeaderDefinitions.GrappleTutorial1] = static (_, _) => NorfairRoomStateDefinitions.GrappleTutorial1,
        [RoomHeaderDefinitions.GrappleBeam] = static (_, _) => NorfairRoomStateDefinitions.GrappleBeam,
        [RoomHeaderDefinitions.NorfairReserveTank] = static (_, _) => NorfairRoomStateDefinitions.NorfairReserveTank,
        [RoomHeaderDefinitions.GreenBubblesMissiles] = static (_, _) => NorfairRoomStateDefinitions.GreenBubblesMissiles,
        [RoomHeaderDefinitions.BubbleMountain] = static (_, _) => NorfairRoomStateDefinitions.BubbleMountain,
        [RoomHeaderDefinitions.SpeedBoosterHall] = static (_, _) => NorfairRoomStateDefinitions.SpeedBoosterHall,
        [RoomHeaderDefinitions.SpeedBooster] = static (_, _) => NorfairRoomStateDefinitions.SpeedBooster,
        [RoomHeaderDefinitions.SingleChamber] = static (_, _) => NorfairRoomStateDefinitions.SingleChamber,
        [RoomHeaderDefinitions.DoubleChamber] = static (_, _) => NorfairRoomStateDefinitions.DoubleChamber,
        [RoomHeaderDefinitions.WaveBeam] = static (_, _) => NorfairRoomStateDefinitions.WaveBeam,
        [RoomHeaderDefinitions.SpikyPlatformsTunnel] = static (_, _) => NorfairRoomStateDefinitions.SpikyPlatformsTunnel,
        [RoomHeaderDefinitions.Volcano] = static (_, _) => NorfairRoomStateDefinitions.Volcano,
        [RoomHeaderDefinitions.KronicBoost] = static (_, _) => NorfairRoomStateDefinitions.KronicBoost,
        [RoomHeaderDefinitions.MagdolliteTunnel] = static (_, _) => NorfairRoomStateDefinitions.MagdolliteTunnel,
        [RoomHeaderDefinitions.PurpleShaft] = static (_, _) => NorfairRoomStateDefinitions.PurpleShaft,
        [RoomHeaderDefinitions.LavaDive] = static (_, _) => NorfairRoomStateDefinitions.LavaDive,
        [RoomHeaderDefinitions.LowerNorfairElev] = static (_, _) => NorfairRoomStateDefinitions.LowerNorfairElev,
        [RoomHeaderDefinitions.UpperNorfairFarming] = static (_, _) => NorfairRoomStateDefinitions.UpperNorfairFarming,
        [RoomHeaderDefinitions.RisingTide] = static (_, _) => NorfairRoomStateDefinitions.RisingTide,
        [RoomHeaderDefinitions.AcidSnakesTunnel] = static (_, _) => NorfairRoomStateDefinitions.AcidSnakesTunnel,
        [RoomHeaderDefinitions.SpikyAcidSnakesTunnel] = static (_, _) => NorfairRoomStateDefinitions.SpikyAcidSnakesTunnel,
        [RoomHeaderDefinitions.UpperNorfairRefill] = static (_, _) => NorfairRoomStateDefinitions.UpperNorfairRefill,
        [RoomHeaderDefinitions.PurpleFarming] = static (_, _) => NorfairRoomStateDefinitions.PurpleFarming,
        [RoomHeaderDefinitions.BatCave] = static (_, _) => NorfairRoomStateDefinitions.BatCave,
        [RoomHeaderDefinitions.NorfairMap] = static (_, _) => NorfairRoomStateDefinitions.NorfairMap,
        [RoomHeaderDefinitions.BubbleMountainSave] = static (_, _) => NorfairRoomStateDefinitions.BubbleMountainSave,
        [RoomHeaderDefinitions.FrogSpeedway] = static (_, _) => NorfairRoomStateDefinitions.FrogSpeedway,
        [RoomHeaderDefinitions.RedPirateShaft] = static (_, _) => NorfairRoomStateDefinitions.RedPirateShaft,
        [RoomHeaderDefinitions.FrogSave] = static (_, _) => NorfairRoomStateDefinitions.FrogSave,
        [RoomHeaderDefinitions.CrocomireSave] = static (_, _) => NorfairRoomStateDefinitions.CrocomireSave,
        [RoomHeaderDefinitions.LNElevSave] = static (_, _) => NorfairRoomStateDefinitions.LNElevSave,
        [RoomHeaderDefinitions.AcidStatue] = static (_, _) => NorfairRoomStateDefinitions.AcidStatue,
        [RoomHeaderDefinitions.MainHall] = static (_, _) => NorfairRoomStateDefinitions.MainHall,
        [RoomHeaderDefinitions.FastRipper] = static (_, _) => NorfairRoomStateDefinitions.FastRipper,
        [RoomHeaderDefinitions.GTEnergyRefill] = static (_, _) => NorfairRoomStateDefinitions.GTEnergyRefill,
        [RoomHeaderDefinitions.LNFarming] = static (_, _) => NorfairRoomStateDefinitions.LNFarming,
        [RoomHeaderDefinitions.FastPillarsSetup] = static (_, _) => NorfairRoomStateDefinitions.FastPillarsSetup,
        [RoomHeaderDefinitions.UnusedRoom8FB3E1] = static (_, _) => NorfairRoomStateDefinitions.UnusedRoom8FB3EE,
        [RoomHeaderDefinitions.MickeyMouse] = static (_, _) => NorfairRoomStateDefinitions.MickeyMouse,
        [RoomHeaderDefinitions.Pillar] = static (_, _) => NorfairRoomStateDefinitions.Pillar,
        [RoomHeaderDefinitions.Plowerhouse] = static (_, _) => NorfairRoomStateDefinitions.Plowerhouse,
        [RoomHeaderDefinitions.WorstRoomInTheGame] = static (_, _) => NorfairRoomStateDefinitions.WorstRoomInTheGame,
        [RoomHeaderDefinitions.Amphitheatre] = static (_, _) => NorfairRoomStateDefinitions.Amphitheatre,
        [RoomHeaderDefinitions.LNSpringBallMaze] = static (_, _) => NorfairRoomStateDefinitions.LNSpringBallMaze,
        [RoomHeaderDefinitions.LNEscapePowerBombs] = static (_, _) => NorfairRoomStateDefinitions.LNEscapePowerBombs,
        [RoomHeaderDefinitions.RedKihunterShaft] = static (_, _) => NorfairRoomStateDefinitions.RedKihunterShaft,
        [RoomHeaderDefinitions.Wasteland] = static (_, _) => NorfairRoomStateDefinitions.Wasteland,
        [RoomHeaderDefinitions.MetalPirates] = static (_, _) => NorfairRoomStateDefinitions.MetalPirates,
        [RoomHeaderDefinitions.ThreeMusketeers] = static (_, _) => NorfairRoomStateDefinitions.ThreeMusketeers,
        [RoomHeaderDefinitions.RidleyETank] = static (_, _) => NorfairRoomStateDefinitions.RidleyETank,
        [RoomHeaderDefinitions.ScrewAttack] = static (_, _) => NorfairRoomStateDefinitions.ScrewAttack,
        [RoomHeaderDefinitions.LNFireflea] = static (_, _) => NorfairRoomStateDefinitions.LNFireflea,
        [RoomHeaderDefinitions.LNSave] = static (_, _) => NorfairRoomStateDefinitions.LNSave,
        [RoomHeaderDefinitions.GlassTunnelSave] = static (_, _) => MaridiaRoomStateDefinitions.GlassTunnelSave,
        [RoomHeaderDefinitions.WestTunnel] = static (_, _) => MaridiaRoomStateDefinitions.WestTunnel,
        [RoomHeaderDefinitions.EastTunnel] = static (_, _) => MaridiaRoomStateDefinitions.EastTunnel,
        [RoomHeaderDefinitions.MainStreet] = static (_, _) => MaridiaRoomStateDefinitions.MainStreet,
        [RoomHeaderDefinitions.FishTank] = static (_, _) => MaridiaRoomStateDefinitions.FishTank,
        [RoomHeaderDefinitions.MamaTurtle] = static (_, _) => MaridiaRoomStateDefinitions.MamaTurtle,
        [RoomHeaderDefinitions.CrabTunnel] = static (_, _) => MaridiaRoomStateDefinitions.CrabTunnel,
        [RoomHeaderDefinitions.MtEverest] = static (_, _) => MaridiaRoomStateDefinitions.MtEverest,
        [RoomHeaderDefinitions.RedFish] = static (_, _) => MaridiaRoomStateDefinitions.RedFish,
        [RoomHeaderDefinitions.WateringHole] = static (_, _) => MaridiaRoomStateDefinitions.WateringHole,
        [RoomHeaderDefinitions.NWestMaridiaBug] = static (_, _) => MaridiaRoomStateDefinitions.NWestMaridiaBug,
        [RoomHeaderDefinitions.CrabShaft] = static (_, _) => MaridiaRoomStateDefinitions.CrabShaft,
        [RoomHeaderDefinitions.PseudoPlasmaSpark] = static (_, _) => MaridiaRoomStateDefinitions.PseudoPlasmaSpark,
        [RoomHeaderDefinitions.CrabHole] = static (_, _) => MaridiaRoomStateDefinitions.CrabHole,
        [RoomHeaderDefinitions.WestSandHallTunnel] = static (_, _) => MaridiaRoomStateDefinitions.WestSandHallTunnel,
        [RoomHeaderDefinitions.PlasmaTutorial] = static (_, _) => MaridiaRoomStateDefinitions.PlasmaTutorial,
        [RoomHeaderDefinitions.Plasma] = static (_, _) => MaridiaRoomStateDefinitions.Plasma,
        [RoomHeaderDefinitions.ThreadTheNeedle] = static (_, _) => MaridiaRoomStateDefinitions.ThreadTheNeedle,
        [RoomHeaderDefinitions.MaridiaElev] = static (_, _) => MaridiaRoomStateDefinitions.MaridiaElev,
        [RoomHeaderDefinitions.PlasmaSpark] = static (_, _) => MaridiaRoomStateDefinitions.PlasmaSpark,
        [RoomHeaderDefinitions.Kassiuz] = static (_, _) => MaridiaRoomStateDefinitions.Kassiuz,
        [RoomHeaderDefinitions.MaridiaMap] = static (_, _) => MaridiaRoomStateDefinitions.MaridiaMap,
        [RoomHeaderDefinitions.ForgottenHighwaySave] = static (_, _) => MaridiaRoomStateDefinitions.ForgottenHighwaySave,
        [RoomHeaderDefinitions.Toilet] = static (_, _) => MaridiaRoomStateDefinitions.Toilet,
        [RoomHeaderDefinitions.BugSandHole] = static (_, _) => MaridiaRoomStateDefinitions.BugSandHole,
        [RoomHeaderDefinitions.WestSandHall] = static (_, _) => MaridiaRoomStateDefinitions.WestSandHall,
        [RoomHeaderDefinitions.Oasis] = static (_, _) => MaridiaRoomStateDefinitions.Oasis,
        [RoomHeaderDefinitions.EastSandHall] = static (_, _) => MaridiaRoomStateDefinitions.EastSandHall,
        [RoomHeaderDefinitions.WestSandHole] = static (_, _) => MaridiaRoomStateDefinitions.WestSandHole,
        [RoomHeaderDefinitions.EastSandHole] = static (_, _) => MaridiaRoomStateDefinitions.EastSandHole,
        [RoomHeaderDefinitions.WestAqueductQuicksand] = static (_, _) => MaridiaRoomStateDefinitions.WestAqueductQuicksand,
        [RoomHeaderDefinitions.EastAqueductQuicksand] = static (_, _) => MaridiaRoomStateDefinitions.EastAqueductQuicksand,
        [RoomHeaderDefinitions.Aqueduct] = static (_, _) => MaridiaRoomStateDefinitions.Aqueduct,
        [RoomHeaderDefinitions.Butterfly] = static (_, _) => MaridiaRoomStateDefinitions.Butterfly,
        [RoomHeaderDefinitions.BotwoonHallway] = static (_, _) => MaridiaRoomStateDefinitions.BotwoonHallway,
        [RoomHeaderDefinitions.Pants] = static (_, _) => MaridiaRoomStateDefinitions.Pants,
        [RoomHeaderDefinitions.EastPants] = static (_, _) => MaridiaRoomStateDefinitions.EastPants,
        [RoomHeaderDefinitions.Springball] = static (_, _) => MaridiaRoomStateDefinitions.Springball,
        [RoomHeaderDefinitions.BelowBotwoonETank] = static (_, _) => MaridiaRoomStateDefinitions.BelowBotwoonETank,
        [RoomHeaderDefinitions.Colosseum] = static (_, _) => MaridiaRoomStateDefinitions.Colosseum,
        [RoomHeaderDefinitions.AqueductSave] = static (_, _) => MaridiaRoomStateDefinitions.AqueductSave,
        [RoomHeaderDefinitions.BotwoonETank] = static (_, _) => MaridiaRoomStateDefinitions.BotwoonETank,
        [RoomHeaderDefinitions.DraygonSave] = static (_, _) => MaridiaRoomStateDefinitions.DraygonSave,
        [RoomHeaderDefinitions.MaridiaMissileRefill] = static (_, _) => MaridiaRoomStateDefinitions.MaridiaMissileRefill,
        [RoomHeaderDefinitions.PlasmaBeachQuicksand] = static (_, _) => MaridiaRoomStateDefinitions.PlasmaBeachQuicksand,
        [RoomHeaderDefinitions.BotwoonQuicksand] = static (_, _) => MaridiaRoomStateDefinitions.BotwoonQuicksand,
        [RoomHeaderDefinitions.HalfieClimb] = static (_, _) => MaridiaRoomStateDefinitions.HalfieClimb,
        [RoomHeaderDefinitions.SpaceJump] = static (_, _) => MaridiaRoomStateDefinitions.SpaceJump,
        [RoomHeaderDefinitions.MaridiaEnergyRefill] = static (_, _) => MaridiaRoomStateDefinitions.MaridiaEnergyRefill,
        [RoomHeaderDefinitions.WestCactusAlley] = static (_, _) => MaridiaRoomStateDefinitions.WestCactusAlley,
        [RoomHeaderDefinitions.EastCactusAlley] = static (_, _) => MaridiaRoomStateDefinitions.EastCactusAlley,
        [RoomHeaderDefinitions.TourianFirst] = static (_, _) => TourianRoomStateDefinitions.TourianFirst,
        [RoomHeaderDefinitions.Seaweed] = static (_, _) => TourianRoomStateDefinitions.Seaweed,
        [RoomHeaderDefinitions.TourianRecharge] = static (_, _) => TourianRoomStateDefinitions.TourianRecharge,
        [RoomHeaderDefinitions.TourianEyeDoor] = static (_, _) => TourianRoomStateDefinitions.TourianEyeDoor,
        [RoomHeaderDefinitions.RinkaShaft] = static (_, _) => TourianRoomStateDefinitions.RinkaShaft,
        [RoomHeaderDefinitions.MotherBrainSave] = static (_, _) => TourianRoomStateDefinitions.MotherBrainSave,
        [RoomHeaderDefinitions.TourianEscape1] = static (_, _) => TourianRoomStateDefinitions.TourianEscape1,
        [RoomHeaderDefinitions.TourianEscape2] = static (_, _) => TourianRoomStateDefinitions.TourianEscape2,
        [RoomHeaderDefinitions.TourianEscape3] = static (_, _) => TourianRoomStateDefinitions.TourianEscape3,
        [RoomHeaderDefinitions.TourianEscape4] = static (_, _) => TourianRoomStateDefinitions.TourianEscape4,
        [RoomHeaderDefinitions.UpperTourianSave] = static (_, _) => TourianRoomStateDefinitions.UpperTourianSave,
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
