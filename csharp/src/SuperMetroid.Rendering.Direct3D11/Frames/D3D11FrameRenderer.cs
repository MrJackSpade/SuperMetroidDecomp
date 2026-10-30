using SuperMetroid.Core;
using SuperMetroid.Core.Rendering;
using Vortice.Direct3D11;
using Vortice.DXGI;
using SuperMetroid.Core.Spatial;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Vortice.Direct3D;
using Vortice.Mathematics;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Presentation;
using SuperMetroid.Rendering.Direct3D11.Telemetry;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hud;

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
    /// <summary>The composition passes: layers, sprite insertion, the floor, blends and brightness.</summary>
    private readonly ID3D11ComputeShader tileShader;
    /// <summary>Scanline gradient compute program for the title screen.</summary>
    private readonly ID3D11ComputeShader titleGradientShader;
    /// <summary>The scene's backdrop and the colours each of its pictures' images shows, bound at register t0.</summary>
    private readonly D3D11SceneColourBuffer sceneColours;
    /// <summary>The scene's whole-frame sprite draws, bound at register t1.</summary>
    private readonly D3D11SpriteDrawBuffer spriteDraws;
    /// <summary>The scene's room planes at t3, repacked only when they change (#1346).</summary>
    private readonly D3D11RoomLevelBuffer roomLevels;
    /// <summary>Each packed background layer's shader index, for the dispatch that draws it.</summary>
    /// <summary>
    /// The sprite layer (#1408): per pixel, the frontmost sprite pixel's colour and its priority, drawn from the draws'
    /// quads and read by every composition pass that inserts or blends sprites.
    /// </summary>
    private readonly ID3D11Texture2D spriteLayer;
    /// <summary>Composition's view of the sprite layer.</summary>
    private readonly ID3D11UnorderedAccessView objectView;
    /// <summary>The sprite quads' target view of the sprite layer.</summary>
    private readonly ID3D11RenderTargetView spriteTarget;
    /// <summary>Places each sprite draw's quad.</summary>
    private readonly ID3D11VertexShader spriteVertexShader;
    /// <summary>Draws each sprite draw's image into the sprite layer.</summary>
    private readonly ID3D11PixelShader spritePixelShader;
    /// <summary>Draws every quad whichever way it winds.</summary>
    private readonly ID3D11RasterizerState spriteRasterizer;
    /// <summary>The installed images the scene draws, as one texture (#1408).</summary>
    private readonly D3D11ImageAtlas images;
    /// <summary>The perspective floor the scene shows, as one image (#1408).</summary>
    private readonly D3D11FloorImage floorImage;
    /// <summary>Owned GPU resources released in reverse creation order.</summary>
    private readonly List<IDisposable> resources = [];
    /// <summary>Guards later render calls after owner-thread disposal.</summary>
    internal bool disposed;
    /// <summary>Reusable constant-buffer source cleared before each compute pass.</summary>
    private readonly uint[] constantUpload = new uint[D3D11ShaderLayout.ConstantWords];

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

    /// <summary>Creates embedded compute/display shaders and native 256-by-224 output and sprite layer on an existing render-owner device; owns those resources but not the device.</summary>
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
                D3D11Screen.Width, D3D11Screen.Height, 1, 1, BindFlags.UnorderedAccess | BindFlags.ShaderResource)));
            view = Own(owner.Device.CreateUnorderedAccessView(output));
            displaySource = Own(owner.Device.CreateShaderResourceView(output));
            displayVertexShader = Own(owner.Device.CreateVertexShader(LoadShaderBytes(D3D11ShaderLayout.DisplayVertexResourceName)));
            displayPixelShader = Own(owner.Device.CreatePixelShader(LoadShaderBytes(D3D11ShaderLayout.DisplayPixelResourceName)));
            constants = Own(owner.Device.CreateBuffer(new BufferDescription(D3D11ShaderLayout.ConstantWords * sizeof(uint), BindFlags.ConstantBuffer)));
            sceneColours = Own(new D3D11SceneColourBuffer(owner.Device));
            spriteDraws = Own(new D3D11SpriteDrawBuffer(owner.Device));
            roomLevels = Own(new D3D11RoomLevelBuffer(owner.Device));
            spriteLayer = Own(owner.Device.CreateTexture2D(new Texture2DDescription(Format.R32G32_UInt,
                D3D11Screen.Width, D3D11Screen.Height, 1, 1, BindFlags.UnorderedAccess | BindFlags.RenderTarget)));
            objectView = Own(owner.Device.CreateUnorderedAccessView(spriteLayer));
            spriteTarget = Own(owner.Device.CreateRenderTargetView(spriteLayer));
            spriteVertexShader = Own(owner.Device.CreateVertexShader(LoadShaderBytes(D3D11ShaderLayout.SpriteVertexResourceName)));
            spritePixelShader = Own(owner.Device.CreatePixelShader(LoadShaderBytes(D3D11ShaderLayout.SpritePixelResourceName)));
            spriteRasterizer = Own(owner.Device.CreateRasterizerState(new RasterizerDescription(CullMode.None, FillMode.Solid)));
            images = Own(new D3D11ImageAtlas(owner.Device, owner.Context));
            floorImage = Own(new D3D11FloorImage(owner.Device, owner.Context));
        }
        catch { DisposeResources(); throw; }
    }

    /// <summary>Submits GPU composition without staging allocation, readback or a CPU raster.</summary>
    public void Render(DisplayPacket packet)
    {
        owner.VerifyOwner(); ObjectDisposedException.ThrowIf(disposed, this);
        Ensure.NotNull(packet);
        renderedIdentity = null;
        RenderCore(packet);
        renderedIdentity = packet.Identity;
    }

    /// <summary>Selects layered, Mode 7, or solid-color composition and submits its compute passes.</summary>
    private unsafe void RenderCore(DisplayPacket packet)
    {
        if (packet.Layers is { } layers) { DrawLayers(packet, layers); return; }
        if (packet.Floor is { } mode7)
        {
            var operations = new List<DisplayLayer>();
            if (mode7.Background is { } background && mode7.Floor is { } shownFloor) operations.Add(new TurningFloorLayer(background, shownFloor));
            operations.Add(new SpritesLayer());
            DrawLayers(packet, new LayeredScene(mode7.Paint,
                operations.ToArray(), mode7.Brightness), mode7.Gradient);
            return;
        }
        RgbaPixel color = packet.SolidColor ?? throw new NotSupportedException("This compute path does not yet support the requested composition.");
        uint[] data = ClearUploadConstants();
        data[0] = D3D11ConstantPacking.Bytes(color.R, color.G, color.B, color.A);
        data[1] = (uint)packet.Brightness;
        data[2] = packet.Size.X; data[3] = packet.Size.Y;
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

    /// <summary>The one layer-3 plane a frame shows (#1348): every layer that reads layer 3 reads the same plane.</summary>
    private static Layer3Cells SameBg3(Layer3Cells? shown, Layer3Cells plane) =>
        shown is null || ReferenceEquals(shown, plane) ? plane
            : throw new InvalidOperationException("A frame's layers show two different layer-3 planes.");

    /// <summary>Adds or subtracts the room effect's layer-3 rows, each line at its own scroll.</summary>
    private unsafe void DispatchBackgroundMath(OverlayBlendLayer layer)
    {
        var data = ClearUploadConstants();
        data[0] = (uint)(layer.Operation == Core.Rendering.BlendOperation.Add
            ? D3D11TileOperation.AddLayer : D3D11TileOperation.SubtractLayer);
        // The plane's rows shown: the first in the layer word, how many in the map height (#1348).
        data[1] = (uint)layer.FirstRow;
        data[5] = 32; data[6] = (uint)layer.MapHeightTiles; data[8] = 1;
        data[14] = (uint)D3D11RoomSource.Layer3;
        data[25] = (uint)layer.FirstLine; data[26] = 224;
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
    private unsafe void DispatchColorWindows(ColourAddBandsLayer layer)
    {
        var data = ClearUploadConstants();
        data[0] = (uint)D3D11TileOperation.LineColourAdd;
        data[26] = 224;
        for (int y = 0; y < layer.Windows.Length; y++)
        {
            ColorAddWindow line = layer.Windows[y];
            int offset = D3D11ShaderLayout.ScanlineParametersWordOffset + y * 4;
            data[offset] = D3D11ConstantPacking.Bytes(checked((byte)line.Left), checked((byte)line.Right), 0, 0);
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

    /// <summary>
    /// Composes a room under the X-ray: its layers in their order, the reveal's window, the layers each line shows, and
    /// the blend with the fixed colour or the layers behind.
    /// </summary>
    private unsafe void DispatchXrayRoom(GameplayBlendLayer layer)
    {
        var r = layer.Gameplay.View;
        var data = ClearUploadConstants();
        data[0] = (uint)D3D11TileOperation.XrayRoom;
        // Xray.hlsli reads layer 1's scroll, layer 2's map size and the blend from the layer header words.
        data[3] = r.ForegroundScroll.X;
        data[4] = unchecked((ushort)(r.ForegroundScroll.Y + D3D11Screen.LayerLineOffset));
        data[5] = r.BackgroundMapSize.X; data[6] = r.BackgroundMapSize.Y;
        data[7] = (uint)r.MainScreenLayers; data[8] = layer.RevealBlocks ? 1u : 0u;
        data[9] = (uint)layer.Blend.Layers;
        data[10] = (uint)layer.Blend.FixedColor.Red; data[11] = (uint)layer.Blend.FixedColor.Green; data[12] = (uint)layer.Blend.FixedColor.Blue;
        // Layer 1 is always the room's; layer 2 is what the room publishes for it (#1403).
        data[14] = D3D11RoomSources.Layer2Of(layer.Gameplay.Rooms);
        data[22] = layer.Blend.BlendsWithLayersBehind ? 1u : 0u;
        data[26] = 224;
        data[27] = (uint)r.BackgroundPixelation.Size;
        // What the blend draws from behind: nothing, layer 3, layer 2 or both.
        if (layer.BlendsWithBackground) data[15] = (uint)D3D11XrayBehindLayers.Layer2;
        if (layer.BlendPlane is { } sub)
        {
            data[15] = (uint)(layer.BlendsWithBackground
                ? D3D11XrayBehindLayers.Layer2AndLayer3
                : D3D11XrayBehindLayers.Layer3);
            data[18] = (uint)sub.FirstRow;
            data[20] = (uint)sub.MapHeightTiles; data[21] = (uint)sub.FirstLine;
        }
        for (int y = 32; y < 224; y++)
        {
            int offset = D3D11ShaderLayout.ScanlineParametersWordOffset + y * 4, line = y - 32;
            var scroll = layer.Gameplay.BackgroundScrollAt(line);
            data[offset] = scroll.X;
            data[offset + 1] = unchecked((ushort)(scroll.Y + D3D11Screen.LayerLineOffset));
            data[offset + 2] = D3D11ConstantPacking.Bytes(checked((byte)layer.Lines[y].Left), checked((byte)layer.Lines[y].Right), 0, 0);
            // The third byte holds the layers this line shows; the low half the line's window edges.
            var mainScreen = layer.Gameplay.LayersShownByLine.IsEmpty
                ? r.MainScreenLayers : layer.Gameplay.LayersShownByLine[line];
            // Draygon's room hides layer 2 outside its band even while the X-ray blends.
            if (y < r.BackgroundFirstLine || y >= r.BackgroundEndLine) mainScreen &= ~ScreenLayers.Background;
            data[offset + 2] |= D3D11ConstantPacking.Bytes(0, 0, (byte)mainScreen, 0);
            if (layer.BlendPlane is { } bg3)
                data[offset + 3] = D3D11ConstantPacking.Halves(bg3.Scrolls[y].X, bg3.Scrolls[y].Y);
        }
        fixed (uint* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.Dispatch(32, 28, 1);
    }

    /// <summary>Draws the message box's text image, coloured in the message's text colours (#1408).</summary>
    private unsafe void DispatchMessage(MessageBoxLayer layer)
    {
        var data = ClearUploadConstants();
        data[0] = (uint)D3D11TileOperation.MessageText;
        // The image's top line goes in the layer word; the room levels hold the image (#1392).
        data[1] = (uint)layer.Top;
        data[25] = (uint)Math.Clamp(layer.FirstLine, 0, D3D11Screen.Height);
        data[26] = (uint)Math.Clamp(layer.EndLine, 0, D3D11Screen.Height);
        fixed (uint* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.Dispatch(32, 28, 1);
    }

    /// <summary>
    /// Composes a room with a perspective floor below the HUD's lines: the floor, the sprites and layer 2 by priority. The
    /// HUD is its own layer (#1466).
    /// </summary>
    private void DispatchFloorRoom(TurningRoomLayer layer)
    {
        // Clipping is in screen lines. Never move the floor's origin to the start of the band: the transform is the
        // whole screen's.
        int floorStart = layer.Floor?.FirstLine ?? 224;
        DispatchFloor(layer.Transform, layer.HudLines, floorStart);
        DispatchTile(D3D11TileOperation.InsertSprites, new D3D11TileDispatch { FirstScanline = (uint)layer.HudLines, EndScanline = (uint)floorStart });
        if (layer.Floor is { } floor)
        {
            // Only the middle band shows the floor. Transparent floor pixels keep the backdrop; every priority pass
            // inserts from the same sprite layer.
            Sprites(0); Sprites(1);
            Background(false);
            Sprites(2);
            Background(true);
            Sprites(3);

            void Sprites(uint priority) => DispatchTile(D3D11TileOperation.InsertSprites, new D3D11TileDispatch { Priority = priority + 1, FirstScanline = (uint)floorStart });
            void Background(bool high) => DispatchTile(D3D11TileOperation.Layer, new D3D11TileDispatch { RoomSource = D3D11RoomSource.Layer2, Scroll = floor.Scroll.AsBgPlaneScroll(), MapSize = floor.MapSize, Priority = Priority(high), FirstScanline = (uint)floorStart });
        }
    }

    /// <summary>
    /// Composes a room: sprites of priority 0 and 1, layer 2 then layer 1 at low priority, sprites of priority 2, layer 2
    /// then layer 1 at high priority, sprites of priority 3; in bands split where the layers a line shows change, below
    /// the HUD's band. The HUD is its own layer (#1466).
    /// </summary>
    private void DispatchOrdinaryGameplay(GameplaySceneLayer layer)
    {
        GameplaySceneView r = layer.View;
        // Every pass inserts from the same sprite layer, so a sprite hidden by one in front never shows at another
        // priority.
        int first = D3D11Screen.HudHeight, end;
        ScreenLayers activeLayers;
        while (first < D3D11Screen.Height)
        {
            activeLayers = layer.LayersShownByLine.IsEmpty ? r.MainScreenLayers
                : layer.LayersShownByLine[first - D3D11Screen.HudHeight];
            end = first + 1;
            while (end < D3D11Screen.Height && (layer.LayersShownByLine.IsEmpty ||
                layer.LayersShownByLine[end - D3D11Screen.HudHeight] == activeLayers)) end++;
            Sprites(0); Sprites(1); Background2(false); Background1(false);
            Sprites(2); Background2(true); Background1(true); Sprites(3);
            first = end;
        }

        void Sprites(uint priority)
        {
            if ((activeLayers & ScreenLayers.Sprites) != 0)
                DispatchTile(D3D11TileOperation.InsertSprites, new D3D11TileDispatch { Priority = priority + 1, FirstScanline = (uint)first, EndScanline = (uint)end });
        }
        void Background1(bool high)
        {
            if ((activeLayers & ScreenLayers.Foreground) != 0)
                DispatchTile(D3D11TileOperation.Layer, new D3D11TileDispatch { RoomSource = D3D11RoomSource.Layer1, Scroll = r.ForegroundScroll.AsBgPlaneScroll() + new Offset16<TilePlanePixels>(0, D3D11Screen.LayerLineOffset), MapSize = new Extent16<TilemapCells>(64, 32), Priority = Priority(high), FirstScanline = (uint)first, EndScanline = (uint)end });
        }
        void Background2(bool high)
        {
            if ((activeLayers & ScreenLayers.Background) != 0) DispatchRoomLayer2(layer, high, first, end);
        }
    }

    /// <summary>Draws the room's layer 2 at one priority in a band, each line at its own scroll, with its mosaic.</summary>
    private unsafe void DispatchRoomLayer2(GameplaySceneLayer layer, bool high, int first, int end)
    {
        var r = layer.View;
        var data = ClearUploadConstants();
        data[0] = (uint)D3D11TileOperation.Layer;
        data[5] = r.BackgroundMapSize.X; data[6] = r.BackgroundMapSize.Y;
        data[7] = Priority(high); data[8] = 1; data[15] = 1;
        data[14] = D3D11RoomSources.Layer2Of(layer.Rooms);
        data[25] = (uint)Math.Max(r.BackgroundFirstLine, first); data[26] = (uint)Math.Min(r.BackgroundEndLine, end);
        // Header word 27 is the mosaic's block size (zero elsewhere).
        data[27] = (uint)r.BackgroundPixelation.Size;
        for (int y = D3D11Screen.HudHeight; y < D3D11Screen.Height; y++)
        {
            int line = y - D3D11Screen.HudHeight;
            int offset = D3D11ShaderLayout.ScanlineParametersWordOffset + y * 4;
            var scroll = layer.BackgroundScrollAt(line);
            data[offset] = scroll.X;
            data[offset + 1] = unchecked((ushort)(scroll.Y + D3D11Screen.LayerLineOffset));
        }
        fixed (uint* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.Dispatch(32, 28, 1);
    }

    /// <summary>Adds a frontend layer's colours where the layer covering it shows, each line at its own scroll.</summary>
    private unsafe void DispatchAddLayerBehind(AddedScreenTilesLayer layer)
    {
        var data = ClearUploadConstants();
        data[0] = (uint)D3D11TileOperation.AddLayerBehind;
        data[1] = roomLevels.ScreenMap(layer.Cells);
        data[4] = (uint)layer.VerticalScroll;
        data[5] = 32; data[6] = 32; data[8] = 1; data[26] = 224;
        data[14] = (uint)D3D11RoomSource.ScreenCells | roomLevels.ScreenTable(layer.Cells.Tiles) << 16;
        data[D3D11ShaderLayout.CoverageTableWord] = layer.MainCoverage is { } covering ? roomLevels.ScreenTable(covering.Cells.Tiles) : 0;
        data[10] = layer.IncludeObjects ? 1u : 0u;
        data[11] = layer.MainObjects ? 1u : 0u;
        if (layer.MainCoverage is { } coverage)
        {
            // This pass reads no floor transform, so the covering layer's words go there, leaving every line's
            // parameters for the scrolls.
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

    /// <summary>The cells a frontend layer draws (#1403): its own and, for a layer added behind, its covering layer's.</summary>
    private static IEnumerable<ScreenTileMap> ScreenMaps(DisplayLayer layer) => layer switch
    {
        ScreenTilesLayer bg => [bg.Cells],
        ScreenTilesViewportLayer bg => [bg.Cells],
        AddedScreenTilesLayer { MainCoverage: { } coverage } sub => [sub.Cells, coverage.Cells],
        AddedScreenTilesLayer sub => [sub.Cells],
        _ => [],
    };

    /// <summary>
    /// Dispatches a frontend tile layer's cells (#1403), each drawn from its tile's pre-coloured picture, colour zero
    /// transparent unless <paramref name="transparentZero"/> is false.
    /// </summary>
    private void DispatchScreenTiles(D3D11TileOperation operation, ScreenTileMap cells, Position16<TilePlanePixels> scroll, bool? priority,
        bool transparentZero = true) =>
        DispatchTile(operation, new D3D11TileDispatch
        {
            Map = roomLevels.ScreenMap(cells), Scroll = scroll, MapSize = cells.Size, Priority = Priority(priority),
            TransparentZero = transparentZero ? 1u : 0u, RoomSource = D3D11RoomSource.ScreenCells, ScreenTable = roomLevels.ScreenTable(cells.Tiles),
        });

    /// <summary>Rejects unsupported layers, uploads the scene's images, colours and layers, then dispatches passes in scene order.</summary>
    private unsafe void DrawLayers(DisplayPacket packet, LayeredScene scene,
        ReadOnlySpan<SuperMetroid.Core.Rendering.TitleGradientLine> gradient = default)
    {
        // Reject unimplemented operations before changing GPU state. No CPU fallback.
        foreach (DisplayLayer layer in scene.Layers)
            if (layer is not (ScreenTilesLayer or ScreenTilesViewportLayer or HudLayer or ScreenTintLayer or SpritesLayer or SpriteDepthLayer or TurningFloorLayer or TurningRoomLayer or ColourAddBandsLayer or OverlayBlendLayer or AddedScreenTilesLayer or MessageBoxLayer or GameplaySceneLayer or InsetSceneLayer or GameplayBlendLayer))
                throw new NotSupportedException($"GPU layer {layer.GetType().Name} is not implemented yet.");
        SuperMetroid.Core.Assets.SpriteSheetFrame? messagePicture = null;
        PicturedHud? hud = null;
        foreach (DisplayLayer layer in scene.Layers)
        {
            if (layer is MessageBoxLayer message)
                messagePicture = messagePicture is null || ReferenceEquals(messagePicture, message.Picture) ? message.Picture
                    : throw new InvalidOperationException("A frame shows two different message boxes.");
            // A frame shows one HUD, which every HUD pass draws from the room buffer (#1466).
            else if (layer is HudLayer shownHud)
                hud = hud is null || ReferenceEquals(hud, shownHud.Hud) ? shownHud.Hud
                    : throw new InvalidOperationException("A frame shows two different HUDs.");
        }
        // The gameplay base layer publishes the room backgrounds; a Mode-7 floor band shows the room's layer-2 plane.
        RoomBackgrounds? rooms = scene.Layers.IsEmpty ? null : scene.Layers[0] switch
        {
            GameplaySceneLayer ordinary => ordinary.Rooms,
            GameplayBlendLayer gameplay => gameplay.Gameplay.Rooms,
            _ => null,
        };
        RoomBackgroundLayer? floor = null;
        Layer3Cells? bg3 = rooms?.Bg3;
        AffineFloor? floorMap = null;
        // A frame shows one Mode 7 floor, which every Mode 7 dispatch draws from the room buffer (#1403).
        AffineFloor? SameFloor(AffineFloor? shown, AffineFloor floor) => shown is null || shown.Equals(floor) ? floor
            : throw new InvalidOperationException("A frame shows two different Mode 7 floors.");
        List<ScreenTileMap> screens = frameScreens;
        screens.Clear();
        foreach (DisplayLayer layer in scene.Layers)
        {
            if (layer is TurningFloorLayer frontendMode7)
                floorMap = SameFloor(floorMap, frontendMode7.Floor);
            // The frontend layers' cells (#1403), each once.
            foreach (ScreenTileMap cells in ScreenMaps(layer))
            {
                if (!screens.Contains(cells))
                    screens.Add(cells);
            }
            if (layer is TurningRoomLayer mode7)
            {
                floor = mode7.Floor?.Room ?? floor;
                floorMap = SameFloor(floorMap, mode7.Map);
            }
            else if (layer is OverlayBlendLayer math)
                bg3 = SameBg3(bg3, math.Plane);
            else if (layer is GameplayBlendLayer { BlendPlane: { } sub })
                bg3 = SameBg3(bg3, sub.Plane);
        }
        var levels = new RoomLevelLayers(rooms?.Bg1, rooms?.Bg2 ?? floor, rooms?.DoorSource, rooms?.Bg1Overlay, rooms?.Sky, bg3,
            rooms?.Bg2Overlay, floorMap, rooms?.BossBody, rooms?.XrayReveal, rooms?.Bg2Image,
            messagePicture, screens.Count == 0 ? null : new ScreenPictureTables([.. screens]), hud);
        // The image texture holds every image the scene shows before either buffer names a place in it (#1408).
        images.Needed.Clear();
        D3D11SpriteDrawBuffer.Collect(scene.Paint.SpriteDraws.AsSpan(), images.Needed);
        roomLevels.Collect(levels, images.Needed);
        images.Hold(images.Needed);
        spriteDraws.Pack(scene.Paint.SpriteDraws.AsSpan(), images);
        fixed (uint* source = spriteDraws.Upload) UploadBuffer(spriteDraws.Buffer, (nint)source, spriteDraws.Upload.Length * sizeof(uint));
        if (floorMap is not null)
            floorImage.Show(floorMap);
        if (roomLevels.Pack(levels, images))
            fixed (uint* source = roomLevels.Upload) UploadBuffer(roomLevels.Buffer, (nint)source, roomLevels.Upload.Length * sizeof(uint));
        // The colours change every frame a colour effect runs, so they are packed every frame, for the slots the
        // pictures were packed with.
        sceneColours.Pack(scene.Paint.Backgrounds, roomLevels.ColourSlots);
        fixed (uint* source = sceneColours.Upload) UploadBuffer(sceneColours.Buffer, (nint)source, sceneColours.Upload.Length * sizeof(uint));
        owner.Context.CSSetShader(tileShader);
        owner.Context.CSSetConstantBuffer(0, constants);
        owner.Context.CSSetShaderResource(0, sceneColours.View);
        owner.Context.CSSetShaderResource(1, spriteDraws.View);
        owner.Context.CSSetShaderResource(3, roomLevels.View);
        owner.Context.CSSetShaderResource(4, images.View);
        owner.Context.CSSetShaderResource(5, floorImage.View);
        owner.Context.CSSetUnorderedAccessView(0, view);
        owner.Context.CSSetUnorderedAccessView(1, objectView);
        DrawSpriteLayer();
        DispatchTile(D3D11TileOperation.Backdrop, new D3D11TileDispatch());
        foreach (DisplayLayer layer in scene.Layers)
        {
            switch (layer)
            {
                case GameplayBlendLayer gameplayXray:
                    DispatchXrayRoom(gameplayXray);
                    break;
                case InsetSceneLayer window:
                    DrawWindow(packet, window);
                    break;
                case GameplaySceneLayer ordinary:
                    DispatchOrdinaryGameplay(ordinary);
                    break;
                case MessageBoxLayer message:
                    DispatchMessage(message);
                    break;
                case AddedScreenTilesLayer sub:
                    DispatchAddLayerBehind(sub);
                    break;
                case OverlayBlendLayer math:
                    DispatchBackgroundMath(math);
                    break;
                case ColourAddBandsLayer windows:
                    DispatchColorWindows(windows);
                    break;
                case TurningRoomLayer gameplay:
                    DispatchFloorRoom(gameplay);
                    break;
                case TurningFloorLayer mode7:
                    DispatchFloor(mode7.Transform, subtractSprites: mode7.SubtractsSprites, addLayer1: mode7.AddsForeground);
                    break;
                case SpritesLayer objLayer:
                    DispatchTile(D3D11TileOperation.InsertSprites, new D3D11TileDispatch { Red = objLayer.AddToScreen ? 1u : 0u, Green = objLayer.FixedColor is not null ? 1u : 0u, Blue = objLayer.FixedColor is { } color ? (uint)(color.Red | color.Green << 5 | color.Blue << 10) : 0 });
                    break;
                case SpriteDepthLayer obj:
                    DispatchTile(D3D11TileOperation.InsertSprites, new D3D11TileDispatch { Priority = (uint)obj.Depth + 1, Green = obj.FixedColor is not null ? 1u : 0u, Blue = obj.FixedColor is { } priorityColor ? (uint)(priorityColor.Red | priorityColor.Green << 5 | priorityColor.Blue << 10) : 0 });
                    break;
                case ScreenTilesLayer bg:
                    DispatchScreenTiles(D3D11TileOperation.Layer, bg.Cells, bg.Scroll, bg.Priority);
                    break;
                case ScreenTilesViewportLayer bg:
                    DispatchScreenTiles(D3D11TileOperation.Layer, bg.Cells, new Position16<TilePlanePixels>(0, checked((ushort)bg.VerticalScroll)), bg.Priority, bg.TransparentColorZero);
                    break;
                case HudLayer hudLayer:
                    // The HUD's placed pictures at the top of the screen, each pixel in its picture's priority tier (#1466).
                    DispatchTile(D3D11TileOperation.Layer, new D3D11TileDispatch
                    {
                        RoomSource = D3D11RoomSource.Hud, TransparentZero = hudLayer.TransparentColorZero ? 1u : 0u,
                        Priority = Priority(hudLayer.Priority), EndScanline = D3D11Screen.HudHeight,
                    });
                    break;
                case ScreenTintLayer add:
                    DispatchTile(D3D11TileOperation.AddColour, new D3D11TileDispatch { Red = (uint)add.Red, Green = (uint)add.Green, Blue = (uint)add.Blue });
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(DisplayLayer)} {layer}.");
            }
        }
        if (!gradient.IsEmpty) DispatchTitleGradient(gradient);
        // The frame's one brightness dims the composed scene once.
        DispatchTile(D3D11TileOperation.Brightness, new D3D11TileDispatch { Level = (uint)packet.Brightness });
    }

    /// <summary>
    /// Draws the sprite layer (#1408): clears it to no sprite, then draws each sprite draw's quad back to front, so each
    /// pixel keeps the frontmost draw's opaque pixel. Leaves the composition's bindings as it found them.
    /// </summary>
    private void DrawSpriteLayer()
    {
        const uint noSprite = 255;
        owner.Context.CSSetUnorderedAccessView(1, null);
        owner.Context.ClearUnorderedAccessView(objectView, new Int4(0, (int)noSprite, 0, 0));
        if (spriteDraws.Count != 0)
        {
            owner.Context.OMSetRenderTargets(spriteTarget);
            owner.Context.RSSetViewport(0, 0, D3D11Screen.Width, D3D11Screen.Height);
            owner.Context.RSSetState(spriteRasterizer);
            owner.Context.IASetInputLayout(null);
            owner.Context.IASetPrimitiveTopology(PrimitiveTopology.TriangleStrip);
            owner.Context.VSSetShader(spriteVertexShader);
            owner.Context.VSSetShaderResource(1, spriteDraws.View);
            owner.Context.PSSetShader(spritePixelShader);
            owner.Context.PSSetShaderResource(1, spriteDraws.View);
            owner.Context.PSSetShaderResource(4, images.View);
            owner.Context.DrawInstanced(4, (uint)spriteDraws.Count, 0, 0);
            owner.Context.VSUnsetShaderResource(1);
            owner.Context.PSUnsetShaderResource(1);
            owner.Context.PSUnsetShaderResource(4);
            owner.Context.RSSetState(null);
            owner.Context.OMSetRenderTargets(Array.Empty<ID3D11RenderTargetView>());
        }
        owner.Context.CSSetUnorderedAccessView(1, objectView);
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
        data[0] = (uint)operation; data[1] = tile.Map; data[3] = tile.Scroll.X;
        data[4] = tile.Scroll.Y; data[5] = size.X; data[6] = size.Y; data[7] = tile.Priority;
        data[8] = tile.TransparentZero; data[9] = tile.Level; data[10] = tile.Red; data[11] = tile.Green; data[12] = tile.Blue;
        data[14] = (uint)tile.RoomSource | tile.ScreenTable << 16;
        data[25] = tile.FirstScanline; data[26] = tile.EndScanline;
        fixed (uint* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.Dispatch(32, 28, 1);
    }

    /// <summary>Encodes unspecified, low, or high tile priority for the compute shader.</summary>
    private static uint Priority(bool? priority) => priority is null ? 0u : priority.Value ? 2u : 1u;

    /// <summary>
    /// Uploads the frame's floor transform: signed matrix, center, scroll, what shows outside the floor, and optional
    /// color math. The floor's pictures' colour slots are in the room buffer (#1403, #1467).
    /// </summary>
    private unsafe void DispatchFloor(AffineTransform transform, int firstScanline = 0, int endScanline = 224,
        bool subtractSprites = false, bool addLayer1 = false)
    {
        // The transform's signed values go as they are; the shader projects every pixel itself.
        var data = MemoryMarshal.Cast<uint, int>(ClearUploadConstants().AsSpan());
        data[0] = (int)D3D11TileOperation.Floor;
        data[16] = transform.MatrixA; data[17] = transform.MatrixB;
        data[18] = transform.MatrixC; data[19] = transform.MatrixD;
        data[20] = transform.Center.X; data[21] = transform.Center.Y;
        data[22] = transform.Scroll.X; data[23] = transform.Scroll.Y;
        data[24] = transform.Outside switch
        {
            FloorOverflowMode.Transparent => 0,
            FloorOverflowMode.Wrap => 2,
            _ => throw new ArgumentOutOfRangeException(nameof(transform), transform.Outside, "Unknown floor overflow."),
        };
        data[25] = firstScanline; data[26] = endScanline;
        data[27] = (int)(addLayer1 ? FloorColorBlend.AddForeground
            : subtractSprites ? FloorColorBlend.SubtractSprites : FloorColorBlend.None);
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
            data[offset + 2] = (uint)lines[y].Blue; data[offset + 3] = (uint)lines[y].Control;
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
    private void DrawWindow(DisplayPacket packet, InsetSceneLayer window)
    {
        if (window.Left == window.Right || window.Top == window.Bottom) return;
        // A single reusable child owns its own buffers, sprite layer and output.
        // The contract prohibits nested windows, bounding scratch resource usage.
        childRenderer ??= Own(new D3D11FrameRenderer(owner));
        var childPacket = new DisplayPacket(packet.Identity, window.Scene);
        childRenderer.DrawLayers(childPacket, window.Scene);
        owner.Context.CSSetUnorderedAccessView(0, null);
        owner.Context.CSSetUnorderedAccessView(1, null);
        owner.Context.CopySubresourceRegion(output, 0, (uint)window.Left, (uint)window.Top, 0,
            childRenderer.output, 0, new Box(window.Left, window.Top, 0, window.Right, window.Bottom, 1));
        // The immediate context is shared but every scene's resources are separate.
        // Restore the parent before later overlays, sprite insertion, or outer fades.
        owner.Context.CSSetShader(tileShader);
        owner.Context.CSSetConstantBuffer(0, constants);
        owner.Context.CSSetShaderResource(0, sceneColours.View);
        owner.Context.CSSetShaderResource(1, spriteDraws.View);
        owner.Context.CSSetShaderResource(3, roomLevels.View);
        owner.Context.CSSetShaderResource(4, images.View);
        owner.Context.CSSetShaderResource(5, floorImage.View);
        owner.Context.CSSetUnorderedAccessView(0, view);
        owner.Context.CSSetUnorderedAccessView(1, objectView);
    }
}
