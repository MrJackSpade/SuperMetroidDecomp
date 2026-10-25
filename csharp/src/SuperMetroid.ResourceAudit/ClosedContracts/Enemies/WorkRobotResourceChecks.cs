using SuperMetroid.Core.Enemies.WorkRobots.Artwork;
using SuperMetroid.Core;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Enemies;

/// <summary>Confirms the specifically identified Work Robot laser resource omission.</summary>
internal static class WorkRobotResourceChecks
{
    /// <summary>Checks seven Work Robot laser frames and preservation of edited legacy art during schema upgrade.</summary>
    public static void Run()
    {
        // Production catalogs, but constructed art: no cartridge, enemy AI, room,
        // frame clock or player input is needed to expose a missing installed binding.
        var frames = EnemyProjectileSpritemapDefinitions.FrameNames.ToDictionary(
            name => name, _ => Array.Empty<SpriteVisualPart>(), StringComparer.Ordinal);
        var programs = EnemyProjectilePresentationFrameSets.All.ToDictionary(
            frame => frame.Name, _ => Array.Empty<SpriteVisualPart>(), StringComparer.Ordinal);
        for (int index = 0; index < WorkRobotLaserFrameDefinitions.PoseCount; index++)
        {
            EnemyProjectileFrameId laser = WorkRobotLaserFrameDefinitions.ShownProjectileFrames[new(index)];
            Require(EnemyProjectilePresentationFrameSets.All.ToArray().Any(frame => frame.Frame == laser),
                $"Work Robot laser frame {laser} must be included in the installed program-frame set");
            string name = EnemyProjectilePresentationFrameSets.All.ToArray()
                .Single(frame => frame.Frame == laser).Name;
            programs[name] = [new SpriteVisualPart
            {
                OffsetX = index, OffsetY = 0, Size = 8, Priority = 2, Palette = 0,
                TileColumn = 0, TileRow = 0, FlipX = false, FlipY = false,
            }];
        }
        var document = new EnemyProjectileSpritemapDocument
        {
            Version = (int)EnemyProjectileSpritemapVersion.Current,
            Frames = frames, ProgramFrames = programs,
        };
        EnemyProjectileSpritemapCatalog stock = Load(document);
        for (int index = 0; index < WorkRobotLaserFrameDefinitions.PoseCount; index++)
        {
            EnemyProjectileFrameId laser = WorkRobotLaserFrameDefinitions.ShownProjectileFrames[new(index)];
            Require(stock.GetProgramFrame(laser).Span.Length == 1 &&
                stock.GetProgramFrame(laser).Span[0].X.SignedOffset == index,
                $"installed Work Robot frame {laser} must preserve its actual draw composition");
        }
        var legacyPrograms = EnemyProjectilePresentationFrameSets.PreWorkRobot.ToDictionary(
            frame => frame.Name, _ => Array.Empty<SpriteVisualPart>(), StringComparer.Ordinal);
        // The Work Robot's own schema generation is the one before the Polyp rock's: compare it, not the current one.
        Require(EnemyProjectilePresentationFrameSets.PrePolypRock.Length - legacyPrograms.Count == WorkRobotLaserFrameDefinitions.PoseCount,
            "the schema change must add only the seven identified Work Robot bindings");
        EnemyProjectilePresentationFrameDefinition edited = EnemyProjectilePresentationFrameSets.PreWorkRobot[0];
        legacyPrograms[edited.Name] = [new SpriteVisualPart
        {
            OffsetX = -1, OffsetY = 0, Size = 8, Priority = 2, Palette = 0,
            TileColumn = 0, TileRow = 0, FlipX = false, FlipY = false,
        }];
        var legacy = document with
        {
            Version = (int)EnemyProjectileSpritemapVersion.PreWorkRobot,
            ProgramFrames = legacyPrograms,
        };
        EnemyProjectileSpritemapCatalog merged = Load(legacy, stock);
        Require(merged.GetProgramFrame(edited.Frame).Span[0].X.SignedOffset == -1,
            "a schema-11 override must retain its edited existing composition");
        for (int index = 0; index < WorkRobotLaserFrameDefinitions.PoseCount; index++)
        {
            EnemyProjectileFrameId laser = WorkRobotLaserFrameDefinitions.ShownProjectileFrames[new(index)];
            Require(merged.GetProgramFrame(laser).Span[0].X.SignedOffset == index,
                "a schema-11 override must inherit each newly required laser composition from stock");
        }
        bool rejected = false;
        try { _ = Load(legacy); }
        catch (InvalidDataException) { rejected = true; }
        Require(rejected, "an incomplete schema-11 stock installation must trigger repair, not silently omit new frames");
        Console.WriteLine("Work Robot resources: all seven bindings load; legacy edits survive, new frames inherit stock, incomplete stock is rejected.");
    }

    /// <summary>Loads a constructed projectile artwork document, optionally merging missing frames from stock.</summary>
    private static EnemyProjectileSpritemapCatalog Load(EnemyProjectileSpritemapDocument document,
        EnemyProjectileSpritemapCatalog? stock = null) =>
        EnemyProjectileSpritemapCatalog.Load(new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })), stock);

    /// <summary>Reports a failed resource contract with context for the audit command.</summary>
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("Work Robot resource contract failed: " + message);
    }
}
