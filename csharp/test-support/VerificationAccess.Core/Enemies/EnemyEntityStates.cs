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
        if (entity is global::SuperMetroid.Core.Areas.Ceres.CeresDoorEntity ceresDoorEntity && ceresDoorEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Areas.Ceres.CeresElevatorDoorEntity ceresElevatorDoorEntity && ceresElevatorDoorEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Areas.Ceres.CeresElevatorDoorEntity ceresElevatorDoorEntity && ceresElevatorDoorEntity.Door is T doorHeld)
            return doorHeld;
        if (entity is global::SuperMetroid.Core.Areas.Ceres.CeresElevatorRoomWallEntity ceresElevatorRoomWallEntity && ceresElevatorRoomWallEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Areas.Ceres.CeresGetawayChamberWallEntity ceresGetawayChamberWallEntity && ceresGetawayChamberWallEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Areas.Ceres.CeresRidleyRoomDoorEntity ceresRidleyRoomDoorEntity && ceresRidleyRoomDoorEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Areas.Ceres.CeresSteamEntity ceresSteamEntity && ceresSteamEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Bosses.Botwoon.BotwoonEntity botwoonEntity && botwoonEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Bosses.Crocomire.CrocomireBodyEntity crocomireBodyEntity && crocomireBodyEntity.Encounter is T encounterHeld)
            return encounterHeld;
        if (entity is global::SuperMetroid.Core.Bosses.Crocomire.CrocomireEntity crocomireEntity && crocomireEntity.Encounter is T encounterHeld)
            return encounterHeld;
        if (entity is global::SuperMetroid.Core.Bosses.Crocomire.CrocomireTongueEntity crocomireTongueEntity && crocomireTongueEntity.Tongue is T tongueHeld)
            return tongueHeld;
        if (entity is global::SuperMetroid.Core.Bosses.Crocomire.CrocomireTongueEntity crocomireTongueEntity && crocomireTongueEntity.Encounter is T encounterHeld)
            return encounterHeld;
        if (entity is global::SuperMetroid.Core.Bosses.Draygon.DraygonBodyEntity draygonBodyEntity && draygonBodyEntity.Draygon is T draygonHeld)
            return draygonHeld;
        if (entity is global::SuperMetroid.Core.Bosses.Draygon.DraygonEntity draygonEntity && draygonEntity.Encounter is T encounterHeld)
            return encounterHeld;
        if (entity is global::SuperMetroid.Core.Bosses.Kraid.FakeKraidEntity fakeKraidEntity && fakeKraidEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Bosses.Kraid.KraidArmEntity kraidArmEntity && kraidArmEntity.Part is T partHeld)
            return partHeld;
        if (entity is global::SuperMetroid.Core.Bosses.Kraid.KraidEntity kraidEntity && kraidEntity.Encounter is T encounterHeld)
            return encounterHeld;
        if (entity is global::SuperMetroid.Core.Bosses.Kraid.KraidFootEntity kraidFootEntity && kraidFootEntity.Part is T partHeld)
            return partHeld;
        if (entity is global::SuperMetroid.Core.Bosses.Kraid.KraidLimbEntity kraidLimbEntity && kraidLimbEntity.Part is T partHeld)
            return partHeld;
        if (entity is global::SuperMetroid.Core.Bosses.Kraid.KraidNailEntity kraidNailEntity && kraidNailEntity.Part is T partHeld)
            return partHeld;
        if (entity is global::SuperMetroid.Core.Bosses.MotherBrain.BabyMetroidCutsceneEntity babyMetroidCutsceneEntity && babyMetroidCutsceneEntity.Baby is T babyHeld)
            return babyHeld;
        if (entity is global::SuperMetroid.Core.Bosses.MotherBrain.BabyMetroidEntity babyMetroidEntity && babyMetroidEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Bosses.MotherBrain.MotherBrainBodyEntity motherBrainBodyEntity && motherBrainBodyEntity.Encounter is T encounterHeld)
            return encounterHeld;
        if (entity is global::SuperMetroid.Core.Bosses.MotherBrain.MotherBrainEntity motherBrainEntity && motherBrainEntity.Encounter is T encounterHeld)
            return encounterHeld;
        if (entity is global::SuperMetroid.Core.Bosses.MotherBrain.MotherBrainHeadEntity motherBrainHeadEntity && motherBrainHeadEntity.Encounter is T encounterHeld)
            return encounterHeld;
        if (entity is global::SuperMetroid.Core.Bosses.MotherBrain.MotherBrainTubesEntity motherBrainTubesEntity && motherBrainTubesEntity.Encounter is T encounterHeld)
            return encounterHeld;
        if (entity is global::SuperMetroid.Core.Bosses.MotherBrain.MotherBrainTubesEntity motherBrainTubesEntity && motherBrainTubesEntity.Tube is T tubeHeld)
            return tubeHeld;
        if (entity is global::SuperMetroid.Core.Bosses.MotherBrain.RinkaEntity rinkaEntity && rinkaEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Bosses.MotherBrain.ZebetiteEntity zebetiteEntity && zebetiteEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Bosses.Phantoon.PhantoonEntity phantoonEntity && phantoonEntity.Encounter is T encounterHeld)
            return encounterHeld;
        if (entity is global::SuperMetroid.Core.Bosses.Ridley.CeresRidleyEntity ceresRidleyEntity && ceresRidleyEntity.Encounter is T encounterHeld)
            return encounterHeld;
        if (entity is global::SuperMetroid.Core.Bosses.Ridley.NorfairRidleyEntity norfairRidleyEntity && norfairRidleyEntity.Encounter is T encounterHeld)
            return encounterHeld;
        if (entity is global::SuperMetroid.Core.Bosses.Ridley.RidleyBreakupPieceEntity ridleyBreakupPieceEntity && ridleyBreakupPieceEntity.Fragment is T fragmentHeld)
            return fragmentHeld;
        if (entity is global::SuperMetroid.Core.Bosses.Ridley.RidleyEntity ridleyEntity && ridleyEntity.Encounter is T encounterHeld)
            return encounterHeld;
        if (entity is global::SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnEntity sporeSpawnEntity && sporeSpawnEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnEntity sporeSpawnEntity && sporeSpawnEntity.Encounter is T encounterHeld)
            return encounterHeld;
        if (entity is global::SuperMetroid.Core.Bosses.Torizo.TorizoEntity torizoEntity && torizoEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Alcoon.AlcoonEntity alcoonEntity && alcoonEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Animals.DachoraEchoEntity dachoraEchoEntity && dachoraEchoEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Animals.DachoraEntity dachoraEntity && dachoraEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Animals.EscapeDachoraEntity escapeDachoraEntity && escapeDachoraEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Animals.EscapeEtecoonEntity escapeEtecoonEntity && escapeEtecoonEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Animals.EtecoonEntity etecoonEntity && etecoonEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Atomic.AtomicEntity atomicEntity && atomicEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Beetom.BeetomEntity beetomEntity && beetomEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Boulder.BoulderEntity boulderEntity && boulderEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Boyon.BoyonEntity boyonEntity && boyonEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Bull.BullEntity bullEntity && bullEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Cacatac.CacatacEntity cacatacEntity && cacatacEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Choot.ChootEntity chootEntity && chootEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Chozo.ChozoStatueEntity chozoStatueEntity && chozoStatueEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Corpses.DeadSidehopperEntity deadSidehopperEntity && deadSidehopperEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Corpses.DeadTorizoEntity deadTorizoEntity && deadTorizoEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Corpses.DeadTourianEntity deadTourianEntity && deadTourianEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Crawlers.CrawlerEntity crawlerEntity && crawlerEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Crawlers.MultiviolaEntity multiviolaEntity && multiviolaEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Dragon.DragonEntity dragonEntity && dragonEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Dragon.DragonWingEntity dragonWingEntity && dragonWingEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Elevator.ElevatorEntity elevatorEntity && elevatorEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Evir.EvirArmsEntity evirArmsEntity && evirArmsEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Evir.EvirBodyEntity evirBodyEntity && evirBodyEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Evir.EvirSpitEntity evirSpitEntity && evirSpitEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.FaceBlocks.BlueBrinstarFaceBlockEntity blueBrinstarFaceBlockEntity && blueBrinstarFaceBlockEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Fireflea.FirefleaEntity firefleaEntity && firefleaEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Flies.FliesEntity fliesEntity && fliesEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.FuneNamihe.FuneEntity funeEntity && funeEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.FuneNamihe.NamiheEntity namiheEntity && namiheEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Gunship.GunshipBottomEntity gunshipBottomEntity && gunshipBottomEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Hibashi.HibashiCollisionPartEntity hibashiCollisionPartEntity && hibashiCollisionPartEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Hibashi.HibashiEntity hibashiEntity && hibashiEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Hoppers.HoppersEntity hoppersEntity && hoppersEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Kago.KagoEntity kagoEntity && kagoEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.KiHunters.KiHunterBodyEntity kiHunterBodyEntity && kiHunterBodyEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.KiHunters.KiHunterWingsEntity kiHunterWingsEntity && kiHunterWingsEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Kzan.KzanTopEntity kzanTopEntity && kzanTopEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.LavaJumpers.NorfairLavaJumperFollowerEntity norfairLavaJumperFollowerEntity && norfairLavaJumperFollowerEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.LavaJumpers.SqueeptEntity squeeptEntity && squeeptEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Magdollite.MagdolliteBodyEntity magdolliteBodyEntity && magdolliteBodyEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Magdollite.MagdolliteBodyEntity magdolliteBodyEntity && magdolliteBodyEntity.Body is T bodyHeld)
            return bodyHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Magdollite.MagdolliteHeadEntity magdolliteHeadEntity && magdolliteHeadEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Magdollite.MagdolliteHeadEntity magdolliteHeadEntity && magdolliteHeadEntity.Head is T headHeld)
            return headHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Magdollite.MagdolliteOverlayEntity magdolliteOverlayEntity && magdolliteOverlayEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Magdollite.MagdolliteOverlayEntity magdolliteOverlayEntity && magdolliteOverlayEntity.Overlay is T overlayHeld)
            return overlayHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Magdollite.MagdollitePartEntity magdollitePartEntity && magdollitePartEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.MamaTurtles.BabyTurtleEntity babyTurtleEntity && babyTurtleEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.MamaTurtles.MamaTurtleEntity mamaTurtleEntity && mamaTurtleEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.MaridiaSnails.OumEntity oumEntity && oumEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Metaree.MetareeEntity metareeEntity && metareeEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Metroids.MetroidEntity metroidEntity && metroidEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Metroids.MochtroidEntity mochtroidEntity && mochtroidEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.MorphBallEye.MorphBallEyeEntity morphBallEyeEntity && morphBallEyeEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.MorphBallEye.MorphBallEyeMountEntity morphBallEyeMountEntity && morphBallEyeMountEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Nuclear.NuclearWaffleEntity nuclearWaffleEntity && nuclearWaffleEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Owtch.OwtchEntity owtchEntity && owtchEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.PipeBugs.BrinstarPipeBugEntity brinstarPipeBugEntity && brinstarPipeBugEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.PipeBugs.NorfairPipeBugEntity norfairPipeBugEntity && norfairPipeBugEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.PipeBugs.YellowPipeBugEntity yellowPipeBugEntity && yellowPipeBugEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Platforms.PlatformsEntity platformsEntity && platformsEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Polyp.PolypEntity polypEntity && polypEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Powamp.PowampBalloonEntity powampBalloonEntity && powampBalloonEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Powamp.PowampBodyEntity powampBodyEntity && powampBodyEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Puyo.PuyoEntity puyoEntity && puyoEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Rios.GerutaEntity gerutaEntity && gerutaEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Rios.GerutaFollowerEntity gerutaFollowerEntity && gerutaFollowerEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Rios.HoltzEntity holtzEntity && holtzEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Rios.HoltzFollowerEntity holtzFollowerEntity && holtzFollowerEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Rios.RioEntity rioEntity && rioEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Ripper.RipperEntity ripperEntity && ripperEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Sbug.SbugEntity sbugEntity && sbugEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Shaktool.ShaktoolEntity shaktoolEntity && shaktoolEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Shutters.GrowingShutterEntity growingShutterEntity && growingShutterEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Shutters.VerticalShutterEntity verticalShutterEntity && verticalShutterEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Skree.SkreeEntity skreeEntity && skreeEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Skultera.SkulteraEntity skulteraEntity && skulteraEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.SpacePirates.NinjaSpacePirateEntity ninjaSpacePirateEntity && ninjaSpacePirateEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.SpacePirates.WalkingSpacePirateEntity walkingSpacePirateEntity && walkingSpacePirateEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.SpacePirates.WallSpacePirateEntity wallSpacePirateEntity && wallSpacePirateEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Sparks.SparkEntity sparkEntity && sparkEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Waver.WaverEntity waverEntity && waverEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.WorkRobots.PoweredWorkRobotEntity poweredWorkRobotEntity && poweredWorkRobotEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.WorkRobots.UnpoweredWorkRobotEntity unpoweredWorkRobotEntity && unpoweredWorkRobotEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.WreckedShip.WreckedShipGhostEntity wreckedShipGhostEntity && wreckedShipGhostEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.YappingMaws.YappingMawEntity yappingMawEntity && yappingMawEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Yard.YardEntity yardEntity && yardEntity.State is T stateHeld)
            return stateHeld;
        if (entity is global::SuperMetroid.Core.Enemies.Zoa.ZoaEntity zoaEntity && zoaEntity.State is T stateHeld)
            return stateHeld;
        return null;
    }
}
