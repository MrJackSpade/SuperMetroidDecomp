using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Bosses.Torizo;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using System.Text.Json.Serialization;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.World.Artwork;

/// <summary>Reads installed extended enemy frame documents against the enemy-bank frame schema (#1395).</summary>
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
        var identities = new Dictionary<string, EnemyFrameId>(StringComparer.Ordinal);
        // Display bindings stay within a family, judged by the frames' names.
        var familyNames = new Dictionary<EnemyFrameId, string>();
        foreach (EnemyFrameName definition in expected)
        {
            string authoredName = definition.Name;
            familyNames.Add(definition.Frame, definition.Name);
            if (!document.Frames.TryGetValue(authoredName,
                    out EnemyExtendedVisualComponent[]? visual) ||
                visual is null || visual.Length > EnemyExtendedFrameBounds.MaximumSpriteComponents(definition) ||
                (visual.Length == 0 && !EnemyExtendedFrameBounds.IsBg2Only(definition)))
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
            identities.Add(authoredName, definition.Frame);
        }
        var displayFrames = new Dictionary<EnemyFrameId, EnemyFrameId>();
        if (document.DisplayFrames is null ||
            document.DisplayFrames.Count != identities.Count)
            throw new InvalidDataException(
                "Extended enemy display bindings require every named frame.");
        foreach ((string name, EnemyFrameId identity) in identities)
        {
            if (!document.DisplayFrames.TryGetValue(name, out string? selectedName) ||
                selectedName is null ||
                !identities.TryGetValue(selectedName, out EnemyFrameId selected) ||
                !SameFrameFamily(familyNames[identity], familyNames[selected]))
                throw new InvalidDataException(
                    $"Extended enemy display binding {name} must select a frame of the same enemy family.");
            displayFrames.Add(identity, selected);
        }
        return new EnemyExtendedFrameCatalog(frames, displayFrames);

        static bool SameFrameFamily(string left, string right)
        {
            if (left.StartsWith("mother_brain_body_oam_", StringComparison.Ordinal))
                return right.StartsWith("mother_brain_body_oam_", StringComparison.Ordinal);
            if (left.StartsWith("torizo_combat_", StringComparison.Ordinal))
                return right.StartsWith("torizo_combat_", StringComparison.Ordinal);
            if (left.StartsWith("kraid_arm_", StringComparison.Ordinal))
                return right.StartsWith("kraid_arm_", StringComparison.Ordinal);
            if (left.StartsWith("kraid_foot_", StringComparison.Ordinal))
                return right.StartsWith("kraid_foot_", StringComparison.Ordinal);
            if (left.StartsWith("golden_torizo_", StringComparison.Ordinal))
                return right.StartsWith("golden_torizo_", StringComparison.Ordinal);
            if (left.StartsWith("bomb_torizo_", StringComparison.Ordinal))
                return right.StartsWith("bomb_torizo_", StringComparison.Ordinal);
            if (left.StartsWith("torizo_jump_back_", StringComparison.Ordinal))
                return right.StartsWith("torizo_jump_back_", StringComparison.Ordinal);
            if (left.StartsWith("torizo_falling_left", StringComparison.Ordinal))
                return right.StartsWith("torizo_falling_left", StringComparison.Ordinal);
            if (left.StartsWith("spore_spawn_", StringComparison.Ordinal))
                return right.StartsWith("spore_spawn_", StringComparison.Ordinal);
            if (left.StartsWith("phantoon_", StringComparison.Ordinal))
                return right.StartsWith("phantoon_", StringComparison.Ordinal);
            if (left.StartsWith("draygon_", StringComparison.Ordinal))
                return right.StartsWith("draygon_", StringComparison.Ordinal);
            if (left.StartsWith("ridley_body_", StringComparison.Ordinal))
                return right.StartsWith("ridley_body_", StringComparison.Ordinal);
            if (left.StartsWith("ceres_steam_", StringComparison.Ordinal))
                return right.StartsWith("ceres_steam_", StringComparison.Ordinal);
            if (left.StartsWith("oum_", StringComparison.Ordinal))
                return right.StartsWith("oum_", StringComparison.Ordinal);
            if (left.StartsWith("crocomire_tongue_", StringComparison.Ordinal))
                return right.StartsWith("crocomire_tongue_", StringComparison.Ordinal);
            if (left.StartsWith("crocomire_skeleton_", StringComparison.Ordinal))
                return right.StartsWith("crocomire_skeleton_", StringComparison.Ordinal);
            if (left.StartsWith("crocomire_body_", StringComparison.Ordinal))
                return right.StartsWith("crocomire_body_", StringComparison.Ordinal);
            int leftEnd = left.IndexOf("_pirate_", StringComparison.Ordinal);
            int rightEnd = right.IndexOf("_pirate_", StringComparison.Ordinal);
            return leftEnd > 0 && rightEnd > 0 && left.AsSpan(0, leftEnd)
                .SequenceEqual(right.AsSpan(0, rightEnd));
        }
    }
}
