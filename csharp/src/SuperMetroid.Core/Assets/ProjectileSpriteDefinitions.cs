using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Stable identities for the spritemaps referenced by native timed projectile records.</summary>
public static class ProjectileSpriteDefinitions
{
    public const int Version = 1;
    public const string FileName = "projectile-compositions.json";
    public const int MaximumParts = 128;
    public const int TileColumns = 16, TileRows = 32;
    /// <summary>The417 timed-projectile identities in their original sorted order; selected OAM groups derive from two-byte headers and five-byte parts.</summary>
    public static PointerSequence NativePointers => default;
    public readonly struct PointerSequence : IReadOnlyList<ushort>
    {
        public int Count => 417;
        public int Length => Count;
        public ushort this[int index] => PointerAt(index);
        public IEnumerator<ushort> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>$93:A117: native Nothing projectile composition group.</summary>
    private const ushort NothingStart = 0xa117;
    /// <summary>$93:A24D: native Power projectile composition group.</summary>
    private const ushort PowerStart = 0xa24d;
    /// <summary>$93:A83E: native BombExplosion projectile composition group.</summary>
    private const ushort BombExplosionStart = 0xa83e;
    /// <summary>$93:AB97: native PowerBomb projectile composition group.</summary>
    private const ushort PowerBombStart = 0xab97;
    /// <summary>$93:AD45: native Bomb projectile composition group.</summary>
    private const ushort BombStart = 0xad45;
    /// <summary>$93:AD61: native Missile projectile composition group.</summary>
    private const ushort MissileStart = 0xad61;
    /// <summary>$93:ADD5: native SuperMissile projectile composition group.</summary>
    private const ushort SuperMissileStart = 0xadd5;
    /// <summary>$93:AE65: native Wave projectile composition group.</summary>
    private const ushort WaveStart = 0xae65;
    /// <summary>$93:EC3E: native ChargedPower projectile composition group.</summary>
    private const ushort ChargedPowerStart = 0xec3e;
    /// <summary>$93:ED9E: native ChargedIce projectile composition group.</summary>
    private const ushort ChargedIceStart = 0xed9e;
    /// <summary>$93:EDF6: native Ice projectile composition group.</summary>
    private const ushort IceStart = 0xedf6;
    /// <summary>Native bank93 compositions serialize a two-byte part count then five bytes per OAM part.</summary>
    private static int RecordBytes(int parts) => sizeof(ushort) + 5 * parts;
    private static ushort PointerAt(int index)
    {
        if ((uint)index >= 417) throw new IndexOutOfRangeException();
        if (index < 1) return (ushort)(NothingStart + index * RecordBytes(0));
        index -= 1;
        if (index < 8) return (ushort)(PowerStart + index * RecordBytes(1));
        index -= 8;
        if (index < 4) return (ushort)(PlasmaStart + index / 2 * (RecordBytes(4) + RecordBytes(6)) + index % 2 * RecordBytes(4));
        index -= 4;
        if (index < 6) return (ushort)(MissileExplosionStart + (index == 0 ? 0 : RecordBytes(1) + (index - 1) * RecordBytes(4)));
        index -= 6;
        if (index < 5) return (ushort)(BombExplosionStart + index * RecordBytes(4));
        index -= 5;
        if (index < 6) return SuperMissileExplosionPointer(index);
        index -= 6;
        if (index < 3) return (ushort)(PowerBombStart + index * RecordBytes(1));
        index -= 3;
        if (index < 6) return (ushort)(BeamExplosionStart + Math.Min(index, 2) * RecordBytes(1) + Math.Max(0, index - 2) * RecordBytes(4));
        index -= 6;
        if (index < 4) return (ushort)(BombStart + index * RecordBytes(1));
        index -= 4;
        if (index < 8) return (ushort)(MissileStart + index / 2 * (RecordBytes(2) + RecordBytes(3)) + index % 2 * RecordBytes(2));
        index -= 8;
        if (index < 8) return (ushort)(SuperMissileStart + index / 2 * (RecordBytes(2) + RecordBytes(3)) + index % 2 * RecordBytes(2));
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
    /// <summary>$93:AF4C/B672, ChargedWave_WaveSBA/ChargedIceWave: two four-quadrant core poses, then paired four-quadrant lobes.</summary>
    private const ushort ChargedWaveStart = 0xaf4c, ChargedIceWaveStart = 0xb672;
    /// <summary>$93:D10E, S_SI_SW_SIW: four orientations of four-part beams, then four of two-part beams; spread repeats each seed on three lanes.</summary>
    private const ushort SpazerStart = 0xd10e;
    /// <summary>$93:D8EE, Charged_S_SI_SW_SIW: two four-part axial groups, six six-part diagonal groups, then four four-part axial groups.</summary>
    private const ushort ChargedSpazerStart = 0xd8ee;
    private static ushort ChargedWavePointer(ushort start, int index)
    {
        // B368/B37E and BA8E/BAA4 each contain two unselected four-part
        // compositions between the first24 paired-lobe phases and the last8.
        return (ushort)(start + Math.Min(index, 2) * RecordBytes(4) +
            Math.Max(0, index - 2) * RecordBytes(8) + (index >= 26 ? 2 * RecordBytes(4) : 0));
    }
    private static int SpreadGroupBytes(int seedParts) => RecordBytes(seedParts) + 5 * RecordBytes(3 * seedParts);
    private static int SpreadPhaseOffset(int seedParts, int phase) =>
        phase == 0 ? 0 : RecordBytes(seedParts) + (phase - 1) * RecordBytes(3 * seedParts);
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
    private static ushort ChargedSpazerPointer(int index)
    {
        int group = index / 6, phase = index % 6;
        int seedParts = group is >= 2 and < 8 ? 6 : 4;
        return (ushort)(ChargedSpazerStart + Math.Min(group, 2) * SpreadGroupBytes(4) +
            Math.Clamp(group - 2, 0, 6) * SpreadGroupBytes(6) + Math.Max(0, group - 8) * SpreadGroupBytes(4) +
            SpreadPhaseOffset(seedParts, phase));
    }
    /// <summary>$93:A37D: Plasma/PlasmaIce axial four-tile strips alternate with six-part diagonal strips.</summary>
    private const ushort PlasmaStart = 0xa37d;
    /// <summary>$93:A7C9: missile explosion grows from one central tile to five four-quadrant poses.</summary>
    private const ushort MissileExplosionStart = 0xa7c9;
    /// <summary>$93:ABB3: beam explosion has two single-tile core poses followed by four four-quadrant poses.</summary>
    private const ushort BeamExplosionStart = 0xabb3;
    /// <summary>$93:EE12: six Spazer startup orientation groups serialize length1..4 strips then one/two/three two-tile pairs.</summary>
    private const ushort SpazerStartupStart = 0xee12;
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
    /// <summary>$93:AA84: SuperMissileExplosion starts with three four-quadrant cores, followed by core/spikes, a large-part ring, then ring/spikes.</summary>
    private const ushort SuperMissileExplosionStart = 0xaa84;
    private static ushort SuperMissileExplosionPointer(int phase) => (ushort)(SuperMissileExplosionStart +
        Math.Min(phase, 3) * RecordBytes(4) + (phase >= 4 ? RecordBytes(4 + 4 * 2) : 0) +
        (phase >= 5 ? RecordBytes(4 * 2) : 0));

    /// <summary>Native Charged_PW_PIW composition groups, identified by the actual directional instruction consumers at $93:8D4F/8D9B/8DE7/8E33 and $93:9C1B..9E3D.</summary>
    private enum PlasmaWaveShape
    {
        HorizontalShort, HorizontalLong, DownRightShort, DownRightLong,
        VerticalShort, VerticalLong, HorizontalAlternate, DownRightAlternate,
        VerticalAlternate, DownLeftAlternate, DownLeftShort, DownLeftLong,
    }
    /// <summary>$93:BC0A/BCC8: horizontal uncharged four-tile and charged seven-tile cores.</summary>
    private const ushort PlasmaWaveHorizontalShort = 0xbc0a, PlasmaWaveHorizontalLong = 0xbcc8;
    /// <summary>$93:BE0D/BF25: down-right/up-left diagonal six-part and ten-part cores.</summary>
    private const ushort PlasmaWaveDownRightShort = 0xbe0d, PlasmaWaveDownRightLong = 0xbf25;
    /// <summary>$93:C0F1/C1AF: vertical uncharged four-tile and charged seven-tile cores.</summary>
    private const ushort PlasmaWaveVerticalShort = 0xc0f1, PlasmaWaveVerticalLong = 0xc1af;
    /// <summary>$93:C3B2/C669: alternate charged horizontal seven-part and down-right twelve-part cores, after native unselected composition groups.</summary>
    private const ushort PlasmaWaveHorizontalAlternate = 0xc3b2, PlasmaWaveDownRightAlternate = 0xc669;
    /// <summary>$93:C94D/CC04: alternate charged vertical seven-part and down-left twelve-part cores, after native unselected composition groups.</summary>
    private const ushort PlasmaWaveVerticalAlternate = 0xc94d, PlasmaWaveDownLeftAlternate = 0xcc04;
    /// <summary>$93:CE2A/CF42: down-left/up-right diagonal six-part and ten-part cores.</summary>
    private const ushort PlasmaWaveDownLeftShort = 0xce2a, PlasmaWaveDownLeftLong = 0xcf42;
    private static ushort PlasmaWavePointer(int index)
    {
        // Core part counts describe the native chosen footprints. Their full
        // independent OAM design remains required under ProjectileSpriteCatalog.frames.
        (ushort start, int coreParts) = (PlasmaWaveShape)(index / 5) switch
        {
            PlasmaWaveShape.HorizontalShort => (PlasmaWaveHorizontalShort, 4),
            PlasmaWaveShape.HorizontalLong => (PlasmaWaveHorizontalLong, 7),
            PlasmaWaveShape.DownRightShort => (PlasmaWaveDownRightShort, 6),
            PlasmaWaveShape.DownRightLong => (PlasmaWaveDownRightLong, 10),
            PlasmaWaveShape.VerticalShort => (PlasmaWaveVerticalShort, 4),
            PlasmaWaveShape.VerticalLong => (PlasmaWaveVerticalLong, 7),
            PlasmaWaveShape.HorizontalAlternate => (PlasmaWaveHorizontalAlternate, 7),
            PlasmaWaveShape.DownRightAlternate => (PlasmaWaveDownRightAlternate, 12),
            PlasmaWaveShape.VerticalAlternate => (PlasmaWaveVerticalAlternate, 7),
            PlasmaWaveShape.DownLeftAlternate => (PlasmaWaveDownLeftAlternate, 12),
            PlasmaWaveShape.DownLeftShort => (PlasmaWaveDownLeftShort, 6),
            PlasmaWaveShape.DownLeftLong => (PlasmaWaveDownLeftLong, 10),
            _ => throw new IndexOutOfRangeException(),
        };
        int phase = index % 5;
        return (ushort)(start + (phase == 0 ? 0 : RecordBytes(coreParts) + (phase - 1) * RecordBytes(2 * coreParts)));
    }

    /// <summary>$93:F0FA: Charged_P_PI_PW_PIW startup, two axial orientation groups then two diagonal groups, repeated with alternate orientation/art.</summary>
    private const ushort PlasmaStartupStart = 0xf0fa;
    /// <summary>Four selected growth stages of the native startup instruction streams. Their selected sizes remain independent FrameBindingCatalog policy.</summary>
    private enum PlasmaGrowthStage { Core, Short, Long, Full }
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
        int selectedLength = ((PlasmaGrowthStage)(index % 4), diagonal) switch
        {
            (PlasmaGrowthStage.Core, false) => 1,
            (PlasmaGrowthStage.Short, false) => 3,
            (PlasmaGrowthStage.Long, false) => 6,
            (PlasmaGrowthStage.Full, false) => 7,
            (PlasmaGrowthStage.Core, true) => 1,
            (PlasmaGrowthStage.Short, true) => 2,
            (PlasmaGrowthStage.Long, true) => 4,
            (PlasmaGrowthStage.Full, true) => 5,
            _ => throw new IndexOutOfRangeException(),
        };
        int preceding = selectedLength - 1;
        int offset = preceding * sizeof(ushort) + 5 * (diagonal ? 2 : 1) * preceding * (preceding + 1) / 2;
        return (ushort)(PlasmaStartupStart + groupOffset + offset);
    }
    /// <summary>$93:A252: Power-beam poses select OBJ tile $30..32 with palette6/priority2; these independent artwork choices remain required.</summary>
    private const int PowerTile = 0x30, PowerPalette = 6, PowerPriority = 2;
    internal static bool TryPowerPhase(ushort pointer, out int phase)
    {
        int relative = pointer - PowerStart;
        phase = relative / RecordBytes(1);
        return relative >= 0 && relative % RecordBytes(1) == 0 && phase < 8;
    }
    /// <summary>$93:A24D..A27E: eight centered one-tile Power poses traverse a triangular three-glyph cycle and rotate its horizontal/vertical reflection phases.</summary>
    internal readonly struct PowerParts(int phase) : IReadOnlyList<CompiledSpritePart>
    {
        public int Count => 1;
        public CompiledSpritePart this[int index]
        {
            get
            {
                if (index != 0) throw new IndexOutOfRangeException();
                int tile = PowerTile + 2 - Math.Abs(2 - (phase & 3));
                var flips = (phase is >= 3 and <= 5 ? SnesTileFlipFlags.Horizontal : 0) |
                    (phase >= 4 ? SnesTileFlipFlags.Vertical : 0);
                return new(SnesSpritemapXWord.Create(-8 / 2, false), unchecked((byte)(-8 / 2)),
                    SnesObjAttributeWord.Create(tile, PowerPalette, PowerPriority, flips), false);
            }
        }
        public IEnumerator<CompiledSpritePart> GetEnumerator()
        {
            yield return this[0];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    public static string Name(ushort pointer) => $"sprite_{pointer:X4}";
}
