namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The states enemy entities hold, read through their own members (#2554): a check that sets an enemy up through its
/// production entity reads the state the entity built. Generated from the entities' state properties; regenerate it when
/// an entity's state properties change.
/// </summary>
internal static class EnemyEntityStates
{
    /// <summary>The first <typeparamref name="T"/> <paramref name="entity"/> holds, or none.</summary>
    /// <typeparam name="T">The species state.</typeparam>
    /// <param name="entity">The entity, or none.</param>
    internal static T? Of<T>(IEnemyEntity? entity) where T : class
    {
        if (entity is null)
            return null;
        if (entity is global::SuperMetroid.Core.Bosses.Botwoon.BotwoonEntity botwoon && botwoon.State is T heldBotwoon)
            return heldBotwoon;
        if (entity is global::SuperMetroid.Core.Bosses.Crocomire.CrocomireBodyEntity entity8 && entity8.Encounter is T held8)
            return held8;
        if (entity is global::SuperMetroid.Core.Bosses.Crocomire.CrocomireEntity entity9 && entity9.Encounter is T held9)
            return held9;
        if (entity is global::SuperMetroid.Core.Bosses.Crocomire.CrocomireTongueEntity entity11 && entity11.Encounter is T held11)
            return held11;
        if (entity is global::SuperMetroid.Core.Bosses.Draygon.DraygonEntity entity13 && entity13.Encounter is T held13)
            return held13;
        if (entity is global::SuperMetroid.Core.Bosses.Kraid.KraidArmEntity entity15 && entity15.Part is T held15)
            return held15;
        if (entity is global::SuperMetroid.Core.Bosses.Kraid.KraidEntity entity16 && entity16.Encounter is T held16)
            return held16;
        if (entity is global::SuperMetroid.Core.Bosses.Kraid.KraidFootEntity entity17 && entity17.Part is T held17)
            return held17;
        if (entity is global::SuperMetroid.Core.Bosses.Kraid.KraidLimbEntity entity18 && entity18.Part is T held18)
            return held18;
        if (entity is global::SuperMetroid.Core.Bosses.Kraid.KraidNailEntity entity19 && entity19.Part is T held19)
            return held19;
        if (entity is global::SuperMetroid.Core.Bosses.MotherBrain.BabyMetroidEntity entity21 && entity21.State is T held21)
            return held21;
        if (entity is global::SuperMetroid.Core.Bosses.MotherBrain.MotherBrainBodyEntity entity22 && entity22.Encounter is T held22)
            return held22;
        if (entity is global::SuperMetroid.Core.Bosses.MotherBrain.MotherBrainEntity entity23 && entity23.Encounter is T held23)
            return held23;
        if (entity is global::SuperMetroid.Core.Bosses.MotherBrain.MotherBrainHeadEntity entity24 && entity24.Encounter is T held24)
            return held24;
        if (entity is global::SuperMetroid.Core.Bosses.MotherBrain.MotherBrainTubesEntity entity25 && entity25.Encounter is T held25)
            return held25;
        if (entity is global::SuperMetroid.Core.Bosses.MotherBrain.ZebetiteEntity entity28 && entity28.State is T held28)
            return held28;
        if (entity is global::SuperMetroid.Core.Bosses.Phantoon.PhantoonEntity entity29 && entity29.Encounter is T held29)
            return held29;
        // Each of Phantoon's places holds its own part of the encounter too, which the replay's view of a cleared Phantoon
        // and the snapshot import read (#2554).
        if (entity is global::SuperMetroid.Core.Bosses.Phantoon.PhantoonEntity phantoonPart && phantoonPart.Part is T heldPart)
            return heldPart;
        if (entity is global::SuperMetroid.Core.Bosses.Ridley.CeresRidleyEntity entity30 && entity30.Encounter is T held30)
            return held30;
        if (entity is global::SuperMetroid.Core.Bosses.Ridley.NorfairRidleyEntity entity31 && entity31.Encounter is T held31)
            return held31;
        if (entity is global::SuperMetroid.Core.Bosses.Ridley.RidleyBreakupPieceEntity entity32 && entity32.Fragment is T held32)
            return held32;
        if (entity is global::SuperMetroid.Core.Bosses.Ridley.RidleyEntity entity33 && entity33.Encounter is T held33)
            return held33;
        if (entity is global::SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnEntity entity35 && entity35.Encounter is T held35)
            return held35;
        if (entity is global::SuperMetroid.Core.Bosses.Torizo.TorizoEntity entity36 && entity36.State is T held36)
            return held36;
        if (entity is global::SuperMetroid.Core.Enemies.Animals.DachoraEchoEntity entity38 && entity38.State is T held38)
            return held38;
        if (entity is global::SuperMetroid.Core.Enemies.Animals.DachoraEntity entity39 && entity39.State is T held39)
            return held39;
        if (entity is global::SuperMetroid.Core.Enemies.Corpses.DeadSidehopperEntity entity51 && entity51.State is T held51)
            return held51;
        if (entity is global::SuperMetroid.Core.Enemies.Corpses.DeadTorizoEntity entity52 && entity52.State is T held52)
            return held52;
        if (entity is global::SuperMetroid.Core.Enemies.Crawlers.CrawlerEntity entity54 && entity54.State is T held54)
            return held54;
        if (entity is global::SuperMetroid.Core.Enemies.Dragon.DragonEntity entity56 && entity56.State is T held56)
            return held56;
        if (entity is global::SuperMetroid.Core.Enemies.Nuclear.NuclearWaffleEntity nuclearWaffle && nuclearWaffle.State is T heldNuclearWaffle)
            return heldNuclearWaffle;
        if (entity is global::SuperMetroid.Core.Enemies.Evir.EvirBodyEntity entity60 && entity60.State is T held60)
            return held60;
        if (entity is global::SuperMetroid.Core.Enemies.Evir.EvirSpitEntity entity61 && entity61.State is T held61)
            return held61;
        if (entity is global::SuperMetroid.Core.Enemies.Gunship.GunshipTopEntity entity67a && entity67a.State is T held67a)
            return held67a;
        if (entity is global::SuperMetroid.Core.Enemies.Gunship.GunshipTopEntity entity67b && entity67b.State.Ship is T held67b)
            return held67b;
        if (entity is global::SuperMetroid.Core.Enemies.Gunship.GunshipTopEntity entity67c && entity67c.State.Hull is T held67c)
            return held67c;
        if (entity is global::SuperMetroid.Core.Enemies.Gunship.GunshipTopEntity entity67d && entity67d.State.Pieces is T held67d)
            return held67d;
        if (entity is global::SuperMetroid.Core.Enemies.KiHunters.KiHunterBodyEntity entity72 && entity72.State is T held72)
            return held72;
        if (entity is global::SuperMetroid.Core.Enemies.KiHunters.KiHunterWingsEntity entity73 && entity73.State is T held73)
            return held73;
        if (entity is global::SuperMetroid.Core.Enemies.LavaJumpers.SqueeptEntity entity76 && entity76.State is T held76)
            return held76;
        if (entity is global::SuperMetroid.Core.Enemies.Magdollite.MagdolliteBodyEntity entity77 && entity77.Partners.BodyState is T held77)
            return held77;
        if (entity is global::SuperMetroid.Core.Enemies.Magdollite.MagdolliteHeadEntity entity79 && entity79.Partners.HeadState is T held79)
            return held79;
        if (entity is global::SuperMetroid.Core.Enemies.Magdollite.MagdolliteOverlayEntity entity81 && entity81.Partners.OverlayState is T held81)
            return held81;
        if (entity is global::SuperMetroid.Core.Enemies.MamaTurtles.MamaTurtleEntity entity85 && entity85.State is T held85)
            return held85;
        if (entity is global::SuperMetroid.Core.Enemies.PipeBugs.BrinstarPipeBugEntity entity94 && entity94.State is T held94)
            return held94;
        if (entity is global::SuperMetroid.Core.Enemies.PipeBugs.NorfairPipeBugEntity entity95 && entity95.State is T held95)
            return held95;
        if (entity is global::SuperMetroid.Core.Enemies.Powamp.PowampBalloonEntity entity99 && entity99.State is T held99)
            return held99;
        if (entity is global::SuperMetroid.Core.Enemies.Rios.GerutaEntity entity102 && entity102.State is T held102)
            return held102;
        if (entity is global::SuperMetroid.Core.Enemies.Rios.HoltzEntity entity104 && entity104.State is T held104)
            return held104;
        if (entity is global::SuperMetroid.Core.Enemies.Ripper.RipperEntity entity107 && entity107.State is T held107)
            return held107;
        if (entity is global::SuperMetroid.Core.Enemies.Sbug.SbugEntity entity109 && entity109.State is T held109)
            return held109;
        if (entity is global::SuperMetroid.Core.Enemies.Shutters.VerticalShutterEntity entity111 && entity111.State is T held111)
            return held111;
        if (entity is global::SuperMetroid.Core.Areas.Ceres.CeresRidleyRoomDoorEntity ridleyRoomDoor && ridleyRoomDoor.State is T heldRidleyRoomDoor)
            return heldRidleyRoomDoor;
        if (entity is global::SuperMetroid.Core.Enemies.FuneNamihe.FuneEntity fune && fune.State is T heldFune)
            return heldFune;
        if (entity is global::SuperMetroid.Core.Enemies.FuneNamihe.NamiheEntity namihe && namihe.State is T heldNamihe)
            return heldNamihe;
        if (entity is global::SuperMetroid.Core.Enemies.Hoppers.HoppersEntity hopper && hopper.State is T heldHopper)
            return heldHopper;
        if (entity is global::SuperMetroid.Core.Enemies.Metroids.MetroidEntity metroid && metroid.State is T heldMetroid)
            return heldMetroid;
        if (entity is global::SuperMetroid.Core.Enemies.WorkRobots.PoweredWorkRobotEntity workRobot && workRobot.State is T heldWorkRobot)
            return heldWorkRobot;
        if (entity is global::SuperMetroid.Core.Enemies.WreckedShip.WreckedShipGhostEntity ghost && ghost.State is T heldGhost)
            return heldGhost;
        if (entity is global::SuperMetroid.Core.Enemies.YappingMaws.YappingMawEntity yappingMaw && yappingMaw.State is T heldYappingMaw)
            return heldYappingMaw;
        return null;
    }
}
