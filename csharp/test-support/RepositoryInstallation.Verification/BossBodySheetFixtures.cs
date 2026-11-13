using SuperMetroid.AssetExtraction.Artwork;
using System.Runtime.CompilerServices;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// Verification access (#1466): the boss body sheets a fixture's boss draws from. A fixture's boss gets a sheet composed
/// from the fixture's own frames for it, as an installation of those frames would hold, over the installation's
/// characters and palettes; every other boss's sheet is composed from the installation's frames. Composing fails, as an
/// installation would, when the fixture holds no frames for its boss.
/// </summary>
internal static class BossBodySheetFixtures
{
    /// <summary>The installation's stock inputs the sheets compose over.</summary>
    private static readonly Lazy<RoomBlockSheetInputs> inputs = new(() => SharedFixtureTiming.Load("Fixture: boss body sheet inputs",
        () => RoomBlockSheetInputs.Load(CartridgeImportContent.Directory)));

    /// <summary>Sheets already composed, by the fixture's boss and then its artwork.</summary>
    private static readonly Dictionary<BossBodyOwner, ConditionalWeakTable<BossSourceArtwork, BossBodyArtworkFixture>> composed =
        Enum.GetValues<BossBodyOwner>().ToDictionary(owner => owner, _ => new ConditionalWeakTable<BossSourceArtwork, BossBodyArtworkFixture>());

    /// <summary>The installation's frames and its installed sheets, for a fixture whose boss draws no body layer.</summary>
    internal static BossBodyArtworkFixture Stock => new(inputs.Value.Bosses, InstalledFixtures.BossBodySheets);

    /// <summary>The frames <paramref name="owner"/> draws its body from in <paramref name="bosses"/>, and the sheets composed from them.</summary>
    /// <exception cref="InvalidDataException"><paramref name="bosses"/> holds no frames for <paramref name="owner"/>, or they cannot be pictured.</exception>
    internal static BossBodyArtworkFixture For(BossSourceArtwork bosses, BossBodyOwner owner) =>
        composed[owner].GetValue(bosses, artwork => Compose(artwork, owner));

    /// <summary>Composes every boss's sheet: <paramref name="owner"/>'s from <paramref name="bosses"/>' frames, the rest from the installation's.</summary>
    private static BossBodyArtworkFixture Compose(BossSourceArtwork bosses, BossBodyOwner owner)
    {
        BossSourceArtwork stock = inputs.Value.Bosses;
        BossSourceArtwork frames = owner switch
        {
            BossBodyOwner.Kraid => stock with { KraidBackground = bosses.KraidBackground },
            BossBodyOwner.Phantoon => stock with { PhantoonBg2Frames = bosses.PhantoonBg2Frames },
            BossBodyOwner.Draygon => stock with { DraygonBg2Frames = bosses.DraygonBg2Frames },
            // Crocomire's melt is pictured from its own installed artwork, which a fixture of his body frames need not carry.
            BossBodyOwner.Crocomire => stock with
            {
                CrocomireBg2Frames = bosses.CrocomireBg2Frames,
                CrocomireMelting = bosses.CrocomireMelting ?? stock.CrocomireMelting,
            },
            BossBodyOwner.MotherBrain => stock with { MotherBrainBodyBg2Frames = bosses.MotherBrainBodyBg2Frames },
            _ => throw new InvalidOperationException($"Undefined boss body owner {owner}."),
        };
        // Only this boss's frames differ from the installation's, so only its sheet is composed; the rest are installed.
        ComposedBossBodySheet sheet = BossBodySheetComposer.Compose(inputs.Value with { Bosses = frames }, owner);
        // The sheet is installed as an installation holds it (#1635): its manifests in the family's directory and its
        // pictures in a picture store beside it, which the loader assembles the sheet's image from.
        string content = Path.Combine(Path.GetFullPath("csharp/test-temp"), "boss-bodies-" + Guid.NewGuid().ToString("N"));
        string directory = Path.Combine(content, GameInstallationLayout.BossBodySheetDirectoryName);
        InstalledSheetFamily family = BossBodySheetFiles.Write([sheet], directory);
        PictureStoreWriter.Write(content, [family]);
        BossBodySheet own = BossBodySheetFiles.LoadEach(directory).Single();
        return new BossBodyArtworkFixture(frames, new BossBodySheets(Enum.GetValues<BossBodyOwner>()
            .Select(boss => boss == owner ? own : InstalledFixtures.BossBodySheets.Of(boss))));
    }
}
