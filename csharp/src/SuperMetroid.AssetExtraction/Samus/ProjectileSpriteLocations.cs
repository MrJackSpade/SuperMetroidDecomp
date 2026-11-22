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
internal static partial class ProjectileSpriteLocations
{
    /// <summary>
    /// The bank-$93 projectile composition groups in storage order, each with where its records lie: from the group's start,
    /// past the records of the sizes before each, and past the records no timed record selects that some groups hold.
    /// </summary>
    private static IReadOnlyList<ProjectileSpriteRecordGroup<ushort>> Groups { get; } =
    [
        new(ProjectileSpriteDefinitions.PowerShots, place => (ushort)(PowerStart + place * RecordBytes(1))),
        new(ProjectileSpriteDefinitions.PlasmaShots, place => (ushort)(PlasmaStart + place / 2 * (RecordBytes(4) + RecordBytes(6)) + place % 2 * RecordBytes(4))),
        new(ProjectileSpriteDefinitions.MissileExplosions, MissileExplosionPointer),
        new(ProjectileSpriteDefinitions.BombExplosions, place => (ushort)(BombExplosionStart + place * RecordBytes(4))),
        new(ProjectileSpriteDefinitions.SuperMissileExplosions, SuperMissileExplosionPointer),
        new(ProjectileSpriteDefinitions.PowerBombs, place => (ushort)(PowerBombStart + place * RecordBytes(1))),
        new(ProjectileSpriteDefinitions.BeamExplosions, BeamExplosionPointer),
        new(ProjectileSpriteDefinitions.Bombs, place => (ushort)(BombStart + place * RecordBytes(1))),
        new(ProjectileSpriteDefinitions.Missiles, place => MissilePointer(MissileStart, place)),
        new(ProjectileSpriteDefinitions.SuperMissiles, place => MissilePointer(SuperMissileStart, place)),
        new(ProjectileSpriteDefinitions.WaveShots, place => (ushort)(WaveStart + place * RecordBytes(1))),
        new(ProjectileSpriteDefinitions.ChargedWaveShots, place => ChargedWavePointer(ChargedWaveStart, place)),
        new(ProjectileSpriteDefinitions.ChargedIceWaveShots, place => ChargedWavePointer(ChargedIceWaveStart, place)),
        new(ProjectileSpriteDefinitions.PlasmaWaveShots, PlasmaWavePointer),
        new(ProjectileSpriteDefinitions.SpazerShots, SpazerPointer),
        new(ProjectileSpriteDefinitions.ChargedSpazerShots, ChargedSpazerPointer),
        new(ProjectileSpriteDefinitions.ChargedPowerShots, place => (ushort)(ChargedPowerStart + place * RecordBytes(4))),
        new(ProjectileSpriteDefinitions.ChargedIceShots, place => (ushort)(ChargedIceStart + place * RecordBytes(4))),
        new(ProjectileSpriteDefinitions.IceShots, place => (ushort)(IceStart + place * RecordBytes(1))),
        new(ProjectileSpriteDefinitions.SpazerStartups, SpazerStartupPointer),
        new(ProjectileSpriteDefinitions.PlasmaStartups, PlasmaStartupPointer),
    ];

    /// <summary>
    /// The bank-$93 charge-flare composition groups: the four main flares, one-part records one after another, then the
    /// right and the left sparks, each twelve three-part records, slow then fast.
    /// </summary>
    private static IReadOnlyList<ProjectileSpriteRecordGroup<ushort>> FlareGroups { get; } =
    [
        new([.. ChargeFlareSpriteDefinitions.All.Where(flare => flare.Group == ProjectileSpriteGroup.FlareMain)],
            place => (ushort)(FirstMainFlare + place * RecordBytes(1))),
        new([.. ChargeFlareSpriteDefinitions.All.Where(flare => flare.Group is ProjectileSpriteGroup.FlareSlowSparksRight or ProjectileSpriteGroup.FlareFastSparksRight)],
            place => (ushort)(FirstRightSpark + place * RecordBytes(3))),
        new([.. ChargeFlareSpriteDefinitions.All.Where(flare => flare.Group is ProjectileSpriteGroup.FlareSlowSparksLeft or ProjectileSpriteGroup.FlareFastSparksLeft)],
            place => (ushort)(FirstLeftSpark + place * RecordBytes(3))),
    ];

