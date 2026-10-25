using SuperMetroid.Core.Bosses.Phantoon;
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
using SuperMetroid.Core.World;
namespace SuperMetroid.Verification.Enemies;

/// <summary>
/// Translates a native snapshot's enemy variable words into the typed state each enemy family owns. The generic
/// snapshot import writes every slot's words first; this codec then gives each family its state from them, rejecting
/// a word outside the range the family's own updates can produce.
/// </summary>
internal static class NativeEnemyFamilyStateImport
{
    /// <summary>Translates the recorded record words of the enemy in <paramref name="slot"/> into its family's state.</summary>
    /// <param name="enemies">The enemy system whose family states receive the values.</param>
    /// <param name="slot">The imported slot, its parameter words already written.</param>
    /// <param name="words">The enemy's six recorded record words.</param>
    internal static void Import(RoomEnemySystem enemies, RoomEnemySlot slot, NativeEnemyRecordWords words)
    {
        int index = slot.SlotIndex;
        if (slot.SpeciesState<PhantoonPartState>() is { } phantoonPartRecord)
        {
            phantoonPartRecord.PathCursor = words.A;
            phantoonPartRecord.SpeedFraction = words.B;
            phantoonPartRecord.SwoopVelocity = new(new SnesSignedEightEight(words.C), new SnesSignedEightEight(words.D));
            phantoonPartRecord.FunctionTimer = words.E;
            phantoonPartRecord.WavyPhaseDelta = words.F;
        }
        if (slot.SpeciesState<KraidPartState>() is { } kraidPartRecord)
        {
            kraidPartRecord.Function = (KraidAiFunction)words.A;
            kraidPartRecord.NailVelocity = FixedVector<RoomPixels>.FromWords(words.C, words.B, words.E, words.D);
            kraidPartRecord.FunctionTimer = words.F;
        }
        if (slot.SpeciesState<GunshipPartState>() is { } gunshipRecord)
        {
            gunshipRecord.FunctionTimer = words.A;
            gunshipRecord.NextLiftoffPage = words.B;
            gunshipRecord.BobPhase = words.C;
            gunshipRecord.BobTimer = words.D;
            gunshipRecord.BaseY = words.E;
            gunshipRecord.EscapeVelocity = words.F;
        }
        if (slot.SpeciesState<CeresSteamEnemyState>() is { } ceresSteamRecord)
        {
            ceresSteamRecord.Function = words.A == 0 ? null : (CeresSteamFunction)words.A;
            ceresSteamRecord.ActivationDelay = WrappingCountdown.FromRaw(words.D);
        }
        if (slot.SpeciesState<CeresDoorEnemyState>() is { } ceresDoorRecord)
        {
            ceresDoorRecord.Function = words.A == 0 ? null : (CeresDoorFunction)words.A;
            ceresDoorRecord.HandsDrawingToRidley = words.B != 0;
            ceresDoorRecord.RumbleCallsLeft = words.D;
            ceresDoorRecord.ExplosionIntervalLeft = words.E;
            ceresDoorRecord.ExplosionPositionIndex = words.F;
        }
        if (slot.SpeciesState<DeadTorizoEnemyState>() is { } deadTorizoRecord)
        {
            deadTorizoRecord.Function = words.A == 0 ? null : CorpsesNativeWords.DeadTorizoFunctions.Decode(words.A, "recorded DeadTorizoFunction");
            deadTorizoRecord.Velocity = new(new SnesSignedEightEight(words.B), new SnesSignedEightEight(words.C));
        }
        if (slot.SpeciesState<MotherBrainFallingTubeState>() is { } fallingTubeRecord)
        {
            fallingTubeRecord.Function = words.A == 0 ? null : (MotherBrainTubeFunction)words.A;
            fallingTubeRecord.FloorY = words.B;
            fallingTubeRecord.FallSpeed = new SnesSignedEightEight(words.C);
            fallingTubeRecord.SmokeDelay = WrappingCountdown.FromRaw(words.D);
            fallingTubeRecord.SmokePositionIndex = words.E;
        }
        if (slot.SpeciesState<RidleyExplosionFragmentState>() is { } ridleyFragmentRecord)
        {
            ridleyFragmentRecord.Velocity = new(new SnesSignedEightEight(words.B), new SnesSignedEightEight(words.C));
            ridleyFragmentRecord.Lifetime = WrappingCountdown.FromRaw(words.F);
        }
        if (slot.SpeciesState<DraygonEnemyState>() is { } draygonRecord)
        {
            draygonRecord.Function = (DraygonAiFunction)words.A;
            draygonRecord.SwoopOffset = words.B;
            draygonRecord.SwoopEndOffset = words.C;
            draygonRecord.SwoopVelocity = SnesSixteenSixteen.Compose(words.D, words.E);
        }
        if (slot.SpeciesState<DraygonEyeState>() is { } draygonEyeRecord)
        {
            draygonEyeRecord.Function = words.A == 0 ? null : (DraygonEyeFunction)words.A;
            draygonEyeRecord.LookAngle = words.F;
        }
        if (slot.SpeciesState<CrocomireEnemyState>() is { } crocomireRecord)
        {
            crocomireRecord.DeathSequenceIndex = (CrocomireDeathPhase)words.A;
            crocomireRecord.FightFlags = CrocomireFightFlagBits.Flags(words.B);
            crocomireRecord.HitCount = CrocomireFightFlagBits.HitCount(words.B);
            crocomireRecord.FightFunction = (CrocomireFightFunction)words.C;
            crocomireRecord.RumbleTableOffset = words.D;
            crocomireRecord.VerticalSpeed = words.E;
            crocomireRecord.ProjectileCounter = words.F;
        }
        if (slot.SpeciesState<CrocomireTongueState>() is { } crocomireTongueRecord)
            crocomireTongueRecord.BodyXOffset = words.A;
        if (slot.SpeciesState<DeadSidehopperEnemyState>() is { } deadSidehopperRecord)
        {
            deadSidehopperRecord.Function = words.A == 0 ? null : CorpsesNativeWords.DeadSidehopperAiFunctions.Decode(words.A, "recorded DeadSidehopperAiFunction");
            deadSidehopperRecord.PreRotDelayCounter = words.B;
            deadSidehopperRecord.StateTimer = WrappingCountdown.FromRaw(words.F);
        }
        if (slot.SpeciesState<BotwoonEnemyState>() is { } botwoonRecord)
        {
            botwoonRecord.Function = words.D == 0 ? null : (BotwoonEnemyFunction)words.D;
            botwoonRecord.MovementFunction = words.E == 0 ? null : (BotwoonMovementFunction)words.E;
            botwoonRecord.HeadFunction = words.F == 0 ? null : (BotwoonHeadFunction)words.F;
        }
        if (slot.SpeciesState<ShitroidEnemyState>() is { } shitroidRecord)
        {
            shitroidRecord.Function = words.A == 0 ? null : (ShitroidAiFunction)words.A;
            shitroidRecord.Velocity = new(new SnesSignedEightEight(words.B), new SnesSignedEightEight(words.C));
            shitroidRecord.PalettePhase = SnesLittleEndian.LowByte(words.D);
            shitroidRecord.PaletteTimer = SnesLittleEndian.HighByte(words.D);
            shitroidRecord.PaletteDelay = words.E;
            shitroidRecord.StateTimer = WrappingCountdown.FromRaw(words.F);
        }
        if (slot.SpeciesState<DachoraEnemyState>() is { } dachoraRecord)
        {
            dachoraRecord.Speed = SnesSignedSixteenSixteen.FromParts(unchecked((short)words.A), words.B);
            dachoraRecord.VerticalAcceleration = SnesSignedSixteenSixteen.FromParts(unchecked((short)words.C), words.D);
            dachoraRecord.SetPaletteCycle(frame: SnesLittleEndian.HighByte(words.E), ticks: SnesLittleEndian.LowByte(words.E));
            dachoraRecord.Function = words.F == 0 ? null : AnimalsNativeWords.DachoraAiFunctions.Decode(words.F, "recorded DachoraAiFunction");
        }
        if (slot.SpeciesState<PipeBugEnemyState>() is { } pipeBugRecord)
        {
            ushort pipeBugFunction = pipeBugRecord.IsBrinstar ? words.F : words.A;
            pipeBugRecord.Function = pipeBugFunction == 0 ? null : PipeBugsNativeWords.PipeBugEnemyFunctions.Decode(pipeBugFunction, "recorded PipeBugEnemyFunction");
            pipeBugRecord.BrinstarFliesLeft = pipeBugRecord.IsBrinstar && EnemyMath.IsNegative16(words.A);
        }
        if (slot.SpeciesState<MagdolliteEnemyState>() is { } magdolliteRecord)
        {
            magdolliteRecord.Function = words.F == 0 ? null : MagdolliteNativeWords.MagdolliteEnemyFunctions.Decode(words.F, "recorded MagdolliteEnemyFunction");
            magdolliteRecord.BodyPhase = words.B / 2;
            magdolliteRecord.ThrowsRight = words.B != 0;
            magdolliteRecord.RiseDisplacement = new(0, unchecked((short)words.E));
        }
        if (slot.SpeciesState<SbugEnemyState>() is { } sbugRecord)
        {
            sbugRecord.StraightMovementTimer = WrappingCountdown.FromRaw(words.A);
            sbugRecord.Function = words.B == 0 ? null : SbugNativeWords.SbugEnemyFunctions.Decode(words.B, "recorded SbugEnemyFunction");
            sbugRecord.FaceCustomHeading(SnesAngle.FromTableIndex(SnesLittleEndian.LowByte(words.C)));
            sbugRecord.MovementTimer = WrappingCountdown.FromRaw(words.F);
        }
        if (slot.SpeciesState<KiHunterEnemyState>() is { } kiHunterRecord)
        {
            kiHunterRecord.Function = words.A == 0 ? null : KiHuntersNativeWords.KiHunterEnemyFunctions.Decode(words.A, "recorded KiHunterEnemyFunction");
            kiHunterRecord.SwoopTarget = new(words.B, 0);
            kiHunterRecord.DetachedSpeedIndex = words.B;
            kiHunterRecord.SwoopOrigin = new(0, words.C);
            kiHunterRecord.DetachedSpeedAccumulator = words.D;
            kiHunterRecord.AnimationChangeAngle = words.E;
            kiHunterRecord.SwoopAngle = words.F;
            kiHunterRecord.WingArcAngle = SnesAngle.FromRaw(words.F);
        }
        if (slot.SpeciesState<ZebetiteEnemyState>() is { } zebetiteRecord)
        {
            zebetiteRecord.Function = words.A == 0 ? null : (ZebetiteAiFunction)words.A;
            zebetiteRecord.Generation = words.D;
            zebetiteRecord.GenerationFlags = (ZebetiteGenerationFlags)words.F;
        }
        // Slot zero's word C is the Zebetites' shared palette counter whichever enemy occupies the slot.
        if (slot.SlotIndex == 0)
            enemies.ZebetiteBehavior.SharedPaletteCycle = words.C;
        if (slot.SpeciesState<ChozoStatueState>() is { } chozoStatueRecord)
        {
            chozoStatueRecord.CarryRecord = words.C / 2;
            chozoStatueRecord.PreInstruction = words.F == 0 ? null : ChozoNativeWords.ChozoStatuePreInstructions.Decode(words.F, "recorded ChozoStatuePreInstruction");
        }
        if (slot.SpeciesState<YardEnemyState>() is { } yardRecord)
        {
            yardRecord.CrawlingVelocity = new(new SnesSignedEightEight(words.A), new SnesSignedEightEight(words.B));
            yardRecord.AirborneFacingDirection = words.C switch
            {
                0 => YardAirborneFacing.Left,
                1 => YardAirborneFacing.Right,
                _ => throw new InvalidDataException($"Native Yard airborne facing {words.C:X4} is neither left nor right."),
            };
            yardRecord.ConsecutiveTurnCounter = words.E;
            yardRecord.MovementFunction = words.F == 0 ? null : YardNativeWords.YardMovementFunctions.Decode(words.F, "recorded YardMovementFunction");
        }
        if (slot.SpeciesState<ZoaEnemyState>() is { } zoaRecord)
        {
            zoaRecord.SpawnPosition = new(words.A, words.B);
            zoaRecord.InstructionListTableIndex = (ZoaAnimationSelector)words.C;
            zoaRecord.PreviousInstructionListTableIndex = (ZoaAnimationSelector)words.D;
            zoaRecord.Function = words.F == 0 ? null : ZoaNativeWords.ZoaEnemyFunctions.Decode(words.F, "recorded ZoaEnemyFunction");
        }
        if (slot.SpeciesState<BabyTurtleEnemyState>() is { } babyTurtleRecord)
        {
            babyTurtleRecord.Function = words.A == 0 ? null : MamaTurtlesNativeWords.BabyTurtleAiFunctions.Decode(words.A, "recorded BabyTurtleAiFunction");
            babyTurtleRecord.ParentSlotIndex = words.B % EnemySlotTable.EnemyIndexStride == 0
            ? words.B / EnemySlotTable.EnemyIndexStride
            : throw new InvalidDataException($"Baby Turtle parent offset {words.B} is not a slot boundary.");
            babyTurtleRecord.SpawnTopCenter = new(words.C, words.D);
            babyTurtleRecord.Velocity = new(unchecked((short)words.E), babyTurtleRecord.Velocity.Y);
        }
        if (slot.SpeciesState<OwtchEnemyState>() is { } owtchRecord)
        {
            // $A2:A423-$A435: fraction then whole for each direction, then the state and the sink depth.
            owtchRecord.RightVelocity = SnesSignedSixteenSixteen.FromParts(unchecked((short)words.B), words.A);
            owtchRecord.LeftVelocity = SnesSignedSixteenSixteen.FromParts(unchecked((short)words.D), words.C);
            owtchRecord.Behavior = (OwtchBehaviorState)words.E;
            owtchRecord.BurialDepth = words.F;
        }
        if (slot.SpeciesState<BullEnemyState>() is { } bullRecord)
        {
            bullRecord.Function = words.A == 0 ? null : BullNativeWords.BullEnemyFunctions.Decode(words.A, "recorded BullEnemyFunction");
            bullRecord.Acceleration = words.B;
            bullRecord.AccelerationDelta = words.C;
            bullRecord.Speed = words.D;
            bullRecord.ActivationTimer = WrappingCountdown.FromRaw(words.E);
            bullRecord.AccelerationIntervalTimer = WrappingCountdown.FromRaw(words.F);
        }
        if (slot.SpeciesState<BoyonEnemyState>() is { } boyonRecord)
        {
            boyonRecord.SpeedMultiplier = words.A;
            boyonRecord.Speed = words.B;
            boyonRecord.JumpHeight = words.C;
            boyonRecord.DistanceAccumulator = words.D;
            boyonRecord.SpeedTableIndex = words.E;
            boyonRecord.Bouncing = words.F != 0;
        }
        if (slot.SpeciesState<GrowingShutterEnemyState>() is { } growingShutterRecord)
        {
            growingShutterRecord.Function = words.A == 0 ? null : ShuttersNativeWords.GrowingShutterFunctions.Decode(words.A, "recorded GrowingShutterFunction");
            ushort[] originRows = [words.B, words.C, words.D, words.E];
            for (int level = 0; level < GrowingShutterEnemyState.SectionCount; level++)
                growingShutterRecord.SetSectionOrigin(level, new(0, originRows[level]));
            growingShutterRecord.GrowthLevel = words.F;
        }
        if (slot.SpeciesState<FirefleaEnemyState>() is { } firefleaRecord)
        {
            firefleaRecord.AngularSpeed = SnesSignedSixteenSixteen.FromParts(unchecked((short)words.B), words.A);
            firefleaRecord.Radius = words.C;
            firefleaRecord.Angle = SnesAngle.FromRaw(words.D);
            firefleaRecord.Center = new(words.E, words.F);
        }
        if (slot.SpeciesState<VerticalShutterEnemyState>() is { } verticalShutterRecord)
        {
            verticalShutterRecord.Function = words.A == 0 ? null : ShuttersNativeWords.VerticalShutterFunctions.Decode(words.A, "recorded VerticalShutterFunction");
            verticalShutterRecord.FunctionTimer = WrappingCountdown.FromRaw(words.B);
            verticalShutterRecord.DownVelocity = SnesSignedSixteenSixteen.FromParts(unchecked((short)words.D), words.C);
            verticalShutterRecord.UpVelocity = SnesSignedSixteenSixteen.FromParts(unchecked((short)words.F), words.E);
        }
        if (slot.SpeciesState<HorizontalShutterEnemyState>() is { } horizontalShutterRecord)
        {
            horizontalShutterRecord.Function = words.A == 0 ? null : ShuttersNativeWords.HorizontalShutterFunctions.Decode(words.A, "recorded HorizontalShutterFunction");
            horizontalShutterRecord.FunctionTimer = WrappingCountdown.FromRaw(words.B);
            horizontalShutterRecord.RightVelocity = SnesSignedSixteenSixteen.FromParts(unchecked((short)words.D), words.C);
            horizontalShutterRecord.LeftVelocity = SnesSignedSixteenSixteen.FromParts(unchecked((short)words.F), words.E);
        }
        if (slot.SpeciesState<CacatacEnemyState>() is { } cacatacRecord)
        {
            cacatacRecord.RightVelocity = cacatacRecord.RightVelocity.WithWholePixels(unchecked((short)words.A));
            cacatacRecord.LeftVelocity = SnesSignedSixteenSixteen.FromParts(unchecked((short)words.C), words.B);
            cacatacRecord.Direction = (CacatacDirection)words.D;
            cacatacRecord.Function = words.E == 0 ? null : CacatacNativeWords.CacatacEnemyFunctions.Decode(words.E, "recorded CacatacEnemyFunction");
        }
        if (slot.SpeciesState<WorkRobotEnemyState>() is { } workRobotRecord)
        {
            workRobotRecord.LaserVelocity = new(new SnesSignedEightEight(words.A), default);
            workRobotRecord.LaserCooldown = WrappingCountdown.FromRaw(words.B);
            workRobotRecord.BackupPosition = new(words.C, words.D);
            workRobotRecord.FallVelocity = SnesSignedSixteenSixteen.FromParts(unchecked((short)words.F), words.E);
        }
        if (slot.SpeciesState<KzanEnemyState>() is { } kzanRecord)
        {
            kzanRecord.Function = words.A == 0 ? null : KzanNativeWords.KzanEnemyFunctions.Decode(words.A, "recorded KzanEnemyFunction");
            kzanRecord.FallWaitTimer = WrappingCountdown.FromRaw(words.B);
            kzanRecord.RisingTarget = new(0, words.C);
            kzanRecord.FallingTarget = new(0, words.D);
            kzanRecord.InitialFallingSpeed = SnesSignedSixteenSixteen.FromParts(unchecked((short)words.F), words.E);
        }
        if (slot.SpeciesState<MetroidEnemyState>() is { } metroidRecord)
        {
            metroidRecord.Velocity = FixedVector<RoomPixels>.FromWords(words.B, words.A, words.D, words.C);
            metroidRecord.EscapeTimer = WrappingCountdown.FromRaw(words.E);
            metroidRecord.Function = (MetroidAiFunction)words.F;
        }
        if (slot.SpeciesState<FakeKraidEnemyState>() is { } fakeKraidRecord)
        {
            fakeKraidRecord.WalkDelta = unchecked((short)words.B);
            fakeKraidRecord.FacingDelta = unchecked((short)words.C);
            fakeKraidRecord.WalkStepTimer = WrappingCountdown.FromRaw(words.D);
            fakeKraidRecord.SpitDecisionTimer = WrappingCountdown.FromRaw(words.E);
        }
        if (slot.SpeciesState<MaridiaLargeSnailEnemyState>() is { } maridiaLargeSnailRecord)
        {
            maridiaLargeSnailRecord.Function = words.A == 0 ? null : MaridiaSnailsNativeWords.MaridiaLargeSnailEnemyFunctions.Decode(words.A, "recorded MaridiaLargeSnailEnemyFunction");
            maridiaLargeSnailRecord.YSpeedTableIndex = unchecked((short)words.B);
            maridiaLargeSnailRecord.AttackCooldown = WrappingCountdown.FromRaw(words.C);
            maridiaLargeSnailRecord.MovingLeft = words.D != 0;
            maridiaLargeSnailRecord.RemainingBounces = WrappingCountdown.FromRaw(words.E);
            maridiaLargeSnailRecord.BounceFunction = words.F == 0 ? null : MaridiaSnailsNativeWords.MaridiaLargeSnailBounceFunctions.Decode(words.F, "recorded MaridiaLargeSnailBounceFunction");
        }
        if (slot.SpeciesState<WallSpacePirateEnemyState>() is { } wallSpacePirateRecord)
        {
            wallSpacePirateRecord.Function = words.A == 0 ? null : SpacePiratesNativeWords.WallSpacePirateFunctions.Decode(words.A, "recorded WallSpacePirateFunction");
            wallSpacePirateRecord.WallJumpDestination = new(words.B, 0);
            wallSpacePirateRecord.ClimbDirection = (WallSpacePirateClimbDirection)words.C;
            wallSpacePirateRecord.WallJumpArcCenter = new(words.D, words.E);
            wallSpacePirateRecord.WallJumpArcAngle = SnesAngle.FromTableIndex(checked((byte)words.F));
        }
        if (slot.SpeciesState<NinjaSpacePirateEnemyState>() is { } ninjaSpacePirateRecord)
        {
            ninjaSpacePirateRecord.Function = words.A == 0 ? null : SpacePiratesNativeWords.NinjaSpacePirateFunctions.Decode(words.A, "recorded NinjaSpacePirateFunction");
            ninjaSpacePirateRecord.CalculatedSpinJumpSpeed = words.B;
            ninjaSpacePirateRecord.PostsMidpoint = new(words.D, 0);
            ninjaSpacePirateRecord.LeftPost = new(words.E, 0);
            ninjaSpacePirateRecord.RightPost = new(words.F, 0);
        }
        if (slot.SpeciesState<SkreeEnemyState>() is { } skreeRecord)
        {
            skreeRecord.BurrowTimer = WrappingCountdown.FromRaw(words.A);
            skreeRecord.Function = words.B == 0 ? null : SkreeNativeWords.SkreeEnemyFunctions.Decode(words.B, "recorded SkreeEnemyFunction");
            skreeRecord.RequestedInstructionIndex = (SkreeMetareeAnimationPhase)words.C;
            skreeRecord.InstalledInstructionIndex = (SkreeMetareeAnimationPhase)words.D;
            skreeRecord.AttackReady = words.E != 0;
        }
        if (slot.SpeciesState<BoulderEnemyState>() is { } boulderRecord)
        {
            boulderRecord.Function = words.A == 0 ? null : BoulderNativeWords.BoulderAiFunctions.Decode(words.A, "recorded BoulderAiFunction");
            boulderRecord.TravelSpeedAccumulator = words.B;
            boulderRecord.BounceSpeedAccumulator = words.C;
            boulderRecord.BounceCounter = WrappingCountdown.FromRaw(words.D);
            boulderRecord.DirectionSelector = words.E;
        }
        if (slot.SpeciesState<RinkaEnemyState>() is { } rinkaRecord)
        {
            rinkaRecord.Function = words.A == 0 ? null : (RinkaEnemyFunction)words.A;
            rinkaRecord.Velocity = new(new SnesSignedEightEight(words.B), new SnesSignedEightEight(words.C));
            rinkaRecord.SpawnResourceToken = words.D;
            rinkaRecord.DelayTimer = WrappingCountdown.FromRaw(words.F);
        }
        if (slot.SpeciesState<EtecoonEnemyState>() is { } etecoonRecord)
        {
            etecoonRecord.Velocity = FixedVector<RoomPixels>.FromWords(words.C, words.D, words.A, words.B);
            etecoonRecord.FunctionTimer = WrappingCountdown.FromRaw(words.E);
            etecoonRecord.Function = words.F == 0 ? null : AnimalsNativeWords.EtecoonAiFunctions.Decode(words.F, "recorded EtecoonAiFunction");
        }
        if (slot.SpeciesState<MochtroidEnemyState>() is { } mochtroidRecord)
        {
            mochtroidRecord.Velocity = FixedVector<RoomPixels>.FromWords(words.B, words.A, words.D, words.C);
            mochtroidRecord.ShakeTimer = WrappingCountdown.FromRaw(words.E);
            mochtroidRecord.MovementMode = (MochtroidMovementMode)words.F;
        }
        if (slot.SpeciesState<MorphBallEyeEnemyState>() is { } morphBallEyeRecord)
        {
            morphBallEyeRecord.IsActivated = words.C != 0;
            morphBallEyeRecord.Angle = SnesAngle.FromTableIndex(unchecked((byte)words.D));
            morphBallEyeRecord.FunctionTimer = WrappingCountdown.FromRaw(words.E);
            morphBallEyeRecord.Function = words.F == 0 ? null : MorphBallEyeNativeWords.MorphBallEyeAiFunctions.Decode(words.F, "recorded MorphBallEyeAiFunction");
        }
        if (slot.SpeciesState<HopperEnemyState>() is { } hopperRecord)
        {
            hopperRecord.Function = words.A == 0 ? null : HoppersNativeWords.HopperEnemyFunctions.Decode(words.A, "recorded HopperEnemyFunction");
            hopperRecord.YSpeedTableIndex = words.B;
            hopperRecord.XVelocity = unchecked((short)words.C);
            hopperRecord.YSpeedTableIndexDelta = words.D;
        }
        if (enemies.PlatformStates[index] is { } platform)
        {
            platform.RestingHeight = new Position16<RoomPixels>(0, words.A);
            platform.RightSpeed = SnesSignedSixteenSixteen.FromParts(unchecked((short)words.C), words.B);
            platform.LeftSpeed = SnesSignedSixteenSixteen.FromParts(unchecked((short)words.E), words.D);
            platform.YSpeedTableIndex = Index(words.F, platform.MaximumYSpeedTableIndex, "platform acceleration index");
        }
        if (enemies.BeetomStates[index] is { } beetom)
        {
            beetom.YSpeedTableIndex = Index(words.B, short.MaxValue, "Beetom speed index");
            beetom.Function = words.C == 0 ? null : BeetomNativeWords.BeetomEnemyFunctions.Decode(words.C, "recorded BeetomEnemyFunction");
            beetom.FunctionTimer = WrappingCountdown.FromRaw(words.D);
            beetom.ButtonCounter = WrappingCountdown.FromRaw(words.E);
            beetom.PreviousController1Input = (SnesButton)words.F;
        }
        if (slot.DefinitionId == EnemyDefinitionId.Shaktool && enemies.ShaktoolSegments[index] is { } segment)
        {
            segment.TargetAngle = SnesAngle.FromRaw(words.A);
            segment.OrbitAngle = SnesAngle.FromRaw(words.B);
            segment.AngularVelocity = Index(words.C, 1020, "Shaktool angular velocity");
            ImportShaktoolOrientation(segment, slot.Spawn.Population.Parameter2 >> 1, words.D);
            segment.OwnerSlotIndex = SlotIndex(words.E, "Shaktool owner");
            segment.PreInstruction = words.F == 0 ? null : ShaktoolNativeWords.ShaktoolPreInstructions.Decode(words.F, "recorded ShaktoolPreInstruction");
        }
        if (enemies.ChootStates[index] is { } choot)
        {
            choot.Function = words.A == 0 ? null : ChootNativeWords.ChootEnemyFunctions.Decode(words.A, "recorded ChootEnemyFunction");
            // The ascent cursor is 8.8 and moves by whole records, so its signed high byte is the record index.
            choot.YSpeedTableIndex = unchecked((short)words.B) >> 8;
            choot.FallingPatternFrame = (words.C & 0xff) == 0
                ? words.C >> 8
                : throw new InvalidDataException($"Recorded Choot falling cursor ${words.C:X4} is not a whole frame.");
            choot.FallingPatternLoopCounter = WrappingCountdown.FromRaw(words.D);
            choot.FallingPath = words.E == 0 ? null : ChootNativeWords.ChootFallingPaths.Decode(words.E, "recorded Choot falling pattern");
            choot.FallingPatternYDistance = words.F;
        }
        if (enemies.StokeStates[index] is { } stoke)
        {
            stoke.RightVelocity = SnesSignedSixteenSixteen.FromParts(unchecked((short)words.B), words.A);
            stoke.LeftVelocity = SnesSignedSixteenSixteen.FromParts(unchecked((short)words.D), words.C);
            stoke.Direction = (StokeDirection)words.E;
            stoke.Function = words.F switch
            {
                0 => null,
                0x8a75 => StokeAiFunction.IdleDuringAttack,
                // The two moving routines must agree with the direction word, which alone carries the side.
                0x8a43 when stoke.Direction == StokeDirection.Left => StokeAiFunction.Moving,
                0x8a5c when stoke.Direction != StokeDirection.Left => StokeAiFunction.Moving,
                _ => throw new InvalidDataException(
                    $"Recorded Stoke function ${words.F:X4} does not agree with direction word ${words.E:X4}."),
            };
        }
        if (enemies.SkulteraStates[index] is { } skultera)
        {
            skultera.Function = words.A == 0 ? null : SkulteraNativeWords.SkulteraEnemyFunctions.Decode(words.A, "recorded SkulteraEnemyFunction");
            skultera.RightVelocity = SnesSignedSixteenSixteen.FromParts(unchecked((short)words.C), words.B);
            skultera.LeftVelocity = SnesSignedSixteenSixteen.FromParts(unchecked((short)words.E), words.D);
            // Only the angle word's low byte is ever read; movement discards its high byte.
            skultera.Angle = SnesAngle.FromTableIndex(SnesLittleEndian.LowByte(words.F));
        }
        if (slot.EntityState<KagoEntity, KagoEnemyState>() is { } kago)
        {
            kago.Function = words.A == 0 ? null : KagoNativeWords.KagoEnemyFunctions.Decode(words.A, "recorded KagoEnemyFunction");
            kago.UsesFastAnimation = words.B != 0;
            kago.DeathAnimationStarted = words.F != 0;
        }
        if (slot.DefinitionId == EnemyDefinitionId.Coven && slot.SpeciesState<WreckedShipGhostEnemyState>() is { } ghost)
        {
            ghost.Function = words.A == 0 ? null : WreckedShipNativeWords.WreckedShipGhostAiFunctions.Decode(words.A, "recorded WreckedShipGhostAiFunction");
            ghost.PhaseTimer = WrappingCountdown.FromRaw(words.B);
            ghost.FlickerTableOffset = words.C;
        }
        if (slot.EntityState<WalkingSpacePirateEntity, WalkingSpacePirateEnemyState>() is { } walkingPirate)
        {
            walkingPirate.Function = words.A == 0 ? null : SpacePiratesNativeWords.WalkingSpacePirateFunctions.Decode(words.A, "recorded WalkingSpacePirateFunction");
            walkingPirate.LeftPost = new(words.E, 0);
            walkingPirate.RightPost = new(words.F, 0);
        }
        if (slot.EntityState<MultiviolaEntity, MultiviolaEnemyState>() is { } multiviola)
            multiviola.Velocity = FixedVector<RoomPixels>.FromWords(words.C, words.D, words.E, words.F);
        if (slot.EntityState<CrawlerEntity, CrawlerEnemyState>() is { } crawler)
        {
            crawler.Velocity = new(new SnesSignedEightEight(words.A), new SnesSignedEightEight(words.B));
            crawler.Function = words.F == 0 ? null : CrawlersNativeWords.CrawlerEnemyFunctions.Decode(words.F, "recorded CrawlerEnemyFunction");
        }
        if (slot.DefinitionId == EnemyDefinitionId.Holtz && slot.SpeciesState<LowerNorfairRioEnemyState>() is { } holtz)
        {
            holtz.Velocity = new(new SnesSignedEightEight(words.D), new SnesSignedEightEight(words.C));
            holtz.Function = words.F == 0 ? null : RiosNativeWords.LowerNorfairRioEnemyFunctions.Decode(words.F, "recorded LowerNorfairRioEnemyFunction");
            holtz.IsFollower = (slot.Spawn.Population.Parameter1 & EnemyParameterMasks.FollowerBit) != 0;
        }
        if (slot.DefinitionId == EnemyDefinitionId.EtecoonEscape && slot.SpeciesState<EscapeEtecoonEnemyState>() is { } escape)
        {
            escape.Velocity = new EightEightVector<RoomPixels>(new SnesSignedEightEight(words.A), default);
            escape.PreInstruction = words.F == 0 ? null : AnimalsNativeWords.EscapeEtecoonPreInstructions.Decode(words.F, "recorded EscapeEtecoonPreInstruction");
        }
        if (slot.DefinitionId == EnemyDefinitionId.FaceBlock && slot.SpeciesState<BlueBrinstarFaceBlockEnemyState>() is { } faceBlock)
        {
            faceBlock.Activated = words.A != 0;
            faceBlock.LastHorizontalDelta = words.B;
        }
        if (slot.DefinitionId == EnemyDefinitionId.Puromi && slot.SpeciesState<NuclearWaffleEnemyState>() is { } waffle)
        {
            waffle.Function = words.A == 0 ? null : NuclearNativeWords.NuclearWaffleEnemyFunctions.Decode(words.A, "recorded NuclearWaffleEnemyFunction");
            waffle.WaitingTimer = WrappingCountdown.FromRaw(words.B);
        }
        if (slot.DefinitionId == EnemyDefinitionId.LavaRocks && slot.SpeciesState<PolypEnemyState>() is { } polyp)
        {
            polyp.Function = words.A == 0 ? null : PolypNativeWords.PolypEnemyFunctions.Decode(words.A, "recorded PolypEnemyFunction");
            polyp.CooldownTimer = WrappingCountdown.FromRaw(words.B);
        }
        if (slot.DefinitionId == EnemyDefinitionId.Powamp && slot.SpeciesState<PowampEnemyState>() is { } powamp)
        {
            // The halves reuse the same words for different values.
            if (powamp.IsBalloon)
            {
                powamp.BalloonSpawn = new(words.A, words.B);
                powamp.BalloonGrappleTravelDistance = words.D;
            }
            else
            {
                powamp.YVelocity = SnesSignedSixteenSixteen.FromParts(unchecked((short)words.A), words.B);
                powamp.WiggleIndex = Index(words.C, 11, "Powamp wiggle phase");
                powamp.WiggleTimer = WrappingCountdown.FromRaw(words.D);
                powamp.FunctionTimer = WrappingCountdown.FromRaw(words.E);
            }
            powamp.Function = words.F == 0 ? null : PowampNativeWords.PowampEnemyFunctions.Decode(words.F, "recorded PowampEnemyFunction");
        }
        if (slot.DefinitionId == EnemyDefinitionId.MamaTurtle && slot.SpeciesState<MamaTurtleEnemyState>() is { } mama)
        {
            mama.Function = words.A == 0 ? null : MamaTurtlesNativeWords.MamaTurtleAiFunctions.Decode(words.A, "recorded MamaTurtleAiFunction");
            // Only the horizontal velocity's whole pixels live in the record; its fraction and the vertical velocity do not.
            mama.Velocity = mama.Velocity.WithX(mama.Velocity.X.WithWholePixels(unchecked((short)words.E)));
            mama.AsleepFlag = WrappingCountdown.FromRaw(words.F);
        }
        if (enemies.HibashiStates[index] is { } hibashi)
        {
            hibashi.Function = words.A == 0 ? null : HibashiNativeWords.HibashiEnemyFunctions.Decode(words.A, "recorded HibashiEnemyFunction");
            hibashi.InactiveTimer = WrappingCountdown.FromRaw(words.B);
            hibashi.ActivityFinished = words.C != 0;
            hibashi.SpawnHeight = new Position16<RoomPixels>(0, words.D);
        }
        if (enemies.AtomicStates[index] is { } atomic)
        {
            atomic.VerticalMovement = words.A == 0 ? null : AtomicNativeWords.AtomicVerticalMovements.Decode(words.A, "recorded AtomicVerticalMovement");
            atomic.HorizontalMovement = words.B == 0 ? null : AtomicNativeWords.AtomicHorizontalMovements.Decode(words.B, "recorded AtomicHorizontalMovement");
        }
        if (enemies.AlcoonStates[index] is { } alcoon)
        {
            alcoon.Function = words.A == 0 ? null : AlcoonNativeWords.AlcoonEnemyFunctions.Decode(words.A, "recorded AlcoonEnemyFunction");
            alcoon.Velocity = FixedVector<RoomPixels>.FromWords(words.D, 0, words.B, words.C);
            alcoon.SpawnPosition = alcoon.SpawnPosition.WithY(words.F);
        }
        if (enemies.DeadTourianCorpses[index] is { } corpse)
        {
            corpse.Function = DeadTourianCorpseFunctionOf(corpse.Species, words.A);
            corpse.PreRotDelayCounter = words.B;
        }
        if (enemies.RioStates[index] is { } rio)
        {
            rio.SavedHorizontalVelocity = new SnesSignedEightEight(words.A);
            rio.Function = words.B == 0 ? null : RiosNativeWords.RioEnemyFunctions.Decode(words.B, "recorded RioEnemyFunction");
            rio.Velocity = new(new SnesSignedEightEight(words.D), new SnesSignedEightEight(words.C));
            rio.AnimationFinished = words.E != 0;
        }
        if (slot.EntityState<NorfairRioEntity, NorfairRioEnemyState>() is { } norfairRio)
        {
            norfairRio.Velocity = new(new SnesSignedEightEight(words.B), new SnesSignedEightEight(words.A));
            norfairRio.Function = words.F == 0 ? null : RiosNativeWords.NorfairRioEnemyFunctions.Decode(words.F, "recorded NorfairRioEnemyFunction");
        }
        if (enemies.WaverStates[index] is { } waver)
        {
            waver.XVelocity = SnesSignedSixteenSixteen.FromParts(unchecked((short)words.B), words.A);
            waver.CurrentInstructionListIndex = (WaverAnimationSelector)words.C;
            // Only the phase word's low byte is ever read.
            waver.Angle = SnesAngle.FromTableIndex(SnesLittleEndian.LowByte(words.D));
            waver.SpinFinished = words.E != 0;
            waver.RequestedInstructionListIndex = (WaverAnimationSelector)words.F;
        }
        if (slot.EntityState<FliesEntity, FlyEnemyState>() is { } fly)
        {
            fly.RetreatTimer = WrappingCountdown.FromRaw(words.A);
            fly.Velocity = new(new SnesSignedEightEight(words.B), new SnesSignedEightEight(words.C));
            fly.TargetHeight = new Position16<RoomPixels>(0, words.D);
            // The phase word is an even byte offset into the 256-entry table, nine bits wide.
            fly.Angle = words.E % 2 == 0
                ? SnesAngle.FromTableIndex(checked((byte)(Index(words.E, 510, "fly phase") / 2)))
                : throw new InvalidDataException($"Recorded fly phase ${words.E:X4} is odd.");
            fly.Function = words.F == 0 ? null : FliesNativeWords.FlyEnemyFunctions.Decode(words.F, "recorded FlyEnemyFunction");
        }
        if (enemies.DragonStates[index] is { } dragon)
        {
            // Only the direction word's sign bit, the facing, is ever read.
            dragon.FacingLeft = (words.A & 0x8000) != 0;
            // One record word serves the motion timer and the shot counter in turn; whichever phase is current reads it.
            dragon.FunctionTimer = WrappingCountdown.FromRaw(words.D);
            dragon.AttackCounter = WrappingCountdown.FromRaw(words.D);
            dragon.Function = words.F == 0 ? null : DragonNativeWords.DragonEnemyFunctions.Decode(words.F, "recorded DragonEnemyFunction");
        }
        if (enemies.ElevatorStates[index] is { } elevator)
            elevator.RestingHeight = new Position16<RoomPixels>(0, words.A);
        if (enemies.FuneNamiheStates[index] is { } funeNamihe)
        {
            funeNamihe.Function = words.B == 0 ? null : FuneNamiheNativeWords.FuneNamiheEnemyFunctions.Decode(words.B, "recorded FuneNamiheEnemyFunction");
            funeNamihe.YProximity = Index(words.C, byte.MaxValue, "Namihe wake distance");
            funeNamihe.VariantIndex = Index(words.D, 15, "Fune/Namihe variant");
            funeNamihe.CooldownTimer = words.E;
            funeNamihe.CooldownTime = Index(words.F, byte.MaxValue, "Fune cooldown time");
            (bool namihe, bool active, bool right) = FuneNamiheSelectorOracle.Decode(words.A);
            if (namihe != funeNamihe.IsNamihe)
                throw new InvalidDataException($"Recorded Fune/Namihe selector ${words.A:X4} names the other species.");
            funeNamihe.ShowsActiveAnimation = active;
            funeNamihe.FacingRight = right;
        }
        if (slot.DefinitionId is EnemyDefinitionId.GRipper or EnemyDefinitionId.Ripper or EnemyDefinitionId.Ripper2 &&
            slot.SpeciesState<RipperEnemyState>() is { } ripperVariant)
        {
            ripperVariant.XVelocity = SnesSignedSixteenSixteen.FromParts(unchecked((short)words.D), words.C);
            ripperVariant.SpeedTableByteOffset = words.E;
        }
        if (enemies.NorfairLavaJumpingEnemyStates[index] is { } lavaJumper)
        {
            lavaJumper.YVelocity = new SnesSignedEightEight(words.C);
            lavaJumper.SpawnPosition = new Position16<RoomPixels>(words.D, words.E);
            lavaJumper.Function = words.F == 0 ? null : LavaJumpersNativeWords.NorfairLavaJumpingEnemyFunctions.Decode(words.F, "recorded NorfairLavaJumpingEnemyFunction");
        }
        if (enemies.MetareeStates[index] is { } metaree)
        {
            metaree.BurrowTimer = WrappingCountdown.FromRaw(words.A);
            metaree.Function = words.B == 0 ? null : MetareeNativeWords.MetareeEnemyFunctions.Decode(words.B, "recorded MetareeEnemyFunction");
            metaree.RequestedInstructionListIndex = (SkreeMetareeAnimationPhase)words.C;
            metaree.InstalledInstructionListIndex = (SkreeMetareeAnimationPhase)words.D;
            metaree.AttackReady = words.E != 0;
            metaree.YVelocity = Index(words.F, 2734, "Metaree dive speed");
        }
        if (enemies.SparkStates[index] is { } spark)
        {
            spark.Function = words.B == 0 ? null : SparksNativeWords.SparkEnemyFunctions.Decode(words.B, "recorded SparkEnemyFunction");
            spark.BaseFunctionTime = words.E;
            spark.FunctionTimer = WrappingCountdown.FromRaw(words.F);
        }
        if (enemies.PuyoStates[index] is { } puyo)
        {
            puyo.YSpeedTableIndex = Index(words.A, short.MaxValue, "Puyo speed index");
            puyo.HopCooldownTimer = WrappingCountdown.FromRaw(words.B);
            puyo.Function = words.C == 0 ? null : PuyoNativeWords.PuyoEnemyFunctions.Decode(words.C, "recorded PuyoEnemyFunction");
            puyo.AirborneFunction = words.D == 0 ? null : PuyoNativeWords.PuyoAirborneFunctions.Decode(words.D, "recorded PuyoAirborneFunction");
            puyo.HopRecord = words.E % PuyoHopDefinitions.RecordSize == 0
                ? Index(words.E, 6 * PuyoHopDefinitions.RecordSize, "Puyo hop record offset") / PuyoHopDefinitions.RecordSize
                : throw new InvalidDataException($"Recorded Puyo hop record offset ${words.E:X4} is not a record.");
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
        word % EnemySlotTable.EnemyIndexStride == 0 && word / EnemySlotTable.EnemyIndexStride < EnemySlotTable.MaximumEnemyCount
            ? word / EnemySlotTable.EnemyIndexStride
            : throw new InvalidDataException($"Recorded {meaning} ${word:X4} is not an enemy slot.");

    /// <summary>Decodes a dead Tourian corpse's recorded bank-$A9 main-AI function word into its phase.</summary>
    /// <param name="species">The corpse's species, whose wait, pre-rot and rotting routines are its own.</param>
    /// <param name="word">The recorded function word; zero is an emptied record.</param>
    private static DeadTourianCorpseFunction? DeadTourianCorpseFunctionOf(DeadTourianCorpseSpecies species, ushort word) =>
        new DeadTourianCorpseFunctionOfCase(species, word) switch
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
