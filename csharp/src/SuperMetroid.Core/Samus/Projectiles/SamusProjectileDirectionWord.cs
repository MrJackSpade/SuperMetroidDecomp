namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>A lossless view over one native projectile direction/lifecycle word.</summary>
public readonly record struct SamusProjectileDirectionWord(ushort Raw)
{
    /// <summary>Four-bit direction-table index retained alongside lifecycle state.</summary>
    private const ushort DirectionMask = 0x000f;
    /// <summary>Low-byte state that blocks the ordinary projectile movement path.</summary>
    private const ushort LowByteLifecycleMask = 0x00f0;

    /// <summary>
    /// Bit written by the common enemy and enemy-projectile collision walkers to make the
    /// projectile's own pre-instruction dispose of or otherwise react to the contact.
    /// </summary>
    private const ushort CollisionLifecycleState = 0x0010;

    /// <summary>Low-nibble native direction-table index.</summary>
    public byte DirectionIndex => (byte)(Raw & DirectionMask);
    /// <summary>Named direction represented by the low nibble.</summary>
    public SamusProjectileDirection Direction => (SamusProjectileDirection)DirectionIndex;

    /// <summary>
    /// Native pre-instructions treat any state in bits 4–7 as a lifecycle transition and
    /// delete or skip the ordinary movement path. No individual meaning is assigned here.
    /// </summary>
    public bool HasLowByteLifecycleState => (Raw & LowByteLifecycleMask) != 0;

    /// <summary>
    /// Firing and missile movement accept only indices 0–9 with no lifecycle bits in the
    /// rest of the low byte. High-byte state remains untouched and intentionally unnamed.
    /// </summary>
    public bool IsValidInitialDirection =>
        !HasLowByteLifecycleState && DirectionIndex <= 9;

    /// <summary>
    /// Returns the native direction word after installing collision lifecycle state
    /// <c>$10</c>, without altering its low-nibble direction or unrelated high-byte state.
    /// </summary>
    public ushort WithCollisionLifecycleState() =>
        unchecked((ushort)(Raw | CollisionLifecycleState));
}
