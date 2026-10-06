namespace SuperMetroid.Core.Game;

/// <summary>Projectile frame durations, trail indices and control flow, independent of spritemap references.</summary>
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

    /// <summary>$93:86DB et al.: REQUIRED selected fifteen-frame Power hold; no timing exception is claimed.</summary>
    private const ushort RequiredPowerHold = 15;

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
                value = RequiredPowerHold;
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
    /// <summary>$93:8743..87C2: REQUIRED sixteen selected trail phases per Wave cycle.</summary>
    private const int RequiredWavePhases = 16;
    /// <summary>$93:873B: REQUIRED upward Wave prelude hold4.</summary>
    private const ushort RequiredWavePreludeHold = 4;
    /// <summary>$93:8743 and $93:8953: REQUIRED one-update Wave/Ice holds, independently unresolved timing.</summary>
    private const ushort RequiredWaveIceHold = 1;
    /// <summary>$93:87C3: each Wave cycle ends with Goto and its own phase-zero target.</summary>
    private const int WaveCycleBytes = RequiredWavePhases * TimedRecordBytes + 2 * sizeof(ushort);
    /// <summary>$93:8953, InstList_SamusProjectile_Ice, follows the four Wave axes.</summary>
    private const int IceCycleStart = WaveCyclesStart + WaveAxisCount * WaveCycleBytes;
    /// <summary>$93:8953..8972: REQUIRED four selected Ice trail phases.</summary>
    private const int RequiredIcePhases = 4;

    internal static bool TryWaveIceWord(int address, out ushort value)
    {
        value = 0;
        if (address == WaveUpPrelude) { value = RequiredWavePreludeHold; return true; }
        if (address == WaveUpPrelude + TimedRecordBytes - sizeof(ushort)) return true;
        int offset = address - WaveCyclesStart;
        if ((uint)offset < WaveAxisCount * WaveCycleBytes)
            return TryCycleWord(address, WaveCyclesStart + offset / WaveCycleBytes * WaveCycleBytes,
                RequiredWavePhases, RequiredWaveIceHold, out value);
        return TryCycleWord(address, IceCycleStart, RequiredIcePhases, RequiredWaveIceHold, out value);
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
    private const int SpazerProgramsStart = IceCycleStart + RequiredIcePhases * TimedRecordBytes + 2 * sizeof(ushort);
    /// <summary>$93:8977..898E: REQUIRED three selected growth stages before holding the final Spazer phase.</summary>
    private const int RequiredSpazerGrowthPhases = 3;
    /// <summary>$93:8977 and $93:8A57: REQUIRED selected two-update Spazer/SpazerWave phase hold.</summary>
    private const ushort RequiredSpazerHold = 2;
    /// <summary>$93:898F Goto and target follow three timed growth stages.</summary>
    private const int SpazerProgramBytes = RequiredSpazerGrowthPhases * TimedRecordBytes + 2 * sizeof(ushort);
    /// <summary>$93:8A57, SpazerWave/SpazerIceWave Up, after eight compass growth programs.</summary>
    private const int SpazerWaveProgramsStart = SpazerProgramsStart + PowerDirectionCount * SpazerProgramBytes;
    /// <summary>$93:8A57..8AA6: REQUIRED ten selected spread-cycle phases.</summary>
    private const int RequiredSpazerWavePhases = 10;
    /// <summary>$93:8AA7 Goto and target follow the complete spread cycle.</summary>
    private const int SpazerWaveProgramBytes = RequiredSpazerWavePhases * TimedRecordBytes + 2 * sizeof(ushort);

    internal static bool TrySpazerWord(int address, out ushort value)
    {
        int offset = address - SpazerProgramsStart;
        if ((uint)offset < PowerDirectionCount * SpazerProgramBytes)
            return TryCycleWord(address, SpazerProgramsStart + offset / SpazerProgramBytes * SpazerProgramBytes,
                RequiredSpazerGrowthPhases, RequiredSpazerHold, out value, RequiredSpazerGrowthPhases - 1);
        offset = address - SpazerWaveProgramsStart;
        if ((uint)offset < PowerDirectionCount * SpazerWaveProgramBytes)
            return TryCycleWord(address, SpazerWaveProgramsStart + offset / SpazerWaveProgramBytes * SpazerWaveProgramBytes,
                RequiredSpazerWavePhases, RequiredSpazerHold, out value);
        value = 0;
        return false;
    }
    /// <summary>$93:8CF7, Plasma/PlasmaIce vertical prelude, after eight SpazerWave directions.</summary>
    private const int PlasmaProgramsStart = SpazerWaveProgramsStart + PowerDirectionCount * SpazerWaveProgramBytes;
    /// <summary>$93:8CF7/8CFF: one initial record followed by one persistent record and its Goto.</summary>
    private const int PlasmaProgramBytes = 2 * TimedRecordBytes + 2 * sizeof(ushort);
    /// <summary>$93:8CF7 and $93:8D47: REQUIRED one-update Plasma entry hold.</summary>
    private const ushort RequiredPlasmaPreludeHold = 1;
    /// <summary>$93:8CFF: REQUIRED fifteen-update persistent Plasma hold.</summary>
    private const ushort RequiredPlasmaHold = 15;
    /// <summary>$93:8D47, PlasmaIceWave vertical prelude, after the four opposite-paired Plasma axes.</summary>
    private const int PlasmaWaveProgramsStart = PlasmaProgramsStart + WaveAxisCount * PlasmaProgramBytes;
    /// <summary>$93:8D4F..8D8E: REQUIRED eight selected cyclic PlasmaWave phases after the entry phase.</summary>
    private const int RequiredPlasmaWaveCyclePhases = 8;
    /// <summary>$93:8D4F: REQUIRED two-update PlasmaWave cyclic hold.</summary>
    private const ushort RequiredPlasmaWaveHold = 2;
    /// <summary>$93:8D47..8D92: one entry record, eight cyclic records, then Goto to the first cyclic phase.</summary>
    private const int PlasmaWaveProgramBytes = (1 + RequiredPlasmaWaveCyclePhases) * TimedRecordBytes + 2 * sizeof(ushort);

    internal static bool TryPlasmaWord(int address, out ushort value)
    {
        int offset = address - PlasmaProgramsStart;
        int start;
        bool accepted;
        if ((uint)offset < WaveAxisCount * PlasmaProgramBytes)
        {
            start = PlasmaProgramsStart + offset / PlasmaProgramBytes * PlasmaProgramBytes;
            accepted = TryCycleWord(address, start, 2, RequiredPlasmaHold, out value, loopPhase: 1);
        }
        else
        {
            offset = address - PlasmaWaveProgramsStart;
            if ((uint)offset >= WaveAxisCount * PlasmaWaveProgramBytes) { value = 0; return false; }
            start = PlasmaWaveProgramsStart + offset / PlasmaWaveProgramBytes * PlasmaWaveProgramBytes;
            accepted = TryCycleWord(address, start, 1 + RequiredPlasmaWaveCyclePhases, RequiredPlasmaWaveHold, out value, loopPhase: 1);
        }
        // The initial entry is skipped by subsequent Gotos; its hold remains independently selected.
        if (address == start) value = RequiredPlasmaPreludeHold;
        return accepted;
    }
    /// <summary>$93:8E77, charged Power Up; follows the four PlasmaWave axes.</summary>
    private const int ChargedPowerStart = PlasmaWaveProgramsStart + WaveAxisCount * PlasmaWaveProgramBytes;
    /// <summary>$93:8E77: REQUIRED two selected charged-Power phases.</summary>
    private const int RequiredChargedPowerPhases = 2;
    /// <summary>$93:8E77..9EBA: REQUIRED common one-update charged phase hold (excluding upward preludes).</summary>
    private const ushort RequiredChargedHold = 1;
    /// <summary>$93:8F17/$9153: REQUIRED three-update upward charged Wave/IceWave prelude hold.</summary>
    private const ushort RequiredChargedWavePreludeHold = 3;
    /// <summary>$93:8F1F and $915B cycles: REQUIRED sixteen charged Wave/IceWave phases.</summary>
    private const int RequiredChargedWavePhases = 16;
    /// <summary>$93:912F: REQUIRED four charged Ice phases.</summary>
    private const int RequiredChargedIcePhases = 4;
    /// <summary>$93:936B: REQUIRED ten charged Spazer phases.</summary>
    private const int RequiredChargedSpazerPhases = 10;
    /// <summary>$93:93BB target93AB: REQUIRED charged Spazer loop starts at phase8.</summary>
    private const int RequiredChargedSpazerLoopPhase = 8;
    /// <summary>$93:94BB: REQUIRED twenty-four charged SpazerWave phases.</summary>
    private const int RequiredChargedSpazerWavePhases = 24;
    /// <summary>$93:957B target94DB: REQUIRED charged SpazerWave loop starts at phase4.</summary>
    private const int RequiredChargedSpazerWaveLoopPhase = 4;
    /// <summary>$93:9ADB: REQUIRED eight charged Plasma phases.</summary>
    private const int RequiredChargedPlasmaPhases = 8;
    /// <summary>$93:9B1B target9B0B: REQUIRED charged Plasma loop starts at phase6.</summary>
    private const int RequiredChargedPlasmaLoopPhase = 6;
    /// <summary>$93:9BEB: REQUIRED twenty-two charged PlasmaWave phases.</summary>
    private const int RequiredChargedPlasmaWavePhases = 22;
    /// <summary>$93:9C9B target9C1B: REQUIRED charged PlasmaWave loop starts at phase6.</summary>
    private const int RequiredChargedPlasmaWaveLoopPhase = 6;

    /// <summary>$93:8F17, charged Wave upward prelude, following eight charged Power compass programs.</summary>
    private static int ChargedWaveStart => ChargedPowerStart + PowerDirectionCount * CycleBytes(RequiredChargedPowerPhases);
    /// <summary>$93:912F, charged Ice, following the upward prelude and four charged Wave axes.</summary>
    private static int ChargedIceStart => ChargedWaveStart + TimedRecordBytes + WaveAxisCount * CycleBytes(RequiredChargedWavePhases);
    /// <summary>$93:9153, charged IceWave upward prelude, following charged Ice.</summary>
    private static int ChargedIceWaveStart => ChargedIceStart + CycleBytes(RequiredChargedIcePhases);
    /// <summary>$93:936B, charged Spazer vertical axis, following charged IceWave.</summary>
    private static int ChargedSpazerStart => ChargedIceWaveStart + TimedRecordBytes + WaveAxisCount * CycleBytes(RequiredChargedWavePhases);
    /// <summary>$93:94BB, charged SpazerWave Up, following four charged Spazer axes.</summary>
    private static int ChargedSpazerWaveStart => ChargedSpazerStart + WaveAxisCount * CycleBytes(RequiredChargedSpazerPhases);
    /// <summary>$93:9ADB, charged Plasma vertical axis, following eight charged SpazerWave directions.</summary>
    private static int ChargedPlasmaStart => ChargedSpazerWaveStart + PowerDirectionCount * CycleBytes(RequiredChargedSpazerWavePhases);
    /// <summary>$93:9BEB, charged PlasmaWave vertical axis, following four charged Plasma axes.</summary>
    private static int ChargedPlasmaWaveStart => ChargedPlasmaStart + WaveAxisCount * CycleBytes(RequiredChargedPlasmaPhases);

    private static int CycleBytes(int phases) => phases * TimedRecordBytes + 2 * sizeof(ushort);

    internal static bool TryChargedWord(int address, out ushort value) =>
        TryDirectionCycles(address, ChargedPowerStart, PowerDirectionCount, RequiredChargedPowerPhases, 0, out value) ||
        TryChargedWaveWord(address, ChargedWaveStart, out value) ||
        TryCycleWord(address, ChargedIceStart, RequiredChargedIcePhases, RequiredChargedHold, out value) ||
        TryChargedWaveWord(address, ChargedIceWaveStart, out value) ||
        TryDirectionCycles(address, ChargedSpazerStart, WaveAxisCount, RequiredChargedSpazerPhases, RequiredChargedSpazerLoopPhase, out value) ||
        TryDirectionCycles(address, ChargedSpazerWaveStart, PowerDirectionCount, RequiredChargedSpazerWavePhases, RequiredChargedSpazerWaveLoopPhase, out value) ||
        TryDirectionCycles(address, ChargedPlasmaStart, WaveAxisCount, RequiredChargedPlasmaPhases, RequiredChargedPlasmaLoopPhase, out value) ||
        TryDirectionCycles(address, ChargedPlasmaWaveStart, WaveAxisCount, RequiredChargedPlasmaWavePhases, RequiredChargedPlasmaWaveLoopPhase, out value);

    private static bool TryDirectionCycles(int address, int start, int directions, int phases, int loopPhase, out ushort value)
    {
        int offset = address - start;
        int stride = CycleBytes(phases);
        if ((uint)offset < directions * stride)
            return TryCycleWord(address, start + offset / stride * stride, phases, RequiredChargedHold, out value, loopPhase);
        value = 0;
        return false;
    }

    private static bool TryChargedWaveWord(int address, int prelude, out ushort value)
    {
        if (address == prelude) { value = RequiredChargedWavePreludeHold; return true; }
        if (address == prelude + TimedRecordBytes - sizeof(ushort)) { value = 0; return true; }
        return TryDirectionCycles(address, prelude + TimedRecordBytes, WaveAxisCount, RequiredChargedWavePhases, 0, out value);
    }
    /// <summary>$93:9EBB, Missile Up; follows all charged PlasmaWave axes.</summary>
    private static int MissileStart => ChargedPlasmaWaveStart + WaveAxisCount * CycleBytes(RequiredChargedPlasmaWavePhases);
    /// <summary>$93:9EBB..9F86: eight missile directions, eight super directions and one link, each a single-phase loop.</summary>
    private const int MissileProgramCount = 2 * PowerDirectionCount + 1;
    /// <summary>$93:9EBB..9F7B: REQUIRED fifteen-update missile/super/link hold.</summary>
    private const ushort RequiredMissileHold = 15;
    /// <summary>$93:9F87: REQUIRED three Power Bomb image phases.</summary>
    private const int RequiredPowerBombPhases = 3;
    /// <summary>$93:9FBF: REQUIRED four Bomb image phases.</summary>
    private const int RequiredBombPhases = 4;
    /// <summary>$93:9F87/$9FBF: REQUIRED normal bomb-family hold5.</summary>
    private const ushort RequiredBombHold = 5;
    /// <summary>$93:9FA3/$9FE3: REQUIRED fast bomb-family hold1.</summary>
    private const ushort RequiredFastBombHold = 1;
    /// <summary>$93:A007/$A039/$A0C1/$A16D: REQUIRED six explosion image phases.</summary>
    private const int RequiredExplosionPhases = 6;
    /// <summary>$93:A007/$A039/$A16D: REQUIRED beam/missile explosion hold3.</summary>
    private const ushort RequiredExplosionHold = 3;
    /// <summary>$93:A06B/$A095: REQUIRED five shared bomb-explosion/Plasma-SBA image phases.</summary>
    private const int RequiredBombExplosionPhases = 5;
    /// <summary>$93:A06B/$A095: REQUIRED shared bomb-explosion/Plasma-SBA hold2.</summary>
    private const ushort RequiredBombExplosionHold = 2;
    /// <summary>$93:A0C1: REQUIRED super-missile explosion hold5.</summary>
    private const ushort RequiredSuperExplosionHold = 5;
    /// <summary>$93:A0F3/$A119: REQUIRED four unused-projectile/echo trail phases.</summary>
    private const int RequiredEchoPhases = 4;
    /// <summary>$93:A0F3/$A119/$A13D: REQUIRED special echo/trail hold2.</summary>
    private const ushort RequiredEchoHold = 2;
    /// <summary>$93:A13D: REQUIRED three Spazer SBA growth phases.</summary>
    private const int RequiredSpazerSbaPhases = 3;
    /// <summary>$93:A159: REQUIRED two Wave SBA image/trail phases.</summary>
    private const int RequiredWaveSbaPhases = 2;
    /// <summary>$93:A159: REQUIRED Wave SBA hold8.</summary>
    private const ushort RequiredWaveSbaHold = 8;

    /// <summary>$93:9F87, Power Bomb normal cycle.</summary>
    private static int PowerBombStart => MissileStart + MissileProgramCount * CycleBytes(1);
    /// <summary>$93:9FA3, Power Bomb fast cycle.</summary>
    private static int FastPowerBombStart => PowerBombStart + CycleBytes(RequiredPowerBombPhases);
    /// <summary>$93:9FBF, Bomb normal cycle.</summary>
    private static int BombStart => FastPowerBombStart + CycleBytes(RequiredPowerBombPhases);
    /// <summary>$93:9FE3, Bomb fast cycle.</summary>
    private static int FastBombStart => BombStart + CycleBytes(RequiredBombPhases);
    /// <summary>$93:A007, BeamExplosion one-shot sequence.</summary>
    private static int BeamExplosionStart => FastBombStart + CycleBytes(RequiredBombPhases);
    /// <summary>$93:A039, MissileExplosion one-shot sequence.</summary>
    private static int MissileExplosionStart => BeamExplosionStart + OneShotBytes(RequiredExplosionPhases);
    /// <summary>$93:A06B, BombExplosion one-shot sequence.</summary>
    private static int BombExplosionStart => MissileExplosionStart + OneShotBytes(RequiredExplosionPhases);
    /// <summary>$93:A095, PlasmaSBA repeating explosion images.</summary>
    private static int PlasmaSbaStart => BombExplosionStart + OneShotBytes(RequiredBombExplosionPhases);
    /// <summary>$93:A0C1, SuperMissileExplosion one-shot sequence.</summary>
    private static int SuperExplosionStart => PlasmaSbaStart + CycleBytes(RequiredBombExplosionPhases);
    /// <summary>$93:A0F3, unused projectile25 loop; bounded reader compatibility remains supported.</summary>
    private static int UnusedProjectile25Start => SuperExplosionStart + OneShotBytes(RequiredExplosionPhases);
    /// <summary>$93:A117, zero-component spritemap word; excluded from mechanics.</summary>
    private static int EmptySpritemap => UnusedProjectile25Start + CycleBytes(RequiredEchoPhases);
    /// <summary>$93:A119, ShinesparkEcho loop after the independent empty-spritemap word.</summary>
    private static int EchoStart => EmptySpritemap + sizeof(ushort);
    /// <summary>$93:A13D, Spazer SBA trail growth sequence.</summary>
    private static int SpazerSbaStart => EchoStart + CycleBytes(RequiredEchoPhases);
    /// <summary>$93:A159, Wave SBA two-phase cycle.</summary>
    private static int WaveSbaStart => SpazerSbaStart + CycleBytes(RequiredSpazerSbaPhases);
    /// <summary>$93:A16D, unused projectile27 repeating beam-explosion images.</summary>
    private static int UnusedProjectile27Start => WaveSbaStart + CycleBytes(RequiredWaveSbaPhases);

    private static int OneShotBytes(int phases) => phases * TimedRecordBytes + sizeof(ushort);

    internal static bool TryNonBeamWord(int address, out ushort value)
    {
        int offset = address - MissileStart;
        if ((uint)offset < MissileProgramCount * CycleBytes(1))
            return TryCycleWord(address, MissileStart + offset / CycleBytes(1) * CycleBytes(1), 1, RequiredMissileHold, out value);
        return TryNoTrailProgram(address, PowerBombStart, RequiredPowerBombPhases, RequiredBombHold, false, out value) ||
            TryNoTrailProgram(address, FastPowerBombStart, RequiredPowerBombPhases, RequiredFastBombHold, false, out value) ||
            TryNoTrailProgram(address, BombStart, RequiredBombPhases, RequiredBombHold, false, out value) ||
            TryNoTrailProgram(address, FastBombStart, RequiredBombPhases, RequiredFastBombHold, false, out value) ||
            TryNoTrailProgram(address, BeamExplosionStart, RequiredExplosionPhases, RequiredExplosionHold, true, out value) ||
            TryNoTrailProgram(address, MissileExplosionStart, RequiredExplosionPhases, RequiredExplosionHold, true, out value) ||
            TryNoTrailProgram(address, BombExplosionStart, RequiredBombExplosionPhases, RequiredBombExplosionHold, true, out value) ||
            TryNoTrailProgram(address, PlasmaSbaStart, RequiredBombExplosionPhases, RequiredBombExplosionHold, false, out value) ||
            TryNoTrailProgram(address, SuperExplosionStart, RequiredExplosionPhases, RequiredSuperExplosionHold, true, out value) ||
            TryCycleWord(address, UnusedProjectile25Start, RequiredEchoPhases, RequiredEchoHold, out value) ||
            TryCycleWord(address, EchoStart, RequiredEchoPhases, RequiredEchoHold, out value) ||
            TryCycleWord(address, SpazerSbaStart, RequiredSpazerSbaPhases, RequiredEchoHold, out value, RequiredSpazerSbaPhases - 1) ||
            TryCycleWord(address, WaveSbaStart, RequiredWaveSbaPhases, RequiredWaveSbaHold, out value) ||
            TryNoTrailProgram(address, UnusedProjectile27Start, RequiredExplosionPhases, RequiredExplosionHold, false, out value);
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
        TryChargedWord(address, out value) || TryNonBeamWord(address, out value) ? value :
        throw new InvalidDataException(
            $"Projectile instruction mechanics word ${address:X6} is outside the compiled definitions.");
}
