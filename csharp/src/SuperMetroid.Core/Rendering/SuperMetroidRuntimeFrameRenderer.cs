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
        if (runtime.RoomLayer3Fx.Type == RoomFxType.Fireflea || runtime.Samus?.Xray.IsActive == true || runtime.LastXrayBeamStep is { Completed: true, PhaseAtStart: XrayBeamPhase.Finish })
            return SoftwareLayeredSnapshotRenderer.Render(GameplayDisplayCapture.TryCaptureFrame(runtime)!);

        GameplayPpuRenderSnapshot displayedPpu = runtime.DisplayedGameplayPpu;
        RoomShakeFrameResult shake = displayedPpu.RoomShake;
        ushort bg1HorizontalScroll = AddShake(
            displayedPpu.Bg1HorizontalScroll,
            shake.Bg1X);
        ushort bg1VerticalScroll = AddShake(
            displayedPpu.Bg1VerticalScroll,
            shake.Bg1Y);
        ushort bg2HorizontalScroll = AddShake(
            displayedPpu.Bg2HorizontalScroll,
            shake.Bg2X);
        ushort bg2VerticalScroll = AddShake(
            displayedPpu.Bg2VerticalScroll,
            shake.Bg2Y);
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
                centerX: unchecked((short)getaway.Mode7CenterX),
                centerY: unchecked((short)getaway.Mode7CenterY),
                horizontalOffset: unchecked((short)(getaway.Mode7HorizontalOffset + shake.Bg1X)),
                verticalOffset: unchecked((short)(getaway.Mode7VerticalOffset + shake.Bg1Y)),
                bg2HorizontalScroll: bg2HorizontalScroll,
                bg2VerticalScroll: bg2VerticalScroll,
                // Setup ASM $8F:C97B writes BG12NBA=$66; the floor slice returns to
                // Mode 1 and therefore consumes the same $6000 character base as BG2 did
                // before the getaway HDMA tables changed BGMODE.
                bg2CharacterBaseWord: 0x6000);
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
                centerX: unchecked((short)mode7.CenterX),
                centerY: unchecked((short)mode7.CenterY),
                // Mode 7 reuses BG1HOFS/BG1VOFS as M7HOFS/M7VOFS. MainScrollingRoutine
                // continues publishing those live camera words even though its ordinary
                // tilemap row/column producers return early while `$0783` is nonzero.
                // Leaving these at zero pins the shaft art to the window while Samus,
                // enemies, and projectiles correctly move relative to the camera.
                horizontalOffset: unchecked((short)bg1HorizontalScroll),
                verticalOffset: unchecked((short)bg1VerticalScroll));
        }
        else
        {
            // Setup ASM $8F:C97B writes BG12NBA=$66 for the Ceres Ridley arena. Each
            // register nibble is a $1000-word character-base selector, so both BG1 and
            // BG2 deliberately reuse VRAM $6000. This is not a Ridley-art special case in
            // the compositor: it is the literal PPU register value selected by the room.
            // Other translated ordinary-room setup routines retain power-on base zero.
            bool usesCeresRidleyCharacterBase =
                runtime.ActiveRoom?.State.SetupCodePointer ==
                    RoomSetupCodePointers.SetCeresRidleyBgCharacterBaseAndSpawnHaze;
            ushort bgCharacterBaseWord = usesCeresRidleyCharacterBase
                ? (ushort)0x6000
                : (ushort)0;
            bool crocomireOwnsBg2 = runtime.Enemies.Crocomire is not null;
            KraidEnemyState? kraid = runtime.Enemies.Kraid;
            bool kraidOwnsBg2 = kraid is { OwnsBg2Tilemap: true };
            ScrollingSkyState? scrollingSky = runtime.ScrollingSky;
            ushort[]? skyHorizontalScrolls = scrollingSky?.BuildGameplayHorizontalScrolls(
                runtime.Camera?.YPosition
                    ?? throw new InvalidOperationException("Scrolling sky has no gameplay camera."));
            if (skyHorizontalScrolls is not null && shake.Bg2X != 0)
            {
                // Room shake is added to the live BG2HOFS register after HDMA supplies
                // each band value. Apply it to every resolved scanline for the same final
                // PPU coordinates instead of inventing a second sky-motion accumulator.
                for (int line = 0; line < skyHorizontalScrolls.Length; line++)
                    skyHorizontalScrolls[line] = AddShake(skyHorizontalScrolls[line], shake.Bg2X);
            }
            ushort[]? waterBg2HorizontalScrolls = displayedRoomFx is { } waterFx
                ? SnesGameplayFrameRenderer.BuildWaterBg2HorizontalScrolls(
                    waterFx,
                    bg2HorizontalScroll,
                    bg2VerticalScroll)
                : null;
            ushort[]? lavaAcidBg2HorizontalScrolls = displayedRoomFx is { } horizontalFx
                ? SnesGameplayFrameRenderer.BuildLavaAcidBg2HorizontalScrolls(
                    horizontalFx,
                    bg2HorizontalScroll,
                    bg2VerticalScroll)
                : null;
            ushort[]? lavaAcidBg2VerticalScrolls = displayedRoomFx is { } verticalFx
                ? SnesGameplayFrameRenderer.BuildLavaAcidBg2VerticalScrolls(
                    verticalFx,
                    bg2VerticalScroll)
                : null;
            frame = SnesGameplayFrameRenderer.RenderHudOrdinaryBackgroundsAndObjs(
                runtime.Vram,
                runtime.Cgram,
                runtime.DisplayedOam,
                bg1HorizontalScroll,
                bg1VerticalScroll,
                kraidOwnsBg2
                    ? AddShake(kraid!.Bg2HorizontalScroll, shake.Bg2X)
                    : crocomireOwnsBg2
                    ? AddShake(runtime.Enemies.CrocomireBg2HorizontalScroll, shake.Bg2X)
                    : bg2HorizontalScroll,
                kraidOwnsBg2
                    ? AddShake(kraid!.Bg2VerticalScroll, shake.Bg2Y)
                    : crocomireOwnsBg2
                    ? AddShake(runtime.Enemies.CrocomireBg2VerticalScroll, shake.Bg2Y)
                    : scrollingSky is not null
                        ? AddShake(scrollingSky.VerticalScroll, shake.Bg2Y)
                        : bg2VerticalScroll,
                bg2HorizontalScrollByLine:
                    lavaAcidBg2HorizontalScrolls ?? waterBg2HorizontalScrolls ?? skyHorizontalScrolls,
                bg2VerticalScrollByLine: crocomireOwnsBg2
                    ? runtime.Enemies.CrocomireDeath is { MeltingHdmaActive: true } melting
                        ? melting.Bg2ScrollByScanline : null
                    : lavaAcidBg2VerticalScrolls,
                bg2TilemapWidthInTiles: kraidOwnsBg2
                    ? KraidBackgroundRomData.TilemapWidthInTiles
                    : scrollingSky is null ? 64 : 32,
                bg2TilemapHeightInTiles: kraidOwnsBg2
                    ? KraidBackgroundRomData.TilemapHeightInTiles
                    : scrollingSky is null ? 32 : 64,
                bg2TilemapBaseWord: kraidOwnsBg2
                    ? KraidBackgroundRomData.LiveBg2TilemapWord
                    : SnesPpuLayout.GameplayBg2TilemapWord,
                bg1CharacterBaseWord: bgCharacterBaseWord,
                bg2CharacterBaseWord: bgCharacterBaseWord,
                bg3CharacterBaseWord: runtime.GameplayHudCharacterBaseWord,
                mainScreenLayers: runtime.DoorTransitionMainScreenLayers ??
                    (SnesMainScreenLayers.Bg1 |
                     SnesMainScreenLayers.Bg2 |
                     SnesMainScreenLayers.Obj));
        }

        bool doorIrqOwnsDisplay = runtime.DoorTransitionMainScreenLayers is not null;
        if (!doorIrqOwnsDisplay && runtime.DisplayedRoomLayer3Fx is { } layer3Fx)
            SnesGameplayFrameRenderer.ApplyRoomLayer3FxColorMath(frame, runtime.Vram, runtime.Cgram, layer3Fx);

        // These are the three setup routines that explicitly call FXType_2C_CeresHaze.
        // Keying the effect from cartridge state avoids applying a guessed “Ceres tint” to
        // scenes that do not spawn the HDMA object.
        if (!doorIrqOwnsDisplay && runtime.ActiveRoom is { } activeRoom &&
            RoomSetupCodePointers.SpawnsCeresHaze(activeRoom.State.SetupCodePointer))
        {
            // FX type $2C selects one of two bank-$88 HDMA definitions from the area's
            // native boss bit. Ridley's retreat sets that bit on the same enemy-main call
            // which starts the escape timer, and it remains set throughout the return
            // route. Hard-coding the alive branch kept the lower-screen haze blue even
            // though every other Ceres system had entered evacuation state.
            bool ridleyIsDead =
                runtime.System.HasAnyBossBits(activeRoom.AreaIndex, BossBits.AreaBoss);
            SnesGameplayFrameRenderer.ApplyCeresHaze(frame, ridleyIsDead);
        }

        if (!doorIrqOwnsDisplay && runtime.DisplayedMorphBallEyeBeam is { } eyeBeam)
        {
            SnesGameplayFrameRenderer.ApplyMorphBallEyeBeamColorMath(
                frame,
                runtime.AddressSpace,
                eyeBeam,
                displayedPpu.Layer1XPosition,
                displayedPpu.Layer1YPosition);
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
            displayedPpu.Layer1XPosition,
            displayedPpu.Layer1YPosition);

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
