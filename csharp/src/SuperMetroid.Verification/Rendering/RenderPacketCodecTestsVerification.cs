using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Hardware.ColorWordsVerification;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Rendering;

/// <summary>Checks render-packet round trips, legacy compatibility, malformed inputs, and layered image fidelity.</summary>
internal static class RenderPacketCodecTestsVerification
{
    /// <summary>Checks packet round trips, legacy versions, malformed data rejection, and layered render compatibility.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native OAM tables, native PPU geometry, native VRAM words")]
    internal static void VerifyRenderPacketCodec()
    {
        var frame = new RenderFrameSnapshot(new(9876543210, 42, ushort.MaxValue),
            new Rgba32(123, 231, 76, 255), new byte[] { 9, 7 });
        byte[] bytes = RenderFrameSnapshotWriter.Serialize(frame);
        RenderFrameSnapshot restored = RoundTripRenderPacket(frame);
        AssertEqual(frame.Identity, restored.Identity, "portable fixture host/cartridge identity");
        byte[] firstVersion = (byte[])bytes.Clone();
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(
            firstVersion.AsSpan(RenderPacketFormat.Signature.Length), RenderPacketFormat.FirstSupportedVersion);
        AssertEqual(frame.SolidColor, RenderFrameSnapshotCodec.Deserialize(firstVersion).SolidColor,
            "version-one fixture remains readable after format extension");
        AssertTrue(SoftwareFrameSnapshotRenderer.Render(frame).SequenceEqual(
            SoftwareFrameSnapshotRenderer.Render(restored)), "fixture ordered-fade pixel parity");
        for (int length = 0; length < bytes.Length; length++)
        {
            byte[] truncated = bytes[..length];
            AssertThrows<IOException>(() => RenderFrameSnapshotCodec.Deserialize(truncated), "reject truncated solid fixture");
        }
        byte[] corrupt = (byte[])bytes.Clone();
        corrupt[0] ^= byte.MaxValue;
        AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize(corrupt), "reject signature mismatch");
        byte[] version = (byte[])bytes.Clone();
        version[RenderPacketFormat.Signature.Length]++;
        AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize(version), "reject unsupported format version");
        AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize([.. bytes, 0]), "reject trailing data");
        byte[] huge = new byte[RenderPacketFormat.MaximumPacketBytes + 1];
        AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize(huge), "reject oversized fixture");
        byte[] badKind = (byte[])bytes.Clone();
        badKind[^5] = byte.MaxValue; // Kind directly precedes solid RGBA.
        AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize(badKind), "reject unknown composition kind");
        byte[] badCount = (byte[])bytes.Clone();
        int fadeCountOffset = RenderPacketFormat.Signature.Length + sizeof(ushort) + 2 * sizeof(long) + sizeof(ushort);
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(badCount.AsSpan(fadeCountOffset), int.MaxValue);
        AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize(badCount), "reject count before operation allocation");

        var memory = new PpuMemorySnapshot(new byte[SnesPpuLayout.VramByteCount],
            ToColors(new ushort[SnesPpuLayout.CgramColorCount]), new byte[SnesPpuLayout.OamUploadByteCount], 0);
        var registers = new Mode7RenderRegisters(short.MinValue, short.MaxValue, -1, 0,
            new Offset16<Mode7Pixels>(-123, 456), new Offset16<Mode7Pixels>(-789, 1023), true);
        var mode7 = new RenderFrameSnapshot(new(1, 1, 0), new Mode7ObjRenderSnapshot(memory, registers, 3, 15));
        AssertEqual(registers, ((RoundTripRenderPacket(mode7).Mode7 ?? throw new InvalidOperationException("Verification requires RoundTripRenderPacket(mode7).Mode7.")).Background ?? throw new InvalidOperationException("Verification requires (RoundTripRenderPacket(mode7).Mode7 ?? throw new InvalidOperationException(\"Verificatio....")), "signed matrix and outside policy preserved");
        var wrapping = registers with { FillOutsideWithCharacterZero = false, WrapOutsideMap = true };
        var wrapPacket = new RenderFrameSnapshot(new(1, 1, 0), new Mode7ObjRenderSnapshot(memory, wrapping, 3, 15));
        AssertEqual(wrapping, ((RoundTripRenderPacket(wrapPacket).Mode7 ?? throw new InvalidOperationException("Verification requires RoundTripRenderPacket(wrapPacket).Mode7.")).Background ?? throw new InvalidOperationException("Verification requires (RoundTripRenderPacket(wrapPacket).Mode7 ?? throw new InvalidOperationException(\"Verifi....")), "Mode 7 wrap policy survives packet round trip");
        byte[] oldWrap = RenderFrameSnapshotWriter.SerializeBeforeSpriteDraws(wrapPacket);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(oldWrap.AsSpan(RenderPacketFormat.Signature.Length), 21);
        AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize(oldWrap), "version 21 cannot claim Mode 7 wrapping");
        byte[] legacyFill = RenderFrameSnapshotWriter.SerializeBeforeSpriteDraws(mode7);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(legacyFill.AsSpan(RenderPacketFormat.Signature.Length), 21);
        AssertEqual(registers, ((RenderFrameSnapshotCodec.Deserialize(legacyFill).Mode7 ?? throw new InvalidOperationException("Verification requires RenderFrameSnapshotCodec.Deserialize(legacyFill).Mode7.")).Background ?? throw new InvalidOperationException("Verification requires (RenderFrameSnapshotCodec.Deserialize(legacyFill).Mode7 ?? throw new InvalidOperationEx....")), "legacy boolean overflow retains character-zero semantics");
        var insertedMode7 = new RenderFrameSnapshot(new(2, 1, 0), new LayeredRenderSnapshot(memory,
            new RenderLayer[] { new Mode7RenderLayer(registers) }, 3, 15));
        byte[] invalidOldVersion = RenderFrameSnapshotWriter.SerializeBeforeSpriteDraws(insertedMode7);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(
            invalidOldVersion.AsSpan(RenderPacketFormat.Signature.Length), RenderPacketFormat.FirstSupportedVersion);
        AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize(invalidOldVersion),
            "version-one cannot claim a version-two layer operation");
        byte[] badBool = RenderFrameSnapshotWriter.Serialize(mode7);
        badBool[^1] = 2;
        AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize(badBool), "reject noncanonical boolean");
        var layered = new RenderFrameSnapshot(new(2, 1, 0), new LayeredRenderSnapshot(memory,
            new RenderLayer[] { new ObjRenderLayer() }, 3, 15));
        byte[] badLayer = RenderFrameSnapshotWriter.Serialize(layered);
        badLayer[^1] = byte.MaxValue;
        AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize(badLayer), "reject unknown layer kind");
        var fixedColor = new RenderFrameSnapshot(new(3, 1, 0), new LayeredRenderSnapshot(memory,
            new RenderLayer[] { new FixedColorAddRenderLayer(31, 15, 1) }, 3, 15));
        byte[] versionTwo = RenderFrameSnapshotWriter.SerializeBeforeSpriteDraws(fixedColor);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(
            versionTwo.AsSpan(RenderPacketFormat.Signature.Length), RenderPacketFormat.Mode7LayerVersion);
        AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize(versionTwo),
            "version-two cannot claim a version-three color operation");
        var viewport = new RenderFrameSnapshot(new(4, 1, 0), new LayeredRenderSnapshot(memory,
            new RenderLayer[] { new Bg2BppViewportRenderLayer(0, 0, 255, true, null) }, 3, 15));
        byte[] versionThree = RenderFrameSnapshotWriter.SerializeBeforeSpriteDraws(viewport);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(
            versionThree.AsSpan(RenderPacketFormat.Signature.Length), RenderPacketFormat.FixedColorLayerVersion);
        AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize(versionThree),
            "version-three cannot claim a version-four viewport operation");
        VerifyBg2ViewportPackets();
        VerifyRenderPacketFieldWidths(memory);
        Console.WriteLine("  Display fixture codec: stable round trip, identity, fades, truncation, signature/version and trailing-data rejection agree.");
    }

    /// <summary>Confirms serialized identity and canonical encoded fields survive a packet round trip.</summary>
    internal static RenderFrameSnapshot RoundTripRenderPacket(RenderFrameSnapshot frame)
    {
        byte[] bytes = RenderFrameSnapshotWriter.Serialize(frame);
        RenderFrameSnapshot restored = RenderFrameSnapshotCodec.Deserialize(bytes);
        AssertEqual(frame.Identity, restored.Identity, "render fixture preserves frame identity");
        AssertTrue(System.Security.Cryptography.SHA256.HashData(bytes).SequenceEqual(PacketDigest(restored)),
            "canonical fixture round trip preserves every encoded field");
        return restored;
    }

    /// <summary>Compares decoded 2bpp viewport layers against scrolled tilemap pixels across transparency and priority choices.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native OAM tables, native PPU geometry, native VRAM words")]
    private static void VerifyBg2ViewportPackets()
    {
        var random = new Random(321);
        byte[] vramBytes = new byte[SnesPpuLayout.VramByteCount];
        random.NextBytes(vramBytes);
        ushort[] colors = Enumerable.Range(0, SnesPpuLayout.CgramColorCount)
            .Select(_ => (ushort)random.Next(32768)).ToArray();
        var memory = new PpuMemorySnapshot(vramBytes, ToColors(colors),
            new byte[SnesPpuLayout.OamUploadByteCount], 0);
        var scratch = new SoftwarePpuSnapshotMemory(memory);
        foreach (ushort scroll in new ushort[] { 0, 8, 255, ushort.MaxValue })
        foreach (bool transparent in new[] { false, true })
        foreach (bool? priority in new bool?[] { null, false, true })
        {
            var layer = new Bg2BppViewportRenderLayer(0, 0x4000, scroll, transparent, priority);
            var frame = new RenderFrameSnapshot(new(1, 1, 0),
                new LayeredRenderSnapshot(memory, new RenderLayer[] { layer }, 3, 15));
            Rgba32[] full = SnesBgTilemapRenderer.Render2Bpp(scratch.Vram, scratch.Cgram,
                layer.TilemapWord, layer.CharacterWord, rowCount: 32,
                transparentColorZero: transparent, priority: priority);
            Rgba32[] expected = SnesLayerCompositor.CreateBackdrop(scratch.Cgram, 256 * 224);
            for (int y = 0; y < 224; y++)
                for (int x = 0; x < 256; x++)
                {
                    Rgba32 pixel = full[((y + scroll) % 256) * 256 + x];
                    if (pixel.A != 0) expected[y * 256 + x] = pixel;
                }
            AssertTrue(expected.SequenceEqual(RenderForComparison(RoundTripRenderPacket(frame))),
                $"2bpp viewport scroll={scroll}, transparent={transparent}, priority={priority}");
        }
    }

    /// <summary>
    /// Confirms the packet writer still emits every typed render field at its packet width. The reader consumes each
    /// field at a fixed width and rejects trailing or missing bytes, so a field written wider or narrower fails the round
    /// trip; the digest comparison then proves every value survives. Each field carries its largest legal value, so a
    /// field that no longer fits its packet width fails the writer's checked conversion.
    /// </summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words and window register values")]
    private static void VerifyRenderPacketFieldWidths(PpuMemorySnapshot memory)
    {
        const int lastWord = VramWordRange.MaximumWord;
        const int lastColumn = XrayWindowLine.MaximumColumn;
        var windows = new SnesWindowRegisters(SnesWindowSelectionPair.LowerEnableFirst | SnesWindowSelectionPair.UpperInvertSecond,
            SnesWindowSelectionPair.UpperEnableSecond, SnesWindowSelectionPair.LowerInvertFirst,
            lastColumn, 1, 2, lastColumn, SnesWindowRegisters.MaximumLogic, SnesWindowRegisters.MaximumLogic);
        var registers = new OrdinaryGameplayRegisters(new Position16<RoomPixels>(1, 2), new Position16<Bg2Pixels>(3, 4),
            new Extent16<TilemapCells>(64, 32), lastWord, lastWord - 1, lastWord - 2, lastWord - 3,
            SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Obj, Windows: windows,
            MainScreenWindowMask: SnesMainScreenLayers.Bg2);
        var lines = Enumerable.Repeat(new XrayWindowLine(lastColumn, 1), SnesPpuLayout.ScreenHeightPixels).ToArray();
        var subscreen = new Bg2BppColorMathRenderLayer(lastWord, lastWord - 4, 32, 32, ExpandedColorMathOperation.Add,
            new Position16<BgPlanePixels>[SnesPpuLayout.ScreenHeightPixels]);
        var gameplay = new GameplayColorMathRenderLayer(new OrdinaryGameplayRenderLayer(registers), lines, true,
            new ColorMathRegisters { ColorMath = SnesColorMathControl.Backdrop, AddSubscreen = true,
                FixedRed = Bgr555.MaxChannel, FixedGreen = 1, FixedBlue = 2 }, subscreen);
        var coverage = new Bg4BppRenderLayer(lastWord, lastWord - 5, new Position16<BgPlanePixels>(5, 6),
            new Extent16<TilemapCells>(32, 64), true);
        var white = new FixedColorAddRenderLayer(Bgr555.MaxChannel, 1, 2);
        var colorWindows = Enumerable.Repeat(new ColorAddWindow(1, lastColumn, byte.MaxValue, 1, 2),
            SnesPpuLayout.ScreenHeightPixels).ToArray();
        var layered = new RenderFrameSnapshot(new(5, 1, RenderFrameIdentity.MaximumSimulationFrame), new LayeredRenderSnapshot(memory,
            new RenderLayer[]
            {
                gameplay,
                new BgSubscreenAddRenderLayer(lastWord, lastWord - 6, coverage, FourBpp: true, IncludeObjects: true,
                    MainObjects: true, VerticalScroll: Bg2BppViewportRenderLayer.MaximumScroll),
                new Bg2BppViewportRenderLayer(lastWord, lastWord - 7, Bg2BppViewportRenderLayer.MaximumScroll, true, false),
                coverage,
                new Bg2BppRenderLayer(lastWord, lastWord - 8, SnesPpuLayout.ScreenHeightPixels / SnesPpuLayout.BackgroundTileSizePixels, true),
                white,
                new ObjPriorityRenderLayer(ObjPriorityRenderLayer.MaximumPriority, white),
                new ObjRenderLayer(FixedColor: white),
                new ScanlineColorAddRenderLayer(colorWindows),
            }, LayeredRenderSnapshot.MaximumObjectSelection, SnesPpuLayout.MaximumMasterBrightness));
        RenderFrameSnapshot restoredLayered = RoundTripRenderPacket(layered);
        var restoredLayers = (restoredLayered.Layers ?? throw new InvalidOperationException("A layered packet restores layers.")).Layers;
        var restoredGameplay = (GameplayColorMathRenderLayer)restoredLayers[0];
        AssertEqual(registers, restoredGameplay.Gameplay.Registers, "ordinary gameplay words and window registers survive at packet width");
        AssertEqual(lines[0], restoredGameplay.Lines[0], "window edges survive at packet width");
        AssertEqual(Bgr555.MaxChannel, restoredGameplay.FixedRed, "fixed-color intensity survives at packet width");
        AssertEqual(coverage, (Bg4BppRenderLayer)restoredLayers[3], "4-bpp plane words survive at packet width");
        AssertEqual(white, (FixedColorAddRenderLayer)restoredLayers[5], "fixed-color addition survives at packet width");
        AssertEqual(ObjPriorityRenderLayer.MaximumPriority, ((ObjPriorityRenderLayer)restoredLayers[6]).Priority,
            "OBJ priority survives at packet width");
        AssertEqual(colorWindows[0], ((ScanlineColorAddRenderLayer)restoredLayers[8]).Windows[0], "colour windows survive at packet width");

        var floor = new Mode1FloorBand(200, lastWord, lastWord - 9, new Position16<Bg2Pixels>(7, 8), new Extent16<TilemapCells>(32, 32));
        var mode7Gameplay = new Mode7GameplayRenderLayer(default, lastWord, lastWord - 10, 32, floor);
        RenderFrameSnapshot restoredMode7Gameplay = RoundTripRenderPacket(new RenderFrameSnapshot(new(6, 1, 0),
            new LayeredRenderSnapshot(memory, new RenderLayer[] { mode7Gameplay }, 0, 0)));
        AssertEqual(mode7Gameplay, (restoredMode7Gameplay.Layers ?? throw new InvalidOperationException("A layered packet restores layers.")).Layers[0],
            "Mode 7 HUD and floor words survive at packet width");

        var gradient = new[]
        {
            new TitleGradientLine(Bgr555.MaxChannel, 1, 2, ColorMathControlFlags.Subtract | ColorMathControlFlags.Backdrop | ColorMathControlFlags.Bg1),
            new TitleGradientLine(3, Bgr555.MaxChannel, 4, ColorMathControlFlags.Backdrop | ColorMathControlFlags.Obj | ColorMathControlFlags.Bg1),
        };
        TitleGradientLine[] gradientLines = Enumerable.Range(0, SnesPpuLayout.ScreenHeightPixels)
            .Select(line => gradient[line % 2]).ToArray();
        var title = new RenderFrameSnapshot(new(7, 1, 1), new Mode7ObjRenderSnapshot(memory, null,
            LayeredRenderSnapshot.MaximumObjectSelection, SnesPpuLayout.MaximumMasterBrightness, gradientLines));
        Mode7ObjRenderSnapshot restoredTitle = RoundTripRenderPacket(title).Mode7
            ?? throw new InvalidOperationException("A Mode 7 packet restores its composition.");
        AssertTrue(restoredTitle.Gradient.SequenceEqual(gradientLines), "title gradient lines survive at packet width");
        AssertEqual(LayeredRenderSnapshot.MaximumObjectSelection, restoredTitle.ObjectSelection, "OBSEL survives at packet width");
        Console.WriteLine("  Display fixture widths: every typed render field round-trips at its packet width.");
    }
}
