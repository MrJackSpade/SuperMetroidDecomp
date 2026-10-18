using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Enemies.PipeBugs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Enemies.Sbug;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Yard;
using SuperMetroid.Core.Enemies.Zoa;
using SuperMetroid.Core.Enemies.Owtch;
using SuperMetroid.Core.Enemies.Bull;
using SuperMetroid.Core.Enemies.Boyon;
using SuperMetroid.Core.Enemies.Fireflea;
using SuperMetroid.Core.Enemies.Shutters;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.Core.Enemies.Kzan;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies.MaridiaSnails;
using SuperMetroid.Core.Enemies.Boulder;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Enemies.Hoppers;
using SuperMetroid.Core.Enemies.Alcoon;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Atomic;
using SuperMetroid.Core.Enemies.Beetom;
using SuperMetroid.Core.Enemies.Choot;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.FaceBlocks;
using SuperMetroid.Core.Enemies.Flies;
using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.Verification.Enemies.FuneNamihe;
using SuperMetroid.Core.Enemies.Hibashi;
using SuperMetroid.Core.Enemies.Kago;
using SuperMetroid.Core.Enemies.LavaJumpers;
using SuperMetroid.Core.Enemies.MamaTurtles;
using SuperMetroid.Core.Enemies.Metaree;
using SuperMetroid.Core.Enemies.Nuclear;
using SuperMetroid.Core.Enemies.Skree;
using SuperMetroid.Core.Enemies.Platforms;
using SuperMetroid.Core.Enemies.Polyp;
using SuperMetroid.Core.Enemies.Powamp;
using SuperMetroid.Core.Enemies.Puyo;
using SuperMetroid.Core.Enemies.Rios;
using SuperMetroid.Core.Enemies.Ripper;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.Core.Enemies.Skultera;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Enemies.Sparks;
using SuperMetroid.Core.Enemies.Stoke;
using SuperMetroid.Core.Enemies.Waver;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
namespace SuperMetroid.Verification.Enemies;

