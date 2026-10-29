using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Areas;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.Bosses.Ridley.Artwork;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ceres Ridley's Mode 7 getaway and the station's self-destruct countdown that follows.</summary>
[RoomLifetime]
internal sealed class CeresRidleyGetaway
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly BossFrameResultBuilder _frameResults;
    /// <summary>Provides room randomness and Ceres artwork state.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Queues Ceres battle and getaway sound effects.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Resolves the door and Mode-7 wall enemy records beside Ridley.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Writes Ceres target colors and supplemental composite drawing.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Owns the shared Ridley encounter and its battle state.</summary>
    private readonly RidleyEncounter _ridleyEncounter;
    /// <summary>Publishes Ceres escape and self-destruct room status.</summary>
    private readonly EnemyRoomStatus _roomStatus;
    /// <summary>The session's installed area artwork.</summary>
    private readonly AreaArtworkBinding _areaArtwork;
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;



    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal CeresRidleyGetaway(CeresRidleyGetawayDependencies dependencies)
    {
        _bossArtwork = dependencies.BossArtwork;
        _frameResults = dependencies.FrameResults;
        _room = dependencies.Room;
        _audio = dependencies.Audio;
        _slotTable = dependencies.SlotTable;
        _presentation = dependencies.Presentation;
        _ridleyEncounter = dependencies.RidleyEncounter;
        _roomStatus = dependencies.RoomStatus;
        _areaArtwork = dependencies.AreaArtwork;
    }

    /// <summary>
    /// Ports $A6:AA54. Every value below is a literal PPU/actor word installed by the retail
    /// setup routine; in particular B and C intentionally begin at $0100 before the first
    /// getaway-table entry replaces the matrix.
    /// </summary>
    internal static void SetupCeresRidleyGetawayFloor(RidleyEnemyState state)
    {
        state.GetawayActive = true;
        state.GetawayFinished = false;
        state.GetawayFrameIndex = 0;
        state.FloorAngle = RotationAngle.FromRaw(1);
        state.FloorOffset = new Offset16<FloorPixels>(-128, 32);
        state.FloorZoom = 2048;
        state.FloorMatrixA = CeresRidleyBehavior.FloorUnitCoefficient;
        state.FloorMatrixB = CeresRidleyBehavior.FloorUnitCoefficient;
        state.FloorMatrixC = CeresRidleyBehavior.FloorUnitCoefficient;
        state.FloorMatrixD = CeresRidleyBehavior.FloorUnitCoefficient;
        state.FloorCenter = new Position16<FloorPixels>(64, 64);
        state.GetawayBabyFrame = 0;
        state.GetawayWingFrame = 0;
    }

    /// <summary>
    /// Room main <c>MainASM_HandleCeresRidleyGetawayCutscene</c> ($8F:E571) and
    /// <c>HandleCeresRidleyGetawayCutsceneIfActive</c> ($A6:AAAF). It runs after the
    /// frame's enemy AI and Samus processing, so the getaway's first step shares the frame
    /// in which Ridley's escape handoff publishes Ceres status one.
    /// </summary>
    public void RunCeresRidleyGetawayRoomMain(SamusState? samus, int frameCounter)
    {
        if (_ridleyEncounter.CeresStatus != CeresEscapeStatus.RidleyEscaped)
            return;
        RidleyEnemyState state = _ridleyEncounter.Ridley ?? throw new InvalidOperationException(
            "Ceres status one requires the Ceres Ridley getaway state.");
        if (state.GetawayFinished)
            return;
        TickCeresRidleyGetawayFloor(state, samus, frameCounter);
    }

    /// <summary>
    /// Ports room-main function $A6:AABD through its $FFFF table terminator. This routine
    /// owns presentation only: ordinary Ridley has already been hidden, while Samus remains
    /// in room-world coordinates exactly as she does during the boss-room getaway.
    /// </summary>
    internal void TickCeresRidleyGetawayFloor(
        RidleyEnemyState state,
        SamusState? samus,
        int frameCounter)
    {
        int tableIndex = state.GetawayFrameIndex;
        state.GetawayFrameIndex = tableIndex + 1;

        // HandleCeresRidleyGetawayCutscene queues the departure sound on the first table
        // entry only ($A6:AABD -> QueueSfx2_Max6($4E)). Tying the request to the native
        // table index preserves the same one-shot behavior after saves/replays and pauses.
        if (tableIndex == 0)
            _audio.QueueEnemySound(Library2Sound.CeresRidleyGetaway, maximumQueued: SoundQueueLimit.Max6);

        // At frame $68 (byte index $D0), `$90:E119` replaces Samus's movement/hack handlers so the
        // rotating boss image cannot overlap her from the following frame onward.
        if (tableIndex == 104 && samus is not null)
            samus.CeresRidleyEjection.Request(samus);

        var frame = CeresRidleyGetawayCurve.FromIndex(tableIndex);
        int zoom = frame.Zoom;
        if (frame.IsFinished)
        {
            // $A6:AB2E restores fake BGMODE=$09 and clears all Mode 7/scroll registers.
            // Preserve that clean handoff explicitly so the renderer cannot accidentally
            // keep sampling Mode 7 VRAM for the self-destruct text scene.
            state.GetawayFinished = true;
            state.GetawayActive = false;
            state.FloorOffset = Offset16<FloorPixels>.Zero;
            state.FloorMatrixA = default;
            state.FloorMatrixB = default;
            state.FloorMatrixC = default;
            state.FloorMatrixD = default;
            state.FloorCenter = Position16<FloorPixels>.Zero;
            // $A6:AB55 hands the same extended actor workspace to $A6:C04E. Samus's
            // special push/fall handler remains responsible for restoring normal input;
            // ending Mode 7 itself does not unlock her in the cartridge.
            state.Function = RidleyAiFunction.CeresActivateSelfDestruct;
            state.FunctionTimer = WrappingCountdown.Start(0);
            return;
        }

        state.FloorZoom = zoom;
        // The getaway frame's velocity is added to the vertical word and subtracted from the
        // horizontal word.
        state.FloorOffset = new Offset16<FloorPixels>(
            unchecked((short)(state.FloorOffset.X - frame.Velocity.X)),
            unchecked((short)(state.FloorOffset.Y + frame.Velocity.Y)));

        UpdateCeresRidleyFloorPalette(zoom);
        state.FloorAngle = state.FloorAngle.AddRaw(48);
        UpdateCeresRidleyFloorMatrix(state);

        // Both animations' floor updates leave their descriptors behind, the capsule's first, so the wing's follow it.
        FloorUpdateResidue residue = _room.FloorQueueResidue ?? throw new InvalidOperationException(
            "The Ceres Ridley getaway requires the session's floor update residue.");
        var updates = new List<FloorUpdate>();

        // $A6:ACBC advances the Baby capsule on every fourth NMI using 0,1,2,1. The transfer
        // lists patch only the two two-byte rows occupied by that animation in the map.
        if ((frameCounter & 3) == 0)
        {
            state.GetawayBabyFrame = (state.GetawayBabyFrame + 1) % 4;
            ApplyFloorAnimationFrame(CeresFloorAnimation.BabyFrameForPhase(state.GetawayBabyFrame), updates);
        }

        // $A6:AD27 alternates the six sparse wing rows on every eighth NMI.
        if ((frameCounter & 7) == 0)
        {
            state.GetawayWingFrame = (state.GetawayWingFrame + 1) % 2;
            ApplyFloorAnimationFrame(state.GetawayWingFrame == 0
                ? CeresFloorAnimationFrame.WingFrame0
                : CeresFloorAnimationFrame.WingFrame1, updates);
        }
        residue.Record(updates);
    }

    /// <summary>
    /// Negates a signed 8.8 coefficient the way the getaway's two's-complement negate does: the most negative value,
    /// which has no positive counterpart, negates to itself.
    /// </summary>
    internal static SignedEightEight NegateFloorCoefficient(SignedEightEight coefficient) =>
        SignedEightEight.FromSignedRaw(unchecked((short)-coefficient.SignedRawValue));

    /// <summary>
    /// The perspective floor's transform as the getaway last turned and moved it, before room shake, or null while the
    /// getaway does not show the floor: the zoom-scaled rotation, the floor point it turns about, and the getaway's
    /// scroll across the floor.
    /// </summary>
    internal static AffineTransform? FloorTransform(RidleyEnemyState? ridley) => ridley is { GetawayActive: true } getaway
        ? new AffineTransform(getaway.FloorMatrixA.SignedRawValue, getaway.FloorMatrixB.SignedRawValue,
            getaway.FloorMatrixC.SignedRawValue, getaway.FloorMatrixD.SignedRawValue,
            getaway.FloorCenter.AsSignedWord(), getaway.FloorOffset)
        : null;

    /// <summary>Updates the getaway's Mode-7 transform matrix from its zoom and rotation words.</summary>
    private static void UpdateCeresRidleyFloorMatrix(RidleyEnemyState state)
    {
        ByteAngle angle = state.FloorAngle.ToByteAngle();
        var diagonal = new SignedEightEight(EnemyMath.MultiplySineSample(checked((ushort)state.FloorZoom), angle.AddTableUnits(ByteAngle.QuarterTurn.RawValue)));
        var offDiagonal = new SignedEightEight(EnemyMath.MultiplySineSample(checked((ushort)state.FloorZoom), angle));
        state.FloorMatrixA = diagonal;
        state.FloorMatrixB = offDiagonal;
        state.FloorMatrixC = NegateFloorCoefficient(offDiagonal);
        state.FloorMatrixD = diagonal;
    }

    /// <summary>Selects the zoom-dependent fifteen-color Ceres getaway palette without replacing transparent color zero.</summary>
    /// <param name="zoom">The floor's zoom, 0 through 65535.</param>
    internal void UpdateCeresRidleyFloorPalette(int zoom)
    {
        // SetCeresRidleyPaletteAccordingToZoomLevel uses this wrapped bank-$A6 expression.
        // There are fifteen colors; palette entry zero remains the shared transparent color.
        int zoomStep = UnsignedEightEight.Whole(zoom);
        if (_bossArtwork.CeresRidleyFloorColors is { } installed &&
            zoomStep < CeresRidleyPaletteDefinitions.GetawayZoomStepCount)
        {
            installed.Apply(_presentation.Colours, zoomStep);
            return;
        }
        throw new InvalidOperationException(
            $"Ceres Ridley Mode 7 zoom step {zoomStep} is not in the installed palette catalog.");
    }

    /// <summary>
    /// Draws one floor animation frame onto the room's floor (#1349) and adds its floor updates to
    /// <paramref name="updates"/>, whose descriptors outlive the update. Its tile numbers are immutable definitions; the
    /// source each update records is installed (#1415), so gameplay needs no cartridge read.
    /// </summary>
    private void ApplyFloorAnimationFrame(CeresFloorAnimationFrame frame, List<FloorUpdate> updates)
    {
        CeresRidleyFloorUpdateSources sources = _bossArtwork.CeresRidleyFloorUpdates ?? throw new InvalidOperationException(
            "The Ceres Ridley getaway requires the installed floor update sources.");
        CeresFloorAnimation.FrameRuns runs = CeresFloorAnimation.Get(frame);
        foreach (CeresFloorTileRun run in runs)
            _room.Services.DrawFloorMap(run.FloorCell, [.. run.TileNumbers]);
        for (int index = 0; index < runs.Count; index++)
            updates.Add(new FloorUpdate(sources.TileSource(frame, index), runs[index].TileNumbers.Count, runs[index].FloorCell));
    }

    /// <summary>
    /// Ports the shared Ridley function at $A6:C04E for the Ceres branch. The phase word
    /// is the actor's native variable F: even values select the exact C062/C08E/C09F/C0BB/
    /// C0F5/C104/C117 entries. English skips only the language-specific phase eight and
    /// enters phase ten to type the cartridge's three-line English warning. Japanese enters
    /// phase eight first, installs its subtitle tilemap, and then uses the same typewriter.
    /// </summary>
    internal void TickCeresRidleySelfDestruct(RidleyEnemyState state)
    {
        switch (state.FunctionTimer.RawValue)
        {
            case 0:
                // $A6:C062 copies four already-live colors before the warning graphics
                // replace their tiles. These are palette indices, not byte offsets.
                CopyCeresWarningColors(_presentation.BackgroundSets.Set(RidleyColourDefinitions.CeresWarningFirstSource),
                    _presentation.BackgroundSets.Set(RidleyColourDefinitions.CeresWarningFirstDestination));
                CopyCeresWarningColors(_presentation.BackgroundSets.Set(RidleyColourDefinitions.CeresWarningSecondSource),
                    _presentation.BackgroundSets.Set(RidleyColourDefinitions.CeresWarningSecondDestination));
                state.CeresEscapePages = CeresEscapePageCursor.Objects;
                state.FunctionTimer = WrappingCountdown.Start(2);
                goto case 2;

            case 2:
                if (ShowNextCeresEscapePage(state))
                {
                    state.CeresEscapePages = CeresEscapePageCursor.Backgrounds;
                    state.FunctionTimer = WrappingCountdown.Start(4);
                    goto case 4;
                }
                return;

            case 4:
                if (ShowNextCeresEscapePage(state))
                {
                    // `$A6:C0AA` calls DrawEmergencyText immediately after the final
                    // BG1/BG2 graphics record. The word was missing from the earlier
                    // translation, so the newly uploaded letter tiles existed but no
                    // tilemap entries ever referenced them on screen.
                    DrawCeresEmergencyText();
                    // $A6:C09F also queues music track seven after installing the final
                    // static warning tilemap page. The frontend consumes this typed request
                    // through the same bank-$80 queue used by every other music producer.
                    state.MusicRequest = MusicRequest.SelectTrack(7);
                    state.FunctionTimer = WrappingCountdown.Start(6);
                    state.CeresEscapeTextDelayTimer = WrappingCountdown.Start(128);
                }
                return;

            case 6:
                UpdateCeresSelfDestructPalette(state, _slotTable.Slots[0].FrameCounter);
                CountdownStep delay = state.CeresEscapeTextDelayTimer.Decrement();
                state.CeresEscapeTextDelayTimer = delay.Remaining;
                if (!delay.IsZero)
                    return;

                state.CeresEscapeTypewriter = _bossArtwork.EscapeTypewriterPresentation is { } presentation
                    ? new(presentation.Get(EscapeTypewriterProgramId.Ceres), EscapeTypewriterDefinitions.CeresTileBase)
                    : null;

                // `$A6:C0E3-$A6:C0F1` advances the even-byte dispatch index twice for
                // English before executing the common final increment: 6 -> 8 -> 10.
                // Japanese skips the first increment and reaches phase 8. The previous
                // translation incorrectly counted the two INC instructions as four phase
                // entries and jumped English directly to phase 12, suppressing every
                // character below "EMERGENCY".
                state.FunctionTimer = WrappingCountdown.FromRaw(_roomStatus.JapaneseText ? (ushort)8 : (ushort)10);
                return;

            case 8:
                UpdateCeresSelfDestructPalette(state, _slotTable.Slots[0].FrameCounter);
                CountdownStep japaneseDelay = state.CeresEscapeTextDelayTimer.Decrement();
                state.CeresEscapeTextDelayTimer = japaneseDelay.Remaining;
                if (japaneseDelay.IsZero)
                {
                    state.FunctionTimer = WrappingCountdown.Start(10);
                    DrawCeresJapaneseOverlay();
                }
                if (StepCeresEscapeTypewriter(state))
                    state.FunctionTimer = state.FunctionTimer.AddFrames(2);
                return;

            case 10:
                UpdateCeresSelfDestructPalette(state, _slotTable.Slots[0].FrameCounter);
                if (StepCeresEscapeTypewriter(state))
                    state.FunctionTimer = WrappingCountdown.Start(12);
                return;

            case 12:
                UpdateCeresSelfDestructPalette(state, _slotTable.Slots[0].FrameCounter);
                state.Velocity = new EightEightVector<RoomPixels>(0, 0);
                state.FunctionTimer = WrappingCountdown.Start(0);
                state.Function = RidleyAiFunction.CeresSelfDestructPaletteOnly;
                _ridleyEncounter.CeresStatus = CeresEscapeStatus.SelfDestructStarted;
                // $A6:C131 sets the area-boss bit inside Ridley's AI, so later slots (the
                // room's exit door at $A6:F562) observe it in this same EnemyMain pass.
                _room.Services.Bosses.SetDefeated(BossBits.AreaBoss);
                _frameResults.CeresEscapeStarted = true;
                return;

            default:
                throw new InvalidDataException(
                    $"Ceres self-destruct phase ${state.FunctionTimer.RawValue:X4} is not a native even dispatcher value.");
        }
    }

    /// <summary>$A6:C062: colours one and three of <paramref name="source"/> replace those of <paramref name="destination"/>.</summary>
    private static void CopyCeresWarningColors(ColourRun source, ColourRun destination)
    {
        destination.Write(1, source[1]);
        destination.Write(3, source[3]);
    }

    /// <summary>Ports DrawEmergencyText at $A6:C136: the EMERGENCY title is written over the room's BG1.</summary>
    internal void DrawCeresEmergencyText()
    {
        DrawCeresOverlay(CeresEscapeOverlayTilemapDefinitions.Emergency);
    }

    /// <summary>Writes one installed warning tilemap over the room's BG1 at its overlay cell.</summary>
    private void DrawCeresOverlay(CeresEscapeOverlayTilemapDefinition page)
    {
        CeresEscapeOverlays overlays = _areaArtwork.Artwork?.CeresEscapeOverlayTilemaps
            ?? throw new InvalidDataException("The Ceres escape has no installed warning tilemaps.");
        if (!overlays.TryResolve(page, out ReadOnlyMemory<BackgroundTile> tiles))
            throw new InvalidDataException($"Installed Ceres escape has no editable {page.Name} tilemap.");
        _room.Services.DrawBg1Overlay(page.Cell, tiles.Span);
    }

    /// <summary>
    /// Ports ProcessSpriteTilesTransfers at $A6:C26E (#1406): shows the list's next page, one an update. A page of the
    /// timer-sprite list shows over the OBJ characters from this update's frame, as the cartridge's copy, made at this
    /// update's NMI, shows in it; the background list's pages show nowhere, so each of its updates shows nothing.
    /// </summary>
    /// <returns>True once every page of the list has been shown.</returns>
    /// <exception cref="InvalidDataException">The cursor names no list.</exception>
    internal bool ShowNextCeresEscapePage(RidleyEnemyState state)
    {
        CeresEscapePageCursor cursor = state.CeresEscapePages;
        if (!cursor.IsStarted)
            throw new InvalidDataException("The Ceres escape graphics dispatcher was never given a page list.");
        if (cursor.IsFinished)
            return true;
        if (cursor.List == CeresEscapePageList.Objects)
        {
            _presentation.ShowLook(CeresEscapeObjectPages.ObjectPages[cursor.Shown]);
        }
        state.CeresEscapePages = cursor.Advance();
        return state.CeresEscapePages.IsFinished;
    }

    /// <summary>Writes every row half of the one-shot Japanese overlay list at $A6:C3B8 over the room's BG1.</summary>
    internal void DrawCeresJapaneseOverlay()
    {
        foreach (CeresEscapeOverlayTilemapDefinition page in CeresEscapeOverlayTilemapDefinitions.Japanese)
            DrawCeresOverlay(page);
    }

    /// <summary>Ports the byte-oriented command stream consumed by $A6:C2A7.</summary>
    private bool StepCeresEscapeTypewriter(RidleyEnemyState state)
    {
        var installed = state.CeresEscapeTypewriter ?? throw new InvalidDataException(
            "Ceres warning text requires installed typewriter artwork.");
        bool completed = installed.Step(out EscapeTypewriterGlyph? glyph);
        // The Ceres program types over the room's BG1 (#1348).
        if (glyph is { } typed)
            _room.Services.DrawBg1Overlay(typed.Cell, [typed.Tile]);
        state.CeresEscapeTextDelayTimer = installed.DelayTimer;
        if (installed.ClickRequested)
            _audio.QueueEnemySound(EscapeTypewriterDefinitions.CeresClick, maximumQueued: SoundQueueLimit.Max3);
        return completed;
    }

    /// <summary>Ports the three-color alarm palette cycle at $A6:C19C.</summary>
    internal void UpdateCeresSelfDestructPalette(RidleyEnemyState state, int frameCounter)
    {
        if ((frameCounter & 3) != 0)
            return;

        state.CeresEscapePaletteFrame = (state.CeresEscapePaletteFrame + 1) % 16;
        if (_bossArtwork.CeresRidleyColors is { } colors)
            colors.ApplyAlarm(_presentation.Colours, state.CeresEscapePaletteFrame);
        else throw new InvalidOperationException(
            "Ceres alarm requires installed Ridley color rows.");
    }
}
