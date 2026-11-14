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
    /// <summary>The display set only after it finishes GPU submission.</summary>
    internal DisplayPacket? renderedDisplay;
    /// <summary>The last successfully submitted display.</summary>
    internal DisplayPacket? SubmittedDisplay => renderedDisplay;
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
    /// <summary>Gradient compute program, drawing a gradient layer's stops over its band (#1468).</summary>
    private readonly ID3D11ComputeShader screenGradientShader;
    /// <summary>The scene's backdrop and the colours each of its pictures' images shows, bound at register t0.</summary>
    private readonly D3D11SceneColourBuffer sceneColours;
    /// <summary>The scene's whole-frame sprite draws, bound at register t1.</summary>
    private readonly D3D11SpriteDrawBuffer spriteDraws;
    /// <summary>The scene's room planes at t3, repacked only when they change (#1346).</summary>
    private readonly D3D11RoomLevelBuffer roomLevels;
    /// <summary>
    /// The sprite layer (#1408): per pixel, the frontmost sprite pixel's colour and whether it blends, drawn from a run of
    /// the frame's draws' quads and read by every composition pass that inserts or blends sprites. A scene's sprite layer
    /// draws its own run of draws into it before it is inserted (#1514); every other pass reads all the frame's draws.
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

    /// <summary>The run of the packed draws the sprite layer holds now, or null before a frame has drawn any (#1514).</summary>
    private SpriteRun? shownSprites;

    /// <summary>The pictured planes the frame being drawn shows, each once (#1467).</summary>
    private readonly List<PicturedPlane> framePlanes = [];

    /// <summary>The frontend picture grids a frame draws, each once; reused across frames (#1467).</summary>
    private readonly List<PictureGrid> frameGrids = [];

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
            screenGradientShader = Own(LoadShader(D3D11ShaderLayout.ScreenGradientResourceName));
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
        renderedDisplay = null;
        RenderCore(packet);
        renderedDisplay = packet;
    }

    /// <summary>Selects layered, turning-floor or solid-color composition and submits its compute passes.</summary>
    private unsafe void RenderCore(DisplayPacket packet)
    {
        if (packet.Layers is { } layers) { DrawLayers(packet, layers); return; }
        if (packet.Floor is { } turningFloor)
        {
            // A turning-floor frame is a frontend screen's: its floor, its sprites in front of it, then its gradient.
            if (turningFloor.Paint.Gameplay is not null)
                throw new NotSupportedException("A turning-floor frame shows a frontend screen, not gameplay.");
            var operations = new List<DisplayLayer>();
            if (turningFloor.Background is { } background && turningFloor.Floor is { } shownFloor) operations.Add(new TurningFloorLayer(background, shownFloor));
            operations.Add(new SpritesLayer(turningFloor.Paint.SpriteDraws));
            if (turningFloor.Gradient is { } gradient) operations.Add(gradient);
            DrawLayers(packet, new LayeredScene(new FramePaint(turningFloor.Paint.Backdrop),
                operations.ToArray(), turningFloor.Brightness));
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

    /// <summary>
    /// The one effect overlay a frame shows (#1348, #1514, #1706): every layer that blends with the overlay shows the same
    /// artwork, placed the same way.
    /// </summary>
    /// <exception cref="InvalidOperationException">Two of the frame's layers show different overlays.</exception>
    private static OverlayBlendLayer SameOverlay(OverlayBlendLayer? shown, OverlayBlendLayer overlay) =>
        shown is null || Equals(shown.Picture, overlay.Picture) ? overlay
            : throw new InvalidOperationException("A frame's layers show two different effect overlays.");

    /// <summary>
    /// Adds or subtracts the room effect's overlay, each line at the scroll its described line scrolls give it (#1468): an
    /// atmosphere's own vertical scroll, or a liquid's lines counted from its surface strip's top (#1706). The room buffer
    /// holds its artwork (#1514).
    /// </summary>
    private unsafe void DispatchBackgroundMath(OverlayBlendLayer layer)
    {
        var data = ClearUploadConstants();
        data[0] = (uint)(layer.Operation == Core.Assets.BlendOperation.Add
            ? D3D11TileOperation.AddLayer : D3D11TileOperation.SubtractLayer);
        data[8] = 1;
        data[14] = (uint)D3D11RoomSource.FxOverlay;
        data[25] = (uint)layer.FirstLine; data[26] = 224;
        D3D11LineScrolls.Pack(data, D3D11LineScrolls.Horizontal, layer.Horizontal, 0);
        D3D11LineScrolls.Pack(data, D3D11LineScrolls.Vertical, ShownFxOverlay.VerticalScroll(layer.Picture), 0);
        fixed (uint* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.Dispatch(32, 28, 1);
    }

    /// <summary>
    /// Uploads a colour added through a window (#1468): the window's shapes, the colour's shader levels and, when
    /// it strengthens down the screen, its steps (the first line, lines per step, steps and the steps added to every line).
    /// </summary>
    private unsafe void DispatchColorWindows(ColourAddWindowLayer layer)
    {
        var data = ClearUploadConstants();
        data[0] = (uint)D3D11TileOperation.LineColourAdd;
        FadeLevelColour added = ShaderLevels(layer.Colour);
        data[10] = (uint)added.Red; data[11] = (uint)added.Green; data[12] = (uint)added.Blue;
        data[26] = 224;
        if (layer.Steps is { } steps)
        {
            data[16] = (uint)steps.FirstLine; data[17] = (uint)steps.LinesPerStep;
            data[18] = (uint)steps.Steps; data[19] = unchecked((uint)steps.Shift);
        }
        D3D11WindowPacking.Pack(data, layer.Window);
        fixed (uint* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.Dispatch(32, 28, 1);
    }

    /// <summary>Draws the submitted integer frame into an owner-device target, without readback.</summary>
    internal unsafe void DrawDisplay(ID3D11RenderTargetView target, Extent16<DisplayPixels> client)
    {
        owner.VerifyOwner(); ObjectDisposedException.ThrowIf(disposed, this);
        if (client.X == 0 || client.Y == 0)
            throw new ArgumentOutOfRangeException(nameof(client), client, "A display target must have positive dimensions.");
        if (renderedDisplay is null) throw new InvalidOperationException("No frame has been submitted for display.");
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
        // Avoid SRV/UAV hazards on the next frame and release target binding
        // before a later swapchain resize. Raster state belongs to this render owner.
        owner.Context.PSUnsetShaderResource(0);
        owner.Context.OMSetRenderTargets(Array.Empty<ID3D11RenderTargetView>());
    }

    /// <summary>
    /// Composes a room under a gameplay blend (#1514): its layers in their order, the window inside which it shows
    /// unblended, the layers each line shows, and outside the window the blend of the frontmost thing with the layers lying
    /// behind it or with the blend's colour.
    /// </summary>
    private unsafe void DispatchXrayRoom(GameplayBlendLayer layer)
    {
        var r = layer.Gameplay.View;
        var data = ClearUploadConstants();
        data[0] = (uint)D3D11TileOperation.XrayRoom;
        // Xray.hlsli reads the foreground pixel the screen's top-left shows and the blend from the layer header words.
        data[3] = r.ForegroundScroll.X;
        data[4] = r.ForegroundScroll.Y;
        data[8] = layer.RevealBlocks ? 1u : 0u;
        data[9] = BlendWord(layer.Blend);
        FadeLevelColour blendColour = ShaderLevels(layer.Blend.Colour);
        data[10] = (uint)blendColour.Red; data[11] = (uint)blendColour.Green; data[12] = (uint)blendColour.Blue;
        // The foreground is always the room's; the background is what the room publishes for it (#1403).
        data[14] = D3D11RoomSources.BackgroundSourceOf(layer.Gameplay.Rooms);
        // What lies behind what blends: nothing (the blend's colour), the overlay, the background or both.
        D3D11BlendBehind? behind = (layer.BackgroundBehind, layer.OverlayBehind is not null) switch
        {
            (false, false) => null,
            (false, true) => D3D11BlendBehind.Overlay,
            (true, false) => D3D11BlendBehind.Background,
            (true, true) => D3D11BlendBehind.BackgroundAndOverlay,
        };
        data[22] = behind is null ? 0u : 1u;
        data[15] = behind is { } lying ? (uint)lying : 0u;
        data[26] = 224;
        data[27] = (uint)r.BackgroundPixelation.Size;
        if (layer.OverlayBehind is { } overlay)
        {
            // The overlay's first line; the room buffer places its picture (#1514).
            data[21] = (uint)overlay.FirstLine;
        }
        for (int y = 32; y < 224; y++)
        {
            int offset = D3D11ShaderLayout.ScanlineParametersWordOffset + y * 4;
            // The third byte holds the layers this line shows.
            ScreenLayers shown = layer.Gameplay.ShownLayersAt(y);
            // Draygon's room hides its background outside his band even while the X-ray blends.
            if (y < r.BackgroundFirstLine || y >= r.BackgroundEndLine) shown &= ~ScreenLayers.Background;
            data[offset + 2] = D3D11ConstantPacking.Bytes(0, 0, (byte)ShownWord(shown), 0);
        }
        // The window (#1468): inside it the room shows unblended.
        D3D11WindowPacking.Pack(data, layer.Window);
        // The background's scrolls and the overlay's, described line by line (#1468).
        PackBackgroundScrolls(data, layer.Gameplay);
        if (layer.OverlayBehind is { } lyingOverlay)
        {
            D3D11LineScrolls.Pack(data, D3D11LineScrolls.BlendPlaneHorizontal, lyingOverlay.Horizontal, 0);
            D3D11LineScrolls.Pack(data, D3D11LineScrolls.BlendPlaneVertical, ShownFxOverlay.VerticalScroll(lyingOverlay.Picture), 0);
        }
        fixed (uint* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.Dispatch(32, 28, 1);
    }

    /// <summary>Xray.hlsli's bit for a line showing the room's foreground.</summary>
    private const uint ShowsForeground = 1;
    /// <summary>Xray.hlsli's bit for a line showing the room's background.</summary>
    private const uint ShowsBackground = 2;
    /// <summary>Xray.hlsli's bit for a line showing the actors.</summary>
    private const uint ShowsActors = 4;

    /// <summary>The bits Xray.hlsli tests for the layers a line shows.</summary>
    private static uint ShownWord(ScreenLayers shown) =>
        ((shown & ScreenLayers.Foreground) != 0 ? ShowsForeground : 0)
        | ((shown & ScreenLayers.Background) != 0 ? ShowsBackground : 0)
        | ((shown & ScreenLayers.Sprites) != 0 ? ShowsActors : 0);

    /// <summary>Xray.hlsli's bit for a blend changing the frontmost foreground; the shader marks the foreground's pixels with it.</summary>
    private const uint BlendsForeground = 1;
    /// <summary>Xray.hlsli's bit for a blend changing the frontmost background.</summary>
    private const uint BlendsBackground = 2;
    /// <summary>Xray.hlsli's bit for a blend changing the frontmost actors that take part in blends.</summary>
    private const uint BlendsActors = 16;
    /// <summary>Xray.hlsli's bit for a blend changing the backdrop.</summary>
    private const uint BlendsBackdrop = 32;
    /// <summary>Xray.hlsli's bit for a blend halving what it combines.</summary>
    private const uint BlendHalves = 64;
    /// <summary>Xray.hlsli's bit for a blend subtracting what it combines.</summary>
    private const uint BlendSubtracts = 128;

    /// <summary>The bits Xray.hlsli tests for what a blend changes and how.</summary>
    /// <exception cref="InvalidOperationException">The blend's operation is not defined.</exception>
    private static uint BlendWord(GameplayBlend blend) =>
        ((blend.Blended & BlendedLayers.Foreground) != 0 ? BlendsForeground : 0)
        | ((blend.Blended & BlendedLayers.Background) != 0 ? BlendsBackground : 0)
        | ((blend.Blended & BlendedLayers.Actors) != 0 ? BlendsActors : 0)
        | ((blend.Blended & BlendedLayers.Backdrop) != 0 ? BlendsBackdrop : 0)
        | (blend.Halved ? BlendHalves : 0)
        | blend.Operation switch
        {
            Core.Assets.BlendOperation.Add => 0u,
            Core.Assets.BlendOperation.Subtract => BlendSubtracts,
            _ => throw new InvalidOperationException($"Undefined blend operation {blend.Operation}."),
        };

    /// <summary>
    /// The depth a gameplay draw sits at among the room's layers (#1514): its frame's. An empty frame draws nothing, so it is
    /// packed with the actors behind the room.
    /// </summary>
    /// <exception cref="InvalidDataException">The draw shows a frame that is not empty and has no depth.</exception>
    internal static ActorDepth DepthOf(ImageSpriteDraw draw) => draw.Depth ?? (draw.Image.IsEmpty ? ActorDepth.BehindRoom
        : throw new InvalidDataException($"Gameplay sprite draw {draw} shows a frame with no depth among the room's layers."));

    /// <summary>The sprite layer's word for a depth: Sprites.hlsl writes it and SpriteLayer.hlsli's Depth constants name it.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="depth"/> is not a defined depth.</exception>
    internal static uint DepthWord(ActorDepth depth) => depth switch
    {
        ActorDepth.BehindRoom => 0,
        ActorDepth.BehindFrontParts => 1,
        ActorDepth.InFront => 2,
        _ => throw new InvalidOperationException($"Undefined actor depth {depth}."),
    };

    /// <summary>The <see cref="D3D11TileDispatch.DrawnDepth"/> of a pass inserting the sprites at <paramref name="depth"/>.</summary>
    private static uint Inserting(ActorDepth depth) => DepthWord(depth) + 1;

    /// <summary>
    /// The <see cref="D3D11TileDispatch.DrawnDepth"/> of a room layer pass drawing its back or its front tiles: the part of its
    /// pictures drawn in front, or the part drawn behind (#1634), as Tiles.hlsl's DrawsInFrontPart and DrawsBehindPart name them.
    /// </summary>
    private static uint PicturePart(bool inFront) => inFront ? 2u : 1u;

    /// <summary>Draws the message box's picture where it lies on the screen, coloured in the message's text colours (#1408, #1555).</summary>
    private unsafe void DispatchMessage(MessageBoxLayer layer)
    {
        var data = ClearUploadConstants();
        data[0] = (uint)D3D11TileOperation.MessageText;
        // The picture's top-left corner goes in the scroll's words, its left column then its top line; the room levels
        // hold the picture (#1392).
        data[3] = layer.Corner.X;
        data[4] = layer.Corner.Y;
        data[25] = (uint)Math.Clamp(layer.FirstLine, 0, D3D11Screen.Height);
        data[26] = (uint)Math.Clamp(layer.EndLine, 0, D3D11Screen.Height);
        fixed (uint* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.Dispatch(32, 28, 1);
    }

    /// <summary>
    /// Composes a room with a perspective floor below the HUD's lines (#1514): the room's scenery behind everything (#1545),
    /// the floor with the actors over it, and on the band below it the background's layers and the actors in
    /// <see cref="GameplaySceneLayer.Order"/>. The HUD is its own layer (#1466).
    /// </summary>
    private void DispatchFloorRoom(TurningRoomLayer layer)
    {
        // Clipping is in screen lines. Never move the floor's origin to the start of the band: the transform is the
        // whole screen's.
        int floorStart = layer.Floor?.FirstLine ?? 224;
        // The pictures fixed in the room lie behind the floor, the actors and the band's background, every pixel of each;
        // the floor's and the background's clear pixels show them.
        if (layer.Scenery is { } scenery)
            DispatchTile(D3D11TileOperation.Layer, new D3D11TileDispatch
            {
                Table = roomLevels.PlaneTable(scenery.Plane), Scroll = PlaneScroll(scenery.Scroll), RoomSource = D3D11RoomSource.PicturedPlane,
                FirstScanline = (uint)layer.HudLines,
            });
        DispatchFloor(layer.Transform, layer.HudLines, floorStart);
        DispatchTile(D3D11TileOperation.InsertSprites, new D3D11TileDispatch { FirstScanline = (uint)layer.HudLines, EndScanline = (uint)floorStart });
        if (layer.Floor is not { } floor)
            return;
        // Only the band below the floor shows the background; transparent pixels keep the backdrop. The band shows no
        // foreground.
        foreach (GameplayLayer shown in GameplaySceneLayer.Order)
        {
            switch (shown)
            {
                case GameplayLayer.ActorsBehindRoom or GameplayLayer.ActorsBehindFrontParts or GameplayLayer.ActorsInFront:
                    DispatchTile(D3D11TileOperation.InsertSprites, new D3D11TileDispatch { DrawnDepth = Inserting(ActorsAt(shown)), FirstScanline = (uint)floorStart });
                    break;
                case GameplayLayer.BackgroundBackPart or GameplayLayer.BackgroundFrontPart:
                    DispatchTile(D3D11TileOperation.Layer, new D3D11TileDispatch
                    {
                        RoomSource = D3D11RoomSource.Background, Scroll = floor.Scroll.AsPlaneScroll(),
                        DrawnDepth = PicturePart(shown == GameplayLayer.BackgroundFrontPart), FirstScanline = (uint)floorStart,
                    });
                    break;
                case GameplayLayer.ForegroundBackPart or GameplayLayer.ForegroundFrontPart:
                    break;
                default:
                    throw new InvalidOperationException($"Undefined gameplay layer {shown}.");
            }
        }
    }

    /// <summary>The depth of the actors a gameplay actor layer shows.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="layer"/> shows no actors.</exception>
    private static ActorDepth ActorsAt(GameplayLayer layer) => layer switch
    {
        GameplayLayer.ActorsBehindRoom => ActorDepth.BehindRoom,
        GameplayLayer.ActorsBehindFrontParts => ActorDepth.BehindFrontParts,
        GameplayLayer.ActorsInFront => ActorDepth.InFront,
        GameplayLayer.BackgroundBackPart or GameplayLayer.ForegroundBackPart or GameplayLayer.BackgroundFrontPart
            or GameplayLayer.ForegroundFrontPart => throw new InvalidOperationException($"Gameplay layer {layer} shows no actors."),
        _ => throw new InvalidOperationException($"Undefined gameplay layer {layer}."),
    };

    /// <summary>
    /// Composes a room (#1514): its layers in <see cref="GameplaySceneLayer.Order"/>, in bands split where the layers a line
    /// shows change, below the HUD's band. The HUD is its own layer (#1466).
    /// </summary>
    private void DispatchOrdinaryGameplay(GameplaySceneLayer layer)
    {
        GameplaySceneView r = layer.View;
        int first = D3D11Screen.HudHeight;
        while (first < D3D11Screen.Height)
        {
            ScreenLayers shownLayers = layer.ShownLayersAt(first);
            int end = first + 1;
            while (end < D3D11Screen.Height && layer.ShownLayersAt(end) == shownLayers) end++;
            foreach (GameplayLayer shown in GameplaySceneLayer.Order)
            {
                switch (shown)
                {
                    case GameplayLayer.ActorsBehindRoom or GameplayLayer.ActorsBehindFrontParts or GameplayLayer.ActorsInFront:
                        if ((shownLayers & ScreenLayers.Sprites) != 0)
                            DispatchTile(D3D11TileOperation.InsertSprites, new D3D11TileDispatch { DrawnDepth = Inserting(ActorsAt(shown)), FirstScanline = (uint)first, EndScanline = (uint)end });
                        break;
                    case GameplayLayer.BackgroundBackPart or GameplayLayer.BackgroundFrontPart:
                        if ((shownLayers & ScreenLayers.Background) != 0)
                            DispatchRoomBackground(layer, shown == GameplayLayer.BackgroundFrontPart, first, end);
                        break;
                    case GameplayLayer.ForegroundBackPart or GameplayLayer.ForegroundFrontPart:
                        if ((shownLayers & ScreenLayers.Foreground) != 0)
                            DispatchTile(D3D11TileOperation.Layer, new D3D11TileDispatch
                            {
                                RoomSource = D3D11RoomSource.Foreground,
                                Scroll = r.ForegroundScroll.AsPlaneScroll(),
                                DrawnDepth = PicturePart(shown == GameplayLayer.ForegroundFrontPart), FirstScanline = (uint)first, EndScanline = (uint)end,
                            });
                        break;
                    default:
                        throw new InvalidOperationException($"Undefined gameplay layer {shown}.");
                }
            }
            first = end;
        }
    }

    /// <summary>
    /// Draws the room's background's back or front tiles in a band, each line at the scroll its line scrolls give it (#1468),
    /// with its pixelation.
    /// </summary>
    private unsafe void DispatchRoomBackground(GameplaySceneLayer layer, bool frontTiles, int first, int end)
    {
        var r = layer.View;
        var data = ClearUploadConstants();
        data[0] = (uint)D3D11TileOperation.Layer;
        data[7] = PicturePart(frontTiles); data[8] = 1; data[15] = 1;
        data[14] = D3D11RoomSources.BackgroundSourceOf(layer.Rooms);
        data[25] = (uint)Math.Max(r.BackgroundFirstLine, first); data[26] = (uint)Math.Min(r.BackgroundEndLine, end);
        // Header word 27 is the mosaic's block size (zero elsewhere).
        data[27] = (uint)r.BackgroundPixelation.Size;
        PackBackgroundScrolls(data, layer);
        fixed (uint* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.Dispatch(32, 28, 1);
    }

    /// <summary>
    /// Packs the room's background scrolls into the pass's own line-scroll slots (#1468): each axis's described shape, or
    /// the background pixel the view shows at the screen's top on every line (#1514).
    /// </summary>
    private static void PackBackgroundScrolls(uint[] data, GameplaySceneLayer layer)
    {
        Position16<BackgroundPixels> own = layer.View.BackgroundScroll;
        D3D11LineScrolls.Pack(data, D3D11LineScrolls.Horizontal, layer.BackgroundLineScrolls.Horizontal, own.X);
        D3D11LineScrolls.Pack(data, D3D11LineScrolls.Vertical, layer.BackgroundLineScrolls.Vertical, own.Y);
    }

    /// <summary>
    /// Adds a pictured plane's colours as a glow (#1467, #1514): everywhere, or only where the plane and the sprites it
    /// glows over show, each line pushed sideways by the plane's wave, which the shader evaluates (#1468). The sprite layer
    /// holds the sprites it glows over, if it names any.
    /// </summary>
    private unsafe void DispatchAddPlaneBehind(AddedPicturedPlaneLayer layer)
    {
        var data = ClearUploadConstants();
        data[0] = (uint)D3D11TileOperation.AddLayerBehind;
        data[1] = roomLevels.PlaneTable(layer.Plane);
        data[8] = 1; data[15] = 1; data[26] = 224;
        data[14] = (uint)D3D11RoomSource.PicturedPlane;
        data[11] = layer.GlowsOverSprites is null ? 0u : 1u;
        if (layer.GlowsOver is { } covered)
        {
            // This pass reads no floor transform, so the covered plane's table and scroll go there.
            int offset = 16;
            data[offset] = roomLevels.PlaneTable(covered.Plane);
            data[offset + 2] = unchecked((uint)covered.Scroll.X); data[offset + 3] = unchecked((uint)covered.Scroll.Y);
            data[offset + 7] = 1;
        }
        D3D11LineScrolls.Pack(data, D3D11LineScrolls.Horizontal, layer.HorizontalWave, 0);
        D3D11LineScrolls.Pack(data, D3D11LineScrolls.Vertical, null, layer.VerticalScroll);
        fixed (uint* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.Dispatch(32, 28, 1);
    }

    /// <summary>
    /// A pictured plane's or picture grid's signed scroll offset (#1555) in the dispatch's sixteen-bit scroll halves, which
    /// the shader reads back as signed: the plane does not repeat, so the halves carry the offset, not a wrapped position.
    /// </summary>
    private static Position16<PlanePixels> PlaneScroll(Offset16<PlanePixels> scroll) =>
        new(unchecked((ushort)scroll.X), unchecked((ushort)scroll.Y));

    /// <summary>The pictured planes a layer draws (#1467): its own, a turning room's scenery (#1545) and, for a glow, the plane it glows over.</summary>
    private static IEnumerable<PicturedPlane> PicturedPlanes(DisplayLayer layer) => layer switch
    {
        PicturedPlaneLayer bg => [bg.Plane],
        TurningRoomLayer { Scenery: { } scenery } => [scenery.Plane],
        AddedPicturedPlaneLayer { GlowsOver: { } covered } glow => [glow.Plane, covered.Plane],
        AddedPicturedPlaneLayer glow => [glow.Plane],
        _ => [],
    };

    /// <summary>
    /// The run of the frame's packed draws each sprite layer of <paramref name="scene"/> shows (#1514), indexed as the
    /// scene's layers are: the draws follow the paint's in <see cref="LayeredScene.SpriteDraws"/>, from the frontmost
    /// sprite layer back. Every other layer has an empty run.
    /// </summary>
    private static SpriteRun[] SpriteRuns(LayeredScene scene)
    {
        ReadOnlySpan<DisplayLayer> layers = scene.Layers;
        var runs = new SpriteRun[layers.Length];
        int first = scene.Paint.SpriteDraws.Length;
        for (int index = layers.Length - 1; index >= 0; index--)
        {
            if (layers[index] is SpritesLayer sprites)
            {
                runs[index] = new SpriteRun(first, sprites.Draws.Length);
                first += sprites.Draws.Length;
            }
        }
        if (first != scene.SpriteDraws.Length)
            throw new InvalidOperationException("A scene's sprite layers do not account for its sprite draws.");
        return runs;
    }

    /// <summary>The run of <paramref name="runs"/> belonging to <paramref name="sprites"/>, one of <paramref name="scene"/>'s layers.</summary>
    /// <exception cref="InvalidOperationException">The sprite layer is not one of the scene's.</exception>
    private static SpriteRun RunOf(LayeredScene scene, SpriteRun[] runs, SpritesLayer sprites)
    {
        ReadOnlySpan<DisplayLayer> layers = scene.Layers;
        for (int index = 0; index < layers.Length; index++)
        {
            if (ReferenceEquals(layers[index], sprites))
                return runs[index];
        }
        throw new InvalidOperationException("A glow names sprites that are not one of its scene's sprite layers.");
    }

    /// <summary>Rejects unsupported layers, uploads the scene's images, colours and layers, then dispatches passes in scene order.</summary>
    private unsafe void DrawLayers(DisplayPacket packet, LayeredScene scene)
    {
        // Reject unimplemented operations before changing GPU state. No CPU fallback.
        foreach (DisplayLayer layer in scene.Layers)
            if (layer is not (PicturedPlaneLayer or AddedPicturedPlaneLayer or PictureGridLayer or HudLayer or ScreenTintLayer or SpritesLayer or TurningFloorLayer or TurningRoomLayer or ColourAddWindowLayer or OverlayBlendLayer or MessageBoxLayer or GameplaySceneLayer or InsetSceneLayer or GameplayBlendLayer or ScreenGradientLayer))
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
        // The gameplay base layer publishes the room backgrounds; a turning floor's band shows the room's background plane.
        RoomBackgrounds? rooms = scene.Layers.IsEmpty ? null : scene.Layers[0] switch
        {
            GameplaySceneLayer ordinary => ordinary.Rooms,
            GameplayBlendLayer gameplay => gameplay.Gameplay.Rooms,
            _ => null,
        };
        RoomBackgroundLayer? floor = null;
        // The effect overlay the frame's blends show (#1514), if any.
        OverlayBlendLayer? overlay = null;
        AffineFloor? floorMap = null;
        // A gameplay room's turning floor shows the room's shading; a frontend screen's the shading its floor names (#1512).
        D3D11PictureColouring floorColouring = D3D11PictureColouring.OwnShading;
        // A frame shows one turning floor, which every floor dispatch draws from the room buffer (#1403).
        AffineFloor? SameFloor(AffineFloor? shown, AffineFloor floor) => shown is null || shown.Equals(floor) ? floor
            : throw new InvalidOperationException("A frame shows two different turning floors.");
        List<PicturedPlane> planes = framePlanes;
        planes.Clear();
        List<PictureGrid> grids = frameGrids;
        grids.Clear();
        foreach (DisplayLayer layer in scene.Layers)
        {
            if (layer is TurningFloorLayer frontendFloor)
            {
                floorMap = SameFloor(floorMap, frontendFloor.Floor);
                floorColouring = D3D11PictureColouring.OwnShading;
            }
            if (layer is PictureGridLayer { Grid: { } grid } && !grids.Contains(grid))
                grids.Add(grid);
            // The pictured planes (#1467), each once.
            foreach (PicturedPlane plane in PicturedPlanes(layer))
            {
                if (!planes.Contains(plane))
                    planes.Add(plane);
            }
            if (layer is TurningRoomLayer turningRoom)
            {
                floor = turningRoom.Floor?.Room ?? floor;
                floorMap = SameFloor(floorMap, turningRoom.Map);
                floorColouring = D3D11PictureColouring.Room;
            }
            else if (layer is OverlayBlendLayer math)
                overlay = SameOverlay(overlay, math);
            else if (layer is GameplayBlendLayer { OverlayBehind: { } behind })
                overlay = SameOverlay(overlay, behind);
        }
        var levels = new RoomLevelLayers(rooms?.Foreground, rooms?.Background ?? floor, rooms?.DoorSource, rooms?.ForegroundText, rooms?.Sky, ShownFxOverlay.Of(overlay?.Picture),
            rooms?.BackgroundText, floorMap, rooms?.BossBody, rooms?.XrayReveal, rooms?.BackgroundImage,
            messagePicture, hud,
            planes.Count == 0 ? null : new PicturedPlaneTables([.. planes]), grids.Count == 0 ? null : new PictureGridTables([.. grids]),
            scene.Paint.Gameplay is null ? D3D11PictureColouring.OwnShading : D3D11PictureColouring.Hud, floorColouring);
        // The image texture holds every image the scene shows before either buffer names a place in it (#1408).
        images.Needed.Clear();
        D3D11SpriteDrawBuffer.Collect(scene.SpriteDraws.AsSpan(), images.Needed);
        roomLevels.Collect(levels, images.Needed);
        images.Hold(images.Needed);
        // Every draw the frame shows, the paint's then each sprite layer's, packed once; each sprite layer draws its own
        // run of them (#1514). The paint's draws are packed by depth, the actors in front first and each depth's in emission
        // order, so the sprite layer keeps the frontmost actor among the layers at each pixel.
        bool composesGameplay = !scene.Layers.IsEmpty && scene.Layers[0] is GameplaySceneLayer or GameplayBlendLayer or TurningRoomLayer;
        PackOrder(scene, composesGameplay);
        spriteDraws.Pack(CollectionsMarshal.AsSpan(packedDraws), CollectionsMarshal.AsSpan(packedDepths), images);
        SpriteRun[] runs = SpriteRuns(scene);
        var allSprites = new SpriteRun(0, scene.SpriteDraws.Length);
        fixed (uint* source = spriteDraws.Upload) UploadBuffer(spriteDraws.Buffer, (nint)source, spriteDraws.Upload.Length * sizeof(uint));
        if (floorMap is not null)
            floorImage.Show(floorMap);
        if (roomLevels.Pack(levels, images))
            fixed (uint* source = roomLevels.Upload) UploadBuffer(roomLevels.Buffer, (nint)source, roomLevels.Upload.Length * sizeof(uint));
        // The colours change every frame a colour effect runs, so they are packed every frame, for the slots the
        // pictures were packed with.
        sceneColours.Pack(scene.Paint, roomLevels.ColourSlots);
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
        shownSprites = null;
        ShowSprites(allSprites);
        DispatchTile(D3D11TileOperation.Backdrop, new D3D11TileDispatch());
        for (int index = 0; index < scene.Layers.Length; index++)
        {
            DisplayLayer layer = scene.Layers[index];
            // A sprite layer, and a glow over one, read that layer's own sprites; every other pass that reads sprites
            // reads all the frame's draws.
            ShowSprites(layer switch
            {
                SpritesLayer => runs[index],
                AddedPicturedPlaneLayer { GlowsOverSprites: { } covered } => RunOf(scene, runs, covered),
                _ => allSprites,
            });
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
                case AddedPicturedPlaneLayer added:
                    DispatchAddPlaneBehind(added);
                    break;
                case OverlayBlendLayer math:
                    DispatchBackgroundMath(math);
                    break;
                case ColourAddWindowLayer windows:
                    DispatchColorWindows(windows);
                    break;
                case TurningRoomLayer gameplay:
                    DispatchFloorRoom(gameplay);
                    break;
                case TurningFloorLayer turning:
                    DispatchFloor(turning.Transform, blend: turning.AddsForeground ? FloorColorBlend.AddForeground
                        : turning.SubtractsSprites ? FloorColorBlend.SubtractSprites : FloorColorBlend.None);
                    break;
                case SpritesLayer sprites:
                    // The layer's sprites over what lies behind them, or glowing over it; a tint brightens those that blend (#1514).
                    DispatchTile(D3D11TileOperation.InsertSprites, new D3D11TileDispatch
                    {
                        Red = sprites.Blend switch
                        {
                            SpriteBlend.Drawn => 0u,
                            SpriteBlend.Glows => 1u,
                            _ => throw new InvalidOperationException($"Undefined sprite blend {sprites.Blend}."),
                        },
                        Green = sprites.Tint is not null ? 1u : 0u,
                        Blue = sprites.Tint is { } tint ? PackedShaderLevels(tint) : 0,
                    });
                    break;
                case PictureGridLayer grid:
                    // The grid's cell pictures, scrolled by its offset and not repeating (#1467, #1555), within its window if it names one (#1514).
                    DispatchTile(D3D11TileOperation.Layer, new D3D11TileDispatch
                    {
                        Table = roomLevels.GridTable(grid.Grid), Scroll = PlaneScroll(grid.Scroll), RoomSource = D3D11RoomSource.PictureGrid,
                        Window = grid.Window,
                    });
                    break;
                case PicturedPlaneLayer pictured:
                    // The plane's placed pictures, every pixel of each (#1467, #1514), scrolled by its offset (#1555) and
                    // pixelated in its blocks (#1774).
                    DispatchTile(D3D11TileOperation.Layer, new D3D11TileDispatch
                    {
                        Table = roomLevels.PlaneTable(pictured.Plane), Scroll = PlaneScroll(pictured.Scroll),
                        RoomSource = D3D11RoomSource.PicturedPlane, TransparentZero = pictured.DrawsOpaque ? 0u : 1u,
                        Pixelation = pictured.Pixelation,
                    });
                    break;
                case HudLayer hudLayer:
                    // The HUD's placed pictures at the top of the screen, every pixel of each (#1466, #1514).
                    DispatchTile(D3D11TileOperation.Layer, new D3D11TileDispatch
                    {
                        RoomSource = D3D11RoomSource.Hud, TransparentZero = hudLayer.DrawsOpaque ? 0u : 1u,
                        EndScanline = D3D11Screen.HudHeight,
                    });
                    break;
                case ScreenTintLayer add:
                    FadeLevelColour addedLevels = ShaderLevels(add.Colour);
                    DispatchTile(D3D11TileOperation.AddColour, new D3D11TileDispatch { Red = (uint)addedLevels.Red, Green = (uint)addedLevels.Green, Blue = (uint)addedLevels.Blue });
                    break;
                case ScreenGradientLayer gradient:
                    DispatchGradient(gradient.Gradient);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(DisplayLayer)} {layer}.");
            }
        }
        // The frame's one brightness dims the composed scene once.
        DispatchTile(D3D11TileOperation.Brightness, new D3D11TileDispatch { Level = (uint)packet.Brightness });
    }

    /// <summary>Draws <paramref name="run"/> into the sprite layer unless it holds that run already (#1514).</summary>
    private void ShowSprites(SpriteRun run)
    {
        if (shownSprites == run)
            return;
        DrawSpriteLayer(run);
        shownSprites = run;
    }

    /// <summary>
    /// Draws the sprite layer (#1408, #1514): clears it to no sprite, then draws each quad of <paramref name="run"/>'s draws
    /// back to front, so each pixel keeps the frontmost draw's opaque pixel. The run's first draw and count go to the
    /// vertex shader in the constant buffer. Leaves the composition's bindings as it found them.
    /// </summary>
    private unsafe void DrawSpriteLayer(SpriteRun run)
    {
        const uint noSprite = 255;
        owner.Context.CSSetUnorderedAccessView(1, null);
        owner.Context.ClearUnorderedAccessView(objectView, new Int4(0, (int)noSprite, 0, 0));
        if (run.Count != 0)
        {
            var data = ClearUploadConstants();
            data[0] = (uint)run.First; data[1] = (uint)run.Count;
            fixed (uint* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
            owner.Context.VSSetConstantBuffer(0, constants);
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
            owner.Context.DrawInstanced(4, (uint)run.Count, 0, 0);
            owner.Context.VSSetConstantBuffer(0, null);
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
        // The tile shader's header words three and four are the layer's scroll; what a layer shows says where it repeats (#1514).
        var data = ClearUploadConstants();
        data[0] = (uint)operation; data[1] = tile.Table; data[3] = tile.Scroll.X;
        data[4] = tile.Scroll.Y; data[7] = tile.DrawnDepth;
        data[8] = tile.TransparentZero; data[9] = tile.Level; data[10] = tile.Red; data[11] = tile.Green; data[12] = tile.Blue;
        data[14] = (uint)tile.RoomSource;
        data[25] = tile.FirstScanline; data[26] = tile.EndScanline;
        // Header word 27 is a layer pass's mosaic block size (#1774); a block one pixel wide groups nothing.
        data[27] = (uint)tile.Pixelation.Size;
        // A layer drawn within a window (#1514) marks it in header word two and packs its shapes.
        if (tile.Window is { } window)
        {
            data[2] = 1;
            D3D11WindowPacking.Pack(data, window);
        }
        fixed (uint* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.Dispatch(32, 28, 1);
    }

    /// <summary>The actor depths from the front back: the order the paint's draws are packed in (#1514).</summary>
    private static readonly ActorDepth[] FrontToBack = [ActorDepth.InFront, ActorDepth.BehindFrontParts, ActorDepth.BehindRoom];

    /// <summary>The frame's draws in the order they are packed, reused from frame to frame.</summary>
    private readonly List<SpriteDraw> packedDraws = [];

    /// <summary>The depth each of <see cref="packedDraws"/> is packed at, reused from frame to frame.</summary>
    private readonly List<ActorDepth> packedDepths = [];

    /// <summary>
    /// Lays out <paramref name="scene"/>'s draws and their depths in the order they are packed (#1514): the paint's draws
    /// grouped by depth from the front back, each depth's in emission order, then every sprite layer's as the scene lists
    /// them, in front, since a sprite layer is drawn whole at its place among the scene's layers. The draws a whole-gameplay
    /// layer composes sit at their frames' depths; a frontend screen's paint draws, which only a turning floor's glow
    /// composes, sit at their frames' depths where their frames have one and in front of the floor otherwise.
    /// </summary>
    /// <exception cref="InvalidDataException">A gameplay draw shows a frame that is not empty and has no depth.</exception>
    /// <exception cref="InvalidOperationException">A paint draw is not an image draw.</exception>
    private void PackOrder(LayeredScene scene, bool composesGameplay)
    {
        packedDraws.Clear();
        packedDepths.Clear();
        ReadOnlySpan<SpriteDraw> draws = scene.SpriteDraws.AsSpan();
        int paintDraws = scene.Paint.SpriteDraws.Length;
        foreach (ActorDepth depth in FrontToBack)
        {
            foreach (SpriteDraw draw in draws[..paintDraws])
            {
                if (draw is not ImageSpriteDraw image)
                    throw new InvalidOperationException($"A frame's paint holds a {draw.GetType().Name}, not an image draw.");
                ActorDepth shown = composesGameplay ? DepthOf(image) : image.Depth ?? ActorDepth.InFront;
                if (shown != depth)
                    continue;
                packedDraws.Add(draw);
                packedDepths.Add(shown);
            }
        }
        foreach (SpriteDraw draw in draws[paintDraws..])
        {
            packedDraws.Add(draw);
            packedDepths.Add(ActorDepth.InFront);
        }
    }

    /// <summary>
    /// Uploads the frame's floor (#1514): the matrix that turns each screen pixel's distance from the floor's centre into a
    /// distance across the floor, built from the floor's angle and scale, then the centre, the scroll, what shows outside
    /// the floor, and how the floor blends. The floor's pictures' colour slots are in the room buffer (#1403, #1467).
    /// </summary>
    private unsafe void DispatchFloor(FloorView view, int firstScanline = 0, int endScanline = 224,
        FloorColorBlend blend = FloorColorBlend.None)
    {
        var data = MemoryMarshal.Cast<uint, int>(ClearUploadConstants().AsSpan());
        data[0] = (int)D3D11TileOperation.Floor;
        // The matrix's four coefficients go as floats in the header's matrix words; the shader projects every pixel.
        double cosine = Math.Cos(view.Angle) * view.Scale, sine = Math.Sin(view.Angle) * view.Scale;
        data[16] = BitConverter.SingleToInt32Bits((float)cosine); data[17] = BitConverter.SingleToInt32Bits((float)sine);
        data[18] = BitConverter.SingleToInt32Bits((float)-sine); data[19] = BitConverter.SingleToInt32Bits((float)cosine);
        data[20] = view.Center.X; data[21] = view.Center.Y;
        data[22] = view.Scroll.X; data[23] = view.Scroll.Y;
        data[24] = view.Outside switch
        {
            FloorOverflowMode.Transparent => 0,
            FloorOverflowMode.Wrap => 2,
            _ => throw new ArgumentOutOfRangeException(nameof(view), view.Outside, "Unknown floor overflow."),
        };
        data[25] = firstScanline; data[26] = endScanline;
        data[27] = (int)FloorBlendWord(blend);
        fixed (int* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.Dispatch(32, 28, 1);
    }

    /// <summary>Tiles.hlsl's word for a floor drawn without blending.</summary>
    private const uint FloorBlendsNot = 0;
    /// <summary>Tiles.hlsl's word for a floor that subtracts the frontmost sprite.</summary>
    private const uint FloorSubtractsSprites = 1;
    /// <summary>Tiles.hlsl's word for a floor that adds its own colour over itself and the blending sprites.</summary>
    private const uint FloorAddsOwnColour = 2;

    /// <summary>The word Tiles.hlsl tests for how a floor blends (#1514).</summary>
    /// <exception cref="InvalidOperationException"><paramref name="blend"/> is not a defined floor blend.</exception>
    private static uint FloorBlendWord(FloorColorBlend blend) => blend switch
    {
        FloorColorBlend.None => FloorBlendsNot,
        FloorColorBlend.SubtractSprites => FloorSubtractsSprites,
        FloorColorBlend.AddForeground => FloorAddsOwnColour,
        _ => throw new InvalidOperationException($"Undefined floor blend {blend}."),
    };

    /// <summary>ScreenGradient.hlsl's bit for a stop changing the floor and the backdrop where no sprite shows.</summary>
    private const uint GradientChangesFloorAndBackdrop = 1;
    /// <summary>ScreenGradient.hlsl's bit for a stop changing the sprites that take part in blends.</summary>
    private const uint GradientChangesSprites = 16;
    /// <summary>ScreenGradient.hlsl's bit for a stop darkening what it changes.</summary>
    private const uint GradientDarkens = 128;

    /// <summary>The bits ScreenGradient.hlsl tests for a stop's blend (#1514).</summary>
    /// <exception cref="InvalidOperationException"><paramref name="blend"/> is not a defined gradient blend.</exception>
    private static uint GradientBlendWord(GradientBlend blend) => blend switch
    {
        GradientBlend.DarkensFloorAndBackdrop => GradientChangesFloorAndBackdrop | GradientDarkens,
        GradientBlend.BrightensFloorBackdropAndSprites => GradientChangesFloorAndBackdrop | GradientChangesSprites,
        _ => throw new InvalidOperationException($"Undefined gradient blend {blend}."),
    };

    /// <summary>
    /// The levels the tile shaders' colour maths takes a described colour in (#1544): the shaders add, blend and halve
    /// colours on the game's fade scale, thirty-two levels a channel, on which every colour the game shows lies, so a colour
    /// reaches them as its own levels; a colour off the scale is rejected.
    /// </summary>
    private static FadeLevelColour ShaderLevels(RgbColor colour) => colour.FadeLevels;

    /// <summary>A sprite tint's levels packed into one shader word, five bits a channel, red lowest, as the sprite pass unpacks it.</summary>
    private static uint PackedShaderLevels(RgbColor colour)
    {
        FadeLevelColour levels = ShaderLevels(colour);
        return (uint)(levels.Red | levels.Green << 5 | levels.Blue << 10);
    }

    /// <summary>
    /// Draws a gradient over the composed layers (#1468): its band in the header's first and end scanline words, its
    /// stop count in the level word, and each stop as a row: its colour's channels, then its blend with the line its
    /// band begins on above it. The shader finds each line's stop.
    /// </summary>
    private unsafe void DispatchGradient(ScreenGradient gradient)
    {
        var data = ClearUploadConstants();
        data[9] = (uint)gradient.Stops.Length;
        data[25] = (uint)gradient.Top; data[26] = (uint)gradient.Bottom;
        for (int stop = 0; stop < gradient.Stops.Length; stop++)
        {
            GradientStop shown = gradient.Stops[stop];
            int offset = D3D11ShaderLayout.ScanlineParametersWordOffset + stop * 4;
            FadeLevelColour stopColour = ShaderLevels(shown.Colour);
            data[offset] = (uint)stopColour.Red; data[offset + 1] = (uint)stopColour.Green;
            data[offset + 2] = (uint)stopColour.Blue;
            data[offset + 3] = GradientBlendWord(shown.Blend) | (uint)shown.Line << D3D11ShaderLayout.GradientStopLineShift;
        }
        fixed (uint* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.CSSetShader(screenGradientShader);
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
        var childPacket = new DisplayPacket(packet.FrameNumber, window.Scene);
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
