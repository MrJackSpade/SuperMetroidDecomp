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

    /// <summary>Compiled OAM parts keyed by the native bank-$8D spritemap pointer.</summary>
    private readonly Dictionary<ushort, EnemySpritemapPart[]> frames;
    /// <summary>Compiled OAM parts keyed by the bank-$86 operand address that selects a timed visual frame.</summary>
    private readonly Dictionary<ushort, EnemySpritemapPart[]> programFrames;

    /// <summary>Creates a catalog from already compiled pointer-keyed compositions and program frames.</summary>
    /// <param name="frames">Named composition parts indexed by native bank-$8D spritemap pointer.</param>
    /// <param name="programFrames">Timed visual parts indexed by their bank-$86 operand addresses.</param>
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

    /// <summary>Determines whether an operand belongs to the Alcoon fireball visual-frame program.</summary>
    /// <param name="operandAddress">Bank-$86 address of a timed program's visual operand.</param>
    /// <returns><see langword="true"/> when the address identifies a compiled Alcoon fireball frame.</returns>
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

    /// <summary>Validates editable sprite-part geometry and packs each part into hardware OAM words.</summary>
    /// <param name="name">Composition identity included in validation errors.</param>
    /// <param name="visual">Ordered authoring parts to convert without changing their draw order.</param>
    /// <returns>Packed spritemap parts ready for projectile rendering.</returns>
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

    /// <summary>Serializes a projectile-presentation document and verifies that the emitted JSON can be loaded.</summary>
    /// <param name="document">Presentation schema to serialize, excluding executable timing and gameplay data.</param>
    /// <returns>Indented UTF-8 JSON bytes for the editable projectile composition file.</returns>
    internal static byte[] Write(EnemyProjectileSpritemapDocument document)
    {
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(document, Options);
        _ = Load(new MemoryStream(json, writable: false));
        return json;
    }

    /// <summary>Serializer policy matching the catalog's camel-case JSON schema and rejecting unknown fields.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}

/// <summary>Presentation-only enemy-projectile JSON schema: named ordered OAM compositions and program-frame bindings, with no executable instruction or gameplay fields.</summary>
public sealed record EnemyProjectileSpritemapDocument
{
    /// <summary>Schema generation determining the required composition and program-frame name sets; older generations require a current-stock fallback at load time.</summary>
    public required int Version { get; init; }
    /// <summary>Named bank-$8D Ceres elevator and Skree/Metaree debris compositions, each represented by ordered sprite parts; version 1 contains only the three Ceres frames.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
    /// <summary>Named visual frames keyed by the schema's bank-$86 spritemap-operand identities, required from version 3 onward; editing these parts does not replace the timed instruction program.</summary>
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

    /// <summary>Current schema generation 13, including the single-frame Polyp lava rock and all earlier projectile presentation bindings.</summary>
    public const int Version = 13;
    /// <summary>Schema before the single-frame Polyp lava-rock composition was installed.</summary>
    public const int PrePolypRockVersion = 12;
    /// <summary>Schema 11: all earlier projectile families, before Work Robot laser bindings.</summary>
    public const int PreWorkRobotVersion = 11;
    /// <summary>Legacy schema 10 containing environment/attack effects but preceding Mother Brain and Bomb Torizo statue program-frame bindings.</summary>
    public const int PreMotherBrainAndStatueVersion = 10;
    /// <summary>Legacy schema 9 containing generic enemy death/pickup frames but preceding the environment and additional attack-effect bindings.</summary>
    public const int PreEnvironmentAndAttackVersion = 9;
    /// <summary>Initial schema generation containing only the two Ceres elevator-pad compositions and elevator platform, without debris or timed-program bindings.</summary>
    public const int CeresOnlyVersion = 1;
    /// <summary>First schema generation requiring named visual frames for the translated shared projectile instruction mechanics.</summary>
    public const int FirstProgramFrameVersion = 3;
    /// <summary>Legacy schema 8 including Torizo effects but preceding generic enemy death and pickup program frames.</summary>
    public const int PreGenericEnemyDeathVersion = 8;
    /// <summary>Legacy schema 7 including Golden Torizo eggs but preceding the additional Torizo drool, swipe, sonic-boom, dust, explosion, and orb effects.</summary>
    public const int PreTorizoEffectsVersion = 7;
    /// <summary>Legacy schema 6 including Golden Torizo super missiles and eye beams but preceding its egg program frames.</summary>
    public const int PreGoldenTorizoEggVersion = 6;
    /// <summary>Legacy schema 5 including Alcoon fireballs but preceding Golden Torizo super-missile and eye-beam program frames.</summary>
    public const int PreGoldenTorizoVersion = 5;
    /// <summary>Legacy schema 4's translated family set before Alcoon fireball bindings; loading omits those operands from the later pre-Golden-Torizo set.</summary>
    public const int PreAlcoonVersion = 4;
    /// <summary>Installed editable JSON filename for projectile OAM compositions and presentation-only timed-program frames.</summary>
    public const string FileName = "enemy-projectile-compositions.json";
    /// <summary>Three bank-$8D compositions in the version-1 Ceres-only catalog, retained when merging that legacy override with current stock.</summary>
    public const int LegacyFrameCount = 3;
    /// <summary>Maximum ordered OBJ parts admitted per composition, matching the SNES's 128 physical sprite slots; empty compositions are valid.</summary>
    public const int MaximumParts = 128;
    /// <summary>Sixteen 8-pixel character columns per OBJ atlas row; row times 16 plus column forms the packed character index.</summary>
    public const int TileColumns = 16;
    /// <summary>Schema-level bound of 64 atlas rows; the selected row/column character number must also fit the nine-bit hardware OBJ field during packed-word compilation.</summary>
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
