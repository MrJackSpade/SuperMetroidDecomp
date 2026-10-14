using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.Runtime;

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
    [PpuLayoutBoundary("backend: gameplay software PPU register setup")]
    public static Rgba32[] Render(SuperMetroidRuntime runtime)
    {
        Ensure.NotNull(runtime);
        if (runtime.Room.ActiveDoor is null)
            throw new InvalidOperationException("A cartridge room and door must be loaded before rendering gameplay.");
        // X-ray needs layer/palette identity until the final color-math operation.
        // Both retained GPU packets and immediate software rendering consume that
        // capture; a post-composition tint cannot implement its native windows.
        if (runtime.Enemies.PhantoonBehavior.Phantoon is not null || runtime.Enemies.MotherBrainEncounter.MotherBrain is not null || runtime.RoomLayer3Fx.Type is RoomFxType.Fireflea or RoomFxType.Spores || runtime.Player.Samus?.Xray.IsActive == true || runtime.Player.LastXrayBeamStep is { Completed: true, PhaseAtStart: XrayBeamPhase.Finish })
            return SoftwareLayeredSnapshotRenderer.Render(GameplayDisplayCapture.CaptureFrame(runtime));

        GameplayPpuRenderSnapshot displayedPpu = runtime.Display.DisplayedGameplayPpu;
        RoomShakeFrameResult shake = displayedPpu.RoomShake;
        // Room shake is added to the latched BG1/BG2 scroll registers.
        Position16<RoomPixels> bg1Scroll = displayedPpu.Bg1Scroll + shake.Bg1;
        Position16<Bg2Pixels> bg2Scroll = displayedPpu.Bg2Scroll + shake.Bg2;
        RoomLayer3FxRenderSnapshot? displayedRoomFx = runtime.Display.DisplayedRoomLayer3Fx;

        Rgba32[] frame;
        if (runtime.Enemies.RidleyEncounter.CeresRidley is { Mode7Active: true } getaway)
        {
            // Ceres room main $A6:AAAF temporarily replaces ordinary mode-nine BG1/BG2
            // below the HUD with the rotating Ridley/Baby Mode-7 map. OAM stays live, so
            // Samus and the timer retain the usual gameplay compositor and priority rules.
            frame = SnesGameplayFrameRenderer.RenderHudCeresRidleyGetawayAndObjs(runtime.Vram, runtime.Cgram, runtime.DisplayedOam, new Mode7RenderRegisters { MatrixA = unchecked((short)getaway.Mode7MatrixA), MatrixB = unchecked((short)getaway.Mode7MatrixB), MatrixC = unchecked((short)getaway.Mode7MatrixC), MatrixD = unchecked((short)getaway.Mode7MatrixD), Center = getaway.Mode7Center.AsSignedRegister(), Scroll = new Offset16<Mode7Pixels>(unchecked((short)(getaway.Mode7Offset.X + shake.Bg1.X)), unchecked((short)(getaway.Mode7Offset.Y + shake.Bg1.Y))) }, new CeresGetawayFloor { Bg2Scroll = bg2Scroll, Bg2CharacterBaseWord = 0x6000 }, outputBuffer: runtime.GameplayFrameBuffer);
        }
        else if (runtime.Room.ActiveDoor.UsesCeresElevatorMode7)
        {
            // DoorCode_CeresElevatorShaft `$8F:E4E0` installs the identity transform, but
            // room main `$89:ACC3` then replaces A/B/C/D from its rotation table throughout
            // the escape. NMI `$80:95A7` publishes those shadow words to $211B..$2120.
            // Samus's OAM origin is calculated through the paired inverse transform in
            // `$8B:8A52`; feeding identity to BG1 here made the visible platforms diverge
            // from both her sprite and the deliberately unrotated physical collision map.
            SamusMode7Transform mode7 = runtime.Display.DisplayedSamusMode7Transform
                ?? throw new InvalidOperationException(
                    "The Ceres elevator shaft has no NMI-published Mode 7 matrix.");
            frame = SnesGameplayFrameRenderer.RenderHudMode7AndObjs(runtime.Vram, runtime.Cgram, runtime.DisplayedOam, new Mode7RenderRegisters { MatrixA = unchecked((short)mode7.MatrixA), MatrixB = unchecked((short)mode7.MatrixB), MatrixC = unchecked((short)mode7.MatrixC), MatrixD = unchecked((short)mode7.MatrixA), Center = mode7.Center.AsMode7Register(), Scroll = new Offset16<Mode7Pixels>(unchecked((short)bg1Scroll.X), unchecked((short)bg1Scroll.Y)) }, outputBuffer: runtime.GameplayFrameBuffer);
        }
        else
        {
            // Setup ASM $8F:C97B writes BG12NBA=$66 for the Ceres Ridley arena. Each
            // register nibble is a $1000-word character-base selector, so both BG1 and
            // BG2 deliberately reuse VRAM $6000. This is not a Ridley-art special case in
            // the compositor: it is the literal PPU register value selected by the room.
            // Other translated ordinary-room setup routines retain power-on base zero.
            bool usesCeresRidleyCharacterBase =
                runtime.Room.ActiveRoom?.State.SetupCallback ==
                    RoomSetupCallback.SetCeresRidleyBgCharacterBaseAndSpawnHaze;
            ushort bgCharacterBaseWord = usesCeresRidleyCharacterBase
                ? (ushort)0x6000
                : (ushort)0;
            bool crocomireOwnsBg2 = runtime.Enemies.CrocomireBehavior.Crocomire is not null;
            KraidEnemyState? kraid = runtime.Enemies.KraidBehavior.Kraid;
            bool kraidOwnsBg2 = kraid is { OwnsBg2Tilemap: true };
            Position16<Bg2Pixels> kraidBg2Scroll = kraid is { OwnsBg2Tilemap: true } owner ? owner.Bg2Scroll : default;
            ScrollingSkyState? scrollingSky = runtime.Room.ScrollingSky;
            ushort[]? skyHorizontalScrolls = scrollingSky is { HorizontalHdmaConfigured: true } ? scrollingSky.BuildGameplayHorizontalScrolls(
                runtime.Room.Camera?.PixelPosition.Y
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
            // Crocomire's melting table spans every PPU scanline; gameplay reads lines 0..191 of it.
            ushort[]? horizontalScrolls = runtime.Enemies.PhantoonBehavior.Phantoon?.Wave.DisplayedScrolls ?? lavaAcidBg2HorizontalScrolls ?? waterBg2HorizontalScrolls ?? skyHorizontalScrolls;
            ushort[]? verticalScrolls = crocomireOwnsBg2
                ? runtime.Enemies.CrocomireBehavior.CrocomireDeath is { MeltingHdmaActive: true } melting
                    ? melting.Bg2ScrollByScanline.Take(Bg2HdmaScrollTables.GameplayLines).ToArray() : null
                : lavaAcidBg2VerticalScrolls;
            var registers = new OrdinaryGameplayRegisters(
                bg1Scroll,
                new Position16<Bg2Pixels>(
                    kraidOwnsBg2
                        ? AddShake(kraidBg2Scroll.X, shake.Bg2.X)
                        : crocomireOwnsBg2
                        ? AddShake(runtime.Enemies.CrocomireBehavior.CrocomireBg2Scroll.X, shake.Bg2.X)
                        : bg2Scroll.X,
                    kraidOwnsBg2
                        ? AddShake(kraidBg2Scroll.Y, shake.Bg2.Y)
                        : crocomireOwnsBg2
                        ? AddShake(runtime.Enemies.CrocomireBehavior.CrocomireBg2Scroll.Y, shake.Bg2.Y)
                        : scrollingSky is not null
                            ? AddShake(scrollingSky.VerticalScroll, shake.Bg2.Y)
                            : runtime.TourianStatues.Enabled
                                ? unchecked((ushort)(displayedPpu.Layer1Position.Y + runtime.TourianStatues.DisplayedVerticalOffset + shake.Bg2.Y))
                                : bg2Scroll.Y),
                kraidOwnsBg2
                    ? new Extent16<TilemapCells>(KraidBackgroundRomData.TilemapWidthInTiles, KraidBackgroundRomData.TilemapHeightInTiles)
                    : runtime.Enemies.MotherBrainEncounter.MotherBrain is { HasBg2ScrollOverride: true } ? new Extent16<TilemapCells>(32, 32)
                    : scrollingSky?.HorizontalHdmaConfigured != true && runtime.RoomLayer3Fx.Type != RoomFxType.TourianEntranceStatue
                        ? new Extent16<TilemapCells>(64, 32)
                        : new Extent16<TilemapCells>(32, 64),
                kraidOwnsBg2
                    ? KraidBackgroundRomData.LiveBg2TilemapWord
                    : GameplayVramLayout.Bg2TilemapWord,
                Bg1CharacterWord: bgCharacterBaseWord,
                Bg2CharacterWord: bgCharacterBaseWord,
                HudCharacterWord: runtime.Display.GameplayHudCharacterBaseWord,
                MainScreenLayers: runtime.Room.DoorTransitionMainScreenLayers ??
                    (SnesMainScreenLayers.Bg1 |
                     SnesMainScreenLayers.Bg2 |
                     SnesMainScreenLayers.Obj),
                Bg2FirstScanline: runtime.Display.DisplayedGameplayPpu.Bg2FirstScanline,
                Bg2EndScanline: runtime.Display.DisplayedGameplayPpu.Bg2EndScanline);
            frame = SnesGameplayFrameRenderer.RenderHudOrdinaryBackgroundsAndObjs(
                runtime.Vram,
                runtime.Cgram,
                runtime.DisplayedOam,
                new OrdinaryGameplayRenderLayer(registers, new Bg2HdmaScrollTables(horizontalScrolls, verticalScrolls),
                    runtime.Room.DoorTransitionMainScreenLayers is null && runtime.Enemies.MotherBrainEncounter.MotherBrain?.RisingHdmaActive == true
                    ? MotherBrainAscentDisplayDefinitions.BuildGameplayLayers() : default),
                outputBuffer: runtime.GameplayFrameBuffer);
        }

        bool doorIrqOwnsDisplay = runtime.Room.DoorTransitionMainScreenLayers is not null;
        if (!doorIrqOwnsDisplay && runtime.Display.DisplayedRoomLayer3Fx is { } layer3Fx)
            SnesGameplayFrameRenderer.ApplyRoomLayer3FxColorMath(frame, runtime.Vram, runtime.Cgram, layer3Fx);

        // These are the three setup routines that explicitly call FXType_2C_CeresHaze.
        // Keying the effect from cartridge state avoids applying a guessed “Ceres tint” to
        // scenes that do not spawn the HDMA object.
        if (!doorIrqOwnsDisplay && runtime.CeresHaze.Enabled)
        {
            // The HDMA object retains its spawn-time channel while the boss bit changes.
            SnesGameplayFrameRenderer.ApplyCeresHaze(frame, runtime.CeresHaze.IsRed,
                runtime.CeresHaze.Intensity, runtime.Artwork.MapPresentation?.RoomFxPaletteBlends);
        }

        if (!doorIrqOwnsDisplay && runtime.Display.DisplayedMorphBallEyeBeam is { } eyeBeam)
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
        SamusSuitPickupRenderer.Composite(frame, runtime.SuitPickup.Flash);

        return frame;
    }

    /// <summary>Adds a signed room-shake displacement to a latched PPU scroll word with native word wraparound.</summary>
    private static ushort AddShake(ushort scroll, short displacement) =>
        unchecked((ushort)(scroll + displacement));
}
