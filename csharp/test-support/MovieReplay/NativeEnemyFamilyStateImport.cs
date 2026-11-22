using SuperMetroid.TestSupport.SamusNativeCodecs;
using SuperMetroid.Core.Animation;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Bosses;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Enemies.PipeBugs;
using SuperMetroid.Core.Enemies.Platforms;
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
using SuperMetroid.Core.Enemies.Atomic;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Evir;
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
        RejectSealedFamily(enemy);
        int index = enemy.Ordinal;
        // The cartridge gives each of Phantoon's parts' record words a meaning per part (#1476); each part's words belong to
        // the encounter's state grouped by what it does (#1933).
        if (enemy.SpeciesState<PhantoonPart>()?.Phantoon is { } phantoon)
        {
            EnemyDefinitionId part = enemy.HeldOccupant().DefinitionId;
            if (part == EnemyDefinitionId.PhantoonEye)
            {
                // The cartridge's count opens the eye on the update it reaches zero; one it ran to zero or below opens it on
                // the next.
                phantoon.Round.EyeClosedWait.SetCartridgeCount(Math.Max(0, CoordinateAxis.AsSigned(words.A)) - 1);
                phantoon.FigureEight.Direction = words.C != 0 ? PhantoonPathDirection.Backwards : PhantoonPathDirection.Forwards;
                phantoon.Death.FadeCycles = words.C;
                // The eye's word E counts the steps of whichever of his fades runs: his body's, or after his death the
                // Wrecked Ship's power coming on, each its own progress (#2251). Word D, the fade's length, is rewritten
                // by every step.
                PhantoonFadeProgress fade = phantoon.Phase == PhantoonPhase.Dead ? phantoon.ShipPowerOn : phantoon.BodyFade;
                fade.Steps = words.E;
                phantoon.BodyFade.IsFinished = words.F != 0;
                phantoon.RageWaveCount = words.F;
                PhantoonNativeWords.ImportDeathMosaic(phantoon.Death, words.C);
            }
            else if (part == EnemyDefinitionId.PhantoonTentacles)
            {
                phantoon.WindowEnd = (words.A != 0) ? PhantoonWindowEnd.Swoop : PhantoonWindowEnd.EyeCloses;
                phantoon.Round.Damage = words.B;
                phantoon.Swoop.Velocity = new(new SignedEightEight(words.C), new SignedEightEight(words.D));
                PhantoonNativeWords.ImportSwoopTarget(phantoon.Swoop, words.E);
                phantoon.Death.NextExplosion = SuperMetroid.Core.ListStep<PhantoonDeathExplosionDefinition>.At(PhantoonDeathExplosions.Schedule, words.F);
            }
            else if (part == EnemyDefinitionId.PhantoonMouth)
            {
                // The record names the burst by its place in the casual-flame update's choices, the roll that chooses it.
                phantoon.CasualFlames.Schedule = PhantoonCasualFlameDefinitions.Burst.Choose(
                    Ensure.BetweenInclusive(0, PhantoonCasualFlameDefinitions.Burst.Field.Outcomes - 1, words.A));
                // Its wait's word holds the updates the wait lasts; a word at or below zero runs out on the next update.
                phantoon.CasualFlames.Hold.SetCartridgeCount(CoordinateAxis.AsSigned(words.B) - 1);
                // The record counts the burst's flames still to come; it reads below zero while no burst is under way, which
                // the burst holds as no next flame. A count of zero spits a last flame as a count of one does.
                short casualFlames = unchecked((short)words.C);
                IReadOnlyList<PhantoonCasualFlame> flames = phantoon.CasualFlames.Schedule.Flames;
                phantoon.CasualFlames.NextFlame = casualFlames < 0
                    ? null
                    : SuperMetroid.Core.ListStep<PhantoonCasualFlame>.At(flames, flames.Count - Math.Max(1, (int)casualFlames));
                phantoon.Wave.Amplitude = UnsignedSixteenSixteen.FromRaw((uint)words.D << 8);
                phantoon.Wave.AmplitudeShrinking = words.E != 0;
                phantoon.Wave.PhaseStep = RotationAngle.WrappedSteps(words.F);
            }
            else if (part == EnemyDefinitionId.PhantoonBody)
            {
                // Word A is the place of the walk his phase is on: the intro's starting flames while they spawn, his
                // figure-eight path while he runs it, and his death explosions' passes once he is dying. Only the starting
                // flames' spawning reads the intro's place.
                if (words.F != 0 && PhantoonNativeWords.Phases.Decode(words.F, "recorded PhantoonPhase") == PhantoonPhase.SpawnStartingFlames)
                    phantoon.StartingFlames.NextFlame = SuperMetroid.Core.ListStep<TrigonometryAngle>.At(PhantoonFlameSpawns.StartingFlameStarts, words.A);
                phantoon.FigureEight.PathPlace = PhantoonPath.At(words.A);
                phantoon.Death.ExplosionPasses = words.A;
                phantoon.FigureEight.Speed = new global::SuperMetroid.Core.Bosses.Phantoon.PhantoonPathSpeed(SignedSixteenSixteen.FromParts(CoordinateAxis.AsSigned(words.C), words.B));
                phantoon.FigureEight.Stage = PhantoonFigureEightStageWords.Decode(words.D);
                if (words.F != 0)
                    phantoon.Phase = PhantoonNativeWords.Phases.Decode(words.F, "recorded PhantoonPhase");
                // The cartridge's count ends a timed phase on the update it reaches zero, and one it ran to zero or below on
                // the next; once he is dead, the power comes on from the update after the count is used up (#2538).
                int phaseCount = Math.Max(0, CoordinateAxis.AsSigned(words.E));
                phantoon.PhaseWait.SetCartridgeCount(
                    phantoon.Phase == PhantoonPhase.Dead ? phaseCount : phaseCount - 1);
            }
            else
            {
                throw new InvalidDataException($"Enemy {part} holds Phantoon's state but is not one of his parts.");
            }
        }
        if (enemy.SpeciesState<KraidPartState>() is { } kraidPartRecord)
            ImportKraidPart(enemy, kraidPartRecord, words);
        if (enemy.SpeciesState<GunshipState>() is { } gunshipRecord)
        {
            // The cartridge keeps the ship's phase timer, bob phase, bob timer, and base Y and landing-brake step in the
            // top hull's variables A and C through E. Variable B counts the records the takeoff has copied while the ship
            // waits for its engines, which the port times on its phase timer from the wait's start (#2469).
            gunshipRecord.PhaseTimer = UpdateCountdownAccess.Recorded(unchecked((short)words.A));
            gunshipRecord.Bob = IdleBobAt(words.C);
            gunshipRecord.BobTimer = UpdateCountdownAccess.Recorded(unchecked((short)words.D));
            gunshipRecord.BaseY = words.E;
            // E holds the next landing brake only while the ship brakes; otherwise it holds the base Y.
            if (words.E < GunshipMotionDefinitions.LandingBrakeYDeltas.Length)
                gunshipRecord.LandingBrake = LandingBrakeAt(words.E);
        }
        if (enemy.Holds(EnemyDefinitionId.ShipBottomEntrance) && index > 0 &&
            enemies.Places.InUpdateOrder[index - 1].SpeciesState<GunshipState>() is { } bottomOfShip)
        {
            // The bottom hull, listed right after the top, keeps the ship's engine-sound timer in variable D, its
            // rumble frame in E and its escape speed in F.
            bottomOfShip.EngineSoundTimer = UpdateCountdown.For(ZeroOrBelowDuration(words.D));
            bottomOfShip.Rumble.PlaceAt(UpdateCountdownAccess.AtAge(GunshipTakeoffDefinitions.EngineRumbleFrames,
                Math.Min((int)words.E, GunshipTakeoffDefinitions.EngineRumbleFrames)));
            bottomOfShip.EscapeVelocity = new SignedEightEight(words.F);
        }
        if (enemy.SpeciesState<DeadTorizoEnemyState>() is { } deadTorizoRecord)
        {
            deadTorizoRecord.Phase = CorpsesNativeWords.DeadTorizoPhases.Decode(words.A, "recorded dead Torizo CorpsePhase");
            // The corpse only sinks: its recorded sideways speed is nothing (#2432).
            if (words.B != 0)
                throw new InvalidDataException($"Recorded dead Torizo sideways speed ${words.B:X4} is not nothing.");
            deadTorizoRecord.FallSpeed = new SignedEightEight(words.C);
        }
        if (enemy.SpeciesState<RidleyExplosionFragmentState>() is { } ridleyFragmentRecord)
        {
            ridleyFragmentRecord.Velocity = new(new SignedEightEight(words.B), new SignedEightEight(words.C));
            ridleyFragmentRecord.Lifetime.SetCartridgeCount(CoordinateAxis.AsSigned(words.F));
        }
        if (enemy.SpeciesState<DraygonEnemyState>() is { } draygonRecord)
        {
            // A zero word is a fight that has not begun; the state keeps the phase it starts in.
            if (words.A != 0)
            {
                // The snapshot import enters the step the phase stands for before this import (DraygonStepImport).
                (_, HorizontalDirection? pass) = DraygonNativeWords.Phases.Decode(words.A, "recorded DraygonPhase");
                // Only a swoop or goop pass names the way it travels (#2165).
                if (pass is { } passDirection)
                    draygonRecord.PassDirection = passDirection;
            }
            switch (DraygonNativeWords.MeaningOfSharedValue(draygonRecord.Phase))
            {
                case DraygonNativeWords.SharedValue.IntroFrames:
                    draygonRecord.IntroStage = UpdateCountdownAccess.AtAge(draygonRecord.Phase == DraygonPhase.IntroDance
                        ? DraygonIntroDanceDefinitions.DurationFrames
                        : DraygonArenaDefinitions.IntroUpdates, words.B);
                    break;
                case DraygonNativeWords.SharedValue.BurialWait:
                    // The cartridge's burial wait holds the updates left, the one he starts sinking on included.
                    draygonRecord.BurialWait.SetCartridgeCount(words.B - 1);
                    break;
                case DraygonNativeWords.SharedValue.SwoopEntry:
                    // The climb counts its entries up from the first; every other phase counts them down.
                    draygonRecord.SwoopHalf = draygonRecord.Phase == DraygonPhase.SwoopAscending
                        ? UpdateCountdownAccess.AtAge(words.C / DraygonNativeWords.SwoopEntryBytes, words.B / DraygonNativeWords.SwoopEntryBytes)
                        : UpdateCountdown.For(words.B / DraygonNativeWords.SwoopEntryBytes);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined Draygon shared value meaning for {draygonRecord.Phase}.");
            }
            // The swoop path's length (word C) is read with the path's heights from the cartridge's table by the snapshot
            // import, which holds the memory the table lies in.
            // The cartridge keeps the pass's speed; the port's velocity takes it toward the pass's side.
            draygonRecord.SwoopVelocity = UnsignedSixteenSixteen.FromRaw(SixteenSixteenWords.Compose(words.D, words.E)).AsDisplacement()
                .Toward(draygonRecord.PassDirection);
        }
        // Draygon's encounter keeps his eye's tracking (#2298).
        if (enemy.Entity is DraygonEyeEntity { Encounter.EyeTracking: var draygonEyeRecord })
        {
            // A zero word is a setup that never ran this far (a defeated room's deleted boss); the state keeps its start.
            if (words.A != 0)
            {
                (DraygonEyeAction eyeAction, HorizontalDirection? eyeFacing) = DraygonNativeWords.EyeActions.Decode(words.A, "recorded DraygonEyeAction");
                // Only a tracking eye names the way it faces (#2165); an inert eye faces no way (#2560).
                draygonEyeRecord.TrackingFacing = eyeAction switch
                {
                    DraygonEyeAction.Inert => null,
                    DraygonEyeAction.Tracking => eyeFacing ?? throw new InvalidDataException("A recorded tracking Draygon eye names no facing."),
                    _ => throw new InvalidDataException($"Undefined {nameof(DraygonEyeAction)} {eyeAction}."),
                };
            }
            // The game compares only the look angle's low byte, the bearing it stored.
            draygonEyeRecord.LookAngle = AimAngleConstants.FromClockwiseSteps(words.F & byte.MaxValue);
        }
        if (enemy.SpeciesState<CrocomireEnemyState>() is { } crocomireRecord)
        {
            crocomireRecord.DeathStage = CrocomireNativeWords.DeathStages.Decode(words.A, "recorded Crocomire death stage");
            if (CrocomireNativeWords.DamageReactionPending(words.B))
                crocomireRecord.StepBack.Ask();
            else
                crocomireRecord.StepBack.Forget();
            // The fight table's first entry, which no live fight selects, stands for a defeated room's corpse, which keeps its
            // constructed phase and never runs it (#1939).
            if (words.C != 0)
                crocomireRecord.Phase = CrocomireNativeWords.Phases.Decode(words.C, "recorded Crocomire fight function");
            // The cartridge keeps several of the body's values in each of words D, E and F; the phase says which.
            CrocomireNativeWords.ImportSharedWords(crocomireRecord, words.D, words.E, words.F);
        }
        if (enemy.SpeciesState<DeadSidehopperEnemyState>() is { } deadSidehopperRecord)
        {
            deadSidehopperRecord.Action = CorpsesNativeWords.DeadSidehopperActions.Decode(words.A, "recorded DeadSidehopperAction");
            deadSidehopperRecord.PreRotDelay.PlaceAt(UpdateCountdownAccess.AtAge(CorpseRotScheduleDefinitions.PreRotDelayUpdates,
                Math.Min((int)words.B, CorpseRotScheduleDefinitions.PreRotDelayUpdates)));
            deadSidehopperRecord.PostLandingPause = TrueWait(words.F);
        }
        if (enemy.SpeciesState<BabyMetroidEnemyState>() is { } babyMetroidRecord)
        {
            // A zero word is a setup that never ran this far (a defeated room's deleted boss); the state keeps its start.
            if (words.A != 0)
            {
                (BabyMetroidAction babyMetroidAction, HorizontalDirection? side) = MotherBrainNativeWords.BabyMetroidActions.Decode(words.A, "recorded ShitroidAction");
                babyMetroidRecord.Action = babyMetroidAction;
                // Only the departing flight names the side of Samus it flies to (#2165).
                if (side is { } departureSide)
                    babyMetroidRecord.DepartureSide = departureSide;
            }
            babyMetroidRecord.Velocity = new(new SignedEightEight(words.B), new SignedEightEight(words.C));
            // Word D holds the membrane's phase and the updates before it moves on, less one; word E its pace, as the
            // updates each phase holds less one (#2404).
            babyMetroidRecord.ResumeMembrane(MotherBrainNativeWords.BabyMetroidMembranePaces.Decode(words.E, "recorded Shitroid membrane pace"),
                BabyMetroidMembranePulse.All[LittleEndianBytes.LowByte(words.D)], LittleEndianBytes.HighByte(words.D) + 1);
            // The cartridge keeps the Sidehopper drain's updates in the same word as every other wait's count.
            if (babyMetroidRecord.Action == BabyMetroidAction.DrainSidehopper)
                babyMetroidRecord.SidehopperDrain.SetCartridgeCount(words.F - 1);
            else
                babyMetroidRecord.StateTimer.SetCartridgeCount(CoordinateAxis.AsSigned(words.F));
        }
        if (enemy.SpeciesState<DachoraEchoState>() is { } dachoraEcho)
        {
            // An echo's record names its flicker routine and keeps its lifetime in variable D. The first echo's variable A
            // holds its body's trail timer, which Core keeps on the body listed right before it (#2147).
            if (words.F != AnimalsNativeWords.DachoraEchoRoutine)
                throw new InvalidDataException($"Recorded Dachora echo {enemy.Ordinal} runs routine {words.F:X4}, not the echo's.");
            dachoraEcho.Lifetime = UpdateCountdownAccess.Recorded(unchecked((short)words.D));
            if ((enemy.Ordinal > 0 ? enemies.Places.PlacesBefore(enemy)(1) : null)?.SpeciesState<DachoraEnemyState>() is { } echoBody)
                echoBody.TrailTimer = UpdateCountdownAccess.Recorded(unchecked((short)words.A));
        }
        if (enemy.SpeciesState<DachoraEnemyState>() is { } dachoraRecord)
        {
            dachoraRecord.SetGlowCycle(SuperMetroid.Core.Compatibility.DachoraGlowCycle.At(LittleEndianBytes.HighByte(words.E), LittleEndianBytes.LowByte(words.E)));
            dachoraRecord.Action = AnimalsNativeWords.DachoraActions.Decode(words.F, "recorded DachoraAction");
            // The cartridge keeps the speed's whole pixels and the countdowns in variable A and the acceleration's fraction
            // in variable D; each is read here as the meaning the recorded phase gives it.
            if (dachoraRecord.Action is DachoraAction.BlinkingBeforeRun or DachoraAction.ChargingShinespark)
            {
                dachoraRecord.PhaseTimer = UpdateCountdownAccess.Recorded(unchecked((short)words.A));
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
            brinstarPipeBug.Flight = CoordinateAxis.AsSigned(words.A) < 0 ? HorizontalDirection.Left : HorizontalDirection.Right;
        }
        if (enemy.SpeciesState<NorfairPipeBugState>() is { } norfairPipeBug)
        {
            // A member's stagger routine names its role's offset (#2071); every other routine names an action.
            if (PipeBugsNativeWords.FormationStaggers.TryDecode(words.A, out NorfairPipeBugRole? staggerRole) &&
                staggerRole.PostRiseAction == NorfairPipeBugAction.Stagger)
            {
                norfairPipeBug.Action = NorfairPipeBugAction.Stagger;
                norfairPipeBug.Role = staggerRole;
            }
            else if (PipeBugsNativeWords.NorfairPipeBugFlights.TryDecode(words.A, out HorizontalDirection flight))
            {
                // Each way a member flies has a routine of its own (#2165).
                norfairPipeBug.Action = NorfairPipeBugAction.Fly;
                norfairPipeBug.FlightDirection = flight;
            }
            else
            {
                norfairPipeBug.Action = PipeBugsNativeWords.NorfairPipeBugActions.Decode(words.A, "recorded NorfairPipeBugAction");
            }
        }
        if (enemy.SpeciesState<MagdolliteHeadState>() is { } magdolliteHead)
            magdolliteHead.Action = MagdolliteNativeWords.MagdolliteHeadActions.Decode(words.F, "recorded MagdolliteHeadAction");
        if (enemy.SpeciesState<MagdolliteBodyState>() is { } magdolliteBody)
        {
            magdolliteBody.Action = MagdolliteNativeWords.MagdolliteBodyActions.Decode(words.F, "recorded MagdolliteBodyAction");
            // The recorded word counts the arm's stages up from its setup, two to a stage.
            MagdolliteRiseStage stage = MagdolliteRiseStage.Setup;
            for (int place = 0; place < words.B / 2; place++)
                stage = stage.Higher;
            magdolliteBody.BodyPhase = stage;
            magdolliteBody.RiseDisplacementY = unchecked((short)words.E);
        }
        if (enemy.SpeciesState<MagdolliteOverlayState>() is { } magdolliteOverlay)
        {
            magdolliteOverlay.Action = MagdolliteNativeWords.MagdolliteOverlayActions.Decode(words.F, "recorded MagdolliteOverlayAction");
            magdolliteOverlay.ThrowDirection = words.B != 0 ? HorizontalDirection.Right : HorizontalDirection.Left;
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
            enemies.ZebetiteBehavior.SharedPulse = ListStep<ZebetitePulse>.At(ZebetitePulse.All, words.C);
        if (enemy.SpeciesState<VerticalShutterEnemyState>() is { } verticalShutterRecord)
        {
            ShutterSpawnSettings verticalShutterSettings = enemy.HeldOccupant().SpawnSettings<ShutterSpawnSettings>();
            verticalShutterRecord.Action = ShuttersNativeWords.DecodeVerticalShutterAction(words.A, verticalShutterSettings.TriggerMode);
            verticalShutterRecord.ActionTimer = WrappingCountdown.Start(words.B);
            // The cartridge rests at an end in the same word as the first wait; the port keeps the rest's own duration.
            if (verticalShutterRecord.Action is VerticalShutterAction.StoppedAfterMovingUp or VerticalShutterAction.StoppedAfterMovingDown)
                verticalShutterRecord.RestTimer = TrueWait(words.B);
            // The upward words are the speed reversed (#2278): the speed is the downward pair. The shutter moves at its
            // placement's speed (#2450), which the recording's must be.
            SignedSixteenSixteen verticalShutterSpeed = SignedSixteenSixteen.FromParts(unchecked((short)words.D), words.C);
            if (verticalShutterSpeed != verticalShutterSettings.Speed)
                throw new InvalidDataException($"Recorded vertical shutter speed {verticalShutterSpeed} is not its placement's {verticalShutterSettings.Speed}.");
        }
        // A Multiviola's velocity words (C to F) are the last update's, which every update rebuilds from its heading before it
        // moves (#2531), so none is imported.
        if (enemy.EntityState<CrawlerEntity, CrawlerEnemyState>() is { } crawler)
        {
            crawler.Velocity = new(new SignedEightEight(words.A), new SignedEightEight(words.B));
            crawler.Action = (enemy.Entity is OrangeZoomerEntity ? CrawlersNativeWords.OrangeZoomerActions : CrawlersNativeWords.SurfaceCrawlerActions)
                .Decode(words.F, "recorded CrawlerAction");
        }
        if (enemy.Holds(EnemyDefinitionId.Holtz) && enemy.SpeciesState<HoltzEnemyState>() is { } holtz)
        {
            holtz.Velocity = new(new SignedEightEight(words.D), new SignedEightEight(words.C));
            holtz.Action = RiosNativeWords.HoltzActions.Decode(words.F, "recorded LowerNorfairRioAction");
        }
        // The halves reuse the same words for different values, each half into its own state (#1845).
        if (enemy.Holds(EnemyDefinitionId.Powamp) && enemy.SpeciesState<PowampBalloonState>() is { } powampBalloon)
        {
            powampBalloon.Spawn = new(words.A, words.B);
            // The balloon reads its grappled rise from its spawn settings (#2439); the recording's copy must be the placement's.
            if (words.D != enemy.HeldOccupant().SpawnSettings<PowampBalloonSpawnSettings>().GrappleTravelDistance)
                throw new InvalidDataException($"Recorded Powamp grapple rise {words.D} is not its placement's {enemy.HeldOccupant().SpawnSettings<PowampBalloonSpawnSettings>().GrappleTravelDistance}.");
            if (words.F != PowampNativeWords.BalloonIdleWord)
                throw new InvalidDataException($"Recorded Powamp balloon runs ${words.F:X4}, not the balloon's empty routine.");
        }
        if (enemy.Holds(EnemyDefinitionId.MamaTurtle) && enemy.SpeciesState<MamaTurtleEnemyState>() is { } mama)
        {
            MamaTurtleAction mamaAction = MamaTurtlesNativeWords.MamaTurtleActions.Decode(words.A, "recorded MamaTurtleAction");
            // Only the horizontal velocity's whole pixels live in the record; its fraction and the vertical velocity do not.
            mama.Velocity = mama.Velocity.WithX(mama.Velocity.X.WithWholePixels(unchecked((short)words.E)));
            // The game keeps a wake count of one that a baby's touch or shot clears, beside her routine; a cleared count before
            // her first update or in her shell is her woken phase (#2439). She leaves her shell only once woken.
            mama.Action = (mamaAction, words.F) switch
            {
                (MamaTurtleAction.Placed, 0) => MamaTurtleAction.PlacedAndWoken,
                (MamaTurtleAction.Asleep, 0) => MamaTurtleAction.Woken,
                (MamaTurtleAction.Placed or MamaTurtleAction.Asleep, 1) => mamaAction,
                (_, 0) => mamaAction,
                (_, 1) => throw new InvalidDataException($"Recorded Mama Turtle {mamaAction} has not been woken."),
                _ => throw new InvalidDataException($"Recorded Mama Turtle wake count {words.F} is neither zero nor one."),
            };
        }
        if (enemy.EntityState<GerutaEntity, GerutaEnemyState>() is { } geruta)
        {
            geruta.Velocity = new(new SignedEightEight(words.B), new SignedEightEight(words.A));
            geruta.Action = RiosNativeWords.GerutaActions.Decode(words.F, "recorded NorfairRioAction");
        }
        if (enemies.DragonStates[index] is { } dragon)
        {
            // Only the direction word's sign bit, the facing, is ever read.
            dragon.Facing = (words.A & 0x8000) != 0 ? HorizontalDirection.Left : HorizontalDirection.Right;
            // One record word serves the motion timer and the shot counter in turn; whichever phase is current reads it.
            dragon.Action = DragonNativeWords.DragonActions.Decode(words.F, "recorded DragonAction");
            // The wait before sinking runs out at zero in the cartridge too; its other waits run past zero.
            dragon.ActionTimer = dragon.Action == DragonAction.WaitToSink ? UpdateCountdownAccess.Recorded(unchecked((short)words.D)) : TrueWait(words.D);
            dragon.AttackCounter = unchecked((short)words.D);
        }
        if (enemy.Occupant?.DefinitionId is EnemyDefinitionId.GrappleRipper or EnemyDefinitionId.Ripper or EnemyDefinitionId.JetRipper &&
            enemy.SpeciesState<RipperEnemyState>() is { } ripperVariant)
        {
            ripperVariant.Heading = unchecked((short)words.D) < 0 ? HorizontalDirection.Left : HorizontalDirection.Right;
            // The Ripper flies its placement's speed (#2450), which the recording's must be.
            SignedSixteenSixteen recordedRipperSpeed = SuperMetroid.AssetExtraction.Enemies.EnemySpeedTableCodec.LinearSpeedAtOffset(words.E);
            // A grapple Ripper's placement keeps its patrol bounds beside its speed (#2512).
            SignedSixteenSixteen placementRipperSpeed = enemy.Holds(EnemyDefinitionId.GrappleRipper)
                ? enemy.HeldOccupant().SpawnSettings<GrappleRipperSpawnSettings>().Speed
                : enemy.HeldOccupant().SpawnSettings<RipperSpawnSettings>().Speed;
            SignedSixteenSixteen ripperRightward = RipperBehavior.FlyingVelocity(placementRipperSpeed, HorizontalDirection.Right);
            if (recordedRipperSpeed != ripperRightward)
                throw new InvalidDataException($"Recorded Ripper speed {recordedRipperSpeed} is not its placement's {ripperRightward}.");
        }
        if (enemies.SqueeptStates[index] is { } lavaJumper)
        {
            lavaJumper.YVelocity = new SignedEightEight(words.C);
            lavaJumper.SpawnPosition = new Position16<RoomPixels>(words.D, words.E);
            lavaJumper.Action = LavaJumpersNativeWords.SqueeptActions.Decode(words.F, "recorded NorfairLavaJumpingEnemyAction");
        }
    }

    /// <summary>
    /// Refuses a recorded enemy whose family keeps its state inside its entity alone (#2554): the game hands that state to
    /// nothing outside the enemy, so its recorded words have nowhere to go, and a snapshot holding it cannot be imported.
    /// </summary>
    /// <exception cref="InvalidDataException">The enemy is of such a family.</exception>
    /// <param name="enemy">The imported slot.</param>
    private static void RejectSealedFamily(RoomEnemyPlace enemy)
    {
        if (enemy.Entity is CeresDoorEntity or CeresElevatorDoorEntity or CeresElevatorRoomWallEntity or CeresGetawayChamberWallEntity
            or CeresRidleyRoomDoorEntity or CeresSteamEntity or AtomicEntity or ElevatorEntity or EscapeDachoraEntity or DeadTourianEntity
            or HibashiCollisionPartEntity or HibashiEntity or RioEntity or GerutaFollowerEntity or HoltzFollowerEntity or EvirArmsEntity
            or GunshipBottomEntity or BlueBrinstarFaceBlockEntity or MultiviolaEntity or PolypEntity or FliesEntity or FirefleaEntity
            or FuneEntity or NamiheEntity or BeetomEntity or BullEntity or BoulderEntity or BoyonEntity or CacatacEntity or ChootEntity
            or DragonWingEntity or KagoEntity or KzanTopEntity or NorfairLavaJumperFollowerEntity or MetareeEntity
            or SkreeEntity or MochtroidEntity or OumEntity or WaverEntity or SbugEntity or SparkEntity or SkulteraEntity
            or UnpoweredWorkRobotEntity or PoweredWorkRobotEntity or ZoaEntity or PlatformsEntity or NuclearWaffleEntity or PuyoEntity
            or BotwoonEntity or FakeKraidEntity or MotherBrainTubesEntity or RinkaEntity
            or EscapeEtecoonEntity or EtecoonEntity or ChozoStatueEntity or HoppersEntity or KiHunterBodyEntity or BabyTurtleEntity
            or MetroidEntity or MorphBallEyeEntity or MorphBallEyeMountEntity or OwtchEntity or YellowPipeBugEntity or PowampBodyEntity
            or GrowingShutterEntity or NinjaSpacePirateEntity or WalkingSpacePirateEntity or WallSpacePirateEntity
            or WreckedShipGhostEntity or YardEntity or AlcoonEntity)
        {
            throw new InvalidDataException(
                $"Recorded enemy {enemy.Ordinal} is a {enemy.HeldOccupant().DefinitionId}, whose state its entity keeps to itself; a native snapshot holding one cannot be imported.");
        }
    }

    /// <summary>
    /// Imports one of Kraid's parts' record (#2208): word A names the phase the part runs, of its own part's type, and
    /// for the foot's walk the way it walks (#2165); words B and C hold the head's hold on its pose, a lint's travel and X
    /// offset, or a nail's velocity; word F is the updates left of the part's phase wait, or the ceiling rocks' cursor
    /// while his growth breaks the ceiling. A part whose setup has not given it a phase records no word A and keeps the
    /// phase it starts in.
    /// </summary>
    private static void ImportKraidPart(RoomEnemyPlace enemy, KraidPartState part, NativeEnemyRecordWords words)
    {
        bool recorded = words.A != 0;
        switch (part)
        {
            case KraidBodyState body:
            {
                // The cartridge holds the head's pose while its count is above zero, and holds at zero.
                body.Kraid.HeadHold.SetUpdatesLeft(words.C);
                bool deathWait = KraidNativeWords.DeathWaitUpdatesLeft.TryGetValue(words.A, out int deathWaitLeft);
                if (deathWait)
                    body.SunkPhase = KraidSunkPhase.WaitBeforeFadeIn;
                else if (recorded)
                    KraidNativeWords.ImportBodyPhase(body, words.A);
                // Word F is the ceiling rocks' byte cursor, two per rock, while Kraid's growth breaks the ceiling.
                if (body.Stage == KraidBodyStage.Growing && body.GrowthPhase == KraidGrowthPhase.BreakCeilingPlatforms)
                {
                    // A cursor just past the last rock names none: every rock has fallen. One further is rejected.
                    int rock = words.F / sizeof(ushort);
                    body.NextCeilingRock = rock == KraidCeilingRockDefinitions.Rocks.Count
                        ? null
                        : ListStep<KraidCeilingRock>.At(KraidCeilingRockDefinitions.Rocks, rock);
                }
                else
                    ImportPhaseWait(body, deathWait ? deathWaitLeft : words.F);
                return;
            }
            case KraidArmState arm:
                // The arm runs no phase (#2208): its record names the bank's bare return, as a resting part's does.
                if (recorded && words.A != KraidNativeWords.RestingWord)
                    throw new InvalidDataException($"Kraid's arm, enemy {enemy.Ordinal}, runs routine {words.A:X4}, not the resting one.");
                ImportPhaseWait(arm, words.F);
                return;
            case KraidLintState lint:
                if (recorded)
                    lint.Phase = KraidNativeWords.LintPhases.Decode(words.A, "recorded KraidLintPhase");
                int emergesAt = CoordinateAxis.AsSigned(words.C);
                lint.Emergence = KraidArenaDefinitions.LintEmergenceXOffsets.Single(pair => pair.Value == emergesAt).Key;
                lint.LintTravel = words.B;
                ImportPhaseWait(lint, words.F);
                return;
            case KraidFootState foot:
            {
                // A foot whose setup has not given it a phase rests.
                // The foot's walk names the way it walks in its phase (#2165, #2560).
                foot.Phase = recorded
                    ? KraidNativeWords.FootPhases.Decode(words.A, "recorded KraidFootPhase").Phase
                    : KraidFootPhase.Resting;
                ImportPhaseWait(foot, words.F);
                return;
            }
            case KraidNailState nail:
                if (recorded)
                    nail.Phase = KraidNativeWords.NailPhases.Decode(words.A, "recorded KraidNailPhase");
                nail.NailVelocity = FixedVector<RoomPixels>.FromStoredParts(words.C, words.B, words.E, words.D);
                ImportPhaseWait(nail, words.F);
                return;
            default:
                throw new InvalidDataException($"Enemy {enemy.Ordinal} holds a Kraid part state of no part, {part.GetType().Name}.");
        }
    }

    /// <summary>
    /// Sets a Kraid part's phase wait from the cartridge's count of its updates left, the one its phase moves on on
    /// included (#2538).
    /// </summary>
    private static void ImportPhaseWait(KraidPartState part, int updatesLeft) =>
        part.PhaseWait.SetCartridgeCount(updatesLeft - 1);

    /// <summary>
    /// Brings the Shaktool chain the room's first enemy leads to the recorded target <paramref name="word"/> as the game
    /// sets one (#2554), before the segments' own records are imported. Only a wall the trailing saw strikes sets the
    /// chain's target: a quarter turn on, the chain's way round, from the trailing saw's bearing from the leading saw once
    /// the wall has turned the chain end for end. A zero target is the one the chain's placement leaves before its first
    /// wall. Any other is reached by standing the trailing saw where its bearing from the leading saw leads to that target
    /// and running its move into solid terrain, which reverses the chain as the game does; the segments' recorded places,
    /// clocks and states are then imported over what the reversal left in them.
    /// </summary>
    /// <param name="enemies">The room's enemies, the chain's segments as the room load placed them.</param>
    /// <param name="word">The leading saw's recorded target word.</param>
    /// <exception cref="InvalidDataException">
    /// The target is not a whole step, which no wall leaves; the chain's placement does not turn it end for end at its
    /// first wall; or no bearing within the room's reach leads to the target.
    /// </exception>
    internal static void ImportShaktoolTarget(RoomEnemySystem enemies, ushort word)
    {
        RotationAngle target = RotationAngle.NormalizeRaw(word);
        if (target == RotationAngle.Zero)
            return;
        if (target != target.WholeStep)
            throw new InvalidDataException($"Recorded Shaktool target ${word:X4} is not a whole step, which no wall leaves.");
        IReadOnlyList<RoomEnemyPlace> places = Ensure.NotNull(enemies).Places.InUpdateOrder;
        RoomEnemyPlace leadingSaw = places[0];
        ShaktoolChainMotion motion = leadingSaw.HeldOccupant().SpawnSettings<ShaktoolSpawnSettings>().Motion;
        if (motion.NextWall != ShaktoolWallResponse.TurnEndForEnd)
            throw new InvalidDataException($"The Shaktool chain is placed with its first wall reversing its orbit ({motion}), where the import reaches its target through a wall that turns it end for end.");
        RoomEnemyPlace trailingSaw = places.Single(place => place.Holds(EnemyDefinitionId.Shaktool) &&
            place.HeldOccupant().SpawnSettings<ShaktoolSpawnSettings>().Segment == ShaktoolSegmentDefinitions.TrailingSaw);
        // The wall turns the chain end for end, so the saws trade places: the bearing is the leading saw's place seen from
        // where the trailing saw stood, measured clockwise from straight up, and the orbit keeps its way round.
        RotationAngle bearing = motion.OrbitDirection == RotationDirection.Clockwise
            ? target - RotationAngle.QuarterTurn
            : target + RotationAngle.QuarterTurn;
        TrigonometryAngle aim = TrigonometryAngle.QuarterTurn - bearing.ToTrigonometryAngle();
        Offset16<RoomPixels> fromTrailingSaw = BearingOffset(aim)
            ?? throw new InvalidDataException($"No offset within a block's reach of the leading saw aims at {aim}, the bearing recorded Shaktool target ${word:X4} follows.");
        Position16<RoomPixels> leading = leadingSaw.Position.Whole;
        var stands = new Position16<RoomPixels>(unchecked((ushort)(leading.X - fromTrailingSaw.X)), unchecked((ushort)(leading.Y - fromTrailingSaw.Y)));
        trailingSaw.Position = trailingSaw.Position.WithWhole(stands);
        int columns = (Math.Max(leading.X, stands.X) >> 4) + 2, rows = (Math.Max(leading.Y, stands.Y) >> 4) + 2;
        trailingSaw.RunMainAi(PlacedEnemyAccess.AiFrame(RoomEnemyFrameFixtures.StandaloneSamus(),
            level: PlacedEnemyAccess.SolidRoom(new(columns, rows))));
        // A wall that turns the chain end for end puts the trailing saw back where it stood and trades the saws' places.
        if (leadingSaw.Position.Whole != stands || trailingSaw.Position.Whole != leading)
            throw new InvalidOperationException("The Shaktool trailing saw's move through solid terrain did not turn its chain end for end.");
    }

    /// <summary>
    /// Translates the recorded record words of the Shaktool segment in <paramref name="enemy"/> into its state in
    /// <paramref name="chain"/>, the chain the import holds (<see cref="ShaktoolChainHolding"/>): its orbit, its angular speed,
    /// the centre's heading or the trailing saw's curl, and its motion. Every segment's record holds a copy of the chain's
    /// target; the leading saw's, which no straightening turns on, is the chain's (#2452), which the chain reached through
    /// its wall before the segments' import (<see cref="ImportShaktoolTarget"/>).
    /// </summary>
    /// <param name="chain">The chain the import holds, whose segments the room's first seven places hold.</param>
    /// <param name="enemy">The imported segment's place.</param>
    /// <param name="words">The segment's six recorded record words.</param>
    /// <exception cref="InvalidDataException">A word lies outside what the segment's own updates leave.</exception>
    internal static void ImportShaktoolSegment(ShaktoolChain chain, RoomEnemyPlace enemy, NativeEnemyRecordWords words)
    {
        ShaktoolSegmentDefinition definition = Ensure.NotNull(enemy).HeldOccupant().SpawnSettings<ShaktoolSpawnSettings>().Segment;
        ShaktoolSegmentState segment = Ensure.NotNull(chain).StateOf(definition);
        segment.OrbitAngle = RotationAngleConstants.FromRaw(words.B);
        segment.AngularVelocity = RotationAngleConstants.FromRaw(Index(words.C, 1020, "Shaktool angular velocity"));
        ImportShaktoolOrientation(segment, Array.IndexOf(ShaktoolSegmentDefinitions.Segments, definition), words.D);
        // Every segment's owner is the chain's leading saw, the first enemy its room lists.
        if (SlotIndex(words.E, "Shaktool owner") != 0)
            throw new InvalidDataException($"Recorded Shaktool owner ${words.E:X4} does not name the first enemy, where the chain's saw is placed.");
        segment.Motion = ShaktoolNativeWords.DecodeMotion(words.F, "recorded ShaktoolSegmentMotion");
    }

    /// <summary>The nearest offset, searched outward ring by ring, whose aim the enemies' aim calculation reads as <paramref name="aim"/>.</summary>
    private static Offset16<RoomPixels>? BearingOffset(TrigonometryAngle aim)
    {
        for (int reach = 1; reach < 128; reach++)
        {
            for (int y = -reach; y <= reach; y++)
            {
                for (int x = -reach; x <= reach; x++)
                {
                    if (Math.Max(Math.Abs(x), Math.Abs(y)) != reach)
                        continue;
                    var offset = new Offset16<RoomPixels>(x, y);
                    if (EnemyMath.CalculateAimAngle(offset) == aim)
                        return offset;
                }
            }
        }
        return null;
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
                segment.Orientation = AimAngleConstants.FromClockwiseSteps(bucket).DirectionOfEighth();
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

    /// <summary>Decodes Fake Kraid's recorded signed four-pixel step into the direction it points (#2214).</summary>
    /// <param name="word">The recorded step: four pixels right, or four left.</param>
    /// <param name="meaning">What the word holds, for the failure message.</param>
    private static HorizontalDirection FakeKraidNativeDirection(ushort word, string meaning) => unchecked((short)word) switch
    {
        4 => HorizontalDirection.Right,
        -4 => HorizontalDirection.Left,
        _ => throw new InvalidDataException($"Recorded Fake Kraid {meaning} ${word:X4} is neither four pixels right nor left."),
    };

    /// <summary>
    /// A cartridge countdown that runs out on the step that takes it below zero, as the port's true duration (#2281): the
    /// updates it has left, the one it runs out on included. A count the cartridge already ran below zero has none left.
    /// </summary>
    /// <param name="word">The recorded countdown, read as signed.</param>
    private static int TrueDuration(ushort word) => Math.Max(0, unchecked((short)word) + 1);

    /// <summary>The idle bob's step at <paramref name="place"/> in its cycle, the place the cartridge records.</summary>
    /// <exception cref="InvalidDataException">The cycle has no such place.</exception>
    private static GunshipIdleBob IdleBobAt(int place)
    {
        if ((uint)place >= (uint)GunshipMotionDefinitions.IdleBobs.Length)
            throw new InvalidDataException($"The gunship's idle bob has no step {place}.");
        GunshipIdleBob bob = GunshipIdleBob.First;
        for (int step = 0; step < place; step++)
            bob = bob.Next;
        return bob;
    }

    /// <summary>The landing brake at <paramref name="place"/> in its schedule, the place the cartridge records.</summary>
    /// <exception cref="InvalidDataException">The schedule has no such place.</exception>
    private static GunshipLandingBrake LandingBrakeAt(int place)
    {
        GunshipLandingBrake brake = GunshipLandingBrake.First;
        for (int step = 0; step < place; step++)
            brake = brake.Next ?? throw new InvalidDataException($"The gunship's landing brakes have no step {place}.");
        return brake;
    }

    /// <summary>A recorded countdown that runs out on the step that takes it to zero, as the wait it stands for: its true duration left.</summary>
    /// <param name="word">The recorded countdown, read as signed.</param>
    private static UpdateCountdown TrueWait(ushort word) => UpdateCountdown.For(TrueDuration(word));

    /// <summary>
    /// A cartridge countdown that runs out on the step that takes it to zero or below, as the port's true duration
    /// (#2373): the updates it has left, the one it runs out on included. A count at one or below runs out on the next step.
    /// </summary>
    /// <param name="word">The recorded countdown, read as signed.</param>
    private static int ZeroOrBelowDuration(ushort word) => Math.Max(1, (int)unchecked((short)word));

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
    private static CorpsePhase DeadTourianCorpsePhaseOf(DeadTourianCorpseSpecies species, ushort word) =>
        new DeadTourianCorpsePhaseOfCase(species, word) switch
        {
            (_, 0xda63) => CorpsePhase.Finished,
            (DeadTourianCorpseSpecies.Zoomer, 0xda69) or (DeadTourianCorpseSpecies.Ripper, 0xda73) or (DeadTourianCorpseSpecies.Skree, 0xda6e) =>
                CorpsePhase.Waiting,
            (DeadTourianCorpseSpecies.Zoomer, 0xda94) or (DeadTourianCorpseSpecies.Ripper, 0xda99) or (DeadTourianCorpseSpecies.Skree, 0xda9e) =>
                CorpsePhase.PreRotDelay,
            (DeadTourianCorpseSpecies.Zoomer, 0xdad0) or (DeadTourianCorpseSpecies.Ripper, 0xdae6) or (DeadTourianCorpseSpecies.Skree, 0xdafc) =>
                CorpsePhase.Rotting,
            _ => throw new InvalidDataException($"Recorded dead {species} function ${word:X4} is not one of its routines."),
        };

    /// <summary>
    /// Decodes a Wrecked Ship ghost's recorded flicker-table byte offset into its next flicker step. The steps are the
    /// table's entries after its first, two bytes each, so offset two is the first step; the terminator's offset and zero,
    /// which the game writes when the flicker ends, are no step: the last has started (#2451). Each step leads to the next
    /// (#2512), so the step at an offset is that many steps on from the first.
    /// </summary>
    /// <param name="word">The recorded byte offset.</param>
    private static WreckedShipGhostFlickerStep? WreckedShipGhostFlickerStepOf(ushort word)
    {
        if (word == 0)
            return null;
        if (word % 2 != 0)
            throw new InvalidDataException($"Recorded Wrecked Ship ghost flicker offset {word} is not a flicker step.");
        WreckedShipGhostFlickerStep? step = WreckedShipGhostFlickerStep.First;
        for (int passed = 0; passed < word / 2 - 1; passed++)
            step = (step ?? throw new InvalidDataException($"Recorded Wrecked Ship ghost flicker offset {word} is not a flicker step.")).Next;
        return step;
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
