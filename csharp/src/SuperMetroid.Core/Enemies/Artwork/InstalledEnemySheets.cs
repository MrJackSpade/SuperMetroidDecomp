using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The installed pre-coloured enemy artwork (#1392, #1599): every variant of each enemy and of each owner's spawned
/// objects, by the variant's name, and the variants each retail population's enemies draw in, by the population's name. A
/// variant shows its enemy's frames in its own colours; the spawned-object variants hold each common picture once, on a
/// common sheet every variant drawing it reads (#1513).
/// </summary>
public sealed class InstalledEnemySheets : IEnemySheetSource
{
    /// <summary>Every enemy variant, by name.</summary>
    internal readonly Dictionary<string, EnemyArtworkVariant<SpriteSheet>> variants;

    /// <summary>Every spawned-object variant, by name.</summary>
    internal readonly Dictionary<string, EnemyArtworkVariant<SpawnedObjectSheet>> spawnedVariants;

    /// <summary>The artwork each retail population draws from, by the population's name.</summary>
    internal readonly Dictionary<string, PopulationArtwork> populations;

    /// <summary>
    /// The installed variants by name, the standard artwork and each population's artwork, each drawing only installed variants.
    /// </summary>
    /// <param name="variants">Every enemy variant, by name.</param>
    /// <param name="spawnedVariants">Every spawned-object variant, by name.</param>
    /// <param name="standard">The artwork a room that places no enemies draws from.</param>
    /// <param name="populations">The artwork each retail population draws from, by the population's name.</param>
    /// <exception cref="InvalidDataException">A variant is filed under another name, or an artwork draws a variant that is not installed.</exception>
    public InstalledEnemySheets(IReadOnlyDictionary<string, EnemyArtworkVariant<SpriteSheet>> variants,
        IReadOnlyDictionary<string, EnemyArtworkVariant<SpawnedObjectSheet>> spawnedVariants, PopulationArtwork standard,
        IReadOnlyDictionary<string, PopulationArtwork> populations)
    {
        this.variants = new Dictionary<string, EnemyArtworkVariant<SpriteSheet>>(Ensure.NotNull(variants), StringComparer.Ordinal);
        this.spawnedVariants = new Dictionary<string, EnemyArtworkVariant<SpawnedObjectSheet>>(Ensure.NotNull(spawnedVariants), StringComparer.Ordinal);
        Standard = Ensure.NotNull(standard);
        this.populations = new Dictionary<string, PopulationArtwork>(Ensure.NotNull(populations), StringComparer.Ordinal);
        RequireFiledByName(this.variants);
        RequireFiledByName(this.spawnedVariants);
        foreach ((string name, PopulationArtwork artwork) in this.populations.Prepend(new KeyValuePair<string, PopulationArtwork>("standard", Standard)))
        {
            RequireInstalled(name, artwork.enemies, this.variants);
            RequireInstalled(name, artwork.spawned, this.spawnedVariants);
        }
    }

    /// <summary>Checks that each of <paramref name="installed"/> is filed under its own name.</summary>
    /// <exception cref="InvalidDataException">A variant is filed under another name.</exception>
    private static void RequireFiledByName<TSheet>(Dictionary<string, EnemyArtworkVariant<TSheet>> installed)
        where TSheet : class
    {
        foreach ((string name, EnemyArtworkVariant<TSheet> variant) in installed)
        {
            if (name != variant.Name)
                throw new InvalidDataException($"Enemy artwork variant {variant.Name} is installed as {name}.");
        }
    }

    /// <summary>Checks that every one of <paramref name="drawn"/>, the variants population <paramref name="population"/> draws, is one of <paramref name="installed"/>.</summary>
    /// <exception cref="InvalidDataException">The population draws a variant that is not installed.</exception>
    private static void RequireInstalled<TKey, TSheet>(string population, Dictionary<TKey, EnemyArtworkVariant<TSheet>> drawn,
        Dictionary<string, EnemyArtworkVariant<TSheet>> installed)
        where TKey : notnull
        where TSheet : class
    {
        foreach (EnemyArtworkVariant<TSheet> variant in drawn.Values)
        {
            if (!installed.TryGetValue(variant.Name, out EnemyArtworkVariant<TSheet>? held) || !ReferenceEquals(held, variant))
                throw new InvalidDataException($"Population {population} draws enemy artwork variant {variant.Name}, which is not installed.");
        }
    }

    /// <inheritdoc/>
    public PopulationArtwork Standard { get; }

    /// <inheritdoc/>
    public PopulationArtwork For(RoomEnemyPopulationDefinition population) =>
        populations.TryGetValue(Ensure.NotNull(population).Name, out PopulationArtwork? artwork) ? artwork
            : throw new InvalidDataException($"No enemy artwork is installed for population {population.Name}.");
}
