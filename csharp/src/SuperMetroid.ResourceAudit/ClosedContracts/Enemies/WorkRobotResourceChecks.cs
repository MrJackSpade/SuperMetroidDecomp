using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.Core;
using System.Text.Json;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Enemies;

/// <summary>Confirms the specifically identified Work Robot laser resource omission.</summary>
internal static class WorkRobotResourceChecks
{
    /// <summary>Checks the seven Work Robot laser frames load with their compositions.</summary>
    public static void Run()
    {
        // Production catalogs, but constructed art: no cartridge, enemy AI, room,
        // frame clock or player input is needed to expose a missing installed binding.
        var frames = EnemyProjectileSpritemapDefinitions.DocumentFrameNames.ToDictionary(
            name => name, _ => Array.Empty<EnemyProjectilePartDocument>(), StringComparer.Ordinal);
        var programs = EnemyProjectilePresentationFrameSets.All.ToDictionary(
            frame => frame.Name, _ => Array.Empty<EnemyProjectilePartDocument>(), StringComparer.Ordinal);
        EnemyProjectileFrameId[] lasers = [.. WorkRobotLaserAnimations.Segments.Frames];
        for (int index = 0; index < lasers.Length; index++)
        {
            EnemyProjectileFrameId laser = lasers[index];
            Require(EnemyProjectilePresentationFrameSets.All.ToArray().Any(frame => frame.Frame == laser),
                $"Work Robot laser frame {laser} must be included in the installed program-frame set");
            string name = EnemyProjectilePresentationFrameSets.All.ToArray()
                .Single(frame => frame.Frame == laser).Name;
            programs[name] = [new EnemyProjectilePartDocument
            {
                OffsetX = index, OffsetY = 0, Size = 8, Depth = SuperMetroid.Core.Assets.ActorDepth.BehindFrontParts, Palette = 0,
                Character = 0, FlipX = false, FlipY = false,
            }];
        }
        var document = new EnemyProjectileSpritemapDocument
        {
            Version = EnemyProjectileSpritemapDocuments.Version,
            Frames = frames, ProgramFrames = programs,
        };
        EnemyProjectileSpritemapCatalog stock = Load(document);
        for (int index = 0; index < lasers.Length; index++)
        {
            EnemyProjectileFrameId laser = lasers[index];
            Require(stock.GetProgramFrame(laser).Span.Length == 1 &&
                stock.GetProgramFrame(laser).Span[0].Offset.X == index,
                $"installed Work Robot frame {laser} must preserve its actual draw composition");
        }
        Console.WriteLine("Work Robot resources: all seven bindings load.");
    }

    /// <summary>Loads a constructed projectile artwork document.</summary>
    private static EnemyProjectileSpritemapCatalog Load(EnemyProjectileSpritemapDocument document) =>
        EnemyProjectileSpritemapDocuments.Load(new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document, global::SuperMetroid.ResourceAudit.ResourceAuditJson.Info<global::SuperMetroid.AssetExtraction.Enemies.EnemyProjectileSpritemapDocument>(new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }))));

    /// <summary>Reports a failed resource contract with context for the audit command.</summary>
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("Work Robot resource contract failed: " + message);
    }
}
