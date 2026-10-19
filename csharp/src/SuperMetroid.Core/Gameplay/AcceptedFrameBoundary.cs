using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus.Projectiles;
using System.Collections.Immutable;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// The NMI: controller latching, frame-counter advance, accepted and forced-blank transfers, hardware-wait NMIs,
/// and the display captures published at the NMI.
/// </summary>
internal sealed class AcceptedFrameBoundary
{
    /// <summary>Publishes rebound HUD, beam, and trail art after accepted VRAM transfers.</summary>
    private readonly RuntimeArtwork _artwork;
    /// <summary>Captures the BG1 and BG2 scroll registers visible with the accepted NMI.</summary>
    private readonly BackgroundScrollState _backgroundScroll;
    /// <summary>Latches remapped controller input only for a main-loop-requested NMI.</summary>
    private readonly ControllerInputState _controller1;
    /// <summary>Physical controller-1 latch, for code that reads raw <c>$8B/$8F</c> bits.</summary>
    private readonly ControllerInputState _physicalController1;
    /// <summary>Supplies the power-bomb explosion whose COLDATA color is latched.</summary>
    private readonly SamusBombProjectileSystem _bombProjectiles;
    /// <summary>Retains the accepted-NMI display latch.</summary>
    private readonly GameplayDisplayState _display;
    /// <summary>Sprite frame made visible at each accepted NMI.</summary>
    private readonly SpriteFrameBuffer _displayedSprites;
    /// <summary>Provides boss-specific display windows and latches Phantoon presentation.</summary>
    private readonly RoomEnemySystem _enemies;
    /// <summary>Window register cache latched with the accepted frame.</summary>
    private readonly GameplayWindowRegisters _gameplayWindows;
    /// <summary>Owns accepted, lagged, and all-NMI frame counters.</summary>
    private readonly FrameClock _clock;
    /// <summary>Sprite frame the main loop finalized for the next NMI.</summary>
    private readonly SpriteFrameBuffer _sprites;
    /// <summary>Controller bindings used to normalize physical buttons before the shared input latch.</summary>
    private readonly GameplayOptions _options;
    /// <summary>Supplies Samus graphics DMA for accepted NMI transfers.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Captures the layer-three room effect visible at the NMI boundary.</summary>
    private readonly RoomLayer3FxState _roomLayer3Fx;
    /// <summary>Supplies the loaded room, door, and scrolling sky whose register shadows are latched.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Supplies the suit pickup whose COLDATA color is latched.</summary>
    private readonly SamusSuitPickupState _suitPickup;
    /// <summary>Latches the Tourian statue scene's displayed state.</summary>
    private readonly TourianStatueSequence _tourianStatues;
    /// <summary>Receives Samus tile DMA and drained video-queue writes.</summary>
    private readonly SnesVram _vram;
    /// <summary>Transfers the X-ray BG1 read at the end of the accepted publication.</summary>
    private readonly XrayReveal _xray;
    /// <summary>Executes the queued video transfers, whose address-sourced entries read live memory.</summary>
    private readonly Action _drainQueuedVramWrites;

    /// <summary>Connects controller, PPU, sprite, video-queue, and frame-clock owners to the NMI boundary.</summary>
    internal AcceptedFrameBoundary(AcceptedFrameBoundaryDependencies dependencies)
    {
        _artwork = dependencies.Artwork;
        _backgroundScroll = dependencies.BackgroundScroll;
        _bombProjectiles = dependencies.BombProjectiles;
        _controller1 = dependencies.Controller1;
        _physicalController1 = dependencies.PhysicalController1;
        _display = dependencies.Display;
        _displayedSprites = dependencies.DisplayedSprites;
        _enemies = dependencies.Enemies;
        _gameplayWindows = dependencies.GameplayWindows;
        _clock = dependencies.Clock;
        _sprites = dependencies.Sprites;
        _options = dependencies.Options;
        _player = dependencies.Player;
        _room = dependencies.Room;
        _roomLayer3Fx = dependencies.RoomLayer3Fx;
        _suitPickup = dependencies.SuitPickup;
        _tourianStatues = dependencies.TourianStatues;
        _vram = dependencies.Vram;
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
        // Bank-$90/$91 action checks use the seven configurable WRAM masks. Canonicalizing
        // here preserves one shared rising-edge latch and prevents different movement
        // subsystems from interpreting a remap on different frames. Code that tests fixed
        // bits of $8B/$8F instead (the pause menu's L/R and A) reads the physical latch.
        _physicalController1.Latch(controller1Input);
        _controller1.Latch(_options.ControllerBindings.Normalize(controller1Input));

        _clock.AdvanceAcceptedFrame();
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
        var bg2Window = _enemies.DisplaySources.Draygon is { } boss
            ? DraygonMainScreenWindow.Select(boss.Body.Position.Whole, _backgroundScroll.Layer1Position,
                boss.Body.Properties.HasAny(EnemyProperties.Deleted))
            : (First: 32, End: 224);
        var ppu = new GameplayPpuRenderSnapshot(
            _backgroundScroll.Layer1Position,
            _backgroundScroll.Bg1Scroll,
            _backgroundScroll.Bg2Scroll,
            _enemies.Earthquake.LastRoomShake, bg2Window.First, bg2Window.End);
        RoomLayer3FxRenderSnapshot? roomLayer3Fx = _roomLayer3Fx.CaptureForDisplay();
        _enemies.DisplaySources.Phantoon?.Wave.LatchDisplay();
        if (_enemies.DisplaySources.Phantoon is { } phantoonDisplay)
            phantoonDisplay.Blending.LatchDisplay(phantoonDisplay.MosaicBlockSize);
        _tourianStatues.LatchDisplay();
        _display.Latched = new GameplayNmiDisplayLatch(
            ppu,
            _gameplayWindows.DisplayedGameplayWindowRegisters,
            _display.ActiveSamusMode7Transform,
            roomLayer3Fx,
            CaptureMorphBallEyeBeamForDisplay(),
            _enemies.DisplaySources.Phantoon is { } phantoon ? PhantoonDisplaySnapshot.Capture(phantoon) : null,
            _tourianStatues.DisplayedVerticalOffset,
            CaptureSceneRegisters(),
            CaptureBossBackgrounds(),
            CaptureFixedColors(),
            CaptureXrayRevealTilemap());
        _artwork.PublishReboundHudArtwork();
        _drainQueuedVramWrites();
        // The displayed enemy-format draws show the OBJ characters this frame's uploads bound, as the OAM it latched
        // reads the VRAM those uploads wrote.
        if (_displayedSprites.Shows(SpriteArtworkKind.GameplayObjects))
            _displayedSprites.RebindArtwork(SpriteArtworkKind.GameplayObjects, _artwork.GameplayObjectPage(_enemies.ObjCharacters));
        // A rebound snapshot may retain old CGRAM and legacy queued transfers.
        // Apply current content only at this accepted NMI, after those writes.
        _artwork.PublishReboundBeamPalette();
        _xray.TransferXrayBg1Read();
    }

