using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace SuperMetroid.Core.World.Artwork;

/// <summary>Reads installed extended enemy frame documents against the enemy-bank frame schema (#1395).</summary>
public static class EnemyExtendedFrameDocuments
{
    /// <summary>
    /// Validates named frames and compiles visual-only OAM pieces. A complete
    /// current stock catalog lets older overrides retain their edits while newly
    /// introduced frame families come from verified stock.
    /// </summary>
    public static EnemyExtendedFrameCatalog Load(Stream json,
        EnemyExtendedFrameCatalog? stockForLegacyOverride = null)
    {
        Ensure.NotNull(json);
        EnemyExtendedFrameDocument document;
        document = JsonAssetDocument.Read<EnemyExtendedFrameDocument>(json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow, }, "extended enemy composition");
        int schema = document.Version;
        if (!EnemyExtendedFrameSchema.IsSupported(schema) ||
            (schema != EnemyExtendedFrameSchema.Current && stockForLegacyOverride is null))
            throw new InvalidDataException(
                "Extended enemy compositions require the current version and every named frame.");
        int expectedCount = schema switch
        {
            EnemyExtendedFrameSchema.PreKraidFoot =>
                EnemyExtendedFrameDefinitions.PreKraidFootFrameCount,
            EnemyExtendedFrameSchema.PreCrocomireSkeleton =>
                EnemyExtendedFrameDefinitions.PreCrocomireSkeletonFrameCount,
            EnemyExtendedFrameSchema.PreBg2BossBindings =>
                EnemyExtendedFrameDefinitions.PreBg2BossBindingsFrameCount,
            EnemyExtendedFrameSchema.PreMotherBrainBody =>
                EnemyExtendedFrameDefinitions.PreMotherBrainBodyFrameCount,
            EnemyExtendedFrameSchema.PreCompleteTorizo =>
                EnemyExtendedFrameDefinitions.PreCompleteTorizoFrameCount,
            EnemyExtendedFrameSchema.PreGoldenTorizoLeftOrb =>
                EnemyExtendedFrameDefinitions.PreGoldenTorizoLeftOrbFrameCount,
            EnemyExtendedFrameSchema.PreTorizoJumpBackLeft =>
                EnemyExtendedFrameDefinitions.PreTorizoJumpBackLeftFrameCount,
            EnemyExtendedFrameSchema.PreGoldenTorizoLeftFootOrb =>
                EnemyExtendedFrameDefinitions.PreGoldenTorizoLeftFootOrbFrameCount,
            EnemyExtendedFrameSchema.PreTorizoFallingLeft =>
                EnemyExtendedFrameDefinitions.PreTorizoFallingLeftFrameCount,
            EnemyExtendedFrameSchema.PreGoldenTorizoRightSonic =>
                EnemyExtendedFrameDefinitions.PreGoldenTorizoRightSonicFrameCount,
            EnemyExtendedFrameSchema.PreGoldenTorizoRightOrb =>
                EnemyExtendedFrameDefinitions.PreGoldenTorizoRightOrbFrameCount,
            EnemyExtendedFrameSchema.PreTorizoJumpBack =>
                EnemyExtendedFrameDefinitions.PreTorizoJumpBackFrameCount,
            EnemyExtendedFrameSchema.PreGoldenTorizoRightward =>
                EnemyExtendedFrameDefinitions.PreGoldenTorizoRightwardFrameCount,
            EnemyExtendedFrameSchema.PreGoldenTorizoWalking =>
                EnemyExtendedFrameDefinitions.PreGoldenTorizoWalkingFrameCount,
            EnemyExtendedFrameSchema.PreGoldenTorizoAwakening =>
                EnemyExtendedFrameDefinitions.PreGoldenTorizoAwakeningFrameCount,
            EnemyExtendedFrameSchema.PreKraidArm =>
                EnemyExtendedFrameDefinitions.PreKraidArmFrameCount,
            EnemyExtendedFrameSchema.PreGoldenTorizo =>
                EnemyExtendedFrameDefinitions.PreGoldenTorizoFrameCount,
            EnemyExtendedFrameSchema.PreBombTorizo =>
                EnemyExtendedFrameDefinitions.PreBombTorizoFrameCount,
            EnemyExtendedFrameSchema.PreCrocomireBody =>
                EnemyExtendedFrameDefinitions.PreCrocomireBodyFrameCount,
            EnemyExtendedFrameSchema.PreCrocomire =>
                EnemyExtendedFrameDefinitions.PreCrocomireFrameCount,
            EnemyExtendedFrameSchema.PreOum =>
                EnemyExtendedFrameDefinitions.PreOumFrameCount,
            EnemyExtendedFrameSchema.PreCeresSteam =>
                EnemyExtendedFrameDefinitions.PreCeresSteamFrameCount,
            EnemyExtendedFrameSchema.PreSporeIdentity =>
                EnemyExtendedFrameDefinitions.PreCeresSteamFrameCount,
            EnemyExtendedFrameSchema.PreDraygon =>
                EnemyExtendedFrameDefinitions.PreDraygonFrameCount,
            EnemyExtendedFrameSchema.PreDisplayBindings =>
                EnemyExtendedFrameDefinitions.PirateFrameCount,
            EnemyExtendedFrameSchema.PirateDisplayBindings =>
                EnemyExtendedFrameDefinitions.PirateFrameCount,
            EnemyExtendedFrameSchema.First =>
                EnemyExtendedFrameDefinitions.WalkingFrameCount,
            EnemyExtendedFrameSchema.Previous =>
                EnemyExtendedFrameDefinitions.WalkingFrameCount +
                EnemyExtendedFrameDefinitions.WallFrameCount,
            EnemyExtendedFrameSchema.Current =>
                EnemyExtendedFrameDefinitions.ExpectedFrameCount,
            _ => throw new InvalidOperationException($"Undefined EnemyExtendedFrameSchema {schema}."),
        };
        bool legacyOverride = schema != EnemyExtendedFrameSchema.Current;
        if (document.Frames is null ||
            document.Frames.Count != expectedCount ||
            (legacyOverride && (stockForLegacyOverride ?? throw new InvalidDataException("A legacy extended enemy composition override requires the current stock catalog.")).frames.Count !=
                EnemyExtendedFrameDefinitions.ExpectedFrameCount))
            throw new InvalidDataException(
                "Extended enemy compositions require the current version and every named frame.");
        EnemyExtendedFrameSequence expected =
            EnemyExtendedFrameDefinitions.Frames[..expectedCount];

