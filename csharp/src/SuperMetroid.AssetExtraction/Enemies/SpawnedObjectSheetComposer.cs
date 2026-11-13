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
/// spawn is also composed at character zero, with the objects of the common spawners its rooms run, as its
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

    /// <summary>
    /// The spawned-object sheets of a set laid out as <paramref name="layout"/>, drawing <paramref name="drawn"/>, each enemy
    /// in the row <paramref name="rowOf"/> gives: the shared sheet, holding every spawner's objects at character zero with
    /// those of the <paramref name="common"/> spawners the set's rooms run, then each spawner's own sheet and Mother Brain's
    /// tank glass's.
    /// </summary>
    internal static IReadOnlyList<SpawnedObjectSheetPlan> Plans(GraphicsSetCharacterLayout layout, IReadOnlyList<EnemyDefinitionId> drawn,
        Func<EnemyDefinitionId, int> rowOf, IReadOnlyList<CommonSpawnedObjects> common)
    {
        Ensure.NotNull(layout);
        Ensure.NotNull(drawn);
        Ensure.NotNull(rowOf);
        Ensure.NotNull(common);
        var shared = new SpawnedObjectSheetPlan(SpawnedObjectOwner.Shared, 0, 0, 0, [], [], common, []);
        // An enemy that spawns no objects with its own graphics hands its spawns the shared sheet.
        foreach (EnemyDefinitionId enemy in drawn.Where(enemy => !SpawnedObjectFrameDefinitions.ByEnemy.ContainsKey(enemy)).Distinct())
            shared.AlsoOwnedBy.Add(SpawnedObjectOwner.Of(enemy));
        var plans = new List<SpawnedObjectSheetPlan> { shared };
        // The frames encounters draw themselves at character zero join the objects any room spawns.
        foreach (EnemyDefinitionId enemy in drawn.Where(EncounterFrameDefinitions.ByEnemy.ContainsKey).Distinct())
            shared.Spawners.Add(enemy);
        // What enemies drawn under the set spawn with the room's graphics draws from the shared sheet (#1513).
        foreach (EnemyDefinitionId enemy in drawn.Where(SpawnedObjectFrameDefinitions.SharedByEnemy.ContainsKey).Distinct())
            shared.SharedSpawners.Add(enemy);
        foreach (EnemyDefinitionId enemy in drawn.Where(SpawnedObjectFrameDefinitions.ByEnemy.ContainsKey).Distinct())
        {
            int tileBase = layout.TileBaseOf(enemy);
            // The objects' sheet is coloured in the row the set holds their spawner's colours in, whatever its character
            // index: spawning with the spawner's graphics ORs its palette bits into the objects' parts (#1513).
            int palette = rowOf(enemy);
            // A projectile draws at its spawner's index's place in its page, a sprite object at the index itself.
            plans.Add(new SpawnedObjectSheetPlan(SpawnedObjectOwner.Of(enemy), tileBase % ProjectilePageCharacters, tileBase, palette,
                [enemy], [], [], []));
            // Mother Brain's tank glass draws from characters of its own, whatever its spawner's (#1435).
        }
        // Mother Brain's tank glass draws from characters of its own, whatever its spawner's (#1435), and holds only what
        // draws from that sheet (#1513).
        if (drawn.Contains(EnemyDefinitionId.MotherBrainBody) && SpawnedObjectFrameDefinitions.BySheet.ContainsKey(SpawnedObjectOwner.MotherBrainTankGlass))
            plans.Add(new SpawnedObjectSheetPlan(SpawnedObjectOwner.MotherBrainTankGlass,
                EnemyCharacterBaseDefinitions.MotherBrainTankGlassCharacters, EnemyCharacterBaseDefinitions.MotherBrainTankGlassCharacters,
                rowOf(EnemyDefinitionId.MotherBrainBody), [EnemyDefinitionId.MotherBrainBody], [], [], []));
        return plans;
    }

    /// <summary>
    /// What the spawned-object sheet of <paramref name="plan"/> composes over <paramref name="characters"/>, pre-coloured in
    /// the shared shades, its spawners' shades and, on the shared sheet, the shades of the objects its enemies and its common
    /// spawners spawn (#1512, #1513). A
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
        if (plan.Owner == SpawnedObjectOwner.Shared)
        {
            // The objects an enemy spawns onto the shared sheet show the shades its fight lays over their rows, and a common
            // spawner's objects those its code lays (#1513).
            foreach (EnemyDefinitionId spawner in plan.SharedSpawners.Concat(plan.Spawners).Distinct())
            {
                (IReadOnlyList<SheetShade> spawnedInks, IReadOnlyList<SheetRowShade> spawnedRows) = shades.SpawnedBy(spawner);
                inkShades.AddRange(spawnedInks);
                rowShades.AddRange(spawnedRows);
            }
            foreach (CommonSpawnedObjects common in plan.Common)
            {
                (IReadOnlyList<SheetShade> commonInks, IReadOnlyList<SheetRowShade> commonRows) =
                    shades.Common(CommonSpawners.Named(common.Spawner).Presence.Shades);
                inkShades.AddRange(commonInks);
                rowShades.AddRange(commonRows);
            }
        }
        EnemyProjectileSpritemapCatalog projectiles = enemies.ProjectileSpritemaps
            ?? throw new InvalidDataException("Spawned-object sheets require the installed projectile compositions.");
        EnemySpritemapCatalog spritemaps = enemies.Spritemaps
            ?? throw new InvalidDataException("Spawned-object sheets require the installed enemy compositions.");
        int projectileBase = checked((ushort)plan.ProjectileCharacters);
        int objectBase = checked((ushort)plan.ObjectCharacters);
        SpawnedObjects spawned = Spawned(plan);
        // Mother Brain's tank glass sheet is drawn from only by the glass's pieces, which shatter before she shows any look
        // after her room's load; the other objects of her body it holds are drawn from her body's own sheet, so none of its
        // frames is drawn in a look of hers and each shows her room's load (#1513, #1538).
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
            .Concat(EncounterFrames(plan.Spawners, plan.Owner == SpawnedObjectOwner.Shared).Select(frame => characters.Pictures(
                EnemySheetDefinitions.EncounterFrameName(frame),
                EnemySpriteFrames.Frame(spritemaps.TryGetDisplay(frame, out EnemySpritemapParts parts) ? parts
                    : throw new InvalidDataException($"Encounter frame {frame} has no installed composition."), objectBase,
                    EncounterFrameDefinitions.DrawnInRowOf.TryGetValue(frame, out EnemyDefinitionId rowOwner) ? rowOf(rowOwner)
                        : EncounterFrameDefinitions.DrawnInEncounterRow.Contains(frame) ? EncounterRow(frame, plan.Spawners, rowOf) : 0),
                Drawn(FrameDrawings.Of(frame)), RottingCorpseCells.ShowsInEncounter(frame))))];
        // Each of the sheet's spawners whose encounter sets colours leaves them in the rows they write; a frame family
        // always drawn in one colouring is coloured in it; and each frame an enemy spawns onto the shared sheet with the
        // room's graphics is coloured over the rows it inks in that spawner's first colouring (the statue fragments and orbs
        // a Torizo's colours show, Spore Spawn's spores), so spawners that show their objects at different times in
        // different colourings each colour only their own objects' rows (#1513).
        RgbColor[] colours = shades.WithFrameColourings(shades.FirstShownColours(plan.Spawners), frames, plan.Palette, SharedSpawnersOf(plan));
        return new SpawnedObjectSheetSource(plan,
            new SpriteSheetSources([.. frames.SelectMany(pictures => pictures.Sources)], colours,
                ObjectSelectionCodec.GameplaySizes, plan.Palette,
                Shades: [.. inkShades.DistinctBy(shade => shade.Shade)], RowShades: [.. rowShades.DistinctBy(shade => shade.Shade)]),
            frames.Where(pictures => pictures.Changes.Count > 0)
                .ToDictionary(pictures => pictures.Name, pictures => pictures.Changes, StringComparer.Ordinal),
            frames.SelectMany(pictures => pictures.Corpses).ToDictionary(corpse => corpse.Key, corpse => corpse.Value, StringComparer.Ordinal),
            frames.SelectMany(pictures => pictures.Blank).ToHashSet(StringComparer.Ordinal));
    }

    /// <summary>
    /// The frames the spawned-object sheet of <paramref name="owner"/> holding the objects of <paramref name="spawners"/> and,
    /// on the shared sheet, the room-graphics spawns of <paramref name="sharedSpawners"/> and the objects of the
    /// <paramref name="common"/> spawners holds.
    /// </summary>
    internal static IEnumerable<string> FrameNames(SpawnedObjectOwner owner, IEnumerable<EnemyDefinitionId> spawners,
        IEnumerable<EnemyDefinitionId> sharedSpawners, IReadOnlyList<CommonSpawnedObjects> common)
    {
        SpawnedObjects spawned = Spawned(new SpawnedObjectSheetPlan(owner, 0, 0, 0, [.. spawners], [.. sharedSpawners], common, []));
        return spawned.Projectiles.Select(EnemySheetDefinitions.ProjectileFrameName)
            .Concat(SpriteObjectFrames(spawned.SpriteObjects).Select(EnemySheetDefinitions.SpriteObjectFrameName))
            .Concat(EncounterFrames(spawners, owner == SpawnedObjectOwner.Shared).Select(EnemySheetDefinitions.EncounterFrameName));
    }

    /// <summary>
    /// The frames the encounters of <paramref name="spawners"/> draw themselves at character zero, which the character-zero
    /// shared sheet holds, each once in a stable order.
    /// </summary>
    private static IEnumerable<EnemyFrameId> EncounterFrames(IEnumerable<EnemyDefinitionId> spawners, bool shared) =>
        shared ? spawners.Where(EncounterFrameDefinitions.ByEnemy.ContainsKey).SelectMany(enemy => EncounterFrameDefinitions.ByEnemy[enemy])
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

    /// <summary>
    /// Every object that draws from a plan's sheet, each once in a stable order: a sheet-specific owner's own objects; a
    /// spawner's own-graphics spawns; on the shared sheet, the room-graphics spawns of the set's enemies and the objects of
    /// the common spawners its rooms run.
    /// </summary>
    private static SpawnedObjects Spawned(SpawnedObjectSheetPlan plan)
    {
        IEnumerable<SpawnedObjects> sources = SpawnedObjectFrameDefinitions.BySheet.TryGetValue(plan.Owner, out SpawnedObjects? owned)
            ? [owned]
            : plan.Spawners.Where(SpawnedObjectFrameDefinitions.ByEnemy.ContainsKey).Select(enemy => SpawnedObjectFrameDefinitions.ByEnemy[enemy])
                .Concat(plan.SharedSpawners.Select(enemy => SpawnedObjectFrameDefinitions.SharedByEnemy[enemy]));
        sources = plan.Common.Select(common => common.Objects).Concat(sources);
        return new SpawnedObjects([.. sources.SelectMany(spawned => spawned.Projectiles).Distinct().Order()],
            [.. sources.SelectMany(spawned => spawned.SpriteObjects).Distinct().Order()]);
    }

    /// <summary>
    /// The enemies of <paramref name="plan"/> that spawn each of its frames with the room's graphics, by frame name: on the
    /// shared sheet, each of its shared spawners for every projectile frame and sprite-object frame it spawns there.
    /// </summary>
    private static SharedSpawnerFrames SharedSpawnersOf(SpawnedObjectSheetPlan plan)
    {
        var spawnersOf = new Dictionary<string, List<EnemyDefinitionId>>(StringComparer.Ordinal);
        foreach (EnemyDefinitionId spawner in plan.SharedSpawners.Distinct())
        {
            SpawnedObjects spawned = SpawnedObjectFrameDefinitions.SharedByEnemy[spawner];
            foreach (string frame in spawned.Projectiles.Select(EnemySheetDefinitions.ProjectileFrameName)
                .Concat(SpriteObjectFrames(spawned.SpriteObjects).Select(EnemySheetDefinitions.SpriteObjectFrameName)).Distinct())
            {
                if (!spawnersOf.TryGetValue(frame, out List<EnemyDefinitionId>? spawners))
                {
                    spawners = [];
                    spawnersOf.Add(frame, spawners);
                }
                spawners.Add(spawner);
            }
        }
        return new SharedSpawnerFrames(spawnersOf);
    }

    /// <summary>The enemy frames the room sprite objects of <paramref name="kinds"/> show, each once.</summary>
    private static IEnumerable<EnemyFrameId> SpriteObjectFrames(IEnumerable<RoomSpriteObjectKind> kinds) =>
        kinds.SelectMany(kind => RoomSpriteObjectAnimations.Segments.FramesOf(RoomSpriteObjectAnimations.StartingWith(RoomSpriteObjectDefinitions.Animations[kind])))
            .Select(RoomSpriteObjectVisualDefinitions.ShownFrames.Of).Distinct().Order();
}
