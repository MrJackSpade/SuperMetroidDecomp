using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>One visual component of an extended enemy frame, without hitbox metadata.</summary>
internal readonly record struct EnemyExtendedDrawComponent(
    short OffsetX, short OffsetY, ReadOnlyMemory<EnemySpritemapPart> Parts);

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
                    content.AppendEnemyParts(component.Parts.Span);
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

    /// <summary>Resolves the matching BG2 half of a mixed display frame without mutating its physical selector.</summary>
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
        int expectedCount = document.Version switch
        {
            EnemyExtendedFrameDefinitions.PreMotherBrainBodyVersion
                when stockForLegacyOverride is not null =>
                EnemyExtendedFrameDefinitions.PreMotherBrainBodyFrameCount,
            EnemyExtendedFrameDefinitions.PreCompleteTorizoVersion
                when stockForLegacyOverride is not null =>
                EnemyExtendedFrameDefinitions.PreCompleteTorizoFrameCount,
            EnemyExtendedFrameDefinitions.PreGoldenTorizoLeftOrbVersion
                when stockForLegacyOverride is not null =>
                EnemyExtendedFrameDefinitions.PreGoldenTorizoLeftOrbFrameCount,
            EnemyExtendedFrameDefinitions.PreTorizoJumpBackLeftVersion
                when stockForLegacyOverride is not null =>
                EnemyExtendedFrameDefinitions.PreTorizoJumpBackLeftFrameCount,
            EnemyExtendedFrameDefinitions.PreGoldenTorizoLeftFootOrbVersion
                when stockForLegacyOverride is not null =>
                EnemyExtendedFrameDefinitions.PreGoldenTorizoLeftFootOrbFrameCount,
            EnemyExtendedFrameDefinitions.PreTorizoFallingLeftVersion
                when stockForLegacyOverride is not null =>
                EnemyExtendedFrameDefinitions.PreTorizoFallingLeftFrameCount,
            EnemyExtendedFrameDefinitions.PreGoldenTorizoRightSonicVersion
                when stockForLegacyOverride is not null =>
                EnemyExtendedFrameDefinitions.PreGoldenTorizoRightSonicFrameCount,
            EnemyExtendedFrameDefinitions.PreGoldenTorizoRightOrbVersion
                when stockForLegacyOverride is not null =>
                EnemyExtendedFrameDefinitions.PreGoldenTorizoRightOrbFrameCount,
            EnemyExtendedFrameDefinitions.PreTorizoJumpBackVersion
                when stockForLegacyOverride is not null =>
                EnemyExtendedFrameDefinitions.PreTorizoJumpBackFrameCount,
            EnemyExtendedFrameDefinitions.PreGoldenTorizoRightwardVersion
                when stockForLegacyOverride is not null =>
                EnemyExtendedFrameDefinitions.PreGoldenTorizoRightwardFrameCount,
            EnemyExtendedFrameDefinitions.PreGoldenTorizoWalkingVersion
                when stockForLegacyOverride is not null =>
                EnemyExtendedFrameDefinitions.PreGoldenTorizoWalkingFrameCount,
            EnemyExtendedFrameDefinitions.PreGoldenTorizoAwakeningVersion
                when stockForLegacyOverride is not null =>
                EnemyExtendedFrameDefinitions.PreGoldenTorizoAwakeningFrameCount,
            EnemyExtendedFrameDefinitions.PreKraidArmVersion
                when stockForLegacyOverride is not null =>
                EnemyExtendedFrameDefinitions.PreKraidArmFrameCount,
            EnemyExtendedFrameDefinitions.PreGoldenTorizoVersion
                when stockForLegacyOverride is not null =>
                EnemyExtendedFrameDefinitions.PreGoldenTorizoFrameCount,
            EnemyExtendedFrameDefinitions.PreBombTorizoVersion
                when stockForLegacyOverride is not null =>
                EnemyExtendedFrameDefinitions.PreBombTorizoFrameCount,
            EnemyExtendedFrameDefinitions.PreCrocomireBodyVersion
                when stockForLegacyOverride is not null =>
                EnemyExtendedFrameDefinitions.PreCrocomireBodyFrameCount,
            EnemyExtendedFrameDefinitions.PreCrocomireVersion
                when stockForLegacyOverride is not null =>
                EnemyExtendedFrameDefinitions.PreCrocomireFrameCount,
            EnemyExtendedFrameDefinitions.PreOumVersion
                when stockForLegacyOverride is not null =>
                EnemyExtendedFrameDefinitions.PreOumFrameCount,
            EnemyExtendedFrameDefinitions.PreCeresSteamVersion
                when stockForLegacyOverride is not null =>
                EnemyExtendedFrameDefinitions.PreCeresSteamFrameCount,
            EnemyExtendedFrameDefinitions.PreSporeIdentityVersion
                when stockForLegacyOverride is not null =>
                EnemyExtendedFrameDefinitions.PreCeresSteamFrameCount,
            EnemyExtendedFrameDefinitions.PreDraygonVersion
                when stockForLegacyOverride is not null =>
                EnemyExtendedFrameDefinitions.PreDraygonFrameCount,
            EnemyExtendedFrameDefinitions.PreDisplayBindingsVersion
                when stockForLegacyOverride is not null =>
                EnemyExtendedFrameDefinitions.PirateFrameCount,
            EnemyExtendedFrameDefinitions.PirateDisplayBindingsVersion
                when stockForLegacyOverride is not null =>
                EnemyExtendedFrameDefinitions.PirateFrameCount,
            EnemyExtendedFrameDefinitions.FirstVersion
                when stockForLegacyOverride is not null =>
                EnemyExtendedFrameDefinitions.WalkingFrameCount,
            EnemyExtendedFrameDefinitions.PreviousVersion
                when stockForLegacyOverride is not null =>
                EnemyExtendedFrameDefinitions.WalkingFrameCount +
                EnemyExtendedFrameDefinitions.WallFrameCount,
            EnemyExtendedFrameDefinitions.Version =>
                EnemyExtendedFrameDefinitions.ExpectedFrameCount,
            _ => -1,
        };
        bool legacyOverride = expectedCount >= 0 &&
            document.Version != EnemyExtendedFrameDefinitions.Version;
        if (expectedCount < 0 || document.Frames is null ||
            document.Frames.Count != expectedCount ||
            (legacyOverride && stockForLegacyOverride!.frames.Count !=
                EnemyExtendedFrameDefinitions.ExpectedFrameCount))
            throw new InvalidDataException(
                "Extended enemy compositions require the current version and every named frame.");
        ReadOnlySpan<EnemyExtendedFrameDefinition> expected =
            EnemyExtendedFrameDefinitions.Frames[..expectedCount];

        var frames = new Dictionary<int, EnemyExtendedDrawComponent[]>();
        var identities = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (EnemyExtendedFrameDefinition definition in expected)
        {
            string authoredName = document.Version ==
                EnemyExtendedFrameDefinitions.PreSporeIdentityVersion &&
                definition.Name.StartsWith("spore_spawn_oam_", StringComparison.Ordinal)
                    ? $"draygon_oam_{definition.Pointer:X4}"
                    : definition.Name;
            if (!document.Frames.TryGetValue(authoredName,
                    out EnemyExtendedVisualComponent[]? visual) ||
                visual is null || visual.Length is < 1 or >
                    EnemyExtendedFrameDefinitions.MaximumComponents)
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
                    (short)component.OffsetX, (short)component.OffsetY, parts);
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
        if (document.Version is EnemyExtendedFrameDefinitions.PirateDisplayBindingsVersion
            or EnemyExtendedFrameDefinitions.PreDraygonVersion
            or EnemyExtendedFrameDefinitions.PreSporeIdentityVersion
            or EnemyExtendedFrameDefinitions.PreCeresSteamVersion
            or EnemyExtendedFrameDefinitions.PreOumVersion
            or EnemyExtendedFrameDefinitions.PreCrocomireVersion
            or EnemyExtendedFrameDefinitions.PreCrocomireBodyVersion
            or EnemyExtendedFrameDefinitions.PreBombTorizoVersion
            or EnemyExtendedFrameDefinitions.PreGoldenTorizoVersion
            or EnemyExtendedFrameDefinitions.PreKraidArmVersion
            or EnemyExtendedFrameDefinitions.PreGoldenTorizoAwakeningVersion
            or EnemyExtendedFrameDefinitions.PreGoldenTorizoWalkingVersion
            or EnemyExtendedFrameDefinitions.PreGoldenTorizoRightwardVersion
            or EnemyExtendedFrameDefinitions.PreTorizoJumpBackVersion
            or EnemyExtendedFrameDefinitions.PreGoldenTorizoRightOrbVersion
            or EnemyExtendedFrameDefinitions.PreGoldenTorizoRightSonicVersion
            or EnemyExtendedFrameDefinitions.PreTorizoFallingLeftVersion
            or EnemyExtendedFrameDefinitions.PreGoldenTorizoLeftFootOrbVersion
            or EnemyExtendedFrameDefinitions.PreTorizoJumpBackLeftVersion
            or EnemyExtendedFrameDefinitions.PreGoldenTorizoLeftOrbVersion
            or EnemyExtendedFrameDefinitions.PreCompleteTorizoVersion
            or EnemyExtendedFrameDefinitions.PreMotherBrainBodyVersion)
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
            if (left.StartsWith("draygon_oam_", StringComparison.Ordinal))
                return right.StartsWith("draygon_oam_", StringComparison.Ordinal);
            if (left.StartsWith("ridley_body_", StringComparison.Ordinal))
                return right.StartsWith("ridley_body_", StringComparison.Ordinal);
            if (left.StartsWith("ceres_steam_oam_", StringComparison.Ordinal))
                return right.StartsWith("ceres_steam_oam_", StringComparison.Ordinal);
            if (left.StartsWith("oum_oam_", StringComparison.Ordinal))
                return right.StartsWith("oum_oam_", StringComparison.Ordinal);
            if (left.StartsWith("crocomire_oam_", StringComparison.Ordinal))
                return right.StartsWith("crocomire_oam_", StringComparison.Ordinal);
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
    public required int OffsetX { get; init; }
    public required int OffsetY { get; init; }
    public required SpriteVisualPart[] Parts { get; init; }
}

/// <summary>Versioned editable extended-enemy-frame compositions.</summary>
public sealed record EnemyExtendedFrameDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, EnemyExtendedVisualComponent[]> Frames { get; init; }
    /// <summary>Visual-only frame selection; native timers and hitboxes remain fixed.</summary>
    public Dictionary<string, string>? DisplayFrames { get; init; }
}
