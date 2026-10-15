using SuperMetroid.Core.Input;
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
internal sealed class NmiHandler
{
    /// <summary>Publishes rebound HUD, beam, and trail art after accepted VRAM transfers.</summary>
    private readonly RuntimeArtwork _artwork;
    /// <summary>Captures the BG1 and BG2 scroll registers visible with the accepted NMI.</summary>
    private readonly BackgroundScrollState _backgroundScroll;
    /// <summary>Latches remapped controller input only for a main-loop-requested NMI.</summary>
    private readonly ControllerInputState _controller1;
    /// <summary>Retains accepted-NMI PPU, Mode 7, layer-three, and eye-beam snapshots.</summary>
    private readonly GameplayDisplayState _display;
    /// <summary>Last finalized OAM image uploaded by an accepted NMI.</summary>
    private readonly OamBuffer _displayedOam;
    /// <summary>Provides boss-specific display windows and latches Phantoon presentation.</summary>
    private readonly RoomEnemySystem _enemies;
    /// <summary>Window register cache latched with the accepted frame.</summary>
    private readonly GameplayWindowRegisters _gameplayWindows;
    /// <summary>Owns accepted, lagged, and all-NMI frame counters.</summary>
    private readonly NmiClock _nmi;
    /// <summary>Finalized main-loop OAM awaiting the next accepted upload.</summary>
    private readonly OamBuffer _oam;
    /// <summary>Controller bindings used to normalize physical buttons before the shared input latch.</summary>
    private readonly GameplayOptions _options;
    /// <summary>Supplies Samus graphics DMA for accepted NMI transfers.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Captures the layer-three room effect visible at the NMI boundary.</summary>
    private readonly RoomLayer3FxState _roomLayer3Fx;
    /// <summary>Latches the Tourian statue scene's displayed state.</summary>
    private readonly TourianStatueSequence _tourianStatues;
    /// <summary>Receives Samus tile DMA and drained video-queue writes.</summary>
    private readonly SnesVram _vram;
    /// <summary>General video transfer queue drained after Samus graphics DMA.</summary>
    private readonly VramWriteQueue _vramWrites;
    /// <summary>Transfers the X-ray BG1 read at the end of the accepted publication.</summary>
    private readonly XrayReveal _xray;
    /// <summary>Memory bus supplied to Samus DMA and queued video transfers.</summary>
    private readonly ISnesAddressSpace _addressSpace;

    /// <summary>Connects controller, PPU, sprite, video-queue, and frame-clock owners to the NMI boundary.</summary>
    internal NmiHandler(NmiHandlerDependencies dependencies)
    {
        _artwork = dependencies.Artwork;
        _backgroundScroll = dependencies.BackgroundScroll;
        _controller1 = dependencies.Controller1;
        _display = dependencies.Display;
        _displayedOam = dependencies.DisplayedOam;
        _enemies = dependencies.Enemies;
        _gameplayWindows = dependencies.GameplayWindows;
        _nmi = dependencies.Nmi;
        _oam = dependencies.Oam;
        _options = dependencies.Options;
        _player = dependencies.Player;
        _roomLayer3Fx = dependencies.RoomLayer3Fx;
        _tourianStatues = dependencies.TourianStatues;
        _vram = dependencies.Vram;
        _vramWrites = dependencies.VramWrites;
        _xray = dependencies.Xray;
        _addressSpace = dependencies.AddressSpace;
    }

