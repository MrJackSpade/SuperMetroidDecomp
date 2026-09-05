using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>Cartridge identities and expected FX words used by the room-$01/$27 audit.</summary>
internal static class WaterFxAuditDefinitions
{
    /// <summary>Skree Boost room header at <c>$8F:A3DD</c>.</summary>
    public const ushort RoomHeader = 0xa3dd;

    /// <summary>Default water FX record at <c>$83:83B0</c>.</summary>
    public const ushort FxRecord = 0x83b0;

    /// <summary>Static water surface Y loaded from record <c>$83:83B0</c>.</summary>
    public const ushort SurfaceY = 0x00be;
}

/// <summary>Exact player-reported rooms covered by atmospheric corruption issue #290.</summary>
internal static class AtmosphericFxAuditDefinitions
{
    private static readonly AtmosphericFxRoomDefinition[] RoomDefinitions =
    [
        new(0x95ff, new RoomIdentity(AreaId.Crateria, 0x0e), RoomFxType.Water, 0x0000),
        new(0x99f9, new RoomIdentity(AreaId.Crateria, 0x1d), RoomFxType.Acid, 0x0780),
        new(0xa5ed, new RoomIdentity(AreaId.Crateria, 0x30), RoomFxType.Fog, 0x0000),
        new(0xa0d2, new RoomIdentity(AreaId.Brinstar, 0x17), RoomFxType.Water, 0x0000),
    ];

    public static ReadOnlySpan<AtmosphericFxRoomDefinition> Rooms => RoomDefinitions;
}

/// <summary>One retail room/camera/type tuple used by the issue-#290 visual audit.</summary>
internal readonly record struct AtmosphericFxRoomDefinition(
    ushort HeaderPointer,
    RoomIdentity Identity,
    RoomFxType ExpectedType,
    ushort CameraY);

