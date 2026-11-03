using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// The shades the import pre-colours the bosses' sheets in (#1512), from the cartridge's colour tables for each boss's
/// colour effects: their body sheets on layer 2, their sprite sheets over the row each draws in, and the sheets of the
/// objects they spawn. Gameplay names each shade by the boss's typed colour state; where each lies among the inks is the
/// import's.
/// </summary>
internal static class BossSheetShades
{
    /// <summary>The first ink of a body picture's sixth tile colour group, which Kraid's body colours recolour.</summary>
    private const int KraidBodyFirstInk = 112;

    /// <summary>The sprite row Kraid's rocks draw in, which his initialization loads.</summary>
    private const int KraidRocksRow = 3;

    /// <summary>The hurt flash's band, before the health bands in Kraid's health colours.</summary>
    private const int KraidHurtBand = -1;

    /// <summary>Kraid's sprite parts, which draw in his row and show his health band.</summary>
    private static readonly HashSet<EnemyDefinitionId> KraidParts =
    [
        EnemyDefinitionId.Kraid, EnemyDefinitionId.KraidArm, EnemyDefinitionId.KraidLintTop, EnemyDefinitionId.KraidLintMiddle,
        EnemyDefinitionId.KraidLintBottom, EnemyDefinitionId.KraidFoot, EnemyDefinitionId.KraidNail, EnemyDefinitionId.KraidNailBad,
    ];

    /// <summary>
    /// The shades of <paramref name="owner"/>'s body sheet, painted from <paramref name="standard"/> with the inks
    /// <paramref name="paletteIndices"/> name.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// The boss's colours are not installed, or a colour gameplay substitutes is not unique on the body's picture.
    /// </exception>
    internal static IReadOnlyList<SheetShade> Body(BossBodyOwner owner, BossArtworkCatalog bosses, ReadOnlySpan<byte> paletteIndices,
        ReadOnlySpan<RgbColor> standard)
    {
        Ensure.NotNull(bosses);
        if (owner != BossBodyOwner.Kraid)
            return [];
        KraidColorCatalog kraid = bosses.KraidColors ?? throw new InvalidDataException("Kraid's body is shaded from the installed Kraid colours.");
        var shades = new List<SheetShade>();
        HashSet<int> used = [.. paletteIndices.ToArray().Select(index => (int)index)];
        for (int band = KraidHurtBand; band < KraidHealthThresholdDefinitions.EighthCount; band++)
        {
            RgbColor[] colours = Band(kraid, KraidPaletteSource.Health, band);
            shades.Add(SheetShade.Run(KraidShades.Health(band), KraidBodyFirstInk, colours));
            RequireUniqueEyes(standard, used, colours, band);
        }
        shades.Add(SheetShade.Run(KraidShades.DeathArm, KraidBodyFirstInk,
            [.. Enumerable.Range(0, KraidPalettes.BandColors).Select(colour => kraid.Resolve(KraidPaletteSource.DeathArm, colour))]));
        return shades;
    }

    /// <summary>The shades of <paramref name="enemy"/>'s sprite sheet over the row from <paramref name="firstInk"/> it draws in.</summary>
    /// <exception cref="InvalidDataException">The boss's colours are not installed.</exception>
    internal static IReadOnlyList<SheetShade> OwnRow(BossArtworkCatalog bosses, EnemyDefinitionId enemy, int firstInk)
    {
        Ensure.NotNull(bosses);
        if (!KraidParts.Contains(enemy))
            return [];
        KraidColorCatalog kraid = bosses.KraidColors ?? throw new InvalidDataException("Kraid's parts are shaded from the installed Kraid colours.");
        return [.. Enumerable.Range(KraidHurtBand, KraidHealthThresholdDefinitions.EighthCount - KraidHurtBand)
            .Select(band => SheetShade.Run(KraidShades.Health(band), firstInk, Band(kraid, KraidPaletteSource.Secondary, band)))];
    }

    /// <summary>The shades every character-zero spawned-object sheet takes for the objects bosses spawn: Kraid's rocks.</summary>
    /// <exception cref="InvalidDataException">The boss's colours are not installed.</exception>
    internal static IReadOnlyList<SheetShade> SpawnedObjects(BossArtworkCatalog bosses)
    {
        Ensure.NotNull(bosses);
        KraidColorCatalog kraid = bosses.KraidColors ?? throw new InvalidDataException("Kraid's rocks are shaded from the installed Kraid colours.");
        return [SheetShade.Run(KraidShades.Rocks, KraidRocksRow * LoadedSetColours.RowColours,
            [.. Enumerable.Range(0, KraidPalettes.BandColors).Select(colour => kraid.Resolve(KraidPaletteSource.InitialTarget, colour))])];
    }

    /// <summary>Health band <paramref name="band"/>'s sixteen colours of <paramref name="source"/>, minus one for the hurt flash's.</summary>
    private static RgbColor[] Band(KraidColorCatalog kraid, KraidPaletteSource source, int band) =>
        [.. Enumerable.Range(0, KraidPalettes.BandColors).Select(colour => kraid.Resolve(source, KraidPalettes.BandColors * (band + 1) + colour))];

    /// <summary>
    /// Requires Kraid's eye colours under <paramref name="band"/>'s <paramref name="colours"/> to differ from each other and
    /// from every other colour his body's picture shows, so gameplay's substitution of his glowing eyes recolours only them.
    /// </summary>
    /// <exception cref="InvalidDataException">An eye colour repeats on the picture.</exception>
    private static void RequireUniqueEyes(ReadOnlySpan<RgbColor> standard, HashSet<int> used, RgbColor[] colours, int band)
    {
        var eyeInks = Enumerable.Range(KraidBodyFirstInk + KraidPalettes.FirstEyeColour, KraidEyeGlow.Eyes).ToHashSet();
        RgbColor[] shown = standard.ToArray();
        colours.CopyTo(shown, KraidBodyFirstInk);
        RgbColor[] eyes = [.. eyeInks.Select(ink => shown[ink])];
        if (eyes.Distinct().Count() != eyes.Length)
            throw new InvalidDataException($"Kraid's band {band} gives two eyes one colour, which his glow cannot tell apart.");
        foreach (int ink in used)
        {
            if (ink % LoadedSetColours.RowColours != 0 && !eyeInks.Contains(ink) && eyes.Contains(shown[ink]))
                throw new InvalidDataException($"Kraid's band {band} shows an eye colour at ink {ink} of his body, which his glow would recolour.");
        }
    }
}