    /// <summary>
    /// Room-level register shadows uploaded by <c>$80:91EE</c>: BGMODE (Ceres elevator Mode 7),
    /// BG12NBA (setup ASM <c>$8F:C97B</c> selects word $6000 for the Ceres Ridley arena), BG34NBA,
    /// the layer-blending selectors behind TM/TS/CGWSEL/CGADSUB (<c>$88:8000</c>), and the sky's
    /// BG2SC and BG2VOFS mirror (<c>$88:AFB2</c>).
    /// </summary>
    private GameplayLatchedSceneRegisters CaptureSceneRegisters()
    {
        var room = _room.ActiveRoom;
        return new(
            _room.ActiveDoor?.UsesCeresElevatorMode7 == true,
            room?.State.SetupCallback == RoomSetupCallback.SetCeresRidleyBgCharacterBaseAndSpawnHaze,
            _display.GameplayHudCharacterBaseWord,
            _roomLayer3Fx.Type,
            _roomLayer3Fx.LayerBlendConfiguration,
            ScrollingSkyRegisterSnapshot.Capture(_room.ScrollingSky),
            room?.Definition,
            _enemies.RoomStatus.BossId);
    }

    /// <summary>
    /// Boss BG register shadows: Ceres Ridley's M7A..M7Y (written by room main <c>$A6:AAAF</c>,
    /// uploaded with BGMODE's fake Mode 7 HDMA byte by <c>$80:91EE</c>), Kraid's and Crocomire's
    /// BG2SC/BG2HOFS/BG2VOFS, and Mother Brain's BG2SC and TM BG2 bit.
    /// </summary>
    private GameplayLatchedBossBackgrounds CaptureBossBackgrounds()
    {
        CeresRidleyMode7Snapshot? ridley = CeresRidleyMode7Snapshot.Capture(_enemies.DisplaySources.CeresRidley);
        var motherBrain = _enemies.DisplaySources.MotherBrain;
        return new(
            ridley,
            _enemies.DisplaySources.Kraid is { OwnsBg2Tilemap: true } kraid ? kraid.Bg2Scroll : null,
            _enemies.DisplaySources.CrocomireBg2Scroll,
            motherBrain is { HasBg2ScrollOverride: true },
            motherBrain is { DeathBg2Hidden: true });
    }

    /// <summary>COLDATA shadows the HDMA pre-instructions wrote during the previous main loop.</summary>
    private GameplayLatchedFixedColors CaptureFixedColors()
    {
        SamusPowerBombExplosionState explosion = _bombProjectiles.PowerBombExplosion;
        return new(
            _roomLayer3Fx.FixedColors.Snapshot(),
            new FixedColorRegisterSnapshot(explosion.FixedColorRed, explosion.FixedColorGreen, explosion.FixedColorBlue),
            new FixedColorRegisterSnapshot(_suitPickup.FixedColorRed, _suitPickup.FixedColorGreen, _suitPickup.FixedColorBlue),
            _enemies.DisplaySources.MotherBrain?.RainbowBeamHdma.Color);
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
        MorphBallEyeBeamState beam = _enemies.DisplaySources.MorphBallEyeBeam;
        if (beam.Phase == MorphBallEyeBeamPhase.Inactive)
            return null;

        int bodyIndex = beam.BodySlotIndex;
        if ((uint)bodyIndex >= _enemies.SlotTable.Slots.Count ||
            (uint)bodyIndex >= _enemies.DisplaySources.MorphBallEyeStates.Count ||
            _enemies.DisplaySources.MorphBallEyeStates[bodyIndex] is not { } eyeState)
        {
            throw new InvalidDataException(
                $"Active Morph Ball eye beam names invalid body slot {bodyIndex}.");
        }

        RoomEnemySlot body = _enemies.SlotTable.Slots[bodyIndex];
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
