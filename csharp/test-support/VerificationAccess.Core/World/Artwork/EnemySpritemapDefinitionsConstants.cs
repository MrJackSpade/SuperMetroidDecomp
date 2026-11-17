using SuperMetroid.Core.Enemies.Artwork;
using static SuperMetroid.AssetExtraction.World.Artwork.EnemySpritemapToolingDefinitions;

namespace SuperMetroid.AssetExtraction.World.Artwork;

/// <summary>
/// Places in the <see cref="EnemySpritemapDefinitions"/> document that only verification reads: the number of frames
/// the document lists before each family's first frame. The places are read from the document order, so a deleted or
/// merged frame moves every later place with it; the per-family counts verification checks between them stay its own.
/// </summary>
internal static class EnemySpritemapDefinitionsConstants
{
    /// <summary>Frames the document lists before the gunship display compositions.</summary>
    internal static int PreGunshipFrameCount => FramesBefore(EnemyFrameId.GunshipEntrancePadShut);
    /// <summary>Frames the document lists before Botwoon's visual frames.</summary>
    internal static int PreBotwoonFrameCount => FramesBefore(EnemyFrameId.BotwoonHeadClosedUpLeft);
    /// <summary>Frames the document lists before the Yard's visual frames.</summary>
    internal static int PreYardFrameCount => FramesBefore(EnemyFrameId.YardRoundingOutsideCornerUpsideRightFacingUpTurned22);
    /// <summary>Frames the document lists before the work robot's visual frames.</summary>
    internal static int PreWorkRobotFrameCount => FramesBefore(EnemyFrameId.WorkRobotWalkingLeftNearFootLeanBack);
    /// <summary>Frames the document lists before the Evir's visual frames.</summary>
    internal static int PreEvirFrameCount => FramesBefore(EnemyFrameId.EvirBodyFacingLeftLegsTucked);
    /// <summary>Frames the document lists before the Mochtroid's visual frames.</summary>
    internal static int PreMochtroidFrameCount => FramesBefore(EnemyFrameId.MochtroidFloatingWide);
    /// <summary>Frames the document lists before the Tourian corpse artwork.</summary>
    internal static int PreDeadTourianCorpseFrameCount => FramesBefore(EnemyFrameId.DeadZoomerCorpseOnItsBack);
    /// <summary>Frames the document lists before the stationary dead Torizo.</summary>
    internal static int PreDeadTorizoStationaryFrameCount => FramesBefore(EnemyFrameId.DeadTorizoStanding);
    /// <summary>Frames the document lists before the Rinka's visual frames.</summary>
    internal static int PreRinkaFrameCount => FramesBefore(EnemyFrameId.RinkaWhiteHot);
    /// <summary>Frames the document lists before the Viola's visual frames.</summary>
    internal static int PreViolaFrameCount => FramesBefore(EnemyFrameId.ViolaPlain);
    /// <summary>Frames the document lists before the Chozo statue artwork.</summary>
    internal static int PreChozoStatueFrameCount => FramesBefore(EnemyFrameId.ChozoStatueLowerNorfairHandsCupped);
    /// <summary>Frames the document lists before the Norfair lava jumper's frames.</summary>
    internal static int PreNorfairLavaJumperFrameCount => FramesBefore(EnemyFrameId.NorfairLavaJumperClawsCurledDown);
    /// <summary>Frames the document lists before the Multiviola's visual frames.</summary>
    internal static int PreMultiviolaFrameCount => FramesBefore(EnemyFrameId.MultiviolaPlain);
    /// <summary>Frames the document lists before the Dragon's visual frames.</summary>
    internal static int PreDragonFrameCount => FramesBefore(EnemyFrameId.DragonBodyFacingLeft);
    /// <summary>Frames the document lists before the Tripper and Kamer compositions.</summary>
    internal static int PreTripperKamerFrameCount => FramesBefore(EnemyFrameId.TripperHoveringFacingLeftJetShort);
    /// <summary>Frames the document lists before Shaktool's visual frames.</summary>
    internal static int PreShaktoolFrameCount => FramesBefore(EnemyFrameId.ShaktoolSawHandFinalPieceTurned0);
    /// <summary>Frames the document lists before the ordinary Metroid's visual frames.</summary>
    internal static int PreMetroidFrameCount => FramesBefore(EnemyFrameId.MetroidNucleiDark);
    /// <summary>Frames the document lists before the shutter compositions.</summary>
    internal static int PreShutterFrameCount => FramesBefore(EnemyFrameId.ShutterTenPixelsLong);
    /// <summary>Frames the document lists before the Morph Ball eye artwork.</summary>
    internal static int PreMorphBallEyeFrameCount => FramesBefore(EnemyFrameId.MorphBallEyeLookingUpUpRight);
    /// <summary>Frames the document lists before the face block's visual frames.</summary>
    internal static int PreFaceBlockFrameCount => FramesBefore(EnemyFrameId.FaceBlockNeutral);
    /// <summary>Frames the document lists before the Kago's visual frames.</summary>
    internal static int PreKagoFrameCount => FramesBefore(EnemyFrameId.KagoHiveAtRest);
    /// <summary>Frames the document lists before the Fly's visual frames.</summary>
    internal static int PreFlyFrameCount => FramesBefore(EnemyFrameId.FlyWingsRaisedOut);
    /// <summary>Frames the document lists before the Sciser's visual frames.</summary>
    internal static int PreSciserFrameCount => FramesBefore(EnemyFrameId.SciserUpsideRightLegsTucked);
    /// <summary>Frames the document lists before the supplementary Ridley artwork.</summary>
    internal static int PreRidleySupplementFrameCount => FramesBefore(EnemyFrameId.RidleyTailBaseLink);
    /// <summary>Frames the document lists before the dead Torizo's artwork.</summary>
    internal static int PreDeadTorizoFrameCount => FramesBefore(EnemyFrameId.DeadTorizoRubbleMound);
    /// <summary>Frames the document lists before Mother Brain's visual frames.</summary>
    internal static int PreMotherBrainFrameCount => FramesBefore(EnemyFrameId.MotherBrainHeadEyeShut);
    /// <summary>Frames the document lists before the Ki-Hunter's artwork.</summary>
    internal static int PreKiHunterFrameCount => FramesBefore(EnemyFrameId.KiHunterHoveringFacingLeft);
    /// <summary>Frames the document lists before the Yapping Maw's artwork.</summary>
    internal static int PreYappingMawFrameCount => FramesBefore(EnemyFrameId.YappingMawFacingUpJawsWide);
    /// <summary>Frames the document lists before the room sprite-object compositions.</summary>
    internal static int PreRoomEffectActorFrameCount => FramesBefore(EnemyFrameId.EffectActorBigDustCloudPuff);
    /// <summary>Frames the document lists before Draygon's breath frames.</summary>
    internal static int PreDraygonBreathFrameCount => FramesBefore(EnemyFrameId.EffectActorDraygonBreathBubblesForming);
    /// <summary>Frames the document lists before Draygon's intro frames.</summary>
    internal static int PreDraygonIntroFrameCount => FramesBefore(EnemyFrameId.EffectActorEvirFacingLeftLegsStretched);
    /// <summary>Frames the document lists before the elevator's visual frames.</summary>
    internal static int PreElevatorFrameCount => FramesBefore(EnemyFrameId.ElevatorPadUnlit);
    /// <summary>Frames the document lists before the Kamer platform frames.</summary>
    internal static int PreKamerFrameCount => FramesBefore(EnemyFrameId.PlatformLightDark);
    /// <summary>Frames the document lists before the Fune and Namihe frames.</summary>
    internal static int PreFuneNamiheFrameCount => FramesBefore(EnemyFrameId.FuneFacingLeftMouthClosed);
    /// <summary>Frames the document lists before the Sbug frames.</summary>
    internal static int PreSbugFrameCount => FramesBefore(EnemyFrameId.SbugFacingRightWingsUp);
    /// <summary>Frames the document lists before the spiny crawlers' frames.</summary>
    internal static int PreSpinyCrawlerFrameCount => FramesBefore(EnemyFrameId.SpinyCrawlerUpsideRightSpinesTurned0);
    /// <summary>Frames the document lists before the Choot frames.</summary>
    internal static int PreChootFrameCount => FramesBefore(EnemyFrameId.ChootLyingFlat);
    /// <summary>Frames the document lists before the hopper frames.</summary>
    internal static int PreHopperFrameCount => FramesBefore(EnemyFrameId.SidehopperLegsStraight);
    /// <summary>Frames the document lists before the Beetom frames.</summary>
    internal static int PreBeetomFrameCount => FramesBefore(EnemyFrameId.BeetomCrawlingLeftLegsTucked);
    /// <summary>Frames the document lists before the Alcoon frames.</summary>
    internal static int PreAlcoonFrameCount => FramesBefore(EnemyFrameId.AlcoonWalkingLeftNearLegStepping);
    /// <summary>Frames the document lists before the Bull frames.</summary>
    internal static int PreBullFrameCount => FramesBefore(EnemyFrameId.BullTentaclesUpright);
    /// <summary>Frames the document lists before the Puyo frames.</summary>
    internal static int PrePuyoFrameCount => FramesBefore(EnemyFrameId.PuyoGroundedTall);
    /// <summary>Frames the document lists before the Norfair Rio frames.</summary>
    internal static int PreNorfairRioFrameCount => FramesBefore(EnemyFrameId.NorfairRioWingsLevel);
    /// <summary>Frames the document lists before the Lower Norfair Rio frames.</summary>
    internal static int PreLowerNorfairRioFrameCount => FramesBefore(EnemyFrameId.LowerNorfairRioWingsRaised);
    /// <summary>Frames the document lists before the ordinary Rio frames.</summary>
    internal static int PreRioFrameCount => FramesBefore(EnemyFrameId.RioWingsUpLegsTucked);
    /// <summary>Frames the document lists before the Ceres Baby Metroid frames.</summary>
    internal static int PreCeresBabyFrameCount => FramesBefore(EnemyFrameId.CeresBabyHorizontal);
    /// <summary>Frames the document lists before the Ceres door frames.</summary>
    internal static int PreCeresDoorFrameCount => FramesBefore(EnemyFrameId.CeresElevatorDoorBlocked);
}
