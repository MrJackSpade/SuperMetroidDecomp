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
    public static EnemySpawnSettings Decode(EnemyDefinitionId enemy, int word)
    {
        PopulationParameterSettings bytes = PopulationParameterSettings.Of(word);
        return enemy switch
        {
            // Viola selects its surface with its property bits and ignores the word.
            EnemyDefinitionId.Sciser or EnemyDefinitionId.Zero or EnemyDefinitionId.Zeela or EnemyDefinitionId.Sova or
                EnemyDefinitionId.Zoomer or EnemyDefinitionId.MZoomer or EnemyDefinitionId.HZoomer =>
                new CrawlerSpawnSettings(CrawlerAnimationDefinitions.Orientations.At(word & CrawlerSurfaceBits)),
            EnemyDefinitionId.Yard => new YardSpawnSettings(word),
            // Boulder reads only the low byte as its trigger height.
            EnemyDefinitionId.Boulder => new BoulderSpawnSettings(bytes.First),
            // Only the low byte reaches the speed-record multiplication; the direction test ignores bit eight, so
            // every word with a nonzero speed starts the GRipper moving right, even with bit eight set.
            EnemyDefinitionId.GRipper => new GRipperSpawnSettings(bytes.First, (word & ~GRipperIgnoredSelectorBit) != 0),
            EnemyDefinitionId.ShutterGrowing => word switch
            {
                0 => new GrowingShutterSpawnSettings(WaitsForSamus: false),
                1 => new GrowingShutterSpawnSettings(WaitsForSamus: true),
                _ => throw new InvalidDataException($"Growing shutter initialization word ${word:X4} selects no wait."),
            },
            EnemyDefinitionId.ShutterShootable or EnemyDefinitionId.ShutterDestroyable or EnemyDefinitionId.Kamer or
                EnemyDefinitionId.ShutterHorizShootable => new ShutterSpawnSettings(bytes.First, bytes.Second),
            _ => EnemySpawnSettings.None,
        };
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
        GRipperSpawnSettings ripper => ripper.MovesRight == (ripper.SpeedIndex != 0) ? ripper.SpeedIndex
            : ripper.MovesRight ? GRipperRightWithoutSpeed
            : throw new ArgumentException($"No initialization word gives a GRipper speed {ripper.SpeedIndex} moving left.", nameof(settings)),
        GrowingShutterSpawnSettings growing => growing.WaitsForSamus ? 1 : 0,
        ShutterSpawnSettings shutter => LittleEndianBytes.Word(checked((byte)shutter.SpeedIndex), checked((byte)shutter.PrimaryDirection)),
        _ when settings == EnemySpawnSettings.None => 0,
        _ => throw new ArgumentException($"Spawn settings {settings.GetType().Name} have no initialization word.", nameof(settings)),
    };
}
