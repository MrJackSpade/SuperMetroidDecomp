using System.Text.Json;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses;

/// <summary>
/// Holds every installed boss body sheet to the whole-picture rule (#1513): each picture on a boss's sheet is the one
/// whole picture of a frame the boss shows, and no frame is shown as its picture with pieces of other pictures laid over
/// it. Crocomire's melt once installed a picture of each band of characters at each dissolve step over every frame that
/// reads them, scattered fragments the game never shows on their own; his melt shows each state of his melt image whole.
/// The rule reads the installed manifest, which names each frame's picture and any pieces laid over it.
/// </summary>
internal static class BossBodyWholePicturesVerification
{
    /// <summary>Fails, naming every boss body picture that is no frame's whole picture and every frame shown in pieces.</summary>
    internal static void VerifyBossBodyWholePictures()
    {
        string manifestPath = Path.Combine(RepositoryInstallation.Installation.BossBodySheetDirectory, BossBodySheetFiles.ManifestFileName);
        using JsonDocument manifest = JsonDocument.Parse(File.ReadAllBytes(manifestPath));
        JsonElement sheets = manifest.RootElement.GetProperty("sheets");
        var findings = new List<string>();
        int pictures = 0;
        foreach (BossBodyOwner owner in Enum.GetValues<BossBodyOwner>())
        {
            var whole = new HashSet<string>(StringComparer.Ordinal);
            foreach (JsonProperty frame in sheets.GetProperty(owner.ToString()).GetProperty("frames").EnumerateObject())
            {
                if (frame.Value.TryGetProperty("picture", out JsonElement picture) && picture.ValueKind == JsonValueKind.String)
                    whole.Add(picture.GetString() ?? throw new InvalidDataException($"{manifestPath} frame {frame.Name} names a picture with no name."));
                if (frame.Value.TryGetProperty("overlays", out JsonElement overlays) && overlays.ValueKind == JsonValueKind.Array
                    && overlays.GetArrayLength() > 0)
                {
                    findings.Add($"{owner} body frame {frame.Name} is shown with {overlays.GetArrayLength()} sets of pieces laid over its picture");
                }
            }
            SpriteSheet sheet = InstalledFixtures.BossBodySheets.Of(owner).Sheet;
            string[] pieces = [.. sheet.frames.Keys.Where(name => !whole.Contains(name)).Order(StringComparer.Ordinal)];
            pictures += sheet.frames.Count;
            if (pieces.Length > 0)
                findings.Add($"{owner} body sheet holds {pieces.Length} pictures that are no frame's whole picture, such as {string.Join(", ", pieces.Take(5))}");
        }
        AssertTrue(findings.Count == 0,
            $"Boss body sheets hold pieces of pictures:{Environment.NewLine}{string.Join(Environment.NewLine, findings)}");
        // Each melt image is one whole frame, with the cells its dissolve erases; no step of the dissolve is pictured (#1652).
        BossBodySheet crocomire = InstalledFixtures.BossBodySheets.Of(BossBodyOwner.Crocomire);
        foreach (CrocomireMeltingIllustration image in Enum.GetValues<CrocomireMeltingIllustration>())
        {
            BossBodyFrame melt = crocomire.Frame(CrocomireMeltingFrameNames.Of(image));
            AssertTrue(melt.Picture is not null && !melt.Dissolving.IsEmpty, $"Crocomire's {image} melt image is pictured once, with the cells its dissolve erases");
            AssertThrows<InvalidDataException>(() => crocomire.Frame($"{CrocomireMeltingFrameNames.Of(image)}.1"),
                $"Crocomire's body sheet pictures no step of his {image} melt image's dissolve");
        }
        Console.WriteLine($"  Boss body pictures: {pictures} pictures are each a frame's whole picture.");
    }

    /// <summary>Runs the boss body sheets' whole-picture rule.</summary>
    [VerificationCommand("--boss-body-whole-pictures")]
    internal static int BossBodyWholePicturesCommand()
    {
        Suite(nameof(VerifyBossBodyWholePictures), VerifyBossBodyWholePictures);
        return 0;
    }
}
