using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using System.Text.Json.Serialization;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.World.Artwork;

/// <summary>
/// Reads the extended enemy frame documents against the enemy-bank frame schema (#1395): import data whose parts are the
/// original's packed sprite placements (#1638), which only the import composes the enemy sheets from.
/// </summary>
public static class EnemyExtendedFrameDocuments
{
    /// <summary>Validates named frames and compiles visual-only OAM pieces.</summary>
    /// <exception cref="InvalidDataException">The schema, complete named-frame set, components, or display bindings are invalid.</exception>
    public static EnemyExtendedFrameCatalog Load(Stream json)
    {
        Ensure.NotNull(json);
        EnemyExtendedFrameDocument document;
        document = JsonAssetDocument.Read<EnemyExtendedFrameDocument>(json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow, }, "extended enemy composition");
        if (document.Version != EnemyExtendedFrameSchema.Revision || document.Frames is null ||
            document.Frames.Count != EnemyExtendedFrameDefinitions.ExpectedFrameCount)
            throw new InvalidDataException(
                "Extended enemy compositions require the current version and every named frame.");
        EnemyExtendedFrameSequence expected = EnemyExtendedFrameDefinitions.Frames;

        var frames = new Dictionary<EnemyFrameId, EnemyExtendedDrawComponent[]>();
        foreach (EnemyFrameName definition in expected)
        {
            string authoredName = definition.Name;
            // A frame drawn only on BG2 has no sprite piece and every other frame has one, so the frames with a picture
            // follow from the definitions alone (#1638).
            if (!document.Frames.TryGetValue(authoredName,
                    out EnemyExtendedVisualComponent[]? visual) ||
                visual is null || visual.Length > EnemyExtendedFrameBounds.MaximumSpriteComponents(definition) ||
                (visual.Length == 0) != EnemyExtendedFrameBounds.IsBg2Only(definition))
                throw new InvalidDataException(
                    $"Extended enemy frame {authoredName} is missing or exceeds component capacity.");
            var compiled = new EnemyExtendedDrawComponent[visual.Length];
            int totalParts = 0;
            for (int index = 0; index < visual.Length; index++)
            {
                EnemyExtendedVisualComponent? component = visual[index];
                if (component is null || component.OffsetX is < short.MinValue or > short.MaxValue ||
                    component.OffsetY is < short.MinValue or > short.MaxValue ||
                    component.Parts is null)
                    throw new InvalidDataException(
                        $"Extended enemy frame {definition.Name} component {index} is invalid.");
                SpritePart[] parts = EnemySpritemapCompiler.CompileParts(
                    component.Parts, $"{definition.Name} component {index}");
                totalParts += parts.Length;
                if (totalParts > EnemySpritemapCompiler.MaximumParts)
                    throw new InvalidDataException(
                        $"Extended enemy frame {definition.Name} exceeds OAM capacity.");
                compiled[index] = new EnemyExtendedDrawComponent(
                    new((short)component.OffsetX, (short)component.OffsetY),
                    EnemySpritemapParts.FromOwnedArray(parts));
            }
            if (!frames.TryAdd(definition.Frame, compiled))
                throw new InvalidDataException(
                    $"Extended enemy frame {definition.Name} repeats a visual identity.");
        }
        return new EnemyExtendedFrameCatalog(frames, EnemyFrameBindingRules.Extended(document.DisplayFrames));
    }
}