    /// <summary>The burst Samus's hit shows at an enemy it hurts (#1713): four four-part records, one after another from the first.</summary>
    private static ProjectileSpriteRecordGroup<SnesAddress> HitBurstGroup { get; } =
        new(HitBurstSpriteDefinitions.All, place => SamusSourceDefinitions.HitBurstSpritemaps.AddWithinBank(place * RecordBytes(4)));

    /// <summary>
    /// The composition each frame draws, by frame: every timed record that shows a frame stores the same composition (#2274),
    /// a record holding a byte-for-byte copy of a composition standing for that composition (#2295).
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// A record selects a bank-$93 address that holds no projectile composition, or two records that show one frame store
    /// different compositions.
    /// </exception>
    /// <returns>The frames that draw a composition, each with it; a frame whose records select the empty record draws none and is left out.</returns>
    internal static Dictionary<SamusProjectileFrameId, ProjectileSpriteId> RecordSprites(ISnesAddressSpace bus)
    {
        IImportCartridgeSource cartridge = bus;
        SamusProjectileListLayout lists = SamusProjectileListEntries.Read(cartridge);
        Dictionary<int, ProjectileSpriteId> stored = StoredProjectiles().ToDictionary(entry => entry.Key, entry => entry.Value);
        int AddressAt(int record) => RomDataReader.ReadWordFixedBank(cartridge,
            SamusSourceDefinitions.ProjectileDataBank.Value | unchecked((ushort)(record + 2)));
        string Selection(SamusProjectileFrameId frame, int record)
        {
            int address = AddressAt(record);
            if (address == NothingStart)
                return EmptySelection;
            return stored.TryGetValue(address, out ProjectileSpriteId sprite)
                ? sprite.Name
                : throw new InvalidDataException($"Timed projectile record {frame} at $93:{record:X4} selects ${address:X4}, which is no projectile composition.");
        }
        var pictured = new Dictionary<SamusProjectileFrameId, ProjectileSpriteId>();
        foreach (SamusProjectileFrameId frame in SamusProjectileFrameOrder.Frames)
        {
            string selected = Selection(frame, lists.RecordPosition(frame));
            foreach (int record in lists.RecordPositions(frame))
            {
                string other = Selection(frame, record);
                if (other != selected)
                    throw new InvalidDataException($"Timed projectile records showing {frame} store compositions {selected} and {other}.");
            }
            if (selected != EmptySelection)
                pictured.Add(frame, stored[AddressAt(lists.RecordPosition(frame))]);
        }
        return pictured;
    }

    /// <summary>How a record selecting the empty record, which draws nothing, is named in a mismatch.</summary>
    private const string EmptySelection = "the empty record";

    /// <summary>The bank-$93 address of each projectile composition: the first record that stores it.</summary>
    internal static Dictionary<ProjectileSpriteId, ushort> Projectiles() =>
        Groups.SelectMany(group => group.Records()).DistinctBy(record => record.Value)
            .ToDictionary(record => record.Value, record => record.Key);

    /// <summary>
    /// Every bank-$93 projectile composition record, in storage order, with the composition it holds: a record that stores a
    /// composition again, byte for byte, holds that composition (#2295).
    /// </summary>
    internal static IEnumerable<KeyValuePair<int, ProjectileSpriteId>> StoredProjectiles() =>
        Groups.SelectMany(group => group.Records()).Select(record => KeyValuePair.Create((int)record.Key, record.Value));

