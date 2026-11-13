using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Botwoon;
using CrocomireBand = SuperMetroid.AssetExtraction.Bosses.Crocomire.Artwork.CrocomireColorCatalog.Band;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.AssetExtraction.Bosses.Botwoon.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Draygon;
using SuperMetroid.AssetExtraction.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.Botwoon.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Crocomire.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Kraid;
using SuperMetroid.AssetExtraction.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Compatibility;

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

    /// <summary>The first ink of a room or body picture's fourth tile colour group, which Draygon's background colours recolour.</summary>
    private const int DraygonBackgroundFirstInk = 80;

    /// <summary>The colour of the fourth tile colour group his health bands start at.</summary>
    private const int DraygonHealthFirstColour = 9;

    /// <summary>The sprite row his opening's colours start at; they run on into the next row.</summary>
    private const int DraygonIntroRow = 1;

    /// <summary>The sprite row his body and parts draw in.</summary>
    private const int DraygonSpriteRow = 7;

    /// <summary>Draygon's sprites, which draw in his sprite row and show his opening's colours.</summary>
    private static readonly HashSet<EnemyDefinitionId> DraygonParts =
        [EnemyDefinitionId.DraygonBody, EnemyDefinitionId.DraygonEye, EnemyDefinitionId.DraygonTail, EnemyDefinitionId.DraygonArms];

    /// <summary>The first ink of a body picture's sixth tile colour group, which Crocomire's fight colours recolour.</summary>
    private const int CrocomireBodyFirstInk = 112;

    /// <summary>The sprite row Crocomire's wall and the bridge's fragments draw in.</summary>
    private const int CrocomireWallRow = 2;

    /// <summary>The sprite row of the enemy projectiles, which Crocomire's fight loads his projectiles' colours into.</summary>
    private const int CrocomireProjectileRow = 5;

    /// <summary>The sprite row his skeleton's arm colours fill.</summary>
    private const int CrocomireSkeletonArmRow = 1;

    /// <summary>The sprite row the wall spikes' colours fill.</summary>
    private const int CrocomireWallSpikesRow = 3;

    /// <summary>The sprite row Botwoon's head and body draw in, which his health bands recolour.</summary>
    private const int BotwoonRow = 7;


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
    internal static IReadOnlyList<SheetShade> Body(BossBodyOwner owner, BossSourceArtwork bosses, ReadOnlySpan<byte> paletteIndices,
        ReadOnlySpan<RgbColor> standard)
    {
        Ensure.NotNull(bosses);
        if (owner == BossBodyOwner.Draygon)
            return DraygonBackground(bosses);
        if (owner == BossBodyOwner.Phantoon)
            return PhantoonSheetShades.Body(bosses);
        if (owner == BossBodyOwner.Crocomire)
        {
            CrocomireColorCatalog crocomire = bosses.CrocomireColors ?? throw new InvalidDataException("Crocomire's body is shaded from the installed Crocomire colours.");
            RgbColor[] fight = Crocomire(crocomire, CrocomireBand.FightBody);
            return [SheetShade.Run(CrocomireShades.FightBody, CrocomireBodyFirstInk, fight),
                SheetShade.Run(CrocomireShades.BodyHurt, CrocomireBodyFirstInk, [.. fight.Select(_ => RgbColor.White)])];
        }
        if (owner != BossBodyOwner.Kraid)
            return [];
        KraidColorCatalog kraid = bosses.KraidColors ?? throw new InvalidDataException("Kraid's body is shaded from the installed Kraid colours.");
        var shades = new List<SheetShade>();
        HashSet<int> used = [.. paletteIndices.ToArray().Select(index => (int)index)];
        // Every band's shade shows his eyes in the colours gameplay's eye glow replaces, whatever the band gives them (#1512).
        ReadOnlySpan<RgbColor> shadedEyes = kraid.Eyes().Shaded;
        foreach (KraidColourBand band in KraidColourBand.All)
        {
            RgbColor[] colours = Band(kraid, KraidPaletteSource.Health, band);
            shadedEyes.CopyTo(colours.AsSpan(KraidPalettes.FirstEyeColour, KraidEyeGlow.Eyes));
            shades.Add(SheetShade.Run(KraidShades.Health(band), KraidBodyFirstInk, colours));
            RequireUniqueEyes(standard, used, colours, band);
        }
        shades.Add(SheetShade.Run(KraidShades.DeathArm, KraidBodyFirstInk,
            [.. Enumerable.Range(0, KraidPalettes.BandColors).Select(colour => kraid.Resolve(KraidPaletteSource.DeathArm, colour))]));
        return shades;
    }

    /// <summary>The shades of <paramref name="enemy"/>'s sprite sheet over the row from <paramref name="firstInk"/> it draws in.</summary>
    /// <exception cref="InvalidDataException">The boss's colours are not installed.</exception>
    internal static IReadOnlyList<SheetShade> OwnRow(BossSourceArtwork bosses, EnemyDefinitionId enemy, int firstInk)
    {
        Ensure.NotNull(bosses);
        if (enemy == EnemyDefinitionId.Botwoon)
            return [.. Botwoon(bosses, firstInk)];
        if (DraygonParts.Contains(enemy))
            return [.. DraygonSprites(bosses)];
        if (enemy == EnemyDefinitionId.SporeSpawn)
            return SporeSpawnSheetShades.Sprites(bosses);
        if (enemy is EnemyDefinitionId.Crocomire or EnemyDefinitionId.CrocomireTongue)
        {
            CrocomireColorCatalog crocomire = bosses.CrocomireColors ?? throw new InvalidDataException("Crocomire is shaded from the installed Crocomire colours.");
            return [SheetShade.Run(CrocomireShades.SkeletonArm, CrocomireSkeletonArmRow * LoadedSetColours.RowColours, Crocomire(crocomire, CrocomireBand.SkeletonArm)),
                SheetShade.Run(CrocomireShades.WallSpikes, CrocomireWallSpikesRow * LoadedSetColours.RowColours, Crocomire(crocomire, CrocomireBand.WallSpikes))];
        }
        if (!KraidParts.Contains(enemy))
            return [];
        KraidColorCatalog kraid = bosses.KraidColors ?? throw new InvalidDataException("Kraid's parts are shaded from the installed Kraid colours.");
        return [.. KraidColourBand.All
            .Select(band => SheetShade.Run(KraidShades.Health(band), firstInk, Band(kraid, KraidPaletteSource.Secondary, band)))];
    }

    /// <summary>
    /// The shades the objects <paramref name="enemy"/> spawns show, wherever they are drawn (#1513): Kraid's rocks,
    /// Botwoon's, Draygon's and Spore Spawn's sprite colours, and Crocomire's wall; none for any other enemy.
    /// </summary>
    /// <exception cref="InvalidDataException">The boss's colours are not installed.</exception>
    internal static IReadOnlyList<SheetShade> SpawnedObjects(BossSourceArtwork bosses, EnemyDefinitionId enemy)
    {
        Ensure.NotNull(bosses);
        if (KraidParts.Contains(enemy))
        {
            KraidColorCatalog kraid = bosses.KraidColors ?? throw new InvalidDataException("Kraid's rocks are shaded from the installed Kraid colours.");
            return [SheetShade.Run(KraidShades.Rocks, KraidRocksRow * LoadedSetColours.RowColours,
                [.. Enumerable.Range(0, KraidPalettes.BandColors).Select(colour => kraid.Resolve(KraidPaletteSource.InitialTarget, colour))])];
        }
        if (enemy == EnemyDefinitionId.Botwoon)
            return [.. Botwoon(bosses, BotwoonRow * LoadedSetColours.RowColours)];
        if (DraygonParts.Contains(enemy))
            return [.. DraygonSprites(bosses)];
        if (enemy == EnemyDefinitionId.SporeSpawn)
            return SporeSpawnSheetShades.Sprites(bosses);
        if (enemy is EnemyDefinitionId.Crocomire or EnemyDefinitionId.CrocomireTongue)
        {
            return [SheetShade.Run(CrocomireShades.Wall, CrocomireWallRow * LoadedSetColours.RowColours,
                Crocomire(Required(bosses.CrocomireColors, "Crocomire"), CrocomireBand.InitialWall)[..LoadedSetColours.RowColours])];
        }
        return [];
    }

    /// <summary>
    /// The row shades the objects <paramref name="enemy"/> spawns in a row its fight loads show (#1513): Crocomire's
    /// projectiles, in the projectiles' row with his colours in it over <paramref name="colours"/>; none for any other enemy.
    /// </summary>
    /// <exception cref="InvalidDataException">The boss's colours are not installed.</exception>
    internal static IReadOnlyList<SheetRowShade> SpawnedRows(BossSourceArtwork bosses, LoadedSetColours colours, EnemyDefinitionId enemy)
    {
        Ensure.NotNull(bosses);
        Ensure.NotNull(colours);
        if (enemy is not (EnemyDefinitionId.Crocomire or EnemyDefinitionId.CrocomireTongue))
            return [];
        RgbColor[] projectiles = Crocomire(Required(bosses.CrocomireColors, "Crocomire"), CrocomireBand.InitialProjectile);
        return [new(CrocomireShades.Projectiles, CrocomireProjectileRow,
            colours.With(CrocomireProjectileRow, 0, projectiles[..LoadedSetColours.RowColours]))];
    }

    /// <summary>
    /// The shades of Draygon's body layer: his material
    /// colours, their white flash and his eight health bands.
    /// </summary>
    /// <exception cref="InvalidDataException">His colours are not installed.</exception>
    internal static IReadOnlyList<SheetShade> DraygonBackground(BossSourceArtwork bosses)
    {
        DraygonColorCatalog draygon = Required(bosses.DraygonColors, "Draygon");
        var shades = new List<SheetShade>
        {
            SheetShade.Run(DraygonShades.Background, DraygonBackgroundFirstInk,
                [.. Enumerable.Range(0, DraygonColorDefinitions.BackgroundCount).Select(draygon.ResolveBackground)]),
            SheetShade.Run(DraygonShades.BackgroundWhite, DraygonBackgroundFirstInk,
                [.. Enumerable.Range(0, DraygonColorDefinitions.WhiteFlashCount).Select(draygon.ResolveWhiteFlash)]),
        };
        for (int band = 0; band < DraygonColorDefinitions.HealthBandCount; band++)
        {
            int shown = band;
            shades.Add(SheetShade.Run(DraygonShades.Health(shown), DraygonBackgroundFirstInk + DraygonHealthFirstColour,
                [.. Enumerable.Range(0, DraygonColorDefinitions.HealthBandColorCount).Select(colour => draygon.ResolveHealthBand(shown, colour))]));
        }
        return shades;
    }

    /// <summary>The shades of Draygon's sprites: his opening's colours and his sprite row's material colours and white flash.</summary>
    /// <exception cref="InvalidDataException">His colours are not installed.</exception>
    private static IEnumerable<SheetShade> DraygonSprites(BossSourceArtwork bosses)
    {
        DraygonColorCatalog draygon = Required(bosses.DraygonColors, "Draygon");
        yield return SheetShade.Run(DraygonShades.Intro, DraygonIntroRow * LoadedSetColours.RowColours,
            [.. Enumerable.Range(0, DraygonColorDefinitions.IntroCount).Select(draygon.ResolveIntro)]);
        yield return SheetShade.Run(DraygonShades.Sprite, DraygonSpriteRow * LoadedSetColours.RowColours,
            [.. Enumerable.Range(0, DraygonColorDefinitions.SpriteCount).Select(draygon.ResolveSprite)]);
        yield return SheetShade.Run(DraygonShades.SpriteWhite, DraygonSpriteRow * LoadedSetColours.RowColours,
            [.. Enumerable.Range(0, DraygonColorDefinitions.WhiteFlashCount).Select(draygon.ResolveWhiteFlash)]);
    }

    /// <summary>Every colour of Crocomire's <paramref name="band"/>.</summary>
    private static RgbColor[] Crocomire(CrocomireColorCatalog crocomire, CrocomireBand band) =>
        [.. Enumerable.Range(0, CrocomireColorCatalog.Counts[band]).Select(colour => crocomire.Get(band, colour))];

    /// <summary><paramref name="colours"/>, or an error naming the <paramref name="boss"/> whose colours are not installed.</summary>
    private static T Required<T>(T? colours, string boss) where T : class =>
        colours ?? throw new InvalidDataException($"{boss} is shaded from the installed {boss} colours.");

    /// <summary>Botwoon's health bands over the row from <paramref name="firstInk"/>.</summary>
    /// <exception cref="InvalidDataException">His colours are not installed.</exception>
    private static IEnumerable<SheetShade> Botwoon(BossSourceArtwork bosses, int firstInk)
    {
        BotwoonColorCatalog botwoon = bosses.BotwoonColors ?? throw new InvalidDataException("Botwoon is shaded from the installed Botwoon colours.");
        for (int band = 0; band < BotwoonHealthBandDefinitions.BandCount; band++)
        {
            int shown = band;
            yield return SheetShade.Run(BotwoonShades.Health(shown), firstInk,
                [.. Enumerable.Range(0, BotwoonColorFormat.ColorsPerPalette).Select(colour => botwoon.HealthColor(shown, colour))]);
        }
    }

    /// <summary><paramref name="band"/>'s sixteen colours of <paramref name="source"/>: a health band's, or the hurt flash's.</summary>
    private static RgbColor[] Band(KraidColorCatalog kraid, KraidPaletteSource source, KraidColourBand band) =>
        [.. Enumerable.Range(0, KraidPalettes.BandColors).Select(colour => kraid.Resolve(source, KraidPalettes.BandColors * band.ColourGroup + colour))];

    /// <summary>
    /// Requires the eye colours <paramref name="band"/>'s shade <paramref name="colours"/> show to differ from each other and
    /// from every other colour his body's picture shows under it, so gameplay's substitution of his eyes recolours only them.
    /// </summary>
    /// <exception cref="InvalidDataException">An eye colour repeats on the picture.</exception>
    private static void RequireUniqueEyes(ReadOnlySpan<RgbColor> standard, HashSet<int> used, RgbColor[] colours, KraidColourBand band)
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
