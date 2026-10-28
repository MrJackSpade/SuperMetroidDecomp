using SuperMetroid.Core.Enemies.Alcoon.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Enemies.EnemyContentIdentityFixtureVerification;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.Verification.Enemies.Projectiles;

/// <summary>Checks historical enemy-projectile visual schemas preserve edited
/// native and program frames while inheriting newly authored stock artwork.</summary>
internal static class EnemyProjectileLegacyOverridesVerification
{
    /// <summary>Checks named historical frame sets rather than assuming current sorted frames form a prefix.</summary>
    internal static void VerifyEnemyProjectileLegacyOverrides()
    {
        var fixture = new EnemyIdentityFixture();
        EnemyProjectileSpritemapDocument stockDocument = fixture.ProjectileDocument();
        EnemyProjectileSpritemapCatalog stock = Load(stockDocument);
        string stockIdentity = stock.ContentIdentity;
        int programSchemas = 0;
        foreach (EnemyProjectileSpritemapVersion version in Enum.GetValues<EnemyProjectileSpritemapVersion>())
        {
            int count = version == EnemyProjectileSpritemapVersion.CeresOnly
                ? EnemyProjectileSpritemapDocuments.LegacyFrameCount : EnemyProjectileSpritemapDefinitions.Frames.Length;
            EnemyProjectilePresentationFrameDefinition[] programs = HistoricalPrograms(version);
            EnemyProjectileSpritemapDocument document = stockDocument with
            {
                Version = (int)version,
                Frames = EnemyProjectileSpritemapDefinitions.Frames.Take(count).ToDictionary(frame => frame.Name,
                    frame => stockDocument.Frames[frame.Name].ToArray(), StringComparer.Ordinal),
                ProgramFrames = version >= EnemyProjectileSpritemapVersion.FirstProgramFrame
                    ? programs.ToDictionary(frame => frame.Name, frame => (stockDocument.ProgramFrames ?? throw new InvalidOperationException("Verification requires stockDocument.ProgramFrames."))[frame.Name].ToArray(), StringComparer.Ordinal) : null,
            };
            string firstName = EnemyProjectileSpritemapDefinitions.Frames[0].Name;
            document.Frames[firstName] = [document.Frames[firstName][0] with { OffsetX = -11, OffsetY = 6, TileRow = 27, FlipX = true }];
            if (programs.Length != 0)
                (document.ProgramFrames ?? throw new InvalidOperationException("Verification requires document.ProgramFrames."))[programs[0].Name] = [document.ProgramFrames[programs[0].Name][0] with { OffsetX = 23, OffsetY = -9, TileRow = 31, FlipY = true }];
            EnemyProjectileSpritemapCatalog merged = Load(document, stock);
            string prefix = $"enemy projectile schema {(int)version}: ";
            foreach (var frame in EnemyProjectileSpritemapDefinitions.Frames)
            {
                SpriteVisualPart[] expected = document.Frames.GetValueOrDefault(frame.Name) ?? stockDocument.Frames[frame.Name];
                AssertParts(expected, merged.Get(frame.Pointer).Span, prefix + frame.Name);
            }
            foreach (EnemyProjectilePresentationFrameDefinition frame in EnemyProjectilePresentationFrameSets.All)
            {
                SpriteVisualPart[] expected = document.ProgramFrames?.GetValueOrDefault(frame.Name) ?? (stockDocument.ProgramFrames ?? throw new InvalidOperationException("Verification requires stockDocument.ProgramFrames."))[frame.Name];
                AssertParts(expected, merged.GetProgramFrame(frame.Frame).Span, prefix + frame.Name);
            }
            AssertTrue(stockIdentity != merged.ContentIdentity, prefix + "edits change identity");
            AssertEqual(merged.ContentIdentity, Load(document, stock).ContentIdentity, prefix + "reload preserves identity");
            AssertEqual(stockIdentity, stock.ContentIdentity, prefix + "override does not mutate stock");
            if (version != EnemyProjectileSpritemapVersion.Current)
                AssertThrows<InvalidDataException>(() => Load(document), prefix + "legacy overrides require current stock");
            var wrongFrame = new Dictionary<string, SpriteVisualPart[]>(document.Frames, StringComparer.Ordinal);
            wrongFrame.Remove(firstName);
            wrongFrame.Add("unknown-frame", document.Frames[firstName]);
            AssertThrows<InvalidDataException>(() => Load(document with { Frames = wrongFrame }, stock), prefix + "same-count wrong frame keys reject");
            if (programs.Length == 0) continue;
            programSchemas++;
            AssertThrows<InvalidDataException>(() => Load(document with { ProgramFrames = null }, stock), prefix + "missing program frames reject");
            var missing = new Dictionary<string, SpriteVisualPart[]>((document.ProgramFrames ?? throw new InvalidOperationException("Verification requires document.ProgramFrames.")), StringComparer.Ordinal);
            missing.Remove(programs[0].Name);
            AssertThrows<InvalidDataException>(() => Load(document with { ProgramFrames = missing }, stock), prefix + "incomplete program frames reject");
            var wrongKey = new Dictionary<string, SpriteVisualPart[]>(missing, StringComparer.Ordinal) { ["unknown-program"] = document.ProgramFrames[programs[0].Name] };
            AssertThrows<InvalidDataException>(() => Load(document with { ProgramFrames = wrongKey }, stock), prefix + "same-count wrong program keys reject");
        }
        AssertThrows<InvalidDataException>(() => Load(stockDocument with { Version = (int)EnemyProjectileSpritemapVersion.CeresOnly - 1 }, stock), "projectile unknown old version rejects");
        AssertThrows<InvalidDataException>(() => Load(stockDocument with { Version = (int)EnemyProjectileSpritemapVersion.Current + 1 }, stock), "projectile unknown future version rejects");
        Console.WriteLine($"PASS enemy projectile overrides: all {(int)EnemyProjectileSpritemapVersion.Current} schemas, {programSchemas} program-frame schemas; " +
            "exact edited parts, historical named sets, new stock inheritance, identity and missing-reference rejection. No ROM or gameplay probes.");
        return;

        EnemyProjectileSpritemapCatalog Load(EnemyProjectileSpritemapDocument value, EnemyProjectileSpritemapCatalog? baseline = null)
        {
            using var json = fixture.Json(value);
            return EnemyProjectileSpritemapDocuments.Load(json, baseline);
        }
        static void AssertParts(SpriteVisualPart[] expected, ReadOnlySpan<SpritePart> actual, string context)
        {
            // Check each authored field of the installed pieces, including signed offsets and mirroring.
            AssertEqual(expected.Length, actual.Length, context + " ordered part count");
            for (int index = 0; index < expected.Length; index++)
            {
                SpriteVisualPart part = expected[index];
                AssertEqual(part.OffsetX, actual[index].Offset.X, context + " X");
                AssertEqual(part.OffsetY, actual[index].Offset.Y, context + " Y");
                AssertEqual(part.TileRow * EnemyProjectileSpritemapDocuments.TileColumns + part.TileColumn,
                    actual[index].Character, context + " tile");
                AssertEqual((part.Palette ?? throw new InvalidOperationException("Verification requires part.Palette.")), actual[index].Palette, context + " palette");
                AssertEqual(part.Priority, actual[index].Priority, context + " priority");
                AssertEqual(part.Size == 16, actual[index].Large, context + " size");
                AssertEqual(part.FlipX, actual[index].FlipsHorizontally, context + " horizontal flip");
                AssertEqual(part.FlipY, actual[index].FlipsVertically, context + " vertical flip");
            }
        }
    }