    /// <summary>The bank-$93 address of each charge-flare composition.</summary>
    internal static Dictionary<ProjectileSpriteId, ushort> Flares() =>
        FlareGroups.SelectMany(group => group.Records()).ToDictionary(record => record.Value, record => record.Key);

    /// <summary>Where each composition of the burst Samus's hit shows at an enemy it hurts lies (#1713).</summary>
    internal static Dictionary<ProjectileSpriteId, SnesAddress> HitBursts() =>
        HitBurstGroup.Records().ToDictionary(record => record.Value, record => record.Key);

    /// <summary>Native bank93 compositions serialize a two-byte part count then five bytes per OAM part.</summary>
    private static int RecordBytes(int parts) => sizeof(ushort) + 5 * parts;
    /// <summary>Finds the selected beam-explosion composition within its variable-size native group.</summary>
    private static ushort BeamExplosionPointer(int phase) => (ushort)(BeamExplosionStart +
        Math.Min(phase, 2) * RecordBytes(1) + Math.Max(0, phase - 2) * RecordBytes(4));
    /// <summary>Finds the selected missile-explosion composition after its initial center tile.</summary>
    private static ushort MissileExplosionPointer(int phase) => (ushort)(MissileExplosionStart +
        (phase == 0 ? 0 : RecordBytes(1) + (phase - 1) * RecordBytes(4)));
    /// <summary>Maps a compass pose to its missile-group composition after preceding variable-size records.</summary>
    private static ushort MissilePointer(int start, int pose) => (ushort)(start +
        pose / 2 * (RecordBytes(2) + RecordBytes(3)) + pose % 2 * RecordBytes(2));

    /// <summary>Skips unselected four-part layouts while indexing charged Wave's paired-lobe records.</summary>
    private static ushort ChargedWavePointer(int start, int index)
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
    /// <summary>Maps PlasmaWave composition-group order to its named core orientation and thickness.</summary>
    private static PlasmaWaveShape PlasmaWaveShapeAt(int group) => group switch
    {
        0 => PlasmaWaveShape.HorizontalShort,
        1 => PlasmaWaveShape.HorizontalLong,
        2 => PlasmaWaveShape.DownRightShort,
        3 => PlasmaWaveShape.DownRightLong,
        4 => PlasmaWaveShape.VerticalShort,
        5 => PlasmaWaveShape.VerticalLong,
        6 => PlasmaWaveShape.HorizontalThick,
        7 => PlasmaWaveShape.DownRightThick,
        8 => PlasmaWaveShape.VerticalThick,
        9 => PlasmaWaveShape.DownLeftThick,
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
        // independent OAM design remains required under InstalledProjectileSprites.frames.
        (int start, int coreParts) = shape switch
        {
            PlasmaWaveShape.HorizontalShort => new PlasmaWaveParts(PlasmaWaveHorizontalShort, 4),
            PlasmaWaveShape.HorizontalLong => new PlasmaWaveParts(PlasmaWaveHorizontalLong, 7),
            PlasmaWaveShape.DownRightShort => new PlasmaWaveParts(PlasmaWaveDownRightShort, 6),
            PlasmaWaveShape.DownRightLong => new PlasmaWaveParts(PlasmaWaveDownRightLong, 10),
            PlasmaWaveShape.VerticalShort => new PlasmaWaveParts(PlasmaWaveVerticalShort, 4),
            PlasmaWaveShape.VerticalLong => new PlasmaWaveParts(PlasmaWaveVerticalLong, 7),
            PlasmaWaveShape.HorizontalThick => new PlasmaWaveParts(PlasmaWaveHorizontalThick, 7),
            PlasmaWaveShape.DownRightThick => new PlasmaWaveParts(PlasmaWaveDownRightThick, 12),
            PlasmaWaveShape.VerticalThick => new PlasmaWaveParts(PlasmaWaveVerticalThick, 7),
            PlasmaWaveShape.DownLeftThick => new PlasmaWaveParts(PlasmaWaveDownLeftThick, 12),
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
