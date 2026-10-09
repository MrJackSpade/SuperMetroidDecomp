using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace SuperMetroid.Rendering.Direct3D11;

/// <summary>Integer compute composition from owned display inputs.</summary>
/// <remarks>Unsupported operations fail explicitly. Readback is diagnostic, not the presentation path.</remarks>
public sealed partial class D3D11FrameRenderer : IDisposable
{
    /// <summary>Shared render device that owns this renderer's thread-affine GPU resources.</summary>
    internal readonly D3D11RenderDevice owner;
    /// <summary>Factory for loading embedded shader bytecode, replaceable by strict missing-resource verification.</summary>
    private readonly Func<string, Stream?> openShaderResource;
    /// <summary>Compute shader that composes solid-color snapshots into the output texture.</summary>
    private readonly ID3D11ComputeShader shader;
    /// <summary>Compute-writable output texture consumed by display and readback paths.</summary>
    internal readonly ID3D11Texture2D output;
    internal RenderFrameIdentity? renderedIdentity;
    /// <summary>Identity of the most recently submitted frame whose composition completed successfully.</summary>
    internal RenderFrameIdentity? SubmittedIdentity => renderedIdentity;
    /// <summary>Device shared with presenters and other renderers.</summary>
    internal D3D11RenderDevice DeviceOwner => owner;
    /// <summary>Unordered-access binding for the composed output texture.</summary>
    private readonly ID3D11UnorderedAccessView view;
    /// <summary>Shader-resource binding that exposes the composed output to the display pixel shader.</summary>
    private readonly ID3D11ShaderResourceView displaySource;
    /// <summary>Vertex shader used to draw the output texture to a presentation target.</summary>
    private readonly ID3D11VertexShader displayVertexShader;
    /// <summary>Pixel shader that presents the composed output texture.</summary>
    private readonly ID3D11PixelShader displayPixelShader;
    /// <summary>Constant buffer for the solid-color compute pass.</summary>
    private readonly ID3D11Buffer constants;
    /// <summary>Compute shader that resolves SNES tile layers.</summary>
    private readonly ID3D11ComputeShader tileShader;
    /// <summary>Compute shader that composes title-screen gradient layers.</summary>
    private readonly ID3D11ComputeShader titleGradientShader;
    /// <summary>Structured shader-resource buffer containing the uploaded PPU memory view.</summary>
    private readonly ID3D11Buffer memoryBuffer;
    /// <summary>Shader-resource binding for the uploaded PPU memory buffer.</summary>
    private readonly ID3D11ShaderResourceView memoryView;
    /// <summary>Compute output texture holding resolved object pixels and metadata.</summary>
    private readonly ID3D11Texture2D resolvedObjects;
    /// <summary>Unordered-access binding for resolved object pixels.</summary>
    private readonly ID3D11UnorderedAccessView objectView;
    /// <summary>GPU resources owned by this renderer, released in reverse creation order.</summary>
    private readonly List<IDisposable> resources = [];
    /// <summary>Whether owned resources have been released; guards repeated disposal.</summary>
    internal bool disposed;
    /// <summary>Reusable upload staging for the packed PPU memory buffer.</summary>
    private readonly uint[] memoryUpload = new uint[D3D11ShaderLayout.PpuMemoryWords];
    /// <summary>Reusable upload staging for solid-pass constants.</summary>
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
        this.owner = owner ?? throw new ArgumentNullException(nameof(owner));
        this.openShaderResource = openShaderResource ?? throw new ArgumentNullException(nameof(openShaderResource));
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
        ArgumentNullException.ThrowIfNull(packet);
        renderedIdentity = null;
        RenderCore(packet);
        renderedIdentity = packet.Identity;
    }

    /// <summary>Dispatches the specialized solid, layered, or Mode 7 compute path for a validated frame snapshot.</summary>
    /// <param name="packet">Snapshot containing the composition inputs and dimensions to submit.</param>
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
        data[0] = (uint)(color.R | color.G << 8 | color.B << 16 | color.A << 24);
        data[1] = (uint)packet.BrightnessPasses.Length;
        data[2] = (uint)packet.Width; data[3] = (uint)packet.Height;
        for (int i = 0; i < packet.BrightnessPasses.Length; i++) data[D3D11ShaderLayout.SolidHeaderWords + i] = packet.BrightnessPasses[i];
        fixed (uint* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.CSSetShader(shader);
        owner.Context.CSSetConstantBuffer(0, constants);
        owner.Context.CSSetUnorderedAccessView(0, view);
        owner.Context.Dispatch((uint)((packet.Width + D3D11ShaderLayout.DispatchTileEdge - 1) / D3D11ShaderLayout.DispatchTileEdge),
            (uint)((packet.Height + D3D11ShaderLayout.DispatchTileEdge - 1) / D3D11ShaderLayout.DispatchTileEdge), 1);
    }

    /// <summary>Unbinds renderer shader/resource state and releases its owned GPU objects in reverse creation order on the owner thread; the shared device remains alive and repeated disposal is harmless.</summary>
    /// <exception cref="InvalidOperationException">First disposal is attempted from a different owner thread.</exception>
    public void Dispose()
    {
        if (disposed) return;
        owner.VerifyOwner();
        owner.Context.CSSetShader(null);
        owner.Context.VSSetShader(null!);
        owner.Context.PSSetShader(null!);
        owner.Context.PSSetShaderResource(0, null!);
        owner.Context.PSSetConstantBuffer(0, null!);
        owner.Context.CSSetConstantBuffer(0, null);
        owner.Context.CSSetShaderResource(0, null);
        owner.Context.CSSetUnorderedAccessView(1, null);
        DisposeResources();
        disposed = true;
    }

    /// <summary>Registers a newly created disposable GPU object for this renderer's reverse-order cleanup.</summary>
    /// <typeparam name="T">A disposable Direct3D resource type.</typeparam>
    /// <param name="value">Resource whose lifetime becomes owned by this renderer.</param>
    /// <returns>The same resource, allowing registration at its creation site.</returns>
    internal T Own<T>(T value) where T : IDisposable { resources.Add(value); return value; }

    /// <summary>Releases every registered resource in reverse creation order and clears the ownership list.</summary>
    private void DisposeResources()
    {
        for (int i = resources.Count - 1; i >= 0; i--) resources[i].Dispose();
        resources.Clear();
    }
    /// <summary>Creates a compute shader from the named embedded bytecode resource.</summary>
    /// <param name="name">Manifest resource name containing compiled compute-shader bytecode.</param>
    /// <returns>The device-created compute shader.</returns>
    private ID3D11ComputeShader LoadShader(string name)
        => owner.Device.CreateComputeShader(LoadShaderBytes(name));

    /// <summary>Reads a named shader resource completely into a byte array for Direct3D shader creation.</summary>
    /// <param name="name">Manifest resource name to request from the configured resource provider.</param>
    /// <returns>The resource's compiled bytecode.</returns>
    /// <exception cref="InvalidDataException">The configured provider does not contain the requested shader.</exception>
    private byte[] LoadShaderBytes(string name)
    {
        using Stream resource = openShaderResource(name)
            ?? throw new InvalidDataException($"Build-generated shader {name} is missing.");
        using var bytes = new MemoryStream(); resource.CopyTo(bytes);
        return bytes.ToArray();
    }
}
