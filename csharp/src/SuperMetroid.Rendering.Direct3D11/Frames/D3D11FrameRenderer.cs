using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using Vortice.Direct3D11;
using Vortice.DXGI;
using SuperMetroid.Core.Spatial;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Vortice.Direct3D;
using Vortice.Mathematics;
using SuperMetroid.Core.Hud;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Telemetry;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>Integer compute composition from owned display inputs.</summary>
/// <remarks>Unsupported operations fail explicitly. Readback is diagnostic, not the presentation path.</remarks>
public sealed class D3D11FrameRenderer : IDisposable
{
    /// <summary>Thread-affine device shared with the output presenter and child scenes.</summary>
    internal readonly D3D11RenderDevice owner;
    /// <summary>Loads embedded shader bytecode and supports missing-resource verification.</summary>
    private readonly Func<string, Stream?> openShaderResource;
    /// <summary>Solid-color compute path used when no layer snapshot is supplied.</summary>
    private readonly ID3D11ComputeShader shader;
    /// <summary>Integer RGBA output texture consumed directly by the display pass.</summary>
    internal readonly ID3D11Texture2D output;
    /// <summary>Identity set only after a frame finishes GPU submission.</summary>
    internal RenderFrameIdentity? renderedIdentity;
    /// <summary>Identity of the last successfully submitted frame.</summary>
    internal RenderFrameIdentity? SubmittedIdentity => renderedIdentity;
    /// <summary>Owner device used by a presenter sharing this frame texture.</summary>
    internal D3D11RenderDevice DeviceOwner => owner;
    /// <summary>Compute write view for the final integer frame.</summary>
    private readonly ID3D11UnorderedAccessView view;
    /// <summary>Pixel-shader read view of the completed integer frame.</summary>
    private readonly ID3D11ShaderResourceView displaySource;
    /// <summary>Full-screen triangle vertex shader for scaled display.</summary>
    private readonly ID3D11VertexShader displayVertexShader;
    /// <summary>Converts integer frame texels for the swapchain target.</summary>
    private readonly ID3D11PixelShader displayPixelShader;
    /// <summary>Per-dispatch tile, window, gradient, or display parameters.</summary>
    private readonly ID3D11Buffer constants;
    /// <summary>Integer tile and OBJ composition compute program.</summary>
    private readonly ID3D11ComputeShader tileShader;
    /// <summary>Scanline gradient compute program for the title screen.</summary>
    private readonly ID3D11ComputeShader titleGradientShader;
    /// <summary>Uploaded VRAM, CGRAM, and OAM words for the current scene.</summary>
    private readonly ID3D11Buffer memoryBuffer;
    /// <summary>Shader read view of the packed PPU memory upload.</summary>
    private readonly ID3D11ShaderResourceView memoryView;
    /// <summary>The scene's whole-frame sprite draws, bound at register t1.</summary>
    private readonly D3D11SpriteDrawBuffer spriteDraws;
    /// <summary>The scene's published background layers, bound at register t2.</summary>
    /// <summary>The scene's room planes at t3, repacked only when they change (#1346).</summary>
    private readonly D3D11RoomLevelBuffer roomLevels;
    /// <summary>Each packed background layer's shader index, for the dispatch that draws it.</summary>
    /// <summary>Per-pixel resolved OBJ winner shared across priority passes.</summary>
    private readonly ID3D11Texture2D resolvedObjects;
    /// <summary>Compute write view for resolved OBJ winners.</summary>
    private readonly ID3D11UnorderedAccessView objectView;
    /// <summary>Owned GPU resources released in reverse creation order.</summary>
    private readonly List<IDisposable> resources = [];
    /// <summary>Guards later render calls after owner-thread disposal.</summary>
    internal bool disposed;
    /// <summary>Reusable CPU packing buffer for one scene's PPU memory.</summary>
    private readonly uint[] memoryUpload = new uint[D3D11ShaderLayout.PpuMemoryWords];
    /// <summary>Reusable constant-buffer source cleared before each compute pass.</summary>
    private readonly uint[] constantUpload = new uint[D3D11ShaderLayout.SolidConstantWords];

    /// <summary>The frontend tile layers' cells a frame draws, each once; reused across frames.</summary>
    private readonly List<ScreenTileMap> frameScreens = [];

    /// <summary>
    /// Reuses owner-thread scratch after UpdateSubresource has copied its source.
    /// Clear every word: optional scanline/coverage fields must never leak across passes.
    /// Child window renderers own separate scratch and cannot overwrite parent uploads.
    /// </summary>
    private uint[] ClearUploadConstants()
    {
        Array.Clear(constantUpload);
        return constantUpload;
    }

    /// <summary>Creates embedded compute/display shaders and native 256-by-224 output/OBJ resources on an existing render-owner device; owns those resources but not the device.</summary>
    /// <param name="owner">Live device owned by the calling thread; must outlive this renderer and share ownership with any presenter using its output.</param>
    /// <exception cref="ArgumentNullException"><paramref name="owner"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Construction is attempted off the device's owner thread.</exception>
    public D3D11FrameRenderer(D3D11RenderDevice owner)
        : this(owner, name => typeof(D3D11FrameRenderer).Assembly.GetManifestResourceStream(name)) { }