    /// <summary>
    /// Adopts <c>$05B5</c>/<c>$05B6</c> from the frontend that counted accepted NMIs before
    /// this runtime existed. The cartridge has one pair of words; allocation never resets it.
    /// </summary>
    internal void AdoptNmiFrameCounters(byte counter8, ushort counter)
    {
        _nmi.NmiFrameCounter8 = counter8;
        _nmi.NmiFrameCounter = counter;
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
    /// Counts NMIs the cartridge accepts while its main loop is stalled in a hardware wait
    /// ($80:95E7-$95F4): each clears the lag counter and advances <c>$05B5/$05B6</c>.
    /// </summary>
    internal void AcceptHardwareWaitNmis(int count)
    {
        Ensure.AtLeastZero(count);
        if (count == 0)
            return;
        _nmi.NmiLagCounter = 0;
        _nmi.NmiFrameCounter8 = unchecked((byte)(_nmi.NmiFrameCounter8 + count));
        _nmi.NmiFrameCounter = unchecked((ushort)(_nmi.NmiFrameCounter + count));
    }

    /// <summary>Runs the NMI handler portion currently translated from <c>$80:9583</c>.</summary>
    /// <param name="controller1Input">Host-provided raw SNES controller word.</param>
    /// <param name="mainLoopRequestedNmi">
    /// Equivalent of WRAM's NMI request flag. False models a lag NMI: it skips transfers,
    /// input, and accepted-frame counters but still advances the all-NMI counter.
    /// </param>
    public void RunNmi(SnesButton controller1Input, bool mainLoopRequestedNmi)
    {
        if (mainLoopRequestedNmi)
        {
            PublishAcceptedNmiTransfers();
            // Menu code consumes raw physical buttons before a runtime exists. Once room
            // gameplay owns the controller, all bank-$90/$91 action checks use the seven
            // configurable WRAM masks. Canonicalizing here preserves one shared rising-edge
            // latch and prevents different movement subsystems from interpreting a remap on
            // different frames.
            _controller1.Latch(_options.ControllerBindings.Normalize(controller1Input));

            _nmi.NmiLagCounter = 0;
            _nmi.NmiFrameCounter8 = unchecked((byte)(_nmi.NmiFrameCounter8 + 1));
            _nmi.NmiFrameCounter = unchecked((ushort)(_nmi.NmiFrameCounter + 1));
        }
        else
        {
            _nmi.NmiLagCounter = unchecked((ushort)(_nmi.NmiLagCounter + 1));
            if (_nmi.NmiLagCounter >= _nmi.MaximumNmiLag)
                _nmi.MaximumNmiLag = _nmi.NmiLagCounter;
        }

        // $80:95F9 lies after the accepted/lagged branches rejoin, so it always advances.
        _nmi.NmiCounterIncludingLag = unchecked((ushort)(_nmi.NmiCounterIncludingLag + 1));
    }

    /// <summary>
    /// Applies queued graphics while a loading dispatch holds forced blank with NMI
    /// disabled. Native loaders write these directly; no NMI runs, so neither the
    /// controller latch nor any NMI counter changes.
    /// </summary>
    internal void PublishForcedBlankTransfers() => PublishAcceptedNmiTransfers();

    /// <summary>The OAM, Samus, video-queue and register publication of $80:959E..$80:95DE.</summary>
    private void PublishAcceptedNmiTransfers()
    {
        // Preserve $80:95A1 -> $80:95D0 -> $80:95E1 order: dedicated Samus graphics
        // DMA precedes the general video queue, and both precede controller latching.
        // $80:959E uploads the finalized main-loop OAM image immediately before that
        // Samus DMA. `$80:95A7` subsequently copies the Mode 7 shadow words to
        // $211B..$2120. Retain both displayed values so software rendering sees one
        // coherent PPU phase instead of combining old OAM with a newer room-main matrix.
        _displayedOam.CopyFinalizedFrom(_oam);
        _gameplayWindows.Cache.LatchNmi(mainLoopRequestedNmi: true);
        _display.DisplayedSamusMode7Transform = _display.ActiveSamusMode7Transform;
        var bg2Window = _enemies.DraygonBehavior.Draygon is { } boss
            ? DraygonMainScreenWindow.Select(boss.Body.Position.Whole, _backgroundScroll.Layer1Position,
                boss.Body.Properties.HasAny(EnemyProperties.Deleted))
            : (First: 32, End: 224);
        _display.DisplayedGameplayPpu = new GameplayPpuRenderSnapshot(
            _backgroundScroll.Layer1Position,
            _backgroundScroll.Bg1Scroll,
            _backgroundScroll.Bg2Scroll,
            _enemies.Earthquake.LastRoomShake, bg2Window.First, bg2Window.End);
        _display.DisplayedRoomLayer3Fx = _roomLayer3Fx.CaptureForDisplay();
        _enemies.PhantoonBehavior.Phantoon?.Wave.LatchDisplay();
        if (_enemies.PhantoonBehavior.Phantoon is { } phantoonDisplay)
            phantoonDisplay.Blending.LatchDisplay(phantoonDisplay.MosaicRegister);
        _tourianStatues.LatchDisplay();
        _display.DisplayedMorphBallEyeBeam = CaptureMorphBallEyeBeamForDisplay();
        _player.Samus?.Presentation.TileTransfers.TransferToVram(_addressSpace, _vram);
        _artwork.PublishReboundHudArtwork();
        _vramWrites.DrainTo(_vram, RuntimeMemory.RequireBus(_addressSpace), _artwork);
        // A rebound snapshot may retain old VRAM and legacy queued transfers.
        // Apply current content only at this accepted NMI, after those writes.
        _artwork.PublishReboundBeamArtwork();
        _artwork.PublishReboundTrailArtwork();
        _xray.TransferXrayBg1Read();
    }

    /// <summary>
    /// Captures the exact eye-body and HDMA-object values that become visible alongside
    /// this NMI's OAM upload. Retail hard-codes body slot one; retaining the translated
    /// slot reference here also makes corrupt actor ownership fail at publication time.
    /// </summary>
    internal MorphBallEyeBeamRenderSnapshot? CaptureMorphBallEyeBeamForDisplay()
    {
        MorphBallEyeBeamState beam = _enemies.MorphBallEyeBehavior.MorphBallEyeBeam;
        if (beam.Phase == MorphBallEyeBeamPhase.Inactive)
            return null;

        int bodyIndex = beam.BodySlotIndex;
        if ((uint)bodyIndex >= _enemies.SlotTable.Slots.Count ||
            (uint)bodyIndex >= _enemies.MorphBallEyeBehavior.MorphBallEyeStates.Count ||
            _enemies.MorphBallEyeBehavior.MorphBallEyeStates[bodyIndex] is not { } eyeState)
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
            beam.Red,
            beam.Green,
            beam.Blue);
    }
}
