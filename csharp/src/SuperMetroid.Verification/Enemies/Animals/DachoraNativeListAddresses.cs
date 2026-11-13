using SuperMetroid.TestSupport.MovieReplay;
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
    internal static readonly NativeProgramEntry<DachoraSegment>[] Entries =
    [
        new NativeProgramEntry<DachoraSegment>(DachoraSegment.RunningLeft, 0xf345),
        new NativeProgramEntry<DachoraSegment>(DachoraSegment.RunningLeftFast, 0xf361),
        new NativeProgramEntry<DachoraSegment>(DachoraSegment.RunningLeftVeryFast, 0xf37d),
        new NativeProgramEntry<DachoraSegment>(DachoraSegment.IdleLeft, 0xf399),
        new NativeProgramEntry<DachoraSegment>(DachoraSegment.BlinkLeft, 0xf3c9),
        new NativeProgramEntry<DachoraSegment>(DachoraSegment.EchoLeft, 0xf3f7),
        new NativeProgramEntry<DachoraSegment>(DachoraSegment.FallingLeft, 0xf3ff),
        new NativeProgramEntry<DachoraSegment>(DachoraSegment.RunningRight, 0xf407),
        new NativeProgramEntry<DachoraSegment>(DachoraSegment.RunningRightFast, 0xf423),
        new NativeProgramEntry<DachoraSegment>(DachoraSegment.RunningRightVeryFast, 0xf43f),
        new NativeProgramEntry<DachoraSegment>(DachoraSegment.IdleRight, 0xf45b),
        new NativeProgramEntry<DachoraSegment>(DachoraSegment.BlinkRight, 0xf48b),
        new NativeProgramEntry<DachoraSegment>(DachoraSegment.ChargeRight, 0xf4b3),
        new NativeProgramEntry<DachoraSegment>(DachoraSegment.EchoRight, 0xf4b9),
        new NativeProgramEntry<DachoraSegment>(DachoraSegment.FallingRight, 0xf4c1),
    ];

}
