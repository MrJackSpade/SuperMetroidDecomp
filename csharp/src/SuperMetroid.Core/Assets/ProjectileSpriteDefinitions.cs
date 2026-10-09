using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Stable identities for the spritemaps referenced by native timed projectile records.</summary>
public static class ProjectileSpriteDefinitions
{
    /// <summary>Supported projectile-composition JSON schema revision, one, requiring every native timed-record sprite identity.</summary>
    public const int Version = 1;
    /// <summary>Installed JSON resource filename for editable projectile OAM compositions, separate from their timing, damage, and collision mechanics.</summary>
    public const string FileName = "projectile-compositions.json";
    /// <summary>Maximum OAM parts allowed in one authored composition, 128, matching the hardware OBJ entry capacity rather than an animation-frame count.</summary>
    public const int MaximumParts = 128;
    /// <summary>Sixteen columns and thirty-two rows in the logical OBJ character grid; row times sixteen plus column forms the nine-bit tile index, with large-part neighbors wrapping natively.</summary>
    public const int TileColumns = 16, TileRows = 32;
    /// <summary>The417 timed-projectile identities in their original sorted order; selected OAM groups derive from two-byte headers and five-byte parts.</summary>
    public static PointerSequence NativePointers => default;
    /// <summary>Allocation-free immutable view of the 417 bank-$93 spritemap offsets selected by native timed projectile records, calculated from composition-group layouts.</summary>
    public readonly struct PointerSequence : IReadOnlyList<ushort>
    {
        /// <summary>Number of required sprite identities, 417, including the zero-part Nothing composition rather than only visible frames.</summary>
        public int Count => 417;
        /// <summary>Required identity count, equivalent to <see cref="Count"/>, for callers using array-style traversal.</summary>
        public int Length => Count;
        /// <summary>Returns one sprite identity in the original sorted native-pointer order, not an instruction-list pointer or animation-stage number.</summary>
        /// <param name="index">Zero-based identity index 0..416.</param>
        /// <returns>Bank-relative spritemap offset in bank $93.</returns>
        /// <exception cref="IndexOutOfRangeException">The index is outside 0..416.</exception>
        public ushort this[int index] => PointerAt(index);
        /// <summary>Enumerates the complete required identity set in ascending native-pointer order without retaining a backing pointer array.</summary>
        /// <returns>An enumerator yielding all 417 bank-relative spritemap offsets.</returns>
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

    /// <summary>Maps an aligned eight-byte native timed record within one contiguous instruction range to its zero-based phase.</summary>
    /// <param name="pointer">Instruction-record address to classify.</param><param name="start">First record address in the range.</param><param name="count">Number of records accepted.</param><param name="phase">Receives the record index when the address is aligned and in range.</param>
    /// <returns><see langword="true"/> only for an address at a record boundary inside the range.</returns>
    private static bool TryTimedPhase(ushort pointer, ushort start, int count, out int phase)
    {
        int offset = pointer - start;
        phase = offset / 8;
        return offset >= 0 && offset < count * 8 && offset % 8 == 0;
    }

    /// <summary>Returns the native composition offset for a beam-explosion stage, accounting for its two single-part cores.</summary>
    /// <param name="phase">Zero-based displayed explosion stage.</param><returns>Bank-$93 composition address for that stage.</returns>
    private static ushort BeamExplosionPointer(int phase) => (ushort)(BeamExplosionStart +
        Math.Min(phase, 2) * RecordBytes(1) + Math.Max(0, phase - 2) * RecordBytes(4));
    /// <summary>Returns the missile-explosion composition address, whose first stage is one part and later stages are four parts.</summary>
    /// <param name="phase">Zero-based displayed explosion stage.</param><returns>Bank-$93 composition address for that stage.</returns>
    private static ushort MissileExplosionPointer(int phase) => (ushort)(MissileExplosionStart +
        (phase == 0 ? 0 : RecordBytes(1) + (phase - 1) * RecordBytes(4)));
    /// <summary>$93:9EBB-9F1A: eight one-record missile compass programs.</summary>
    private const ushort MissileProgramStart = 0x9ebb;
    /// <summary>$93:9F1B-9F7A: eight one-record super-missile compass programs.</summary>
    private const ushort SuperMissileProgramStart = 0x9f1b;
    /// <summary>$93:9F7B: the super-missile link has no visible composition.</summary>
    private const ushort SuperMissileLinkProgram = 0x9f7b;

    /// <summary>Decodes a one-record directional program address into the matching rotated OAM pose.</summary>
    /// <param name="pointer">Instruction record address.</param><param name="start">First of eight compass programs.</param><param name="pose">Receives the OAM compass index when recognized.</param>
    /// <returns><see langword="true"/> for one of the eight record starts, excluding each program's trailing jump.</returns>
    private static bool TryCompassPose(ushort pointer, ushort start, out int pose)
    {
        int offset = pointer - start;
        const int programBytes = 8 + 4;
        pose = ((offset / programBytes) + 2) & 7;
        return offset >= 0 && offset < 8 * programBytes && offset % programBytes == 0;
    }

    /// <summary>Calculates the packed native address for a missile compass pose, whose axial and diagonal records have different sizes.</summary>
    /// <param name="start">First composition address in the missile family.</param><param name="pose">Zero-based compass pose.</param><returns>Bank-$93 composition address.</returns>
    private static ushort MissilePointer(ushort start, int pose) => (ushort)(start +
        pose / 2 * (RecordBytes(2) + RecordBytes(3)) + pose % 2 * RecordBytes(2));
    /// <summary>$93:9F87 and9FA3: normal/fast PowerBomb cycles each traverse the same three poses.</summary>
    private const ushort PowerBombProgram = 0x9f87, FastPowerBombProgram = 0x9fa3;
    /// <summary>$93:9FBF and9FE3: normal/fast Bomb cycles each traverse the same four poses.</summary>
    private const ushort BombProgram = 0x9fbf, FastBombProgram = 0x9fe3;
    /// <summary>$93:8F17/8F1F: invisible upward charged-Wave lead-in followed by four sixteen-frame travel-axis loops.</summary>
    private const ushort ChargedWaveLeadIn = 0x8f17, ChargedWaveProgram = 0x8f1f;
    /// <summary>$93:9153/915B: corresponding charged IceWave lead-in and four travel-axis loops.</summary>
    private const ushort ChargedIceWaveLeadIn = 0x9153, ChargedIceWaveProgram = 0x915b;

    /// <summary>Resolves a charged Wave or charged IceWave timed record to its axis- and phase-specific artwork.</summary>
    /// <param name="pointer">Timed-record address in the selected instruction family.</param><param name="programStart">First axis loop in that family.</param><param name="compositionStart">First composition in the corresponding Wave artwork group.</param><param name="sprite">Receives the calculated composition pointer.</param>
    /// <returns><see langword="true"/> for one of the sixteen records in any of the four axis loops.</returns>
    private static bool TryChargedWaveCycle(ushort pointer, ushort programStart, ushort compositionStart, out ushort sprite)
    {
        const int cycleBytes = 16 * 8 + 4;
        int offset = pointer - programStart;
        sprite = default;
        if (offset < 0 || offset >= 4 * cycleBytes) return false;
        int recordOffset = offset % cycleBytes;
        if (recordOffset >= 16 * 8 || recordOffset % 8 != 0) return false;
        int phase = recordOffset / 8;
        WaveTravelAxis axis = (WaveTravelAxis)(offset / cycleBytes);
        // The last axial pair reverses glyph order in native data. Its independent
        // parity-selection policy remains REQUIRED; only its exact pointer mapping calculates.
        int glyphParity = phase % 2;
        if (phase >= 16 - 2 && axis is WaveTravelAxis.Vertical or WaveTravelAxis.Horizontal) glyphParity ^= 1;
        int halfPhase = phase / 2;
        int stage = Math.Min(halfPhase, 8 - halfPhase);
        int shapeGroup = axis switch
        {
            WaveTravelAxis.Vertical => 3,
            WaveTravelAxis.RisingDiagonal => 2,
            WaveTravelAxis.Horizontal => 0,
            WaveTravelAxis.FallingDiagonal => 1,
            _ => throw new InvalidOperationException("Unknown charged Wave travel axis."),
        };
        int pose = stage == 0 ? glyphParity : 2 + shapeGroup * 8 + (stage - 1) * 2 + glyphParity;
        sprite = ChargedWavePointer(compositionStart, pose);
        return true;
    }
    /// <summary>$93:8977..8A56: eight compass Spazer/SpazerIce lists, each three timed records and a self-jump.</summary>
    private const ushort SpazerProgramStart = 0x8977;
    /// <summary>Native semantic spread phases of each ordinary Spazer list.</summary>
    private enum SpazerSpreadPhase
    {
        /// <summary>The initial unspread beam seed.</summary>
        Seed,
        /// <summary>The first selected spread stage; axial and diagonal families use different physical stage numbers.</summary>
        Intermediate,
        /// <summary>The terminal selected spread stage for the ordinary compass cycle.</summary>
        Full
    }
    /// <summary>Selected physical spread stages remain REQUIRED phase-policy inputs: axial2/5, diagonal1/4. Pointer arithmetic does not derive these choices.</summary>
    private static int SpazerSelectedStage(SpazerSpreadPhase phase, bool diagonal) => phase switch
    {
        SpazerSpreadPhase.Seed => 0,
        SpazerSpreadPhase.Intermediate => diagonal ? 1 : 2,
        SpazerSpreadPhase.Full => diagonal ? 4 : 5,
        _ => throw new ArgumentOutOfRangeException(nameof(phase)),
    };
    /// <summary>Resolves one of the three timed records in an ordinary Spazer compass program.</summary>
    /// <param name="pointer">Address of the native timed record.</param><param name="sprite">Receives the seed or selected spread composition pointer.</param>
    /// <returns><see langword="true"/> when the address is a record boundary in one of the eight programs.</returns>
    private static bool TrySpazerCompassSprite(ushort pointer, out ushort sprite)
    {
        const int programBytes = 3 * 8 + 4;
        int offset = pointer - SpazerProgramStart;
        int within = offset % programBytes;
        if (offset < 0 || offset >= 8 * programBytes || within >= 3 * 8 || within % 8 != 0)
        {
            sprite = default;
            return false;
        }
        SpazerSeedDirection direction = SpazerCompassDirection(offset / programBytes);
        int group = (int)direction;
        bool diagonal = group < 4;
        int seedParts = diagonal ? 4 : 2;
        int stage = SpazerSelectedStage((SpazerSpreadPhase)(within / 8), diagonal);
        int start = SpazerStart + Math.Min(group, 4) * SpreadGroupBytes(4) + Math.Max(0, group - 4) * SpreadGroupBytes(2);
        sprite = (ushort)(start + (stage == 0 ? 0 : RecordBytes(seedParts) + (stage - 1) * RecordBytes(3 * seedParts)));
        return true;
    }
    /// <summary>Converts the instruction table's up-first compass numbering to the seed direction numbering used by composition groups.</summary>
    /// <param name="compass">Zero-based instruction-list direction, from up clockwise.</param><returns>The corresponding Spazer seed orientation.</returns>
    private static SpazerSeedDirection SpazerCompassDirection(int compass) => compass switch
        {
            0 => SpazerSeedDirection.Up,
            1 => SpazerSeedDirection.UpRight,
            2 => SpazerSeedDirection.Right,
            3 => SpazerSeedDirection.DownRight,
            4 => SpazerSeedDirection.Down,
            5 => SpazerSeedDirection.DownLeft,
            6 => SpazerSeedDirection.Left,
            7 => SpazerSeedDirection.UpLeft,
            _ => throw new InvalidOperationException("Unknown Spazer compass direction."),
        };
    /// <summary>$93:8A57..8CF6: eight SpazerWave compass cycles, ten timed stages outward and back plus self-jump.</summary>
    private const ushort SpazerWaveProgramStart = 0x8a57;
    /// <summary>The last physical diagonal composition is the selected near-center pose at cycle phases1/9. This placement/selection choice remains REQUIRED independently of address calculation.</summary>
    private const int SpazerDiagonalNearCenterStage = 5;
    /// <summary>$93:8BFB..8C4E selects a capped0..4..0 sweep with three phases at4; this chosen ceiling/plateau remains REQUIRED phase policy.</summary>
    private const int SpazerDownLeftSelectedCeiling = 4;
    /// <summary>Maps a SpazerWave outward-and-return record to the native selected spread stage for its compass direction.</summary>
    /// <param name="pointer">Address of a timed record in the SpazerWave loops.</param><param name="sprite">Receives the selected composition address.</param>
    /// <returns><see langword="true"/> for one of the ten timed records in any directional loop.</returns>
    private static bool TrySpazerWaveSprite(ushort pointer, out ushort sprite)
    {
        const int cycleLength = 10, cycleBytes = cycleLength * 8 + 4;
        int offset = pointer - SpazerWaveProgramStart;
        int within = offset % cycleBytes;
        if (offset < 0 || offset >= 8 * cycleBytes || within >= cycleLength * 8 || within % 8 != 0)
        {
            sprite = default;
            return false;
        }
        int group = (int)SpazerCompassDirection(offset / cycleBytes);

        bool diagonal = group < 4;
        int phase = within / 8;
        int spread = Math.Min(phase, cycleLength - phase);
        int stage = group == (int)SpazerSeedDirection.DownLeft
            ? Math.Min(spread, SpazerDownLeftSelectedCeiling)
            : !diagonal || spread == 0 ? spread : spread == 1 ? SpazerDiagonalNearCenterStage : spread - 1;
        int seedParts = diagonal ? 4 : 2;
        int start = SpazerStart + Math.Min(group, 4) * SpreadGroupBytes(4) + Math.Max(0, group - 4) * SpreadGroupBytes(2);
        sprite = (ushort)(start + (stage == 0 ? 0 : RecordBytes(seedParts) + (stage - 1) * RecordBytes(3 * seedParts)));
        return true;
    }
    /// <summary>$93:8CF7..8D46: four Plasma/PlasmaIce axes, each startup core then mature pose and self-jump.</summary>
    private const ushort PlasmaProgramStart = 0x8cf7;
    /// <summary>$93:8D47..8E76: four PlasmaWave axes, each an optional core lead and eight outward/return phases.</summary>
    private const ushort PlasmaWaveProgramStart = 0x8d47;
    /// <summary>Native axis selects the named core orientation; independent chosen growth/artwork policy remains REQUIRED.</summary>
    private static int PlasmaCoreGroup(WaveTravelAxis axis) => axis switch
    {
        WaveTravelAxis.Vertical => 1,
        WaveTravelAxis.RisingDiagonal => 3,
        WaveTravelAxis.Horizontal => 0,
        WaveTravelAxis.FallingDiagonal => 2,
        _ => throw new ArgumentOutOfRangeException(nameof(axis)),
    };
    /// <summary>Resolves ordinary Plasma and PlasmaWave records, including their shared orientation-specific startup compositions.</summary>
    /// <param name="pointer">Timed-record address in either instruction family.</param><param name="sprite">Receives the matching startup or mature composition pointer.</param>
    /// <returns><see langword="true"/> when the address belongs to a timed record in either family.</returns>
    private static bool TryPlasmaSprite(ushort pointer, out ushort sprite)
    {
        const int programBytes = 2 * 8 + 4;
        int offset = pointer - PlasmaProgramStart;
        int within = offset % programBytes;
        if (offset >= 0 && offset < 4 * programBytes && within < 2 * 8 && within % 8 == 0)
        {
            var axis = (WaveTravelAxis)(offset / programBytes);
            int coreGroup = PlasmaCoreGroup(axis);
            if (within == 0) sprite = PlasmaStartupPointer(coreGroup * 4);
            else
            {
                int pose = axis switch
                {
                    WaveTravelAxis.Vertical => 2,
                    WaveTravelAxis.RisingDiagonal => 3,
                    WaveTravelAxis.Horizontal => 0,
                    WaveTravelAxis.FallingDiagonal => 1,
                    _ => throw new InvalidOperationException("Unknown Plasma axis."),
                };
                sprite = (ushort)(PlasmaStart + pose / 2 * (RecordBytes(4) + RecordBytes(6)) + pose % 2 * RecordBytes(4));
            }
            return true;
        }
        const int waveBytes = 9 * 8 + 4;
        offset = pointer - PlasmaWaveProgramStart;
        within = offset % waveBytes;
        if (offset >= 0 && offset < 4 * waveBytes && within < 9 * 8 && within % 8 == 0)
        {
            var axis = (WaveTravelAxis)(offset / waveBytes);
            if (within == 0) sprite = PlasmaStartupPointer(PlasmaCoreGroup(axis) * 4);
            else
            {
                PlasmaWaveShape shape = axis switch
                {
                    WaveTravelAxis.Vertical => PlasmaWaveShape.VerticalShort,
                    WaveTravelAxis.RisingDiagonal => PlasmaWaveShape.DownLeftShort,
                    WaveTravelAxis.Horizontal => PlasmaWaveShape.HorizontalShort,
                    WaveTravelAxis.FallingDiagonal => PlasmaWaveShape.DownRightShort,
                    _ => throw new InvalidOperationException("Unknown PlasmaWave axis."),
                };
                int phase = within / 8 - 1;
                int spread = Math.Min(phase, 8 - phase);
                sprite = PlasmaWavePointer((int)shape * 5 + spread);
            }
            return true;
        }
        sprite = default;
        return false;
    }
    /// <summary>$93:9ADB..9BEA: four charged Plasma axes, eight alternating growth records and a full-size self-loop.</summary>
    private const ushort ChargedPlasmaProgramStart = 0x9adb;
    /// <summary>$93:9BEB..9EBA: four charged PlasmaWave axes, six alternating growth leads then sixteen spread records.</summary>
    private const ushort ChargedPlasmaWaveProgramStart = 0x9beb;
    /// <summary>Selects the alternating startup artwork used while charged Plasma grows along an axis.</summary>
    /// <param name="axis">Travel orientation of the projectile.</param><param name="phase">Zero-based growth record.</param><returns>The selected startup composition pointer.</returns>
    private static ushort ChargedPlasmaGrowthSprite(WaveTravelAxis axis, int phase) =>
        PlasmaStartupPointer((PlasmaCoreGroup(axis) + (phase % 2) * 4) * 4 + phase / 2);
    /// <summary>Resolves charged Plasma and charged PlasmaWave records to startup-growth or spreading artwork.</summary>
    /// <param name="pointer">Timed-record address in either charged instruction family.</param><param name="sprite">Receives the corresponding composition address.</param>
    /// <returns><see langword="true"/> when the address is an aligned record in either family.</returns>
    private static bool TryChargedPlasmaSprite(ushort pointer, out ushort sprite)
    {
        const int programBytes = 8 * 8 + 4;
        int offset = pointer - ChargedPlasmaProgramStart;
        int within = offset % programBytes;
        if (offset >= 0 && offset < 4 * programBytes && within < 8 * 8 && within % 8 == 0)
        {
            sprite = ChargedPlasmaGrowthSprite((WaveTravelAxis)(offset / programBytes), within / 8);
            return true;
        }
        const int waveBytes = (6 + 16) * 8 + 4;
        offset = pointer - ChargedPlasmaWaveProgramStart;
        within = offset % waveBytes;
        if (offset >= 0 && offset < 4 * waveBytes && within < 22 * 8 && within % 8 == 0)
        {
            var axis = (WaveTravelAxis)(offset / waveBytes);
            int phase = within / 8;
            if (phase < 6) sprite = ChargedPlasmaGrowthSprite(axis, phase);
            else
            {
                // Alternating native long/alternate artwork and selected growth extents
                // remain required art/policy inputs; the outward/return phase calculates.
                bool alternate = (phase & 1) != 0;
                PlasmaWaveShape shape = (axis, alternate) switch
                {
                    (WaveTravelAxis.Vertical, false) => PlasmaWaveShape.VerticalLong,
                    (WaveTravelAxis.Vertical, true) => PlasmaWaveShape.VerticalAlternate,
                    (WaveTravelAxis.RisingDiagonal, false) => PlasmaWaveShape.DownLeftLong,
                    (WaveTravelAxis.RisingDiagonal, true) => PlasmaWaveShape.DownLeftAlternate,
                    (WaveTravelAxis.Horizontal, false) => PlasmaWaveShape.HorizontalLong,
                    (WaveTravelAxis.Horizontal, true) => PlasmaWaveShape.HorizontalAlternate,
                    (WaveTravelAxis.FallingDiagonal, false) => PlasmaWaveShape.DownRightLong,
                    (WaveTravelAxis.FallingDiagonal, true) => PlasmaWaveShape.DownRightAlternate,
                    _ => throw new InvalidOperationException("Unknown charged PlasmaWave axis."),
                };
                int spreadPhase = (phase - 6) / 2;
                int spread = Math.Min(spreadPhase, 8 - spreadPhase);
                sprite = PlasmaWavePointer((int)shape * 5 + spread);
            }
            return true;
        }
        sprite = default;
        return false;
    }
    /// <summary>$93:94BB..9ADA: eight charged SpazerWave compass lists, four startup rows then twenty alternating spread rows.</summary>
    private const ushort ChargedSpazerWaveProgramStart = 0x94bb;
    /// <summary>Native six-phase composition groups fromD8EE, named by actual directional/alternate consumers. Choosing these artworks remains REQUIRED.</summary>
    private enum ChargedSpazerShape
    {
        /// <summary>Horizontal alternate spread used when the charged seed faces either horizontal direction.</summary>
        HorizontalAlternate,
        /// <summary>Alternate vertical spread shared by upward and downward seed directions.</summary>
        VerticalAlternate,
        /// <summary>Alternate falling diagonal spread group.</summary>
        FallingAlternate,
        /// <summary>Alternate rising diagonal spread group.</summary>
        RisingAlternate,
        /// <summary>Ordinary spread group for the upper-right orientation.</summary>
        UpRight,
        /// <summary>Ordinary spread group for the lower-right orientation.</summary>
        DownRight,
        /// <summary>Ordinary diagonal artwork selected for the lower-left seed orientation.</summary>
        DownLeft,
        /// <summary>Ordinary spread group for the upper-left orientation.</summary>
        UpLeft,
        /// <summary>Ordinary spread group for the downward orientation.</summary>
        Down,
        /// <summary>Ordinary spread group for the leftward orientation.</summary>
        Left,
        /// <summary>Ordinary spread group for the upward orientation.</summary>
        Up,
        /// <summary>Ordinary spread group for the rightward orientation.</summary>
        Right,
    }
    /// <summary>Selected two-pose startup groups EE12..F0C4 in native identity order. Their directional/art selections remain REQUIRED.</summary>
    private enum SpazerStartupShape
    {
        /// <summary>Left-facing two-pose startup strip.</summary>
        Left,
        /// <summary>Upper-left diagonal two-pose startup strip.</summary>
        UpLeft,
        /// <summary>Upward two-pose startup strip.</summary>
        Up,
        /// <summary>Upper-right diagonal two-pose startup strip.</summary>
        UpRight,
        /// <summary>Right-facing two-pose startup strip used by the horizontal seed.</summary>
        Right,
        /// <summary>Down-right diagonal two-pose startup strip.</summary>
        DownRight,
        /// <summary>Downward two-pose startup strip.</summary>
        Down,
        /// <summary>Alternate horizontal two-pose startup selected for horizontal seed directions.</summary>
        HorizontalAlternate,
        /// <summary>Alternate falling diagonal startup strip.</summary>
        FallingAlternate,
        /// <summary>Alternate vertical startup strip shared by the up and down directions.</summary>
        VerticalAlternate,
        /// <summary>Alternate rising diagonal startup strip.</summary>
        RisingAlternate,
    }
    /// <summary>Maps a seed orientation to the ordinary charged Spazer spread group used by its native axis.</summary>
    /// <param name="direction">Seed orientation from the projectile's compass instruction list.</param>
    /// <returns>The ordinary axial or diagonal spread group for that orientation.</returns>
    private static ChargedSpazerShape ChargedSpazerBase(SpazerSeedDirection direction) => direction switch
    {
        SpazerSeedDirection.Up => ChargedSpazerShape.Up,
        SpazerSeedDirection.UpRight => ChargedSpazerShape.UpRight,
        SpazerSeedDirection.Right => ChargedSpazerShape.Right,
        SpazerSeedDirection.DownRight => ChargedSpazerShape.DownRight,
        SpazerSeedDirection.Down => ChargedSpazerShape.Down,
        SpazerSeedDirection.DownLeft => ChargedSpazerShape.DownLeft,
        SpazerSeedDirection.Left => ChargedSpazerShape.Left,
        SpazerSeedDirection.UpLeft => ChargedSpazerShape.UpLeft,
        _ => throw new ArgumentOutOfRangeException(nameof(direction)),
    };
    /// <summary>Maps a seed orientation to the alternate charged Spazer spread group shared by its paired directions.</summary>
    /// <param name="direction">Seed orientation from the projectile's compass instruction list.</param>
    /// <returns>The alternate horizontal, vertical, rising, or falling artwork group.</returns>
    private static ChargedSpazerShape ChargedSpazerAlternate(SpazerSeedDirection direction) => direction switch
    {
        SpazerSeedDirection.Up or SpazerSeedDirection.Down => ChargedSpazerShape.VerticalAlternate,
        SpazerSeedDirection.UpRight or SpazerSeedDirection.DownLeft => ChargedSpazerShape.RisingAlternate,
        SpazerSeedDirection.Left or SpazerSeedDirection.Right => ChargedSpazerShape.HorizontalAlternate,
        SpazerSeedDirection.UpLeft or SpazerSeedDirection.DownRight => ChargedSpazerShape.FallingAlternate,
        _ => throw new ArgumentOutOfRangeException(nameof(direction)),
    };
    /// <summary>Selects the startup composition for a charged Spazer direction and one of its four lead-in records.</summary>
    /// <param name="direction">Seed orientation associated with the native instruction-list axis.</param>
    /// <param name="phase">Lead-in record index; odd phases use alternate artwork.</param>
    /// <returns>The bank-$93 composition address consumed by the selected record.</returns>
    private static ushort ChargedSpazerStartup(SpazerSeedDirection direction, int phase)
    {
        SpazerStartupShape shape;
        if ((phase & 1) != 0)
            shape = ChargedSpazerAlternate(direction) switch
            {
                ChargedSpazerShape.HorizontalAlternate => SpazerStartupShape.HorizontalAlternate,
                ChargedSpazerShape.VerticalAlternate => SpazerStartupShape.VerticalAlternate,
                ChargedSpazerShape.RisingAlternate => SpazerStartupShape.RisingAlternate,
                ChargedSpazerShape.FallingAlternate => SpazerStartupShape.FallingAlternate,
                _ => throw new InvalidOperationException("Unknown Spazer alternate startup."),
            };
        else
            shape = direction switch
            {
                SpazerSeedDirection.Up => SpazerStartupShape.Up,
                SpazerSeedDirection.UpRight or SpazerSeedDirection.DownLeft => SpazerStartupShape.UpRight,
                SpazerSeedDirection.Right => SpazerStartupShape.Right,
                SpazerSeedDirection.DownRight => SpazerStartupShape.DownRight,
                SpazerSeedDirection.Down => SpazerStartupShape.Down,
                SpazerSeedDirection.Left => SpazerStartupShape.Left,
                SpazerSeedDirection.UpLeft => SpazerStartupShape.UpLeft,
                _ => throw new ArgumentOutOfRangeException(nameof(direction)),
            };
        return SpazerStartupPointer((int)shape * 2 + phase / 2);
    }
    /// <summary>Resolves timed records in the eight charged SpazerWave compass loops to startup or spread compositions.</summary>
    /// <param name="pointer">Bank-relative instruction record address.</param>
    /// <param name="sprite">Receives the matching composition address, or the default value when unrecognized.</param>
    /// <returns>True only for one of the aligned timed records, excluding each loop's trailing jump.</returns>
    private static bool TryChargedSpazerWaveSprite(ushort pointer, out ushort sprite)
    {
        const int programBytes = 24 * 8 + 4;
        int offset = pointer - ChargedSpazerWaveProgramStart;
        int within = offset % programBytes;
        if (offset < 0 || offset >= 8 * programBytes || within >= 24 * 8 || within % 8 != 0)
        {
            sprite = default;
            return false;
        }
        SpazerSeedDirection direction = SpazerCompassDirection(offset / programBytes);
        int phase = within / 8;
        if (phase < 4) sprite = ChargedSpazerStartup(direction, phase);
        else
        {
            bool alternate = (phase & 1) != 0;
            ChargedSpazerShape shape = alternate ? ChargedSpazerAlternate(direction) : ChargedSpazerBase(direction);
            int spreadPhase = (phase - 4) / 2;
            int spread = Math.Min(spreadPhase, 10 - spreadPhase);
            // Directional diagonal base groups place their near-center pose last;
            // this native selection policy remains REQUIRED independently of addresses.
            int stage = !alternate && (int)direction < 4 && spread != 0
                ? (spread == 1 ? SpazerDiagonalNearCenterStage : spread - 1) : spread;
            sprite = ChargedSpazerPointer((int)shape * 6 + stage);
        }
        return true;
    }
    /// <summary>$93:936B..94BA: four ordinary charged Spazer axes, four startup and six selected spread rows.</summary>
    private const ushort ChargedSpazerProgramStart = 0x936b;
    /// <summary>Resolves an ordinary charged Spazer instruction record to its directional startup or spread artwork.</summary>
    /// <param name="pointer">Bank-relative instruction record address.</param>
    /// <param name="sprite">Receives the matching composition address, or the default value when unrecognized.</param>
    /// <returns>True only for one of the aligned timed records in the four ordinary axis loops.</returns>
    private static bool TryChargedSpazerSprite(ushort pointer, out ushort sprite)
    {
        const int programBytes = 10 * 8 + 4;
        int offset = pointer - ChargedSpazerProgramStart;
        int within = offset % programBytes;
        if (offset < 0 || offset >= 4 * programBytes || within >= 10 * 8 || within % 8 != 0)
        {
            sprite = default;
            return false;
        }
        var axis = (WaveTravelAxis)(offset / programBytes);
        SpazerSeedDirection direction = axis switch
        {
            WaveTravelAxis.Vertical => SpazerSeedDirection.Up,
            WaveTravelAxis.RisingDiagonal => SpazerSeedDirection.UpRight,
            WaveTravelAxis.Horizontal => SpazerSeedDirection.Left,
            WaveTravelAxis.FallingDiagonal => SpazerSeedDirection.DownRight,
            _ => throw new InvalidOperationException("Unknown charged Spazer axis."),
        };
        int phase = within / 8;
        if (phase < 4)
        {
            // The ordinary horizontal list chooses right-facing startup before its
            // left-axis mature shape. This native art selection remains REQUIRED.
            sprite = ChargedSpazerStartup(axis == WaveTravelAxis.Horizontal ? SpazerSeedDirection.Right : direction, phase);
        }
        else
        {
            bool alternate = (phase & 1) != 0;
            ChargedSpazerShape shape = alternate ? ChargedSpazerAlternate(direction) : ChargedSpazerBase(direction);
            int stage = SpazerSelectedStage((SpazerSpreadPhase)((phase - 4) / 2), !alternate && (int)direction < 4);
            sprite = ChargedSpazerPointer((int)shape * 6 + stage);
        }
        return true;
    }
    /// <summary>$93:A119: ShinesparkEcho's four invisible timed poses; visual echo rendering is separate from this selector.</summary>
    private const ushort ShinesparkEchoProgram = 0xa119;
    /// <summary>$93:A13D: Spazer SBA trail's three invisible timed collision stages.</summary>
    private const ushort SpazerSpecialBeamTrailProgram = 0xa13d;
    /// <summary>$93:A159: Wave SBA alternates the two centered charged Wave core compositions.</summary>
    private const ushort WaveSpecialBeamProgram = 0xa159;
    /// <summary>$93:A16D: unused Shinespark beam (projectile27h) traverses six beam-explosion visual stages.</summary>
    private const ushort UnusedShinesparkBeamProgram = 0xa16d;
    /// <summary>Maps a supported projectile instruction record to its calculated composition identity.</summary>
    /// <param name="instructionPointer">Bank-relative address of the current instruction record.</param>
    /// <param name="sprite">Receives the resolved composition address, or the default value if no selector recognizes it.</param>
    /// <returns>True when a compiled projectile family defines artwork for the instruction record.</returns>
    internal static bool TryCalculatedFrameSprite(ushort instructionPointer, out ushort sprite)
    {
        if (TryPowerDirectionSprite(instructionPointer, out sprite)) return true;
        if (TryTimedPhase(instructionPointer, ShinesparkEchoProgram, 4, out _) ||
            TryTimedPhase(instructionPointer, SpazerSpecialBeamTrailProgram, 3, out _))
        {
            sprite = NothingStart;
            return true;
        }
        if (TryTimedPhase(instructionPointer, WaveSpecialBeamProgram, 2, out int specialPhase))
        {
            sprite = ChargedWavePointer(ChargedWaveStart, specialPhase);
            return true;
        }
        if (TryTimedPhase(instructionPointer, UnusedShinesparkBeamProgram, 6, out specialPhase))
        {
            sprite = BeamExplosionPointer(specialPhase);
            return true;
        }
        if (TrySpazerCompassSprite(instructionPointer, out sprite)) return true;
        if (TrySpazerWaveSprite(instructionPointer, out sprite)) return true;
        if (TryPlasmaSprite(instructionPointer, out sprite)) return true;
        if (TryChargedPlasmaSprite(instructionPointer, out sprite)) return true;
        if (TryChargedSpazerWaveSprite(instructionPointer, out sprite)) return true;
        if (TryChargedSpazerSprite(instructionPointer, out sprite)) return true;
        if (instructionPointer is ChargedWaveLeadIn or ChargedIceWaveLeadIn)
        {
            sprite = NothingStart;
            return true;
        }
        if (TryChargedWaveCycle(instructionPointer, ChargedWaveProgram, ChargedWaveStart, out sprite) ||
            TryChargedWaveCycle(instructionPointer, ChargedIceWaveProgram, ChargedIceWaveStart, out sprite)) return true;
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
    private enum WaveTravelAxis
    {
        /// <summary>Projectile moves along the vertical axis.</summary>
        Vertical,
        /// <summary>Projectile moves diagonally up and right.</summary>
        RisingDiagonal,
        /// <summary>Projectile moves along the horizontal axis.</summary>
        Horizontal,
        /// <summary>Projectile moves diagonally down and right.</summary>
        FallingDiagonal
    }
    /// <summary>Returns the zero-based offset in the ordered set of required native sprite compositions.</summary>
    /// <param name="index">Identity index in the 417-entry sorted sequence.</param><returns>Bank-$93 spritemap address.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the identity sequence.</exception>
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
    /// <summary>Calculates a charged Wave composition address while skipping the two native records not selected by instruction lists.</summary>
    /// <param name="start">First core composition for the Wave family.</param><param name="index">Zero-based selected stage.</param><returns>Bank-$93 composition address.</returns>
    private static ushort ChargedWavePointer(ushort start, int index)
    {
        // B368/B37E and BA8E/BAA4 each contain two unselected four-part
        // compositions between the first24 paired-lobe phases and the last8.
        return (ushort)(start + Math.Min(index, 2) * RecordBytes(4) +
            Math.Max(0, index - 2) * RecordBytes(8) + (index >= 26 ? 2 * RecordBytes(4) : 0));
    }
    /// <summary>Computes the byte extent of a native seed record followed by its five three-lane spread records.</summary>
    /// <param name="seedParts">Number of parts in each seed.</param><returns>Total serialized bytes in one spread group.</returns>
    private static int SpreadGroupBytes(int seedParts) => RecordBytes(seedParts) + 5 * RecordBytes(3 * seedParts);
    /// <summary>Computes the within-group offset for a seed or one of its spread phases.</summary>
    /// <param name="seedParts">Number of parts in the seed record.</param><param name="phase">Zero for the seed; positive values select successive spread records.</param><returns>Byte offset from the start of the group.</returns>
    private static int SpreadPhaseOffset(int seedParts, int phase) =>
        phase == 0 ? 0 : RecordBytes(seedParts) + (phase - 1) * RecordBytes(3 * seedParts);
    /// <summary>Maps the compact selected Spazer identity index across physical groups, including a skipped unreferenced native pose.</summary>
    /// <param name="index">Zero-based index in the selected ordinary Spazer composition sequence.</param><returns>Bank-$93 spritemap address.</returns>
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
    /// <summary>Maps the selected charged Spazer sequence into packed axial, diagonal, and trailing axial groups.</summary>
    /// <param name="index">Zero-based selected composition index.</param><returns>Bank-$93 spritemap address.</returns>
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
    /// <summary>Calculates a selected Spazer startup address while retaining intervening unselected records in each physical group.</summary>
    /// <param name="index">Zero-based selected startup composition index.</param><returns>Bank-$93 spritemap address.</returns>
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
    /// <summary>Returns a super-missile explosion stage address, accounting for its larger ring record before the final pose.</summary>
    /// <param name="phase">Zero-based explosion stage.</param><returns>Bank-$93 composition address.</returns>
    private static ushort SuperMissileExplosionPointer(int phase) => (ushort)(SuperMissileExplosionStart +
        Math.Min(phase, 3) * RecordBytes(4) + (phase >= 4 ? RecordBytes(4 + 4 * 2) : 0) +
        (phase >= 5 ? RecordBytes(4 * 2) : 0));

    /// <summary>Native Charged_PW_PIW composition groups, identified by the actual directional instruction consumers at $93:8D4F/8D9B/8DE7/8E33 and $93:9C1B..9E3D.</summary>
    private enum PlasmaWaveShape
    {
        /// <summary>Four-part horizontal uncharged core.</summary>
        HorizontalShort,
        /// <summary>Seven-part horizontal charged core.</summary>
        HorizontalLong,
        /// <summary>Six-part down-right diagonal core.</summary>
        DownRightShort,
        /// <summary>Ten-part down-right diagonal core.</summary>
        DownRightLong,
        /// <summary>Four-part vertical uncharged core.</summary>
        VerticalShort,
        /// <summary>Seven-part vertical charged core.</summary>
        VerticalLong,
        /// <summary>Alternate seven-part horizontal core.</summary>
        HorizontalAlternate,
        /// <summary>Alternate twelve-part down-right core.</summary>
        DownRightAlternate,
        /// <summary>Alternate seven-part vertical core.</summary>
        VerticalAlternate,
        /// <summary>Alternate twelve-part down-left core.</summary>
        DownLeftAlternate,
        /// <summary>Six-part down-left diagonal core.</summary>
        DownLeftShort,
        /// <summary>Ten-part down-left diagonal core.</summary>
        DownLeftLong,
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
    /// <summary>Calculates a PlasmaWave composition address from its shape family and selected spread phase.</summary>
    /// <param name="index">Packed shape and phase index; each shape contributes five selected poses.</param><returns>Bank-$93 composition address.</returns>
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
    private enum PlasmaGrowthStage
    {
        /// <summary>Smallest selected startup pose.</summary>
        Core,
        /// <summary>Intermediate short startup footprint.</summary>
        Short,
        /// <summary>Expanded startup footprint before the terminal pose.</summary>
        Long,
        /// <summary>Largest selected startup footprint.</summary>
        Full
    }
    /// <summary>Calculates the selected startup composition address while accounting for unselected native growth records.</summary>
    /// <param name="index">Packed orientation group and selected growth stage.</param><returns>Bank-$93 startup composition address.</returns>
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
    /// <summary>$93:A252: Power-beam poses select OBJ tile $30..32 with priority2; those artwork choices remain required, while palette selects the installed CGRAM row.</summary>
    private const int PowerTile = 0x30, PowerPriority = 2;
    /// <summary>$90:ACDE-ACE8 loads the active beam colors at CGRAM224. OBJ attributes select a sixteen-color row in CGRAM's upper half, so its palette field derives from that destination.</summary>
    private const int PowerPalette = (SamusProjectileRomData.Palettes.BeamDestinationIndex - SnesCgram.ColorCount / 2) /
        GameplayBasePaletteFormat.SpriteColorCount;
    /// <summary>$82:E13E-E148 restores9A:81A0 at SpriteP5/CGRAM208. Fixed projectile artwork selects that installed sixteen-color row; its color payload remains required.</summary>
    private const int FixedProjectilePalette = (GameplayBasePaletteFormat.EnemyProjectileInitialColor - SnesCgram.ColorCount / 2) /
        GameplayBasePaletteFormat.SpriteColorCount;
    /// <summary>Recognizes a one-part Power or Ice beam composition and returns its animated glyph phase.</summary>
    /// <param name="pointer">Bank-$93 composition address.</param><param name="phase">Receives the phase in the matching beam family.</param>
    /// <returns><see langword="true"/> when the pointer identifies a single-part beam pose.</returns>
    internal static bool TrySingleBeamPhase(ushort pointer, out int phase) =>
        TryPhase(pointer, PowerStart, 1, 8, out phase) || TryPhase(pointer, IceStart, 1, 4, out phase);
    /// <summary>Classifies a composition pointer in a fixed-size sequence of equal-part-count records.</summary>
    /// <param name="pointer">Composition address to classify.</param><param name="start">First record address.</param><param name="parts">Part count, which determines each record's serialized size.</param><param name="count">Number of records in the sequence.</param><param name="phase">Receives the zero-based record index.</param>
    /// <returns><see langword="true"/> only for an aligned record start in the sequence.</returns>
    private static bool TryPhase(ushort pointer, ushort start, int parts, int count, out int phase)
    {
        int relative = pointer - start;
        phase = relative / RecordBytes(parts);
        return relative >= 0 && relative % RecordBytes(parts) == 0 && phase < count;
    }
    /// <summary>Identifies a four-part charged Power or Ice pose and indicates which tile-order policy applies.</summary>
    /// <param name="pointer">Composition address.</param><param name="phase">Receives the selected pose index.</param><param name="ice">Receives <see langword="true"/> for the Ice family.</param>
    /// <returns><see langword="true"/> for a recognized charged-beam composition.</returns>
    internal static bool TryChargedBeamPhase(ushort pointer, out int phase, out bool ice)
    {
        ice = false;
        if (TryPhase(pointer, ChargedPowerStart, 4, 16, out phase))
            return phase != 0; // EC3E's independently selected ordering remains required.
        ice = true;
        return TryPhase(pointer, ChargedIceStart, 4, 4, out phase);
    }
    /// <summary>$93:ADD5/ADF2/AE0F/AE2C: axial SuperMissile poses, separated by one two-part axial and one three-part diagonal record.</summary>
    /// <summary>Recognizes the four axial SuperMissile compositions, skipping the intervening diagonal records.</summary>
    /// <param name="pointer">Composition address.</param><param name="pose">Receives the axial orientation and reflection index.</param>
    /// <returns><see langword="true"/> for an axial record start in the SuperMissile group.</returns>
    internal static bool TryAxialSuperMissilePose(ushort pointer, out int pose)
    {
        int offset = pointer - SuperMissileStart;
        int stride = RecordBytes(2) + RecordBytes(3);
        pose = offset / stride;
        return offset >= 0 && offset < 4 * stride && offset % stride == 0;
    }
    /// <summary>$93:ADDC/ADF9: independently selected horizontal/vertical SuperMissile glyph roots and priority. These artwork inputs remain required under frames; palette selects the installed fixed-projectile CGRAM row.</summary>
    private const int SuperMissileHorizontalGlyph = 0x65, SuperMissileVerticalGlyph = 0x69,
        SuperMissilePriority = 2;
    /// <summary>Four axial native poses use a centered two-cell strip, reflected with travel direction; independent glyph/style/pixel content remains required.</summary>
    /// <summary>Provides the two OAM tiles of an axial SuperMissile pose in native order.</summary>
    /// <param name="pose">Decoded orientation and reflection selector from the native composition address.</param>
    internal readonly struct AxialSuperMissileParts(int pose) : IReadOnlyList<CompiledSpritePart>
    {
        /// <summary>The axial strip consists of two adjacent OAM tiles.</summary>
        public int Count => 2;
        /// <summary>Returns one tile in native traversal order, reflecting the strip for the reverse pose.</summary>
        /// <param name="index">Tile index 0 or 1.</param><exception cref="IndexOutOfRangeException">The index is outside the two-tile strip.</exception>
        public CompiledSpritePart this[int index]
        {
            get
            {
                if ((uint)index >= 2) throw new IndexOutOfRangeException();
                bool vertical = (pose & 1) != 0;
                bool reflected = pose >= 2;
                int x = vertical ? -8 / 2 : -index * 8;
                int y = vertical ? (index - 1) * 8 : -8 / 2;
                if (reflected)
                {
                    if (vertical) y = -8 - y;
                    else x = -8 - x;
                }
                int tile = vertical ? SuperMissileVerticalGlyph + index : SuperMissileHorizontalGlyph - index;
                var flips = reflected ? (vertical ? SnesTileFlipFlags.Vertical : SnesTileFlipFlags.Horizontal) : 0;
                return new(SnesSpritemapXWord.Create(x, false), unchecked((byte)y),
                    SnesObjAttributeWord.Create(tile, FixedProjectilePalette, SuperMissilePriority, flips), false);
            }
        }
        /// <summary>Enumerates the two tiles of the axial SuperMissile pose in native order.</summary>
        public IEnumerator<CompiledSpritePart> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>$93:A24D..A27E: eight centered one-tile Power poses (also the first four Ice poses at $93:EDF6) traverse a triangular three-glyph cycle and rotate its horizontal/vertical reflection phases.</summary>
    /// <summary>Builds the single-tile animated Power or Ice beam pose for a selected phase.</summary>
    /// <param name="phase">Native pose phase, controlling glyph and reflection.</param>
    internal readonly struct SingleBeamParts(int phase) : IReadOnlyList<CompiledSpritePart>
    {
        /// <summary>A single-beam pose is represented by one OAM part.</summary>
        public int Count => 1;
        /// <summary>Returns the sole animated beam tile.</summary>
        /// <param name="index">Must be zero.</param><exception cref="IndexOutOfRangeException">The index is not zero.</exception>
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
        /// <summary>Enumerates the single animated Power or Ice beam tile.</summary>
        public IEnumerator<CompiledSpritePart> GetEnumerator()
        {
            yield return this[0];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>$93:EC54..ED88 and ED9E..EDE0 mirror one glyph across a centered two-by-two cell square; Power uses column order and Ice row order.</summary>
    /// <summary>Builds a four-quadrant charged beam using the traversal order of its Power or Ice family.</summary>
    /// <param name="phase">Selected charged pose, including the glyph-bank phase.</param><param name="ice">Selects Ice's row-first traversal when true and Power's column-first traversal otherwise.</param>
    internal readonly struct ChargedBeamParts(int phase, bool ice) : IReadOnlyList<CompiledSpritePart>
    {
        /// <summary>The charged beam is a four-tile square.</summary>
        public int Count => 4;
        /// <summary>Returns one quadrant in the family-specific native traversal order.</summary>
        /// <param name="index">Zero-based quadrant index from 0 through 3.</param><exception cref="IndexOutOfRangeException">The index is outside the four-part square.</exception>
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
        /// <summary>Enumerates the four charged-beam quadrants in the selected family's traversal order.</summary>
        public IEnumerator<CompiledSpritePart> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>$93:EC43/EDA3: charged Power/Ice use independently selected OBJ glyphs $33/$34, sharing palette6/priority2 with the single-tile beam.</summary>
    private const int ChargedBeamTile = 0x33;
    /// <summary>Maps a one-part Wave composition pointer to its position among the centered pose and directional displacement stages.</summary>
    /// <param name="pointer">Composition address in the ordinary Wave group.</param><param name="phase">Receives the native sequence index.</param>
    /// <returns><see langword="true"/> for one of the 33 one-part Wave records.</returns>
    internal static bool TryWavePhase(ushort pointer, out int phase) => TryPhase(pointer, WaveStart, 1, 33, out phase);
    /// <summary>$93:AEA4..AEC0 and other diagonal groups: component magnitudes6/9/11/12 equal floor(3*axial/4). This selected ratio remains required, not a trigonometric assertion.</summary>
    private const int WaveDiagonalNumerator = 3, WaveDiagonalDenominator = 4;
    /// <summary>Signed compass displacements used to lay out the native Wave spread poses.</summary>
    private enum WaveDirection
    {
        /// <summary>Displacement toward the top of the screen.</summary>
        Up,
        /// <summary>Displacement toward the bottom of the screen.</summary>
        Down,
        /// <summary>Displacement toward the upper right.</summary>
        UpRight,
        /// <summary>Displacement toward the lower left.</summary>
        DownLeft,
        /// <summary>Displacement toward the right.</summary>
        Right,
        /// <summary>Displacement toward the left.</summary>
        Left,
        /// <summary>Displacement toward the upper left.</summary>
        UpLeft,
        /// <summary>Displacement toward the lower right.</summary>
        DownRight
    }
    /// <summary>$93:AE65..AF4B: one centered pose then four stages in each of eight signed directional displacement groups.</summary>
    /// <summary>Creates the centered one-tile ordinary Wave pose for a native direction and displacement phase.</summary>
    /// <param name="phase">Zero selects the centered pose; later values encode direction and displacement step.</param>
    internal readonly struct WaveParts(int phase) : IReadOnlyList<CompiledSpritePart>
    {
        /// <summary>Every native Wave phase contains one OAM part.</summary>
        public int Count => 1;
        /// <summary>Returns the single tile at the requested collection index.</summary>
        /// <param name="index">Must be zero, the only valid index.</param><exception cref="IndexOutOfRangeException">The index is not zero.</exception>
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
                    int distance = SuperMetroid.Core.Game.ProjectileWaveEnvelopeDefinitions.AxialLobeDistance(step);
                    if (dx != 0 && dy != 0) distance = distance * WaveDiagonalNumerator / WaveDiagonalDenominator;
                    x += dx * distance;
                    y += dy * distance;
                    tile += (step + 1) / 2;
                }
                return new(SnesSpritemapXWord.Create(x, false), unchecked((byte)y),
                    SnesObjAttributeWord.Create(tile, PowerPalette, PowerPriority, 0), false);
            }
        }
        /// <summary>Enumerates the sole tile in the selected ordinary Wave pose.</summary>
        public IEnumerator<CompiledSpritePart> GetEnumerator() { yield return this[0]; }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>Recognizes vertical charged Wave and IceWave seed/spread compositions.</summary>
    /// <param name="pointer">Bank-$93 composition address.</param><param name="phase">Receives the vertical phase index shared by the recognized families.</param>
    /// <returns><see langword="true"/> for a vertical seed or one of its selected spread poses.</returns>
    /// <summary>Recognizes vertical charged Wave and IceWave seed or spread compositions.</summary>
    /// <param name="pointer">Bank-$93 composition address.</param><param name="phase">Receives the phase index shared by the recognized families.</param>
    /// <returns><see langword="true"/> for a vertical seed or selected spread pose.</returns>
    internal static bool TryVerticalChargedWavePhase(ushort pointer, out int phase) =>
        TryVerticalChargedWavePhase(pointer, ChargedWaveStart, out phase) ||
        TryVerticalChargedWavePhase(pointer, ChargedIceWaveStart, out phase);
    /// <summary>Classifies vertical records within one charged Wave family, skipping its unselected intervening compositions.</summary>
    /// <param name="pointer">Composition address.</param><param name="start">Family's first core address.</param><param name="phase">Receives the seed or spread phase.</param>
    /// <returns><see langword="true"/> when the pointer is one of the selected vertical records.</returns>
    /// <summary>Classifies vertical records within one charged Wave family, skipping its unselected compositions.</summary>
    /// <param name="pointer">Composition address.</param><param name="start">Family's first core address.</param><param name="phase">Receives the seed or spread phase.</param>
    /// <returns><see langword="true"/> when the pointer is a selected vertical record.</returns>
    private static bool TryVerticalChargedWavePhase(ushort pointer, ushort start, out int phase)
    {
        if (TryPhase(pointer, start, 4, 2, out phase)) return true;
        if (!TryPhase(pointer, (ushort)(start + 2 * RecordBytes(4)), 8, 8, out phase)) return false;
        phase += 2;
        return true;
    }
    /// <summary>$93:AF4C..B0C7 and B672..B7ED: two centered glyphs followed by four paired vertical displacements, each with swapped lobe glyphs.</summary>
    /// <summary>Provides the seed or vertical eight-part lobe spread for a charged Wave family.</summary>
    /// <param name="phase">Selected vertical composition phase; initial phases are the four-part core.</param>
    internal readonly struct VerticalChargedWaveParts(int phase) : IReadOnlyList<CompiledSpritePart>
    {
        /// <summary>The vertical charged-wave composition contains four seed or eight spread parts.</summary>
        public int Count => phase < 2 ? 4 : 8;
        /// <summary>Returns the selected part in native record order.</summary>
        /// <param name="index">Zero-based part position in this phase.</param><exception cref="IndexOutOfRangeException">The index exceeds the current phase's part count.</exception>
        public CompiledSpritePart this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new IndexOutOfRangeException();
                int lobe = index / 4, quadrant = index % 4;
                int column = quadrant % 2, row = quadrant / 2;
                int centerY = phase < 2 ? 0 : (lobe == 0 ? 1 : -1) * SuperMetroid.Core.Game.ProjectileWaveEnvelopeDefinitions.AxialLobeDistance((phase - 2) / 2);
                int tile = ChargedBeamTile + (phase < 2 ? 1 - phase : (phase + lobe) % 2);
                var flips = (column == 0 ? SnesTileFlipFlags.Horizontal : 0) |
                    (row == 0 ? SnesTileFlipFlags.Vertical : 0);
                return new(SnesSpritemapXWord.Create(-column * 8, false), unchecked((byte)(centerY - row * 8)),
                    SnesObjAttributeWord.Create(tile, PowerPalette, PowerPriority, flips), false);
            }
        }
        /// <summary>Enumerates the vertical charged-Wave core or its two four-tile lobes in native order.</summary>
        public IEnumerator<CompiledSpritePart> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>Native93:AD6D/AD8A/ADA7/ADC4 andADE1/ADFE/AE1B/AE38: diagonal records follow each two-part axial record.</summary>
    /// <summary>Recognizes diagonal Missile or SuperMissile records and identifies the selected projectile artwork family.</summary>
    /// <param name="pointer">Composition address.</param><param name="pose">Receives the diagonal orientation.</param><param name="super">Receives <see langword="true"/> for SuperMissile.</param>
    /// <returns><see langword="true"/> when the pointer identifies a diagonal record in either missile group.</returns>
    /// <summary>Recognizes diagonal Missile or SuperMissile records and identifies the artwork family.</summary>
    /// <param name="pointer">Composition address.</param><param name="pose">Receives the diagonal orientation.</param><param name="super">Receives <see langword="true"/> for SuperMissile.</param>
    /// <returns><see langword="true"/> when the pointer identifies a diagonal record in either missile group.</returns>
    internal static bool TryDiagonalMissilePose(ushort pointer, out int pose, out bool super)
    {
        super = false;
        if (TryDiagonalMissilePose(pointer, MissileStart, out pose)) return true;
        super = true;
        return TryDiagonalMissilePose(pointer, SuperMissileStart, out pose);
    }
    /// <summary>Maps a three-part diagonal missile record within one missile family to its orientation.</summary>
    /// <param name="pointer">Composition address.</param><param name="start">First address in the selected missile group.</param><param name="pose">Receives the diagonal orientation index.</param>
    /// <returns><see langword="true"/> for a diagonal record start, excluding axial records.</returns>
    private static bool TryDiagonalMissilePose(ushort pointer, ushort start, out int pose)
    {
        int offset = pointer - start - RecordBytes(2);
        int stride = RecordBytes(2) + RecordBytes(3);
        pose = offset / stride;
        return offset >= 0 && offset < 4 * stride && offset % stride == 0;
    }
    /// <summary>Native AD6F andADE3 L-footprint origins. These four independently selected pivot coordinates remain REQUIRED artwork inputs.</summary>
    private const int MissileDiagonalX = -8, MissileDiagonalY = -11,
        SuperMissileDiagonalX = -6, SuperMissileDiagonalY = -10;
    /// <summary>Native AD72/ADF0 glyph roots and AD6F/ADE3 first corner in bottom-right,bottom-left,top-left order; both the chosen three-corner footprint and these artwork/order seeds remain REQUIRED.</summary>
    private const int MissileDiagonalGlyph = 0x56, SuperMissileDiagonalGlyph = 0x66,
        MissileFirstCorner = 2, SuperMissileFirstCorner = 0;
    /// <summary>Builds the three-corner diagonal footprint for a Missile or SuperMissile pose.</summary>
    /// <param name="pose">Diagonal orientation and reflection index.</param><param name="super">Selects SuperMissile's independently authored footprint and glyphs.</param>
    internal readonly struct DiagonalMissileParts(int pose, bool super) : IReadOnlyList<CompiledSpritePart>
    {
        /// <summary>Diagonal missile artwork uses three neighboring OAM parts.</summary>
        public int Count => 3;
        /// <summary>Returns a tile from the selected diagonal missile pose.</summary>
        /// <param name="index">Zero-based part index from 0 through 2.</param><exception cref="IndexOutOfRangeException">The index is outside the three-part pose.</exception>
        public CompiledSpritePart this[int index]
        {
            get
            {
                if ((uint)index >= 3) throw new IndexOutOfRangeException();
                int corner = (index + (super ? SuperMissileFirstCorner : MissileFirstCorner)) % 3;
                int x = (super ? SuperMissileDiagonalX : MissileDiagonalX) + (corner == 0 ? 8 : 0);
                int y = (super ? SuperMissileDiagonalY : MissileDiagonalY) + (corner == 2 ? 0 : 8);
                bool flipX = pose is 1 or 2;
                bool flipY = pose >= 2;
                if (flipX) x = -8 - x;
                if (flipY) y = -8 - y;
                int tile = (super ? SuperMissileDiagonalGlyph : MissileDiagonalGlyph) + 2 - corner;
                var flips = (flipX ? SnesTileFlipFlags.Horizontal : 0) | (flipY ? SnesTileFlipFlags.Vertical : 0);
                return new(SnesSpritemapXWord.Create(x, false), unchecked((byte)y),
                    SnesObjAttributeWord.Create(tile, FixedProjectilePalette, SuperMissilePriority, flips), false);
            }
        }
        /// <summary>Enumerates the three tiles of the selected diagonal missile pose.</summary>
        public IEnumerator<CompiledSpritePart> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>$93:AD45-AD60: four centered single-cell Bomb poses use consecutive glyphs from4C. Glyph origin/priority remain REQUIRED artwork inputs; palette selects the installed fixed-projectile row.</summary>
    private const int BombGlyphStart = 0x4c, BombPalette = FixedProjectilePalette, EffectPriority = 3;
    /// <summary>$93:ABC1-AC18: four mirrored quadrant beam-explosion stages use consecutive glyphs60-63 and the installed beam-color row. Glyph selection/priority/pixels remain REQUIRED.</summary>
    private const int BeamExplosionQuadGlyphStart = 0x60, BeamExplosionPalette = PowerPalette;
    /// <summary>Recognizes single-tile bomb poses and the four-part beam-explosion poses.</summary>
    /// <param name="pointer">Composition address.</param><param name="phase">Receives the pose index in its family.</param><param name="quad">Receives <see langword="true"/> when the pose uses four parts.</param>
    /// <returns><see langword="true"/> for a supported simple effect composition.</returns>
    internal static bool TrySimpleEffectPose(ushort pointer, out int phase, out bool quad)
    {
        quad = false;
        if (TryPhase(pointer, BombStart, 1, 4, out phase)) return true;
        quad = true;
        return TryPhase(pointer, BeamExplosionPointer(2), 4, 4, out phase);
    }
    /// <summary>Provides either a single Bomb tile or a four-part beam-explosion pose.</summary>
    /// <param name="phase">Selected effect stage and glyph offset.</param><param name="quad">Chooses the four-quadrant explosion form when true.</param>
    internal readonly struct SimpleEffectParts(int phase, bool quad) : IReadOnlyList<CompiledSpritePart>
    {
        /// <summary>Single-tile effects contain one part; expanded explosion poses contain four.</summary>
        public int Count => quad ? 4 : 1;
        /// <summary>Returns the requested tile of the effect pose.</summary>
        /// <param name="index">Zero-based part position.</param><exception cref="IndexOutOfRangeException">The index exceeds the pose's part count.</exception>
        public CompiledSpritePart this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new IndexOutOfRangeException();
                int column = index / 2, row = index % 2;
                int x = quad ? -column * 8 : -8 / 2;
                int y = quad ? -row * 8 : -8 / 2;
                int tile = (quad ? BeamExplosionQuadGlyphStart : BombGlyphStart) + phase;
                var flips = quad ? (column == 0 ? SnesTileFlipFlags.Horizontal : 0) |
                    (row == 0 ? SnesTileFlipFlags.Vertical : 0) : 0;
                return new(SnesSpritemapXWord.Create(x, false), unchecked((byte)y),
                    SnesObjAttributeWord.Create(tile, quad ? BeamExplosionPalette : BombPalette, EffectPriority, flips), false);
            }
        }
        /// <summary>Enumerates the selected single-part or four-part effect composition.</summary>
        public IEnumerator<CompiledSpritePart> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>Recognizes the selected eight-part horizontal spread records in charged Wave and IceWave groups.</summary>
    /// <param name="pointer">Composition address.</param><param name="phase">Receives the spread phase.</param>
    /// <returns><see langword="true"/> for one of the selected horizontal spread records.</returns>
    /// <summary>Recognizes selected eight-part horizontal spread records in charged Wave and IceWave groups.</summary>
    /// <param name="pointer">Composition address.</param><param name="phase">Receives the spread phase.</param>
    /// <returns><see langword="true"/> for one of the selected horizontal spread records.</returns>
    internal static bool TryHorizontalChargedWavePhase(ushort pointer, out int phase) =>
        TryPhase(pointer, ChargedWavePointer(ChargedWaveStart, 26), 8, 8, out phase) ||
        TryPhase(pointer, ChargedWavePointer(ChargedIceWaveStart, 26), 8, 8, out phase);
    /// <summary>$93:B394-B4E3 and BABA-BC09: paired horizontal lobes reuse the four required axial distances and alternating glyphs. The independently chosen left clockwise/right row-order traversal remains REQUIRED artwork policy.</summary>
    /// <summary>Builds the two four-part lobes of a horizontal charged Wave spread.</summary>
    /// <param name="phase">Selected spread stage controlling lobe displacement and glyph pair.</param>
    internal readonly struct HorizontalChargedWaveParts(int phase) : IReadOnlyList<CompiledSpritePart>
    {
        /// <summary>Each horizontal charged-wave spread is represented by eight parts.</summary>
        public int Count => 8;
        /// <summary>Returns the requested tile in the two-lobe native order.</summary>
        /// <param name="index">Zero-based part position from 0 through 7.</param><exception cref="IndexOutOfRangeException">The index exceeds the eight-part spread.</exception>
        public CompiledSpritePart this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new IndexOutOfRangeException();
                int lobe = index / 4, corner = index % 4;
                bool left = lobe == 0;
                // The left lobe follows upper-right,lower-right,lower-left,upper-left;
                // the right lobe follows the native row traversal. Selection of these
                // two orders is still a required source input, not a derived choice.
                int column = left ? corner / 2 : corner % 2;
                int row = left ? 1 - ((corner ^ (corner >> 1)) & 1) : corner / 2;
                int centerX = (left ? -1 : 1) * SuperMetroid.Core.Game.ProjectileWaveEnvelopeDefinitions.AxialLobeDistance(phase / 2);
                int tile = ChargedBeamTile + 1 - (phase + lobe) % 2;
                var flips = (column == 0 ? SnesTileFlipFlags.Horizontal : 0) |
                    (row == 0 ? SnesTileFlipFlags.Vertical : 0);
                return new(SnesSpritemapXWord.Create(centerX - column * 8, false), unchecked((byte)(-row * 8)),
                    SnesObjAttributeWord.Create(tile, PowerPalette, PowerPriority, flips), false);
            }
        }
        /// <summary>Enumerates the eight tiles of a horizontal charged-Wave spread in native lobe order.</summary>
        public IEnumerator<CompiledSpritePart> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>Classifies a Spazer seed record and returns its orientation across the diagonal and axial groups.</summary>
    /// <param name="pointer">Composition address.</param><param name="pose">Receives the seed orientation index.</param>
    /// <returns><see langword="true"/> for one of the eight native seed records.</returns>
    internal static bool TrySpazerSeedPose(ushort pointer, out int pose)
    {
        int offset = pointer - SpazerStart;
        int diagonalGroupBytes = SpreadGroupBytes(4);
        if (offset >= 0 && offset < 4 * diagonalGroupBytes && offset % diagonalGroupBytes == 0)
        {
            pose = offset / diagonalGroupBytes;
            return true;
        }
        offset -= 4 * diagonalGroupBytes;
        int axialGroupBytes = SpreadGroupBytes(2);
        pose = 4 + offset / axialGroupBytes;
        return offset >= 0 && offset < 4 * axialGroupBytes && offset % axialGroupBytes == 0;
    }
    /// <summary>$93:D10E/D25A/D3A6/D4F2/D63E/D6EA/D796/D842: native seed travel directions; chosen footprint lengths/glyphs/priority remain REQUIRED.</summary>
    private enum SpazerSeedDirection { UpRight, DownRight, DownLeft, UpLeft, Down, Left, Up, Right }
    /// <summary>$93:D113/D118 and D643/D6EF: selected diagonal32/31,vertical33,horizontal30 atlas cells remain REQUIRED artwork identities.</summary>
    private const int SpazerDiagonalGlyph = 0x32, SpazerVerticalGlyph = 0x33, SpazerHorizontalGlyph = 0x30;
    /// <summary>Provides the four-cell diagonal or two-cell axial seed in a decoded Spazer orientation.</summary>
    /// <param name="pose">Native seed orientation index: diagonal groups precede axial groups.</param>
    internal readonly struct SpazerSeedParts(int pose) : IReadOnlyList<CompiledSpritePart>
    {
        /// <summary>Number of OAM tiles in this seed: four for axial orientations and two for diagonal orientations.</summary>
        public int Count => pose < 4 ? 4 : 2;
        /// <summary>Gets one seed tile in the native composition order for the decoded orientation.</summary>
        /// <param name="index">Zero-based tile position within this seed.</param>
        /// <exception cref="IndexOutOfRangeException">The index is outside this orientation's seed.</exception>
        public CompiledSpritePart this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new IndexOutOfRangeException();
                int size = SpazerCompositionGeometryDefinitions.TileSize;
                SpazerSeedDirection direction = (SpazerSeedDirection)pose;
                int x, y, tile;
                bool flipX, flipY;
                if (pose < 4)
                {
                    x = SpazerCompositionGeometryDefinitions.DiagonalFirstPairOriginX + (index / 2 + index % 2) * size;
                    y = SpazerCompositionGeometryDefinitions.DiagonalFirstPairOriginY - (index / 2) * size;
                    flipX = direction is SpazerSeedDirection.UpRight or SpazerSeedDirection.DownRight;
                    flipY = direction is SpazerSeedDirection.DownRight or SpazerSeedDirection.DownLeft;
                    if (!flipX) x = -size - x;
                    if (flipY) y = -size - y;
                    tile = SpazerDiagonalGlyph - index % 2;
                }
                else
                {
                    bool vertical = direction is SpazerSeedDirection.Up or SpazerSeedDirection.Down;
                    bool reverse = direction is SpazerSeedDirection.Down or SpazerSeedDirection.Right;
                    int along = (reverse ? index - 1 : -index) * size;
                    x = vertical ? -size / 2 : along;
                    y = vertical ? along : SpazerCompositionGeometryDefinitions.HorizontalStripOriginY;
                    flipX = direction == SpazerSeedDirection.Right;
                    flipY = direction == SpazerSeedDirection.Down;
                    tile = vertical ? SpazerVerticalGlyph : SpazerHorizontalGlyph;
                }
                var flips = (flipX ? SnesTileFlipFlags.Horizontal : 0) | (flipY ? SnesTileFlipFlags.Vertical : 0);
                return new(SnesSpritemapXWord.Create(x, false), unchecked((byte)y),
                    SnesObjAttributeWord.Create(tile, PowerPalette, PowerPriority, flips), false);
            }
        }
        /// <summary>Enumerates the seed tiles in the decoded Spazer orientation.</summary>
        public IEnumerator<CompiledSpritePart> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>Recognizes a selected diagonal Spazer spread record and decodes its direction and spread stage.</summary>
    /// <param name="pointer">Composition address.</param><param name="pose">Receives the diagonal orientation.</param><param name="phase">Receives the spread stage within that orientation.</param>
    /// <returns><see langword="true"/> for an aligned selected spread record in a diagonal group.</returns>
    internal static bool TrySpazerDiagonalSpread(ushort pointer, out int pose, out int phase)
    {
        int offset = pointer - SpazerStart;
        int groupBytes = SpreadGroupBytes(4);
        pose = offset / groupBytes;
        int withinGroup = offset % groupBytes - RecordBytes(4);
        phase = withinGroup / RecordBytes(12);
        return offset >= 0 && offset < 4 * groupBytes && withinGroup >= 0 &&
            withinGroup < 4 * RecordBytes(12) && withinGroup % RecordBytes(12) == 0;
    }
    /// <summary>The first four diagonal spreads atD124/D270/D3BC/D508 repeat each seed on center and opposite perpendicular lanes. Distances6/9/11/12 share floor(3*required axial distance/4); spacing/projection and phase-selection inputs remain REQUIRED.</summary>
    /// <summary>Builds the twelve-part, three-lane diagonal spread by repeating the oriented four-part seed.</summary>
    /// <param name="pose">Diagonal seed orientation.</param><param name="phase">Selected spread phase determining lane separation.</param>
    internal readonly struct SpazerDiagonalSpreadParts(int pose, int phase) : IReadOnlyList<CompiledSpritePart>
    {
        /// <summary>Each diagonal spread pose contains two three-tile lobes.</summary>
        public int Count => 12;
        /// <summary>Gets a tile from the selected diagonal spread lobes in their native traversal order.</summary>
        /// <param name="index">Zero-based tile position across both lobes.</param>
        /// <exception cref="IndexOutOfRangeException">The index is outside the twelve-tile spread.</exception>
        public CompiledSpritePart this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new IndexOutOfRangeException();
                var seed = new SpazerSeedParts(pose)[index % 4];
                int lane = index / 4;
                int direction = lane == 0 ? 0 : lane == 1 ? -1 : 1;
                int distance = WaveDiagonalNumerator * SuperMetroid.Core.Game.ProjectileWaveEnvelopeDefinitions.AxialLobeDistance(phase) / WaveDiagonalDenominator;
                int dx = direction * distance * (pose >= 2 ? -1 : 1);
                int dy = direction * distance * (pose is 1 or 2 ? -1 : 1);
                return new(SnesSpritemapXWord.Create(seed.X.SignedOffset + dx, seed.X.IsLarge),
                    unchecked((byte)((sbyte)seed.Y + dy)), seed.Attributes, seed.InheritPalette);
            }
        }
        /// <summary>Enumerates all twelve tiles of the selected diagonal Spazer spread.</summary>
        public IEnumerator<CompiledSpritePart> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>$93:D64A/D6F6/D7A2/D84E: axial spread records follow their two-cell seeds. D84E's distinct flip/order policy remains required.</summary>
    internal static bool TrySpazerAxialSpread(ushort pointer, out int pose, out int phase)
    {
        int offset = pointer - SpazerStart - 4 * SpreadGroupBytes(4);
        int groupBytes = SpreadGroupBytes(2);
        pose = 4 + offset / groupBytes;
        int withinGroup = offset % groupBytes - RecordBytes(2);
        phase = withinGroup / RecordBytes(6);
        return offset >= 0 && offset < 4 * groupBytes && withinGroup >= 0 &&
            withinGroup < 5 * RecordBytes(6) && withinGroup % RecordBytes(6) == 0 &&
            !(pose == 7 && phase == 0);
    }
    /// <summary>Repeated axial seed lanes. Native vertical, initial-left and later horizontal traversal policies remain REQUIRED composition choices.</summary>
    /// <summary>Builds three repeated two-cell lanes for an axial Spazer spread.</summary>
    /// <param name="pose">Axial seed orientation.</param><param name="phase">Seed or spread phase controlling lane spacing and order.</param>
    internal readonly struct SpazerAxialSpreadParts(int pose, int phase) : IReadOnlyList<CompiledSpritePart>
    {
        /// <summary>Each axial spread pose contains two three-tile lobes.</summary>
        public int Count => 6;
        /// <summary>Gets a tile from the selected axial spread lobes in native order.</summary>
        /// <param name="index">Zero-based tile position across both lobes.</param>
        /// <exception cref="IndexOutOfRangeException">The index is outside the six-tile spread.</exception>
        public CompiledSpritePart this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new IndexOutOfRangeException();
                int distance = phase == 0 ? SuperMetroid.Core.Game.ProjectileWaveEnvelopeDefinitions.SpazerInitialAxialSpread : SuperMetroid.Core.Game.ProjectileWaveEnvelopeDefinitions.AxialLobeDistance(phase - 1);
                bool vertical = pose is 4 or 6;
                int seedIndex, lane;
                if (vertical)
                {
                    seedIndex = index % 2;
                    lane = index / 2 == 0 ? 1 : index / 2 == 1 ? -1 : 0;
                }
                else if (phase == 0)
                {
                    seedIndex = index % 2;
                    lane = 1 - index / 2;
                }
                else
                {
                    seedIndex = index / 3;
                    lane = seedIndex == 0 ? index % 3 - 1 : 1 - index % 3;
                }
                var seed = new SpazerSeedParts(pose)[seedIndex];
                return new(SnesSpritemapXWord.Create(seed.X.SignedOffset + (vertical ? lane * distance : 0), seed.X.IsLarge),
                    unchecked((byte)((sbyte)seed.Y + (vertical ? 0 : lane * distance))), seed.Attributes, seed.InheritPalette);
            }
        }
        /// <summary>Enumerates the six tiles of the selected axial Spazer spread.</summary>
        public IEnumerator<CompiledSpritePart> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>$93:D8EE..DA39: horizontal charged Spazer seed and five spread records, ending before the vertical family.</summary>
    internal static bool TryHorizontalChargedSpazer(ushort pointer, out int phase)
    {
        if (pointer == ChargedSpazerStart) { phase = 0; return true; }
        if (!TryPhase(pointer, (ushort)(ChargedSpazerStart + RecordBytes(4)), 12, 5, out phase)) return false;
        phase++;
        return true;
    }
    /// <summary>$93:D8EE: the chosen four-cell length and tile34 artwork remain REQUIRED inputs; cell adjacency and centering calculate.</summary>
    private const int ChargedSpazerHorizontalCells = 4, ChargedSpazerHorizontalGlyph = 0x34;
    /// <summary>Horizontal charged Spazer lanes share required spread distances. Center-first initial versus lower-first later ordering remains REQUIRED composition policy.</summary>
    /// <summary>Builds the centered horizontal charged Spazer seed or its three displaced copies.</summary>
    /// <param name="phase">Selected charged Spazer spread phase.</param>
    internal readonly struct HorizontalChargedSpazerParts(int phase) : IReadOnlyList<CompiledSpritePart>
    {
        /// <summary>Returns four tiles for the centered core or twelve across its three horizontal copies.</summary>
        public int Count => ChargedSpazerHorizontalCells * (phase == 0 ? 1 : 3);
        /// <summary>Gets a core tile or a tile in one of the phase-selected horizontal lobes.</summary>
        /// <param name="index">Zero-based position in the native core/lobe traversal.</param>
        /// <exception cref="IndexOutOfRangeException">The index is outside this phase's composition.</exception>
        public CompiledSpritePart this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new IndexOutOfRangeException();
                int lane = index / ChargedSpazerHorizontalCells;
                int direction = phase <= 1 ? (lane == 0 ? 0 : lane == 1 ? 1 : -1) : 1 - lane;
                int distance = phase <= 1 ? SuperMetroid.Core.Game.ProjectileWaveEnvelopeDefinitions.SpazerInitialAxialSpread : SuperMetroid.Core.Game.ProjectileWaveEnvelopeDefinitions.AxialLobeDistance(phase - 2);
                int size = SpazerCompositionGeometryDefinitions.TileSize;
                int x = (ChargedSpazerHorizontalCells / 2 - 1 - index % ChargedSpazerHorizontalCells) * size;
                int y = SpazerCompositionGeometryDefinitions.HorizontalStripOriginY + direction * distance;
                return new(SnesSpritemapXWord.Create(x, false), unchecked((byte)y),
                    SnesObjAttributeWord.Create(ChargedSpazerHorizontalGlyph, PowerPalette, PowerPriority, 0), false);
            }
        }
        /// <summary>Enumerates the horizontal charged-Spazer core or its displaced copies.</summary>
        public IEnumerator<CompiledSpritePart> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>$93:DA8E..DB85: later vertical charged Spazer spreads follow the vertical seed and its first spread. Initial DA3A/DA50 ordering remains required.</summary>
    internal static bool TryVerticalChargedSpazerSpread(ushort pointer, out int phase) =>
        TryPhase(pointer, (ushort)(ChargedSpazerStart + SpreadGroupBytes(4) + RecordBytes(4) + RecordBytes(12)), 12, 4, out phase);
    /// <summary>$93:DA8E: the selected four-cell column and glyph37 remain REQUIRED artwork inputs.</summary>
    private const int ChargedSpazerVerticalCells = 4, ChargedSpazerVerticalGlyph = 0x37;
    /// <summary>Three descending columns. Side placement measures a required distance to the near cell edge and reflects across X=0; this placement policy and right/left/center traversal remain REQUIRED choices.</summary>
    /// <summary>Builds the three descending four-cell columns of a vertical charged Spazer spread.</summary>
    /// <param name="phase">Spread phase controlling the distance of the two side columns.</param>
    internal readonly struct VerticalChargedSpazerSpreadParts(int phase) : IReadOnlyList<CompiledSpritePart>
    {
        /// <summary>Three descending lanes each contain the four cells selected by the vertical spread.</summary>
        public int Count => 3 * ChargedSpazerVerticalCells;
        /// <summary>Gets a cell from the selected vertical spread lane and row.</summary>
        /// <param name="index">Zero-based position in right, left, then centered lane order.</param>
        /// <exception cref="IndexOutOfRangeException">The index is outside the three-lane composition.</exception>
        public CompiledSpritePart this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new IndexOutOfRangeException();
                int size = SpazerCompositionGeometryDefinitions.TileSize;
                int lane = index / ChargedSpazerVerticalCells;
                int distance = SuperMetroid.Core.Game.ProjectileWaveEnvelopeDefinitions.AxialLobeDistance(phase);
                int x = lane == 0 ? distance : lane == 1 ? -size - distance : -size / 2;
                int y = (ChargedSpazerVerticalCells / 2 - 1 - index % ChargedSpazerVerticalCells) * size;
                return new(SnesSpritemapXWord.Create(x, false), unchecked((byte)y),
                    SnesObjAttributeWord.Create(ChargedSpazerVerticalGlyph, PowerPalette, PowerPriority, 0), false);
            }
        }
        /// <summary>Enumerates the twelve tiles of the vertical charged-Spazer spread by lane.</summary>
        public IEnumerator<CompiledSpritePart> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>$93:EE12..F085: each startup orientation group begins with a one-cell and two-cell axial strip, before its diagonal records.</summary>
    internal static bool TrySpazerAxialStartup(ushort pointer, out int group, out int length)
    {
        int groupBytes = 7 * sizeof(ushort) + 5 * (4 * 5 / 2 + 2 * (3 * 4 / 2));
        int offset = pointer - SpazerStartupStart;
        group = offset / groupBytes;
        int within = offset % groupBytes;
        length = within == 0 ? 1 : 2;
        return offset >= 0 && group < 6 && (within == 0 || within == RecordBytes(1));
    }
    /// <summary>One/two-cell centered startup strips; native group glyph/reflection selection and traversal remain REQUIRED composition inputs, while adjacency and centering calculate.</summary>
    /// <summary>Builds a centered horizontal or vertical startup strip with the native reflection for its group.</summary>
    /// <param name="group">Selected startup orientation and artwork family.</param><param name="length">Number of adjacent cells in the strip.</param>
    internal readonly struct SpazerAxialStartupParts(int group, int length) : IReadOnlyList<CompiledSpritePart>
    {
        /// <summary>Number of adjacent cells in the selected one- or two-cell axial strip.</summary>
        public int Count => length;
        /// <summary>Gets one cell in the startup strip's centered traversal order.</summary>
        /// <param name="index">Zero-based cell position along the strip.</param>
        /// <exception cref="IndexOutOfRangeException">The index is outside the selected strip.</exception>
        public CompiledSpritePart this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new IndexOutOfRangeException();
                int size = SpazerCompositionGeometryDefinitions.TileSize;
                bool vertical = (group & 1) != 0;
                bool reflected = group is 2 or 3;
                int along = -length * size / 2 + index * size;
                if (vertical != reflected) along = -size - along;
                int x = vertical ? -size / 2 : along;
                int y = vertical ? along : SpazerCompositionGeometryDefinitions.HorizontalStripOriginY;
                int glyph = group >= 4
                    ? (vertical ? ChargedSpazerVerticalGlyph : ChargedSpazerHorizontalGlyph)
                    : (vertical ? SpazerVerticalGlyph : SpazerHorizontalGlyph);
                var flips = reflected ? (vertical ? SnesTileFlipFlags.Vertical : SnesTileFlipFlags.Horizontal) : 0;
                return new(SnesSpritemapXWord.Create(x, false), unchecked((byte)y),
                    SnesObjAttributeWord.Create(glyph, PowerPalette, PowerPriority, flips), false);
            }
        }
        /// <summary>Enumerates the cells of the selected centered axial startup strip.</summary>
        public IEnumerator<CompiledSpritePart> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>$93:EE4C/EE58,EEC8/EED4,EF44/EF50: ordinary diagonal startup strips select one or two tile pairs after four physical axial records.</summary>
    internal static bool TrySpazerDiagonalStartup(ushort pointer, out int group, out int pairs)
    {
        int groupBytes = 7 * sizeof(ushort) + 5 * (4 * 5 / 2 + 2 * (3 * 4 / 2));
        int offset = pointer - SpazerStartupStart;
        group = offset / groupBytes;
        int within = offset % groupBytes - (4 * sizeof(ushort) + 5 * 4 * 5 / 2);
        pairs = within == 0 ? 1 : 2;
        return offset >= 0 && group < 3 && (within == 0 || within == RecordBytes(2));
    }
    /// <summary>Ordinary diagonal startup strips keep the required two-pair origin and shift by half a cell when shortened to one pair. Pair adjacency/reflection calculate; selected footprint lengths, origin, glyphs and order remain REQUIRED.</summary>
    /// <summary>Builds one or two adjacent tile pairs for an ordinary diagonal Spazer startup pose.</summary>
    /// <param name="group">Selected diagonal orientation group.</param><param name="pairs">Number of tile pairs in the selected footprint.</param>
    internal readonly struct SpazerDiagonalStartupParts(int group, int pairs) : IReadOnlyList<CompiledSpritePart>
    {
        /// <summary>Each startup pose is composed of the selected number of two-cell pairs.</summary>
        public int Count => pairs * 2;
        /// <summary>Gets a tile within the selected adjacent pairs in native traversal order.</summary>
        /// <param name="index">Zero-based tile position in the startup footprint.</param>
        /// <exception cref="IndexOutOfRangeException">The index is outside the selected footprint.</exception>
        public CompiledSpritePart this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new IndexOutOfRangeException();
                int size = SpazerCompositionGeometryDefinitions.TileSize;
                int x = SpazerCompositionGeometryDefinitions.DiagonalFirstPairOriginX + (2 - pairs) * size / 2 +
                    (index / 2 + index % 2) * size;
                int y = SpazerCompositionGeometryDefinitions.DiagonalFirstPairOriginY + (pairs - 2) * size / 2 - index / 2 * size;
                bool flipX = group != 0, flipY = group == 2;
                if (!flipX) x = -size - x;
                if (flipY) y = -size - y;
                var flips = (flipX ? SnesTileFlipFlags.Horizontal : 0) | (flipY ? SnesTileFlipFlags.Vertical : 0);
                return new(SnesSpritemapXWord.Create(x, false), unchecked((byte)y),
                    SnesObjAttributeWord.Create(SpazerDiagonalGlyph - index % 2, PowerPalette, PowerPriority, flips), false);
            }
        }
        /// <summary>Enumerates the tile pairs in the selected ordinary diagonal startup pose.</summary>
        public IEnumerator<CompiledSpritePart> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>Recognizes one of the selected Plasma startup core compositions.</summary>
    /// <param name="pointer">Composition address.</param><param name="group">Receives the selected startup orientation and artwork group.</param>
    /// <returns><see langword="true"/> when the pointer matches one of eight startup core records.</returns>
    internal static bool TryPlasmaStartupCore(ushort pointer, out int group)
    {
        for (group = 0; group < 8; group++)
            if (pointer == PlasmaStartupPointer(group * 4)) return true;
        return false;
    }
    /// <summary>$93:F03C/F048/F0B8/F0C4: alternate diagonal Spazer startup pair and expanded two-endcap poses.</summary>
    internal static bool TryAlternateDiagonalStartup(ushort pointer, out bool reflected, out int phase)
    {
        for (int variant = 0; variant < 2; variant++)
        {
            int shape = (int)(variant == 0 ? SpazerStartupShape.FallingAlternate : SpazerStartupShape.RisingAlternate);
            for (phase = 0; phase < 2; phase++)
                if (pointer == SpazerStartupPointer(shape * 2 + phase))
                {
                    reflected = shape == (int)SpazerStartupShape.RisingAlternate;
                    return true;
                }
        }
        reflected = false;
        phase = 0;
        return false;
    }
    /// <summary>$93:F048: chosen inner/endcap glyphs35/36 remain REQUIRED artwork inputs.</summary>
    private const int SpazerDiagonalInnerGlyph = 0x35, SpazerDiagonalEndcapGlyph = 0x36;
    /// <summary>One centered endcap pair, then two rotated pairs. Footprint, glyph/endcap selection and native traversal remain REQUIRED; cell adjacency/reflection calculate.</summary>
    /// <summary>Builds the endcap or expanded rotated footprint used by alternate diagonal Spazer startups.</summary>
    /// <param name="reflected">Selects the mirrored rising or falling alternate pose.</param><param name="phase">Zero selects the centered endcap pair; the later phase selects the expanded pose.</param>
    internal readonly struct AlternateDiagonalStartupParts(bool reflected, int phase) : IReadOnlyList<CompiledSpritePart>
    {
        /// <summary>The endcap phase has two tiles; the expanded alternate pose has four.</summary>
        public int Count => phase == 0 ? 2 : 4;
        /// <summary>Gets an endcap or expanded-pose tile with the selected diagonal reflection.</summary>
        /// <param name="index">Zero-based tile position in the selected phase.</param>
        /// <exception cref="IndexOutOfRangeException">The index is outside the selected footprint.</exception>
        public CompiledSpritePart this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new IndexOutOfRangeException();
                int size = SpazerCompositionGeometryDefinitions.TileSize;
                int x, y, tile;
                bool rotated;
                if (phase == 0)
                {
                    x = -size / 2;
                    y = (index - 1) * size;
                    tile = SpazerDiagonalEndcapGlyph;
                    rotated = index != 0;
                }
                else
                {
                    int cell = index < 2 ? index : 3 - index;
                    x = 0;
                    y = -size / 2 + cell * size;
                    tile = SpazerDiagonalInnerGlyph + cell;
                    rotated = index < 2;
                    if (!rotated) { x = -size - x; y = -size - y; }
                }
                if (reflected) x = -size - x;
                var flips = (rotated != reflected ? SnesTileFlipFlags.Horizontal : 0) |
                    (rotated ? SnesTileFlipFlags.Vertical : 0);
                return new(SnesSpritemapXWord.Create(x, false), unchecked((byte)y),
                    SnesObjAttributeWord.Create(tile, PowerPalette, PowerPriority, flips), false);
            }
        }
        /// <summary>Enumerates the endcap pair or expanded footprint of the alternate diagonal startup.</summary>
        public IEnumerator<CompiledSpritePart> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>$93:F0FA/F194/F22E/F2CE and alternate groupsF36E/F408/F4A2/F542: centered startup cores share tile geometry; selected glyph/footprint/style remain REQUIRED.</summary>
    /// <summary>Builds the selected one- or two-part centered core for a Plasma startup orientation group.</summary>
    /// <param name="group">Selected Plasma startup orientation and alternate-art group.</param>
    internal readonly struct PlasmaStartupCoreParts(int group) : IReadOnlyList<CompiledSpritePart>
    {
        /// <summary>Axial startup groups use one core tile and diagonal groups use two.</summary>
        public int Count => group % 4 < 2 ? 1 : 2;
        /// <summary>Gets a tile from the selected axial or diagonal startup core.</summary>
        /// <param name="index">Zero-based tile position in the core.</param>
        /// <exception cref="IndexOutOfRangeException">The index is outside the selected core.</exception>
        public CompiledSpritePart this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new IndexOutOfRangeException();
                int orientation = group % 4;
                if (orientation < 2)
                    return new SpazerAxialStartupParts(group < 4 ? orientation : orientation + 4, 1)[index];
                if (group >= 4) return new AlternateDiagonalStartupParts(orientation == 3, 0)[index];
                int size = SpazerCompositionGeometryDefinitions.TileSize;
                bool reflected = orientation == 3;
                int x = -index * size;
                if (reflected) x = -size - x;
                return new(SnesSpritemapXWord.Create(x, false), unchecked((byte)(-size / 2)),
                    SnesObjAttributeWord.Create(SpazerDiagonalGlyph - index, PowerPalette, PowerPriority,
                        reflected ? SnesTileFlipFlags.Horizontal : 0), false);
            }
        }
        /// <summary>Enumerates the one- or two-part core selected for a Plasma startup group.</summary>
        public IEnumerator<CompiledSpritePart> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>$93:BC0A..BCC7: horizontal PlasmaWave Short core and four two-lobe spreads.</summary>
    internal static bool TryHorizontalPlasmaWaveShort(ushort pointer, out int phase)
    {
        if (pointer == PlasmaWaveHorizontalShort) { phase = 0; return true; }
        if (!TryPhase(pointer, (ushort)(PlasmaWaveHorizontalShort + RecordBytes(4)), 8, 4, out phase)) return false;
        phase++;
        return true;
    }
    /// <summary>$93:BC0A: the chosen four-cell short Plasma footprint remains REQUIRED artwork input.</summary>
    private const int PlasmaWaveShortCells = 4;
    /// <summary>Centered horizontal strip or opposite displaced copies. Selected length/glyph, lobe order and four spread distances remain REQUIRED; repeated cell geometry calculates.</summary>
    /// <summary>Builds a short horizontal PlasmaWave core or its two oppositely displaced copies.</summary>
    /// <param name="phase">Zero selects the core; later values select a spread distance.</param>
    internal readonly struct HorizontalPlasmaWaveShortParts(int phase) : IReadOnlyList<CompiledSpritePart>
    {
        /// <summary>The core uses one four-cell strip; spread phases use two opposing strips.</summary>
        public int Count => PlasmaWaveShortCells * (phase == 0 ? 1 : 2);
        /// <summary>Gets a cell in the core strip or in one of its phase-displaced copies.</summary>
        /// <param name="index">Zero-based tile position in the native strip/lobe order.</param>
        /// <exception cref="IndexOutOfRangeException">The index is outside the selected composition.</exception>
        public CompiledSpritePart this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new IndexOutOfRangeException();
                int size = SpazerCompositionGeometryDefinitions.TileSize;
                int x = (PlasmaWaveShortCells / 2 - 1 - index % PlasmaWaveShortCells) * size;
                int direction = phase == 0 ? 0 : index < PlasmaWaveShortCells ? 1 : -1;
                int y = -size / 2 + (phase == 0 ? 0 : direction * SuperMetroid.Core.Game.ProjectileWaveEnvelopeDefinitions.AxialLobeDistance(phase - 1));
                return new(SnesSpritemapXWord.Create(x, false), unchecked((byte)y),
                    SnesObjAttributeWord.Create(SpazerHorizontalGlyph, PowerPalette, PowerPriority, 0), false);
            }
        }
        /// <summary>Enumerates the horizontal PlasmaWave core or both of its displaced copies.</summary>
        public IEnumerator<CompiledSpritePart> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>Formats a bank-relative sprite identity as the case-sensitive projectile-composition JSON frame key, such as sprite_A117 for the Nothing composition.</summary>
    /// <param name="pointer">Sixteen-bit spritemap identity; formatting does not validate membership in <see cref="NativePointers"/>.</param>
    /// <returns>The sprite_ prefix followed by exactly four uppercase hexadecimal digits.</returns>
    public static string Name(ushort pointer) => $"sprite_{pointer:X4}";
}
