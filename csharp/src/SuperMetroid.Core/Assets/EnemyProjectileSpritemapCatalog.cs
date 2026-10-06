using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

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

    private readonly Dictionary<ushort, EnemySpritemapPart[]> frames;
    private readonly Dictionary<ushort, EnemySpritemapPart[]> programFrames;

    private EnemyProjectileSpritemapCatalog(Dictionary<ushort, EnemySpritemapPart[]> frames,
        Dictionary<ushort, EnemySpritemapPart[]> programFrames)
    {
        this.frames = frames;
        this.programFrames = programFrames;
    }

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

    public static EnemyProjectileSpritemapCatalog Load(Stream json,
        EnemyProjectileSpritemapCatalog? stock = null)
    {
        ArgumentNullException.ThrowIfNull(json);
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
        bool legacyOverride = document.Version is >= 1 and < EnemyProjectileSpritemapDefinitions.Version &&
            stock is not null;
        int expectedFrames = document.Version == EnemyProjectileSpritemapDefinitions.CeresOnlyVersion &&
            legacyOverride
            ? EnemyProjectileSpritemapDefinitions.LegacyFrameCount
            : EnemyProjectileSpritemapDefinitions.Frames.Length;
        if ((!legacyOverride && document.Version != EnemyProjectileSpritemapDefinitions.Version) ||
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
        if (document.Version >= EnemyProjectileSpritemapDefinitions.FirstProgramFrameVersion)
        {
            EnemyProjectilePresentationFrameDefinition[] definitions = document.Version switch
            {
                EnemyProjectileSpritemapDefinitions.FirstProgramFrameVersion =>
                    EnemyProjectileInstructionMechanicsDefinitions.VisualFrames.ToArray(),
                EnemyProjectileSpritemapDefinitions.PreAlcoonVersion =>
                    EnemyProjectilePresentationFrameDefinitions.PreGoldenTorizo.ToArray()
                        .Where(frame => !IsAlcoonFireballOperand(frame.OperandAddress))
                        .ToArray(),
                EnemyProjectileSpritemapDefinitions.PreGoldenTorizoVersion =>
                    EnemyProjectilePresentationFrameDefinitions.PreGoldenTorizo.ToArray(),
                EnemyProjectileSpritemapDefinitions.PreGoldenTorizoEggVersion =>
                    EnemyProjectilePresentationFrameDefinitions.PreGoldenTorizoEgg.ToArray(),
                EnemyProjectileSpritemapDefinitions.PreTorizoEffectsVersion =>
                    EnemyProjectilePresentationFrameDefinitions.PreTorizoEffects.ToArray(),
                EnemyProjectileSpritemapDefinitions.PreGenericEnemyDeathVersion =>
                    EnemyProjectilePresentationFrameDefinitions.PreGenericEnemyDeath.ToArray(),
                EnemyProjectileSpritemapDefinitions.PreEnvironmentAndAttackVersion =>
                    EnemyProjectilePresentationFrameDefinitions.PreEnvironmentAndAttack.ToArray(),
                EnemyProjectileSpritemapDefinitions.PreMotherBrainAndStatueVersion =>
                    EnemyProjectilePresentationFrameDefinitions.PreMotherBrainAndStatue.ToArray(),
                EnemyProjectileSpritemapDefinitions.PreWorkRobotVersion =>
                    EnemyProjectilePresentationFrameDefinitions.PreWorkRobot.ToArray(),
                EnemyProjectileSpritemapDefinitions.PrePolypRockVersion =>
                    EnemyProjectilePresentationFrameDefinitions.PrePolypRock.ToArray(),
                EnemyProjectileSpritemapDefinitions.Version =>
                    EnemyProjectilePresentationFrameDefinitions.All.ToArray(),
                _ => throw new InvalidDataException(
                    $"Enemy-projectile program-frame version {document.Version} is unsupported."),
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

    private static bool IsAlcoonFireballOperand(ushort operandAddress)
    {
        for (int index = 0;
             index < AlcoonFireballInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            if (operandAddress == AlcoonFireballInstructionProgramDefinitions
                    .PresentationWordAddress(index))
                return true;
        }
        return false;
    }

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
                unchecked((byte)(sbyte)part.OffsetY),
                SnesObjAttributeWord.Create(
                    part.TileRow * EnemyProjectileSpritemapDefinitions.TileColumns +
                        part.TileColumn,
                    part.Palette.Value, part.Priority, flips));
        }
        return parts;
    }

    internal static byte[] Write(EnemyProjectileSpritemapDocument document)
    {
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(document, Options);
        _ = Load(new MemoryStream(json, writable: false));
        return json;
    }

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}

public sealed record EnemyProjectileSpritemapDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
    public Dictionary<string, SpriteVisualPart[]>? ProgramFrames { get; init; }
}

