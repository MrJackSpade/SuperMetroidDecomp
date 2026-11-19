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
        if (enemy.SpeciesState<PhantoonPart>()?.Phantoon is { } phantoon)
        {
            EnemyDefinitionId part = enemy.DefinitionId;
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
                phantoon.Death.NextExplosion = SuperMetroid.Core.Bosses.ListStep<PhantoonDeathExplosionDefinition>.At(PhantoonDeathExplosions.Schedule, words.F);
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
                    : SuperMetroid.Core.Bosses.ListStep<PhantoonCasualFlame>.At(flames, flames.Count - Math.Max(1, (int)casualFlames));
                phantoon.Wave.Amplitude = SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(words.D);
                phantoon.Wave.AmplitudeShrinking = words.E != 0;
                phantoon.Wave.PhaseStep = RotationAngle.WrappedSteps(words.F);
            }
            else if (part == EnemyDefinitionId.PhantoonBody)
            {
                // Word A is the place of the walk his phase is on: the intro's starting flames while they spawn, his
                // figure-eight path while he runs it, and his death explosions' passes once he is dying. Only the starting
                // flames' spawning reads the intro's place.
                if (words.F != 0 && PhantoonNativeWords.Phases.Decode(words.F, "recorded PhantoonPhase") == PhantoonPhase.SpawnStartingFlames)
                    phantoon.StartingFlames.NextFlame = SuperMetroid.Core.Bosses.ListStep<TrigonometryAngle>.At(PhantoonFlameSpawns.StartingFlameStarts, words.A);
                phantoon.FigureEight.PathPlace = PhantoonPath.At(words.A);
                phantoon.Death.ExplosionPasses = words.A;
                phantoon.StartingFlames.Orbiting = words.B != 0;
                phantoon.FigureEight.Speed = new PhantoonPathSpeed(SignedSixteenSixteen.FromParts(CoordinateAxis.AsSigned(words.C), words.B));
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
            gunshipRecord.Bob = GunshipIdleBob.All[words.C];
            gunshipRecord.BobTimer = UpdateCountdownAccess.Recorded(unchecked((short)words.D));
            gunshipRecord.BaseY = words.E;
            // E holds the next landing brake only while the ship brakes; otherwise it holds the base Y.
            if (words.E < GunshipLandingBrake.All.Count)
                gunshipRecord.LandingBrake = GunshipLandingBrake.All[words.E];
        }
        if (enemy.DefinitionId == EnemyDefinitionId.ShipBottomEntrance && index > 0 &&
            enemies.Places.InUpdateOrder[index - 1].SpeciesState<GunshipState>() is { } bottomOfShip)
        {
            // The bottom hull, listed right after the top, keeps the ship's engine-sound timer in variable D, its
            // rumble frame in E and its escape speed in F.
            bottomOfShip.EngineSoundTimer = UpdateCountdown.For(ZeroOrBelowDuration(words.D));
            bottomOfShip.Rumble = UpdateCountdownAccess.AtAge(GunshipTakeoffDefinitions.EngineRumbleFrames,
                Math.Min((int)words.E, GunshipTakeoffDefinitions.EngineRumbleFrames));
            bottomOfShip.EscapeVelocity = new SignedEightEight(words.F);
        }
        if (enemy.SpeciesState<CeresSteamEnemyState>() is { } ceresSteamRecord)
        {
            // The vent's action is its population variant's, which its initialization turned into its motion: the recorded
            // action must be the one that motion carries out.
            CeresSteamAction recordedAction = CeresNativeWords.DecodeSteamAction(words.A, "recorded CeresSteamAction");
            CeresSteamAction initializedAction = ceresSteamRecord.Motion is CeresSteamMotion.RidesShaftTurn
                ? CeresSteamAction.ApplyRotatingElevatorOffset
                : CeresSteamAction.HoldsItsPlace;
            if (recordedAction != initializedAction)
                throw new InvalidDataException($"Recorded Ceres steam action {recordedAction} differs from the vent's initialized {initializedAction}.");
            // Until its first count-down the cartridge's activation count is the wait the port holds the vent hidden for once
            // its dormant animation has hidden it (#2480), the update it appears on included.
            ceresSteamRecord.UpdatesToActivation = words.D;
        }
        if (enemy.SpeciesState<CeresElevatorDoorState>() is { } elevatorDoorRecord)
        {
            elevatorDoorRecord.Phase = CeresNativeWords.ElevatorDoorPhases.Decode(words.A, "recorded Ceres elevator door phase");
            elevatorDoorRecord.Rumble = TrueWait(words.D);
            elevatorDoorRecord.ExplosionWait = TrueWait(words.E);
            // The recorded word is the native table place of the last burst, which the rumble walks from the table's last
            // place to its first; the port lists the places in burst order, so the next burst is the one after it (#2424).
            IReadOnlyList<Offset16<RoomPixels>> bursts = CeresDoorRumbleGeometryDefinitions.ExplosionOffsets;
            int lastBurst = words.F < bursts.Count
                ? words.F
                : throw new InvalidDataException($"Recorded Ceres elevator door burst place {words.F} is not one of its {bursts.Count}.");
            elevatorDoorRecord.NextExplosion = CyclingSequence<Offset16<RoomPixels>>.At(bursts, (bursts.Count - lastBurst) % bursts.Count);
        }
        else if (enemy.SpeciesState<CeresDoorAnimationState>() is not null && words.A != CeresNativeWords.MainBehaviourWord(enemy))
        {
            // Every other Ceres door and wall runs one main behaviour, which its actor names (#1953).
            throw new InvalidDataException(
                $"Recorded Ceres door or wall {enemy.Ordinal} runs routine {words.A:X4}, not its actor's {CeresNativeWords.MainBehaviourWord(enemy):X4}.");
        }
        if (enemy.SpeciesState<CeresRidleyRoomDoorState>() is { } ridleyRoomDoorRecord)
            ridleyRoomDoorRecord.Drawing = words.B != 0 ? CeresRidleyRoomDoorDrawing.ByRidleysScene : CeresRidleyRoomDoorDrawing.ItsOwn;
        if (enemy.SpeciesState<DeadTorizoEnemyState>() is { } deadTorizoRecord)
        {
            deadTorizoRecord.Phase = CorpsesNativeWords.DeadTorizoPhases.Decode(words.A, "recorded DeadTorizoPhase");
            // The corpse only sinks: its recorded sideways speed is nothing (#2432).
            if (words.B != 0)
                throw new InvalidDataException($"Recorded dead Torizo sideways speed ${words.B:X4} is not nothing.");
            deadTorizoRecord.FallSpeed = new SignedEightEight(words.C);
        }
        if (enemy.SpeciesState<MotherBrainFallingTubeState>() is { } fallingTubeRecord)
        {
            // A zero word is a setup that never ran this far (a defeated room's deleted boss); the state keeps its start.
            if (words.A != 0)
                fallingTubeRecord.Action = MotherBrainNativeWords.MotherBrainTubeActions.Decode(words.A, "recorded MotherBrainTubeAction");
            fallingTubeRecord.FloorY = words.B;
            fallingTubeRecord.FallSpeed = new SignedEightEight(words.C);
            fallingTubeRecord.SmokeDelay.SetCartridgeCount(CoordinateAxis.AsSigned(words.D));
            fallingTubeRecord.SmokePuff = SuperMetroid.Core.Bosses.ListStep<int>.At(MotherBrainTubeCollapseDefinitions.SmokeXOffsets, words.E);
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
                (DraygonPhase phase, HorizontalDirection? pass) = DraygonNativeWords.Phases.Decode(words.A, "recorded DraygonPhase");
                draygonRecord.Phase = phase;
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
                draygonEyeRecord.Action = eyeAction;
                // Only a tracking eye names the way it faces (#2165).
                if (eyeFacing is { } facing)
                    draygonEyeRecord.Facing = facing;
            }
            // The game compares only the look angle's low byte, the bearing it stored.
            draygonEyeRecord.LookAngle = AimAngleConstants.FromClockwiseSteps(words.F & byte.MaxValue);
        }
        if (enemy.SpeciesState<CrocomireEnemyState>() is { } crocomireRecord)
        {
            crocomireRecord.DeathStage = CrocomireNativeWords.DeathStages.Decode(words.A, "recorded Crocomire death stage");
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
            deadSidehopperRecord.PreRotDelay = UpdateCountdownAccess.AtAge(CorpseRotScheduleDefinitions.PreRotDelayUpdates,
                Math.Min((int)words.B, CorpseRotScheduleDefinitions.PreRotDelayUpdates));
            deadSidehopperRecord.PostLandingPause = TrueWait(words.F);
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
            {
                (ShitroidAction shitroidAction, HorizontalDirection? side) = MotherBrainNativeWords.ShitroidActions.Decode(words.A, "recorded ShitroidAction");
                shitroidRecord.Action = shitroidAction;
                // Only the departing flight names the side of Samus it flies to (#2165).
                if (side is { } departureSide)
                    shitroidRecord.DepartureSide = departureSide;
            }
            shitroidRecord.Velocity = new(new SignedEightEight(words.B), new SignedEightEight(words.C));
            // Word D holds the membrane's phase and the updates before it moves on, less one; word E its pace, as the
            // updates each phase holds less one (#2404).
            shitroidRecord.ResumeMembrane(MotherBrainNativeWords.ShitroidMembranePaces.Decode(words.E, "recorded Shitroid membrane pace"),
                ShitroidMembranePulse.All[LittleEndianBytes.LowByte(words.D)], LittleEndianBytes.HighByte(words.D) + 1);
            // The cartridge keeps the Sidehopper drain's updates in the same word as every other wait's count.
            if (shitroidRecord.Action == ShitroidAction.DrainSidehopper)
                shitroidRecord.SidehopperDrain.SetCartridgeCount(words.F - 1);
            else
                shitroidRecord.StateTimer.SetCartridgeCount(CoordinateAxis.AsSigned(words.F));
        }
        if (enemy.SpeciesState<DachoraEchoState>() is { } dachoraEcho)
        {
            // An echo's record names its flicker routine and keeps its lifetime in variable D. The first echo's variable A
            // holds its body's trail timer, which Core keeps on the body listed right before it (#2147).
            if (words.F != AnimalsNativeWords.DachoraEchoRoutine)
                throw new InvalidDataException($"Recorded Dachora echo {enemy.Ordinal} runs routine {words.F:X4}, not the echo's.");
            dachoraEcho.Lifetime = UpdateCountdownAccess.Recorded(unchecked((short)words.D));
            if (enemies.Places.ListedBeforeOrNone(enemy, 1)?.SpeciesState<DachoraEnemyState>() is { } echoBody)
                echoBody.TrailTimer = UpdateCountdownAccess.Recorded(unchecked((short)words.A));
        }
        if (enemy.SpeciesState<DachoraEnemyState>() is { } dachoraRecord)
        {
            dachoraRecord.SetGlowCycle(frame: LittleEndianBytes.HighByte(words.E), ticks: LittleEndianBytes.LowByte(words.E));
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
        if (enemy.SpeciesState<YellowPipeBugState>() is { } yellowPipeBug)
        {
            // Each way a flight or arc goes has a routine of its own (#2165); the bug flies the way its placement faces
            // (#2474), which the recorded routine's way must be. An arc's dip direction word (F) is set while it dips and
            // cleared once it climbs back (#2473).
            if (PipeBugsNativeWords.YellowPipeBugFlights.TryDecode(words.A, out (YellowPipeBugAction Action, HorizontalDirection Direction) yellowFlight))
            {
                HorizontalDirection yellowFacing = enemy.SpawnSettings<YellowPipeBugSpawnSettings>().Facing;
                if (yellowFlight.Direction != yellowFacing)
                    throw new InvalidDataException($"Recorded yellow Pipe Bug flies {yellowFlight.Direction}, not its placement's {yellowFacing}.");
                yellowPipeBug.Action = yellowFlight.Action == YellowPipeBugAction.ArcDipping && words.F == 0
                    ? YellowPipeBugAction.ArcClimbing
                    : yellowFlight.Action;
            }
            else
            {
                yellowPipeBug.Action = PipeBugsNativeWords.YellowPipeBugActions.Decode(words.A, "recorded YellowPipeBugAction");
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
        if (enemy.SpeciesState<SbugEnemyState>() is { } sbugRecord)
        {
            sbugRecord.Action = SbugNativeWords.SbugActions.Decode(words.B, "recorded SbugAction");
            sbugRecord.MovementTimer = TrueWait(words.F);
        }
        if (enemy.SpeciesState<KiHunterBodyState>() is { } kiHunterBodyRecord)
        {
            kiHunterBodyRecord.Action = KiHuntersNativeWords.KiHunterActions.Decode(words.A, "recorded KiHunterAction");
            kiHunterBodyRecord.SwoopTargetX = words.B;
            kiHunterBodyRecord.SwoopOriginY = words.C;
            // The swipe's trigger angle follows from the way the body swoops (#2474); the game keeps it in an extra word.
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
            enemies.ZebetiteBehavior.SharedPulse = ListStep<ZebetitePulse>.At(ZebetitePulse.All, words.C);
        if (enemy.SpeciesState<ChozoStatueState>() is { } chozoStatueRecord)
        {
            chozoStatueRecord.Phase = ChozoNativeWords.PhaseAt(words.F, enemy.SpawnSettings<ChozoStatueSpawnSettings>().Site == ChozoStatueSite.WreckedShip);
        }
        if (enemy.SpeciesState<YardEnemyState>() is { } yardRecord)
        {
            yardRecord.CrawlingVelocity = new(new SignedEightEight(words.A), new SignedEightEight(words.B));
            yardRecord.AirborneFacingDirection = words.C switch
            {
                0 => HorizontalDirection.Left,
                1 => HorizontalDirection.Right,
                _ => throw new InvalidDataException($"Native Yard airborne facing {words.C:X4} is neither left nor right."),
            };
            yardRecord.ConsecutiveTurnCounter = words.E;
            yardRecord.Movement = YardNativeWords.YardMovements.Decode(words.F, "recorded YardMovement");
            // A crawl's routine names its way (#2278).
            if (yardRecord.Movement == YardMovement.Crawling)
                yardRecord.Crawl = YardNativeWords.Crawls.Decode(words.F, "recorded Yard crawl");
        }
        if (enemy.SpeciesState<ZoaEnemyState>() is { } zoaRecord)
        {
            zoaRecord.SpawnPosition = new(words.A, words.B);
            // The record keeps the requested animation beside the installed one (#2145): Core keeps the Zoa's facing, which
            // the requested animation shows, and only the playing animation, which this import does not read.
            ZoaSegment requested = ZoaNativeWords.AnimationSelectorOf(words.C);
            zoaRecord.Facing = requested is ZoaSegment.FacingRightRising or ZoaSegment.FacingRightShooting
                ? HorizontalDirection.Right
                : HorizontalDirection.Left;
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
            // The record keeps the baby's signed whole-pixel speed (#2338): its sign the way it heads, a still baby's way kept.
            short babySpeed = unchecked((short)words.E);
            if (babySpeed != 0)
                babyTurtleRecord.Heading = babySpeed < 0 ? HorizontalDirection.Left : HorizontalDirection.Right;
            babyTurtleRecord.HorizontalSpeed = Math.Abs(babySpeed);
        }
        if (enemy.SpeciesState<OwtchEnemyState>() is { } owtchRecord)
        {
            // $A2:A423-$A435: fraction then whole for each direction, then the state and the sink depth. The Owtch walks at its
            // placement's speed (#2439), which the recording's must be.
            SignedSixteenSixteen owtchSpeed = NegatedPair(SignedSixteenSixteen.FromParts(unchecked((short)words.B), words.A),
                SignedSixteenSixteen.FromParts(unchecked((short)words.D), words.C), "Owtch speed");
            if (owtchSpeed != enemy.SpawnSettings<OwtchSpawnSettings>().Speed)
                throw new InvalidDataException($"Recorded Owtch speed {owtchSpeed} is not its placement's {enemy.SpawnSettings<OwtchSpawnSettings>().Speed}.");
            owtchRecord.Behavior = OwtchNativeWords.Behaviors.Decode(words.E, "recorded OwtchBehaviorState");
            // The sink depth is the sink's age while sinking and the rise's updates left while rising; underground the Owtch
            // is wholly buried, and walking it is not buried at all.
            if (owtchRecord.Behavior == OwtchBehaviorState.Sinking)
                owtchRecord.Sink = UpdateCountdownAccess.AtAge(OwtchMovementDefinitions.MaximumBurialDepth, words.F);
            else if (owtchRecord.Behavior == OwtchBehaviorState.Rising)
                owtchRecord.Rise = UpdateCountdown.For(words.F);
            else if (owtchRecord.Behavior == OwtchBehaviorState.Underground && words.F != OwtchMovementDefinitions.MaximumBurialDepth)
                throw new InvalidDataException($"Recorded underground Owtch sink depth {words.F} is not a whole burial.");
        }
        if (enemy.SpeciesState<BullEnemyState>() is { } bullRecord)
        {
            bullRecord.Action = BullNativeWords.BullActions.Decode(words.A, "recorded BullAction");
            bullRecord.Acceleration = new SignedEightEight(words.B);
            bullRecord.AccelerationDelta = new SignedEightEight(words.C);
            bullRecord.Speed = new SignedEightEight(words.D);
            bullRecord.ActivationTimer = UpdateCountdownAccess.Recorded(unchecked((short)words.E));
            bullRecord.AccelerationIntervalTimer = UpdateCountdownAccess.Recorded(unchecked((short)words.F));
        }
        if (enemy.SpeciesState<BoyonEnemyState>() is { } boyonRecord)
        {
            // The Boyon scales its curve and bounces as its placement says (#2476), which the recording's copies must be.
            BoyonSpawnSettings boyonSettings = enemy.SpawnSettings<BoyonSpawnSettings>();
            if (words.A != boyonSettings.SpeedMultiplier)
                throw new InvalidDataException($"Recorded Boyon multiplier {words.A} is not its placement's {boyonSettings.SpeedMultiplier}.");
            // The speed word is the last update's product, which every update multiplies afresh before it moves (#2531).
            if (SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(words.C) != SignedSixteenSixteen.FromWholePixels(boyonSettings.JumpHeight))
                throw new InvalidDataException($"Recorded Boyon height ${words.C:X4} is not its placement's {boyonSettings.JumpHeight} pixels.");
            // The arc's distance is only the first update's working sum (#2432): a Boyon still to derive its arc has summed
            // nothing, and one that has derived it no longer reads it.
            if (boyonRecord.Phase == BoyonPhase.DerivingArc && words.D != 0)
                throw new InvalidDataException($"Recorded Boyon arc distance ${words.D:X4} belongs to a derived arc, which the port's Boyon has still to derive.");
            // The rise leaves the curve index one below the top entry, the word's largest value, past the top.
            boyonRecord.CurvePlace = words.E == ushort.MaxValue ? BoyonCurvePlaceConstants.PastTheTop : BoyonCurvePlaceConstants.AtEntry(words.E);
            // The bouncing word is the phase's own: the record holds none of the latches the rest of the phase reads.
            if ((words.F != 0) != boyonRecord.IsBouncing)
                throw new InvalidDataException($"Recorded Boyon bouncing word ${words.F:X4} disagrees with its {boyonRecord.Phase} phase.");
        }
        if (enemy.SpeciesState<GrowingShutterEnemyState>() is { } growingShutterRecord)
        {
            // Each routine names the way the shutter grows, which is the placement's (#2512).
            (GrowingShutterAction growingAction, VerticalDirection recordedGrowth) =
                ShuttersNativeWords.GrowingShutterActions.Decode(words.A, "recorded GrowingShutterAction");
            if (recordedGrowth != growingShutterRecord.Growth)
                throw new InvalidDataException($"Recorded growing shutter routine ${words.A:X4} grows {recordedGrowth}, not its placement's {growingShutterRecord.Growth}.");
            growingShutterRecord.Action = growingAction;
            // The record keeps every section's origin; Core derives them from the first, the spawn row (#2183), and the
            // recorded origins must be the ones it derives.
            growingShutterRecord.SpawnY = words.B;
            ushort[] originRows = [words.B, words.C, words.D, words.E];
            for (int level = 0; level < GrowingShutterEnemyState.SectionCount; level++)
            {
                if (growingShutterRecord.SectionOriginY(level) != originRows[level])
                    throw new InvalidDataException(
                        $"Recorded growing shutter section {level} origin {originRows[level]:X4} does not follow from its spawn row {words.B:X4}.");
            }
            growingShutterRecord.GrowthLevel = words.F;
        }
        // A bobbing Fireflea keeps none of these words.
        if (enemy.SpeciesState<FirefleaEnemyState>() is { Flight: CirclingFirefleaState firefleaRecord })
        {
            // The Fireflea circles as its placement chose (#2476), which the recording's turn and radius must be.
            AngularSpeed firefleaTurn = SuperMetroid.AssetExtraction.Enemies.EnemySpawnSelectors.FirefleaTurnPerUpdate(SignedSixteenSixteen.FromParts(unchecked((short)words.B), words.A));
            if (firefleaTurn != firefleaRecord.Definition.TurnPerUpdate)
                throw new InvalidDataException($"Recorded Fireflea turn {firefleaTurn} is not its placement's {firefleaRecord.Definition.TurnPerUpdate}.");
            if (words.C != enemy.SpawnSettings<FirefleaSpawnSettings>().Radius)
                throw new InvalidDataException($"Recorded Fireflea radius {words.C} is not its placement's {enemy.SpawnSettings<FirefleaSpawnSettings>().Radius}.");
            firefleaRecord.Angle = RotationAngleConstants.FromRaw(words.D);
            firefleaRecord.Center = new(words.E, words.F);
        }
        if (enemy.SpeciesState<VerticalShutterEnemyState>() is { } verticalShutterRecord)
        {
            ShutterSpawnSettings verticalShutterSettings = enemy.SpawnSettings<ShutterSpawnSettings>();
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
        if (enemy.SpeciesState<CacatacEnemyState>() is { } cacatacRecord)
        {
            // The record holds the speed's whole pixels and their negation for walking left; the Cacatac walks at its
            // placement's speed (#2476), whose whole pixels the recording's must be.
            if (unchecked((short)words.A) != enemy.SpawnSettings<CacatacSpawnSettings>().Speed.WholePixels)
                throw new InvalidDataException($"Recorded Cacatac speed {unchecked((short)words.A)} is not its placement's {enemy.SpawnSettings<CacatacSpawnSettings>().Speed}.");
            // Every recorded direction but left patrols right, as the game's restore-patrol cue reads it (#2277).
            HorizontalDirection cacatacPatrol = CacatacNativeWords.Directions.Decode(words.D, "recorded Cacatac direction").Direction;
            (CacatacAction cacatacAction, HorizontalDirection? cacatacPatrolling) = CacatacNativeWords.CacatacActions.Decode(words.E, "recorded CacatacAction");
            if (cacatacPatrolling is { } patrolling && patrolling != cacatacPatrol)
                throw new InvalidDataException($"Recorded Cacatac patrols {patrolling} but its direction word ${words.D:X4} names {cacatacPatrol}.");
            cacatacRecord.PatrolDirection = cacatacPatrol;
            cacatacRecord.Action = cacatacAction;
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
        // An unpowered robot keeps no facing, laser cooldown or fall: it only holds its pose (#2179, #2455).
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
            metroidRecord.EscapeMovesLeft = UpdateCountdown.For(words.E);
            metroidRecord.Movement = MetroidsNativeWords.Movements.Decode(words.F, "recorded MetroidMovement");
        }
        if (enemy.SpeciesState<FakeKraidEnemyState>() is { } fakeKraidRecord)
        {
            // The cartridge keeps Fake Kraid's walk and facing as signed four-pixel steps (#2214).
            fakeKraidRecord.Walking = FakeKraidNativeDirection(words.B, "walk");
            fakeKraidRecord.Facing = FakeKraidNativeDirection(words.C, "facing");
            // The cartridge counts both in walk steps left: the walk turns on the step its count reaches zero, and the spit
            // count stops at zero.
            fakeKraidRecord.WalkTurnWait.SetCartridgeCount(words.D - 1);
            fakeKraidRecord.SpitWait.SetUpdatesLeft(words.E);
        }
        if (enemy.SpeciesState<MaridiaLargeSnailEnemyState>() is { } maridiaLargeSnailRecord)
        {
            maridiaLargeSnailRecord.Action = MaridiaSnailsNativeWords.MaridiaLargeSnailActions.Decode(words.A, "recorded MaridiaLargeSnailAction");
            maridiaLargeSnailRecord.YSpeedCurvePlace = QuadraticSpeedCurvePlace.FromTwoHundredFiftySixthsOfMark(unchecked((short)words.B));
            maridiaLargeSnailRecord.AttackCooldown = TrueWait(words.C);
            maridiaLargeSnailRecord.Heading = words.D != 0 ? HorizontalDirection.Left : HorizontalDirection.Right;
            maridiaLargeSnailRecord.RemainingBounces = unchecked((short)words.E);
            maridiaLargeSnailRecord.BouncePhase = MaridiaSnailsNativeWords.MaridiaLargeSnailBouncePhases.Decode(words.F, "recorded MaridiaLargeSnailBouncePhase");
        }
        if (enemy.SpeciesState<WallSpacePirateEnemyState>() is { } wallSpacePirateRecord)
        {
            (WallSpacePirateAction wallAction, HorizontalDirection? wall) = SpacePiratesNativeWords.WallSpacePirateActions.Decode(words.A, "recorded WallSpacePirateAction");
            wallSpacePirateRecord.Action = wallAction;
            // An attack's routine names no wall: the Pirate stays on the one it clung to (#2165).
            if (wall is { } clung)
                wallSpacePirateRecord.Wall = clung;
            wallSpacePirateRecord.ClimbDirection = SpacePiratesNativeWords.WallClimbDirections.Decode(words.C, "recorded WallSpacePirateClimbDirection");
            wallSpacePirateRecord.WallJumpArcCenter = new(words.D, words.E);
            wallSpacePirateRecord.WallJumpArcAngle = RotationAngle.FromSteps(checked((byte)words.F));
        }
        if (enemy.SpeciesState<NinjaSpacePirateEnemyState>() is { } ninjaSpacePirateRecord)
        {
            (NinjaSpacePirateAction ninjaAction, HorizontalDirection? ninjaHeading) = SpacePiratesNativeWords.NinjaSpacePirateActions.Decode(words.A, "recorded NinjaSpacePirateAction");
            ninjaSpacePirateRecord.Action = ninjaAction;
            // Only a move that goes one way names its heading (#2165).
            if (ninjaHeading is { } heading)
                ninjaSpacePirateRecord.Heading = heading;
            ninjaSpacePirateRecord.PostsMidpointX = words.D;
            ninjaSpacePirateRecord.LeftPostX = words.E;
            ninjaSpacePirateRecord.RightPostX = words.F;
        }
        if (enemy.SpeciesState<SkreeEnemyState>() is { } skreeRecord)
        {
            skreeRecord.BurrowTimer = UpdateCountdownAccess.Recorded(unchecked((short)words.A));
            skreeRecord.Action = SkreeNativeWords.SkreeActions.Decode(words.B, "recorded SkreeAction");
            RequirePlayingSkreeMetareeAnimation(skreeRecord.Playback, SkreeNativeWords.SkreeAnimationPhases, words, "Skree");
            // The game keeps a readied dive as a flag beside its winding-up routine (#2450), set only while it winds up.
            skreeRecord.Action = (skreeRecord.Action, words.E) switch
            {
                (SkreeAction.PreparingAttack, not 0) => SkreeAction.AttackReady,
                (_, 0) => skreeRecord.Action,
                (var readied, _) => throw new InvalidDataException($"Recorded Skree {readied} has its dive readied."),
            };
        }
        if (enemy.SpeciesState<BoulderEnemyState>() is { } boulderRecord)
        {
            boulderRecord.Action = BoulderNativeWords.BoulderActions.Decode(words.A, "recorded BoulderAction");
            boulderRecord.TravelSpeedAccumulator = QuadraticSpeedCurvePlace.FromTwoHundredFiftySixthsOfMark(words.B);
            boulderRecord.BounceSpeedAccumulator = QuadraticSpeedCurvePlace.FromTwoHundredFiftySixthsOfMark(words.C);
            // The record counts the bounces left, two before the first impact and minus one once rolling.
            boulderRecord.ImpactsSoFar = BoulderBounces.ImpactCount - 1 - unchecked((short)words.D);
            // The Boulder rolls as its placement names (#2476), which the recording's roll must be.
            BoulderRoll boulderRoll = words.E switch
            {
                0 => BoulderRoll.Right,
                1 => BoulderRoll.Left,
                2 => BoulderRoll.LeftBreakingOnImpact,
                _ => throw new InvalidDataException($"Recorded Boulder roll {words.E} is not a roll."),
            };
            if (boulderRoll != enemy.SpawnSettings<BoulderSpawnSettings>().Roll)
                throw new InvalidDataException($"Recorded Boulder roll {boulderRoll} is not its placement's {enemy.SpawnSettings<BoulderSpawnSettings>().Roll}.");
        }
        if (enemy.SpeciesState<RinkaEnemyState>() is { } rinkaRecord)
        {
            // A zero word is a setup that never ran this far (a defeated room's deleted boss); the state keeps its start.
            if (words.A != 0)
                rinkaRecord.Action = MotherBrainNativeWords.RinkaActions.Decode(words.A, "recorded RinkaAction");
            rinkaRecord.Velocity = new(new SignedEightEight(words.B), new SignedEightEight(words.C));
            rinkaRecord.HeldSpawnPoint = words.D == 0 ? null : MotherBrainNativeWords.RinkaSpawnPoints.Decode(words.D, "recorded RinkaSpawnPoint");
            // A special Rinka holding a point was placed at it, which makes it the point it respawns at (#2256).
            if (rinkaRecord.HeldSpawnPoint is { } heldPoint)
                rinkaRecord.SpawnPoint = heldPoint;
            // The record's one wait is the respawn delay while the Rinka waits to respawn, and its aim delay otherwise.
            (rinkaRecord.Action == RinkaAction.DeathRespawnDelay ? rinkaRecord.RespawnDelay : rinkaRecord.AimDelay)
                .SetCartridgeCount(CoordinateAxis.AsSigned(words.F));
        }
        if (enemy.SpeciesState<EtecoonEnemyState>() is { } etecoonRecord)
        {
            etecoonRecord.Velocity = FixedVector<RoomPixels>.FromStoredParts(words.C, words.D, words.A, words.B);
            etecoonRecord.Action = AnimalsNativeWords.EtecoonActions.Decode(words.F, "recorded EtecoonAction");
            // A dormant Etecoon's record holds its countdown below zero while it waits for Samus; the same routine flexes
            // once the countdown holds the flex's (#2476). The port's dormant Etecoon waits without one, and every other
            // record's countdown is the wait it has left.
            if (etecoonRecord.Action == EtecoonAction.Dormant && unchecked((short)words.E) >= 0)
                etecoonRecord.Action = EtecoonAction.Flexing;
            if (etecoonRecord.Action != EtecoonAction.Dormant)
                etecoonRecord.ActionTimer = UpdateCountdown.For(words.E);
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
            morphBallEyeRecord.ActionTimer = UpdateCountdownAccess.Recorded(unchecked((short)words.E));
            morphBallEyeRecord.Action = MorphBallEyeNativeWords.MorphBallEyeActions.Decode(words.F, "recorded MorphBallEyeAction");
        }
        if (enemy.SpeciesState<HopperEnemyState>() is { } hopperRecord)
        {
            (HopperAction hopperAction, HopperHopSize? hopSize, HorizontalDirection? startHeading) =
                HoppersNativeWords.HopperActions.Decode(words.A, "recorded HopperAction");
            hopperRecord.Action = hopperAction;
            hopperRecord.YSpeedCurvePlace = QuadraticSpeedCurvePlace.FromMarks(words.B);
            // The record keeps the heading as three pixels either way (#2338), none before the first hop; a starting hop's
            // routine names the heading it takes, which its velocity only gains as the hop starts.
            hopperRecord.Heading = unchecked((short)words.C) switch
            {
                -HopperHopDefinitions.HorizontalSpeed => HorizontalDirection.Left,
                HopperHopDefinitions.HorizontalSpeed => HorizontalDirection.Right,
                0 => hopperRecord.Heading,
                var other => throw new InvalidDataException($"Recorded hopper X velocity {other} is not three pixels either way."),
            };
            if (startHeading is { } starts)
                hopperRecord.Heading = starts;
            if (hopSize is { } size)
                hopperRecord.HopSize = size;
            hopperRecord.YSpeedCurvePlaceStep = QuadraticSpeedCurvePlace.FromMarks(words.D);
        }
        if (enemies.PlatformStates[index] is { } platform)
        {
            platform.RestingY = words.A;
            // The platform reads its speed and its acceleration's cap from its spawn settings where it uses them (#2439); the
            // recording's copy of the speed must be the placement's.
            PlatformSpawnSettings platformSettings = enemy.SpawnSettings<PlatformSpawnSettings>();
            SignedSixteenSixteen platformSpeed = NegatedPair(SignedSixteenSixteen.FromParts(unchecked((short)words.C), words.B),
                SignedSixteenSixteen.FromParts(unchecked((short)words.E), words.D), "platform speed");
            if (platformSpeed != platformSettings.Speed)
                throw new InvalidDataException($"Recorded platform speed {platformSpeed} is not its placement's {platformSettings.Speed}.");
            platform.YSpeedCurvePlace = QuadraticSpeedCurvePlace.FromMarks(Index(words.F, platformSettings.VerticalAccelerationUpdates, "platform acceleration index"));
        }
        if (enemies.BeetomStates[index] is { } beetom)
        {
            beetom.YSpeedCurvePlace = QuadraticSpeedCurvePlace.FromMarks(Index(words.B, short.MaxValue, "Beetom speed index"));
            (BeetomAction beetomAction, HorizontalDirection? beetomHeading) = BeetomNativeWords.BeetomActions.Decode(words.C, "recorded BeetomAction");
            beetom.Action = beetomAction;
            // Only an action that goes one way names its heading (#2277).
            if (beetomHeading is { } beetomGoes)
                beetom.Heading = beetomGoes;
            beetom.ActionTimer = TrueWait(words.D);
            beetom.ButtonCounter = unchecked((short)words.E);
            beetom.LastSeenPlayerActions = GameplayActionWordCodec.FromJoypadWord(words.F, "native Beetom previous controller input");
        }
        if (enemy.DefinitionId == EnemyDefinitionId.Shaktool && enemies.ShaktoolSegments[index] is { } segment)
        {
            int segmentIndex = Array.IndexOf(ShaktoolSegmentDefinitions.Segments, enemy.SpawnSettings<ShaktoolSpawnSettings>().Segment);
            // Every segment's record holds a copy of the chain's target; the leading saw's, which no straightening turns
            // on, is the chain's (#2452).
            if (segmentIndex == 0)
                enemies.ShaktoolBehavior.ChainOf(enemy).TargetAngle = RotationAngleConstants.FromRaw(words.A);
            segment.OrbitAngle = RotationAngleConstants.FromRaw(words.B);
            segment.AngularVelocity = RotationAngleConstants.FromRaw(Index(words.C, 1020, "Shaktool angular velocity"));
            ImportShaktoolOrientation(segment, segmentIndex, words.D);
            // Every segment's owner is the chain's leading saw, the first enemy its room lists.
            if (SlotIndex(words.E, "Shaktool owner") != 0)
                throw new InvalidDataException($"Recorded Shaktool owner ${words.E:X4} does not name the first enemy, where the chain's saw is placed.");
            segment.Motion = ShaktoolNativeWords.DecodeMotion(words.F, "recorded ShaktoolSegmentMotion");
        }
        if (enemies.ChootStates[index] is { } choot)
        {
            choot.Action = ChootNativeWords.ChootActions.Decode(words.A, "recorded ChootAction");
            // The ascent cursor is 8.8 and moves by whole records, so its signed high byte is the record index.
            choot.YSpeedCurvePlace = QuadraticSpeedCurvePlace.FromMarks(unchecked((short)words.B) >> 8);
            choot.FallingLoop = (words.C & 0xff) == 0
                ? UpdateCountdownAccess.AtAge(ChootFalls.LoopUpdates(enemy.SpawnSettings<ChootSpawnSettings>().FallingPath), words.C >> 8)
                : throw new InvalidDataException($"Recorded Choot falling cursor ${words.C:X4} is not a whole frame.");
            choot.FallingPatternLoopCounter = TrueDuration(words.D);
            // The Choot falls along its placement's path (#2476), which the recording's must be.
            ChootFallingPath chootPath = ChootNativeWords.ChootFallingPaths.Decode(words.E, "recorded Choot falling pattern");
            if (chootPath != enemy.SpawnSettings<ChootSpawnSettings>().FallingPath)
                throw new InvalidDataException($"Recorded Choot falling path {chootPath} is not its placement's {enemy.SpawnSettings<ChootSpawnSettings>().FallingPath}.");
            // The loop's Y distance is the placement's path's (#2531), which the recording's copy must be.
            if (words.F != ChootFalls.LoopYDistance(chootPath))
                throw new InvalidDataException($"Recorded Choot loop Y distance {words.F} is not its {chootPath} path's {ChootFalls.LoopYDistance(chootPath)}.");
        }
        if (enemies.SkulteraStates[index] is { } skultera)
        {
            (skultera.Action, skultera.Heading) = SkulteraNativeWords.SkulteraActions.Decode(words.A, "recorded SkulteraAction");
            // The leftward words are the speed reversed (#2278): the speed is the rightward pair. The Skultera swims at its
            // placement's speed (#2450), which the recording's must be.
            SignedSixteenSixteen skulteraSpeed = SignedSixteenSixteen.FromParts(unchecked((short)words.C), words.B);
            if (skulteraSpeed != enemy.SpawnSettings<SkulteraSpawnSettings>().Speed)
                throw new InvalidDataException($"Recorded Skultera speed {skulteraSpeed} is not its placement's {enemy.SpawnSettings<SkulteraSpawnSettings>().Speed}.");
            // Only the angle word's low byte is ever read; movement discards its high byte.
            skultera.Angle = RotationAngle.FromSteps(LittleEndianBytes.LowByte(words.F));
        }
        if (enemy.DefinitionId == EnemyDefinitionId.Coven && enemy.SpeciesState<WreckedShipGhostEnemyState>() is { } ghost)
        {
            ghost.Action = WreckedShipNativeWords.WreckedShipGhostActions.Decode(words.A, "recorded WreckedShipGhostAction");
            // Word B is the one timer the cartridge keeps for every phase; the port keeps a countdown for each timed phase
            // (#2512). Watching Samus, the word holds the flicker's start, which the port's appearance sets; fading away, the
            // bob's run-out.
            int ghostTimer = unchecked((short)words.B);
            switch (ghost.Action)
            {
                case WreckedShipGhostAction.InitialInvisibleDelay:
                    ghost.InvisibleDelay = UpdateCountdownAccess.Recorded(ghostTimer);
                    break;
                case WreckedShipGhostAction.BrighteningAndFlickering or WreckedShipGhostAction.FadingToGhostColours:
                    ghost.FlickerTimer = UpdateCountdownAccess.Recorded(ghostTimer);
                    ghost.NextFlickerStep = WreckedShipGhostFlickerStepOf(words.C);
                    break;
                case WreckedShipGhostAction.BobbingWhileVisible:
                    ghost.VisibleTimer = UpdateCountdownAccess.Recorded(ghostTimer);
                    break;
                case WreckedShipGhostAction.TrackingSamusForSpawn:
                    if (ghostTimer != WreckedShipGhostAppearanceDefinitions.FlickerFirstStepDelay)
                        throw new InvalidDataException($"Recorded watching Wrecked Ship ghost's timer {ghostTimer} is not its flicker's start.");
                    break;
                case WreckedShipGhostAction.WaitingForWhiteFade:
                    if (ghostTimer != 0)
                        throw new InvalidDataException($"Recorded vanishing Wrecked Ship ghost's timer {ghostTimer} is not its bob's run-out.");
                    break;
                default:
                    throw new InvalidDataException($"Undefined {nameof(WreckedShipGhostAction)} {ghost.Action}.");
            }
        }
        if (enemy.EntityState<WalkingSpacePirateEntity, WalkingSpacePirateEnemyState>() is { } walkingPirate)
        {
            (WalkingSpacePirateAction walkingAction, HorizontalDirection? walkingHeading) = SpacePiratesNativeWords.WalkingSpacePirateActions.Decode(words.A, "recorded WalkingSpacePirateAction");
            walkingPirate.Action = walkingAction;
            // Only a walk names its heading (#2165).
            if (walkingHeading is { } walk)
                walkingPirate.Heading = walk;
            walkingPirate.LeftPostX = words.E;
            walkingPirate.RightPostX = words.F;
        }
        // A Multiviola's velocity words (C to F) are the last update's, which every update rebuilds from its heading before it
        // moves (#2531), so none is imported.
        if (enemy.EntityState<CrawlerEntity, CrawlerEnemyState>() is { } crawler)
        {
            crawler.Velocity = new(new SignedEightEight(words.A), new SignedEightEight(words.B));
            crawler.Action = (enemy.Entity is OrangeZoomerEntity ? CrawlersNativeWords.OrangeZoomerActions : CrawlersNativeWords.SurfaceCrawlerActions)
                .Decode(words.F, "recorded CrawlerAction");
        }
        if (enemy.DefinitionId == EnemyDefinitionId.Holtz && enemy.SpeciesState<LowerNorfairRioEnemyState>() is { } holtz)
        {
            holtz.Velocity = new(new SignedEightEight(words.D), new SignedEightEight(words.C));
            holtz.Action = RiosNativeWords.LowerNorfairRioActions.Decode(words.F, "recorded LowerNorfairRioAction");
        }
        if (enemy.DefinitionId == EnemyDefinitionId.EtecoonEscape && enemy.SpeciesState<EscapeEtecoonEnemyState>() is { } escape)
        {
            // The record's signed walking speed: its sign is the way the Etecoon walks (#2369).
            var escapeVelocity = new SignedEightEight(words.A);
            PrivateState.Set(escape, nameof(EscapeEtecoonEnemyState.Direction), escapeVelocity.IsNegative ? HorizontalDirection.Left : HorizontalDirection.Right);
            PrivateState.Set(escape, nameof(EscapeEtecoonEnemyState.Speed), escapeVelocity.IsNegative ? escapeVelocity.Negated() : escapeVelocity);
            PrivateState.Set(escape, nameof(EscapeEtecoonEnemyState.Action), AnimalsNativeWords.EscapeEtecoonActionAt(words.F));
        }
        if (enemy.DefinitionId == EnemyDefinitionId.FaceBlock && enemy.SpeciesState<BlueBrinstarFaceBlockEnemyState>() is { } faceBlock)
        {
            // The game keeps a woken face as a flag beside the turn its wake installed; the port's face is awake while it
            // plays that turn (#2476). The recording's place in the turn is not imported: a woken face holds its turn's end.
            if (words.A != 0)
            {
                Animation<BlueBrinstarFaceBlockSegment> turn = BlueBrinstarFaceBlockAnimations.StartingWith(
                    BlueBrinstarFaceBlockBehavior.Wake(enemy.SpawnSettings<FaceBlockSpawnSettings>().Facing));
                faceBlock.Playback = new(turn, AnimationPlace.HoldingIn(0, turn.Phases[0].BeatCount));
            }
        }
        if (enemy.DefinitionId == EnemyDefinitionId.NuclearWaffle && enemy.SpeciesState<NuclearWaffleEnemyState>() is { } waffle)
        {
            waffle.Action = NuclearNativeWords.NuclearWaffleActions.Decode(words.A, "recorded NuclearWaffleAction");
            waffle.WaitingTimer = TrueWait(words.B);
        }
        if (enemy.DefinitionId == EnemyDefinitionId.Polyp && enemy.SpeciesState<PolypEnemyState>() is { } polyp)
        {
            polyp.Action = PolypNativeWords.PolypActions.Decode(words.A, "recorded PolypAction");
            polyp.CooldownTimer = TrueWait(words.B);
        }
        // The halves reuse the same words for different values, each half into its own state (#1845).
        if (enemy.DefinitionId == EnemyDefinitionId.Powamp && enemy.SpeciesState<PowampBalloonState>() is { } powampBalloon)
        {
            powampBalloon.Spawn = new(words.A, words.B);
            // The balloon reads its grappled rise from its spawn settings (#2439); the recording's copy must be the placement's.
            if (words.D != enemy.SpawnSettings<PowampSpawnSettings>().GrappleTravelDistance)
                throw new InvalidDataException($"Recorded Powamp grapple rise {words.D} is not its placement's {enemy.SpawnSettings<PowampSpawnSettings>().GrappleTravelDistance}.");
            if (words.F != PowampNativeWords.BalloonIdleWord)
                throw new InvalidDataException($"Recorded Powamp balloon runs ${words.F:X4}, not the balloon's empty routine.");
        }
        if (enemy.DefinitionId == EnemyDefinitionId.Powamp && enemy.SpeciesState<PowampBodyState>() is { } powampBody)
        {
            powampBody.YVelocity = SignedSixteenSixteen.FromParts(unchecked((short)words.A), words.B);
            powampBody.Wiggle = PowampWiggle.All[Index(words.C, PowampWiggle.All.Count - 1, "Powamp wiggle phase")];
            powampBody.WiggleTimer = UpdateCountdown.For(ZeroOrBelowDuration(words.D));
            powampBody.ActionTimer = UpdateCountdown.For(ZeroOrBelowDuration(words.E));
            powampBody.Action = PowampNativeWords.PowampActions.Decode(words.F, "recorded PowampAction");
        }
        if (enemy.DefinitionId == EnemyDefinitionId.MamaTurtle && enemy.SpeciesState<MamaTurtleEnemyState>() is { } mama)
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
        if (enemies.HibashiStates[index] is { } hibashi)
        {
            hibashi.Action = HibashiNativeWords.HibashiActions.Decode(words.A, "recorded HibashiAction");
            hibashi.InactiveTimer = TrueWait(words.B);
            // The game keeps a finished eruption as a flag beside its active routine: an active flame whose flag is set has
            // finished and rests on its next update (#2439). An inactive flame's flag is left over from its last eruption.
            if (hibashi.Action == HibashiAction.Active && words.C != 0)
                hibashi.Action = HibashiAction.Finished;
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
            corpse.PreRotDelay = UpdateCountdownAccess.AtAge(CorpseRotScheduleDefinitions.PreRotDelayUpdates,
                Math.Min((int)words.B, CorpseRotScheduleDefinitions.PreRotDelayUpdates));
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

            // The record keeps the requested animation beside the installed one (#2145): Core keeps the Waver's facing,
            // which the requested animation shows, and only the playing animation, which this import does not read.
            RequirePlaying(waver.Playback, WaverAnimations.StartingWith(WaverNativeWords.Animations.Decode(words.C, "recorded Waver installed animation")), "Waver");
            // Only the phase word's low byte is ever read.
            waver.Angle = RotationAngle.FromSteps(LittleEndianBytes.LowByte(words.D));
            // The game keeps a finished spin as a flag beside the spin it installed (#2450): a spinning Waver whose flag is
            // set has finished its spin, and only a spin sets it.
            bool spins = WaverAnimationChoices.IsSpin(WaverNativeWords.Animations.Decode(words.C, "recorded Waver installed animation"));
            waver.Action = (spins, words.E) switch
            {
                (true, 0) => WaverAction.Spinning,
                (true, _) => WaverAction.SpinFinished,
                (false, 0) => WaverAction.Gliding,
                (false, _) => throw new InvalidDataException("Recorded steady Waver has finished a spin."),
            };
            waver.Facing = WaverAnimationChoicesConstants.Facing(WaverNativeWords.Animations.Decode(words.F, "recorded Waver requested animation"));
            // The Waver flies its one speed toward its facing (#2278): the recorded velocity must be that.
            if (waver.XVelocity != SignedSixteenSixteen.FromParts(unchecked((short)words.B), words.A))
                throw new InvalidDataException($"Recorded Waver velocity ${words.B:X4}.{words.A:X4} is not its speed toward its facing {waver.Facing}.");
        }
        if (enemy.EntityState<FliesEntity, FlyEnemyState>() is { } fly)
        {
            fly.Velocity = new(new SignedEightEight(words.B), new SignedEightEight(words.C));
            fly.TargetY = words.D;
            // The phase word is an even byte offset into the 256-entry table, nine bits wide.
            fly.Angle = words.E % 2 == 0
                ? RotationAngle.FromSteps(checked((byte)(Index(words.E, 510, "fly phase") / 2)))
                : throw new InvalidDataException($"Recorded fly phase ${words.E:X4} is odd.");
            (FlyAction flyAction, RotationDirection? flyRotation) = FliesNativeWords.FlyActions.Decode(words.F, "recorded FlyAction");
            fly.Action = flyAction;
            // Only circling names the way it circles (#2277); a retreat starts circling clockwise again.
            if (flyRotation is { } flyCircles)
                fly.Rotation = flyCircles;
            // The word counts the dive up, its retreat down past zero and the circling cooldown down (#2369): it is the
            // current phase's count; the port's retreat holds its true duration.
            switch (fly.Action)
            {
                case FlyAction.AttackSamus:
                    PrivateState.Set(fly.Dive, nameof(ElapsedUpdates.Age), (int)unchecked((short)words.A));
                    break;
                case FlyAction.Retreat:
                    fly.RetreatUpdatesLeft = TrueWait(words.A);
                    break;
                case FlyAction.Circling:
                    fly.CooldownUpdatesLeft = UpdateCountdownAccess.Recorded(unchecked((short)words.A));
                    break;
                default:
                    throw new InvalidDataException($"Recorded fly action {fly.Action} has no count.");
            }
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
        if (enemies.ElevatorStates[index] is { } elevator)
            elevator.RestingY = words.A;
        // A Fune reads no wake distance and a Namihe no cooldown; each takes only its own species' words.
        if (enemies.FuneStates[index] is { } fune)
        {
            fune.Action = FuneNamiheNativeWords.FuneActions.Decode(words.B, "recorded FuneAction");
            if (Index(words.D, 15, "Fune/Namihe behaviour") != 0)
                throw new InvalidDataException($"Recorded Fune/Namihe behaviour ${words.D:X4} names a Namihe.");
            int cooldownUpdates = FuneNamiheBehavior.CooldownOf(enemy).Length;
            fune.Cooldown = UpdateCountdownAccess.AtAge(cooldownUpdates, Math.Min(words.E, cooldownUpdates));
            // The Fune waits its placement's cooldown (#2476), which the recording's must be.
            if (Index(words.F, byte.MaxValue, "Fune cooldown time") != enemy.SpawnSettings<FuneSpawnSettings>().CooldownTime)
                throw new InvalidDataException($"Recorded Fune cooldown {words.F} is not its placement's {enemy.SpawnSettings<FuneSpawnSettings>().CooldownTime}.");
            (bool namihe, bool active, bool right) = FuneNamiheSelectorOracle.Decode(words.A);
            if (namihe)
                throw new InvalidDataException($"Recorded Fune/Namihe selector ${words.A:X4} names a Namihe.");
            // The selector's activity is the spit the recorded action names (#2432).
            if (active != fune.IsSpitting)
                throw new InvalidDataException($"Recorded Fune selector ${words.A:X4} disagrees with its recorded action {fune.Action}.");
            // The Fune faces as its placement says (#2476), which the recording's selector must.
            if ((right ? HorizontalDirection.Right : HorizontalDirection.Left) != enemy.SpawnSettings<FuneSpawnSettings>().Facing)
                throw new InvalidDataException($"Recorded Fune selector ${words.A:X4} does not face its placement's {enemy.SpawnSettings<FuneSpawnSettings>().Facing}.");
        }
        if (enemies.NamiheStates[index] is { } namiheState)
        {
            namiheState.Action = FuneNamiheNativeWords.NamiheActions.Decode(words.B, "recorded NamiheAction");
            // The Namihe wakes at its placement's distance (#2476), which the recording's must be.
            if (Index(words.C, byte.MaxValue, "Namihe wake distance") != enemy.SpawnSettings<NamiheSpawnSettings>().YProximity)
                throw new InvalidDataException($"Recorded Namihe wake distance {words.C} is not its placement's {enemy.SpawnSettings<NamiheSpawnSettings>().YProximity}.");
            if (Index(words.D, 15, "Fune/Namihe behaviour") == 0)
                throw new InvalidDataException($"Recorded Fune/Namihe behaviour ${words.D:X4} names a Fune.");
            (bool namihe, bool active, bool right) = FuneNamiheSelectorOracle.Decode(words.A);
            if (!namihe)
                throw new InvalidDataException($"Recorded Fune/Namihe selector ${words.A:X4} names a Fune.");
            // The selector's activity is the spit the recorded action names (#2432).
            if (active != namiheState.IsSpitting)
                throw new InvalidDataException($"Recorded Namihe selector ${words.A:X4} disagrees with its recorded action {namiheState.Action}.");
            // The Namihe faces as its placement says (#2476), which the recording's selector must.
            if ((right ? HorizontalDirection.Right : HorizontalDirection.Left) != enemy.SpawnSettings<NamiheSpawnSettings>().Facing)
                throw new InvalidDataException($"Recorded Namihe selector ${words.A:X4} does not face its placement's {enemy.SpawnSettings<NamiheSpawnSettings>().Facing}.");
        }
        if (enemy.DefinitionId is EnemyDefinitionId.GrappleRipper or EnemyDefinitionId.Ripper or EnemyDefinitionId.JetRipper &&
            enemy.SpeciesState<RipperEnemyState>() is { } ripperVariant)
        {
            ripperVariant.Heading = unchecked((short)words.D) < 0 ? HorizontalDirection.Left : HorizontalDirection.Right;
            // The Ripper flies its placement's speed (#2450), which the recording's must be.
            EnemyLinearSpeed recordedRipperSpeed = SuperMetroid.AssetExtraction.Enemies.EnemySpeedTableCodec.LinearSpeedAtOffset(words.E);
            // A grapple Ripper's placement keeps its patrol bounds beside its speed (#2512).
            SignedSixteenSixteen placementRipperSpeed = enemy.DefinitionId == EnemyDefinitionId.GrappleRipper
                ? enemy.SpawnSettings<GrappleRipperSpawnSettings>().Speed
                : enemy.SpawnSettings<RipperSpawnSettings>().Speed;
            SignedSixteenSixteen ripperRightward = RipperBehavior.FlyingVelocity(placementRipperSpeed, HorizontalDirection.Right);
            if (recordedRipperSpeed.Speed != ripperRightward)
                throw new InvalidDataException($"Recorded Ripper speed {recordedRipperSpeed.Speed} is not its placement's {ripperRightward}.");
        }
        if (enemies.NorfairLavaJumpingEnemyStates[index] is { } lavaJumper)
        {
            lavaJumper.YVelocity = new SignedEightEight(words.C);
            lavaJumper.SpawnPosition = new Position16<RoomPixels>(words.D, words.E);
            lavaJumper.Action = LavaJumpersNativeWords.NorfairLavaJumpingEnemyActions.Decode(words.F, "recorded NorfairLavaJumpingEnemyAction");
        }
        if (enemies.MetareeStates[index] is { } metaree)
        {
            metaree.BurrowTimer = UpdateCountdownAccess.Recorded(unchecked((short)words.A));
            metaree.Action = MetareeNativeWords.MetareeActions.Decode(words.B, "recorded MetareeAction");
            RequirePlayingSkreeMetareeAnimation(metaree.Playback, SkreeNativeWords.MetareeAnimationPhases, words, "Metaree");
            // The game keeps a readied attack as a flag beside its preparing routine (#2439), set only while it prepares.
            metaree.Action = (metaree.Action, words.E) switch
            {
                (MetareeAction.PreparingAttack, not 0) => MetareeAction.AttackReady,
                (_, 0) => metaree.Action,
                (var readied, _) => throw new InvalidDataException($"Recorded Metaree {readied} has its attack readied."),
            };
            metaree.YVelocity = Index(words.F, 2734, "Metaree dive speed");
        }
        if (enemies.SparkStates[index] is { } spark)
        {
            spark.Action = SparksNativeWords.SparkActions.Decode(words.B, "recorded SparkAction");
            // The Spark waits its placement's time between actions (#2450), which the recording's copy must be.
            if (words.E != enemy.SpawnSettings<SparkSpawnSettings>().BaseActionTime)
                throw new InvalidDataException($"Recorded Spark wait {words.E} is not its placement's {enemy.SpawnSettings<SparkSpawnSettings>().BaseActionTime}.");
            // The cartridge's countdown expires on the update it would step from one, which the wait's last update stands for.
            spark.ActionTimer = UpdateCountdownAccess.Recorded(unchecked((short)words.F));
        }
        if (enemies.PuyoStates[index] is { } puyo)
        {
            puyo.YSpeedCurvePlace = QuadraticSpeedCurvePlace.FromTwoHundredFiftySixthsOfMark(Index(words.A, short.MaxValue, "Puyo speed index"));
            puyo.HopCooldownTimer = TrueWait(words.B);
            puyo.Action = PuyoNativeWords.PuyoActions.Decode(words.C, "recorded PuyoAction");
            // A Puyo that has not hopped yet records no airborne behaviour; the port chooses one as each hop starts, so a
            // grounded Puyo's is not read before its next hop writes it.
            if (words.D != 0)
                puyo.AirborneMovement = PuyoNativeWords.PuyoAirborneMovements.Decode(words.D, "recorded PuyoAirborneMovement");
            else if (puyo.Action != PuyoAction.Grounded)
                throw new InvalidDataException($"Recorded {puyo.Action} Puyo has no airborne behaviour.");
            puyo.CurrentHop = PuyoNativeWords.HopRecordOffsets.Decode(words.E, "recorded Puyo hop record offset");
        }
    }

    /// <summary>
    /// Translates the family facts some enemies keep among their extra variables rather than their record words (#2473):
    /// a Ki-Hunter body's swipe and wingless flags, a hopper's, a Puyo's and a Beetom's falling flags, a Boyon's bounce
    /// movement (#2531), a Wrecked Ship ghost's watch timers and a Kago's hits left (#2512).
    /// </summary>
    /// <param name="enemy">The imported slot, its record words already imported.</param>
    /// <param name="words">The enemy's six recorded record words.</param>
    /// <param name="extraWord">The enemy's extra variable word at a byte offset from the start of its extra variables.</param>
    internal static void ImportExtraWords(RoomEnemyPlace enemy, NativeEnemyRecordWords words, Func<int, ushort> extraWord)
    {
        if (enemy.SpeciesState<KiHunterBodyState>() is { } kiHunterBody)
        {
            // The swipe flag ($7E:7820) marks a swoop whose swipe has started: the swipe half of the dive.
            if (kiHunterBody.Action == KiHunterAction.Swooping && extraWord(KiHunterSwipeFlagOffset) != 0)
                kiHunterBody.Action = KiHunterAction.Swiping;
            // The wingless flag ($7E:7828) is the body's having lost its wings, which its routine already says.
            if ((extraWord(KiHunterWinglessFlagOffset) != 0) == kiHunterBody.Flies)
                throw new InvalidDataException($"Recorded Ki-Hunter {kiHunterBody.Action} disagrees with its wingless flag ${extraWord(KiHunterWinglessFlagOffset):X4}.");
        }
        if (enemy.SpeciesState<HopperEnemyState>() is { } hopper)
        {
            // The falling flag ($7E:7806) is set only while a hop returns to its surface (#2473).
            bool returning = extraWord(HopperFallingFlagOffset) != 0;
            hopper.Action = (hopper.Action, returning) switch
            {
                (HopperAction.HoppingAway, true) => HopperAction.Returning,
                (_, false) => hopper.Action,
                (var other, true) => throw new InvalidDataException($"Recorded hopper {other} has its falling flag set."),
            };
        }
        // The falling flag ($7E:780A) is an arc's having passed its apex (#2531); a Beetom on the ground keeps the last arc's,
        // which its next hop or lunge clears.
        if (enemy.SpeciesState<BeetomEnemyState>() is { } beetomArc && extraWord(BeetomFallingFlagOffset) != 0 &&
            BeetomArcFalls.TryGetValue(beetomArc.Action, out BeetomAction beetomFall))
            beetomArc.Action = beetomFall;
        if (enemy.SpeciesState<BoyonEnemyState>() is { } boyon)
        {
            // The bounce movement index ($7E:7802) names a bounce's half, one the rise and zero the fall (#2531); the load
            // and every landing leave it rising, so only a bouncing Boyon falls.
            bool boyonFalls = extraWord(BoyonBounceMovementOffset) switch
            {
                0 => true,
                1 => false,
                var other => throw new InvalidDataException($"Recorded Boyon bounce movement index ${other:X4} is neither the rise nor the fall."),
            };
            if (boyon.IsBouncing)
                boyon.Phase = boyonFalls ? BoyonPhase.Falling : BoyonPhase.Rising;
            else if (boyonFalls)
                throw new InvalidDataException($"Recorded Boyon falls in its {boyon.Phase} phase.");
        }
        if (enemy.SpeciesState<PuyoEnemyState>() is { Action: PuyoAction.Rising } puyo && extraWord(PuyoFallingFlagOffset) != 0)
        {
            // The falling flag ($7E:7806) is the airborne Puyo's having passed its hop's apex (#2512); a grounded Puyo keeps
            // the last hop's, which its next hop clears.
            puyo.Action = PuyoAction.Falling;
        }
        if (enemy.DefinitionId == EnemyDefinitionId.Coven && enemy.SpeciesState<WreckedShipGhostEnemyState>() is { } ghost)
        {
            // The lock-on timer ($7E:781A) counts Samus's keeping still, the directed movement timer ($7E:781C) her keeping
            // her direction; the appearance sets both, which the ghost keeps until it watches her again (#2512).
            ghost.StablePositionTimer = UpdateCountdownAccess.Recorded(extraWord(CovenLockOnTimerOffset));
            ghost.StableDirectionTimer = UpdateCountdownAccess.Recorded(extraWord(CovenDirectedMovementTimerOffset));
        }
        if (enemy.EntityState<KagoEntity, KagoEnemyState>() is { } kago)
        {
            // The hit counter ($7E:7808) counts the shots left before the one that kills, from the placement's hit count; the
            // fast animation flag (word B) is the shell's having been hit, which the hits taken say (#2473).
            kago.HitsTaken = enemy.SpawnSettings<KagoSpawnSettings>().HitCount - unchecked((short)extraWord(KagoHitCounterOffset));
            if ((words.B != 0) != (kago.HitsTaken > 0))
                throw new InvalidDataException($"Recorded Kago fast animation flag ${words.B:X4} disagrees with its {kago.HitsTaken} hits taken.");
        }
    }

    /// <summary>The byte offset of a Ki-Hunter body's swipe flag among its extra variables ($7E:7820).</summary>
    private const int KiHunterSwipeFlagOffset = 0x20;

    /// <summary>The byte offset of a Ki-Hunter body's wingless flag among its extra variables ($7E:7828).</summary>
    private const int KiHunterWinglessFlagOffset = 0x28;

    /// <summary>The byte offset of a hopper's falling flag among its extra variables ($7E:7806).</summary>
    private const int HopperFallingFlagOffset = 0x06;

    /// <summary>The byte offset of a Boyon's bounce movement index among its extra variables ($7E:7802).</summary>
    private const int BoyonBounceMovementOffset = 0x02;

    /// <summary>The byte offset of a Beetom's falling flag among its extra variables ($7E:780A).</summary>
    private const int BeetomFallingFlagOffset = 0x0a;

    /// <summary>Each Beetom arc's rise, which its routine word names, and the fall its falling flag tells (#2531).</summary>
    private static readonly Dictionary<BeetomAction, BeetomAction> BeetomArcFalls = new()
    {
        [BeetomAction.ShortHopRising] = BeetomAction.ShortHopFalling,
        [BeetomAction.LongHopRising] = BeetomAction.LongHopFalling,
        [BeetomAction.LungeRising] = BeetomAction.LungeFalling,
    };

    /// <summary>The byte offset of a Wrecked Ship ghost's lock-on timer among its extra variables ($7E:781A).</summary>
    private const int CovenLockOnTimerOffset = 0x1a;

    /// <summary>The byte offset of a Wrecked Ship ghost's directed movement timer among its extra variables ($7E:781C).</summary>
    private const int CovenDirectedMovementTimerOffset = 0x1c;

    /// <summary>The byte offset of a Puyo's falling flag among its extra variables ($7E:7806).</summary>
    private const int PuyoFallingFlagOffset = 0x06;

    /// <summary>The byte offset of a Kago's hit counter among its extra variables ($7E:7808).</summary>
    private const int KagoHitCounterOffset = 0x08;

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
                    body.DeathPhase = KraidDeathPhase.WaitBeforeFadeIn;
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
                lint.LintXOffset = CoordinateAxis.AsSigned(words.C);
                lint.LintTravel = words.B;
                ImportPhaseWait(lint, words.F);
                return;
            case KraidFootState foot:
            {
                // A foot whose setup has not given it a phase rests.
                (KraidFootPhase phase, HorizontalDirection? walk) = recorded
                    ? KraidNativeWords.FootPhases.Decode(words.A, "recorded KraidFootPhase")
                    : (KraidFootPhase.Resting, null);
                foot.Phase = phase;
                // Only the foot's walk names the way it walks (#2165).
                if (walk is { } direction)
                    foot.WalkDirection = direction;
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
