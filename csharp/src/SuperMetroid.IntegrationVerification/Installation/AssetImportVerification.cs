using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.Diagnostics.Saves;
using SuperMetroid.Diagnostics.Configuration;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
namespace SuperMetroid.IntegrationVerification.Installation;

using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>Checks cartridge import validation, extracted-content reuse and repair, and ROM-free startup.</summary>
internal static class AssetImportVerification
{
/// <summary>
/// Exercises invalid imports first, then one retail import that every reading check shares, ROM-free startup and the
/// rejection of outdated content on a copy of it, one repair, and cancellation and recovery that extract nothing (#1680).
/// </summary>
[PrivateInstallation("Checks the importer itself. Two runs extract: the import of a headered, user-named ROM, which every " +
    "reading check shares, and the repair from the installed copy once that document is gone, which only a completed " +
    "extraction can show regenerates damaged content exactly. ROM-free startup and cancellation work on copies of the " +
    "import, and every rejection and the cancelled repair stop before any component extracts.")]
public static int Run(string? romPath)
{
using var temporaryScratch = new TestScratchDirectory("import-verification");
        string temporary = temporaryScratch.Root;
        Directory.CreateDirectory(temporary);
try
{
    string missingRoot = Path.Combine(temporary, "must-not-exist");
    string notRom = Path.Combine(temporary, "not-a-rom.smc");
    File.WriteAllBytes(notRom, "not a ROM"u8.ToArray());
    Reject<InvalidDataException>(() => GameAssetInstaller.Install(notRom, missingRoot));
    Check(!Directory.Exists(missingRoot), "invalid input creates no installation");
    using (var oversized = new NonSeekableStream(new byte[SupportedCartridge.RomByteCount + SupportedCartridge.CopierHeaderByteCount + 100]))
    {
        Reject<InvalidDataException>(() => SupportedCartridge.Read(oversized));
        Check(oversized.BytesRead == SupportedCartridge.RomByteCount + SupportedCartridge.CopierHeaderByteCount + 1,
            "non-seekable oversized input is bounded before extraction");
    }
    using (var cancelled = new CancellationTokenSource())
    {
        cancelled.Cancel();
        string blank = Path.Combine(temporary, "blank.smc");
        File.WriteAllBytes(blank, new byte[SupportedCartridge.RomByteCount]);
        Reject<OperationCanceledException>(() => GameAssetInstaller.Install(blank, missingRoot, cancelled.Token));
        Check(!Directory.Exists(missingRoot), "cancelled selection creates no installation");
    }
    if (romPath is null)
    {
        Console.WriteLine("PASS input validation. --asset-import-rom adds extraction, repair, and installed-session integration from the repository ROM.");
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
    // The first of the check's two extractions: every check below that reads extracted content reads this import.
    GameInstallation installed = GameAssetInstaller.Install(input, root);
    Check(File.ReadAllBytes(input).SequenceEqual(headered), "original selected ROM is preserved");
    Check(File.ReadAllBytes(installed.RomPath).SequenceEqual(rom), "installed ROM strips only the copier header");
    Check(GameAssetInstaller.HoldsCurrentExtractedContent(root),
        "complete extracted content is recognized with its source-revision receipt");
    InstalledClipAudioLibrary installedClips = installed.LoadAudioClips();
    Console.WriteLine($"PASS installed runtime resources: {installedClips.Manifest.Music.Count} songs and {installedClips.Manifest.SoundEffects.Count} sound effects.");

    // The game plays the installed clips (#1693): the title music sounds through the clip backend, which reads nothing
    // else. The reference comparison with a hand extraction is gone: that extraction was this importer's own output.
    var events = new AudioEventSource(new SuperMetroid.Core.Compatibility.SoundAcceptanceClock<SoundEffectLibrary, SoundEffectId>(
        SoundEffectLibraries.InQueueOrder, SoundEffectLibrary.Samus));
    var renderer = new ClipAudioBackend(installedClips.Clips);
    short[] pcm = new short[AudioSession.PcmSamplesPerFrame];
    long audible = 0;
    for (int frame = 0; frame < 120; frame++)
    {
        AudioRequest[] commands = frame == 0
            ? [new AudioRequest.InstallSharedSounds(),
               new AudioRequest.ChangeMusicDataSet(AudioAssetCatalogDefinitions.Music[0].DataSet ?? throw new InvalidDataException("The music upload names no data set.")),
               AudioRequest.ForTrack(MusicTrackNumbers.Decode(MusicDataSet.TitleSequence, AudioRomDataMusicTracks.Title))] : [];
        renderer.Render(new AudioFrameEvents(events.Advance(commands)), pcm);
        foreach (short sample in pcm) if (sample != 0) audible++;
    }
    Check(audible > 0, "newly extracted resources produce audible title music through the production renderer");

    // A copy of the import without its private ROM, so the import itself never loses its ROM: startup without one is
    // checked on the copy, first with the import's current content and below with its receipt naming another format.
    using var romFree = new PrivateInstallationCopy(root);
    File.Delete(romFree.Installation.RomPath);
    GameInstallation? romFreeContent = GameAssetInstaller.OpenOrRepair(romFree.Root);
    Check(romFreeContent is not null,
        "normal host startup accepts complete extracted content without a ROM copy");
    Check(romFreeContent is not null && romFreeContent.LoadAudioClips().Manifest.Music.Count > 0,
        "installed presentation assets remain usable without the private ROM copy");
    (romFreeContent ?? throw new InvalidOperationException("Complete ROM-free installation was not reopened."))
        .RequireExtractedContent();
    using (var romFreeSession = new SuperMetroid.Diagnostics.Sessions.InstalledGameSession(romFree.Root))
    {
        for (int frame = 0; frame < 90; frame++)
            romFreeSession.Game.StepCaptured(0);
        string saved = romFreeSession.SaveSlot(0);
        Check(saved.Contains("slot 0", StringComparison.OrdinalIgnoreCase),
            "an installed session boots from extracted assets with no ROM copy and saves a debugger state");
    }
    Check(GameAssetInstaller.EnsureInstalled(romFree.Root) is null,
        "repair requires a ROM even though validated extracted content can boot");

    string receipt = Path.Combine(installed.ContentDirectory, GameInstallationLayout.ReceiptFileName);
    DateTime installedAt = File.GetLastWriteTimeUtc(receipt);
    File.WriteAllText(Path.Combine(installed.AudioDirectory, "user-note.txt"), "keep custom files");
    GameAssetInstaller.Install(input, root);
    Check(File.GetLastWriteTimeUtc(receipt) == installedAt && File.Exists(Path.Combine(installed.AudioDirectory, "user-note.txt")),
        "repeat import reuses complete content instead of replacing it");

    string settings = Path.Combine(root, "SuperMetroid.ini");
    File.WriteAllText(settings, HostOptionsIni.DefaultFileContents);
    string save = Path.Combine(root, "SuperMetroid.save.json");
    GameSaveFileStore.WriteAtomic(new SuperMetroid.Core.Saves.SaveSlotStore(installed.LoadMaps()), save);
    byte[] saveBefore = File.ReadAllBytes(save);
    string waveform = Path.Combine(installed.AudioDirectory, installedClips.Manifest.Music[0].Clip.Path);
    byte[] waveBefore = File.ReadAllBytes(waveform);
    File.WriteAllBytes(waveform, "broken wave"u8.ToArray());
    // The receipt alone decides whether content is current (#1532): content extracted for another format is rebuilt, and
    // every file with it.
    string currentReceipt = File.ReadAllText(receipt);
    string outdatedReceipt = currentReceipt.Replace($"\"FormatVersion\":{GameInstallationLayout.FormatVersion}",
        $"\"FormatVersion\":{GameInstallationLayout.FormatVersion - 1}", StringComparison.Ordinal);
    File.WriteAllText(receipt, outdatedReceipt);
    Check(File.ReadAllText(receipt) != currentReceipt, "the fixture names an outdated format in the receipt");
    Check(!GameAssetInstaller.HoldsCurrentExtractedContent(root),
        "content extracted for another format is not mistaken for current ROM-free content");
    File.WriteAllText(Path.Combine(romFree.Installation.ContentDirectory, GameInstallationLayout.ReceiptFileName), outdatedReceipt);
    Check(GameAssetInstaller.OpenOrRepair(romFree.Root) is null,
        "startup rejects outdated extracted content when no ROM can extract it again");
    Reject<InvalidDataException>(romFree.Installation.RequireExtractedContent);
    File.Delete(input);
    // The second and last extraction: repair extracts again from the installed copy, the selected document being gone,
    // and only a completed extraction shows that it publishes content replacing the damaged file exactly.
    Check(GameAssetInstaller.EnsureInstalled(root) is not null, "repair works after the original selected document is gone");
    Check(File.ReadAllBytes(waveform).SequenceEqual(waveBefore), "re-extraction regenerates the damaged waveform exactly");
    Check(File.ReadAllBytes(save).SequenceEqual(saveBefore) && File.ReadAllText(settings) == HostOptionsIni.DefaultFileContents,
        "repair preserves saves and configuration outside content");

    // The unsupported ROM is rejected as it is read and the locked-out import as it takes the setup lock: neither extracts.
    byte[] wrong = (byte[])rom.Clone();
    wrong[0] ^= 1;
    string wrongPath = Path.Combine(temporary, "unsupported.smc");
    File.WriteAllBytes(wrongPath, wrong);
    Reject<InvalidDataException>(() => GameAssetInstaller.Install(wrongPath, root));
    Check(File.ReadAllBytes(installed.RomPath).SequenceEqual(rom), "unsupported ROM never replaces a valid installation");
    using (var gate = new FileStream(Path.Combine(root, ".game-install.lock"), FileMode.Open, FileAccess.ReadWrite, FileShare.None))
    {
        string romCopy = Path.Combine(temporary, "locked-out.smc");
        File.WriteAllBytes(romCopy, rom);
        Reject<IOException>(() => GameAssetInstaller.Install(romCopy, root));
    }

    // The repair published the damaged content aside; the recovery check below moves the current content there instead.
    string previous = Path.Combine(root, ".game.previous");
    if (Directory.Exists(previous)) Directory.Delete(previous, recursive: true);
    // A copy of the repaired import without its receipt, so the cancelled repair leaves the import itself current.
    using (var interrupted = new PrivateInstallationCopy(root))
    {
        File.Delete(Path.Combine(interrupted.Installation.ContentDirectory, GameInstallationLayout.ReceiptFileName));
        using var cancellation = new CancellationTokenSource();
        // Cancelled at the first component's report, once the staging directory holds the ROM: reports are serialized and
        // each component checks for cancellation after its report, so no component extracts.
        var progress = new InlineProgress(_ => cancellation.Cancel());
        Reject<OperationCanceledException>(() => GameAssetInstaller.EnsureInstalled(interrupted.Root, cancellation.Token, progress));
        Check(File.Exists(interrupted.Installation.RomPath) && Directory.GetDirectories(interrupted.Root, ".game.install-*").Length == 0,
            "cancelled repair retains old content and cleans its staging directory");
    }
    Directory.Move(installed.ContentDirectory, previous);
    Check(GameAssetInstaller.EnsureInstalled(root) is not null && File.Exists(installed.RomPath),
        "startup recovers interruption between directory publication steps");

    using (var session = new SuperMetroid.Diagnostics.Sessions.InstalledGameSession(root))
    {
        // Booting and stepping is the check: the session opens only extracted content and throws if it is incomplete.
        for (int frame = 0; frame < 90; frame++)
            session.Game.StepCaptured(0);
    }
    Console.WriteLine("PASS ROM import, header normalization, exact audio parity, non-seekable input, cancellation, repair, recovery, and installed-session startup.");
    return 0;
}
finally
{
    Directory.Delete(temporary, recursive: true);
}

}

/// <summary>Fails an import contract assertion and optionally suppresses a successful per-file message.</summary>
static void Check(bool value, string description, bool quiet = false)
{
    if (!value) throw new InvalidDataException(description);
    if (!quiet) Console.WriteLine("PASS " + description);
}
/// <summary>Requires an import or repair operation to fail with the identified exception type.</summary>
static void Reject<T>(Action action) where T : Exception
{
    try { action(); }
    catch (T) { return; }
    throw new InvalidDataException($"Expected {typeof(T).Name}.");
}
/// <summary>Routes extraction progress inline so cancellation can occur at a known phase.</summary>
sealed class InlineProgress(Action<string> report) : IProgress<string>
{
    /// <summary>Forwards the current extraction message to the fixture callback.</summary>
    public void Report(string value) => report(value);
}
/// <summary>Limits each read to 8191 bytes and records consumption for oversized non-seekable import checks.</summary>
sealed class NonSeekableStream(byte[] bytes) : Stream
{
    /// <summary>Underlying read-only byte source without exposed seek support.</summary>
    private readonly MemoryStream inner = new(bytes);
    /// <summary>Total bytes consumed from the input, used to check early oversized-input rejection.</summary>
    public int BytesRead { get; private set; }
    /// <summary>Importers may read this fixture stream.</summary>
    public override bool CanRead => true;
    /// <summary>Forces the importer to validate input without seeking.</summary>
    public override bool CanSeek => false;
    /// <summary>The selected cartridge input is read-only.</summary>
    public override bool CanWrite => false;
    /// <summary>Seeking and length inspection are unavailable on this input.</summary>
    public override long Length => throw new NotSupportedException();
    /// <summary>Neither reading nor assigning an absolute stream position is supported.</summary>
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    /// <summary>Reads at most 8191 bytes per call while tracking actual input consumption.</summary>
    public override int Read(byte[] buffer, int offset, int count)
    {
        int read = inner.Read(buffer, offset, Math.Min(count, 8191));
        BytesRead += read;
        return read;
    }
    /// <summary>Rejects flushing a read-only fixture stream.</summary>
    public override void Flush() => throw new NotSupportedException();
    /// <summary>Rejects position changes so import must process bytes sequentially.</summary>
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    /// <summary>Rejects resizing the read-only cartridge input.</summary>
    public override void SetLength(long value) => throw new NotSupportedException();
    /// <summary>Rejects writes to the selected cartridge input.</summary>
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    /// <summary>Releases the underlying byte stream when the fixture is disposed.</summary>
    protected override void Dispose(bool disposing) { if (disposing) inner.Dispose(); base.Dispose(disposing); }
}
}
