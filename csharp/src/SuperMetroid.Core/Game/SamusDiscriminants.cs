namespace SuperMetroid.Core.Game;

/// <summary>
/// Verified movement-dispatch indices from pose-definition byte one. This is an ordinary
/// enum, not a flag set: each pose selects exactly one bank-$90 movement handler.
/// </summary>
public enum SamusMovementType : byte
{
    Standing = 0x00,
    Running = 0x01,
    NormalJumping = 0x02,
    SpinJumping = 0x03,
    MorphBallGround = 0x04,
    Crouching = 0x05,
    Falling = 0x06,
    UnusedGlitchBall = 0x07,
    MorphBallFalling = 0x08,
    UnusedGlitchBallAlternate = 0x09,
    Knockback = 0x0a,
    Unused0B = 0x0b,
    Unused0C = 0x0c,
    Unused0D = 0x0d,
    TurningOnGround = 0x0e,
    PostureTransition = 0x0f,
    Moonwalking = 0x10,
    SpringBallGround = 0x11,
    SpringBallInAir = 0x12,
    SpringBallFalling = 0x13,
    WallJumping = 0x14,
    RanIntoWall = 0x15,
    Grappling = 0x16,
    TurningWhileJumping = 0x17,
    TurningWhileFalling = 0x18,
    DamageBoost = 0x19,
    DraygonHeld = 0x1a,
    Special = 0x1b,
}

/// <summary>
/// Verified pose-definition byte-zero values. This is not a flags enum: a pose has one
/// facing discriminator, and zero remains meaningful for front-view/special records.
/// Unnamed cartridge values remain losslessly representable through the underlying byte.
/// </summary>
public enum SamusFacingDirection : byte
{
    ForwardOrSpecial = 0,
    Left = 4,
    Right = 8,
}

/// <summary>State values stored in Samus's remembered liquid-physics word.</summary>
public enum SamusLiquidMedium : ushort
{
    Air = 0,
    Water = 1,
    LavaOrAcid = 2,
}

/// <summary>
/// Verified low-nibble room-FX dispatcher values relevant to Samus physics. Other native
/// FX handlers remain representable as unnamed enum values and are not assigned guesses.
/// </summary>
public enum RoomFxType : ushort
{
    None = 0x0,
    Lava = 0x2,
    Acid = 0x4,
    Water = 0x6,
    Spores = 0x8,
    Rain = 0x0a,
    Fog = 0x0c,
}