/// <summary>
/// Retail-ROM visual/state regression for issue #251. This deliberately compares the
/// complete runtime renderer with the same frame before room-FX composition so a valid
/// physics surface cannot masquerade as visible water.
/// </summary>
internal static class WaterFxAudit
{
    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(
            bus,
            WaterFxAuditDefinitions.RoomHeader);
        if (room.Identity != new RoomIdentity(AreaId.Brinstar, 0x27) ||
            room.State.FxPointer != WaterFxAuditDefinitions.FxRecord)
        {
            throw new InvalidDataException(
                $"Water audit selected room {room.Identity}, FX $83:{room.State.FxPointer:X4}.");
        }

        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(
            WaterFxAuditDefinitions.RoomHeader,
            cameraX: 0x0100,
            cameraY: 0);
        runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);

        RoomLayer3FxRenderSnapshot fx = runtime.DisplayedRoomLayer3Fx
            ?? throw new InvalidDataException("Room $01/$27 published no visible water FX.");
        if (fx.Type != RoomFxType.Water ||
            fx.LayerBlendConfiguration != LayerBlendingConfiguration.LiquidOrFogAdditive ||
            fx.CurrentYPosition != WaterFxAuditDefinitions.SurfaceY ||
            fx.WaterSurfaceScreenY != WaterFxAuditDefinitions.SurfaceY ||
            fx.LiquidOptions != 3)
        {
            throw new InvalidDataException(
                $"Room $01/$27 water snapshot mismatch: type/blend={fx.Type}/" +
                $"${(ushort)fx.LayerBlendConfiguration:X2}, surface=" +
                $"${fx.CurrentYPosition:X4}/{fx.WaterSurfaceScreenY}, options=${fx.LiquidOptions:X2}.");
        }

        SamusLiquidPhysicsState liquid = runtime.Samus?.LiquidPhysics
            ?? throw new InvalidDataException("Water audit has no live Samus liquid state.");
        if (liquid.FxType != RoomFxType.Water ||
            liquid.FxYPosition != WaterFxAuditDefinitions.SurfaceY ||
            liquid.LiquidOptions != 3)
        {
            throw new InvalidDataException(
                $"Room FX did not publish to Samus: {liquid.FxType}/" +
                $"${liquid.FxYPosition:X4}/${liquid.LiquidOptions:X2}.");
        }

        GameplayPpuRenderSnapshot ppu = runtime.DisplayedGameplayPpu;
        Rgba32[] withoutFx = SnesGameplayFrameRenderer.RenderHudOrdinaryBackgroundsAndObjs(
            runtime.Vram,
            runtime.Cgram,
            runtime.DisplayedOam,
            ppu.Bg1HorizontalScroll,
            ppu.Bg1VerticalScroll,
            ppu.Bg2HorizontalScroll,
            ppu.Bg2VerticalScroll);
        Rgba32[] withFx = SuperMetroidRuntimeFrameRenderer.Render(runtime);
        int changedAbove = CountChangedRows(
            withoutFx,
            withFx,
            SnesGameplayFrameRenderer.HudHeight,
            fx.WaterSurfaceScreenY + 1);
        int changedBelow = CountChangedRows(
            withoutFx,
            withFx,
            fx.WaterSurfaceScreenY + 1,
            SnesGameplayFrameRenderer.Height);
        if (changedAbove != 0 || changedBelow == 0)
        {
            throw new InvalidDataException(
                $"Room $01/$27 water changed {changedAbove} air pixels and " +
                $"{changedBelow} underwater pixels.");
        }

        ushort initialHorizontal = runtime.RoomLayer3Fx.HorizontalScroll;
        int initialPhase = fx.WaterBg3WavePhase;
        for (int frame = 0; frame < RoomFxRomData.Water.Bg3WavePhaseDuration; frame++)
        {
            runtime.RoomLayer3Fx.Step(
                bus,
                runtime.Vram,
                cameraX: 0x0100,
                cameraY: 0,
                timeIsFrozen: false);
        }
        RoomLayer3FxRenderSnapshot animated = runtime.RoomLayer3Fx.CaptureForDisplay()!.Value;
        if (animated.HorizontalScroll == initialHorizontal ||
            animated.WaterBg3WavePhase == initialPhase)
        {
            throw new InvalidDataException(
                $"Water animation did not advance: X ${initialHorizontal:X4}->" +
                $"${animated.HorizontalScroll:X4}, phase {initialPhase}->{animated.WaterBg3WavePhase}.");
        }

        VerifyReportedAtmosphericRooms(bus);

        Console.WriteLine(
            "Water FX audit passed: retail room $01/$27 loaded surface $00BE, additive " +
            $"BG3 changed {changedBelow} submerged pixels only, both wave options animated, " +
            "Samus consumed the same cartridge liquid state, and all four issue-#290 " +
            "rooms use clean padding/their cartridge BG3 page.");
        return 0;
    }

    /// <summary>
    /// Loads every room named by issue #290 through the shared runtime. Liquid rooms must
    /// leave the cleared air band unchanged while producing pixels below the surface; fog
    /// must produce pixels from its standalone $5C00 tilemap page.
    /// </summary>
    private static void VerifyReportedAtmosphericRooms(SuperMetroidAddressSpace bus)
    {
        foreach (AtmosphericFxRoomDefinition definition in AtmosphericFxAuditDefinitions.Rooms)
        {
            CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, definition.HeaderPointer);
            if (room.Identity != definition.Identity)
            {
                throw new InvalidDataException(
                    $"Atmospheric audit header $8F:{definition.HeaderPointer:X4} resolved to " +
                    $"{room.Identity}, not {definition.Identity}.");
            }

            var runtime = new SuperMetroidRuntime(bus);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(
                definition.HeaderPointer,
                cameraX: 0,
                cameraY: definition.CameraY);

            // Lava/acid characters are installed by the first ordinary bank-$87 handler
            // pass. Water/fog use fixed standard-BG3 characters and need no animation DMA.
            runtime.RoomLayer3Fx.Step(
                bus,
                runtime.Vram,
                cameraX: 0,
                cameraY: definition.CameraY,
                timeIsFrozen: false);
            runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);

            RoomLayer3FxRenderSnapshot fx = runtime.DisplayedRoomLayer3Fx
                ?? throw new InvalidDataException(
                    $"Room {definition.Identity} published no atmospheric FX snapshot.");
            if (fx.Type != definition.ExpectedType)
            {
                throw new InvalidDataException(
                    $"Room {definition.Identity} published {fx.Type}, expected " +
                    $"{definition.ExpectedType}.");
            }
            if (runtime.Vram.ReadWord(RoomFxRomData.Layer3.ClearDestinationWord) !=
                RoomFxRomData.Layer3.ClearTilemapWord)
            {
                throw new InvalidDataException(
                    $"Room {definition.Identity} did not retain the native blank word at " +
                    $"VRAM ${RoomFxRomData.Layer3.ClearDestinationWord:X4}.");
            }

            GameplayPpuRenderSnapshot ppu = runtime.DisplayedGameplayPpu;
            Rgba32[] withoutFx = SnesGameplayFrameRenderer.RenderHudOrdinaryBackgroundsAndObjs(
                runtime.Vram,
                runtime.Cgram,
                runtime.DisplayedOam,
                ppu.Bg1HorizontalScroll,
                ppu.Bg1VerticalScroll,
                ppu.Bg2HorizontalScroll,
                ppu.Bg2VerticalScroll);
            Rgba32[] withFx = SuperMetroidRuntimeFrameRenderer.Render(runtime);

            if (definition.ExpectedType is RoomFxType.Water or RoomFxType.Acid)
            {
                int effectStart = Math.Max(
                    SnesGameplayFrameRenderer.HudHeight,
                    fx.WaterSurfaceScreenY - (SnesPpuLayout.BackgroundTileSizePixels - 1));
                int changedAir = CountChangedRows(
                    withoutFx,
                    withFx,
                    SnesGameplayFrameRenderer.HudHeight,
                    effectStart);
                int changedLiquid = CountChangedRows(
                    withoutFx,
                    withFx,
                    Math.Max(effectStart, SnesGameplayFrameRenderer.HudHeight),
                    SnesGameplayFrameRenderer.Height);
                if (changedAir != 0 || changedLiquid == 0)
                {
                    throw new InvalidDataException(
                        $"Room {definition.Identity} changed {changedAir} cleared-air pixels " +
                        $"and {changedLiquid} liquid pixels.");
                }
            }
            else
            {
                int changedFog = CountChangedRows(
                    withoutFx,
                    withFx,
                    SnesGameplayFrameRenderer.HudHeight,
                    SnesGameplayFrameRenderer.Height);
                if (changedFog == 0)
                {
                    throw new InvalidDataException(
                        $"Room {definition.Identity} fog produced no visible BG3 pixels.");
                }
            }
        }
    }

    private static int CountChangedRows(
        Rgba32[] first,
        Rgba32[] second,
        int firstRow,
        int endRow)
    {
        int count = 0;
        for (int y = Math.Max(0, firstRow); y < Math.Min(SnesGameplayFrameRenderer.Height, endRow); y++)
        {
            int row = y * SnesGameplayFrameRenderer.Width;
            for (int x = 0; x < SnesGameplayFrameRenderer.Width; x++)
                count += first[row + x] != second[row + x] ? 1 : 0;
        }
        return count;
    }
}
