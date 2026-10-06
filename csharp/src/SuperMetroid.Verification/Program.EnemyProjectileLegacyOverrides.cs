using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks named historical frame sets rather than assuming current sorted frames form a prefix.</summary>
    private static void VerifyEnemyProjectileLegacyOverrides()
    {
        var fixture = new EnemyIdentityFixture();
        EnemyProjectileSpritemapDocument stockDocument = fixture.ProjectileDocument();
        EnemyProjectileSpritemapCatalog stock = Load(stockDocument);
        string stockIdentity = stock.ContentIdentity;
        int programSchemas = 0;
        for (int version = EnemyProjectileSpritemapDefinitions.CeresOnlyVersion; version <= EnemyProjectileSpritemapDefinitions.Version; version++)
        {
            int count = version == EnemyProjectileSpritemapDefinitions.CeresOnlyVersion
                ? EnemyProjectileSpritemapDefinitions.LegacyFrameCount : EnemyProjectileSpritemapDefinitions.Frames.Length;
            EnemyProjectilePresentationFrameDefinition[] programs = HistoricalPrograms(version);
            EnemyProjectileSpritemapDocument document = stockDocument with
            {
                Version = version,
                Frames = EnemyProjectileSpritemapDefinitions.Frames.Take(count).ToDictionary(frame => frame.Name,
                    frame => stockDocument.Frames[frame.Name].ToArray(), StringComparer.Ordinal),
                ProgramFrames = version >= EnemyProjectileSpritemapDefinitions.FirstProgramFrameVersion
                    ? programs.ToDictionary(frame => frame.Name, frame => stockDocument.ProgramFrames![frame.Name].ToArray(), StringComparer.Ordinal) : null,
            };
            string firstName = EnemyProjectileSpritemapDefinitions.Frames[0].Name;
            document.Frames[firstName] = [document.Frames[firstName][0] with { OffsetX = -11, OffsetY = 6, TileRow = 27, FlipX = true }];
            if (programs.Length != 0)
                document.ProgramFrames![programs[0].Name] = [document.ProgramFrames[programs[0].Name][0] with { OffsetX = 23, OffsetY = -9, TileRow = 31, FlipY = true }];
            EnemyProjectileSpritemapCatalog merged = Load(document, stock);
            string prefix = $"enemy projectile schema {version}: ";
            foreach (var frame in EnemyProjectileSpritemapDefinitions.Frames)
            {
                SpriteVisualPart[] expected = document.Frames.GetValueOrDefault(frame.Name) ?? stockDocument.Frames[frame.Name];
                AssertParts(expected, merged.Get(frame.Pointer).Span, prefix + frame.Name);
            }
            foreach (EnemyProjectilePresentationFrameDefinition frame in EnemyProjectilePresentationFrameDefinitions.All)
            {
                SpriteVisualPart[] expected = document.ProgramFrames?.GetValueOrDefault(frame.Name) ?? stockDocument.ProgramFrames![frame.Name];
                AssertParts(expected, merged.GetProgramFrame(frame.OperandAddress).Span, prefix + frame.Name);
            }
            AssertTrue(stockIdentity != merged.ContentIdentity, prefix + "edits change identity");
            AssertEqual(merged.ContentIdentity, Load(document, stock).ContentIdentity, prefix + "reload preserves identity");
            AssertEqual(stockIdentity, stock.ContentIdentity, prefix + "override does not mutate stock");
            if (version != EnemyProjectileSpritemapDefinitions.Version)
                AssertThrows<InvalidDataException>(() => Load(document), prefix + "legacy overrides require current stock");
            var wrongFrame = new Dictionary<string, SpriteVisualPart[]>(document.Frames, StringComparer.Ordinal);
            wrongFrame.Remove(firstName);
            wrongFrame.Add("unknown-frame", document.Frames[firstName]);
            AssertThrows<InvalidDataException>(() => Load(document with { Frames = wrongFrame }, stock), prefix + "same-count wrong frame keys reject");
            if (programs.Length == 0) continue;
            programSchemas++;
            AssertThrows<InvalidDataException>(() => Load(document with { ProgramFrames = null }, stock), prefix + "missing program frames reject");
            var missing = new Dictionary<string, SpriteVisualPart[]>(document.ProgramFrames!, StringComparer.Ordinal);
            missing.Remove(programs[0].Name);
            AssertThrows<InvalidDataException>(() => Load(document with { ProgramFrames = missing }, stock), prefix + "incomplete program frames reject");
            var wrongKey = new Dictionary<string, SpriteVisualPart[]>(missing, StringComparer.Ordinal) { ["unknown-program"] = document.ProgramFrames![programs[0].Name] };
            AssertThrows<InvalidDataException>(() => Load(document with { ProgramFrames = wrongKey }, stock), prefix + "same-count wrong program keys reject");
        }
        AssertThrows<InvalidDataException>(() => Load(stockDocument with { Version = EnemyProjectileSpritemapDefinitions.CeresOnlyVersion - 1 }, stock), "projectile unknown old version rejects");
        AssertThrows<InvalidDataException>(() => Load(stockDocument with { Version = EnemyProjectileSpritemapDefinitions.Version + 1 }, stock), "projectile unknown future version rejects");
        Console.WriteLine($"PASS enemy projectile overrides: all {EnemyProjectileSpritemapDefinitions.Version} schemas, {programSchemas} program-frame schemas; " +
            "exact edited parts, historical named sets, new stock inheritance, identity and missing-reference rejection. No ROM or gameplay probes.");
        return;

        EnemyProjectileSpritemapCatalog Load(EnemyProjectileSpritemapDocument value, EnemyProjectileSpritemapCatalog? baseline = null)
        {
            using var json = fixture.Json(value);
            return EnemyProjectileSpritemapCatalog.Load(json, baseline);
        }
        static void AssertParts(SpriteVisualPart[] expected, ReadOnlySpan<EnemySpritemapPart> actual, string context)
        {
            // Check each encoded field, including signed offsets and flip bits.
            AssertEqual(expected.Length, actual.Length, context + " ordered part count");
            for (int index = 0; index < expected.Length; index++)
            {
                SpriteVisualPart part = expected[index];
                AssertEqual(part.OffsetX, actual[index].X.SignedOffset, context + " X");
                AssertEqual(unchecked((byte)(sbyte)part.OffsetY), actual[index].Y, context + " Y");
                AssertEqual(part.TileRow * EnemyProjectileSpritemapDefinitions.TileColumns + part.TileColumn,
                    actual[index].Attributes.TileNumber, context + " tile");
                AssertEqual(part.Palette!.Value, actual[index].Attributes.PaletteIndex, context + " palette");
                AssertEqual(part.Priority, actual[index].Attributes.Priority, context + " priority");
                AssertEqual(part.Size == 16, actual[index].X.IsLarge, context + " size");
                AssertEqual(part.FlipX, actual[index].Attributes.FlipHorizontally, context + " horizontal flip");
                AssertEqual(part.FlipY, actual[index].Attributes.FlipVertically, context + " vertical flip");
            }
        }
    }

    private static EnemyProjectilePresentationFrameDefinition[] HistoricalPrograms(int version) => version switch
    {
        < EnemyProjectileSpritemapDefinitions.FirstProgramFrameVersion => [],
        EnemyProjectileSpritemapDefinitions.FirstProgramFrameVersion => EnemyProjectileInstructionMechanicsDefinitions.VisualFrames.ToArray(),
        EnemyProjectileSpritemapDefinitions.PreAlcoonVersion => EnemyProjectilePresentationFrameDefinitions.PreGoldenTorizo.ToArray()
            .Where(frame => !Enumerable.Range(0, AlcoonFireballInstructionProgramDefinitions.PresentationWordCount)
                .Any(index => frame.OperandAddress == AlcoonFireballInstructionProgramDefinitions.PresentationWordAddress(index))).ToArray(),
        EnemyProjectileSpritemapDefinitions.PreGoldenTorizoVersion => EnemyProjectilePresentationFrameDefinitions.PreGoldenTorizo.ToArray(),
        EnemyProjectileSpritemapDefinitions.PreGoldenTorizoEggVersion => EnemyProjectilePresentationFrameDefinitions.PreGoldenTorizoEgg.ToArray(),
        EnemyProjectileSpritemapDefinitions.PreTorizoEffectsVersion => EnemyProjectilePresentationFrameDefinitions.PreTorizoEffects.ToArray(),
        EnemyProjectileSpritemapDefinitions.PreGenericEnemyDeathVersion => EnemyProjectilePresentationFrameDefinitions.PreGenericEnemyDeath.ToArray(),
        EnemyProjectileSpritemapDefinitions.PreEnvironmentAndAttackVersion => EnemyProjectilePresentationFrameDefinitions.PreEnvironmentAndAttack.ToArray(),
        EnemyProjectileSpritemapDefinitions.PreMotherBrainAndStatueVersion => EnemyProjectilePresentationFrameDefinitions.PreMotherBrainAndStatue.ToArray(),
        EnemyProjectileSpritemapDefinitions.PreWorkRobotVersion => EnemyProjectilePresentationFrameDefinitions.PreWorkRobot.ToArray(),
        EnemyProjectileSpritemapDefinitions.PrePolypRockVersion => EnemyProjectilePresentationFrameDefinitions.PrePolypRock.ToArray(),
        EnemyProjectileSpritemapDefinitions.Version => EnemyProjectilePresentationFrameDefinitions.All.ToArray(),
        _ => throw new InvalidOperationException($"Projectile fixture has no historical schema {version}."),
    };
}
