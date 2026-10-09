using System.Collections.Frozen;
namespace SuperMetroid.Core.Game;

/// <summary>Projectile duration/trail/control programs, independent of installed spritemap references.
/// Address layout, paired/reflected phases and terminal targets calculate. The named holds and
/// content/startup/terminal policies retain the precisely reviewed native growth/reach/lifetime design;
/// they do not dispose of artwork, fuse timing, damage or the shared lobe source geometry.</summary>
internal static class SamusProjectileInstructionDefinitions
{
    /// <summary>$93:86DB, InstList_SamusProjectile_Power_Up; clockwise compass lists follow.</summary>
    internal const int PowerProgramsStart = 0x9386db;

    /// <summary>$93:86DB..873A: eight distinct compass directions, with up/down facing aliases sharing lists.</summary>
    internal const int PowerDirectionCount = 8;

    /// <summary>$93:86DB record: duration, spritemap, two radius bytes and trail phase occupy eight bytes.</summary>
    internal const int TimedRecordBytes = 4 * sizeof(ushort);

    /// <summary>$93:86E3 Goto and its operand follow the sole Power timed record.</summary>
    internal const int PowerProgramBytes = TimedRecordBytes + 2 * sizeof(ushort);

    /// <summary>$93:86DB et al.: Selected fifteen-update persistent Power reload cadence.</summary>
    private const ushort PowerHold = 15;

    /// <summary>$93:86DB..873A: calculate the sole phase and its self-loop, preserving the mechanics-only domain.</summary>
    internal static bool TryPowerWord(int address, out ushort value)
    {
        int offset = address - PowerProgramsStart;
        value = 0;
        if ((uint)offset >= PowerDirectionCount * PowerProgramBytes) return false;
        int field = offset % PowerProgramBytes;
        switch (field)
        {
            case 0:
                value = PowerHold;
                return true;
            case TimedRecordBytes - sizeof(ushort):
                // A single phase loops to itself; its trail phase is the zero-based phase index.
                return true;
            case TimedRecordBytes:
                value = SamusProjectileRomData.Instructions.GoTo;
                return true;
            case TimedRecordBytes + sizeof(ushort):
                value = (ushort)(PowerProgramsStart + offset / PowerProgramBytes * PowerProgramBytes);
                return true;
            default:
                return false;
        }
    }

    /// <summary>$93:873B, Wave/IceWave upward invisible prelude before the shared vertical cycle.</summary>
    private const int WaveUpPrelude = PowerProgramsStart + PowerDirectionCount * PowerProgramBytes;
    /// <summary>$93:8743, Wave/IceWave Down cycle; subsequent axis cycles are adjacent.</summary>
    private const int WaveCyclesStart = WaveUpPrelude + TimedRecordBytes;
    /// <summary>$93:8743..8952: vertical, rising diagonal, horizontal and falling diagonal axes.</summary>
    private const int WaveAxisCount = PowerDirectionCount / 2;
    /// <summary>$93:8743..87C2: Two signed excursions each traverse the shared outward and return positions: four quarters, sixteen trail phases.</summary>
    private const int WavePhases = 4 * ProjectileWaveEnvelopeDefinitions.OutwardPositionCount;
    /// <summary>$93:873B: Selected upward Wave prelude hold4.</summary>
    private const ushort WavePreludeHold = 4;
    /// <summary>$93:8743 and $93:8953: Selected one-update Wave/Ice holds, selected exposure cadence.</summary>
    private const ushort WaveIceHold = 1;
    /// <summary>$93:87C3: each Wave cycle ends with Goto and its own phase-zero target.</summary>
    private const int WaveCycleBytes = WavePhases * TimedRecordBytes + 2 * sizeof(ushort);
    /// <summary>$93:8953, InstList_SamusProjectile_Ice, follows the four Wave axes.</summary>
    private const int IceCycleStart = WaveCyclesStart + WaveAxisCount * WaveCycleBytes;
    /// <summary>$93:8953..8972: Selected four Ice trail phases.</summary>
    private const int IcePhases = 4;

    internal static bool TryWaveIceWord(int address, out ushort value)
    {
        value = 0;
        if (address == WaveUpPrelude) { value = WavePreludeHold; return true; }
        if (address == WaveUpPrelude + TimedRecordBytes - sizeof(ushort)) return true;
        int offset = address - WaveCyclesStart;
        if ((uint)offset < WaveAxisCount * WaveCycleBytes)
            return TryCycleWord(address, WaveCyclesStart + offset / WaveCycleBytes * WaveCycleBytes,
                WavePhases, WaveIceHold, out value);
        return TryCycleWord(address, IceCycleStart, IcePhases, WaveIceHold, out value);
    }

