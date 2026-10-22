using SuperMetroid.Core.Bosses.Ridley;
using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.World;

/// <summary>
/// The NMI: controller latching, frame-counter advance, accepted and forced-blank transfers, hardware-wait NMIs,
/// and the display captures published at the NMI.
/// </summary>
internal sealed class AcceptedFrameBoundary
{
    /// <summary>Latches the controllers and advances the frame counters after publication.</summary>
    private readonly AcceptedFrameInput _input;
    /// <summary>Publishes rebound HUD, beam, and trail art after accepted VRAM transfers.</summary>
    private readonly RuntimeArtwork _artwork;
    /// <summary>Captures the BG1 and BG2 scroll registers visible with the accepted NMI.</summary>
    private readonly BackgroundScrollState _backgroundScroll;
    /// <summary>Retains the accepted-NMI display latch.</summary>
    private readonly GameplayDisplayState _display;
    /// <summary>Sprite frame made visible at each accepted NMI.</summary>
    private readonly SpriteFrameBuffer _displayedSprites;
    /// <summary>The session's OBJ character table, which the displayed gameplay object page binds.</summary>
    private readonly EnemySession _enemySession;

    /// <summary>
    /// The loading or loaded room's enemy displays, whose bosses the publication latches; null before the gameplay
    /// runtime's first room, when no enemy is displayed.
    /// </summary>
    private IEnemyDisplaySources? Displays => _room.Enemies?.DisplaySources;
    /// <summary>Window register cache latched with the accepted frame.</summary>
    private readonly GameplayWindowRegisters _gameplayWindows;
    /// <summary>Sprite frame the main loop finalized for the next NMI.</summary>
    private readonly SpriteFrameBuffer _sprites;
    /// <summary>Supplies Samus graphics DMA for accepted NMI transfers.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Supplies the session COLDATA shadows.</summary>
    private readonly RoomFxSession _roomFx;
    /// <summary>The HUD whose rows the BG3 plane shows.</summary>
    private readonly HudState _hud;
    /// <summary>Supplies the loaded room, door, and scrolling sky whose register shadows are latched.</summary>
    private readonly ActiveRoomState _room;

    /// <summary>The background character bindings the room's block pictures are resolved over (#1392).</summary>
    private readonly BgArtworkBindings _backgrounds;
    /// <summary>Supplies the suit pickup whose COLDATA color is latched.</summary>
    private readonly SamusSuitPickupState _suitPickup;
    /// <summary>Supplies the frozen X-ray reveal map the publication latches.</summary>
    private readonly XrayReveal _xray;
    /// <summary>Executes the queued video transfers, whose address-sourced entries read live memory.</summary>
    private readonly Action _drainQueuedVramWrites;

    /// <summary>Connects controller, PPU, sprite, video-queue, and frame-clock owners to the NMI boundary.</summary>
    internal AcceptedFrameBoundary(AcceptedFrameBoundaryDependencies dependencies)
    {
        _input = dependencies.Input;
        _artwork = dependencies.Artwork;
        _backgroundScroll = dependencies.BackgroundScroll;
        _display = dependencies.Display;
        _displayedSprites = dependencies.DisplayedSprites;
        _enemySession = dependencies.EnemySession;
        _gameplayWindows = dependencies.GameplayWindows;
        _sprites = dependencies.Sprites;
        _player = dependencies.Player;
        _room = dependencies.Room;
        _backgrounds = dependencies.Backgrounds;
        _roomFx = dependencies.RoomFx;
        _hud = dependencies.Hud;
        _suitPickup = dependencies.SuitPickup;
        _xray = dependencies.Xray;
        _drainQueuedVramWrites = dependencies.DrainQueuedVramWrites;
    }

    /// <summary>
    /// Monotonic host sequence advanced only after a complete state-eight gameplay owner
    /// pass has published its per-frame audio requests.
    /// </summary>
    /// <remarks>
    /// Door transitions and other frontend coroutines can run accepted NMIs without
    /// running enemy, projectile, or PLM owners. Consumers must distinguish those waits
    /// from a fresh gameplay publication; otherwise the last request is re-enqueued on
    /// every transition NMI and the native sound-queue drain can never finish.
    /// </remarks>
    public ulong CompletedGameplayAudioPublication { get; internal set; }

    /// <summary>
    /// Runs the accepted NMI portion currently translated from <c>$80:9583</c>: publication, the controller latch, then
    /// the frame counters. The port has no lagged NMIs; hardware lag is eliminated.
    /// </summary>
    /// <param name="controller1Input">Host-provided raw SNES controller word.</param>
    public void BeginAcceptedFrame(SnesButton controller1Input)
    {
        PublishAcceptedFrameTransfers();
        _input.Latch(controller1Input);
    }

