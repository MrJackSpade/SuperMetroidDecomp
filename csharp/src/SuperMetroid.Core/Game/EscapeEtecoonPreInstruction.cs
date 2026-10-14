namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$B3 pre-instruction words stored by escape Etecoon enemy <c>$F2D3</c>. These are
/// executable cartridge addresses, not host state identifiers: the main AI at
/// <c>$B3:E655</c> performs an indirect call through common enemy variable F every frame.
/// </summary>
public enum EscapeEtecoonPreInstruction : ushort
{
    /// <summary><c>$B3:807B</c>, the bank-common one-byte RTL installed by opcode $8074.</summary>
    Cleared = 0x807b,

    /// <summary><c>$B3:E65C</c>, add 3.5 pixels to X without terrain collision.</summary>
    EscapeRight = 0xe65c,

    /// <summary><c>$B3:E670</c>, wait for the persistent “critters escaped” event.</summary>
    WaitForEscapeEvent = 0xe670,

    /// <summary><c>$B3:E680</c>, walk, reverse at walls, and remain floor-aligned.</summary>
    WalkAndFall = 0xe680,
}
