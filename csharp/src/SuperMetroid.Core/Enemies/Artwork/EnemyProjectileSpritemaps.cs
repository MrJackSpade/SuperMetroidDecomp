using SuperMetroid.Core.Enemies.Alcoon.Artwork;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The installed enemy-projectile OAM compositions: loading, validating and writing their named document, finding a
/// composition or timed program frame, and their identity.
/// </summary>
internal static class EnemyProjectileSpritemaps
{
    /// <summary>Hashes each composition, then each timed program frame, in name order.</summary>
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    internal static string ContentIdentity(EnemyProjectileSpritemapCatalog catalog) =>
        SelectedPresentationHash.Create("enemy-projectile-oam-v3", content =>
        {
            foreach ((EnemyProjectileFrameId frame, SpritePart[] parts) in catalog.selection.Frames.OrderBy(pair => pair.Key.ToString(), StringComparer.Ordinal))
            {
                content.AppendName("frame", frame.ToString());
                content.AppendEnemyParts(parts);
            }
            foreach ((EnemyProjectileFrameId frame, SpritePart[] parts) in catalog.selection.ProgramFrames.OrderBy(pair => pair.Key.ToString(), StringComparer.Ordinal))
            {
                content.AppendName("program-frame", frame.ToString());
                content.AppendEnemyParts(parts);
            }
        });


    /// <summary>Gets ordered compiled OAM parts by bank-$8D spritemap identity; the native $8000 zero-entry map returns no parts, while an uninstalled nonblank identity is rejected.</summary>
    internal static ReadOnlyMemory<SpritePart> Get(EnemyProjectileSpritemapCatalog catalog, EnemyProjectileFrameId frame) =>
        frame == EnemyProjectileFrameId.Blank
            ? ReadOnlyMemory<SpritePart>.Empty
            : catalog.selection.Frames.TryGetValue(frame, out SpritePart[]? parts)
            ? parts
            : throw new InvalidDataException(
                $"Installed enemy projectile has no composition {frame}.");

    /// <summary>Gets the ordered OAM parts of installed timed program frame <paramref name="frame"/>; an uninstalled frame is rejected.</summary>
    internal static ReadOnlyMemory<SpritePart> GetProgramFrame(EnemyProjectileSpritemapCatalog catalog, EnemyProjectileFrameId frame) =>
        catalog.selection.ProgramFrames.TryGetValue(frame, out SpritePart[]? parts)
            ? parts
            : throw new InvalidDataException(
                $"Installed enemy projectile has no program frame {frame}.");

