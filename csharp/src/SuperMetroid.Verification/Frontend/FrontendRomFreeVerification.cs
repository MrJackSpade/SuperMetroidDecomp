using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Assets;
using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Bosses.Torizo.GoldenTorizoRoomSetupVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Enemies.BossBodies;
namespace SuperMetroid.Verification.Frontend;

using SuperMetroid.Verification.Bosses.Torizo;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Progression;

/// <summary>The installed game bindings, the paired-room run and the cartridge read guard other checks share.</summary>
internal static class FrontendRomFreeVerification
{
    /// <summary>
    /// Binds the installation's catalogs, the ones a host binds, to a new or restored game, as a host binds every game it
    /// runs. <paramref name="enemyArtwork"/> replaces the
    /// installed enemy, boss and area artwork; when the action's second argument is false, the intro cinematic artwork is
    /// left as the game has it.
    /// </summary>
    internal static Action<SuperMetroidGame, bool> PrepareRomFreeBindings(GameInstallation installation,
        InstalledEnemyArtwork? enemyArtwork = null)
    {
        GameArtwork artwork = InstalledGameBindings.Artwork(installation);
        if (enemyArtwork is not null)
            artwork = artwork with
            {
                EnemyTileArtwork = enemyArtwork.Enemies,
                BossArtwork = enemyArtwork.Bosses,
                
            };
        // An already-finished intro is deliberately not rebound for the isolated gameplay-room census: its restored DMA
        // side effect would test a retired cinematic, not the room under examination.
        return (game, bindIntro) => game.BindArtwork(bindIntro
            ? artwork
            : artwork with { IntroCinematicArt = game.Artwork.IntroCinematicArt });
    }

    /// <summary>
    /// Whether frame <paramref name="frame"/> of a lockstep run renders both games and compares their pixels: every
    /// frame on which the frontend state or phase <paramref name="changed"/>, and every thirty-seventh frame between.
    /// Each render is a full software Direct3D frame and readback, and game state and phase are compared on every frame.
    /// </summary>
    private static bool ComparesPixels(int frame, bool changed) => changed || frame % 37 == 0;

    /// <summary>Whether the frontend's game state or phase changed from <paramref name="before"/> to <paramref name="after"/>.</summary>
    private static bool Changed(FrontendFrame before, FrontendFrame after) =>
        before.GameState != after.GameState || before.Phase != after.Phase;

