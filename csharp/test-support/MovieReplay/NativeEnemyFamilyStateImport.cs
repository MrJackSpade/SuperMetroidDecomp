using SuperMetroid.TestSupport.SamusNativeCodecs;
using SuperMetroid.Core.Animation;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
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
    /// <summary>Translates the recorded record words of the enemy in <paramref name="enemy"/> into its family's state.</summary>
    /// <param name="enemies">The enemy system whose family states receive the values.</param>
    /// <param name="enemy">The imported slot, its parameter words already written.</param>
    /// <param name="words">The enemy's six recorded record words.</param>
    internal static void Import(RoomEnemySystem enemies, RoomEnemyPlace enemy, NativeEnemyRecordWords words)
    {
        int index = enemy.Ordinal;
        // The cartridge gives each of Phantoon's parts' record words a meaning per part (#1476); each part's words belong to
        // the encounter's state grouped by what it does (#1933).
        if (enemy.SpeciesState<PhantoonEnemyState>() is { } phantoon)
        {
            EnemyDefinitionId part = enemy.DefinitionId;
            if (part == EnemyDefinitionId.PhantoonEye)
            {
                phantoon.Round.EyeClosedTimer = CoordinateAxis.AsSigned(words.A);
                phantoon.FigureEight.Reversed = words.C != 0;
                phantoon.Death.FadeCycles = words.C;
                phantoon.BodyFade.Denominator = words.D;
                phantoon.BodyFade.Numerator = words.E;
                phantoon.BodyFade.Complete = words.F != 0;
                phantoon.RageWaveCount = words.F;
                PhantoonNativeWords.ImportDeathMosaic(phantoon.Death, words.C);
            }
            else if (part == EnemyDefinitionId.PhantoonTentacles)
            {
                phantoon.Swoop.Requested = words.A != 0;
                phantoon.Round.Damage = words.B;
                phantoon.Swoop.Velocity = new(new SignedEightEight(words.C), new SignedEightEight(words.D));
                PhantoonNativeWords.ImportSwoopTarget(phantoon.Swoop, words.E);
                phantoon.Death.NextExplosion = words.F;
            }
            else if (part == EnemyDefinitionId.PhantoonMouth)
            {
                phantoon.CasualFlames.Pattern = words.A;
                phantoon.CasualFlames.Hold = CoordinateAxis.AsSigned(words.B);
                // The record's count reads below zero while no burst is under way, which the burst holds as no count.
                short casualFlames = unchecked((short)words.C);
                phantoon.CasualFlames.Remaining = casualFlames < 0 ? null : casualFlames;
                phantoon.Wave.Amplitude = SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(words.D);
                phantoon.Wave.AmplitudeShrinking = words.E != 0;
                phantoon.Wave.PhaseStep = words.F;
            }
            else if (part == EnemyDefinitionId.PhantoonBody)
            {
                phantoon.StartingFlames.NextFlame = words.A;
                phantoon.FigureEight.PathCursor = words.A;
                phantoon.Death.ExplosionPasses = words.A;
                phantoon.StartingFlames.Orbiting = words.B != 0;
                phantoon.FigureEight.Speed = SignedSixteenSixteen.FromParts(CoordinateAxis.AsSigned(words.C), words.B);
                phantoon.FigureEight.Stage = PhantoonFigureEightStageWords.Decode(words.D);
                phantoon.PhaseTimer = CoordinateAxis.AsSigned(words.E);
                if (words.F != 0)
                    phantoon.Phase = PhantoonNativeWords.Phases.Decode(words.F, "recorded PhantoonPhase");
            }
            else
            {
                throw new InvalidDataException($"Enemy {part} holds Phantoon's state but is not one of his parts.");
            }
        }
        if (enemy.SpeciesState<KraidPartState>() is { } kraidPartRecord)
        {
            bool deathWait = KraidNativeWords.DeathWaitUpdatesLeft.TryGetValue(words.A, out int deathWaitLeft);
            // A part whose setup has not given it a phase records no word; it rests.
            kraidPartRecord.Phase = words.A == 0
                ? KraidPhase.Resting
                : deathWait ? KraidPhase.DeathWaitBeforeFadeIn : KraidNativeWords.Phases.Decode(words.A, "recorded KraidPhase");
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
                else if (deathWait)
                    kraidBody.PhaseTimer = deathWaitLeft;
                else
                    kraidBody.PhaseTimer = words.F;
            }
            else
            {
                kraidPartRecord.PhaseTimer = words.F;
            }
        }
        if (enemy.SpeciesState<GunshipState>() is { } gunshipRecord)
        {
            // The cartridge keeps the ship's phase timer, engine readying, bob phase, bob timer, and base Y and
            // landing-brake step in the top hull's variables A through E.
            gunshipRecord.PhaseTimer = unchecked((short)words.A);
            gunshipRecord.EngineReadyingElapsed = words.B;
            gunshipRecord.BobPhase = words.C;
            gunshipRecord.BobTimer = unchecked((short)words.D);
            gunshipRecord.BaseY = words.E;
            gunshipRecord.LandingBrakeStep = words.E;
        }
        if (enemy.DefinitionId == EnemyDefinitionId.ShipBottomEntrance && index > 0 &&
            enemies.Places.InUpdateOrder[index - 1].SpeciesState<GunshipState>() is { } bottomOfShip)
        {
            // The bottom hull, listed right after the top, keeps the ship's engine-sound timer in variable D, its
            // rumble frame in E and its escape speed in F.
            bottomOfShip.EngineSoundTimer = unchecked((short)words.D);
            bottomOfShip.RumbleFrame = words.E;
            bottomOfShip.EscapeVelocity = new SignedEightEight(words.F);
        }
        if (enemy.SpeciesState<CeresSteamEnemyState>() is { } ceresSteamRecord)
        {
            ceresSteamRecord.Action = CeresNativeWords.DecodeSteamAction(words.A, "recorded CeresSteamAction");
            ceresSteamRecord.ActivationDelay = WrappingCountdown.Start(words.D);
        }
        if (enemy.SpeciesState<CeresElevatorDoorState>() is { } elevatorDoorRecord)
        {
            elevatorDoorRecord.Phase = CeresNativeWords.ElevatorDoorPhases.Decode(words.A, "recorded Ceres elevator door phase");
            elevatorDoorRecord.RumbleCallsLeft = words.D;
            elevatorDoorRecord.ExplosionIntervalLeft = words.E;
            elevatorDoorRecord.LastExplosionPlace = words.F;
        }
        else if (enemy.SpeciesState<CeresDoorAnimationState>() is not null && words.A != CeresNativeWords.MainBehaviourWord(enemy))
        {
            // Every other Ceres door and wall runs one main behaviour, which its actor names (#1953).
            throw new InvalidDataException(
                $"Recorded Ceres door or wall {enemy.Ordinal} runs routine {words.A:X4}, not its actor's {CeresNativeWords.MainBehaviourWord(enemy):X4}.");
        }
        if (enemy.SpeciesState<CeresRidleyRoomDoorState>() is { } ridleyRoomDoorRecord)
            ridleyRoomDoorRecord.HandsDrawingToRidley = words.B != 0;
        if (enemy.SpeciesState<DeadTorizoEnemyState>() is { } deadTorizoRecord)
        {
            deadTorizoRecord.Phase = CorpsesNativeWords.DeadTorizoPhases.Decode(words.A, "recorded DeadTorizoPhase");
            deadTorizoRecord.Velocity = new(new SignedEightEight(words.B), new SignedEightEight(words.C));
        }
        if (enemy.SpeciesState<MotherBrainFallingTubeState>() is { } fallingTubeRecord)
        {
            // A zero word is a setup that never ran this far (a defeated room's deleted boss); the state keeps its start.
            if (words.A != 0)
                fallingTubeRecord.Action = MotherBrainNativeWords.MotherBrainTubeActions.Decode(words.A, "recorded MotherBrainTubeAction");
            fallingTubeRecord.FloorY = words.B;
            fallingTubeRecord.FallSpeed = new SignedEightEight(words.C);
            fallingTubeRecord.SmokeDelay = CoordinateAxis.AsSigned(words.D);
            fallingTubeRecord.SmokePositionIndex = words.E;
        }
        if (enemy.SpeciesState<RidleyExplosionFragmentState>() is { } ridleyFragmentRecord)
        {
            ridleyFragmentRecord.Velocity = new(new SignedEightEight(words.B), new SignedEightEight(words.C));
            ridleyFragmentRecord.Lifetime = CoordinateAxis.AsSigned(words.F);
        }
        if (enemy.SpeciesState<DraygonEnemyState>() is { } draygonRecord)
        {
            draygonRecord.Phase = words.A == 0
                ? DraygonPhase.None
                : DraygonNativeWords.Phases.Decode(words.A, "recorded DraygonPhase");
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
        if (enemy.SpeciesState<DraygonEyeState>() is { } draygonEyeRecord)
        {
            // A zero word is a setup that never ran this far (a defeated room's deleted boss); the state keeps its start.
            if (words.A != 0)
                draygonEyeRecord.Action = DraygonNativeWords.EyeActions.Decode(words.A, "recorded DraygonEyeAction");
            // The game compares only the look angle's low byte, the bearing it stored.
            draygonEyeRecord.LookAngle = new Bearing(words.F & byte.MaxValue);
        }
        if (enemy.SpeciesState<CrocomireEnemyState>() is { } crocomireRecord)
        {
            crocomireRecord.DeathPhase = CrocomireNativeWords.DeathPhases.Decode(words.A, "recorded Crocomire death phase");
            crocomireRecord.DamageReactionPending = CrocomireNativeWords.DamageReactionPending(words.B);
            // The fight table's first entry, which no live fight selects, stands for a defeated room's corpse, which keeps its
            // constructed phase and never runs it (#1939).
            if (words.C != 0)
                crocomireRecord.Phase = CrocomireNativeWords.Phases.Decode(words.C, "recorded Crocomire fight function");
            // The cartridge keeps several of the body's values in each of words D, E and F; the phase says which.
            CrocomireNativeWords.ImportSharedWords(crocomireRecord, words.D, words.E, words.F);
        }
        if (enemy.SpeciesState<CrocomireTongueState>() is { } crocomireTongueRecord)
            crocomireTongueRecord.BodyXOffset = words.A;
        if (enemy.SpeciesState<DeadSidehopperEnemyState>() is { } deadSidehopperRecord)
        {
            deadSidehopperRecord.Action = CorpsesNativeWords.DeadSidehopperActions.Decode(words.A, "recorded DeadSidehopperAction");
            deadSidehopperRecord.PreRotDelayCounter = words.B;
            deadSidehopperRecord.StateTimer = unchecked((short)words.F);
        }
        if (enemy.SpeciesState<BotwoonEnemyState>() is { } botwoonRecord)
        {
            // A zero word is a setup that never ran this far (a defeated room's deleted boss); the state keeps its start.
            if (words.D != 0)
                botwoonRecord.Phase = BotwoonNativeWords.Phases.Decode(words.D, "recorded BotwoonPhase");
            if (words.E != 0)
                botwoonRecord.Movement = BotwoonNativeWords.Movements.Decode(words.E, "recorded BotwoonMovement");
            if (words.F != 0)
                botwoonRecord.HeadAction = BotwoonNativeWords.HeadActions.Decode(words.F, "recorded BotwoonHeadAction");
        }
        if (enemy.SpeciesState<ShitroidEnemyState>() is { } shitroidRecord)
        {
            // A zero word is a setup that never ran this far (a defeated room's deleted boss); the state keeps its start.
            if (words.A != 0)
                shitroidRecord.Action = MotherBrainNativeWords.ShitroidActions.Decode(words.A, "recorded ShitroidAction");
            shitroidRecord.Velocity = new(new SignedEightEight(words.B), new SignedEightEight(words.C));
            shitroidRecord.MembranePulsePhase = LittleEndianBytes.LowByte(words.D);
            shitroidRecord.ColourTimer = LittleEndianBytes.HighByte(words.D);
            shitroidRecord.ColourDelay = words.E;
            shitroidRecord.StateTimer = CoordinateAxis.AsSigned(words.F);
        }
        if (enemy.SpeciesState<DachoraEchoState>() is { } dachoraEcho)
        {
            // An echo's record names its flicker routine and keeps its lifetime in variable D. The first echo's variable A
            // holds its body's trail timer, which Core keeps on the body listed right before it (#2147).
            if (words.F != AnimalsNativeWords.DachoraEchoRoutine)
                throw new InvalidDataException($"Recorded Dachora echo {enemy.Ordinal} runs routine {words.F:X4}, not the echo's.");
            dachoraEcho.Lifetime = unchecked((short)words.D);
            if (enemies.Places.ListedBeforeOrNone(enemy, 1)?.SpeciesState<DachoraEnemyState>() is { } echoBody)
                echoBody.TrailTimer = unchecked((short)words.A);
        }
        if (enemy.SpeciesState<DachoraEnemyState>() is { } dachoraRecord)
        {
            dachoraRecord.SetGlowCycle(frame: LittleEndianBytes.HighByte(words.E), ticks: LittleEndianBytes.LowByte(words.E));
            dachoraRecord.Action = AnimalsNativeWords.DachoraActions.Decode(words.F, "recorded DachoraAction");
            // The cartridge keeps the speed's whole pixels and the countdowns in variable A and the acceleration's fraction
            // in variable D; each is read here as the meaning the recorded phase gives it.
            if (dachoraRecord.Action is DachoraAction.BlinkingBeforeRun or DachoraAction.ChargingShinespark)
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
        // A Brinstar Pipe Bug keeps its action in word F and its flight direction in word A's sign; the Norfair and yellow
        // bugs keep their action in word A.
        if (enemy.SpeciesState<BrinstarPipeBugState>() is { } brinstarPipeBug)
        {
            brinstarPipeBug.Action = PipeBugsNativeWords.BrinstarPipeBugActions.Decode(words.F, "recorded BrinstarPipeBugAction");
            brinstarPipeBug.FliesLeft = CoordinateAxis.AsSigned(words.A) < 0;
        }
        if (enemy.SpeciesState<NorfairPipeBugState>() is { } norfairPipeBug)
        {
            // A member's stagger routine names its role's offset (#2071); every other routine names an action.
            if (PipeBugsNativeWords.FormationStaggers.TryDecode(words.A, out NorfairPipeBugRole? staggerRole) &&
                staggerRole.PostRiseAction == NorfairPipeBugAction.Stagger)
            {
                norfairPipeBug.Action = NorfairPipeBugAction.Stagger;
                norfairPipeBug.StaggerOffset = staggerRole.StaggerOffset;
            }
            else
            {
                norfairPipeBug.Action = PipeBugsNativeWords.NorfairPipeBugActions.Decode(words.A, "recorded NorfairPipeBugAction");
            }
        }
        if (enemy.SpeciesState<YellowPipeBugState>() is { } yellowPipeBug)
            yellowPipeBug.Action = PipeBugsNativeWords.YellowPipeBugActions.Decode(words.A, "recorded YellowPipeBugAction");
        if (enemy.SpeciesState<MagdollitePartState>() is MagdolliteHeadState magdolliteHead)
            magdolliteHead.Action = MagdolliteNativeWords.MagdolliteHeadActions.Decode(words.F, "recorded MagdolliteHeadAction");
        if (enemy.SpeciesState<MagdollitePartState>() is MagdolliteBodyState magdolliteBody)
        {
            magdolliteBody.Action = MagdolliteNativeWords.MagdolliteBodyActions.Decode(words.F, "recorded MagdolliteBodyAction");
            // The recorded word counts the arm's stages up from its setup, two to a stage.
            MagdolliteRiseStage stage = MagdolliteRiseStage.Setup;
            for (int place = 0; place < words.B / 2; place++)
                stage = stage.Higher;
            magdolliteBody.BodyPhase = stage;
            magdolliteBody.RiseDisplacementY = unchecked((short)words.E);
        }
        if (enemy.SpeciesState<MagdollitePartState>() is MagdolliteOverlayState magdolliteOverlay)
        {
            magdolliteOverlay.Action = MagdolliteNativeWords.MagdolliteOverlayActions.Decode(words.F, "recorded MagdolliteOverlayAction");
            magdolliteOverlay.ThrowsRight = words.B != 0;
        }
        if (enemy.SpeciesState<SbugEnemyState>() is { } sbugRecord)
        {
            sbugRecord.StraightMovementTimer = WrappingCountdown.Start(words.A);
            sbugRecord.Action = SbugNativeWords.SbugActions.Decode(words.B, "recorded SbugAction");
            sbugRecord.FaceCustomHeading(RotationAngle.FromSteps(LittleEndianBytes.LowByte(words.C)));
            sbugRecord.MovementTimer = unchecked((short)words.F);
        }
        if (enemy.SpeciesState<KiHunterBodyState>() is { } kiHunterBodyRecord)
        {
            kiHunterBodyRecord.Action = KiHuntersNativeWords.KiHunterActions.Decode(words.A, "recorded KiHunterAction");
            kiHunterBodyRecord.SwoopTargetX = words.B;
            kiHunterBodyRecord.SwoopOriginY = words.C;
            kiHunterBodyRecord.AnimationChangeAngle = RotationAngle.FromSteps(words.E);
            kiHunterBodyRecord.SwoopAngle = RotationAngle.WrappedSteps(words.F);
        }
        if (enemy.SpeciesState<KiHunterWingState>() is { } kiHunterWingRecord)
        {
            kiHunterWingRecord.Action = KiHuntersNativeWords.KiHunterWingActions.Decode(words.A, "recorded KiHunterWingAction");
            kiHunterWingRecord.DetachedSpeedCurvePlace = QuadraticSpeedCurvePlace.FromTwoHundredFiftySixthsOfMark(words.B);
            kiHunterWingRecord.WingArcAngle = RotationAngleConstants.FromRaw(words.F);
        }
        if (enemy.SpeciesState<ZebetiteEnemyState>() is { } zebetiteRecord)
        {
            // A zero word is a setup that never ran this far (a defeated room's deleted boss); the state keeps its start.
            if (words.A != 0)
                zebetiteRecord.Action = MotherBrainNativeWords.ZebetiteActions.Decode(words.A, "recorded ZebetiteAction");
            zebetiteRecord.Generation = words.D;
            zebetiteRecord.Form = MotherBrainNativeWords.DecodeBarrierForm(words.F);
        }
        // The native snapshot keeps the Zebetites' shared pulse counter in slot zero's word C, whichever enemy occupies the slot.
        if (enemy.Ordinal == 0)
            enemies.ZebetiteBehavior.SharedColourCycle = words.C;
        if (enemy.SpeciesState<ChozoStatueState>() is { } chozoStatueRecord)
        {
            chozoStatueRecord.AwaitingHand = ChozoNativeWords.AwaitingHandAt(words.F, chozoStatueRecord.IsWreckedShip);
        }
        if (enemy.SpeciesState<YardEnemyState>() is { } yardRecord)
        {
            yardRecord.CrawlingVelocity = new(new SignedEightEight(words.A), new SignedEightEight(words.B));
            yardRecord.AirborneFacingDirection = words.C switch
            {
                0 => YardAirborneFacing.Left,
                1 => YardAirborneFacing.Right,
                _ => throw new InvalidDataException($"Native Yard airborne facing {words.C:X4} is neither left nor right."),
            };
            yardRecord.ConsecutiveTurnCounter = words.E;
            yardRecord.Movement = YardNativeWords.YardMovements.Decode(words.F, "recorded YardMovement");
        }
        if (enemy.SpeciesState<ZoaEnemyState>() is { } zoaRecord)
        {
            zoaRecord.SpawnPosition = new(words.A, words.B);
            // The record keeps the requested animation beside the installed one (#2145): Core keeps the Zoa's facing, which
            // the requested animation shows, and only the playing animation, which this import does not read.
            ZoaSegment requested = ZoaNativeWords.AnimationSelectorOf(words.C);
            zoaRecord.FacesRight = requested is ZoaSegment.FacingRightRising or ZoaSegment.FacingRightShooting;
            RequirePlaying(zoaRecord.Playback, ZoaAnimations.StartingWith(ZoaNativeWords.AnimationSelectorOf(words.D)), "Zoa");
            zoaRecord.Action = ZoaNativeWords.ZoaActions.Decode(words.F, "recorded ZoaAction");
        }
        if (enemy.SpeciesState<BabyTurtleEnemyState>() is { } babyTurtleRecord)
        {
            babyTurtleRecord.Action = MamaTurtlesNativeWords.BabyTurtleActions.Decode(words.A, "recorded BabyTurtleAction");
            // Every Baby Turtle's parent is a Mama Turtle, who links her babies when she is placed.
            if (enemies.Places.InUpdateOrder[SlotIndex(words.B, "Baby Turtle parent offset")].DefinitionId != EnemyDefinitionId.MamaTurtle)
                throw new InvalidDataException($"Recorded Baby Turtle parent offset ${words.B:X4} does not name a Mama Turtle.");
            babyTurtleRecord.SpawnTopCenter = new(words.C, words.D);
            babyTurtleRecord.Velocity = new(unchecked((short)words.E), babyTurtleRecord.Velocity.Y);
        }
        if (enemy.SpeciesState<OwtchEnemyState>() is { } owtchRecord)
        {
            // $A2:A423-$A435: fraction then whole for each direction, then the state and the sink depth.
            owtchRecord.Speed = NegatedPair(SignedSixteenSixteen.FromParts(unchecked((short)words.B), words.A),
                SignedSixteenSixteen.FromParts(unchecked((short)words.D), words.C), "Owtch speed");
            owtchRecord.Behavior = OwtchNativeWords.Behaviors.Decode(words.E, "recorded OwtchBehaviorState");
            owtchRecord.BurialDepth = words.F;
        }
        if (enemy.SpeciesState<BullEnemyState>() is { } bullRecord)
        {
            bullRecord.Action = BullNativeWords.BullActions.Decode(words.A, "recorded BullAction");
            bullRecord.Acceleration = new SignedEightEight(words.B);
            bullRecord.AccelerationDelta = new SignedEightEight(words.C);
            bullRecord.Speed = new SignedEightEight(words.D);
            bullRecord.ActivationTimer = unchecked((short)words.E);
            bullRecord.AccelerationIntervalTimer = unchecked((short)words.F);
        }
        if (enemy.SpeciesState<BoyonEnemyState>() is { } boyonRecord)
        {
            boyonRecord.SpeedMultiplier = words.A;
            boyonRecord.Speed = UnsignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(words.B);
            boyonRecord.JumpHeight = SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(words.C);
            boyonRecord.ArcDistance = SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(words.D);
            // The rise leaves the curve index one below the top entry, the word's largest value, past the top.
            boyonRecord.CurvePlace = words.E == ushort.MaxValue ? BoyonCurvePlaceConstants.PastTheTop : BoyonCurvePlaceConstants.AtEntry(words.E);
            boyonRecord.Bouncing = words.F != 0;
        }
        if (enemy.SpeciesState<GrowingShutterEnemyState>() is { } growingShutterRecord)
        {
            growingShutterRecord.Action = ShuttersNativeWords.GrowingShutterActions.Decode(words.A, "recorded GrowingShutterAction");
            ushort[] originRows = [words.B, words.C, words.D, words.E];
            for (int level = 0; level < GrowingShutterEnemyState.SectionCount; level++)
                growingShutterRecord.SetSectionOriginY(level, originRows[level]);
            growingShutterRecord.GrowthLevel = words.F;
        }
        // A bobbing Fireflea keeps none of these words.
        if (enemy.SpeciesState<FirefleaEnemyState>() is { Flight: CirclingFirefleaState firefleaRecord })
        {
            firefleaRecord.AngularSpeed = SuperMetroid.AssetExtraction.Enemies.EnemySpawnSelectors.FirefleaTurnPerUpdate(SignedSixteenSixteen.FromParts(unchecked((short)words.B), words.A));
            firefleaRecord.Radius = words.C;
            firefleaRecord.Angle = RotationAngleConstants.FromRaw(words.D);
            firefleaRecord.Center = new(words.E, words.F);
        }
        if (enemy.SpeciesState<VerticalShutterEnemyState>() is { } verticalShutterRecord)
        {
            verticalShutterRecord.Action = ShuttersNativeWords.DecodeVerticalShutterAction(words.A, verticalShutterRecord.TriggerMode);
            verticalShutterRecord.ActionTimer = WrappingCountdown.Start(words.B);
            verticalShutterRecord.DownVelocity = SignedSixteenSixteen.FromParts(unchecked((short)words.D), words.C);
            verticalShutterRecord.UpVelocity = SignedSixteenSixteen.FromParts(unchecked((short)words.F), words.E);
        }
        if (enemy.SpeciesState<CacatacEnemyState>() is { } cacatacRecord)
        {
            // The record holds the speed and its negation for walking left; the speed alone is the state.
            cacatacRecord.Speed = cacatacRecord.Speed.WithWholePixels(unchecked((short)words.A));
            cacatacRecord.Direction = CacatacNativeWords.Directions.Decode(words.D, "recorded CacatacDirection");
            cacatacRecord.Action = CacatacNativeWords.CacatacActions.Decode(words.E, "recorded CacatacAction");
        }
        if (enemy.SpeciesState<WorkRobotEnemyState>() is { } workRobotRecord)
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
        if (enemy.SpeciesState<KzanEnemyState>() is { } kzanRecord)
        {
            kzanRecord.Action = KzanNativeWords.KzanActions.Decode(words.A, "recorded KzanAction");
            kzanRecord.FallWaitTimer = WrappingCountdown.Start(words.B);
            kzanRecord.RisingTargetY = words.C;
            kzanRecord.FallingTargetY = words.D;
        }
        if (enemy.SpeciesState<MetroidEnemyState>() is { } metroidRecord)
        {
            metroidRecord.Velocity = FixedVector<RoomPixels>.FromStoredParts(words.B, words.A, words.D, words.C);
            metroidRecord.EscapeTimer = unchecked((short)words.E);
            metroidRecord.Movement = MetroidsNativeWords.Movements.Decode(words.F, "recorded MetroidMovement");
        }
        if (enemy.SpeciesState<FakeKraidEnemyState>() is { } fakeKraidRecord)
        {
            fakeKraidRecord.WalkDelta = unchecked((short)words.B);
            fakeKraidRecord.FacingDelta = unchecked((short)words.C);
            fakeKraidRecord.WalkStepTimer = words.D;
            fakeKraidRecord.SpitDecisionTimer = words.E;
        }
        if (enemy.SpeciesState<MaridiaLargeSnailEnemyState>() is { } maridiaLargeSnailRecord)
        {
            maridiaLargeSnailRecord.Action = MaridiaSnailsNativeWords.MaridiaLargeSnailActions.Decode(words.A, "recorded MaridiaLargeSnailAction");
            maridiaLargeSnailRecord.YSpeedCurvePlace = QuadraticSpeedCurvePlace.FromTwoHundredFiftySixthsOfMark(unchecked((short)words.B));
            maridiaLargeSnailRecord.AttackCooldown = unchecked((short)words.C);
            maridiaLargeSnailRecord.MovingLeft = words.D != 0;
            maridiaLargeSnailRecord.RemainingBounces = unchecked((short)words.E);
            maridiaLargeSnailRecord.BouncePhase = MaridiaSnailsNativeWords.MaridiaLargeSnailBouncePhases.Decode(words.F, "recorded MaridiaLargeSnailBouncePhase");
        }
        if (enemy.SpeciesState<WallSpacePirateEnemyState>() is { } wallSpacePirateRecord)
        {
            wallSpacePirateRecord.Action = SpacePiratesNativeWords.WallSpacePirateActions.Decode(words.A, "recorded WallSpacePirateAction");
            wallSpacePirateRecord.ClimbDirection = SpacePiratesNativeWords.WallClimbDirections.Decode(words.C, "recorded WallSpacePirateClimbDirection");
            wallSpacePirateRecord.WallJumpArcCenter = new(words.D, words.E);
            wallSpacePirateRecord.WallJumpArcAngle = RotationAngle.FromSteps(checked((byte)words.F));
        }
        if (enemy.SpeciesState<NinjaSpacePirateEnemyState>() is { } ninjaSpacePirateRecord)
        {
            ninjaSpacePirateRecord.Action = SpacePiratesNativeWords.NinjaSpacePirateActions.Decode(words.A, "recorded NinjaSpacePirateAction");
            ninjaSpacePirateRecord.PostsMidpointX = words.D;
            ninjaSpacePirateRecord.LeftPostX = words.E;
            ninjaSpacePirateRecord.RightPostX = words.F;
        }
        if (enemy.SpeciesState<SkreeEnemyState>() is { } skreeRecord)
        {
            skreeRecord.BurrowTimer = unchecked((short)words.A);
            skreeRecord.Action = SkreeNativeWords.SkreeActions.Decode(words.B, "recorded SkreeAction");
            RequirePlayingSkreeMetareeAnimation(skreeRecord.Playback, SkreeNativeWords.SkreeAnimationPhases, words, "Skree");
            skreeRecord.AttackReady = words.E != 0;
        }
        if (enemy.SpeciesState<BoulderEnemyState>() is { } boulderRecord)
        {
            boulderRecord.Action = BoulderNativeWords.BoulderActions.Decode(words.A, "recorded BoulderAction");
            boulderRecord.TravelSpeedAccumulator = QuadraticSpeedCurvePlace.FromTwoHundredFiftySixthsOfMark(words.B);
            boulderRecord.BounceSpeedAccumulator = QuadraticSpeedCurvePlace.FromTwoHundredFiftySixthsOfMark(words.C);
            // The record counts the bounces left, two before the first impact and minus one once rolling.
            boulderRecord.ImpactsSoFar = BoulderBounces.ImpactCount - 1 - unchecked((short)words.D);
            boulderRecord.Roll = words.E switch
            {
                0 => BoulderRoll.Right,
                1 => BoulderRoll.Left,
                2 => BoulderRoll.LeftBreakingOnImpact,
                _ => throw new InvalidDataException($"Recorded Boulder roll {words.E} is not a roll."),
            };
        }
        if (enemy.SpeciesState<RinkaEnemyState>() is { } rinkaRecord)
        {
            // A zero word is a setup that never ran this far (a defeated room's deleted boss); the state keeps its start.
            if (words.A != 0)
                rinkaRecord.Action = MotherBrainNativeWords.RinkaActions.Decode(words.A, "recorded RinkaAction");
            rinkaRecord.Velocity = new(new SignedEightEight(words.B), new SignedEightEight(words.C));
            rinkaRecord.HeldSpawnPoint = words.D == 0 ? null : MotherBrainNativeWords.RinkaSpawnPoints.Decode(words.D, "recorded RinkaSpawnPoint");
            rinkaRecord.DelayTimer = CoordinateAxis.AsSigned(words.F);
        }
        if (enemy.SpeciesState<EtecoonEnemyState>() is { } etecoonRecord)
        {
            etecoonRecord.Velocity = FixedVector<RoomPixels>.FromStoredParts(words.C, words.D, words.A, words.B);
            etecoonRecord.Action = AnimalsNativeWords.EtecoonActions.Decode(words.F, "recorded EtecoonAction");
            // A dormant Etecoon's record holds its countdown below zero while it waits for Samus; the port's dormant
            // Etecoon waits without one, and every other record's countdown is the wait it has left.
            etecoonRecord.AwaitingSamus = etecoonRecord.Action == EtecoonAction.WaitingForSamus && unchecked((short)words.E) < 0;
            if (!etecoonRecord.AwaitingSamus)
                etecoonRecord.ActionTimer = words.E;
        }
        if (enemy.SpeciesState<MochtroidEnemyState>() is { } mochtroidRecord)
        {
            mochtroidRecord.Velocity = FixedVector<RoomPixels>.FromStoredParts(words.B, words.A, words.D, words.C);
            mochtroidRecord.MovementMode = MetroidsNativeWords.MochtroidMovementModes.Decode(words.F, "recorded MochtroidMovementMode");
        }
        if (enemy.SpeciesState<MorphBallEyeEnemyState>() is { } morphBallEyeRecord)
        {
            morphBallEyeRecord.IsActivated = words.C != 0;
            morphBallEyeRecord.Angle = RotationAngle.FromSteps(unchecked((byte)words.D));
            morphBallEyeRecord.ActionTimer = unchecked((short)words.E);
            morphBallEyeRecord.Action = MorphBallEyeNativeWords.MorphBallEyeActions.Decode(words.F, "recorded MorphBallEyeAction");
        }
        if (enemy.SpeciesState<HopperEnemyState>() is { } hopperRecord)
        {
            hopperRecord.Action = HoppersNativeWords.HopperActions.Decode(words.A, "recorded HopperAction");
            hopperRecord.YSpeedCurvePlace = QuadraticSpeedCurvePlace.FromMarks(words.B);
            hopperRecord.XVelocity = unchecked((short)words.C);
            hopperRecord.YSpeedCurvePlaceStep = QuadraticSpeedCurvePlace.FromMarks(words.D);
        }
        if (enemies.PlatformStates[index] is { } platform)
        {
            platform.RestingY = words.A;
            platform.Speed = NegatedPair(SignedSixteenSixteen.FromParts(unchecked((short)words.C), words.B),
                SignedSixteenSixteen.FromParts(unchecked((short)words.E), words.D), "platform speed");
            platform.YSpeedCurvePlace = QuadraticSpeedCurvePlace.FromMarks(Index(words.F, platform.MaximumYSpeedCurvePlace.WholeMarks, "platform acceleration index"));
        }
        if (enemies.BeetomStates[index] is { } beetom)
        {
            beetom.YSpeedCurvePlace = QuadraticSpeedCurvePlace.FromMarks(Index(words.B, short.MaxValue, "Beetom speed index"));
            beetom.Action = BeetomNativeWords.BeetomActions.Decode(words.C, "recorded BeetomAction");
            beetom.ActionTimer = unchecked((short)words.D);
            beetom.ButtonCounter = unchecked((short)words.E);
            beetom.LastSeenPlayerActions = GameplayActionWordCodec.FromJoypadWord(words.F, "native Beetom previous controller input");
        }
        if (enemy.DefinitionId == EnemyDefinitionId.Shaktool && enemies.ShaktoolSegments[index] is { } segment)
        {
            segment.TargetAngle = RotationAngleConstants.FromRaw(words.A);
            segment.OrbitAngle = RotationAngleConstants.FromRaw(words.B);
            segment.AngularVelocity = RotationAngleConstants.FromRaw(Index(words.C, 1020, "Shaktool angular velocity"));
            ImportShaktoolOrientation(segment, Array.IndexOf(ShaktoolSegmentDefinitions.Segments, enemy.SpawnSettings<ShaktoolSpawnSettings>().Segment), words.D);
            // Every segment's owner is the chain's leading saw, the first enemy its room lists.
            if (SlotIndex(words.E, "Shaktool owner") != 0)
                throw new InvalidDataException($"Recorded Shaktool owner ${words.E:X4} does not name the first enemy, where the chain's saw is placed.");
            segment.Motion = words.F == 0 ? null : ShaktoolNativeWords.DecodeMotion(words.F, "recorded ShaktoolSegmentMotion");
        }
        if (enemies.ChootStates[index] is { } choot)
        {
            choot.Action = ChootNativeWords.ChootActions.Decode(words.A, "recorded ChootAction");
            // The ascent cursor is 8.8 and moves by whole records, so its signed high byte is the record index.
            choot.YSpeedCurvePlace = QuadraticSpeedCurvePlace.FromMarks(unchecked((short)words.B) >> 8);
            choot.FallingPatternFrame = (words.C & 0xff) == 0
                ? words.C >> 8
                : throw new InvalidDataException($"Recorded Choot falling cursor ${words.C:X4} is not a whole frame.");
            choot.FallingPatternLoopCounter = unchecked((short)words.D);
            PrivateState.Set(choot, nameof(ChootEnemyState.FallingPath), ChootNativeWords.ChootFallingPaths.Decode(words.E, "recorded Choot falling pattern"));
            choot.FallingPatternYDistance = words.F;
        }
        if (enemies.SkulteraStates[index] is { } skultera)
        {
            skultera.Action = SkulteraNativeWords.SkulteraActions.Decode(words.A, "recorded SkulteraAction");
            skultera.RightVelocity = SignedSixteenSixteen.FromParts(unchecked((short)words.C), words.B);
            skultera.LeftVelocity = SignedSixteenSixteen.FromParts(unchecked((short)words.E), words.D);
            // Only the angle word's low byte is ever read; movement discards its high byte.
            skultera.Angle = RotationAngle.FromSteps(LittleEndianBytes.LowByte(words.F));
        }
        if (enemy.EntityState<KagoEntity, KagoEnemyState>() is { } kago)
        {
            kago.UsesFastAnimation = words.B != 0;
        }
        if (enemy.DefinitionId == EnemyDefinitionId.Coven && enemy.SpeciesState<WreckedShipGhostEnemyState>() is { } ghost)
        {
            ghost.Action = WreckedShipNativeWords.WreckedShipGhostActions.Decode(words.A, "recorded WreckedShipGhostAction");
            ghost.PhaseTimer = unchecked((short)words.B);
            ghost.NextFlickerStep = WreckedShipGhostFlickerStepOf(words.C);
        }
        if (enemy.EntityState<WalkingSpacePirateEntity, WalkingSpacePirateEnemyState>() is { } walkingPirate)
        {
            walkingPirate.Action = SpacePiratesNativeWords.WalkingSpacePirateActions.Decode(words.A, "recorded WalkingSpacePirateAction");
            walkingPirate.LeftPostX = words.E;
            walkingPirate.RightPostX = words.F;
        }
        if (enemy.EntityState<MultiviolaEntity, MultiviolaEnemyState>() is { } multiviola)
            multiviola.Velocity = FixedVector<RoomPixels>.FromStoredParts(words.C, words.D, words.E, words.F);
        if (enemy.EntityState<CrawlerEntity, CrawlerEnemyState>() is { } crawler)
        {
            crawler.Velocity = new(new SignedEightEight(words.A), new SignedEightEight(words.B));
            crawler.Action = CrawlersNativeWords.CrawlerActions.Decode(words.F, "recorded CrawlerAction");
        }
        if (enemy.DefinitionId == EnemyDefinitionId.Holtz && enemy.SpeciesState<LowerNorfairRioEnemyState>() is { } holtz)
        {
            holtz.Velocity = new(new SignedEightEight(words.D), new SignedEightEight(words.C));
            holtz.Action = RiosNativeWords.LowerNorfairRioActions.Decode(words.F, "recorded LowerNorfairRioAction");
        }
        if (enemy.DefinitionId == EnemyDefinitionId.EtecoonEscape && enemy.SpeciesState<EscapeEtecoonEnemyState>() is { } escape)
        {
            PrivateState.Set(escape, nameof(EscapeEtecoonEnemyState.Velocity), new SignedEightEight(words.A));
            PrivateState.Set(escape, nameof(EscapeEtecoonEnemyState.Action), AnimalsNativeWords.EscapeEtecoonActionAt(words.F));
        }
        if (enemy.DefinitionId == EnemyDefinitionId.FaceBlock && enemy.SpeciesState<BlueBrinstarFaceBlockEnemyState>() is { } faceBlock)
        {
            faceBlock.Activated = words.A != 0;
        }
        if (enemy.DefinitionId == EnemyDefinitionId.NuclearWaffle && enemy.SpeciesState<NuclearWaffleEnemyState>() is { } waffle)
        {
            waffle.Action = NuclearNativeWords.NuclearWaffleActions.Decode(words.A, "recorded NuclearWaffleAction");
            waffle.WaitingTimer = unchecked((short)words.B);
        }
        if (enemy.DefinitionId == EnemyDefinitionId.Polyp && enemy.SpeciesState<PolypEnemyState>() is { } polyp)
        {
            polyp.Action = PolypNativeWords.PolypActions.Decode(words.A, "recorded PolypAction");
            polyp.CooldownTimer = unchecked((short)words.B);
        }
        // The halves reuse the same words for different values, each half into its own state (#1845).
        if (enemy.DefinitionId == EnemyDefinitionId.Powamp && enemy.SpeciesState<PowampBalloonState>() is { } powampBalloon)
        {
            powampBalloon.Spawn = new(words.A, words.B);
            powampBalloon.GrappleTravelDistance = words.D;
            if (words.F != PowampNativeWords.BalloonIdleWord)
                throw new InvalidDataException($"Recorded Powamp balloon runs ${words.F:X4}, not the balloon's empty routine.");
        }
        if (enemy.DefinitionId == EnemyDefinitionId.Powamp && enemy.SpeciesState<PowampBodyState>() is { } powampBody)
        {
            powampBody.YVelocity = SignedSixteenSixteen.FromParts(unchecked((short)words.A), words.B);
            powampBody.WiggleIndex = Index(words.C, 11, "Powamp wiggle phase");
            powampBody.WiggleTimer = unchecked((short)words.D);
            powampBody.ActionTimer = unchecked((short)words.E);
            powampBody.Action = PowampNativeWords.PowampActions.Decode(words.F, "recorded PowampAction");
        }
        if (enemy.DefinitionId == EnemyDefinitionId.MamaTurtle && enemy.SpeciesState<MamaTurtleEnemyState>() is { } mama)
        {
            mama.Action = MamaTurtlesNativeWords.MamaTurtleActions.Decode(words.A, "recorded MamaTurtleAction");
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
            hibashi.Action = HibashiNativeWords.HibashiActions.Decode(words.A, "recorded HibashiAction");
            hibashi.InactiveTimer = unchecked((short)words.B);
            hibashi.ActivityFinished = words.C != 0;
            hibashi.SpawnY = words.D;
        }
        if (enemies.AlcoonStates[index] is { } alcoon)
        {
            alcoon.Action = AlcoonNativeWords.AlcoonActions.Decode(words.A, "recorded AlcoonAction");
            alcoon.WalkingDirection = CoordinateAxis.AsSigned(words.D) switch
            {
                -2 => HorizontalDirection.Left,
                2 => HorizontalDirection.Right,
                0 => null,
                var speed => throw new InvalidDataException($"Recorded Alcoon walking speed {speed} is not -2, 0 or 2."),
            };
            alcoon.VerticalVelocity = SignedSixteenSixteen.FromParts(CoordinateAxis.AsSigned(words.B), words.C);
            alcoon.SpawnPosition = alcoon.SpawnPosition.WithY(words.F);
        }
        if (enemies.DeadTourianCorpses[index] is { } corpse)
        {
            corpse.Phase = DeadTourianCorpsePhaseOf(corpse.Species, words.A);
            corpse.PreRotDelayCounter = words.B;
        }
        if (enemies.RioStates[index] is { } rio)
        {
            rio.SavedHorizontalVelocity = new SignedEightEight(words.A);
            rio.Action = RiosNativeWords.RioActions.Decode(words.B, "recorded RioAction");
            rio.Velocity = new(new SignedEightEight(words.D), new SignedEightEight(words.C));
            rio.AnimationFinished = words.E != 0;
        }
        if (enemy.EntityState<NorfairRioEntity, NorfairRioEnemyState>() is { } norfairRio)
        {
            norfairRio.Velocity = new(new SignedEightEight(words.B), new SignedEightEight(words.A));
            norfairRio.Action = RiosNativeWords.NorfairRioActions.Decode(words.F, "recorded NorfairRioAction");
        }
        if (enemies.WaverStates[index] is { } waver)
        {
            waver.XVelocity = SignedSixteenSixteen.FromParts(unchecked((short)words.B), words.A);
            // The record keeps the requested animation beside the installed one (#2145): Core keeps the Waver's facing,
            // which the requested animation shows, and only the playing animation, which this import does not read.
            RequirePlaying(waver.Playback, WaverAnimations.StartingWith(WaverNativeWords.Animations.Decode(words.C, "recorded Waver installed animation")), "Waver");
            // Only the phase word's low byte is ever read.
            waver.Angle = RotationAngle.FromSteps(LittleEndianBytes.LowByte(words.D));
            waver.SpinFinished = words.E != 0;
            waver.Facing = WaverAnimationChoicesConstants.Facing(WaverNativeWords.Animations.Decode(words.F, "recorded Waver requested animation"));
        }
        if (enemy.EntityState<FliesEntity, FlyEnemyState>() is { } fly)
        {
            fly.RetreatTimer = unchecked((short)words.A);
            fly.Velocity = new(new SignedEightEight(words.B), new SignedEightEight(words.C));
            fly.TargetY = words.D;
            // The phase word is an even byte offset into the 256-entry table, nine bits wide.
            fly.Angle = words.E % 2 == 0
                ? RotationAngle.FromSteps(checked((byte)(Index(words.E, 510, "fly phase") / 2)))
                : throw new InvalidDataException($"Recorded fly phase ${words.E:X4} is odd.");
            fly.Action = FliesNativeWords.FlyActions.Decode(words.F, "recorded FlyAction");
        }
        if (enemies.DragonStates[index] is { } dragon)
        {
            // Only the direction word's sign bit, the facing, is ever read.
            dragon.FacingLeft = (words.A & 0x8000) != 0;
            // One record word serves the motion timer and the shot counter in turn; whichever phase is current reads it.
            dragon.ActionTimer = unchecked((short)words.D);
            dragon.AttackCounter = unchecked((short)words.D);
            dragon.Action = DragonNativeWords.DragonActions.Decode(words.F, "recorded DragonAction");
        }
        if (enemies.ElevatorStates[index] is { } elevator)
            elevator.RestingY = words.A;
        // A Fune reads no wake distance and a Namihe no cooldown; each takes only its own species' words.
        if (enemies.FuneStates[index] is { } fune)
        {
            fune.Action = FuneNamiheNativeWords.FuneActions.Decode(words.B, "recorded FuneAction");
            if (Index(words.D, 15, "Fune/Namihe behaviour") != 0)
                throw new InvalidDataException($"Recorded Fune/Namihe behaviour ${words.D:X4} names a Namihe.");
            fune.CooldownTimer = words.E;
            fune.CooldownTime = Index(words.F, byte.MaxValue, "Fune cooldown time");
            (bool namihe, bool active, bool right) = FuneNamiheSelectorOracle.Decode(words.A);
            if (namihe)
                throw new InvalidDataException($"Recorded Fune/Namihe selector ${words.A:X4} names a Namihe.");
            fune.ShowsActiveAnimation = active;
            fune.FacingRight = right;
        }
        if (enemies.NamiheStates[index] is { } namiheState)
        {
            namiheState.Action = FuneNamiheNativeWords.NamiheActions.Decode(words.B, "recorded NamiheAction");
            namiheState.YProximity = Index(words.C, byte.MaxValue, "Namihe wake distance");
            if (Index(words.D, 15, "Fune/Namihe behaviour") == 0)
                throw new InvalidDataException($"Recorded Fune/Namihe behaviour ${words.D:X4} names a Fune.");
            (bool namihe, bool active, bool right) = FuneNamiheSelectorOracle.Decode(words.A);
            if (!namihe)
                throw new InvalidDataException($"Recorded Fune/Namihe selector ${words.A:X4} names a Fune.");
            namiheState.ShowsActiveAnimation = active;
            namiheState.FacingRight = right;
        }
        if (enemy.DefinitionId is EnemyDefinitionId.GrappleRipper or EnemyDefinitionId.Ripper or EnemyDefinitionId.JetRipper &&
            enemy.SpeciesState<RipperEnemyState>() is { } ripperVariant)
        {
            ripperVariant.XVelocity = SignedSixteenSixteen.FromParts(unchecked((short)words.D), words.C);
            ripperVariant.Speeds = SuperMetroid.AssetExtraction.Enemies.EnemySpeedTableCodec.LinearSpeedAtOffset(words.E);
        }
        if (enemies.NorfairLavaJumpingEnemyStates[index] is { } lavaJumper)
        {
            lavaJumper.YVelocity = new SignedEightEight(words.C);
            lavaJumper.SpawnPosition = new Position16<RoomPixels>(words.D, words.E);
            lavaJumper.Action = LavaJumpersNativeWords.NorfairLavaJumpingEnemyActions.Decode(words.F, "recorded NorfairLavaJumpingEnemyAction");
        }
        if (enemies.MetareeStates[index] is { } metaree)
        {
            metaree.BurrowTimer = unchecked((short)words.A);
            metaree.Action = MetareeNativeWords.MetareeActions.Decode(words.B, "recorded MetareeAction");
            RequirePlayingSkreeMetareeAnimation(metaree.Playback, SkreeNativeWords.MetareeAnimationPhases, words, "Metaree");
            metaree.AttackReady = words.E != 0;
            metaree.YVelocity = Index(words.F, 2734, "Metaree dive speed");
        }
        if (enemies.SparkStates[index] is { } spark)
        {
            spark.Action = SparksNativeWords.SparkActions.Decode(words.B, "recorded SparkAction");
            spark.BaseActionTime = words.E;
            spark.ActionTimer = unchecked((short)words.F);
        }
        if (enemies.PuyoStates[index] is { } puyo)
        {
            puyo.YSpeedCurvePlace = QuadraticSpeedCurvePlace.FromTwoHundredFiftySixthsOfMark(Index(words.A, short.MaxValue, "Puyo speed index"));
            puyo.HopCooldownTimer = unchecked((short)words.B);
            puyo.Action = PuyoNativeWords.PuyoActions.Decode(words.C, "recorded PuyoAction");
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
                segment.CurlProgress = RotationAngleConstants.FromRaw(Index(word, 0xf3fb, "Shaktool curl progress"));
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
    private static DeadTourianCorpsePhase DeadTourianCorpsePhaseOf(DeadTourianCorpseSpecies species, ushort word) =>
        new DeadTourianCorpsePhaseOfCase(species, word) switch
        {
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
    /// Requires a recorded Skree or Metaree to have installed the animation the imported one plays (#1810, #2145): the game
    /// keeps the requested phase beside the installed one and installs a request in the same AI update, and Core keeps only
    /// the playing animation, which this import does not read from the record, so a record whose two phases differ, or
    /// whose installed phase is not the playing animation, fails.
    /// </summary>
    private static void RequirePlayingSkreeMetareeAnimation(
        AnimationPlayback<SkreeMetareeSegment> playing, NativeWordIdentities<SkreeMetareeSegment> phases, NativeEnemyRecordWords words, string enemyName)
    {
        if (words.C != words.D)
            throw new InvalidDataException($"Recorded {enemyName} requests animation phase {words.C} while phase {words.D} is installed.");
        RequirePlaying(playing, SkreeMetareeAnimations.StartingWith(phases.Decode(words.D, $"recorded installed {enemyName} animation phase")), enemyName);
    }

    /// <summary>
    /// Requires the imported enemy to play the animation its record installed (#2145): Core keeps no installed animation
    /// beside its playback, which this import does not read from the record, so a record that installed another fails.
    /// </summary>
    private static void RequirePlaying<TSegment>(AnimationPlayback<TSegment> playing, Animation<TSegment> installed, string enemyName)
        where TSegment : struct
    {
        if (!ReferenceEquals(playing.Animation, installed))
            throw new InvalidDataException($"A recorded {enemyName} installed {installed.Phases[0].Segment}, which the imported {enemyName} does not play.");
    }

    /// <summary>
    /// The speed a recorded pair holds: the game stores a speed beside its negation, and Core keeps the speed alone and
    /// negates it at use (#1804), so a pair whose second value is not the first's negation fails.
    /// </summary>
    private static SignedSixteenSixteen NegatedPair(SignedSixteenSixteen speed, SignedSixteenSixteen negation, string meaning) =>
        negation == -speed ? speed : throw new InvalidDataException($"Recorded {meaning} {speed} is stored beside {negation}, not its negation.");
}
