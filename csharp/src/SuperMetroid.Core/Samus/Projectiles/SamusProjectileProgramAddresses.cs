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
    private static readonly SamusProjectileProgramAddressDefinition[] Entries =
    [
        new(SamusProjectileProgram.PowerUpFacingRight, 0x86db, 0xa16d),
        new(SamusProjectileProgram.PowerUpRight, 0x86e7, 0x86db),
        new(SamusProjectileProgram.PowerRight, 0x86f3, 0x86e7),
        new(SamusProjectileProgram.PowerDownRight, 0x86ff, 0x86f3),
        new(SamusProjectileProgram.PowerDownFacingRight, 0x870b, 0x86ff),
        new(SamusProjectileProgram.PowerDownLeft, 0x8717, 0x870b),
        new(SamusProjectileProgram.PowerLeft, 0x8723, 0x8717),
        new(SamusProjectileProgram.PowerUpLeft, 0x872f, 0x8723),
        new(SamusProjectileProgram.WaveUpFacingRight, 0x873b, 0x872f),
        new(SamusProjectileProgram.WaveDownFacingRight, 0x8743, 0x0000),
        new(SamusProjectileProgram.WaveUpRight, 0x87c7, 0x8743),
        new(SamusProjectileProgram.WaveRight, 0x884b, 0x87c7),
        new(SamusProjectileProgram.WaveDownRight, 0x88cf, 0x884b),
        new(SamusProjectileProgram.IceUpFacingRight, 0x8953, 0x88cf),
        new(SamusProjectileProgram.SpazerUpFacingRight, 0x8977, 0x8953),
        new(SamusProjectileProgram.SpazerUpFacingRightLoop, 0x8987, 0x0001),
        new(SamusProjectileProgram.SpazerUpRight, 0x8993, 0x8987),
        new(SamusProjectileProgram.SpazerUpRightLoop, 0x89a3, 0x0001),
        new(SamusProjectileProgram.SpazerRight, 0x89af, 0x89a3),
        new(SamusProjectileProgram.SpazerRightLoop, 0x89bf, 0x0001),
        new(SamusProjectileProgram.SpazerDownRight, 0x89cb, 0x89bf),
        new(SamusProjectileProgram.SpazerDownRightLoop, 0x89db, 0x0001),
        new(SamusProjectileProgram.SpazerDownFacingRight, 0x89e7, 0x89db),
        new(SamusProjectileProgram.SpazerDownFacingRightLoop, 0x89f7, 0x0001),
        new(SamusProjectileProgram.SpazerDownLeft, 0x8a03, 0x89f7),
        new(SamusProjectileProgram.SpazerDownLeftLoop, 0x8a13, 0x0001),
        new(SamusProjectileProgram.SpazerLeft, 0x8a1f, 0x8a13),
        new(SamusProjectileProgram.SpazerLeftLoop, 0x8a2f, 0x0001),
        new(SamusProjectileProgram.SpazerUpLeft, 0x8a3b, 0x8a2f),
        new(SamusProjectileProgram.SpazerUpLeftLoop, 0x8a4b, 0x0001),
        new(SamusProjectileProgram.SpazerWaveUpFacingRight, 0x8a57, 0x8a4b),
        new(SamusProjectileProgram.SpazerWaveUpRight, 0x8aab, 0x8a57),
        new(SamusProjectileProgram.SpazerWaveRight, 0x8aff, 0x8aab),
        new(SamusProjectileProgram.SpazerWaveDownRight, 0x8b53, 0x8aff),
        new(SamusProjectileProgram.SpazerWaveDownFacingRight, 0x8ba7, 0x8b53),
        new(SamusProjectileProgram.SpazerWaveDownLeft, 0x8bfb, 0x8ba7),
        new(SamusProjectileProgram.SpazerWaveLeft, 0x8c4f, 0x8bfb),
        new(SamusProjectileProgram.SpazerWaveUpLeft, 0x8ca3, 0x8c4f),
        new(SamusProjectileProgram.PlasmaUpFacingRight, 0x8cf7, 0x8ca3),
        new(SamusProjectileProgram.PlasmaUpFacingRightLoop, 0x8cff, 0x0000),
        new(SamusProjectileProgram.PlasmaUpRight, 0x8d0b, 0x8cff),
        new(SamusProjectileProgram.PlasmaUpRightLoop, 0x8d13, 0x0000),
        new(SamusProjectileProgram.PlasmaRight, 0x8d1f, 0x8d13),
        new(SamusProjectileProgram.PlasmaRightLoop, 0x8d27, 0x0000),
        new(SamusProjectileProgram.PlasmaDownRight, 0x8d33, 0x8d27),
        new(SamusProjectileProgram.PlasmaDownRightLoop, 0x8d3b, 0x0000),
        new(SamusProjectileProgram.PlasmaIceWaveUpFacingRight, 0x8d47, 0x8d3b),
        new(SamusProjectileProgram.PlasmaWaveUpFacingRight, 0x8d4f, 0x0000),
        new(SamusProjectileProgram.PlasmaIceWaveUpRight, 0x8d93, 0x8d4f),
        new(SamusProjectileProgram.PlasmaWaveUpRight, 0x8d9b, 0x0000),
        new(SamusProjectileProgram.PlasmaWaveRight, 0x8ddf, 0x8d9b),
        new(SamusProjectileProgram.PlasmaWaveRightLoop, 0x8de7, 0x0000),
        new(SamusProjectileProgram.PlasmaIceWaveDownRight, 0x8e2b, 0x8de7),
        new(SamusProjectileProgram.PlasmaWaveDownRight, 0x8e33, 0x0000),
        new(SamusProjectileProgram.ChargedPowerUpFacingRight, 0x8e77, 0x8e33),
        new(SamusProjectileProgram.ChargedPowerUpRight, 0x8e8b, 0x8e77),
        new(SamusProjectileProgram.ChargedPowerRight, 0x8e9f, 0x8e8b),
        new(SamusProjectileProgram.ChargedPowerDownRight, 0x8eb3, 0x8e9f),
        new(SamusProjectileProgram.ChargedPowerDownFacingRight, 0x8ec7, 0x8eb3),
        new(SamusProjectileProgram.ChargedPowerDownLeft, 0x8edb, 0x8ec7),
        new(SamusProjectileProgram.ChargedPowerLeft, 0x8eef, 0x8edb),
        new(SamusProjectileProgram.ChargedPowerUpLeft, 0x8f03, 0x8eef),
        new(SamusProjectileProgram.ChargedWaveUpFacingRight, 0x8f17, 0x8f03),
        new(SamusProjectileProgram.ChargedWaveDownFacingRight, 0x8f1f, 0x0000),
        new(SamusProjectileProgram.ChargedWaveUpRight, 0x8fa3, 0x8f1f),
        new(SamusProjectileProgram.ChargedWaveRight, 0x9027, 0x8fa3),
        new(SamusProjectileProgram.ChargedWaveDownRight, 0x90ab, 0x9027),
        new(SamusProjectileProgram.ChargedIceUpFacingRight, 0x912f, 0x90ab),
        new(SamusProjectileProgram.ChargedIceWaveUpFacingRight, 0x9153, 0x912f),
        new(SamusProjectileProgram.ChargedIceWaveDownFacingRight, 0x915b, 0x0000),
        new(SamusProjectileProgram.ChargedIceWaveUpRight, 0x91df, 0x915b),
        new(SamusProjectileProgram.ChargedIceWaveRight, 0x9263, 0x91df),
        new(SamusProjectileProgram.ChargedIceWaveDownRight, 0x92e7, 0x9263),
        new(SamusProjectileProgram.ChargedSpazerUpFacingRight, 0x936b, 0x92e7),
        new(SamusProjectileProgram.ChargedSpazerUpFacingRightLoop, 0x93ab, 0x0007),
        new(SamusProjectileProgram.ChargedSpazerUpRight, 0x93bf, 0x93ab),
        new(SamusProjectileProgram.ChargedSpazerUpRightLoop, 0x93ff, 0x0007),
        new(SamusProjectileProgram.ChargedSpazerRight, 0x9413, 0x93ff),
        new(SamusProjectileProgram.ChargedSpazerRightLoop, 0x9453, 0x0007),
        new(SamusProjectileProgram.ChargedSpazerDownRight, 0x9467, 0x9453),
        new(SamusProjectileProgram.ChargedSpazerDownRightLoop, 0x94a7, 0x0007),
        new(SamusProjectileProgram.ChargedSpazerWaveUpFacingRight, 0x94bb, 0x94a7),
        new(SamusProjectileProgram.ChargedSpazerWaveUpFacingRightLoop, 0x94db, 0x0003),
        new(SamusProjectileProgram.ChargedSpazerWaveUpRight, 0x957f, 0x94db),
        new(SamusProjectileProgram.ChargedSpazerWaveUpRightLoop, 0x959f, 0x0003),
        new(SamusProjectileProgram.ChargedSpazerWaveRight, 0x9643, 0x959f),
        new(SamusProjectileProgram.ChargedSpazerWaveRightLoop, 0x9663, 0x0003),
        new(SamusProjectileProgram.ChargedSpazerWaveDownRight, 0x9707, 0x9663),
        new(SamusProjectileProgram.ChargedSpazerWaveDownRightLoop, 0x9727, 0x0003),
        new(SamusProjectileProgram.ChargedSpazerWaveDownFacingRight, 0x97cb, 0x9727),
        new(SamusProjectileProgram.ChargedSpazerWaveDownFacingRightLoop, 0x97eb, 0x0003),
        new(SamusProjectileProgram.ChargedSpazerWaveDownLeft, 0x988f, 0x97eb),
        new(SamusProjectileProgram.ChargedSpazerWaveDownLeftLoop, 0x98af, 0x0003),
        new(SamusProjectileProgram.ChargedSpazerWaveLeft, 0x9953, 0x98af),
        new(SamusProjectileProgram.ChargedSpazerWaveLeftLoop, 0x9973, 0x0003),
        new(SamusProjectileProgram.ChargedSpazerWaveUpLeft, 0x9a17, 0x9973),
        new(SamusProjectileProgram.ChargedSpazerWaveUpLeftLoop, 0x9a37, 0x0003),
        new(SamusProjectileProgram.ChargedPlasmaUpFacingRight, 0x9adb, 0x9a37),
        new(SamusProjectileProgram.ChargedPlasmaUpFacingRightLoop, 0x9b0b, 0x0005),
        new(SamusProjectileProgram.ChargedPlasmaUpRight, 0x9b1f, 0x9b0b),
        new(SamusProjectileProgram.ChargedPlasmaUpRightLoop, 0x9b4f, 0x0005),
        new(SamusProjectileProgram.ChargedPlasmaRight, 0x9b63, 0x9b4f),
        new(SamusProjectileProgram.ChargedPlasmaRightLoop, 0x9b93, 0x0005),
        new(SamusProjectileProgram.ChargedPlasmaDownRight, 0x9ba7, 0x9b93),
        new(SamusProjectileProgram.ChargedPlasmaDownRightLoop, 0x9bd7, 0x0005),
        new(SamusProjectileProgram.ChargedPlasmaWaveUpFacingRight, 0x9beb, 0x9bd7),
        new(SamusProjectileProgram.ChargedPlasmaWaveUpFacingRightLoop, 0x9c1b, 0x0005),
        new(SamusProjectileProgram.ChargedPlasmaWaveUpRight, 0x9c9f, 0x9c1b),
        new(SamusProjectileProgram.ChargedPlasmaWaveUpRightLoop, 0x9ccf, 0x0005),
        new(SamusProjectileProgram.ChargedPlasmaWaveRight, 0x9d53, 0x9ccf),
        new(SamusProjectileProgram.ChargedPlasmaWaveRightLoop, 0x9d83, 0x0005),
        new(SamusProjectileProgram.ChargedPlasmaWaveDownRight, 0x9e07, 0x9d83),
        new(SamusProjectileProgram.ChargedPlasmaWaveDownRightLoop, 0x9e37, 0x0005),
        new(SamusProjectileProgram.MissileUpFacingRight, 0x9ebb, 0x9e37),
        new(SamusProjectileProgram.MissileUpRight, 0x9ec7, 0x9ebb),
        new(SamusProjectileProgram.MissileRight, 0x9ed3, 0x9ec7),
        new(SamusProjectileProgram.MissileDownRight, 0x9edf, 0x9ed3),
        new(SamusProjectileProgram.MissileDownFacingRight, 0x9eeb, 0x9edf),
        new(SamusProjectileProgram.MissileDownLeft, 0x9ef7, 0x9eeb),
        new(SamusProjectileProgram.MissileLeft, 0x9f03, 0x9ef7),
        new(SamusProjectileProgram.MissileUpLeft, 0x9f0f, 0x9f03),
        new(SamusProjectileProgram.SuperMissileUpFacingRight, 0x9f1b, 0x9f0f),
        new(SamusProjectileProgram.SuperMissileUpRight, 0x9f27, 0x9f1b),
        new(SamusProjectileProgram.SuperMissileRight, 0x9f33, 0x9f27),
        new(SamusProjectileProgram.SuperMissileDownRight, 0x9f3f, 0x9f33),
        new(SamusProjectileProgram.SuperMissileDownFacingRight, 0x9f4b, 0x9f3f),
        new(SamusProjectileProgram.SuperMissileDownLeft, 0x9f57, 0x9f4b),
        new(SamusProjectileProgram.SuperMissileLeft, 0x9f63, 0x9f57),
        new(SamusProjectileProgram.SuperMissileUpLeft, 0x9f6f, 0x9f63),
        new(SamusProjectileProgram.SuperMissileLink, 0x9f7b, 0x9f6f),
        new(SamusProjectileProgram.PowerBomb, 0x9f87, 0x9f7b),
        new(SamusProjectileProgram.FastPowerBomb, 0x9fa3, 0x9f87),
        new(SamusProjectileProgram.Bomb, 0x9fbf, 0x9fa3),
        new(SamusProjectileProgram.FastBomb, 0x9fe3, 0x9fbf),
        new(SamusProjectileProgram.BeamExplosion, 0xa007, 0x9fe3),
        new(SamusProjectileProgram.MissileExplosion, 0xa039, 0x822f),
        new(SamusProjectileProgram.BombExplosion, 0xa06b, 0x822f),
        new(SamusProjectileProgram.PlasmaSBA, 0xa095, 0x822f),
        new(SamusProjectileProgram.SuperMissileExplosion, 0xa0c1, 0xa095),
        new(SamusProjectileProgram.Projectile25UpFacingRight, 0xa0f3, 0x822f),
        new(SamusProjectileProgram.ShinesparkEchoUpFacingRight, 0xa119, 0x0000),
        new(SamusProjectileProgram.SpazerSBATrailUpFacingRight, 0xa13d, 0xa119),
        new(SamusProjectileProgram.SpazerSBATrailUpFacingRightLoop, 0xa14d, 0x0001),
        new(SamusProjectileProgram.WaveSBA, 0xa159, 0xa14d),
        new(SamusProjectileProgram.Projectile27, 0xa16d, 0xa159),
    ];

    /// <summary>Each step's native address, by program.</summary>
    internal static readonly Dictionary<SamusProjectileProgram, ushort[]> StepAddresses = Entries.ToDictionary(
        entry => entry.Program, entry => Layout(entry.Program, entry.Entry));

    /// <summary>The cursor each native address stands for: a program's entry, else the one step at that address.</summary>
    private static readonly Dictionary<ushort, AnimationProgramCursor<SamusProjectileProgram>> Cursors = BuildCursors();

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
            addresses[position] = step is AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue>.FallIntoStep fallInto
                ? Entries.Single(other => other.Program == fallInto.Target).Entry
                : (ushort)address;
            address += step is AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue>.ShowStep ? SamusProjectileRomData.Instructions.AnimationRecordByteCount : sizeof(ushort);
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
