using System.Runtime.CompilerServices;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Bosses.Artwork;
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
    private static readonly Lazy<RoomBlockSheetInputs> inputs = new(() => RoomBlockSheetInputs.Load(RepositoryInstallation.Installation.ContentDirectory));

    /// <summary>Sheets already composed, by the fixture's boss and then its artwork.</summary>
    private static readonly Dictionary<BossBodyOwner, ConditionalWeakTable<BossArtworkCatalog, BossBodyArtworkFixture>> composed =
        Enum.GetValues<BossBodyOwner>().ToDictionary(owner => owner, _ => new ConditionalWeakTable<BossArtworkCatalog, BossBodyArtworkFixture>());

    /// <summary>The installation's frames and its installed sheets, for a fixture whose boss draws no body layer.</summary>
    internal static BossBodyArtworkFixture Stock => new(inputs.Value.Bosses, RepositoryInstallation.Artwork.BossBodySheets
        ?? throw new InvalidOperationException("The repository installation has no boss body sheets."));

    /// <summary>The frames <paramref name="owner"/> draws its body from in <paramref name="bosses"/>, and the sheets composed from them.</summary>
    /// <exception cref="InvalidDataException"><paramref name="bosses"/> holds no frames for <paramref name="owner"/>, or they cannot be pictured.</exception>
    internal static BossBodyArtworkFixture For(BossArtworkCatalog bosses, BossBodyOwner owner) =>
        composed[owner].GetValue(bosses, catalog => Compose(catalog, owner));

    /// <summary>Composes every boss's sheet: <paramref name="owner"/>'s from <paramref name="bosses"/>' frames, the rest from the installation's.</summary>
    private static BossBodyArtworkFixture Compose(BossArtworkCatalog bosses, BossBodyOwner owner)
    {
        BossArtworkCatalog stock = inputs.Value.Bosses;
        BossArtworkCatalog frames = owner switch
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
        IReadOnlyList<ComposedBossBodySheet> sheets = BossBodySheetComposer.Compose(inputs.Value with { Bosses = frames });
        string directory = Path.Combine(Path.GetFullPath("csharp/test-temp"), "boss-bodies-" + Guid.NewGuid().ToString("N"));
        BossBodySheetFiles.Write(sheets, directory, SupportedCartridge.Sha256);
        return new BossBodyArtworkFixture(frames, BossBodySheetFiles.Load(directory, null));
    }
}
