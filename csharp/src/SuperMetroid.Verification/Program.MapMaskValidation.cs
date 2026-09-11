using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;

internal static partial class Program
{
    private static void VerifyBundledMapMaskValidation(string directory)
    {
        string maskPath = Path.Combine(directory, AreaMapCatalogFormat.StationRevealFile);
        string manifestPath = Path.Combine(directory, AreaMapCatalogFormat.ManifestFile);
        byte[] originalMask = File.ReadAllBytes(maskPath);
        byte[] originalManifest = File.ReadAllBytes(manifestPath);
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        try
        {
            File.Delete(maskPath);
            AssertThrows<FileNotFoundException>(() => AreaMapPresentationCatalog.Load(directory, null), "missing bundled mask fails loudly");
            File.WriteAllText(maskPath, "corrupt");
            AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(directory, null), "corrupt mask fails hash check");

            void CheckInvalid(Action<Dictionary<string, int[]>> change, string context)
            {
                var masks = JsonSerializer.Deserialize<Dictionary<string, int[]>>(originalMask)!;
                change(masks);
                byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(masks);
                File.WriteAllBytes(maskPath, bytes);
                var manifest = JsonSerializer.Deserialize<AreaMapCatalogManifest>(originalManifest, options)!;
                manifest.Sha256[AreaMapCatalogFormat.StationRevealFile] = Convert.ToHexString(SHA256.HashData(bytes));
                File.WriteAllBytes(manifestPath, JsonSerializer.SerializeToUtf8Bytes(manifest, options));
                AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(directory, null), context);
            }

            CheckInvalid(masks => masks["Crateria"] = [0, 0], "duplicate authored reveal cells rejected even with valid hash");
            CheckInvalid(masks => masks["Crateria"] = [2048], "out-of-range authored reveal cells rejected");
            CheckInvalid(masks => masks.Remove("Crateria"), "missing area mask rejected");
            CheckInvalid(masks => masks["Crateria"] = null!, "null area mask rejected");
        }
        finally
        {
            File.WriteAllBytes(maskPath, originalMask);
            File.WriteAllBytes(manifestPath, originalManifest);
        }
        _ = AreaMapPresentationCatalog.Load(directory, null);
        Console.WriteLine("Bundled map masks: missing/corrupt/duplicate/out-of-range/incomplete data fail loudly.");
    }
}
