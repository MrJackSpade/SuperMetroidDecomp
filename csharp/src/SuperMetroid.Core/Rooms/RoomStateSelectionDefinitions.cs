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
    public static ushort Select(RoomHeaderDefinition room, RoomStateSelectionContext selection) =>
        Resolve(Ensure.NotNull(room), selection, null);

    /// <summary>Returns the default first, then every alternative in native order.
    /// The caller-owned view is constructed on demand from the same branches;
    /// it is never cached as a second program table.</summary>
    public static IReadOnlyList<ushort> GetStatePointers(RoomHeaderDefinition room)
    {
        Ensure.NotNull(room);
        var variants = new List<ushort>();
        ushort fallback = Resolve(room, default, variants);
        variants.Insert(0, fallback);
        return variants.AsReadOnly();
    }

    /// <summary>Evaluates each room's native conditional branches in order, or records their alternatives before the default state.</summary>
    private static ushort Resolve(RoomHeaderDefinition room, RoomStateSelectionContext selection, List<ushort>? variants)
    {
        ushort target = 0;
        bool Match(bool condition, ushort candidate)
        {
            target = candidate;
            variants?.Add(candidate);
            // Enumeration records every branch and reaches the default. Runtime
            // selection returns immediately on its first satisfied condition.
            return variants is null && condition;
        }
        switch (room.Name)
        {
            case nameof(RoomHeaderDefinitions.LandingSite):
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesTimebombSet), 0x9261)) return target;
                if (Match(selection.HasPowerBombs, 0x9247)) return target;
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesAwake), 0x922d)) return target;
                return 0x9213;
            case nameof(RoomHeaderDefinitions.GauntletEntrance):
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesAwake), 0x92df)) return target;
                return 0x92c5;
            case nameof(RoomHeaderDefinitions.Parlor):
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesTimebombSet), 0x9348)) return target;
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesAwake), 0x932e)) return target;
                return 0x9314;
            case nameof(RoomHeaderDefinitions.Climb):
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesTimebombSet), 0x9705)) return target;
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesAwake), 0x96eb)) return target;
                return 0x96d1;
            case nameof(RoomHeaderDefinitions.Pit):
                if (Match(selection.HasMorphBallAndMissiles, 0x9787)) return target;
                return 0x976d;
            case nameof(RoomHeaderDefinitions.ElevToBlueBrinstar):
                if (Match(selection.HasMorphBallAndMissiles, 0x97e0)) return target;
                return 0x97c6;
            case nameof(RoomHeaderDefinitions.BombTorizo):
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesTimebombSet), 0x984f)) return target;
                if (Match(selection.IsBossDead(BossBits.AreaTorizo), 0x9835)) return target;
                return 0x981b;
            case nameof(RoomHeaderDefinitions.Flyway):
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesTimebombSet), 0x98c4)) return target;
                if (Match(selection.IsBossDead(BossBits.AreaTorizo), 0x98aa)) return target;
                return 0x9890;
            case nameof(RoomHeaderDefinitions.FinalMissileBombway):
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesAwake), 0x9a70)) return target;
                return 0x9a56;
            case nameof(RoomHeaderDefinitions.FinalMissile):
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesAwake), 0x9abc)) return target;
                return 0x9aa2;
            case nameof(RoomHeaderDefinitions.SporeSpawn):
                if (Match(selection.IsBossDead(BossBits.AreaMiniBoss), 0x9df3)) return target;
                return 0x9dd9;
            case nameof(RoomHeaderDefinitions.MorphBall):
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesAwake), 0x9ecb)) return target;
                return 0x9eb1;
            case nameof(RoomHeaderDefinitions.ConstructionZone):
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesAwake), 0x9f3d)) return target;
                return 0x9f23;
            case nameof(RoomHeaderDefinitions.BlueBrinstarETank):
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesAwake), 0x9f90)) return target;
                return 0x9f76;
            case nameof(RoomHeaderDefinitions.MiniKraid):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xa54d)) return target;
                return 0xa533;
            case nameof(RoomHeaderDefinitions.Kraid):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xa5cb)) return target;
                return 0xa5b1;
            case nameof(RoomHeaderDefinitions.Crocomire):
                if (Match(selection.IsBossDead(BossBits.AreaMiniBoss), 0xa9b9)) return target;
                return 0xa99f;
            case nameof(RoomHeaderDefinitions.GoldenTorizo):
                if (Match(selection.IsBossDead(BossBits.AreaTorizo), 0xb2af)) return target;
                return 0xb295;
            case nameof(RoomHeaderDefinitions.Ridley):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xb35a)) return target;
                return 0xb340;
            case nameof(RoomHeaderDefinitions.BowlingAlley):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xc9ba)) return target;
                return 0xc9a0;
            case nameof(RoomHeaderDefinitions.WreckedShipEntrance):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xca34)) return target;
                return 0xca1a;
            case nameof(RoomHeaderDefinitions.Attic):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xca7e)) return target;
                return 0xca64;
            case nameof(RoomHeaderDefinitions.AssemblyLine):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xcada)) return target;
                return 0xcac0;
            case nameof(RoomHeaderDefinitions.WreckedShipMainShaft):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xcb22)) return target;
                return 0xcb08;
            case nameof(RoomHeaderDefinitions.SpikyDeath):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xcbb7)) return target;
                return 0xcb9d;
            case nameof(RoomHeaderDefinitions.ElectricDeath):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xcc01)) return target;
                return 0xcbe7;
            case nameof(RoomHeaderDefinitions.WreckedShipETank):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xcc53)) return target;
                return 0xcc39;
            case nameof(RoomHeaderDefinitions.Basement):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xcc9b)) return target;
                return 0xcc81;
            case nameof(RoomHeaderDefinitions.WreckedShipMap):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xccf7)) return target;
                return 0xccdd;
            case nameof(RoomHeaderDefinitions.Phantoon):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xcd3f)) return target;
                return 0xcd25;
            case nameof(RoomHeaderDefinitions.SpongeBath):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xcd88)) return target;
                return 0xcd6e;
            case nameof(RoomHeaderDefinitions.WSWestSuper):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xcdd4)) return target;
                return 0xcdba;
            case nameof(RoomHeaderDefinitions.WSEastSuper):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xce1d)) return target;
                return 0xce03;
            case nameof(RoomHeaderDefinitions.GravitySuit):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xce6c)) return target;
                return 0xce52;
            case nameof(RoomHeaderDefinitions.WreckedShipSave):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xceb6)) return target;
                return 0xce9c;
            case nameof(RoomHeaderDefinitions.GlassTunnel):
                if (Match(selection.IsEventSet((byte)EventNumber.MaridiaNoobTubeBroken), 0xcf27)) return target;
                return 0xcf0d;
            case nameof(RoomHeaderDefinitions.ThePrecious):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xd7bb)) return target;
                return 0xd7a1;
            case nameof(RoomHeaderDefinitions.Shaktool):
                if (Match(selection.IsEventSet((byte)EventNumber.ShaktoolClearedPath), 0xd8f1)) return target;
                return 0xd8d7;
            case nameof(RoomHeaderDefinitions.Botwoon):
                if (Match(selection.IsBossDead(BossBits.AreaMiniBoss), 0xd98a)) return target;
                return 0xd970;
            case nameof(RoomHeaderDefinitions.Draygon):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xda8c)) return target;
                return 0xda72;
            case nameof(RoomHeaderDefinitions.Metroids1):
                if (Match(selection.IsEventSet((byte)EventNumber.FirstMetroidHallCleared), 0xdb0d)) return target;
                return 0xdaf3;
            case nameof(RoomHeaderDefinitions.Metroids2):
                if (Match(selection.IsEventSet((byte)EventNumber.FirstMetroidShaftCleared), 0xdb5d)) return target;
                return 0xdb43;
            case nameof(RoomHeaderDefinitions.Metroids3):
                if (Match(selection.IsEventSet((byte)EventNumber.SecondMetroidHallCleared), 0xdba9)) return target;
                return 0xdb8f;
            case nameof(RoomHeaderDefinitions.Metroids4):
                if (Match(selection.IsEventSet((byte)EventNumber.SecondMetroidShaftCleared), 0xdbf9)) return target;
                return 0xdbdf;
            case nameof(RoomHeaderDefinitions.BlueHopper):
                if (Match(selection.IsEventSet((byte)EventNumber.Unused14), 0xdc45)) return target;
                return 0xdc2b;
            case nameof(RoomHeaderDefinitions.DustTorizo):
                if (Match(selection.IsEventSet((byte)EventNumber.Unused14), 0xdc91)) return target;
                return 0xdc77;
            case nameof(RoomHeaderDefinitions.BigBoy):
                if (Match(selection.IsEventSet((byte)EventNumber.Unused14), 0xdcdd)) return target;
                return 0xdcc3;
            case nameof(RoomHeaderDefinitions.MotherBrain):
                if (Match(selection.IsBossDead(RoomStateSelectorOperands.MainAreaBoss), 0xdda2)) return target;
                if (Match(selection.IsEventSet((byte)EventNumber.MotherBrainGlassDestroyed), 0xdd88)) return target;
                return 0xdd6e;
            case nameof(RoomHeaderDefinitions.CeresElev):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xdf71)) return target;
                return 0xdf57;
            case nameof(RoomHeaderDefinitions.FallingTile):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xdfb9)) return target;
                return 0xdf9f;
            case nameof(RoomHeaderDefinitions.MagnetStairs):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xe003)) return target;
                return 0xdfe9;
            case nameof(RoomHeaderDefinitions.DeadScientist):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xe04d)) return target;
                return 0xe033;
            case nameof(RoomHeaderDefinitions.Room58Escape):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xe097)) return target;
                return 0xe07d;
            case nameof(RoomHeaderDefinitions.CeresRidley):
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xe0e1)) return target;
                return 0xe0c7;
            case nameof(RoomHeaderDefinitions.CrateriaPowerBombs):
                return 0x93b7;
            case nameof(RoomHeaderDefinitions.CrateriaSave):
                return 0x93e2;
            case nameof(RoomHeaderDefinitions.WestOcean):
                return 0x940b;
            case nameof(RoomHeaderDefinitions.BowlingAlleyPath):
                return 0x946e;
            case nameof(RoomHeaderDefinitions.CrateriaKihunter):
                return 0x9499;
            case nameof(RoomHeaderDefinitions.ForgottenHighwayElev):
                return 0x94d9;
            case nameof(RoomHeaderDefinitions.EastOcean):
                return 0x950a;
            case nameof(RoomHeaderDefinitions.ForgottenHighwayKagos):
                return 0x955f;
            case nameof(RoomHeaderDefinitions.CrabMaze):
                return 0x958a;
            case nameof(RoomHeaderDefinitions.ForgottenHighwayElbow):
                return 0x95b5;
            case nameof(RoomHeaderDefinitions.CrateriaTube):
                return 0x95e1;
            case nameof(RoomHeaderDefinitions.Moat):
                return 0x960c;
            case nameof(RoomHeaderDefinitions.RedBinstarElev):
                return 0x9637;
            case nameof(RoomHeaderDefinitions.GauntletETank):
                return 0x9668;
            case nameof(RoomHeaderDefinitions.PreBowling):
                return 0x969c;
            case nameof(RoomHeaderDefinitions.PreMapFlyway):
                return 0x98ef;
            case nameof(RoomHeaderDefinitions.Terminator):
                return 0x991a;
            case nameof(RoomHeaderDefinitions.GreenBrinstarElev):
                return 0x9945;
            case nameof(RoomHeaderDefinitions.LowerMushrooms):
                return 0x9976;
            case nameof(RoomHeaderDefinitions.CrateriaMap):
                return 0x99a1;
            case nameof(RoomHeaderDefinitions.GreenPiratesShaft):
                return 0x99ca;
            case nameof(RoomHeaderDefinitions.CrateriaSuper):
                return 0x9a06;
            case nameof(RoomHeaderDefinitions.GreenBrinstarMainShaft):
                return 0x9ae6;
            case nameof(RoomHeaderDefinitions.SporeSpawnSuper):
                return 0x9b68;
            case nameof(RoomHeaderDefinitions.BrinstarPreMap):
                return 0x9baa;
            case nameof(RoomHeaderDefinitions.EarlySupers):
                return 0x9bd5;
            case nameof(RoomHeaderDefinitions.BrinstarReserveTank):
                return 0x9c14;
            case nameof(RoomHeaderDefinitions.GreenBrinstarMap):
                return 0x9c42;
            case nameof(RoomHeaderDefinitions.GreenBrinstarFirefleas):
                return 0x9c6b;
            case nameof(RoomHeaderDefinitions.GreenBrinstarMissileRefill):
                return 0x9c96;
            case nameof(RoomHeaderDefinitions.Dachora):
                return 0x9cc0;
            case nameof(RoomHeaderDefinitions.BigPink):
                return 0x9d26;
            case nameof(RoomHeaderDefinitions.SporeSpawnKihunters):
                return 0x9da9;
            case nameof(RoomHeaderDefinitions.PinkBrinstarPowerBombs):
                return 0x9e1e;
            case nameof(RoomHeaderDefinitions.GreenHillZone):
                return 0x9e5f;
            case nameof(RoomHeaderDefinitions.NoobBridge):
                return 0x9fc7;
            case nameof(RoomHeaderDefinitions.GreenBrinstarBeetoms):
                return 0x9ff2;
            case nameof(RoomHeaderDefinitions.EtecoonETank):
                return 0xa01e;
            case nameof(RoomHeaderDefinitions.EtecoonSuper):
                return 0xa05e;
            case nameof(RoomHeaderDefinitions.DachoraEnergyRefill):
                return 0xa088;
            case nameof(RoomHeaderDefinitions.SporeSpawnFarming):
                return 0xa0b1;
            case nameof(RoomHeaderDefinitions.WaterwayETank):
                return 0xa0df;
            case nameof(RoomHeaderDefinitions.FirstMissile):
                return 0xa114;
            case nameof(RoomHeaderDefinitions.PinkBrinstarHoppers):
                return 0xa13d;
            case nameof(RoomHeaderDefinitions.HopperETank):
                return 0xa168;
            case nameof(RoomHeaderDefinitions.BigPinkSaveRoom):
                return 0xa191;
            case nameof(RoomHeaderDefinitions.BlueBrinstarBoulders):
                return 0xa1ba;
            case nameof(RoomHeaderDefinitions.BillyMays):
                return 0xa1e5;
            case nameof(RoomHeaderDefinitions.GreenBrinstarSave):
                return 0xa20e;
            case nameof(RoomHeaderDefinitions.EtecoonSave):
                return 0xa237;
            case nameof(RoomHeaderDefinitions.RedTower):
                return 0xa260;
            case nameof(RoomHeaderDefinitions.RedBrinstarFirefleas):
                return 0xa2a0;
            case nameof(RoomHeaderDefinitions.XrayScope):
                return 0xa2db;
            case nameof(RoomHeaderDefinitions.Hellway):
                return 0xa304;
            case nameof(RoomHeaderDefinitions.Caterpillar):
                return 0xa32f;
            case nameof(RoomHeaderDefinitions.BetaPowerBomb):
                return 0xa389;
            case nameof(RoomHeaderDefinitions.AlphaPowerBomb):
                return 0xa3bb;
            case nameof(RoomHeaderDefinitions.SkreeBoost):
                return 0xa3ea;
            case nameof(RoomHeaderDefinitions.BelowSpazer):
                return 0xa415;
            case nameof(RoomHeaderDefinitions.Spazer):
                return 0xa454;
            case nameof(RoomHeaderDefinitions.WarehouseZeela):
                return 0xa47e;
            case nameof(RoomHeaderDefinitions.WarehouseETank):
                return 0xa4be;
            case nameof(RoomHeaderDefinitions.WarehouseKihunter):
                return 0xa4e7;
            case nameof(RoomHeaderDefinitions.KraidEyeDoor):
                return 0xa578;
            case nameof(RoomHeaderDefinitions.StatuesHallway):
                return 0xa5fa;
            case nameof(RoomHeaderDefinitions.RedTowerRefill):
                return 0xa625;
            case nameof(RoomHeaderDefinitions.KraidRefill):
                return 0xa64e;
            case nameof(RoomHeaderDefinitions.Statues):
                return 0xa677;
            case nameof(RoomHeaderDefinitions.WarehouseEntrance):
                return 0xa6ae;
            case nameof(RoomHeaderDefinitions.VariaSuit):
                return 0xa6ef;
            case nameof(RoomHeaderDefinitions.WarehouseSave):
                return 0xa718;
            case nameof(RoomHeaderDefinitions.RedBrinstarSave):
                return 0xa741;
            case nameof(RoomHeaderDefinitions.IceBeamAcid):
                return 0xa76a;
            case nameof(RoomHeaderDefinitions.Cathedral):
                return 0xa795;
            case nameof(RoomHeaderDefinitions.CathedralEntrance):
                return 0xa7c0;
            case nameof(RoomHeaderDefinitions.BusinessCenter):
                return 0xa7eb;
            case nameof(RoomHeaderDefinitions.IceBeamGate):
                return 0xa822;
            case nameof(RoomHeaderDefinitions.IceBeamTutorial):
                return 0xa872;
            case nameof(RoomHeaderDefinitions.IceBeam):
                return 0xa89d;
            case nameof(RoomHeaderDefinitions.IceBeamSnake):
                return 0xa8c6;
            case nameof(RoomHeaderDefinitions.CrumbleShaft):
                return 0xa905;
            case nameof(RoomHeaderDefinitions.CrocomireSpeedway):
                return 0xa930;
            case nameof(RoomHeaderDefinitions.HiJumpBoots):
                return 0xa9f2;
            case nameof(RoomHeaderDefinitions.CrocomireEscape):
                return 0xaa1b;
            case nameof(RoomHeaderDefinitions.HiJumpETank):
                return 0xaa4e;
            case nameof(RoomHeaderDefinitions.PostCrocFarming):
                return 0xaa8f;
            case nameof(RoomHeaderDefinitions.PostCrocSave):
                return 0xaac2;
            case nameof(RoomHeaderDefinitions.PostCrocPowerBombs):
                return 0xaaeb;
            case nameof(RoomHeaderDefinitions.PostCrocShaft):
                return 0xab14;
            case nameof(RoomHeaderDefinitions.PostCrocMissile):
                return 0xab48;
            case nameof(RoomHeaderDefinitions.GrappleTutorial3):
                return 0xab71;
            case nameof(RoomHeaderDefinitions.PostCrocJump):
                return 0xab9c;
            case nameof(RoomHeaderDefinitions.GrappleTutorial2):
                return 0xabdf;
            case nameof(RoomHeaderDefinitions.GrappleTutorial1):
                return 0xac0d;
            case nameof(RoomHeaderDefinitions.GrappleBeam):
                return 0xac38;
            case nameof(RoomHeaderDefinitions.NorfairReserveTank):
                return 0xac67;
            case nameof(RoomHeaderDefinitions.GreenBubblesMissiles):
                return 0xac90;
            case nameof(RoomHeaderDefinitions.BubbleMountain):
                return 0xacc0;
            case nameof(RoomHeaderDefinitions.SpeedBoosterHall):
                return 0xacfd;
            case nameof(RoomHeaderDefinitions.SpeedBooster):
                return 0xad28;
            case nameof(RoomHeaderDefinitions.SingleChamber):
                return 0xad6b;
            case nameof(RoomHeaderDefinitions.DoubleChamber):
                return 0xadba;
            case nameof(RoomHeaderDefinitions.WaveBeam):
                return 0xadeb;
            case nameof(RoomHeaderDefinitions.SpikyPlatformsTunnel):
                return 0xae14;
            case nameof(RoomHeaderDefinitions.Volcano):
                return 0xae3f;
            case nameof(RoomHeaderDefinitions.KronicBoost):
                return 0xae81;
            case nameof(RoomHeaderDefinitions.MagdolliteTunnel):
                return 0xaec1;
            case nameof(RoomHeaderDefinitions.PurpleShaft):
                return 0xaeec;
            case nameof(RoomHeaderDefinitions.LavaDive):
                return 0xaf21;
            case nameof(RoomHeaderDefinitions.LowerNorfairElev):
                return 0xaf4c;
            case nameof(RoomHeaderDefinitions.UpperNorfairFarming):
                return 0xaf7f;
            case nameof(RoomHeaderDefinitions.RisingTide):
                return 0xafb0;
            case nameof(RoomHeaderDefinitions.AcidSnakesTunnel):
                return 0xafdb;
            case nameof(RoomHeaderDefinitions.SpikyAcidSnakesTunnel):
                return 0xb008;
            case nameof(RoomHeaderDefinitions.UpperNorfairRefill):
                return 0xb033;
            case nameof(RoomHeaderDefinitions.PurpleFarming):
                return 0xb05e;
            case nameof(RoomHeaderDefinitions.BatCave):
                return 0xb087;
            case nameof(RoomHeaderDefinitions.NorfairMap):
                return 0xb0c1;
            case nameof(RoomHeaderDefinitions.BubbleMountainSave):
                return 0xb0ea;
            case nameof(RoomHeaderDefinitions.FrogSpeedway):
                return 0xb113;
            case nameof(RoomHeaderDefinitions.RedPirateShaft):
                return 0xb146;
            case nameof(RoomHeaderDefinitions.FrogSave):
                return 0xb174;
            case nameof(RoomHeaderDefinitions.CrocomireSave):
                return 0xb19f;
            case nameof(RoomHeaderDefinitions.LNElevSave):
                return 0xb1c8;
            case nameof(RoomHeaderDefinitions.AcidStatue):
                return 0xb1f2;
            case nameof(RoomHeaderDefinitions.MainHall):
                return 0xb243;
            case nameof(RoomHeaderDefinitions.FastRipper):
                return 0xb2e7;
            case nameof(RoomHeaderDefinitions.GTEnergyRefill):
                return 0xb312;
            case nameof(RoomHeaderDefinitions.LNFarming):
                return 0xb387;
            case nameof(RoomHeaderDefinitions.FastPillarsSetup):
                return 0xb3b2;
            case nameof(RoomHeaderDefinitions.UnusedRoom8FB3E1):
                return 0xb3ee;
            case nameof(RoomHeaderDefinitions.MickeyMouse):
                return 0xb417;
            case nameof(RoomHeaderDefinitions.Pillar):
                return 0xb464;
            case nameof(RoomHeaderDefinitions.Plowerhouse):
                return 0xb48f;
            case nameof(RoomHeaderDefinitions.WorstRoomInTheGame):
                return 0xb4ba;
            case nameof(RoomHeaderDefinitions.Amphitheatre):
                return 0xb4f2;
            case nameof(RoomHeaderDefinitions.LNSpringBallMaze):
                return 0xb51d;
            case nameof(RoomHeaderDefinitions.LNEscapePowerBombs):
                return 0xb567;
            case nameof(RoomHeaderDefinitions.RedKihunterShaft):
                return 0xb592;
            case nameof(RoomHeaderDefinitions.Wasteland):
                return 0xb5e2;
            case nameof(RoomHeaderDefinitions.MetalPirates):
                return 0xb638;
            case nameof(RoomHeaderDefinitions.ThreeMusketeers):
                return 0xb663;
            case nameof(RoomHeaderDefinitions.RidleyETank):
                return 0xb6a5;
            case nameof(RoomHeaderDefinitions.ScrewAttack):
                return 0xb6ce;
            case nameof(RoomHeaderDefinitions.LNFireflea):
                return 0xb6fb;
            case nameof(RoomHeaderDefinitions.LNSave):
                return 0xb74e;
            case nameof(RoomHeaderDefinitions.GlassTunnelSave):
                return 0xcedf;
            case nameof(RoomHeaderDefinitions.WestTunnel):
                return 0xcf61;
            case nameof(RoomHeaderDefinitions.EastTunnel):
                return 0xcf8d;
            case nameof(RoomHeaderDefinitions.MainStreet):
                return 0xcfd6;
            case nameof(RoomHeaderDefinitions.FishTank):
                return 0xd024;
            case nameof(RoomHeaderDefinitions.MamaTurtle):
                return 0xd062;
            case nameof(RoomHeaderDefinitions.CrabTunnel):
                return 0xd097;
            case nameof(RoomHeaderDefinitions.MtEverest):
                return 0xd0c6;
            case nameof(RoomHeaderDefinitions.RedFish):
                return 0xd111;
            case nameof(RoomHeaderDefinitions.WateringHole):
                return 0xd148;
            case nameof(RoomHeaderDefinitions.NWestMaridiaBug):
                return 0xd17a;
            case nameof(RoomHeaderDefinitions.CrabShaft):
                return 0xd1b0;
            case nameof(RoomHeaderDefinitions.PseudoPlasmaSpark):
                return 0xd1ea;
            case nameof(RoomHeaderDefinitions.CrabHole):
                return 0xd229;
            case nameof(RoomHeaderDefinitions.WestSandHallTunnel):
                return 0xd25f;
            case nameof(RoomHeaderDefinitions.PlasmaTutorial):
                return 0xd28b;
            case nameof(RoomHeaderDefinitions.Plasma):
                return 0xd2b7;
            case nameof(RoomHeaderDefinitions.ThreadTheNeedle):
                return 0xd2e6;
            case nameof(RoomHeaderDefinitions.MaridiaElev):
                return 0xd318;
            case nameof(RoomHeaderDefinitions.PlasmaSpark):
                return 0xd34d;
            case nameof(RoomHeaderDefinitions.Kassiuz):
                return 0xd394;
            case nameof(RoomHeaderDefinitions.MaridiaMap):
                return 0xd3c3;
            case nameof(RoomHeaderDefinitions.ForgottenHighwaySave):
                return 0xd3ec;
            case nameof(RoomHeaderDefinitions.Toilet):
                return 0xd415;
            case nameof(RoomHeaderDefinitions.BugSandHole):
                return 0xd440;
            case nameof(RoomHeaderDefinitions.WestSandHall):
                return 0xd46e;
            case nameof(RoomHeaderDefinitions.Oasis):
                return 0xd49b;
            case nameof(RoomHeaderDefinitions.EastSandHall):
                return 0xd4cf;
            case nameof(RoomHeaderDefinitions.WestSandHole):
                return 0xd4fc;
            case nameof(RoomHeaderDefinitions.EastSandHole):
                return 0xd52b;
            case nameof(RoomHeaderDefinitions.WestAqueductQuicksand):
                return 0xd55a;
            case nameof(RoomHeaderDefinitions.EastAqueductQuicksand):
                return 0xd587;
            case nameof(RoomHeaderDefinitions.Aqueduct):
                return 0xd5b4;
            case nameof(RoomHeaderDefinitions.Butterfly):
                return 0xd5f9;
            case nameof(RoomHeaderDefinitions.BotwoonHallway):
                return 0xd624;
            case nameof(RoomHeaderDefinitions.Pants):
                return 0xd653;
            case nameof(RoomHeaderDefinitions.EastPants):
                return 0xd6a7;
            case nameof(RoomHeaderDefinitions.Springball):
                return 0xd6dd;
            case nameof(RoomHeaderDefinitions.BelowBotwoonETank):
                return 0xd70a;
            case nameof(RoomHeaderDefinitions.Colosseum):
                return 0xd737;
            case nameof(RoomHeaderDefinitions.AqueductSave):
                return 0xd772;
            case nameof(RoomHeaderDefinitions.BotwoonETank):
                return 0xd7f1;
            case nameof(RoomHeaderDefinitions.DraygonSave):
                return 0xd827;
            case nameof(RoomHeaderDefinitions.MaridiaMissileRefill):
                return 0xd852;
            case nameof(RoomHeaderDefinitions.PlasmaBeachQuicksand):
                return 0xd87b;
            case nameof(RoomHeaderDefinitions.BotwoonQuicksand):
                return 0xd8a5;
            case nameof(RoomHeaderDefinitions.HalfieClimb):
                return 0xd920;
            case nameof(RoomHeaderDefinitions.SpaceJump):
                return 0xd9b7;
            case nameof(RoomHeaderDefinitions.MaridiaEnergyRefill):
                return 0xd9e1;
            case nameof(RoomHeaderDefinitions.WestCactusAlley):
                return 0xda0b;
            case nameof(RoomHeaderDefinitions.EastCactusAlley):
                return 0xda38;
            case nameof(RoomHeaderDefinitions.TourianFirst):
                return 0xdabb;
            case nameof(RoomHeaderDefinitions.Seaweed):
                return 0xdd0c;
            case nameof(RoomHeaderDefinitions.TourianRecharge):
                return 0xdd3b;
            case nameof(RoomHeaderDefinitions.TourianEyeDoor):
                return 0xddd1;
            case nameof(RoomHeaderDefinitions.RinkaShaft):
                return 0xde00;
            case nameof(RoomHeaderDefinitions.MotherBrainSave):
                return 0xde30;
            case nameof(RoomHeaderDefinitions.TourianEscape1):
                return 0xde5a;
            case nameof(RoomHeaderDefinitions.TourianEscape2):
                return 0xde87;
            case nameof(RoomHeaderDefinitions.TourianEscape3):
                return 0xdeb4;
            case nameof(RoomHeaderDefinitions.TourianEscape4):
                return 0xdeeb;
            case nameof(RoomHeaderDefinitions.UpperTourianSave):
                return 0xdf28;
            default:
                throw new ArgumentOutOfRangeException(nameof(room), room.Name,
                    "Room is not one of the 262 compiled retail rooms.");
        }
    }
}
