using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Rendering;

/// <summary>Simulation-side resolution of live room state into owned PPU composition inputs.</summary>
public static partial class GameplayDisplayCapture
{
    /// <summary>
    /// Captures the full currently modeled gameplay composition, including mixed video-mode bands.
    /// </summary>
    public static LayeredRenderSnapshot? TryCaptureFrame(SuperMetroidRuntime runtime)
    {
        ArgumentNullException.ThrowIfNull(runtime);
        LayeredRenderSnapshot basis = runtime.ActiveDoor?.UsesCeresElevatorMode7 == true || runtime.Enemies.CeresRidley is { Mode7Active: true }
            ? CaptureMode7Base(runtime) : CaptureOrdinaryBase(runtime);
        var layers = new List<RenderLayer>(basis.Layers.ToArray());
        GameplayPpuRenderSnapshot ppu = runtime.DisplayedGameplayPpu;
        bool doorOwnsDisplay = runtime.DoorTransitionMainScreenLayers is not null;
        if (!doorOwnsDisplay && CaptureXray(runtime, basis) is { } xray) return xray;
        if (!doorOwnsDisplay && runtime.RoomLayer3Fx.Type == RoomFxType.Fireflea)
            layers[0] = CaptureFirefleaDarkness(runtime, basis);
        if (!doorOwnsDisplay)
        {
            // Rainbow configuration $24 removes BG3 from the main screen. Its fixed
            // color window replaces room liquid/fog blending while the beam owns HDMA.
            if (runtime.Enemies.MotherBrain?.RainbowBeamHdma.Active != true && runtime.DisplayedRoomLayer3Fx is { } fx)
                AddLayer(SnesGameplayFrameRenderer.CaptureRoomLayer3Fx(fx));
            if (runtime.CeresHaze.Enabled)
                AddLayer(SnesGameplayFrameRenderer.CaptureCeresHaze(runtime.CeresHaze.IsRed, runtime.CeresHaze.Intensity));
            if (runtime.DisplayedMorphBallEyeBeam is { } eye)
                AddLayer(SnesGameplayFrameRenderer.CaptureMorphBallEyeBeam(runtime.AddressSpace, eye, ppu.Layer1XPosition, ppu.Layer1YPosition));
            if (runtime.Enemies.MotherBrain is { } motherBrain)
                AddLayer(SnesGameplayFrameRenderer.CaptureMotherBrainRainbowBeam(motherBrain.RainbowBeamHdma));
        }
        // Preserve the reference ordering, including the effects which remain enabled
        // during the door IRQ. Message glyphs precede the suit's final color window.
        AddLayer(SnesGameplayFrameRenderer.CapturePowerBombColorMath(runtime.AddressSpace,
            runtime.BombProjectiles.PowerBombExplosion, ppu.Layer1XPosition, ppu.Layer1YPosition));
        AddLayer(GameplayMessageBoxRenderer.Capture(runtime.MessageBox));
        AddLayer(SamusSuitPickupRenderer.Capture(runtime.SuitPickup));
        return new(basis.Memory, layers.ToArray(), basis.ObjectSelection, basis.Brightness);

        void AddLayer(RenderLayer? layer) { if (layer is not null) layers.Add(layer); }
    }

