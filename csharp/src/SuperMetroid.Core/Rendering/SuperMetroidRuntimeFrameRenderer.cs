using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Selects the PPU compositor required by the room and door currently owned by a runtime.
/// </summary>
/// <remarks>
/// This decision used to live in the debug executable, which made the standalone game and
/// visual tests vulnerable to choosing a different rendering path. Keeping it beside the
/// renderers makes every caller honor the cartridge door setup in exactly the same way.
/// </remarks>
public static class SuperMetroidRuntimeFrameRenderer
{
    /// <summary>Renders the runtime's last NMI-published OAM and current room PPU state.</summary>
    public static Rgba32[] Render(SuperMetroidRuntime runtime)
    {
        ArgumentNullException.ThrowIfNull(runtime);
        if (runtime.ActiveDoor is null)
            throw new InvalidOperationException("A cartridge room and door must be loaded before rendering gameplay.");
        // X-ray needs layer/palette identity until the final color-math operation.
        // Both retained GPU packets and immediate software rendering consume that
        // capture; a post-composition tint cannot implement its native windows.
        if (runtime.Enemies.Phantoon is not null || runtime.Enemies.MotherBrain is not null || runtime.RoomLayer3Fx.Type is RoomFxType.Fireflea or RoomFxType.Spores || runtime.Samus?.Xray.IsActive == true || runtime.LastXrayBeamStep is { Completed: true, PhaseAtStart: XrayBeamPhase.Finish })
            return SoftwareLayeredSnapshotRenderer.Render(GameplayDisplayCapture.TryCaptureFrame(runtime)!);

        GameplayPpuRenderSnapshot displayedPpu = runtime.DisplayedGameplayPpu;
        RoomShakeFrameResult shake = displayedPpu.RoomShake;
        // Room shake is added to the latched BG1/BG2 scroll registers.
        Position16<RoomPixels> bg1Scroll = displayedPpu.Bg1Scroll + shake.Bg1;
        Position16<Bg2Pixels> bg2Scroll = displayedPpu.Bg2Scroll + shake.Bg2;
        RoomLayer3FxRenderSnapshot? displayedRoomFx = runtime.DisplayedRoomLayer3Fx;

        Rgba32[] frame;
        if (runtime.Enemies.CeresRidley is { Mode7Active: true } getaway)
        {
            // Ceres room main $A6:AAAF temporarily replaces ordinary mode-nine BG1/BG2
            // below the HUD with the rotating Ridley/Baby Mode-7 map. OAM stays live, so
            // Samus and the timer retain the usual gameplay compositor and priority rules.
            frame = SnesGameplayFrameRenderer.RenderHudCeresRidleyGetawayAndObjs(
                runtime.Vram,
                runtime.Cgram,
                runtime.DisplayedOam,
                matrixA: unchecked((short)getaway.Mode7MatrixA),
                matrixB: unchecked((short)getaway.Mode7MatrixB),
                matrixC: unchecked((short)getaway.Mode7MatrixC),
                matrixD: unchecked((short)getaway.Mode7MatrixD),
                center: getaway.Mode7Center.AsSignedRegister(),
                scroll: new Offset16<Mode7Pixels>(unchecked((short)(getaway.Mode7Offset.X + shake.Bg1.X)), unchecked((short)(getaway.Mode7Offset.Y + shake.Bg1.Y))),
                bg2Scroll: bg2Scroll,
                // Setup ASM $8F:C97B writes BG12NBA=$66; the floor slice returns to
                // Mode 1 and therefore consumes the same $6000 character base as BG2 did
                // before the getaway HDMA tables changed BGMODE.
                bg2CharacterBaseWord: 0x6000,
                outputBuffer: runtime.GameplayFrameBuffer);
        }
        else if (runtime.ActiveDoor.UsesCeresElevatorMode7)
        {
            // DoorCode_CeresElevatorShaft `$8F:E4E0` installs the identity transform, but
            // room main `$89:ACC3` then replaces A/B/C/D from its rotation table throughout
            // the escape. NMI `$80:95A7` publishes those shadow words to $211B..$2120.
            // Samus's OAM origin is calculated through the paired inverse transform in
            // `$8B:8A52`; feeding identity to BG1 here made the visible platforms diverge
            // from both her sprite and the deliberately unrotated physical collision map.
            SamusMode7Transform mode7 = runtime.DisplayedSamusMode7Transform
                ?? throw new InvalidOperationException(
                    "The Ceres elevator shaft has no NMI-published Mode 7 matrix.");
            frame = SnesGameplayFrameRenderer.RenderHudMode7AndObjs(
                runtime.Vram,
                runtime.Cgram,
                runtime.DisplayedOam,
                matrixA: unchecked((short)mode7.MatrixA),
                matrixB: unchecked((short)mode7.MatrixB),
                matrixC: unchecked((short)mode7.MatrixC),
                matrixD: unchecked((short)mode7.MatrixA),
                center: mode7.Center.AsMode7Register(),
                // Mode 7 reuses BG1HOFS/BG1VOFS as M7HOFS/M7VOFS. MainScrollingRoutine
                // continues publishing those live camera words even though its ordinary
                // tilemap row/column producers return early while `$0783` is nonzero.
                // Leaving these at zero pins the shaft art to the window while Samus,
                // enemies, and projectiles correctly move relative to the camera.
                scroll: new Offset16<Mode7Pixels>(unchecked((short)bg1Scroll.X), unchecked((short)bg1Scroll.Y)),
                outputBuffer: runtime.GameplayFrameBuffer);
        }
        else
        {
            // Setup ASM $8F:C97B writes BG12NBA=$66 for the Ceres Ridley arena. Each
            // register nibble is a $1000-word character-base selector, so both BG1 and
            // BG2 deliberately reuse VRAM $6000. This is not a Ridley-art special case in
            // the compositor: it is the literal PPU register value selected by the room.
            // Other translated ordinary-room setup routines retain power-on base zero.
            bool usesCeresRidleyCharacterBase =
                runtime.ActiveRoom?.State.SetupCallback ==
                    RoomSetupCallback.SetCeresRidleyBgCharacterBaseAndSpawnHaze;
            ushort bgCharacterBaseWord = usesCeresRidleyCharacterBase
                ? (ushort)0x6000
                : (ushort)0;
            bool crocomireOwnsBg2 = runtime.Enemies.Crocomire is not null;
            KraidEnemyState? kraid = runtime.Enemies.Kraid;
            bool kraidOwnsBg2 = kraid is { OwnsBg2Tilemap: true };
            ScrollingSkyState? scrollingSky = runtime.ScrollingSky;
            ushort[]? skyHorizontalScrolls = scrollingSky is { HorizontalHdmaConfigured: true } ? scrollingSky.BuildGameplayHorizontalScrolls(
                runtime.Camera?.PixelPosition.Y
                    ?? throw new InvalidOperationException("Scrolling sky has no gameplay camera.")) : null;
            if (skyHorizontalScrolls is not null && shake.Bg2.X != 0)
            {
                // Room shake is added to the live BG2HOFS register after HDMA supplies
                // each band value. Apply it to every resolved scanline for the same final
                // PPU coordinates instead of inventing a second sky-motion accumulator.
                for (int line = 0; line < skyHorizontalScrolls.Length; line++)
                    skyHorizontalScrolls[line] = AddShake(skyHorizontalScrolls[line], shake.Bg2.X);
            }
            ushort[]? waterBg2HorizontalScrolls = displayedRoomFx is { } waterFx
                ? SnesGameplayFrameRenderer.BuildWaterBg2HorizontalScrolls(
                    waterFx,
                    bg2Scroll)
                : null;
            ushort[]? lavaAcidBg2HorizontalScrolls = displayedRoomFx is { } horizontalFx
                ? SnesGameplayFrameRenderer.BuildLavaAcidBg2HorizontalScrolls(
                    horizontalFx,
                    bg2Scroll)
                : null;
            ushort[]? lavaAcidBg2VerticalScrolls = displayedRoomFx is { } verticalFx
                ? SnesGameplayFrameRenderer.BuildLavaAcidBg2VerticalScrolls(
                    verticalFx,
                    bg2Scroll.Y)
                : null;
            frame = SnesGameplayFrameRenderer.RenderHudOrdinaryBackgroundsAndObjs(
                runtime.Vram,
                runtime.Cgram,
                runtime.DisplayedOam,
                bg1Scroll,
                new Position16<Bg2Pixels>(
                    kraidOwnsBg2
                        ? AddShake(kraid!.Bg2Scroll.X, shake.Bg2.X)
                        : crocomireOwnsBg2
                        ? AddShake(runtime.Enemies.CrocomireBg2Scroll.X, shake.Bg2.X)
                        : bg2Scroll.X,
                    kraidOwnsBg2
                        ? AddShake(kraid!.Bg2Scroll.Y, shake.Bg2.Y)
                        : crocomireOwnsBg2
                        ? AddShake(runtime.Enemies.CrocomireBg2Scroll.Y, shake.Bg2.Y)
                        : scrollingSky is not null
                            ? AddShake(scrollingSky.VerticalScroll, shake.Bg2.Y)
                            : runtime.TourianStatues.Enabled
                                ? unchecked((ushort)(displayedPpu.Layer1Position.Y + runtime.TourianStatues.DisplayedVerticalOffset + shake.Bg2.Y))
                                : bg2Scroll.Y),
                bg2HorizontalScrollByLine:
                    runtime.Enemies.Phantoon?.Wave.DisplayedScrolls ?? lavaAcidBg2HorizontalScrolls ?? waterBg2HorizontalScrolls ?? skyHorizontalScrolls,
                bg2VerticalScrollByLine: crocomireOwnsBg2
                    ? runtime.Enemies.CrocomireDeath is { MeltingHdmaActive: true } melting
                        ? melting.Bg2ScrollByScanline : null
                    : lavaAcidBg2VerticalScrolls,
                bg2TilemapSize: kraidOwnsBg2
                    ? new Extent16<TilemapCells>(KraidBackgroundRomData.TilemapWidthInTiles, KraidBackgroundRomData.TilemapHeightInTiles)
                    : runtime.Enemies.MotherBrain is { HasBg2ScrollOverride: true } ? new Extent16<TilemapCells>(32, 32)
                    : scrollingSky?.HorizontalHdmaConfigured != true && runtime.RoomLayer3Fx.Type != RoomFxType.TourianEntranceStatue
                        ? new Extent16<TilemapCells>(64, 32)
                        : new Extent16<TilemapCells>(32, 64),
                bg2TilemapBaseWord: kraidOwnsBg2
                    ? KraidBackgroundRomData.LiveBg2TilemapWord
                    : GameplayVramLayout.Bg2TilemapWord,
                bg1CharacterBaseWord: bgCharacterBaseWord,
                bg2CharacterBaseWord: bgCharacterBaseWord,
                bg3CharacterBaseWord: runtime.GameplayHudCharacterBaseWord,
                mainScreenLayers: runtime.DoorTransitionMainScreenLayers ??
                    (SnesMainScreenLayers.Bg1 |
                     SnesMainScreenLayers.Bg2 |
                     SnesMainScreenLayers.Obj),
                bg2FirstScanline: runtime.DisplayedGameplayPpu.Bg2FirstScanline,
                bg2EndScanline: runtime.DisplayedGameplayPpu.Bg2EndScanline,
                mainScreenLayersByLine: runtime.DoorTransitionMainScreenLayers is null && runtime.Enemies.MotherBrain?.RisingHdmaActive == true
                    ? MotherBrainAscentDisplayDefinitions.BuildGameplayLayers() : default,
                outputBuffer: runtime.GameplayFrameBuffer);
        }

        bool doorIrqOwnsDisplay = runtime.DoorTransitionMainScreenLayers is not null;
        if (!doorIrqOwnsDisplay && runtime.DisplayedRoomLayer3Fx is { } layer3Fx)
            SnesGameplayFrameRenderer.ApplyRoomLayer3FxColorMath(frame, runtime.Vram, runtime.Cgram, layer3Fx);

        // These are the three setup routines that explicitly call FXType_2C_CeresHaze.
        // Keying the effect from cartridge state avoids applying a guessed “Ceres tint” to
        // scenes that do not spawn the HDMA object.
        if (!doorIrqOwnsDisplay && runtime.CeresHaze.Enabled)
        {
            // The HDMA object retains its spawn-time channel while the boss bit changes.
            SnesGameplayFrameRenderer.ApplyCeresHaze(frame, runtime.CeresHaze.IsRed,
                runtime.CeresHaze.Intensity, runtime.MapPresentation?.RoomFxPaletteBlends);
        }

        if (!doorIrqOwnsDisplay && runtime.DisplayedMorphBallEyeBeam is { } eyeBeam)
        {
            SnesGameplayFrameRenderer.ApplyMorphBallEyeBeamColorMath(
                frame,
                runtime.AddressSpace,
                eyeBeam,
                displayedPpu.Layer1Position);
        }

        // The bank-$88 Power Bomb objects own a final PPU color-math window, not OAM or
        // either room background. Every gameplay presentation therefore has to apply the
        // shared window after its room-specific layers have been composed. The Room Viewer
        // already called this compositor directly; omitting it here let the real desktop
        // advance the complete damaging radius while showing no explosion at all.
        SnesGameplayFrameRenderer.ApplyPowerBombColorMath(
            frame,
            runtime.AddressSpace,
            runtime.BombProjectiles.PowerBombExplosion,
            displayedPpu.Layer1Position);

        // Bank $85 temporarily owns BG3 and disables gameplay color math while an item
        // message is active. Its scanline-window result belongs above every room-specific
        // compositor and haze path, so one shared overlay covers ordinary and Mode-7 rooms.
        GameplayMessageBoxRenderer.Composite(
            frame,
            runtime.MessageBox,
            runtime.Vram,
            runtime.Cgram);

        // The suit transformation begins only after its bank-$85 message has completely
        // closed. Its bank-$88 window then overlays every ordinary/Mode-7 room through the
        // same PPU fixed-color path, so it belongs after room-specific haze as well.
        SamusSuitPickupRenderer.Composite(frame, runtime.SuitPickup);

        return frame;
    }

    private static ushort AddShake(ushort scroll, short displacement) =>
        unchecked((ushort)(scroll + displacement));
}
