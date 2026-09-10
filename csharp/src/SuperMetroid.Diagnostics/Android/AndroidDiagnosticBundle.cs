using System.IO.Compression;
using System.Text.Json;

namespace SuperMetroid.Android;

/// <summary>
/// Local, portable diagnostic package. Includes an explicit allowlist rather than the
/// app directory: installed cartridge/audio assets and unrelated state slots stay private.
/// Debugger graphs themselves can embed cartridge data, so the UI warns before export.
/// </summary>
internal static class AndroidDiagnosticBundle
{
    public static string Create(string root, string destination, int slot)
    {
        if (slot is < 0 or > 9) throw new ArgumentOutOfRangeException(nameof(slot));
        string state = $"debug-states/SuperMetroid-debug-slot-{slot}.smstate";
        var files = new List<string>();
        foreach (string name in new[] { "SuperMetroid.save.json", "SuperMetroid.save.json.bak",
            "SuperMetroid.ini", "controller-bindings.json", "last-error.txt", "timing.log", "resume-timing.log", "frame-handoff.log", "input-events.log", state })
            if (File.Exists(Path.Combine(root, name))) files.Add(name);
        string recordings = Path.Combine(root, "input-recordings");
        if (Directory.Exists(recordings))
            foreach (string path in Directory.EnumerateFiles(recordings).Order(StringComparer.Ordinal))
                if (Path.GetExtension(path) is ".smrec" or ".json" or ".smstate")
                    files.Add("input-recordings/" + Path.GetFileName(path));

        // CreateNew prevents a diagnostic export from overwriting any existing artifact.
        using var output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write);
        using var zip = new ZipArchive(output, ZipArchiveMode.Create);
        foreach (string name in files)
            zip.CreateEntryFromFile(Path.Combine(root, name), name, CompressionLevel.Fastest);
        using var manifest = zip.CreateEntry("bundle.json").Open();
        JsonSerializer.Serialize(manifest, new
        {
            format = "SuperMetroid.Android.DiagnosticBundle.v1",
            createdUtc = DateTimeOffset.UtcNow,
            selectedSlot = slot,
            selectedStatePresent = files.Contains(state),
            privateDataWarning = "Debugger states and recording seeds may embed cartridge data. Keep this bundle private.",
            files,
        }, new JsonSerializerOptions { WriteIndented = true });
        return destination;
    }
}
