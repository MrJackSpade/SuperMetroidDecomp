namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The routine an escape Etecoon's main AI calls every frame.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum EscapeEtecoonPreInstruction : ushort
{
    /// <summary><c>$B3:807B</c>, the bank-common one-byte RTL installed by opcode $8074.</summary>
    Cleared = 1,

    /// <summary><c>$B3:E65C</c>, add 3.5 pixels to X without terrain collision.</summary>
    EscapeRight,

    /// <summary><c>$B3:E670</c>, wait for the persistent “critters escaped” event.</summary>
    WaitForEscapeEvent,

    /// <summary><c>$B3:E680</c>, walk, reverse at walls, and remain floor-aligned.</summary>
    WalkAndFall,
}
