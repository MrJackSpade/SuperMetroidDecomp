using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies.Alcoon;
using SuperMetroid.Core.Enemies.FuneNamihe;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Cartridge visual identities translated for bank-$8D projectile drawing.</summary>
public static class EnemyProjectileSpritemapDefinitions
{
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
        if (operandAddress < (ushort)BombTorizoDroolProgram.NoDelay) return BlankSpritemap;
        int frame = operandAddress < (ushort)BombTorizoDroolProgram.FloorImpact ? 0 :
            1 + (operandAddress - ((ushort)BombTorizoDroolProgram.FloorImpact + 4)) / 4;
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
