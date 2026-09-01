using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// Verifies the pause/inventory seam at the end of the controller-only early-game route.
/// The production route must use the outer frontend; the compact room-policy harness keeps
/// a direct fallback so it can remain useful without pretending that fallback is end to end.
/// </summary>
internal static partial class EarlyControllerRouteAudit
{
    /// <summary>
    /// Opens the cartridge-backed pause screen over the completed route's live Samus,
    /// changes to equipment, toggles Bombs off and on, and returns to gameplay. When a
    /// frontend owns the route every operation passes through <see cref="SuperMetroidGame.Step"/>.
    /// </summary>
    private static void VerifyPauseEquipment(
        ISnesAddressSpace bus,
        SuperMetroidRuntime runtime,
        ControllerRouteHost host)
    {
        if (host.Frontend is not null)
        {
            VerifyFrontendPauseEquipment(runtime, host.Frontend, host);
            return;
        }

        VerifyDirectPauseEquipment(bus, runtime);
    }

    /// <summary>
    /// Drives native states $0C-$12 with ordinary controller samples. This catches input
    /// latching, brightness fades, pause-asset setup, page timing, and gameplay restoration
    /// that a directly constructed <see cref="PauseMenuState"/> cannot exercise.
    /// </summary>
    private static void VerifyFrontendPauseEquipment(
        SuperMetroidRuntime runtime,
        SuperMetroidGame game,
        ControllerRouteHost host)
    {
        SamusState samus = runtime.Samus ?? throw new InvalidOperationException(
            "Frontend pause verification requires live Samus.");
        if (!samus.EquippedItems.HasAny(SamusEquipmentFlags.Bombs))
            throw new InvalidDataException("Pause route began without the acquired Bombs equipped.");
        if (game.GameState != SuperMetroidGameState.MainGameplay)
        {
            throw new InvalidDataException(
                $"Pause route began in frontend state {game.GameState}, not gameplay.");
        }

        // Pause admission consumes Start's rising edge in the already-running state-eight
        // frame. The following frames must traverse the gameplay darken, forced-blank setup,
        // and pause-page fade-in before stable state $0F accepts menu input.
        host.StepFrame((ushort)SnesButton.Start);
        if (game.GameState != SuperMetroidGameState.PausingDarkening)
        {
            throw new InvalidDataException(
                $"Frontend Start edge selected {game.GameState}, not pause darkening.");
        }
        int pauseEntryFrames = StepFrontendUntil(
            host,
            game,
            candidate => candidate.GameState == SuperMetroidGameState.PausedB,
            maximumFrames: 48,
            "pause fade-in did not reach stable state $0F");
        AssertVisiblePauseFrame(game.CurrentFrame, "map");
        if (game.PauseScreenMode != 0)
            throw new InvalidDataException($"Frontend pause opened screen mode {game.PauseScreenMode}.");

        // Shared pause chrome reads bank-$80's delayed-held word, not the raw rising edge.
        // Hold R long enough to pass its three-frame filter, then release while the two
        // fifteen-step page fades complete. The selected item must be the first collected
        // equipment entry: category two/item two is Morph Ball in the cartridge tables.
        for (int frame = 0; frame < 40; frame++)
            host.StepFrame(frame < 8 ? (ushort)SnesButton.R : (ushort)0);
        if (game.GameState != SuperMetroidGameState.PausedB ||
            game.PauseScreenMode != 1 ||
            game.PauseSelectedEquipmentCategory != 2 ||
            game.PauseSelectedEquipmentItem != 2)
        {
            throw new InvalidDataException(
                $"Frontend equipment page settled at state/mode/category/item " +
                $"{game.GameState}/{game.PauseScreenMode}/" +
                $"{game.PauseSelectedEquipmentCategory}/{game.PauseSelectedEquipmentItem}.");
        }

        host.StepFrame((ushort)SnesButton.Down);
        if (game.PauseSelectedEquipmentItem != 3)
            throw new InvalidDataException("Frontend pause selector did not move from Morph Ball to Bombs.");
        host.StepFrame(0);
        host.StepFrame((ushort)SnesButton.A);
        if (samus.EquippedItems.HasAny(SamusEquipmentFlags.Bombs))
            throw new InvalidDataException("Frontend pause A edge did not unequip Bombs.");
        host.StepFrame(0);
        host.StepFrame((ushort)SnesButton.A);
        if (!samus.EquippedItems.HasAny(SamusEquipmentFlags.Bombs))
            throw new InvalidDataException("Frontend pause A edge did not re-equip Bombs.");
        AssertVisiblePauseFrame(game.CurrentFrame, "equipment");

        // Start uses delayed-held input here too. Hold only until state $10 accepts it,
        // then release through both pause fade-out and gameplay fade-in so a stuck button
        // cannot accidentally satisfy another state transition.
        int unpauseRequestFrames = 0;
        while (game.GameState == SuperMetroidGameState.PausedB && unpauseRequestFrames < 12)
        {
            host.StepFrame((ushort)SnesButton.Start);
            unpauseRequestFrames++;
        }
        if (game.GameState != SuperMetroidGameState.UnpausingA)
        {
            throw new InvalidDataException(
                $"Frontend pause Start hold stopped in {game.GameState} after " +
                $"{unpauseRequestFrames} frames.");
        }
        int unpauseFrames = StepFrontendUntil(
            host,
            game,
            candidate => candidate.GameState == SuperMetroidGameState.MainGameplay,
            maximumFrames: 48,
            "pause restoration did not return to state $08");

        Console.WriteLine(
            $"  Frontend pause route: entered in {pauseEntryFrames} frames, changed ROM " +
            $"pages, toggled live Bombs, and restored gameplay in " +
            $"{unpauseRequestFrames + unpauseFrames} frames.");
    }

