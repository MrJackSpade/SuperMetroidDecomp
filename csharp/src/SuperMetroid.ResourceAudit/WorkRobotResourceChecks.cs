using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirms the specifically identified Work Robot laser resource omission.</summary>
internal static class WorkRobotResourceChecks
{
    public static void Run()
    {
        // Production catalogs, but constructed art: no cartridge, enemy AI, room,
        // frame clock or player input is needed to expose a missing installed binding.
        var frames = EnemyProjectileSpritemapDefinitions.Frames.ToDictionary(
            frame => frame.Name, _ => Array.Empty<SpriteVisualPart>(), StringComparer.Ordinal);
        var programs = EnemyProjectilePresentationFrameDefinitions.All.ToArray().ToDictionary(
            frame => frame.Name, _ => Array.Empty<SpriteVisualPart>(), StringComparer.Ordinal);
        for (int index = 0; index < WorkRobotLaserInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort operand = WorkRobotLaserInstructionProgramDefinitions.PresentationWordAddress(index);
            Require(EnemyProjectilePresentationFrameDefinitions.Contains(operand),
                $"Work Robot laser operand {operand:X4} must be included in the production extraction catalog");
            string name = EnemyProjectilePresentationFrameDefinitions.All.ToArray()
                .Single(frame => frame.OperandAddress == operand).Name;
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
        for (int index = 0; index < WorkRobotLaserInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort operand = WorkRobotLaserInstructionProgramDefinitions.PresentationWordAddress(index);
            Require(stock.GetProgramFrame(operand).Span.Length == 1 &&
                stock.GetProgramFrame(operand).Span[0].X.SignedOffset == index,
                $"installed Work Robot frame {operand:X4} must preserve its actual draw composition");
        }
        var legacyPrograms = EnemyProjectilePresentationFrameDefinitions.PreWorkRobot.ToArray().ToDictionary(
            frame => frame.Name, _ => Array.Empty<SpriteVisualPart>(), StringComparer.Ordinal);
        Require(programs.Count - legacyPrograms.Count == WorkRobotLaserInstructionProgramDefinitions.PresentationWordCount,
            "the schema change must add only the seven identified Work Robot bindings");
        EnemyProjectilePresentationFrameDefinition edited = EnemyProjectilePresentationFrameDefinitions.PreWorkRobot[0];
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
        Require(merged.GetProgramFrame(edited.OperandAddress).Span[0].X.SignedOffset == -1,
            "a schema-11 override must retain its edited existing composition");
        for (int index = 0; index < WorkRobotLaserInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort operand = WorkRobotLaserInstructionProgramDefinitions.PresentationWordAddress(index);
            Require(merged.GetProgramFrame(operand).Span[0].X.SignedOffset == index,
                "a schema-11 override must inherit each newly required laser composition from stock");
        }
        bool rejected = false;
        try { _ = Load(legacy); }
        catch (InvalidDataException) { rejected = true; }
        Require(rejected, "an incomplete schema-11 stock installation must trigger repair, not silently omit new frames");
        Console.WriteLine("Work Robot resources: all seven bindings load; legacy edits survive, new frames inherit stock, incomplete stock is rejected.");
    }

    private static EnemyProjectileSpritemapCatalog Load(EnemyProjectileSpritemapDocument document,
        EnemyProjectileSpritemapCatalog? stock = null) =>
        EnemyProjectileSpritemapCatalog.Load(new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })), stock);

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("Work Robot resource contract failed: " + message);
    }
}