    // Timed records advance the trail phase; terminal control selects the loop phase.
    private static bool TryCycleWord(int address, int start, int phases, ushort hold, out ushort value, int loopPhase = 0)
    {
        int offset = address - start;
        int recordsBytes = phases * TimedRecordBytes;
        value = 0;
        if ((uint)offset < recordsBytes)
        {
            if (offset % TimedRecordBytes == 0) { value = hold; return true; }
            if (offset % TimedRecordBytes != TimedRecordBytes - sizeof(ushort)) return false;
            value = (ushort)(offset / TimedRecordBytes);
            return true;
        }
        if (offset == recordsBytes) { value = SamusProjectileRomData.Instructions.GoTo; return true; }
        if (offset == recordsBytes + sizeof(ushort)) { value = (ushort)(start + loopPhase * TimedRecordBytes); return true; }
        return false;
    }
    /// <summary>$93:8977, Spazer/SpazerIce Up; follows the Ice cycle and its Goto.</summary>
    private const int SpazerProgramsStart = IceCycleStart + IcePhases * TimedRecordBytes + 2 * sizeof(ushort);
    /// <summary>$93:8977..898E: Selected three growth stages before holding the final Spazer phase.</summary>
    private const int SpazerGrowthPhases = 3;
    /// <summary>$93:8977 and $93:8A57: Selected two-update Spazer/SpazerWave phase hold.</summary>
    private const ushort SpazerHold = 2;
    /// <summary>$93:898F Goto and target follow three timed growth stages.</summary>
    private const int SpazerProgramBytes = SpazerGrowthPhases * TimedRecordBytes + 2 * sizeof(ushort);
    /// <summary>$93:8A57, SpazerWave/SpazerIceWave Up, after eight compass growth programs.</summary>
    private const int SpazerWaveProgramsStart = SpazerProgramsStart + PowerDirectionCount * SpazerProgramBytes;
    /// <summary>$93:8A57..8AA6: A centered strip, initial Spazer lane and four outward Wave positions reflect without duplicating endpoints.</summary>
    private const int SpazerWavePhases = 2 * (ProjectileWaveEnvelopeDefinitions.OutwardPositionCount + 1);
    /// <summary>$93:8AA7 Goto and target follow the complete spread cycle.</summary>
    private const int SpazerWaveProgramBytes = SpazerWavePhases * TimedRecordBytes + 2 * sizeof(ushort);

    internal static bool TrySpazerWord(int address, out ushort value)
    {
        int offset = address - SpazerProgramsStart;
        if ((uint)offset < PowerDirectionCount * SpazerProgramBytes)
            return TryCycleWord(address, SpazerProgramsStart + offset / SpazerProgramBytes * SpazerProgramBytes,
                SpazerGrowthPhases, SpazerHold, out value, SpazerGrowthPhases - 1);
        offset = address - SpazerWaveProgramsStart;
        if ((uint)offset < PowerDirectionCount * SpazerWaveProgramBytes)
            return TryCycleWord(address, SpazerWaveProgramsStart + offset / SpazerWaveProgramBytes * SpazerWaveProgramBytes,
                SpazerWavePhases, SpazerHold, out value);
        value = 0;
        return false;
    }
    /// <summary>$93:8CF7, Plasma/PlasmaIce vertical prelude, after eight SpazerWave directions.</summary>
    private const int PlasmaProgramsStart = SpazerWaveProgramsStart + PowerDirectionCount * SpazerWaveProgramBytes;
    /// <summary>$93:8CF7/8CFF: one initial record followed by one persistent record and its Goto.</summary>
    private const int PlasmaProgramBytes = 2 * TimedRecordBytes + 2 * sizeof(ushort);
    /// <summary>$93:8CF7 and $93:8D47: Selected one-update Plasma entry hold.</summary>
    private const ushort PlasmaPreludeHold = 1;
    /// <summary>$93:8CFF: Selected fifteen-update persistent Plasma hold.</summary>
    private const ushort PlasmaHold = 15;
    /// <summary>$93:8D47, PlasmaIceWave vertical prelude, after the four opposite-paired Plasma axes.</summary>
    private const int PlasmaWaveProgramsStart = PlasmaProgramsStart + WaveAxisCount * PlasmaProgramBytes;
    /// <summary>$93:8D4F..8D8E: Outward and return traversal of the four shared lobe positions, after the entry phase.</summary>
    private const int PlasmaWaveCyclePhases = 2 * ProjectileWaveEnvelopeDefinitions.OutwardPositionCount;
    /// <summary>$93:8D4F: Selected two-update PlasmaWave cyclic hold.</summary>
    private const ushort PlasmaWaveHold = 2;
    /// <summary>$93:8D47..8D92: one entry record, eight cyclic records, then Goto to the first cyclic phase.</summary>
    private const int PlasmaWaveProgramBytes = (1 + PlasmaWaveCyclePhases) * TimedRecordBytes + 2 * sizeof(ushort);

