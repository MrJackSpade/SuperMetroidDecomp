using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Where each Samus projectile program stands in bank $93 (#1301): its list entry and the word just before it, which a
/// trail reads as an animation frame from a freshly installed list. The native instruction pointer a cursor stands for
/// is what the projectile WRAM mirrors and the glitch beams' list arithmetic see.
/// </summary>
internal static class SamusProjectileProgramAddresses
{
    /// <summary>Each program's list entry and the word preceding it.</summary>
    private static readonly (SamusProjectileProgram Program, ushort Entry, ushort Prefix)[] Entries =
    [
        (SamusProjectileProgram.PowerUpFacingRight, 0x86db, 0xa16d),
        (SamusProjectileProgram.PowerUpRight, 0x86e7, 0x86db),
        (SamusProjectileProgram.PowerRight, 0x86f3, 0x86e7),
        (SamusProjectileProgram.PowerDownRight, 0x86ff, 0x86f3),
        (SamusProjectileProgram.PowerDownFacingRight, 0x870b, 0x86ff),
        (SamusProjectileProgram.PowerDownLeft, 0x8717, 0x870b),
        (SamusProjectileProgram.PowerLeft, 0x8723, 0x8717),
        (SamusProjectileProgram.PowerUpLeft, 0x872f, 0x8723),
        (SamusProjectileProgram.WaveUpFacingRight, 0x873b, 0x872f),
        (SamusProjectileProgram.WaveDownFacingRight, 0x8743, 0x0000),
        (SamusProjectileProgram.WaveUpRight, 0x87c7, 0x8743),
        (SamusProjectileProgram.WaveRight, 0x884b, 0x87c7),
        (SamusProjectileProgram.WaveDownRight, 0x88cf, 0x884b),
        (SamusProjectileProgram.IceUpFacingRight, 0x8953, 0x88cf),
        (SamusProjectileProgram.SpazerUpFacingRight, 0x8977, 0x8953),
        (SamusProjectileProgram.SpazerUpFacingRightLoop, 0x8987, 0x0001),
        (SamusProjectileProgram.SpazerUpRight, 0x8993, 0x8987),
        (SamusProjectileProgram.SpazerUpRightLoop, 0x89a3, 0x0001),
        (SamusProjectileProgram.SpazerRight, 0x89af, 0x89a3),
        (SamusProjectileProgram.SpazerRightLoop, 0x89bf, 0x0001),
        (SamusProjectileProgram.SpazerDownRight, 0x89cb, 0x89bf),
        (SamusProjectileProgram.SpazerDownRightLoop, 0x89db, 0x0001),
        (SamusProjectileProgram.SpazerDownFacingRight, 0x89e7, 0x89db),
        (SamusProjectileProgram.SpazerDownFacingRightLoop, 0x89f7, 0x0001),
        (SamusProjectileProgram.SpazerDownLeft, 0x8a03, 0x89f7),
        (SamusProjectileProgram.SpazerDownLeftLoop, 0x8a13, 0x0001),
        (SamusProjectileProgram.SpazerLeft, 0x8a1f, 0x8a13),
        (SamusProjectileProgram.SpazerLeftLoop, 0x8a2f, 0x0001),
        (SamusProjectileProgram.SpazerUpLeft, 0x8a3b, 0x8a2f),
        (SamusProjectileProgram.SpazerUpLeftLoop, 0x8a4b, 0x0001),
        (SamusProjectileProgram.SpazerWaveUpFacingRight, 0x8a57, 0x8a4b),
        (SamusProjectileProgram.SpazerWaveUpRight, 0x8aab, 0x8a57),
        (SamusProjectileProgram.SpazerWaveRight, 0x8aff, 0x8aab),
        (SamusProjectileProgram.SpazerWaveDownRight, 0x8b53, 0x8aff),
        (SamusProjectileProgram.SpazerWaveDownFacingRight, 0x8ba7, 0x8b53),
        (SamusProjectileProgram.SpazerWaveDownLeft, 0x8bfb, 0x8ba7),
        (SamusProjectileProgram.SpazerWaveLeft, 0x8c4f, 0x8bfb),
        (SamusProjectileProgram.SpazerWaveUpLeft, 0x8ca3, 0x8c4f),
        (SamusProjectileProgram.PlasmaUpFacingRight, 0x8cf7, 0x8ca3),
        (SamusProjectileProgram.PlasmaUpFacingRightLoop, 0x8cff, 0x0000),
        (SamusProjectileProgram.PlasmaUpRight, 0x8d0b, 0x8cff),
        (SamusProjectileProgram.PlasmaUpRightLoop, 0x8d13, 0x0000),
        (SamusProjectileProgram.PlasmaRight, 0x8d1f, 0x8d13),
        (SamusProjectileProgram.PlasmaRightLoop, 0x8d27, 0x0000),
        (SamusProjectileProgram.PlasmaDownRight, 0x8d33, 0x8d27),
        (SamusProjectileProgram.PlasmaDownRightLoop, 0x8d3b, 0x0000),
        (SamusProjectileProgram.PlasmaIceWaveUpFacingRight, 0x8d47, 0x8d3b),
        (SamusProjectileProgram.PlasmaWaveUpFacingRight, 0x8d4f, 0x0000),
        (SamusProjectileProgram.PlasmaIceWaveUpRight, 0x8d93, 0x8d4f),
        (SamusProjectileProgram.PlasmaWaveUpRight, 0x8d9b, 0x0000),
        (SamusProjectileProgram.PlasmaWaveRight, 0x8ddf, 0x8d9b),
        (SamusProjectileProgram.PlasmaWaveRightLoop, 0x8de7, 0x0000),
        (SamusProjectileProgram.PlasmaIceWaveDownRight, 0x8e2b, 0x8de7),
        (SamusProjectileProgram.PlasmaWaveDownRight, 0x8e33, 0x0000),
        (SamusProjectileProgram.ChargedPowerUpFacingRight, 0x8e77, 0x8e33),
        (SamusProjectileProgram.ChargedPowerUpRight, 0x8e8b, 0x8e77),
        (SamusProjectileProgram.ChargedPowerRight, 0x8e9f, 0x8e8b),
        (SamusProjectileProgram.ChargedPowerDownRight, 0x8eb3, 0x8e9f),
        (SamusProjectileProgram.ChargedPowerDownFacingRight, 0x8ec7, 0x8eb3),
        (SamusProjectileProgram.ChargedPowerDownLeft, 0x8edb, 0x8ec7),
        (SamusProjectileProgram.ChargedPowerLeft, 0x8eef, 0x8edb),
        (SamusProjectileProgram.ChargedPowerUpLeft, 0x8f03, 0x8eef),
        (SamusProjectileProgram.ChargedWaveUpFacingRight, 0x8f17, 0x8f03),
        (SamusProjectileProgram.ChargedWaveDownFacingRight, 0x8f1f, 0x0000),
        (SamusProjectileProgram.ChargedWaveUpRight, 0x8fa3, 0x8f1f),
        (SamusProjectileProgram.ChargedWaveRight, 0x9027, 0x8fa3),
        (SamusProjectileProgram.ChargedWaveDownRight, 0x90ab, 0x9027),
        (SamusProjectileProgram.ChargedIceUpFacingRight, 0x912f, 0x90ab),
        (SamusProjectileProgram.ChargedIceWaveUpFacingRight, 0x9153, 0x912f),
        (SamusProjectileProgram.ChargedIceWaveDownFacingRight, 0x915b, 0x0000),
        (SamusProjectileProgram.ChargedIceWaveUpRight, 0x91df, 0x915b),
        (SamusProjectileProgram.ChargedIceWaveRight, 0x9263, 0x91df),
        (SamusProjectileProgram.ChargedIceWaveDownRight, 0x92e7, 0x9263),
        (SamusProjectileProgram.ChargedSpazerUpFacingRight, 0x936b, 0x92e7),
        (SamusProjectileProgram.ChargedSpazerUpFacingRightLoop, 0x93ab, 0x0007),
        (SamusProjectileProgram.ChargedSpazerUpRight, 0x93bf, 0x93ab),
        (SamusProjectileProgram.ChargedSpazerUpRightLoop, 0x93ff, 0x0007),
        (SamusProjectileProgram.ChargedSpazerRight, 0x9413, 0x93ff),
        (SamusProjectileProgram.ChargedSpazerRightLoop, 0x9453, 0x0007),
        (SamusProjectileProgram.ChargedSpazerDownRight, 0x9467, 0x9453),
        (SamusProjectileProgram.ChargedSpazerDownRightLoop, 0x94a7, 0x0007),
        (SamusProjectileProgram.ChargedSpazerWaveUpFacingRight, 0x94bb, 0x94a7),
        (SamusProjectileProgram.ChargedSpazerWaveUpFacingRightLoop, 0x94db, 0x0003),
        (SamusProjectileProgram.ChargedSpazerWaveUpRight, 0x957f, 0x94db),
        (SamusProjectileProgram.ChargedSpazerWaveUpRightLoop, 0x959f, 0x0003),
        (SamusProjectileProgram.ChargedSpazerWaveRight, 0x9643, 0x959f),
        (SamusProjectileProgram.ChargedSpazerWaveRightLoop, 0x9663, 0x0003),
        (SamusProjectileProgram.ChargedSpazerWaveDownRight, 0x9707, 0x9663),
        (SamusProjectileProgram.ChargedSpazerWaveDownRightLoop, 0x9727, 0x0003),
        (SamusProjectileProgram.ChargedSpazerWaveDownFacingRight, 0x97cb, 0x9727),
        (SamusProjectileProgram.ChargedSpazerWaveDownFacingRightLoop, 0x97eb, 0x0003),
        (SamusProjectileProgram.ChargedSpazerWaveDownLeft, 0x988f, 0x97eb),
        (SamusProjectileProgram.ChargedSpazerWaveDownLeftLoop, 0x98af, 0x0003),
        (SamusProjectileProgram.ChargedSpazerWaveLeft, 0x9953, 0x98af),
        (SamusProjectileProgram.ChargedSpazerWaveLeftLoop, 0x9973, 0x0003),
        (SamusProjectileProgram.ChargedSpazerWaveUpLeft, 0x9a17, 0x9973),
        (SamusProjectileProgram.ChargedSpazerWaveUpLeftLoop, 0x9a37, 0x0003),
        (SamusProjectileProgram.ChargedPlasmaUpFacingRight, 0x9adb, 0x9a37),
        (SamusProjectileProgram.ChargedPlasmaUpFacingRightLoop, 0x9b0b, 0x0005),
        (SamusProjectileProgram.ChargedPlasmaUpRight, 0x9b1f, 0x9b0b),
        (SamusProjectileProgram.ChargedPlasmaUpRightLoop, 0x9b4f, 0x0005),
        (SamusProjectileProgram.ChargedPlasmaRight, 0x9b63, 0x9b4f),
        (SamusProjectileProgram.ChargedPlasmaRightLoop, 0x9b93, 0x0005),
        (SamusProjectileProgram.ChargedPlasmaDownRight, 0x9ba7, 0x9b93),
        (SamusProjectileProgram.ChargedPlasmaDownRightLoop, 0x9bd7, 0x0005),
        (SamusProjectileProgram.ChargedPlasmaWaveUpFacingRight, 0x9beb, 0x9bd7),
        (SamusProjectileProgram.ChargedPlasmaWaveUpFacingRightLoop, 0x9c1b, 0x0005),
        (SamusProjectileProgram.ChargedPlasmaWaveUpRight, 0x9c9f, 0x9c1b),
        (SamusProjectileProgram.ChargedPlasmaWaveUpRightLoop, 0x9ccf, 0x0005),
        (SamusProjectileProgram.ChargedPlasmaWaveRight, 0x9d53, 0x9ccf),
        (SamusProjectileProgram.ChargedPlasmaWaveRightLoop, 0x9d83, 0x0005),
        (SamusProjectileProgram.ChargedPlasmaWaveDownRight, 0x9e07, 0x9d83),
        (SamusProjectileProgram.ChargedPlasmaWaveDownRightLoop, 0x9e37, 0x0005),
        (SamusProjectileProgram.MissileUpFacingRight, 0x9ebb, 0x9e37),
        (SamusProjectileProgram.MissileUpRight, 0x9ec7, 0x9ebb),
        (SamusProjectileProgram.MissileRight, 0x9ed3, 0x9ec7),
        (SamusProjectileProgram.MissileDownRight, 0x9edf, 0x9ed3),
        (SamusProjectileProgram.MissileDownFacingRight, 0x9eeb, 0x9edf),
        (SamusProjectileProgram.MissileDownLeft, 0x9ef7, 0x9eeb),
        (SamusProjectileProgram.MissileLeft, 0x9f03, 0x9ef7),
        (SamusProjectileProgram.MissileUpLeft, 0x9f0f, 0x9f03),
        (SamusProjectileProgram.SuperMissileUpFacingRight, 0x9f1b, 0x9f0f),
        (SamusProjectileProgram.SuperMissileUpRight, 0x9f27, 0x9f1b),
        (SamusProjectileProgram.SuperMissileRight, 0x9f33, 0x9f27),
        (SamusProjectileProgram.SuperMissileDownRight, 0x9f3f, 0x9f33),
        (SamusProjectileProgram.SuperMissileDownFacingRight, 0x9f4b, 0x9f3f),
        (SamusProjectileProgram.SuperMissileDownLeft, 0x9f57, 0x9f4b),
        (SamusProjectileProgram.SuperMissileLeft, 0x9f63, 0x9f57),
        (SamusProjectileProgram.SuperMissileUpLeft, 0x9f6f, 0x9f63),
        (SamusProjectileProgram.SuperMissileLink, 0x9f7b, 0x9f6f),
        (SamusProjectileProgram.PowerBomb, 0x9f87, 0x9f7b),
        (SamusProjectileProgram.FastPowerBomb, 0x9fa3, 0x9f87),
        (SamusProjectileProgram.Bomb, 0x9fbf, 0x9fa3),
        (SamusProjectileProgram.FastBomb, 0x9fe3, 0x9fbf),
        (SamusProjectileProgram.BeamExplosion, 0xa007, 0x9fe3),
        (SamusProjectileProgram.MissileExplosion, 0xa039, 0x822f),
        (SamusProjectileProgram.BombExplosion, 0xa06b, 0x822f),
        (SamusProjectileProgram.PlasmaSBA, 0xa095, 0x822f),
        (SamusProjectileProgram.SuperMissileExplosion, 0xa0c1, 0xa095),
        (SamusProjectileProgram.Projectile25UpFacingRight, 0xa0f3, 0x822f),
        (SamusProjectileProgram.ShinesparkEchoUpFacingRight, 0xa119, 0x0000),
        (SamusProjectileProgram.SpazerSBATrailUpFacingRight, 0xa13d, 0xa119),
        (SamusProjectileProgram.SpazerSBATrailUpFacingRightLoop, 0xa14d, 0x0001),
        (SamusProjectileProgram.WaveSBA, 0xa159, 0xa14d),
        (SamusProjectileProgram.Projectile27, 0xa16d, 0xa159),
    ];

    /// <summary>Each step's native address, by program.</summary>
    private static readonly Dictionary<SamusProjectileProgram, ushort[]> StepAddresses = Entries.ToDictionary(
        entry => entry.Program, entry => Layout(entry.Program, entry.Entry));

    /// <summary>The cursor each native address stands for: a program's entry, else the one step at that address.</summary>
    private static readonly Dictionary<ushort, AnimationProgramCursor<SamusProjectileProgram>> Cursors = BuildCursors();

    /// <summary>The list entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(SamusProjectileProgram program) => StepAddresses[program][0];

    /// <summary>The word just before <paramref name="program"/>'s list.</summary>
    internal static ushort Prefix(SamusProjectileProgram program) => Entries.Single(entry => entry.Program == program).Prefix;

    /// <summary>The native instruction pointer <paramref name="cursor"/> stands for.</summary>
    internal static ushort Address(AnimationProgramCursor<SamusProjectileProgram> cursor) => StepAddresses[cursor.Program][cursor.Position];

    /// <summary>The native instruction pointer of a projectile's cursor; a cleared projectile's is zero.</summary>
    internal static ushort Address(AnimationProgramCursor<SamusProjectileProgram>? cursor) => cursor is { } live ? Address(live) : (ushort)0;

    /// <summary>The cursor native instruction pointer <paramref name="address"/> stands for; zero is a cleared projectile.</summary>
    internal static AnimationProgramCursor<SamusProjectileProgram>? CursorAt(ushort address) =>
        address == 0 ? null : Cursors.TryGetValue(address, out var cursor) ? cursor
            : throw new InvalidDataException($"$93:{address:X4} is not a Samus projectile list position.");

    /// <summary>
    /// Moves <paramref name="cursor"/> <paramref name="bytes"/> further through bank $93, as native list arithmetic does;
    /// the result must land on a list position.
    /// </summary>
    internal static AnimationProgramCursor<SamusProjectileProgram> Offset(AnimationProgramCursor<SamusProjectileProgram> cursor, int bytes) =>
        CursorAt(unchecked((ushort)(Address(cursor) + bytes))) ?? throw new InvalidDataException("A list offset cleared the projectile.");

    /// <summary>Each step's address: eight bytes per timed record, two per delete, four per goto; a fall-through rests on the next list.</summary>
    private static ushort[] Layout(SamusProjectileProgram program, ushort entry)
    {
        var steps = SamusProjectileAnimations.Programs.Steps(program);
        var addresses = new ushort[steps.Count];
        int address = entry;
        for (int position = 0; position < steps.Count; position++)
        {
            var step = steps[position];
            addresses[position] = step.Kind == AnimationProgramStepKind.FallInto
                ? Entries.Single(other => other.Program == step.Target).Entry
                : (ushort)address;
            address += step.Kind == AnimationProgramStepKind.Show ? SamusProjectileRomData.Instructions.AnimationRecordByteCount : sizeof(ushort);
        }
        return addresses;
    }

    /// <summary>Indexes every list position; an entry stands for its own program's start.</summary>
    private static Dictionary<ushort, AnimationProgramCursor<SamusProjectileProgram>> BuildCursors()
    {
        var cursors = Entries.ToDictionary(entry => entry.Entry, entry => AnimationProgramCursor<SamusProjectileProgram>.Start(entry.Program));
        foreach ((SamusProjectileProgram program, ushort[] addresses) in StepAddresses)
            for (int position = 0; position < addresses.Length; position++)
                if (!cursors.ContainsKey(addresses[position]))
                    cursors.Add(addresses[position], new(program, position));
        return cursors;
    }
}
