using System.Runtime.InteropServices;
using SuperMetroid.Core.Audio;

/// <summary>Explicitly loaded translated SPC/DSP reference, not an original SPC CPU oracle.</summary>
internal sealed class ClimbNativeAudioReference : IDisposable
{
    private readonly nint library, player;
    private readonly Destroy destroy;
    private readonly Upload upload;
    private readonly Write write;
    private readonly Read read;
    private readonly Generate generate;
    private readonly short[] raw = new short[1068], host = new short[1600];
    public int Frames { get; private set; }

    public ClimbNativeAudioReference(string path)
    {
        library = NativeLibrary.Load(Path.GetFullPath(path));
        T Export<T>(string name) where T : Delegate =>
            Marshal.GetDelegateForFunctionPointer<T>(NativeLibrary.GetExport(library, name));
        try
        {
            destroy = Export<Destroy>("sm_audio_destroy");
            upload = Export<Upload>("sm_audio_upload");
            write = Export<Write>("sm_audio_write_port");
            read = Export<Read>("sm_audio_read_port");
            generate = Export<Generate>("sm_audio_generate_frame");
            player = Export<Create>("sm_audio_create")();
            if (player == 0) throw new InvalidOperationException("Native audio allocation failed.");
        }
        catch { NativeLibrary.Free(library); throw; }
    }

    public void Apply(CartridgeAudioCommand command, ExtractedAudioAssetCatalog assets)
    {
        if (command.Kind == CartridgeAudioCommandKind.Upload)
        {
            var stream = assets.GetUpload(command.UploadAddress).ToArray();
            if (upload(player, stream, stream.Length) != 1)
                throw new InvalidDataException("Native Climb audio upload rejected.");
        }
        else if (write(player, command.Port, command.Value) != 1)
            throw new InvalidDataException("Native Climb audio port write rejected.");
    }

    public void Verify(short[] actual, ManagedSpcPlayer managed)
    {
        if (generate(player, raw, 534) != 534)
            throw new InvalidDataException("Incomplete native Climb audio frame.");
        PcmFrameResampler.ResampleStereoLinear(raw, host);
        for (int i = 0; i < actual.Length; i++)
            if (actual[i] != host[i])
                throw new InvalidDataException($"Climb mixed PCM differs at audio frame {Frames}, sample {i}: managed={actual[i]}, native={host[i]}.");
        for (int port = 0; port < 4; port++)
            if (read(player, port) != managed.ReadPort(port))
                throw new InvalidDataException($"Climb acknowledgement differs at audio frame {Frames}, port {port}.");
        Frames++;
    }

    public void Dispose() { destroy(player); NativeLibrary.Free(library); }
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate nint Create();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void Destroy(nint player);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int Upload(nint player, byte[] data, int length);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int Write(nint player, int port, byte value);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int Read(nint player, int port);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int Generate(nint player, [Out] short[] output, int frames);
}
