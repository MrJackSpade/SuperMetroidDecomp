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
    /// <summary>$93:86DB-873A: eight directional Power programs, each one eight-byte timed record followed by a four-byte self-jump.</summary>
    private const ushort PowerProgramStart = 0x86db;

    /// <summary>Native Power direction order starts at up; the OAM rotation starts at left, so its eight compass poses are offset by two.</summary>
    internal static bool TryPowerDirectionSprite(ushort instructionPointer, out ushort sprite)
    {
        int offset = instructionPointer - PowerProgramStart;
        const int programBytes = 8 + 4;
        if (offset >= 0 && offset < 8 * programBytes && offset % programBytes == 0)
        {
            int direction = offset / programBytes;
            sprite = (ushort)(PowerStart + ((direction + 2) & 7) * RecordBytes(1));
            return true;
        }
        sprite = default;
        return false;
    }
    /// <summary>$93:873B: upward Wave's invisible lead-in before the shared vertical-travel oscillation.</summary>
    private const ushort WaveInvisibleLeadIn = 0x873b;
    /// <summary>$93:8743-8952: four sixteen-record Wave loops in vertical, rising diagonal, horizontal, falling diagonal travel order, each ending in a four-byte self-jump.</summary>
    private const ushort WaveProgramStart = 0x8743;
    /// <summary>$93:8953-8976: Ice's four successive centered glyph poses followed by its self-jump.</summary>
    private const ushort IceProgramStart = 0x8953;

    /// <summary>$93:8E77-8F16: eight charged Power compass programs, each two timed records alternating glyph banks, followed by a self-jump.</summary>
    private const ushort ChargedPowerProgramStart = 0x8e77;
    /// <summary>$93:912F-9152: charged Ice's four successive centered quad poses followed by its self-jump.</summary>
    private const ushort ChargedIceProgramStart = 0x912f;
    /// <summary>$93:A007: six successive beam-explosion artwork stages, then delete.</summary>
    private const ushort BeamExplosionProgram = 0xa007;
    /// <summary>$93:A039: six successive missile-explosion artwork stages, then delete.</summary>
    private const ushort MissileExplosionProgram = 0xa039;
    /// <summary>$93:A06B: five successive bomb-explosion artwork stages, then delete.</summary>
    private const ushort BombExplosionProgram = 0xa06b;
    /// <summary>$93:A095: Plasma SBA cycles the same five bomb-explosion artwork stages.</summary>
    private const ushort PlasmaSpecialBeamProgram = 0xa095;
    /// <summary>$93:A0C1: six successive super-missile explosion artwork stages, then delete.</summary>
    private const ushort SuperMissileExplosionProgram = 0xa0c1;
    /// <summary>$93:A0F3: unused projectile25h executes four invisible timed records.</summary>
    private const ushort InvisibleProjectileProgram = 0xa0f3;

    private static bool TryTimedPhase(ushort pointer, ushort start, int count, out int phase)
    {
        int offset = pointer - start;
        phase = offset / 8;
        return offset >= 0 && offset < count * 8 && offset % 8 == 0;
    }

    private static ushort BeamExplosionPointer(int phase) => (ushort)(BeamExplosionStart +
        Math.Min(phase, 2) * RecordBytes(1) + Math.Max(0, phase - 2) * RecordBytes(4));
    private static ushort MissileExplosionPointer(int phase) => (ushort)(MissileExplosionStart +
        (phase == 0 ? 0 : RecordBytes(1) + (phase - 1) * RecordBytes(4)));
    /// <summary>$93:9EBB-9F1A: eight one-record missile compass programs.</summary>
    private const ushort MissileProgramStart = 0x9ebb;
    /// <summary>$93:9F1B-9F7A: eight one-record super-missile compass programs.</summary>
    private const ushort SuperMissileProgramStart = 0x9f1b;
    /// <summary>$93:9F7B: the super-missile link has no visible composition.</summary>
    private const ushort SuperMissileLinkProgram = 0x9f7b;

    private static bool TryCompassPose(ushort pointer, ushort start, out int pose)
    {
        int offset = pointer - start;
        const int programBytes = 8 + 4;
        pose = ((offset / programBytes) + 2) & 7;
        return offset >= 0 && offset < 8 * programBytes && offset % programBytes == 0;
    }

    private static ushort MissilePointer(ushort start, int pose) => (ushort)(start +
        pose / 2 * (RecordBytes(2) + RecordBytes(3)) + pose % 2 * RecordBytes(2));
    /// <summary>$93:9F87 and9FA3: normal/fast PowerBomb cycles each traverse the same three poses.</summary>
    private const ushort PowerBombProgram = 0x9f87, FastPowerBombProgram = 0x9fa3;
    /// <summary>$93:9FBF and9FE3: normal/fast Bomb cycles each traverse the same four poses.</summary>
    private const ushort BombProgram = 0x9fbf, FastBombProgram = 0x9fe3;
    internal static bool TryCalculatedFrameSprite(ushort instructionPointer, out ushort sprite)
    {
        if (TryPowerDirectionSprite(instructionPointer, out sprite)) return true;
        if (TryTimedPhase(instructionPointer, PowerBombProgram, 3, out int bombPhase) ||
            TryTimedPhase(instructionPointer, FastPowerBombProgram, 3, out bombPhase))
        {
            sprite = (ushort)(PowerBombStart + bombPhase * RecordBytes(1));
            return true;
        }
        if (TryTimedPhase(instructionPointer, BombProgram, 4, out bombPhase) ||
            TryTimedPhase(instructionPointer, FastBombProgram, 4, out bombPhase))
        {
            sprite = (ushort)(BombStart + bombPhase * RecordBytes(1));
            return true;
        }
        if (TryCompassPose(instructionPointer, MissileProgramStart, out int missilePose))
        {
            sprite = MissilePointer(MissileStart, missilePose);
            return true;
        }
        if (TryCompassPose(instructionPointer, SuperMissileProgramStart, out missilePose))
        {
            sprite = MissilePointer(SuperMissileStart, missilePose);
            return true;
        }
        if (instructionPointer == SuperMissileLinkProgram)
        {
            sprite = NothingStart;
            return true;
        }
        if (TryTimedPhase(instructionPointer, BeamExplosionProgram, 6, out int explosionPhase))
        {
            sprite = BeamExplosionPointer(explosionPhase);
            return true;
        }
        if (TryTimedPhase(instructionPointer, MissileExplosionProgram, 6, out explosionPhase))
        {
            sprite = MissileExplosionPointer(explosionPhase);
            return true;
        }
        if (TryTimedPhase(instructionPointer, BombExplosionProgram, 5, out explosionPhase) ||
            TryTimedPhase(instructionPointer, PlasmaSpecialBeamProgram, 5, out explosionPhase))
        {
            sprite = (ushort)(BombExplosionStart + explosionPhase * RecordBytes(4));
            return true;
        }
        if (TryTimedPhase(instructionPointer, SuperMissileExplosionProgram, 6, out explosionPhase))
        {
            sprite = SuperMissileExplosionPointer(explosionPhase);
            return true;
        }
        if (TryTimedPhase(instructionPointer, InvisibleProjectileProgram, 4, out _))
        {
            sprite = NothingStart;
            return true;
        }
        if (instructionPointer == WaveInvisibleLeadIn)
        {
            sprite = NothingStart;
            return true;
        }
        int offset = instructionPointer - WaveProgramStart;
        const int cycleBytes = 16 * 8 + 4;
        if (offset >= 0 && offset < 4 * cycleBytes)
        {
            int recordOffset = offset % cycleBytes;
            if (recordOffset < 16 * 8 && recordOffset % 8 == 0)
            {
                int phase = recordOffset / 8;
                int halfPhase = phase % 8;
                int distanceStage = Math.Min(halfPhase, 8 - halfPhase);
                // Travel direction chooses the perpendicular displacement axis. Each half-cycle
                // traverses the same four spatial stages outward and back on opposite sides.
                WaveTravelAxis travel = (WaveTravelAxis)(offset / cycleBytes);
                int directionPair = travel switch
                {
                    WaveTravelAxis.Vertical => 2,
                    WaveTravelAxis.RisingDiagonal => 3,
                    WaveTravelAxis.Horizontal => 0,
                    WaveTravelAxis.FallingDiagonal => 1,
                    _ => throw new InvalidOperationException("Unknown Wave travel axis."),
                };
                int pose = distanceStage == 0 ? 0 : directionPair * 8 + (phase / 8) * 4 + distanceStage;
                sprite = (ushort)(WaveStart + pose * RecordBytes(1));
                return true;
            }
        }
        offset = instructionPointer - ChargedPowerProgramStart;
        const int chargedProgramBytes = 2 * 8 + 4;
        if (offset >= 0 && offset < 8 * chargedProgramBytes)
        {
            int recordOffset = offset % chargedProgramBytes;
            if (recordOffset < 2 * 8 && recordOffset % 8 == 0)
            {
                int direction = offset / chargedProgramBytes;
                int pose = ((direction + 2) & 7) + (recordOffset / 8) * 8;
                sprite = (ushort)(ChargedPowerStart + pose * RecordBytes(4));
                return true;
            }
        }
        offset = instructionPointer - ChargedIceProgramStart;
        if (offset >= 0 && offset < 4 * 8 && offset % 8 == 0)
        {
            sprite = (ushort)(ChargedIceStart + (offset / 8) * RecordBytes(4));
            return true;
        }
        offset = instructionPointer - IceProgramStart;
        if (offset >= 0 && offset < 4 * 8 && offset % 8 == 0)
        {
            sprite = (ushort)(IceStart + (offset / 8) * RecordBytes(1));
            return true;
        }
        sprite = default;
        return false;
    }

    /// <summary>Mutually exclusive travel axes of the four native Wave instruction loops at93:8743,87C7,884B,88CF.</summary>
    private enum WaveTravelAxis { Vertical, RisingDiagonal, Horizontal, FallingDiagonal }
    private static ushort PointerAt(int index)
    {
        if ((uint)index >= 417) throw new IndexOutOfRangeException();
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
    internal static bool TrySingleBeamPhase(ushort pointer, out int phase) =>
        TryPhase(pointer, PowerStart, 1, 8, out phase) || TryPhase(pointer, IceStart, 1, 4, out phase);
    private static bool TryPhase(ushort pointer, ushort start, int parts, int count, out int phase)
    {
        int relative = pointer - start;
        phase = relative / RecordBytes(parts);
        return relative >= 0 && relative % RecordBytes(parts) == 0 && phase < count;
    }
    internal static bool TryChargedBeamPhase(ushort pointer, out int phase, out bool ice)
    {
        ice = false;
        if (TryPhase(pointer, ChargedPowerStart, 4, 16, out phase))
            return phase != 0; // EC3E's independently selected ordering remains required.
        ice = true;
        return TryPhase(pointer, ChargedIceStart, 4, 4, out phase);
    }
    /// <summary>$93:A24D..A27E: eight centered one-tile Power poses (also the first four Ice poses at $93:EDF6) traverse a triangular three-glyph cycle and rotate its horizontal/vertical reflection phases.</summary>
    internal readonly struct SingleBeamParts(int phase) : IReadOnlyList<CompiledSpritePart>
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
    /// <summary>$93:EC54..ED88 and ED9E..EDE0 mirror one glyph across a centered two-by-two cell square; Power uses column order and Ice row order.</summary>
    internal readonly struct ChargedBeamParts(int phase, bool ice) : IReadOnlyList<CompiledSpritePart>
    {
        public int Count => 4;
        public CompiledSpritePart this[int index]
        {
            get
            {
                if ((uint)index >= 4) throw new IndexOutOfRangeException();
                int column = ice ? index % 2 : index / 2;
                int row = ice ? index / 2 : index % 2;
                int tile = ChargedBeamTile + (ice ? phase % 2 : phase / 8);
                var flips = (column == 0 ? SnesTileFlipFlags.Horizontal : 0) |
                    (row == 0 ? SnesTileFlipFlags.Vertical : 0);
                return new(SnesSpritemapXWord.Create(-column * 8, false), unchecked((byte)(-row * 8)),
                    SnesObjAttributeWord.Create(tile, PowerPalette, PowerPriority, flips), false);
            }
        }
        public IEnumerator<CompiledSpritePart> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>$93:EC43/EDA3: charged Power/Ice use independently selected OBJ glyphs $33/$34, sharing palette6/priority2 with the single-tile beam.</summary>
    private const int ChargedBeamTile = 0x33;
    internal static bool TryWavePhase(ushort pointer, out int phase) => TryPhase(pointer, WaveStart, 1, 33, out phase);
    /// <summary>$93:AE70/AE77/AE7E/AE85: four independent axial offsets from the centered sprite origin; these source choices remain required under frames.</summary>
    private static ReadOnlySpan<int> UnresolvedWaveDistances => [8, 13, 15, 16];
    /// <summary>$93:AEA4..AEC0 and other diagonal groups: component magnitudes6/9/11/12 equal floor(3*axial/4). This selected ratio remains required, not a trigonometric assertion.</summary>
    private const int WaveDiagonalNumerator = 3, WaveDiagonalDenominator = 4;
    private enum WaveDirection { Up, Down, UpRight, DownLeft, Right, Left, UpLeft, DownRight }
    /// <summary>$93:AE65..AF4B: one centered pose then four stages in each of eight signed directional displacement groups.</summary>
    internal readonly struct WaveParts(int phase) : IReadOnlyList<CompiledSpritePart>
    {
        public int Count => 1;
        public CompiledSpritePart this[int index]
        {
            get
            {
                if (index != 0) throw new IndexOutOfRangeException();
                int x = -8 / 2, y = -8 / 2, tile = PowerTile;
                if (phase != 0)
                {
                    int step = (phase - 1) % 4;
                    (int dx, int dy) = (WaveDirection)((phase - 1) / 4) switch
                    {
                        WaveDirection.Up => (0, -1),
                        WaveDirection.Down => (0, 1),
                        WaveDirection.UpRight => (1, -1),
                        WaveDirection.DownLeft => (-1, 1),
                        WaveDirection.Right => (1, 0),
                        WaveDirection.Left => (-1, 0),
                        WaveDirection.UpLeft => (-1, -1),
                        WaveDirection.DownRight => (1, 1),
                        _ => throw new IndexOutOfRangeException(),
                    };
                    int distance = UnresolvedWaveDistances[step];
                    if (dx != 0 && dy != 0) distance = distance * WaveDiagonalNumerator / WaveDiagonalDenominator;
                    x += dx * distance;
                    y += dy * distance;
                    tile += (step + 1) / 2;
                }
                return new(SnesSpritemapXWord.Create(x, false), unchecked((byte)y),
                    SnesObjAttributeWord.Create(tile, PowerPalette, PowerPriority, 0), false);
            }
        }
        public IEnumerator<CompiledSpritePart> GetEnumerator() { yield return this[0]; }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    internal static bool TryVerticalChargedWavePhase(ushort pointer, out int phase) =>
        TryVerticalChargedWavePhase(pointer, ChargedWaveStart, out phase) ||
        TryVerticalChargedWavePhase(pointer, ChargedIceWaveStart, out phase);
    private static bool TryVerticalChargedWavePhase(ushort pointer, ushort start, out int phase)
    {
        if (TryPhase(pointer, start, 4, 2, out phase)) return true;
        if (!TryPhase(pointer, (ushort)(start + 2 * RecordBytes(4)), 8, 8, out phase)) return false;
        phase += 2;
        return true;
    }
    /// <summary>$93:AF4C..B0C7 and B672..B7ED: two centered glyphs followed by four paired vertical displacements, each with swapped lobe glyphs.</summary>
    internal readonly struct VerticalChargedWaveParts(int phase) : IReadOnlyList<CompiledSpritePart>
    {
        public int Count => phase < 2 ? 4 : 8;
        public CompiledSpritePart this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new IndexOutOfRangeException();
                int lobe = index / 4, quadrant = index % 4;
                int column = quadrant % 2, row = quadrant / 2;
                int centerY = phase < 2 ? 0 : (lobe == 0 ? 1 : -1) * UnresolvedWaveDistances[(phase - 2) / 2];
                int tile = ChargedBeamTile + (phase < 2 ? 1 - phase : (phase + lobe) % 2);
                var flips = (column == 0 ? SnesTileFlipFlags.Horizontal : 0) |
                    (row == 0 ? SnesTileFlipFlags.Vertical : 0);
                return new(SnesSpritemapXWord.Create(-column * 8, false), unchecked((byte)(centerY - row * 8)),
                    SnesObjAttributeWord.Create(tile, PowerPalette, PowerPriority, flips), false);
            }
        }
        public IEnumerator<CompiledSpritePart> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    public static string Name(ushort pointer) => $"sprite_{pointer:X4}";
}