    /// <summary>Resource-lookup seam for strict missing-shader startup verification.</summary>
    internal D3D11FrameRenderer(D3D11RenderDevice owner, Func<string, Stream?> openShaderResource)
    {
        this.owner = Ensure.NotNull(owner);
        this.openShaderResource = Ensure.NotNull(openShaderResource);
        owner.VerifyOwner();
        try
        {
            shader = Own(LoadShader(D3D11ShaderLayout.SolidResourceName));
            tileShader = Own(LoadShader(D3D11ShaderLayout.TileResourceName));
            titleGradientShader = Own(LoadShader(D3D11ShaderLayout.TitleGradientResourceName));
            output = Own(owner.Device.CreateTexture2D(new Texture2DDescription(Format.R32_UInt,
                SnesPpuLayout.ScreenWidthPixels, SnesPpuLayout.ScreenHeightPixels, 1, 1, BindFlags.UnorderedAccess | BindFlags.ShaderResource)));
            view = Own(owner.Device.CreateUnorderedAccessView(output));
            displaySource = Own(owner.Device.CreateShaderResourceView(output));
            displayVertexShader = Own(owner.Device.CreateVertexShader(LoadShaderBytes(D3D11ShaderLayout.DisplayVertexResourceName)));
            displayPixelShader = Own(owner.Device.CreatePixelShader(LoadShaderBytes(D3D11ShaderLayout.DisplayPixelResourceName)));
            constants = Own(owner.Device.CreateBuffer(new BufferDescription(D3D11ShaderLayout.SolidConstantWords * sizeof(uint), BindFlags.ConstantBuffer)));
            memoryBuffer = Own(owner.Device.CreateBuffer(new BufferDescription(D3D11ShaderLayout.PpuMemoryWords * sizeof(uint),
                BindFlags.ShaderResource, ResourceUsage.Default, CpuAccessFlags.None, ResourceOptionFlags.BufferStructured, sizeof(uint))));
            memoryView = Own(owner.Device.CreateShaderResourceView(memoryBuffer));
            spriteDraws = Own(new D3D11SpriteDrawBuffer(owner.Device));
            roomLevels = Own(new D3D11RoomLevelBuffer(owner.Device));
            resolvedObjects = Own(owner.Device.CreateTexture2D(new Texture2DDescription(Format.R32G32_UInt,
                SnesPpuLayout.ScreenWidthPixels, SnesPpuLayout.ScreenHeightPixels, 1, 1, BindFlags.UnorderedAccess)));
            objectView = Own(owner.Device.CreateUnorderedAccessView(resolvedObjects));
        }
        catch { DisposeResources(); throw; }
    }

    /// <summary>Submits GPU composition without staging allocation, readback or a CPU raster.</summary>
    public void Render(RenderFrameSnapshot packet)
    {
        owner.VerifyOwner(); ObjectDisposedException.ThrowIf(disposed, this);
        Ensure.NotNull(packet);
        renderedIdentity = null;
        RenderCore(packet);
        renderedIdentity = packet.Identity;
    }