    internal static bool TryPlasmaWord(int address, out ushort value)
    {
        int offset = address - PlasmaProgramsStart;
        int start;
        bool accepted;
        if ((uint)offset < WaveAxisCount * PlasmaProgramBytes)
        {
            start = PlasmaProgramsStart + offset / PlasmaProgramBytes * PlasmaProgramBytes;
            accepted = TryCycleWord(address, start, 2, PlasmaHold, out value, loopPhase: 1);
        }
        else
        {
            offset = address - PlasmaWaveProgramsStart;
            if ((uint)offset >= WaveAxisCount * PlasmaWaveProgramBytes) { value = 0; return false; }
            start = PlasmaWaveProgramsStart + offset / PlasmaWaveProgramBytes * PlasmaWaveProgramBytes;
            accepted = TryCycleWord(address, start, 1 + PlasmaWaveCyclePhases, PlasmaWaveHold, out value, loopPhase: 1);
        }
        // The initial entry is skipped by subsequent Gotos; its hold remains independently selected.
        if (address == start) value = PlasmaPreludeHold;
        return accepted;
    }
    /// <summary>$93:8E77, charged Power Up; follows the four PlasmaWave axes.</summary>
    private const int ChargedPowerStart = PlasmaWaveProgramsStart + WaveAxisCount * PlasmaWaveProgramBytes;
    /// <summary>$93:8E77: Selected two selected charged-Power phases.</summary>
    private const int ChargedAlternateGlyphCount = 2;
    private const int ChargedPowerPhases = ChargedAlternateGlyphCount;
    /// <summary>$93:8E77..9EBA: Selected common one-update charged phase hold (excluding upward preludes).</summary>
    private const ushort ChargedHold = 1;
    /// <summary>$93:8F17/$9153: Selected three-update upward charged Wave/IceWave prelude hold.</summary>
    private const ushort ChargedWavePreludeHold = 3;
    /// <summary>$93:8F1F and $915B cycles: Each outward/return position has the two selected alternating charged glyphs.</summary>
    private const int ChargedWavePhases = ChargedAlternateGlyphCount * PlasmaWaveCyclePhases;
    /// <summary>$93:912F: Selected four charged Ice phases.</summary>
    private const int ChargedIcePhases = 4;
    /// <summary>$93:936B: Selected five charged Spazer growth poses, each using the selected alternate-glyph pair.</summary>
    private const int ChargedSpazerGrowthPoses = 5;
    private const int ChargedSpazerPhases = ChargedAlternateGlyphCount * ChargedSpazerGrowthPoses;
    /// <summary>$93:93BB target93AB: Only the mature alternate-glyph pair repeats after growth.</summary>
    private const int ChargedSpazerLoopPhase = ChargedSpazerPhases - ChargedAlternateGlyphCount;
    /// <summary>$93:94BB: Two-glyph startup plus the paired shared outward/return Spazer lane cycle.</summary>
    private const int ChargedSpazerWavePhases = ChargedSpazerWaveLoopPhase + ChargedAlternateGlyphCount * SpazerWavePhases;
    /// <summary>$93:957B target94DB: Selected two startup poses, each paired, precede the repeating SpazerWave spread.</summary>
    private const int ChargedSpazerStartupPoses = 2;
    private const int ChargedSpazerWaveLoopPhase = ChargedSpazerStartupPoses * ChargedAlternateGlyphCount;
    /// <summary>$93:9ADB: Selected four charged Plasma growth poses, each using the selected alternate-glyph pair.</summary>
    private const int ChargedPlasmaGrowthPoses = 4;
    private const int ChargedPlasmaPhases = ChargedPlasmaGrowthPoses * ChargedAlternateGlyphCount;
    /// <summary>$93:9B1B target9B0B: Only the mature alternate-glyph pair repeats after growth.</summary>
    private const int ChargedPlasmaLoopPhase = ChargedPlasmaPhases - ChargedAlternateGlyphCount;
    /// <summary>$93:9BEB: Shared Plasma startup followed by two glyphs at each outward/return Wave position.</summary>
    private const int ChargedPlasmaWavePhases = ChargedPlasmaWaveLoopPhase + ChargedAlternateGlyphCount * PlasmaWaveCyclePhases;
    /// <summary>$93:9C9B target9C1B: The same six-record Plasma growth prefix precedes the repeating Wave path.</summary>
    private const int ChargedPlasmaWaveLoopPhase = ChargedPlasmaLoopPhase;

