using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Fx;

/// <summary>Compiled Maridia quicksand dispatch and physical definition data.</summary>
public static class QuicksandDefinitions
{
    /// <summary>
    /// `$84:B48B-$84:B495`, suit-indexed moving displacement, stationary
    /// displacement, and upward-speed-limit values used by the surface reaction.
    /// Gravity equipment selects one of two named scalar cases. Moving displacement is
    /// 2 pixels per frame for either suit, stationary displacement is 1.125/1, and the
    /// NTSC upward cap is 2.5/3.5 pixels per frame without/with Gravity. The cartridge
    /// stores them as unsigned 8.8 words; they are held here as the 16.16 values its
    /// consumers widen them to.
    /// </summary>
    public static QuicksandSurfacePhysics SurfacePhysics(bool gravitySuit) =>
        gravitySuit ? withGravitySuit : withoutGravitySuit;

    /// <summary>
    /// Named setup dispatch for the eight supported sand headers at $84:B713..B73F.
    /// All select the shared delete list; unsupported pointers return false/default,
    /// including the unused surface clones between the named headers. No table is stored.
    /// </summary>
    internal static bool TryGetReaction(
        PlmHeaderId header,
        out QuicksandReactionDefinition definition)
    {
        // Native headers select behavior; the gaps contain unused clones outside this API.
        if (header is not (PlmHeaderId.InsideReactionQuicksandSurface or PlmHeaderId.InsideReactionSubmergingQuicksand or
            PlmHeaderId.InsideReactionSandFallsSlow or PlmHeaderId.InsideReactionSandFallsFast or
            PlmHeaderId.CollisionReactionQuicksandSurface or PlmHeaderId.CollisionReactionSubmergingQuicksand or
            PlmHeaderId.CollisionReactionSandFallsSlow or PlmHeaderId.CollisionReactionSandFallsFast))
        {
            definition = default;
            return false;
        }
        SpecialAirReactionSetup setup = header switch
        {
            PlmHeaderId.InsideReactionQuicksandSurface => SpecialAirReactionSetup.QuicksandSurface,
            PlmHeaderId.InsideReactionSubmergingQuicksand => SpecialAirReactionSetup.SubmergingQuicksand,
            PlmHeaderId.InsideReactionSandFallsSlow => SpecialAirReactionSetup.SandFallsSlow,
            PlmHeaderId.InsideReactionSandFallsFast => SpecialAirReactionSetup.SandFallsFast,
            PlmHeaderId.CollisionReactionQuicksandSurface => SpecialAirReactionSetup.QuicksandSurfaceCollision,
            PlmHeaderId.CollisionReactionSubmergingQuicksand => SpecialAirReactionSetup.SubmergingQuicksandCollision,
            PlmHeaderId.CollisionReactionSandFallsSlow or PlmHeaderId.CollisionReactionSandFallsFast =>
                SpecialAirReactionSetup.SandFallsCollision,
            PlmHeaderId.None or
                PlmHeaderId.Nothing or
                PlmHeaderId.CollisionReactionClearCarry or
                PlmHeaderId.RightwardsScrollExtension or
                PlmHeaderId.LeftwardsScrollExtension or
                PlmHeaderId.DownwardsScrollExtension or
                PlmHeaderId.UpwardsScrollExtension or
                PlmHeaderId.WreckedShipEntranceTreadmillFromWest or
                PlmHeaderId.WreckedShipEntranceTreadmillFromEast or
                PlmHeaderId.InsideReactionNothingFirst or
                PlmHeaderId.InsideReactionNothingSecond or
                PlmHeaderId.InsideReactionNothingThird or
                PlmHeaderId.FillMotherBrainsWall or
                PlmHeaderId.MotherBrainsRoomEscapeDoor or
                PlmHeaderId.MotherBrainsBackgroundRow2 or
                PlmHeaderId.MotherBrainsBackgroundRow3 or
                PlmHeaderId.MotherBrainsBackgroundRow4 or
                PlmHeaderId.MotherBrainsBackgroundRow5 or
                PlmHeaderId.MotherBrainsBackgroundRow6 or
                PlmHeaderId.MotherBrainsBackgroundRow7 or
                PlmHeaderId.MotherBrainsBackgroundRow8 or
                PlmHeaderId.MotherBrainsBackgroundRow9 or
                PlmHeaderId.MotherBrainsBackgroundRowA or
                PlmHeaderId.MotherBrainsBackgroundRowB or
                PlmHeaderId.MotherBrainsBackgroundRowC or
                PlmHeaderId.MotherBrainsBackgroundRowD or
                PlmHeaderId.ClearMotherBrainCeilingBlock or
                PlmHeaderId.ClearMotherBrainCeilingTube or
                PlmHeaderId.ClearMotherBrainBottomMiddleSideTube or
                PlmHeaderId.ClearMotherBrainBottomMiddleTubes or
                PlmHeaderId.ClearMotherBrainBottomLeftTube or
                PlmHeaderId.ClearMotherBrainBottomRightTube or
                PlmHeaderId.InsideReactionBrinstarFloorPlant or
                PlmHeaderId.InsideReactionBrinstarCeilingPlant or
                PlmHeaderId.MapStation or
                PlmHeaderId.MapStationRightAccess or
                PlmHeaderId.MapStationLeftAccess or
                PlmHeaderId.EnergyStation or
                PlmHeaderId.EnergyStationRightAccess or
                PlmHeaderId.EnergyStationLeftAccess or
                PlmHeaderId.MissileStation or
                PlmHeaderId.MissileStationRightAccess or
                PlmHeaderId.MissileStationLeftAccess or
                PlmHeaderId.NothingSecond or
                PlmHeaderId.NothingThird or
                PlmHeaderId.ScrollTriggerCollision or
                PlmHeaderId.ScrollTrigger or
                PlmHeaderId.UnusedSolidScrollPLM or
                PlmHeaderId.ElevatorPlatform or
                PlmHeaderId.InsideReactionCrateria80 or
                PlmHeaderId.ClearCrocomireBridge or
                PlmHeaderId.CrumbleCrocomireBridgeBlock or
                PlmHeaderId.ClearCrocomireBridgeBlock or
                PlmHeaderId.ClearCrocomireInvisibleWall or
                PlmHeaderId.CreateCrocomireInvisibleWall or
                PlmHeaderId.UnusedDraw13BlankAirTiles or
                PlmHeaderId.UnusedDraw13BlankSolidTiles or
                PlmHeaderId.ClearBabyMetroidInvisibleWall or
                PlmHeaderId.CreateBabyMetroidInvisibleWall or
                PlmHeaderId.SaveStationTrigger or
                PlmHeaderId.SaveStation or
                PlmHeaderId.CrumbleAccessToTourianElevator or
                PlmHeaderId.ClearAccessToTourianElevator or
                PlmHeaderId.DrawPhantoonDoorDuringBossFight or
                PlmHeaderId.RestorePhantoonDoorAfterBossFight or
                PlmHeaderId.CrumbleSporeSpawnCeiling or
                PlmHeaderId.ClearSporeSpawnCeiling or
                PlmHeaderId.ClearBotwoonWall or
                PlmHeaderId.CrumbleBotwoonWall or
                PlmHeaderId.UnusedSetKraidCeilingBlockToBackground1 or
                PlmHeaderId.CrumbleKraidCeilingIntoBackground1 or
                PlmHeaderId.CrumbleKraidPlatformVariant1 or
                PlmHeaderId.CrumbleKraidCeilingIntoBackground2 or
                PlmHeaderId.CrumbleKraidPlatformVariant2 or
                PlmHeaderId.CrumbleKraidCeilingIntoBackground3 or
                PlmHeaderId.ClearKraidCeiling or
                PlmHeaderId.ClearKraidSpikes or
                PlmHeaderId.CrumbleKraidSpikes or
                PlmHeaderId.EnableSoundsIn20FramesF0FramesIfCeres or
                PlmHeaderId.SpeedBoosterEscape or
                PlmHeaderId.ShaktoolsRoom or
                PlmHeaderId.MaridiaElevatube or
                PlmHeaderId.OldTourianEscapeShaftFakeWall or
                PlmHeaderId.RaiseAcidInEscapeRoomBeforeOldTourianEscapeShaft or
                PlmHeaderId.GateBlock or
                PlmHeaderId.ReactionCrittersEscapeBlock or
                PlmHeaderId.CrittersEscapeBlock or
                PlmHeaderId.TurnCeresElevatorDoorToSolidBlocksDuringEscape or
                PlmHeaderId.BombTorizoGreyDoor or
                PlmHeaderId.WreckedShipAttic or
                PlmHeaderId.CrateriaMainstreetEscapePassage or
                PlmHeaderId.LeftGreenGateTrigger or
                PlmHeaderId.RightGreenGateTrigger or
                PlmHeaderId.LeftRedGateTrigger or
                PlmHeaderId.RightRedGateTrigger or
                PlmHeaderId.LeftBlueGateTrigger or
                PlmHeaderId.RightBlueGateTrigger or
                PlmHeaderId.LeftYellowGateTrigger or
                PlmHeaderId.RightYellowGateTrigger or
                PlmHeaderId.DownwardsOpenGate or
                PlmHeaderId.DownwardGate or
                PlmHeaderId.UpwardsOpenGate or
                PlmHeaderId.UpwardsClosedGate or
                PlmHeaderId.DownwardGateShotBlock or
                PlmHeaderId.UpwardsGateShotblock or
                PlmHeaderId.GenericShotTriggerForAPLM or
                PlmHeaderId.GreyDoorFacingLeft or
                PlmHeaderId.GreyDoorFacingRight or
                PlmHeaderId.GreyDoorFacingUp or
                PlmHeaderId.GreyDoorFacingDown or
                PlmHeaderId.YellowDoorFacingLeft or
                PlmHeaderId.YellowDoorFacingRight or
                PlmHeaderId.YellowDoorFacingUp or
                PlmHeaderId.YellowDoorFacingDown or
                PlmHeaderId.GreenDoorFacingLeft or
                PlmHeaderId.GreenDoorFacingRight or
                PlmHeaderId.GreenDoorFacingUp or
                PlmHeaderId.GreenDoorFacingDown or
                PlmHeaderId.RedDoorFacingLeft or
                PlmHeaderId.RedDoorFacingRight or
                PlmHeaderId.RedDoorFacingUp or
                PlmHeaderId.RedDoorFacingDown or
                PlmHeaderId.BlueDoorFacingLeft or
                PlmHeaderId.BlueDoorFacingRight or
                PlmHeaderId.BlueDoorFacingUp or
                PlmHeaderId.BlueDoorFacingDown or
                PlmHeaderId.BlueDoorClosingFacingLeft or
                PlmHeaderId.BlueDoorClosingFacingRight or
                PlmHeaderId.BlueDoorClosingFacingUp or
                PlmHeaderId.BlueDoorClosingFacingDown or
                PlmHeaderId.MotherBrainEscapeRoomGate or
                PlmHeaderId.MotherBrainEscapeRoomGateClosing or
                PlmHeaderId.Plm1x1RespawningCrumbleBlock or
                PlmHeaderId.Plm2x1RespawningCrumbleBlock or
                PlmHeaderId.Plm1x2RespawningCrumbleBlock or
                PlmHeaderId.Plm2x2RespawningCrumbleBlock or
                PlmHeaderId.BombReactionSpeedBoostBlock or
                PlmHeaderId.SpeedBlockBrinstarSlowRespawning or
                PlmHeaderId.SpeedBlockBrinstarSlowPermanent or
                PlmHeaderId.SpeedBlockRespawning or
                PlmHeaderId.SpeedBlockDachoraRespawning or
                PlmHeaderId.SpeedBlockPermanent or
                PlmHeaderId.ContactCrumble1x1Respawning or
                PlmHeaderId.ContactCrumble2x1Respawning or
                PlmHeaderId.ContactCrumble1x2Respawning or
                PlmHeaderId.ContactCrumble2x2Respawning or
                PlmHeaderId.ContactCrumble1x1Permanent or
                PlmHeaderId.ContactCrumble2x1Permanent or
                PlmHeaderId.ContactCrumble1x2Permanent or
                PlmHeaderId.ContactCrumble2x2Permanent or
                PlmHeaderId.Reaction1x1RespawningShotBlock or
                PlmHeaderId.Reaction2x1RespawningShotBlock or
                PlmHeaderId.Reaction1x2RespawningShotBlock or
                PlmHeaderId.Reaction2x2RespawningShotBlock or
                PlmHeaderId.Reaction1x1ShotBlock or
                PlmHeaderId.Reaction2x1ShotBlock or
                PlmHeaderId.Reaction1x2ShotBlock or
                PlmHeaderId.Reaction2x2ShotBlock or
                PlmHeaderId.ReactionRespawningPowerBombBlock or
                PlmHeaderId.ReactionPowerBombBlock or
                PlmHeaderId.ReactionRespawningSuperMissileBlock or
                PlmHeaderId.ReactionSuperMissileBlock or
                PlmHeaderId.EnemyBreakableBlock or
                PlmHeaderId.Collision1x1RespawningBombBlock or
                PlmHeaderId.Collision2x1RespawningBombBlock or
                PlmHeaderId.Collision1x2RespawningBombBlock or
                PlmHeaderId.Collision2x2RespawningBombBlock or
                PlmHeaderId.Collision1x1BombBlock or
                PlmHeaderId.Collision2x1BombBlock or
                PlmHeaderId.Collision1x2BombBlock or
                PlmHeaderId.Collision2x2BombBlock or
                PlmHeaderId.Reaction1x1RespawningBombBlock or
                PlmHeaderId.Reaction2x1RespawningBombBlock or
                PlmHeaderId.Reaction1x2RespawningBombBlock or
                PlmHeaderId.Reaction2x2RespawningBombBlock or
                PlmHeaderId.Reaction1x1BombBlock or
                PlmHeaderId.Reaction2x1BombBlock or
                PlmHeaderId.Reaction1x2BombBlock or
                PlmHeaderId.Reaction2x2BombBlock or
                PlmHeaderId.GrappledGrappleBlock or
                PlmHeaderId.GrappledRespawningBreakableGrappleBlock or
                PlmHeaderId.GrappledBreakableGrappleBlock or
                PlmHeaderId.GrappledGenericSpikeBlock or
                PlmHeaderId.GrappledDraygonsBrokenTurret or
                PlmHeaderId.UnusedBlueBrinstarFaceBlock or
                PlmHeaderId.CrumbleLowerNorfairChozoRoomPlug or
                PlmHeaderId.UnusedShotBlock or
                PlmHeaderId.UnusedGrappleBlock or
                PlmHeaderId.LowerNorfairChozoHand or
                PlmHeaderId.CollisionLowerNorfairChozoHandCheck or
                PlmHeaderId.MotherBrainGlass or
                PlmHeaderId.BombTorizoHand or
                PlmHeaderId.WreckedShipChozoHand or
                PlmHeaderId.CollisionWreckedShipChozoHandCheck or
                PlmHeaderId.ClearSlopeAccessForWreckedShipChozo or
                PlmHeaderId.BlockSlopeAccessForWreckedShipChozo or
                PlmHeaderId.NoobTube or
                PlmHeaderId.SetMetroidsClearedStatesWhenRequired or
                PlmHeaderId.EyeDoorEyeFacingRight or
                PlmHeaderId.EyeDoorFacingRight or
                PlmHeaderId.EyeDoorBottomFacingRight or
                PlmHeaderId.EyeDoorEyeFacingLeft or
                PlmHeaderId.EyeDoorFacingLeft or
                PlmHeaderId.EyeDoorBottomFacingLeft or
                PlmHeaderId.DraygonCannonFacingRight or
                PlmHeaderId.DraygonCannonFacingRightDestroyed or
                PlmHeaderId.DraygonCannonFacingLeft or
                PlmHeaderId.DraygonCannonFacingLeftUnshielded or
                PlmHeaderId.ItemCollisionDetection or
                PlmHeaderId.ExposedEnergyTank or
                PlmHeaderId.ExposedMissileTank or
                PlmHeaderId.ExposedSuperMissileTank or
                PlmHeaderId.ExposedPowerBombTank or
                PlmHeaderId.ExposedBombs or
                PlmHeaderId.ExposedChargeBeam or
                PlmHeaderId.ExposedIceBeam or
                PlmHeaderId.ExposedHiJumpBoots or
                PlmHeaderId.ExposedSpeedBooster or
                PlmHeaderId.ExposedWaveBeam or
                PlmHeaderId.ExposedSpazerBeam or
                PlmHeaderId.ExposedSpringBall or
                PlmHeaderId.ExposedVariaSuit or
                PlmHeaderId.ExposedGravitySuit or
                PlmHeaderId.ExposedXrayScope or
                PlmHeaderId.ExposedPlasmaBeam or
                PlmHeaderId.ExposedGrappleBeam or
                PlmHeaderId.ExposedSpaceJump or
                PlmHeaderId.ExposedScrewAttack or
                PlmHeaderId.ExposedMorphBall or
                PlmHeaderId.ExposedReserveTank or
                PlmHeaderId.ChozoEnergyTank or
                PlmHeaderId.ChozoMissileTank or
                PlmHeaderId.ChozoSuperMissileTank or
                PlmHeaderId.ChozoPowerBombTank or
                PlmHeaderId.ChozoBombs or
                PlmHeaderId.ChozoChargeBeam or
                PlmHeaderId.ChozoIceBeam or
                PlmHeaderId.ChozoHiJumpBoots or
                PlmHeaderId.ChozoSpeedBooster or
                PlmHeaderId.ChozoWaveBeam or
                PlmHeaderId.ChozoSpazerBeam or
                PlmHeaderId.ChozoSpringBall or
                PlmHeaderId.ChozoVariaSuit or
                PlmHeaderId.ChozoGravitySuit or
                PlmHeaderId.ChozoXrayScope or
                PlmHeaderId.ChozoPlasmaBeam or
                PlmHeaderId.ChozoGrappleBeam or
                PlmHeaderId.ChozoSpaceJump or
                PlmHeaderId.ChozoScrewAttack or
                PlmHeaderId.ChozoMorphBall or
                PlmHeaderId.ChozoReserveTank or
                PlmHeaderId.ShotBlockEnergyTank or
                PlmHeaderId.ShotBlockMissileTank or
                PlmHeaderId.ShotBlockSuperMissileTank or
                PlmHeaderId.ShotBlockPowerBombTank or
                PlmHeaderId.ShotBlockBombs or
                PlmHeaderId.ShotBlockChargeBeam or
                PlmHeaderId.ShotBlockIceBeam or
                PlmHeaderId.ShotBlockHiJumpBoots or
                PlmHeaderId.ShotBlockSpeedBooster or
                PlmHeaderId.ShotBlockWaveBeam or
                PlmHeaderId.ShotBlockSpazerBeam or
                PlmHeaderId.ShotBlockSpringBall or
                PlmHeaderId.ShotBlockVariaSuit or
                PlmHeaderId.ShotBlockGravitySuit or
                PlmHeaderId.ShotBlockXrayScope or
                PlmHeaderId.ShotBlockPlasmaBeam or
                PlmHeaderId.ShotBlockGrappleBeam or
                PlmHeaderId.ShotBlockSpaceJump or
                PlmHeaderId.ShotBlockScrewAttack or
                PlmHeaderId.ShotBlockMorphBall or
                PlmHeaderId.ShotBlockReserveTank => throw new InvalidOperationException($"Undefined quicksand header {header}."),
            _ => throw new InvalidOperationException($"Undefined quicksand header {header}."),
        };
        definition = new(setup, PlmProgramStart.Delete);
        return true;
    }

