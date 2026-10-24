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
    internal static void SetupCeresRidleyMode7(RidleyEnemyState state)
    {
        state.Mode7Active = true;
        state.Mode7Finished = false;
        state.Mode7TableIndex = 0;
        state.FloorAngle = SnesAngle.FromRaw(1);
        state.Mode7Offset = new Offset16<Mode7Pixels>(-128, 32);
        state.FloorZoom = 2048;
        state.Mode7MatrixA = CeresRidleyBehavior.Mode7UnitCoefficient;
        state.Mode7MatrixB = CeresRidleyBehavior.Mode7UnitCoefficient;
        state.Mode7MatrixC = CeresRidleyBehavior.Mode7UnitCoefficient;
        state.Mode7MatrixD = CeresRidleyBehavior.Mode7UnitCoefficient;
        state.Mode7Center = new Position16<Mode7Pixels>(64, 64);
        state.Mode7BabyFrame = 0;
        state.Mode7WingFrame = 0;
    }

    /// <summary>
    /// Room main <c>MainASM_HandleCeresRidleyGetawayCutscene</c> ($8F:E571) and
    /// <c>HandleCeresRidleyGetawayCutsceneIfActive</c> ($A6:AAAF). It runs after the
    /// frame's enemy AI and Samus processing, so the getaway's first step shares the frame
    /// in which Ridley's escape handoff publishes Ceres status one.
    /// </summary>
    public void RunCeresRidleyGetawayRoomMain(SamusState? samus, int nmiFrameCounter)
    {
        if (_ridleyEncounter.CeresStatus != CeresEscapeStatus.RidleyEscaped)
            return;
        RidleyEnemyState state = _ridleyEncounter.Ridley ?? throw new InvalidOperationException(
            "Ceres status one requires the Ceres Ridley getaway state.");
        if (state.Mode7Finished)
            return;
        TickCeresRidleyMode7Getaway(state, samus, nmiFrameCounter);
    }

    /// <summary>
    /// Ports room-main function $A6:AABD through its $FFFF table terminator. This routine
    /// owns presentation only: ordinary Ridley has already been hidden, while Samus remains
    /// in room-world coordinates exactly as she does during the boss-room getaway.
    /// </summary>
    internal void TickCeresRidleyMode7Getaway(
        RidleyEnemyState state,
        SamusState? samus,
        int nmiFrameCounter)
    {
        int tableIndex = state.Mode7TableIndex;
        state.Mode7TableIndex = tableIndex + 1;

        // HandleCeresRidleyGetawayCutscene queues the departure sound on the first table
        // entry only ($A6:AABD -> QueueSfx2_Max6($4E)). Tying the request to the native
        // table index preserves the same one-shot behavior after saves/replays and pauses.
        if (tableIndex == 0)
            _audio.QueueEnemySound(Library2Sound.CeresRidleyGetaway, maximumQueued: 6);

        // At frame $68 (byte index $D0), `$90:E119` replaces Samus's movement/hack handlers so the
        // rotating boss image cannot overlap her from the following frame onward.
        if (tableIndex == 104 && samus is not null)
            samus.CeresRidleyEjection.Request(samus);

        var frame = CeresRidleyGetawayDefinitions.FromIndex(tableIndex);
        ushort zoom = checked((ushort)frame.Zoom);
        if (zoom == CeresRidleyGetawayDefinitions.Finished)
        {
            // $A6:AB2E restores fake BGMODE=$09 and clears all Mode 7/scroll registers.
            // Preserve that clean handoff explicitly so the renderer cannot accidentally
            // keep sampling Mode 7 VRAM for the self-destruct text scene.
            state.Mode7Finished = true;
            state.Mode7Active = false;
            state.Mode7Offset = Offset16<Mode7Pixels>.Zero;
            state.Mode7MatrixA = default;
            state.Mode7MatrixB = default;
            state.Mode7MatrixC = default;
            state.Mode7MatrixD = default;
            state.Mode7Center = Position16<Mode7Pixels>.Zero;
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
        state.Mode7Offset = new Offset16<Mode7Pixels>(
            unchecked((short)(state.Mode7Offset.X - frame.Velocity.X)),
            unchecked((short)(state.Mode7Offset.Y + frame.Velocity.Y)));

        UpdateCeresRidleyMode7Palette(zoom);
        state.FloorAngle = state.FloorAngle.AddRaw(48);
        UpdateCeresRidleyMode7Matrix(state);

        // Both animations' floor updates leave their descriptors behind, the capsule's first, so the wing's follow it.
        FloorUpdateResidue residue = _room.FloorQueueResidue ?? throw new InvalidOperationException(
            "The Ceres Ridley getaway requires the session's floor update residue.");
        var updates = new List<FloorUpdate>();

        // $A6:ACBC advances the Baby capsule on every fourth NMI using 0,1,2,1. The transfer
        // lists patch only the two two-byte rows occupied by that animation in the map.
        if ((nmiFrameCounter & 3) == 0)
        {
            state.Mode7BabyFrame = (state.Mode7BabyFrame + 1) % 4;
            ApplyMode7TransferList(CeresMode7TransferDefinitions.BabyFrameForPhase(state.Mode7BabyFrame), updates);
        }

        // $A6:AD27 alternates the six sparse wing rows on every eighth NMI.
        if ((nmiFrameCounter & 7) == 0)
        {
            state.Mode7WingFrame = (state.Mode7WingFrame + 1) % 2;
            ApplyMode7TransferList(state.Mode7WingFrame == 0
                ? CeresMode7TransferList.WingFrame0
                : CeresMode7TransferList.WingFrame1, updates);
        }
        residue.Record(updates);
    }

    /// <summary>
    /// Negates a signed 8.8 coefficient the way the getaway's two's-complement negate does: the most negative value,
    /// which has no positive counterpart, negates to itself.
    /// </summary>
    internal static SnesSignedEightEight NegateMode7Coefficient(SnesSignedEightEight coefficient) =>
        SnesSignedEightEight.FromSignedRaw(unchecked((short)-coefficient.SignedRawValue));

    /// <summary>
    /// The perspective floor's transform as the getaway last turned and moved it, before room shake, or null while the
    /// getaway does not show the floor: the zoom-scaled rotation, the floor point it turns about, and the getaway's
    /// scroll across the floor.
    /// </summary>
    internal static AffineTransform? FloorTransform(RidleyEnemyState? ridley) => ridley is { Mode7Active: true } getaway
        ? new AffineTransform(getaway.Mode7MatrixA.SignedRawValue, getaway.Mode7MatrixB.SignedRawValue,
            getaway.Mode7MatrixC.SignedRawValue, getaway.Mode7MatrixD.SignedRawValue,
            getaway.Mode7Center.AsSignedRegister(), getaway.Mode7Offset)
        : null;

    /// <summary>Updates the getaway's Mode-7 transform matrix from its zoom and rotation words.</summary>
    private static void UpdateCeresRidleyMode7Matrix(RidleyEnemyState state)
    {
        byte angle = state.FloorAngle.TableIndex;
        ushort zoom = checked((ushort)state.FloorZoom);
        var diagonal = new SnesSignedEightEight(EnemyMath.MultiplyCartridgeSinCos(zoom, new SnesByteAngle(angle).AddTableUnits(SnesByteAngle.QuarterTurn.RawValue)));
        var offDiagonal = new SnesSignedEightEight(EnemyMath.MultiplyCartridgeSinCos(zoom, new SnesByteAngle(angle)));
        state.Mode7MatrixA = diagonal;
        state.Mode7MatrixB = offDiagonal;
        state.Mode7MatrixC = NegateMode7Coefficient(offDiagonal);
        state.Mode7MatrixD = diagonal;
    }

    /// <summary>Selects the zoom-dependent fifteen-color Ceres getaway palette without replacing transparent color zero.</summary>
    internal void UpdateCeresRidleyMode7Palette(ushort zoom)
    {
        // SetCeresRidleyPaletteAccordingToZoomLevel uses this wrapped bank-$A6 expression.
        // There are fifteen colors; palette entry zero remains the shared transparent color.
        int zoomHighByte = SnesLittleEndian.HighByte(zoom);
        if (_bossArtwork.CeresRidleyFloorColors is { } installed &&
            zoomHighByte < CeresRidleyPaletteRomData.Mode7ZoomRowCount)
        {
            installed.Apply(_presentation.Cgram, zoomHighByte);
            return;
        }
        throw new InvalidOperationException(
            $"Ceres Ridley Mode 7 zoom row {zoomHighByte} is not in the installed palette catalog.");
    }

    /// <summary>
    /// Writes one compiled $80:8B4F Mode-7 transfer list into the room's Mode 7 map (#1349) and adds its floor updates to
    /// <paramref name="updates"/>, whose descriptors outlive the update. Its tile numbers are immutable definitions, so
    /// gameplay needs no bank-$A6 read.
    /// </summary>
    private void ApplyMode7TransferList(CeresMode7TransferList transferList, List<FloorUpdate> updates)
    {
        foreach (CeresMode7Transfer transfer in CeresMode7TransferDefinitions.Get(transferList))
            _room.Services.DrawMode7Map(transfer.MapCell, [.. transfer.TileNumbers]);
        foreach (CeresMode7Transfer transfer in CeresMode7TransferDefinitions.Get(transferList))
            updates.Add(new FloorUpdate(transfer.SourceOffset, transfer.TileNumbers.Count, transfer.MapCell));
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
                CopyCeresWarningColors(_presentation.BackgroundSets.Set(0), _presentation.BackgroundSets.Set(6));
                CopyCeresWarningColors(_presentation.BackgroundSets.Set(1), _presentation.BackgroundSets.Set(5));
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
                    state.FunctionTimer = WrappingCountdown.FromRaw(unchecked((ushort)(state.FunctionTimer.RawValue + 2)));
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
        CeresEscapeOverlayTilemapCatalog overlays = _areaArtwork.Artwork?.CeresEscapeOverlayTilemaps
            ?? throw new InvalidDataException("The Ceres escape has no installed warning tilemaps.");
        if (!overlays.TryResolve(page.SourceAddress, page.WordCount * sizeof(ushort), out ReadOnlyMemory<byte> data))
            throw new InvalidDataException($"Installed Ceres escape has no editable {page.Name} tilemap.");
        var words = new ushort[page.WordCount];
        for (int index = 0; index < words.Length; index++)
            words[index] = SnesLittleEndian.Word(data.Span[index * 2], data.Span[index * 2 + 1]);
        _room.Services.DrawBg1Overlay(page.Cell, words);
    }

    /// <summary>
    /// Ports ProcessSpriteTilesTransfers at $A6:C26E (#1406): shows the list's next page, one an update. A page of the
    /// timer-sprite list shows over the OBJ characters from this update's frame, as the cartridge's copy, made at this
    /// update's NMI, shows in it; the background list's pages show nowhere, so each of its updates shows nothing.
    /// </summary>
    /// <returns>True once every page of the list has been shown.</returns>
    /// <exception cref="InvalidDataException">The cursor names no list, or a page's installed artwork is missing.</exception>
    internal bool ShowNextCeresEscapePage(RidleyEnemyState state)
    {
        CeresEscapePageCursor cursor = state.CeresEscapePages;
        if (!cursor.IsStarted)
            throw new InvalidDataException("The Ceres escape graphics dispatcher was never given a page list.");
        if (cursor.IsFinished)
            return true;
        if (cursor.List == CeresEscapePageList.Objects)
        {
            ObjCharacterSource page = CeresEscapePageDefinitions.ObjectPages[cursor.Shown];
            RequireInstalled(page);
            _presentation.ShowCharacters(page);
        }
        state.CeresEscapePages = cursor.Advance();
        return state.CeresEscapePages.IsFinished;
    }

    /// <summary>Requires the installed artwork a timer-sprite page shows, when the room is drawn from installed artwork.</summary>
    /// <exception cref="InvalidDataException">The installed escape timer or Ceres escape character pages are missing.</exception>
    private void RequireInstalled(ObjCharacterSource source)
    {
        if (_room.Artwork is null)
            return;
        if (source is AssetObjCharacters && _bossArtwork.EscapeTimerArtwork is null)
            throw new InvalidDataException("Installed Ceres escape timer has no editable sprite tile artwork.");
        if (source is CeresEscapeTileCharacters characters &&
            _areaArtwork.Artwork?.CeresEscapeTiles?.TryResolve(
                CeresEscapeTileArtworkDefinitions.All[characters.Page].SourceAddress + characters.Offset, characters.ByteCount, out _) != true)
            throw new InvalidDataException($"Installed Ceres escape has no editable {characters}.");
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
            _room.Services.DrawBg1Overlay(typed.Cell, [typed.Tile.Raw]);
        state.CeresEscapeTextDelayTimer = installed.DelayTimer;
        if (installed.ClickRequested)
            _audio.QueueEnemySound(EscapeTypewriterRomData.CeresClick, maximumQueued: 3);
        return completed;
    }

    /// <summary>Ports the three-color alarm palette cycle at $A6:C19C.</summary>
    internal void UpdateCeresSelfDestructPalette(RidleyEnemyState state, int frameCounter)
    {
        if ((frameCounter & 3) != 0)
            return;

        state.CeresEscapePaletteFrame = (state.CeresEscapePaletteFrame + 1) % 16;
        if (_bossArtwork.CeresRidleyColors is { } colors)
            colors.ApplyAlarm(_presentation.Cgram, state.CeresEscapePaletteFrame);
        else throw new InvalidOperationException(
            "Ceres alarm requires installed Ridley color rows.");
    }
}