    /// <summary>$93:8F17, charged Wave upward prelude, following eight charged Power compass programs.</summary>
    private static int ChargedWaveStart => ChargedPowerStart + PowerDirectionCount * CycleBytes(ChargedPowerPhases);
    /// <summary>$93:912F, charged Ice, following the upward prelude and four charged Wave axes.</summary>
    private static int ChargedIceStart => ChargedWaveStart + TimedRecordBytes + WaveAxisCount * CycleBytes(ChargedWavePhases);
    /// <summary>$93:9153, charged IceWave upward prelude, following charged Ice.</summary>
    private static int ChargedIceWaveStart => ChargedIceStart + CycleBytes(ChargedIcePhases);
    /// <summary>$93:936B, charged Spazer vertical axis, following charged IceWave.</summary>
    private static int ChargedSpazerStart => ChargedIceWaveStart + TimedRecordBytes + WaveAxisCount * CycleBytes(ChargedWavePhases);
    /// <summary>$93:94BB, charged SpazerWave Up, following four charged Spazer axes.</summary>
    private static int ChargedSpazerWaveStart => ChargedSpazerStart + WaveAxisCount * CycleBytes(ChargedSpazerPhases);
    /// <summary>$93:9ADB, charged Plasma vertical axis, following eight charged SpazerWave directions.</summary>
    private static int ChargedPlasmaStart => ChargedSpazerWaveStart + PowerDirectionCount * CycleBytes(ChargedSpazerWavePhases);
    /// <summary>$93:9BEB, charged PlasmaWave vertical axis, following four charged Plasma axes.</summary>
    private static int ChargedPlasmaWaveStart => ChargedPlasmaStart + WaveAxisCount * CycleBytes(ChargedPlasmaPhases);

    private static int CycleBytes(int phases) => phases * TimedRecordBytes + 2 * sizeof(ushort);

    internal static bool TryChargedWord(int address, out ushort value) =>
        TryDirectionCycles(address, ChargedPowerStart, PowerDirectionCount, ChargedPowerPhases, 0, out value) ||
        TryChargedWaveWord(address, ChargedWaveStart, out value) ||
        TryCycleWord(address, ChargedIceStart, ChargedIcePhases, ChargedHold, out value) ||
        TryChargedWaveWord(address, ChargedIceWaveStart, out value) ||
        TryDirectionCycles(address, ChargedSpazerStart, WaveAxisCount, ChargedSpazerPhases, ChargedSpazerLoopPhase, out value) ||
        TryDirectionCycles(address, ChargedSpazerWaveStart, PowerDirectionCount, ChargedSpazerWavePhases, ChargedSpazerWaveLoopPhase, out value) ||
        TryDirectionCycles(address, ChargedPlasmaStart, WaveAxisCount, ChargedPlasmaPhases, ChargedPlasmaLoopPhase, out value) ||
        TryDirectionCycles(address, ChargedPlasmaWaveStart, WaveAxisCount, ChargedPlasmaWavePhases, ChargedPlasmaWaveLoopPhase, out value);

    private static bool TryDirectionCycles(int address, int start, int directions, int phases, int loopPhase, out ushort value)
    {
        int offset = address - start;
        int stride = CycleBytes(phases);
        if ((uint)offset < directions * stride)
            return TryCycleWord(address, start + offset / stride * stride, phases, ChargedHold, out value, loopPhase);
        value = 0;
        return false;
    }