    /// <summary>
    /// Applies queued graphics while a loading dispatch holds forced blank with NMI
    /// disabled. Native loaders write these directly; no NMI runs, so neither the
    /// controller latch nor any NMI counter changes.
    /// </summary>
    internal void PublishForcedBlankTransfers() => PublishAcceptedFrameTransfers();

    /// <summary>The OAM, Samus, video-queue and register publication of $80:959E..$80:95DE.</summary>
    private void PublishAcceptedFrameTransfers()
    {
        // Preserve $80:95A1 -> $80:95D0 -> $80:95E1 order: dedicated Samus graphics
        // DMA precedes the general video queue, and both precede controller latching.
        // $80:959E uploads the finalized main-loop OAM image immediately before that
        // Samus DMA. `$80:95A7` subsequently copies the Mode 7 shadow words to
        // $211B..$2120. Retain both displayed values so software rendering sees one
        // coherent PPU phase instead of combining old OAM with a newer room-main matrix.
        _displayedSprites.LatchFrom(_sprites);
        _gameplayWindows.Cache.LatchNmi();
        var ppu = new GameplayPpuRenderSnapshot(
            _backgroundScroll.Layer1Position,
            _backgroundScroll.Bg1Scroll,
            _backgroundScroll.Bg2Scroll,
            // Before the first room no enemy earthquake shakes the room.
            _room.Enemies?.Earthquake.LastRoomShake ?? default);
        RoomLayer3FxRenderSnapshot? roomLayer3Fx = _room.Layer3Fx?.CaptureForDisplay();
        Displays?.Phantoon?.Wave.LatchDisplay();
        if (Displays?.Phantoon is { } phantoonDisplay)
            phantoonDisplay.Blending.LatchDisplay(phantoonDisplay.MosaicBlockSize);
        _room.TourianStatues?.LatchDisplay();
        ImmutableArray<ushort>? reveal = CaptureXrayRevealTilemap();
        _display.Latched = new GameplayNmiDisplayLatch(
            ppu,
            _gameplayWindows.DisplayedGameplayWindowRegisters,
            _display.ActiveSamusMode7Transform,
            roomLayer3Fx,
            CaptureMorphBallEyeBeamForDisplay(),
            Displays?.Phantoon is { } phantoon ? PhantoonDisplaySnapshot.Capture(phantoon) : null,
            // Before the first room there are no statues, so no descent is displayed.
            _room.TourianStatues?.DisplayedVerticalOffset ?? 0,
            CaptureSceneRegisters(),
            CaptureBossBackgrounds(),
            CaptureFixedColors(),
            // The reveal and the room's planes are drawn once this NMI's uploads have bound (WithBlockPictures).
            null,
            CaptureRoomBackgrounds());
        _artwork.PublishReboundHudArtwork();
        _drainQueuedVramWrites();
        // The planes' blocks show what this NMI's uploads bound, as the characters they read did (#1392).
        if (_display.Latched.Rooms is { } rooms)
            _display.Latched = WithBlockPictures(_display.Latched, rooms, reveal);
        // The displayed enemy-format draws show the OBJ characters this frame's uploads bound, as the OAM it latched
        // reads the VRAM those uploads wrote.
        if (_displayedSprites.Shows(SpriteArtworkKind.GameplayObjects))
            _displayedSprites.RebindArtwork(SpriteArtworkKind.GameplayObjects, _artwork.GameplayObjectPage(_enemySession.ObjCharacters));
        // A rebound snapshot may retain old CGRAM and legacy queued transfers.
        // Apply current content only at this accepted NMI, after those writes.
        _artwork.PublishReboundBeamPalette();
    }

    /// <summary>
    /// Room-level register shadows uploaded by <c>$80:91EE</c>: BGMODE (Ceres elevator Mode 7), BG34NBA,
    /// the layer-blending selectors behind TM/TS/CGWSEL/CGADSUB (<c>$88:8000</c>), and the sky's
    /// BG2SC and BG2VOFS mirror (<c>$88:AFB2</c>).
    /// </summary>
    private GameplayLatchedSceneRegisters CaptureSceneRegisters()
    {
        var room = _room.ActiveRoom;
        return new(
            _room.ActiveDoor?.UsesCeresElevatorMode7 == true,
            _display.GameplayHudCharacterBaseWord,
            // Before the first room no FX record is selected and no blending is configured.
            _room.Layer3Fx?.Type ?? RoomFxType.None,
            _room.Layer3Fx?.LayerBlendConfiguration ?? LayerBlendingConfiguration.Unconfigured,
            ScrollingSkyRegisterSnapshot.Capture(_room.ScrollingSky),
            room?.Definition,
            // Before the first room no boss is loaded.
            _room.Enemies?.RoomStatus.BossId ?? 0);
    }

