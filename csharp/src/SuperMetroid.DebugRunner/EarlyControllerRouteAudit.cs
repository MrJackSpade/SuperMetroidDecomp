using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// Incremental controller-only playthrough from the completed Landing Site gunship scene.
/// </summary>
/// <remarks>
/// Unlike <see cref="EarlyRouteAudit"/>, this audit is forbidden from publishing a door
/// collision or changing Samus coordinates. It deliberately starts with just the first
/// surface exit; each subsequently proven segment will be appended until the complete
/// Bomb Torizo route runs from ordinary controller samples alone.
/// </remarks>
internal static partial class EarlyControllerRouteAudit
{
    private const int LevelBlockSizePixels = 16;

    // Successful route audits default to concise milestone output. Set SM_ROUTE_TRACE=1
    // when diagnosing a planner regression; failure paths always retain their full trace,
    // collision image, and local block dump regardless of this presentation switch.
    private static bool VerboseDiagnostics { get; } =
        Environment.GetEnvironmentVariable("SM_ROUTE_TRACE") == "1";

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.InitializePostCeresZebesRoom();

        // Keep the historical fast audit available, but make its execution boundary
        // explicit. The same route below can now be hosted by the outer frontend without
        // teaching any room planner about game-state transitions or letting it load a door.
        var host = new ControllerRouteHost(
            StepFrame: input => runtime.StepFrame(input),
            LoadPendingDoor: () => runtime.LoadPendingDoorDestination());
        return RunLoadedRoute(bus, runtime, host);
    }

    /// <summary>
    /// Runs the proven Landing Site-to-Bombs controller policy against an already loaded
    /// post-Ceres runtime. <paramref name="host"/> is the sole authority allowed to advance
    /// time or consume a pending door, so callers can use either the compact runtime loop or
    /// the real outer frontend dispatcher without duplicating route decisions.
    /// </summary>
    internal static int RunLoadedRoute(
        ISnesAddressSpace bus,
        SuperMetroidRuntime runtime,
        ControllerRouteHost host)
    {

        int landingFrames = 0;
        while (runtime.Enemies.LastGunshipEvent != GunshipFrameEvent.LandingCompleted &&
               landingFrames < 1200)
        {
            host.StepFrame(0);
            landingFrames++;
        }
        if (runtime.Enemies.LastGunshipEvent != GunshipFrameEvent.LandingCompleted)
            throw new InvalidDataException("Controller route did not reach gunship control handoff.");

        AssertRoomLayer3FxIsVisible(runtime, RoomFxType.Rain, "Landing Site rain");
        PrintDoorBlocks(bus, runtime, "Landing Site");
        SamusState samus = runtime.Samus ?? throw new InvalidOperationException(
            "Gunship landing did not retain Samus.");
        DriveResult landing = DriveUntilDoor(bus, runtime, host, "Landing Site", maximumFrames: 2400);
        AssertPendingDoor(runtime, 0x8916, 0x92fd, "Landing Site -> Parlor");

        // Loading a collision-published door is the normal outer game-state action. The
        // audit does not identify the destination to the loader and does not edit placement;
        // bank-$83's record supplies both, exactly as the desktop frontend does.
        // Model state $0B's completed source fade: the shared destination loader must run
        // Samus_LoadSuitTargetPalette instead of inheriting this black OBJ palette row.
        for (int color = 192; color < 208; color++)
            runtime.Cgram.SetColor(color, 0);
        host.LoadPendingDoor();
        AssertSuitPaletteReloaded(bus, runtime, samus);
        AssertRoom(runtime, 0x92fd, 0x9314, "Parlor");
        PrintDoorBlocks(bus, runtime, "Parlor");
        Console.WriteLine(
            $"  Parlor camera metadata: scrolls={string.Join(',', runtime.Camera!.Scrolls.Storage.ToArray().Take(runtime.Camera.Scrolls.LogicalCellCount).Select(value => value.ToString("X2")))} " +
            $"scrollers=(${runtime.ActiveRoom!.UpScroller:X2},${runtime.ActiveRoom.DownScroller:X2}) " +
            $"entry=(${runtime.Camera.XPosition:X4},${runtime.Camera.YPosition:X4}) " +
            $"main=$8F:{runtime.ActiveRoom.State.MainCodePointer:X4} setup=$8F:{runtime.ActiveRoom.State.SetupCodePointer:X4}.");
        PrintPlmPopulation(bus, runtime.ActiveRoom.State.PlmPointer);
        Console.WriteLine(
            $"  Loaded scroll PLMs: {string.Join(' ', runtime.Plms.ScrollPlms.Select(scroll => $"{scroll.BlockIndex}/${scroll.DataPointer:X4}"))}.");

        DriveResult parlor = DriveUntilDoor(bus, runtime, host, "Parlor", maximumFrames: 3600);
        AssertPendingDoor(runtime, 0x898e, 0x96ba, "Parlor -> Climb");

        host.LoadPendingDoor();
        AssertRoom(runtime, 0x96ba, 0x96d1, "Climb");
        // A direct-runtime host loads the room between accepted NMIs, whereas the real
        // frontend completes the load inside its door state machine. Publish the first
        // loaded-room PPU snapshot before asserting what the player actually sees.
        host.StepFrame(0);
        AssertRoomLayer3FxIsVisible(runtime, RoomFxType.Fog, "Climb red fog");
        AssertCommonEnemyProjectilePalette(bus, runtime, "Climb");
        PrintDoorBlocks(bus, runtime, "Climb");
        PrintPlmPopulation(bus, runtime.ActiveRoom!.State.PlmPointer);
        DriveResult climb = DriveUntilDoor(bus, runtime, host, "Climb", maximumFrames: 6000);
        AssertPendingDoor(runtime, 0x8b62, 0x975c, "Climb -> Pit");

        host.LoadPendingDoor();
        AssertRoom(runtime, 0x975c, 0x976d, "Pit");
        PrintDoorBlocks(bus, runtime, "Pit");
        PrintPlmPopulation(bus, runtime.ActiveRoom!.State.PlmPointer);
        DriveResult pit = DriveUntilDoor(bus, runtime, host, "Pit", maximumFrames: 2400);
        AssertPendingDoor(runtime, 0x8b86, 0x97b5, "Pit -> elevator room");

        host.LoadPendingDoor();
        AssertRoom(runtime, 0x97b5, 0x97c6, "Elevator to Blue Brinstar");
        PrintDoorBlocks(bus, runtime, "Elevator to Blue Brinstar");
        PrintPlmPopulation(bus, runtime.ActiveRoom!.State.PlmPointer);
        DriveResult elevator = DriveUntilDoor(
            bus,
            runtime,
            host,
            "Elevator to Blue Brinstar",
            maximumFrames: 2400);
        CartridgeDoorHeader elevatorExit = runtime.LevelData?.PendingDoorTransition ??
            throw new InvalidDataException("Elevator departure ended without its pseudo-door.");
        Console.WriteLine(
            $"  Elevator pseudo-door: $83:{elevatorExit.Pointer:X4}, " +
            $"destination=${elevatorExit.DestinationRoomPointer:X4}, flags=${elevatorExit.BitFlags:X2}, " +
            $"orientation=${elevatorExit.Orientation:X2}, PLM=({elevatorExit.PlmX:X2},{elevatorExit.PlmY:X2}), " +
            $"screen=({elevatorExit.DestinationScreenX:X2},{elevatorExit.DestinationScreenY:X2}), " +
            $"distance=${elevatorExit.SamusDistance:X4}, setup=${elevatorExit.SetupCodePointer:X4}.");
        AssertPendingDoor(runtime, 0x8b9e, 0x9e9f, "Elevator -> Morph Ball room");

        host.LoadPendingDoor();
        AssertRoom(runtime, 0x9e9f, 0x9eb1, "Morph Ball room");
        PrintDoorBlocks(bus, runtime, "Morph Ball room");
        PrintPlmPopulation(bus, runtime.ActiveRoom!.State.PlmPointer);
        Console.WriteLine(
            $"  Morph Ball scrolls: {string.Join(',', runtime.Camera!.Scrolls.Storage[..runtime.Camera.Scrolls.LogicalCellCount].ToArray().Select(value => value.ToString("X2")))}.");
        PrintCollisionRegion(runtime, 0x40, 0x59, 0x20, 0x2d);
        DriveResult morphBall = DriveUntilDoor(
            bus,
            runtime,
            host,
            "Morph Ball room",
            maximumFrames: 5000);
        if (!samus.CollectedItems.HasAny(SamusEquipmentFlags.MorphBall) ||
            !samus.EquippedItems.HasAny(SamusEquipmentFlags.MorphBall))
        {
            throw new InvalidDataException(
                "Morph Ball room exited without the cartridge collectible setting both item words.");
        }
        // The elevator at X=$57 is an optional return to Crateria. Normal new-game
        // progression keeps travelling east through the same room and enters the
        // cartridge's pre-Missiles room through door-list entry one.
        AssertPendingDoor(runtime, 0x8eaa, 0x9f11, "Morph Ball -> pre-Missiles");

        host.LoadPendingDoor();
        AssertRoom(runtime, 0x9f11, 0x9f23, "Pre-Missiles room");
        PrintDoorBlocks(bus, runtime, "Pre-Missiles room");
        PrintPlmPopulation(bus, runtime.ActiveRoom!.State.PlmPointer);
        PrintCollisionFeatureRows(runtime, 0x00, 0x0f, 0x00, 0x1a);
        DriveResult preMissiles = DriveUntilDoor(
            bus,
            runtime,
            host,
            "Pre-Missiles room",
            maximumFrames: 2400);
        AssertPendingDoor(runtime, 0x8eda, 0xa107, "Pre-Missiles -> first Missile");

        host.LoadPendingDoor();
        CartridgeRoomHeader firstMissileRoom = runtime.ActiveRoom ??
            throw new InvalidDataException("First Missile door did not publish its room.");
        AssertRoom(runtime, 0xa107, 0xa114, "First Missile room");
        PrintDoorBlocks(bus, runtime, "First Missile room");
        PrintPlmPopulation(bus, firstMissileRoom.State.PlmPointer);
        DriveResult firstMissile = DriveUntilDoor(
            bus,
            runtime,
            host,
            "First Missile room",
            maximumFrames: 2400);
        if (samus.MaxMissiles != 5 || samus.Missiles != 5)
        {
            throw new InvalidDataException(
                $"First Missile collectible left ammo at {samus.Missiles}/{samus.MaxMissiles}; " +
                "expected the cartridge's initial 5/5 capacity.");
        }
        AssertPendingDoor(runtime, 0x8fa6, 0x9f11, "First Missile -> pre-Missiles");

        host.LoadPendingDoor();
        AssertRoom(runtime, 0x9f11, 0x9f23, "Pre-Missiles return");
        AssertSamusWithinDisplayedCamera(runtime, "first Missile return");
        Console.WriteLine(
            $"  Returned to pre-Missiles at Samus (${samus.XPosition:X4},${samus.YPosition:X4}); " +
            $"camera=(${runtime.Camera!.XPosition:X4},${runtime.Camera.YPosition:X4}), " +
            $"scrollers=(${runtime.ActiveRoom!.UpScroller:X2},${runtime.ActiveRoom.DownScroller:X2}), " +
            $"scrolls={string.Join(',', runtime.Camera.Scrolls.Storage.ToArray().Take(runtime.Camera.Scrolls.LogicalCellCount).Select(value => value.ToString("X2")))}, " +
            $"scroll PLMs=[{string.Join(' ', runtime.Plms.ScrollPlms.Select(scroll => $"{scroll.BlockIndex}/{scroll.DataPointer:X4}"))}], " +
            $"colored doors: {string.Join(' ', runtime.Plms.ColoredDoors)}.");
        DriveResult constructionZoneReturn = DriveUntilDoor(
            bus,
            runtime,
            host,
            "Pre-Missiles room",
            maximumFrames: 3600);
        AssertPendingDoor(runtime, 0x8ec2, 0x9e9f, "Pre-Missiles return -> Morph Ball");

        host.LoadPendingDoor();
        AssertRoom(runtime, 0x9e9f, 0x9eb1, "Morph Ball return");
        DriveResult morphBallReturn = DriveUntilDoor(
            bus,
            runtime,
            host,
            "Morph Ball return",
            maximumFrames: 6000);
        AssertPendingDoor(runtime, 0x8eb6, 0x97b5, "Morph Ball return -> elevator");

        host.LoadPendingDoor();
        // The room's event/item state selector deliberately chooses $97E0 on the upward
        // trip after Morph Ball and the first Missile; $97C6 is only the initial descent.
        AssertRoom(runtime, 0x97b5, 0x97e0, "Elevator return");
        DriveResult elevatorReturn = DriveUntilDoor(
            bus,
            runtime,
            host,
            "Elevator to Blue Brinstar",
            maximumFrames: 2400);
        AssertAscendingElevatorCameraIsSynchronized(runtime);
        AssertPendingDoor(runtime, 0x8b92, 0x975c, "Elevator return -> Pit");

        host.LoadPendingDoor();
        AssertRoom(runtime, 0x975c, 0x9787, "Pit return");
        DriveResult pitReturn = DriveUntilDoor(bus, runtime, host, "Pit", maximumFrames: 2400);
        AssertPendingDoor(runtime, 0x8b7a, 0x96ba, "Pit return -> Climb");

        host.LoadPendingDoor();
        // Satisfying Old Mother Brain room's five-Pirate quota runs native PLM
        // pre-instruction $84:BE01, which publishes event zero ("Zebes is awake") before
        // either grey cap can open. Climb therefore selects its event-zero state $96EB;
        // $96D1 is correct only for the outbound, sleeping-world traversal above.
        AssertRoom(runtime, 0x96ba, 0x96eb, "Climb return");
        // The awakened state fills the nine-screen shaft with eleven active Pirates. The
        // deterministic platform finder still performs only legal run/jump/wall-jump input,
        // but enemy contacts make it substantially slower than the empty outbound state.
        // Keep the watchdog finite while allowing that cartridge-authored interference.
        DriveResult climbReturn = DriveUntilDoor(bus, runtime, host, "Climb", maximumFrames: 30000);
        AssertPendingDoor(runtime, 0x8b3e, 0x92fd, "Climb return -> Parlor");

        host.LoadPendingDoor();
        // The same cartridge event also selects Parlor's awakened state. This assertion is
        // intentionally independent of Bomb Torizo's later area-boss bit: merely passing
        // through the quota gate must already have changed the global room-state branch.
        AssertRoom(runtime, 0x92fd, 0x932e, "Parlor return");
        Console.WriteLine(
            $"  Reloaded Parlor PLMs: active={runtime.Plms.ActiveCount}, " +
            $"scrolls=[{string.Join(' ', runtime.Plms.ScrollPlms.Select(scroll => $"{scroll.BlockIndex}/{scroll.DataPointer:X4}"))}], " +
            $"population=$8F:{runtime.ActiveRoom!.State.PlmPointer:X4}.");
        PrintCollisionFeatureRows(runtime, 0x10, 0x1f, 0x00, 0x4c);
        PrintCollisionFeatureRows(runtime, 0x20, 0x4f, 0x00, 0x2a);
        DriveResult parlorReturn = DriveUntilDoor(
            bus,
            runtime,
            host,
            "Parlor return",
            // The awakened state's enemies interrupt long traversal arcs and make the
            // collision-derived route deliberately less deterministic than empty Parlor.
            maximumFrames: 12000);
        AssertPendingDoor(runtime, 0x8982, 0x9879, "Parlor return -> Flyway");

        host.LoadPendingDoor();
        AssertRoom(runtime, 0x9879, 0x9890, "Flyway");
        DriveResult flyway = DriveUntilDoor(bus, runtime, host, "Flyway", maximumFrames: 2400);
        AssertPendingDoor(runtime, 0x8bc2, 0x9804, "Flyway -> Bomb Torizo");

        host.LoadPendingDoor();
        AssertRoom(runtime, 0x9804, 0x981b, "Bomb Torizo room");
        PrintDoorBlocks(bus, runtime, "Bomb Torizo room");
        PrintPlmPopulation(bus, runtime.ActiveRoom!.State.PlmPointer);
        PrintCollisionFeatureRows(runtime, 0, 15, 0, 11);
        Console.WriteLine(
            $"  Bomb Torizo entry: Samus=(${samus.XPosition:X4},${samus.YPosition:X4}), " +
            $"health={samus.Health}, " +
            $"collectibles=[{string.Join(' ', runtime.Plms.Collectibles)}], " +
            $"enemy={runtime.Enemies.BombTorizo}, " +
            $"hand-active={runtime.Plms.HasActiveHeader(0xd6ea)}.");

        int bombPickupFrames = DriveBombTorizoPickup(runtime, host, maximumFrames: 1800);
        BombTorizoAwakeningResult awakening = DriveBombTorizoAwakening(
            runtime,
            host,
            maximumFrames: 1800);
        BombTorizoFightResult fight = DriveBombTorizoFight(
            bus,
            runtime,
            host,
            maximumFrames: 12000);
        VerifyPauseEquipment(bus, runtime, host);

        // A defeated boss is not a playable endpoint unless its ordinary door/room-state
        // consequences work. Leave the arena and Flyway through physical collision after
        // the pause round-trip; this proves the area-Torizo bit opens the cap, selects
        // Flyway's $98AA state, publishes event zero, and selects awakened Parlor $932E.
        DriveResult bombTorizoReturn = DriveUntilDoor(
            bus,
            runtime,
            host,
            "Bomb Torizo return",
            maximumFrames: 2400);
        AssertPendingDoor(runtime, 0x8baa, 0x9879, "Bomb Torizo return -> Flyway");
        host.LoadPendingDoor();
        AssertRoom(runtime, 0x9879, 0x98aa, "Defeated Bomb Torizo Flyway");

        DriveResult flywayReturn = DriveUntilDoor(
            bus,
            runtime,
            host,
            "Flyway return",
            maximumFrames: 2400);
        AssertPendingDoor(runtime, 0x8bb6, 0x92fd, "Flyway return -> awakened Parlor");
        host.LoadPendingDoor();
        AssertRoom(runtime, 0x92fd, 0x932e, "Awakened Parlor after Bomb Torizo");

        Console.WriteLine(
            $"Controller route: gunship {landingFrames} frames; Landing Site -> Parlor " +
            $"after {landing.Frames} ordinary gameplay frames; Parlor -> Climb after " +
            $"{parlor.Frames} more frames; Climb -> Pit after {climb.Frames} more; Pit exit " +
            $"after {pit.Frames} more; elevator descent after {elevator.Frames} more; " +
            $"Morph Ball acquired and the pre-Missiles door reached after " +
            $"{morphBall.Frames} more; pre-Missiles exited after " +
            $"{preMissiles.Frames} more; first Missile collected and room exited after " +
            $"{firstMissile.Frames} more; climbed Construction Zone and returned to the " +
            $"Morph Ball door after {constructionZoneReturn.Frames} more at Samus " +
            $"(${samus.XPosition:X4},${samus.YPosition:X4}); returned to the elevator " +
            $"after {morphBallReturn.Frames} more; elevator/Pit/Climb return took " +
            $"{elevatorReturn.Frames}/{pitReturn.Frames}/{climbReturn.Frames} frames; " +
            $"returned through Parlor and Flyway in {parlorReturn.Frames}/" +
            $"{flyway.Frames} frames; Bombs acquired after {bombPickupFrames} room frames; " +
            $"the item message closed in {awakening.MessageFrames} frames and the " +
            $"cartridge hand sequence released Bomb Torizo after " +
            $"{awakening.SequenceFrames} more frames; Bomb Torizo was defeated after " +
            $"{fight.Frames} controller frames and {fight.FireInputs} fire-button edges; " +
            $"the unlocked arena/Flyway return reached awakened Parlor in " +
            $"{bombTorizoReturn.Frames}/{flywayReturn.Frames} frames.");
        return 0;
    }

    /// <summary>
    /// Creates a route host whose every gameplay tick and door transition passes through
    /// the production outer dispatcher. The door callback intentionally does not call the
    /// runtime loader: it advances state $09/$0B until <see cref="SuperMetroidGame"/> has
    /// consumed the collision-published bank-$83 record and restored normal gameplay.
    /// </summary>
    internal static ControllerRouteHost CreateFrontendHost(SuperMetroidGame game)
    {
        var acknowledgements = new byte[4];

        // This controller audit intentionally has no SPC/DSP process. Emulate only the
        // hardware port handshake by echoing every main-CPU write on the following frame;
        // uploads and PCM synthesis remain covered by the desktop audio smoke tests. The
        // queue therefore drains with the same request/acknowledge/clear sequence instead
        // of deadlocking a door on an acknowledgement a headless runner cannot produce.
        void StepAndAcknowledge(ushort input)
        {
            FrontendFrame frame = game.Step(input);
            foreach (CartridgeAudioCommand command in frame.AudioCommands)
            {
                if (command.Kind == CartridgeAudioCommandKind.WritePort)
                    acknowledgements[command.Port] = command.Value;
            }
            game.SetAudioAcknowledgements(new CartridgeAudioAcknowledgements(
                acknowledgements[0],
                acknowledgements[1],
                acknowledgements[2],
                acknowledgements[3]));
        }

        return new ControllerRouteHost(
        StepFrame: StepAndAcknowledge,
        LoadPendingDoor: () =>
        {
            // `$82:E310` aligns the low byte of the camera coordinate perpendicular to
            // travel one pixel per accepted NMI before any destination-room setup begins.
            // The worst legal starting byte is therefore 128 alignment frames, followed
            // by the explicit setup/load/scroll/nudge phases. Keep a finite, deliberately
            // generous cartridge-derived ceiling so a stalled transition still fails
            // loudly without assuming every source room happens to start tile-aligned.
            const int MaximumDoorDispatcherFrames = 256;
            SuperMetroidRuntime runtime = game.RuntimeForVerification
                ?? throw new InvalidOperationException(
                    "Frontend door transition began without a live runtime.");
            ushort sourceRoomPointer = runtime.ActiveRoom?.Pointer
                ?? throw new InvalidOperationException(
                    "Frontend door transition began without a source room.");
            CartridgeDoorHeader openingDoor = runtime.PendingDoorTransition
                ?? throw new InvalidOperationException(
                    "Frontend door transition began without its bank-$83 header.");
            byte doorOrientation = openingDoor.Orientation;
            bool observedDestinationPublication = false;
            bool observedOpeningSetupStream = false;
            int openingScrollStreamFrames = 0;
            for (int frame = 0;
                frame < MaximumDoorDispatcherFrames &&
                game.GameState != SuperMetroidGameState.MainGameplay;
                frame++)
            {
                DoorTransitionPhase phaseBeforeStep = game.DoorTransitionPhaseForVerification;
                StepAndAcknowledge(0);
                DoorTransitionPhase phaseAfterStep = game.DoorTransitionPhaseForVerification;
                if (phaseBeforeStep == DoorTransitionPhase.LoadMoreThingsAndOpenDoor &&
                    phaseAfterStep == DoorTransitionPhase.WaitForDoorOpeningScroll)
                {
                    observedOpeningSetupStream = runtime.LastBackgroundUpdateCount > 0;
                }
                if (phaseBeforeStep == DoorTransitionPhase.WaitForDoorOpeningScroll &&
                    runtime.LastBackgroundUpdateCount > 0)
                {
                    openingScrollStreamFrames++;
                }
                if (!observedDestinationPublication &&
                    runtime.ActiveRoom?.Pointer is ushort activeRoomPointer &&
                    activeRoomPointer != sourceRoomPointer)
                {
                    observedDestinationPublication = true;

                    // The host renderer reads DisplayedOam, never the main-loop staging
                    // buffer. At the exact dispatcher call that makes a different room
                    // visible, both byte planes must already be the destination draw pass.
                    // A mismatch is the one-frame source-Samus/destination-background tear
                    // this end-to-end route is intended to prevent.
                    if (!runtime.DisplayedOam.LowTable.SequenceEqual(runtime.Oam.LowTable) ||
                        !runtime.DisplayedOam.HighTable.SequenceEqual(runtime.Oam.HighTable))
                    {
                        throw new InvalidDataException(
                            $"Door transition $8F:{sourceRoomPointer:X4} -> " +
                            $"$8F:{activeRoomPointer:X4} exposed destination VRAM before " +
                            "publishing its completed destination OAM image.");
                    }
                }
            }
            if (game.GameState != SuperMetroidGameState.MainGameplay)
            {
                throw new InvalidDataException(
                    $"Frontend door transition stopped in {game.GameState} after " +
                    $"{MaximumDoorDispatcherFrames} dispatcher frames; coroutine phase " +
                    $"is {game.DoorTransitionPhaseForVerification}.");
            }
            if (!observedDestinationPublication)
            {
                throw new InvalidDataException(
                    $"Frontend door dispatcher returned to gameplay without replacing " +
                    $"source room $8F:{sourceRoomPointer:X4}.");
            }
            // `$0783` suppresses normal BG1/BG2 streaming while the Ceres elevator door's
            // Mode-7 IRQ owns the PPU. It is horizontal in the header but deliberately has
            // no ordinary boundary-column traffic, so it is not a valid specimen for the
            // left/right tilemap-ring regression.
            if ((doorOrientation & 2) == 0 && !openingDoor.UsesCeresElevatorMode7 &&
                (!observedOpeningSetupStream || openingScrollStreamFrames < 15))
            {
                throw new InvalidDataException(
                    $"Horizontal door $8F:{sourceRoomPointer:X4} orientation " +
                    $"{doorOrientation & 3} streamed setup={observedOpeningSetupStream} and " +
                    $"only {openingScrollStreamFrames}/15 subsequent boundary columns. " +
                    "A missing request leaves a stale source-door column in the VRAM ring buffer.");
            }
        },
        Frontend: game);
    }

    /// <summary>
    /// Uses ordinary rightward movement and beam input to open Bomb Torizo's Chozo orb and
    /// touch the exposed item. Coordinates are observed only for diagnostics; collision,
    /// projectile impact, orb animation, and pickup remain owned by live cartridge systems.
    /// </summary>
    /// <summary>
    /// Fights Bomb Torizo using only ordinary controller words. Enemy position and health
    /// are read to make the deterministic test driver behave like a cautious player, but
    /// the driver never writes actor, projectile, collision, boss-bit, or death state. Every
    /// point of damage must therefore originate in the live arm cannon and travel through
    /// ordinary projectile/enemy collision before the ROM-backed death list runs. Bomb
    /// Torizo's retail vulnerability table assigns normal bombs multiplier zero, so using
    /// the newly acquired item offensively here would only exercise valid dud collisions.
    /// </summary>
    private static BombTorizoFightResult DriveBombTorizoFight(
        ISnesAddressSpace bus,
        SuperMetroidRuntime runtime,
        ControllerRouteHost host,
        int maximumFrames)
    {
        SamusState samus = runtime.Samus ?? throw new InvalidOperationException(
            "Bomb Torizo fight began without Samus.");
        TorizoEnemyState torizo = runtime.Enemies.BombTorizo ??
            throw new InvalidOperationException("Bomb Torizo fight began without its enemy.");
        ushort initialEnemyHealth = torizo.Slot.Health;
        ushort initialSamusHealth = samus.Health;
        ushort initialMissiles = samus.Missiles;
        ushort previousEnemyHealth = initialEnemyHealth;
        ushort previousBossFunction = torizo.Function;
        ushort previousBossPreInstruction = torizo.PreInstruction;
        ushort previousShotGuard = torizo.ShotGuard;
        int damagingHits = 0;
        int fireInputs = 0;
        int fireCooldown = 0;
        int orbAimHoldFrames = 0;
        int targetedOrbSlotIndex = -1;
        int dodgeFramesRemaining = 0;
        SnesButton dodgeDirection = 0;
        bool dodgeReady = true;
        bool sawDeathMusic = false;
        var naturalAttacks = new HashSet<RoomEnemyProjectileKind>();
        // A Chozo orb is a deliberate refill opportunity, not merely another hazard. Do
        // not, however, chase every spread throughout the fight: doing so roughly doubles
        // encounter length and exposes Samus to more swipes than the drops can repay. Below
        // forty energy, the same ordinary shoot/collect loop becomes worth that risk.
        const ushort ResourceHuntHealth = 40;

        for (int frame = 0; frame < maximumFrames; frame++)
        {
            ushort healthBeforeFrame = samus.Health;
            if (fireCooldown != 0)
                fireCooldown--;
            ushort input = 0;
            bool liveOrbNeedsBeam = samus.Health < ResourceHuntHealth &&
                runtime.Enemies.EnemyProjectiles.Any(projectile =>
                    projectile.IsActive &&
                    projectile.Kind == RoomEnemyProjectileKind.BombTorizoChozoOrb);
            if (liveOrbNeedsBeam && samus.SelectedHudItem != 0)
            {
                // Chozo orbs accept the ordinary beam and exist to restore resources. Do
                // not squander a newly dropped missile on one: with no Supers/Power Bombs
                // on this early route, another retail Select edge returns directly to Beam.
                input = (ushort)SnesButton.Select;
            }
            else if (!liveOrbNeedsBeam && samus.SelectedHudItem == 0 && samus.Missiles != 0)
            {
                // This is the same Select edge a player uses. It selects both the route's
                // initial five missiles and any ammunition later restored by native enemy
                // drops after depletion automatically returned the HUD to Beam.
                input = (ushort)SnesButton.Select;
            }
            else if (!torizo.DeathStarted)
            {
                int signedDistance = torizo.Slot.XPosition - samus.XPosition;
                int absoluteDistance = Math.Abs(signedDistance);
                SnesButton towardBoss = signedDistance >= 0 ? SnesButton.Right : SnesButton.Left;
                SnesButton awayFromBoss = signedDistance >= 0 ? SnesButton.Left : SnesButton.Right;
                bool bossAcceptsShots = torizo.Function == 0xc6ff && torizo.ShotGuard == 0;
                SamusMovementType movement = samus.ReadMovementKind(bus);
                bool canStartGroundedVault = movement is
                    SamusMovementType.Standing or
                    SamusMovementType.Running or
                    SamusMovementType.Crouching or
                    SamusMovementType.TurningOnGround or
                    SamusMovementType.Moonwalking or
                    SamusMovementType.RanIntoWall;
                bool samusFacesLeft = SamusState.IsFacingLeft(bus, samus.Pose);
                bool samusFacesBoss = signedDistance < 0 == samusFacesLeft;
                RoomEnemyProjectileSlot? safePickup = runtime.Enemies.EnemyProjectiles
                    .Where(projectile =>
                        projectile.IsActive &&
                        projectile.Kind == RoomEnemyProjectileKind.EnemyDeathPickup &&
                        IsOnSafeSideOfSamus(projectile.XPosition, samus.XPosition, signedDistance))
                    .OrderBy(projectile => Math.Abs(projectile.XPosition - samus.XPosition))
                    .FirstOrDefault();
                // A refill is not "safe" merely because its projectile kind says pickup.
                // Torizo emits the spread from inside the approach range. A pickup or orb
                // between Samus and the boss is bait, not a safe refill: walking toward it
                // was responsible for two deterministic deaths in the private-ROM route.
                // Only collect from nearly opposite sides of the arena and from drops
                // already behind Samus relative to Torizo. Incoming Chozo orbs are handled
                // separately below: shooting a threat in front is safe much sooner than
                // walking forward to touch its eventual refill. The actual collisions still
                // decide whether either controller-driven attempt succeeds.
                bool resourceLaneIsSafe = absoluteDistance >= 144;
                bool orbDefenseLaneIsSafe = absoluteDistance >= 80;
                bool collectingPickup = samus.Health < ResourceHuntHealth &&
                    safePickup is not null && resourceLaneIsSafe;
                // Keep aiming at one native enemy-projectile slot until it disappears.
                // The six-orb spread often puts equally near targets on opposite sides of
                // Samus; re-sorting every frame made the controller alternate turn poses
                // and never reach the subsequent shoulder-aim/fire edge. Slot identity is
                // observation only and remains valid for exactly as long as that live orb.
                RoomEnemyProjectileSlot? chozoOrb = runtime.Enemies.EnemyProjectiles
                    .FirstOrDefault(projectile =>
                        projectile.SlotIndex == targetedOrbSlotIndex &&
                        projectile.IsActive &&
                        projectile.Kind == RoomEnemyProjectileKind.BombTorizoChozoOrb);
                if (chozoOrb is null)
                {
                    chozoOrb = runtime.Enemies.EnemyProjectiles
                        .Where(projectile =>
                            projectile.IsActive &&
                            projectile.Kind == RoomEnemyProjectileKind.BombTorizoChozoOrb)
                        .OrderBy(projectile => Math.Abs(projectile.XPosition - samus.XPosition))
                        .FirstOrDefault();
                    targetedOrbSlotIndex = chozoOrb?.SlotIndex ?? -1;
                    orbAimHoldFrames = 0;
                }
                bool huntingOrb = samus.Health < ResourceHuntHealth &&
                    chozoOrb is not null && orbDefenseLaneIsSafe;
                // Simply retreating eventually pins Samus against a one-screen room wall.
                // The bounded controller vault keeps one direction after the actors cross;
                // recalculating toward/away in mid-air would reverse back into Torizo.
                if (collectingPickup)
                {
                    // Shot Chozo orbs execute $86:AB8A and create ordinary enemy-drop
                    // projectiles. Low-energy players should actually collect those drops;
                    // reading their live coordinates only chooses a D-pad direction and
                    // never grants health or alters the pickup slot from the audit.
                    dodgeFramesRemaining = 0;
                    dodgeReady = true;
                    orbAimHoldFrames = 0;
                    int pickupDistance = safePickup!.XPosition - samus.XPosition;
                    if (Math.Abs(pickupDistance) > 5)
                    {
                        input = (ushort)(pickupDistance > 0
                            ? SnesButton.Right
                            : SnesButton.Left);
                    }
                    if (safePickup.YPosition + 6 < samus.YPosition && frame % 48 < 24)
                    {
                        // Enemy drops spend much of their lifetime above standing Samus.
                        // A released half-cycle preserves real new-button edges on repeated
                        // attempts; the pickup's own collision decides whether an arc reaches it.
                        input |= (ushort)SnesButton.A;
                    }
                }
                else if (huntingOrb)
                {
                    // Bomb Torizo's thrown orbs are intended resource targets. Face the
                    // moving object first, then hold the retail shoulder aim for one full
                    // pose-update frame before adding a Fire edge. As with boss shots, this
                    // avoids pretending same-frame input can retroactively rotate a beam.
                    dodgeFramesRemaining = 0;
                    dodgeReady = true;
                    int orbXDistance = chozoOrb!.XPosition - samus.XPosition;
                    int orbYDistance = chozoOrb.YPosition - samus.YPosition;
                    bool facesOrb = orbXDistance < 0 == samusFacesLeft;
                    if (!facesOrb)
                    {
                        input = (ushort)(orbXDistance < 0
                            ? SnesButton.Left
                            : SnesButton.Right);
                        orbAimHoldFrames = 0;
                    }
                    else
                    {
                        // Stored projectile/body origins are not muzzle coordinates. An orb
                        // resting at Y=$C6 is only eleven pixels below standing Samus's
                        // Y=$BB origin, but it is substantially below her arm cannon. Use the
                        // native diagonal-down pose for even that small positive body delta.
                        SnesButton aim = orbYDistance < -18
                            ? SnesButton.R
                            : orbYDistance > 6
                                ? SnesButton.L
                                : 0;
                        input = (ushort)aim;
                        if (orbAimHoldFrames != 0 && fireCooldown == 0)
                        {
                            input |= (ushort)SnesButton.X;
                            fireInputs++;
                            fireCooldown = 4;
                        }
                        orbAimHoldFrames++;
                    }
                }
                else if (!bossAcceptsShots)
                {
                    orbAimHoldFrames = 0;
                    dodgeFramesRemaining = 0;
                    dodgeReady = true;
                    if (torizo.Function == 0xc6ab)
                    {
                        // `$C6AB` owns the low-health interruption and death wind-up. The
                        // old generic "return left" behavior walked Samus straight into a
                        // temporarily idle boss at one energy. Retreat from the live origin
                        // while the cartridge list, palette breakup, and shot guard proceed.
                        input = (ushort)(awayFromBoss | SnesButton.B);
                    }
                    else if (torizo.ShotGuard == 0)
                    {
                        // The stand-up list clears $0FAA sixteen frames before active
                        // `$C6FF`, providing a cartridge-authored running-jump run-up.
                        input = (ushort)(towardBoss | SnesButton.B);
                    }
                    else if (samus.XPosition > 48)
                        input = (ushort)SnesButton.Left;
                }
                else if (!dodgeReady && absoluteDistance > 96)
                {
                    dodgeReady = true;
                }
                else if (dodgeReady && dodgeFramesRemaining == 0 &&
                         absoluteDistance < 144 && canStartGroundedVault)
                {
                    dodgeFramesRemaining = 64;
                    dodgeDirection = towardBoss;
                    dodgeReady = false;
                }
                if (!huntingOrb && !collectingPickup &&
                    bossAcceptsShots && dodgeFramesRemaining != 0)
                {
                    input = (ushort)(dodgeDirection | SnesButton.B);
                    if (dodgeFramesRemaining > 32)
                        input |= (ushort)SnesButton.A;
                    dodgeFramesRemaining--;
                }
                else if (!huntingOrb && !collectingPickup &&
                         bossAcceptsShots && absoluteDistance < 80)
                {
                    input |= (ushort)(awayFromBoss | SnesButton.B);
                }
                else if (!huntingOrb && !collectingPickup &&
                         bossAcceptsShots && absoluteDistance > 128)
                {
                    input |= (ushort)(towardBoss | SnesButton.B);
                }

                // Power-beam shots are one-frame edges; a held X would charge and would no
                // longer prove that the regular producer repeatedly allocates and disposes
                // projectile slots. Fire only while there is room to face the boss safely.
                if (!huntingOrb && !collectingPickup &&
                    dodgeFramesRemaining == 0 && bossAcceptsShots &&
                    absoluteDistance >= 64 && !samusFacesBoss)
                {
                    // Input alpha selects a new pose, but the projectile producer still sees
                    // the old pose until UpdateSamusPose late in the frame. Turn first; never
                    // pretend a simultaneous direction+X chord can rotate an existing shot.
                    input = (ushort)towardBoss;
                }
                if (!huntingOrb && !collectingPickup && bossAcceptsShots && samusFacesBoss)
                {
                    // Horizontal fire intersects the standing actor from Samus's ordinary
                    // floor poses. During either actor's jump, however, the same muzzle row
                    // can pass entirely above or below the active extended spritemap. Hold
                    // the retail shoulder aim from the live relative origins; pose update
                    // gets at least one frame to establish it before the next four-frame
                    // Fire edge, and the ROM projectile tables still choose the trajectory.
                    int bossYDistance = torizo.Slot.YPosition - samus.YPosition;
                    if (bossYDistance > 32)
                        input |= (ushort)SnesButton.L;
                    else if (bossYDistance < -40)
                        input |= (ushort)SnesButton.R;
                }
                if (!huntingOrb && !collectingPickup && bossAcceptsShots &&
                    absoluteDistance >= 40 && samusFacesBoss && fireCooldown == 0)
                {
                    // Firing is independent of locomotion on the controller. Preserve a
                    // simultaneous run/vault direction and add X only after the cartridge
                    // pose already faces Torizo; the projectile producer therefore emits in
                    // the established direction while ordinary movement continues.
                    input |= (ushort)SnesButton.X;
                    fireInputs++;
                    fireCooldown = 4;
                }

            }

            host.StepFrame(input);
            if (VerboseDiagnostics &&
                (torizo.Function != previousBossFunction ||
                 torizo.PreInstruction != previousBossPreInstruction ||
                 torizo.ShotGuard != previousShotGuard))
            {
                Console.WriteLine(
                    $"    boss state f{frame + 1}: function " +
                    $"${previousBossFunction:X4}->${torizo.Function:X4}, guard " +
                    $"${previousShotGuard:X4}->${torizo.ShotGuard:X4}, " +
                    $"pre=${previousBossPreInstruction:X4}->${torizo.PreInstruction:X4}, " +
                    $"Samus=(${samus.XPosition:X4},${samus.YPosition:X4})/p${samus.Pose:X2}, " +
                    $"boss=(${torizo.Slot.XPosition:X4},${torizo.Slot.YPosition:X4}), " +
                    $"instruction=${torizo.Slot.CurrentInstruction:X4}.");
                previousBossFunction = torizo.Function;
                previousBossPreInstruction = torizo.PreInstruction;
                previousShotGuard = torizo.ShotGuard;
            }
            if (VerboseDiagnostics && samus.Health < healthBeforeFrame)
            {
                // Keep the private-ROM route diagnosable without reaching into collision
                // state: the frame's controller word, authored actor positions, and live
                // projectile kinds identify which ordinary interaction caused each loss.
                string liveProjectileTrace = string.Join(
                    ' ',
                    runtime.Enemies.EnemyProjectiles
                        .Where(projectile => projectile.IsActive)
                        .Select(projectile =>
                            $"{projectile.Kind}@{projectile.XPosition:X2}/{projectile.YPosition:X2}"));
                Console.WriteLine(
                    $"    damage f{frame + 1}: {healthBeforeFrame}->{samus.Health}, " +
                    $"Samus=(${samus.XPosition:X4},${samus.YPosition:X4})/p${samus.Pose:X2}/" +
                    $"inv={samus.InvincibilityTimer}, boss=(${torizo.Slot.XPosition:X4}," +
                    $"${torizo.Slot.YPosition:X4})/d={Math.Abs(torizo.Slot.XPosition - samus.XPosition)}/" +
                    $"fn=${torizo.Function:X4}/ins=${torizo.Slot.CurrentInstruction:X4}, " +
                    $"input=${input:X4}, projectiles=[{liveProjectileTrace}].");
            }
            if (frame == 0 && initialMissiles != 0 && samus.SelectedHudItem != 1)
            {
                throw new InvalidDataException(
                    $"Controller Select edge did not choose Missiles for Bomb Torizo; " +
                    $"HUD selection is {samus.SelectedHudItem}.");
            }
            foreach (RoomEnemyProjectileSlot projectile in runtime.Enemies.EnemyProjectiles)
            {
                if (projectile.IsActive)
                    naturalAttacks.Add(projectile.Kind);
            }
            sawDeathMusic |= runtime.Enemies.LastBombTorizoMusicRequest is
                { Track: 3, DelayFrames: 8 };

            if (torizo.Slot.Health < previousEnemyHealth)
            {
                damagingHits++;
                if (VerboseDiagnostics)
                {
                    Console.WriteLine(
                        $"    boss hit f{frame + 1}: {previousEnemyHealth}->{torizo.Slot.Health}, " +
                        $"Samus=({samus.XPosition:X2}/{samus.YPosition:X2})/p${samus.Pose:X2}, " +
                        $"boss=({torizo.Slot.XPosition:X2}/{torizo.Slot.YPosition:X2})/" +
                        $"ext=${torizo.Slot.SpritemapPointer:X4}, input=${input:X4}, " +
                        $"spawn={FormatSpawn(runtime.Projectiles.LastFiredProjectileSnapshot)}.");
                }
                previousEnemyHealth = torizo.Slot.Health;
            }
            if (samus.Health == 0)
            {
                throw new InvalidDataException(
                    $"Samus died during controller-driven Bomb Torizo combat at frame " +
                    $"{frame + 1}; boss health={torizo.Slot.Health}, inputs={fireInputs}.");
            }
            if (VerboseDiagnostics && frame % 300 == 299)
            {
                Console.WriteLine(
                    $"  Bomb Torizo fight f{frame + 1}: Samus=(${samus.XPosition:X4}," +
                    $"${samus.YPosition:X4})/{samus.Health}/p${samus.Pose:X2}, " +
                    $"bombs={runtime.BombProjectiles.Slots.Count(slot => slot.IsActive)}, " +
                    $"pickups={runtime.Enemies.EnemyProjectiles.Count(projectile => projectile.IsActive && projectile.Kind == RoomEnemyProjectileKind.EnemyDeathPickup)}, " +
                    $"orbs={runtime.Enemies.EnemyProjectiles.Count(projectile => projectile.IsActive && projectile.Kind == RoomEnemyProjectileKind.BombTorizoChozoOrb)}, " +
                    $"orb-pos=[{string.Join(' ', runtime.Enemies.EnemyProjectiles.Where(projectile => projectile.IsActive && projectile.Kind == RoomEnemyProjectileKind.BombTorizoChozoOrb).Select(projectile => $"{projectile.XPosition:X2}/{projectile.YPosition:X2}"))}], " +
                    $"orb-drops={runtime.Enemies.TorizoOrbDropRequests.Count}, " +
                    $"boss=(${torizo.Slot.XPosition:X4}," +
                    $"${torizo.Slot.YPosition:X4})/{torizo.Slot.Health}, " +
                    $"ammo={samus.Missiles}/{samus.SelectedHudItem}, " +
                    $"function=${torizo.Function:X4}, instruction=${torizo.Slot.CurrentInstruction:X4}, " +
                    $"hits={damagingHits}, fire={fireInputs}, attacks=[" +
                    $"{string.Join(',', naturalAttacks)}].");
            }

            if (!torizo.BossBitSet)
                continue;
            if (!torizo.DeathStarted || torizo.Slot.Health != 0 ||
                !torizo.ItemDropRequested || !sawDeathMusic)
            {
                throw new InvalidDataException(
                    $"Bomb Torizo set its boss bit with incomplete death state: " +
                    $"health={torizo.Slot.Health}, death={torizo.DeathStarted}, " +
                    $"drop={torizo.ItemDropRequested}, music={sawDeathMusic}.");
            }
            if (!runtime.System.HasAnyBossBits(0, BossBits.AreaTorizo))
            {
                throw new InvalidDataException(
                    "Bomb Torizo death did not persist Crateria's area-Torizo boss bit.");
            }
            if (damagingHits == 0 || initialEnemyHealth <= torizo.Slot.Health)
            {
                throw new InvalidDataException(
                    "Bomb Torizo died without any controller-produced beam, missile, or bomb damage.");
            }

            Console.WriteLine(
                $"  Bomb Torizo fight: frames={frame + 1}, fire={fireInputs}, " +
                $"damage-events={damagingHits}, Samus={initialSamusHealth}->{samus.Health}, " +
                $"missiles={initialMissiles}->{samus.Missiles}, " +
                $"attacks=[{string.Join(',', naturalAttacks)}], boss-bit persisted.");
            return new BombTorizoFightResult(frame + 1, fireInputs, damagingHits);
        }

        throw new InvalidDataException(
            $"Bomb Torizo remained undefeated after {maximumFrames} controller frames; " +
            $"health={torizo.Slot.Health}/{initialEnemyHealth}, Samus={samus.Health}, " +
            $"fire={fireInputs}, damage-events={damagingHits}, function=${torizo.Function:X4}, " +
            $"instruction=${torizo.Slot.CurrentInstruction:X4}.");
    }

    /// <summary>
    /// Returns whether a resource projectile lies on the side of Samus opposite Torizo.
    /// This is deliberately a controller-policy predicate, not collision authority: it
    /// prevents the route driver from walking through the boss to chase a tempting drop,
    /// while the runtime still owns projectile motion, collection, and health changes.
    /// </summary>
    private static bool IsOnSafeSideOfSamus(
        ushort projectileX,
        ushort samusX,
        int signedBossDistance) => signedBossDistance >= 0
            ? projectileX <= samusX
            : projectileX >= samusX;

    private static DriveResult DriveUntilDoor(
        ISnesAddressSpace bus,
        SuperMetroidRuntime runtime,
        ControllerRouteHost host,
        string roomName,
        int maximumFrames)
    {
        SamusState samus = runtime.Samus ?? throw new InvalidOperationException(
            $"{roomName} controller drive began without Samus.");
        ushort previousX = samus.XPosition;
        ushort previousY = samus.YPosition;
        int lastHorizontalDelta = 0;
        int stationaryFrames = 0;
        int horizontallyStationaryFrames = 0;
        int firedShots = 0;
        int collisionExplosions = 0;
        int outboundClimbDownAimFrames = 0;
        int outboundClimbDownShotPulses = 0;
        int outboundClimbPostShotLandings = 0;
        int lastOutboundClimbDownShotFrame = int.MinValue;
        ushort healthBeforeFrame = samus.Health;
        var damageTrace = new List<string>();
        var climbCombatTrace = new List<string>();
        int climbThreatWaitFrames = 0;
        int jumpHoldFrames = 0;
        int floorHatchCycle = -1;
        int climbJumpHoldFrames = 0;
        int climbGroundedRunupFrames = 0;
        int climbAirborneFrames = 0;
        int climbSteeringDelayFrames = 0;
        int climbApproachX = 0x0178;
        int climbTargetSurfaceY = 0;
        int climbPlanningSupportSurfaceY = 0;
        int? climbTargetX = null;
        bool climbTargetIsDoorApproach = false;
        bool climbTargetIsSolidLedge = false;
        bool climbUsingWallJumps = false;
        bool climbWallTargetIsRight = true;
        SnesButton climbWallContactDirection = 0;
        int climbWallJumpButtonHoldFrames = 0;
        bool climbHoldingTriggeredWallJump = false;
        bool climbWallLandingEnabled = false;
        bool climbWaitingAboveWallLanding = false;
        bool climbWallJumpHasLandingTarget = false;
        bool climbLaunchAwayFromAdjacentWall = false;
        int climbWallLandingX = 0;
        int climbWallLandingSurfaceY = 0;
        bool climbRequiresGroundApproach = false;
        bool climbLaunchTowardApproach = false;
        bool climbWaitForSurfaceClearance = false;
        bool climbNeedsMomentumRunup = false;
        bool climbRunningTowardLaunch = false;
        bool climbPreserveTargetwardMomentum = false;
        bool climbUseLowArc = false;
        bool climbArcHitCeiling = false;
        bool climbRequiresOutwardLaunch = false;
        bool climbWaitingForOutwardMotion = false;
        int climbRunupStartX = 0;
        int climbRunupLaunchX = 0;
        bool climbJumpReady = true;
        bool climbWasAirborne = false;
        bool parlorUpperShaftCleared = false;
        bool parlorDescentLipCleared = false;
        int parlorLaneTargetX = -1;
        int parlorBestLaneDistance = int.MaxValue;
        int parlorLaneStallFrames = 0;
        int parlorDeepestY = 0;
        int parlorDescentStallFrames = 0;
        bool parlorMorphTunnelActive = false;
        bool parlorMorphTunnelEnteredBall = false;
        int parlorMorphTunnelInputFrames = 0;
        int parlorMorphTunnelExitX = -1;
        var rejectedClimbTargets = new HashSet<(int X, int SurfaceY)>();
        var lowArcClimbTargets = new HashSet<(int X, int SurfaceY)>();
        var failedClimbTargetCounts = new Dictionary<(int X, int SurfaceY), int>();
        // Landing Site, Parlor, and Climb begin by travelling left or descending
        // from a leftward approach. Pit is entered through its left cap and the
        // cartridge route continues to the right.
        bool returningWithMorphBallAtEntry = samus.CollectedItems.HasAny(
            SamusEquipmentFlags.MorphBall);
        bool verifiedOutboundElevatorArtwork = false;
        SnesButton horizontalDirection = roomName switch
        {
            "Pit" when returningWithMorphBallAtEntry => SnesButton.Left,
            "Pit" or "Elevator to Blue Brinstar" or "Pre-Missiles room" or
                "Parlor return" or "Flyway" => SnesButton.Right,
            _ => SnesButton.Left,
        };
        var firedByDirection = new int[16];
        var collisionsByDirection = new int[16];
        var floorHatchShotTrace = new List<string>();
        var morphBallShotTrace = new List<string>();
        var morphBallCollisionTrace = new List<string>();
        int frame = 0;
        var routeTrace = new List<string>();
        int previousScreenX = -1;
        int previousScreenY = -1;
        int previousScrollPlmCount = runtime.Plms.ScrollPlms.Count;
        ElevatorActorStatus previousElevatorStatus = runtime.Enemies.ElevatorStatus;
        int preMissilesAscentStage = 0;
        int preMissilesStageEnteredFrame = 0;
        bool verifiedMorphBallBeforePickup = false;

        while (!runtime.HasPendingDoorTransition && frame < maximumFrames)
        {
            // The outbound new-game descent trends left in both Landing Site and Parlor.
            // A fresh shot every 24 frames opens encountered blue caps. If collision has
            // held both coordinates still for half a second, one ordinary Jump edge clears
            // low terrain; ROM pose tables and collision remain the only authorities that
            // can accept it. This script is intentionally simple so each room that needs a
            // different directional decision becomes an explicit later route segment.
            // Parlor's upper approach ends on a small left-wall ledge. Continuing to hold
            // left there cannot enter the descending zig-zag shaft: the player must cross
            // each ledge and walk off its opposite edge. Reverse only after the ordinary
            // horizontal collision code has held X fixed for twenty frames. This is an
            // input policy, not a coordinate/path shortcut; slopes, gravity, and walls are
            // still resolved by the translated game systems on every frame.
            if ((roomName is "Parlor" or "Climb") &&
                !(roomName == "Climb" && returningWithMorphBallAtEntry) &&
                samus.YPosition >= 0x0100 &&
                horizontallyStationaryFrames == 20)
            {
                horizontalDirection = horizontalDirection == SnesButton.Left
                    ? SnesButton.Right
                    : SnesButton.Left;
            }

            bool aboveParlorFloorHatch = roomName == "Parlor" &&
                samus.YPosition >= 0x0480;
            bool approachingBlueBrinstarElevator =
                roomName == "Elevator to Blue Brinstar";
            bool crossingMorphBallRoom = roomName == "Morph Ball room";
            bool returningAcrossMorphBallRoom = roomName == "Morph Ball return";
            bool descendingPreMissiles = roomName == "Pre-Missiles room";
            bool collectingFirstMissile = roomName == "First Missile room";
            bool climbAlignedForJump = false;
            bool climbStartingJump = false;
            SnesButton climbJumpDirection = 0;
            ushort input;
            if (aboveParlorFloorHatch)
            {
                // The Climb exit is a downward-facing blue cap. Center over its four-block
                // width, jump, hold Down to select the cartridge-authored downward aerial
                // pose, and pulse Fire into the cap. No audit-side PLM spawn is permitted.
                input = samus.XPosition switch
                {
                    < 0x0174 => (ushort)SnesButton.Right,
                    > 0x018c => (ushort)SnesButton.Left,
                    _ => 0,
                };
                bool centered = samus.XPosition is >= 0x0174 and <= 0x018c;
                floorHatchCycle = centered ? (floorHatchCycle + 1) % 60 : -1;
                if (floorHatchCycle is >= 0 and < 8)
                {
                    // Jump must be established before Down is introduced. Pressing both
                    // from a grounded pose selects crouch in the retail transition table.
                    input |= (ushort)SnesButton.A;
                }
                else if (floorHatchCycle is >= 8 and < 50)
                {
                    input |= (ushort)SnesButton.Down;
                    if (floorHatchCycle % 6 == 0)
                        input |= (ushort)SnesButton.X;
                }
            }
            else if (approachingBlueBrinstarElevator)
            {
                bool returningWithMorphBall = samus.CollectedItems.HasAny(
                    SamusEquipmentFlags.MorphBall);
                if (returningWithMorphBall)
                {
                    // On the return trip the destination actor first carries locked Samus
                    // back to its rest point. Left input is harmless during that ownership
                    // window and becomes ordinary movement toward Pit as soon as the actor
                    // restores controls. Periodic shots reopen any cartridge door cap.
                    input = (ushort)SnesButton.Left;
                    if (frame % 24 == 0)
                        input |= (ushort)SnesButton.X;
                }
                else
                {
                    // The elevator actor waits for a newly-pressed direction only after
                    // the floor's type-$9 pseudo-door collision has set $0E16. Walk onto
                    // the two cartridge-authored platform columns, then pulse Down with
                    // released frames so the actor owns the departure.
                    input = samus.XPosition switch
                    {
                        < 0x0074 => (ushort)SnesButton.Right,
                        > 0x008c => (ushort)SnesButton.Left,
                        _ => frame % 30 == 0 ? (ushort)SnesButton.Down : (ushort)0,
                    };
                }
            }
            else if (returningAcrossMorphBallRoom)
            {
                // The acquired-item return enters at the east cap and traverses the same
                // cartridge room westward to the elevator at columns $57/$58. Repeated
                // held/released jumps clear ruined-floor lips; only once Samus is centred
                // over the real platform do pulsed Up edges request its upward departure.
                if (samus.XPosition > 0x0590)
                {
                    input = (ushort)SnesButton.Left;
                    if (frame % 60 < 30)
                        input |= (ushort)SnesButton.A;
                }
                else if (samus.XPosition < 0x0568)
                {
                    input = (ushort)SnesButton.Right;
                }
                else
                {
                    input = frame % 30 == 0 ? (ushort)SnesButton.Up : (ushort)0;
                }
            }
            else if (crossingMorphBallRoom)
            {
                bool hasMorphBall = samus.CollectedItems.HasAny(
                    SamusEquipmentFlags.MorphBall);
                if (!hasMorphBall)
                {
                    // Static face blocks and the ruined floor form several narrow lips on
                    // the westbound approach. A thirty-frame held Jump followed by a full
                    // release produces genuine new-button edges on each attempt and keeps
                    // Left held throughout; bank-$90 owns the resulting arc and collision.
                    if (samus.XPosition > 0x0470)
                    {
                        input = (ushort)SnesButton.Left;
                        if (frame % 60 < 30)
                            input |= (ushort)SnesButton.A;
                    }
                    else
                    {
                        // The item sits above a small solid pedestal. Once horizontally
                        // aligned, counter any carried running momentum from either side,
                        // then use repeatable neutral jumps through its collision box. The
                        // cartridge's acceleration/deceleration still owns the correction.
                        input = samus.XPosition switch
                        {
                            < 0x0440 => (ushort)SnesButton.Right,
                            > 0x0458 => (ushort)SnesButton.Left,
                            _ => frame % 60 < 25
                                ? (ushort)SnesButton.A
                                : (ushort)0,
                        };
                    }
                }
                else
                {
                    RoomCollisionBlock returnBlock = runtime.LevelData!
                        .GetCollisionBlock(0x4c, 0x2c);
                    bool returnBlockStillSolid = returnBlock.CollisionType == 0x0c &&
                        returnBlock.Behavior == 0x04;
                    bool isMorphBall = SamusState.IsGroundedMorphBallPose(samus.Pose) ||
                        SamusState.IsAirborneMorphBallPose(samus.Pose);
                    if (returnBlockStillSolid)
                    {
                        // The return tunnel passes underneath the four-column wall at
                        // X=$4C..$4F. Its cartridge-authored beam block is the floor-level
                        // western cap at (4C,2C), so Samus must first cross the ruined
                        // pedestal pieces and stand immediately west of the wall. Shooting
                        // diagonally from the item pedestal is not equivalent: that line
                        // correctly intersects the wall's ordinary solids several rows
                        // above the tunnel. The retail default maps Aim Down to L (R is
                        // Aim Up), and the projectile/terrain systems alone decide whether
                        // this nearby downward shot actually opens the cap.
                        input = samus.XPosition switch
                        {
                            < 0x04a0 => (ushort)SnesButton.Right,
                            > 0x04b0 => (ushort)SnesButton.Left,
                            _ when samus.IsFacingLeft(bus) => (ushort)SnesButton.Right,
                            _ => (ushort)SnesButton.L,
                        };
                        // Ordinary held jumps clear the two one-block pedestal fragments.
                        // Stop jumping once aligned with the tunnel so the downward beam
                        // begins from the grounded pose expected by the room geometry.
                        if (samus.XPosition < 0x04a0 && frame % 60 < 30)
                            input |= (ushort)SnesButton.A;
                        if (samus.XPosition is >= 0x04a0 and <= 0x04b0 && frame % 12 == 0)
                            input |= (ushort)SnesButton.X;
                    }
                    else if (samus.XPosition >= 0x0530 && isMorphBall)
                    {
                        // The low tunnel ends at a one-block step. There is full standing
                        // headroom here, so use a clean release/Up cycle to invoke the ROM
                        // un-morph transition before attempting the ordinary jump.
                        input = frame % 60 < 20
                            ? (ushort)0
                            : (ushort)SnesButton.Up;
                    }
                    else if (samus.XPosition >= 0x0530)
                    {
                        input = (ushort)SnesButton.Right;
                        if (frame % 60 < 30)
                            input |= (ushort)SnesButton.A;
                        // The east exit is an ordinary blue cap at X=$7E. Keep the same
                        // periodic Fire policy used elsewhere on the route once Samus is
                        // in its final screen; the projectile/PLM collision path remains
                        // solely responsible for opening it.
                        if (samus.XPosition >= 0x0780 && frame % 24 == 0)
                            input |= (ushort)SnesButton.X;
                    }
                    else if (isMorphBall)
                    {
                        input = (ushort)SnesButton.Right;
                    }
                    else
                    {
                        // Left+Down is an aimed-running command, not a morph command. Give
                        // the pose table a clean release window and then Down by itself so
                        // it can perform stand -> crouch -> morph. Repeating the cycle is
                        // necessary because the item-acquisition message temporarily owns
                        // Samus input; the first edge may legitimately occur during it.
                        input = frame % 60 < 20
                            ? (ushort)0
                            : (ushort)SnesButton.Down;
                    }
                }
            }
            else if (descendingPreMissiles)
            {
                if (samus.MaxMissiles != 0)
                {
                    int previousAscentStage = preMissilesAscentStage;
                    if (samus.Kinematics.YDirection == 0 && samus.YPosition < 0x0170)
                    {
                        if (samus.YPosition < 0x0090)
                            preMissilesAscentStage = 3;
                        else if (samus.XPosition >= 0x00a0)
                            preMissilesAscentStage = 1;
                        else if (samus.YPosition < 0x0130 && samus.XPosition <= 0x0080)
                            preMissilesAscentStage = 2;
                    }
                    else if (samus.Kinematics.YDirection == 0 && samus.YPosition >= 0x0170)
                    {
                        // A missed shelf returns to the physical lower floor. Restart the
                        // three-transfer policy instead of applying an upper-shelf steering
                        // decision to a new bottom jump.
                        preMissilesAscentStage = 0;
                    }
                    if (preMissilesAscentStage != previousAscentStage)
                        preMissilesStageEnteredFrame = frame;
                    input = BuildPreMissilesReturnInput(
                        bus,
                        samus,
                        frame,
                        preMissilesAscentStage,
                        frame - preMissilesStageEnteredFrame);
                }
                else if (samus.YPosition < 0x0100)
                {
                    // Permanent shot blocks cover the opening at row $0A, preceded by a
                    // one-block lip at X=$04. Use a complete jump/release cycle to mount
                    // the lip, then establish an aerial pose before holding Down for the
                    // cartridge's straight-down shot. No block is removed here.
                    input = samus.XPosition < 0x0060
                        ? (ushort)SnesButton.Right
                        : samus.XPosition > 0x0090
                            ? (ushort)SnesButton.Left
                            : (ushort)0;
                    int floorShotCycle = frame % 60;
                    if (floorShotCycle < 8)
                        input |= (ushort)SnesButton.A;
                    else if (floorShotCycle < 50)
                    {
                        input |= (ushort)SnesButton.Down;
                        if (floorShotCycle % 6 == 0)
                            input |= (ushort)SnesButton.X;
                    }
                }
                else
                {
                    // The west door is not reachable over the ledge. Cartridge collision
                    // rows $15-$18 contain a three-block dividing wall, with the only drop
                    // at X=$08/$09. Walk right off that ledge, let gravity put Samus below
                    // the wall, then walk left through the lower passage. These thresholds
                    // choose buttons only; the runtime remains responsible for the fall.
                    bool belowDividingWall = samus.YPosition >= 0x0170;
                    SamusMovementType movement = samus.ReadMovementKind(bus);
                    bool morphed = movement is SamusMovementType.MorphBallGround or
                        SamusMovementType.MorphBallFalling;
                    bool touchingWestDoorCap = samus.XPosition <= 0x0030;
                    if (!belowDividingWall)
                    {
                        input = (ushort)SnesButton.Right;
                    }
                    else if (!morphed && !touchingWestDoorCap)
                    {
                        // The lower passage is one block tall, so this is the route's first
                        // mandatory Morph Ball use. A release interval followed by Down
                        // supplies the same stand -> crouch -> morph edges as player input.
                        input = frame % 60 < 20
                            ? (ushort)0
                            : (ushort)SnesButton.Down;
                    }
                    else if (morphed && !touchingWestDoorCap)
                    {
                        input = (ushort)SnesButton.Left;
                    }
                    else if (morphed || movement == SamusMovementType.PostureTransition)
                    {
                        // The arm cannon is unavailable in Morph Ball form. Unmorph in the
                        // west chamber's full-height clearance before addressing the cap.
                        input = (ushort)SnesButton.Up;
                    }
                    else
                    {
                        // Face left and pulse Fire at the cartridge-authored
                        // type-$C/BTS-$41 blue cap.
                        input = (ushort)SnesButton.Left;
                        if (frame % 24 == 0)
                            input |= (ushort)SnesButton.X;
                    }
                }
            }
            else if (collectingFirstMissile)
            {
                // Enter from the east, touch the sole cartridge Missile PLM at block
                // (4,7), wait through its message-owned input lock, and return east. The
                // capacity word—not a timer or coordinate—selects the return leg.
                input = samus.MaxMissiles == 0
                    ? (ushort)SnesButton.Left
                    : (ushort)SnesButton.Right;

                // Header $EF2F is the Missile entry in the Chozo-orb table, not the
                // exposed-item table. Fire opens the shell; continued Left then touches
                // the visible tank after the cartridge's three burst frames.
                if (samus.MaxMissiles == 0 && frame % 24 == 0)
                    input |= (ushort)SnesButton.X;
                else if (samus.MaxMissiles != 0 && frame % 24 == 0)
                    input |= (ushort)SnesButton.X;
            }
            else if (roomName == "Pit" && returningWithMorphBallAtEntry &&
                runtime.Enemies.EnemiesKilled < runtime.Enemies.DeathQuota)
            {
                // State $9787 closes both exits behind a five-enemy death quota. Four
                // walking Pirates naturally cross a horizontal beam fired while travelling
                // west, but the fifth actor begins on the east wall above the entry cannon
                // line. Treat the population as a real encounter: select a live cartridge
                // actor, face/approach it with controller input, and use the configured
                // aim-up shoulder only when its complete hitbox sits above Samus' muzzle.
                // Projectile hitboxes, health, death animation, quota increment, BE01's
                // event publication, and the grey-door opening remain runtime-owned.
                input = BuildPitEnemyQuotaInput(bus, runtime, samus, frame);
            }
            else
            {
                // Climb's last two screens are a special case for the deliberately tiny
                // controller driver, not for gameplay. The long shaft alternates shallow
                // lips on its left and right walls; choosing the direction toward the
                // shaft centre makes Samus walk off whichever lip caught her. Once the
                // bottom screen is reached, the cartridge collision map puts the Pit door
                // at the lower-right edge, so ordinary Right input completes the segment.
                // Coordinates are inspected only to choose controller buttons: movement,
                // falling, collision, scrolling, and door publication all remain owned by
                // the translated runtime.
                bool returningUpClimb = roomName == "Climb" &&
                    returningWithMorphBallAtEntry;
                int currentRouteSurfaceY =
                    samus.YPosition + samus.Kinematics.YRadius;
                bool traversingParlorUpperExit = roomName == "Parlor return" &&
                    !parlorUpperShaftCleared && currentRouteSurfaceY <= 0x00b0;
                bool returningUpParlorShaft = roomName == "Parlor return" &&
                    !parlorUpperShaftCleared && !traversingParlorUpperExit;
                // The screen-boundary wall occupies row $20; the apparent shelf at $0280
                // is only an intermediate landing. A jump onto the wall's top necessarily
                // crosses the exit plane before Samus' feet clear its surface. Retain the
                // ascent owner while a collision-derived target or wall-jump cycle remains
                // active, then switch to ordinary eastbound input above the wall.
                bool returningUpVerticalRoute = returningUpClimb ||
                    returningUpParlorShaft || traversingParlorUpperExit;
                bool descendingLowerClimb = roomName == "Climb" &&
                    !returningUpClimb &&
                    samus.YPosition is >= 0x0700 and < 0x0800;
                bool approachingPitDoor = roomName == "Climb" &&
                    !returningUpClimb &&
                    samus.YPosition >= 0x0800;
                if (descendingLowerClimb)
                {
                    input = samus.XPosition < 0x0180
                        ? (ushort)SnesButton.Right
                        : (ushort)SnesButton.Left;
                }
                else if (approachingPitDoor)
                {
                    input = (ushort)SnesButton.Right;
                }
                else if (traversingParlorUpperExit)
                {
                    // The upper-left slope terminates at the east doorway's elevation.
                    // From here the route is a horizontal gap, not another node in the
                    // "strictly higher floor" graph. Hold Run+Right and request an ordinary
                    // spin jump whenever the native landing state rearms it. This phase is
                    // selected from Samus' collision-resolved foot position; the cartridge
                    // still owns acceleration, the gap, the opposite lip, and publication
                    // of the boundary door once the body genuinely reaches screen two.
                    horizontalDirection = SnesButton.Right;
                    input = (ushort)(SnesButton.Right | SnesButton.B);
                    climbTargetX = null;
                    climbUsingWallJumps = false;
                    climbAlignedForJump = samus.Kinematics.YDirection == 0 &&
                        !SamusState.IsRightFacingLandingPose(samus.Pose) &&
                        !SamusState.IsLeftFacingLandingPose(samus.Pose);
                }
                else if (returningUpVerticalRoute)
                {
                    // Select the nearest authored floor-slope span above the current
                    // support. The driver still supplies buttons only; slope quadrants,
                    // acceleration, gravity, landings, PLM mutation, and the eventual door
                    // trigger remain entirely inside the translated game systems.
                    bool airborne = samus.Kinematics.YDirection != 0;
                    bool waitingForLandingAnimation = !airborne &&
                        (SamusState.IsRightFacingLandingPose(samus.Pose) ||
                         SamusState.IsLeftFacingLandingPose(samus.Pose));
                    if (!airborne)
                    {
                        int currentSupportSurfaceY =
                            samus.YPosition + samus.Kinematics.YRadius;
                        if (!waitingForLandingAnimation && climbTargetX is not null &&
                            !climbNeedsMomentumRunup &&
                            currentSupportSurfaceY > climbPlanningSupportSurfaceY + 4)
                        {
                            // Grounded slope movement can step onto a lower authored floor
                            // without ever publishing an airborne/landing pair. A target
                            // selected from the old elevation is no longer a valid transfer:
                            // its approach coordinate may now lie underneath the shelf.
                            // Invalidate only after a material downward surface change, then
                            // let the same collision-derived selector plan from the body’s
                            // new support. A certified momentum runway is exempt because its
                            // per-pixel probes already proved that a gradual slope belongs to
                            // the same walkable support. No position or velocity is altered.
                            climbTargetX = null;
                            climbGroundedRunupFrames = 0;
                            climbRequiresGroundApproach = false;
                            climbLaunchTowardApproach = false;
                            climbWaitForSurfaceClearance = false;
                            climbNeedsMomentumRunup = false;
                            climbRunningTowardLaunch = false;
                            climbPreserveTargetwardMomentum = false;
                        }
                        if (!waitingForLandingAnimation && climbTargetX is null &&
                            !climbUsingWallJumps)
                        {
                            ClimbPlatformTarget? nextPlatform = FindNextClimbPlatformCenter(
                                bus,
                                runtime.LevelData!,
                                runtime.Plms,
                                samus,
                                preferSupportedApproach: returningUpParlorShaft,
                                rejectedTargets: rejectedClimbTargets);
                            if (nextPlatform is null)
                            {
                                // The return ascent contains cartridge-authored gaps whose
                                // next floor exceeds the ordinary spin-jump envelope. Switch
                                // the input driver to the game's native wall-jump handshake;
                                // neither position nor velocity is patched by the audit.
                                climbUsingWallJumps = true;
                                climbWallTargetIsRight = samus.XPosition >= 0x0180;
                                if (!returningUpParlorShaft)
                                {
                                    // Climb has one exceptional gap, after which the route
                                    // deliberately returns to ordinary shelf planning. Save
                                    // that genuine destination so a successful wall launch
                                    // can cross over and land on it.
                                    ClimbPlatformTarget landingAfterWallJump =
                                        FindNextClimbPlatformCenter(
                                            bus,
                                            runtime.LevelData!,
                                            runtime.Plms,
                                            samus,
                                            maximumRise: 224,
                                            rejectedTargets: rejectedClimbTargets) ??
                                        throw new InvalidDataException(
                                            "Climb wall-jump gap has no cartridge floor or top-door " +
                                            "approach within 224 pixels.");
                                    climbWallLandingX = landingAfterWallJump.LandingX;
                                    climbWallLandingSurfaceY = landingAfterWallJump.SurfaceY;
                                    climbWallJumpHasLandingTarget = true;
                                }
                            }
                            if (nextPlatform is not { } platform)
                                goto BuildClimbWallJumpInput;
                            if (roomName == "Parlor return" && VerboseDiagnostics)
                            {
                                Console.WriteLine(
                                    $"  Selected Parlor climb target " +
                                    $"(${platform.LandingX:X4},${platform.SurfaceY:X4}) " +
                                    $"from ${currentSupportSurfaceY:X4} at frame {frame}; " +
                                    $"rejected={rejectedClimbTargets.Count}.");
                            }
                            climbTargetX = platform.LandingX;
                            climbTargetSurfaceY = platform.SurfaceY;
                            climbPlanningSupportSurfaceY = currentSupportSurfaceY;
                            climbTargetIsDoorApproach = platform.IsDoorApproach;
                            climbTargetIsSolidLedge = platform.IsSolidLedge;
                            climbRequiresOutwardLaunch = false;
                            climbWaitingForOutwardMotion = false;
                            int launchDistance = Math.Abs(platform.LandingX - samus.XPosition);
                            // Samus is only ten pixels wide, but a square slope's solid
                            // quadrant can project much farther than the block selected as
                            // the eventual landing sample. Treat transfers under three
                            // blocks as under-ledged and clear the whole underside first.
                            int targetBlockLeft = platform.ShadowLeftX;
                            int targetBlockRight = platform.ShadowRightX;
                            bool directlyUnderTarget = !platform.IsDoorApproach &&
                                samus.XPosition >=
                                targetBlockLeft - samus.Kinematics.XRadius &&
                                samus.XPosition <= targetBlockRight + samus.Kinematics.XRadius;
                            // Every ordinary landing surface has a solid underside. Even
                            // when Samus starts beside rather than directly below it, moving
                            // toward the landing center before her feet clear the surface
                            // can clip that underside and erase the horizontal momentum the
                            // remaining arc needs. Stay in the collision-proven outside lane
                            // for all non-door targets; the top-door approach has no shelf
                            // crossing and therefore needs no such gate.
                            climbWaitForSurfaceClearance = !platform.IsDoorApproach;
                            // A normal jump cannot pass through the selected ledge. The
                            // approach coordinate is the nearest X whose complete body is
                            // just outside its tile shadow. If Samus begins under that
                            // shadow, reach the coordinate on the ground first. Otherwise
                            // jump immediately and build horizontal speed toward it during
                            // ascent; the hardest retail gap offers only one pixel of
                            // vertical clearance at the apex.
                            climbApproachX = platform.IsDoorApproach
                                ? platform.LandingX
                                : FindClearClimbAscentX(
                                    bus,
                                    runtime.LevelData!,
                                    runtime.Plms,
                                    samus,
                                    platform) ?? throw new InvalidDataException(
                                        $"Selected climb platform (${platform.LandingX:X4}," +
                                        $"${platform.SurfaceY:X4}) lost its validated ascent lane.");
                            bool currentLaneIsClear = platform.IsDoorApproach ||
                                IsClimbAscentLaneClear(
                                    bus,
                                    runtime.LevelData!,
                                    runtime.Plms,
                                    samus,
                                    platform,
                                    samus.XPosition);
                            // A tiny supporting lip may end before the safe outside lane.
                            // Trace the proposed run-up with the real downward dispatcher.
                            // If cartridge collision cannot support that walk, launch toward
                            // the outside lane immediately and clear the overhang in the air.
                            // This derives the decision from room geometry instead of naming
                            // one particular shelf or installing a route-only position fix.
                            bool canWalkToApproach = HasClimbGroundSupportToApproach(
                                bus,
                                runtime.LevelData!,
                                runtime.Plms,
                                samus,
                                climbApproachX);
                            int selectedPlatformRise =
                                currentSupportSurfaceY - platform.SurfaceY;
                            int selectedPlatformWidth =
                                platform.ShadowRightX - platform.ShadowLeftX;
                            bool narrowTallParlorLedge =
                                returningUpParlorShaft && platform.IsSolidLedge &&
                                selectedPlatformRise > 3 * LevelBlockSizePixels &&
                                selectedPlatformWidth <= 3 * LevelBlockSizePixels;
                            bool narrowTallLedgeNeedsMomentum =
                                narrowTallParlorLedge &&
                                selectedPlatformWidth < 2 * LevelBlockSizePixels;
                            bool longHorizontalTransfer =
                                launchDistance > 4 * LevelBlockSizePixels &&
                                (selectedPlatformRise <= 2 * LevelBlockSizePixels ||
                                 selectedPlatformRise <= 4 * LevelBlockSizePixels &&
                                 // Shadow endpoints are inclusive pixel coordinates, so a
                                 // four-tile authored span measures 63 rather than 64 here.
                                 selectedPlatformWidth >= 4 * LevelBlockSizePixels - 1);
                            if (VerboseDiagnostics &&
                                returningUpParlorShaft &&
                                platform.SurfaceY <= 0x0160)
                            {
                                Console.WriteLine(
                                    $"    Parlor target geometry: distance={launchDistance}, " +
                                    $"rise={selectedPlatformRise}, width={selectedPlatformWidth}, " +
                                    $"approach=${climbApproachX:X4}, walk={canWalkToApproach}, " +
                                    $"lane={currentLaneIsClear}, long={longHorizontalTransfer}.");
                            }
                            climbPreserveTargetwardMomentum = longHorizontalTransfer;
                            // A previous full-height attempt at this exact ROM-selected
                            // destination may have contacted authored ceiling geometry. In
                            // that case retry with native variable-height release; unrelated
                            // platforms retain the full jump that already works for them.
                            climbUseLowArc = lowArcClimbTargets.Contains(
                                (platform.LandingX, platform.SurfaceY));
                            climbArcHitCeiling = false;
                            climbRequiresOutwardLaunch = narrowTallParlorLedge;
                            if (narrowTallParlorLedge && currentLaneIsClear)
                            {
                                // The current collision-proven lane is already wholly
                                // outside the narrow ledge. Walking to its nearest edge and
                                // only then reversing lets native running inertia carry the
                                // body underneath before outward motion begins. Launch from
                                // this existing clear lane instead: no coordinate is changed,
                                // and the next phase still waits until controller-driven X
                                // motion has genuinely reversed away from the shelf.
                                climbApproachX = samus.XPosition;
                                canWalkToApproach = true;
                            }
                            // If native floor probes cannot connect the current support to
                            // the clear outside lane, a directly-under or genuinely tall
                            // transfer must launch from the real support instead of walking
                            // into the authored gap. Shorter side approaches retain their
                            // already-proven targetward launch; redirecting those toward the
                            // approach lane can throw away the horizontal range they require.
                            climbLaunchTowardApproach = !canWalkToApproach &&
                                (directlyUnderTarget ||
                                 selectedPlatformRise > 3 * LevelBlockSizePixels);
                            climbNeedsMomentumRunup = false;
                            climbRunningTowardLaunch = false;
                            bool standingOnLevelSupport =
                                currentSupportSurfaceY % LevelBlockSizePixels == 0;
                            if (returningUpParlorShaft &&
                                (standingOnLevelSupport &&
                                 (!canWalkToApproach && !narrowTallParlorLedge ||
                                  narrowTallLedgeNeedsMomentum) ||
                                 longHorizontalTransfer))
                            {
                                // Two cartridge-derived cases need horizontal speed before
                                // takeoff: an approach lane that cannot be reached on foot,
                                // a one- or two-block ledge more than three blocks above a
                                // level source, and a destination over four blocks away.
                                // A long transfer may begin on a slope: the span helper below
                                // validates every prospective body centre with both native
                                // horizontal collision and the downward floor dispatcher, so
                                // requiring a numerically level starting surface would discard
                                // a perfectly genuine cartridge runway. A broad four-block
                                // destination can also accept a run-assisted transfer up to
                                // four blocks high. Narrow shelves retain the conservative
                                // two-block bound: testing the taller policy there proved that
                                // native momentum simply carries Samus past their safe span.
                                // The tall case consumes most of a standing arc merely gaining
                                // enough height; the long case needs base X speed before its
                                // descending edge reaches the landing plane. Certify the complete
                                // runway with repeated bank-$94 floor probes rather than assuming
                                // the visible tile art is safe to run across.
                                (int supportLeftX, int supportRightX) =
                                    FindClimbGroundSupportSpan(
                                        bus,
                                        runtime.LevelData!,
                                        runtime.Plms,
                                        samus);
                                if (supportRightX - supportLeftX >= LevelBlockSizePixels)
                                {
                                    bool targetIsRight =
                                        platform.LandingX > samus.XPosition;
                                    // Keep both endpoints eight pixels inside the certified
                                    // body-center span. The ordinary flat-shelf transfers use
                                    // this margin to absorb multi-pixel running frames without
                                    // walking off either support edge.
                                    int supportedRunwayWidth = supportRightX - supportLeftX;
                                    int runningEdgeInset =
                                        supportedRunwayWidth < 2 * LevelBlockSizePixels
                                            // A two-tile shelf has too little usable span
                                            // after the ordinary eight-pixel margins. Three
                                            // pixels still tolerates one native multi-pixel
                                            // running step while preserving most of its runway.
                                            ? 3
                                            : 8;
                                    climbRunupStartX = targetIsRight
                                        ? supportLeftX + runningEdgeInset
                                        : supportRightX - runningEdgeInset;
                                    climbRunupLaunchX = targetIsRight
                                        ? supportRightX - runningEdgeInset
                                        : supportLeftX + runningEdgeInset;
                                    climbNeedsMomentumRunup = true;
                                    climbLaunchTowardApproach = false;
                                    // A certified target-ward run supplies the horizontal
                                    // speed this transfer needs. Do not also perform the
                                    // zero-speed outward launch used when the source is an
                                    // irregular slope with no safe runway.
                                    climbRequiresOutwardLaunch = false;
                                }
                            }
                            climbRequiresGroundApproach = !climbLaunchTowardApproach &&
                                (directlyUnderTarget || !currentLaneIsClear ||
                                 (returningUpParlorShaft &&
                                  (canWalkToApproach || climbNeedsMomentumRunup)));
                            // Long transfers need immediate acceleration. On short transfers
                            // that same input reaches the platform's vertical side before
                            // Samus' feet clear its sloped surface, so defer steering until
                            // later in the cartridge-authored arc.
                            climbSteeringDelayFrames = Math.Clamp(
                                28 - launchDistance / 3,
                                0,
                                24);
                            if (directlyUnderTarget)
                                climbSteeringDelayFrames = 0;
                        }

                        if (roomName == "Parlor return" && climbTargetX is { } wallLandingX &&
                            climbUseLowArc &&
                            failedClimbTargetCounts.GetValueOrDefault(
                                (wallLandingX, climbTargetSurfaceY)) >= 2)
                        {
                            // A full-height arc that hit the ceiling and two subsequent
                            // variable-height attempts have all landed below this exact
                            // collision-selected shelf. Preserve that destination and switch
                            // only the audit's buttons to the game's native wall-jump handshake.
                            // Wall contact, animation rewind, launch velocity, and the eventual
                            // landing remain cartridge-owned behavior.
                            (int LeftCenterLimit, int RightCenterLimit)? shaftLimits =
                                TryFindContinuousShaftWallLimits(runtime.LevelData!, samus);
                            int wallJumpApexSampleY = Math.Max(
                                0,
                                climbTargetSurfaceY - 4 * LevelBlockSizePixels);
                            bool shaftWallsSpanTransfer =
                                TryFindContinuousShaftWallLimits(
                                    runtime.LevelData!,
                                    samus,
                                    wallJumpApexSampleY) is not null;
                            if (shaftLimits is null || !shaftWallsSpanTransfer)
                            {
                                // The top of Parlor opens into a staggered ledge field, not
                                // a pair of continuous shaft walls. A ceiling-constrained
                                // target there cannot use the native wall-jump fallback.
                                // Reject only this collision-observed edge and let the same
                                // cartridge platform search choose the next reachable floor.
                                rejectedClimbTargets.Add(
                                    (wallLandingX, climbTargetSurfaceY));
                                climbTargetX = null;
                                climbGroundedRunupFrames = 0;
                                input = 0;
                                goto ClimbInputBuilt;
                            }
                            (int leftWallLimit, int rightWallLimit) = shaftLimits.Value;
                            climbWallTargetIsRight = samus.XPosition >=
                                (leftWallLimit + rightWallLimit) / 2;
                            climbWallLandingX = wallLandingX;
                            climbWallLandingSurfaceY = climbTargetSurfaceY;
                            climbWallJumpHasLandingTarget = true;
                            climbUsingWallJumps = true;
                            climbTargetX = null;
                            climbGroundedRunupFrames = 0;
                        }

                    BuildClimbWallJumpInput:
                        if (climbTargetIsDoorApproach &&
                            IsClimbTopDoorCapClosed(runtime.LevelData!))
                        {
                            int currentSurfaceY =
                                samus.YPosition + samus.Kinematics.YRadius;
                            if (currentSurfaceY < 0x0080)
                            {
                                // The row-$7 post is only one block wide and cannot provide
                                // a centred firing stance. Walk off its inner edge onto the
                                // broad row-$8 ledge; gravity and floor collision own the drop.
                                input = (ushort)SnesButton.Right;
                            }
                            else if (samus.XPosition < 0x0174)
                            {
                                input = (ushort)SnesButton.Right;
                            }
                            else if (samus.XPosition > 0x018c)
                            {
                                input = (ushort)SnesButton.Left;
                            }
                            else
                            {
                                // Row $2 is a type-$C main block followed by type-$5
                                // extensions. Aim upward and send distinct beam edges until
                                // the translated shot-block owner removes that cartridge cap.
                                input = (ushort)SnesButton.Up;
                                if (frame % 20 < 4)
                                    input |= (ushort)SnesButton.X;
                            }
                            goto ClimbInputBuilt;
                        }

                        if (climbUsingWallJumps)
                        {
                            // Begin each first wall approach with a real running spin jump.
                            // The slope lip supporting this gap is only a few usable pixels
                            // wide: trying to manufacture a long run-up simply walks Samus
                            // off it. One direction-only frame establishes running, and the
                            // following fresh Jump edge selects the native spin pose.
                            SnesButton towardWall = climbWallTargetIsRight
                                ? SnesButton.Right
                                : SnesButton.Left;
                            SnesButton groundedLaunchDirection =
                                climbLaunchAwayFromAdjacentWall
                                    ? climbWallTargetIsRight
                                        ? SnesButton.Left
                                        : SnesButton.Right
                                    : towardWall;
                            input = (ushort)(groundedLaunchDirection | SnesButton.B);
                            climbGroundedRunupFrames++;
                            if (climbGroundedRunupFrames >= 2 && climbJumpReady)
                            {
                                // Hold for the full native arc unless wall proximity below
                                // asks for an early release. This gives the short lip enough
                                // flight time to reach the shaft wall, while still ensuring
                                // contact precedes a newly-pressed A edge.
                                input |= (ushort)SnesButton.A;
                                climbWallJumpButtonHoldFrames = 48;
                                climbJumpReady = false;
                            }
                            climbAlignedForJump = false;
                        }
                        else
                        {
                        int groundTargetX = climbNeedsMomentumRunup
                            ? climbRunningTowardLaunch
                                ? climbRunupLaunchX
                                : climbRunupStartX
                            : climbApproachX;
                        // A running frame advances several pixels and can skip a single
                        // exact coordinate. The run-up endpoints are already eight pixels
                        // inside a bank-$94-certified support span, so accepting a four-
                        // pixel window preserves that safety margin while allowing native
                        // inertia to cross the requested turnaround or takeoff coordinate.
                        int groundTargetTolerance = climbNeedsMomentumRunup ? 4 : 1;
                        bool reachedLaunchX = Math.Abs(
                            samus.XPosition - groundTargetX) <= groundTargetTolerance;
                        if (climbNeedsMomentumRunup && reachedLaunchX &&
                            !climbRunningTowardLaunch)
                        {
                            // The first leg only creates room to accelerate. Reverse on
                            // the next frame and consume the complete cartridge-supported
                            // span before taking the jump edge at its target-side boundary.
                            climbRunningTowardLaunch = true;
                            climbGroundedRunupFrames = 0;
                            groundTargetX = climbRunupLaunchX;
                            reachedLaunchX = Math.Abs(
                                samus.XPosition - groundTargetX) <= groundTargetTolerance;
                        }
                        int launchTargetX = climbLaunchTowardApproach
                            ? climbApproachX
                            : climbTargetX ?? 0x0178;
                        bool launchAwayFromParlorLedge =
                            climbWaitingForOutwardMotion ||
                            roomName == "Parlor return" &&
                            climbWaitForSurfaceClearance && reachedLaunchX &&
                            climbTargetX is not null &&
                            !climbNeedsMomentumRunup;
                        SnesButton launchDirection = launchAwayFromParlorLedge
                            // The certified approach is one body radius outside a solid
                            // vertical face. Spending the mandatory pre-jump running frame
                            // toward that face selects native ran-into-wall art, after which
                            // A can only produce a neutral jump. Face outward for that one
                            // grounded frame; releasing direction in the air clears its base
                            // speed, and the normal clearance gate crosses back afterward.
                            ? climbApproachX < climbTargetX
                                ? SnesButton.Left
                                : SnesButton.Right
                            : samus.XPosition < launchTargetX
                                ? SnesButton.Right
                                : SnesButton.Left;
                        if (climbRequiresOutwardLaunch && reachedLaunchX)
                            climbWaitingForOutwardMotion = true;
                        // `previousX` is deliberately synchronized to Samus after every
                        // runtime step for the stationary counters below. Consequently it
                        // cannot reveal the preceding frame's movement while this frame's
                        // controller word is being assembled. Use the displacement captured
                        // immediately after the last step so A is added only after the ROM
                        // has really accelerated Samus away from the adjoining wall.
                        bool movingOutward = climbWaitingForOutwardMotion &&
                            (launchDirection == SnesButton.Right
                                ? lastHorizontalDelta > 0
                                : lastHorizontalDelta < 0);
                        horizontalDirection = climbWaitingForOutwardMotion || reachedLaunchX
                            ? launchDirection
                            : samus.XPosition < groundTargetX
                                ? SnesButton.Right
                                : SnesButton.Left;

                        // Jump during `$A4/$A5/$A6/$A7` legitimately selects a landing-
                        // interrupt target. Let that animation settle before issuing the
                        // fresh directional edge that selects spin-jump art.
                        bool walkingToRunupStart = climbNeedsMomentumRunup &&
                            !climbRunningTowardLaunch;
                        input = waitingForLandingAnimation
                            ? (ushort)0
                            : walkingToRunupStart
                                // The setup leg only creates runway. Holding Dash here
                                // builds velocity away from the eventual jump and requires
                                // enough braking distance to overrun a narrow shelf. Walk
                                // to the inset turnaround, then enable Dash on the target-
                                // ward leg whose momentum the jump is meant to preserve.
                                ? (ushort)horizontalDirection
                                : (ushort)(horizontalDirection | SnesButton.B);
                        if (!waitingForLandingAnimation)
                            climbGroundedRunupFrames++;
                        climbAlignedForJump = !waitingForLandingAnimation &&
                            (!climbRequiresGroundApproach || reachedLaunchX ||
                             movingOutward) &&
                            (!climbNeedsMomentumRunup || climbRunningTowardLaunch) &&
                            (!climbWaitingForOutwardMotion || movingOutward) &&
                            // One direction-only frame establishes running state before
                            // the new Jump edge. Pressing both on the first supported frame
                            // selects normal-jump `$4B/$4D`; the authored Climb gaps rely on
                            // spin-jump `$19/$1A` and its twelve-pixel vertical radius.
                            climbGroundedRunupFrames >= 2;
                        climbJumpDirection = launchDirection;
                        }
                    }
                    else
                    {
                        climbAirborneFrames++;
                        if (climbUsingWallJumps)
                        {
                            SnesButton towardWall = climbWallTargetIsRight
                                ? SnesButton.Right
                                : SnesButton.Left;
                            SnesButton awayFromWall = climbWallTargetIsRight
                                ? SnesButton.Left
                                : SnesButton.Right;
                            // Ask the exact observational bank-$94 wall probe used by native
                            // spin movement. This adapts to Parlor's wider shaft and to
                            // asymmetric square slopes without embedding either room's wall
                            // coordinates. A result below eight pixels is precisely the
                            // cartridge wall-jump eligibility distance.
                            int wallProbeDisplacement = climbWallTargetIsRight
                                ? 8 << 16
                                : -(8 << 16);
                            BlockMoveResult wallProbe = SamusBlockCollision.ProbeWallHorizontal(
                                bus,
                                runtime.LevelData!,
                                samus.Kinematics,
                                wallProbeDisplacement,
                                runtime.Plms);
                            (int LeftCenterLimit, int RightCenterLimit)? liveShaftLimits =
                                TryFindContinuousShaftWallLimits(
                                    runtime.LevelData!,
                                    samus);
                            if (liveShaftLimits is null)
                            {
                                // Crossing above the last continuous wall column is a
                                // geometry transition, not a route failure. Retire wall-jump
                                // mode and steer the already-native airborne arc toward its
                                // saved landing. The next collision-resolved landing will
                                // either complete that edge or feed it back to the ordinary
                                // failure/rejection logic.
                                climbUsingWallJumps = false;
                                climbWaitingAboveWallLanding = false;
                                climbWallLandingEnabled = false;
                                climbHoldingTriggeredWallJump = false;
                                climbTargetX = climbWallJumpHasLandingTarget
                                    ? climbWallLandingX
                                    : null;
                                input = (ushort)SnesButton.B;
                                if (climbTargetX is { } resumedLandingX)
                                {
                                    input |= (ushort)(samus.XPosition < resumedLandingX
                                        ? SnesButton.Right
                                        : SnesButton.Left);
                                }
                                if (samus.Kinematics.YDirection == 1 &&
                                    climbWallJumpButtonHoldFrames > 0)
                                {
                                    input |= (ushort)SnesButton.A;
                                    climbWallJumpButtonHoldFrames--;
                                }
                                goto ClimbInputBuilt;
                            }
                            (int leftWallCenterLimit, int rightWallCenterLimit) =
                                liveShaftLimits.Value;
                            bool reachedIntendedWall = climbWallTargetIsRight
                                ? samus.XPosition >= rightWallCenterLimit - 7
                                : samus.XPosition <= leftWallCenterLimit + 7;
                            bool movedIntoSelectedWallLastFrame =
                                runtime.LastAerialSamusMovement?.Horizontal.Collided == true &&
                                (runtime.Controller1.Current & (ushort)towardWall) != 0;
                            bool nearWall =
                                reachedIntendedWall && wallProbe.Collided &&
                                    Math.Abs(wallProbe.AcceptedDisplacement >> 16) < 8 ||
                                movedIntoSelectedWallLastFrame;
                            bool contactedLastFrame = runtime.LastAerialSamusMovement is
                                { WallContact: true, WallJumpTriggered: false };

                            if (climbLaunchAwayFromAdjacentWall)
                            {
                                if (contactedLastFrame)
                                {
                                    // The outbound direction itself probes the wall behind
                                    // the newly-facing spin pose. If that native probe has
                                    // already rewound the animation, it is authoritative:
                                    // stop the launch-away setup and let the ordinary
                                    // release/new-A handshake below finish the wall jump.
                                    climbLaunchAwayFromAdjacentWall = false;
                                }
                                bool movedAwayFromWall = climbWallTargetIsRight
                                    ? samus.XPosition < rightWallCenterLimit
                                    : samus.XPosition > leftWallCenterLimit;
                                if (climbLaunchAwayFromAdjacentWall && !movedAwayFromWall)
                                {
                                    // Pose selection alone does not move Samus. Keep the
                                    // outbound direction until native acceleration changes
                                    // her world X by at least one pixel. Reverse immediately
                                    // after that real movement: waiting for her whole body to
                                    // leave the probe window makes the return happen after the
                                    // jump apex, too late for the native wall-contact handshake.
                                    input = (ushort)(awayFromWall | SnesButton.A | SnesButton.B);
                                    if (climbWallJumpButtonHoldFrames > 0)
                                        climbWallJumpButtonHoldFrames--;
                                    goto ClimbInputBuilt;
                                }
                                climbLaunchAwayFromAdjacentWall = false;
                            }

                            if (climbWaitingAboveWallLanding)
                            {
                                // Preserve the launch's outbound direction while Samus rises
                                // beside the selected shelf. Spin/wall-jump movement clears
                                // base X speed whenever forward is released, so a neutral
                                // wait strands her over the source wall before the landing
                                // phase can accelerate across the gap. The authored shelf's
                                // underside still owns early contact; this supplies only the
                                // same held direction a player uses after the wall-jump edge.
                                bool feetAboveLanding = samus.YPosition +
                                    samus.Kinematics.YRadius < climbWallLandingSurfaceY;
                                if (feetAboveLanding)
                                {
                                    climbWaitingAboveWallLanding = false;
                                    climbWallLandingEnabled = true;
                                }
                                else
                                {
                                    input = (ushort)(towardWall | SnesButton.B);
                                    if (climbWallJumpButtonHoldFrames > 0)
                                    {
                                        input |= (ushort)SnesButton.A;
                                        climbWallJumpButtonHoldFrames--;
                                    }
                                    goto ClimbInputBuilt;
                                }
                            }

                            if (climbWallLandingEnabled)
                            {
                                // The second wall launch is now above the otherwise
                                // unreachable shelf. Stop crossing the shaft and center
                                // over the floor resolved by the same bank-$94 probe used
                                // by ordinary targets, so downward collision owns landing.
                                input = (ushort)SnesButton.B;
                                if (samus.XPosition < climbWallLandingX - 1)
                                    input |= (ushort)SnesButton.Right;
                                else if (samus.XPosition > climbWallLandingX + 1)
                                    input |= (ushort)SnesButton.Left;
                                if (climbWallJumpButtonHoldFrames > 0)
                                {
                                    input |= (ushort)SnesButton.A;
                                    climbWallJumpButtonHoldFrames--;
                                }
                                goto ClimbInputBuilt;
                            }

                            // CheckBlockWallJump probes the wall behind the newly-facing
                            // spin pose. Thus Left tests the right wall and Right tests the
                            // left wall. First make one contact frame with A released; on
                            // the following frame add A to create the edge required by
                            // `$90:9E7F`. After a successful launch, keep A held for native
                            // variable-height physics while travelling to the opposite wall.
                            if (contactedLastFrame)
                            {
                                // Repeat the exact directional probe that made contact.
                                // The direction names the wall *behind* the facing pose
                                // (Right probes left, Left probes right); recomputing it
                                // from the intended travel wall can reverse the pose and
                                // discard ApplyWallContactAnimationRewind's eligible frame.
                                input = (ushort)(climbWallContactDirection | SnesButton.B);

                                // Early contact rewinds ordinary spin art to frame $0A;
                                // the wall-jump test does not accept Jump until frame $0B.
                                // Animation advances after movement, so the A edge which
                                // first made contact can be one frame too early even though
                                // the post-step debugger already displays $0B. If that edge
                                // is still held, spend one contact frame releasing it. The
                                // following iteration sees both an eligible animation frame
                                // and a genuinely released controller latch, then supplies
                                // the new A edge consumed by `$90:9E7F`.
                                ushort firstEligibleWallFrame =
                                    SamusState.IsScrewAttackPose(samus.Pose) ? (ushort)0x1b : (ushort)0x0b;
                                bool jumpWasHeldLastFrame =
                                    (runtime.Controller1.Current & (ushort)SnesButton.A) != 0;
                                if (samus.AnimationFrame >= firstEligibleWallFrame &&
                                    !jumpWasHeldLastFrame)
                                {
                                    input |= (ushort)SnesButton.A;
                                }
                            }
                            else if (climbWallJumpButtonHoldFrames > 0 &&
                                (climbHoldingTriggeredWallJump || !nearWall))
                            {
                                // A real `$90:9D35` contact above takes priority over this
                                // coarse approach estimate. In particular, Parlor's square
                                // slopes can be probe-eligible while their derived center
                                // limit still says Samus is one pixel outside the margin.
                                input = (ushort)(towardWall | SnesButton.A | SnesButton.B);
                                climbWallJumpButtonHoldFrames--;
                                if (climbWallJumpButtonHoldFrames == 0)
                                    climbHoldingTriggeredWallJump = false;
                            }
                            else
                            {
                                if (nearWall)
                                    climbWallJumpButtonHoldFrames = 0;
                                input = (ushort)((nearWall ? awayFromWall : towardWall) |
                                    SnesButton.B);
                            }
                            goto ClimbInputBuilt;
                        }
                        int targetX = climbTargetX ?? 0x0178;
                        // Ascent first stays outside the ledge's tile shadow. Once Samus'
                        // feet are above its probed surface, crossing over it is physically
                        // possible and should begin immediately; waiting for the apex left
                        // too little horizontal travel before the landing row.
                        bool feetAboveTargetSurface =
                            samus.YPosition + samus.Kinematics.YRadius < climbTargetSurfaceY;
                        int plannedSurfaceRise =
                            climbPlanningSupportSurfaceY - climbTargetSurfaceY;
                        if (roomName == "Parlor return" &&
                            climbWaitForSurfaceClearance && feetAboveTargetSurface &&
                            plannedSurfaceRise <= 2 * LevelBlockSizePixels &&
                            (!climbPreserveTargetwardMomentum || climbUseLowArc))
                        {
                            // Stop extending the jump at the first frame Samus' complete
                            // body clears the selected cartridge surface. Holding Jump for
                            // a fixed maximum arc is wrong for a short step: it can carry
                            // her into the side or underside of a different shelf several
                            // rows above the destination. A long transfer still preserves
                            // its targetward directional input and running inertia; Jump is
                            // solely the native variable-height control. For a long transfer,
                            // use the cutoff only after the collision dispatcher reported a
                            // ceiling hit on a prior attempt at this exact destination. That
                            // keeps Samus beneath the intervening shelf without imposing one
                            // room-coordinate rule on every shallow jump. Climb's ledges retain
                            // the proven full-height controller policy.
                            climbJumpHoldFrames = 0;
                        }
                        int steeringTargetX = climbWaitForSurfaceClearance &&
                            samus.Kinematics.YDirection == 1 &&
                            !feetAboveTargetSurface &&
                            !climbPreserveTargetwardMomentum
                            ? climbApproachX
                            : targetX;
                        bool horizontallyAligned = Math.Abs(
                            steeringTargetX - samus.XPosition) <= 1;
                        if (!horizontallyAligned)
                        {
                            horizontalDirection = samus.XPosition < steeringTargetX
                                ? SnesButton.Right
                                : SnesButton.Left;
                        }

                        // A one-frame directional launch selects spin art. Releasing the
                        // direction then clears base X speed through `$90:9078`; reapply it
                        // only after the distance-derived delay so a nearby ledge is reached
                        // around the apex rather than struck from underneath.
                        input = (ushort)SnesButton.B;
                        if (((roomName == "Parlor return" && feetAboveTargetSurface) ||
                             climbAirborneFrames > climbSteeringDelayFrames) &&
                            !horizontallyAligned)
                        {
                            input |= (ushort)horizontalDirection;
                        }
                    }

                }
                else
                {
                    if (roomName == "Parlor return" && parlorUpperShaftCleared)
                    {
                        // Flyway is not the far-right door in Parlor's top corridor. Its
                        // door is two screens lower, at the foot of the narrow winding
                        // passage beginning near column $34. Walk to that opening without
                        // Run (a running Samus can clear the two-tile gap), then steer each
                        // fall using the widest cartridge-air span immediately below her.
                        // Once the feet reach the destination door band, ordinary Right
                        // input and the shared Fire pulse open/publish the authored door.
                        int supportSurfaceY =
                            samus.YPosition + samus.Kinematics.YRadius;
                        int horizontalTargetX;
                        int selectedWaypointRow = -1;
                        int selectedRouteDistance = -1;
                        string selectedRoutePreview = "-";
                        if (!parlorDescentLipCleared)
                        {
                            horizontalTargetX = 0x0340;
                        }
                        else if (supportSurfaceY < 0x0240)
                        {
                            // Do not let the sampled row travel through the collision map
                            // during one airborne arc. Adjacent rows can legitimately expose
                            // opposite sides of this winding passage; recomputing after the
                            // apex (or an underside collision) reversed Samus before she could
                            // land on the platform selected from her last support. The lane is
                            // therefore chosen while grounded and held until the next landing.
                            horizontalTargetX = samus.Kinematics.YDirection != 0 &&
                                parlorLaneTargetX >= 0
                                    ? parlorLaneTargetX
                                    : FindParlorDescentLaneCenter(
                                        runtime.LevelData!,
                                        samus,
                                        out selectedWaypointRow,
                                        out selectedRouteDistance,
                                        out selectedRoutePreview);
                        }
                        else
                        {
                            // The destination's left-facing blue cap begins at block X $3E.
                            // Walking Samus all the way to her collision stop at $03DB puts
                            // the arm-cannon muzzle beyond that cap: the beam's first leading-
                            // edge sample then sees the type-$9 door block at X $3F and
                            // explodes without ever invoking the cap's type-$C/BTS-$40 shot
                            // reaction. Stop two blocks back while the authored cap remains
                            // shootable. Once its ordinary PLM list converts the origin away
                            // from type $C, resume walking into the now-exposed door trigger.
                            // This is controller policy only; collision, projectile placement,
                            // the blue-door PLM, and the door transition all remain runtime-
                            // owned and are exercised exactly as they are during live play.
                            RoomCollisionBlock flywayCap =
                                runtime.LevelData!.GetCollisionBlock(0x3e, 0x26);
                            horizontalTargetX = flywayCap is
                                { CollisionType: 12, Behavior: 0x40 }
                                    ? 0x03c0
                                    : 0x0400;
                        }

                        if (horizontalTargetX != parlorLaneTargetX)
                        {
                            // Emit only state changes: this ties a steering reversal to the
                            // exact collision row selected by the destination-connectivity
                            // search without flooding a six-thousand-frame private-ROM run.
                            int sampledLaneRow = Math.Clamp(
                                (samus.YPosition + samus.Kinematics.YRadius + 8) >> 4,
                                0,
                                runtime.LevelData!.HeightInBlocks - 1);
                            if (VerboseDiagnostics)
                            {
                                Console.WriteLine(
                                    $"  Parlor descent target ${horizontalTargetX:X4} at frame {frame}; " +
                                    $"Samus=(${samus.XPosition:X4},${samus.YPosition:X4}), " +
                                    $"row=${sampledLaneRow:X2}, waypoint=" +
                                    (selectedWaypointRow >= 0
                                        ? $"({horizontalTargetX >> 4:X2},{selectedWaypointRow:X2})/d{selectedRouteDistance}/" +
                                          selectedRoutePreview
                                        : "-") +
                                    $", lip={parlorDescentLipCleared}.");
                            }
                            parlorLaneTargetX = horizontalTargetX;
                            parlorBestLaneDistance = int.MaxValue;
                            parlorLaneStallFrames = 0;
                        }
                        int laneDistance = Math.Abs(
                            horizontalTargetX - samus.XPosition);
                        if (laneDistance < parlorBestLaneDistance)
                        {
                            parlorBestLaneDistance = laneDistance;
                            parlorLaneStallFrames = 0;
                        }
                        else
                        {
                            parlorLaneStallFrames++;
                        }
                        if (samus.YPosition > parlorDeepestY)
                        {
                            parlorDeepestY = samus.YPosition;
                            parlorDescentStallFrames = 0;
                        }
                        else
                        {
                            parlorDescentStallFrames++;
                        }

                        horizontalDirection = samus.XPosition < horizontalTargetX - 2
                            ? SnesButton.Right
                            : samus.XPosition > horizontalTargetX + 2
                                ? SnesButton.Left
                                : 0;

                        SamusMovementType parlorMovement = samus.ReadMovementKind(bus);
                        bool parlorIsMorphed = parlorMovement is
                            SamusMovementType.MorphBallGround or
                            SamusMovementType.MorphBallFalling;
                        if (!parlorMorphTunnelActive &&
                            !parlorIsMorphed &&
                            parlorMovement != SamusMovementType.PostureTransition &&
                            horizontalDirection != 0 &&
                            HorizontalPassageRequiresMorphBall(
                                runtime.LevelData!,
                                samus,
                                horizontalDirection))
                        {
                            // The destination-connected path can contain a one-block-high
                            // horizontal run. Detect that from the collision map rather than
                            // naming Parlor coordinates: compare the rows occupied by the
                            // current humanoid body with the radius-seven ball at the same
                            // feet position. Activation is meaningful only for a stable humanoid:
                            // when Samus is already a ball, the lower half of a vertical door
                            // cap has exactly this silhouette and previously started a bogus
                            // second tunnel whose unreachable target sat beyond the cap. The
                            // intermediate `$3D/$3E` unmorph bodies must also finish before
                            // geometry may request another Down edge; their radius sixteen can
                            // otherwise misclassify that same cap while Up is legitimately
                            // expanding Samus at the tunnel exit. Existing ball motion can
                            // simply continue, and the expansion collision pass below decides
                            // when there is room to unmorph.
                            // The audit supplies only normal Down/Up inputs; the ROM pose
                            // table, equipment bit, animation, radius change, and collision
                            // code remain authoritative.
                            parlorMorphTunnelActive = true;
                            parlorMorphTunnelEnteredBall = false;
                            parlorMorphTunnelInputFrames = 0;
                            parlorMorphTunnelExitX = horizontalTargetX;
                        }

                        if (parlorMorphTunnelActive)
                        {
                            parlorMorphTunnelEnteredBall |= parlorIsMorphed;
                            if (!parlorIsMorphed)
                            {
                                // Down is edge-sensitive across stand -> crouch -> morph.
                                // Begin with a deterministic release window so arriving at
                                // the obstruction while another direction is held cannot
                                // turn the intended morph into an aimed-running transition.
                                int morphInputPhase = parlorMorphTunnelInputFrames % 60;
                                horizontalDirection = morphInputPhase < 20
                                    ? 0
                                    : SnesButton.Down;
                                parlorMorphTunnelInputFrames++;
                                jumpHoldFrames = 0;
                            }
                            else if (Math.Abs(samus.XPosition - parlorMorphTunnelExitX) <= 4)
                            {
                                // Crossing the first path segment proves that the ball has
                                // traversed the low channel. It does *not* prove there is
                                // humanoid headroom at that edge: Parlor immediately drops
                                // into the next shaft. Retain the legitimate ball pose and
                                // hand steering back to the destination-distance field.
                                horizontalDirection = 0;
                                parlorMorphTunnelActive = false;
                                parlorMorphTunnelEnteredBall = false;
                                parlorMorphTunnelExitX = -1;
                                if (!parlorDescentLipCleared)
                                {
                                    parlorDescentLipCleared = true;
                                    parlorDeepestY = samus.YPosition;
                                    parlorDescentStallFrames = 0;
                                }
                            }
                            else
                            {
                                horizontalDirection = samus.XPosition < parlorMorphTunnelExitX
                                    ? SnesButton.Right
                                    : SnesButton.Left;
                            }

                            if (parlorMorphTunnelEnteredBall &&
                                !parlorIsMorphed &&
                                parlorMovement != SamusMovementType.PostureTransition &&
                                parlorMorphTunnelInputFrames > 12)
                            {
                                // A completed Up transition leaves both ball and posture
                                // movement types. Resume destination-derived steering; a
                                // later low run will independently retrigger this detector.
                                parlorMorphTunnelActive = false;
                                parlorMorphTunnelEnteredBall = false;
                                parlorMorphTunnelExitX = -1;
                            }
                        }

                        if (!parlorMorphTunnelActive &&
                            supportSurfaceY >= 0x0240 &&
                            (parlorIsMorphed ||
                             parlorMovement == SamusMovementType.PostureTransition))
                        {
                            // Flyway's cap needs the arm cannon. Only request Up once the
                            // destination door band has full-height room; the translated
                            // expansion probe still has final authority over the unmorph.
                            horizontalDirection = SnesButton.Up;
                        }
                        if (parlorDescentLipCleared &&
                            !parlorMorphTunnelActive &&
                            samus.Kinematics.YDirection == 0 &&
                            horizontalDirection != 0 &&
                            (parlorLaneStallFrames >= 30 ||
                             parlorDescentStallFrames >= 45) &&
                            jumpHoldFrames == 0)
                        {
                            // A square-slope staircase can alternate one-pixel movement and
                            // collision forever, which never satisfies the room-wide stationary
                            // heuristic. Request the same ordinary jump only after thirty frames
                            // without reducing distance to the destination-connected lane.
                            jumpHoldFrames = 24;
                            parlorBestLaneDistance = laneDistance;
                            parlorLaneStallFrames = 0;
                            parlorDescentStallFrames = 0;
                        }
                    }
                    input = (ushort)horizontalDirection;
                }
                if (roomName == "Flyway" && samus.XPosition >= 0x02b0)
                {
                    // The red cap occupies rows six through nine while the cartridge's
                    // small step at columns `$2C-$2D` blocks a floor-height or diagonal-up
                    // shot. Jump onto/over that step with ordinary input, then fire only
                    // while the arm cannon is horizontally level with the cap. The finite
                    // first Missile pack is therefore spent on the door rather than terrain.
                    input = (ushort)SnesButton.Right;
                    if (samus.Kinematics.YDirection == 0 && jumpHoldFrames == 0)
                        jumpHoldFrames = 24;
                }
                if (roomName == "Flyway" && samus.XPosition >= 0x02b0 &&
                    samus.SelectedHudItem != 1 && frame % 2 == 0)
                {
                    // Flyway's `$44` cap is the first red door on the controller route.
                    // Select is deliberately supplied as alternating pressed/released
                    // samples: bank `$90:C4E7` consumes a new edge, skips unavailable HUD
                    // entries, and chooses the Missile collected in Blue Brinstar. Nothing
                    // here edits the item index or the door; the runtime's translated HUD
                    // selector and the resident PLM must perform both state changes.
                    input |= (ushort)SnesButton.Select;
                }
                // Fire cancels spin into normal-jump gun art, which cannot execute the
                // block wall-jump check. Suppress opportunistic shots during a planned
                // vertical transfer; the explicit top-cap phase fires only while grounded.
                int routeFireCadence = roomName == "Bomb Torizo return" ? 8 : 24;
                if (frame % routeFireCadence == 0 && !returningUpVerticalRoute &&
                    !parlorMorphTunnelActive &&
                    roomName != "Flyway")
                    input |= (ushort)SnesButton.X;
                if (roomName == "Bomb Torizo return")
                {
                    // The arena's west cap occupies rows $06-$09 while its floor is row
                    // $0D. A horizontal floor shot correctly explodes against the masonry
                    // below the door. Hold the configured diagonal-up shoulder so ordinary
                    // jump/fall poses send fresh beam edges through the flashing cap; the
                    // type-$C/BTS-$44 collision and grey-door PLM remain sole gate owners.
                    input |= (ushort)SnesButton.R;
                }
                if (roomName == "Flyway" && samus.XPosition < 0x02b0)
                {
                    // The corridor population is live on the return trip and approaches
                    // from above. Use the configured diagonal-up shoulder with ordinary
                    // power-beam edges so the audit does not deliberately absorb those
                    // contacts before selecting Missiles at the red cap. The X pulses stop
                    // before the door phase, preserving all five rounds.
                    input |= (ushort)SnesButton.R;
                    if (frame % 8 == 0)
                        input |= (ushort)SnesButton.X;
                }
                if (roomName == "Flyway" && samus.SelectedHudItem == 1 &&
                    samus.XPosition >= 0x02d0 && samus.YPosition is >= 0x0068 and <= 0x0098 &&
                    frame % 2 == 0)
                {
                    // Alternating input supplies genuine Shoot edges. Missile cooldown still
                    // limits allocation to the cartridge cadence, and the altitude band is
                    // derived from the live four-block door cap rather than forcing a hit.
                    input |= (ushort)SnesButton.X;
                }
                if (returningUpVerticalRoute && samus.Kinematics.YDirection == 0 &&
                    climbAlignedForJump && climbJumpReady)
                {
                    // A room-global modulo pulse made later jumps arbitrarily short: a
                    // landing near the end of the pulse could receive only a handful of
                    // held frames, invoking the game's variable-height cutoff. Start one
                    // complete button hold from the actual supported launch instead.
                    climbJumpHoldFrames = 48;
                    climbJumpReady = false;
                    climbStartingJump = true;
                }
                if (returningUpVerticalRoute && climbJumpHoldFrames > 0)
                {
                    // The directional chord is needed only on the new Jump edge to select
                    // spin-jump art and seed its horizontal motion. On later frames the
                    // target-X steering above must be free to reverse direction around a
                    // ledge; holding the launch direction here would press Left+Right at
                    // once and let the ROM priority table defeat that correction.
                    input |= (ushort)SnesButton.A;
                    if (climbStartingJump)
                        input |= (ushort)climbJumpDirection;
                    climbJumpHoldFrames--;
                }
            ClimbInputBuilt:
                if (roomName != "Flyway" &&
                    (roomName != "Parlor" || samus.YPosition < 0x0100) &&
                    !returningUpVerticalRoute &&
                    !parlorMorphTunnelActive &&
                    stationaryFrames == 30 && jumpHoldFrames == 0)
                {
                    jumpHoldFrames = 24;
                }
                if (jumpHoldFrames > 0)
                {
                    input |= (ushort)SnesButton.A;
                    jumpHoldFrames--;
                }
            }

            if (roomName == "Climb" && returningWithMorphBallAtEntry &&
                samus.Kinematics.YDirection == 0)
            {
                ushort? combatInput = BuildNearbyRouteEnemyCombatInput(
                    bus,
                    runtime,
                    samus,
                    frame,
                    maximumTargetHealth: 20,
                    out bool nearbyInteractiveThreat);
                if (combatInput is { } pirateInput)
                {
                    // Combat takes ownership before a newly planned jump leaves the shelf.
                    // Re-arm the controller planner because its provisional A hold was not
                    // delivered to the game; when no interactive target remains, the next
                    // supported frame must be able to create a genuine new Jump edge.
                    climbJumpHoldFrames = 0;
                    climbJumpReady = true;
                    climbStartingJump = false;
                    climbThreatWaitFrames = 0;
                    input = pirateInput;
                }
                else if (nearbyInteractiveThreat && climbThreatWaitFrames < 420)
                {
                    // A wall Pirate can be active and close while temporarily sitting
                    // between the three Power-Beam axes. Do not launch directly through it:
                    // wait up to seven seconds for its cartridge AI to enter a legal lane.
                    // The bound prevents a stationary actor behind terrain from owning the
                    // controller forever; expiry restores the unmodified platform route.
                    climbJumpHoldFrames = 0;
                    climbJumpReady = true;
                    climbStartingJump = false;
                    climbThreatWaitFrames++;
                    input = 0;
                }
                else if (!nearbyInteractiveThreat)
                {
                    climbThreatWaitFrames = 0;
                }
                else if (frame % 12 == 0)
                {
                    // Keep a sparse forward beam for enemies just outside the local combat
                    // window. Never add Fire to an airborne arc: the cartridge correctly
                    // cancels spin into gun art, changing collision radii and invalidating
                    // the platform transfer that the controller planner is performing.
                    input |= (ushort)SnesButton.X;
                }
            }

            // The reported outbound Climb failure happened while falling down the right
            // half of the shaft and repeatedly firing downward. Exercise that exact input
            // family during the existing one-room descent: Down selects the cartridge's
            // compact `$2D/$2E` falling body and alternating X samples create real shot
            // edges. This deliberately does not move Samus or choose a platform in host
            // code; the ordinary pose-change resolver, projectile producer, gravity, and
            // bottom-boundary block scan remain the only owners of the result.
            if (roomName == "Climb" && !returningWithMorphBallAtEntry &&
                samus.Kinematics.YDirection == 2 && samus.XPosition >= 0x0180)
            {
                outboundClimbDownAimFrames++;
                input |= (ushort)SnesButton.Down;
                if (frame % 6 == 0)
                {
                    outboundClimbDownShotPulses++;
                    lastOutboundClimbDownShotFrame = frame;
                    input |= (ushort)SnesButton.X;
                }
            }
            byte poseBeforeStep = samus.Pose;
            ushort yBeforeStep = samus.YPosition;
            ushort yDirectionBeforeStep = samus.Kinematics.YDirection;
            int topBeforeStep = samus.YPosition - samus.Kinematics.YRadius;
            int bottomBeforeStep = samus.YPosition + samus.Kinematics.YRadius - 1;
            try
            {
                host.StepFrame(input);
            }
            catch (Exception exception)
            {
                // A translated-dispatch failure is itself route evidence. Emit the bounded
                // controller history before rethrowing so private-ROM audits preserve the
                // lead-up instead of reporting only the final pose pair. This does not
                // recover or modify runtime state; the original exception remains fatal.
                Console.WriteLine(
                    $"  {roomName} failed while stepping frame {frame + 1} with " +
                    $"input ${input:X4}: {exception.Message}");
                foreach (string sample in routeTrace.TakeLast(240))
                    Console.WriteLine($"    {sample}");
                throw;
            }
            frame++;

            if (roomName == "Pre-Missiles room" && yDirectionBeforeStep == 1)
            {
                int topAfterStep = samus.YPosition - samus.Kinematics.YRadius;
                if (topAfterStep < topBeforeStep)
                {
                    int leftBlock = Math.Max(0,
                        (samus.XPosition - samus.Kinematics.XRadius) >> 4);
                    int rightBlock = Math.Min(runtime.LevelData!.WidthInBlocks - 1,
                        (samus.XPosition + samus.Kinematics.XRadius - 1) >> 4);
                    int firstCrossedRow = Math.Max(0, topAfterStep >> 4);
                    int lastCrossedRow = Math.Min(runtime.LevelData.HeightInBlocks - 1,
                        (topBeforeStep - 1) >> 4);
                    for (int blockY = firstCrossedRow; blockY <= lastCrossedRow; blockY++)
                    {
                        int surfaceBottom = blockY * 16 + 15;
                        if (surfaceBottom < topAfterStep || surfaceBottom >= topBeforeStep)
                            continue;
                        for (int blockX = leftBlock; blockX <= rightBlock; blockX++)
                        {
                            RoomCollisionBlock block = runtime.LevelData.GetCollisionBlock(blockX, blockY);
                            if (block.CollisionType is 8 or 12 or 14)
                            {
                                throw new InvalidDataException(
                                    $"Pre-Missiles frame {frame} crossed solid ceiling block " +
                                    $"({blockX:X2},{blockY:X2}) while rising: top " +
                                    $"{topBeforeStep:X4}->{topAfterStep:X4}, " +
                                    $"Samus=(${samus.XPosition:X4},${samus.YPosition:X4}), " +
                                    $"pose ${poseBeforeStep:X2}->${samus.Pose:X2}, input=${input:X4}.");
                            }
                        }
                    }
                }
            }

            if (roomName == "Climb" && !returningWithMorphBallAtEntry &&
                yDirectionBeforeStep == 2)
            {
                int bottomAfterStep = samus.YPosition + samus.Kinematics.YRadius - 1;
                if (bottomAfterStep > bottomBeforeStep)
                {
                    int leftBlock = Math.Max(0,
                        (samus.XPosition - samus.Kinematics.XRadius) >> 4);
                    int rightBlock = Math.Min(runtime.LevelData!.WidthInBlocks - 1,
                        (samus.XPosition + samus.Kinematics.XRadius - 1) >> 4);
                    int firstCrossedRow = Math.Max(0, (bottomBeforeStep + 1) >> 4);
                    int lastCrossedRow = Math.Min(runtime.LevelData.HeightInBlocks - 1,
                        bottomAfterStep >> 4);
                    for (int blockY = firstCrossedRow; blockY <= lastCrossedRow; blockY++)
                    {
                        int surfaceY = blockY * 16;
                        if (surfaceY <= bottomBeforeStep || surfaceY > bottomAfterStep)
                            continue;
                        for (int blockX = leftBlock; blockX <= rightBlock; blockX++)
                        {
                            RoomCollisionBlock block = runtime.LevelData.GetCollisionBlock(blockX, blockY);
                            if (block.CollisionType is 8 or 12 or 14)
                            {
                                throw new InvalidDataException(
                                    $"Climb frame {frame} crossed solid platform block " +
                                    $"({blockX:X2},{blockY:X2}) while falling/down-firing: " +
                                    $"bottom {bottomBeforeStep:X4}->{bottomAfterStep:X4}, " +
                                    $"Samus=(${samus.XPosition:X4},${samus.YPosition:X4}), " +
                                    $"pose ${poseBeforeStep:X2}->${samus.Pose:X2}, input=${input:X4}.");
                            }
                        }
                    }
                }
            }

            if (roomName == "Pre-Missiles room")
            {
                CartridgeRoomHeader activeRoom = runtime.ActiveRoom ??
                    throw new InvalidOperationException("Camera regression audit lost its active room.");
                int roomHeightPixels = activeRoom.HeightInScreens * 256;
                if (samus.YPosition >= roomHeightPixels)
                {
                    foreach (string sample in routeTrace.TakeLast(80))
                        Console.WriteLine($"    {sample}");
                    throw new InvalidDataException(
                        $"{roomName} frame {frame} wrapped Samus Y to ${samus.YPosition:X4} " +
                        $"outside the {roomHeightPixels:X4}-pixel room after input ${input:X4}; " +
                        $"pose=${samus.Pose:X2}, priorY=${yBeforeStep:X4}.");
                }
                if (!runtime.HasPendingDoorTransition && runtime.Camera is { } activeCamera)
                {
                    int maximumCameraY = Math.Max(0, roomHeightPixels - 224);
                    if (activeCamera.YPosition > maximumCameraY)
                    {
                        throw new InvalidDataException(
                            $"{roomName} frame {frame} moved camera Y to " +
                            $"${activeCamera.YPosition:X4}, beyond ${maximumCameraY:X4}; " +
                            $"Samus=(${samus.XPosition:X4},${samus.YPosition:X4}), input=${input:X4}.");
                    }
                    int samusScreenY = samus.YPosition - activeCamera.YPosition;
                    if (samusScreenY is < -32 or > 256)
                    {
                        foreach (string sample in routeTrace.TakeLast(80))
                            Console.WriteLine($"    {sample}");
                        throw new InvalidDataException(
                            $"{roomName} frame {frame} desynchronized Samus and camera by " +
                            $"{samusScreenY} pixels; camera=${activeCamera.YPosition:X4}, " +
                            $"ideal=${activeCamera.IdealYPosition:X4}, speed=${activeCamera.CameraYSpeed:X4}, " +
                            $"Samus=(${samus.XPosition:X4},${samus.YPosition:X4}), priorY=${yBeforeStep:X4}, " +
                            $"direction=${samus.Kinematics.YDirection:X4}, " +
                            $"scrolls={string.Join(',', activeCamera.Scrolls.Storage.ToArray().Take(activeCamera.Scrolls.LogicalCellCount).Select(value => value.ToString("X2")))}, " +
                            $"input=${input:X4}.");
                    }
                }
            }

            if (roomName == "Climb" && !returningWithMorphBallAtEntry &&
                runtime.LastAerialSamusMovement is { Landed: true } &&
                frame - lastOutboundClimbDownShotFrame <= 30)
            {
                outboundClimbPostShotLandings++;
            }

            if (!returningWithMorphBallAtEntry &&
                !verifiedOutboundElevatorArtwork &&
                roomName == "Elevator to Blue Brinstar" &&
                frame >= 20)
            {
                AssertElevatorPlatformSurvivesGameplayCompositor(runtime);
                verifiedOutboundElevatorArtwork = true;
            }

            CollectiblePlmSnapshot? visibleMorphBall = crossingMorphBallRoom
                ? runtime.Plms.Collectibles.FirstOrDefault(item =>
                    item.Kind == InWorldCollectibleKind.MorphBall &&
                    item.Phase == CollectiblePhase.Visible)
                : null;
            RoomLevelData? morphRoomLevel = runtime.LevelData;
            bool morphBallIsInsideViewport = visibleMorphBall is { } morphBall &&
                morphRoomLevel is not null &&
                runtime.Camera is { } morphCamera &&
                morphBall.BlockIndex % morphRoomLevel.WidthInBlocks * LevelBlockSizePixels >=
                    morphCamera.XPosition + 32 &&
                morphBall.BlockIndex % morphRoomLevel.WidthInBlocks * LevelBlockSizePixels <
                    morphCamera.XPosition + 224 &&
                morphBall.BlockIndex / morphRoomLevel.WidthInBlocks * LevelBlockSizePixels >=
                    morphCamera.YPosition + 32 &&
                morphBall.BlockIndex / morphRoomLevel.WidthInBlocks * LevelBlockSizePixels <
                    morphCamera.YPosition + 224;
            if (!verifiedMorphBallBeforePickup && morphBallIsInsideViewport)
            {
                // Verify the first comfortably visible frame after the cartridge PLM has
                // drawn Morph Ball. Room load runs setup only; the following PLM_Handler
                // mutates the level word, expands the newly installed dynamic definitions,
                // and publishes those four character names into BG1's circular tilemap.
                // Reading the actual VRAM destination catches stale streamer definitions:
                // inventory acquisition alone would pass even while the pickup is invisible.
                RoomCollisionBlock visibleBlock = morphRoomLevel!.GetCollisionBlockByIndex(
                    visibleMorphBall!.Value.BlockIndex);
                int visibleBlockDefinition = visibleBlock.LevelWord & 0x03ff;
                ReadOnlySpan<byte> definitionBytes = morphRoomLevel.BlockDefinitions.Span;
                var definitionWords = new ushort[4];
                for (int child = 0; child < definitionWords.Length; child++)
                {
                    int byteIndex = (visibleBlockDefinition * 4 + child) * 2;
                    definitionWords[child] = unchecked((ushort)(
                        definitionBytes[byteIndex] |
                        (definitionBytes[byteIndex + 1] << 8)));
                }
                int blockX = visibleMorphBall.Value.BlockIndex % morphRoomLevel.WidthInBlocks;
                int blockY = visibleMorphBall.Value.BlockIndex / morphRoomLevel.WidthInBlocks;
                int ringX = blockX & 0x1f;
                ushort tilemapDestination = unchecked((ushort)(
                    (ringX < 0x10 ? 0x5000 : 0x53e0) +
                    (blockY & 0x0f) * 0x40 + ringX * 2));
                if ((runtime.BackgroundScroll.Bg1XOffset & 0x0100) != 0)
                {
                    tilemapDestination = ringX < 0x10
                        ? unchecked((ushort)(tilemapDestination + 0x0400))
                        : unchecked((ushort)(tilemapDestination - 0x0400));
                }
                ushort[] displayedWords =
                {
                    runtime.Vram.ReadWord(tilemapDestination),
                    runtime.Vram.ReadWord(tilemapDestination + 1),
                    runtime.Vram.ReadWord(tilemapDestination + 0x20),
                    runtime.Vram.ReadWord(tilemapDestination + 0x21),
                };
                if (!displayedWords.SequenceEqual(definitionWords))
                {
                    throw new InvalidDataException(
                        "Morph Ball became visible in level data without its dynamic " +
                        $"definition reaching BG1: expected [{string.Join(',', definitionWords.Select(word => word.ToString("X4")))}], " +
                        $"found [{string.Join(',', displayedWords.Select(word => word.ToString("X4")))}] " +
                        $"at VRAM ${tilemapDestination:X4}.");
                }
                verifiedMorphBallBeforePickup = true;
            }

            if (roomName == "Climb" && returningWithMorphBallAtEntry &&
                (input & (ushort)SnesButton.X) != 0 && climbCombatTrace.Count < 16)
            {
                // Record only genuine controller Fire edges from the first combat window.
                // This is read-only route telemetry: it lets a failed private-ROM audit
                // distinguish a bad firing lane from projectile allocation or enemy-hitbox
                // faults without changing cooldowns, projectile positions, or enemy state.
                RoomEnemySlot? nearestEnemy = runtime.Enemies.Slots
                    .Where(slot => slot.EnemyDefinitionPointer is not (0 or 0xdaff))
                    .OrderBy(slot =>
                        Math.Abs(unchecked((short)(slot.XPosition - samus.XPosition))) +
                        Math.Abs(unchecked((short)(slot.YPosition - samus.YPosition))))
                    .FirstOrDefault();
                climbCombatTrace.Add(
                    $"f{frame}/i${input:X4}/Samus=(${samus.XPosition:X4},${samus.YPosition:X4})/" +
                    $"p${samus.Pose:X2}/target=" +
                    (nearestEnemy is null
                        ? "-"
                        : $"${nearestEnemy.EnemyDefinitionPointer:X4}@" +
                          $"(${nearestEnemy.XPosition:X4},${nearestEnemy.YPosition:X4})/" +
                          $"hp{nearestEnemy.Health}") +
                    $"/shots=[{string.Join(',', runtime.Projectiles.Slots
                        .Where(slot => slot.InstructionPointer != 0)
                        .Select(slot =>
                            $"d{slot.Direction}@(${slot.XPosition:X4},${slot.YPosition:X4})/" +
                            $"t${slot.Type:X4}"))}]");
            }

            if (samus.Health < healthBeforeFrame && damageTrace.Count < 32)
            {
                // Keep route damage attributable to live cartridge actors. The controller
                // audit must never refill energy or suppress contact, so a failed survival
                // run needs a small, durable record of where ordinary collision consumed
                // it. Nearest-enemy data is diagnostic only; enemy AI and the shared Samus
                // damage routine remain the sole writers of health and knockback state.
                RoomEnemySlot? nearestEnemy = runtime.Enemies.Slots
                    .Where(slot => slot.EnemyDefinitionPointer is not (0 or 0xdaff))
                    .OrderBy(slot =>
                        Math.Abs(unchecked((short)(slot.XPosition - samus.XPosition))) +
                        Math.Abs(unchecked((short)(slot.YPosition - samus.YPosition))))
                    .FirstOrDefault();
                damageTrace.Add(
                    $"f{frame}:{healthBeforeFrame}->{samus.Health}/" +
                    $"Samus=(${samus.XPosition:X4},${samus.YPosition:X4})/p${samus.Pose:X2}/" +
                    $"enemy={(nearestEnemy is null ? "-" : $"${nearestEnemy.EnemyDefinitionPointer:X4}@(${nearestEnemy.XPosition:X4},${nearestEnemy.YPosition:X4})/hp{nearestEnemy.Health}")}");
            }
            healthBeforeFrame = samus.Health;

            if (roomName == "Parlor return" && climbTargetX is not null &&
                runtime.LastAerialSamusMovement is { HitCeiling: true })
            {
                // Contact alone is not failure: several valid full-height transfers brush
                // an authored ceiling and still land correctly. Defer the policy change
                // until the collision system later reports a landing below this target.
                climbArcHitCeiling = true;
            }

            if (roomName == "Parlor return" && samus.XPosition >= 0x0200)
            {
                // Returned Parlor's shaft wall continues far above the row-$20 shelf. A
                // landing height therefore cannot prove that Samus reached the eastbound
                // passage: several perfectly valid upper ledges remain on the shaft side
                // of the same solid screen-boundary column. End vertical-route ownership
                // only after native horizontal collision has allowed her body centre into
                // screen 2. This observes the cartridge-authored opening itself and cannot
                // mistake a nearby slope or landing animation for completion.
                parlorUpperShaftCleared = true;
            }

            if (roomName == "Parlor return" && parlorUpperShaftCleared &&
                !parlorDescentLipCleared &&
                runtime.LastAerialSamusMovement is { Landed: true } &&
                samus.XPosition >= 0x0330 &&
                samus.YPosition + samus.Kinematics.YRadius < 0x00a0)
            {
                // The one required obstacle jump has landed beyond the raised column-$32
                // lip. From this lower authored shelf onward the intended route descends;
                // suppressing the audit's generic "stuck" jump prevents it from undoing
                // that progress while normal walking and gravity follow the passage.
                parlorDescentLipCleared = true;
                // The earlier shaft ascent naturally visited much larger world Y values.
                // Those are not a descent-progress baseline: retaining them would make the
                // first lower platform appear stalled immediately and launch Samus back up.
                parlorDeepestY = samus.YPosition;
                parlorDescentStallFrames = 0;
            }

            if (roomName == "Parlor return" && !climbUsingWallJumps &&
                runtime.LastAerialSamusMovement is { Landed: true } &&
                climbTargetX is { } attemptedTargetX)
            {
                int landedSupportSurfaceY =
                    samus.YPosition + samus.Kinematics.YRadius;
                var failedTarget = (attemptedTargetX, climbTargetSurfaceY);
                int failureCount = failedClimbTargetCounts.GetValueOrDefault(
                    failedTarget);
                if (landedSupportSurfaceY > climbTargetSurfaceY + 4)
                {
                    failureCount++;
                    failedClimbTargetCounts[failedTarget] = failureCount;
                    // A ceiling-constrained miss is remembered for this exact target. A
                    // shallow transfer will first retry with variable-height release; a
                    // taller or repeatedly missed transfer can use the native wall-jump
                    // fallback. No unrelated graph edge inherits either adaptation.
                    if (climbArcHitCeiling)
                        lowArcClimbTargets.Add(failedTarget);
                }
                if (landedSupportSurfaceY > climbTargetSurfaceY + 4 &&
                    IsCoveredClimbTarget(
                        runtime.LevelData!,
                        attemptedTargetX,
                        climbTargetSurfaceY,
                        samus.Kinematics.XRadius))
                {
                    // Observe the collision system's authoritative landing event before
                    // any grounded planner state can clear the attempted target. A miss is
                    // inferred from pose and Y direction alone can miss the relevant frame.
                    // Restrict adaptive rejection to a cartridge-covered target: an ordinary
                    // open ledge may remain valid after a mistimed controller arc and should
                    // not be silently removed from the route graph.
                    const int failuresBeforeRejectingClimbTarget = 4;
                    if (failureCount >= failuresBeforeRejectingClimbTarget &&
                        rejectedClimbTargets.Add(failedTarget))
                    {
                        if (VerboseDiagnostics)
                        {
                            Console.WriteLine(
                                $"  Rejected repeatedly failed Parlor climb target " +
                                $"(${attemptedTargetX:X4},${climbTargetSurfaceY:X4}) " +
                                $"from ${climbPlanningSupportSurfaceY:X4}; " +
                                $"landed on ${landedSupportSurfaceY:X4} at frame {frame} " +
                                $"after {failureCount} failed arcs.");
                        }
                    }
                }
            }
            if (runtime.LastAerialSamusMovement is
                { WallContact: true, WallJumpTriggered: false })
            {
                climbWallContactDirection = (input & (ushort)SnesButton.Left) != 0
                    ? SnesButton.Left
                    : SnesButton.Right;
            }
            if ((roomName == "Climb" || roomName == "Parlor return") &&
                returningWithMorphBallAtEntry)
            {
                if (runtime.LastAerialSamusMovement is { WallJumpTriggered: true })
                {
                    // The trigger frame installs `$83/$84` and its upward speed, but the
                    // following wall-jump movement frames still use ordinary variable-jump
                    // cutoff. Continue holding A or native quite correctly truncates the
                    // launch to a tiny hop on the very next frame.
                    if (climbUsingWallJumps)
                    {
                        // Right input probes a wall on Samus' left and launches toward the
                        // right; Left is its mirror. Aim at the opposite shaft boundary.
                        climbWallTargetIsRight = climbWallContactDirection == SnesButton.Right;
                        climbWallJumpButtonHoldFrames = 48;
                        climbHoldingTriggeredWallJump = true;
                        // An oversized-gap plan records a collision-selected landing shelf.
                        // Do not enter the centering phase for an unbounded wall-jump climb:
                        // removing horizontal input there would leave Samus sliding down
                        // the wall she had just escaped.
                        if (climbWallJumpHasLandingTarget &&
                            samus.YPosition <= climbWallLandingSurfaceY + 0x30)
                            climbWaitingAboveWallLanding = true;
                    }
                    else
                    {
                        climbJumpHoldFrames = 48;
                    }
                }
                bool climbIsAirborne = samus.Kinematics.YDirection != 0;
                if (climbIsAirborne)
                    climbWaitingForOutwardMotion = false;
                if (climbWasAirborne && !climbIsAirborne)
                {
                    int landedSupportSurfaceY =
                        samus.YPosition + samus.Kinematics.YRadius;
                    if (climbUsingWallJumps)
                    {
                        // A wall-jump arc may terminate on one of the narrow side lips.
                        // Launch across the shaft from whichever half caught Samus; aiming
                        // back into that adjacent wall produces only a one-frame hop.
                        if (roomName == "Parlor return")
                        {
                            // In Parlor the ledges sit well inside the outer shaft walls.
                            // An ordinary jump that catches one must continue outward until
                            // it reaches a real boundary. A completed wall jump, however,
                            // has already selected the *opposite* boundary; if a middle ledge
                            // interrupts that arc, preserve the selection across the new
                            // grounded launch. Recomputing from the ledge's half would send
                            // Samus straight back to the wall she just left.
                            (int LeftCenterLimit, int RightCenterLimit)? shaftLimits =
                                TryFindContinuousShaftWallLimits(runtime.LevelData!, samus);
                            if (shaftLimits is null)
                            {
                                // Above Parlor's continuous shaft, authored side ledges replace
                                // the two uninterrupted wall columns. Landing there completes
                                // the wall-jump fallback; retire its contact handshake and let
                                // the collision-derived upper-room planner choose the next arc.
                                climbUsingWallJumps = false;
                                climbLaunchAwayFromAdjacentWall = false;
                                climbWaitingAboveWallLanding = false;
                                climbWallLandingEnabled = false;
                                climbWallJumpHasLandingTarget = false;
                            }
                            else
                            {
                                (int leftLimit, int rightLimit) = shaftLimits.Value;
                                const int nativeWallJumpProbeDistance = 8;
                                bool landedAtIntendedWall = climbWallTargetIsRight
                                    ? samus.XPosition >=
                                        rightLimit - nativeWallJumpProbeDistance + 1
                                    : samus.XPosition <=
                                        leftLimit + nativeWallJumpProbeDistance - 1;
                                if (landedAtIntendedWall)
                                {
                                    // A shelf flush with the selected boundary needs the native
                                    // wall-jump setup: launch a spin away from the wall, then let
                                    // the airborne branch turn back into this *same* wall before
                                    // supplying the released/newly-pressed Jump handshake. Simply
                                    // pressing into the wall from the ground selects neutral art;
                                    // changing the target wall here creates an ordinary away arc.
                                    climbLaunchAwayFromAdjacentWall = true;
                                    // A collision-resolved landing ends the previous airborne
                                    // shelf-centering phase even when it missed that shelf. If
                                    // these latches survive, they intercept the next wall-contact
                                    // handshake and hold Jump continuously, so `$90:9E7F` can
                                    // never observe the required new A edge.
                                    climbWaitingAboveWallLanding = false;
                                    climbWallLandingEnabled = false;
                                }
                                else if (!climbHoldingTriggeredWallJump)
                                {
                                    int shaftCenter = (leftLimit + rightLimit) / 2;
                                    climbWallTargetIsRight = samus.XPosition >= shaftCenter;
                                }
                            }
                        }
                        else
                        {
                            climbWallTargetIsRight = samus.XPosition < 0x0180;
                        }
                        if (climbWallLandingEnabled)
                        {
                            bool reachedSelectedLanding =
                                landedSupportSurfaceY <= climbWallLandingSurfaceY + 4;

                            // Only the saved bank-$94 elevation completes this transfer.
                            // A short wall arc can legitimately catch the lower source slope
                            // again; treating that collision as success discards the selected
                            // shelf and sends the ordinary planner downhill. Instead clear the
                            // centering phase and let the next grounded input begin another
                            // native wall-jump cycle from the real collision-resolved support.
                            climbUsingWallJumps = !reachedSelectedLanding;
                            climbWallLandingEnabled = false;
                            climbWaitingAboveWallLanding = false;
                            if (reachedSelectedLanding)
                                climbWallJumpHasLandingTarget = false;
                        }
                    }
                    // A variable-height hold belongs only to the arc that created it. If
                    // its remaining frames leak across landing, the next launch never has
                    // a newly-pressed A edge and the ROM table produces a tiny neutral hop.
                    climbJumpHoldFrames = 0;
                    climbJumpReady = true;
                    climbGroundedRunupFrames = 0;
                    climbAirborneFrames = 0;
                    climbTargetX = null;
                    climbWallJumpButtonHoldFrames = 0;
                    climbHoldingTriggeredWallJump = false;
                    climbNeedsMomentumRunup = false;
                    climbRunningTowardLaunch = false;
                    climbPreserveTargetwardMomentum = false;
                    climbRequiresOutwardLaunch = false;
                    climbWaitingForOutwardMotion = false;
                }
                climbWasAirborne = climbIsAirborne;
            }
            if (approachingBlueBrinstarElevator &&
                (runtime.Enemies.ElevatorStatus != previousElevatorStatus ||
                 runtime.Enemies.LastElevatorEvent != ElevatorFrameEvent.None ||
                 runtime.HasPendingDoorTransition))
            {
                if (VerboseDiagnostics)
                {
                    Console.WriteLine(
                        $"  Elevator f{frame}: status={previousElevatorStatus}->" +
                        $"{runtime.Enemies.ElevatorStatus}, flags=${runtime.Enemies.ElevatorFlags:X4}, " +
                        $"event={runtime.Enemies.LastElevatorEvent}, input=${input:X4}, " +
                        $"Samus=(${samus.XPosition:X4},${samus.YPosition:X4}), " +
                        $"pending={runtime.LevelData?.PendingDoorTransition?.Pointer.ToString("X4") ?? "-"}.");
                }
                previousElevatorStatus = runtime.Enemies.ElevatorStatus;
            }
            int currentScrollPlmCount = runtime.Plms.ScrollPlms.Count;
            if (currentScrollPlmCount != previousScrollPlmCount)
            {
                if (VerboseDiagnostics)
                {
                    Console.WriteLine(
                        $"  {roomName} scroll PLMs changed {previousScrollPlmCount}->{currentScrollPlmCount} " +
                        $"at frame {frame}, Samus=(${samus.XPosition:X4},${samus.YPosition:X4}).");
                }
                previousScrollPlmCount = currentScrollPlmCount;
            }

            if (aboveParlorFloorHatch &&
                (input & (ushort)SnesButton.X) != 0 &&
                floorHatchShotTrace.Count < 16)
            {
                floorHatchShotTrace.Add(
                    $"f{frame}:p${poseBeforeStep:X2}->${samus.Pose:X2}/" +
                    $"y${yBeforeStep:X4}->${samus.YPosition:X4}/" +
                    $"fire={runtime.Projectiles.LastFrameResult.FiredSlot?.ToString() ?? "-"}/" +
                    $"hit={runtime.Projectiles.LastFrameResult.CollisionStartedExplosion}/" +
                    $"deleted={runtime.Projectiles.LastFrameResult.ProjectileDeleted}/" +
                    $"spawn={FormatSpawn(runtime.Projectiles.LastFiredProjectileSnapshot)}/" +
                    $"cam=(${runtime.Camera?.XPosition:X4},${runtime.Camera?.YPosition:X4})/" +
                    $"slots={string.Join('|', runtime.Projectiles.Slots.Where(slot => slot.IsActive)
                        .Select(slot => $"{slot.SlotIndex}:d{slot.PackedDirection.DirectionIndex}@${slot.YPosition:X4}"))}");
            }

            // Retain only screen-boundary and one-second samples. A failed autonomous
            // route needs enough evidence to distinguish a bad directional decision from
            // collision or camera trouble, without producing a frame-by-frame log large
            // enough to bury the actual exception in CI output.
            int screenX = samus.XPosition >> 8;
            int screenY = samus.YPosition >> 8;
            bool detailedReturnedClimbSample =
                roomName is "Climb" or "Parlor return" &&
                returningWithMorphBallAtEntry &&
                ((frame <= 260 && frame % 10 == 0) ||
                 (roomName == "Parlor return" &&
                  (frame is >= 3880 and <= 4040 or >= 6050 and <= 6250)));
            bool detailedPreMissilesReturnSample = descendingPreMissiles &&
                samus.MaxMissiles != 0 &&
                frame <= 420 && frame % 10 == 0;
            if (screenX != previousScreenX || screenY != previousScreenY ||
                (frame % 60 == 0 && frame <= 700) || detailedReturnedClimbSample ||
                detailedPreMissilesReturnSample)
            {
                routeTrace.Add(
                    $"f{frame}:(${samus.XPosition:X4},${samus.YPosition:X4})/" +
                    $"p${samus.Pose:X2}/s({screenX},{screenY})" +
                    (roomName == "Flyway"
                        ? $"/hud={samus.SelectedHudItem}/missiles={samus.Missiles}/" +
                          $"doors=[{string.Join(' ', runtime.Plms.ColoredDoors)}]/" +
                          $"shots=[{string.Join(' ', runtime.Projectiles.Slots.Where(slot => slot.IsActive).Select(slot => $"{slot.Type:X4}@{slot.XPosition:X4},{slot.YPosition:X4}"))}]"
                        : "") +
                    (roomName is "Climb" or "Parlor return" &&
                     returningWithMorphBallAtEntry
                        ? $"/lock={samus.InputLocked}/" +
                          $"target={climbTargetX?.ToString("X4") ?? "-"}/" +
                          $"surface={climbTargetSurfaceY:X4}/r={samus.Kinematics.YRadius}/" +
                          $"approach={climbApproachX:X4}/dir={horizontalDirection}/in=${input:X4}/" +
                          $"runup={climbRunupStartX:X4}->{climbRunupLaunchX:X4}/" +
                          $"active={climbNeedsMomentumRunup}/{climbRunningTowardLaunch}/" +
                          $"vcol={runtime.LastAerialSamusMovement?.Vertical?.Collided}/" +
                          $"vdisp={runtime.LastAerialSamusMovement?.Vertical?.AcceptedDisplacement}/" +
                          $"land={runtime.LastAerialSamusMovement?.Landed}"
                        : "") +
                    (detailedPreMissilesReturnSample
                        ? $"/stage={preMissilesAscentStage}/" +
                          $"ydir={samus.Kinematics.YDirection}/" +
                          $"move={samus.ReadMovementKind(bus)}/in=${input:X4}"
                        : "") +
                    (detailedReturnedClimbSample
                        ? $"/af{samus.AnimationFrame}/in${input:X4}/" +
                          $"wall={runtime.LastAerialSamusMovement?.WallContact}/" +
                          $"hcol={runtime.LastAerialSamusMovement?.Horizontal.Collided}/" +
                          $"ceil={runtime.LastAerialSamusMovement?.HitCeiling}/" +
                          $"hold={climbJumpHoldFrames}/wallMode={climbUsingWallJumps}/" +
                          $"wallRight={climbWallTargetIsRight}/wallHold={climbWallJumpButtonHoldFrames}/" +
                          $"launchAway={climbLaunchAwayFromAdjacentWall}/contactDir={climbWallContactDirection}/" +
                          $"yd={samus.Kinematics.YDirection}/" +
                          $"ys=${samus.Kinematics.YSpeed:X4}.${samus.Kinematics.YSubspeed:X4}"
                        : ""));
                if (VerboseDiagnostics && detailedReturnedClimbSample)
                    Console.WriteLine($"  TRACE {routeTrace[^1]}");
                previousScreenX = screenX;
                previousScreenY = screenY;
            }
            if (runtime.Projectiles.LastFrameResult.FiredSlot is { } firedSlot)
            {
                firedShots++;
                if (crossingMorphBallRoom && morphBallShotTrace.Count < 24)
                {
                    morphBallShotTrace.Add(
                        $"f{frame}:samus=(${samus.XPosition:X4},${samus.YPosition:X4})/" +
                        $"p${samus.Pose:X2}/spawn={FormatSpawn(runtime.Projectiles.LastFiredProjectileSnapshot)}/" +
                        $"cam=(${runtime.Camera?.XPosition:X4},${runtime.Camera?.YPosition:X4})/" +
                        $"hit={runtime.Projectiles.LastFrameResult.CollisionStartedExplosion}");
                }
                int firedDirection = runtime.Projectiles.LastFiredProjectileSnapshot is { } spawn
                    ? new SamusProjectileDirectionWord(spawn.Direction).DirectionIndex
                    : runtime.Projectiles.Slots[firedSlot].PackedDirection.DirectionIndex;
                firedByDirection[firedDirection]++;
            }
            if (runtime.Projectiles.LastFrameResult.CollisionStartedExplosion)
            {
                collisionExplosions++;
                // Explosion setup preserves the direction nibble in its allocated slot.
                // Attribute the event before the slot's later animation deletes it.
                SamusProjectileSlot? collisionSlot = runtime.Projectiles.Slots.FirstOrDefault(
                    slot => slot.PackedDirection.HasLowByteLifecycleState);
                if (collisionSlot is not null)
                    collisionsByDirection[collisionSlot.PackedDirection.DirectionIndex]++;
                if (crossingMorphBallRoom && morphBallCollisionTrace.Count < 24)
                {
                    SamusProjectileSlot? impact = runtime.Projectiles.Slots.FirstOrDefault(
                        slot => slot.PackedType.Family == SamusProjectileFamily.BeamExplosion);
                    if (impact is not null)
                    {
                        RoomCollisionBlock impactBlock = runtime.LevelData!.GetCollisionBlockAtPixel(
                            impact.XPosition,
                            impact.YPosition);
                        morphBallCollisionTrace.Add(
                            $"f{frame}:d{impact.PackedDirection.DirectionIndex}@" +
                            $"(${impact.XPosition:X4},${impact.YPosition:X4})/" +
                            $"r({impact.XRadius},{impact.YRadius})/" +
                            $"block={impactBlock.Index}:" +
                            $"{impactBlock.CollisionType:X1}/{impactBlock.Behavior:X2}");
                    }
                }
            }

            bool moved = samus.XPosition != previousX || samus.YPosition != previousY;
            stationaryFrames = moved ? 0 : stationaryFrames + 1;
            // Preserve the actual post-step horizontal displacement before updating the
            // position snapshot. The next controller decision uses this to distinguish a
            // held direction from genuine movement produced by bank-$90 acceleration.
            lastHorizontalDelta = samus.XPosition - previousX;
            horizontallyStationaryFrames = samus.XPosition != previousX
                ? 0
                : horizontallyStationaryFrames + 1;
            previousX = samus.XPosition;
            previousY = samus.YPosition;
        }

        if (!runtime.HasPendingDoorTransition)
        {
            Console.WriteLine($"  {roomName} route trace:");
            foreach (string sample in routeTrace)
                Console.WriteLine($"    {sample}");
            if (floorHatchShotTrace.Count != 0)
                Console.WriteLine($"  Floor-hatch shots: {string.Join(' ', floorHatchShotTrace)}");
            if (morphBallShotTrace.Count != 0)
                Console.WriteLine($"  Morph-Ball shots: {string.Join(' ', morphBallShotTrace)}");
            if (morphBallCollisionTrace.Count != 0)
                Console.WriteLine($"  Morph-Ball impacts: {string.Join(' ', morphBallCollisionTrace)}");
            WriteCollisionMap(runtime, samus, roomName);
            PrintCollisionNeighborhood(runtime, samus);
            Console.WriteLine(
                $"  Enemy quota: {runtime.Enemies.EnemiesKilled}/" +
                $"{runtime.Enemies.DeathQuota}; grey doors: " +
                $"[{string.Join(", ", runtime.Plms.GreyDoors)}]; interactive native slots: " +
                $"[{string.Join(',', runtime.Enemies.InteractiveEnemyIndexes.Select(index =>
                    $"${index:X4}"))}].");
            Console.WriteLine(
                $"  Live enemies: [{string.Join(", ", runtime.Enemies.Slots
                    .Where(slot => slot.EnemyDefinitionPointer is not (0 or 0xdaff))
                    .Select(slot =>
                        $"{slot.SlotIndex}:${slot.EnemyDefinitionPointer:X4}/" +
                        $"hp{slot.Health}@(${slot.XPosition:X4},${slot.YPosition:X4})/" +
                        $"map${slot.SpritemapPointer:X4}"))}].");
            throw new InvalidDataException(
                $"Controller route did not leave {roomName} in {frame} frames; " +
                $"Samus=(${samus.XPosition:X4},${samus.YPosition:X4}), pose=${samus.Pose:X2}, " +
                $"inputLocked={samus.InputLocked}, " +
                $"Y={samus.Kinematics.YDirection}:" +
                $"${samus.Kinematics.YSpeed:X4}.${samus.Kinematics.YSubspeed:X4}, " +
                $"stationary={stationaryFrames}, shots={firedShots}, " +
                $"collision explosions={collisionExplosions}, PLMs={runtime.Plms.ActiveCount}, " +
                $"direction={horizontalDirection}, morphTunnel=" +
                $"{parlorMorphTunnelActive}/{parlorMorphTunnelEnteredBall}/" +
                $"exit={parlorMorphTunnelExitX:X4}/phase={parlorMorphTunnelInputFrames}, " +
                $"shot directions=[{FormatCounts(firedByDirection)}], " +
                $"collision directions=[{FormatCounts(collisionsByDirection)}], " +
                $"projectiles=[{string.Join(", ", runtime.Projectiles.Slots
                    .Where(slot => slot.InstructionPointer != 0)
                    .Select(slot => $"d{slot.Direction}:(${slot.XPosition:X4},${slot.YPosition:X4})/t${slot.Type:X4}"))}].");
        }

        Console.WriteLine(
            $"  {roomName} controller exit: frames={frame}, health={samus.Health}, " +
            $"Samus=(${samus.XPosition:X4},${samus.YPosition:X4}).");
        if (damageTrace.Count != 0)
            Console.WriteLine($"  {roomName} damage: {string.Join(' ', damageTrace)}");
        if (climbCombatTrace.Count != 0)
            Console.WriteLine($"  {roomName} combat: {string.Join(' ', climbCombatTrace)}");
        if (roomName == "Climb" && !returningWithMorphBallAtEntry)
        {
            if (outboundClimbDownAimFrames == 0 ||
                outboundClimbDownShotPulses == 0 ||
                outboundClimbPostShotLandings == 0)
            {
                throw new InvalidDataException(
                    "Outbound Climb route did not exercise and survive the reported " +
                    $"downward-fire fall: aim={outboundClimbDownAimFrames}, " +
                    $"shots={outboundClimbDownShotPulses}, " +
                    $"landings={outboundClimbPostShotLandings}.");
            }
            Console.WriteLine(
                $"  Climb downward-fire fall: aim={outboundClimbDownAimFrames}, " +
                $"shots={outboundClimbDownShotPulses}, " +
                $"post-shot landings={outboundClimbPostShotLandings}.");
        }
        if (roomName == "Morph Ball room" && !verifiedMorphBallBeforePickup)
        {
            throw new InvalidDataException(
                "Controller route collected Morph Ball without observing its cartridge " +
                "dynamic block at a fully visible BG1 location before acquisition.");
        }
        return new DriveResult(frame, firedShots, collisionExplosions);
    }

    private static int FindParlorDescentLaneCenter(
        RoomLevelData level,
        SamusState samus,
        out int waypointRow,
        out int routeDistance,
        out string routePreview)
    {
        const int firstPassageColumn = 0x28;
        const int lastPassageColumn = 0x3d;
        const int flywayDoorInteriorColumn = 0x3d;
        const int flywayDoorInteriorRow = 0x27;
        const int pathLookAheadCells = 32;
        const int maximumStartSnapCells = 8;

        // A single row contains several genuine-looking pits that terminate on solid
        // terrain one or two blocks later. Build a distance field backwards from the air
        // cell immediately inside Flyway's cartridge door. Unlike merely choosing an open
        // run on Samus' current row, following decreasing distance distinguishes the route
        // ahead from destination-connected air that happens to lie behind her.
        var distanceToFlyway = new int[level.WidthInBlocks, level.HeightInBlocks];
        var frontier = new Queue<(int X, int Y)>();
        if (level.GetCollisionBlock(
                flywayDoorInteriorColumn,
                flywayDoorInteriorRow).CollisionType != 0)
        {
            throw new InvalidDataException(
                "Flyway door interior is not cartridge air in the active Parlor state.");
        }
        // Zero means unreachable. Storing the destination as one avoids a separate visited
        // bitmap and leaves every predecessor's value equal to its edge distance plus one.
        distanceToFlyway[flywayDoorInteriorColumn, flywayDoorInteriorRow] = 1;
        frontier.Enqueue((flywayDoorInteriorColumn, flywayDoorInteriorRow));
        ReadOnlySpan<(int X, int Y)> neighborOffsets =
            [(1, 0), (-1, 0), (0, 1), (0, -1)];
        while (frontier.TryDequeue(out (int X, int Y) cell))
        {
            foreach ((int offsetX, int offsetY) in neighborOffsets)
            {
                int neighborX = cell.X + offsetX;
                int neighborY = cell.Y + offsetY;
                if ((uint)neighborX >= (uint)level.WidthInBlocks ||
                    (uint)neighborY >= (uint)level.HeightInBlocks ||
                    distanceToFlyway[neighborX, neighborY] != 0 ||
                    level.GetCollisionBlock(neighborX, neighborY).CollisionType is not
                        (0 or 1 or 3 or 5))
                {
                    continue;
                }
                distanceToFlyway[neighborX, neighborY] =
                    distanceToFlyway[cell.X, cell.Y] + 1;
                frontier.Enqueue((neighborX, neighborY));
            }
        }

        int routeX = Math.Clamp(
            samus.XPosition >> 4,
            firstPassageColumn,
            lastPassageColumn);
        int routeY = Math.Clamp(
            samus.YPosition >> 4,
            0,
            level.HeightInBlocks - 1);
        if (distanceToFlyway[routeX, routeY] == 0)
        {
            // Samus' centre can briefly occupy a pixel column whose containing block is a
            // slope even though her physical body is in the open quadrant. Snap only to the
            // nearest destination-connected cell; a broad global search could jump across
            // an authored wall and manufacture a route that collision cannot traverse.
            int bestSnapDistance = int.MaxValue;
            int snappedX = -1;
            int snappedY = -1;
            for (int radius = 1; radius <= maximumStartSnapCells; radius++)
            {
                for (int candidateY = Math.Max(0, routeY - radius);
                     candidateY <= Math.Min(level.HeightInBlocks - 1, routeY + radius);
                     candidateY++)
                {
                    for (int candidateX = Math.Max(firstPassageColumn, routeX - radius);
                         candidateX <= Math.Min(lastPassageColumn, routeX + radius);
                         candidateX++)
                    {
                        int snapDistance = Math.Abs(candidateX - routeX) +
                            Math.Abs(candidateY - routeY);
                        if (snapDistance > radius || snapDistance >= bestSnapDistance ||
                            distanceToFlyway[candidateX, candidateY] == 0)
                        {
                            continue;
                        }
                        bestSnapDistance = snapDistance;
                        snappedX = candidateX;
                        snappedY = candidateY;
                    }
                }
                if (snappedX >= 0)
                    break;
            }
            if (snappedX < 0)
            {
                throw new InvalidDataException(
                    $"Parlor position (${samus.XPosition:X4},${samus.YPosition:X4}) " +
                    "has no nearby cartridge-connected route cell to Flyway.");
            }
            routeX = snappedX;
            routeY = snappedY;
        }

        int waypointX = routeX;
        int waypointY = routeY;
        routeDistance = distanceToFlyway[routeX, routeY] - 1;
        var previewCells = new List<string> { $"{routeX:X2},{routeY:X2}" };
        int horizontalSegmentDirection = 0;
        for (int step = 0; step < pathLookAheadCells; step++)
        {
            int currentDistance = distanceToFlyway[routeX, routeY];
            int nextX = routeX;
            int nextY = routeY;
            foreach ((int offsetX, int offsetY) in neighborOffsets)
            {
                int candidateX = routeX + offsetX;
                int candidateY = routeY + offsetY;
                if ((uint)candidateX >= (uint)level.WidthInBlocks ||
                    (uint)candidateY >= (uint)level.HeightInBlocks ||
                    candidateX < firstPassageColumn || candidateX > lastPassageColumn)
                {
                    continue;
                }
                int candidateDistance = distanceToFlyway[candidateX, candidateY];
                if (candidateDistance != 0 && candidateDistance < currentDistance)
                {
                    nextX = candidateX;
                    nextY = candidateY;
                    break;
                }
            }
            if (nextX == routeX && nextY == routeY)
                break;

            int nextHorizontalDirection = Math.Sign(nextX - routeX);
            if (nextHorizontalDirection != 0)
            {
                if (horizontalSegmentDirection != 0 &&
                    nextHorizontalDirection != horizontalSegmentDirection)
                {
                    // A reversal begins a later corridor segment. Stop at the bend rather
                    // than steering through intervening geometry toward a distant endpoint.
                    break;
                }
                horizontalSegmentDirection = nextHorizontalDirection;
            }
            else if (horizontalSegmentDirection != 0)
            {
                // The first vertical edge after horizontal travel is the platform opening
                // this controller must reach. Gravity owns the descent from that point and
                // the next supported frame will compute the following path segment.
                break;
            }

            routeX = nextX;
            routeY = nextY;
            previewCells.Add($"{routeX:X2},{routeY:X2}");
            // Slopes/extensions are admitted only to preserve topological connectivity.
            // The steering point itself remains in cartridge air so the controller never
            // deliberately aims Samus' centre into a solid quadrant or PLM carrier block.
            if (level.GetCollisionBlock(routeX, routeY).CollisionType == 0)
            {
                waypointX = routeX;
                waypointY = routeY;
            }
        }

        waypointRow = waypointY;
        routePreview = string.Join('>', previewCells);
        int targetX = waypointX * LevelBlockSizePixels + LevelBlockSizePixels / 2;
        return Math.Clamp(
            targetX,
            firstPassageColumn * LevelBlockSizePixels + samus.Kinematics.XRadius,
            (lastPassageColumn + 1) * LevelBlockSizePixels - samus.Kinematics.XRadius - 1);
    }

    private static (int LeftCenterLimit, int RightCenterLimit)
        FindContinuousShaftWallLimits(RoomLevelData level, SamusState samus) =>
        TryFindContinuousShaftWallLimits(level, samus) ??
            throw new InvalidDataException(
                $"No continuous cartridge shaft walls bracket Samus at " +
                $"(${samus.XPosition:X4},${samus.YPosition:X4}).");

    private static (int LeftCenterLimit, int RightCenterLimit)?
        TryFindContinuousShaftWallLimits(
            RoomLevelData level,
            SamusState samus,
            int? sampleCenterY = null)
    {
        const int verticalSampleRadius = 4;
        const int requiredSolidSamples = 7;
        int centerBlockX = samus.XPosition >> 4;
        int centerBlockY = (sampleCenterY ?? samus.YPosition) >> 4;
        int top = Math.Max(0, centerBlockY - verticalSampleRadius);
        int bottom = Math.Min(
            level.HeightInBlocks - 1,
            centerBlockY + verticalSampleRadius);

        static bool IsContinuousWallColumn(
            RoomLevelData room,
            int blockX,
            int firstRow,
            int lastRow)
        {
            int solidSamples = 0;
            for (int blockY = firstRow; blockY <= lastRow; blockY++)
            {
                // Long shaft boundaries are ordinary type-$8 blocks. Counting only that
                // family deliberately excludes one-row ledges and their type-$1 slopes,
                // which are valid wall-jump collision but not the controller's intended
                // opposite boundary.
                if (room.GetCollisionBlock(blockX, blockY).CollisionType == 8)
                    solidSamples++;
            }
            return solidSamples >= Math.Min(
                requiredSolidSamples,
                lastRow - firstRow + 1);
        }

        int leftWallBlock = -1;
        for (int blockX = centerBlockX - 1; blockX >= 0; blockX--)
        {
            if (!IsContinuousWallColumn(level, blockX, top, bottom))
                continue;
            leftWallBlock = blockX;
            break;
        }

        int rightWallBlock = -1;
        for (int blockX = centerBlockX + 1;
             blockX < level.WidthInBlocks;
             blockX++)
        {
            if (!IsContinuousWallColumn(level, blockX, top, bottom))
                continue;
            rightWallBlock = blockX;
            break;
        }

        if (leftWallBlock < 0 || rightWallBlock < 0)
            return null;

        // For an ordinary solid, bank $94 clips the body center exactly one radius away
        // from the wall's inner tile edge. Keep those derived limits separate from the
        // eight-pixel native wall-jump probe window used by the caller.
        int leftCenterLimit = (leftWallBlock + 1) * 16 + samus.Kinematics.XRadius;
        int rightCenterLimit = rightWallBlock * 16 - samus.Kinematics.XRadius;
        return (leftCenterLimit, rightCenterLimit);
    }

    private static ClimbPlatformTarget? FindNextClimbPlatformCenter(
        ISnesAddressSpace bus,
        RoomLevelData level,
        RoomPlmSystem plms,
        SamusState samus,
        int maximumRise = 112,
        bool preferSupportedApproach = false,
        IReadOnlySet<(int X, int SurfaceY)>? rejectedTargets = null)
    {
        const int shaftLeftBlock = 0x12;
        const int shaftRightBlock = 0x1d;
        const int upperBoundaryTopBlock = 0x1e;
        const int upperBoundaryTopRow = 0x20;
        const int topDoorCapRow = 0x02;
        const int upwardSearchRows = 12;
        const int topDoorCenterX = 0x0178;
        const int minimumSurfaceRise = 8;

        // BTS bit seven is a slope mirror bit, not a universal floor/ceiling flag. Its
        // meaning depends on the low five-bit shape index, so classifying `$82` as a
        // ceiling and `$02` as a floor (or vice versa) eventually chooses an underside.
        // Ask the actual bank-$94 downward dispatcher instead. More importantly, probe
        // every possible Samus center pixel rather than each block center. Square slopes
        // deliberately make only one quadrant solid; a block-center target can therefore
        // put one foot over the hollow half even though the block itself is a valid floor.
        // The disposable body has Samus' real radii, so any accepted X coordinate already
        // includes the horizontal clearance that a genuine landing requires.
        int currentSurfaceY = samus.YPosition + samus.Kinematics.YRadius;
        int supportRow = currentSurfaceY >> 4;
        if (currentSurfaceY <= 0x0080)
        {
            // Once Samus reaches the broad top ledge (or either one-block post above it),
            // no further landing target is required. The remaining geometry is a breakable
            // cap at row $2 and the upward type-$9 door at row $0. A terminal target keeps
            // steering inside that open four-block shaft while ordinary projectile and
            // vertical collision code perform both state changes.
            return new ClimbPlatformTarget(
                topDoorCenterX,
                SurfaceY: topDoorCapRow * 16,
                ShadowLeftX: 0x0160,
                ShadowRightX: 0x019f,
                IsDoorApproach: true);
        }
        int bestCenter = 0;
        int bestSurfaceY = 0;
        int bestShadowLeftX = 0;
        int bestShadowRightX = 0;
        int bestRise = int.MaxValue;
        int bestHorizontalDistance = int.MaxValue;
        bool bestIsSolidLedge = false;
        bool bestHasSupportedApproach = false;
        (int connectedSupportLeftX, int connectedSupportRightX) =
            preferSupportedApproach
                ? FindClimbGroundSupportSpan(bus, level, plms, samus)
                : (samus.XPosition, samus.XPosition);
        int firstRow = supportRow - 1;
        int lastRow = Math.Max(0, supportRow - upwardSearchRows);
        for (int blockY = firstRow; blockY >= lastRow; blockY--)
        {
            var rowSamples = new List<ClimbLandingSample>();
            int firstCenterX = shaftLeftBlock * 16 + samus.Kinematics.XRadius;
            // Column $1E is the shaft's vertical right wall below row $20 and must not be
            // sampled as a landing. At and above row $20 its exposed top is the genuine
            // eastbound transfer that crosses into Parlor's next screen.
            int rowRightBlock = blockY <= upperBoundaryTopRow
                ? upperBoundaryTopBlock
                : shaftRightBlock;
            int lastCenterX = (rowRightBlock + 1) * 16 -
                samus.Kinematics.XRadius - 1;
            for (int centerX = firstCenterX; centerX <= lastCenterX; centerX++)
            {
                RoomCollisionBlock candidate = level.GetCollisionBlock(centerX >> 4, blockY);
                // Climb's repeating shaft uses square slopes, but its final visible ledges
                // are ordinary solid blocks. Admit both candidate families, then let the
                // direction-aware bank-$94 probe below decide whether Samus can stand at
                // this exact body coordinate. Skipping air here is important: a 17-pixel
                // probe from an empty row can otherwise rediscover the floor below it and
                // incorrectly widen a narrow platform's usable centre range.
                if (candidate.CollisionType is not (1 or 8))
                    continue;
                if (candidate.CollisionType == 8 && blockY > 0 &&
                    level.GetCollisionBlock(centerX >> 4, blockY - 1).CollisionType == 8)
                {
                    // The side walls are long vertical stacks of type-$8 blocks. Their top
                    // edge is technically a downward collision surface, but it is not an
                    // in-shaft landing: the block immediately above is solid too. Reject
                    // those wall columns while retaining isolated type-$8 top ledges.
                    continue;
                }

                var probe = new SamusKinematicsState
                {
                    XPosition = unchecked((ushort)centerX),
                    YPosition = unchecked((ushort)(blockY * 16 - samus.Kinematics.YRadius - 1)),
                    XRadius = samus.Kinematics.XRadius,
                    YRadius = samus.Kinematics.YRadius,
                    YDirection = 2,
                    HorizontalSlopeCollisionEnable = samus.Kinematics.HorizontalSlopeCollisionEnable,
                };
                BlockMoveResult result = SamusBlockCollision.MoveVertical(
                    bus,
                    level,
                    probe,
                    // Bank $94 samples the destination row rather than sweeping every
                    // crossed row. End with the leading boundary in this exact block;
                    // extending one pixel farther would test the row below and falsely
                    // reject a perfectly valid slope candidate.
                    displacement: 17 << 16,
                    scanLeftToRight: true,
                    includeSolidEnemies: false,
                    plms: plms,
                    publishDoorSideEffects: false);
                // A square slope is commonly paired with an adjacent mirrored record.
                // Samus' real-width probe samples both feet, and bank $94 may therefore
                // report the neighboring member even though this exact centre coordinate
                // is supported by the same authored platform. Collision plus the resolved
                // surface is authoritative; nominal block-index identity is not.
                if (!result.Collided)
                    continue;

                int surfaceY = probe.YPosition + probe.YRadius;
                int rise = currentSurfaceY - surfaceY;
                if (rise < minimumSurfaceRise || rise > maximumRise)
                    continue;
                rowSamples.Add(new ClimbLandingSample(centerX, surfaceY));
            }

            // A square-slope platform commonly spans two BTS records and changes its
            // surface Y across the run. Comparing individual pixels chooses the lowest
            // extreme of that slope. Collapse consecutive accepted body centers into one
            // authored platform and aim at its middle, where both feet have useful margin.
            // The outside approach lane is already chosen separately; aiming at the first
            // legal pixel of a wide solid shelf made Samus reach its edge too late during
            // descent and intermittently miss a physically straightforward landing.
            for (int runStart = 0; runStart < rowSamples.Count;)
            {
                int runEnd = runStart;
                while (runEnd + 1 < rowSamples.Count &&
                    rowSamples[runEnd + 1].X == rowSamples[runEnd].X + 1)
                {
                    runEnd++;
                }

                ClimbLandingSample middle = rowSamples[(runStart + runEnd) / 2];
                if (preferSupportedApproach &&
                    middle.X >= connectedSupportLeftX &&
                    middle.X <= connectedSupportRightX)
                {
                    // A collision-continuous floor run containing Samus is her current
                    // support even when a multi-record square slope presents a higher
                    // surface in the row above. It is not a jump destination. Skipping it
                    // lets the selector consider the next disconnected cartridge ledge;
                    // the eventual run-up may still walk across this entire certified span.
                    runStart = runEnd + 1;
                    continue;
                }
                if (rejectedTargets?.Contains((middle.X, middle.SurfaceY)) == true)
                {
                    // This exact target has already produced a controller-driven fall below
                    // its source support. Preserve the authored run and all collision data;
                    // merely omit it from this audit's next input choice.
                    runStart = runEnd + 1;
                    continue;
                }
                bool runIsSolidLedge =
                    level.GetCollisionBlock(middle.X >> 4, blockY).CollisionType == 8;
                int middleRise = currentSurfaceY - middle.SurfaceY;
                int horizontalDistance = Math.Abs(middle.X - samus.XPosition);
                int shadowLeftX = (rowSamples[runStart].X >> 4) * 16;
                int shadowRightX = (rowSamples[runEnd].X >> 4) * 16 + 15;
                bool hasSupportedApproach = false;
                if (preferSupportedApproach)
                {
                    var candidatePlatform = new ClimbPlatformTarget(
                        middle.X,
                        middle.SurfaceY,
                        shadowLeftX,
                        shadowRightX,
                        IsSolidLedge: runIsSolidLedge);
                    int? candidateApproachX = FindClearClimbAscentX(
                        bus,
                        level,
                        plms,
                        samus,
                        candidatePlatform);
                    if (candidateApproachX is null)
                    {
                        // A downward probe can identify a real floor whose underside or an
                        // intervening shelf blocks every full-body ascent lane. That is not
                        // corrupt room data; it is simply not an edge in the controller
                        // route graph from this support, so continue evaluating the row.
                        runStart = runEnd + 1;
                        continue;
                    }
                    hasSupportedApproach = HasClimbGroundSupportToApproach(
                        bus,
                        level,
                        plms,
                        samus,
                        candidateApproachX.Value);
                }
                if (middleRise < bestRise ||
                    (middleRise == bestRise &&
                     (hasSupportedApproach && !bestHasSupportedApproach ||
                      hasSupportedApproach == bestHasSupportedApproach &&
                      horizontalDistance < bestHorizontalDistance)))
                {
                    bestCenter = middle.X;
                    bestSurfaceY = middle.SurfaceY;
                    bestShadowLeftX = shadowLeftX;
                    bestShadowRightX = shadowRightX;
                    bestRise = middleRise;
                    bestHorizontalDistance = horizontalDistance;
                    bestIsSolidLedge = runIsSolidLedge;
                    bestHasSupportedApproach = hasSupportedApproach;
                }

                runStart = runEnd + 1;
            }
        }

        if (bestRise != int.MaxValue)
        {
            return new ClimbPlatformTarget(
                bestCenter,
                bestSurfaceY,
                bestShadowLeftX,
                bestShadowRightX,
                IsSolidLedge: bestIsSolidLedge);
        }

        return null;
    }

    private static bool IsCoveredClimbTarget(
        RoomLevelData level,
        int targetX,
        int targetSurfaceY,
        int horizontalRadius)
    {
        const int spinJumpVerticalRadius = 12;
        int firstBodyRow = (targetSurfaceY - 2 * spinJumpVerticalRadius) >> 4;
        int candidateRow = targetSurfaceY >> 4;
        int firstBodyColumn = (targetX - horizontalRadius) >> 4;
        int lastBodyColumn = (targetX + horizontalRadius - 1) >> 4;

        // A downward bank-$94 probe can expose a square-slope floor face inside a larger
        // solid mass because it samples only the feet's destination row. For failure
        // recovery, inspect the prospective spin body's rows above that face. The support
        // row itself is excluded so a legitimate slope quadrant is never mistaken for a
        // ceiling; only collision families that are unconditionally solid to Samus count.
        for (int blockY = firstBodyRow; blockY < candidateRow; blockY++)
        {
            for (int blockX = firstBodyColumn; blockX <= lastBodyColumn; blockX++)
            {
                if (level.GetCollisionBlock(blockX, blockY).CollisionType is
                    8 or 10 or 12 or 14 or 15)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private static bool IsClimbTopDoorCapClosed(RoomLevelData level)
    {
        const int capRow = 0x02;
        const int capLeftBlock = 0x16;
        const int capRightBlock = 0x19;

        // The main type-$C block owns the mutation; the three type-$5 records extend its
        // collision across the four-tile shaft. Consult all four so the controller waits
        // through any multi-frame removal state instead of assuming one header byte is the
        // complete door-cap lifecycle.
        for (int blockX = capLeftBlock; blockX <= capRightBlock; blockX++)
        {
            if (level.GetCollisionBlock(blockX, capRow).CollisionType is 0x0c or 0x05)
                return true;
        }
        return false;
    }

    private static bool HasClimbGroundSupportToApproach(
        ISnesAddressSpace bus,
        RoomLevelData level,
        RoomPlmSystem plms,
        SamusState samus,
        int approachX)
    {
        const int horizontalSampleSpacing = 2;
        const int probeClearanceAboveExpectedSurface = 8;
        const int maximumSurfaceChangePerSample = 4;
        const int maximumDownwardProbeDistance =
            probeClearanceAboveExpectedSurface + maximumSurfaceChangePerSample;

        int expectedSurfaceY = samus.YPosition + samus.Kinematics.YRadius;
        int direction = Math.Sign(approachX - samus.XPosition);
        if (direction == 0)
            return true;

        // A safe ground run-up needs continuous support for Samus' complete body, not just
        // a solid-looking BTS tile beneath its centre. Sample every two horizontal pixels
        // and let bank $94 resolve the exact square-slope surface under the real body radii.
        // Each sample begins above the preceding surface and descends one pixel at a time;
        // this follows legitimate shallow slopes while rejecting a lip, gap, or large drop.
        for (int centerX = samus.XPosition;
             centerX != approachX;
             centerX = direction > 0
                 ? Math.Min(centerX + horizontalSampleSpacing, approachX)
                 : Math.Max(centerX - horizontalSampleSpacing, approachX))
        {
            int sampledX = direction > 0
                ? Math.Min(centerX + horizontalSampleSpacing, approachX)
                : Math.Max(centerX - horizontalSampleSpacing, approachX);
            var probe = new SamusKinematicsState
            {
                XPosition = unchecked((ushort)sampledX),
                YPosition = unchecked((ushort)(
                    expectedSurfaceY - samus.Kinematics.YRadius -
                    probeClearanceAboveExpectedSurface)),
                XRadius = samus.Kinematics.XRadius,
                YRadius = samus.Kinematics.YRadius,
                YDirection = 2,
                HorizontalSlopeCollisionEnable =
                    samus.Kinematics.HorizontalSlopeCollisionEnable,
            };

            bool foundSupport = false;
            for (int pixel = 0; pixel < maximumDownwardProbeDistance; pixel++)
            {
                BlockMoveResult result = SamusBlockCollision.MoveVertical(
                    bus,
                    level,
                    probe,
                    displacement: 1 << 16,
                    scanLeftToRight: true,
                    includeSolidEnemies: false,
                    plms: plms,
                    publishDoorSideEffects: false);
                if (!result.Collided)
                    continue;

                int resolvedSurfaceY = probe.YPosition + probe.YRadius;
                if (Math.Abs(resolvedSurfaceY - expectedSurfaceY) >
                    maximumSurfaceChangePerSample)
                {
                    return false;
                }
                expectedSurfaceY = resolvedSurfaceY;
                foundSupport = true;
                break;
            }

            if (!foundSupport)
                return false;
        }

        return true;
    }

    private static (int LeftX, int RightX) FindClimbGroundSupportSpan(
        ISnesAddressSpace bus,
        RoomLevelData level,
        RoomPlmSystem plms,
        SamusState samus)
    {
        const int shaftLeftBlock = 0x12;
        const int shaftRightBlock = 0x1d;
        int firstCenterX = shaftLeftBlock * LevelBlockSizePixels +
            samus.Kinematics.XRadius;
        int lastCenterX = (shaftRightBlock + 1) * LevelBlockSizePixels -
            samus.Kinematics.XRadius - 1;
        int leftX = samus.XPosition;
        int rightX = samus.XPosition;
        SamusKinematicsState leftProbe = CreateClimbHorizontalProbe(samus.Kinematics);
        SamusKinematicsState rightProbe = CreateClimbHorizontalProbe(samus.Kinematics);

        // A high Parlor shelf can require more horizontal travel than a standing jump
        // supplies. Find the complete collision-supported span containing Samus so the
        // audit can run to the far edge, reverse, and carry native running momentum into
        // the jump. Each candidate is certified by HasClimbGroundSupportToApproach, which
        // uses the translated bank-$94 downward dispatcher and therefore respects square
        // slopes, body radii, PLM mutations, and genuine gaps in the cartridge-authored
        // floor. A second, cumulative bank-$94 horizontal probe is essential: floor-only
        // samples on opposite sides of a vertical wall can both report valid support even
        // though Samus cannot walk between them. These fixed columns are the continuous
        // Parlor shaft bounds, not guessed platform coordinates; the helper stops at the
        // first unsupported or horizontally obstructed body position.
        for (int candidateX = samus.XPosition - 1;
             candidateX >= firstCenterX;
             candidateX--)
        {
            BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(
                bus,
                level,
                leftProbe,
                displacement: -1 << 16,
                plms: plms,
                // Planning must observe the same door collision result without publishing
                // a transition before the controller-driven body actually reaches it.
                publishDoorSideEffects: false);
            if (horizontal.Collided || leftProbe.XPosition != candidateX)
                break;
            if (!HasClimbGroundSupportToApproach(
                    bus,
                    level,
                    plms,
                    samus,
                    candidateX))
            {
                break;
            }
            leftX = candidateX;
        }

        for (int candidateX = samus.XPosition + 1;
             candidateX <= lastCenterX;
             candidateX++)
        {
            BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(
                bus,
                level,
                rightProbe,
                displacement: 1 << 16,
                plms: plms,
                publishDoorSideEffects: false);
            if (horizontal.Collided || rightProbe.XPosition != candidateX)
                break;
            if (!HasClimbGroundSupportToApproach(
                    bus,
                    level,
                    plms,
                    samus,
                    candidateX))
            {
                break;
            }
            rightX = candidateX;
        }

        return (leftX, rightX);
    }

    private static SamusKinematicsState CreateClimbHorizontalProbe(
        SamusKinematicsState source) => new()
    {
        // Only geometry and the immutable current enemy snapshot participate in this
        // prospective walk. Native fixed-point halves are retained because a wall clip can
        // distinguish `$0000` from `$FFFF`, and slope alignment consumes vertical speed.
        XPosition = source.XPosition,
        XSubposition = source.XSubposition,
        YPosition = source.YPosition,
        YSubposition = source.YSubposition,
        XRadius = source.XRadius,
        YRadius = source.YRadius,
        YSpeed = source.YSpeed,
        YSubspeed = source.YSubspeed,
        YDirection = source.YDirection,
        YAcceleration = source.YAcceleration,
        YSubacceleration = source.YSubacceleration,
        HorizontalSlopeCollisionEnable = source.HorizontalSlopeCollisionEnable,
        PositionAdjustedBySlope = source.PositionAdjustedBySlope,
        InteractiveEnemies = source.InteractiveEnemies,
    };

    private static int? FindClearClimbAscentX(
        ISnesAddressSpace bus,
        RoomLevelData level,
        RoomPlmSystem plms,
        SamusState samus,
        ClimbPlatformTarget platform)
    {
        const int shaftLeftBlock = 0x12;
        const int shaftRightBlock = 0x1d;
        int firstCenterX = shaftLeftBlock * 16 + samus.Kinematics.XRadius;
        int lastCenterX = (shaftRightBlock + 1) * 16 - samus.Kinematics.XRadius - 1;
        var clearCenters = new List<int>();
        for (int centerX = firstCenterX; centerX <= lastCenterX; centerX++)
        {
            // Stay wholly outside the destination platform until Samus' feet rise above
            // it. The upward probe also rejects any intervening mirrored slope, wall, or
            // PLM extension that would turn this nominal edge into a blocked ascent lane.
            bool outsideDestinationShadow =
                centerX + samus.Kinematics.XRadius < platform.ShadowLeftX ||
                centerX - samus.Kinematics.XRadius > platform.ShadowRightX;
            if (outsideDestinationShadow && IsClimbAscentLaneClear(
                    bus,
                    level,
                    plms,
                    samus,
                    platform,
                    centerX))
            {
                clearCenters.Add(centerX);
            }
        }

        int bestX = 0;
        int bestDistance = int.MaxValue;
        for (int runStart = 0; runStart < clearCenters.Count;)
        {
            int runEnd = runStart;
            while (runEnd + 1 < clearCenters.Count &&
                clearCenters[runEnd + 1] == clearCenters[runEnd] + 1)
            {
                runEnd++;
            }

            int runLeft = clearCenters[runStart];
            int runRight = clearCenters[runEnd];
            // Square slopes have a forgiving hollow quadrant and the proven Climb route
            // needs the nearest legal edge so it has enough time to cross onto the top.
            // A flat type-$8 ledge has an abrupt full-width underside instead; use the
            // middle of its clear approach lane so native inertia cannot carry Samus one
            // pixel back into a neighboring ceiling during the ascent.
            int centerX;
            if (platform.IsSolidLedge)
            {
                // Every coordinate in this run has already moved Samus' complete spin body
                // to the destination height in both native scan orders. Use the proven edge
                // nearest the ledge. An extra fractional inset looked conservative, but it
                // increased the post-clearance crossing beyond the cartridge jump arc and
                // made otherwise reachable flat shelves impossible to land on.
                bool laneIsLeftOfLedge = runRight < platform.ShadowLeftX;
                centerX = laneIsLeftOfLedge
                    ? runRight
                    : runLeft;
            }
            else
            {
                centerX = Math.Clamp(samus.XPosition, runLeft, runRight);
            }
            int distance = Math.Abs(centerX - samus.XPosition);
            if (distance < bestDistance)
            {
                bestX = centerX;
                bestDistance = distance;
            }

            runStart = runEnd + 1;
        }

        if (bestDistance != int.MaxValue)
            return bestX;

        return null;
    }

    private static bool IsClimbAscentLaneClear(
        ISnesAddressSpace bus,
        RoomLevelData level,
        RoomPlmSystem plms,
        SamusState samus,
        ClimbPlatformTarget platform,
        int centerX)
    {
        const ushort spinJumpVerticalRadius = 12;
        int destinationCenterY = platform.SurfaceY - spinJumpVerticalRadius - 1;
        int pixelDisplacement = destinationCenterY - samus.YPosition;
        if (pixelDisplacement >= 0)
            return true;

        // Native alternates the order in which the two body-edge columns are inspected by
        // NMI-frame parity. Square slopes are deliberately asymmetric, so a route lane is
        // safe only if *both* orders can traverse it; checking one order let the Parlor
        // planner choose the hollow half of a mirrored ceiling and then collide on the
        // first opposite-parity gameplay frame.
        foreach (bool scanLeftToRight in new[] { true, false })
        {
            var probe = new SamusKinematicsState
            {
                XPosition = unchecked((ushort)centerX),
                YPosition = samus.YPosition,
                XRadius = samus.Kinematics.XRadius,
                YRadius = spinJumpVerticalRadius,
                YDirection = 1,
                HorizontalSlopeCollisionEnable =
                    samus.Kinematics.HorizontalSlopeCollisionEnable,
            };
            // MoveVertical intentionally mirrors bank $94 and samples the destination row;
            // it is not a swept-volume API. Even a four-pixel diagnostic request can step
            // across the one-pixel contact boundary of a square slope. Walk the disposable
            // body upward one pixel at a time so every authored surface and both native
            // scan orders are tested.
            while (probe.YPosition > destinationCenterY)
            {
                BlockMoveResult result = SamusBlockCollision.MoveVertical(
                    bus,
                    level,
                    probe,
                    displacement: -(1 << 16),
                    scanLeftToRight,
                    includeSolidEnemies: false,
                    plms: plms,
                    publishDoorSideEffects: false);
                if (result.Collided)
                    return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Returns true when the next horizontal collision column contains a clear ball-height
    /// channel but an unconditionally solid block in the extra rows occupied by Samus's
    /// current humanoid body.
    /// </summary>
    private static bool HorizontalPassageRequiresMorphBall(
        RoomLevelData level,
        SamusState samus,
        SnesButton direction)
    {
        if (direction is not (SnesButton.Left or SnesButton.Right))
            return false;

        // Morph entry preserves the feet position while changing the vertical radius to
        // seven. Deriving the prospective rows this way makes the detector independent of
        // pose IDs and avoids asking gameplay code to perform a speculative pose mutation.
        const int morphBallRadius = 7;
        int feetY = samus.YPosition + samus.Kinematics.YRadius;
        int ballTopRow = (feetY - 2 * morphBallRadius) >> 4;
        int ballBottomRow = (feetY - 1) >> 4;
        int humanoidTopRow =
            (samus.YPosition - samus.Kinematics.YRadius) >> 4;
        int humanoidBottomRow = (feetY - 1) >> 4;

        // Match MoveHorizontal's destination-leading-boundary convention for a one-pixel
        // request. On the right that is centre + radius; on the left it is centre-radius-1.
        int leadingPixelX = direction == SnesButton.Right
            ? samus.XPosition + samus.Kinematics.XRadius
            : samus.XPosition - samus.Kinematics.XRadius - 1;
        int blockX = leadingPixelX >> 4;
        if (blockX < 0 || blockX >= level.WidthInBlocks ||
            ballTopRow < 0 || humanoidBottomRow >= level.HeightInBlocks)
        {
            return false;
        }

        // Every ball row must be body-passable. Special-air scroll triggers and ordinary
        // air therefore count as clearance, while the same solid families handled by the
        // horizontal bank-$94 dispatcher reject the candidate. Square slopes are left to
        // normal collision: this helper recognizes only the unambiguous solid-wall case.
        for (int row = ballTopRow; row <= ballBottomRow; row++)
        {
            if (IsUnconditionallyBodySolid(level.GetCollisionBlock(blockX, row)))
                return false;
        }

        for (int row = humanoidTopRow; row <= humanoidBottomRow; row++)
        {
            bool outsideBallBody = row < ballTopRow || row > ballBottomRow;
            if (outsideBallBody &&
                IsUnconditionallyBodySolid(level.GetCollisionBlock(blockX, row)))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Mirrors only collision families whose horizontal bank-$94 handlers always return
    /// carry set for Samus's body. Contextual slopes, doors, items, and special air are
    /// deliberately excluded so the route audit cannot invent clearance semantics.
    /// </summary>
    private static bool IsUnconditionallyBodySolid(RoomCollisionBlock block) =>
        block.CollisionType is 8 or 10 or 12 or 14 or 15 ||
        block.CollisionType == 11 && block.Behavior != 0x45;

    private static string FormatCounts(IReadOnlyList<int> counts) => string.Join(
        ", ",
        counts.Select((count, direction) => (count, direction))
            .Where(entry => entry.count != 0)
            .Select(entry => $"{entry.direction}:{entry.count}"));

    /// <summary>
    /// Chooses ordinary controller input for Old Mother Brain room's five-Pirate quota.
    /// This is deterministic test-player policy, not encounter mutation: the selected slot
    /// is observed only to decide facing, aim, and whether to walk closer.
    /// </summary>
    private static ushort BuildPitEnemyQuotaInput(
        ISnesAddressSpace bus,
        SuperMetroidRuntime runtime,
        SamusState samus,
        int frame)
    {
        RoomEnemySlot? target = runtime.Enemies.Slots
            .Where(slot => slot.EnemyDefinitionPointer is not (0 or 0xdaff))
            .OrderBy(slot =>
                Math.Abs(unchecked((short)(slot.XPosition - samus.XPosition))) +
                Math.Abs(unchecked((short)(slot.YPosition - samus.YPosition))))
            .FirstOrDefault();
        if (target is null)
            return 0;

        int horizontalDistance = unchecked((short)(target.XPosition - samus.XPosition));
        bool targetIsLeft = horizontalDistance < 0;
        bool facingLeft = samus.IsFacingLeft(bus);
        SnesButton towardTarget = targetIsLeft ? SnesButton.Left : SnesButton.Right;
        if (targetIsLeft != facingLeft)
        {
            // Facing is pose-table state. Give the turn one complete frame before adding
            // an aim shoulder or Fire edge; same-frame input cannot retroactively rotate a
            // projectile which the native producer has already placed at the old muzzle.
            return (ushort)towardTarget;
        }

        int targetBottom = target.YPosition + target.YRadius;
        int approximateMuzzleY = samus.YPosition - 14;
        bool targetIsEntirelyAboveMuzzle = targetBottom < approximateMuzzleY;
        // Close horizontal distance before aiming. A diagonal shot launched from the east
        // entry can intersect the room's broken floor/ceiling scenery long before reaching
        // an otherwise valid target two screens away; walking within the visible combat
        // lane is the normal controller solution and leaves collision fully authoritative.
        bool needsTraversal = Math.Abs(horizontalDistance) > 88;
        ushort input = needsTraversal
            ? (ushort)(towardTarget | SnesButton.B)
            : targetIsEntirelyAboveMuzzle
                ? (ushort)SnesButton.R
                : (ushort)0;

        if (needsTraversal && frame % 60 < 30)
        {
            // Old Mother Brain room is divided by full standing-height ruined bulkheads.
            // A held direction alone legitimately stops at their collision columns, so use
            // the same pressed/released spin-jump cadence as a player crossing the room.
            // Bank $90 still owns launch speed and gravity and bank $94 still decides
            // whether each wall, ceiling, platform, and landing can be crossed.
            input |= (ushort)SnesButton.A;
        }

        // Eight-frame pressed/released spacing is frequent enough to retry immediately
        // after the beam cooldown, while every non-pulse frame provides a genuine new X
        // edge. The projectile allocator—not this driver—still decides whether it fires.
        if (frame % 8 == 0)
            input |= (ushort)SnesButton.X;
        return input;
    }

    /// <summary>
    /// Produces ordinary controller input for a nearby route enemy on a valid beam lane.
    /// </summary>
    /// <remarks>
    /// Event zero replaces Climb's sleepers with eleven live `$F353` actors. A
    /// horizontal-only route shot passes under wall-bound actors and lets repeated contacts
    /// kill new-game Samus. This helper does not identify a ROM enemy by coordinates or
    /// write combat state: it selects the nearest sufficiently fragile actor in the native
    /// interactive-enemy list, faces it through the pose table, chooses the configured aim
    /// shoulder, and supplies fresh Fire edges. The caller's health ceiling prevents this
    /// traversal audit from stopping to duel durable optional actors. Restricting the list is
    /// equally essential because bank $A0 excludes allocated off-screen actors from
    /// projectile collision. Collision, health, drops, and Samus damage remain runtime-owned.
    /// </remarks>
    private static ushort? BuildNearbyRouteEnemyCombatInput(
        ISnesAddressSpace bus,
        SuperMetroidRuntime runtime,
        SamusState samus,
        int frame,
        int maximumTargetHealth,
        out bool nearbyInteractiveThreat)
    {
        const int maximumHorizontalEngagementDistance = 192;
        const int maximumVerticalEngagementDistance = 160;
        const int verticalAimDeadZone = 12;
        const int straightUpLaneHalfWidth = 16;
        const int diagonalLaneTolerance = 24;

        RoomEnemySlot[] nearbyTargets = runtime.Enemies.Slots
            .Where(slot =>
                slot.EnemyDefinitionPointer is not (0 or 0xdaff) &&
                slot.Health <= maximumTargetHealth &&
                runtime.Enemies.InteractiveEnemyIndexes.Contains(slot.NativeIndex) &&
                Math.Abs(unchecked((short)(slot.XPosition - samus.XPosition))) <=
                    maximumHorizontalEngagementDistance &&
                Math.Abs(unchecked((short)(slot.YPosition - samus.YPosition))) <=
                    maximumVerticalEngagementDistance)
            .ToArray();
        nearbyInteractiveThreat = nearbyTargets.Length != 0;

        RoomEnemySlot? target = nearbyTargets
            .Where(slot =>
                IsInsideRouteBeamLane(
                    unchecked((short)(slot.XPosition - samus.XPosition)),
                    unchecked((short)(slot.YPosition - samus.YPosition)),
                    verticalAimDeadZone,
                    straightUpLaneHalfWidth,
                    diagonalLaneTolerance))
            .OrderBy(slot =>
                Math.Abs(unchecked((short)(slot.XPosition - samus.XPosition))) +
                Math.Abs(unchecked((short)(slot.YPosition - samus.YPosition))))
            .FirstOrDefault();
        if (target is null)
            return null;

        int horizontalDistance = unchecked((short)(target.XPosition - samus.XPosition));
        bool targetIsLeft = horizontalDistance < 0;
        SnesButton towardTarget = targetIsLeft ? SnesButton.Left : SnesButton.Right;
        if (targetIsLeft != samus.IsFacingLeft(bus))
        {
            // The projectile producer samples the established pose, so spend one complete
            // frame turning before asking it to allocate a beam from the old-facing muzzle.
            return (ushort)towardTarget;
        }

        int verticalDistance = unchecked((short)(target.YPosition - samus.YPosition));
        ushort input = verticalDistance < -verticalAimDeadZone &&
            Math.Abs(horizontalDistance) <= straightUpLaneHalfWidth
            ? (ushort)SnesButton.Up
            : verticalDistance < -verticalAimDeadZone
                ? (ushort)SnesButton.R
            : verticalDistance > verticalAimDeadZone
                ? (ushort)SnesButton.L
                : (ushort)0;
        if (frame % 8 == 0)
            input |= (ushort)SnesButton.X;
        return input;
    }

    private static bool IsInsideRouteBeamLane(
        int horizontalDistance,
        int verticalDistance,
        int horizontalLaneHalfHeight,
        int straightUpLaneHalfWidth,
        int diagonalLaneTolerance)
    {
        int absoluteX = Math.Abs(horizontalDistance);
        int absoluteY = Math.Abs(verticalDistance);
        if (absoluteY <= horizontalLaneHalfHeight)
            return true;
        if (verticalDistance < 0 && absoluteX <= straightUpLaneHalfWidth)
            return true;

        // The unmodified Power Beam has only horizontal, vertical-up, and 45-degree
        // diagonal controller lanes. Selecting merely by screen distance can lock onto an
        // actor underneath a solid shelf and spend every shot against the floor. Admit a
        // diagonal target only when its two deltas agree within the combined actor/beam
        // width; terrain collision still has final authority over the actual projectile.
        return Math.Abs(absoluteX - absoluteY) <= diagonalLaneTolerance;
    }

    private static ushort BuildPreMissilesReturnInput(
        ISnesAddressSpace bus,
        SamusState samus,
        int frame,
        int ascentStage,
        int framesInStage)
    {
        SamusMovementType movement = samus.ReadMovementKind(bus);
        bool morphed = movement is SamusMovementType.MorphBallGround or
            SamusMovementType.MorphBallFalling;

        if (samus.YPosition >= 0x0170)
        {
            // The return begins in the west chamber below the divider. Morph through the
            // same one-tile passage used outbound, but travel east to the only vertical
            // opening at columns $08/$09. This remains controller-only: these thresholds
            // choose buttons while native posture/collision decides whether motion occurs.
            if (samus.XPosition < 0x0080)
            {
                if (!morphed)
                    return frame % 60 < 20 ? (ushort)0 : (ushort)SnesButton.Down;
                return (ushort)SnesButton.Right;
            }

            if (morphed || movement == SamusMovementType.PostureTransition)
                return (ushort)SnesButton.Up;

            if (samus.Kinematics.YDirection == 0)
            {
                // A failed upper transfer can deposit Samus against the passage's east
                // wall at X=$9B. Running farther right there can never establish the spin
                // jump required by the shaft. Walk back to its centre before beginning a
                // fresh launch; this is recovery policy in the audit, not gameplay state.
                if (samus.XPosition > 0x0094)
                    return (ushort)SnesButton.Left;
                if (samus.XPosition < 0x0084)
                    return (ushort)SnesButton.Right;

                // A standing A press selects normal-jump pose $4D and its larger body
                // tops out one row short of this narrow shaft. Establish running with the
                // configured Run button, then add a fresh Jump edge to select spin $19.
                ushort launch = (ushort)(SnesButton.Right | SnesButton.B);
                if (movement == SamusMovementType.Running)
                    launch |= (ushort)SnesButton.A;
                return launch;
            }
        }

        if (ascentStage == 2)
        {
            // Rows $0C and $08 contain cartridge type-$C/BTS-$00 shot blocks directly
            // above the row-$10 floor. Stand under their shared X=$06..$09 chimney and
            // send two distinct upward shots through it. The first dies on row $0C; the
            // second reaches row $08 after the first block has become air.
            if (samus.Kinematics.YDirection == 0 && framesInStage < 48)
            {
                if (samus.XPosition < 0x0076)
                    return (ushort)SnesButton.Right;
                if (samus.XPosition > 0x0079)
                    return (ushort)SnesButton.Left;

                ushort clearingInput = (ushort)SnesButton.Up;
                if (framesInStage % 20 < 4)
                    clearingInput |= (ushort)SnesButton.X;
                return clearingInput;
            }

            // Once both blocks have received a beam, hold a plain Jump through their
            // temporary air cells. Keeping Up held permits another shot if the leading
            // edge happened to meet a block on its restoration frame.
            // Jump is edge-triggered. Give every retry a release interval so a collision
            // with a just-restored block cannot leave the audit holding an inert A button
            // forever. X continues to pulse during both halves and keeps the column open.
            int jumpCycle = (framesInStage - 48) % 60;
            ushort chimneyInput = (ushort)SnesButton.Up;
            if (jumpCycle >= 12)
                chimneyInput |= (ushort)SnesButton.A;
            if (framesInStage % 20 < 4)
                chimneyInput |= (ushort)SnesButton.X;
            return chimneyInput;
        }

        if (ascentStage >= 3)
        {
            // Above row $08, the restored shot blocks become the top corridor's floor.
            // Walk west and pulse Fire only near the blue cap. Door collision and the
            // transition itself remain owned by the normal projectile/PLM/door systems.
            ushort corridorInput = (ushort)SnesButton.Left;
            if (samus.XPosition <= 0x0030 && frame % 24 < 4)
                corridorInput |= (ushort)SnesButton.X;
            return corridorInput;
        }

        if (samus.YPosition >= 0x00f0)
        {
            // The shaft is intentionally taller than one jump. Rise between its two walls,
            // then cross onto the right wall's row-$15 top only after Samus' feet clear it.
            // A second spin jump from that genuine collision surface reaches the upper room.
            bool feetAboveRightPlatform =
                samus.YPosition + samus.Kinematics.YRadius < 0x0150;
            if (samus.Kinematics.YDirection == 0)
            {
                // Construction Zone alternates real landing shelves: right at row $15,
                // then left at row $12. Run away from each adjoining wall before adding
                // Jump so every transfer begins with the cartridge's spin pose.
                SnesButton platformDirection = ascentStage == 1
                    ? SnesButton.Left
                    : SnesButton.Right;
                ushort platformLaunch = (ushort)(platformDirection | SnesButton.B);
                if (movement == SamusMovementType.Running)
                    platformLaunch |= (ushort)SnesButton.A;
                return platformLaunch;
            }
            ushort input = (ushort)(SnesButton.A | SnesButton.B);
            if (ascentStage == 1)
                input |= (ushort)SnesButton.Left;
            else if (ascentStage >= 2)
                input |= (ushort)SnesButton.Right;
            else if (feetAboveRightPlatform)
                input |= (ushort)SnesButton.Right;
            else if (samus.XPosition < 0x0084)
                input |= (ushort)SnesButton.Right;
            else if (samus.XPosition > 0x0094)
                input |= (ushort)SnesButton.Left;
            return input;
        }

        // Normal progression returns west to the Morph Ball room. The red east cap leads
        // to the optional Blue Brinstar ceiling E-tank hall and must not be mistaken for
        // the route simply because the first Missile can now satisfy it.
        ushort upperInput;
        if (ascentStage == 2)
        {
            // Preserve leftward spin through the short gap between the bridge and the
            // west door floor. Landing there advances the audit to stage 3 above.
            upperInput = (ushort)(SnesButton.Left | SnesButton.B | SnesButton.A);
        }
        else if (samus.Kinematics.YDirection != 0)
        {
            // Land on the restored row-$10 shot-block bridge before walking west. Holding
            // Left throughout the second arc carries Samus past its block-$06 edge while
            // still airborne and drops her all the way back into the shaft.
            upperInput = samus.Kinematics.YDirection == 2
                ? samus.XPosition > 0x0080
                    ? (ushort)SnesButton.Left
                    : (ushort)SnesButton.Right
                : samus.XPosition > 0x00a0
                    ? (ushort)SnesButton.Left
                    : (ushort)SnesButton.Right;
            if (samus.Kinematics.YDirection == 1)
                upperInput |= (ushort)SnesButton.A;
        }
        else
        {
            upperInput = (ushort)SnesButton.Left;
        }
        if (samus.XPosition <= 0x0030 && frame % 24 == 0)
            upperInput |= (ushort)SnesButton.X;
        return upperInput;
    }

    private static string FormatSpawn(SamusProjectileSpawnSnapshot? snapshot) => snapshot is { } spawn
        ? $"d{new SamusProjectileDirectionWord(spawn.Direction).DirectionIndex}@" +
          $"(${spawn.XPosition:X4},${spawn.YPosition:X4})/" +
          $"v({spawn.XVelocity:X4},{spawn.YVelocity:X4})"
        : "-";

    private static void AssertPendingDoor(
        SuperMetroidRuntime runtime,
        ushort expectedDoorPointer,
        ushort expectedRoomPointer,
        string segmentName)
    {
        CartridgeDoorHeader door = runtime.LevelData?.PendingDoorTransition ??
            throw new InvalidDataException($"{segmentName} ended without a published door.");
        if (door.Pointer != expectedDoorPointer ||
            door.DestinationRoomPointer != expectedRoomPointer)
        {
            throw new InvalidDataException(
                $"{segmentName} reached $83:{door.Pointer:X4} -> " +
                $"$8F:{door.DestinationRoomPointer:X4}; expected " +
                $"$83:{expectedDoorPointer:X4} -> $8F:{expectedRoomPointer:X4}.");
        }
    }

    private static void AssertRoom(
        SuperMetroidRuntime runtime,
        ushort expectedRoomPointer,
        ushort expectedStatePointer,
        string roomName)
    {
        CartridgeRoomHeader room = runtime.ActiveRoom ?? throw new InvalidOperationException(
            $"{roomName} did not publish an active room.");
        if (room.Pointer != expectedRoomPointer || room.State.Pointer != expectedStatePointer)
        {
            throw new InvalidDataException(
                $"{roomName} loaded $8F:{room.Pointer:X4}/$8F:{room.State.Pointer:X4}; " +
                $"expected $8F:{expectedRoomPointer:X4}/$8F:{expectedStatePointer:X4}.");
        }
    }

    private static void AssertSuitPaletteReloaded(
        ISnesAddressSpace bus,
        SuperMetroidRuntime runtime,
        SamusState samus)
    {
        var expected = new SnesCgram();
        samus.LoadSuitPalette(bus, expected);
        for (int color = 192; color < 208; color++)
        {
            ushort actualColor = runtime.Cgram.Colors[color];
            ushort expectedColor = expected.Colors[color];
            if (actualColor != expectedColor)
            {
                throw new InvalidDataException(
                    $"First door left suit color {color} at ${actualColor:X4}; " +
                    $"Samus_LoadSuitTargetPalette requires ${expectedColor:X4}.");
            }
        }
    }

    private static void PrintCollisionNeighborhood(
        SuperMetroidRuntime runtime,
        SamusState samus)
    {
        RoomLevelData level = runtime.LevelData ?? throw new InvalidOperationException(
            "Controller route lost its level before the failure diagnostic.");
        int centerBlockX = samus.XPosition >> 4;
        int centerBlockY = samus.YPosition >> 4;
        Console.WriteLine(
            $"  Samus collision neighborhood (center block {centerBlockX:X2},{centerBlockY:X2}; " +
            $"radii {samus.Kinematics.XRadius},{samus.Kinematics.YRadius}):");

        // Print the exact packed collision metadata around the body. A door cap is a level
        // mutation owned by a PLM, whereas the edge trigger behind it is type $9. Showing
        // both nibbles here keeps the next failure tied to cartridge data rather than a
        // guess based on the composite screenshot.
        for (int y = Math.Max(0, centerBlockY - 3);
             y <= Math.Min(level.HeightInBlocks - 1, centerBlockY + 3);
             y++)
        {
            var row = new List<string>();
            for (int x = Math.Max(0, centerBlockX - 2);
                 x <= Math.Min(level.WidthInBlocks - 1, centerBlockX + 2);
                 x++)
            {
                RoomCollisionBlock block = level.GetCollisionBlock(x, y);
                row.Add($"{x:X2}:{block.CollisionType:X1}/{block.Behavior:X2}");
            }
            Console.WriteLine($"    y={y:X2} {string.Join(' ', row)}");
        }
    }

    private static void PrintCollisionRegion(
        SuperMetroidRuntime runtime,
        int left,
        int right,
        int top,
        int bottom)
    {
        if (!VerboseDiagnostics)
            return;
        RoomLevelData level = runtime.LevelData ?? throw new InvalidOperationException(
            "Collision-region diagnostic requires active room data.");
        Console.WriteLine(
            $"  Collision types X={left:X2}..{right:X2}, Y={top:X2}..{bottom:X2}:");
        for (int y = top; y <= bottom; y++)
        {
            var types = new char[right - left + 1];
            for (int x = left; x <= right; x++)
                types[x - left] = "0123456789ABCDEF"[level.GetCollisionBlock(x, y).CollisionType];
            Console.WriteLine($"    {y:X2}: {new string(types)}");
        }
    }

    private static void PrintCollisionFeatureRows(
        SuperMetroidRuntime runtime,
        int left,
        int right,
        int top,
        int bottom)
    {
        if (!VerboseDiagnostics)
            return;
        RoomLevelData level = runtime.LevelData ?? throw new InvalidOperationException(
            "Collision-feature diagnostic requires active room data.");
        Console.WriteLine(
            $"  Collision features X={left:X2}..{right:X2}, Y={top:X2}..{bottom:X2}:");
        for (int y = top; y <= bottom; y++)
        {
            var features = new List<string>();
            for (int x = left; x <= right; x++)
            {
                RoomCollisionBlock block = level.GetCollisionBlock(x, y);
                if (block.CollisionType != 0)
                    features.Add($"{x:X2}:{block.CollisionType:X1}/{block.Behavior:X2}");
            }
            if (features.Count != 0)
                Console.WriteLine($"    {y:X2}: {string.Join(' ', features)}");
        }
    }

    private static void WriteCollisionMap(
        SuperMetroidRuntime runtime,
        SamusState samus,
        string roomName)
    {
        RoomLevelData level = runtime.LevelData ?? throw new InvalidOperationException(
            "Controller route lost its level before the collision-map diagnostic.");
        var pixels = new Rgba32[level.WidthInBlocks * level.HeightInBlocks];
        Rgba32[] collisionColors =
        [
            new(8, 8, 12),       // $0: air
            new(150, 150, 150),  // $1: slopes
            new(70, 100, 180),   // $2: air spikes
            new(100, 65, 45),    // $3: crumble
            new(90, 45, 130),    // $4: shootable
            new(35, 110, 130),   // $5: horizontal extension
            new(80, 60, 120),    // $6: unused/room-specific
            new(110, 80, 45),    // $7: bombable
            new(235, 235, 235),  // $8: solid
            new(0, 220, 255),    // $9: door trigger
            new(220, 80, 70),    // $A: spike
            new(160, 100, 30),   // $B: special collision
            new(40, 170, 80),    // $C: shootable BTS/door cap
            new(40, 130, 100),   // $D: vertical extension
            new(190, 70, 190),   // $E: grapple
            new(230, 170, 45),   // $F: bomb block
        ];

        for (int y = 0; y < level.HeightInBlocks; y++)
        {
            for (int x = 0; x < level.WidthInBlocks; x++)
            {
                RoomCollisionBlock block = level.GetCollisionBlock(x, y);
                pixels[y * level.WidthInBlocks + x] = collisionColors[block.CollisionType];
            }
        }

        // Magenta is reserved for Samus so the last autonomous position remains obvious
        // even when she overlaps a slope or door family with its own diagnostic color.
        int samusX = Math.Clamp(samus.XPosition >> 4, 0, level.WidthInBlocks - 1);
        int samusY = Math.Clamp(samus.YPosition >> 4, 0, level.HeightInBlocks - 1);
        pixels[samusY * level.WidthInBlocks + samusX] = new Rgba32(255, 0, 255);

        string safeName = roomName.Replace(' ', '-');
        string path = Path.Combine(
            "csharp", "test-temp", "early-controller", $"{safeName}-collision.png");
        PngWriter.WriteRgba(
            path,
            level.WidthInBlocks,
            level.HeightInBlocks,
            pixels,
            scale: 8);
        Console.WriteLine($"  Wrote collision map to {Path.GetFullPath(path)}.");
    }

    private static void PrintDoorBlocks(
        ISnesAddressSpace bus,
        SuperMetroidRuntime runtime,
        string roomName)
    {
        if (!VerboseDiagnostics)
            return;
        RoomLevelData level = runtime.LevelData ?? throw new InvalidOperationException(
            $"{roomName} has no level data.");
        var doors = new List<string>();
        for (int y = 0; y < level.HeightInBlocks; y++)
        {
            for (int x = 0; x < level.WidthInBlocks; x++)
            {
                RoomCollisionBlock block = level.GetCollisionBlock(x, y);
                if (block.CollisionType == 9)
                {
                    CartridgeDoorHeader definition = level.ResolveDoorCollision(
                        bus,
                        block.Behavior,
                        runtime.Samus?.Pose ?? 0,
                        publishDoorSideEffects: false);
                    doors.Add(
                        $"({x:X2},{y:X2})=${block.Behavior:X2}" +
                        $"->$83:{definition.Pointer:X4}/$8F:{definition.DestinationRoomPointer:X4}");
                }
            }
        }
        Console.WriteLine($"  {roomName} door blocks: {string.Join(' ', doors)}");
    }

    /// <summary>
    /// Proves that the Blue Brinstar elevator is not merely present in cartridge enemy
    /// state or isolated OBJ output: at least one opaque pixel from its retail sprite must
    /// survive the same BG/OBJ priority compositor used by the desktop frontend.
    /// </summary>
    /// <remarks>
    /// The elevator is enemy definition $D73F. Its two live spritemaps use character names
    /// $06C-$06E, OBJ palette 5, and priority 2. Restricting the scan to those finalized OAM
    /// records prevents Samus or an unrelated enemy with a coincidentally similar palette
    /// color from satisfying the assertion. A black platform caused by missing OBJ tiles,
    /// a missing palette, an incorrect priority rank, or a stale displayed-OAM page will
    /// therefore fail at the production-render boundary instead of passing a state-only
    /// test while remaining visibly broken.
    /// </remarks>
    private static void AssertElevatorPlatformSurvivesGameplayCompositor(
        SuperMetroidRuntime runtime)
    {
        RoomEnemySlot elevator = runtime.Enemies.Slots.FirstOrDefault(slot =>
            slot.EnemyDefinitionPointer == RoomEnemySystem.ElevatorDefinition)
            ?? throw new InvalidDataException(
                "Blue Brinstar elevator room reached the visual audit without enemy $D73F.");
        if (elevator.SpritemapPointer is not 0x962f and not 0x9645)
        {
            throw new InvalidDataException(
                $"Elevator $D73F selected non-retail spritemap " +
                $"$A2:{elevator.SpritemapPointer:X4}.");
        }

        ResolvedObjFrame objects = SnesObjRenderer.RenderResolved(
            runtime.DisplayedOam,
            runtime.Vram,
            runtime.Cgram,
            obsel: 0x03);
        Rgba32[] composed = SuperMetroidRuntimeFrameRenderer.Render(runtime);
        int opaqueElevatorPixels = 0;
        int coloredElevatorPixels = 0;
        int visibleElevatorPixels = 0;
        int visibleColoredElevatorPixels = 0;

        for (int spriteIndex = 0;
            spriteIndex < runtime.DisplayedOam.LastFinalizedSpriteCount;
            spriteIndex++)
        {
            OamEntry entry = runtime.DisplayedOam.GetEntry(spriteIndex);
            if (entry.TileNumber is < 0x006c or > 0x006e ||
                entry.Palette != 5 ||
                entry.Priority != 2 ||
                entry.IsLarge)
            {
                continue;
            }

            // Gameplay OBSEL $03 selects the 8x8/16x16 size mode. These elevator pieces
            // are explicitly small, so each finalized OAM record contributes one 8x8
            // character. Convert the modular SNES coordinates exactly as the OBJ renderer
            // does before clipping to the 256x224 visible field.
            int objectX = entry.X >= 0x100 ? entry.X - 0x200 : entry.X;
            int objectY = entry.Y >= 0xe0 ? entry.Y - 0x100 : entry.Y;
            for (int localY = 0; localY < 8; localY++)
            {
                int screenY = objectY + localY;
                if ((uint)screenY >= 224)
                    continue;
                for (int localX = 0; localX < 8; localX++)
                {
                    int screenX = objectX + localX;
                    if ((uint)screenX >= 256)
                        continue;
                    int pixelIndex = screenY * 256 + screenX;
                    if (objects.Priorities[pixelIndex] != 2 ||
                        objects.Pixels[pixelIndex].A == 0)
                    {
                        continue;
                    }

                    opaqueElevatorPixels++;
                    bool isColored = objects.Pixels[pixelIndex].R != 0 ||
                        objects.Pixels[pixelIndex].G != 0 ||
                        objects.Pixels[pixelIndex].B != 0;
                    if (isColored)
                        coloredElevatorPixels++;
                    if (composed[pixelIndex] == objects.Pixels[pixelIndex])
                    {
                        visibleElevatorPixels++;
                        if (isColored)
                            visibleColoredElevatorPixels++;
                    }
                }
            }
        }

        if (opaqueElevatorPixels == 0)
        {
            throw new InvalidDataException(
                "Elevator $D73F produced no opaque $06C-$06E pixels in finalized OAM.");
        }
        if (coloredElevatorPixels == 0)
        {
            throw new InvalidDataException(
                $"All {opaqueElevatorPixels} opaque elevator pixels resolved to RGB black; " +
                "the platform OBJ palette or character data is not visibly initialized.");
        }
        if (visibleElevatorPixels == 0)
        {
            throw new InvalidDataException(
                $"All {opaqueElevatorPixels} opaque elevator pixels were lost behind the " +
                "ordinary gameplay compositor; the platform would appear black/invisible.");
        }
        if (visibleColoredElevatorPixels == 0)
        {
            throw new InvalidDataException(
                $"All {coloredElevatorPixels} colored elevator pixels were hidden by the " +
                "ordinary gameplay compositor; only black platform pixels remain visible.");
        }
    }

    /// <summary>
    /// Proves that the door-selected bank-$83 FX record reached the NMI-visible snapshot,
    /// uploaded nonempty BG3 art, and changes actual production pixels below the HUD.
    /// </summary>
    private static void AssertRoomLayer3FxIsVisible(
        SuperMetroidRuntime runtime,
        RoomFxType expectedType,
        string description)
    {
        RoomLayer3FxRenderSnapshot fx = runtime.DisplayedRoomLayer3Fx
            ?? throw new InvalidDataException($"{description} has no displayed BG3 FX snapshot.");
        if (fx.Type != expectedType)
        {
            throw new InvalidDataException(
                $"{description} selected FX ${(byte)fx.Type:X2}, expected ${(byte)expectedType:X2}.");
        }

        var overlayOnly = new Rgba32[FrontendFrame.Width * FrontendFrame.Height];
        SnesGameplayFrameRenderer.ApplyRoomLayer3FxColorMath(
            overlayOnly,
            runtime.Vram,
            runtime.Cgram,
            fx);
        int coloredPixels = overlayOnly.Count(pixel => pixel.R != 0 || pixel.G != 0 || pixel.B != 0);
        if (coloredPixels == 0)
        {
            throw new InvalidDataException(
                $"{description} reached the compositor but produced no colored gameplay pixels.");
        }
    }

    /// <summary>
    /// Verifies the cartridge's globally shared bank-$86 OBJ palette after a real door
    /// transition. Enemy death effects, drops, Pirate lasers, Ceres timer/steam, and the
    /// Ceres elevator all consume this same row, so a stale black row is a systemic fault.
    /// </summary>
    internal static void AssertCommonEnemyProjectilePalette(
        ISnesAddressSpace bus,
        SuperMetroidRuntime runtime,
        string description)
    {
        const int sourceAddress = 0x9a81a0;
        const int destinationColor = 208;
        int nonBlackColors = 0;
        for (int color = 0; color < 16; color++)
        {
            int address = sourceAddress + color * 2;
            ushort expected = unchecked((ushort)(
                bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
            ushort actual = runtime.Cgram.Colors[destinationColor + color];
            if (actual != expected)
            {
                throw new InvalidDataException(
                    $"{description} common OBJ palette color {color} is ${actual:X4}; " +
                    $"cartridge $9A:{0x81a0 + color * 2:X4} is ${expected:X4}.");
            }
            if ((actual & 0x7fff) != 0)
                nonBlackColors++;
        }
        if (nonBlackColors == 0)
            throw new InvalidDataException($"{description} common OBJ palette is entirely black.");
    }

    private static void AssertSamusWithinDisplayedCamera(
        SuperMetroidRuntime runtime,
        string description)
    {
        SamusState samus = runtime.Samus ?? throw new InvalidDataException(
            $"{description} has no Samus state.");
        ScrollBoundaryCamera camera = runtime.Camera ?? throw new InvalidDataException(
            $"{description} has no camera state.");
        int screenX = unchecked((short)(samus.XPosition - camera.XPosition));
        int screenY = unchecked((short)(samus.YPosition - camera.YPosition));
        if (screenX is < -32 or > 288 || screenY is < -32 or > 256)
        {
            throw new InvalidDataException(
                $"{description} placed Samus outside the displayed viewport: " +
                $"Samus=(${samus.XPosition:X4},${samus.YPosition:X4}), " +
                $"camera=(${camera.XPosition:X4},${camera.YPosition:X4}), " +
                $"screen=({screenX},{screenY}).");
        }
    }

    /// <summary>
    /// Verifies the production camera, BG1 world position, and PPU scroll mirror after the
    /// first upward elevator journey. This is deliberately checked after the platform has
    /// landed and ordinary control has opened the Pit door, not at an invented intermediate
    /// coordinate where the cartridge is still applying green-scroll alignment.
    /// </summary>
    private static void AssertAscendingElevatorCameraIsSynchronized(
        SuperMetroidRuntime runtime)
    {
        ScrollBoundaryCamera camera = runtime.Camera ?? throw new InvalidDataException(
            "Ascending elevator completed without an active camera.");
        SamusState samus = runtime.Samus ?? throw new InvalidDataException(
            "Ascending elevator completed without Samus.");
        CartridgeRoomHeader room = runtime.ActiveRoom ?? throw new InvalidDataException(
            "Ascending elevator completed without its room header.");

        // Front-facing elevator setup clears Y direction to zero. `$90:964F` therefore
        // selects the room's up-scroller byte, and the stationary frames after landing let
        // camera Y converge exactly to SamusY-upScroller. For room `$97B5`, that legitimate
        // native endpoint is `$008B-$0070=$001B`; forcing zero would itself be a host-only
        // “one row fix” and would disagree with the cartridge's green-scroll alignment.
        ushort expectedCameraY = unchecked((ushort)(samus.YPosition - room.UpScroller));
        if (camera.YPosition != expectedCameraY ||
            camera.IdealYPosition != expectedCameraY)
        {
            throw new InvalidDataException(
                $"Ascending elevator camera did not converge to the cartridge target: " +
                $"SamusY=${samus.YPosition:X4}, up-scroller=${room.UpScroller:X2}, " +
                $"camera=${camera.YPosition:X4}, ideal=${camera.IdealYPosition:X4}, " +
                $"expected=${expectedCameraY:X4}.");
        }
        if (runtime.BackgroundScroll.Layer1YPosition != camera.YPosition ||
            runtime.BackgroundScroll.Bg1VerticalScroll != unchecked((ushort)(
                camera.YPosition + runtime.BackgroundScroll.Bg1YOffset)))
        {
            throw new InvalidDataException(
                $"Ascending elevator split camera/BG1 state: camera=${camera.YPosition:X4}, " +
                $"layer1=${runtime.BackgroundScroll.Layer1YPosition:X4}, " +
                $"BG1VOFS=${runtime.BackgroundScroll.Bg1VerticalScroll:X4}, " +
                $"offset=${runtime.BackgroundScroll.Bg1YOffset:X4}.");
        }
    }

    private static void PrintPlmPopulation(ISnesAddressSpace bus, ushort populationPointer)
    {
        if (!VerboseDiagnostics)
            return;
        var records = new List<string>();
        ushort cursor = populationPointer;
        for (int index = 0; index < 64; index++, cursor = unchecked((ushort)(cursor + 6)))
        {
            int address = 0x8f0000 | cursor;
            ushort header = (ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8));
            if (header == 0)
                break;
            byte x = bus.ReadByte(address + 2);
            byte y = bus.ReadByte(address + 3);
            ushort argument = (ushort)(bus.ReadByte(address + 4) | (bus.ReadByte(address + 5) << 8));
            records.Add($"${header:X4}@({x:X2},{y:X2})/${argument:X4}");
        }
        Console.WriteLine($"  PLM population $8F:{populationPointer:X4}: {string.Join(' ', records)}");
    }

    /// <summary>
    /// Supplies the two scheduler operations that differ between the direct runtime audit
    /// and a production-frontend playthrough. Route code may inspect cartridge-owned state
    /// to choose buttons, but all mutation must enter through these controller/door calls.
    /// </summary>
    internal sealed record ControllerRouteHost(
        Action<ushort> StepFrame,
        Action LoadPendingDoor,
        SuperMetroidGame? Frontend = null);

    private readonly record struct DriveResult(
        int Frames,
        int FiredShots,
        int CollisionExplosions);

    private readonly record struct BombTorizoAwakeningResult(
        int MessageFrames,
        int SequenceFrames);

    private readonly record struct BombTorizoFightResult(
        int Frames,
        int FireInputs,
        int DamagingHits);

    private readonly record struct ClimbPlatformTarget(
        int LandingX,
        int SurfaceY,
        int ShadowLeftX,
        int ShadowRightX,
        bool IsDoorApproach = false,
        bool IsSolidLedge = false);

    private readonly record struct ClimbLandingSample(
        int X,
        int SurfaceY);
}
