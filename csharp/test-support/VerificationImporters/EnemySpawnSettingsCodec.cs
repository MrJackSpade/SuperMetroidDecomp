using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Boulder;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.Enemies.Ripper;
using SuperMetroid.Core.Enemies.Shutters;
using SuperMetroid.Core.Enemies.Yard;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The codec between a population entry's initialization word and the typed spawn settings its species reads (#1471).
/// The cartridge copies the word into the enemy's instruction-list field, and a few species read that field at
/// initialization as packed selectors before replacing it; every other species ignores the word. This type is the one
/// place that knows each species' packing, for the checks that compare the compiled populations and fixtures with native words.
/// </summary>
[EncodingOwner("population entry initialization word")]
internal static class EnemySpawnSettingsCodec
{
    /// <summary>The one GRipper selector bit its direction test ignores: any other set bit starts it moving right.</summary>
    private const int GRipperIgnoredSelectorBit = 1 << 8;

    /// <summary>A GRipper word with no speed that still starts it moving right: a set bit outside the speed and bit eight.</summary>
    private const int GRipperRightWithoutSpeed = 1 << 9;

    /// <summary>The bits of the word a crawler's surface is selected by, the low two.</summary>
    private const int CrawlerSurfaceBits = 3;

    /// <summary>The settings the word <paramref name="word"/> gives an <paramref name="enemy"/>.</summary>
    /// <exception cref="InvalidDataException">The word does not hold a setting the species can start with.</exception>
    public static EnemySpawnSettings Decode(EnemyDefinitionId enemy, int word) =>
        Decoders.TryGetValue(enemy, out Func<int, EnemySpawnSettings>? decode) ? decode(word) : EnemySpawnSettings.None;

    /// <summary>The species that read the word, each with its packing; every other species ignores the word.</summary>
    private static readonly Dictionary<EnemyDefinitionId, Func<int, EnemySpawnSettings>> Decoders = BuildDecoders();

    private static Dictionary<EnemyDefinitionId, Func<int, EnemySpawnSettings>> BuildDecoders()
    {
        Func<int, EnemySpawnSettings> crawler = DecodeCrawler;
        Func<int, EnemySpawnSettings> shutter = DecodeShutter;
        // Viola selects its surface with its property bits and ignores the word.
        return new()
        {
            [EnemyDefinitionId.Sciser] = crawler,
            [EnemyDefinitionId.Zero] = crawler,
            [EnemyDefinitionId.Zeela] = crawler,
            [EnemyDefinitionId.Sova] = crawler,
            [EnemyDefinitionId.Zoomer] = crawler,
            [EnemyDefinitionId.MZoomer] = crawler,
            [EnemyDefinitionId.HZoomer] = crawler,
            [EnemyDefinitionId.Yard] = word => new YardSpawnSettings(word),
            // Boulder reads only the low byte as its trigger height.
            [EnemyDefinitionId.Boulder] = word => new BoulderSpawnSettings(PopulationParameterSettings.Of(word).First),
            [EnemyDefinitionId.GRipper] = DecodeGRipper,
            [EnemyDefinitionId.ShutterGrowing] = DecodeGrowingShutter,
            [EnemyDefinitionId.ShutterShootable] = shutter,
            [EnemyDefinitionId.ShutterDestroyable] = shutter,
            [EnemyDefinitionId.Kamer] = shutter,
            [EnemyDefinitionId.ShutterHorizShootable] = shutter,
        };
    }

    private static CrawlerSpawnSettings DecodeCrawler(int word) =>
        new CrawlerSpawnSettings(CrawlerAnimationDefinitions.Orientations.At(word & CrawlerSurfaceBits));

    // Only the low byte reaches the speed-record multiplication; the direction test ignores bit eight, so every word
    // with a nonzero speed starts the GRipper moving right, even with bit eight set.
    private static GRipperSpawnSettings DecodeGRipper(int word) =>
        new GRipperSpawnSettings(PopulationParameterSettings.Of(word).First,
            (word & ~GRipperIgnoredSelectorBit) != 0 ? GRipperStartDirection.Right : GRipperStartDirection.Left);

    private static GrowingShutterSpawnSettings DecodeGrowingShutter(int word) => word switch
    {
        0 => new GrowingShutterSpawnSettings(GrowingShutterWait.Timer),
        1 => new GrowingShutterSpawnSettings(GrowingShutterWait.SamusNear),
        _ => throw new InvalidDataException($"Growing shutter initialization word ${word:X4} selects no wait."),
    };

    private static ShutterSpawnSettings DecodeShutter(int word)
    {
        PopulationParameterSettings bytes = PopulationParameterSettings.Of(word);
        return new ShutterSpawnSettings(bytes.First, bytes.Second);
    }

    /// <summary>
    /// A word that decodes to <paramref name="settings"/> for its species, for fixtures that author population entries
    /// as cartridge words. A species told nothing gets zero, and a GRipper's direction its lowest selecting word.
    /// </summary>
    /// <exception cref="ArgumentException">The settings are of a type this codec does not know.</exception>
    public static int Encode(EnemySpawnSettings settings) => Ensure.NotNull(settings) switch
    {
        CrawlerSpawnSettings crawler => CrawlerAnimationDefinitions.Orientations.PositionOf(crawler.Orientation),
        YardSpawnSettings yard => yard.Direction,
        BoulderSpawnSettings boulder => boulder.TriggerHeight,
        GRipperSpawnSettings ripper => (ripper.Direction == GRipperStartDirection.Right) == (ripper.SpeedIndex != 0)
            ? ripper.SpeedIndex
            : ripper.Direction == GRipperStartDirection.Right ? GRipperRightWithoutSpeed
            : throw new ArgumentException($"No initialization word gives a GRipper speed {ripper.SpeedIndex} moving left.", nameof(settings)),
        GrowingShutterSpawnSettings growing => growing.Wait == GrowingShutterWait.SamusNear ? 1 : 0,
        ShutterSpawnSettings shutter => LittleEndianBytes.Word(checked((byte)shutter.SpeedIndex), checked((byte)shutter.PrimaryDirection)),
        _ when settings == EnemySpawnSettings.None => 0,
        _ => throw new ArgumentException($"Spawn settings {settings.GetType().Name} have no initialization word.", nameof(settings)),
    };
}