    /// <summary>
    /// Boss BG register shadows: Ceres Ridley's M7A..M7Y (written by room main <c>$A6:AAAF</c>,
    /// uploaded with BGMODE's fake Mode 7 HDMA byte by <c>$80:91EE</c>), Kraid's body on BG2 (#1352), Crocomire's
    /// BG2SC/BG2HOFS/BG2VOFS, and Mother Brain's BG2SC and TM BG2 bit.
    /// </summary>
    private GameplayLatchedBossBackgrounds CaptureBossBackgrounds()
    {
        CeresRidleyMode7Snapshot? ridley = CeresRidleyMode7Snapshot.Capture(Displays?.CeresRidley);
        var motherBrain = Displays?.MotherBrain;
        return new(
            ridley,
            Displays?.KraidBody,
            Displays?.EnemyBg2,
            Displays?.Draygon is { } draygon
                ? new DraygonBodyPlacement(draygon.Body.Position.Whole, draygon.BodyBg2Scroll, draygon.Body.Properties.HasAny(EnemyProperties.Deleted))
                : null,
            Displays?.CrocomireBg2Scroll,
            motherBrain is { HasBg2ScrollOverride: true } or { DeathBg2Hidden: true }
                ? new MotherBrainBodyPlacement(motherBrain.HasBg2ScrollOverride ? motherBrain.Bg2Scroll : null, motherBrain.DeathBg2Hidden)
                : null);
    }

    /// <summary>
    /// The room backgrounds this NMI displays (#1346): BG1 from the level plane through the BG1 offset, BG2 from the
    /// layer-2 plane when that layer streams (otherwise BG2 shows the tilemap its owner loaded), the source screen of a
    /// door's opening scroll, and the room's BG1 overlay. The planes are shared until gameplay changes level data.
    /// </summary>
    private RoomBackgrounds? CaptureRoomBackgrounds()
    {
        // Before the first room no background is shown.
        if (_room.Scope is not { } scope)
            return null;
        RoomLevelData level = scope.LevelData;
        Layer2ScrollModes layer2 = _backgroundScroll.Layer2Scroll;
        // Each plane's pictures are resolved once this NMI's uploads have bound (WithBlockPictures).
        return new RoomBackgrounds(
            new RoomBackgroundLayer(level.ForegroundPlane, _backgroundScroll.Bg1Offset.AsBgPlaneOffset(), null),
            layer2.Horizontal.Streams || layer2.Vertical.Streams
                ? new RoomBackgroundLayer(level.BackgroundPlane, _backgroundScroll.Bg2Offset.AsBgPlaneOffset(), null)
                : null,
            CaptureLayer3Plane(),
            _room.DoorSourceBackground,
            scope.Bg1Overlay,
            Bg2Overlay: scope.Bg2Overlay,
            Mode7: scope.Mode7Map?.Publish(),
            Characters: RoomScope.CharactersOf(_room.ActiveRoom));
    }

    /// <summary>
    /// <paramref name="latched"/> with <paramref name="rooms"/>' BG1 and BG2 planes, and the X-ray reveal built from
    /// <paramref name="reveal"/>'s words, showing the pictures their blocks show over what the bindings show now (#1392);
    /// unchanged while the planes draw from the object character page, where X-ray is never used.
    /// </summary>
    /// <exception cref="InvalidOperationException">X-ray reveals a room whose planes draw from the object character page.</exception>
    private GameplayNmiDisplayLatch WithBlockPictures(GameplayNmiDisplayLatch latched, RoomBackgrounds rooms, ImmutableArray<ushort>? reveal)
    {
        if (rooms.Characters != RoomBackgroundCharacters.Tileset)
        {
            return reveal is null ? latched
                : throw new InvalidOperationException("X-ray reveals a room whose planes draw from the object character page.");
        }
        RoomScope scope = _room.Scope ?? throw new InvalidOperationException("Room backgrounds were captured with no room loaded.");
        RevealedBlock[]? revealed = null;
        if (reveal is { } words)
        {
            if (_revealIndex is null || _revealIndex.Definitions != rooms.Bg1.Plane.Definitions)
                _revealIndex = new RevealedBlockIndex(rooms.Bg1.Plane.Definitions);
            revealed = XrayRevealPlane.BlocksOf(words.AsSpan(), _revealIndex);
        }
        RoomBlockPictureTable pictures = scope.BlockPictures.Resolve(rooms.Bg1.Plane, rooms.Bg2?.Plane, _backgrounds,
            revealed?.Select(block => block.Definition) ?? []);
        return latched with
        {
            Rooms = rooms with { Bg1 = rooms.Bg1 with { Pictures = pictures }, Bg2 = rooms.Bg2 is { } bg2 ? bg2 with { Pictures = pictures } : null },
            XrayReveal = revealed is null ? null : new XrayRevealPlane(revealed, pictures),
        };
    }