/// <summary>Cartridge visual identities translated for bank-$8D projectile drawing.</summary>
public static class EnemyProjectileSpritemapDefinitions
{
    /// <summary>Botwoon body/tail maps start at $8D:B62E/$B72A and spit at
    /// $8D:B8B4. Each visible map has one OAM entry (seven bytes); hidden uses
    /// the zero-entry record $8D:B8B2. Physical body slots include the unused fourth.</summary>
    internal static ushort BotwoonProjectileFrameAt(ushort operandAddress)
    {
        if (!BotwoonProjectileInstructionProgramDefinitions.IsPresentationWord(operandAddress))
            throw new InvalidDataException($"Botwoon projectile visual operand $86:{operandAddress:X4} is not compiled.");
        if (operandAddress >= BotwoonProjectileInstructionProgramDefinitions.Spit)
            return (ushort)(0xb8b4 + 7 * ((operandAddress - BotwoonProjectileInstructionProgramDefinitions.Spit - 2) / 4));
        if (operandAddress == BotwoonProjectileInstructionProgramDefinitions.Hidden + 2) return 0xb8b2;
        if (operandAddress >= BotwoonProjectileInstructionProgramDefinitions.TailUpFacingRight)
            return (ushort)(0xb72a + 7 * ((operandAddress - BotwoonProjectileInstructionProgramDefinitions.TailUpFacingRight - 2) / 6));
        int relative = operandAddress - BotwoonProjectileInstructionProgramDefinitions.BodyUpLeft;
        return (ushort)(0xb62e + 7 * (4 * (relative / 20) + (relative % 20) / 4));
    }
    /// <summary>Sixteen consecutive single-entry maps at $8D:8DFB. Each fragment
    /// selects the same seven-byte record for its waiting and falling poses.</summary>
    internal static ushort BombTorizoStatueFrameAt(ushort operandAddress)
    {
        if (!BombTorizoStatueInstructionProgramDefinitions.IsPresentationWord(operandAddress))
            throw new InvalidDataException($"Bomb Torizo statue visual operand $86:{operandAddress:X4} is not compiled.");
        int program = (operandAddress - BombTorizoStatueInstructionProgramDefinitions.FirstProgram) /
            BombTorizoStatueInstructionProgramDefinitions.ProgramStride;
        return (ushort)(0x8dfb + 7 * program);
    }
    /// <summary>Drool delays use the blank map; falling uses $8D:8C54 and floor
    /// impact advances through the next three single-entry maps (seven bytes each).</summary>
    internal static ushort BombTorizoDroolFrameAt(ushort operandAddress)
    {
        if (!BombTorizoDroolInstructionProgramDefinitions.IsPresentationWord(operandAddress))
            throw new InvalidDataException($"Bomb Torizo drool visual operand $86:{operandAddress:X4} is not compiled.");
        if (operandAddress < BombTorizoDroolInstructionProgramDefinitions.NoDelay) return BlankSpritemap;
        int frame = operandAddress < BombTorizoDroolInstructionProgramDefinitions.FloorImpact ? 0 :
            1 + (operandAddress - (BombTorizoDroolInstructionProgramDefinitions.FloorImpact + 4)) / 4;
        return (ushort)(0x8c54 + 7 * frame);
    }
    /// <summary>Four consecutive fireball maps at $8D:8404, each with a
    /// two-byte count and one five-byte OAM record, selected in animation order.</summary>
    internal static ushort AlcoonFireballFrameAt(ushort operandAddress)
    {
        if (!AlcoonFireballInstructionProgramDefinitions.IsPresentationWord(operandAddress))
            throw new InvalidDataException(
                $"Alcoon fireball visual operand $86:{operandAddress:X4} is not compiled.");
        int frame = (operandAddress - (AlcoonFireballInstructionProgramDefinitions.Initial + 2)) / 4;
        return (ushort)(0x8404 + 7 * frame);
    }
    /// <summary>Three fireball frames per facing at $8D:AAB9, each with a
    /// two-byte count and one five-byte OAM record; right-facing maps follow left.</summary>
    internal static ushort FuneNamiheFireballFrameAt(ushort operandAddress)
    {
        if (!FuneNamiheFireballInstructionProgramDefinitions.IsPresentationWord(operandAddress))
            throw new InvalidDataException(
                $"Fune/Namihe fireball visual operand $86:{operandAddress:X4} is not compiled.");
        int offset = operandAddress - (FuneNamiheFireballInstructionProgramDefinitions.Left + 2);
        return (ushort)(0xaab9 + 7 * (3 * (offset / 16) + (offset % 16) / 4));
    }
    /// <summary>
    /// EnemyProjSpritemaps_Blank_Default at $8D:8000; the native count is zero.
    /// Newly initialized bank-$86 slots select it before their first timed frame.
    /// </summary>
    public const ushort BlankSpritemap = 0x8000;

    public const int Version = 13;
    /// <summary>Schema before the single-frame Polyp lava-rock composition was installed.</summary>
    public const int PrePolypRockVersion = 12;
    /// <summary>Schema 11: all earlier projectile families, before Work Robot laser bindings.</summary>
    public const int PreWorkRobotVersion = 11;
    public const int PreMotherBrainAndStatueVersion = 10;
    public const int PreEnvironmentAndAttackVersion = 9;
    public const int CeresOnlyVersion = 1;
    public const int FirstProgramFrameVersion = 3;
    public const int PreGenericEnemyDeathVersion = 8;
    public const int PreTorizoEffectsVersion = 7;
    public const int PreGoldenTorizoEggVersion = 6;
    public const int PreGoldenTorizoVersion = 5;
    public const int PreAlcoonVersion = 4;
    public const string FileName = "enemy-projectile-compositions.json";
    public const int LegacyFrameCount = 3;
    public const int MaximumParts = 128;
    public const int TileColumns = 16;
    public const int TileRows = 64;

    /// <summary>Ceres arrival frames and Skree/Metaree debris selected by their bank-$86 programs.</summary>
    internal static readonly (ushort Pointer, string Name)[] Frames =
    [
        (0xb1ba, "ceres_elevator_pad_0"),
        (0xb1d0, "ceres_elevator_pad_1"),
        (0x846d, "ceres_elevator_platform"),
        (SkreeMetareeParticleVisualDefinitions.SkreeComposition, "skree_debris"),
        (SkreeMetareeParticleVisualDefinitions.MetareeComposition, "metaree_debris"),
    ];
}
