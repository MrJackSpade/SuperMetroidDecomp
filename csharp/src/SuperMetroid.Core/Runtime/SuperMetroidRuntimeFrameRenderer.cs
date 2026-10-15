using SuperMetroid.Core.Assets;
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
/// Selects the PPU compositor required by one published gameplay display frame.
/// </summary>
/// <remarks>
/// This decision used to live in the debug executable, which made the standalone game and
/// visual tests vulnerable to choosing a different rendering path. Keeping it beside the
/// renderers makes every caller honor the cartridge door setup in exactly the same way.
/// </remarks>
public static class SuperMetroidRuntimeFrameRenderer
{
    /// <summary>Renders a published gameplay display frame into <paramref name="outputBuffer"/> or a new raster.</summary>
    [PpuLayoutBoundary("backend: gameplay software PPU register setup")]
    public static Rgba32[] Render(GameplayDisplayFrame frame, Rgba32[]? outputBuffer = null)
    {
        Ensure.NotNull(frame);
        GameplayNmiDisplayLatch latched = frame.Latched;
        GameplayFrameEndDisplayInputs live = frame.FrameEnd;
        // X-ray needs layer/palette identity until the final color-math operation.
        // Both retained GPU packets and immediate software rendering consume that
        // capture; a post-composition tint cannot implement its native windows.
        if (latched.Phantoon is not null || live.MotherBrainPresent || latched.Scene.FxType is RoomFxType.Fireflea or RoomFxType.Spores || live.Xray is not null)
            return SoftwareLayeredSnapshotRenderer.Render(GameplayDisplayCapture.Capture(frame), outputBuffer);

        using var scratch = new RenderScratch();
        SoftwarePpuSnapshotMemory memory = scratch.Memory(frame.Memory);
        SnesVram vram = memory.Vram;
        SnesCgram cgram = memory.Cgram;
        SpriteFrameBuffer sprites = memory.Sprites;
        GameplayPpuRenderSnapshot displayedPpu = latched.Ppu;
        GameplayLatchedSceneRegisters scene = latched.Scene;
        GameplayLatchedBossBackgrounds bosses = latched.Bosses;
        RoomShakeFrameResult shake = displayedPpu.RoomShake;
        // Room shake is added to the latched BG1/BG2 scroll registers.
        Position16<RoomPixels> bg1Scroll = displayedPpu.Bg1Scroll + shake.Bg1;
        Position16<Bg2Pixels> bg2Scroll = displayedPpu.Bg2Scroll + shake.Bg2;
        RoomLayer3FxRenderSnapshot? displayedRoomFx = latched.RoomLayer3Fx;

        Rgba32[] pixels;
        if (bosses.CeresRidleyMode7 is { } ridley)
        {
            Mode7RenderRegisters getaway = GameplayDisplayCapture.CeresRidleyRegisters(ridley);
            // Ceres room main $A6:AAAF temporarily replaces ordinary mode-nine BG1/BG2
            // below the HUD with the rotating Ridley/Baby Mode-7 map. OAM stays live, so
            // Samus and the timer retain the usual gameplay compositor and priority rules.
            pixels = SnesGameplayFrameRenderer.RenderHudCeresRidleyGetawayAndObjs(vram, cgram, sprites, getaway with { Scroll = new Offset16<Mode7Pixels>(unchecked((short)(getaway.Scroll.X + shake.Bg1.X)), unchecked((short)(getaway.Scroll.Y + shake.Bg1.Y))) }, new CeresGetawayFloor { Bg2Scroll = bg2Scroll, Bg2CharacterBaseWord = GameplayRenderDefinitions.CeresCharacterWord }, outputBuffer: outputBuffer);
        }
        else if (scene.CeresElevatorMode7)
        {
            // DoorCode_CeresElevatorShaft `$8F:E4E0` installs the identity transform, but
            // room main `$89:ACC3` then replaces A/B/C/D from its rotation table throughout
            // the escape. NMI `$80:95A7` publishes those shadow words to $211B..$2120.
            // Samus's OAM origin is calculated through the paired inverse transform in
            // `$8B:8A52`; feeding identity to BG1 here made the visible platforms diverge
            // from both her sprite and the deliberately unrotated physical collision map.
            SamusMode7Transform mode7 = latched.SamusMode7Transform
                ?? throw new InvalidOperationException(
                    "The Ceres elevator shaft has no NMI-published Mode 7 matrix.");
            pixels = SnesGameplayFrameRenderer.RenderHudMode7AndObjs(vram, cgram, sprites, new Mode7RenderRegisters { MatrixA = unchecked((short)mode7.MatrixA), MatrixB = unchecked((short)mode7.MatrixB), MatrixC = unchecked((short)mode7.MatrixC), MatrixD = unchecked((short)mode7.MatrixA), Center = mode7.Center.AsMode7Register(), Scroll = new Offset16<Mode7Pixels>(unchecked((short)bg1Scroll.X), unchecked((short)bg1Scroll.Y)) }, outputBuffer: outputBuffer);
        }
        else
        {
            // Setup ASM $8F:C97B writes BG12NBA=$66 for the Ceres Ridley arena. Each
            // register nibble is a $1000-word character-base selector, so both BG1 and
            // BG2 deliberately reuse VRAM $6000. This is not a Ridley-art special case in
            // the compositor: it is the literal PPU register value selected by the room.
            // Other translated ordinary-room setup routines retain power-on base zero.
            ushort bgCharacterBaseWord = scene.UsesCeresRidleyCharacterBase ? GameplayRenderDefinitions.CeresCharacterWord : (ushort)0;
            Position16<Bg2Pixels>? kraidBg2Scroll = bosses.KraidBg2Scroll;
            Position16<Bg2Pixels>? crocomireBg2Scroll = bosses.CrocomireBg2Scroll;
            ScrollingSkyRegisterSnapshot? scrollingSky = scene.ScrollingSky;
            // Room shake is added to the live BG2HOFS register after HDMA supplies
            // each band value, giving the same final PPU coordinates as the capture.
            ushort[]? skyHorizontalScrolls = GameplayDisplayCapture.SkyHorizontalScrolls(live, shake);
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
            ushort[]? horizontalScrolls = latched.Phantoon?.Scrolls?.ToArray() ?? lavaAcidBg2HorizontalScrolls ?? waterBg2HorizontalScrolls ?? skyHorizontalScrolls;
            ushort[]? verticalScrolls = crocomireBg2Scroll is not null
                ? live.CrocomireMeltingScrolls is { } melting
                    ? melting.AsSpan(0, Bg2HdmaScrollTables.GameplayLines).ToArray() : null
                : lavaAcidBg2VerticalScrolls;
            var registers = new OrdinaryGameplayRegisters(
                bg1Scroll,
                new Position16<Bg2Pixels>(
                    kraidBg2Scroll is { } kraidX
                        ? AddShake(kraidX.X, shake.Bg2.X)
                        : crocomireBg2Scroll is { } crocomireX
                        ? AddShake(crocomireX.X, shake.Bg2.X)
                        : bg2Scroll.X,
                    kraidBg2Scroll is { } kraidY
                        ? AddShake(kraidY.Y, shake.Bg2.Y)
                        : crocomireBg2Scroll is { } crocomireY
                        ? AddShake(crocomireY.Y, shake.Bg2.Y)
                        : scrollingSky is { } skyRegisters
                            ? AddShake(skyRegisters.VerticalScroll, shake.Bg2.Y)
                            : live.TourianStatuesEnabled
                                ? unchecked((ushort)(displayedPpu.Layer1Position.Y + latched.TourianStatueVerticalOffset + shake.Bg2.Y))
                                : bg2Scroll.Y),
                kraidBg2Scroll is not null
                    ? new Extent16<TilemapCells>(KraidBackgroundRomData.TilemapWidthInTiles, KraidBackgroundRomData.TilemapHeightInTiles)
                    : bosses.MotherBrainBg2ScrollOverride ? new Extent16<TilemapCells>(32, 32)
                    : scrollingSky?.HorizontalHdmaConfigured != true && scene.FxType != RoomFxType.TourianEntranceStatue
                        ? new Extent16<TilemapCells>(64, 32)
                        : new Extent16<TilemapCells>(32, 64),
                kraidBg2Scroll is not null
                    ? KraidBackgroundRomData.LiveBg2TilemapWord
                    : GameplayVramLayout.Bg2TilemapWord,
                Bg1CharacterWord: bgCharacterBaseWord,
                Bg2CharacterWord: bgCharacterBaseWord,
                HudCharacterWord: scene.HudCharacterWord,
                MainScreenLayers: live.DoorTransitionMainScreenLayers ??
                    (SnesMainScreenLayers.Bg1 |
                     SnesMainScreenLayers.Bg2 |
                     SnesMainScreenLayers.Obj),
                Bg2FirstScanline: displayedPpu.Bg2FirstScanline,
                Bg2EndScanline: displayedPpu.Bg2EndScanline);
            pixels = SnesGameplayFrameRenderer.RenderHudOrdinaryBackgroundsAndObjs(
                vram,
                cgram,
                sprites,
                new OrdinaryGameplayRenderLayer(registers, new Bg2HdmaScrollTables(horizontalScrolls, verticalScrolls),
                    live.DoorTransitionMainScreenLayers is null && live.MotherBrainRisingHdmaActive
                    ? MotherBrainAscentDisplayDefinitions.BuildGameplayLayers() : default),
                outputBuffer: outputBuffer);
        }

        bool doorIrqOwnsDisplay = live.DoorTransitionMainScreenLayers is not null;
        if (!doorIrqOwnsDisplay && displayedRoomFx is { } layer3Fx)
            SnesGameplayFrameRenderer.ApplyRoomLayer3FxColorMath(pixels, vram, cgram, layer3Fx);

        // These are the three setup routines that explicitly call FXType_2C_CeresHaze.
        // Keying the effect from cartridge state avoids applying a guessed “Ceres tint” to
        // scenes that do not spawn the HDMA object.
        if (!doorIrqOwnsDisplay && live.CeresHaze is { } haze)
        {
            // The HDMA object retains its spawn-time channel while the boss bit changes.
            SnesGameplayFrameRenderer.ApplyCeresHaze(pixels, haze.IsRed, haze.Intensity, live.RoomFxPaletteBlends);
        }

        if (!doorIrqOwnsDisplay && latched.MorphBallEyeBeam is { } eyeBeam)
        {
            SnesGameplayFrameRenderer.ApplyMorphBallEyeBeamColorMath(
                pixels,
                eyeBeam,
                displayedPpu.Layer1Position);
        }

        // The bank-$88 Power Bomb objects own a final PPU color-math window, not OAM or
        // either room background. Every gameplay presentation therefore has to apply the
        // shared window after its room-specific layers have been composed. The Room Viewer
        // already called this compositor directly; omitting it here let the real desktop
        // advance the complete damaging radius while showing no explosion at all.
        SnesGameplayFrameRenderer.ApplyPowerBombColorMath(
            pixels,
            GameplayDisplayCapture.PowerBomb(frame),
            displayedPpu.Layer1Position);

        // Bank $85 temporarily owns BG3 and disables gameplay color math while an item
        // message is active. Its scanline-window result belongs above every room-specific
        // compositor and haze path, so one shared overlay covers ordinary and Mode-7 rooms.
        if (live.MessageBox is { } message)
            GameplayMessageBoxRenderer.Composite(pixels, message, vram, cgram);

        // The suit transformation begins only after its bank-$85 message has completely
        // closed. Its bank-$88 window then overlays every ordinary/Mode-7 room through the
        // same PPU fixed-color path, so it belongs after room-specific haze as well.
        SamusSuitPickupRenderer.Composite(pixels, GameplayDisplayCapture.SuitPickup(frame));

        return pixels;
    }

    /// <summary>Adds a signed room-shake displacement to a latched PPU scroll word with native word wraparound.</summary>
    private static ushort AddShake(ushort scroll, short displacement) =>
        unchecked((ushort)(scroll + displacement));
}