        var frames = new Dictionary<EnemyFrameId, EnemyExtendedDrawComponent[]>();
        var identities = new Dictionary<string, EnemyFrameId>(StringComparer.Ordinal);
        foreach (EnemySpritemapDefinition definition in expected)
        {
            string authoredName = schema == EnemyExtendedFrameSchema.PreSporeIdentity &&
                definition.Name.StartsWith("spore_spawn_oam_", StringComparison.Ordinal)
                    ? "draygon_oam_" + definition.Name["spore_spawn_oam_".Length..]
                    : definition.Name;
            if (!document.Frames.TryGetValue(authoredName,
                    out EnemyExtendedVisualComponent[]? visual) ||
                visual is null || visual.Length > EnemyExtendedFrameDefinitions.MaximumSpriteComponents(definition) ||
                (visual.Length == 0 && !EnemyExtendedFrameDefinitions.IsBg2Only(definition)))
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
                SpritemapPart[] parts = EnemySpritemapCatalog.CompileParts(
                    component.Parts, $"{definition.Name} component {index}");
                totalParts += parts.Length;
                if (totalParts > EnemySpritemapCatalog.MaximumParts)
                    throw new InvalidDataException(
                        $"Extended enemy frame {definition.Name} exceeds OAM capacity.");
                compiled[index] = new EnemyExtendedDrawComponent(
                    new((short)component.OffsetX, (short)component.OffsetY),
                    GoldenTorizoStrideGeometryDefinitions.Compile(definition.Frame,
                        index, EnemySpritemapParts.FromOwnedArray(parts)));
            }
            if (!frames.TryAdd(definition.Frame, compiled))
                throw new InvalidDataException(
                    $"Extended enemy frame {definition.Name} repeats a visual identity.");
            identities.Add(authoredName, definition.Frame);
        }
        var displayFrames = new Dictionary<EnemyFrameId, EnemyFrameId>();
        if (!legacyOverride)
        {
            if (document.DisplayFrames is null ||
                document.DisplayFrames.Count != identities.Count)
                throw new InvalidDataException(
                    "Extended enemy display bindings require every named frame.");
            foreach ((string name, EnemyFrameId identity) in identities)
            {
                if (!document.DisplayFrames.TryGetValue(name, out string? selectedName) ||
                    selectedName is null ||
                    !identities.TryGetValue(selectedName, out EnemyFrameId selected) ||
                    !SameFrameFamily(name, selectedName))
                    throw new InvalidDataException(
                        $"Extended enemy display binding {name} must select a frame of the same enemy family.");
                displayFrames.Add(identity, selected);
            }
        }
        if (!legacyOverride)
            return new EnemyExtendedFrameCatalog(frames, displayFrames);
        var merged = new Dictionary<EnemyFrameId, EnemyExtendedDrawComponent[]>(
            (stockForLegacyOverride ?? throw new InvalidDataException("A legacy extended enemy composition override requires the current stock catalog.")).frames);
        foreach ((EnemyFrameId identity, EnemyExtendedDrawComponent[] components) in frames)
            merged[identity] = components;
        var mergedBindings = new Dictionary<EnemyFrameId, EnemyFrameId>(stockForLegacyOverride.displayFrames);
        if (schema is EnemyExtendedFrameSchema.PirateDisplayBindings
            or EnemyExtendedFrameSchema.PreDraygon
            or EnemyExtendedFrameSchema.PreSporeIdentity
            or EnemyExtendedFrameSchema.PreCeresSteam
            or EnemyExtendedFrameSchema.PreOum
            or EnemyExtendedFrameSchema.PreCrocomire
            or EnemyExtendedFrameSchema.PreCrocomireBody
            or EnemyExtendedFrameSchema.PreBombTorizo
            or EnemyExtendedFrameSchema.PreGoldenTorizo
            or EnemyExtendedFrameSchema.PreKraidArm
            or EnemyExtendedFrameSchema.PreGoldenTorizoAwakening
            or EnemyExtendedFrameSchema.PreGoldenTorizoWalking
            or EnemyExtendedFrameSchema.PreGoldenTorizoRightward
            or EnemyExtendedFrameSchema.PreTorizoJumpBack
            or EnemyExtendedFrameSchema.PreGoldenTorizoRightOrb
            or EnemyExtendedFrameSchema.PreGoldenTorizoRightSonic
            or EnemyExtendedFrameSchema.PreTorizoFallingLeft
            or EnemyExtendedFrameSchema.PreGoldenTorizoLeftFootOrb
            or EnemyExtendedFrameSchema.PreTorizoJumpBackLeft
            or EnemyExtendedFrameSchema.PreGoldenTorizoLeftOrb
            or EnemyExtendedFrameSchema.PreCompleteTorizo
            or EnemyExtendedFrameSchema.PreMotherBrainBody
            or EnemyExtendedFrameSchema.PreBg2BossBindings
            or EnemyExtendedFrameSchema.PreCrocomireSkeleton
            or EnemyExtendedFrameSchema.PreKraidFoot)
        {
            if (document.DisplayFrames is null ||
                document.DisplayFrames.Count != identities.Count)
                throw new InvalidDataException(
                    "Legacy extended enemy display bindings require every authored frame.");
            foreach ((string name, EnemyFrameId identity) in identities)
            {
                if (!document.DisplayFrames.TryGetValue(name, out string? selectedName) ||
                    selectedName is null ||
                    !identities.TryGetValue(selectedName, out EnemyFrameId selected) ||
                    !SameFrameFamily(name, selectedName))
                    throw new InvalidDataException(
                        $"Legacy extended enemy display binding {name} is invalid.");
                mergedBindings[identity] = selected;
            }
        }
        return new EnemyExtendedFrameCatalog(merged, mergedBindings);

        static bool SameFrameFamily(string left, string right)
        {
            if (left.StartsWith("mother_brain_body_oam_", StringComparison.Ordinal))
                return right.StartsWith("mother_brain_body_oam_", StringComparison.Ordinal);
            if (left.StartsWith("torizo_combat_", StringComparison.Ordinal))
                return right.StartsWith("torizo_combat_", StringComparison.Ordinal);
            if (left.StartsWith("kraid_arm_oam_", StringComparison.Ordinal))
                return right.StartsWith("kraid_arm_oam_", StringComparison.Ordinal);
            if (left.StartsWith("kraid_foot_oam_", StringComparison.Ordinal))
                return right.StartsWith("kraid_foot_oam_", StringComparison.Ordinal);
            if (left.StartsWith("golden_torizo_", StringComparison.Ordinal))
                return right.StartsWith("golden_torizo_", StringComparison.Ordinal);
            if (left.StartsWith("bomb_torizo_", StringComparison.Ordinal))
                return right.StartsWith("bomb_torizo_", StringComparison.Ordinal);
            if (left.StartsWith("torizo_jump_back_", StringComparison.Ordinal))
                return right.StartsWith("torizo_jump_back_", StringComparison.Ordinal);
            if (left.StartsWith("torizo_falling_left_", StringComparison.Ordinal))
                return right.StartsWith("torizo_falling_left_", StringComparison.Ordinal);
            if (left.StartsWith("spore_spawn_oam_", StringComparison.Ordinal))
                return right.StartsWith("spore_spawn_oam_", StringComparison.Ordinal);
            if (left.StartsWith("phantoon_bg2_", StringComparison.Ordinal))
                return right.StartsWith("phantoon_bg2_", StringComparison.Ordinal);
            if (left.StartsWith("draygon_oam_", StringComparison.Ordinal) ||
                left.StartsWith("draygon_bg2_", StringComparison.Ordinal))
                return right.StartsWith("draygon_oam_", StringComparison.Ordinal) ||
                    right.StartsWith("draygon_bg2_", StringComparison.Ordinal);
            if (left.StartsWith("ridley_body_", StringComparison.Ordinal))
                return right.StartsWith("ridley_body_", StringComparison.Ordinal);
            if (left.StartsWith("ceres_steam_oam_", StringComparison.Ordinal))
                return right.StartsWith("ceres_steam_oam_", StringComparison.Ordinal);
            if (left.StartsWith("oum_oam_", StringComparison.Ordinal))
                return right.StartsWith("oum_oam_", StringComparison.Ordinal);
            if (left.StartsWith("crocomire_oam_", StringComparison.Ordinal))
                return right.StartsWith("crocomire_oam_", StringComparison.Ordinal);
            if (left.StartsWith("crocomire_skeleton_oam_", StringComparison.Ordinal))
                return right.StartsWith("crocomire_skeleton_oam_", StringComparison.Ordinal);
            if (left.StartsWith("crocomire_body_oam_", StringComparison.Ordinal))
                return right.StartsWith("crocomire_body_oam_", StringComparison.Ordinal);
            int leftEnd = left.IndexOf("_pirate_", StringComparison.Ordinal);
            int rightEnd = right.IndexOf("_pirate_", StringComparison.Ordinal);
            return leftEnd > 0 && rightEnd > 0 && left.AsSpan(0, leftEnd)
                .SequenceEqual(right.AsSpan(0, rightEnd));
        }
    }
}
