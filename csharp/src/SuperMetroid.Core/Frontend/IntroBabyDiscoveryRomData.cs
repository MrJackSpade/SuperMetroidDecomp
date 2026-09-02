namespace SuperMetroid.Core.Frontend;

/// <summary>Bank-$91 demo-input routines and lists used by Baby Metroid discovery.</summary>
internal static class IntroBabyDiscoveryRomData
{
    /// <summary>Common inert demo pre-instruction RTS at $91:83BF.</summary>
    public const ushort InertPreInstruction = 0x83bf;

    /// <summary>Second inert demo pre-instruction RTS at $91:8447.</summary>
    public const ushort InertPreInstructionAlternate = 0x8447;

    /// <summary>Stop-and-look demo input list at $91:8623.</summary>
    public const ushort StopAndLookInputList = 0x8623;

    /// <summary>End-of-discovery demo input list at $91:864B.</summary>
    public const ushort EndInputList = 0x864b;

    /// <summary>Running-left demo pre-instruction at $91:864F.</summary>
    public const ushort RunningLeftPreInstruction = 0x864f;

    /// <summary>Stop-and-look demo pre-instruction at $91:866A.</summary>
    public const ushort StopAndLookPreInstruction = 0x866a;

    /// <summary>End-demo-input instruction at $91:8682.</summary>
    public const ushort EndDemoInputInstruction = 0x8682;
}
