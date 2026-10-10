namespace SuperMetroid.Core.Frontend;

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

/// <summary>Bank-$91 demo-input routines and lists used by Baby Metroid discovery.</summary>
internal static class IntroBabyDiscoveryRomData
{
    /// <summary>Stop-and-look demo input list at $91:8623.</summary>
    public const ushort StopAndLookInputList = 0x8623;

    /// <summary>End-of-discovery demo input list at $91:864B.</summary>
    public const ushort EndInputList = 0x864b;

    /// <summary>End-demo-input instruction at $91:8682.</summary>
    public const ushort EndDemoInputInstruction = 0x8682;
}