    /// <summary>Selects layered, Mode 7, or solid-color composition and submits its compute passes.</summary>
    private unsafe void RenderCore(RenderFrameSnapshot packet)
    {
        if (packet.Layers is { } layers) { DrawLayers(packet, layers); return; }
        if (packet.Mode7 is { } mode7)
        {
            var operations = new List<RenderLayer>();
            if (mode7.Background is { } background && mode7.Floor is { } shownFloor) operations.Add(new Mode7RenderLayer(background, shownFloor));
            operations.Add(new ObjRenderLayer());
            DrawLayers(packet, new LayeredRenderSnapshot(mode7.Memory,
                operations.ToArray(), mode7.Brightness), mode7.Gradient);
            return;
        }
        Rgba32 color = packet.SolidColor ?? throw new NotSupportedException("This compute path does not yet support the requested composition.");
        if (packet.Fades.Length > D3D11ShaderLayout.MaximumBrightnessPasses) throw new ArgumentOutOfRangeException(nameof(packet));
        uint[] data = ClearUploadConstants();
        data[0] = D3D11ConstantPacking.Bytes(color.R, color.G, color.B, color.A);
        data[1] = (uint)packet.Fades.Length;
        data[2] = packet.Size.X; data[3] = packet.Size.Y;
        for (int i = 0; i < packet.Fades.Length; i++) data[D3D11ShaderLayout.SolidHeaderWords + i] = (uint)packet.Fades[i].Brightness;
        fixed (uint* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.CSSetShader(shader);
        owner.Context.CSSetConstantBuffer(0, constants);
        owner.Context.CSSetUnorderedAccessView(0, view);
        owner.Context.Dispatch((uint)((packet.Size.X + D3D11ShaderLayout.DispatchTileEdge - 1) / D3D11ShaderLayout.DispatchTileEdge),
            (uint)((packet.Size.Y + D3D11ShaderLayout.DispatchTileEdge - 1) / D3D11ShaderLayout.DispatchTileEdge), 1);
    }

    /// <summary>Unbinds renderer shader/resource state and releases its owned GPU objects in reverse creation order on the owner thread; the shared device remains alive and repeated disposal is harmless.</summary>
    /// <exception cref="InvalidOperationException">First disposal is attempted from a different owner thread.</exception>
    public void Dispose()
    {
        if (disposed) return;
        owner.VerifyOwner();
        owner.Context.CSSetShader(null);
        owner.Context.VSSetShader(null);
        owner.Context.PSSetShader(null);
        owner.Context.PSUnsetShaderResource(0);
        owner.Context.PSSetConstantBuffer(0, null);
        owner.Context.CSSetConstantBuffer(0, null);
        owner.Context.CSSetShaderResource(0, null);
        owner.Context.CSSetUnorderedAccessView(1, null);
        DisposeResources();
        disposed = true;
    }

    /// <summary>Tracks a newly created renderer resource for reverse-order disposal.</summary>
    internal T Own<T>(T value) where T : IDisposable { resources.Add(value); return value; }
    /// <summary>Disposes all successfully created GPU objects, including partial constructor state.</summary>
    private void DisposeResources()
    {
        for (int i = resources.Count - 1; i >= 0; i--) resources[i].Dispose();
        resources.Clear();
    }
    /// <summary>Creates a compute shader from the named embedded bytecode.</summary>
    private ID3D11ComputeShader LoadShader(string name)
        => owner.Device.CreateComputeShader(LoadShaderBytes(name));

    /// <summary>Requires and reads one build-generated shader resource.</summary>
    private byte[] LoadShaderBytes(string name)
    {
        using Stream resource = openShaderResource(name)
            ?? throw new InvalidDataException($"Build-generated shader {name} is missing.");
        using var bytes = new MemoryStream(); resource.CopyTo(bytes);
        return bytes.ToArray();
    }

    /// <summary>The one BG3 plane a frame shows (#1348): every layer that reads BG3 reads the same plane.</summary>
    private static Layer3Cells SameBg3(Layer3Cells? shown, Layer3Cells plane) =>
        shown is null || ReferenceEquals(shown, plane) ? plane
            : throw new InvalidOperationException("A frame's layers show two different BG3 planes.");

    /// <summary>Uploads BG2 scanline scrolls for add/subtract color math.</summary>
    private unsafe void DispatchBackgroundMath(Bg2BppColorMathRenderLayer layer)
    {
        var data = ClearUploadConstants();
        data[0] = (uint)(layer.Operation == ExpandedColorMathOperation.Add
            ? D3D11TileOperation.BgAdd : D3D11TileOperation.BgSubtract);
        // The BG3 plane window: its first row stands in the tilemap word, its height in the map height (#1348).
        data[1] = (uint)layer.FirstRow;
        data[5] = 32; data[6] = (uint)layer.MapHeightTiles; data[8] = 1;
        data[14] = (uint)D3D11RoomSource.Bg3;
        data[25] = (uint)layer.FirstScanline; data[26] = 224;
        for (int y = 0; y < layer.Scrolls.Length; y++)
        {
            int offset = D3D11ShaderLayout.ScanlineParametersWordOffset + y * 4;
            data[offset] = layer.Scrolls[y].X;
            data[offset + 1] = layer.Scrolls[y].Y;
        }
        fixed (uint* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.Dispatch(32, 28, 1);
    }

    /// <summary>Uploads each scanline's color window edges and fixed RGB channels.</summary>
    private unsafe void DispatchColorWindows(ScanlineColorAddRenderLayer layer)
    {
        var data = ClearUploadConstants();
        data[0] = (uint)D3D11TileOperation.ScanlineAdd;
        data[26] = 224;
        for (int y = 0; y < layer.Windows.Length; y++)
        {
            ColorAddWindow line = layer.Windows[y];
            int offset = D3D11ShaderLayout.ScanlineParametersWordOffset + y * 4;
            data[offset] = SnesLittleEndian.Word(checked((byte)line.Left), checked((byte)line.Right));
            data[offset + 1] = (uint)line.Red;
            data[offset + 2] = (uint)line.Green;
            data[offset + 3] = (uint)line.Blue;
        }
        fixed (uint* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.Dispatch(32, 28, 1);
    }

    /// <summary>Draws the submitted integer frame into an owner-device target, without readback.</summary>
    internal unsafe void DrawDisplay(ID3D11RenderTargetView target, Extent16<DisplayPixels> client)
    {
        owner.VerifyOwner(); ObjectDisposedException.ThrowIf(disposed, this);
        if (client.X == 0 || client.Y == 0)
            throw new ArgumentOutOfRangeException(nameof(client), client, "A display target must have positive dimensions.");
        if (renderedIdentity is null) throw new InvalidOperationException("No frame has been submitted for display.");
        var viewport = DisplayViewport.ForClient(client);
        var data = System.Runtime.InteropServices.MemoryMarshal.Cast<uint, int>(ClearUploadConstants().AsSpan());
        data[0] = viewport.Offset.X; data[1] = viewport.Offset.Y;
        data[2] = viewport.Size.X; data[3] = viewport.Size.Y;
        fixed (int* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.CSSetUnorderedAccessView(0, null);
        owner.Context.CSSetUnorderedAccessView(1, null);
        owner.Context.OMSetRenderTargets(target);
        owner.Context.RSSetViewport(0, 0, client.X, client.Y);
        owner.Context.IASetInputLayout(null);
        owner.Context.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);
        owner.Context.VSSetShader(displayVertexShader);
        owner.Context.PSSetShader(displayPixelShader);
        owner.Context.PSSetConstantBuffer(0, constants);
        owner.Context.PSSetShaderResource(0, displaySource);
        owner.Context.Draw(3, 0);
        // Avoid SRV/UAV hazards on the next emulation frame and release target binding
        // before a later swapchain resize. Raster state belongs to this render owner.
        owner.Context.PSUnsetShaderResource(0);
        owner.Context.OMSetRenderTargets(Array.Empty<ID3D11RenderTargetView>());
    }

    /// <summary>Combines gameplay BG2, captured BG3, windows, and HDMA layer enables for X-Ray color math.</summary>
    private unsafe void DispatchGameplayColorMath(LayeredRenderSnapshot scene, GameplayColorMathRenderLayer layer)
    {
        if (layer.Gameplay.Registers.MainScreenWindowMask != SuperMetroid.Core.Hardware.SnesMainScreenLayers.None)
            throw new NotSupportedException("Combining captured hardware windows with GPU gameplay color math is not translated.");
        var r = layer.Gameplay.Registers;
        var data = ClearUploadConstants();
        data[0] = (uint)D3D11TileOperation.XrayGameplay;
        // Xray.hlsli reuses the tile header for BG1/BG2 registers and CGADSUB.
        data[1] = (uint)r.Bg1CharacterWord; data[2] = (uint)r.Bg2CharacterWord;
        data[3] = r.Bg1Scroll.X;
        data[4] = unchecked((ushort)(r.Bg1Scroll.Y + SnesPpuLayout.FirstVisibleBackgroundScanline));
        data[5] = r.Bg2MapSize.X; data[6] = r.Bg2MapSize.Y;
        data[7] = (uint)r.MainScreenLayers; data[8] = layer.RevealBlocks ? 1u : 0u;
        data[9] = (uint)layer.ColorMath;
        data[10] = (uint)layer.FixedRed; data[11] = (uint)layer.FixedGreen; data[12] = (uint)layer.FixedBlue;
        data[16] = (uint)r.Bg2TilemapWord;
        // BG1 is always the room's; BG2 is its layer-2 plane, the sky's rows or the tilemap at word 16.
        data[14] = D3D11RoomSources.Bg2Of(layer.Gameplay.Rooms);
        data[22] = layer.AddSubscreen ? 1u : 0u;
        data[26] = 224;
        data[27] = (uint)r.Bg2Mosaic.Size;
        // Subscreen descriptor: none, captured BG3, or gameplay BG2 registers.
        if (layer.SubscreenUsesBg2) data[15] = (uint)D3D11GameplaySubscreenKind.GameplayBg2;
        if (layer.Subscreen is { } sub)
        {
            data[15] = (uint)(layer.SubscreenUsesBg2
                ? D3D11GameplaySubscreenKind.GameplayBg2AndCapturedBg3
                : D3D11GameplaySubscreenKind.CapturedBg3);
            data[18] = (uint)sub.FirstRow;
            data[20] = (uint)sub.MapHeightTiles; data[21] = (uint)sub.FirstScanline;
        }
        for (int y = 32; y < 224; y++)
        {
            int offset = D3D11ShaderLayout.ScanlineParametersWordOffset + y * 4, line = y - 32;
            var scroll = layer.Gameplay.Bg2ScrollAt(line);
            data[offset] = scroll.X;
            data[offset + 1] = unchecked((ushort)(scroll.Y + SnesPpuLayout.FirstVisibleBackgroundScanline));
            data[offset + 2] = D3D11ConstantPacking.Bytes(checked((byte)layer.Lines[y].Left), checked((byte)layer.Lines[y].Right), 0, 0);
            // The high half carries the main-screen enable register for this line;
            // the low half retains the independent color-window edges.
            var mainScreen = layer.Gameplay.MainScreenLayersByLine.IsEmpty
                ? r.MainScreenLayers : (SnesMainScreenLayers)layer.Gameplay.MainScreenLayersByLine[line];
            // Draygon's TM HDMA still suppresses repeated BG2 art while X-Ray owns color math.
            if (y < r.Bg2FirstScanline || y >= r.Bg2EndScanline) mainScreen &= ~SnesMainScreenLayers.Bg2;
            data[offset + 2] |= D3D11ConstantPacking.Bytes(0, 0, (byte)mainScreen, 0);
            if (layer.Subscreen is { } bg3)
                data[offset + 3] = D3D11ConstantPacking.Halves(bg3.Scrolls[y].X, bg3.Scrolls[y].Y);
        }
        fixed (uint* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.Dispatch(32, 28, 1);
    }

    /// <summary>Draws the message text layer from t2 with bank $85's two temporary palette colors.</summary>
    private unsafe void DispatchMessage(MessageBoxRenderLayer layer)
    {
        var data = ClearUploadConstants();
        data[0] = (uint)D3D11TileOperation.Message;
        // The image's top scanline stands in the tilemap word; the room levels hold the image (#1392).
        data[1] = (uint)layer.Top;
        data[10] = GameplayMessageRomData.Palette.TemporaryLightColor.ToWord();
        data[11] = GameplayMessageRomData.Palette.TemporaryDarkColor.ToWord();
        data[12] = GameplayMessageRomData.Palette.TemporaryLightIndex;
        data[13] = GameplayMessageRomData.Palette.TemporaryDarkIndex;
        data[25] = (uint)Math.Clamp(layer.FirstScanline, 0, SnesPpuLayout.ScreenHeightPixels);
        data[26] = (uint)Math.Clamp(layer.EndScanline, 0, SnesPpuLayout.ScreenHeightPixels);
        fixed (uint* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.Dispatch(32, 28, 1);
    }

    /// <summary>Composes Mode 7 floor, resolved OBJ ranks, and opaque HUD in physical scanline bands.</summary>
    private void DispatchMode7Gameplay(Mode7GameplayRenderLayer layer)
    {
        // Clipping is in physical scanlines. Never translate the projection origin
        // to the start of the band: that changes the matrix/scroll relationship.
        int floorStart = layer.Floor?.FirstScanline ?? 224;
        DispatchMode7(layer.Transform, layer.HudScanlines, floorStart);
        DispatchTile(D3D11TileOperation.InsertObj, new D3D11TileDispatch { FirstScanline = (uint)layer.HudScanlines, EndScanline = (uint)floorStart });
        if (layer.Floor is { } floor)
        {
            // Only the middle band sampled Mode 7. Transparent floor pixels retain
            // backdrop; the same resolved OAM winner is used in every priority pass.
            Objects(0); Objects(1);
            Background(false);
            Objects(2);
            Background(true);
            Objects(3);

            void Objects(uint priority) => DispatchTile(D3D11TileOperation.InsertObj, new D3D11TileDispatch { Priority = priority + 1, FirstScanline = (uint)floorStart });
            void Background(bool high) => DispatchTile(D3D11TileOperation.Bg4, new D3D11TileDispatch { RoomSource = floor.Room is null ? D3D11RoomSource.VramTilemap : D3D11RoomSource.Bg2, Map = (uint)floor.TilemapWord, Characters = (uint)floor.CharacterWord, Scroll = floor.Scroll.AsBgPlaneScroll(), MapSize = floor.MapSize, Priority = Priority(high), FirstScanline = (uint)floorStart });
        }
        // HUD color zero is opaque, and OBJ never overlays this band.
        if (layer.HudScanlines != 0)
            DispatchTile(D3D11TileOperation.Bg2, new D3D11TileDispatch { RoomSource = D3D11RoomSource.Bg3, Map = 0, TransparentZero = 0, EndScanline = (uint)layer.HudScanlines });
    }

    /// <summary>Composes ordinary gameplay in Mode-1 priority order, splitting bands at layer-enable changes.</summary>
    [PpuLayoutBoundary("backend: Direct3D 11 PPU dispatch of the ordinary gameplay layers")]
    private void DispatchOrdinaryGameplay(OrdinaryGameplayRenderLayer layer)
    {
        OrdinaryGameplayRegisters r = layer.Registers;
        // Preserve the Mode-1 priority ladder. Resolving OAM before these passes
        // prevents a hidden higher-index sprite from reappearing at another rank.
        int first = SnesPpuLayout.GameplayHudHeightPixels, end;
        SnesMainScreenLayers activeLayers;
        while (first < SnesPpuLayout.ScreenHeightPixels)
        {
            activeLayers = layer.MainScreenLayersByLine.IsEmpty ? r.MainScreenLayers
                : (SnesMainScreenLayers)layer.MainScreenLayersByLine[first - SnesPpuLayout.GameplayHudHeightPixels];
            end = first + 1;
            while (end < SnesPpuLayout.ScreenHeightPixels && (layer.MainScreenLayersByLine.IsEmpty ||
                layer.MainScreenLayersByLine[end - SnesPpuLayout.GameplayHudHeightPixels] == (ushort)activeLayers)) end++;
            Objects(0); Objects(1); Background2(false); Background1(false);
            Objects(2); Background2(true); Background1(true); Objects(3);
            first = end;
        }
        DispatchTile(D3D11TileOperation.Bg2, new D3D11TileDispatch { RoomSource = D3D11RoomSource.Bg3, Map = 0, TransparentZero = 0, EndScanline = SnesPpuLayout.GameplayHudHeightPixels, Windows = r.Windows, WindowMask = r.MainScreenWindowMask, WindowTarget = SnesWindowTarget.Bg3 });

        void Objects(uint priority)
        {
            if ((activeLayers & SnesMainScreenLayers.Obj) != 0)
                DispatchTile(D3D11TileOperation.InsertObj, new D3D11TileDispatch { Priority = priority + 1, FirstScanline = (uint)first, EndScanline = (uint)end, Windows = r.Windows, WindowMask = r.MainScreenWindowMask, WindowTarget = SnesWindowTarget.Obj });
        }
        void Background1(bool high)
        {
            if ((activeLayers & SnesMainScreenLayers.Bg1) != 0)
                DispatchTile(D3D11TileOperation.Bg4, new D3D11TileDispatch { RoomSource = D3D11RoomSource.Bg1, Characters = (uint)r.Bg1CharacterWord, Scroll = r.Bg1Scroll.AsBgPlaneScroll() + new Offset16<BgPlanePixels>(0, SnesPpuLayout.FirstVisibleBackgroundScanline), MapSize = new Extent16<TilemapCells>(64, 32), Priority = Priority(high), FirstScanline = (uint)first, EndScanline = (uint)end, Windows = r.Windows, WindowMask = r.MainScreenWindowMask, WindowTarget = SnesWindowTarget.Bg1 });
        }
        void Background2(bool high)
        {
            if ((activeLayers & SnesMainScreenLayers.Bg2) != 0) DispatchGameplayBg2(layer, high, first, end);
        }
    }

    /// <summary>Draws one BG2 priority band with HDMA scroll, mosaic, and window masking.</summary>
    private unsafe void DispatchGameplayBg2(OrdinaryGameplayRenderLayer layer, bool high, int first, int end)
    {
        var r = layer.Registers;
        var data = ClearUploadConstants();
        data[0] = (uint)D3D11TileOperation.Bg4;
        data[1] = (uint)r.Bg2TilemapWord; data[2] = (uint)r.Bg2CharacterWord;
        data[5] = r.Bg2MapSize.X; data[6] = r.Bg2MapSize.Y;
        data[7] = Priority(high); data[8] = 1; data[15] = 1;
        data[14] = D3D11RoomSources.Bg2Of(layer.Rooms);
        data[25] = (uint)Math.Max(r.Bg2FirstScanline, first); data[26] = (uint)Math.Min(r.Bg2EndScanline, end);
        // Header word 27 is the BG4 gameplay mosaic width (zero elsewhere).
        data[27] = (uint)r.Bg2Mosaic.Size;
        SetWindowConstants(data, r.Windows, r.MainScreenWindowMask, SnesWindowTarget.Bg2);
        for (int y = SnesPpuLayout.GameplayHudHeightPixels; y < SnesPpuLayout.ScreenHeightPixels; y++)
        {
            int line = y - SnesPpuLayout.GameplayHudHeightPixels;
            int offset = D3D11ShaderLayout.ScanlineParametersWordOffset + y * 4;
            var scroll = layer.Bg2ScrollAt(line);
            data[offset] = scroll.X;
            data[offset + 1] = unchecked((ushort)(scroll.Y + SnesPpuLayout.FirstVisibleBackgroundScanline));
        }
        fixed (uint* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.Dispatch(32, 28, 1);
    }

    /// <summary>Uploads BG subscreen scroll and main-coverage data for color addition.</summary>
    private unsafe void DispatchSubscreen(BgSubscreenAddRenderLayer layer)
    {
        var data = ClearUploadConstants();
        data[0] = (uint)D3D11TileOperation.SubscreenAdd;
        data[1] = roomLevels.ScreenMap(layer.Cells);
        data[4] = (uint)layer.VerticalScroll;
        data[5] = 32; data[6] = 32; data[8] = 1; data[26] = 224;
        data[14] = (uint)D3D11RoomSource.ScreenTiles | roomLevels.ScreenTable(layer.Cells.Tiles) << 16;
        data[D3D11ShaderLayout.CoverageTableWord] = layer.MainCoverage is { } covering ? roomLevels.ScreenTable(covering.Cells.Tiles) : 0;
        data[10] = layer.IncludeObjects ? 1u : 0u;
        data[11] = layer.MainObjects ? 1u : 0u;
        if (layer.MainCoverage is { } coverage)
        {
            // Subscreen operations do not use the Mode-7 matrix header. Keep coverage
            // there so all physical scanline rows remain available to HDMA scrolls.
            int offset = 16;
            data[offset] = roomLevels.ScreenMap(coverage.Cells);
            data[offset + 2] = coverage.Scroll.X; data[offset + 3] = coverage.Scroll.Y;
            data[offset + 4] = coverage.MapSize.X; data[offset + 5] = coverage.MapSize.Y;
            data[offset + 6] = Priority(coverage.Priority); data[offset + 7] = 1;
        }
        if (!layer.Scrolls.IsEmpty)
        {
            data[15] = 1;
            for (int line = 0; line < layer.Scrolls.Length; line++)
            {
                int offset = D3D11ShaderLayout.ScanlineParametersWordOffset + line * 4;
                data[offset] = layer.Scrolls[line].X;
                data[offset + 1] = layer.Scrolls[line].Y;
            }
        }
        fixed (uint* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.Dispatch(32, 28, 1);
    }

    /// <summary>The cells a frontend layer draws (#1403): its own and, for a subscreen, its main coverage's.</summary>
    private static IEnumerable<ScreenTileMap> ScreenMaps(RenderLayer layer) => layer switch
    {
        Bg4BppRenderLayer bg => [bg.Cells],
        Bg2BppViewportRenderLayer bg => [bg.Cells],
        BgSubscreenAddRenderLayer { MainCoverage: { } coverage } sub => [sub.Cells, coverage.Cells],
        BgSubscreenAddRenderLayer sub => [sub.Cells],
        _ => [],
    };

    /// <summary>
    /// Dispatches a frontend tile layer's cells (#1403), each drawn from its tile's pre-coloured picture, colour zero
    /// transparent unless <paramref name="transparentZero"/> is false.
    /// </summary>
    private void DispatchScreenTiles(D3D11TileOperation operation, ScreenTileMap cells, Position16<BgPlanePixels> scroll, bool? priority,
        bool transparentZero = true) =>
        DispatchTile(operation, new D3D11TileDispatch
        {
            Map = roomLevels.ScreenMap(cells), Scroll = scroll, MapSize = cells.Size, Priority = Priority(priority),
            TransparentZero = transparentZero ? 1u : 0u, RoomSource = D3D11RoomSource.ScreenTiles, ScreenTable = roomLevels.ScreenTable(cells.Tiles),
        });

    /// <summary>Rejects unsupported layers, uploads PPU memory once, then dispatches passes in scene order.</summary>
    private unsafe void DrawLayers(RenderFrameSnapshot packet, LayeredRenderSnapshot scene,
        ReadOnlySpan<SuperMetroid.Core.Rendering.TitleGradientLine> gradient = default)
    {
        // Reject unimplemented operations before changing GPU state. No CPU fallback.
        foreach (RenderLayer layer in scene.Layers)
            if (layer is not (Bg4BppRenderLayer or Bg2BppViewportRenderLayer or FixedColorAddRenderLayer or ObjRenderLayer or ObjPriorityRenderLayer or Mode7RenderLayer or Mode7GameplayRenderLayer or ScanlineColorAddRenderLayer or Bg2BppColorMathRenderLayer or BgSubscreenAddRenderLayer or MessageBoxRenderLayer or OrdinaryGameplayRenderLayer or WindowedSceneRenderLayer or GameplayColorMathRenderLayer))
                throw new NotSupportedException($"GPU layer {layer.GetType().Name} is not implemented yet.");
        var memory = memoryUpload;
        MemoryMarshal.Cast<byte, uint>(scene.Memory.Vram).CopyTo(memory);
        for (int i = 0; i < scene.Memory.Cgram.Length; i++) memory[D3D11ShaderLayout.VramPackedWords + i] = scene.Memory.Cgram[i].ToWord();
        fixed (uint* source = memory) UploadBuffer(memoryBuffer, (nint)source, memory.Length * sizeof(uint));
        spriteDraws.Pack(scene.Memory.SpriteDraws.AsSpan(), scene.Memory.Cgram);
        fixed (uint* source = spriteDraws.Upload) UploadBuffer(spriteDraws.Buffer, (nint)source, spriteDraws.Upload.Length * sizeof(uint));
        SuperMetroid.Core.Assets.SpriteSheetFrame? messagePicture = null;
        foreach (RenderLayer layer in scene.Layers)
        {
            if (layer is MessageBoxRenderLayer message)
                messagePicture = messagePicture is null || ReferenceEquals(messagePicture, message.Picture) ? message.Picture
                    : throw new InvalidOperationException("A frame shows two different message boxes.");
        }
        // The gameplay base layer publishes the room backgrounds; a Mode-7 floor band shows the room's layer-2 plane.
        RoomBackgrounds? rooms = scene.Layers.IsEmpty ? null : scene.Layers[0] switch
        {
            OrdinaryGameplayRenderLayer ordinary => ordinary.Rooms,
            GameplayColorMathRenderLayer gameplay => gameplay.Gameplay.Rooms,
            _ => null,
        };
        RoomBackgroundLayer? floor = null;
        Layer3Cells? bg3 = rooms?.Bg3;
        AffineFloor? mode7Map = null;
        // A frame shows one Mode 7 floor, which every Mode 7 dispatch draws from the room buffer (#1403).
        AffineFloor? SameFloor(AffineFloor? shown, AffineFloor floor) => shown is null || shown.Equals(floor) ? floor
            : throw new InvalidOperationException("A frame shows two different Mode 7 floors.");
        List<ScreenTileMap> screens = frameScreens;
        screens.Clear();
        foreach (RenderLayer layer in scene.Layers)
        {
            if (layer is Mode7RenderLayer frontendMode7)
                mode7Map = SameFloor(mode7Map, frontendMode7.Floor);
            // The frontend layers' cells (#1403), each once.
            foreach (ScreenTileMap cells in ScreenMaps(layer))
            {
                if (!screens.Contains(cells))
                    screens.Add(cells);
            }
            if (layer is Mode7GameplayRenderLayer mode7)
            {
                floor = mode7.Floor?.Room ?? floor;
                bg3 = SameBg3(bg3, mode7.Hud);
                mode7Map = SameFloor(mode7Map, mode7.Map);
            }
            else if (layer is Bg2BppColorMathRenderLayer math)
                bg3 = SameBg3(bg3, math.Plane);
            else if (layer is GameplayColorMathRenderLayer { Subscreen: { } sub })
                bg3 = SameBg3(bg3, sub.Plane);
        }
        if (roomLevels.Pack(new RoomLevelLayers(rooms?.Bg1, rooms?.Bg2 ?? floor, rooms?.DoorSource, rooms?.Bg1Overlay, rooms?.Sky, bg3,
                rooms?.Bg2Overlay, mode7Map, rooms?.BossBg2, rooms?.XrayReveal, rooms?.Bg2Image,
                messagePicture, screens.Count == 0 ? null : new ScreenPictureTables([.. screens]))))
            fixed (uint* source = roomLevels.Upload) UploadBuffer(roomLevels.Buffer, (nint)source, roomLevels.Upload.Length * sizeof(uint));
        owner.Context.CSSetShader(tileShader);
        owner.Context.CSSetConstantBuffer(0, constants);
        owner.Context.CSSetShaderResource(0, memoryView);
        owner.Context.CSSetShaderResource(1, spriteDraws.View);
        owner.Context.CSSetShaderResource(3, roomLevels.View);
        owner.Context.CSSetUnorderedAccessView(0, view);
        owner.Context.CSSetUnorderedAccessView(1, objectView);
        DispatchTile(D3D11TileOperation.Backdrop, new D3D11TileDispatch());
        bool needsObjects = false;
        foreach (RenderLayer layer in scene.Layers) needsObjects |= layer is ObjRenderLayer or ObjPriorityRenderLayer or Mode7GameplayRenderLayer or OrdinaryGameplayRenderLayer or Mode7RenderLayer { SubtractObjSubscreen: true } or BgSubscreenAddRenderLayer { IncludeObjects: true };
        if (needsObjects) DispatchTile(D3D11TileOperation.ResolveObj, new D3D11TileDispatch());
        foreach (RenderLayer layer in scene.Layers)
        {
            switch (layer)
            {
                case GameplayColorMathRenderLayer gameplayXray:
                    DispatchGameplayColorMath(scene, gameplayXray);
                    break;
                case WindowedSceneRenderLayer window:
                    DrawWindow(packet, window);
                    break;
                case OrdinaryGameplayRenderLayer ordinary:
                    DispatchOrdinaryGameplay(ordinary);
                    break;
                case MessageBoxRenderLayer message:
                    DispatchMessage(message);
                    break;
                case BgSubscreenAddRenderLayer sub:
                    DispatchSubscreen(sub);
                    break;
                case Bg2BppColorMathRenderLayer math:
                    DispatchBackgroundMath(math);
                    break;
                case ScanlineColorAddRenderLayer windows:
                    DispatchColorWindows(windows);
                    break;
                case Mode7GameplayRenderLayer gameplay:
                    DispatchMode7Gameplay(gameplay);
                    break;
                case Mode7RenderLayer mode7:
                    DispatchMode7(mode7.Transform, subtractObj: mode7.SubtractObjSubscreen, addBg1: mode7.AddBg1Subscreen);
                    break;
                case ObjRenderLayer objLayer:
                    DispatchTile(D3D11TileOperation.InsertObj, new D3D11TileDispatch { Red = objLayer.AddToScreen ? 1u : 0u, Green = objLayer.FixedColor is not null ? 1u : 0u, Blue = objLayer.FixedColor is { } color ? (uint)(color.Red | color.Green << 5 | color.Blue << 10) : 0 });
                    break;
                case ObjPriorityRenderLayer obj:
                    DispatchTile(D3D11TileOperation.InsertObj, new D3D11TileDispatch { Priority = (uint)obj.Priority + 1, Green = obj.FixedColor is not null ? 1u : 0u, Blue = obj.FixedColor is { } priorityColor ? (uint)(priorityColor.Red | priorityColor.Green << 5 | priorityColor.Blue << 10) : 0 });
                    break;
                case Bg4BppRenderLayer bg:
                    DispatchScreenTiles(D3D11TileOperation.Bg4, bg.Cells, bg.Scroll, bg.Priority);
                    break;
                case Bg2BppViewportRenderLayer bg:
                    DispatchScreenTiles(D3D11TileOperation.Bg2, bg.Cells, new Position16<BgPlanePixels>(0, checked((ushort)bg.VerticalScroll)), bg.Priority, bg.TransparentColorZero);
                    break;
                case FixedColorAddRenderLayer add:
                    DispatchTile(D3D11TileOperation.FixedAdd, new D3D11TileDispatch { Red = (uint)add.Red, Green = (uint)add.Green, Blue = (uint)add.Blue });
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(RenderLayer)} {layer}.");
            }
        }
        if (!gradient.IsEmpty) DispatchTitleGradient(gradient);
        DispatchTile(D3D11TileOperation.Brightness, new D3D11TileDispatch { Level = (uint)scene.Brightness });
        foreach (FadeLevel fade in packet.Fades) DispatchTile(D3D11TileOperation.Brightness, new D3D11TileDispatch { Level = (uint)fade.Brightness });
    }

    /// <summary>Packs one tile operation's full constant buffer and dispatches the native screen grid.</summary>
    private unsafe void DispatchTile(D3D11TileOperation operation, D3D11TileDispatch tile)
    {
        // Upload the complete allocated cbuffer, so UpdateSubresource cannot read past
        // a short managed array. The trailing header words supply scanline clipping.
        // The tile shader's header words three through six are the BG scroll and map size;
        // unmapped operations keep the shader's default 32x32-cell plane.
        Extent16<TilemapCells> size = tile.MapSize ?? new Extent16<TilemapCells>(32, 32);
        var data = ClearUploadConstants();
        data[0] = (uint)operation; data[1] = tile.Map; data[2] = tile.Characters; data[3] = tile.Scroll.X;
        data[4] = tile.Scroll.Y; data[5] = size.X; data[6] = size.Y; data[7] = tile.Priority;
        data[8] = tile.TransparentZero; data[9] = tile.Level; data[10] = tile.Red; data[11] = tile.Green; data[12] = tile.Blue;
        data[14] = (uint)tile.RoomSource | tile.ScreenTable << 16;
        data[25] = tile.FirstScanline; data[26] = tile.EndScanline;
        SetWindowConstants(data, tile.Windows, tile.WindowMask, tile.WindowTarget);
        fixed (uint* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.Dispatch(32, 28, 1);
    }

    /// <summary>Encodes unspecified, low, or high tile priority for the compute shader.</summary>
    private static uint Priority(bool? priority) => priority is null ? 0u : priority.Value ? 2u : 1u;

    /// <summary>
    /// Uploads the frame's floor transform: signed matrix, center, scroll, what shows outside the floor, and optional
    /// color math. The floor's map and tile pictures are in the room buffer (#1403).
    /// </summary>
    private unsafe void DispatchMode7(AffineTransform registers, int firstScanline = 0, int endScanline = 224,
        bool subtractObj = false, bool addBg1 = false)
    {
        // Preserve signed register values, including negative products and arithmetic
        // right shifts. No projected coordinates or raster pixels are uploaded.
        var data = MemoryMarshal.Cast<uint, int>(ClearUploadConstants().AsSpan());
        data[0] = (int)D3D11TileOperation.Mode7;
        data[14] = (int)D3D11RoomSource.Mode7Map;
        data[16] = registers.MatrixA; data[17] = registers.MatrixB;
        data[18] = registers.MatrixC; data[19] = registers.MatrixD;
        data[20] = registers.Center.X; data[21] = registers.Center.Y;
        data[22] = registers.Scroll.X; data[23] = registers.Scroll.Y;
        data[24] = registers.Outside switch
        {
            Mode7OverflowMode.Transparent => 0,
            Mode7OverflowMode.CharacterZero => 1,
            Mode7OverflowMode.Wrap => 2,
            _ => throw new ArgumentOutOfRangeException(nameof(registers), registers.Outside, "Unknown Mode 7 overflow."),
        };
        data[25] = firstScanline; data[26] = endScanline;
        data[27] = (int)(addBg1 ? Mode7ColorMathOperation.AddBg1
            : subtractObj ? Mode7ColorMathOperation.SubtractObj : Mode7ColorMathOperation.None);
        fixed (int* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.Dispatch(32, 28, 1);
    }

    /// <summary>Applies authored RGB/control values across title scanlines after tile composition.</summary>
    private unsafe void DispatchTitleGradient(ReadOnlySpan<SuperMetroid.Core.Rendering.TitleGradientLine> lines)
    {
        var data = ClearUploadConstants();
        data[26] = 224;
        for (int y = 0; y < lines.Length; y++)
        {
            int offset = D3D11ShaderLayout.ScanlineParametersWordOffset + y * 4;
            data[offset] = (uint)lines[y].Red; data[offset + 1] = (uint)lines[y].Green;
            data[offset + 2] = (uint)lines[y].Blue; data[offset + 3] = lines[y].Control.ToNativeByte();
        }
        fixed (uint* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.CSSetShader(titleGradientShader);
        owner.Context.Dispatch(32, 28, 1);
        owner.Context.CSSetShader(tileShader);
    }

    /// <summary>CPU-side byte, call, and timestamp totals for UpdateSubresource uploads.</summary>
    private long uploadBytes, uploadCalls, uploadTicks;

    /// <summary>Owner-thread cumulative CPU upload submission, including reusable child scenes.
    /// This measures UpdateSubresource calls, not GPU transfer completion or memory packing.</summary>
    public RenderUploadStatistics UploadStatistics
    {
        get
        {
            owner.VerifyOwner();
            var child = childRenderer?.UploadStatistics ?? default;
            return new(uploadBytes + child.Bytes, uploadCalls + child.Calls,
                uploadTicks * 1000.0 / Stopwatch.Frequency + child.CpuMilliseconds);
        }
    }

    /// <summary>Copies a packed CPU span to a GPU buffer and records submission cost.</summary>
    private void UploadBuffer(ID3D11Buffer buffer, nint source, int bytes)
    {
        long started = Stopwatch.GetTimestamp();
        owner.Context.UpdateSubresource(buffer, 0, null, source, 0, 0);
        uploadTicks += Stopwatch.GetTimestamp() - started;
        uploadBytes += bytes;
        uploadCalls++;
    }

    /// <summary>Reusable independent renderer for a single bounded child window.</summary>
    private D3D11FrameRenderer? childRenderer;

    /// <summary>Renders a non-nested child scene and copies its selected rectangle into the parent output.</summary>
    private void DrawWindow(RenderFrameSnapshot packet, WindowedSceneRenderLayer window)
    {
        if (window.Left == window.Right || window.Top == window.Bottom) return;
        // A single reusable child owns distinct memory, resolved OAM and output.
        // The contract prohibits nested windows, bounding scratch resource usage.
        childRenderer ??= Own(new D3D11FrameRenderer(owner));
        var childPacket = new RenderFrameSnapshot(packet.Identity, window.Scene);
        childRenderer.DrawLayers(childPacket, window.Scene);
        owner.Context.CSSetUnorderedAccessView(0, null);
        owner.Context.CSSetUnorderedAccessView(1, null);
        owner.Context.CopySubresourceRegion(output, 0, (uint)window.Left, (uint)window.Top, 0,
            childRenderer.output, 0, new Box(window.Left, window.Top, 0, window.Right, window.Bottom, 1));
        // The immediate context is shared but every scene's resources are separate.
        // Restore the parent before later overlays, OBJ insertion, or outer fades.
        owner.Context.CSSetShader(tileShader);
        owner.Context.CSSetConstantBuffer(0, constants);
        owner.Context.CSSetShaderResource(0, memoryView);
        owner.Context.CSSetShaderResource(1, spriteDraws.View);
        owner.Context.CSSetShaderResource(3, roomLevels.View);
        owner.Context.CSSetUnorderedAccessView(0, view);
        owner.Context.CSSetUnorderedAccessView(1, objectView);
    }

    /// <summary>Packs the selected BG or OBJ window logic, edges, and enable bit when active.</summary>
    private static void SetWindowConstants(Span<uint> data, SnesWindowRegisters windows,
        SnesMainScreenLayers enabled, SnesWindowTarget target)
    {
        int index = (int)target;
        if (index > (int)SnesWindowTarget.Obj)
            throw new ArgumentOutOfRangeException(nameof(target));
        if ((enabled & (SnesMainScreenLayers)(1 << index)) == 0) return;
        SnesWindowSelectionPair selection = target switch
        {
            SnesWindowTarget.Bg1 or SnesWindowTarget.Bg2 => windows.Window12Selection,
            SnesWindowTarget.Bg3 or SnesWindowTarget.Bg4 => windows.Window34Selection,
            SnesWindowTarget.Obj or SnesWindowTarget.ColorMath => windows.ObjectColorSelection,
            var undefined => throw new InvalidOperationException($"Undefined {nameof(SnesWindowTarget)} {undefined}."),
        };
        data[D3D11ShaderLayout.WindowSelectionWord] = (uint)selection.Layer((index & 1) != 0);
        data[D3D11ShaderLayout.WindowLogicWord] = (uint)(target == SnesWindowTarget.Obj
            ? windows.ObjectColorLogic : windows.BackgroundLogic >> (index * 2)) & 3;
        data[D3D11ShaderLayout.WindowEdgesWord] = D3D11ConstantPacking.Bytes(
            checked((byte)windows.FirstLeft), checked((byte)windows.FirstRight), checked((byte)windows.SecondLeft),
            checked((byte)windows.SecondRight));
        data[D3D11ShaderLayout.WindowEnabledWord] = 1;
    }
}
