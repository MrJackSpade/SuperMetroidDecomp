using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Animals;

namespace SuperMetroid.Verification.Enemies.Animals;

/// <summary>
/// The native bank-$A7 lists the Dachora ROM oracle (#641) walks: each program's entry point.
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class DachoraNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa7;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<DachoraProgram>[] Entries =
    [
        new NativeProgramEntry<DachoraProgram>(DachoraProgram.RunningLeft, 0xf345),
        new NativeProgramEntry<DachoraProgram>(DachoraProgram.RunningLeftFast, 0xf361),
        new NativeProgramEntry<DachoraProgram>(DachoraProgram.RunningLeftVeryFast, 0xf37d),
        new NativeProgramEntry<DachoraProgram>(DachoraProgram.IdleLeft, 0xf399),
        new NativeProgramEntry<DachoraProgram>(DachoraProgram.BlinkLeft, 0xf3c9),
        new NativeProgramEntry<DachoraProgram>(DachoraProgram.EchoLeft, 0xf3f7),
        new NativeProgramEntry<DachoraProgram>(DachoraProgram.FallingLeft, 0xf3ff),
        new NativeProgramEntry<DachoraProgram>(DachoraProgram.RunningRight, 0xf407),
        new NativeProgramEntry<DachoraProgram>(DachoraProgram.RunningRightFast, 0xf423),
        new NativeProgramEntry<DachoraProgram>(DachoraProgram.RunningRightVeryFast, 0xf43f),
        new NativeProgramEntry<DachoraProgram>(DachoraProgram.IdleRight, 0xf45b),
        new NativeProgramEntry<DachoraProgram>(DachoraProgram.BlinkRight, 0xf48b),
        new NativeProgramEntry<DachoraProgram>(DachoraProgram.ChargeRight, 0xf4b3),
        new NativeProgramEntry<DachoraProgram>(DachoraProgram.EchoRight, 0xf4b9),
        new NativeProgramEntry<DachoraProgram>(DachoraProgram.FallingRight, 0xf4c1),
    ];

}
