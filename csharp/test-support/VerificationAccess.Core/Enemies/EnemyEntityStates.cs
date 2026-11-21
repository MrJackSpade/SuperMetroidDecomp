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
        if (entity is global::SuperMetroid.Core.Bosses.Botwoon.BotwoonEntity entity7 && entity7.State is T held7)
            return held7;
        if (entity is global::SuperMetroid.Core.Bosses.Crocomire.CrocomireBodyEntity entity8 && entity8.Encounter is T held8)
            return held8;
        if (entity is global::SuperMetroid.Core.Bosses.Crocomire.CrocomireEntity entity9 && entity9.Encounter is T held9)
            return held9;
        if (entity is global::SuperMetroid.Core.Bosses.Crocomire.CrocomireTongueEntity entity10 && entity10.Tongue is T held10)
            return held10;
        if (entity is global::SuperMetroid.Core.Bosses.Crocomire.CrocomireTongueEntity entity11 && entity11.Encounter is T held11)
            return held11;
        if (entity is global::SuperMetroid.Core.Bosses.Draygon.DraygonBodyEntity entity12 && entity12.Draygon is T held12)
            return held12;
        if (entity is global::SuperMetroid.Core.Bosses.Draygon.DraygonEntity entity13 && entity13.Encounter is T held13)
            return held13;
        if (entity is global::SuperMetroid.Core.Bosses.Kraid.FakeKraidEntity entity14 && entity14.State is T held14)
            return held14;
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
        if (entity is global::SuperMetroid.Core.Bosses.MotherBrain.BabyMetroidCutsceneEntity entity20 && entity20.Baby is T held20)
            return held20;
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
        if (entity is global::SuperMetroid.Core.Bosses.MotherBrain.MotherBrainTubesEntity entity26 && entity26.Tube is T held26)
            return held26;
        if (entity is global::SuperMetroid.Core.Bosses.MotherBrain.RinkaEntity entity27 && entity27.State is T held27)
            return held27;
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
        if (entity is global::SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnEntity entity34 && entity34.State is T held34)
            return held34;
        if (entity is global::SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnEntity entity35 && entity35.Encounter is T held35)
            return held35;
        if (entity is global::SuperMetroid.Core.Bosses.Torizo.TorizoEntity entity36 && entity36.State is T held36)
            return held36;
        if (entity is global::SuperMetroid.Core.Enemies.Alcoon.AlcoonEntity entity37 && entity37.State is T held37)
            return held37;
        if (entity is global::SuperMetroid.Core.Enemies.Animals.DachoraEchoEntity entity38 && entity38.State is T held38)
            return held38;
        if (entity is global::SuperMetroid.Core.Enemies.Animals.DachoraEntity entity39 && entity39.State is T held39)
            return held39;
        if (entity is global::SuperMetroid.Core.Enemies.Animals.EscapeEtecoonEntity entity41 && entity41.State is T held41)
            return held41;
        if (entity is global::SuperMetroid.Core.Enemies.Animals.EtecoonEntity entity42 && entity42.State is T held42)
            return held42;
        if (entity is global::SuperMetroid.Core.Enemies.Chozo.ChozoStatueEntity entity50 && entity50.State is T held50)
            return held50;
        if (entity is global::SuperMetroid.Core.Enemies.Corpses.DeadSidehopperEntity entity51 && entity51.State is T held51)
            return held51;
        if (entity is global::SuperMetroid.Core.Enemies.Corpses.DeadTorizoEntity entity52 && entity52.State is T held52)
            return held52;
        if (entity is global::SuperMetroid.Core.Enemies.Crawlers.CrawlerEntity entity54 && entity54.State is T held54)
            return held54;
        if (entity is global::SuperMetroid.Core.Enemies.Dragon.DragonEntity entity56 && entity56.State is T held56)
            return held56;
        if (entity is global::SuperMetroid.Core.Enemies.Evir.EvirBodyEntity entity60 && entity60.State is T held60)
            return held60;
        if (entity is global::SuperMetroid.Core.Enemies.Evir.EvirSpitEntity entity61 && entity61.State is T held61)
            return held61;
        if (entity is global::SuperMetroid.Core.Enemies.Hoppers.HoppersEntity entity70 && entity70.State is T held70)
            return held70;
        if (entity is global::SuperMetroid.Core.Enemies.Kago.KagoEntity entity71 && entity71.State is T held71)
            return held71;
        if (entity is global::SuperMetroid.Core.Enemies.KiHunters.KiHunterBodyEntity entity72 && entity72.State is T held72)
            return held72;
        if (entity is global::SuperMetroid.Core.Enemies.KiHunters.KiHunterWingsEntity entity73 && entity73.State is T held73)
            return held73;
        if (entity is global::SuperMetroid.Core.Enemies.Kzan.KzanTopEntity entity74 && entity74.State is T held74)
            return held74;
        if (entity is global::SuperMetroid.Core.Enemies.LavaJumpers.NorfairLavaJumperFollowerEntity entity75 && entity75.State is T held75)
            return held75;
        if (entity is global::SuperMetroid.Core.Enemies.LavaJumpers.SqueeptEntity entity76 && entity76.State is T held76)
            return held76;
        if (entity is global::SuperMetroid.Core.Enemies.Magdollite.MagdolliteBodyEntity entity77 && entity77.State is T held77)
            return held77;
        if (entity is global::SuperMetroid.Core.Enemies.Magdollite.MagdolliteBodyEntity entity78 && entity78.Body is T held78)
            return held78;
        if (entity is global::SuperMetroid.Core.Enemies.Magdollite.MagdolliteHeadEntity entity79 && entity79.State is T held79)
            return held79;
        if (entity is global::SuperMetroid.Core.Enemies.Magdollite.MagdolliteHeadEntity entity80 && entity80.Head is T held80)
            return held80;
        if (entity is global::SuperMetroid.Core.Enemies.Magdollite.MagdolliteOverlayEntity entity81 && entity81.State is T held81)
            return held81;
        if (entity is global::SuperMetroid.Core.Enemies.Magdollite.MagdolliteOverlayEntity entity82 && entity82.Overlay is T held82)
            return held82;
        if (entity is global::SuperMetroid.Core.Enemies.Magdollite.MagdollitePartEntity entity83 && entity83.State is T held83)
            return held83;
        if (entity is global::SuperMetroid.Core.Enemies.MamaTurtles.BabyTurtleEntity entity84 && entity84.State is T held84)
            return held84;
        if (entity is global::SuperMetroid.Core.Enemies.MamaTurtles.MamaTurtleEntity entity85 && entity85.State is T held85)
            return held85;
        if (entity is global::SuperMetroid.Core.Enemies.MaridiaSnails.OumEntity entity86 && entity86.State is T held86)
            return held86;
        if (entity is global::SuperMetroid.Core.Enemies.Metaree.MetareeEntity entity87 && entity87.State is T held87)
            return held87;
        if (entity is global::SuperMetroid.Core.Enemies.Metroids.MetroidEntity entity88 && entity88.State is T held88)
            return held88;
        if (entity is global::SuperMetroid.Core.Enemies.Metroids.MochtroidEntity entity89 && entity89.State is T held89)
            return held89;
        if (entity is global::SuperMetroid.Core.Enemies.MorphBallEye.MorphBallEyeEntity entity90 && entity90.State is T held90)
            return held90;
        if (entity is global::SuperMetroid.Core.Enemies.MorphBallEye.MorphBallEyeMountEntity entity91 && entity91.State is T held91)
            return held91;
        if (entity is global::SuperMetroid.Core.Enemies.Nuclear.NuclearWaffleEntity entity92 && entity92.State is T held92)
            return held92;
        if (entity is global::SuperMetroid.Core.Enemies.Owtch.OwtchEntity entity93 && entity93.State is T held93)
            return held93;
        if (entity is global::SuperMetroid.Core.Enemies.PipeBugs.BrinstarPipeBugEntity entity94 && entity94.State is T held94)
            return held94;
        if (entity is global::SuperMetroid.Core.Enemies.PipeBugs.NorfairPipeBugEntity entity95 && entity95.State is T held95)
            return held95;
        if (entity is global::SuperMetroid.Core.Enemies.PipeBugs.YellowPipeBugEntity entity96 && entity96.State is T held96)
            return held96;
        if (entity is global::SuperMetroid.Core.Enemies.Platforms.PlatformsEntity entity97 && entity97.State is T held97)
            return held97;
        if (entity is global::SuperMetroid.Core.Enemies.Powamp.PowampBalloonEntity entity99 && entity99.State is T held99)
            return held99;
        if (entity is global::SuperMetroid.Core.Enemies.Powamp.PowampBodyEntity entity100 && entity100.State is T held100)
            return held100;
        if (entity is global::SuperMetroid.Core.Enemies.Puyo.PuyoEntity entity101 && entity101.State is T held101)
            return held101;
        if (entity is global::SuperMetroid.Core.Enemies.Rios.GerutaEntity entity102 && entity102.State is T held102)
            return held102;
        if (entity is global::SuperMetroid.Core.Enemies.Rios.HoltzEntity entity104 && entity104.State is T held104)
            return held104;
        if (entity is global::SuperMetroid.Core.Enemies.Ripper.RipperEntity entity107 && entity107.State is T held107)
            return held107;
        if (entity is global::SuperMetroid.Core.Enemies.Sbug.SbugEntity entity108 && entity108.State is T held108)
            return held108;
        if (entity is global::SuperMetroid.Core.Enemies.Shaktool.ShaktoolEntity entity109 && entity109.State is T held109)
            return held109;
        if (entity is global::SuperMetroid.Core.Enemies.Shutters.GrowingShutterEntity entity110 && entity110.State is T held110)
            return held110;
        if (entity is global::SuperMetroid.Core.Enemies.Shutters.VerticalShutterEntity entity111 && entity111.State is T held111)
            return held111;
        if (entity is global::SuperMetroid.Core.Enemies.Skree.SkreeEntity entity112 && entity112.State is T held112)
            return held112;
        if (entity is global::SuperMetroid.Core.Enemies.Skultera.SkulteraEntity entity113 && entity113.State is T held113)
            return held113;
        if (entity is global::SuperMetroid.Core.Enemies.SpacePirates.NinjaSpacePirateEntity entity114 && entity114.State is T held114)
            return held114;
        if (entity is global::SuperMetroid.Core.Enemies.SpacePirates.WalkingSpacePirateEntity entity115 && entity115.State is T held115)
            return held115;
        if (entity is global::SuperMetroid.Core.Enemies.SpacePirates.WallSpacePirateEntity entity116 && entity116.State is T held116)
            return held116;
        if (entity is global::SuperMetroid.Core.Enemies.Sparks.SparkEntity entity117 && entity117.State is T held117)
            return held117;
        if (entity is global::SuperMetroid.Core.Enemies.Waver.WaverEntity entity118 && entity118.State is T held118)
            return held118;
        if (entity is global::SuperMetroid.Core.Enemies.WorkRobots.PoweredWorkRobotEntity entity119 && entity119.State is T held119)
            return held119;
        if (entity is global::SuperMetroid.Core.Enemies.WorkRobots.UnpoweredWorkRobotEntity entity120 && entity120.State is T held120)
            return held120;
        if (entity is global::SuperMetroid.Core.Enemies.WreckedShip.WreckedShipGhostEntity entity121 && entity121.State is T held121)
            return held121;
        if (entity is global::SuperMetroid.Core.Enemies.YappingMaws.YappingMawEntity entity122 && entity122.State is T held122)
            return held122;
        if (entity is global::SuperMetroid.Core.Enemies.Yard.YardEntity entity123 && entity123.State is T held123)
            return held123;
        if (entity is global::SuperMetroid.Core.Enemies.Zoa.ZoaEntity entity124 && entity124.State is T held124)
            return held124;
        return null;
    }
}
