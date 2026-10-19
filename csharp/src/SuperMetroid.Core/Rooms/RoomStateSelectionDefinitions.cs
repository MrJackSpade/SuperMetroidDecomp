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
        switch (room.Name)
        {
            case nameof(RoomHeaderDefinitions.LandingSite):
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesTimebombSet), RoomStateDefinitions.LandingSite3)) return RoomStateDefinitions.LandingSite3;
                if (Match(selection.HasPowerBombs, RoomStateDefinitions.LandingSite2)) return RoomStateDefinitions.LandingSite2;
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesAwake), RoomStateDefinitions.LandingSite1)) return RoomStateDefinitions.LandingSite1;
                return RoomStateDefinitions.LandingSite0;
            case nameof(RoomHeaderDefinitions.GauntletEntrance):
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesAwake), RoomStateDefinitions.GauntletEntrance1)) return RoomStateDefinitions.GauntletEntrance1;
                return RoomStateDefinitions.GauntletEntrance0;
            case nameof(RoomHeaderDefinitions.Parlor):
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesTimebombSet), RoomStateDefinitions.Parlor2)) return RoomStateDefinitions.Parlor2;
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesAwake), RoomStateDefinitions.Parlor1)) return RoomStateDefinitions.Parlor1;
                return RoomStateDefinitions.Parlor0;
            case nameof(RoomHeaderDefinitions.Climb):
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesTimebombSet), RoomStateDefinitions.Climb2)) return RoomStateDefinitions.Climb2;
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesAwake), RoomStateDefinitions.Climb1)) return RoomStateDefinitions.Climb1;
                return RoomStateDefinitions.Climb0;
            case nameof(RoomHeaderDefinitions.Pit):
                if (Match(selection.HasMorphBallAndMissiles, RoomStateDefinitions.Pit1)) return RoomStateDefinitions.Pit1;
                return RoomStateDefinitions.Pit0;
            case nameof(RoomHeaderDefinitions.ElevToBlueBrinstar):
                if (Match(selection.HasMorphBallAndMissiles, RoomStateDefinitions.ElevToBlueBrinstar1)) return RoomStateDefinitions.ElevToBlueBrinstar1;
                return RoomStateDefinitions.ElevToBlueBrinstar0;
            case nameof(RoomHeaderDefinitions.BombTorizo):
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesTimebombSet), RoomStateDefinitions.BombTorizo2)) return RoomStateDefinitions.BombTorizo2;
                if (Match(selection.IsBossDead(BossBits.AreaTorizo), RoomStateDefinitions.BombTorizo1)) return RoomStateDefinitions.BombTorizo1;
                return RoomStateDefinitions.BombTorizo0;
            case nameof(RoomHeaderDefinitions.Flyway):
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesTimebombSet), RoomStateDefinitions.Flyway2)) return RoomStateDefinitions.Flyway2;
                if (Match(selection.IsBossDead(BossBits.AreaTorizo), RoomStateDefinitions.Flyway1)) return RoomStateDefinitions.Flyway1;
                return RoomStateDefinitions.Flyway0;
            case nameof(RoomHeaderDefinitions.FinalMissileBombway):
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesAwake), RoomStateDefinitions.FinalMissileBombway1)) return RoomStateDefinitions.FinalMissileBombway1;
                return RoomStateDefinitions.FinalMissileBombway0;
            case nameof(RoomHeaderDefinitions.FinalMissile):
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesAwake), RoomStateDefinitions.FinalMissile1)) return RoomStateDefinitions.FinalMissile1;
                return RoomStateDefinitions.FinalMissile0;
            case nameof(RoomHeaderDefinitions.SporeSpawn):
                if (Match(selection.IsBossDead(BossBits.AreaMiniBoss), RoomStateDefinitions.SporeSpawn1)) return RoomStateDefinitions.SporeSpawn1;
                return RoomStateDefinitions.SporeSpawn0;
            case nameof(RoomHeaderDefinitions.MorphBall):
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesAwake), RoomStateDefinitions.MorphBall1)) return RoomStateDefinitions.MorphBall1;
                return RoomStateDefinitions.MorphBall0;
            case nameof(RoomHeaderDefinitions.ConstructionZone):
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesAwake), RoomStateDefinitions.ConstructionZone1)) return RoomStateDefinitions.ConstructionZone1;
                return RoomStateDefinitions.ConstructionZone0;
            case nameof(RoomHeaderDefinitions.BlueBrinstarETank):
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesAwake), RoomStateDefinitions.BlueBrinstarETank1)) return RoomStateDefinitions.BlueBrinstarETank1;
                return RoomStateDefinitions.BlueBrinstarETank0;
            case nameof(RoomHeaderDefinitions.MiniKraid):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.MiniKraid1)) return RoomStateDefinitions.MiniKraid1;
                return RoomStateDefinitions.MiniKraid0;
            case nameof(RoomHeaderDefinitions.Kraid):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.Kraid1)) return RoomStateDefinitions.Kraid1;
                return RoomStateDefinitions.Kraid0;
            case nameof(RoomHeaderDefinitions.Crocomire):
                if (Match(selection.IsBossDead(BossBits.AreaMiniBoss), RoomStateDefinitions.Crocomire1)) return RoomStateDefinitions.Crocomire1;
                return RoomStateDefinitions.Crocomire0;
            case nameof(RoomHeaderDefinitions.GoldenTorizo):
                if (Match(selection.IsBossDead(BossBits.AreaTorizo), RoomStateDefinitions.GoldenTorizo1)) return RoomStateDefinitions.GoldenTorizo1;
                return RoomStateDefinitions.GoldenTorizo0;
            case nameof(RoomHeaderDefinitions.Ridley):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.Ridley1)) return RoomStateDefinitions.Ridley1;
                return RoomStateDefinitions.Ridley0;
            case nameof(RoomHeaderDefinitions.BowlingAlley):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.BowlingAlley1)) return RoomStateDefinitions.BowlingAlley1;
                return RoomStateDefinitions.BowlingAlley0;
            case nameof(RoomHeaderDefinitions.WreckedShipEntrance):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.WreckedShipEntrance1)) return RoomStateDefinitions.WreckedShipEntrance1;
                return RoomStateDefinitions.WreckedShipEntrance0;
            case nameof(RoomHeaderDefinitions.Attic):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.Attic1)) return RoomStateDefinitions.Attic1;
                return RoomStateDefinitions.Attic0;
            case nameof(RoomHeaderDefinitions.AssemblyLine):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.AssemblyLine1)) return RoomStateDefinitions.AssemblyLine1;
                return RoomStateDefinitions.AssemblyLine0;
            case nameof(RoomHeaderDefinitions.WreckedShipMainShaft):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.WreckedShipMainShaft1)) return RoomStateDefinitions.WreckedShipMainShaft1;
                return RoomStateDefinitions.WreckedShipMainShaft0;
            case nameof(RoomHeaderDefinitions.SpikyDeath):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.SpikyDeath1)) return RoomStateDefinitions.SpikyDeath1;
                return RoomStateDefinitions.SpikyDeath0;
            case nameof(RoomHeaderDefinitions.ElectricDeath):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.ElectricDeath1)) return RoomStateDefinitions.ElectricDeath1;
                return RoomStateDefinitions.ElectricDeath0;
            case nameof(RoomHeaderDefinitions.WreckedShipETank):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.WreckedShipETank1)) return RoomStateDefinitions.WreckedShipETank1;
                return RoomStateDefinitions.WreckedShipETank0;
            case nameof(RoomHeaderDefinitions.Basement):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.Basement1)) return RoomStateDefinitions.Basement1;
                return RoomStateDefinitions.Basement0;
            case nameof(RoomHeaderDefinitions.WreckedShipMap):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.WreckedShipMap1)) return RoomStateDefinitions.WreckedShipMap1;
                return RoomStateDefinitions.WreckedShipMap0;
            case nameof(RoomHeaderDefinitions.Phantoon):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.Phantoon1)) return RoomStateDefinitions.Phantoon1;
                return RoomStateDefinitions.Phantoon0;
            case nameof(RoomHeaderDefinitions.SpongeBath):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.SpongeBath1)) return RoomStateDefinitions.SpongeBath1;
                return RoomStateDefinitions.SpongeBath0;
            case nameof(RoomHeaderDefinitions.WSWestSuper):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.WSWestSuper1)) return RoomStateDefinitions.WSWestSuper1;
                return RoomStateDefinitions.WSWestSuper0;
            case nameof(RoomHeaderDefinitions.WSEastSuper):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.WSEastSuper1)) return RoomStateDefinitions.WSEastSuper1;
                return RoomStateDefinitions.WSEastSuper0;
            case nameof(RoomHeaderDefinitions.GravitySuit):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.GravitySuit1)) return RoomStateDefinitions.GravitySuit1;
                return RoomStateDefinitions.GravitySuit0;
            case nameof(RoomHeaderDefinitions.WreckedShipSave):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.WreckedShipSave1)) return RoomStateDefinitions.WreckedShipSave1;
                return RoomStateDefinitions.WreckedShipSave0;
            case nameof(RoomHeaderDefinitions.GlassTunnel):
                if (Match(selection.IsEventSet((byte)EventNumber.MaridiaNoobTubeBroken), RoomStateDefinitions.GlassTunnel1)) return RoomStateDefinitions.GlassTunnel1;
                return RoomStateDefinitions.GlassTunnel0;
            case nameof(RoomHeaderDefinitions.ThePrecious):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.ThePrecious1)) return RoomStateDefinitions.ThePrecious1;
                return RoomStateDefinitions.ThePrecious0;
            case nameof(RoomHeaderDefinitions.Shaktool):
                if (Match(selection.IsEventSet((byte)EventNumber.ShaktoolClearedPath), RoomStateDefinitions.Shaktool1)) return RoomStateDefinitions.Shaktool1;
                return RoomStateDefinitions.Shaktool0;
            case nameof(RoomHeaderDefinitions.Botwoon):
                if (Match(selection.IsBossDead(BossBits.AreaMiniBoss), RoomStateDefinitions.Botwoon1)) return RoomStateDefinitions.Botwoon1;
                return RoomStateDefinitions.Botwoon0;
            case nameof(RoomHeaderDefinitions.Draygon):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.Draygon1)) return RoomStateDefinitions.Draygon1;
                return RoomStateDefinitions.Draygon0;
            case nameof(RoomHeaderDefinitions.Metroids1):
                if (Match(selection.IsEventSet((byte)EventNumber.FirstMetroidHallCleared), RoomStateDefinitions.Metroids11)) return RoomStateDefinitions.Metroids11;
                return RoomStateDefinitions.Metroids10;
            case nameof(RoomHeaderDefinitions.Metroids2):
                if (Match(selection.IsEventSet((byte)EventNumber.FirstMetroidShaftCleared), RoomStateDefinitions.Metroids21)) return RoomStateDefinitions.Metroids21;
                return RoomStateDefinitions.Metroids20;
            case nameof(RoomHeaderDefinitions.Metroids3):
                if (Match(selection.IsEventSet((byte)EventNumber.SecondMetroidHallCleared), RoomStateDefinitions.Metroids31)) return RoomStateDefinitions.Metroids31;
                return RoomStateDefinitions.Metroids30;
            case nameof(RoomHeaderDefinitions.Metroids4):
                if (Match(selection.IsEventSet((byte)EventNumber.SecondMetroidShaftCleared), RoomStateDefinitions.Metroids41)) return RoomStateDefinitions.Metroids41;
                return RoomStateDefinitions.Metroids40;
            case nameof(RoomHeaderDefinitions.BlueHopper):
                if (Match(selection.IsEventSet((byte)EventNumber.Unused14), RoomStateDefinitions.BlueHopper1)) return RoomStateDefinitions.BlueHopper1;
                return RoomStateDefinitions.BlueHopper0;
            case nameof(RoomHeaderDefinitions.DustTorizo):
                if (Match(selection.IsEventSet((byte)EventNumber.Unused14), RoomStateDefinitions.DustTorizo1)) return RoomStateDefinitions.DustTorizo1;
                return RoomStateDefinitions.DustTorizo0;
            case nameof(RoomHeaderDefinitions.BigBoy):
                if (Match(selection.IsEventSet((byte)EventNumber.Unused14), RoomStateDefinitions.BigBoy1)) return RoomStateDefinitions.BigBoy1;
                return RoomStateDefinitions.BigBoy0;
            case nameof(RoomHeaderDefinitions.MotherBrain):
                if (Match(selection.IsBossDead(RoomStateSelectorOperands.MainAreaBoss), RoomStateDefinitions.MotherBrain2)) return RoomStateDefinitions.MotherBrain2;
                if (Match(selection.IsEventSet((byte)EventNumber.MotherBrainGlassDestroyed), RoomStateDefinitions.MotherBrain1)) return RoomStateDefinitions.MotherBrain1;
                return RoomStateDefinitions.MotherBrain0;
            case nameof(RoomHeaderDefinitions.CeresElev):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.CeresElev1)) return RoomStateDefinitions.CeresElev1;
                return RoomStateDefinitions.CeresElev0;
            case nameof(RoomHeaderDefinitions.FallingTile):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.FallingTile1)) return RoomStateDefinitions.FallingTile1;
                return RoomStateDefinitions.FallingTile0;
            case nameof(RoomHeaderDefinitions.MagnetStairs):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.MagnetStairs1)) return RoomStateDefinitions.MagnetStairs1;
                return RoomStateDefinitions.MagnetStairs0;
            case nameof(RoomHeaderDefinitions.DeadScientist):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.DeadScientist1)) return RoomStateDefinitions.DeadScientist1;
                return RoomStateDefinitions.DeadScientist0;
            case nameof(RoomHeaderDefinitions.Room58Escape):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.Room58Escape1)) return RoomStateDefinitions.Room58Escape1;
                return RoomStateDefinitions.Room58Escape0;
            case nameof(RoomHeaderDefinitions.CeresRidley):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), RoomStateDefinitions.CeresRidley1)) return RoomStateDefinitions.CeresRidley1;
                return RoomStateDefinitions.CeresRidley0;
            case nameof(RoomHeaderDefinitions.CrateriaPowerBombs):
                return RoomStateDefinitions.CrateriaPowerBombs;
            case nameof(RoomHeaderDefinitions.CrateriaSave):
                return RoomStateDefinitions.CrateriaSave;
            case nameof(RoomHeaderDefinitions.WestOcean):
                return RoomStateDefinitions.WestOcean;
            case nameof(RoomHeaderDefinitions.BowlingAlleyPath):
                return RoomStateDefinitions.BowlingAlleyPath;
            case nameof(RoomHeaderDefinitions.CrateriaKihunter):
                return RoomStateDefinitions.CrateriaKihunter;
            case nameof(RoomHeaderDefinitions.ForgottenHighwayElev):
                return RoomStateDefinitions.ForgottenHighwayElev;
            case nameof(RoomHeaderDefinitions.EastOcean):
                return RoomStateDefinitions.EastOcean;
            case nameof(RoomHeaderDefinitions.ForgottenHighwayKagos):
                return RoomStateDefinitions.ForgottenHighwayKagos;
            case nameof(RoomHeaderDefinitions.CrabMaze):
                return RoomStateDefinitions.CrabMaze;
            case nameof(RoomHeaderDefinitions.ForgottenHighwayElbow):
                return RoomStateDefinitions.ForgottenHighwayElbow;
            case nameof(RoomHeaderDefinitions.CrateriaTube):
                return RoomStateDefinitions.CrateriaTube;
            case nameof(RoomHeaderDefinitions.Moat):
                return RoomStateDefinitions.Moat;
            case nameof(RoomHeaderDefinitions.RedBinstarElev):
                return RoomStateDefinitions.RedBinstarElev;
            case nameof(RoomHeaderDefinitions.GauntletETank):
                return RoomStateDefinitions.GauntletETank;
            case nameof(RoomHeaderDefinitions.PreBowling):
                return RoomStateDefinitions.PreBowling;
            case nameof(RoomHeaderDefinitions.PreMapFlyway):
                return RoomStateDefinitions.PreMapFlyway;
            case nameof(RoomHeaderDefinitions.Terminator):
                return RoomStateDefinitions.Terminator;
            case nameof(RoomHeaderDefinitions.GreenBrinstarElev):
                return RoomStateDefinitions.GreenBrinstarElev;
            case nameof(RoomHeaderDefinitions.LowerMushrooms):
                return RoomStateDefinitions.LowerMushrooms;
            case nameof(RoomHeaderDefinitions.CrateriaMap):
                return RoomStateDefinitions.CrateriaMap;
            case nameof(RoomHeaderDefinitions.GreenPiratesShaft):
                return RoomStateDefinitions.GreenPiratesShaft;
            case nameof(RoomHeaderDefinitions.CrateriaSuper):
                return RoomStateDefinitions.CrateriaSuper;
            case nameof(RoomHeaderDefinitions.GreenBrinstarMainShaft):
                return RoomStateDefinitions.GreenBrinstarMainShaft;
            case nameof(RoomHeaderDefinitions.SporeSpawnSuper):
                return RoomStateDefinitions.SporeSpawnSuper;
            case nameof(RoomHeaderDefinitions.BrinstarPreMap):
                return RoomStateDefinitions.BrinstarPreMap;
            case nameof(RoomHeaderDefinitions.EarlySupers):
                return RoomStateDefinitions.EarlySupers;
            case nameof(RoomHeaderDefinitions.BrinstarReserveTank):
                return RoomStateDefinitions.BrinstarReserveTank;
            case nameof(RoomHeaderDefinitions.GreenBrinstarMap):
                return RoomStateDefinitions.GreenBrinstarMap;
            case nameof(RoomHeaderDefinitions.GreenBrinstarFirefleas):
                return RoomStateDefinitions.GreenBrinstarFirefleas;
            case nameof(RoomHeaderDefinitions.GreenBrinstarMissileRefill):
                return RoomStateDefinitions.GreenBrinstarMissileRefill;
            case nameof(RoomHeaderDefinitions.Dachora):
                return RoomStateDefinitions.Dachora;
            case nameof(RoomHeaderDefinitions.BigPink):
                return RoomStateDefinitions.BigPink;
            case nameof(RoomHeaderDefinitions.SporeSpawnKihunters):
                return RoomStateDefinitions.SporeSpawnKihunters;
            case nameof(RoomHeaderDefinitions.PinkBrinstarPowerBombs):
                return RoomStateDefinitions.PinkBrinstarPowerBombs;
            case nameof(RoomHeaderDefinitions.GreenHillZone):
                return RoomStateDefinitions.GreenHillZone;
            case nameof(RoomHeaderDefinitions.NoobBridge):
                return RoomStateDefinitions.NoobBridge;
            case nameof(RoomHeaderDefinitions.GreenBrinstarBeetoms):
                return RoomStateDefinitions.GreenBrinstarBeetoms;
            case nameof(RoomHeaderDefinitions.EtecoonETank):
                return RoomStateDefinitions.EtecoonETank;
            case nameof(RoomHeaderDefinitions.EtecoonSuper):
                return RoomStateDefinitions.EtecoonSuper;
            case nameof(RoomHeaderDefinitions.DachoraEnergyRefill):
                return RoomStateDefinitions.DachoraEnergyRefill;
            case nameof(RoomHeaderDefinitions.SporeSpawnFarming):
                return RoomStateDefinitions.SporeSpawnFarming;
            case nameof(RoomHeaderDefinitions.WaterwayETank):
                return RoomStateDefinitions.WaterwayETank;
            case nameof(RoomHeaderDefinitions.FirstMissile):
                return RoomStateDefinitions.FirstMissile;
            case nameof(RoomHeaderDefinitions.PinkBrinstarHoppers):
                return RoomStateDefinitions.PinkBrinstarHoppers;
            case nameof(RoomHeaderDefinitions.HopperETank):
                return RoomStateDefinitions.HopperETank;
            case nameof(RoomHeaderDefinitions.BigPinkSaveRoom):
                return RoomStateDefinitions.BigPinkSaveRoom;
            case nameof(RoomHeaderDefinitions.BlueBrinstarBoulders):
                return RoomStateDefinitions.BlueBrinstarBoulders;
            case nameof(RoomHeaderDefinitions.BillyMays):
                return RoomStateDefinitions.BillyMays;
            case nameof(RoomHeaderDefinitions.GreenBrinstarSave):
                return RoomStateDefinitions.GreenBrinstarSave;
            case nameof(RoomHeaderDefinitions.EtecoonSave):
                return RoomStateDefinitions.EtecoonSave;
            case nameof(RoomHeaderDefinitions.RedTower):
                return RoomStateDefinitions.RedTower;
            case nameof(RoomHeaderDefinitions.RedBrinstarFirefleas):
                return RoomStateDefinitions.RedBrinstarFirefleas;
            case nameof(RoomHeaderDefinitions.XrayScope):
                return RoomStateDefinitions.XrayScope;
            case nameof(RoomHeaderDefinitions.Hellway):
                return RoomStateDefinitions.Hellway;
            case nameof(RoomHeaderDefinitions.Caterpillar):
                return RoomStateDefinitions.Caterpillar;
            case nameof(RoomHeaderDefinitions.BetaPowerBomb):
                return RoomStateDefinitions.BetaPowerBomb;
            case nameof(RoomHeaderDefinitions.AlphaPowerBomb):
                return RoomStateDefinitions.AlphaPowerBomb;
            case nameof(RoomHeaderDefinitions.SkreeBoost):
                return RoomStateDefinitions.SkreeBoost;
            case nameof(RoomHeaderDefinitions.BelowSpazer):
                return RoomStateDefinitions.BelowSpazer;
            case nameof(RoomHeaderDefinitions.Spazer):
                return RoomStateDefinitions.Spazer;
            case nameof(RoomHeaderDefinitions.WarehouseZeela):
                return RoomStateDefinitions.WarehouseZeela;
            case nameof(RoomHeaderDefinitions.WarehouseETank):
                return RoomStateDefinitions.WarehouseETank;
            case nameof(RoomHeaderDefinitions.WarehouseKihunter):
                return RoomStateDefinitions.WarehouseKihunter;
            case nameof(RoomHeaderDefinitions.KraidEyeDoor):
                return RoomStateDefinitions.KraidEyeDoor;
            case nameof(RoomHeaderDefinitions.StatuesHallway):
                return RoomStateDefinitions.StatuesHallway;
            case nameof(RoomHeaderDefinitions.RedTowerRefill):
                return RoomStateDefinitions.RedTowerRefill;
            case nameof(RoomHeaderDefinitions.KraidRefill):
                return RoomStateDefinitions.KraidRefill;
            case nameof(RoomHeaderDefinitions.Statues):
                return RoomStateDefinitions.Statues;
            case nameof(RoomHeaderDefinitions.WarehouseEntrance):
                return RoomStateDefinitions.WarehouseEntrance;
            case nameof(RoomHeaderDefinitions.VariaSuit):
                return RoomStateDefinitions.VariaSuit;
            case nameof(RoomHeaderDefinitions.WarehouseSave):
                return RoomStateDefinitions.WarehouseSave;
            case nameof(RoomHeaderDefinitions.RedBrinstarSave):
                return RoomStateDefinitions.RedBrinstarSave;
            case nameof(RoomHeaderDefinitions.IceBeamAcid):
                return RoomStateDefinitions.IceBeamAcid;
            case nameof(RoomHeaderDefinitions.Cathedral):
                return RoomStateDefinitions.Cathedral;
            case nameof(RoomHeaderDefinitions.CathedralEntrance):
                return RoomStateDefinitions.CathedralEntrance;
            case nameof(RoomHeaderDefinitions.BusinessCenter):
                return RoomStateDefinitions.BusinessCenter;
            case nameof(RoomHeaderDefinitions.IceBeamGate):
                return RoomStateDefinitions.IceBeamGate;
            case nameof(RoomHeaderDefinitions.IceBeamTutorial):
                return RoomStateDefinitions.IceBeamTutorial;
            case nameof(RoomHeaderDefinitions.IceBeam):
                return RoomStateDefinitions.IceBeam;
            case nameof(RoomHeaderDefinitions.IceBeamSnake):
                return RoomStateDefinitions.IceBeamSnake;
            case nameof(RoomHeaderDefinitions.CrumbleShaft):
                return RoomStateDefinitions.CrumbleShaft;
            case nameof(RoomHeaderDefinitions.CrocomireSpeedway):
                return RoomStateDefinitions.CrocomireSpeedway;
            case nameof(RoomHeaderDefinitions.HiJumpBoots):
                return RoomStateDefinitions.HiJumpBoots;
            case nameof(RoomHeaderDefinitions.CrocomireEscape):
                return RoomStateDefinitions.CrocomireEscape;
            case nameof(RoomHeaderDefinitions.HiJumpETank):
                return RoomStateDefinitions.HiJumpETank;
            case nameof(RoomHeaderDefinitions.PostCrocFarming):
                return RoomStateDefinitions.PostCrocFarming;
            case nameof(RoomHeaderDefinitions.PostCrocSave):
                return RoomStateDefinitions.PostCrocSave;
            case nameof(RoomHeaderDefinitions.PostCrocPowerBombs):
                return RoomStateDefinitions.PostCrocPowerBombs;
            case nameof(RoomHeaderDefinitions.PostCrocShaft):
                return RoomStateDefinitions.PostCrocShaft;
            case nameof(RoomHeaderDefinitions.PostCrocMissile):
                return RoomStateDefinitions.PostCrocMissile;
            case nameof(RoomHeaderDefinitions.GrappleTutorial3):
                return RoomStateDefinitions.GrappleTutorial3;
            case nameof(RoomHeaderDefinitions.PostCrocJump):
                return RoomStateDefinitions.PostCrocJump;
            case nameof(RoomHeaderDefinitions.GrappleTutorial2):
                return RoomStateDefinitions.GrappleTutorial2;
            case nameof(RoomHeaderDefinitions.GrappleTutorial1):
                return RoomStateDefinitions.GrappleTutorial1;
            case nameof(RoomHeaderDefinitions.GrappleBeam):
                return RoomStateDefinitions.GrappleBeam;
            case nameof(RoomHeaderDefinitions.NorfairReserveTank):
                return RoomStateDefinitions.NorfairReserveTank;
            case nameof(RoomHeaderDefinitions.GreenBubblesMissiles):
                return RoomStateDefinitions.GreenBubblesMissiles;
            case nameof(RoomHeaderDefinitions.BubbleMountain):
                return RoomStateDefinitions.BubbleMountain;
            case nameof(RoomHeaderDefinitions.SpeedBoosterHall):
                return RoomStateDefinitions.SpeedBoosterHall;
            case nameof(RoomHeaderDefinitions.SpeedBooster):
                return RoomStateDefinitions.SpeedBooster;
            case nameof(RoomHeaderDefinitions.SingleChamber):
                return RoomStateDefinitions.SingleChamber;
            case nameof(RoomHeaderDefinitions.DoubleChamber):
                return RoomStateDefinitions.DoubleChamber;
            case nameof(RoomHeaderDefinitions.WaveBeam):
                return RoomStateDefinitions.WaveBeam;
            case nameof(RoomHeaderDefinitions.SpikyPlatformsTunnel):
                return RoomStateDefinitions.SpikyPlatformsTunnel;
            case nameof(RoomHeaderDefinitions.Volcano):
                return RoomStateDefinitions.Volcano;
            case nameof(RoomHeaderDefinitions.KronicBoost):
                return RoomStateDefinitions.KronicBoost;
            case nameof(RoomHeaderDefinitions.MagdolliteTunnel):
                return RoomStateDefinitions.MagdolliteTunnel;
            case nameof(RoomHeaderDefinitions.PurpleShaft):
                return RoomStateDefinitions.PurpleShaft;
            case nameof(RoomHeaderDefinitions.LavaDive):
                return RoomStateDefinitions.LavaDive;
            case nameof(RoomHeaderDefinitions.LowerNorfairElev):
                return RoomStateDefinitions.LowerNorfairElev;
            case nameof(RoomHeaderDefinitions.UpperNorfairFarming):
                return RoomStateDefinitions.UpperNorfairFarming;
            case nameof(RoomHeaderDefinitions.RisingTide):
                return RoomStateDefinitions.RisingTide;
            case nameof(RoomHeaderDefinitions.AcidSnakesTunnel):
                return RoomStateDefinitions.AcidSnakesTunnel;
            case nameof(RoomHeaderDefinitions.SpikyAcidSnakesTunnel):
                return RoomStateDefinitions.SpikyAcidSnakesTunnel;
            case nameof(RoomHeaderDefinitions.UpperNorfairRefill):
                return RoomStateDefinitions.UpperNorfairRefill;
            case nameof(RoomHeaderDefinitions.PurpleFarming):
                return RoomStateDefinitions.PurpleFarming;
            case nameof(RoomHeaderDefinitions.BatCave):
                return RoomStateDefinitions.BatCave;
            case nameof(RoomHeaderDefinitions.NorfairMap):
                return RoomStateDefinitions.NorfairMap;
            case nameof(RoomHeaderDefinitions.BubbleMountainSave):
                return RoomStateDefinitions.BubbleMountainSave;
            case nameof(RoomHeaderDefinitions.FrogSpeedway):
                return RoomStateDefinitions.FrogSpeedway;
            case nameof(RoomHeaderDefinitions.RedPirateShaft):
                return RoomStateDefinitions.RedPirateShaft;
            case nameof(RoomHeaderDefinitions.FrogSave):
                return RoomStateDefinitions.FrogSave;
            case nameof(RoomHeaderDefinitions.CrocomireSave):
                return RoomStateDefinitions.CrocomireSave;
            case nameof(RoomHeaderDefinitions.LNElevSave):
                return RoomStateDefinitions.LNElevSave;
            case nameof(RoomHeaderDefinitions.AcidStatue):
                return RoomStateDefinitions.AcidStatue;
            case nameof(RoomHeaderDefinitions.MainHall):
                return RoomStateDefinitions.MainHall;
            case nameof(RoomHeaderDefinitions.FastRipper):
                return RoomStateDefinitions.FastRipper;
            case nameof(RoomHeaderDefinitions.GTEnergyRefill):
                return RoomStateDefinitions.GTEnergyRefill;
            case nameof(RoomHeaderDefinitions.LNFarming):
                return RoomStateDefinitions.LNFarming;
            case nameof(RoomHeaderDefinitions.FastPillarsSetup):
                return RoomStateDefinitions.FastPillarsSetup;
            case nameof(RoomHeaderDefinitions.UnusedRoom8FB3E1):
                return RoomStateDefinitions.UnusedRoom8FB3EE;
            case nameof(RoomHeaderDefinitions.MickeyMouse):
                return RoomStateDefinitions.MickeyMouse;
            case nameof(RoomHeaderDefinitions.Pillar):
                return RoomStateDefinitions.Pillar;
            case nameof(RoomHeaderDefinitions.Plowerhouse):
                return RoomStateDefinitions.Plowerhouse;
            case nameof(RoomHeaderDefinitions.WorstRoomInTheGame):
                return RoomStateDefinitions.WorstRoomInTheGame;
            case nameof(RoomHeaderDefinitions.Amphitheatre):
                return RoomStateDefinitions.Amphitheatre;
            case nameof(RoomHeaderDefinitions.LNSpringBallMaze):
                return RoomStateDefinitions.LNSpringBallMaze;
            case nameof(RoomHeaderDefinitions.LNEscapePowerBombs):
                return RoomStateDefinitions.LNEscapePowerBombs;
            case nameof(RoomHeaderDefinitions.RedKihunterShaft):
                return RoomStateDefinitions.RedKihunterShaft;
            case nameof(RoomHeaderDefinitions.Wasteland):
                return RoomStateDefinitions.Wasteland;
            case nameof(RoomHeaderDefinitions.MetalPirates):
                return RoomStateDefinitions.MetalPirates;
            case nameof(RoomHeaderDefinitions.ThreeMusketeers):
                return RoomStateDefinitions.ThreeMusketeers;
            case nameof(RoomHeaderDefinitions.RidleyETank):
                return RoomStateDefinitions.RidleyETank;
            case nameof(RoomHeaderDefinitions.ScrewAttack):
                return RoomStateDefinitions.ScrewAttack;
            case nameof(RoomHeaderDefinitions.LNFireflea):
                return RoomStateDefinitions.LNFireflea;
            case nameof(RoomHeaderDefinitions.LNSave):
                return RoomStateDefinitions.LNSave;
            case nameof(RoomHeaderDefinitions.GlassTunnelSave):
                return RoomStateDefinitions.GlassTunnelSave;
            case nameof(RoomHeaderDefinitions.WestTunnel):
                return RoomStateDefinitions.WestTunnel;
            case nameof(RoomHeaderDefinitions.EastTunnel):
                return RoomStateDefinitions.EastTunnel;
            case nameof(RoomHeaderDefinitions.MainStreet):
                return RoomStateDefinitions.MainStreet;
            case nameof(RoomHeaderDefinitions.FishTank):
                return RoomStateDefinitions.FishTank;
            case nameof(RoomHeaderDefinitions.MamaTurtle):
                return RoomStateDefinitions.MamaTurtle;
            case nameof(RoomHeaderDefinitions.CrabTunnel):
                return RoomStateDefinitions.CrabTunnel;
            case nameof(RoomHeaderDefinitions.MtEverest):
                return RoomStateDefinitions.MtEverest;
            case nameof(RoomHeaderDefinitions.RedFish):
                return RoomStateDefinitions.RedFish;
            case nameof(RoomHeaderDefinitions.WateringHole):
                return RoomStateDefinitions.WateringHole;
            case nameof(RoomHeaderDefinitions.NWestMaridiaBug):
                return RoomStateDefinitions.NWestMaridiaBug;
            case nameof(RoomHeaderDefinitions.CrabShaft):
                return RoomStateDefinitions.CrabShaft;
            case nameof(RoomHeaderDefinitions.PseudoPlasmaSpark):
                return RoomStateDefinitions.PseudoPlasmaSpark;
            case nameof(RoomHeaderDefinitions.CrabHole):
                return RoomStateDefinitions.CrabHole;
            case nameof(RoomHeaderDefinitions.WestSandHallTunnel):
                return RoomStateDefinitions.WestSandHallTunnel;
            case nameof(RoomHeaderDefinitions.PlasmaTutorial):
                return RoomStateDefinitions.PlasmaTutorial;
            case nameof(RoomHeaderDefinitions.Plasma):
                return RoomStateDefinitions.Plasma;
            case nameof(RoomHeaderDefinitions.ThreadTheNeedle):
                return RoomStateDefinitions.ThreadTheNeedle;
            case nameof(RoomHeaderDefinitions.MaridiaElev):
                return RoomStateDefinitions.MaridiaElev;
            case nameof(RoomHeaderDefinitions.PlasmaSpark):
                return RoomStateDefinitions.PlasmaSpark;
            case nameof(RoomHeaderDefinitions.Kassiuz):
                return RoomStateDefinitions.Kassiuz;
            case nameof(RoomHeaderDefinitions.MaridiaMap):
                return RoomStateDefinitions.MaridiaMap;
            case nameof(RoomHeaderDefinitions.ForgottenHighwaySave):
                return RoomStateDefinitions.ForgottenHighwaySave;
            case nameof(RoomHeaderDefinitions.Toilet):
                return RoomStateDefinitions.Toilet;
            case nameof(RoomHeaderDefinitions.BugSandHole):
                return RoomStateDefinitions.BugSandHole;
            case nameof(RoomHeaderDefinitions.WestSandHall):
                return RoomStateDefinitions.WestSandHall;
            case nameof(RoomHeaderDefinitions.Oasis):
                return RoomStateDefinitions.Oasis;
            case nameof(RoomHeaderDefinitions.EastSandHall):
                return RoomStateDefinitions.EastSandHall;
            case nameof(RoomHeaderDefinitions.WestSandHole):
                return RoomStateDefinitions.WestSandHole;
            case nameof(RoomHeaderDefinitions.EastSandHole):
                return RoomStateDefinitions.EastSandHole;
            case nameof(RoomHeaderDefinitions.WestAqueductQuicksand):
                return RoomStateDefinitions.WestAqueductQuicksand;
            case nameof(RoomHeaderDefinitions.EastAqueductQuicksand):
                return RoomStateDefinitions.EastAqueductQuicksand;
            case nameof(RoomHeaderDefinitions.Aqueduct):
                return RoomStateDefinitions.Aqueduct;
            case nameof(RoomHeaderDefinitions.Butterfly):
                return RoomStateDefinitions.Butterfly;
            case nameof(RoomHeaderDefinitions.BotwoonHallway):
                return RoomStateDefinitions.BotwoonHallway;
            case nameof(RoomHeaderDefinitions.Pants):
                return RoomStateDefinitions.Pants;
            case nameof(RoomHeaderDefinitions.EastPants):
                return RoomStateDefinitions.EastPants;
            case nameof(RoomHeaderDefinitions.Springball):
                return RoomStateDefinitions.Springball;
            case nameof(RoomHeaderDefinitions.BelowBotwoonETank):
                return RoomStateDefinitions.BelowBotwoonETank;
            case nameof(RoomHeaderDefinitions.Colosseum):
                return RoomStateDefinitions.Colosseum;
            case nameof(RoomHeaderDefinitions.AqueductSave):
                return RoomStateDefinitions.AqueductSave;
            case nameof(RoomHeaderDefinitions.BotwoonETank):
                return RoomStateDefinitions.BotwoonETank;
            case nameof(RoomHeaderDefinitions.DraygonSave):
                return RoomStateDefinitions.DraygonSave;
            case nameof(RoomHeaderDefinitions.MaridiaMissileRefill):
                return RoomStateDefinitions.MaridiaMissileRefill;
            case nameof(RoomHeaderDefinitions.PlasmaBeachQuicksand):
                return RoomStateDefinitions.PlasmaBeachQuicksand;
            case nameof(RoomHeaderDefinitions.BotwoonQuicksand):
                return RoomStateDefinitions.BotwoonQuicksand;
            case nameof(RoomHeaderDefinitions.HalfieClimb):
                return RoomStateDefinitions.HalfieClimb;
            case nameof(RoomHeaderDefinitions.SpaceJump):
                return RoomStateDefinitions.SpaceJump;
            case nameof(RoomHeaderDefinitions.MaridiaEnergyRefill):
                return RoomStateDefinitions.MaridiaEnergyRefill;
            case nameof(RoomHeaderDefinitions.WestCactusAlley):
                return RoomStateDefinitions.WestCactusAlley;
            case nameof(RoomHeaderDefinitions.EastCactusAlley):
                return RoomStateDefinitions.EastCactusAlley;
            case nameof(RoomHeaderDefinitions.TourianFirst):
                return RoomStateDefinitions.TourianFirst;
            case nameof(RoomHeaderDefinitions.Seaweed):
                return RoomStateDefinitions.Seaweed;
            case nameof(RoomHeaderDefinitions.TourianRecharge):
                return RoomStateDefinitions.TourianRecharge;
            case nameof(RoomHeaderDefinitions.TourianEyeDoor):
                return RoomStateDefinitions.TourianEyeDoor;
            case nameof(RoomHeaderDefinitions.RinkaShaft):
                return RoomStateDefinitions.RinkaShaft;
            case nameof(RoomHeaderDefinitions.MotherBrainSave):
                return RoomStateDefinitions.MotherBrainSave;
            case nameof(RoomHeaderDefinitions.TourianEscape1):
                return RoomStateDefinitions.TourianEscape1;
            case nameof(RoomHeaderDefinitions.TourianEscape2):
                return RoomStateDefinitions.TourianEscape2;
            case nameof(RoomHeaderDefinitions.TourianEscape3):
                return RoomStateDefinitions.TourianEscape3;
            case nameof(RoomHeaderDefinitions.TourianEscape4):
                return RoomStateDefinitions.TourianEscape4;
            case nameof(RoomHeaderDefinitions.UpperTourianSave):
                return RoomStateDefinitions.UpperTourianSave;
            default:
                throw new ArgumentOutOfRangeException(nameof(room), room.Name,
                    "Room is not one of the 262 compiled retail rooms.");
        }
    }
}
