using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.World.Artwork;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies.Corpses;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.SpriteObjects;
using SuperMetroid.AssetExtraction.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Composes the spawned-object sheets of a room graphics set (#1392, #1513): the enemy projectiles and room sprite objects
/// its enemies spawn, over the set's OBJ page and in its colours, one sheet per owner the game names. A spawned object's
/// parts count from its spawner's character index or from character zero, so each spawner's objects are composed at its
/// own index, in its row, as the sheet of <see cref="SpawnedObjectOwner.Of"/> that spawner; every object the set can
/// spawn is also composed at character zero, with the objects any room spawns, as its
/// <see cref="SpawnedObjectOwner.Shared"/> sheet; and the pieces of Mother Brain's tank glass at the characters they
/// draw from, as the <see cref="SpawnedObjectOwner.MotherBrainTankGlass"/> sheet.
/// </summary>
internal static class SpawnedObjectSheetComposer
{
    /// <summary>
    /// The characters one projectile page holds: a projectile's drawing reads only the part of its spawner's character
    /// index within its page, so its parts count from there.
    /// </summary>
    private const int ProjectilePageCharacters = 256;

    /// <summary>The frames rotting corpses' encounters draw themselves, which show the rot: the dead Torizo's hook.</summary>
    private static readonly HashSet<EnemyFrameId> RotEncounterFrames =
        [.. EncounterFrameDefinitions.ByEnemy.Where(encounter => CorpseRotSimulation.Rots(encounter.Key)).SelectMany(encounter => encounter.Value)];

    /// <summary>Whether spawned-object sheet frame <paramref name="name"/> is a rotting corpse's own, which shows its rot.</summary>
    internal static bool ShowsRot(string name) => RotEncounterFrames.Any(frame => EnemySheetDefinitions.EncounterFrameName(frame) == name);

    /// <summary>
    /// The spawned-object sheets of a set laid out as <paramref name="layout"/>, drawing <paramref name="drawn"/>, each enemy
    /// in the row <paramref name="rowOf"/> gives: the shared sheet, holding every spawner's objects at character zero with
    /// those any room spawns, then each spawner's own sheet and Mother Brain's tank glass's.
    /// </summary>
    internal static IReadOnlyList<SpawnedObjectSheetPlan> Plans(GraphicsSetCharacterLayout layout, IReadOnlyList<EnemyDefinitionId> drawn,
        Func<EnemyDefinitionId, int> rowOf)
    {
        Ensure.NotNull(layout);
        Ensure.NotNull(drawn);
        Ensure.NotNull(rowOf);
        var shared = new SpawnedObjectSheetPlan(SpawnedObjectOwner.Shared, 0, 0, 0, [], Anywhere: true, []);
        // An enemy that spawns no objects of its own hands its spawns, the objects any room spawns, the shared sheet.
        foreach (EnemyDefinitionId enemy in drawn.Where(enemy => !SpawnedObjectFrameDefinitions.ByEnemy.ContainsKey(enemy)).Distinct())
            shared.AlsoOwnedBy.Add(SpawnedObjectOwner.Of(enemy));
        var plans = new List<SpawnedObjectSheetPlan> { shared };
        // The frames encounters draw themselves at character zero join the objects any room spawns.
        foreach (EnemyDefinitionId enemy in drawn.Where(EncounterFrameDefinitions.ByEnemy.ContainsKey).Distinct())
            shared.Spawners.Add(enemy);
        foreach (EnemyDefinitionId enemy in drawn.Where(SpawnedObjectFrameDefinitions.ByEnemy.ContainsKey).Distinct())
        {
            if (!shared.Spawners.Contains(enemy))
                shared.Spawners.Add(enemy);
            int tileBase = layout.TileBaseOf(enemy);
            // The objects' sheet is coloured in the row the set holds their spawner's colours in, or the common row for a
            // spawner drawing from character zero, as objects drawn there are.
            int palette = tileBase == 0 ? 0 : rowOf(enemy);
            // A projectile draws at its spawner's index's place in its page, a sprite object at the index itself.
            plans.Add(new SpawnedObjectSheetPlan(SpawnedObjectOwner.Of(enemy), tileBase % ProjectilePageCharacters, tileBase, palette,
                [enemy], Anywhere: false, []));
            // Mother Brain's tank glass draws from characters of its own, whatever its spawner's (#1435).
            if (enemy == EnemyDefinitionId.MotherBrainBody)
                plans.Add(new SpawnedObjectSheetPlan(SpawnedObjectOwner.MotherBrainTankGlass,
                    EnemyCharacterBaseDefinitions.MotherBrainTankGlassCharacters, EnemyCharacterBaseDefinitions.MotherBrainTankGlassCharacters,
                    rowOf(enemy), [enemy], Anywhere: false, []));
        }
        return plans;
    }

    /// <summary>
    /// What the spawned-object sheet of <paramref name="plan"/> composes over <paramref name="characters"/>, pre-coloured in
    /// the shared shades, its spawners' shades and, at character zero, the shades of the objects any room spawns (#1512). A
    /// spawned object draws in its spawner's shading, so a spawner's effects on its own row, which <paramref name="rowOf"/>
    /// gives, hold on every sheet coloured in that row, the character-zero sheet among them. The sheet is coloured in the
    /// set's colours with each spawner's row holding the colours its frames first show in, as its spawner's are (#1513).
    /// </summary>
    internal static SpawnedObjectSheetSource Sources(EnemyTileSourceArtwork enemies, SpawnedObjectSheetPlan plan, SetCharacters characters,
        EnemySheetShades shades, Func<EnemyDefinitionId, int> rowOf)
    {
        Ensure.NotNull(enemies);
        Ensure.NotNull(plan);
        Ensure.NotNull(shades);
        Ensure.NotNull(rowOf);
        var inkShades = new List<SheetShade>();
        var rowShades = new List<SheetRowShade>(shades.Shared());
        foreach (EnemyDefinitionId spawner in plan.Spawners)
        {
            if (rowOf(spawner) == plan.Palette)
                inkShades.AddRange(shades.OwnRow(spawner, plan.Palette));
            (IReadOnlyList<SheetShade> fixedInks, IReadOnlyList<SheetRowShade> fixedRows) = shades.Fixed(spawner);
            inkShades.AddRange(fixedInks);
            rowShades.AddRange(fixedRows);
        }
        if (plan.Anywhere)
        {
            (IReadOnlyList<SheetShade> anywhereInks, IReadOnlyList<SheetRowShade> anywhereRows) = shades.Anywhere();
            inkShades.AddRange(anywhereInks);
            rowShades.AddRange(anywhereRows);
        }
        EnemyProjectileSpritemapCatalog projectiles = enemies.ProjectileSpritemaps
            ?? throw new InvalidDataException("Spawned-object sheets require the installed projectile compositions.");
        EnemySpritemapCatalog spritemaps = enemies.Spritemaps
            ?? throw new InvalidDataException("Spawned-object sheets require the installed enemy compositions.");
        int projectileBase = checked((ushort)plan.ProjectileCharacters);
        int objectBase = checked((ushort)plan.ObjectCharacters);
        SpawnedObjects spawned = Spawned(plan);
        RgbColor[] colours = shades.FirstShownColours(plan.Spawners);
        // Mother Brain's tank glass sheet is drawn from only by the glass's pieces, which shatter before any of her artwork
        // sequence's steps; the other objects of her body it holds are drawn from her body's own sheet, so none of its
        // frames is drawn under a sequence and none has a picture of a step (#1513).
        FrameDrawings Drawn(FrameDrawings drawings) => plan.Owner.IsMotherBrainTankGlass ? FrameDrawings.None : drawings;
        // The palette bits are the sheet's, ORed in at composition, so each pixel keeps its part's own row.
        FramePictures[] frames = [.. spawned.Projectiles.Select(frame => characters.Pictures(
                EnemySheetDefinitions.ProjectileFrameName(frame), EnemySpriteFrames.Frame(EnemyProjectileFrames.Parts(projectiles, frame), projectileBase, 0),
                Drawn(FrameDrawings.Of(frame))))
            .Concat(SpriteObjectFrames(spawned.SpriteObjects).Select(frame => characters.Pictures(
                EnemySheetDefinitions.SpriteObjectFrameName(frame),
                EnemySpriteFrames.Frame(spritemaps.TryGetDisplay(frame, out EnemySpritemapParts parts) ? parts
                    : throw new InvalidDataException($"Room sprite-object frame {frame} has no installed composition."), objectBase, 0),
                Drawn(FrameDrawings.Of(frame)))))
            .Concat(EncounterFrames(plan.Spawners, plan.Anywhere).Select(frame => characters.Pictures(
                EnemySheetDefinitions.EncounterFrameName(frame),
                EnemySpriteFrames.Frame(spritemaps.TryGetDisplay(frame, out EnemySpritemapParts parts) ? parts
                    : throw new InvalidDataException($"Encounter frame {frame} has no installed composition."), objectBase,
                    EncounterFrameDefinitions.DrawnInRowOf.TryGetValue(frame, out EnemyDefinitionId rowOwner) ? rowOf(rowOwner)
                        : EncounterFrameDefinitions.DrawnInEncounterRow.Contains(frame) ? EncounterRow(frame, plan.Spawners, rowOf) : 0),
                Drawn(FrameDrawings.Of(frame)), RotEncounterFrames.Contains(frame))))];
        return new SpawnedObjectSheetSource(plan,
            new SpriteSheetSources([.. frames.SelectMany(pictures => pictures.Sources)], colours,
                ObjectSelectionCodec.GameplaySizes, plan.Palette,
                Shades: [.. inkShades.DistinctBy(shade => shade.Shade)], RowShades: [.. rowShades.DistinctBy(shade => shade.Shade)]),
            frames.Where(pictures => pictures.Variations.Length > 0)
                .ToDictionary(pictures => pictures.Name, pictures => pictures.Variations, StringComparer.Ordinal),
            frames.Where(pictures => pictures.RotCorpse is not null)
                .ToDictionary(pictures => pictures.Name, pictures => pictures.RotCorpse ?? string.Empty, StringComparer.Ordinal));
    }

    /// <summary>The frames a spawned-object sheet holding the objects of <paramref name="spawners"/> holds.</summary>
    internal static IEnumerable<string> FrameNames(IEnumerable<EnemyDefinitionId> spawners, bool anywhere)
    {
        SpawnedObjects spawned = Spawned(new SpawnedObjectSheetPlan(SpawnedObjectOwner.Shared, 0, 0, 0, [.. spawners], anywhere, []));
        return spawned.Projectiles.Select(EnemySheetDefinitions.ProjectileFrameName)
            .Concat(SpriteObjectFrames(spawned.SpriteObjects).Select(EnemySheetDefinitions.SpriteObjectFrameName))
            .Concat(EncounterFrames(spawners, anywhere).Select(EnemySheetDefinitions.EncounterFrameName));
    }

    /// <summary>
    /// The frames the encounters of <paramref name="spawners"/> draw themselves at character zero, which the character-zero
    /// sheet, the one holding the objects any room spawns, holds, each once in a stable order.
    /// </summary>
    private static IEnumerable<EnemyFrameId> EncounterFrames(IEnumerable<EnemyDefinitionId> spawners, bool anywhere) =>
        anywhere ? spawners.Where(EncounterFrameDefinitions.ByEnemy.ContainsKey).SelectMany(enemy => EncounterFrameDefinitions.ByEnemy[enemy])
            .Distinct().Order() : [];

    /// <summary>
    /// The row the encounter drawing <paramref name="frame"/> draws it in, for a frame its encounter draws in its own row: its
    /// enemy's, as the cartridge ORs the palette index the encounter's initialization stores into every part.
    /// </summary>
    /// <exception cref="InvalidDataException">Two of the sheet's encounters draw the frame in different rows.</exception>
    private static int EncounterRow(EnemyFrameId frame, IEnumerable<EnemyDefinitionId> spawners, Func<EnemyDefinitionId, int> rowOf)
    {
        int[] rows = [.. spawners.Where(enemy => EncounterFrameDefinitions.ByEnemy.TryGetValue(enemy, out IReadOnlyList<EnemyFrameId>? frames) && frames.Contains(frame))
            .Select(rowOf).Distinct()];
        return rows.Length == 1 ? rows[0]
            : throw new InvalidDataException($"Encounter frame {frame} is drawn in {rows.Length} rows by the sheet's encounters, not one.");
    }

    /// <summary>Every object a plan's spawners, and any room when it says so, can spawn, each once in a stable order.</summary>
    private static SpawnedObjects Spawned(SpawnedObjectSheetPlan plan)
    {
        IEnumerable<SpawnedObjects> sources = plan.Spawners.Where(SpawnedObjectFrameDefinitions.ByEnemy.ContainsKey)
            .Select(enemy => SpawnedObjectFrameDefinitions.ByEnemy[enemy]);
        if (plan.Anywhere)
            sources = sources.Prepend(SpawnedObjectFrameDefinitions.Anywhere);
        return new SpawnedObjects([.. sources.SelectMany(spawned => spawned.Projectiles).Distinct().Order()],
            [.. sources.SelectMany(spawned => spawned.SpriteObjects).Distinct().Order()]);
    }

    /// <summary>The enemy frames the room sprite objects of <paramref name="kinds"/> show, each once.</summary>
    private static IEnumerable<EnemyFrameId> SpriteObjectFrames(IEnumerable<RoomSpriteObjectKind> kinds) =>
        kinds.SelectMany(kind => RoomSpriteObjectAnimations.Programs.FramesFrom(RoomSpriteObjectDefinitions.Programs[kind]))
            .Select(RoomSpriteObjectVisualDefinitions.ShownFrames.Of).Distinct().Order();
}
