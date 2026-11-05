using SuperMetroid.Tooling;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Frontend.Map;

namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>Checks that bundled station-reveal masks reject missing, corrupt, and structurally invalid authored map data.</summary>
internal static class MapMaskValidationVerification
{
    /// <summary>Temporarily mutates the bundled mask, confirms each invalid case fails loading, then restores it and reloads.</summary>
    internal static void VerifyBundledMapMaskValidation(string directory)
    {
        string maskPath = Path.Combine(directory, AreaMapCatalogFormat.StationRevealFile);
        byte[] originalMask = File.ReadAllBytes(maskPath);
        try
        {
            File.Delete(maskPath);
            AssertThrows<FileNotFoundException>(() => MapPresentationFiles.Load(directory), "missing bundled mask fails loudly");
            File.WriteAllText(maskPath, "corrupt");
            AssertThrows<InvalidDataException>(() => MapPresentationFiles.Load(directory), "corrupt mask fails where it is decoded");

            void CheckInvalid(Action<Dictionary<string, int[]?>> change, string context)
            {
                var masks = RequiredJson.Deserialize<Dictionary<string, int[]?>>(originalMask);
                change(masks);
                byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(masks);
                File.WriteAllBytes(maskPath, bytes);
                AssertThrows<InvalidDataException>(() => MapPresentationFiles.Load(directory), context);
            }

            CheckInvalid(masks => masks["Crateria"] = [0, 0], "duplicate authored reveal cells rejected");
            CheckInvalid(masks => masks["Crateria"] = [2048], "out-of-range authored reveal cells rejected");
            CheckInvalid(masks => masks.Remove("Crateria"), "missing area mask rejected");
            CheckInvalid(masks => masks["Crateria"] = null, "null area mask rejected");
        }
        finally
        {
            File.WriteAllBytes(maskPath, originalMask);
        }
        _ = MapPresentationFiles.Load(directory);
        Console.WriteLine("Bundled map masks: missing/corrupt/duplicate/out-of-range/incomplete data fail loudly.");
    }
}