    private static bool TryChargedWaveWord(int address, int prelude, out ushort value)
    {
        if (address == prelude) { value = ChargedWavePreludeHold; return true; }
        if (address == prelude + TimedRecordBytes - sizeof(ushort)) { value = 0; return true; }
        return TryDirectionCycles(address, prelude + TimedRecordBytes, WaveAxisCount, ChargedWavePhases, 0, out value);
    }
    /// <summary>$93:9EBB, Missile Up; follows all charged PlasmaWave axes.</summary>
    private static int MissileStart => ChargedPlasmaWaveStart + WaveAxisCount * CycleBytes(ChargedPlasmaWavePhases);
    /// <summary>$93:9EBB..9F86: eight missile directions, eight super directions and one link, each a single-phase loop.</summary>
    private const int MissileProgramCount = 2 * PowerDirectionCount + 1;
    /// <summary>$93:9EBB..9F7B: Selected fifteen-update missile/super/link hold.</summary>
    private const ushort MissileHold = 15;
    /// <summary>$93:9F87: Selected three Power Bomb image phases.</summary>
    private const int PowerBombPhases = 3;
    /// <summary>$93:9FBF: Selected four Bomb image phases.</summary>
    private const int BombPhases = 4;
    /// <summary>$93:9F87/$9FBF: Selected normal bomb-family hold5.</summary>
    private const ushort BombHold = 5;
    /// <summary>$93:9FA3/$9FE3: Selected fast bomb-family hold1.</summary>
    private const ushort FastBombHold = 1;
    /// <summary>$93:A007/$A039/$A0C1/$A16D: Selected six explosion image phases.</summary>
    private const int ExplosionPhases = 6;
    /// <summary>$93:A007/$A039/$A16D: Selected beam/missile explosion hold3.</summary>
    private const ushort ExplosionHold = 3;
    /// <summary>$93:A06B/$A095: Selected five shared bomb-explosion/Plasma-SBA image phases.</summary>
    private const int BombExplosionPhases = 5;
    /// <summary>$93:A06B/$A095: Selected shared bomb-explosion/Plasma-SBA hold2.</summary>
    private const ushort BombExplosionHold = 2;
    /// <summary>$93:A0C1: Selected super-missile explosion hold5.</summary>
    private const ushort SuperExplosionHold = 5;
    /// <summary>$93:A0F3/$A119: Selected four unused-projectile/echo trail phases.</summary>
    private const int EchoPhases = 4;
    /// <summary>$93:A0F3/$A119/$A13D: Selected special echo/trail hold2.</summary>
    private const ushort EchoHold = 2;
    /// <summary>$93:A13D: Selected three Spazer SBA growth phases.</summary>
    private const int SpazerSbaPhases = 3;
    /// <summary>$93:A159: Selected two Wave SBA image/trail phases.</summary>
    private const int WaveSbaPhases = 2;
    /// <summary>$93:A159: Selected Wave SBA hold8.</summary>
    private const ushort WaveSbaHold = 8;

    /// <summary>$93:9F87, Power Bomb normal cycle.</summary>
    private static int PowerBombStart => MissileStart + MissileProgramCount * CycleBytes(1);
    /// <summary>$93:9FA3, Power Bomb fast cycle.</summary>
    private static int FastPowerBombStart => PowerBombStart + CycleBytes(PowerBombPhases);
    /// <summary>$93:9FBF, Bomb normal cycle.</summary>
    private static int BombStart => FastPowerBombStart + CycleBytes(PowerBombPhases);
    /// <summary>$93:9FE3, Bomb fast cycle.</summary>
    private static int FastBombStart => BombStart + CycleBytes(BombPhases);
    /// <summary>$93:A007, BeamExplosion one-shot sequence.</summary>
    private static int BeamExplosionStart => FastBombStart + CycleBytes(BombPhases);
    /// <summary>$93:A039, MissileExplosion one-shot sequence.</summary>
    private static int MissileExplosionStart => BeamExplosionStart + OneShotBytes(ExplosionPhases);
    /// <summary>$93:A06B, BombExplosion one-shot sequence.</summary>
    private static int BombExplosionStart => MissileExplosionStart + OneShotBytes(ExplosionPhases);
    /// <summary>$93:A095, PlasmaSBA repeating explosion images.</summary>
    private static int PlasmaSbaStart => BombExplosionStart + OneShotBytes(BombExplosionPhases);
    /// <summary>$93:A0C1, SuperMissileExplosion one-shot sequence.</summary>
    private static int SuperExplosionStart => PlasmaSbaStart + CycleBytes(BombExplosionPhases);
    /// <summary>$93:A0F3, unused projectile25 loop; bounded reader compatibility remains supported.</summary>
    private static int UnusedProjectile25Start => SuperExplosionStart + OneShotBytes(ExplosionPhases);
    /// <summary>$93:A117, zero-component spritemap word; excluded from mechanics.</summary>
    private static int EmptySpritemap => UnusedProjectile25Start + CycleBytes(EchoPhases);
    /// <summary>$93:A119, ShinesparkEcho loop after the independent empty-spritemap word.</summary>
    private static int EchoStart => EmptySpritemap + sizeof(ushort);
    /// <summary>$93:A13D, Spazer SBA trail growth sequence.</summary>
    private static int SpazerSbaStart => EchoStart + CycleBytes(EchoPhases);
    /// <summary>$93:A159, Wave SBA two-phase cycle.</summary>
    private static int WaveSbaStart => SpazerSbaStart + CycleBytes(SpazerSbaPhases);
    /// <summary>$93:A16D, unused projectile27 repeating beam-explosion images.</summary>
    private static int UnusedProjectile27Start => WaveSbaStart + CycleBytes(WaveSbaPhases);

    private static int OneShotBytes(int phases) => phases * TimedRecordBytes + sizeof(ushort);