    /// <summary>The ways the X-ray reveal lays out the room's definitions, kept while they stand (#1392).</summary>
    private RevealedBlockIndex? _revealIndex;

    /// <summary>
    /// The BG3 plane this NMI displays (#1348): the HUD's static top row and its three rows as this NMI uploads them,
    /// over the room effect's area. A state import that resumes inside pause takes the plane gameplay would have latched
    /// from it (#1351).
    /// </summary>
    internal Layer3Plane CaptureLayer3Plane()
    {
        var hudRows = new ushort[Layer3Plane.HudRows * Layer3Plane.Columns];
        // Without installed HUD artwork the HUD shows the stock top row, as the bus-sourced upload does.
        ushort[] topRow = _artwork.MapPresentation?.GameplayHud.TopRow() ?? GameplayHudDefinitions.StockTopRow();
        topRow.CopyTo(hudRows, 0);
        _hud.MutableRows.CopyTo(hudRows.AsSpan(Layer3Plane.Columns));
        return Layer3Plane.Compose(hudRows, _room.Layer3Fx?.EffectPageWords() ?? []);
    }

    /// <summary>COLDATA shadows the HDMA pre-instructions wrote during the previous main loop.</summary>
    private GameplayLatchedFixedColors CaptureFixedColors()
    {
        // Before the first room exists there is no explosion; its COLDATA shadow is the cleared one.
        FixedColorRegisterSnapshot powerBomb = _room.BombProjectiles?.PowerBombExplosion is { } explosion
            ? new(explosion.FixedColorRed, explosion.FixedColorGreen, explosion.FixedColorBlue)
            : default;
        return new(
            _roomFx.FixedColors.Snapshot(),
            powerBomb,
            new FixedColorRegisterSnapshot(_suitPickup.FixedColorRed, _suitPickup.FixedColorGreen, _suitPickup.FixedColorBlue),
            Displays?.MotherBrain?.RainbowBeamHdma.Color);
    }

    /// <summary>
    /// The reveal tilemap the X-ray setup calls queue for VRAM upload. It is copied while X-ray
    /// is active; otherwise the displayed BG2 tilemap is the room's own.
    /// </summary>
    private ImmutableArray<ushort>? CaptureXrayRevealTilemap() =>
        _player.Samus?.Xray.IsActive == true
            ? ImmutableArray.Create(_xray.RevealTilemap.ToArray())
            : null;

    /// <summary>
    /// Captures the exact eye-body and HDMA-object values that become visible alongside
    /// this NMI's OAM upload. Retail hard-codes body slot one; retaining the translated
    /// slot reference here also makes corrupt actor ownership fail at publication time.
    /// </summary>
    internal MorphBallEyeBeamRenderSnapshot? CaptureMorphBallEyeBeamForDisplay()
    {
        // Before the first room no eye beam is shown.
        if (_room.Enemies is not { } enemies)
            return null;
        MorphBallEyeBeamState beam = enemies.DisplaySources.MorphBallEyeBeam;
        if (beam.Phase == MorphBallEyeBeamPhase.Inactive)
            return null;

        int bodyIndex = beam.BodySlotIndex;
        if ((uint)bodyIndex >= enemies.SlotTable.Slots.Count ||
            (uint)bodyIndex >= enemies.DisplaySources.MorphBallEyeStates.Count ||
            enemies.DisplaySources.MorphBallEyeStates[bodyIndex] is not { } eyeState)
        {
            throw new InvalidDataException(
                $"Active Morph Ball eye beam names invalid body slot {bodyIndex}.");
        }

        RoomEnemySlot body = enemies.SlotTable.Slots[bodyIndex];
        return new MorphBallEyeBeamRenderSnapshot(
            beam.Phase,
            body.Position.Whole,
            eyeState.Angle,
            beam.AngularWidth,
            checked((byte)beam.Red),
            checked((byte)beam.Green),
            checked((byte)beam.Blue));
    }
}
