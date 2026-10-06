using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Installed enemy OAM compositions keyed by native visual identity.</summary>
public sealed class EnemySpritemapCatalog
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("enemy-oam-v1", content =>
        {
            foreach ((int frame, EnemySpritemapParts parts) in frames.OrderBy(pair => pair.Key))
            {
                content.Append("frame", frame);
                content.AppendEnemyParts(parts);
            }
            foreach ((int native, int selected) in displayFrames.OrderBy(pair => pair.Key))
            {
                content.Append("native-binding", native);
                content.Append("selected-binding", selected);
            }
        });

    private readonly Dictionary<int, EnemySpritemapParts> frames;
    private readonly Dictionary<int, int> displayFrames;

    private EnemySpritemapCatalog(Dictionary<int, EnemySpritemapParts> frames,
        Dictionary<int, int> displayFrames)
    {
        this.frames = frames;
        this.displayFrames = displayFrames;
    }

    /// <summary>Returns a known installed frame; callers must reject missing artwork.</summary>
    public bool TryGet(byte bank, ushort pointer, out EnemySpritemapParts parts)
    {
        if (frames.TryGetValue((bank << 16) | pointer, out EnemySpritemapParts? found))
        {
            parts = found;
            return true;
        }
        parts = EnemySpritemapParts.Empty;
        return false;
    }

    /// <summary>
    /// Resolves an editable presentation binding without changing the native frame pointer
    /// retained by enemy AI, hitbox selection, or instruction timing.
    /// </summary>
    public bool TryGetDisplay(byte bank, ushort nativePointer,
        out EnemySpritemapParts parts)
    {
        int identity = (bank << 16) | nativePointer;
        if (displayFrames.TryGetValue(identity, out int selected) &&
            frames.TryGetValue(selected, out EnemySpritemapParts? found))
        {
            parts = found;
            return true;
        }
        parts = EnemySpritemapParts.Empty;
        return false;
    }

    /// <summary>
    /// Validates authored frames and compiles visual-only fields into OAM parts.
    /// A complete stock catalog permits a previous-version override to retain its
    /// existing edits while newly added frame identities come from stock content.
    /// </summary>
    public static EnemySpritemapCatalog Load(Stream json,
        EnemySpritemapCatalog? stockForLegacyOverride = null)
    {
        ArgumentNullException.ThrowIfNull(json);
        EnemySpritemapDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<EnemySpritemapDocument>(new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
            }) ?? throw new InvalidDataException("Enemy composition JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid enemy composition JSON.", error);
        }
        int expectedCount = document.Version switch
        {
            EnemySpritemapDefinitions.PreNuclearWaffleVersion when
                stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreNuclearWaffleFrameCount,
            EnemySpritemapDefinitions.PreKraidLintVersion when
                stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreKraidLintFrameCount,
            EnemySpritemapDefinitions.PreAuditOrdinaryVersion when
                stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreAuditOrdinaryFrameCount,
            EnemySpritemapDefinitions.PreFriendlyAnimalVersion when
                stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreFriendlyAnimalFrameCount,
            EnemySpritemapDefinitions.PreZeroVersion when
                stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreZeroFrameCount,
            EnemySpritemapDefinitions.PreMamaTurtleVersion when
                stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreMamaTurtleFrameCount,
            EnemySpritemapDefinitions.PreGunshipVersion when
                stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreGunshipFrameCount,
            EnemySpritemapDefinitions.PreBotwoonVersion when
                stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreBotwoonFrameCount,
            EnemySpritemapDefinitions.PreYardVersion when
                stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreYardFrameCount,
            EnemySpritemapDefinitions.PreWorkRobotVersion when
                stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreWorkRobotFrameCount,
            EnemySpritemapDefinitions.PreEvirVersion when
                stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreEvirFrameCount,
            EnemySpritemapDefinitions.PreMochtroidVersion when
                stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreMochtroidFrameCount,
            EnemySpritemapDefinitions.PreDeadTourianCorpseVersion when
                stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreDeadTourianCorpseFrameCount,
            EnemySpritemapDefinitions.PreDeadTorizoStationaryVersion when
                stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreDeadTorizoStationaryFrameCount,
            EnemySpritemapDefinitions.PreRinkaVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreRinkaFrameCount,
            EnemySpritemapDefinitions.PreViolaVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreViolaFrameCount,
            EnemySpritemapDefinitions.PreChozoStatueVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreChozoStatueFrameCount,
            EnemySpritemapDefinitions.PreNorfairLavaJumperVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreNorfairLavaJumperFrameCount,
            EnemySpritemapDefinitions.PreMultiviolaVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreMultiviolaFrameCount,
            EnemySpritemapDefinitions.PreDragonVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreDragonFrameCount,
            EnemySpritemapDefinitions.PreTripperKamerVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreTripperKamerFrameCount,
            EnemySpritemapDefinitions.PreShaktoolVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreShaktoolFrameCount,
            EnemySpritemapDefinitions.PreMetroidVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreMetroidFrameCount,
            EnemySpritemapDefinitions.PreShutterVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreShutterFrameCount,
            EnemySpritemapDefinitions.PreMorphBallEyeVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreMorphBallEyeFrameCount,
            EnemySpritemapDefinitions.PreFaceBlockVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreFaceBlockFrameCount,
            EnemySpritemapDefinitions.PreKagoVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreKagoFrameCount,
            EnemySpritemapDefinitions.PreFlyVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreFlyFrameCount,
            EnemySpritemapDefinitions.PreSciserVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreSciserFrameCount,
            EnemySpritemapDefinitions.PreRidleySupplementVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreRidleySupplementFrameCount,
            EnemySpritemapDefinitions.PreDeadTorizoVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreDeadTorizoFrameCount,
            EnemySpritemapDefinitions.PreMotherBrainVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreMotherBrainFrameCount,
            EnemySpritemapDefinitions.PreKiHunterVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreKiHunterFrameCount,
            EnemySpritemapDefinitions.PreYappingMawVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreYappingMawFrameCount,
            EnemySpritemapDefinitions.PreRoomSpriteObjectVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreRoomSpriteObjectFrameCount,
            EnemySpritemapDefinitions.PreDraygonBreathVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreDraygonBreathFrameCount,
            EnemySpritemapDefinitions.PreDraygonIntroVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreDraygonIntroFrameCount,
            EnemySpritemapDefinitions.PreElevatorVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreElevatorFrameCount,
            EnemySpritemapDefinitions.PreKamerVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreKamerFrameCount,
            EnemySpritemapDefinitions.PreFuneNamiheVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreFuneNamiheFrameCount,
            EnemySpritemapDefinitions.PreSbugVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreSbugFrameCount,
            EnemySpritemapDefinitions.PreHZoomerVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreHZoomerFrameCount,
            EnemySpritemapDefinitions.PreChootVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreChootFrameCount,
            EnemySpritemapDefinitions.PreHopperVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreHopperFrameCount,
            EnemySpritemapDefinitions.PreBeetomVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreBeetomFrameCount,
            EnemySpritemapDefinitions.PreAlcoonVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreAlcoonFrameCount,
            EnemySpritemapDefinitions.PreBullVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreBullFrameCount,
            EnemySpritemapDefinitions.PrePuyoVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PrePuyoFrameCount,
            EnemySpritemapDefinitions.PreNorfairRioVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreNorfairRioFrameCount,
            EnemySpritemapDefinitions.PreLowerNorfairRioVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreLowerNorfairRioFrameCount,
            EnemySpritemapDefinitions.PreRioVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreRioFrameCount,
            EnemySpritemapDefinitions.PreCeresBabyVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreCeresBabyFrameCount,
            EnemySpritemapDefinitions.PreCeresDoorVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreCeresDoorFrameCount,
            EnemySpritemapDefinitions.PreDisplayBindingsVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreDisplayBindingsFrameCount,
            EnemySpritemapDefinitions.PreMagdolliteVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreMagdolliteFrameCount,
            EnemySpritemapDefinitions.PreFirefleaVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreFirefleaFrameCount,
            EnemySpritemapDefinitions.LegacyVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.LegacyFrameCount,
            EnemySpritemapDefinitions.IntermediateVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.IntermediateFrameCount,
            EnemySpritemapDefinitions.PreOwtchStokeVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreOwtchStokeFrameCount,
            EnemySpritemapDefinitions.PreRipperVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreRipperFrameCount,
            EnemySpritemapDefinitions.EarlierVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.EarlierFrameCount,
            EnemySpritemapDefinitions.PriorVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PriorFrameCount,
            EnemySpritemapDefinitions.PreviousVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreviousFrameCount,
            EnemySpritemapDefinitions.Version => EnemySpritemapDefinitions.Frames.Length,
            _ => -1,
        };
        bool legacyOverride = expectedCount >= 0 &&
            document.Version != EnemySpritemapDefinitions.Version;
        ReadOnlySpan<EnemySpritemapDefinition> expected = expectedCount >= 0
            ? EnemySpritemapDefinitions.Frames[..expectedCount]
            : [];
        if (expectedCount < 0 ||
            document.Frames is null || document.Frames.Count != expected.Length ||
            (legacyOverride && stockForLegacyOverride!.frames.Count !=
                EnemySpritemapDefinitions.Frames.Length))
            throw new InvalidDataException(
                "Enemy compositions require the current version and every named frame.");

        var frames = new Dictionary<int, EnemySpritemapParts>();
        BabyMetroidSpriteParts? sharedBabyBody = null;
        var identities = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (EnemySpritemapDefinition frame in expected)
        {
            if (!document.Frames.TryGetValue(frame.Name, out SpriteVisualPart[]? visual) ||
                visual is null || visual.Length > EnemySpritemapDefinitions.MaximumParts)
                throw new InvalidDataException(
                    $"Enemy composition {frame.Name} is missing or exceeds OAM capacity.");
            EnemySpritemapParts parts = BabyMetroidSpriteParts.Compile((frame.Bank << 16) | frame.Pointer,
                CompileParts(visual, frame.Name), ref sharedBabyBody);
            if (!frames.TryAdd((frame.Bank << 16) | frame.Pointer, parts))
                throw new InvalidDataException(
                    $"Enemy composition {frame.Name} repeats a visual identity.");
            identities.Add(frame.Name, (frame.Bank << 16) | frame.Pointer);
        }
        var displayFrames = new Dictionary<int, int>();
        // Schema/count validation above has already rejected unknown versions.
        // Every accepted schema after the binding boundary owns its authored
        // selection; only earlier art-only schemas inherit stock bindings.
        // Do not maintain a second version list that can silently lose new edits.
        bool hasAuthoredBindings =
            document.Version > EnemySpritemapDefinitions.PreDisplayBindingsVersion;
        if (hasAuthoredBindings)
        {
            if (document.DisplayFrames is null ||
                document.DisplayFrames.Count != identities.Count)
                throw new InvalidDataException(
                    "Enemy display bindings require every named native frame.");
            foreach ((string name, int identity) in identities)
            {
                if (!document.DisplayFrames.TryGetValue(name, out string? selectedName) ||
                    selectedName is null ||
                    !identities.TryGetValue(selectedName, out int selected) ||
                    (identity >> 16) != (selected >> 16))
                    throw new InvalidDataException(
                        $"Enemy display binding {name} must select a named frame in the same bank.");
                displayFrames.Add(identity, selected);
            }
        }
        if (!legacyOverride)
            return new EnemySpritemapCatalog(frames, displayFrames);
        var merged = new Dictionary<int, EnemySpritemapParts>(stockForLegacyOverride!.frames);
        foreach ((int identity, EnemySpritemapParts parts) in frames)
            merged[identity] = parts;
        var mergedBindings = new Dictionary<int, int>(stockForLegacyOverride.displayFrames);
        foreach ((int identity, int selected) in displayFrames)
            mergedBindings[identity] = selected;
        return new EnemySpritemapCatalog(merged, mergedBindings);
    }

    /// <summary>Compiles ordinary OAM pieces shared by plain and extended enemy frames.</summary>
    internal static EnemySpritemapPart[] CompileParts(
        SpriteVisualPart[] visual, string frameName)
    {
        if (visual.Length > EnemySpritemapDefinitions.MaximumParts)
            throw new InvalidDataException(
                $"Enemy composition {frameName} exceeds OAM part capacity.");
        var parts = new EnemySpritemapPart[visual.Length];
        for (int index = 0; index < parts.Length; index++)
        {
            SpriteVisualPart? part = visual[index];
            if (part is null || part.OffsetX is < -256 or > 255 ||
                part.OffsetY is < -128 or > 127 || part.Size is not (8 or 16) ||
                part.Priority is < 0 or > 3 || part.Palette is null or < 0 or > 7 ||
                part.TileColumn is < 0 or >= EnemySpritemapDefinitions.TileColumns ||
                part.TileRow is < 0 or >= EnemySpritemapDefinitions.TileRows)
                throw new InvalidDataException(
                    $"Enemy composition {frameName} part {index} has invalid visual fields.");
            SnesTileFlipFlags flips =
                (part.FlipX ? SnesTileFlipFlags.Horizontal : 0) |
                (part.FlipY ? SnesTileFlipFlags.Vertical : 0);
            parts[index] = new EnemySpritemapPart(
                SnesSpritemapXWord.Create(part.OffsetX, part.Size == 16),
                unchecked((byte)(sbyte)part.OffsetY),
                SnesObjAttributeWord.Create(
                    part.TileRow * EnemySpritemapDefinitions.TileColumns + part.TileColumn,
                    part.Palette.Value, part.Priority, flips));
        }
        return parts;
    }

    internal static void RejectDuplicateProperties(JsonElement element, StringComparer? propertyComparer = null)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(propertyComparer ?? StringComparer.Ordinal);
            foreach (JsonProperty property in element.EnumerateObject())
            {
                if (!names.Add(property.Name))
                    throw new InvalidDataException(
                        $"Duplicate enemy composition property {property.Name}.");
                RejectDuplicateProperties(property.Value, propertyComparer);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
            foreach (JsonElement item in element.EnumerateArray())
                RejectDuplicateProperties(item, propertyComparer);
    }
}

/// <summary>Versioned, semantic enemy frame names mapped to editable OAM parts.</summary>
public sealed record EnemySpritemapDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
    /// <summary>Visual-only native-frame-to-displayed-frame bindings; never AI timing.</summary>
    public Dictionary<string, string>? DisplayFrames { get; init; }
}