    internal static bool TryNonBeamWord(int address, out ushort value)
    {
        int offset = address - MissileStart;
        if ((uint)offset < MissileProgramCount * CycleBytes(1))
            return TryCycleWord(address, MissileStart + offset / CycleBytes(1) * CycleBytes(1), 1, MissileHold, out value);
        return TryNoTrailProgram(address, PowerBombStart, PowerBombPhases, BombHold, false, out value) ||
            TryNoTrailProgram(address, FastPowerBombStart, PowerBombPhases, FastBombHold, false, out value) ||
            TryNoTrailProgram(address, BombStart, BombPhases, BombHold, false, out value) ||
            TryNoTrailProgram(address, FastBombStart, BombPhases, FastBombHold, false, out value) ||
            TryNoTrailProgram(address, BeamExplosionStart, ExplosionPhases, ExplosionHold, true, out value) ||
            TryNoTrailProgram(address, MissileExplosionStart, ExplosionPhases, ExplosionHold, true, out value) ||
            TryNoTrailProgram(address, BombExplosionStart, BombExplosionPhases, BombExplosionHold, true, out value) ||
            TryNoTrailProgram(address, PlasmaSbaStart, BombExplosionPhases, BombExplosionHold, false, out value) ||
            TryNoTrailProgram(address, SuperExplosionStart, ExplosionPhases, SuperExplosionHold, true, out value) ||
            TryCycleWord(address, UnusedProjectile25Start, EchoPhases, EchoHold, out value) ||
            TryCycleWord(address, EchoStart, EchoPhases, EchoHold, out value) ||
            TryCycleWord(address, SpazerSbaStart, SpazerSbaPhases, EchoHold, out value, SpazerSbaPhases - 1) ||
            TryCycleWord(address, WaveSbaStart, WaveSbaPhases, WaveSbaHold, out value) ||
            TryNoTrailProgram(address, UnusedProjectile27Start, ExplosionPhases, ExplosionHold, false, out value);
    }

    // These image-only programs keep the trail frame at zero; one-shots delete rather than loop.
    private static bool TryNoTrailProgram(int address, int start, int phases, ushort hold, bool delete, out ushort value)
    {
        int offset = address - start;
        int recordsBytes = phases * TimedRecordBytes;
        value = 0;
        if ((uint)offset >= recordsBytes + (delete ? sizeof(ushort) : 2 * sizeof(ushort))) return false;
        if (offset < recordsBytes && offset % TimedRecordBytes == TimedRecordBytes - sizeof(ushort)) return true;
        if (delete && offset == recordsBytes) { value = SamusProjectileRomData.Instructions.Delete; return true; }
        return TryCycleWord(address, start, phases, hold, out value);
    }
    internal static ushort ReadWord(int address) =>
        TryPowerWord(address, out ushort value) || TryWaveIceWord(address, out value) ||
        TrySpazerWord(address, out value) || TryPlasmaWord(address, out value) ||
        TryChargedWord(address, out value) || TryNonBeamWord(address, out value) ||
        ReflectedListPrefixWords.TryGetValue(address, out value) ? value :
        throw new InvalidDataException(
            $"Projectile instruction mechanics word ${address:X6} is outside the compiled definitions.");

