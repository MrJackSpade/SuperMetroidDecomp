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
    internal static readonly (DachoraProgram Program, ushort Entry)[] Entries =
    [
        (DachoraProgram.RunningLeft, 0xf345),
        (DachoraProgram.RunningLeftFast, 0xf361),
        (DachoraProgram.RunningLeftVeryFast, 0xf37d),
        (DachoraProgram.IdleLeft, 0xf399),
        (DachoraProgram.BlinkLeft, 0xf3c9),
        (DachoraProgram.EchoLeft, 0xf3f7),
        (DachoraProgram.FallingLeft, 0xf3ff),
        (DachoraProgram.RunningRight, 0xf407),
        (DachoraProgram.RunningRightFast, 0xf423),
        (DachoraProgram.RunningRightVeryFast, 0xf43f),
        (DachoraProgram.IdleRight, 0xf45b),
        (DachoraProgram.BlinkRight, 0xf48b),
        (DachoraProgram.ChargeRight, 0xf4b3),
        (DachoraProgram.EchoRight, 0xf4b9),
        (DachoraProgram.FallingRight, 0xf4c1),
    ];

}