    /// <summary>Loads named projectile compositions and timed-program visual frames, validating version-specific names/counts and OAM bounds without importing timing, motion, collision, or callbacks.</summary>
    /// <param name="json">Caller-owned <c>enemy-projectile-compositions.json</c> stream, read from its current position and left open; unknown and duplicate properties are rejected.</param>
    /// <param name="stock">Current verified installation used to retain later-added frames when loading an older override; without this fallback only the current schema is accepted.</param>
    /// <returns>Compiled packed spritemap parts, preserving supplied part order and merging legacy edits by frame identity.</returns>
    internal static EnemyProjectileSpritemapCatalog Load(Stream json,
        EnemyProjectileSpritemapCatalog? stock = null)
    {
        Ensure.NotNull(json);
        EnemyProjectileSpritemapDocument document;
        document = JsonAssetDocument.Read<EnemyProjectileSpritemapDocument>(json, EnemyProjectileSpritemapCatalog.Options, "enemy-projectile compositions");
        if (!EnemyProjectileSpritemapCatalog.Versions.TryDecode(document.Version, out EnemyProjectileSpritemapVersion version))
            throw new InvalidDataException(
                $"Enemy-projectile compositions have unknown schema version {document.Version}.");
        bool legacyOverride = version != EnemyProjectileSpritemapVersion.Current && stock is not null;
        int expectedFrames = version == EnemyProjectileSpritemapVersion.CeresOnly && legacyOverride
            ? EnemyProjectileSpritemapDefinitions.LegacyFrameCount
            : EnemyProjectileSpritemapDefinitions.FrameNames.Length;
        if ((!legacyOverride && version != EnemyProjectileSpritemapVersion.Current) ||
            document.Frames is null || document.Frames.Count != expectedFrames)
            throw new InvalidDataException(
                "Enemy-projectile compositions have the wrong version or frame count.");
        // Older overrides retain their edited Ceres/debris frames. Newly introduced
        // shared-program frames come only from the hash-checked stock installation.
        var compiled = stock is not null && legacyOverride
            ? new Dictionary<EnemyProjectileFrameId, SpritePart[]>(stock.selection.Frames)
            : new Dictionary<EnemyProjectileFrameId, SpritePart[]>();
        var compiledPrograms = stock is not null && legacyOverride
            ? new Dictionary<EnemyProjectileFrameId, SpritePart[]>(stock.selection.ProgramFrames)
            : new Dictionary<EnemyProjectileFrameId, SpritePart[]>();
        foreach (string name in EnemyProjectileSpritemapDefinitions.FrameNames.Take(expectedFrames))
        {
            if (!document.Frames.TryGetValue(name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException(
                    $"Enemy-projectile composition {name} is missing.");
            compiled[EnemyProjectileFrames.Identity(name)] = CompileParts(name, visual);
        }
        if (version is not (EnemyProjectileSpritemapVersion.CeresOnly or EnemyProjectileSpritemapVersion.PreProgramFrames))
        {
            EnemyProjectilePresentationFrameDefinition[] definitions = version switch
            {
                EnemyProjectileSpritemapVersion.FirstProgramFrame =>
                    [.. EnemyProjectilePresentationFrameDefinitions.FirstProgramFrames],
                EnemyProjectileSpritemapVersion.PreAlcoon =>
                    EnemyProjectilePresentationFrameSets.PreGoldenTorizo
                        .WhereToArray(frame => !IsAlcoonFireballFrame(frame.Frame))
                        ,
                EnemyProjectileSpritemapVersion.PreGoldenTorizo =>
                    EnemyProjectilePresentationFrameSets.PreGoldenTorizo.ToArray(),
                EnemyProjectileSpritemapVersion.PreGoldenTorizoEgg =>
                    EnemyProjectilePresentationFrameSets.PreGoldenTorizoEgg.ToArray(),
                EnemyProjectileSpritemapVersion.PreTorizoEffects =>
                    EnemyProjectilePresentationFrameSets.PreTorizoEffects.ToArray(),
                EnemyProjectileSpritemapVersion.PreGenericEnemyDeath =>
                    EnemyProjectilePresentationFrameSets.PreGenericEnemyDeath.ToArray(),
                EnemyProjectileSpritemapVersion.PreEnvironmentAndAttack =>
                    EnemyProjectilePresentationFrameSets.PreEnvironmentAndAttack.ToArray(),
                EnemyProjectileSpritemapVersion.PreMotherBrainAndStatue =>
                    EnemyProjectilePresentationFrameSets.PreMotherBrainAndStatue.ToArray(),
                EnemyProjectileSpritemapVersion.PreWorkRobot =>
                    EnemyProjectilePresentationFrameSets.PreWorkRobot.ToArray(),
                EnemyProjectileSpritemapVersion.PrePolypRock =>
                    EnemyProjectilePresentationFrameSets.PrePolypRock.ToArray(),
                EnemyProjectileSpritemapVersion.Current =>
                    EnemyProjectilePresentationFrameSets.All.ToArray(),
                EnemyProjectileSpritemapVersion.CeresOnly or
                    EnemyProjectileSpritemapVersion.PreProgramFrames => throw new InvalidOperationException(
                    $"Enemy-projectile schema {version} has no program frames."),
                _ => throw new InvalidOperationException(
                    $"Enemy-projectile schema {version} has no program frames."),
            };
            if (document.ProgramFrames is null ||
                document.ProgramFrames.Count != definitions.Length)
                throw new InvalidDataException(
                    $"Version-{document.Version} enemy-projectile program frames have the wrong count.");
            foreach (EnemyProjectilePresentationFrameDefinition frame in definitions)
            {
                if (!document.ProgramFrames.TryGetValue(frame.Name,
                        out SpriteVisualPart[]? visual) || visual is null)
                    throw new InvalidDataException(
                        $"Version-{document.Version} enemy-projectile frame {frame.Name} is missing.");
                compiledPrograms[frame.Frame] = CompileParts(frame.Name, visual);
            }
        }
        return new EnemyProjectileSpritemapCatalog(compiled, compiledPrograms);
    }

    /// <summary>Identifies the Alcoon fireball frames the pre-Alcoon legacy schema omitted.</summary>
    private static bool IsAlcoonFireballFrame(EnemyProjectileFrameId frame) =>
        AlcoonFireballFrameDefinitions.ShownProjectileFrames.Values.Contains(frame);

    /// <summary>Validates ordered editable sprite parts against the sheet's bounds.</summary>
    private static SpritePart[] CompileParts(string name, SpriteVisualPart[] visual)
    {
        if (visual.Length > EnemyProjectileSpritemapDefinitions.MaximumParts)
            throw new InvalidDataException(
                $"Enemy-projectile composition {name} is oversized.");
        var parts = new SpritePart[visual.Length];
        for (int index = 0; index < visual.Length; index++)
        {
            SpriteVisualPart? part = visual[index];
            if (part is null || part.OffsetX is < -256 or > 255 ||
                part.OffsetY is < -128 or > 127 || part.Size is not (8 or 16) ||
                part.Priority is < 0 or > 3 || part.Palette is null or < 0 or > 7 ||
                part.TileColumn is < 0 or >= EnemyProjectileSpritemapDefinitions.TileColumns ||
                part.TileRow is < 0 or >= EnemyProjectileSpritemapDefinitions.TileRows)
                throw new InvalidDataException(
                    $"Enemy-projectile composition {name} part {index} is invalid.");
            TileFlip flips =
                (part.FlipX ? TileFlip.Horizontal : 0) |
                (part.FlipY ? TileFlip.Vertical : 0);
            parts[index] = SpritePart.Create(Offset16<ScreenPixels>.Of(part.OffsetX, part.OffsetY), part.Size == 16,
                part.TileRow * EnemyProjectileSpritemapDefinitions.TileColumns +
                        part.TileColumn, part.Palette.Value, part.Priority, flips);
        }
        return parts;
    }

    /// <summary>Writes camel-case JSON only after round-trip loading validates the current schema and OAM bounds.</summary>
    internal static byte[] Write(EnemyProjectileSpritemapDocument document)
    {
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(document, EnemyProjectileSpritemapCatalog.Options);
        _ = Load(new MemoryStream(json, writable: false));
        return json;
    }
}
