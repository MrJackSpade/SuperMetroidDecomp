using System.Security.Cryptography;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

/// <summary>Exercises ROM import validation, extraction repair, ROM-free startup, and audio parity scenarios.</summary>
internal static class AssetImportVerification
{
/// <summary>Runs input validation and, when a ROM path is supplied, the full install and integration verification flow.</summary>
/// <param name="romPath">Optional retail ROM path; null limits the run to input-validation scenarios.</param>
/// <returns>Zero when every verification succeeds; failures propagate as exceptions to the process boundary.</returns>
public static int Run(string? romPath)
{
string temporary = Directory.CreateTempSubdirectory("SuperMetroid-import-verification-").FullName;
try
{
    string missingRoot = Path.Combine(temporary, "must-not-exist");
    Reject<InvalidDataException>(() => GameAssetInstaller.Install(new MemoryStream("not a ROM"u8.ToArray()), missingRoot));
    Check(!Directory.Exists(missingRoot), "invalid input creates no installation");
    using (var oversized = new NonSeekableStream(new byte[SupportedCartridge.RomByteCount + SupportedCartridge.CopierHeaderByteCount + 100]))
    {
        Reject<InvalidDataException>(() => GameAssetInstaller.Install(oversized, missingRoot));
        Check(oversized.BytesRead == SupportedCartridge.RomByteCount + SupportedCartridge.CopierHeaderByteCount + 1,
            "non-seekable oversized documents are bounded before extraction");
    }
    using (var cancelled = new CancellationTokenSource())
    {
        cancelled.Cancel();
        Reject<OperationCanceledException>(() => GameAssetInstaller.Install(new MemoryStream(new byte[SupportedCartridge.RomByteCount]), missingRoot, cancelled.Token));
        Check(!Directory.Exists(missingRoot), "cancelled selection creates no installation");
    }
    if (romPath is null)
    {
        Console.WriteLine("PASS input validation. --asset-import-rom adds extraction, repair, and Android-session integration from the repository ROM.");
        return 0;
    }
    using Stream original = File.OpenRead(romPath);
    byte[] rom = SupportedCartridge.Read(original);
    string input = Path.Combine(temporary, "user-chosen-name.sfc");
    byte[] headered = new byte[rom.Length + SupportedCartridge.CopierHeaderByteCount];
    Array.Fill<byte>(headered, 123, 0, SupportedCartridge.CopierHeaderByteCount);
    rom.CopyTo(headered, SupportedCartridge.CopierHeaderByteCount);
    File.WriteAllBytes(input, headered);
    string root = Path.Combine(temporary, "app-data");
    GameInstallation installed = GameAssetInstaller.Install(input, root);
    Check(File.ReadAllBytes(input).AsSpan().SequenceEqual(headered), "original selected ROM is preserved");
    Check(File.ReadAllBytes(installed.RomPath).AsSpan().SequenceEqual(rom), "installed ROM strips only the copier header");
    Check(GameAssetInstaller.TryOpenExtractedContent(root) is not null,
        "complete extracted content is recognized with its source-revision receipt");
    string heldRom = Path.Combine(temporary, "temporarily-held-installed-rom.smc");
    File.Move(installed.RomPath, heldRom);
    try
    {
        GameInstallation? romFreeContent = GameAssetInstaller.TryOpenExtractedContent(root);
        Check(romFreeContent is not null && romFreeContent.LoadAudio().CanonicalSampleCount == 112,
            "installed presentation assets remain usable without the private ROM copy");
        Check(GameAssetInstaller.OpenOrRepair(root) is not null,
            "normal host startup accepts complete extracted content without a ROM copy");
        SuperMetroidAddressSpace romFreeMemory = (romFreeContent ??
            throw new InvalidOperationException("Complete ROM-free installation was not reopened."))
            .OpenRuntimeAddressSpace();
        Check(romFreeMemory.GetType().GetProperty("Rom") is null &&
            romFreeMemory.WorkRam.Length == SuperMetroidAddressSpace.WorkRamByteCount &&
            romFreeMemory.SaveRam.Length == SuperMetroidAddressSpace.SaveRamByteCount,
            "installed runtime starts with mutable memory but no cartridge allocation");
        using (var romFreeSession = new SuperMetroid.Android.AndroidSessionData(root))
        {
            Check(romFreeSession.Bus.GetType().GetProperty("Rom") is null,
                "installed Android host boots from extracted assets with no ROM copy");
            for (int frame = 0; frame < 90; frame++)
            {
                romFreeSession.Game.SetAudioAcknowledgements(
                    romFreeSession.Audio.ReadAcknowledgements());
                var captured = romFreeSession.Game.StepCaptured(0, frame + 1,
                    romFreeSession.Generation);
                romFreeSession.Audio.RenderFrame(captured.Frame.AudioCommands);
            }
            string saved = romFreeSession.SaveSlot(0);
            string loaded = romFreeSession.LoadSlot(0);
            Check(saved.Contains("slot 0", StringComparison.OrdinalIgnoreCase) &&
                loaded.Contains("slot 0", StringComparison.OrdinalIgnoreCase) &&
                romFreeSession.Bus.GetType().GetProperty("Rom") is null,
                "installed Android state save/load retains cartridge-free memory");
        }
        Check(GameAssetInstaller.EnsureInstalled(root) is null,
            "repair requires a ROM even though validated extracted content can boot");
    }
    finally
    {
        File.Move(heldRom, installed.RomPath);
    }
    var catalog = ExtractedAudioAssetCatalog.Load(installed.AudioDirectory);
    Check(catalog.CanonicalSampleCount == 112 && catalog.SourceMappingCount == 935, "all canonical samples and source aliases extracted");
    Console.WriteLine($"PASS installed runtime resources: {catalog.CanonicalSampleCount} WAV samples, {catalog.SourceMappingCount} aliases.");

    // The repository's standalone extraction is the independent reference audio set.
    {
        string reference = Path.GetFullPath("standalone-assets/audio");
        string[] expected = Directory.GetFiles(reference, "*", SearchOption.AllDirectories);
        foreach (string file in expected)
        {
            string relative = Path.GetRelativePath(reference, file);
            Check(File.ReadAllBytes(file).AsSpan().SequenceEqual(File.ReadAllBytes(Path.Combine(installed.AudioDirectory, relative))),
                $"reference parity: {relative}", quiet: true);
        }
        Check(expected.Length == Directory.GetFiles(installed.AudioDirectory, "*", SearchOption.AllDirectories).Length,
            "ROM-only extraction matches every reference file, with no missing or extra files");
    }

    var renderer = new CartridgeAudioRenderer(catalog);
    long audible = 0;
    for (int frame = 0; frame < 120; frame++)
    {
        CartridgeAudioCommand[] commands = frame == 0
            ? [CartridgeAudioCommand.Upload(AudioAssetCatalogData.Common.SnesAddress),
               CartridgeAudioCommand.Upload(AudioAssetCatalogData.Music[0].SnesAddress),
               CartridgeAudioCommand.WritePort(AudioRomData.Apu.MusicPort, AudioRomDataMusicTracks.Title)] : [];
        foreach (short sample in renderer.RenderFrame(commands)) if (sample != 0) audible++;
    }
    Check(audible > 0, "newly extracted resources produce audible title music through the production renderer");

    string receipt = Path.Combine(installed.ContentDirectory, GameInstallationLayout.ReceiptFileName);
    DateTime installedAt = File.GetLastWriteTimeUtc(receipt);
    File.WriteAllText(Path.Combine(installed.AudioDirectory, "user-note.txt"), "keep custom files");
    using (var stream = new NonSeekableStream(headered)) GameAssetInstaller.Install(stream, root);
    Check(File.GetLastWriteTimeUtc(receipt) == installedAt && File.Exists(Path.Combine(installed.AudioDirectory, "user-note.txt")),
        "repeat Android-style stream import reuses complete content instead of replacing it");

    string settings = Path.Combine(root, "SuperMetroid.ini");
    File.WriteAllText(settings, SuperMetroidGameOptionsIni.DefaultFileContents);
    string save = Path.Combine(root, "SuperMetroid.save.json");
    GameSaveFileStore.WriteAtomic(new SuperMetroid.AssetExtraction.CartridgeImportAddressSpace(rom), save, installed.LoadMaps());
    byte[] saveBefore = File.ReadAllBytes(save);
    string waveform = Directory.GetFiles(Path.Combine(installed.AudioDirectory, "samples"), "*.wav")[0];
    byte[] waveBefore = File.ReadAllBytes(waveform);
    File.WriteAllBytes(waveform, "broken wave"u8.ToArray());
    Check(GameAssetInstaller.TryOpenExtractedContent(root) is null,
        "a damaged extracted asset is not mistaken for valid ROM-free content");
    File.Move(installed.RomPath, heldRom);
    try
    {
        Check(GameAssetInstaller.OpenOrRepair(root) is null,
            "startup rejects damaged extracted content when no ROM can repair it");
        Reject<InvalidDataException>(() => installed.OpenRuntimeAddressSpace());
    }
    finally
    {
        File.Move(heldRom, installed.RomPath);
    }
    File.Delete(input);
    Check(GameAssetInstaller.EnsureInstalled(root) is not null, "repair works after the original selected document is gone");
    Check(File.ReadAllBytes(waveform).AsSpan().SequenceEqual(waveBefore), "damaged waveform is regenerated exactly");
    Check(File.ReadAllBytes(save).AsSpan().SequenceEqual(saveBefore) && File.ReadAllText(settings) == SuperMetroidGameOptionsIni.DefaultFileContents,
        "repair preserves saves and configuration outside content");

    byte[] wrong = (byte[])rom.Clone();
    wrong[0] ^= 1;
    Reject<InvalidDataException>(() => GameAssetInstaller.Install(new MemoryStream(wrong), root));
    Check(File.ReadAllBytes(installed.RomPath).AsSpan().SequenceEqual(rom), "unsupported ROM never replaces a valid installation");
    using (var gate = new FileStream(Path.Combine(root, ".game-install.lock"), FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        Reject<IOException>(() => GameAssetInstaller.Install(new MemoryStream(rom), root));

    File.Delete(receipt);
    using (var cancellation = new CancellationTokenSource())
    {
        var progress = new InlineProgress(message => { if (message == "Extracting audio…") cancellation.Cancel(); });
        Reject<OperationCanceledException>(() => GameAssetInstaller.EnsureInstalled(root, cancellation.Token, progress));
    }
    Check(File.Exists(installed.RomPath) && Directory.GetDirectories(root, ".game.install-*").Length == 0,
        "cancelled repair retains old content and cleans its staging directory");
    GameAssetInstaller.EnsureInstalled(root);
    string previous = Path.Combine(root, ".game.previous");
    if (Directory.Exists(previous)) Directory.Delete(previous, recursive: true);
    Directory.Move(installed.ContentDirectory, previous);
    Check(GameAssetInstaller.EnsureInstalled(root) is not null && File.Exists(installed.RomPath),
        "startup recovers interruption between directory publication steps");

    using (var session = new SuperMetroid.Android.AndroidSessionData(root))
    {
        for (int frame = 0; frame < 90; frame++)
        {
            session.Game.SetAudioAcknowledgements(session.Audio.ReadAcknowledgements());
            var captured = session.Game.StepCaptured(0, frame + 1, session.Generation);
            session.Audio.RenderFrame(captured.Frame.AudioCommands);
        }
        Check(session.Bus.GetType().GetProperty("Rom") is null,
            "Android session boots from extracted content without retaining cartridge bytes");
    }
    Console.WriteLine("PASS ROM import, header normalization, exact audio parity, non-seekable input, cancellation, repair, recovery, and Android startup.");
    return 0;
}
finally
{
    Directory.Delete(temporary, recursive: true);
}

}

/// <summary>Fails the verification with the supplied description when an expected condition is false.</summary>
/// <param name="value">Condition that must hold.</param>
/// <param name="description">Failure text and successful console label.</param>
/// <param name="quiet">When true, suppresses the success line while retaining failure behavior.</param>
static void Check(bool value, string description, bool quiet = false)
{
    if (!value) throw new InvalidDataException(description);
    if (!quiet) Console.WriteLine("PASS " + description);
}
/// <summary>Runs an action and fails unless it throws the expected exception type.</summary>
/// <typeparam name="T">Exception type required for the action to count as rejected.</typeparam>
/// <param name="action">Operation expected to fail with <typeparamref name="T"/>.</param>
static void Reject<T>(Action action) where T : Exception
{
    try { action(); }
    catch (T) { return; }
    throw new InvalidDataException($"Expected {typeof(T).Name}.");
}
/// <summary>Adapts a callback to the progress interface used by installation cancellation scenarios.</summary>
/// <param name="report">Callback invoked for each reported installation message.</param>
sealed class InlineProgress(Action<string> report) : IProgress<string>
{
    /// <summary>Forwards an installation progress message to the configured callback.</summary>
    /// <param name="value">Progress message reported by the installer.</param>
    public void Report(string value) => report(value);
}
/// <summary>Read-only stream wrapper that disables seeking and caps each read to exercise bounded streaming input.</summary>
/// <param name="bytes">Input data exposed through the non-seekable wrapper.</param>
sealed class NonSeekableStream(byte[] bytes) : Stream
{
    /// <summary>Underlying in-memory source used for the bounded reads.</summary>
    private readonly MemoryStream inner = new(bytes);
    /// <summary>Cumulative number of bytes returned to callers.</summary>
    public int BytesRead { get; private set; }
    /// <summary>Indicates that input reads are supported.</summary>
    public override bool CanRead => true;
    /// <summary>Indicates that seeking is intentionally unavailable.</summary>
    public override bool CanSeek => false;
    /// <summary>Indicates that writes are intentionally unavailable.</summary>
    public override bool CanWrite => false;
    /// <summary>Seeking is unsupported for this test stream.</summary>
    public override long Length => throw new NotSupportedException();
    /// <summary>Seeking and position assignment are unsupported for this test stream.</summary>
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

    /// <summary>Reads at most 8191 bytes and tracks the total bytes consumed from the input.</summary>
    /// <param name="buffer">Destination for the bytes read.</param>
    /// <param name="offset">Starting destination offset.</param>
    /// <param name="count">Maximum requested number of bytes.</param>
    /// <returns>Number of bytes copied to the destination.</returns>
    public override int Read(byte[] buffer, int offset, int count)
    {
        int read = inner.Read(buffer, offset, Math.Min(count, 8191));
        BytesRead += read;
        return read;
    }
    /// <summary>Writing is unsupported because the wrapper is a read-only input source.</summary>
    public override void Flush() => throw new NotSupportedException();
    /// <summary>Seeking is unsupported because the wrapper exposes a forward-only input stream.</summary>
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    /// <summary>Changing the input length is unsupported.</summary>
    public override void SetLength(long value) => throw new NotSupportedException();
    /// <summary>Writing is unsupported because the wrapper is a read-only input source.</summary>
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    /// <summary>Disposes the in-memory source when requested, then releases the base stream resources.</summary>
    /// <param name="disposing">True when called by <see cref="IDisposable.Dispose"/> rather than finalization.</param>
    protected override void Dispose(bool disposing) { if (disposing) inner.Dispose(); base.Dispose(disposing); }
}
}
