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
            phantoonBody.PhaseTimer = CoordinateAxis.AsSigned(words.E);
            if (words.F != 0)
                phantoonBody.Phase = PhantoonNativeWords.Phases.Decode(words.F, "recorded PhantoonPhase");
        }
        if (slot.SpeciesState<KraidPartState>() is { } kraidPartRecord)
        {
            kraidPartRecord.Phase = (KraidPhase)words.A;
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
                if (kraidBody.Phase == KraidPhase.GrowBreakCeilingPlatforms)
                    kraidBody.CeilingRock = words.F / sizeof(ushort);
                else
                    kraidBody.PhaseTimer = words.F;
            }
            else
            {
                kraidPartRecord.PhaseTimer = words.F;
            }
        }
        if (slot.SpeciesState<GunshipPartState>() is { } gunshipRecord)
        {
            gunshipRecord.PhaseTimer = unchecked((short)words.A);
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
            ceresSteamRecord.Action = words.A == 0 ? null : CeresNativeWords.SteamActions.Decode(words.A, "recorded CeresSteamAction");
            ceresSteamRecord.ActivationDelay = WrappingCountdown.Start(words.D);
        }
        if (slot.SpeciesState<CeresDoorEnemyState>() is { } ceresDoorRecord)
        {
            ceresDoorRecord.Action = words.A == 0 ? null : CeresNativeWords.DoorActions.Decode(words.A, "recorded CeresDoorAction");
            ceresDoorRecord.HandsDrawingToRidley = words.B != 0;
            ceresDoorRecord.RumbleCallsLeft = words.D;
            ceresDoorRecord.ExplosionIntervalLeft = words.E;
            ceresDoorRecord.ExplosionPositionIndex = words.F;
        }
        if (slot.SpeciesState<DeadTorizoEnemyState>() is { } deadTorizoRecord)
        {
            deadTorizoRecord.Phase = words.A == 0 ? null : CorpsesNativeWords.DeadTorizoPhases.Decode(words.A, "recorded DeadTorizoPhase");
            deadTorizoRecord.Velocity = new(new SignedEightEight(words.B), new SignedEightEight(words.C));
        }
        if (slot.SpeciesState<MotherBrainFallingTubeState>() is { } fallingTubeRecord)
        {
            fallingTubeRecord.Action = words.A == 0 ? null : MotherBrainNativeWords.MotherBrainTubeActions.Decode(words.A, "recorded MotherBrainTubeAction");
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
            draygonRecord.Phase = (DraygonPhase)words.A;
            switch (DraygonNativeWords.MeaningOfSharedValue(draygonRecord.Phase))
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
                    throw new InvalidOperationException($"Undefined Draygon shared value meaning for {draygonRecord.Phase}.");
            }
            // The swoop path's length (word C) is read with the path's heights from the cartridge's table by the snapshot
            // import, which holds the memory the table lies in.
            draygonRecord.SwoopVelocity = UnsignedSixteenSixteen.FromRaw(SixteenSixteenWords.Compose(words.D, words.E));
        }
        if (slot.SpeciesState<DraygonEyeState>() is { } draygonEyeRecord)
        {
            draygonEyeRecord.Action = words.A == 0 ? null : DraygonNativeWords.EyeActions.Decode(words.A, "recorded DraygonEyeAction");
            // The game compares only the look angle's low byte, the bearing it stored.
            draygonEyeRecord.LookAngle = new Bearing(words.F & byte.MaxValue);
        }
        if (slot.SpeciesState<CrocomireEnemyState>() is { } crocomireRecord)
        {
            crocomireRecord.DeathPhase = CrocomireNativeWords.DeathPhases.Decode(words.A, "recorded Crocomire death phase");
            crocomireRecord.DamageReactionPending = CrocomireNativeWords.DamageReactionPending(words.B);
            crocomireRecord.Phase = CrocomireNativeWords.Phases.Decode(words.C, "recorded Crocomire fight function");
            // The cartridge keeps several of the body's values in each of words D, E and F; the phase says which.
            CrocomireNativeWords.ImportSharedWords(crocomireRecord, words.D, words.E, words.F);
        }
        if (slot.SpeciesState<CrocomireTongueState>() is { } crocomireTongueRecord)
            crocomireTongueRecord.BodyXOffset = words.A;
        if (slot.SpeciesState<DeadSidehopperEnemyState>() is { } deadSidehopperRecord)
        {
            deadSidehopperRecord.Action = words.A == 0 ? null : CorpsesNativeWords.DeadSidehopperActions.Decode(words.A, "recorded DeadSidehopperAction");
            deadSidehopperRecord.PreRotDelayCounter = words.B;
            deadSidehopperRecord.StateTimer = unchecked((short)words.F);
        }
        if (slot.SpeciesState<BotwoonEnemyState>() is { } botwoonRecord)
        {
            botwoonRecord.Phase = words.D == 0 ? null : (BotwoonPhase)words.D;
            botwoonRecord.Movement = words.E == 0 ? null : (BotwoonMovement)words.E;
            botwoonRecord.HeadAction = words.F == 0 ? null : (BotwoonHeadAction)words.F;
        }
        if (slot.SpeciesState<ShitroidEnemyState>() is { } shitroidRecord)
        {
            shitroidRecord.Action = words.A == 0 ? null : MotherBrainNativeWords.ShitroidActions.Decode(words.A, "recorded ShitroidAction");
            shitroidRecord.Velocity = new(new SignedEightEight(words.B), new SignedEightEight(words.C));
            shitroidRecord.MembranePulsePhase = LittleEndianBytes.LowByte(words.D);
            shitroidRecord.PaletteTimer = LittleEndianBytes.HighByte(words.D);
            shitroidRecord.PaletteDelay = words.E;
            shitroidRecord.StateTimer = CoordinateAxis.AsSigned(words.F);
        }
        if (slot.SpeciesState<DachoraEnemyState>() is { } dachoraRecord)
        {
            dachoraRecord.SetGlowCycle(frame: LittleEndianBytes.HighByte(words.E), ticks: LittleEndianBytes.LowByte(words.E));
            dachoraRecord.Action = words.F == 0 ? null : AnimalsNativeWords.DachoraActions.Decode(words.F, "recorded DachoraAction");
            // The cartridge keeps the speed's whole pixels and the countdowns in variable A, and the acceleration's
            // fraction and an echo's lifetime in variable D; each is read here as the meaning the recorded phase gives it.
            if (dachoraRecord.Action == DachoraAction.Echo)
            {
                dachoraRecord.EchoPositionTimer = unchecked((short)words.A);
                dachoraRecord.EchoLifetime = unchecked((short)words.D);
            }
            else if (dachoraRecord.Action is DachoraAction.BlinkingBeforeRun or DachoraAction.ChargingShinespark)
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
            pipeBugRecord.Action = pipeBugFunction == 0 ? null : PipeBugsNativeWords.PipeBugActions.Decode(pipeBugFunction, "recorded PipeBugAction");
            pipeBugRecord.BrinstarFliesLeft = pipeBugRecord.IsBrinstar && CoordinateAxis.AsSigned(words.A) < 0;
        }
        if (slot.SpeciesState<MagdolliteEnemyState>() is { } magdolliteRecord)
        {
            magdolliteRecord.Action = words.F == 0 ? null : MagdolliteNativeWords.MagdolliteActions.Decode(words.F, "recorded MagdolliteAction");
            magdolliteRecord.BodyPhase = words.B / 2;
            magdolliteRecord.ThrowsRight = words.B != 0;
            magdolliteRecord.RiseDisplacement = new(0, unchecked((short)words.E));
        }
        if (slot.SpeciesState<SbugEnemyState>() is { } sbugRecord)
        {
            sbugRecord.StraightMovementTimer = WrappingCountdown.Start(words.A);
            sbugRecord.Action = words.B == 0 ? null : SbugNativeWords.SbugActions.Decode(words.B, "recorded SbugAction");
            sbugRecord.FaceCustomHeading(RotationAngle.FromSteps(LittleEndianBytes.LowByte(words.C)));
            sbugRecord.MovementTimer = unchecked((short)words.F);
        }
        if (slot.SpeciesState<KiHunterEnemyState>() is { } kiHunterRecord)
        {
            kiHunterRecord.Action = words.A == 0 ? null : KiHuntersNativeWords.KiHunterActions.Decode(words.A, "recorded KiHunterAction");
            kiHunterRecord.SwoopTarget = new(words.B, 0);
            kiHunterRecord.DetachedSpeedCurvePlace = QuadraticSpeedCurvePlace.FromTwoHundredFiftySixthsOfMark(words.B);
            kiHunterRecord.SwoopOrigin = new(0, words.C);
            kiHunterRecord.DetachedSpeedAccumulator = words.D;
            kiHunterRecord.AnimationChangeAngle = words.E;
            kiHunterRecord.SwoopAngle = words.F;
            kiHunterRecord.WingArcAngle = RotationAngle.FromRaw(words.F);
        }
        if (slot.SpeciesState<ZebetiteEnemyState>() is { } zebetiteRecord)
        {
            zebetiteRecord.Action = words.A == 0 ? null : MotherBrainNativeWords.ZebetiteActions.Decode(words.A, "recorded ZebetiteAction");
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
            yardRecord.Movement = words.F == 0 ? null : YardNativeWords.YardMovements.Decode(words.F, "recorded YardMovement");
        }
        if (slot.SpeciesState<ZoaEnemyState>() is { } zoaRecord)
        {
            zoaRecord.SpawnPosition = new(words.A, words.B);
            zoaRecord.RequestedAnimation = ZoaNativeWords.AnimationSelectorOf(words.C);
            zoaRecord.InstalledAnimation = ZoaNativeWords.AnimationSelectorOf(words.D);
            zoaRecord.Action = words.F == 0 ? null : ZoaNativeWords.ZoaActions.Decode(words.F, "recorded ZoaAction");
        }
        if (slot.SpeciesState<BabyTurtleEnemyState>() is { } babyTurtleRecord)
        {
            babyTurtleRecord.Action = words.A == 0 ? null : MamaTurtlesNativeWords.BabyTurtleActions.Decode(words.A, "recorded BabyTurtleAction");
            // Every Baby Turtle's parent is a Mama Turtle, who links her babies when she is placed.
            if (enemies.SlotTable.Slots[SlotIndex(words.B, "Baby Turtle parent offset")].DefinitionId != EnemyDefinitionId.MamaTurtle)
                throw new InvalidDataException($"Recorded Baby Turtle parent offset ${words.B:X4} does not name a Mama Turtle.");
            babyTurtleRecord.SpawnTopCenter = new(words.C, words.D);
            babyTurtleRecord.Velocity = new(unchecked((short)words.E), babyTurtleRecord.Velocity.Y);
        }
        if (slot.SpeciesState<OwtchEnemyState>() is { } owtchRecord)
        {
            // $A2:A423-$A435: fraction then whole for each direction, then the state and the sink depth.
            owtchRecord.Speed = NegatedPair(SignedSixteenSixteen.FromParts(unchecked((short)words.B), words.A),
                SignedSixteenSixteen.FromParts(unchecked((short)words.D), words.C), "Owtch speed");
            owtchRecord.Behavior = OwtchNativeWords.Behaviors.Decode(words.E, "recorded OwtchBehaviorState");
            owtchRecord.BurialDepth = words.F;
        }
        if (slot.SpeciesState<BullEnemyState>() is { } bullRecord)
        {
            bullRecord.Action = words.A == 0 ? null : BullNativeWords.BullActions.Decode(words.A, "recorded BullAction");
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
            growingShutterRecord.Action = words.A == 0 ? null : ShuttersNativeWords.GrowingShutterActions.Decode(words.A, "recorded GrowingShutterAction");
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
            verticalShutterRecord.Action = words.A == 0 ? null : ShuttersNativeWords.VerticalShutterActions.Decode(words.A, "recorded VerticalShutterAction");
            verticalShutterRecord.ActionTimer = WrappingCountdown.Start(words.B);
            verticalShutterRecord.DownVelocity = SignedSixteenSixteen.FromParts(unchecked((short)words.D), words.C);
            verticalShutterRecord.UpVelocity = SignedSixteenSixteen.FromParts(unchecked((short)words.F), words.E);
        }
        if (slot.SpeciesState<CacatacEnemyState>() is { } cacatacRecord)
        {
            cacatacRecord.RightVelocity = cacatacRecord.RightVelocity.WithWholePixels(unchecked((short)words.A));
            cacatacRecord.LeftVelocity = SignedSixteenSixteen.FromParts(unchecked((short)words.C), words.B);
            cacatacRecord.Direction = CacatacNativeWords.Directions.Decode(words.D, "recorded CacatacDirection");
            cacatacRecord.Action = words.E == 0 ? null : CacatacNativeWords.CacatacActions.Decode(words.E, "recorded CacatacAction");
        }
        if (slot.SpeciesState<WorkRobotEnemyState>() is { } workRobotRecord)
        {
            // The game keeps the robot's facing as its laser velocity's sign, two pixels either way.
            workRobotRecord.Facing = words.A switch
            {
                0xfe00 => HorizontalDirection.Left,
                0x0200 => HorizontalDirection.Right,
                _ => throw new InvalidDataException($"Recorded Work Robot laser velocity ${words.A:X4} is not two pixels either way."),
            };
            workRobotRecord.LaserCooldown = WrappingCountdown.Start(words.B);
            workRobotRecord.FallVelocity = SignedSixteenSixteen.FromParts(unchecked((short)words.F), words.E);
        }
        if (slot.SpeciesState<KzanEnemyState>() is { } kzanRecord)
        {
            kzanRecord.Action = words.A == 0 ? null : KzanNativeWords.KzanActions.Decode(words.A, "recorded KzanAction");
            kzanRecord.FallWaitTimer = WrappingCountdown.Start(words.B);
            kzanRecord.RisingTarget = new(0, words.C);
            kzanRecord.FallingTarget = new(0, words.D);
        }
        if (slot.SpeciesState<MetroidEnemyState>() is { } metroidRecord)
        {
            metroidRecord.Velocity = FixedVector<RoomPixels>.FromStoredParts(words.B, words.A, words.D, words.C);
            metroidRecord.EscapeTimer = unchecked((short)words.E);
            metroidRecord.Movement = MetroidsNativeWords.Movements.Decode(words.F, "recorded MetroidMovement");
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
            maridiaLargeSnailRecord.Action = words.A == 0 ? null : MaridiaSnailsNativeWords.MaridiaLargeSnailActions.Decode(words.A, "recorded MaridiaLargeSnailAction");
            maridiaLargeSnailRecord.YSpeedCurvePlace = QuadraticSpeedCurvePlace.FromTwoHundredFiftySixthsOfMark(unchecked((short)words.B));
            maridiaLargeSnailRecord.AttackCooldown = unchecked((short)words.C);
            maridiaLargeSnailRecord.MovingLeft = words.D != 0;
            maridiaLargeSnailRecord.RemainingBounces = unchecked((short)words.E);
            maridiaLargeSnailRecord.BouncePhase = words.F == 0 ? null : MaridiaSnailsNativeWords.MaridiaLargeSnailBouncePhases.Decode(words.F, "recorded MaridiaLargeSnailBouncePhase");
        }
        if (slot.SpeciesState<WallSpacePirateEnemyState>() is { } wallSpacePirateRecord)
        {
            wallSpacePirateRecord.Action = words.A == 0 ? null : SpacePiratesNativeWords.WallSpacePirateActions.Decode(words.A, "recorded WallSpacePirateAction");
            wallSpacePirateRecord.ClimbDirection = SpacePiratesNativeWords.WallClimbDirections.Decode(words.C, "recorded WallSpacePirateClimbDirection");
            wallSpacePirateRecord.WallJumpArcCenter = new(words.D, words.E);
            wallSpacePirateRecord.WallJumpArcAngle = RotationAngle.FromSteps(checked((byte)words.F));
        }
        if (slot.SpeciesState<NinjaSpacePirateEnemyState>() is { } ninjaSpacePirateRecord)
        {
            ninjaSpacePirateRecord.Action = words.A == 0 ? null : SpacePiratesNativeWords.NinjaSpacePirateActions.Decode(words.A, "recorded NinjaSpacePirateAction");
            ninjaSpacePirateRecord.PostsMidpoint = new(words.D, 0);
            ninjaSpacePirateRecord.LeftPost = new(words.E, 0);
            ninjaSpacePirateRecord.RightPost = new(words.F, 0);
        }
        if (slot.SpeciesState<SkreeEnemyState>() is { } skreeRecord)
        {
            skreeRecord.BurrowTimer = unchecked((short)words.A);
            skreeRecord.Action = words.B == 0 ? null : SkreeNativeWords.SkreeActions.Decode(words.B, "recorded SkreeAction");
            skreeRecord.RequestedAnimation = SkreeNativeWords.AnimationPhases.Decode(words.C, "recorded requested Skree animation phase");
            skreeRecord.InstalledAnimation = SkreeNativeWords.AnimationPhases.Decode(words.D, "recorded installed Skree animation phase");
            skreeRecord.AttackReady = words.E != 0;
        }
        if (slot.SpeciesState<BoulderEnemyState>() is { } boulderRecord)
        {
            boulderRecord.Action = words.A == 0 ? null : BoulderNativeWords.BoulderActions.Decode(words.A, "recorded BoulderAction");
            boulderRecord.TravelSpeedAccumulator = QuadraticSpeedCurvePlace.FromTwoHundredFiftySixthsOfMark(words.B);
            boulderRecord.BounceSpeedAccumulator = QuadraticSpeedCurvePlace.FromTwoHundredFiftySixthsOfMark(words.C);
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
            rinkaRecord.Action = words.A == 0 ? null : MotherBrainNativeWords.RinkaActions.Decode(words.A, "recorded RinkaAction");
            rinkaRecord.Velocity = new(new SignedEightEight(words.B), new SignedEightEight(words.C));
            rinkaRecord.HeldSpawnPoint = words.D == 0 ? null : MotherBrainNativeWords.RinkaSpawnPoints.Decode(words.D, "recorded RinkaSpawnPoint");
            rinkaRecord.DelayTimer = CoordinateAxis.AsSigned(words.F);
        }
        if (slot.SpeciesState<EtecoonEnemyState>() is { } etecoonRecord)
        {
            etecoonRecord.Velocity = FixedVector<RoomPixels>.FromStoredParts(words.C, words.D, words.A, words.B);
            etecoonRecord.ActionTimer = WrappingCountdown.Start(words.E);
            etecoonRecord.Action = words.F == 0 ? null : AnimalsNativeWords.EtecoonActions.Decode(words.F, "recorded EtecoonAction");
            // A dormant Etecoon's record holds its countdown below zero while it waits for Samus.
            etecoonRecord.AwaitingSamus = etecoonRecord.Action == EtecoonAction.WaitingForSamus && etecoonRecord.ActionTimer.IsNegative;
        }
        if (slot.SpeciesState<MochtroidEnemyState>() is { } mochtroidRecord)
        {
            mochtroidRecord.Velocity = FixedVector<RoomPixels>.FromStoredParts(words.B, words.A, words.D, words.C);
            mochtroidRecord.ShakeTimer = unchecked((short)words.E);
            mochtroidRecord.MovementMode = MetroidsNativeWords.MochtroidMovementModes.Decode(words.F, "recorded MochtroidMovementMode");
        }
        if (slot.SpeciesState<MorphBallEyeEnemyState>() is { } morphBallEyeRecord)
        {
            morphBallEyeRecord.IsActivated = words.C != 0;
            morphBallEyeRecord.Angle = RotationAngle.FromSteps(unchecked((byte)words.D));
            morphBallEyeRecord.ActionTimer = unchecked((short)words.E);
            morphBallEyeRecord.Action = words.F == 0 ? null : MorphBallEyeNativeWords.MorphBallEyeActions.Decode(words.F, "recorded MorphBallEyeAction");
        }
        if (slot.SpeciesState<HopperEnemyState>() is { } hopperRecord)
        {
            hopperRecord.Action = words.A == 0 ? null : HoppersNativeWords.HopperActions.Decode(words.A, "recorded HopperAction");
            hopperRecord.YSpeedCurvePlace = QuadraticSpeedCurvePlace.FromMarks(words.B);
            hopperRecord.XVelocity = unchecked((short)words.C);
            hopperRecord.YSpeedCurvePlaceDelta = words.D;
        }
        if (enemies.PlatformStates[index] is { } platform)
        {
            platform.RestingHeight = new Position16<RoomPixels>(0, words.A);
            platform.Speed = NegatedPair(SignedSixteenSixteen.FromParts(unchecked((short)words.C), words.B),
                SignedSixteenSixteen.FromParts(unchecked((short)words.E), words.D), "platform speed");
            platform.YSpeedCurvePlace = QuadraticSpeedCurvePlace.FromMarks(Index(words.F, platform.MaximumYSpeedCurvePlace.WholeMarks, "platform acceleration index"));
        }
        if (enemies.BeetomStates[index] is { } beetom)
        {
            beetom.YSpeedCurvePlace = QuadraticSpeedCurvePlace.FromMarks(Index(words.B, short.MaxValue, "Beetom speed index"));
            beetom.Action = words.C == 0 ? null : BeetomNativeWords.BeetomActions.Decode(words.C, "recorded BeetomAction");
            beetom.ActionTimer = unchecked((short)words.D);
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
            choot.Action = words.A == 0 ? null : ChootNativeWords.ChootActions.Decode(words.A, "recorded ChootAction");
            // The ascent cursor is 8.8 and moves by whole records, so its signed high byte is the record index.
            choot.YSpeedCurvePlace = QuadraticSpeedCurvePlace.FromMarks(unchecked((short)words.B) >> 8);
            choot.FallingPatternFrame = (words.C & 0xff) == 0
                ? words.C >> 8
                : throw new InvalidDataException($"Recorded Choot falling cursor ${words.C:X4} is not a whole frame.");
            choot.FallingPatternLoopCounter = unchecked((short)words.D);
            choot.FallingPath = words.E == 0 ? null : ChootNativeWords.ChootFallingPaths.Decode(words.E, "recorded Choot falling pattern");
            choot.FallingPatternYDistance = words.F;
        }
        if (enemies.SkulteraStates[index] is { } skultera)
        {
            skultera.Action = words.A == 0 ? null : SkulteraNativeWords.SkulteraActions.Decode(words.A, "recorded SkulteraAction");
            skultera.RightVelocity = SignedSixteenSixteen.FromParts(unchecked((short)words.C), words.B);
            skultera.LeftVelocity = SignedSixteenSixteen.FromParts(unchecked((short)words.E), words.D);
            // Only the angle word's low byte is ever read; movement discards its high byte.
            skultera.Angle = RotationAngle.FromSteps(LittleEndianBytes.LowByte(words.F));
        }
        if (slot.EntityState<KagoEntity, KagoEnemyState>() is { } kago)
        {
            kago.Action = words.A == 0 ? null : KagoNativeWords.KagoActions.Decode(words.A, "recorded KagoAction");
            kago.UsesFastAnimation = words.B != 0;
        }
        if (slot.DefinitionId == EnemyDefinitionId.Coven && slot.SpeciesState<WreckedShipGhostEnemyState>() is { } ghost)
        {
            ghost.Action = words.A == 0 ? null : WreckedShipNativeWords.WreckedShipGhostActions.Decode(words.A, "recorded WreckedShipGhostAction");
            ghost.PhaseTimer = unchecked((short)words.B);
            ghost.NextFlickerStep = WreckedShipGhostFlickerStepOf(words.C);
        }
        if (slot.EntityState<WalkingSpacePirateEntity, WalkingSpacePirateEnemyState>() is { } walkingPirate)
        {
            walkingPirate.Action = words.A == 0 ? null : SpacePiratesNativeWords.WalkingSpacePirateActions.Decode(words.A, "recorded WalkingSpacePirateAction");
            walkingPirate.LeftPost = new(words.E, 0);
            walkingPirate.RightPost = new(words.F, 0);
        }
        if (slot.EntityState<MultiviolaEntity, MultiviolaEnemyState>() is { } multiviola)
            multiviola.Velocity = FixedVector<RoomPixels>.FromStoredParts(words.C, words.D, words.E, words.F);
        if (slot.EntityState<CrawlerEntity, CrawlerEnemyState>() is { } crawler)
        {
            crawler.Velocity = new(new SignedEightEight(words.A), new SignedEightEight(words.B));
            crawler.Action = words.F == 0 ? null : CrawlersNativeWords.CrawlerActions.Decode(words.F, "recorded CrawlerAction");
        }
        if (slot.DefinitionId == EnemyDefinitionId.Holtz && slot.SpeciesState<LowerNorfairRioEnemyState>() is { } holtz)
        {
            holtz.Velocity = new(new SignedEightEight(words.D), new SignedEightEight(words.C));
            holtz.Action = words.F == 0 ? null : RiosNativeWords.LowerNorfairRioActions.Decode(words.F, "recorded LowerNorfairRioAction");
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
            waffle.Action = words.A == 0 ? null : NuclearNativeWords.NuclearWaffleActions.Decode(words.A, "recorded NuclearWaffleAction");
            waffle.WaitingTimer = unchecked((short)words.B);
        }
        if (slot.DefinitionId == EnemyDefinitionId.LavaRocks && slot.SpeciesState<PolypEnemyState>() is { } polyp)
        {
            polyp.Action = words.A == 0 ? null : PolypNativeWords.PolypActions.Decode(words.A, "recorded PolypAction");
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
                powamp.ActionTimer = unchecked((short)words.E);
            }
            if (powamp.IsBalloon)
            {
                if (words.F != PowampNativeWords.BalloonIdleWord)
                    throw new InvalidDataException($"Recorded Powamp balloon runs ${words.F:X4}, not the balloon's empty routine.");
                powamp.Action = null;
            }
            else
                powamp.Action = words.F == 0 ? null : PowampNativeWords.PowampActions.Decode(words.F, "recorded PowampAction");
        }
        if (slot.DefinitionId == EnemyDefinitionId.MamaTurtle && slot.SpeciesState<MamaTurtleEnemyState>() is { } mama)
        {
            mama.Action = words.A == 0 ? null : MamaTurtlesNativeWords.MamaTurtleActions.Decode(words.A, "recorded MamaTurtleAction");
            // Only the horizontal velocity's whole pixels live in the record; its fraction and the vertical velocity do not.
            mama.Velocity = mama.Velocity.WithX(mama.Velocity.X.WithWholePixels(unchecked((short)words.E)));
            // The game keeps a wake count of one that a baby's touch or shot clears.
            mama.Asleep = words.F switch
            {
                0 => false,
                1 => true,
                _ => throw new InvalidDataException($"Recorded Mama Turtle wake count {words.F} is neither zero nor one."),
            };
        }
        if (enemies.HibashiStates[index] is { } hibashi)
        {
            hibashi.Action = words.A == 0 ? null : HibashiNativeWords.HibashiActions.Decode(words.A, "recorded HibashiAction");
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
            alcoon.Action = words.A == 0 ? null : AlcoonNativeWords.AlcoonActions.Decode(words.A, "recorded AlcoonAction");
            alcoon.Velocity = FixedVector<RoomPixels>.FromStoredParts(words.D, 0, words.B, words.C);
            alcoon.SpawnPosition = alcoon.SpawnPosition.WithY(words.F);
        }
        if (enemies.DeadTourianCorpses[index] is { } corpse)
        {
            corpse.Phase = DeadTourianCorpsePhaseOf(corpse.Profile.Species, words.A);
            corpse.PreRotDelayCounter = words.B;
        }
        if (enemies.RioStates[index] is { } rio)
        {
            rio.SavedHorizontalVelocity = new SignedEightEight(words.A);
            rio.Action = words.B == 0 ? null : RiosNativeWords.RioActions.Decode(words.B, "recorded RioAction");
            rio.Velocity = new(new SignedEightEight(words.D), new SignedEightEight(words.C));
            rio.AnimationFinished = words.E != 0;
        }
        if (slot.EntityState<NorfairRioEntity, NorfairRioEnemyState>() is { } norfairRio)
        {
            norfairRio.Velocity = new(new SignedEightEight(words.B), new SignedEightEight(words.A));
            norfairRio.Action = words.F == 0 ? null : RiosNativeWords.NorfairRioActions.Decode(words.F, "recorded NorfairRioAction");
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
            fly.Action = words.F == 0 ? null : FliesNativeWords.FlyActions.Decode(words.F, "recorded FlyAction");
        }
        if (enemies.DragonStates[index] is { } dragon)
        {
            // Only the direction word's sign bit, the facing, is ever read.
            dragon.FacingLeft = (words.A & 0x8000) != 0;
            // One record word serves the motion timer and the shot counter in turn; whichever phase is current reads it.
            dragon.ActionTimer = unchecked((short)words.D);
            dragon.AttackCounter = unchecked((short)words.D);
            dragon.Action = words.F == 0 ? null : DragonNativeWords.DragonActions.Decode(words.F, "recorded DragonAction");
        }
        if (enemies.ElevatorStates[index] is { } elevator)
            elevator.RestingHeight = new Position16<RoomPixels>(0, words.A);
        if (enemies.FuneNamiheStates[index] is { } funeNamihe)
        {
            funeNamihe.Action = words.B == 0 ? null : FuneNamiheNativeWords.FuneNamiheActions.Decode(words.B, "recorded FuneNamiheAction");
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
            lavaJumper.Action = words.F == 0 ? null : LavaJumpersNativeWords.NorfairLavaJumpingEnemyActions.Decode(words.F, "recorded NorfairLavaJumpingEnemyAction");
        }
        if (enemies.MetareeStates[index] is { } metaree)
        {
            metaree.BurrowTimer = unchecked((short)words.A);
            metaree.Action = words.B == 0 ? null : MetareeNativeWords.MetareeActions.Decode(words.B, "recorded MetareeAction");
            metaree.RequestedAnimation = SkreeNativeWords.AnimationPhases.Decode(words.C, "recorded requested Metaree animation phase");
            metaree.InstalledAnimation = SkreeNativeWords.AnimationPhases.Decode(words.D, "recorded installed Metaree animation phase");
            metaree.AttackReady = words.E != 0;
            metaree.YVelocity = Index(words.F, 2734, "Metaree dive speed");
        }
        if (enemies.SparkStates[index] is { } spark)
        {
            spark.Action = words.B == 0 ? null : SparksNativeWords.SparkActions.Decode(words.B, "recorded SparkAction");
            spark.BaseActionTime = words.E;
            spark.ActionTimer = unchecked((short)words.F);
        }
        if (enemies.PuyoStates[index] is { } puyo)
        {
            puyo.YSpeedCurvePlace = QuadraticSpeedCurvePlace.FromTwoHundredFiftySixthsOfMark(Index(words.A, short.MaxValue, "Puyo speed index"));
            puyo.HopCooldownTimer = unchecked((short)words.B);
            puyo.Action = words.C == 0 ? null : PuyoNativeWords.PuyoActions.Decode(words.C, "recorded PuyoAction");
            puyo.AirborneMovement = words.D == 0 ? null : PuyoNativeWords.PuyoAirborneMovements.Decode(words.D, "recorded PuyoAirborneMovement");
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
    private static DeadTourianCorpsePhase? DeadTourianCorpsePhaseOf(DeadTourianCorpseSpecies species, ushort word) =>
        new DeadTourianCorpsePhaseOfCase(species, word) switch
        {
            (_, 0) => null,
            (_, 0xda63) => DeadTourianCorpsePhase.Finished,
            (DeadTourianCorpseSpecies.Zoomer, 0xda69) or (DeadTourianCorpseSpecies.Ripper, 0xda73) or (DeadTourianCorpseSpecies.Skree, 0xda6e) =>
                DeadTourianCorpsePhase.Waiting,
            (DeadTourianCorpseSpecies.Zoomer, 0xda94) or (DeadTourianCorpseSpecies.Ripper, 0xda99) or (DeadTourianCorpseSpecies.Skree, 0xda9e) =>
                DeadTourianCorpsePhase.PreRotDelay,
            (DeadTourianCorpseSpecies.Zoomer, 0xdad0) or (DeadTourianCorpseSpecies.Ripper, 0xdae6) or (DeadTourianCorpseSpecies.Skree, 0xdafc) =>
                DeadTourianCorpsePhase.Rotting,
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

    /// <summary>
    /// The speed a recorded pair holds: the game stores a speed beside its negation, and Core keeps the speed alone and
    /// negates it at use (#1804), so a pair whose second value is not the first's negation fails.
    /// </summary>
    private static SignedSixteenSixteen NegatedPair(SignedSixteenSixteen speed, SignedSixteenSixteen negation, string meaning) =>
        negation == -speed ? speed : throw new InvalidDataException($"Recorded {meaning} {speed} is stored beside {negation}, not its negation.");
}
