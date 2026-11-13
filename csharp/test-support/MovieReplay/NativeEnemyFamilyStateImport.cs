using SuperMetroid.Diagnostics.Input;
using SuperMetroid.AssetExtraction.Cartridge;
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
using SuperMetroid.Core.World;
namespace SuperMetroid.TestSupport.MovieReplay;

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
        // The cartridge gives each record word a meaning per part (#1476); each part's own state says which.
        if (slot.SpeciesState<PhantoonEyeState>() is { } phantoonEye)
        {
            phantoonEye.ClosedTimer = CoordinateAxis.AsSigned(words.A);
            phantoonEye.FigureEightReversed = words.C != 0;
            phantoonEye.DyingFadeCycles = words.C;
            phantoonEye.FadeDenominator = words.D;
            phantoonEye.FadeNumerator = words.E;
            phantoonEye.FadeComplete = words.F != 0;
            phantoonEye.RageWaveCount = words.F;
            PhantoonNativeWords.ImportDeathMosaic(phantoonEye, words.C);
        }
        if (slot.SpeciesState<PhantoonTentaclesState>() is { } phantoonTentacles)
        {
            phantoonTentacles.SwoopRequested = words.A != 0;
            phantoonTentacles.RoundDamage = words.B;
            phantoonTentacles.SwoopVelocity = new(new SignedEightEight(words.C), new SignedEightEight(words.D));
            PhantoonNativeWords.ImportSwoopTarget(phantoonTentacles, words.E);
            phantoonTentacles.ExplosionIndex = words.F;
        }
        if (slot.SpeciesState<PhantoonMouthState>() is { } phantoonMouth)
        {
            phantoonMouth.CasualFlamePattern = words.A;
            phantoonMouth.CasualFlameHold = CoordinateAxis.AsSigned(words.B);
            // The record's count reads below zero while no burst is under way, which the mouth holds as no count.
            short casualFlames = unchecked((short)words.C);
            phantoonMouth.CasualFlameCount = casualFlames < 0 ? null : casualFlames;
            phantoonMouth.WaveAmplitude = words.D;
            phantoonMouth.WaveAmplitudeShrinking = words.E != 0;
            phantoonMouth.WavyPhaseDelta = words.F;
        }
        if (slot.SpeciesState<PhantoonBodyState>() is { } phantoonBody)
        {
            phantoonBody.StartingFlameIndex = words.A;
            phantoonBody.PathCursor = words.A;
            phantoonBody.ExplosionPass = words.A;
            phantoonBody.StartingFlamesActive = words.B != 0;
            phantoonBody.FigureEightSpeed = SignedSixteenSixteen.FromParts(CoordinateAxis.AsSigned(words.C), words.B);
            phantoonBody.FigureEightStage = PhantoonFigureEightStageWords.Decode(words.D);
            phantoonBody.FunctionTimer = CoordinateAxis.AsSigned(words.E);
            if (words.F != 0)
                phantoonBody.Function = PhantoonNativeWords.AiFunctions.Decode(words.F, "recorded PhantoonAiFunction");
        }
        if (slot.SpeciesState<KraidPartState>() is { } kraidPartRecord)
        {
            kraidPartRecord.Function = (KraidAiFunction)words.A;
            // Words B and C hold the body's head timer, a lint's travel and X offset, or a nail's horizontal velocity;
            // each part's own state takes its own.
            if (kraidPartRecord is KraidNailState kraidNail)
                kraidNail.NailVelocity = FixedVector<RoomPixels>.FromStoredParts(words.C, words.B, words.E, words.D);
            if (kraidPartRecord is KraidLintState kraidLint)
            {
                kraidLint.LintXOffset = CoordinateAxis.AsSigned(words.C);
                kraidLint.LintTravel = words.B;
            }
            // Word F is the ceiling rocks' byte cursor, two per rock, while Kraid's growth breaks the ceiling.
            if (kraidPartRecord is KraidBodyState kraidBody)
            {
                kraidBody.HeadTimer = words.C;
                if (kraidBody.Function == KraidAiFunction.GrowBreakCeilingPlatforms)
                    kraidBody.CeilingRock = words.F / sizeof(ushort);
                else
                    kraidBody.FunctionTimer = words.F;
            }
            else
            {
                kraidPartRecord.FunctionTimer = words.F;
            }
        }
        if (slot.SpeciesState<GunshipPartState>() is { } gunshipRecord)
        {
            gunshipRecord.FunctionTimer = unchecked((short)words.A);
            gunshipRecord.EngineReadyingElapsed = words.B;
            gunshipRecord.BobPhase = words.C;
            // The cartridge keeps the top hull's bob timer and the bottom hull's engine-sound timer in variable D, and
            // the base Y, the landing-brake step and the rumble frame in variable E; each part reads only its own.
            gunshipRecord.BobTimer = unchecked((short)words.D);
            gunshipRecord.EngineSoundTimer = unchecked((short)words.D);
            gunshipRecord.BaseY = words.E;
            gunshipRecord.LandingBrakeStep = words.E;
            gunshipRecord.RumbleFrame = words.E;
            gunshipRecord.EscapeVelocity = new SignedEightEight(words.F);
        }
        if (slot.SpeciesState<CeresSteamEnemyState>() is { } ceresSteamRecord)
        {
            ceresSteamRecord.Function = words.A == 0 ? null : CeresNativeWords.SteamFunctions.Decode(words.A, "recorded CeresSteamFunction");
            ceresSteamRecord.ActivationDelay = WrappingCountdown.Start(words.D);
        }
        if (slot.SpeciesState<CeresDoorEnemyState>() is { } ceresDoorRecord)
        {
            ceresDoorRecord.Function = words.A == 0 ? null : CeresNativeWords.DoorFunctions.Decode(words.A, "recorded CeresDoorFunction");
            ceresDoorRecord.HandsDrawingToRidley = words.B != 0;
            ceresDoorRecord.RumbleCallsLeft = words.D;
            ceresDoorRecord.ExplosionIntervalLeft = words.E;
            ceresDoorRecord.ExplosionPositionIndex = words.F;
        }
        if (slot.SpeciesState<DeadTorizoEnemyState>() is { } deadTorizoRecord)
        {
            deadTorizoRecord.Function = words.A == 0 ? null : CorpsesNativeWords.DeadTorizoFunctions.Decode(words.A, "recorded DeadTorizoFunction");
            deadTorizoRecord.Velocity = new(new SignedEightEight(words.B), new SignedEightEight(words.C));
        }
        if (slot.SpeciesState<MotherBrainFallingTubeState>() is { } fallingTubeRecord)
        {
            fallingTubeRecord.Function = words.A == 0 ? null : MotherBrainNativeWords.MotherBrainTubeFunctions.Decode(words.A, "recorded MotherBrainTubeFunction");
            fallingTubeRecord.FloorY = words.B;
            fallingTubeRecord.FallSpeed = new SignedEightEight(words.C);
            fallingTubeRecord.SmokeDelay = CoordinateAxis.AsSigned(words.D);
            fallingTubeRecord.SmokePositionIndex = words.E;
        }
        if (slot.SpeciesState<RidleyExplosionFragmentState>() is { } ridleyFragmentRecord)
        {
            ridleyFragmentRecord.Velocity = new(new SignedEightEight(words.B), new SignedEightEight(words.C));
            ridleyFragmentRecord.Lifetime = CoordinateAxis.AsSigned(words.F);
        }
        if (slot.SpeciesState<DraygonEnemyState>() is { } draygonRecord)
        {
            draygonRecord.Function = (DraygonAiFunction)words.A;
            switch (DraygonNativeWords.MeaningOfSharedValue(draygonRecord.Function))
            {
                case DraygonNativeWords.SharedValue.IntroFrames:
                    draygonRecord.IntroFrames = words.B;
                    break;
                case DraygonNativeWords.SharedValue.BurialWait:
                    draygonRecord.BurialWait = words.B;
                    break;
                case DraygonNativeWords.SharedValue.SwoopEntry:
                    draygonRecord.SwoopEntry = words.B / DraygonNativeWords.SwoopEntryBytes;
                    break;
                default:
                    throw new InvalidOperationException($"Undefined Draygon shared value meaning for {draygonRecord.Function}.");
            }
            // The swoop path's length (word C) is read with the path's heights from the cartridge's table by the snapshot
            // import, which holds the memory the table lies in.
            draygonRecord.SwoopVelocity = UnsignedSixteenSixteen.FromRaw(SixteenSixteenWords.Compose(words.D, words.E));
        }
        if (slot.SpeciesState<DraygonEyeState>() is { } draygonEyeRecord)
        {
            draygonEyeRecord.Function = words.A == 0 ? null : DraygonNativeWords.EyeFunctions.Decode(words.A, "recorded DraygonEyeFunction");
            // The game compares only the look angle's low byte, the bearing it stored.
            draygonEyeRecord.LookAngle = new Bearing(words.F & byte.MaxValue);
        }
        if (slot.SpeciesState<CrocomireEnemyState>() is { } crocomireRecord)
        {
            crocomireRecord.DeathPhase = CrocomireNativeWords.DeathPhases.Decode(words.A, "recorded Crocomire death phase");
            crocomireRecord.FightFlags = CrocomireNativeWords.FightFlags(words.B);
            crocomireRecord.FightFunction = CrocomireNativeWords.FightFunctions.Decode(words.C, "recorded Crocomire fight function");
            // The cartridge keeps several of the body's values in each of words D, E and F; the phase says which.
            CrocomireNativeWords.ImportSharedWords(crocomireRecord, words.D, words.E, words.F);
        }
        if (slot.SpeciesState<CrocomireTongueState>() is { } crocomireTongueRecord)
            crocomireTongueRecord.BodyXOffset = words.A;
        if (slot.SpeciesState<DeadSidehopperEnemyState>() is { } deadSidehopperRecord)
        {
            deadSidehopperRecord.Function = words.A == 0 ? null : CorpsesNativeWords.DeadSidehopperAiFunctions.Decode(words.A, "recorded DeadSidehopperAiFunction");
            deadSidehopperRecord.PreRotDelayCounter = words.B;
            deadSidehopperRecord.StateTimer = unchecked((short)words.F);
        }
        if (slot.SpeciesState<BotwoonEnemyState>() is { } botwoonRecord)
        {
            botwoonRecord.Function = words.D == 0 ? null : (BotwoonEnemyFunction)words.D;
            botwoonRecord.MovementFunction = words.E == 0 ? null : (BotwoonMovementFunction)words.E;
            botwoonRecord.HeadFunction = words.F == 0 ? null : (BotwoonHeadFunction)words.F;
        }
        if (slot.SpeciesState<ShitroidEnemyState>() is { } shitroidRecord)
        {
            shitroidRecord.Function = words.A == 0 ? null : MotherBrainNativeWords.ShitroidAiFunctions.Decode(words.A, "recorded ShitroidAiFunction");
            shitroidRecord.Velocity = new(new SignedEightEight(words.B), new SignedEightEight(words.C));
            shitroidRecord.MembranePulsePhase = LittleEndianBytes.LowByte(words.D);
            shitroidRecord.PaletteTimer = LittleEndianBytes.HighByte(words.D);
            shitroidRecord.PaletteDelay = words.E;
            shitroidRecord.StateTimer = CoordinateAxis.AsSigned(words.F);
        }
        if (slot.SpeciesState<DachoraEnemyState>() is { } dachoraRecord)
        {
            dachoraRecord.SetGlowCycle(frame: LittleEndianBytes.HighByte(words.E), ticks: LittleEndianBytes.LowByte(words.E));
            dachoraRecord.Function = words.F == 0 ? null : AnimalsNativeWords.DachoraAiFunctions.Decode(words.F, "recorded DachoraAiFunction");
            // The cartridge keeps the speed's whole pixels and the countdowns in variable A, and the acceleration's
            // fraction and an echo's lifetime in variable D; each is read here as the meaning the recorded phase gives it.
            if (dachoraRecord.Function == DachoraAiFunction.Echo)
            {
                dachoraRecord.EchoPositionTimer = unchecked((short)words.A);
                dachoraRecord.EchoLifetime = unchecked((short)words.D);
            }
            else if (dachoraRecord.Function is DachoraAiFunction.BlinkingBeforeRun or DachoraAiFunction.ChargingShinespark)
            {
                dachoraRecord.PhaseTimer = unchecked((short)words.A);
                dachoraRecord.Speed = SignedSixteenSixteen.FromParts(0, words.B);
                dachoraRecord.VerticalAcceleration = SignedSixteenSixteen.FromParts(unchecked((short)words.C), words.D);
            }
            else
            {
                dachoraRecord.Speed = SignedSixteenSixteen.FromParts(unchecked((short)words.A), words.B);
                dachoraRecord.VerticalAcceleration = SignedSixteenSixteen.FromParts(unchecked((short)words.C), words.D);
            }
        }
        if (slot.SpeciesState<PipeBugEnemyState>() is { } pipeBugRecord)
        {
            ushort pipeBugFunction = pipeBugRecord.IsBrinstar ? words.F : words.A;
            pipeBugRecord.Function = pipeBugFunction == 0 ? null : PipeBugsNativeWords.PipeBugEnemyFunctions.Decode(pipeBugFunction, "recorded PipeBugEnemyFunction");
            pipeBugRecord.BrinstarFliesLeft = pipeBugRecord.IsBrinstar && CoordinateAxis.AsSigned(words.A) < 0;
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
            sbugRecord.StraightMovementTimer = WrappingCountdown.Start(words.A);
            sbugRecord.Function = words.B == 0 ? null : SbugNativeWords.SbugEnemyFunctions.Decode(words.B, "recorded SbugEnemyFunction");
            sbugRecord.FaceCustomHeading(RotationAngle.FromSteps(LittleEndianBytes.LowByte(words.C)));
            sbugRecord.MovementTimer = unchecked((short)words.F);
        }
        if (slot.SpeciesState<KiHunterEnemyState>() is { } kiHunterRecord)
        {
            kiHunterRecord.Function = words.A == 0 ? null : KiHuntersNativeWords.KiHunterEnemyFunctions.Decode(words.A, "recorded KiHunterEnemyFunction");
            kiHunterRecord.SwoopTarget = new(words.B, 0);
            kiHunterRecord.DetachedSpeedIndex = EnemyQuadraticSpeedIndex.FromTwoHundredFiftySixthsOfRow(words.B);
            kiHunterRecord.SwoopOrigin = new(0, words.C);
            kiHunterRecord.DetachedSpeedAccumulator = words.D;
            kiHunterRecord.AnimationChangeAngle = words.E;
            kiHunterRecord.SwoopAngle = words.F;
            kiHunterRecord.WingArcAngle = RotationAngle.FromRaw(words.F);
        }
        if (slot.SpeciesState<ZebetiteEnemyState>() is { } zebetiteRecord)
        {
            zebetiteRecord.Function = words.A == 0 ? null : MotherBrainNativeWords.ZebetiteAiFunctions.Decode(words.A, "recorded ZebetiteAiFunction");
            zebetiteRecord.Generation = words.D;
            zebetiteRecord.GenerationFlags = MotherBrainNativeWords.DecodeGenerationFlags(words.F);
        }
        // The native snapshot keeps the Zebetites' shared pulse counter in slot zero's word C, whichever enemy occupies the slot.
        if (slot.SlotIndex == 0)
            enemies.ZebetiteBehavior.SharedPaletteCycle = words.C;
        if (slot.SpeciesState<ChozoStatueState>() is { } chozoStatueRecord)
        {
            chozoStatueRecord.FrameBehavior = words.F == 0 ? null : ChozoNativeWords.ChozoStatueFrameBehaviors.Decode(words.F, "recorded ChozoStatueFrameBehavior");
        }
        if (slot.SpeciesState<YardEnemyState>() is { } yardRecord)
        {
            yardRecord.CrawlingVelocity = new(new SignedEightEight(words.A), new SignedEightEight(words.B));
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
            zoaRecord.RequestedAnimation = ZoaNativeWords.AnimationSelectorOf(words.C);
            zoaRecord.InstalledAnimation = ZoaNativeWords.AnimationSelectorOf(words.D);
            zoaRecord.Function = words.F == 0 ? null : ZoaNativeWords.ZoaEnemyFunctions.Decode(words.F, "recorded ZoaEnemyFunction");
        }
        if (slot.SpeciesState<BabyTurtleEnemyState>() is { } babyTurtleRecord)
        {
            babyTurtleRecord.Function = words.A == 0 ? null : MamaTurtlesNativeWords.BabyTurtleAiFunctions.Decode(words.A, "recorded BabyTurtleAiFunction");
            // Every Baby Turtle's parent is the room's Mama Turtle, the first enemy her room lists.
            if (SlotIndex(words.B, "Baby Turtle parent offset") != 0)
                throw new InvalidDataException($"Recorded Baby Turtle parent offset ${words.B:X4} does not name the first enemy, where Mama Turtle is placed.");
            babyTurtleRecord.SpawnTopCenter = new(words.C, words.D);
            babyTurtleRecord.Velocity = new(unchecked((short)words.E), babyTurtleRecord.Velocity.Y);
        }
        if (slot.SpeciesState<OwtchEnemyState>() is { } owtchRecord)
        {
            // $A2:A423-$A435: fraction then whole for each direction, then the state and the sink depth.
            owtchRecord.RightVelocity = SignedSixteenSixteen.FromParts(unchecked((short)words.B), words.A);
            owtchRecord.LeftVelocity = SignedSixteenSixteen.FromParts(unchecked((short)words.D), words.C);
            owtchRecord.Behavior = OwtchNativeWords.Behaviors.Decode(words.E, "recorded OwtchBehaviorState");
            owtchRecord.BurialDepth = words.F;
        }
        if (slot.SpeciesState<BullEnemyState>() is { } bullRecord)
        {
            bullRecord.Function = words.A == 0 ? null : BullNativeWords.BullEnemyFunctions.Decode(words.A, "recorded BullEnemyFunction");
            bullRecord.Acceleration = new SignedEightEight(words.B);
            bullRecord.AccelerationDelta = new SignedEightEight(words.C);
            bullRecord.Speed = new SignedEightEight(words.D);
            bullRecord.ActivationTimer = unchecked((short)words.E);
            bullRecord.AccelerationIntervalTimer = unchecked((short)words.F);
        }
        if (slot.SpeciesState<BoyonEnemyState>() is { } boyonRecord)
        {
            boyonRecord.SpeedMultiplier = words.A;
            boyonRecord.Speed = words.B;
            boyonRecord.JumpHeight = SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(words.C);
            boyonRecord.ArcDistance = SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(words.D);
            // The rise leaves the curve index one below the top entry, the word's largest value, past the top.
            boyonRecord.CurvePlace = words.E == ushort.MaxValue ? BoyonCurvePlace.PastTheTop : BoyonCurvePlace.AtEntry(words.E);
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
            firefleaRecord.AngularSpeed = FirefleaBehavior.TurnPerUpdate(SignedSixteenSixteen.FromParts(unchecked((short)words.B), words.A));
            firefleaRecord.Radius = words.C;
            firefleaRecord.Angle = RotationAngle.FromRaw(words.D);
            firefleaRecord.Center = new(words.E, words.F);
        }
        if (slot.SpeciesState<VerticalShutterEnemyState>() is { } verticalShutterRecord)
        {
            verticalShutterRecord.Function = words.A == 0 ? null : ShuttersNativeWords.VerticalShutterFunctions.Decode(words.A, "recorded VerticalShutterFunction");
            verticalShutterRecord.FunctionTimer = WrappingCountdown.Start(words.B);
            verticalShutterRecord.DownVelocity = SignedSixteenSixteen.FromParts(unchecked((short)words.D), words.C);
            verticalShutterRecord.UpVelocity = SignedSixteenSixteen.FromParts(unchecked((short)words.F), words.E);
        }
        if (slot.SpeciesState<HorizontalShutterEnemyState>() is { } horizontalShutterRecord)
        {
            horizontalShutterRecord.Function = words.A == 0 ? null : ShuttersNativeWords.HorizontalShutterFunctions.Decode(words.A, "recorded HorizontalShutterFunction");
            horizontalShutterRecord.FunctionTimer = WrappingCountdown.Start(words.B);
            horizontalShutterRecord.RightVelocity = SignedSixteenSixteen.FromParts(unchecked((short)words.D), words.C);
            horizontalShutterRecord.LeftVelocity = SignedSixteenSixteen.FromParts(unchecked((short)words.F), words.E);
        }
        if (slot.SpeciesState<CacatacEnemyState>() is { } cacatacRecord)
        {
            cacatacRecord.RightVelocity = cacatacRecord.RightVelocity.WithWholePixels(unchecked((short)words.A));
            cacatacRecord.LeftVelocity = SignedSixteenSixteen.FromParts(unchecked((short)words.C), words.B);
            cacatacRecord.Direction = (CacatacDirection)words.D;
            cacatacRecord.Function = words.E == 0 ? null : CacatacNativeWords.CacatacEnemyFunctions.Decode(words.E, "recorded CacatacEnemyFunction");
        }
        if (slot.SpeciesState<WorkRobotEnemyState>() is { } workRobotRecord)
        {
            workRobotRecord.LaserVelocity = new(new SignedEightEight(words.A), default);
            workRobotRecord.LaserCooldown = WrappingCountdown.Start(words.B);
            workRobotRecord.BackupPosition = new(words.C, words.D);
            workRobotRecord.FallVelocity = SignedSixteenSixteen.FromParts(unchecked((short)words.F), words.E);
        }
        if (slot.SpeciesState<KzanEnemyState>() is { } kzanRecord)
        {
            kzanRecord.Function = words.A == 0 ? null : KzanNativeWords.KzanEnemyFunctions.Decode(words.A, "recorded KzanEnemyFunction");
            kzanRecord.FallWaitTimer = WrappingCountdown.Start(words.B);
            kzanRecord.RisingTarget = new(0, words.C);
            kzanRecord.FallingTarget = new(0, words.D);
        }
        if (slot.SpeciesState<MetroidEnemyState>() is { } metroidRecord)
        {
            metroidRecord.Velocity = FixedVector<RoomPixels>.FromStoredParts(words.B, words.A, words.D, words.C);
            metroidRecord.EscapeTimer = unchecked((short)words.E);
            metroidRecord.Function = (MetroidAiFunction)words.F;
        }
        if (slot.SpeciesState<FakeKraidEnemyState>() is { } fakeKraidRecord)
        {
            fakeKraidRecord.WalkDelta = unchecked((short)words.B);
            fakeKraidRecord.FacingDelta = unchecked((short)words.C);
            fakeKraidRecord.WalkStepTimer = words.D;
            fakeKraidRecord.SpitDecisionTimer = words.E;
        }
        if (slot.SpeciesState<MaridiaLargeSnailEnemyState>() is { } maridiaLargeSnailRecord)
        {
            maridiaLargeSnailRecord.Function = words.A == 0 ? null : MaridiaSnailsNativeWords.MaridiaLargeSnailEnemyFunctions.Decode(words.A, "recorded MaridiaLargeSnailEnemyFunction");
            maridiaLargeSnailRecord.YSpeedTableIndex = EnemyQuadraticSpeedIndex.FromTwoHundredFiftySixthsOfRow(unchecked((short)words.B));
            maridiaLargeSnailRecord.AttackCooldown = unchecked((short)words.C);
            maridiaLargeSnailRecord.MovingLeft = words.D != 0;
            maridiaLargeSnailRecord.RemainingBounces = unchecked((short)words.E);
            maridiaLargeSnailRecord.BounceFunction = words.F == 0 ? null : MaridiaSnailsNativeWords.MaridiaLargeSnailBounceFunctions.Decode(words.F, "recorded MaridiaLargeSnailBounceFunction");
        }
        if (slot.SpeciesState<WallSpacePirateEnemyState>() is { } wallSpacePirateRecord)
        {
            wallSpacePirateRecord.Function = words.A == 0 ? null : SpacePiratesNativeWords.WallSpacePirateFunctions.Decode(words.A, "recorded WallSpacePirateFunction");
            wallSpacePirateRecord.ClimbDirection = (WallSpacePirateClimbDirection)words.C;
            wallSpacePirateRecord.WallJumpArcCenter = new(words.D, words.E);
            wallSpacePirateRecord.WallJumpArcAngle = RotationAngle.FromSteps(checked((byte)words.F));
        }
        if (slot.SpeciesState<NinjaSpacePirateEnemyState>() is { } ninjaSpacePirateRecord)
        {
            ninjaSpacePirateRecord.Function = words.A == 0 ? null : SpacePiratesNativeWords.NinjaSpacePirateFunctions.Decode(words.A, "recorded NinjaSpacePirateFunction");
            ninjaSpacePirateRecord.PostsMidpoint = new(words.D, 0);
            ninjaSpacePirateRecord.LeftPost = new(words.E, 0);
            ninjaSpacePirateRecord.RightPost = new(words.F, 0);
        }
        if (slot.SpeciesState<SkreeEnemyState>() is { } skreeRecord)
        {
            skreeRecord.BurrowTimer = unchecked((short)words.A);
            skreeRecord.Function = words.B == 0 ? null : SkreeNativeWords.SkreeEnemyFunctions.Decode(words.B, "recorded SkreeEnemyFunction");
            skreeRecord.RequestedAnimation = (SkreeMetareeAnimationPhase)words.C;
            skreeRecord.InstalledAnimation = (SkreeMetareeAnimationPhase)words.D;
            skreeRecord.AttackReady = words.E != 0;
        }
        if (slot.SpeciesState<BoulderEnemyState>() is { } boulderRecord)
        {
            boulderRecord.Function = words.A == 0 ? null : BoulderNativeWords.BoulderAiFunctions.Decode(words.A, "recorded BoulderAiFunction");
            boulderRecord.TravelSpeedAccumulator = EnemyQuadraticSpeedIndex.FromTwoHundredFiftySixthsOfRow(words.B);
            boulderRecord.BounceSpeedAccumulator = EnemyQuadraticSpeedIndex.FromTwoHundredFiftySixthsOfRow(words.C);
            boulderRecord.BounceCounter = unchecked((short)words.D);
            boulderRecord.Roll = words.E switch
            {
                0 => BoulderRoll.Right,
                1 => BoulderRoll.Left,
                2 => BoulderRoll.LeftBreakingOnImpact,
                _ => throw new InvalidDataException($"Recorded Boulder roll {words.E} is not a roll."),
            };
        }
        if (slot.SpeciesState<RinkaEnemyState>() is { } rinkaRecord)
        {
            rinkaRecord.Function = words.A == 0 ? null : MotherBrainNativeWords.RinkaEnemyFunctions.Decode(words.A, "recorded RinkaEnemyFunction");
            rinkaRecord.Velocity = new(new SignedEightEight(words.B), new SignedEightEight(words.C));
            rinkaRecord.SpawnResourceToken = words.D;
            rinkaRecord.DelayTimer = CoordinateAxis.AsSigned(words.F);
        }
        if (slot.SpeciesState<EtecoonEnemyState>() is { } etecoonRecord)
        {
            etecoonRecord.Velocity = FixedVector<RoomPixels>.FromStoredParts(words.C, words.D, words.A, words.B);
            etecoonRecord.FunctionTimer = WrappingCountdown.Start(words.E);
            etecoonRecord.Function = words.F == 0 ? null : AnimalsNativeWords.EtecoonAiFunctions.Decode(words.F, "recorded EtecoonAiFunction");
            // A dormant Etecoon's record holds its countdown below zero while it waits for Samus.
            etecoonRecord.AwaitingSamus = etecoonRecord.Function == EtecoonAiFunction.WaitingForSamus && etecoonRecord.FunctionTimer.IsNegative;
        }
        if (slot.SpeciesState<MochtroidEnemyState>() is { } mochtroidRecord)
        {
            mochtroidRecord.Velocity = FixedVector<RoomPixels>.FromStoredParts(words.B, words.A, words.D, words.C);
            mochtroidRecord.ShakeTimer = unchecked((short)words.E);
            mochtroidRecord.MovementMode = (MochtroidMovementMode)words.F;
        }
        if (slot.SpeciesState<MorphBallEyeEnemyState>() is { } morphBallEyeRecord)
        {
            morphBallEyeRecord.IsActivated = words.C != 0;
            morphBallEyeRecord.Angle = RotationAngle.FromSteps(unchecked((byte)words.D));
            morphBallEyeRecord.FunctionTimer = unchecked((short)words.E);
            morphBallEyeRecord.Function = words.F == 0 ? null : MorphBallEyeNativeWords.MorphBallEyeAiFunctions.Decode(words.F, "recorded MorphBallEyeAiFunction");
        }
        if (slot.SpeciesState<HopperEnemyState>() is { } hopperRecord)
        {
            hopperRecord.Function = words.A == 0 ? null : HoppersNativeWords.HopperEnemyFunctions.Decode(words.A, "recorded HopperEnemyFunction");
            hopperRecord.YSpeedTableIndex = EnemyQuadraticSpeedIndex.FromRows(words.B);
            hopperRecord.XVelocity = unchecked((short)words.C);
            hopperRecord.YSpeedTableIndexDelta = words.D;
        }
        if (enemies.PlatformStates[index] is { } platform)
        {
            platform.RestingHeight = new Position16<RoomPixels>(0, words.A);
            platform.RightSpeed = SignedSixteenSixteen.FromParts(unchecked((short)words.C), words.B);
            platform.LeftSpeed = SignedSixteenSixteen.FromParts(unchecked((short)words.E), words.D);
            platform.YSpeedTableIndex = EnemyQuadraticSpeedIndex.FromRows(Index(words.F, platform.MaximumYSpeedTableIndex.WholeRows, "platform acceleration index"));
        }
        if (enemies.BeetomStates[index] is { } beetom)
        {
            beetom.YSpeedTableIndex = EnemyQuadraticSpeedIndex.FromRows(Index(words.B, short.MaxValue, "Beetom speed index"));
            beetom.Function = words.C == 0 ? null : BeetomNativeWords.BeetomEnemyFunctions.Decode(words.C, "recorded BeetomEnemyFunction");
            beetom.FunctionTimer = unchecked((short)words.D);
            beetom.ButtonCounter = unchecked((short)words.E);
            beetom.PreviousController1Input = ControllerWordCodec.FromJoypadWord(words.F, "native Beetom previous controller input");
        }
        if (slot.DefinitionId == EnemyDefinitionId.Shaktool && enemies.ShaktoolSegments[index] is { } segment)
        {
            segment.TargetAngle = RotationAngle.FromRaw(words.A);
            segment.OrbitAngle = RotationAngle.FromRaw(words.B);
            segment.AngularVelocity = RotationAngle.FromRaw(Index(words.C, 1020, "Shaktool angular velocity"));
            ImportShaktoolOrientation(segment, Array.IndexOf(ShaktoolSegmentDefinitions.Segments, slot.SpawnSettings<ShaktoolSpawnSettings>().Segment), words.D);
            // Every segment's owner is the chain's leading saw, the first enemy its room lists.
            if (SlotIndex(words.E, "Shaktool owner") != 0)
                throw new InvalidDataException($"Recorded Shaktool owner ${words.E:X4} does not name the first enemy, where the chain's saw is placed.");
            segment.FrameBehavior = words.F == 0 ? null : ShaktoolNativeWords.DecodeFrameBehavior(words.F, "recorded ShaktoolFrameBehavior");
        }
        if (enemies.ChootStates[index] is { } choot)
        {
            choot.Function = words.A == 0 ? null : ChootNativeWords.ChootEnemyFunctions.Decode(words.A, "recorded ChootEnemyFunction");
            // The ascent cursor is 8.8 and moves by whole records, so its signed high byte is the record index.
            choot.YSpeedTableIndex = EnemyQuadraticSpeedIndex.FromRows(unchecked((short)words.B) >> 8);
            choot.FallingPatternFrame = (words.C & 0xff) == 0
                ? words.C >> 8
                : throw new InvalidDataException($"Recorded Choot falling cursor ${words.C:X4} is not a whole frame.");
            choot.FallingPatternLoopCounter = unchecked((short)words.D);
            choot.FallingPath = words.E == 0 ? null : ChootNativeWords.ChootFallingPaths.Decode(words.E, "recorded Choot falling pattern");
            choot.FallingPatternYDistance = words.F;
        }
        if (enemies.StokeStates[index] is { } stoke)
        {
            stoke.RightVelocity = SignedSixteenSixteen.FromParts(unchecked((short)words.B), words.A);
            stoke.LeftVelocity = SignedSixteenSixteen.FromParts(unchecked((short)words.D), words.C);
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
            skultera.RightVelocity = SignedSixteenSixteen.FromParts(unchecked((short)words.C), words.B);
            skultera.LeftVelocity = SignedSixteenSixteen.FromParts(unchecked((short)words.E), words.D);
            // Only the angle word's low byte is ever read; movement discards its high byte.
            skultera.Angle = RotationAngle.FromSteps(LittleEndianBytes.LowByte(words.F));
        }
        if (slot.EntityState<KagoEntity, KagoEnemyState>() is { } kago)
        {
            kago.Function = words.A == 0 ? null : KagoNativeWords.KagoEnemyFunctions.Decode(words.A, "recorded KagoEnemyFunction");
            kago.UsesFastAnimation = words.B != 0;
        }
        if (slot.DefinitionId == EnemyDefinitionId.Coven && slot.SpeciesState<WreckedShipGhostEnemyState>() is { } ghost)
        {
            ghost.Function = words.A == 0 ? null : WreckedShipNativeWords.WreckedShipGhostAiFunctions.Decode(words.A, "recorded WreckedShipGhostAiFunction");
            ghost.PhaseTimer = unchecked((short)words.B);
            ghost.NextFlickerStep = WreckedShipGhostFlickerStepOf(words.C);
        }
        if (slot.EntityState<WalkingSpacePirateEntity, WalkingSpacePirateEnemyState>() is { } walkingPirate)
        {
            walkingPirate.Function = words.A == 0 ? null : SpacePiratesNativeWords.WalkingSpacePirateFunctions.Decode(words.A, "recorded WalkingSpacePirateFunction");
            walkingPirate.LeftPost = new(words.E, 0);
            walkingPirate.RightPost = new(words.F, 0);
        }
        if (slot.EntityState<MultiviolaEntity, MultiviolaEnemyState>() is { } multiviola)
            multiviola.Velocity = FixedVector<RoomPixels>.FromStoredParts(words.C, words.D, words.E, words.F);
        if (slot.EntityState<CrawlerEntity, CrawlerEnemyState>() is { } crawler)
        {
            crawler.Velocity = new(new SignedEightEight(words.A), new SignedEightEight(words.B));
            crawler.Function = words.F == 0 ? null : CrawlersNativeWords.CrawlerEnemyFunctions.Decode(words.F, "recorded CrawlerEnemyFunction");
        }
        if (slot.DefinitionId == EnemyDefinitionId.Holtz && slot.SpeciesState<LowerNorfairRioEnemyState>() is { } holtz)
        {
            holtz.Velocity = new(new SignedEightEight(words.D), new SignedEightEight(words.C));
            holtz.Function = words.F == 0 ? null : RiosNativeWords.LowerNorfairRioEnemyFunctions.Decode(words.F, "recorded LowerNorfairRioEnemyFunction");
            holtz.IsFollower = slot.SpawnSettings<EnemyFollowerSpawnSettings>().Role == EnemyFollowerRole.Follower;
        }
        if (slot.DefinitionId == EnemyDefinitionId.EtecoonEscape && slot.SpeciesState<EscapeEtecoonEnemyState>() is { } escape)
        {
            escape.Velocity = new EightEightVector<RoomPixels>(new SignedEightEight(words.A), default);
            escape.FrameBehavior = words.F == 0 ? null : AnimalsNativeWords.EscapeEtecoonFrameBehaviors.Decode(words.F, "recorded EscapeEtecoonFrameBehavior");
        }
        if (slot.DefinitionId == EnemyDefinitionId.FaceBlock && slot.SpeciesState<BlueBrinstarFaceBlockEnemyState>() is { } faceBlock)
        {
            faceBlock.Activated = words.A != 0;
        }
        if (slot.DefinitionId == EnemyDefinitionId.Puromi && slot.SpeciesState<NuclearWaffleEnemyState>() is { } waffle)
        {
            waffle.Function = words.A == 0 ? null : NuclearNativeWords.NuclearWaffleEnemyFunctions.Decode(words.A, "recorded NuclearWaffleEnemyFunction");
            waffle.WaitingTimer = unchecked((short)words.B);
        }
        if (slot.DefinitionId == EnemyDefinitionId.LavaRocks && slot.SpeciesState<PolypEnemyState>() is { } polyp)
        {
            polyp.Function = words.A == 0 ? null : PolypNativeWords.PolypEnemyFunctions.Decode(words.A, "recorded PolypEnemyFunction");
            polyp.CooldownTimer = unchecked((short)words.B);
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
                powamp.YVelocity = SignedSixteenSixteen.FromParts(unchecked((short)words.A), words.B);
                powamp.WiggleIndex = Index(words.C, 11, "Powamp wiggle phase");
                powamp.WiggleTimer = unchecked((short)words.D);
                powamp.FunctionTimer = unchecked((short)words.E);
            }
            powamp.Function = words.F == 0 ? null : PowampNativeWords.PowampEnemyFunctions.Decode(words.F, "recorded PowampEnemyFunction");
        }
        if (slot.DefinitionId == EnemyDefinitionId.MamaTurtle && slot.SpeciesState<MamaTurtleEnemyState>() is { } mama)
        {
            mama.Function = words.A == 0 ? null : MamaTurtlesNativeWords.MamaTurtleAiFunctions.Decode(words.A, "recorded MamaTurtleAiFunction");
            // Only the horizontal velocity's whole pixels live in the record; its fraction and the vertical velocity do not.
            mama.Velocity = mama.Velocity.WithX(mama.Velocity.X.WithWholePixels(unchecked((short)words.E)));
            mama.AsleepFlag = unchecked((short)words.F);
        }
        if (enemies.HibashiStates[index] is { } hibashi)
        {
            hibashi.Function = words.A == 0 ? null : HibashiNativeWords.HibashiEnemyFunctions.Decode(words.A, "recorded HibashiEnemyFunction");
            hibashi.InactiveTimer = unchecked((short)words.B);
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
            alcoon.Velocity = FixedVector<RoomPixels>.FromStoredParts(words.D, 0, words.B, words.C);
            alcoon.SpawnPosition = alcoon.SpawnPosition.WithY(words.F);
        }
        if (enemies.DeadTourianCorpses[index] is { } corpse)
        {
            corpse.Function = DeadTourianCorpseFunctionOf(corpse.Profile.Species, words.A);
            corpse.PreRotDelayCounter = words.B;
        }
        if (enemies.RioStates[index] is { } rio)
        {
            rio.SavedHorizontalVelocity = new SignedEightEight(words.A);
            rio.Function = words.B == 0 ? null : RiosNativeWords.RioEnemyFunctions.Decode(words.B, "recorded RioEnemyFunction");
            rio.Velocity = new(new SignedEightEight(words.D), new SignedEightEight(words.C));
            rio.AnimationFinished = words.E != 0;
        }
        if (slot.EntityState<NorfairRioEntity, NorfairRioEnemyState>() is { } norfairRio)
        {
            norfairRio.Velocity = new(new SignedEightEight(words.B), new SignedEightEight(words.A));
            norfairRio.Function = words.F == 0 ? null : RiosNativeWords.NorfairRioEnemyFunctions.Decode(words.F, "recorded NorfairRioEnemyFunction");
        }
        if (enemies.WaverStates[index] is { } waver)
        {
            waver.XVelocity = SignedSixteenSixteen.FromParts(unchecked((short)words.B), words.A);
            waver.InstalledAnimation = WaverNativeWords.Animations.Decode(words.C, "recorded Waver installed animation");
            // Only the phase word's low byte is ever read.
            waver.Angle = RotationAngle.FromSteps(LittleEndianBytes.LowByte(words.D));
            waver.SpinFinished = words.E != 0;
            waver.RequestedAnimation = WaverNativeWords.Animations.Decode(words.F, "recorded Waver requested animation");
        }
        if (slot.EntityState<FliesEntity, FlyEnemyState>() is { } fly)
        {
            fly.RetreatTimer = unchecked((short)words.A);
            fly.Velocity = new(new SignedEightEight(words.B), new SignedEightEight(words.C));
            fly.TargetHeight = new Position16<RoomPixels>(0, words.D);
            // The phase word is an even byte offset into the 256-entry table, nine bits wide.
            fly.Angle = words.E % 2 == 0
                ? RotationAngle.FromSteps(checked((byte)(Index(words.E, 510, "fly phase") / 2)))
                : throw new InvalidDataException($"Recorded fly phase ${words.E:X4} is odd.");
            fly.Function = words.F == 0 ? null : FliesNativeWords.FlyEnemyFunctions.Decode(words.F, "recorded FlyEnemyFunction");
        }
        if (enemies.DragonStates[index] is { } dragon)
        {
            // Only the direction word's sign bit, the facing, is ever read.
            dragon.FacingLeft = (words.A & 0x8000) != 0;
            // One record word serves the motion timer and the shot counter in turn; whichever phase is current reads it.
            dragon.FunctionTimer = unchecked((short)words.D);
            dragon.AttackCounter = unchecked((short)words.D);
            dragon.Function = words.F == 0 ? null : DragonNativeWords.DragonEnemyFunctions.Decode(words.F, "recorded DragonEnemyFunction");
        }
        if (enemies.ElevatorStates[index] is { } elevator)
            elevator.RestingHeight = new Position16<RoomPixels>(0, words.A);
        if (enemies.FuneNamiheStates[index] is { } funeNamihe)
        {
            funeNamihe.Function = words.B == 0 ? null : FuneNamiheNativeWords.FuneNamiheEnemyFunctions.Decode(words.B, "recorded FuneNamiheEnemyFunction");
            funeNamihe.YProximity = Index(words.C, byte.MaxValue, "Namihe wake distance");
            funeNamihe.Variant = Index(words.D, 15, "Fune/Namihe variant") == 0 ? FuneNamiheVariant.Fune : FuneNamiheVariant.Namihe;
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
            ripperVariant.XVelocity = SignedSixteenSixteen.FromParts(unchecked((short)words.D), words.C);
            ripperVariant.Speeds = SuperMetroid.AssetExtraction.Enemies.EnemySpeedTableCodec.LinearRowAtOffset(words.E);
        }
        if (enemies.NorfairLavaJumpingEnemyStates[index] is { } lavaJumper)
        {
            lavaJumper.YVelocity = new SignedEightEight(words.C);
            lavaJumper.SpawnPosition = new Position16<RoomPixels>(words.D, words.E);
            lavaJumper.Function = words.F == 0 ? null : LavaJumpersNativeWords.NorfairLavaJumpingEnemyFunctions.Decode(words.F, "recorded NorfairLavaJumpingEnemyFunction");
        }
        if (enemies.MetareeStates[index] is { } metaree)
        {
            metaree.BurrowTimer = unchecked((short)words.A);
            metaree.Function = words.B == 0 ? null : MetareeNativeWords.MetareeEnemyFunctions.Decode(words.B, "recorded MetareeEnemyFunction");
            metaree.RequestedAnimation = (SkreeMetareeAnimationPhase)words.C;
            metaree.InstalledAnimation = (SkreeMetareeAnimationPhase)words.D;
            metaree.AttackReady = words.E != 0;
            metaree.YVelocity = Index(words.F, 2734, "Metaree dive speed");
        }
        if (enemies.SparkStates[index] is { } spark)
        {
            spark.Function = words.B == 0 ? null : SparksNativeWords.SparkEnemyFunctions.Decode(words.B, "recorded SparkEnemyFunction");
            spark.BaseFunctionTime = words.E;
            spark.FunctionTimer = unchecked((short)words.F);
        }
        if (enemies.PuyoStates[index] is { } puyo)
        {
            puyo.YSpeedTableIndex = EnemyQuadraticSpeedIndex.FromTwoHundredFiftySixthsOfRow(Index(words.A, short.MaxValue, "Puyo speed index"));
            puyo.HopCooldownTimer = unchecked((short)words.B);
            puyo.Function = words.C == 0 ? null : PuyoNativeWords.PuyoEnemyFunctions.Decode(words.C, "recorded PuyoEnemyFunction");
            puyo.AirborneFunction = words.D == 0 ? null : PuyoNativeWords.PuyoAirborneFunctions.Decode(words.D, "recorded PuyoAirborneFunction");
            puyo.CurrentHop = PuyoNativeWords.HopRecordOffsets.Decode(words.E, "recorded Puyo hop record offset");
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
                // The recorded bucket is the bearing of the direction the head aims, a whole number of eighths of a turn.
                segment.Orientation = new Bearing(bucket).DirectionOfEighth();
                return;
            case 6:
                segment.CurlProgress = RotationAngle.FromRaw(Index(word, 0xf3fb, "Shaktool curl progress"));
                return;
            default:
                if (word != 0)
                    throw new InvalidDataException($"Recorded Shaktool segment {segmentIndex} orientation ${word:X4} is not zero.");
                return;
        }
    }

    /// <summary>Decodes a recorded enemy byte offset into the slot it names.</summary>
    /// <param name="word">The recorded byte offset, the start of an enemy record.</param>
    /// <param name="meaning">What the word holds, for the failure message.</param>
    private static int SlotIndex(ushort word, string meaning) =>
        SuperMetroid.AssetExtraction.Enemies.EnemySlotOffsets.IsSlotOffset(word)
            ? SuperMetroid.AssetExtraction.Enemies.EnemySlotOffsets.SlotOf(word)
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

    /// <summary>
    /// Decodes a Wrecked Ship ghost's recorded flicker-table byte offset into its next flicker step. The steps are the
    /// table's entries after its first, two bytes each, so offset two is the first step; the terminator's offset and zero,
    /// which the game writes when the flicker ends, are the position past the last step.
    /// </summary>
    /// <param name="word">The recorded byte offset.</param>
    private static int WreckedShipGhostFlickerStepOf(ushort word)
    {
        int count = WreckedShipGhostAppearanceDefinitions.FlickerSteps.Count;
        if (word == 0)
            return count;
        if (word % 2 != 0 || word / 2 - 1 > count)
            throw new InvalidDataException($"Recorded Wrecked Ship ghost flicker offset {word} is not a flicker step.");
        return word / 2 - 1;
    }

    /// <summary>Decodes a recorded index word that the family's updates keep within 0 through <paramref name="maximum"/>.</summary>
    /// <param name="word">The recorded word.</param>
    /// <param name="maximum">The largest value the family produces.</param>
    /// <param name="meaning">What the word holds, for the failure message.</param>
    private static int Index(ushort word, int maximum, string meaning) =>
        word <= maximum ? word : throw new InvalidDataException($"Recorded {meaning} {word} exceeds {maximum}.");
}