/// <summary>
/// Translates a native snapshot's enemy variable words into the typed state each enemy family owns. The generic
/// snapshot import writes every slot's words first; this codec then gives each family its state from them, rejecting
/// a word outside the range the family's own updates can produce.
/// </summary>
internal static class NativeEnemyFamilyStateImport
{
    /// <summary>Translates the recorded variable words of the enemy in <paramref name="slot"/> into its family's state.</summary>
    /// <param name="enemies">The enemy system whose family states receive the values.</param>
    /// <param name="slot">The imported slot, its variable and parameter words already written.</param>
    internal static void Import(RoomEnemySystem enemies, RoomEnemySlot slot)
    {
        int index = slot.SlotIndex;
        if (slot.SpeciesState<GunshipPartState>() is { } gunshipRecord)
        {
            gunshipRecord.FunctionTimer = slot.VariableA;
            gunshipRecord.LiftoffTransferIndex = slot.VariableB;
            gunshipRecord.BobPhase = slot.VariableC;
            gunshipRecord.BobTimer = slot.VariableD;
            gunshipRecord.BaseY = slot.VariableE;
            gunshipRecord.EscapeVelocity = slot.VariableF;
        }
        if (slot.SpeciesState<CeresSteamEnemyState>() is { } ceresSteamRecord)
        {
            ceresSteamRecord.Function = slot.VariableA == 0 ? null : (CeresSteamFunction)slot.VariableA;
            ceresSteamRecord.ActivationDelay = NativeCountdown.FromRaw(slot.VariableD);
        }
        if (slot.SpeciesState<CeresDoorEnemyState>() is { } ceresDoorRecord)
        {
            ceresDoorRecord.Function = slot.VariableA == 0 ? null : (CeresDoorFunction)slot.VariableA;
            ceresDoorRecord.HandsDrawingToRidley = slot.VariableB != 0;
            ceresDoorRecord.RumbleCallsLeft = slot.VariableD;
            ceresDoorRecord.ExplosionIntervalLeft = slot.VariableE;
            ceresDoorRecord.ExplosionPositionIndex = slot.VariableF;
        }
        if (slot.SpeciesState<DeadTorizoEnemyState>() is { } deadTorizoRecord)
        {
            deadTorizoRecord.Function = slot.VariableA == 0 ? null : (DeadTorizoFunction)slot.VariableA;
            deadTorizoRecord.Velocity = new(new SnesSignedEightEight(slot.VariableB), new SnesSignedEightEight(slot.VariableC));
        }
        if (slot.SpeciesState<MotherBrainFallingTubeState>() is { } fallingTubeRecord)
        {
            fallingTubeRecord.Function = slot.VariableA == 0 ? null : (MotherBrainTubeFunction)slot.VariableA;
            fallingTubeRecord.FloorY = slot.VariableB;
            fallingTubeRecord.FallSpeed = new SnesSignedEightEight(slot.VariableC);
            fallingTubeRecord.SmokeDelay = NativeCountdown.FromRaw(slot.VariableD);
            fallingTubeRecord.SmokePositionIndex = slot.VariableE;
        }
        if (slot.SpeciesState<RidleyExplosionFragmentState>() is { } ridleyFragmentRecord)
        {
            ridleyFragmentRecord.Velocity = new(new SnesSignedEightEight(slot.VariableB), new SnesSignedEightEight(slot.VariableC));
            ridleyFragmentRecord.Lifetime = NativeCountdown.FromRaw(slot.VariableF);
        }
        if (slot.SpeciesState<DraygonEnemyState>() is { } draygonRecord)
        {
            draygonRecord.Function = (DraygonAiFunction)slot.VariableA;
            draygonRecord.SwoopOffset = slot.VariableB;
            draygonRecord.SwoopEndOffset = slot.VariableC;
            draygonRecord.SwoopVelocity = SnesSixteenSixteen.Compose(slot.VariableD, slot.VariableE);
        }
        if (slot.SpeciesState<DraygonEyeState>() is { } draygonEyeRecord)
        {
            draygonEyeRecord.Function = slot.VariableA == 0 ? null : (DraygonEyeFunction)slot.VariableA;
            draygonEyeRecord.LookAngle = slot.VariableF;
        }
        if (slot.SpeciesState<CrocomireEnemyState>() is { } crocomireRecord)
        {
            crocomireRecord.DeathSequenceIndex = (CrocomireDeathPhase)slot.VariableA;
            crocomireRecord.FightFlags = (CrocomireFightFlags)slot.VariableB;
            crocomireRecord.FightFunction = (CrocomireFightFunction)slot.VariableC;
            crocomireRecord.RumbleTableOffset = slot.VariableD;
            crocomireRecord.VerticalSpeed = slot.VariableE;
            crocomireRecord.ProjectileCounter = slot.VariableF;
        }
        if (slot.SpeciesState<CrocomireTongueState>() is { } crocomireTongueRecord)
            crocomireTongueRecord.BodyXOffset = slot.VariableA;
        if (slot.SpeciesState<DeadSidehopperEnemyState>() is { } deadSidehopperRecord)
        {
            deadSidehopperRecord.Function = slot.VariableA == 0 ? null : (DeadSidehopperAiFunction)slot.VariableA;
            deadSidehopperRecord.PreRotDelayCounter = slot.VariableB;
            deadSidehopperRecord.StateTimer = NativeCountdown.FromRaw(slot.VariableF);
        }
        if (slot.SpeciesState<BotwoonEnemyState>() is { } botwoonRecord)
        {
            botwoonRecord.Function = slot.VariableD == 0 ? null : (BotwoonEnemyFunction)slot.VariableD;
            botwoonRecord.MovementFunction = slot.VariableE == 0 ? null : (BotwoonMovementFunction)slot.VariableE;
            botwoonRecord.HeadFunction = slot.VariableF == 0 ? null : (BotwoonHeadFunction)slot.VariableF;
        }
        if (slot.SpeciesState<ShitroidEnemyState>() is { } shitroidRecord)
        {
            shitroidRecord.Function = slot.VariableA == 0 ? null : (ShitroidAiFunction)slot.VariableA;
            shitroidRecord.Velocity = new(new SnesSignedEightEight(slot.VariableB), new SnesSignedEightEight(slot.VariableC));
            shitroidRecord.PalettePhase = SnesLittleEndian.LowByte(slot.VariableD);
            shitroidRecord.PaletteTimer = SnesLittleEndian.HighByte(slot.VariableD);
            shitroidRecord.PaletteDelay = slot.VariableE;
            shitroidRecord.StateTimer = NativeCountdown.FromRaw(slot.VariableF);
        }
        if (slot.SpeciesState<DachoraEnemyState>() is { } dachoraRecord)
        {
            dachoraRecord.Speed = SnesSignedSixteenSixteen.FromParts(unchecked((short)slot.VariableA), slot.VariableB);
            dachoraRecord.VerticalAcceleration = SnesSignedSixteenSixteen.FromParts(unchecked((short)slot.VariableC), slot.VariableD);
            dachoraRecord.SetPaletteCycle(frame: SnesLittleEndian.HighByte(slot.VariableE), ticks: SnesLittleEndian.LowByte(slot.VariableE));
            dachoraRecord.Function = slot.VariableF == 0 ? null : (DachoraAiFunction)slot.VariableF;
        }
        if (slot.SpeciesState<PipeBugEnemyState>() is { } pipeBugRecord)
        {
            ushort pipeBugFunction = pipeBugRecord.IsBrinstar ? slot.VariableF : slot.VariableA;
            pipeBugRecord.Function = pipeBugFunction == 0 ? null : (PipeBugEnemyFunction)pipeBugFunction;
            pipeBugRecord.BrinstarFliesLeft = pipeBugRecord.IsBrinstar && EnemyMath.IsNegative16(slot.VariableA);
        }
        if (slot.SpeciesState<MagdolliteEnemyState>() is { } magdolliteRecord)
        {
            magdolliteRecord.Function = slot.VariableF == 0 ? null : (MagdolliteEnemyFunction)slot.VariableF;
            magdolliteRecord.BodyPhase = slot.VariableB / 2;
            magdolliteRecord.ThrowsRight = slot.VariableB != 0;
            magdolliteRecord.RiseDisplacement = new(0, unchecked((short)slot.VariableE));
        }
        if (slot.SpeciesState<SbugEnemyState>() is { } sbugRecord)
        {
            sbugRecord.StraightMovementTimer = NativeCountdown.FromRaw(slot.VariableA);
            sbugRecord.Function = slot.VariableB == 0 ? null : (SbugEnemyFunction)slot.VariableB;
            sbugRecord.FaceCustomHeading(SnesAngle.FromTableIndex(SnesLittleEndian.LowByte(slot.VariableC)));
            sbugRecord.MovementTimer = NativeCountdown.FromRaw(slot.VariableF);
        }
        if (slot.SpeciesState<KiHunterEnemyState>() is { } kiHunterRecord)
        {
            kiHunterRecord.Function = slot.VariableA == 0 ? null : (KiHunterEnemyFunction)slot.VariableA;
            kiHunterRecord.SwoopTarget = new(slot.VariableB, 0);
            kiHunterRecord.DetachedSpeedIndex = slot.VariableB;
            kiHunterRecord.SwoopOrigin = new(0, slot.VariableC);
            kiHunterRecord.DetachedSpeedAccumulator = slot.VariableD;
            kiHunterRecord.AnimationChangeAngle = slot.VariableE;
            kiHunterRecord.SwoopAngle = slot.VariableF;
            kiHunterRecord.WingArcAngle = SnesAngle.FromRaw(slot.VariableF);
        }
        if (slot.SpeciesState<ZebetiteEnemyState>() is { } zebetiteRecord)
        {
            zebetiteRecord.Function = slot.VariableA == 0 ? null : (ZebetiteAiFunction)slot.VariableA;
            zebetiteRecord.PaletteCycle = slot.VariableC;
            zebetiteRecord.Generation = slot.VariableD;
            zebetiteRecord.GenerationFlags = (ZebetiteGenerationFlags)slot.VariableF;
        }
        if (slot.SpeciesState<ChozoStatueState>() is { } chozoStatueRecord)
        {
            chozoStatueRecord.CarryRecord = slot.VariableC / 2;
            chozoStatueRecord.PreInstruction = slot.VariableF == 0 ? null : (ChozoStatuePreInstruction)slot.VariableF;
        }
        if (slot.SpeciesState<YardEnemyState>() is { } yardRecord)
        {
            yardRecord.CrawlingVelocity = new(new SnesSignedEightEight(slot.VariableA), new SnesSignedEightEight(slot.VariableB));
            yardRecord.AirborneFacingDirection = slot.VariableC;
            yardRecord.ConsecutiveTurnCounter = slot.VariableE;
            yardRecord.MovementFunction = slot.VariableF == 0 ? null : (YardMovementFunction)slot.VariableF;
        }
        if (slot.SpeciesState<ZoaEnemyState>() is { } zoaRecord)
        {
            zoaRecord.SpawnPosition = new(slot.VariableA, slot.VariableB);
            zoaRecord.InstructionListTableIndex = (ZoaAnimationSelector)slot.VariableC;
            zoaRecord.PreviousInstructionListTableIndex = (ZoaAnimationSelector)slot.VariableD;
            zoaRecord.Function = slot.VariableF == 0 ? null : (ZoaEnemyFunction)slot.VariableF;
        }
        if (slot.SpeciesState<BabyTurtleEnemyState>() is { } babyTurtleRecord)
        {
            babyTurtleRecord.Function = slot.VariableA == 0 ? null : (BabyTurtleAiFunction)slot.VariableA;
            babyTurtleRecord.ParentSlotIndex = slot.VariableB % RoomEnemySystem.NativeSlotSize == 0
            ? slot.VariableB / RoomEnemySystem.NativeSlotSize
            : throw new InvalidDataException($"Baby Turtle parent offset {slot.VariableB} is not a slot boundary.");
            babyTurtleRecord.SpawnTopCenter = new(slot.VariableC, slot.VariableD);
            babyTurtleRecord.Velocity = new(unchecked((short)slot.VariableE), babyTurtleRecord.Velocity.Y);
        }
        if (slot.SpeciesState<OwtchEnemyState>() is { } owtchRecord)
        {
            owtchRecord.RightVelocity = owtchRecord.RightVelocity.WithWholePixels(unchecked((short)slot.VariableA));
            owtchRecord.LeftVelocity = SnesSignedSixteenSixteen.FromParts(unchecked((short)slot.VariableC), slot.VariableB);
            owtchRecord.Behavior = (OwtchBehaviorState)slot.VariableD;
            owtchRecord.BurialDepth = slot.VariableE;
        }
        if (slot.SpeciesState<BullEnemyState>() is { } bullRecord)
        {
            bullRecord.Function = slot.VariableA == 0 ? null : (BullEnemyFunction)slot.VariableA;
            bullRecord.Acceleration = slot.VariableB;
            bullRecord.AccelerationDelta = slot.VariableC;
            bullRecord.Speed = slot.VariableD;
            bullRecord.ActivationTimer = NativeCountdown.FromRaw(slot.VariableE);
            bullRecord.AccelerationIntervalTimer = NativeCountdown.FromRaw(slot.VariableF);
        }
        if (slot.SpeciesState<BoyonEnemyState>() is { } boyonRecord)
        {
            boyonRecord.SpeedMultiplier = slot.VariableA;
            boyonRecord.Speed = slot.VariableB;
            boyonRecord.JumpHeight = slot.VariableC;
            boyonRecord.DistanceAccumulator = slot.VariableD;
            boyonRecord.SpeedTableIndex = slot.VariableE;
            boyonRecord.Bouncing = slot.VariableF != 0;
        }
        if (slot.SpeciesState<GrowingShutterEnemyState>() is { } growingShutterRecord)
        {
            growingShutterRecord.Function = slot.VariableA == 0 ? null : (GrowingShutterFunction)slot.VariableA;
            growingShutterRecord.GrowthLevel0Origin = new(0, slot.VariableB);
            growingShutterRecord.GrowthLevel1Origin = new(0, slot.VariableC);
            growingShutterRecord.GrowthLevel2Origin = new(0, slot.VariableD);
            growingShutterRecord.GrowthLevel3Origin = new(0, slot.VariableE);
            growingShutterRecord.GrowthLevel = slot.VariableF;
        }
        if (slot.SpeciesState<FirefleaEnemyState>() is { } firefleaRecord)
        {
            firefleaRecord.AngularSpeed = SnesSignedSixteenSixteen.FromParts(unchecked((short)slot.VariableB), slot.VariableA);
            firefleaRecord.Radius = slot.VariableC;
            firefleaRecord.Angle = SnesAngle.FromRaw(slot.VariableD);
            firefleaRecord.Center = new(slot.VariableE, slot.VariableF);
        }
        if (slot.SpeciesState<VerticalShutterEnemyState>() is { } verticalShutterRecord)
        {
            verticalShutterRecord.Function = slot.VariableA == 0 ? null : (VerticalShutterFunction)slot.VariableA;
            verticalShutterRecord.FunctionTimer = NativeCountdown.FromRaw(slot.VariableB);
            verticalShutterRecord.DownVelocity = SnesSignedSixteenSixteen.FromParts(unchecked((short)slot.VariableD), slot.VariableC);
            verticalShutterRecord.UpVelocity = SnesSignedSixteenSixteen.FromParts(unchecked((short)slot.VariableF), slot.VariableE);
        }
        if (slot.SpeciesState<HorizontalShutterEnemyState>() is { } horizontalShutterRecord)
        {
            horizontalShutterRecord.Function = slot.VariableA == 0 ? null : (HorizontalShutterFunction)slot.VariableA;
            horizontalShutterRecord.FunctionTimer = NativeCountdown.FromRaw(slot.VariableB);
            horizontalShutterRecord.RightVelocity = SnesSignedSixteenSixteen.FromParts(unchecked((short)slot.VariableD), slot.VariableC);
            horizontalShutterRecord.LeftVelocity = SnesSignedSixteenSixteen.FromParts(unchecked((short)slot.VariableF), slot.VariableE);
        }
        if (slot.SpeciesState<CacatacEnemyState>() is { } cacatacRecord)
        {
            cacatacRecord.RightVelocity = cacatacRecord.RightVelocity.WithWholePixels(unchecked((short)slot.VariableA));
            cacatacRecord.LeftVelocity = SnesSignedSixteenSixteen.FromParts(unchecked((short)slot.VariableC), slot.VariableB);
            cacatacRecord.Direction = (CacatacDirection)slot.VariableD;
            cacatacRecord.Function = slot.VariableE == 0 ? null : (CacatacEnemyFunction)slot.VariableE;
        }
        if (slot.SpeciesState<WorkRobotEnemyState>() is { } workRobotRecord)
        {
            workRobotRecord.LaserVelocity = new(new SnesSignedEightEight(slot.VariableA), default);
            workRobotRecord.LaserCooldown = NativeCountdown.FromRaw(slot.VariableB);
            workRobotRecord.BackupPosition = new(slot.VariableC, slot.VariableD);
            workRobotRecord.FallVelocity = SnesSignedSixteenSixteen.FromParts(unchecked((short)slot.VariableF), slot.VariableE);
        }
        if (slot.SpeciesState<KzanEnemyState>() is { } kzanRecord)
        {
            kzanRecord.Function = slot.VariableA == 0 ? null : (KzanEnemyFunction)slot.VariableA;
            kzanRecord.FallWaitTimer = NativeCountdown.FromRaw(slot.VariableB);
            kzanRecord.RisingTarget = new(0, slot.VariableC);
            kzanRecord.FallingTarget = new(0, slot.VariableD);
            kzanRecord.InitialFallingSpeed = SnesSignedSixteenSixteen.FromParts(unchecked((short)slot.VariableF), slot.VariableE);
        }
        if (slot.SpeciesState<MetroidEnemyState>() is { } metroidRecord)
        {
            metroidRecord.Velocity = FixedVector<RoomPixels>.FromWords(slot.VariableB, slot.VariableA, slot.VariableD, slot.VariableC);
            metroidRecord.EscapeTimer = NativeCountdown.FromRaw(slot.VariableE);
            metroidRecord.Function = (MetroidAiFunction)slot.VariableF;
        }
        if (slot.SpeciesState<FakeKraidEnemyState>() is { } fakeKraidRecord)
        {
            fakeKraidRecord.WalkDelta = unchecked((short)slot.VariableB);
            fakeKraidRecord.FacingDelta = unchecked((short)slot.VariableC);
            fakeKraidRecord.WalkStepTimer = NativeCountdown.FromRaw(slot.VariableD);
            fakeKraidRecord.SpitDecisionTimer = NativeCountdown.FromRaw(slot.VariableE);
        }
        if (slot.SpeciesState<MaridiaLargeSnailEnemyState>() is { } maridiaLargeSnailRecord)
        {
            maridiaLargeSnailRecord.Function = slot.VariableA == 0 ? null : (MaridiaLargeSnailEnemyFunction)slot.VariableA;
            maridiaLargeSnailRecord.YSpeedTableIndex = unchecked((short)slot.VariableB);
            maridiaLargeSnailRecord.AttackCooldown = NativeCountdown.FromRaw(slot.VariableC);
            maridiaLargeSnailRecord.MovingLeft = slot.VariableD != 0;
            maridiaLargeSnailRecord.RemainingBounces = NativeCountdown.FromRaw(slot.VariableE);
            maridiaLargeSnailRecord.BounceFunction = slot.VariableF == 0 ? null : (MaridiaLargeSnailBounceFunction)slot.VariableF;
        }
        if (slot.SpeciesState<WallSpacePirateEnemyState>() is { } wallSpacePirateRecord)
        {
            wallSpacePirateRecord.Function = slot.VariableA == 0 ? null : (WallSpacePirateFunction)slot.VariableA;
            wallSpacePirateRecord.WallJumpDestination = new(slot.VariableB, 0);
            wallSpacePirateRecord.ClimbDirection = (WallSpacePirateClimbDirection)slot.VariableC;
            wallSpacePirateRecord.WallJumpArcCenter = new(slot.VariableD, slot.VariableE);
            wallSpacePirateRecord.WallJumpArcAngle = SnesAngle.FromTableIndex(checked((byte)slot.VariableF));
        }
        if (slot.SpeciesState<NinjaSpacePirateEnemyState>() is { } ninjaSpacePirateRecord)
        {
            ninjaSpacePirateRecord.Function = slot.VariableA == 0 ? null : (NinjaSpacePirateFunction)slot.VariableA;
            ninjaSpacePirateRecord.CalculatedSpinJumpSpeed = slot.VariableB;
            ninjaSpacePirateRecord.PostsMidpoint = new(slot.VariableD, 0);
            ninjaSpacePirateRecord.LeftPost = new(slot.VariableE, 0);
            ninjaSpacePirateRecord.RightPost = new(slot.VariableF, 0);
        }
        if (slot.SpeciesState<SkreeEnemyState>() is { } skreeRecord)
        {
            skreeRecord.BurrowTimer = NativeCountdown.FromRaw(slot.VariableA);
            skreeRecord.Function = slot.VariableB == 0 ? null : (SkreeEnemyFunction)slot.VariableB;
            skreeRecord.RequestedInstructionIndex = (SkreeMetareeAnimationPhase)slot.VariableC;
            skreeRecord.InstalledInstructionIndex = (SkreeMetareeAnimationPhase)slot.VariableD;
            skreeRecord.AttackReady = slot.VariableE != 0;
        }
        if (slot.SpeciesState<BoulderEnemyState>() is { } boulderRecord)
        {
            boulderRecord.Function = slot.VariableA == 0 ? null : (BoulderAiFunction)slot.VariableA;
            boulderRecord.TravelSpeedAccumulator = slot.VariableB;
            boulderRecord.BounceSpeedAccumulator = slot.VariableC;
            boulderRecord.BounceCounter = NativeCountdown.FromRaw(slot.VariableD);
            boulderRecord.DirectionSelector = slot.VariableE;
        }
        if (slot.SpeciesState<RinkaEnemyState>() is { } rinkaRecord)
        {
            rinkaRecord.Function = slot.VariableA == 0 ? null : (RinkaEnemyFunction)slot.VariableA;
            rinkaRecord.Velocity = new(new SnesSignedEightEight(slot.VariableB), new SnesSignedEightEight(slot.VariableC));
            rinkaRecord.SpawnResourceToken = slot.VariableD;
            rinkaRecord.DelayTimer = NativeCountdown.FromRaw(slot.VariableF);
        }
        if (slot.SpeciesState<EtecoonEnemyState>() is { } etecoonRecord)
        {
            etecoonRecord.Velocity = FixedVector<RoomPixels>.FromWords(slot.VariableC, slot.VariableD, slot.VariableA, slot.VariableB);
            etecoonRecord.FunctionTimer = NativeCountdown.FromRaw(slot.VariableE);
            etecoonRecord.Function = slot.VariableF == 0 ? null : (EtecoonAiFunction)slot.VariableF;
        }
        if (slot.SpeciesState<MochtroidEnemyState>() is { } mochtroidRecord)
        {
            mochtroidRecord.Velocity = FixedVector<RoomPixels>.FromWords(slot.VariableB, slot.VariableA, slot.VariableD, slot.VariableC);
            mochtroidRecord.ShakeTimer = NativeCountdown.FromRaw(slot.VariableE);
            mochtroidRecord.MovementMode = (MochtroidMovementMode)slot.VariableF;
        }
        if (slot.SpeciesState<MorphBallEyeEnemyState>() is { } morphBallEyeRecord)
        {
            morphBallEyeRecord.IsActivated = slot.VariableC != 0;
            morphBallEyeRecord.Angle = SnesAngle.FromTableIndex(unchecked((byte)slot.VariableD));
            morphBallEyeRecord.FunctionTimer = NativeCountdown.FromRaw(slot.VariableE);
            morphBallEyeRecord.Function = slot.VariableF == 0 ? null : (MorphBallEyeAiFunction)slot.VariableF;
        }
        if (slot.SpeciesState<HopperEnemyState>() is { } hopperRecord)
        {
            hopperRecord.Function = slot.VariableA == 0 ? null : (HopperEnemyFunction)slot.VariableA;
            hopperRecord.YSpeedTableIndex = slot.VariableB;
            hopperRecord.XVelocity = unchecked((short)slot.VariableC);
            hopperRecord.YSpeedTableIndexDelta = slot.VariableD;
        }
        if (enemies.PlatformStates[index] is { } platform)
        {
            platform.RestingHeight = new Position16<RoomPixels>(0, slot.VariableA);
            platform.RightSpeed = SnesSignedSixteenSixteen.FromParts(unchecked((short)slot.VariableC), slot.VariableB);
            platform.LeftSpeed = SnesSignedSixteenSixteen.FromParts(unchecked((short)slot.VariableE), slot.VariableD);
            platform.YSpeedTableIndex = Index(slot.VariableF, platform.MaximumYSpeedTableIndex, "platform acceleration index");
        }
        if (enemies.BeetomStates[index] is { } beetom)
        {
            beetom.YSpeedTableIndex = Index(slot.VariableB, short.MaxValue, "Beetom speed index");
            beetom.Function = slot.VariableC == 0 ? null : (BeetomEnemyFunction)slot.VariableC;
            beetom.FunctionTimer = NativeCountdown.FromRaw(slot.VariableD);
            beetom.ButtonCounter = NativeCountdown.FromRaw(slot.VariableE);
            beetom.PreviousController1Input = (SnesButton)slot.VariableF;
        }
        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Shaktool && enemies.ShaktoolSegments[index] is { } segment)
        {
            segment.TargetAngle = SnesAngle.FromRaw(slot.VariableA);
            segment.OrbitAngle = SnesAngle.FromRaw(slot.VariableB);
            segment.AngularVelocity = Index(slot.VariableC, 1020, "Shaktool angular velocity");
            ImportShaktoolOrientation(segment, slot.Parameter2 >> 1, slot.VariableD);
            segment.OwnerSlotIndex = SlotIndex(slot.VariableE, "Shaktool owner");
            segment.PreInstruction = slot.VariableF == 0 ? null : (ShaktoolPreInstruction)slot.VariableF;
        }
        if (enemies.ChootStates[index] is { } choot)
        {
            choot.Function = slot.VariableA == 0 ? null : (ChootEnemyFunction)slot.VariableA;
            // The ascent cursor is 8.8 and moves by whole records, so its signed high byte is the record index.
            choot.YSpeedTableIndex = unchecked((short)slot.VariableB) >> 8;
            choot.FallingPatternFrame = (slot.VariableC & 0xff) == 0
                ? slot.VariableC >> 8
                : throw new InvalidDataException($"Recorded Choot falling cursor ${slot.VariableC:X4} is not a whole frame.");
            choot.FallingPatternLoopCounter = NativeCountdown.FromRaw(slot.VariableD);
            choot.FallingPath = slot.VariableE == 0 ? null : ClosedNativeWords.Decode<ChootFallingPath>(slot.VariableE, "recorded Choot falling pattern");
            choot.FallingPatternYDistance = slot.VariableF;
        }
        if (enemies.StokeStates[index] is { } stoke)
        {
            stoke.RightVelocity = SnesSignedSixteenSixteen.FromParts(unchecked((short)slot.VariableB), slot.VariableA);
            stoke.LeftVelocity = SnesSignedSixteenSixteen.FromParts(unchecked((short)slot.VariableD), slot.VariableC);
            stoke.Direction = (StokeDirection)slot.VariableE;
            stoke.Function = slot.VariableF == 0 ? null : (StokeAiFunction)slot.VariableF;
        }
        if (enemies.SkulteraStates[index] is { } skultera)
        {
            skultera.Function = slot.VariableA == 0 ? null : (SkulteraEnemyFunction)slot.VariableA;
            skultera.RightVelocity = SnesSignedSixteenSixteen.FromParts(unchecked((short)slot.VariableC), slot.VariableB);
            skultera.LeftVelocity = SnesSignedSixteenSixteen.FromParts(unchecked((short)slot.VariableE), slot.VariableD);
            // Only the angle word's low byte is ever read; movement discards its high byte.
            skultera.Angle = SnesAngle.FromTableIndex(SnesLittleEndian.LowByte(slot.VariableF));
        }
        if (slot.EntityState<KagoEntity, KagoEnemyState>() is { } kago)
        {
            kago.Function = slot.VariableA == 0 ? null : (KagoEnemyFunction)slot.VariableA;
            kago.UsesFastAnimation = slot.VariableB != 0;
            kago.DeathAnimationStarted = slot.VariableF != 0;
        }
        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Coven && slot.SpeciesState<WreckedShipGhostEnemyState>() is { } ghost)
        {
            ghost.Function = slot.VariableA == 0 ? null : (WreckedShipGhostAiFunction)slot.VariableA;
            ghost.PhaseTimer = NativeCountdown.FromRaw(slot.VariableB);
            ghost.FlickerTableOffset = slot.VariableC;
        }
        if (slot.EntityState<WalkingSpacePirateEntity, WalkingSpacePirateEnemyState>() is { } walkingPirate)
        {
            walkingPirate.Function = slot.VariableA == 0 ? null : (WalkingSpacePirateFunction)slot.VariableA;
            walkingPirate.LeftPost = new(slot.VariableE, 0);
            walkingPirate.RightPost = new(slot.VariableF, 0);
        }
        if (slot.EntityState<MultiviolaEntity, MultiviolaEnemyState>() is { } multiviola)
            multiviola.Velocity = FixedVector<RoomPixels>.FromWords(slot.VariableC, slot.VariableD, slot.VariableE, slot.VariableF);
        if (slot.EntityState<CrawlerEntity, CrawlerEnemyState>() is { } crawler)
        {
            crawler.Velocity = new(new SnesSignedEightEight(slot.VariableA), new SnesSignedEightEight(slot.VariableB));
            crawler.Function = slot.VariableF == 0 ? null : (CrawlerEnemyFunction)slot.VariableF;
        }
        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Holtz && slot.SpeciesState<LowerNorfairRioEnemyState>() is { } holtz)
        {
            holtz.Velocity = new(new SnesSignedEightEight(slot.VariableD), new SnesSignedEightEight(slot.VariableC));
            holtz.Function = slot.VariableF == 0 ? null : (LowerNorfairRioEnemyFunction)slot.VariableF;
            holtz.IsFollower = (slot.Parameter1 & EnemyParameterMasks.FollowerBit) != 0;
        }
        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.EtecoonEscape && slot.SpeciesState<EscapeEtecoonEnemyState>() is { } escape)
        {
            escape.Velocity = new EightEightVector<RoomPixels>(new SnesSignedEightEight(slot.VariableA), default);
            escape.PreInstruction = slot.VariableF == 0 ? null : (EscapeEtecoonPreInstruction)slot.VariableF;
        }
        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.FaceBlock && slot.SpeciesState<BlueBrinstarFaceBlockEnemyState>() is { } faceBlock)
        {
            faceBlock.Activated = slot.VariableA != 0;
            faceBlock.LastHorizontalDelta = slot.VariableB;
        }
        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Puromi && slot.SpeciesState<NuclearWaffleEnemyState>() is { } waffle)
        {
            waffle.Function = slot.VariableA == 0 ? null : (NuclearWaffleEnemyFunction)slot.VariableA;
            waffle.WaitingTimer = NativeCountdown.FromRaw(slot.VariableB);
        }
        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.LavaRocks && slot.SpeciesState<PolypEnemyState>() is { } polyp)
        {
            polyp.Function = slot.VariableA == 0 ? null : (PolypEnemyFunction)slot.VariableA;
            polyp.CooldownTimer = NativeCountdown.FromRaw(slot.VariableB);
        }
        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Powamp && slot.SpeciesState<PowampEnemyState>() is { } powamp)
        {
            // The halves reuse the same words for different values.
            if (powamp.IsBalloon)
            {
                powamp.BalloonSpawn = new(slot.VariableA, slot.VariableB);
                powamp.BalloonGrappleTravelDistance = slot.VariableD;
            }
            else
            {
                powamp.YVelocity = SnesSignedSixteenSixteen.FromParts(unchecked((short)slot.VariableA), slot.VariableB);
                powamp.WiggleIndex = Index(slot.VariableC, 11, "Powamp wiggle phase");
                powamp.WiggleTimer = NativeCountdown.FromRaw(slot.VariableD);
                powamp.FunctionTimer = NativeCountdown.FromRaw(slot.VariableE);
            }
            powamp.Function = slot.VariableF == 0 ? null : (PowampEnemyFunction)slot.VariableF;
        }
        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.MamaTurtle && slot.SpeciesState<MamaTurtleEnemyState>() is { } mama)
        {
            mama.Function = slot.VariableA == 0 ? null : (MamaTurtleAiFunction)slot.VariableA;
            // Only the horizontal velocity's whole pixels live in the record; its fraction and the vertical velocity do not.
            mama.Velocity = mama.Velocity.WithX(mama.Velocity.X.WithWholePixels(unchecked((short)slot.VariableE)));
            mama.AsleepFlag = NativeCountdown.FromRaw(slot.VariableF);
        }
        if (enemies.HibashiStates[index] is { } hibashi)
        {
            hibashi.Function = slot.VariableA == 0 ? null : (HibashiEnemyFunction)slot.VariableA;
            hibashi.InactiveTimer = NativeCountdown.FromRaw(slot.VariableB);
            hibashi.ActivityFinished = slot.VariableC != 0;
            hibashi.SpawnHeight = new Position16<RoomPixels>(0, slot.VariableD);
        }
        if (enemies.AtomicStates[index] is { } atomic)
        {
            atomic.VerticalMovement = slot.VariableA == 0 ? null : (AtomicVerticalMovement)slot.VariableA;
            atomic.HorizontalMovement = slot.VariableB == 0 ? null : (AtomicHorizontalMovement)slot.VariableB;
        }
        if (enemies.AlcoonStates[index] is { } alcoon)
        {
            alcoon.Function = slot.VariableA == 0 ? null : (AlcoonEnemyFunction)slot.VariableA;
            alcoon.Velocity = FixedVector<RoomPixels>.FromWords(slot.VariableD, 0, slot.VariableB, slot.VariableC);
            alcoon.SpawnPosition = alcoon.SpawnPosition.WithY(slot.VariableF);
        }
        if (enemies.DeadTourianCorpses[index] is { } corpse)
        {
            corpse.Function = DeadTourianCorpseFunctionOf(corpse.Species, slot.VariableA);
            corpse.PreRotDelayCounter = slot.VariableB;
        }
        if (enemies.RioStates[index] is { } rio)
        {
            rio.SavedHorizontalVelocity = new SnesSignedEightEight(slot.VariableA);
            rio.Function = slot.VariableB == 0 ? null : (RioEnemyFunction)slot.VariableB;
            rio.Velocity = new(new SnesSignedEightEight(slot.VariableD), new SnesSignedEightEight(slot.VariableC));
            rio.AnimationFinished = slot.VariableE != 0;
        }
        if (slot.EntityState<NorfairRioEntity, NorfairRioEnemyState>() is { } norfairRio)
        {
            norfairRio.Velocity = new(new SnesSignedEightEight(slot.VariableB), new SnesSignedEightEight(slot.VariableA));
            norfairRio.Function = slot.VariableF == 0 ? null : (NorfairRioEnemyFunction)slot.VariableF;
        }
        if (enemies.WaverStates[index] is { } waver)
        {
            waver.XVelocity = SnesSignedSixteenSixteen.FromParts(unchecked((short)slot.VariableB), slot.VariableA);
            waver.CurrentInstructionListIndex = (WaverAnimationSelector)slot.VariableC;
            // Only the phase word's low byte is ever read.
            waver.Angle = SnesAngle.FromTableIndex(SnesLittleEndian.LowByte(slot.VariableD));
            waver.SpinFinished = slot.VariableE != 0;
            waver.RequestedInstructionListIndex = (WaverAnimationSelector)slot.VariableF;
        }
        if (slot.EntityState<FliesEntity, FlyEnemyState>() is { } fly)
        {
            fly.RetreatTimer = NativeCountdown.FromRaw(slot.VariableA);
            fly.Velocity = new(new SnesSignedEightEight(slot.VariableB), new SnesSignedEightEight(slot.VariableC));
            fly.TargetHeight = new Position16<RoomPixels>(0, slot.VariableD);
            // The phase word is an even byte offset into the 256-entry table, nine bits wide.
            fly.Angle = slot.VariableE % 2 == 0
                ? SnesAngle.FromTableIndex(checked((byte)(Index(slot.VariableE, 510, "fly phase") / 2)))
                : throw new InvalidDataException($"Recorded fly phase ${slot.VariableE:X4} is odd.");
            fly.Function = slot.VariableF == 0 ? null : (FlyEnemyFunction)slot.VariableF;
        }
        if (enemies.DragonStates[index] is { } dragon)
        {
            // Only the direction word's sign bit, the facing, is ever read.
            dragon.FacingLeft = (slot.VariableA & 0x8000) != 0;
            // One record word serves the motion timer and the shot counter in turn; whichever phase is current reads it.
            dragon.FunctionTimer = NativeCountdown.FromRaw(slot.VariableD);
            dragon.AttackCounter = NativeCountdown.FromRaw(slot.VariableD);
            dragon.Function = slot.VariableF == 0 ? null : (DragonEnemyFunction)slot.VariableF;
        }
        if (enemies.ElevatorStates[index] is { } elevator)
            elevator.RestingHeight = new Position16<RoomPixels>(0, slot.VariableA);
        if (enemies.FuneNamiheStates[index] is { } funeNamihe)
        {
            funeNamihe.Function = slot.VariableB == 0 ? null : (FuneNamiheEnemyFunction)slot.VariableB;
            funeNamihe.YProximity = Index(slot.VariableC, byte.MaxValue, "Namihe wake distance");
            funeNamihe.VariantIndex = Index(slot.VariableD, 15, "Fune/Namihe variant");
            funeNamihe.CooldownTimer = slot.VariableE;
            funeNamihe.CooldownTime = Index(slot.VariableF, byte.MaxValue, "Fune cooldown time");
            (bool namihe, bool active, bool right) = FuneNamiheSelectorOracle.Decode(slot.VariableA);
            if (namihe != funeNamihe.IsNamihe)
                throw new InvalidDataException($"Recorded Fune/Namihe selector ${slot.VariableA:X4} names the other species.");
            funeNamihe.ShowsActiveAnimation = active;
            funeNamihe.FacingRight = right;
        }
        if (slot.EnemyDefinitionPointer is EnemyDefinitionId.GRipper or EnemyDefinitionId.Ripper or EnemyDefinitionId.Ripper2 &&
            slot.SpeciesState<RipperEnemyState>() is { } ripperVariant)
        {
            ripperVariant.XVelocity = SnesSignedSixteenSixteen.FromParts(unchecked((short)slot.VariableD), slot.VariableC);
            ripperVariant.SpeedTableByteOffset = slot.VariableE;
        }
        if (enemies.NorfairLavaJumpingEnemyStates[index] is { } lavaJumper)
        {
            lavaJumper.YVelocity = new SnesSignedEightEight(slot.VariableC);
            lavaJumper.SpawnPosition = new Position16<RoomPixels>(slot.VariableD, slot.VariableE);
            lavaJumper.Function = slot.VariableF == 0 ? null : (NorfairLavaJumpingEnemyFunction)slot.VariableF;
        }
        if (enemies.MetareeStates[index] is { } metaree)
        {
            metaree.BurrowTimer = NativeCountdown.FromRaw(slot.VariableA);
            metaree.Function = slot.VariableB == 0 ? null : (MetareeEnemyFunction)slot.VariableB;
            metaree.RequestedInstructionListIndex = (SkreeMetareeAnimationPhase)slot.VariableC;
            metaree.InstalledInstructionListIndex = (SkreeMetareeAnimationPhase)slot.VariableD;
            metaree.AttackReady = slot.VariableE != 0;
            metaree.YVelocity = Index(slot.VariableF, 2734, "Metaree dive speed");
        }
        if (enemies.SparkStates[index] is { } spark)
        {
            spark.Function = slot.VariableB == 0 ? null : (SparkEnemyFunction)slot.VariableB;
            spark.BaseFunctionTime = slot.VariableE;
            spark.FunctionTimer = NativeCountdown.FromRaw(slot.VariableF);
        }
        if (enemies.PuyoStates[index] is { } puyo)
        {
            puyo.YSpeedTableIndex = Index(slot.VariableA, short.MaxValue, "Puyo speed index");
            puyo.HopCooldownTimer = NativeCountdown.FromRaw(slot.VariableB);
            puyo.Function = slot.VariableC == 0 ? null : (PuyoEnemyFunction)slot.VariableC;
            puyo.AirborneFunction = slot.VariableD == 0 ? null : (PuyoAirborneFunction)slot.VariableD;
            puyo.HopRecord = slot.VariableE % PuyoHopDefinitions.RecordSize == 0
                ? Index(slot.VariableE, 6 * PuyoHopDefinitions.RecordSize, "Puyo hop record offset") / PuyoHopDefinitions.RecordSize
                : throw new InvalidDataException($"Recorded Puyo hop record offset ${slot.VariableE:X4} is not a record.");
        }
    }

    /// <summary>
    /// Splits a Shaktool segment's shared orientation word by the segment's role: the centre's heading bucket and
    /// reversal bit, or the final saw's curl progress; every other segment keeps it zero.
    /// </summary>
    /// <param name="segment">The segment's state.</param>
    /// <param name="segmentIndex">The segment's place in the chain, zero through six.</param>
    /// <param name="word">The recorded orientation word.</param>
    private static void ImportShaktoolOrientation(ShaktoolSegmentState segment, int segmentIndex, ushort word)
    {
        segment.ResetOrientation();
        switch (segmentIndex)
        {
            case 3:
                segment.OrientationReversed = (word & 0x8000) != 0;
                int bucket = word & 0x7fff;
                if ((bucket & 0x1f) != 0 || bucket > 0xe0)
                    throw new InvalidDataException($"Recorded Shaktool centre orientation ${word:X4} has no heading bucket.");
                segment.OrientationBucket = bucket;
                return;
            case 6:
                segment.CurlProgress = Index(word, 0xf3fb, "Shaktool curl progress");
                return;
            default:
                if (word != 0)
                    throw new InvalidDataException($"Recorded Shaktool segment {segmentIndex} orientation ${word:X4} is not zero.");
                return;
        }
    }

    /// <summary>Decodes a recorded enemy byte index into the slot it names.</summary>
    /// <param name="word">The recorded byte index, a multiple of the record size.</param>
    /// <param name="meaning">What the word holds, for the failure message.</param>
    private static int SlotIndex(ushort word, string meaning) =>
        word % RoomEnemySystem.NativeSlotSize == 0 && word / RoomEnemySystem.NativeSlotSize < RoomEnemySystem.MaximumEnemyCount
            ? word / RoomEnemySystem.NativeSlotSize
            : throw new InvalidDataException($"Recorded {meaning} ${word:X4} is not an enemy slot.");

    /// <summary>Decodes a dead Tourian corpse's recorded bank-$A9 main-AI function word into its phase.</summary>
    /// <param name="species">The corpse's species, whose wait, pre-rot and rotting routines are its own.</param>
    /// <param name="word">The recorded function word; zero is an emptied record.</param>
    private static DeadTourianCorpseFunction? DeadTourianCorpseFunctionOf(DeadTourianCorpseSpecies species, ushort word) =>
        (species, word) switch
        {
            (_, 0) => null,
            (_, 0xda63) => DeadTourianCorpseFunction.Finished,
            (DeadTourianCorpseSpecies.Zoomer, 0xda69) or (DeadTourianCorpseSpecies.Ripper, 0xda73) or (DeadTourianCorpseSpecies.Skree, 0xda6e) =>
                DeadTourianCorpseFunction.Waiting,
            (DeadTourianCorpseSpecies.Zoomer, 0xda94) or (DeadTourianCorpseSpecies.Ripper, 0xda99) or (DeadTourianCorpseSpecies.Skree, 0xda9e) =>
                DeadTourianCorpseFunction.PreRotDelay,
            (DeadTourianCorpseSpecies.Zoomer, 0xdad0) or (DeadTourianCorpseSpecies.Ripper, 0xdae6) or (DeadTourianCorpseSpecies.Skree, 0xdafc) =>
                DeadTourianCorpseFunction.Rotting,
            _ => throw new InvalidDataException($"Recorded dead {species} function ${word:X4} is not one of its routines."),
        };

    /// <summary>Decodes a recorded index word that the family's updates keep within 0 through <paramref name="maximum"/>.</summary>
    /// <param name="word">The recorded word.</param>
    /// <param name="maximum">The largest value the family produces.</param>
    /// <param name="meaning">What the word holds, for the failure message.</param>
    private static int Index(ushort word, int maximum, string meaning) =>
        word <= maximum ? word : throw new InvalidDataException($"Recorded {meaning} {word} exceeds {maximum}.");
}