    private static int StepFrontendUntil(
        ControllerRouteHost host,
        SuperMetroidGame game,
        Func<SuperMetroidGame, bool> predicate,
        int maximumFrames,
        string failure)
    {
        for (int frame = 0; frame < maximumFrames; frame++)
        {
            host.StepFrame(0);
            if (predicate(game))
                return frame + 1;
        }
        throw new InvalidDataException(
            $"{failure}; stopped in {game.GameState} ({game.CurrentFrame.Phase}).");
    }

    private static void AssertVisiblePauseFrame(FrontendFrame frame, string page)
    {
        if (frame.Pixels.Length != FrontendFrame.Width * FrontendFrame.Height ||
            frame.Pixels.All(pixel => pixel.R == 0 && pixel.G == 0 && pixel.B == 0))
        {
            throw new InvalidDataException($"Frontend pause {page} page rendered an empty frame.");
        }
    }

    /// <summary>
    /// Retains the historical fast audit for a runtime without an outer dispatcher. It
    /// verifies ROM tilemaps and OAM in detail, but is intentionally not the authoritative
    /// proof that Start and the frontend pause states are connected.
    /// </summary>
    private static void VerifyDirectPauseEquipment(
        ISnesAddressSpace bus,
        SuperMetroidRuntime runtime)
    {
        SamusState samus = runtime.Samus ?? throw new InvalidOperationException(
            "Pause route verification requires live Samus.");
        CartridgeRoomHeader room = runtime.ActiveRoom ?? throw new InvalidOperationException(
            "Pause route verification requires an active room.");
        if (!samus.EquippedItems.HasAny(SamusEquipmentFlags.Bombs))
            throw new InvalidDataException("Pause route began without the acquired Bombs equipped.");

        var pause = new PauseMenuState(
            bus,
            samus,
            runtime.System,
            room.AreaIndex,
            room.MapX,
            room.MapY);
        Rgba32[] mapFrame = pause.Render();
        if (mapFrame.Length != FrontendFrame.Width * FrontendFrame.Height ||
            mapFrame.All(pixel => pixel.R == 0 && pixel.G == 0 && pixel.B == 0))
        {
            throw new InvalidDataException("Cartridge pause map rendered an empty frame.");
        }
        if (pause.LastRenderedSpriteCount == 0 ||
            pause.LastIndicatorSpritemapId is < 0x5f or > 0x61)
        {
            throw new InvalidDataException(
                $"Cartridge pause map emitted {pause.LastRenderedSpriteCount} sprites with " +
                $"indicator ID ${pause.LastIndicatorSpritemapId:X2}.");
        }

        pause.Step((ushort)SnesButton.R, 0);
        for (int frame = 0; frame < 32; frame++)
            pause.Step(0, 0);
        if (pause.ScreenMode != 1 || pause.SelectedCategory != 2 || pause.SelectedItem != 2)
        {
            throw new InvalidDataException(
                $"Pause equipment opened at mode/category/item " +
                $"{pause.ScreenMode}/{pause.SelectedCategory}/{pause.SelectedItem}; " +
                "the first collected early-game item must be Morph Ball (2/2).");
        }

        pause.Step(0, (ushort)SnesButton.Down);
        if (pause.SelectedItem != 3)
            throw new InvalidDataException("Pause selector did not move from Morph Ball to collected Bombs.");
        pause.Step(0, (ushort)SnesButton.A);
        if (samus.EquippedItems.HasAny(SamusEquipmentFlags.Bombs))
            throw new InvalidDataException("Pause A input did not unequip Bombs.");
        pause.Step(0, 0);
        pause.Step(0, (ushort)SnesButton.A);
        if (!samus.EquippedItems.HasAny(SamusEquipmentFlags.Bombs))
            throw new InvalidDataException("Pause A input did not re-equip Bombs.");

        Rgba32[] equipmentFrame = pause.Render();
        if (equipmentFrame.All(pixel => pixel.R == 0 && pixel.G == 0 && pixel.B == 0))
            throw new InvalidDataException("Cartridge pause equipment page rendered an empty frame.");
        if (pause.LastRenderedSpriteCount == 0)
            throw new InvalidDataException("Cartridge pause equipment selector emitted no OAM records.");
        Console.WriteLine(
            "  Direct pause fallback: ROM map/selector OAM and Morph/Bombs inventory toggles agree.");
    }
}