    /// <summary>
    /// Captures the ordinary Mode-1 base only. Room color math, windows, messages and
    /// suit effects are deliberately excluded; this is not a substitute for a complete frame.
    /// Mode-7 rooms are rejected rather than silently interpreting their memory as tiles.
    /// </summary>
    public static LayeredRenderSnapshot CaptureOrdinaryBase(SuperMetroidRuntime runtime)
    {
        ArgumentNullException.ThrowIfNull(runtime);
        if (runtime.ActiveDoor is null)
            throw new InvalidOperationException("A cartridge room and door must be loaded before capture.");
        if (runtime.ActiveDoor.UsesCeresElevatorMode7 || runtime.Enemies.CeresRidley is { Mode7Active: true })
            throw new InvalidOperationException("Ordinary base capture cannot represent a Mode-7 scene.");

        GameplayPpuRenderSnapshot ppu = runtime.DisplayedGameplayPpu;
        RoomShakeFrameResult shake = ppu.RoomShake;
        ushort bg1X = Add(ppu.Bg1HorizontalScroll, shake.Bg1X);
        ushort bg1Y = Add(ppu.Bg1VerticalScroll, shake.Bg1Y);
        ushort bg2X = Add(ppu.Bg2HorizontalScroll, shake.Bg2X);
        ushort bg2Y = Add(ppu.Bg2VerticalScroll, shake.Bg2Y);
        if (runtime.TourianStatues.Enabled)
            bg2Y = Add(unchecked((ushort)(ppu.Layer1YPosition + runtime.TourianStatues.DisplayedVerticalOffset)), shake.Bg2Y);
        RoomLayer3FxRenderSnapshot? fx = runtime.DisplayedRoomLayer3Fx;
        ScrollingSkyState? sky = runtime.ScrollingSky;
        ushort[]? skyX = sky?.BuildGameplayHorizontalScrolls(runtime.Camera?.YPosition
            ?? throw new InvalidOperationException("Scrolling sky has no gameplay camera."));
        if (skyX is not null)
            for (int line = 0; line < skyX.Length; line++) skyX[line] = Add(skyX[line], shake.Bg2X);

        ushort[]? waterX = fx is { } water
            ? SnesGameplayFrameRenderer.BuildWaterBg2HorizontalScrolls(water, bg2X, bg2Y) : null;
        ushort[]? lavaX = fx is { } lava
            ? SnesGameplayFrameRenderer.BuildLavaAcidBg2HorizontalScrolls(lava, bg2X, bg2Y) : null;
        ushort[]? lavaY = fx is { } vertical
            ? SnesGameplayFrameRenderer.BuildLavaAcidBg2VerticalScrolls(vertical, bg2Y) : null;
        KraidEnemyState? kraid = runtime.Enemies.Kraid;
        bool kraidBg = kraid is { OwnsBg2Tilemap: true };
        bool crocomireBg = runtime.Enemies.Crocomire is not null;
        bool verticalStatueMap = runtime.RoomLayer3Fx.Type == RoomFxType.TourianEntranceStatue;
        ushort character = runtime.ActiveRoom?.State.SetupCodePointer ==
            RoomSetupCodePointers.SetCeresRidleyBgCharacterBaseAndSpawnHaze
            ? GameplayRenderDefinitions.CeresCharacterWord : (ushort)0;
        var registers = new OrdinaryGameplayRegisters(bg1X, bg1Y,
            kraidBg ? Add(kraid!.Bg2HorizontalScroll, shake.Bg2X)
                : crocomireBg ? Add(runtime.Enemies.CrocomireBg2HorizontalScroll, shake.Bg2X) : bg2X,
            kraidBg ? Add(kraid!.Bg2VerticalScroll, shake.Bg2Y)
                : crocomireBg ? Add(runtime.Enemies.CrocomireBg2VerticalScroll, shake.Bg2Y)
                : sky is not null ? Add(sky.VerticalScroll, shake.Bg2Y) : bg2Y,
            kraidBg ? KraidBackgroundRomData.TilemapWidthInTiles : runtime.Enemies.MotherBrain is { HasBg2ScrollOverride: true } ? 32 : sky is null && !verticalStatueMap ? 64 : 32,
            kraidBg ? KraidBackgroundRomData.TilemapHeightInTiles : runtime.Enemies.MotherBrain is { HasBg2ScrollOverride: true } ? 32 : sky is null && !verticalStatueMap ? 32 : 64,
            kraidBg ? KraidBackgroundRomData.LiveBg2TilemapWord : SnesPpuLayout.GameplayBg2TilemapWord,
            character, character, runtime.GameplayHudCharacterBaseWord,
            runtime.DoorTransitionMainScreenLayers ??
                (SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Obj |
                    (runtime.Enemies.MotherBrain?.DeathBg2Hidden == true ? 0 : SnesMainScreenLayers.Bg2)),
            ppu.Bg2FirstScanline, ppu.Bg2EndScanline);
        // Producers can return longer HDMA storage; the renderer consumes only the
        // gameplay region. Own exactly those visible register values in the packet.
        var layer = new OrdinaryGameplayRenderLayer(registers,
            VisibleLines(lavaX ?? waterX ?? skyX),
            VisibleLines(crocomireBg
                ? runtime.Enemies.CrocomireDeath is { MeltingHdmaActive: true } melting
                    ? melting.Bg2ScrollByScanline : null
                : lavaY));
        return new(PpuMemorySnapshot.Capture(runtime.Vram, runtime.Cgram, runtime.DisplayedOam),
            new RenderLayer[] { layer }, GameplayRenderDefinitions.ObjectSelection,
            SnesPpuLayout.MaximumMasterBrightness);
    }

    private static ushort[] VisibleLines(IReadOnlyList<ushort>? values)
    {
        if (values is null) return [];
        int count = SnesPpuLayout.ScreenHeightPixels - SnesPpuLayout.GameplayHudHeightPixels;
        if (values.Count < count) throw new InvalidDataException("Published HDMA does not cover the gameplay viewport.");
        var result = new ushort[count];
        for (int i = 0; i < count; i++) result[i] = values[i];
        return result;
    }

    private static ushort Add(ushort scroll, short shake) => unchecked((ushort)(scroll + shake));
}
