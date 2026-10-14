namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>The four bank-$91 pre-instructions the Baby-discovery demo object references.</summary>
internal enum IntroBabyDiscoveryPreInstruction : ushort
{
    /// <summary>Common inert demo pre-instruction RTS at $91:83BF.</summary>
    Inert = 0x83bf,

    /// <summary>Second inert demo pre-instruction RTS at $91:8447.</summary>
    InertAlternate = 0x8447,

    /// <summary>Running-left demo pre-instruction at $91:864F.</summary>
    RunningLeft = 0x864f,

    /// <summary>Stop-and-look demo pre-instruction at $91:866A.</summary>
    StopAndLook = 0x866a,
}
