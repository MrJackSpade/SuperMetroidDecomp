using System.Runtime.InteropServices;
using SuperMetroid.Core.Audio;

/// <summary>
/// Isolates incoming BRR history at an SRCN change with entirely constructed
/// samples. This is an exact diagnostic, not a tolerance-based playback test.
/// </summary>
internal static class DspSourceTransitionAudit
{
    public static int Run(string dllPath)
    {
        nint library = NativeLibrary.Load(Path.GetFullPath(dllPath));
        try
        {
            T Export<T>(string name) where T : Delegate => Marshal.GetDelegateForFunctionPointer<T>(NativeLibrary.GetExport(library, name));
            var create = Export<Create>("sm_audio_create");
            var destroy = Export<Destroy>("sm_audio_destroy");
            var ram = Export<Write>("sm_audio_diagnostic_write_ram");
            var register = Export<Write>("sm_audio_diagnostic_write_dsp");
            var cycle = Export<Cycle>("sm_audio_diagnostic_cycle_dsp");
            var read = Export<Read>("sm_audio_read_dsp_register");
            int failures = 0;
            foreach (bool retainHistory in new[] { false, true })
            {
                nint native = create();
                if (native == 0) throw new InvalidOperationException("Native DSP allocation failed.");
                try
                {
                    if (ram(native, -1, 0) != 0 || ram(native, 65536, 0) != 0 ||
                        register(native, -1, 0) != 0 || register(native, 128, 0) != 0 || cycle(0) != 0)
                        throw new InvalidOperationException("Native diagnostic controls accepted an invalid target.");
                    var managed = new ManagedSnesDsp(new byte[65536]);
                    // A is filter-zero constant 1024, B is zero residuals. B's
                    // extracted waveform is zero from reset for either filter;
                    // filter one only differs when entered with nonzero history.
                    managed.SetSampleBank(new ManagedPcmSampleBank("constructed transition", 0,
                        new Dictionary<byte, ManagedPcmSample>
                        {
                            [0] = new("constant", PcmSampleFormat.StockSampleRate, Enumerable.Repeat((short)1024, 16).ToArray(), 0),
                            [1] = new("zero", PcmSampleFormat.StockSampleRate, new short[16], 0),
                        }));
                    void Ram(int address, byte value)
                    {
                        if (ram(native, address, value) != 1) throw new InvalidOperationException("Native diagnostic RAM write rejected.");
                    }
                    void Register(byte address, byte value)
                    {
                        if (register(native, address, value) != 1) throw new InvalidOperationException("Native diagnostic DSP write rejected.");
                        managed.WriteRegister(address, value);
                    }
                    // Private source directory at $1000: two one-block loops at
                    // $2000/$2010. These are fixture addresses, not cartridge data.
                    for (int source = 0; source < 2; source++)
                    {
                        Ram(0x1000 + source * 4, (byte)(source * 16));
                        Ram(0x1001 + source * 4, 0x20);
                        Ram(0x1002 + source * 4, (byte)(source * 16));
                        Ram(0x1003 + source * 4, 0x20);
                    }
                    Ram(0x2000, 0xb3);
                    Ram(0x2010, retainHistory ? (byte)7 : (byte)3);
                    for (int i = 1; i <= 8; i++) { Ram(0x2000 + i, 0x11); Ram(0x2010 + i, 0); }
                    Register(SnesDspRegisterMap.Global.Flags, 0x20);
                    Register(SnesDspRegisterMap.Global.SourceDirectory, 0x10);
                    Register(SnesDspRegisterMap.Voice.VolumeLeft, 0x7f);
                    Register(SnesDspRegisterMap.Voice.VolumeRight, 0x7f);
                    Register(SnesDspRegisterMap.Voice.PitchLow, 0);
                    Register(SnesDspRegisterMap.Voice.PitchHigh, 0x10);
                    Register(SnesDspRegisterMap.Voice.SourceNumber, 0);
                    Register(SnesDspRegisterMap.Voice.Adsr1, 0);
                    Register(SnesDspRegisterMap.Voice.Gain, 0x7f);
                    Register(SnesDspRegisterMap.Global.KeyOff, 0);
                    Register(SnesDspRegisterMap.Global.KeyOn, 1);
                    int first = -1, differences = 0, initialPeak = 0;
                    for (int tick = 0; tick < 320; tick++)
                    {
                        if (tick == 40) Register(SnesDspRegisterMap.Voice.SourceNumber, 1);
                        if (cycle(native) != 1) throw new InvalidOperationException("Native diagnostic DSP cycle rejected.");
                        managed.Cycle();
                        int expected = read(native, SnesDspRegisterMap.Voice.SampleOutput);
                        byte actual = managed.ReadRegister(SnesDspRegisterMap.Voice.SampleOutput);
                        if (tick < 40) initialPeak = Math.Max(initialPeak, Math.Abs((int)unchecked((sbyte)actual)));
                        if (expected == actual) continue;
                        if (first < 0)
                        {
                            first = tick;
                            Console.WriteLine($"filter={(retainHistory ? 1 : 0)} first difference tick={tick}: OUTX managed={actual:X2}, native={expected:X2}");
                        }
                        differences++;
                    }
                    if (initialPeak == 0) throw new InvalidDataException("Constructed source never produced a nonzero starting signal.");
                    Console.WriteLine($"Constructed source transition filter={(retainHistory ? 1 : 0)}: {differences} differing OUTX ticks; first={first}.");
                    if (differences != 0) failures++;
                }
                finally { destroy(native); }
            }
            return failures == 0 ? 0 : 1;
        }
        finally { NativeLibrary.Free(library); }
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate nint Create();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void Destroy(nint player);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int Write(nint player, int address, byte value);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int Read(nint player, int address);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int Cycle(nint player);
}