    /// <summary>
    /// Compare representative directly loaded room states without crossing a door.
    /// This covers Ceres and ordinary enemy composition, scrolling skies,
    /// elevators, save rooms, Norfair FX, and Phantoon's BG2 frames. It tests room
    /// initialization and neutral-frame presentation, not incoming doors or travel.
    /// </summary>
    internal static void VerifyFrontendRomFreeRoom(SuperMetroidGame native, SuperMetroidGame installed, RomFreeRoom room, int frameCount = 90, Action<SuperMetroidGame, SuperMetroidGame>? setup = null, GoldenTorizoForcing golden = default, ControllerButton heldInput = ControllerButton.None)
    {
        native.GameplayRuntimeForVerification.LoadCartridgeRoomForDebug(
            room.Room);
        installed.GameplayRuntimeForVerification.LoadCartridgeRoomForDebug(
            room.Room);
        AssertEqual(SuperMetroidGameState.MainGameplay, native.GameState,
            $"native {room.RoomName} fixture begins in active gameplay");
        AssertEqual(SuperMetroidGameState.MainGameplay, installed.GameState,
            $"installed {room.RoomName} fixture begins in active gameplay");
        // These isolated rooms are not one continuous playthrough. Refill on
        // entry so damage in an earlier room cannot turn a later comparison
        // into two identical death screens.
        native.GameplayRuntimeForVerification.Player.IntroducedSamus.MaxHealth = 1499;
        native.GameplayRuntimeForVerification.Player.IntroducedSamus.Health = 1499;
        installed.GameplayRuntimeForVerification.Player.IntroducedSamus.MaxHealth = 1499;
        installed.GameplayRuntimeForVerification.Player.IntroducedSamus.Health = 1499;
        setup?.Invoke(native, installed);
        RoomEnemySlot? awakenedGoldenTorizo = room.Room == RoomHeaderDefinitions.GoldenTorizo &&
            setup is not null
            ? installed.GameplayRuntimeForVerification.Enemies.SlotTable.Slots.FirstOrDefault(slot =>
                slot.DefinitionId == EnemyDefinitionId.GoldenTorizo)
            : null;
        if (room.Room == RoomHeaderDefinitions.GoldenTorizo && setup is not null)
        {
            AssertTrue(awakenedGoldenTorizo is not null,
                "Golden Torizo wake-up fixture loaded its live boss slot");
        }
        bool goldenWakeObserved = false;
        bool goldenWalkingObserved = false;
        bool goldenRightwardObserved = false;
        bool goldenJumpBackObserved = false;
        bool goldenRightOrbObserved = false;
        bool goldenEyeBeamAttackObserved = false;
        bool forcedGoldenSonicObserved = false;
        bool forcedGoldenStunObserved = false;
        bool forcedGoldenStunReleased = false;
        bool forcedGoldenLeftTurnObserved = false;
        bool forcedGoldenFallingLeftObserved = false;
        bool goldenLeftFootOrbProgramObserved = false;
        ReadOnlySpan<byte> nativeLoadedVram = native.GameplayRuntimeForVerification.Vram.Bytes;
        ReadOnlySpan<byte> installedLoadedVram = installed.GameplayRuntimeForVerification.Vram.Bytes;
        int firstLoadVram = 0;
        while (firstLoadVram < nativeLoadedVram.Length &&
               nativeLoadedVram[firstLoadVram] == installedLoadedVram[firstLoadVram])
            firstLoadVram++;
        for (int frame = 0; frame < frameCount; frame++)
        {
            FrontendFrame before = installed.CurrentFrame;
            FrontendFrame expected = native.Step(heldInput);
            FrontendFrame actual;
            try
            {
                actual = installed.Step(heldInput);
            }
            catch (Exception error)
            {
                throw new InvalidOperationException(
                    $"Installed {room.RoomName} failed at neutral frame {frame}.", error);
            }
            bool comparesPixels = ComparesPixels(frame, Changed(before, actual));
            RgbaPixel[] expectedPixels = comparesPixels ? native.PublishedPixels() : [];
            RgbaPixel[] actualPixels = comparesPixels ? installed.PublishedPixels() : [];
            AssertEqual(room.Room,
                installed.GameplayActiveRoom,
                $"installed {room.RoomName} fixture remains in one room at frame {frame}");
            AssertEqual(expected.GameState, actual.GameState,
                $"installed {room.RoomName} game state at frame {frame}");
            AssertEqual(expected.Phase, actual.Phase,
                $"installed {room.RoomName} phase at frame {frame}");
            TorizoBodyProgram? goldenProgram = awakenedGoldenTorizo is null ? null
                : (installed.GameplayRuntimeForVerification.Enemies.BombTorizoBehavior.Attacks.GoldenTorizo
                    ?? throw new InvalidOperationException("Verification requires the Golden Torizo state.")).BodyProgram.Program;
            bool GoldenRuns(TorizoBodyProgram first, TorizoBodyProgram last) =>
                goldenProgram is { } running && running >= first && running <= last;
            if (goldenProgram is { } awake && awake != TorizoBodyProgram.GoldenInitial0)
                goldenWakeObserved = true;
            if (GoldenRuns(TorizoBodyProgram.GoldenWalkingLeftRightLegMoving, TorizoBodyProgram.GoldenWalkingLeftLeftLegMoving))
                goldenWalkingObserved = true;
            if (GoldenRuns(TorizoBodyProgram.GoldenDodgeTurningRight, TorizoBodyProgram.GoldenWalkingRightRightLegMoving))
                goldenRightwardObserved = true;
            if (GoldenRuns(TorizoBodyProgram.FacingRightJumpBackwardLandRightFootFwd0, TorizoBodyProgram.FacingRightJumpBackwardsLandLeftFootFwd4))
                goldenJumpBackObserved = true;
            if (GoldenRuns(TorizoBodyProgram.GoldenSpewChozoOrbFacingLeftRightFootFwd0, TorizoBodyProgram.GoldenSpewChozoOrbFacingLeftRightFootFwd1))
                goldenRightOrbObserved = true;
            if (GoldenRuns(TorizoBodyProgram.GoldenEyeBeamAttack0, TorizoBodyProgram.GoldenEyeBeamAttack2))
                goldenEyeBeamAttackObserved = true;
            // Each forced sonic attack is its entry program and the loop that follows it.
            if (golden.ForcedGoldenSonicStart is { } sonicStart && GoldenRuns(sonicStart, sonicStart + 1))
                forcedGoldenSonicObserved = true;
            if (golden.ForcedGoldenStun && GoldenRuns(TorizoBodyProgram.Stunned0, TorizoBodyProgram.Stunned1))
                forcedGoldenStunObserved = true;
            // The stun's ClearAnimationLock and its return path's UnmarkStunned release the
            // boss; later attacks may lock again, so observe the release, not the last frame.
            else if (forcedGoldenStunObserved && !forcedGoldenStunReleased &&
                     (installed.GameplayRuntimeForVerification.Enemies.BombTorizoBehavior.Attacks.GoldenTorizo ?? throw new InvalidOperationException("Verification requires installed.GameplayRuntimeForVerification.Enemies.BombTorizoBehavior.GoldenTorizo.")).AnimationLocked == false &&
                     (installed.GameplayRuntimeForVerification.Enemies.BombTorizoBehavior.Attacks.GoldenTorizo ?? throw new InvalidOperationException("Verification requires installed.GameplayRuntimeForVerification.Enemies.BombTorizoBehavior.GoldenTorizo.")).Behavior.HasFlag(TorizoBehaviorFlags.Stunned) == false)
                forcedGoldenStunReleased = true;
            if (awakenedGoldenTorizo is not null &&
                golden.ForcedGoldenLeftTurnStart is not null &&
                awakenedGoldenTorizo.SpritemapPointer == GoldenTorizoFacingScreenFrame)
                forcedGoldenLeftTurnObserved = true;
            if (awakenedGoldenTorizo is not null &&
                golden.ForcedGoldenLeftTurnStart == TorizoBodyProgram.GoldenDodgeTurningLeft &&
                awakenedGoldenTorizo.SpritemapPointer ==
                    TorizoAnimationsConstants.Spritemaps(TorizoBodyProgram.FacingLeftFalling1).Single())
                forcedGoldenFallingLeftObserved = true;
            if (GoldenRuns(TorizoBodyProgram.GoldenSpewChozoOrbFacingLeftLeftFootFwd0, TorizoBodyProgram.GoldenSpewChozoOrbFacingLeftLeftFootFwd1))
                goldenLeftFootOrbProgramObserved = true;
            if (comparesPixels && !actualPixels.SequenceEqual(expectedPixels))
            {
                int first = -1;
                int count = 0;
                int minX = FrontendFrame.Width;
                int minY = FrontendFrame.Height;
                int maxX = -1;
                int maxY = -1;
                for (int pixel = 0; pixel < actualPixels.Length; pixel++)
                {
                    if (actualPixels[pixel].Equals(expectedPixels[pixel]))
                        continue;
                    if (first < 0) first = pixel;
                    count++;
                    int x = pixel % FrontendFrame.Width;
                    int y = pixel / FrontendFrame.Width;
                    minX = Math.Min(minX, x);
                    minY = Math.Min(minY, y);
                    maxX = Math.Max(maxX, x);
                    maxY = Math.Max(maxY, y);
                }
                ReadOnlySpan<byte> nativeVram = native.GameplayRuntimeForVerification.Vram.Bytes;
                ReadOnlySpan<byte> installedVram = installed.GameplayRuntimeForVerification.Vram.Bytes;
                int firstVram = 0;
                while (firstVram < nativeVram.Length &&
                       nativeVram[firstVram] == installedVram[firstVram])
                    firstVram++;
                var changedVramPages = new List<string>();
                for (int page = 0; page < nativeVram.Length; page += 0x1000)
                {
                    int changed = 0;
                    for (int offset = page; offset < page + 0x1000; offset++)
                        if (nativeVram[offset] != installedVram[offset]) changed++;
                    if (changed != 0) changedVramPages.Add($"${page:X4}:{changed}");
                }
                throw new InvalidOperationException(
                    $"Installed {room.RoomName} pixels differ at frame {frame}: " +
                    $"{count} pixels, bounds ({minX},{minY})..({maxX},{maxY}), " +
                    $"first ({first % FrontendFrame.Width},{first / FrontendFrame.Width}) " +
                    $"native={expectedPixels[first]}, installed={actualPixels[first]}; " +
                    $"first loaded VRAM difference byte ${firstLoadVram:X4}, " +
                    $"first displayed VRAM difference byte ${firstVram:X4}; " +
                    $"VRAM pages=[{string.Join(", ", changedVramPages)}].");
            }
            if (room.Room == RoomHeaderDefinitions.Draygon && frame == 0)
            {
                // Draygon's opening readies the Evirs from the installed tiles in both runtimes (#1513).
                AssertEqual(native.GameplayRuntimeForVerification.EnemySession.ArtworkProgress.StepOf(SuperMetroid.Core.Assets.EncounterArtworkSequence.DraygonEvirs),
                    installed.GameplayRuntimeForVerification.EnemySession.ArtworkProgress.StepOf(SuperMetroid.Core.Assets.EncounterArtworkSequence.DraygonEvirs),
                    "installed Draygon's opening reaches the native step with bank-$B1 reads forbidden");
            }
        }
        if (awakenedGoldenTorizo is not null)
        {
            if (golden.ForcedGoldenSonicStart is not null)
            {
                AssertTrue(forcedGoldenSonicObserved &&
                           installed.GameState == SuperMetroidGameState.MainGameplay,
                    "forced Golden Torizo fixture executes the sonic program in active gameplay");
                Console.WriteLine($"Frontend {room.RoomName} room: {frameCount} native-parity frames; all cartridge reads guarded.");
                return;
            }
            if (golden.ForcedGoldenStun)
            {
                AssertTrue(forcedGoldenStunObserved, "forced Golden Torizo stun loops its tiles");
                AssertTrue(forcedGoldenStunReleased, "forced Golden Torizo stun then clears its shot lock and stun flag");
                AssertEqual(SuperMetroidGameState.MainGameplay, installed.GameState,
                    "forced Golden Torizo stun ends in active gameplay");
                Console.WriteLine($"Frontend {room.RoomName} room: {frameCount} native-parity frames; all cartridge reads guarded.");
                return;
            }
            if (golden.ForcedGoldenLeftFootOrb)
            {
                AssertTrue(goldenLeftFootOrbProgramObserved, "forced Golden Torizo runs its left-foot Chozo-orb program");
                AssertEqual(SuperMetroidGameState.MainGameplay, installed.GameState,
                    "forced Golden Torizo Chozo orb ends in active gameplay");
                Console.WriteLine($"Frontend {room.RoomName} room: {frameCount} native-parity frames; all cartridge reads guarded.");
                return;
            }
            if (golden.ForcedGoldenLeftTurnStart is not null)
            {
                AssertTrue(forcedGoldenLeftTurnObserved &&
                           (golden.ForcedGoldenLeftTurnStart != TorizoBodyProgram.GoldenDodgeTurningLeft ||
                            forcedGoldenFallingLeftObserved) &&
                           installed.GameState == SuperMetroidGameState.MainGameplay,
                    "forced Golden Torizo left turn reaches required poses: " +
                    $"turn={forcedGoldenLeftTurnObserved}, " +
                    $"fall={forcedGoldenFallingLeftObserved}, " +
                    $"state={installed.GameState}");
                Console.WriteLine($"Frontend {room.RoomName} room: {frameCount} native-parity frames; all cartridge reads guarded.");
                return;
            }
            AssertTrue(goldenWakeObserved,
                "Golden Torizo wake-up fixture actually advances beyond the initial sleep");
            AssertTrue(goldenWalkingObserved && goldenRightwardObserved &&
                       goldenJumpBackObserved && goldenRightOrbObserved &&
                       goldenEyeBeamAttackObserved &&
                       installed.GameState == SuperMetroidGameState.MainGameplay,
                "Golden Torizo wake-up parity crosses walking-left and turning-right " +
                "and both orb and eye-beam attacks while gameplay remains active: " +
                $"walk={goldenWalkingObserved}, right={goldenRightwardObserved}, " +
                $"jumpBack={goldenJumpBackObserved}, rightOrb={goldenRightOrbObserved}, " +
                $"eyeBeam={goldenEyeBeamAttackObserved}, state={installed.GameState}");
        }
        Console.WriteLine($"Frontend {room.RoomName} room: {frameCount} native-parity frames; all cartridge reads guarded.");
    }

    /// <summary>Wraps installed memory with a guard rejecting cartridge reads and marked BG2 visual streams.</summary>
    internal static MutableCartridgeReadGuard NewFrontendCartridgeReadGuard(
        ISnesAddressSpace source, HashSet<int>? blockedPresentationBytes = null)
    {
        HashSet<int> blocked = blockedPresentationBytes ?? new();
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(blocked.Contains,
                address => $"Installed frontend reread BG2 visual stream byte ${address:X6}.")
            .Forbid(address => SnesAddressMath.BankNumber(address) is not (0x7e or 0x7f) && (address & 0x8000) != 0,
                address => $"Installed frontend reread cartridge byte ${address:X6}.");
        return guard;
    }
}
