using SuperMetroid.Core.Enemies.Alcoon.Artwork;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Alcoon;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Named, editable bank-$8D enemy-projectile OAM compositions.</summary>
public sealed class EnemyProjectileSpritemapCatalog
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("enemy-projectile-oam-v1", content =>
        {
            foreach ((ushort frame, EnemySpritemapPart[] parts) in frames.OrderBy(pair => pair.Key))
            {
                content.Append("frame", frame);
                content.AppendEnemyParts(parts);
            }
            foreach ((ushort operand, EnemySpritemapPart[] parts) in programFrames.OrderBy(pair => pair.Key))
            {
                content.Append("program-operand", operand);
                content.AppendEnemyParts(parts);
            }
        });

    /// <summary>Installed ordered OAM parts keyed by bank-$8D spritemap pointer.</summary>
    private readonly Dictionary<ushort, EnemySpritemapPart[]> frames;
    /// <summary>Installed timed-frame OAM parts keyed by bank-$86 visual operand address.</summary>
    private readonly Dictionary<ushort, EnemySpritemapPart[]> programFrames;

    /// <summary>Retains independently validated compositions and timed-program visual bindings.</summary>
    private EnemyProjectileSpritemapCatalog(Dictionary<ushort, EnemySpritemapPart[]> frames,
        Dictionary<ushort, EnemySpritemapPart[]> programFrames)
    {
        this.frames = frames;
        this.programFrames = programFrames;
    }

    /// <summary>Gets ordered compiled OAM parts by bank-$8D spritemap identity; the native $8000 zero-entry map returns no parts, while an uninstalled nonblank identity is rejected.</summary>
    public ReadOnlyMemory<EnemySpritemapPart> Get(ushort pointer) =>
        pointer == EnemyProjectileSpritemapDefinitions.BlankSpritemap
            ? ReadOnlyMemory<EnemySpritemapPart>.Empty
            : frames.TryGetValue(pointer, out EnemySpritemapPart[]? parts)
            ? parts
            : throw new InvalidDataException(
                $"Installed enemy projectile has no composition at $8D:{pointer:X4}.");

    /// <summary>Draws a timed program frame without reading its bank-$86/$8D visual operands.</summary>
    public ReadOnlyMemory<EnemySpritemapPart> GetProgramFrame(ushort operandAddress) =>
        programFrames.TryGetValue(operandAddress, out EnemySpritemapPart[]? parts)
            ? parts
            : throw new InvalidDataException(
                $"Installed enemy projectile has no frame for $86:{operandAddress:X4}.");

    /// <summary>Loads named projectile compositions and timed-program visual frames, validating version-specific names/counts and OAM bounds without importing timing, motion, collision, or callbacks.</summary>
    /// <param name="json">Caller-owned <c>enemy-projectile-compositions.json</c> stream, read from its current position and left open; unknown and duplicate properties are rejected.</param>
    /// <param name="stock">Current verified installation used to retain later-added frames when loading an older override; without this fallback only the current schema is accepted.</param>
    /// <returns>Compiled packed spritemap parts, preserving supplied part order and merging legacy edits by native pointer or visual-operand identity.</returns>
    public static EnemyProjectileSpritemapCatalog Load(Stream json,
        EnemyProjectileSpritemapCatalog? stock = null)
    {
        Ensure.NotNull(json);
        EnemyProjectileSpritemapDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            EnemySpritemapCatalog.RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<EnemyProjectileSpritemapDocument>(Options)
                ?? throw new InvalidDataException("Enemy-projectile compositions are null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid enemy-projectile compositions JSON.", error);
        }
        var version = (EnemyProjectileSpritemapVersion)document.Version;
        if (!Enum.IsDefined(version))
            throw new InvalidDataException(
                $"Enemy-projectile compositions have unknown schema version {document.Version}.");
        bool legacyOverride = version < EnemyProjectileSpritemapVersion.Current && stock is not null;
        int expectedFrames = version == EnemyProjectileSpritemapVersion.CeresOnly && legacyOverride
            ? EnemyProjectileSpritemapDefinitions.LegacyFrameCount
            : EnemyProjectileSpritemapDefinitions.Frames.Length;
        if ((!legacyOverride && version != EnemyProjectileSpritemapVersion.Current) ||
            document.Frames is null || document.Frames.Count != expectedFrames)
            throw new InvalidDataException(
                "Enemy-projectile compositions have the wrong version or frame count.");
        // Older overrides retain their edited Ceres/debris frames. Newly introduced
        // shared-program frames come only from the hash-checked stock installation.
        var compiled = stock is not null && legacyOverride
            ? new Dictionary<ushort, EnemySpritemapPart[]>(stock.frames)
            : new Dictionary<ushort, EnemySpritemapPart[]>();
        var compiledPrograms = stock is not null && legacyOverride
            ? new Dictionary<ushort, EnemySpritemapPart[]>(stock.programFrames)
            : new Dictionary<ushort, EnemySpritemapPart[]>();
        foreach ((ushort pointer, string name) in
                 EnemyProjectileSpritemapDefinitions.Frames.Take(expectedFrames))
        {
            if (!document.Frames.TryGetValue(name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException(
                    $"Enemy-projectile composition {name} is missing.");
            compiled[pointer] = CompileParts(name, visual);
        }
        if (version >= EnemyProjectileSpritemapVersion.FirstProgramFrame)
        {
            EnemyProjectilePresentationFrameDefinition[] definitions = version switch
            {
                EnemyProjectileSpritemapVersion.FirstProgramFrame =>
                    [.. EnemyProjectilePresentationFrameDefinitions.FirstProgramFrames],
                EnemyProjectileSpritemapVersion.PreAlcoon =>
                    EnemyProjectilePresentationFrameSets.PreGoldenTorizo
                        .WhereToArray(frame => !IsAlcoonFireballOperand(frame.OperandAddress))
                        ,
                EnemyProjectileSpritemapVersion.PreGoldenTorizo =>
                    EnemyProjectilePresentationFrameSets.PreGoldenTorizo.ToArray(),
                EnemyProjectileSpritemapVersion.PreGoldenTorizoEgg =>
                    EnemyProjectilePresentationFrameSets.PreGoldenTorizoEgg.ToArray(),
                EnemyProjectileSpritemapVersion.PreTorizoEffects =>
                    EnemyProjectilePresentationFrameSets.PreTorizoEffects.ToArray(),
                EnemyProjectileSpritemapVersion.PreGenericEnemyDeath =>
                    EnemyProjectilePresentationFrameSets.PreGenericEnemyDeath.ToArray(),
                EnemyProjectileSpritemapVersion.PreEnvironmentAndAttack =>
                    EnemyProjectilePresentationFrameSets.PreEnvironmentAndAttack.ToArray(),
                EnemyProjectileSpritemapVersion.PreMotherBrainAndStatue =>
                    EnemyProjectilePresentationFrameSets.PreMotherBrainAndStatue.ToArray(),
                EnemyProjectileSpritemapVersion.PreWorkRobot =>
                    EnemyProjectilePresentationFrameSets.PreWorkRobot.ToArray(),
                EnemyProjectileSpritemapVersion.PrePolypRock =>
                    EnemyProjectilePresentationFrameSets.PrePolypRock.ToArray(),
                EnemyProjectileSpritemapVersion.Current =>
                    EnemyProjectilePresentationFrameSets.All.ToArray(),
                _ => throw new InvalidOperationException(
                    $"Enemy-projectile schema {version} has no program frames."),
            };
            if (document.ProgramFrames is null ||
                document.ProgramFrames.Count != definitions.Length)
                throw new InvalidDataException(
                    $"Version-{document.Version} enemy-projectile program frames have the wrong count.");
            foreach (EnemyProjectilePresentationFrameDefinition frame in definitions)
            {
                if (!document.ProgramFrames.TryGetValue(frame.Name,
                        out SpriteVisualPart[]? visual) || visual is null)
                    throw new InvalidDataException(
                        $"Version-{document.Version} enemy-projectile frame {frame.Name} is missing.");
                compiledPrograms[frame.OperandAddress] = CompileParts(frame.Name, visual);
            }
        }
        return new EnemyProjectileSpritemapCatalog(compiled, compiledPrograms);
    }

    /// <summary>Identifies Alcoon fireball presentation operands omitted by the pre-Alcoon legacy schema.</summary>
    private static bool IsAlcoonFireballOperand(ushort operandAddress)
    {
        return AlcoonFireballFrameDefinitions.TryFrame(operandAddress, out _);
    }

    /// <summary>Validates ordered editable sprite parts against OBJ bounds and packs X, Y, tile, palette, priority and flip words.</summary>
    private static EnemySpritemapPart[] CompileParts(string name, SpriteVisualPart[] visual)
    {
        if (visual.Length > EnemyProjectileSpritemapDefinitions.MaximumParts)
            throw new InvalidDataException(
                $"Enemy-projectile composition {name} is oversized.");
        var parts = new EnemySpritemapPart[visual.Length];
        for (int index = 0; index < visual.Length; index++)
        {
            SpriteVisualPart? part = visual[index];
            if (part is null || part.OffsetX is < -256 or > 255 ||
                part.OffsetY is < -128 or > 127 || part.Size is not (8 or 16) ||
                part.Priority is < 0 or > 3 || part.Palette is null or < 0 or > 7 ||
                part.TileColumn is < 0 or >= EnemyProjectileSpritemapDefinitions.TileColumns ||
                part.TileRow is < 0 or >= EnemyProjectileSpritemapDefinitions.TileRows)
                throw new InvalidDataException(
                    $"Enemy-projectile composition {name} part {index} is invalid.");
            SnesTileFlipFlags flips =
                (part.FlipX ? SnesTileFlipFlags.Horizontal : 0) |
                (part.FlipY ? SnesTileFlipFlags.Vertical : 0);
            parts[index] = new EnemySpritemapPart(
                SnesSpritemapXWord.Create(part.OffsetX, part.Size == 16),
                SnesSpritemapYByte.Create(part.OffsetY),
                SnesObjAttributeWord.Create(
                    part.TileRow * EnemyProjectileSpritemapDefinitions.TileColumns +
                        part.TileColumn,
                    part.Palette.Value, part.Priority, flips));
        }
        return parts;
    }

    /// <summary>Writes camel-case JSON only after round-trip loading validates the current schema and OAM bounds.</summary>
    internal static byte[] Write(EnemyProjectileSpritemapDocument document)
    {
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(document, Options);
        _ = Load(new MemoryStream(json, writable: false));
        return json;
    }

    /// <summary>Strict editable composition JSON policy, rejecting unknown fields while retaining readable output.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}