    /// <summary>
    /// The word preceding every list start $90:BE17 can install when it reflects a beam or
    /// missile. A trail spawned before the new list's first record reads it as an animation
    /// frame ($93:81D8); for the first list it lies in the data tables before $93:86DB.
    /// </summary>
    private static readonly FrozenDictionary<int, ushort> ReflectedListPrefixWords =
        new Dictionary<int, ushort>
        {
            [0x9386d9] = 0xa16d,
            [0x9386e5] = 0x86db,
            [0x9386f1] = 0x86e7,
            [0x9386fd] = 0x86f3,
            [0x938709] = 0x86ff,
            [0x938715] = 0x870b,
            [0x938721] = 0x8717,
            [0x93872d] = 0x8723,
            [0x938739] = 0x872f,
            [0x938741] = 0x0000,
            [0x9387c5] = 0x8743,
            [0x938849] = 0x87c7,
            [0x9388cd] = 0x884b,
            [0x938951] = 0x88cf,
            [0x938975] = 0x8953,
            [0x938991] = 0x8987,
            [0x9389ad] = 0x89a3,
            [0x9389c9] = 0x89bf,
            [0x9389e5] = 0x89db,
            [0x938a01] = 0x89f7,
            [0x938a1d] = 0x8a13,
            [0x938a39] = 0x8a2f,
            [0x938a55] = 0x8a4b,
            [0x938aa9] = 0x8a57,
            [0x938afd] = 0x8aab,
            [0x938b51] = 0x8aff,
            [0x938ba5] = 0x8b53,
            [0x938bf9] = 0x8ba7,
            [0x938c4d] = 0x8bfb,
            [0x938ca1] = 0x8c4f,
            [0x938cf5] = 0x8ca3,
            [0x938d09] = 0x8cff,
            [0x938d1d] = 0x8d13,
            [0x938d31] = 0x8d27,
            [0x938d45] = 0x8d3b,
            [0x938d4d] = 0x0000,
            [0x938d91] = 0x8d4f,
            [0x938d99] = 0x0000,
            [0x938ddd] = 0x8d9b,
            [0x938e29] = 0x8de7,
            [0x938e31] = 0x0000,
            [0x938e75] = 0x8e33,
            [0x938e89] = 0x8e77,
            [0x938e9d] = 0x8e8b,
            [0x938eb1] = 0x8e9f,
            [0x938ec5] = 0x8eb3,
            [0x938ed9] = 0x8ec7,
            [0x938eed] = 0x8edb,
            [0x938f01] = 0x8eef,
            [0x938f15] = 0x8f03,
            [0x938f1d] = 0x0000,
            [0x938fa1] = 0x8f1f,
            [0x939025] = 0x8fa3,
            [0x9390a9] = 0x9027,
            [0x93912d] = 0x90ab,
            [0x939151] = 0x912f,
            [0x939159] = 0x0000,
            [0x9391dd] = 0x915b,
            [0x939261] = 0x91df,
            [0x9392e5] = 0x9263,
            [0x939369] = 0x92e7,
            [0x9393bd] = 0x93ab,
            [0x939411] = 0x93ff,
            [0x939465] = 0x9453,
            [0x9394b9] = 0x94a7,
            [0x93957d] = 0x94db,
            [0x939641] = 0x959f,
            [0x939705] = 0x9663,
            [0x9397c9] = 0x9727,
            [0x93988d] = 0x97eb,
            [0x939951] = 0x98af,
            [0x939a15] = 0x9973,
            [0x939ad9] = 0x9a37,
            [0x939b1d] = 0x9b0b,
            [0x939b61] = 0x9b4f,
            [0x939ba5] = 0x9b93,
            [0x939be9] = 0x9bd7,
            [0x939c9d] = 0x9c1b,
            [0x939d51] = 0x9ccf,
            [0x939e05] = 0x9d83,
            [0x939eb9] = 0x9e37,
            [0x939ec5] = 0x9ebb,
            [0x939ed1] = 0x9ec7,
            [0x939edd] = 0x9ed3,
            [0x939ee9] = 0x9edf,
            [0x939ef5] = 0x9eeb,
            [0x939f01] = 0x9ef7,
            [0x939f0d] = 0x9f03,
            [0x939f19] = 0x9f0f,
            [0x939f25] = 0x9f1b,
            [0x939f31] = 0x9f27,
            [0x939f3d] = 0x9f33,
            [0x939f49] = 0x9f3f,
            [0x939f55] = 0x9f4b,
            [0x939f61] = 0x9f57,
            [0x939f6d] = 0x9f63,
        }.ToFrozenDictionary();

    /// <summary>Mutually exclusive native projectile program families; direction and frame remain separate axes.</summary>
    internal enum FrameFamily
    {
        Power, Wave, Ice, Spazer, SpazerWave, Plasma, PlasmaWave,
        ChargedPower, ChargedWave, ChargedIce, ChargedIceWave, ChargedSpazer,
        ChargedSpazerWave, ChargedPlasma, ChargedPlasmaWave,
        Missile, SuperMissile, SuperMissileLink, PowerBomb, FastPowerBomb,
        Bomb, FastBomb, BeamExplosion, MissileExplosion, BombExplosion, PlasmaSba,
        SuperExplosion, UnusedEcho, Echo, SpazerSba, WaveSba, UnusedExplosion,
    }

    /// <summary>Native timed-record identity; axis is program order and phase is the zero-based timed record.</summary>
    internal readonly record struct TimedFrame(FrameFamily Family, int Axis, int Phase);

