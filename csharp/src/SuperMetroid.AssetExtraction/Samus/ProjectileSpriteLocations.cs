using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using static SuperMetroid.AssetExtraction.CartridgeTables.ProjectileSpriteCartridgeLayout;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Where the cartridge stores each Samus projectile and charge-flare composition: its bank-$93 layout of composition
/// groups, extraction's knowledge, which the game never reads.
/// </summary>
internal static class ProjectileSpriteLocations
{

    /// <summary>The bank-$93 address of the composition each timed projectile record draws, by record.</summary>
    internal static ushort[] RecordSprites(ISnesAddressSpace bus)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        SamusProjectileListLayout lists = SamusProjectileListEntries.Read(cartridge);
        return [.. Enumerable.Range(0, SamusProjectileRecordDefinitions.Records.Length).Select(record => RomDataReader.ReadWordFixedBank(cartridge,
            SamusSourceDefinitions.ProjectileDataBank.Value | unchecked((ushort)(lists.RecordPosition(record) + 2))))];
    }

    /// <summary>The bank-$93 address of each projectile composition.</summary>
    internal static Dictionary<ProjectileSpriteId, ushort> Projectiles() =>
        ProjectileSpriteDefinitions.All.Select((id, place) => KeyValuePair.Create(id, PointerAt(place))).ToDictionary();

    /// <summary>The bank-$93 address of each charge-flare composition: four main flares, then two runs of twelve seventeen-byte sparks.</summary>
    internal static Dictionary<ProjectileSpriteId, ushort> Flares() =>
        ChargeFlareSpriteDefinitions.All.Select((id, place) => KeyValuePair.Create(id, place < 4
            ? (ushort)(FirstMainFlare + place * 7)
            : (ushort)((place - 4 < 12 ? FirstRightSpark : FirstLeftSpark) + (place - 4) % 12 * 17))).ToDictionary();

    /// <summary>Native bank93 compositions serialize a two-byte part count then five bytes per OAM part.</summary>
    private static int RecordBytes(int parts) => sizeof(ushort) + 5 * parts;
    /// <summary>Finds the selected beam-explosion composition within its variable-size native group.</summary>
    private static ushort BeamExplosionPointer(int phase) => (ushort)(BeamExplosionStart +
        Math.Min(phase, 2) * RecordBytes(1) + Math.Max(0, phase - 2) * RecordBytes(4));
    /// <summary>Finds the selected missile-explosion composition after its initial center tile.</summary>
    private static ushort MissileExplosionPointer(int phase) => (ushort)(MissileExplosionStart +
        (phase == 0 ? 0 : RecordBytes(1) + (phase - 1) * RecordBytes(4)));
    /// <summary>Maps a compass pose to its missile-group composition after preceding variable-size records.</summary>
    private static ushort MissilePointer(ushort start, int pose) => (ushort)(start +
        pose / 2 * (RecordBytes(2) + RecordBytes(3)) + pose % 2 * RecordBytes(2));
    /// <summary>Calculates the sorted bank-$93 composition pointer at one catalog index from native group layouts.</summary>
    internal static ushort PointerAt(int index)
    {
        if ((uint)index >= 417) throw new ArgumentOutOfRangeException(nameof(index));
        if (index < 1) return (ushort)(NothingStart + index * RecordBytes(0));
        index -= 1;
        if (index < 8) return (ushort)(PowerStart + index * RecordBytes(1));
        index -= 8;
        if (index < 4) return (ushort)(PlasmaStart + index / 2 * (RecordBytes(4) + RecordBytes(6)) + index % 2 * RecordBytes(4));
        index -= 4;
        if (index < 6) return MissileExplosionPointer(index);
        index -= 6;
        if (index < 5) return (ushort)(BombExplosionStart + index * RecordBytes(4));
        index -= 5;
        if (index < 6) return SuperMissileExplosionPointer(index);
        index -= 6;
        if (index < 3) return (ushort)(PowerBombStart + index * RecordBytes(1));
        index -= 3;
        if (index < 6) return BeamExplosionPointer(index);
        index -= 6;
        if (index < 4) return (ushort)(BombStart + index * RecordBytes(1));
        index -= 4;
        if (index < 8) return MissilePointer(MissileStart, index);
        index -= 8;
        if (index < 8) return MissilePointer(SuperMissileStart, index);
        index -= 8;
        if (index < 33) return (ushort)(WaveStart + index * RecordBytes(1));
        index -= 33;
        if (index < 34) return ChargedWavePointer(ChargedWaveStart, index);
        index -= 34;
        if (index < 34) return ChargedWavePointer(ChargedIceWaveStart, index);
        index -= 34;
        if (index < 60) return PlasmaWavePointer(index);
        index -= 60;
        if (index < 47) return SpazerPointer(index);
        index -= 47;
        if (index < 72) return ChargedSpazerPointer(index);
        index -= 72;
        if (index < 16) return (ushort)(ChargedPowerStart + index * RecordBytes(4));
        index -= 16;
        if (index < 4) return (ushort)(ChargedIceStart + index * RecordBytes(4));
        index -= 4;
        if (index < 4) return (ushort)(IceStart + index * RecordBytes(1));
        index -= 4;
        if (index < 22) return SpazerStartupPointer(index);
        index -= 22;
        return PlasmaStartupPointer(index);
    }
    /// <summary>Skips unselected four-part layouts while indexing charged Wave's paired-lobe records.</summary>
    private static ushort ChargedWavePointer(ushort start, int index)
    {
        // B368/B37E and BA8E/BAA4 each contain two unselected four-part
        // compositions between the first24 paired-lobe phases and the last8.
        return (ushort)(start + Math.Min(index, 2) * RecordBytes(4) +
            Math.Max(0, index - 2) * RecordBytes(8) + (index >= 26 ? 2 * RecordBytes(4) : 0));
    }
    /// <summary>Measures one Spazer seed plus its five three-lane spread compositions.</summary>
    private static int SpreadGroupBytes(int seedParts) => RecordBytes(seedParts) + 5 * RecordBytes(3 * seedParts);
    /// <summary>Locates a selected Spazer spread phase after its seed record.</summary>
    private static int SpreadPhaseOffset(int seedParts, int phase) =>
        phase == 0 ? 0 : RecordBytes(seedParts) + (phase - 1) * RecordBytes(3 * seedParts);
    /// <summary>Calculates an ordinary Spazer composition pointer across its direction groups and unreferenced gap.</summary>
    private static ushort SpazerPointer(int index)
    {
        // D4B4 is an unreferenced twelve-part spread pose at the end of the
        // third large-beam orientation. It occupies native layout space only.
        int physical = index + (index >= 17 ? 1 : 0);
        int group = physical / 6, phase = physical % 6;
        int seedParts = group < 4 ? 4 : 2;
        return (ushort)(SpazerStart + Math.Min(group, 4) * SpreadGroupBytes(4) +
            Math.Max(0, group - 4) * SpreadGroupBytes(2) + SpreadPhaseOffset(seedParts, phase));
    }
    /// <summary>Calculates one charged-Spazer pointer through axial and diagonal native group sizes.</summary>
    private static ushort ChargedSpazerPointer(int index)
    {
        int group = index / 6, phase = index % 6;
        int seedParts = group is >= 2 and < 8 ? 6 : 4;
        return (ushort)(ChargedSpazerStart + Math.Min(group, 2) * SpreadGroupBytes(4) +
            Math.Clamp(group - 2, 0, 6) * SpreadGroupBytes(6) + Math.Max(0, group - 8) * SpreadGroupBytes(4) +
            SpreadPhaseOffset(seedParts, phase));
    }
    /// <summary>Finds a selected startup pose while accounting for unselected records in earlier orientations.</summary>
    private static ushort SpazerStartupPointer(int index)
    {
        // The fourth orientation selects only its first two poses. Its other
        // native records still occupy layout space before the fifth group.
        int physical = index + (index >= 14 ? 2 : 0);
        int group = physical / 4, phase = physical % 4;
        int strips = phase < 2 ? phase : 4;
        int prefix = strips * sizeof(ushort) + 5 * strips * (strips + 1) / 2;
        if (phase == 3) prefix += RecordBytes(2);
        int groupBytes = 7 * sizeof(ushort) + 5 * (4 * 5 / 2 + 2 * (3 * 4 / 2));
        return (ushort)(SpazerStartupStart + group * groupBytes + prefix);
    }
    /// <summary>Steps over the explosion's mixed four-, eight- and twelve-part records to the requested stage.</summary>
    private static ushort SuperMissileExplosionPointer(int phase) => (ushort)(SuperMissileExplosionStart +
        Math.Min(phase, 3) * RecordBytes(4) + (phase >= 4 ? RecordBytes(4 + 4 * 2) : 0) +
        (phase >= 5 ? RecordBytes(4 * 2) : 0));
    /// <summary>Maps PlasmaWave composition-group order to its named core orientation and alternate.</summary>
    private static PlasmaWaveShape PlasmaWaveShapeAt(int group) => group switch
    {
        0 => PlasmaWaveShape.HorizontalShort,
        1 => PlasmaWaveShape.HorizontalLong,
        2 => PlasmaWaveShape.DownRightShort,
        3 => PlasmaWaveShape.DownRightLong,
        4 => PlasmaWaveShape.VerticalShort,
        5 => PlasmaWaveShape.VerticalLong,
        6 => PlasmaWaveShape.HorizontalAlternate,
        7 => PlasmaWaveShape.DownRightAlternate,
        8 => PlasmaWaveShape.VerticalAlternate,
        9 => PlasmaWaveShape.DownLeftAlternate,
        10 => PlasmaWaveShape.DownLeftShort,
        11 => PlasmaWaveShape.DownLeftLong,
        _ => throw new ArgumentOutOfRangeException(nameof(group)),
    };
    /// <summary>Resolves a flattened PlasmaWave frame index to a shape and spread phase.</summary>
    private static ushort PlasmaWavePointer(int index) => PlasmaWavePointer(PlasmaWaveShapeAt(index / 5), index % 5);
    /// <summary>Locates one selected PlasmaWave core or lobe composition within its variable-size shape group.</summary>
    private static ushort PlasmaWavePointer(PlasmaWaveShape shape, int phase)
    {
        // Core part counts describe the native chosen footprints. Their full
        // independent OAM design remains required under ProjectileSpriteCatalog.frames.
        (int start, int coreParts) = shape switch
        {
            PlasmaWaveShape.HorizontalShort => new PlasmaWaveParts(PlasmaWaveHorizontalShort, 4),
            PlasmaWaveShape.HorizontalLong => new PlasmaWaveParts(PlasmaWaveHorizontalLong, 7),
            PlasmaWaveShape.DownRightShort => new PlasmaWaveParts(PlasmaWaveDownRightShort, 6),
            PlasmaWaveShape.DownRightLong => new PlasmaWaveParts(PlasmaWaveDownRightLong, 10),
            PlasmaWaveShape.VerticalShort => new PlasmaWaveParts(PlasmaWaveVerticalShort, 4),
            PlasmaWaveShape.VerticalLong => new PlasmaWaveParts(PlasmaWaveVerticalLong, 7),
            PlasmaWaveShape.HorizontalAlternate => new PlasmaWaveParts(PlasmaWaveHorizontalAlternate, 7),
            PlasmaWaveShape.DownRightAlternate => new PlasmaWaveParts(PlasmaWaveDownRightAlternate, 12),
            PlasmaWaveShape.VerticalAlternate => new PlasmaWaveParts(PlasmaWaveVerticalAlternate, 7),
            PlasmaWaveShape.DownLeftAlternate => new PlasmaWaveParts(PlasmaWaveDownLeftAlternate, 12),
            PlasmaWaveShape.DownLeftShort => new PlasmaWaveParts(PlasmaWaveDownLeftShort, 6),
            PlasmaWaveShape.DownLeftLong => new PlasmaWaveParts(PlasmaWaveDownLeftLong, 10),
            _ => throw new ArgumentOutOfRangeException(nameof(shape)),
        };
        return (ushort)(start + (phase == 0 ? 0 : RecordBytes(coreParts) + (phase - 1) * RecordBytes(2 * coreParts)));
    }
    /// <summary>Locates one charged Plasma startup composition despite intervening unselected physical growth records.</summary>
    private static ushort PlasmaStartupPointer(int index)
    {
        int group = index / 4, localGroup = group % 4;
        bool diagonal = localGroup >= 2;
        // Native physical groups include every axial length1..7 or paired
        // diagonal length2..10. Unselected intermediate records still occupy space.
        int axialBytes = 7 * sizeof(ushort) + 5 * (7 * 8 / 2);
        int diagonalBytes = 5 * sizeof(ushort) + 5 * 2 * (5 * 6 / 2);
        int groupOffset = group / 4 * (2 * axialBytes + 2 * diagonalBytes) +
            Math.Min(localGroup, 2) * axialBytes + Math.Max(0, localGroup - 2) * diagonalBytes;
        PlasmaGrowthStage stage = (index % 4) switch
        {
            0 => PlasmaGrowthStage.Core,
            1 => PlasmaGrowthStage.Short,
            2 => PlasmaGrowthStage.Long,
            3 => PlasmaGrowthStage.Full,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
        int selectedLength = new PlasmaGrowthShape(stage, diagonal) switch
        {
            (PlasmaGrowthStage.Core, false) => 1,
            (PlasmaGrowthStage.Short, false) => 3,
            (PlasmaGrowthStage.Long, false) => 6,
            (PlasmaGrowthStage.Full, false) => 7,
            (PlasmaGrowthStage.Core, true) => 1,
            (PlasmaGrowthStage.Short, true) => 2,
            (PlasmaGrowthStage.Long, true) => 4,
            (PlasmaGrowthStage.Full, true) => 5,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
        int preceding = selectedLength - 1;
        int offset = preceding * sizeof(ushort) + 5 * (diagonal ? 2 : 1) * preceding * (preceding + 1) / 2;
        return (ushort)(PlasmaStartupStart + groupOffset + offset);
    }
}