    /// <summary>Resolves one supported sand header or rejects an invalid allocator request.</summary>
    internal static QuicksandReactionDefinition ResolveReaction(PlmHeaderId header) =>
        TryGetReaction(header, out QuicksandReactionDefinition definition)
            ? definition
            : throw new ArgumentOutOfRangeException(
                nameof(header),
                header,
                "Quicksand reaction header must be one of the eight bank-$84 sand entries.");

    /// <summary>Without Gravity: 2-pixel moving and 1.125-pixel stationary displacement, 2.5-pixel upward speed cap.</summary>
    private static readonly QuicksandSurfacePhysics withoutGravitySuit =
        new(SnesSignedSixteenSixteen.FromSixteenthsOfPixel(32),
            SnesSignedSixteenSixteen.FromSixteenthsOfPixel(18),
            SnesSignedSixteenSixteen.FromSixteenthsOfPixel(40));

    /// <summary>With Gravity: 2-pixel moving and 1-pixel stationary displacement, 3.5-pixel upward speed cap.</summary>
    private static readonly QuicksandSurfacePhysics withGravitySuit =
        new(SnesSignedSixteenSixteen.FromSixteenthsOfPixel(32),
            SnesSignedSixteenSixteen.FromSixteenthsOfPixel(16),
            SnesSignedSixteenSixteen.FromSixteenthsOfPixel(56));

    /// <summary>How far submerging quicksand pulls Samus down each frame she is inside it: 1.125 pixels.</summary>
    internal static readonly SnesSignedSixteenSixteen SubmergingSinkDisplacement = SnesSignedSixteenSixteen.FromSixteenthsOfPixel(18);

    /// <summary>How far a slow sandfall carries Samus down each frame she is inside it: 1.25 pixels.</summary>
    internal static readonly SnesSignedSixteenSixteen SlowSandfallDisplacement = SnesSignedSixteenSixteen.FromSixteenthsOfPixel(20);

    /// <summary>How far a fast sandfall carries Samus down each frame she is inside it: 1.75 pixels.</summary>
    internal static readonly SnesSignedSixteenSixteen FastSandfallDisplacement = SnesSignedSixteenSixteen.FromSixteenthsOfPixel(28);

    /// <summary>
    /// The most a stationary Samus's downward probe may sink into a quicksand surface: 0.1875 pixels. The surface
    /// compares and replaces only the probe's middle word (whole and upper fraction byte).
    /// </summary>
    internal static readonly SnesSignedSixteenSixteen StationarySurfaceProbeLimit = SnesSignedSixteenSixteen.FromSixteenthsOfPixel(3);
}
