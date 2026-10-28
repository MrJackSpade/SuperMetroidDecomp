namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>A lossless view over one native projectile direction/lifecycle word.</summary>
[EncodingOwner("projectile direction word")]
public readonly record struct SamusProjectileDirectionWord(ushort Raw)
{
    /// <summary>Four-bit direction-table index retained alongside lifecycle state.</summary>
    private const ushort DirectionMask = 15;
    /// <summary>Low-byte state that blocks the ordinary projectile movement path.</summary>
    internal const ushort LifecycleBits = 15 << 4;

    /// <summary>
    /// Bit written by the common enemy and enemy-projectile collision walkers to make the
    /// projectile's own pre-instruction dispose of or otherwise react to the contact.
    /// </summary>
    private const ushort CollisionLifecycleState = 1 << 4;

    /// <summary>Direction-table index, 0 through 15, held in the word's low four bits; 0 through 9 name a direction.</summary>
    public int DirectionIndex => Raw & DirectionMask;
    /// <summary>The direction the low nibble names, or null for the six indices past the ten directions.</summary>
    public SamusProjectileDirection? Direction => SamusProjectileDirectionCodec.TryFromAimIndex(DirectionIndex);

    /// <summary>
    /// Native pre-instructions treat any state in bits 4–7 as a lifecycle transition and
    /// delete or skip the ordinary movement path. No individual meaning is assigned here.
    /// </summary>
    public bool HasLowByteLifecycleState => (Raw & LifecycleBits) != 0;

    /// <summary>
    /// Whether a collision walker has installed collision lifecycle state <c>$10</c>: the projectile already struck
    /// something, so later collision walkers skip it.
    /// </summary>
    public bool HasCollisionLifecycleState => (Raw & CollisionLifecycleState) != 0;

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

    /// <summary>
    /// Returns the native direction word after clearing collision lifecycle state <c>$10</c>, without altering its
    /// low-nibble direction or unrelated high-byte state.
    /// </summary>
    public ushort WithoutCollisionLifecycleState() =>
        unchecked((ushort)(Raw & ~CollisionLifecycleState));
}