    /// <summary>Reconstructs each schema's named instruction-frame domain,
    /// including the versions before particular projectile families were added.</summary>
    private static EnemyProjectilePresentationFrameDefinition[] HistoricalPrograms(EnemyProjectileSpritemapVersion version) => version switch
    {
        EnemyProjectileSpritemapVersion.CeresOnly or EnemyProjectileSpritemapVersion.PreProgramFrames => [],
        EnemyProjectileSpritemapVersion.FirstProgramFrame => [.. EnemyProjectilePresentationFrameDefinitions.FirstProgramFrames],
        EnemyProjectileSpritemapVersion.PreAlcoon => EnemyProjectilePresentationFrameSets.PreGoldenTorizo
            .WhereToArray(frame => !AlcoonFireballFrameDefinitions.ShownProjectileFrames.Values.Contains(frame.Frame)),
        EnemyProjectileSpritemapVersion.PreGoldenTorizo => EnemyProjectilePresentationFrameSets.PreGoldenTorizo.ToArray(),
        EnemyProjectileSpritemapVersion.PreGoldenTorizoEgg => EnemyProjectilePresentationFrameSets.PreGoldenTorizoEgg.ToArray(),
        EnemyProjectileSpritemapVersion.PreTorizoEffects => EnemyProjectilePresentationFrameSets.PreTorizoEffects.ToArray(),
        EnemyProjectileSpritemapVersion.PreGenericEnemyDeath => EnemyProjectilePresentationFrameSets.PreGenericEnemyDeath.ToArray(),
        EnemyProjectileSpritemapVersion.PreEnvironmentAndAttack => EnemyProjectilePresentationFrameSets.PreEnvironmentAndAttack.ToArray(),
        EnemyProjectileSpritemapVersion.PreMotherBrainAndStatue => EnemyProjectilePresentationFrameSets.PreMotherBrainAndStatue.ToArray(),
        EnemyProjectileSpritemapVersion.PreWorkRobot => EnemyProjectilePresentationFrameSets.PreWorkRobot.ToArray(),
        EnemyProjectileSpritemapVersion.PrePolypRock => EnemyProjectilePresentationFrameSets.PrePolypRock.ToArray(),
        EnemyProjectileSpritemapVersion.Current => EnemyProjectilePresentationFrameSets.All.ToArray(),
        _ => throw new InvalidOperationException($"Projectile fixture has no historical schema {version}."),
    };
}
