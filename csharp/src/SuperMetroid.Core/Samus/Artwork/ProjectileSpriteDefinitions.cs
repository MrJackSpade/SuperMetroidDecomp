namespace SuperMetroid.Core.Samus.Artwork;

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
    public const int TileColumns = 16;
    /// <summary>Sixteen columns and thirty-two rows in the logical OBJ character grid; row times sixteen plus column forms the nine-bit tile index, with large-part neighbors wrapping natively.</summary>
    public const int TileRows = 32;
    /// <summary>The417 timed-projectile identities in their original sorted order; selected OAM groups derive from two-byte headers and five-byte parts.</summary>
    public static PointerSequence NativePointers => default;

    /// <summary>$93:A117: native Nothing projectile composition group.</summary>
    private const ushort NothingStart = 0xa117;
    /// <summary>$93:A24D: native Power projectile composition group.</summary>
    internal const ushort PowerStart = 0xa24d;
    /// <summary>$93:A83E: native BombExplosion projectile composition group.</summary>
    private const ushort BombExplosionStart = 0xa83e;
    /// <summary>$93:AB97: native PowerBomb projectile composition group.</summary>
    private const ushort PowerBombStart = 0xab97;
    /// <summary>$93:AD45: native Bomb projectile composition group.</summary>
    internal const ushort BombStart = 0xad45;
    /// <summary>$93:AD61: native Missile projectile composition group.</summary>
    internal const ushort MissileStart = 0xad61;
    /// <summary>$93:ADD5: native SuperMissile projectile composition group.</summary>
    internal const ushort SuperMissileStart = 0xadd5;
    /// <summary>$93:AE65: native Wave projectile composition group.</summary>
    internal const ushort WaveStart = 0xae65;
    /// <summary>$93:EC3E: native ChargedPower projectile composition group.</summary>
    internal const ushort ChargedPowerStart = 0xec3e;
    /// <summary>$93:ED9E: native ChargedIce projectile composition group.</summary>
    internal const ushort ChargedIceStart = 0xed9e;
    /// <summary>$93:EDF6: native Ice projectile composition group.</summary>
    internal const ushort IceStart = 0xedf6;
    /// <summary>Native bank93 compositions serialize a two-byte part count then five bytes per OAM part.</summary>
    internal static int RecordBytes(int parts) => sizeof(ushort) + 5 * parts;
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

    /// <summary>Maps an eight-byte timed-record address within a native program to its zero-based phase.</summary>
    private static bool TryTimedPhase(ushort pointer, ushort start, int count, out int phase)
    {
        int offset = pointer - start;
        phase = offset / 8;
        return offset >= 0 && offset < count * 8 && offset % 8 == 0;
    }

    /// <summary>Finds the selected beam-explosion composition within its variable-size native group.</summary>
    internal static ushort BeamExplosionPointer(int phase) => (ushort)(BeamExplosionStart +
        Math.Min(phase, 2) * RecordBytes(1) + Math.Max(0, phase - 2) * RecordBytes(4));
    /// <summary>Finds the selected missile-explosion composition after its initial center tile.</summary>
    private static ushort MissileExplosionPointer(int phase) => (ushort)(MissileExplosionStart +
        (phase == 0 ? 0 : RecordBytes(1) + (phase - 1) * RecordBytes(4)));
    /// <summary>$93:9EBB-9F1A: eight one-record missile compass programs.</summary>
    private const ushort MissileProgramStart = 0x9ebb;
    /// <summary>$93:9F1B-9F7A: eight one-record super-missile compass programs.</summary>
    private const ushort SuperMissileProgramStart = 0x9f1b;

    /// <summary>Resolves one of eight one-record missile compass programs from its bank-$93 pointer.</summary>
    private static bool TryCompassPose(ushort pointer, ushort start, out int pose)
    {
        int offset = pointer - start;
        const int programBytes = 8 + 4;
        pose = ((offset / programBytes) + 2) & 7;
        return offset >= 0 && offset < 8 * programBytes && offset % programBytes == 0;
    }

    /// <summary>Maps a compass pose to its missile-group composition after preceding variable-size records.</summary>
    private static ushort MissilePointer(ushort start, int pose) => (ushort)(start +
        pose / 2 * (RecordBytes(2) + RecordBytes(3)) + pose % 2 * RecordBytes(2));
    /// <summary>$93:9F87 and9FA3: normal/fast PowerBomb cycles each traverse the same three poses.</summary>
    private const ushort PowerBombProgram = 0x9f87;
    /// <summary>$93:9F87 and9FA3: normal/fast PowerBomb cycles each traverse the same three poses.</summary>
    private const ushort FastPowerBombProgram = 0x9fa3;
    /// <summary>$93:9FBF and9FE3: normal/fast Bomb cycles each traverse the same four poses.</summary>
    private const ushort BombProgram = 0x9fbf;
    /// <summary>$93:9FBF and9FE3: normal/fast Bomb cycles each traverse the same four poses.</summary>
    private const ushort FastBombProgram = 0x9fe3;
    /// <summary>$93:8F1F: the four sixteen-frame travel-axis loops after the invisible upward charged-Wave lead-in.</summary>
    private const ushort ChargedWaveProgram = 0x8f1f;
    /// <summary>$93:915B: the corresponding charged IceWave travel-axis loops.</summary>
    private const ushort ChargedIceWaveProgram = 0x915b;

    /// <summary>Resolves a charged Wave travel-loop record to its selected paired-lobe composition.</summary>
    private static bool TryChargedWaveCycle(ushort pointer, ushort programStart, ushort compositionStart, out ushort sprite)
    {
        const int cycleBytes = 16 * 8 + 4;
        int offset = pointer - programStart;
        sprite = default;
        if (offset is < 0 or >= (4 * cycleBytes)) return false;
        int recordOffset = offset % cycleBytes;
        if (recordOffset >= 16 * 8 || recordOffset % 8 != 0) return false;
        int phase = recordOffset / 8;
        WaveTravelAxis axis = WaveTravelAxisAt(offset / cycleBytes);
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
    /// <summary>Selected physical spread stages remain REQUIRED phase-policy inputs: axial2/5, diagonal1/4. Pointer arithmetic does not derive these choices.</summary>
    private static SpazerSpreadPhase SpazerSpreadPhaseAt(int record) => record switch
    {
        0 => SpazerSpreadPhase.Seed,
        1 => SpazerSpreadPhase.Intermediate,
        2 => SpazerSpreadPhase.Full,
        _ => throw new ArgumentOutOfRangeException(nameof(record), record, "Spazer lists have three spread phases."),
    };
    /// <summary>Selects the physical spread extent used by a diagonal or axial Spazer phase.</summary>
    private static int SpazerSelectedStage(SpazerSpreadPhase phase, bool diagonal) => phase switch
    {
        SpazerSpreadPhase.Seed => 0,
        SpazerSpreadPhase.Intermediate => diagonal ? 1 : 2,
        SpazerSpreadPhase.Full => diagonal ? 4 : 5,
        _ => throw new ArgumentOutOfRangeException(nameof(phase)),
    };
    /// <summary>Maps an ordinary Spazer instruction frame to its direction-specific seed or spread artwork.</summary>
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
        int stage = SpazerSelectedStage(SpazerSpreadPhaseAt(within / 8), diagonal);
        int start = SpazerStart + Math.Min(group, 4) * SpreadGroupBytes(4) + Math.Max(0, group - 4) * SpreadGroupBytes(2);
        sprite = (ushort)(start + (stage == 0 ? 0 : RecordBytes(seedParts) + (stage - 1) * RecordBytes(3 * seedParts)));
        return true;
    }
    /// <summary>Translates the eight instruction-list compass indices into native Spazer artwork direction order.</summary>
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
    /// <summary>Resolves an outward/return SpazerWave instruction stage, including its down-left plateau policy.</summary>
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
        SpazerSeedDirection direction = SpazerCompassDirection(offset / cycleBytes);
        int group = (int)direction;

        bool diagonal = group < 4;
        int phase = within / 8;
        int spread = Math.Min(phase, cycleLength - phase);
        int stage = direction == SpazerSeedDirection.DownLeft
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
    /// <summary>Maps ordinary Plasma and PlasmaWave instruction records to core or spreading composition groups.</summary>
    private static bool TryPlasmaSprite(ushort pointer, out ushort sprite)
    {
        const int programBytes = 2 * 8 + 4;
        int offset = pointer - PlasmaProgramStart;
        int within = offset % programBytes;
        if (offset >= 0 && offset < 4 * programBytes && within < 2 * 8 && within % 8 == 0)
        {
            WaveTravelAxis axis = WaveTravelAxisAt(offset / programBytes);
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
            WaveTravelAxis axis = WaveTravelAxisAt(offset / waveBytes);
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
                sprite = PlasmaWavePointer(shape, spread);
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
    /// <summary>Selects the charged Plasma core or growth artwork for one travel-axis phase.</summary>
    private static ushort ChargedPlasmaGrowthSprite(WaveTravelAxis axis, int phase) =>
        PlasmaStartupPointer((PlasmaCoreGroup(axis) + (phase % 2) * 4) * 4 + phase / 2);
    /// <summary>Resolves charged Plasma and PlasmaWave growth/return records to their native composition identities.</summary>
    private static bool TryChargedPlasmaSprite(ushort pointer, out ushort sprite)
    {
        const int programBytes = 8 * 8 + 4;
        int offset = pointer - ChargedPlasmaProgramStart;
        int within = offset % programBytes;
        if (offset >= 0 && offset < 4 * programBytes && within < 8 * 8 && within % 8 == 0)
        {
            sprite = ChargedPlasmaGrowthSprite(WaveTravelAxisAt(offset / programBytes), within / 8);
            return true;
        }
        const int waveBytes = (6 + 16) * 8 + 4;
        offset = pointer - ChargedPlasmaWaveProgramStart;
        within = offset % waveBytes;
        if (offset >= 0 && offset < 4 * waveBytes && within < 22 * 8 && within % 8 == 0)
        {
            WaveTravelAxis axis = WaveTravelAxisAt(offset / waveBytes);
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
                sprite = PlasmaWavePointer(shape, spread);
            }
            return true;
        }
        sprite = default;
        return false;
    }
    /// <summary>$93:94BB..9ADA: eight charged SpazerWave compass lists, four startup rows then twenty alternating spread rows.</summary>
    private const ushort ChargedSpazerWaveProgramStart = 0x94bb;
    /// <summary>Selects the primary charged-Spazer shape group for an authored travel direction.</summary>
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
    /// <summary>Selects the alternate charged-Spazer group used on alternating spread frames.</summary>
    private static ChargedSpazerShape ChargedSpazerAlternate(SpazerSeedDirection direction) => direction switch
    {
        SpazerSeedDirection.Up or SpazerSeedDirection.Down => ChargedSpazerShape.VerticalAlternate,
        SpazerSeedDirection.UpRight or SpazerSeedDirection.DownLeft => ChargedSpazerShape.RisingAlternate,
        SpazerSeedDirection.Left or SpazerSeedDirection.Right => ChargedSpazerShape.HorizontalAlternate,
        SpazerSeedDirection.UpLeft or SpazerSeedDirection.DownRight => ChargedSpazerShape.FallingAlternate,
        _ => throw new ArgumentOutOfRangeException(nameof(direction)),
    };
    /// <summary>Finds a charged-Spazer startup composition for its direction and selected physical pose.</summary>
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
                ChargedSpazerShape.UpRight or
                    ChargedSpazerShape.DownRight or
                    ChargedSpazerShape.DownLeft or
                    ChargedSpazerShape.UpLeft or
                    ChargedSpazerShape.Down or
                    ChargedSpazerShape.Left or
                    ChargedSpazerShape.Up or
                    ChargedSpazerShape.Right => throw new InvalidOperationException("Unknown Spazer alternate startup."),
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
    /// <summary>Maps charged SpazerWave's startup and alternating twenty-stage spreads to compiled artwork.</summary>
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
    /// <summary>Maps ordinary charged-Spazer startup and selected spread rows to compiled artwork.</summary>
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
        WaveTravelAxis axis = WaveTravelAxisAt(offset / programBytes);
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
            int stage = SpazerSelectedStage(SpazerSpreadPhaseAt((phase - 4) / 2), !alternate && (int)direction < 4);
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
    /// <summary>Resolves a timed projectile instruction pointer to its derived native composition when the visual layout is fully modeled.</summary>
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
        if (Enum.IsDefined((InvisibleSingleRecordProgram)instructionPointer))
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
        int offset = instructionPointer - WaveProgramStart;
        const int cycleBytes = 16 * 8 + 4;
        if (offset is >= 0 and < (4 * cycleBytes))
        {
            int recordOffset = offset % cycleBytes;
            if (recordOffset < 16 * 8 && recordOffset % 8 == 0)
            {
                int phase = recordOffset / 8;
                int halfPhase = phase % 8;
                int distanceStage = Math.Min(halfPhase, 8 - halfPhase);
                // Travel direction chooses the perpendicular displacement axis. Each half-cycle
                // traverses the same four spatial stages outward and back on opposite sides.
                WaveTravelAxis travel = WaveTravelAxisAt(offset / cycleBytes);
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
        if (offset is >= 0 and < (8 * chargedProgramBytes))
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

    /// <summary>Resolves the four native Wave list starts into their travel-axis order.</summary>
    private static WaveTravelAxis WaveTravelAxisAt(int loop) => loop switch
    {
        0 => WaveTravelAxis.Vertical,
        1 => WaveTravelAxis.RisingDiagonal,
        2 => WaveTravelAxis.Horizontal,
        3 => WaveTravelAxis.FallingDiagonal,
        _ => throw new ArgumentOutOfRangeException(nameof(loop), loop, "Projectile programs have four travel-axis loops."),
    };
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
    /// <summary>$93:AF4C/B672, ChargedWave_WaveSBA/ChargedIceWave: two four-quadrant core poses, then paired four-quadrant lobes.</summary>
    internal const ushort ChargedWaveStart = 0xaf4c;
    /// <summary>$93:AF4C/B672, ChargedWave_WaveSBA/ChargedIceWave: two four-quadrant core poses, then paired four-quadrant lobes.</summary>
    internal const ushort ChargedIceWaveStart = 0xb672;
    /// <summary>$93:D10E, S_SI_SW_SIW: four orientations of four-part beams, then four of two-part beams; spread repeats each seed on three lanes.</summary>
    internal const ushort SpazerStart = 0xd10e;
    /// <summary>$93:D8EE, Charged_S_SI_SW_SIW: two four-part axial groups, six six-part diagonal groups, then four four-part axial groups.</summary>
    internal const ushort ChargedSpazerStart = 0xd8ee;
    /// <summary>Skips unselected four-part layouts while indexing charged Wave's paired-lobe records.</summary>
    internal static ushort ChargedWavePointer(ushort start, int index)
    {
        // B368/B37E and BA8E/BAA4 each contain two unselected four-part
        // compositions between the first24 paired-lobe phases and the last8.
        return (ushort)(start + Math.Min(index, 2) * RecordBytes(4) +
            Math.Max(0, index - 2) * RecordBytes(8) + (index >= 26 ? 2 * RecordBytes(4) : 0));
    }
    /// <summary>Measures one Spazer seed plus its five three-lane spread compositions.</summary>
    internal static int SpreadGroupBytes(int seedParts) => RecordBytes(seedParts) + 5 * RecordBytes(3 * seedParts);
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
    /// <summary>$93:A37D: Plasma/PlasmaIce axial four-tile strips alternate with six-part diagonal strips.</summary>
    private const ushort PlasmaStart = 0xa37d;
    /// <summary>$93:A7C9: missile explosion grows from one central tile to five four-quadrant poses.</summary>
    private const ushort MissileExplosionStart = 0xa7c9;
    /// <summary>$93:ABB3: beam explosion has two single-tile core poses followed by four four-quadrant poses.</summary>
    private const ushort BeamExplosionStart = 0xabb3;
    /// <summary>$93:EE12: six Spazer startup orientation groups serialize length1..4 strips then one/two/three two-tile pairs.</summary>
    internal const ushort SpazerStartupStart = 0xee12;
    /// <summary>Finds a selected startup pose while accounting for unselected records in earlier orientations.</summary>
    internal static ushort SpazerStartupPointer(int index)
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
    /// <summary>Steps over the explosion's mixed four-, eight- and twelve-part records to the requested stage.</summary>
    private static ushort SuperMissileExplosionPointer(int phase) => (ushort)(SuperMissileExplosionStart +
        Math.Min(phase, 3) * RecordBytes(4) + (phase >= 4 ? RecordBytes(4 + 4 * 2) : 0) +
        (phase >= 5 ? RecordBytes(4 * 2) : 0));

    /// <summary>$93:BC0A/BCC8: horizontal uncharged four-tile and charged seven-tile cores.</summary>
    internal const ushort PlasmaWaveHorizontalShort = 0xbc0a;
    /// <summary>$93:BC0A/BCC8: horizontal uncharged four-tile and charged seven-tile cores.</summary>
    private const ushort PlasmaWaveHorizontalLong = 0xbcc8;
    /// <summary>$93:BE0D/BF25: down-right/up-left diagonal six-part and ten-part cores.</summary>
    private const ushort PlasmaWaveDownRightShort = 0xbe0d;
    /// <summary>$93:BE0D/BF25: down-right/up-left diagonal six-part and ten-part cores.</summary>
    private const ushort PlasmaWaveDownRightLong = 0xbf25;
    /// <summary>$93:C0F1/C1AF: vertical uncharged four-tile and charged seven-tile cores.</summary>
    private const ushort PlasmaWaveVerticalShort = 0xc0f1;
    /// <summary>$93:C0F1/C1AF: vertical uncharged four-tile and charged seven-tile cores.</summary>
    private const ushort PlasmaWaveVerticalLong = 0xc1af;
    /// <summary>$93:C3B2/C669: alternate charged horizontal seven-part and down-right twelve-part cores, after native unselected composition groups.</summary>
    private const ushort PlasmaWaveHorizontalAlternate = 0xc3b2;
    /// <summary>$93:C3B2/C669: alternate charged horizontal seven-part and down-right twelve-part cores, after native unselected composition groups.</summary>
    private const ushort PlasmaWaveDownRightAlternate = 0xc669;
    /// <summary>$93:C94D/CC04: alternate charged vertical seven-part and down-left twelve-part cores, after native unselected composition groups.</summary>
    private const ushort PlasmaWaveVerticalAlternate = 0xc94d;
    /// <summary>$93:C94D/CC04: alternate charged vertical seven-part and down-left twelve-part cores, after native unselected composition groups.</summary>
    private const ushort PlasmaWaveDownLeftAlternate = 0xcc04;
    /// <summary>$93:CE2A/CF42: down-left/up-right diagonal six-part and ten-part cores.</summary>
    private const ushort PlasmaWaveDownLeftShort = 0xce2a;
    /// <summary>$93:CE2A/CF42: down-left/up-right diagonal six-part and ten-part cores.</summary>
    private const ushort PlasmaWaveDownLeftLong = 0xcf42;
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
        (ushort start, int coreParts) = shape switch
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
            _ => throw new ArgumentOutOfRangeException(nameof(shape)),
        };
        return (ushort)(start + (phase == 0 ? 0 : RecordBytes(coreParts) + (phase - 1) * RecordBytes(2 * coreParts)));
    }

    /// <summary>$93:F0FA: Charged_P_PI_PW_PIW startup, two axial orientation groups then two diagonal groups, repeated with alternate orientation/art.</summary>
    private const ushort PlasmaStartupStart = 0xf0fa;
    /// <summary>Locates one charged Plasma startup composition despite intervening unselected physical growth records.</summary>
    internal static ushort PlasmaStartupPointer(int index)
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
        int selectedLength = (stage, diagonal) switch
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
    /// <summary>Formats a bank-relative sprite identity as the case-sensitive projectile-composition JSON frame key, such as sprite_A117 for the Nothing composition.</summary>
    /// <param name="pointer">Sixteen-bit spritemap identity; formatting does not validate membership in <see cref="NativePointers"/>.</param>
    /// <returns>The sprite_ prefix followed by exactly four uppercase hexadecimal digits.</returns>
    public static string Name(ushort pointer) => $"sprite_{pointer:X4}";
}
