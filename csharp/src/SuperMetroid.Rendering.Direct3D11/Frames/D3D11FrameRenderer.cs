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
            if (mode7.Background is { } background) operations.Add(new Mode7RenderLayer(background));
            operations.Add(new ObjRenderLayer());
            DrawLayers(packet, new LayeredRenderSnapshot(mode7.Memory,
                operations.ToArray(), mode7.ObjectSelection, mode7.Brightness), mode7.Gradient);
            return;
        }
        Rgba32 color = packet.SolidColor ?? throw new NotSupportedException("This compute path does not yet support the requested composition.");
        if (packet.BrightnessPasses.Length > D3D11ShaderLayout.MaximumBrightnessPasses) throw new ArgumentOutOfRangeException(nameof(packet));
        uint[] data = ClearUploadConstants();
        data[0] = D3D11ConstantPacking.Bytes(color.R, color.G, color.B, color.A);
        data[1] = (uint)packet.BrightnessPasses.Length;
        data[2] = packet.Size.X; data[3] = packet.Size.Y;
        for (int i = 0; i < packet.BrightnessPasses.Length; i++) data[D3D11ShaderLayout.SolidHeaderWords + i] = packet.BrightnessPasses[i];
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

    /// <summary>Uploads BG2 scanline scrolls for add/subtract color math.</summary>
    private unsafe void DispatchBackgroundMath(Bg2BppColorMathRenderLayer layer)
    {
        var data = ClearUploadConstants();
        data[0] = (uint)(layer.Operation == ExpandedColorMathOperation.Add
            ? D3D11TileOperation.BgAdd : D3D11TileOperation.BgSubtract);
        data[1] = layer.TilemapWord; data[2] = layer.CharacterWord;
        data[5] = 32; data[6] = (uint)layer.MapHeightTiles; data[8] = 1;
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
            data[offset] = (uint)(SnesLittleEndian.Word(line.Left, line.Right));
            data[offset + 1] = line.Red;
            data[offset + 2] = line.Green;
            data[offset + 3] = line.Blue;
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
        data[1] = r.Bg1CharacterWord; data[2] = r.Bg2CharacterWord;
        data[3] = r.Bg1Scroll.X;
        data[4] = unchecked((ushort)(r.Bg1Scroll.Y + SnesPpuLayout.FirstVisibleBackgroundScanline));
        data[5] = (uint)r.Bg2MapSize.X; data[6] = (uint)r.Bg2MapSize.Y;
        data[7] = (uint)r.MainScreenLayers; data[8] = layer.RevealBlocks ? 1u : 0u;
        data[9] = (uint)layer.ColorMath;
        data[10] = layer.FixedRed; data[11] = layer.FixedGreen; data[12] = layer.FixedBlue;
        data[13] = (uint)scene.Memory.ModeledSpriteCount; data[14] = scene.ObjectSelection;
        data[16] = r.Bg2TilemapWord; data[17] = r.HudCharacterWord;
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
            data[18] = sub.TilemapWord; data[19] = sub.CharacterWord;
            data[20] = (uint)sub.MapHeightTiles; data[21] = (uint)sub.FirstScanline;
        }
        for (int y = 32; y < 224; y++)
        {
            int offset = D3D11ShaderLayout.ScanlineParametersWordOffset + y * 4, line = y - 32;
            var scroll = layer.Gameplay.Bg2ScrollAt(line);
            data[offset] = scroll.X;
            data[offset + 1] = unchecked((ushort)(scroll.Y + SnesPpuLayout.FirstVisibleBackgroundScanline));
            data[offset + 2] = D3D11ConstantPacking.Bytes(layer.Lines[y].Left, layer.Lines[y].Right, 0, 0);
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

    /// <summary>Uploads centered message tile rows and the expanding window's scanline bounds.</summary>
    private unsafe void DispatchMessage(MessageBoxRenderLayer layer)
    {
        var data = ClearUploadConstants();
        data[0] = (uint)D3D11TileOperation.Message;
        data[1] = (uint)(GameplayMessageRomData.Layout.WindowCenterY - layer.RowCount * 4);
        data[2] = GameplayMessageRomData.Layout.CharacterBaseWord;
        data[3] = (uint)layer.RowCount;
        data[10] = GameplayMessageRomData.Palette.TemporaryLightColor.ToWord();
        data[11] = GameplayMessageRomData.Palette.TemporaryDarkColor.ToWord();
        data[12] = GameplayMessageRomData.Palette.TemporaryLightIndex;
        data[13] = GameplayMessageRomData.Palette.TemporaryDarkIndex;
        data[25] = (uint)(GameplayMessageRomData.Layout.WindowCenterY - layer.RadiusPixels);
        data[26] = (uint)(GameplayMessageRomData.Layout.WindowCenterY + layer.RadiusPixels);
        for (int i = 0; i < layer.Tilemap.Length; i++)
            data[D3D11ShaderLayout.ScanlineParametersWordOffset + i * 4] = layer.Tilemap[i];
        fixed (uint* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.Dispatch(32, 28, 1);
    }

    /// <summary>Composes Mode 7 floor, resolved OBJ ranks, and opaque HUD in physical scanline bands.</summary>
    private void DispatchMode7Gameplay(Mode7GameplayRenderLayer layer)
    {
        // Clipping is in physical scanlines. Never translate the projection origin
        // to the start of the band: that changes the matrix/scroll relationship.
        int floorStart = layer.Floor?.FirstScanline ?? 224;
        DispatchMode7(layer.Registers, layer.HudScanlines, floorStart);
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
            void Background(bool high) => DispatchTile(D3D11TileOperation.Bg4, new D3D11TileDispatch { Map = floor.TilemapWord, Characters = floor.CharacterWord, Scroll = floor.Scroll.AsBgPlaneScroll(), MapSize = floor.MapSize, Priority = Priority(high), FirstScanline = (uint)floorStart });
        }
        // HUD color zero is opaque, and OBJ never overlays this band.
        if (layer.HudScanlines != 0)
            DispatchTile(D3D11TileOperation.Bg2, new D3D11TileDispatch { Map = layer.HudTilemapWord, Characters = layer.HudCharacterWord, TransparentZero = 0, EndScanline = (uint)layer.HudScanlines });
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
        DispatchTile(D3D11TileOperation.Bg2, new D3D11TileDispatch { Map = GameplayVramLayout.HudTilemapWord, Characters = r.HudCharacterWord, TransparentZero = 0, EndScanline = SnesPpuLayout.GameplayHudHeightPixels, Windows = r.Windows, WindowMask = r.MainScreenWindowMask, WindowTarget = SnesWindowTarget.Bg3 });

        void Objects(uint priority)
        {
            if ((activeLayers & SnesMainScreenLayers.Obj) != 0)
                DispatchTile(D3D11TileOperation.InsertObj, new D3D11TileDispatch { Priority = priority + 1, FirstScanline = (uint)first, EndScanline = (uint)end, Windows = r.Windows, WindowMask = r.MainScreenWindowMask, WindowTarget = SnesWindowTarget.Obj });
        }
        void Background1(bool high)
        {
            if ((activeLayers & SnesMainScreenLayers.Bg1) != 0)
                DispatchTile(D3D11TileOperation.Bg4, new D3D11TileDispatch { Map = GameplayVramLayout.Bg1TilemapWord, Characters = r.Bg1CharacterWord, Scroll = r.Bg1Scroll.AsBgPlaneScroll() + new Offset16<BgPlanePixels>(0, SnesPpuLayout.FirstVisibleBackgroundScanline), MapSize = new Extent16<TilemapCells>(64, 32), Priority = Priority(high), FirstScanline = (uint)first, EndScanline = (uint)end, Windows = r.Windows, WindowMask = r.MainScreenWindowMask, WindowTarget = SnesWindowTarget.Bg1 });
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
        data[1] = r.Bg2TilemapWord; data[2] = r.Bg2CharacterWord;
        data[5] = (uint)r.Bg2MapSize.X; data[6] = (uint)r.Bg2MapSize.Y;
        data[7] = Priority(high); data[8] = 1; data[15] = 1;
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
    private unsafe void DispatchSubscreen(BgSubscreenAddRenderLayer layer, uint objectCount, byte objectSelection)
    {
        var data = ClearUploadConstants();
        data[0] = (uint)D3D11TileOperation.SubscreenAdd;
        data[1] = layer.TilemapWord; data[2] = layer.CharacterWord;
        data[4] = layer.VerticalScroll;
        data[5] = 32; data[6] = 32; data[8] = 1; data[26] = 224;
        data[27] = layer.FourBpp ? 1u : 0u;
        data[10] = layer.IncludeObjects ? 1u : 0u;
        data[11] = layer.MainObjects ? 1u : 0u;
        data[13] = objectCount; data[14] = objectSelection;
        if (layer.MainCoverage is { } coverage)
        {
            // Subscreen operations do not use the Mode-7 matrix header. Keep coverage
            // there so all physical scanline rows remain available to HDMA scrolls.
            int offset = 16;
            data[offset] = coverage.TilemapWord; data[offset + 1] = coverage.CharacterWord;
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

    /// <summary>Rejects unsupported layers, uploads PPU memory once, then dispatches passes in scene order.</summary>
    private unsafe void DrawLayers(RenderFrameSnapshot packet, LayeredRenderSnapshot scene,
        ReadOnlySpan<SuperMetroid.Core.Rendering.TitleGradientLine> gradient = default)
    {
        // Reject unimplemented operations before changing GPU state. No CPU fallback.
        foreach (RenderLayer layer in scene.Layers)
            if (layer is not (Bg4BppRenderLayer or Bg2BppRenderLayer or Bg2BppViewportRenderLayer or FixedColorAddRenderLayer or ObjRenderLayer or ObjPriorityRenderLayer or Mode7RenderLayer or Mode7GameplayRenderLayer or ScanlineColorAddRenderLayer or Bg2BppColorMathRenderLayer or BgSubscreenAddRenderLayer or MessageBoxRenderLayer or OrdinaryGameplayRenderLayer or WindowedSceneRenderLayer or GameplayColorMathRenderLayer))
                throw new NotSupportedException($"GPU layer {layer.GetType().Name} is not implemented yet.");
        var memory = memoryUpload;
        MemoryMarshal.Cast<byte, uint>(scene.Memory.Vram).CopyTo(memory);
        for (int i = 0; i < scene.Memory.Cgram.Length; i++) memory[D3D11ShaderLayout.VramPackedWords + i] = scene.Memory.Cgram[i].ToWord();
        MemoryMarshal.Cast<byte, uint>(scene.Memory.Oam).CopyTo(memory.AsSpan(D3D11ShaderLayout.OamPackedWordOffset));
        fixed (uint* source = memory) UploadBuffer(memoryBuffer, (nint)source, memory.Length * sizeof(uint));
        spriteDraws.Pack(scene.Memory.SpriteDraws.AsSpan());
        fixed (uint* source = spriteDraws.Upload) UploadBuffer(spriteDraws.Buffer, (nint)source, spriteDraws.Upload.Length * sizeof(uint));
        owner.Context.CSSetShader(tileShader);
        owner.Context.CSSetConstantBuffer(0, constants);
        owner.Context.CSSetShaderResource(0, memoryView);
        owner.Context.CSSetShaderResource(1, spriteDraws.View);
        owner.Context.CSSetUnorderedAccessView(0, view);
        owner.Context.CSSetUnorderedAccessView(1, objectView);
        DispatchTile(D3D11TileOperation.Backdrop, new D3D11TileDispatch());
        bool needsObjects = false;
        foreach (RenderLayer layer in scene.Layers) needsObjects |= layer is ObjRenderLayer or ObjPriorityRenderLayer or Mode7GameplayRenderLayer or OrdinaryGameplayRenderLayer or Mode7RenderLayer { SubtractObjSubscreen: true } or BgSubscreenAddRenderLayer { IncludeObjects: true };
        if (needsObjects) DispatchTile(D3D11TileOperation.ResolveObj, new D3D11TileDispatch { ObjectCount = (uint)scene.Memory.ModeledSpriteCount, ObjectSelection = scene.ObjectSelection });
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
                    DispatchSubscreen(sub, (uint)scene.Memory.ModeledSpriteCount, scene.ObjectSelection);
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
                    DispatchMode7(mode7.Registers, subtractObj: mode7.SubtractObjSubscreen, addBg1: mode7.AddBg1Subscreen,
                        objectCount: scene.Memory.ModeledSpriteCount, objectSelection: scene.ObjectSelection);
                    break;
                case ObjRenderLayer objLayer:
                    DispatchTile(D3D11TileOperation.InsertObj, new D3D11TileDispatch { Red = objLayer.AddToScreen ? 1u : 0u, Green = objLayer.FixedColor is not null ? 1u : 0u, Blue = objLayer.FixedColor is { } color ? (uint)(color.Red | color.Green << 5 | color.Blue << 10) : 0, ObjectCount = (uint)scene.Memory.ModeledSpriteCount, ObjectSelection = scene.ObjectSelection });
                    break;
                case ObjPriorityRenderLayer obj:
                    DispatchTile(D3D11TileOperation.InsertObj, new D3D11TileDispatch { Priority = (uint)obj.Priority + 1, Green = obj.FixedColor is not null ? 1u : 0u, Blue = obj.FixedColor is { } priorityColor ? (uint)(priorityColor.Red | priorityColor.Green << 5 | priorityColor.Blue << 10) : 0, ObjectCount = (uint)scene.Memory.ModeledSpriteCount, ObjectSelection = scene.ObjectSelection });
                    break;
                case Bg4BppRenderLayer bg:
                    DispatchTile(D3D11TileOperation.Bg4, new D3D11TileDispatch { Map = bg.TilemapWord, Characters = bg.CharacterWord, Scroll = bg.Scroll, MapSize = bg.MapSize, Priority = Priority(bg.Priority) });
                    break;
                case Bg2BppRenderLayer bg:
                    DispatchTile(D3D11TileOperation.Bg2, new D3D11TileDispatch { Map = bg.TilemapWord, Characters = bg.CharacterWord, Priority = Priority(bg.Priority) });
                    break;
                case Bg2BppViewportRenderLayer bg:
                    DispatchTile(D3D11TileOperation.Bg2, new D3D11TileDispatch { Map = bg.TilemapWord, Characters = bg.CharacterWord, Scroll = new Position16<BgPlanePixels>(0, bg.VerticalScroll), Priority = Priority(bg.Priority), TransparentZero = bg.TransparentColorZero ? 1u : 0u });
                    break;
                case FixedColorAddRenderLayer add:
                    DispatchTile(D3D11TileOperation.FixedAdd, new D3D11TileDispatch { Red = add.Red, Green = add.Green, Blue = add.Blue });
                    break;
            }
        }
        if (!gradient.IsEmpty) DispatchTitleGradient(gradient, scene.Memory.ModeledSpriteCount, scene.ObjectSelection);
        DispatchTile(D3D11TileOperation.Brightness, new D3D11TileDispatch { Level = scene.Brightness });
        foreach (byte level in packet.BrightnessPasses) DispatchTile(D3D11TileOperation.Brightness, new D3D11TileDispatch { Level = level });
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
        data[13] = tile.ObjectCount; data[14] = tile.ObjectSelection;
        data[25] = tile.FirstScanline; data[26] = tile.EndScanline;
        SetWindowConstants(data, tile.Windows, tile.WindowMask, tile.WindowTarget);
        fixed (uint* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.Dispatch(32, 28, 1);
    }

    /// <summary>Encodes unspecified, low, or high tile priority for the compute shader.</summary>
    private static uint Priority(bool? priority) => priority is null ? 0u : priority.Value ? 2u : 1u;

    /// <summary>Uploads signed Mode 7 matrix, center, scroll, overflow, and optional color math.</summary>
    private unsafe void DispatchMode7(Mode7RenderRegisters registers, int firstScanline = 0, int endScanline = 224,
        bool subtractObj = false, bool addBg1 = false, int objectCount = 0, byte objectSelection = 0)
    {
        // Preserve signed register values, including negative products and arithmetic
        // right shifts. No projected coordinates or raster pixels are uploaded.
        var data = MemoryMarshal.Cast<uint, int>(ClearUploadConstants().AsSpan());
        data[0] = (int)D3D11TileOperation.Mode7;
        data[13] = objectCount; data[14] = objectSelection;
        data[16] = registers.MatrixA; data[17] = registers.MatrixB;
        data[18] = registers.MatrixC; data[19] = registers.MatrixD;
        data[20] = registers.Center.X; data[21] = registers.Center.Y;
        data[22] = registers.Scroll.X; data[23] = registers.Scroll.Y;
        data[24] = (int)Mode7OverflowPolicy.FromRegisters(registers);
        data[25] = firstScanline; data[26] = endScanline;
        data[27] = (int)(addBg1 ? Mode7ColorMathOperation.AddBg1
            : subtractObj ? Mode7ColorMathOperation.SubtractObj : Mode7ColorMathOperation.None);
        fixed (int* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.Dispatch(32, 28, 1);
    }

    /// <summary>Applies authored RGB/control values across title scanlines after tile composition.</summary>
    private unsafe void DispatchTitleGradient(ReadOnlySpan<SuperMetroid.Core.Rendering.TitleGradientLine> lines, int objects, byte selection)
    {
        var data = ClearUploadConstants();
        data[13] = (uint)objects; data[14] = selection; data[26] = 224;
        for (int y = 0; y < lines.Length; y++)
        {
            int offset = D3D11ShaderLayout.ScanlineParametersWordOffset + y * 4;
            data[offset] = lines[y].Red; data[offset + 1] = lines[y].Green;
            data[offset + 2] = lines[y].Blue; data[offset + 3] = lines[y].Control;
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
        byte selection = target switch
        {
            SnesWindowTarget.Bg1 or SnesWindowTarget.Bg2 => windows.Window12Selection,
            SnesWindowTarget.Bg3 or SnesWindowTarget.Bg4 => windows.Window34Selection,
            _ => windows.ObjectColorSelection,
        };
        data[D3D11ShaderLayout.WindowSelectionWord] = (uint)(selection >> ((index & 1) * 4)) & 15;
        data[D3D11ShaderLayout.WindowLogicWord] = (uint)(target == SnesWindowTarget.Obj
            ? windows.ObjectColorLogic : windows.BackgroundLogic >> (index * 2)) & 3;
        data[D3D11ShaderLayout.WindowEdgesWord] = D3D11ConstantPacking.Bytes(
            windows.FirstLeft, windows.FirstRight, windows.SecondLeft, windows.SecondRight);
        data[D3D11ShaderLayout.WindowEnabledWord] = 1;
    }
}
