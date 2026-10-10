using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>One visual component of an extended enemy frame, without hitbox metadata.</summary>
internal readonly record struct EnemyExtendedDrawComponent(
    short OffsetX, short OffsetY, EnemySpritemapParts Parts);

/// <summary>
/// Installed visual compositions for extended enemy frames. Hitbox records and
/// callback pointers remain application-owned and never come from this JSON.
/// </summary>
public sealed class EnemyExtendedFrameCatalog
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("enemy-extended-oam-v1", content =>
        {
            foreach ((int frame, EnemyExtendedDrawComponent[] components) in frames.OrderBy(pair => pair.Key))
            {
                content.Append("frame", frame);
                content.Append("components", components.Length);
                foreach (EnemyExtendedDrawComponent component in components)
                {
                    content.Append("offset-x", component.OffsetX);
                    content.Append("offset-y", component.OffsetY);
                    content.AppendEnemyParts(component.Parts);
                }
            }
            foreach ((int native, int selected) in displayFrames.OrderBy(pair => pair.Key))
            {
                content.Append("native-binding", native);
                content.Append("selected-binding", selected);
            }
        });

    private readonly Dictionary<int, EnemyExtendedDrawComponent[]> frames;
    private readonly Dictionary<int, int> displayFrames;

    private EnemyExtendedFrameCatalog(
        Dictionary<int, EnemyExtendedDrawComponent[]> frames,
        Dictionary<int, int> displayFrames)
    {
        this.frames = frames;
        this.displayFrames = displayFrames;
    }

    internal bool TryGet(byte bank, ushort pointer,
        out ReadOnlyMemory<EnemyExtendedDrawComponent> components)
    {
        // The bank-local $804F extended frame is shared by ordinary enemy
        // banks. Its sole component points to the empty $804D OAM frame; no
        // editable drawing or cartridge lookup is needed before initialization.
        if (CommonEnemyEmptyExtendedFrameDefinitions.HasFrame(bank, pointer))
        {
            components = ReadOnlyMemory<EnemyExtendedDrawComponent>.Empty;
            return true;
        }
        if (frames.TryGetValue((bank << 16) | pointer,
                out EnemyExtendedDrawComponent[]? found))
        {
            components = found;
            return true;
        }
        components = default;
        return false;
    }

    /// <summary>Maps an immutable physical extended-frame identity to editable draw components.</summary>
    internal bool TryGetDisplay(byte bank, ushort nativePointer,
        out ReadOnlyMemory<EnemyExtendedDrawComponent> components)
    {
        int identity = (bank << 16) | nativePointer;
        if (displayFrames.TryGetValue(identity, out int selected) &&
            frames.TryGetValue(selected, out EnemyExtendedDrawComponent[]? found))
        {
            components = found;
            return true;
        }
        return TryGet(bank, nativePointer, out components);
    }

    /// <summary>Resolves the matching BG2 half of a display frame without mutating its physical selector.</summary>
    internal ushort GetDisplayPointer(byte bank, ushort nativePointer) =>
        displayFrames.TryGetValue((bank << 16) | nativePointer, out int selected)
            ? unchecked((ushort)selected) : nativePointer;

    /// <summary>
    /// Validates named frames and compiles visual-only OAM pieces. A complete
    /// current stock catalog lets older overrides retain their edits while newly
    /// introduced frame families come from verified stock.
    /// </summary>
    public static EnemyExtendedFrameCatalog Load(Stream json,
        EnemyExtendedFrameCatalog? stockForLegacyOverride = null)
    {
        ArgumentNullException.ThrowIfNull(json);
        EnemyExtendedFrameDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            EnemySpritemapCatalog.RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<EnemyExtendedFrameDocument>(
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
                }) ?? throw new InvalidDataException("Extended enemy composition JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid extended enemy composition JSON.", error);
        }
        var schema = (EnemyExtendedFrameSchema)document.Version;
        if (!Enum.IsDefined(schema) ||
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
            (legacyOverride && stockForLegacyOverride!.frames.Count !=
                EnemyExtendedFrameDefinitions.ExpectedFrameCount))
            throw new InvalidDataException(
                "Extended enemy compositions require the current version and every named frame.");
        EnemyExtendedFrameSequence expected =
            EnemyExtendedFrameDefinitions.Frames[..expectedCount];

        var frames = new Dictionary<int, EnemyExtendedDrawComponent[]>();
        var identities = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (EnemyExtendedFrameDefinition definition in expected)
        {
            string authoredName = schema == EnemyExtendedFrameSchema.PreSporeIdentity &&
                definition.Name.StartsWith("spore_spawn_oam_", StringComparison.Ordinal)
                    ? $"draygon_oam_{definition.Pointer:X4}"
                    : definition.Name;
            if (!document.Frames.TryGetValue(authoredName,
                    out EnemyExtendedVisualComponent[]? visual) ||
                visual is null || visual.Length > EnemyExtendedFrameDefinitions.MaximumOamComponents(definition) ||
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
                EnemySpritemapPart[] parts = EnemySpritemapCatalog.CompileParts(
                    component.Parts, $"{definition.Name} component {index}");
                totalParts += parts.Length;
                if (totalParts > EnemySpritemapDefinitions.MaximumParts)
                    throw new InvalidDataException(
                        $"Extended enemy frame {definition.Name} exceeds OAM capacity.");
                compiled[index] = new EnemyExtendedDrawComponent(
                    (short)component.OffsetX, (short)component.OffsetY,
                    GoldenTorizoStrideGeometryDefinitions.Compile((definition.Bank << 16) | definition.Pointer,
                        index, EnemySpritemapParts.FromOwnedArray(parts)));
            }
            if (!frames.TryAdd((definition.Bank << 16) | definition.Pointer,
                    compiled))
                throw new InvalidDataException(
                    $"Extended enemy frame {definition.Name} repeats a visual identity.");
            identities.Add(authoredName,
                (definition.Bank << 16) | definition.Pointer);
        }
        var displayFrames = new Dictionary<int, int>();
        if (!legacyOverride)
        {
            if (document.DisplayFrames is null ||
                document.DisplayFrames.Count != identities.Count)
                throw new InvalidDataException(
                    "Extended enemy display bindings require every named frame.");
            foreach ((string name, int identity) in identities)
            {
                if (!document.DisplayFrames.TryGetValue(name, out string? selectedName) ||
                    selectedName is null ||
                    !identities.TryGetValue(selectedName, out int selected) ||
                    !SameFrameFamily(name, selectedName))
                    throw new InvalidDataException(
                        $"Extended enemy display binding {name} must select a frame of the same enemy family.");
                displayFrames.Add(identity, selected);
            }
        }
        if (!legacyOverride)
            return new EnemyExtendedFrameCatalog(frames, displayFrames);
        var merged = new Dictionary<int, EnemyExtendedDrawComponent[]>(
            stockForLegacyOverride!.frames);
        foreach ((int identity, EnemyExtendedDrawComponent[] components) in frames)
            merged[identity] = components;
        var mergedBindings = new Dictionary<int, int>(stockForLegacyOverride.displayFrames);
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
            foreach ((string name, int identity) in identities)
            {
                if (!document.DisplayFrames.TryGetValue(name, out string? selectedName) ||
                    selectedName is null ||
                    !identities.TryGetValue(selectedName, out int selected) ||
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

/// <summary>Editable, visual-only coordinates and OAM parts of one component.</summary>
public sealed record EnemyExtendedVisualComponent
{
    /// <summary>Gets the signed whole-pixel horizontal displacement from the enemy anchor.</summary>
    public required int OffsetX { get; init; }
    /// <summary>Gets the signed whole-pixel vertical displacement from the enemy anchor.</summary>
    public required int OffsetY { get; init; }
    /// <summary>Gets the ordered visual-only OAM parts drawn for this component.</summary>
    public required SpriteVisualPart[] Parts { get; init; }
}

/// <summary>Versioned editable extended-enemy-frame compositions.</summary>
public sealed record EnemyExtendedFrameDocument
{
    /// <summary>Gets the extended-frame schema revision.</summary>
    public required int Version { get; init; }
    /// <summary>Gets every named visual frame composition defined by this schema revision.</summary>
    public required Dictionary<string, EnemyExtendedVisualComponent[]> Frames { get; init; }
    /// <summary>Visual-only frame selection; native timers and hitboxes remain fixed.</summary>
    public Dictionary<string, string>? DisplayFrames { get; init; }
}
