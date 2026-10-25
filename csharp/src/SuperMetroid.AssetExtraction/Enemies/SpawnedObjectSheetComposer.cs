using SuperMetroid.Core.World.Artwork;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies.Corpses;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.SpriteObjects;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Composes the spawned-object sheets of a room graphics set (#1392): the enemy projectiles and room sprite objects its
/// enemies spawn, over the set's OBJ page and in its colours. A spawned object's parts draw at its spawner's tile base or
/// at base 0, so each spawner's objects are composed at its own base, in its row, and every object the set can spawn is
/// also composed at base 0, with the objects any room spawns.
/// </summary>
internal static class SpawnedObjectSheetComposer
{
    /// <summary>The frames rotting corpses' encounters draw themselves, which show the rot: the dead Torizo's hook.</summary>
    private static readonly HashSet<EnemyFrameId> RotEncounterFrames =
        [.. EncounterFrameDefinitions.ByEnemy.Where(encounter => CorpseRotSimulation.Rots(encounter.Key)).SelectMany(encounter => encounter.Value)];

    /// <summary>Whether spawned-object sheet frame <paramref name="name"/> is a rotting corpse's own, which shows its rot.</summary>
    internal static bool ShowsRot(string name) => RotEncounterFrames.Any(frame => EnemySheetDefinitions.EncounterFrameName(frame) == name);

    /// <summary>
    /// The spawned-object sheets of a set whose enemy graphics are <paramref name="graphics"/>, drawing
    /// <paramref name="drawn"/>: by tile base, the spawners whose objects each holds and whether it holds those any room spawns.
    /// </summary>
    internal static IReadOnlyList<SpawnedObjectSheetPlan> Plans(EnemyGraphicsSet graphics, IReadOnlyList<EnemyDefinitionId> drawn)
    {
        Ensure.NotNull(graphics);
        Ensure.NotNull(drawn);
        var plans = new SortedDictionary<int, SpawnedObjectSheetPlan>
        {
            [0] = new SpawnedObjectSheetPlan(0, 0, [], Anywhere: true),
        };
        // The frames encounters draw themselves at character zero join the objects any room spawns.
        foreach (EnemyDefinitionId enemy in drawn.Where(EncounterFrameDefinitions.ByEnemy.ContainsKey).Distinct())
            plans[0].Spawners.Add(enemy);
        foreach (EnemyDefinitionId enemy in drawn.Where(SpawnedObjectFrameDefinitions.ByEnemy.ContainsKey).Distinct())
        {
            if (!plans[0].Spawners.Contains(enemy))
                plans[0].Spawners.Add(enemy);
            EnemyGraphicsIndexes indexes = graphics.DrawIndexes(enemy);
            var word = new SnesObjAttributeWord(checked((ushort)(indexes.TileIndex | indexes.PaletteIndex)));
            int palette = SnesObjAttributeWord.FromPaletteBits(checked((ushort)indexes.PaletteIndex)).PaletteIndex;
            // A projectile draws at its graphics index's low byte, a sprite object at the index's character.
            foreach (int tileBase in new[] { unchecked((byte)word.Raw), word.TileNumber }.Distinct())
            {
                if (!plans.TryGetValue(tileBase, out SpawnedObjectSheetPlan? plan))
                    plans.Add(tileBase, plan = new SpawnedObjectSheetPlan(tileBase, palette, [], Anywhere: false));
                if (!plan.Spawners.Contains(enemy))
                    plan.Spawners.Add(enemy);
            }
        }
        return [.. plans.Values];
    }

    /// <summary>What the spawned-object sheet of <paramref name="plan"/> composes over <paramref name="characters"/>.</summary>
    internal static SpawnedObjectSheetSource Sources(EnemyTileArtworkCatalog enemies, SpawnedObjectSheetPlan plan, SetCharacters characters,
        RgbColor[] colours)
    {
        Ensure.NotNull(enemies);
        Ensure.NotNull(plan);
        EnemyProjectileSpritemapCatalog projectiles = enemies.ProjectileSpritemaps
            ?? throw new InvalidDataException("Spawned-object sheets require the installed projectile compositions.");
        EnemySpritemapCatalog spritemaps = enemies.Spritemaps
            ?? throw new InvalidDataException("Spawned-object sheets require the installed enemy compositions.");
        var tileBase = checked((ushort)plan.TileBase);
        SpawnedObjects spawned = Spawned(plan);
        // The palette bits are the sheet's, ORed in at composition, so each pixel keeps its part's own row.
        FramePictures[] frames = [.. spawned.Projectiles.Select(frame => characters.Pictures(
                EnemySheetDefinitions.ProjectileFrameName(frame), EnemySpriteFrames.Frame(EnemyProjectileFrames.Parts(projectiles, frame), tileBase, 0)))
            .Concat(SpriteObjectFrames(spawned.SpriteObjects).Select(frame => characters.Pictures(
                EnemySheetDefinitions.SpriteObjectFrameName(frame),
                EnemySpriteFrames.Frame(spritemaps.TryGetDisplay(frame, out EnemySpritemapParts parts) ? parts
                    : throw new InvalidDataException($"Room sprite-object frame {frame} has no installed composition."), tileBase, 0))))
            .Concat(EncounterFrames(plan.Spawners, plan.Anywhere).Select(frame => characters.Pictures(
                EnemySheetDefinitions.EncounterFrameName(frame),
                EnemySpriteFrames.Frame(spritemaps.TryGetDisplay(frame, out EnemySpritemapParts parts) ? parts
                    : throw new InvalidDataException($"Encounter frame {frame} has no installed composition."), tileBase, 0),
                RotEncounterFrames.Contains(frame))))];
        return new SpawnedObjectSheetSource(plan,
            new SpriteSheetSources([.. frames.SelectMany(pictures => pictures.Sources)], colours,
                ObjectSelectionCodec.Sizes(GameplayRenderDefinitions.ObjectSelection), plan.Palette),
            frames.Where(pictures => pictures.Layers.Length > 0)
                .ToDictionary(pictures => pictures.Name, pictures => pictures.Layers, StringComparer.Ordinal),
            frames.Where(pictures => pictures.RotCorpse is not null)
                .ToDictionary(pictures => pictures.Name, pictures => pictures.RotCorpse ?? string.Empty, StringComparer.Ordinal));
    }

    /// <summary>The frames a spawned-object sheet holding the objects of <paramref name="spawners"/> holds.</summary>
    internal static IEnumerable<string> FrameNames(IEnumerable<EnemyDefinitionId> spawners, bool anywhere)
    {
        SpawnedObjects spawned = Spawned(new SpawnedObjectSheetPlan(0, 0, [.. spawners], anywhere));
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
        kinds.SelectMany(kind => RoomSpriteObjectAnimations.Programs.FramesFrom(RoomSpriteObjectDefinitions.Program(kind)))
            .Select(RoomSpriteObjectVisualDefinitions.ShownFrames.Of).Distinct().Order();
}