    /// <summary>$93:86DB..A19C: classify exactly the 805 timed records, excluding every operand/control word.</summary>
    internal static bool TryTimedFrame(int address, out TimedFrame frame)
    {
        if (address == WaveUpPrelude) { frame = new(FrameFamily.Wave, 0, 0); return true; }
        if (address == ChargedWaveStart) { frame = new(FrameFamily.ChargedWave, 0, 0); return true; }
        if (address == ChargedIceWaveStart) { frame = new(FrameFamily.ChargedIceWave, 0, 0); return true; }
        return Group(PowerProgramsStart, PowerDirectionCount, 1, FrameFamily.Power, out frame) ||
            Group(WaveCyclesStart, WaveAxisCount, WavePhases, FrameFamily.Wave, out frame) ||
            Group(IceCycleStart, 1, IcePhases, FrameFamily.Ice, out frame) ||
            Group(SpazerProgramsStart, PowerDirectionCount, SpazerGrowthPhases, FrameFamily.Spazer, out frame) ||
            Group(SpazerWaveProgramsStart, PowerDirectionCount, SpazerWavePhases, FrameFamily.SpazerWave, out frame) ||
            Group(PlasmaProgramsStart, WaveAxisCount, 2, FrameFamily.Plasma, out frame) ||
            Group(PlasmaWaveProgramsStart, WaveAxisCount, 1 + PlasmaWaveCyclePhases, FrameFamily.PlasmaWave, out frame) ||
            Group(ChargedPowerStart, PowerDirectionCount, ChargedPowerPhases, FrameFamily.ChargedPower, out frame) ||
            Group(ChargedWaveStart + TimedRecordBytes, WaveAxisCount, ChargedWavePhases, FrameFamily.ChargedWave, out frame) ||
            Group(ChargedIceStart, 1, ChargedIcePhases, FrameFamily.ChargedIce, out frame) ||
            Group(ChargedIceWaveStart + TimedRecordBytes, WaveAxisCount, ChargedWavePhases, FrameFamily.ChargedIceWave, out frame) ||
            Group(ChargedSpazerStart, WaveAxisCount, ChargedSpazerPhases, FrameFamily.ChargedSpazer, out frame) ||
            Group(ChargedSpazerWaveStart, PowerDirectionCount, ChargedSpazerWavePhases, FrameFamily.ChargedSpazerWave, out frame) ||
            Group(ChargedPlasmaStart, WaveAxisCount, ChargedPlasmaPhases, FrameFamily.ChargedPlasma, out frame) ||
            Group(ChargedPlasmaWaveStart, WaveAxisCount, ChargedPlasmaWavePhases, FrameFamily.ChargedPlasmaWave, out frame) ||
            Group(MissileStart, PowerDirectionCount, 1, FrameFamily.Missile, out frame) ||
            Group(MissileStart + PowerDirectionCount * CycleBytes(1), PowerDirectionCount, 1, FrameFamily.SuperMissile, out frame) ||
            Group(MissileStart + 2 * PowerDirectionCount * CycleBytes(1), 1, 1, FrameFamily.SuperMissileLink, out frame) ||
            Group(PowerBombStart, 1, PowerBombPhases, FrameFamily.PowerBomb, out frame) ||
            Group(FastPowerBombStart, 1, PowerBombPhases, FrameFamily.FastPowerBomb, out frame) ||
            Group(BombStart, 1, BombPhases, FrameFamily.Bomb, out frame) ||
            Group(FastBombStart, 1, BombPhases, FrameFamily.FastBomb, out frame) ||
            Group(BeamExplosionStart, 1, ExplosionPhases, FrameFamily.BeamExplosion, out frame, delete: true) ||
            Group(MissileExplosionStart, 1, ExplosionPhases, FrameFamily.MissileExplosion, out frame, delete: true) ||
            Group(BombExplosionStart, 1, BombExplosionPhases, FrameFamily.BombExplosion, out frame, delete: true) ||
            Group(PlasmaSbaStart, 1, BombExplosionPhases, FrameFamily.PlasmaSba, out frame) ||
            Group(SuperExplosionStart, 1, ExplosionPhases, FrameFamily.SuperExplosion, out frame, delete: true) ||
            Group(UnusedProjectile25Start, 1, EchoPhases, FrameFamily.UnusedEcho, out frame) ||
            Group(EchoStart, 1, EchoPhases, FrameFamily.Echo, out frame) ||
            Group(SpazerSbaStart, 1, SpazerSbaPhases, FrameFamily.SpazerSba, out frame) ||
            Group(WaveSbaStart, 1, WaveSbaPhases, FrameFamily.WaveSba, out frame) ||
            Group(UnusedProjectile27Start, 1, ExplosionPhases, FrameFamily.UnusedExplosion, out frame);

        bool Group(int start, int directions, int phases, FrameFamily family, out TimedFrame result, bool delete = false)
        {
            long offset = (long)address - start;
            int stride = delete ? OneShotBytes(phases) : CycleBytes(phases);
            result = default;
            if (offset < 0 || offset >= directions * stride) return false;
            int local = (int)(offset % stride);
            if (local >= phases * TimedRecordBytes || local % TimedRecordBytes != 0) return false;
            result = new(family, (int)(offset / stride), local / TimedRecordBytes);
            return true;
        }
    }

    /// <summary>Ordered native pointer identities calculated from program layout; no cached pointer table.</summary>
    internal static IEnumerable<ushort> EnumerateTimedPointers()
    {
        int end = UnusedProjectile27Start + CycleBytes(ExplosionPhases);
        for (int address = PowerProgramsStart; address < end; address += sizeof(ushort))
            if (TryTimedFrame(address, out _)) yield return unchecked((ushort)address);
    }
}
