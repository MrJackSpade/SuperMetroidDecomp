namespace SuperMetroid.Core.Audio;

/// <summary>
/// Lossless identity of one cartridge sound effect, including the library in which its
/// byte-sized sequence number is meaningful.
/// </summary>
/// <remarks>
/// Sound numbers are not global: for example, <c>$02</c> in library one is unrelated to
/// <c>$02</c> in library two. Keeping both fields in one value prevents APIs from swapping
/// their order or accidentally comparing IDs that belong to different SPC queues. Unknown
/// cartridge sequence numbers remain representable because the numeric component is not an
/// enum.
/// </remarks>
public readonly record struct SoundEffectId
{
    /// <summary>Creates a library-qualified sound effect identity.</summary>
    public SoundEffectId(SoundEffectLibrary library, byte value)
    {
        // Enum casts can forge undefined values, so validate at construction even though
        // ordinary callers use one of the three named enum members.
        _ = SoundEffectLibraries.ToQueueIndex(library);
        Library = library;
        Value = value;
    }

    /// <summary>The exclusive cartridge SFX library containing this sequence.</summary>
    public SoundEffectLibrary Library { get; }

    /// <summary>The library-relative byte written to the corresponding SPC input port.</summary>
    public byte Value { get; }

    /// <summary>
    /// Preserves a cartridge value after proving that it is a valid byte-sized SPC sequence
    /// number. Accepting the decoded word keeps ROM instruction interpreters free of
    /// scattered casts while still accepting byte-valued dynamic data and numeric literals.
    /// </summary>
    public static SoundEffectId FromCartridge(SoundEffectLibrary library, ushort value)
    {
        if (value > byte.MaxValue)
        {
            throw new InvalidDataException(
                $"Sound effect ${value:X4} for {library} does not fit the cartridge's byte-sized SPC request port.");
        }

        return new SoundEffectId(library, unchecked((byte)value));
    }

    /// <inheritdoc />
    public override string ToString() => $"{Library}:${Value:X2}";
}

/// <summary>Named, proven sound sequences in cartridge SFX library one.</summary>
public static class SoundEffectLibrary1Sounds
{
    /// <summary>$91:F624, SamusFunc_F468_SpinJump: library-one $31, initial ordinary spin.</summary>
    public static readonly SoundEffectId SpinJump = new(SoundEffectLibrary.Library1, 0x31); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>$90:F576, Samus_ShootCheck: library-one $32 stops a spin when movement leaves spin/wall jump.</summary>
    public static readonly SoundEffectId StopSpinJump = new(SoundEffectLibrary.Library1, 0x32); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>$91:F624, SamusFunc_F468_SpinJump: library-one $33, initial Screw Attack spin.</summary>
    public static readonly SoundEffectId ScrewAttack = new(SoundEffectLibrary.Library1, 0x33); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>$91:F624, SamusFunc_F468_SpinJump: library-one $3E, Space Jump pose selection.</summary>
    public static readonly SoundEffectId SpaceJump = new(SoundEffectLibrary.Library1, 0x3e); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>$85:811B queues library-one $2E for the gunship saving wait.</summary>
    public static readonly SoundEffectId Saving = new(SoundEffectLibrary.Library1, 0x2e); // magic-number-audit: allow(AudioId) - named cartridge SFX identity
    /// <summary>
    /// Starts the Power Bomb explosion at <c>$88:8AA9-$8AAC</c> through the library-one
    /// max-fifteen queue entry.
    /// </summary>
    public static readonly SoundEffectId PowerBombExplosion =
        new(SoundEffectLibrary.Library1, 0x01); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>Stops/cancels every currently active library-one sound.</summary>
    public static readonly SoundEffectId CancelAll = new(SoundEffectLibrary.Library1, 0x02);

    /// <summary>Uncharged Power Beam projectile launch.</summary>
    public static readonly SoundEffectId PowerBeam = new(SoundEffectLibrary.Library1, 0x0b); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>
    /// Starts the sustained Charge Beam sound when <c>HandleChargingBeamGfxAudio</c> sees
    /// flare counter sixteen at <c>$90:BB45-$BB4B</c>.
    /// </summary>
    public static readonly SoundEffectId ChargeBeamStart =
        new(SoundEffectLibrary.Library1, 0x08); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>Moves a cursor among file, options, game-over, or pause-menu entries.</summary>
    public static readonly SoundEffectId MenuCursor = new(SoundEffectLibrary.Library1, 0x37);

    /// <summary>Library-one $39, HUD weapon-selection feedback (distinct from menu cursor $37).</summary>
    public static readonly SoundEffectId HudWeaponSelect = new(SoundEffectLibrary.Library1, 0x39); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>Accepts the currently highlighted menu or pause-screen entry.</summary>
    public static readonly SoundEffectId MenuConfirm = new(SoundEffectLibrary.Library1, 0x38);
    /// <summary>$81:A1C2 queues library-one $2A when accepting a file, before the helmet turns.</summary>
    public static readonly SoundEffectId FileSelectSwoosh = new(SoundEffectLibrary.Library1, 0x2a); // magic-number-audit: allow(AudioId) - named cartridge SFX identity
    /// <summary>$82:9299 completes an eight-tick map scroll.</summary>
    public static readonly SoundEffectId MapScroll = new(SoundEffectLibrary.Library1, 0x36); // magic-number-audit: allow(AudioId) - named cartridge SFX identity
    /// <summary>$81:AAAC begins the area-to-room expanding window.</summary>
    public static readonly SoundEffectId MapExpand = new(SoundEffectLibrary.Library1, 0x3b); // magic-number-audit: allow(AudioId) - named cartridge SFX identity
    /// <summary>$81:AD7F/$81:AFF6 return from the room map to the area map.</summary>
    public static readonly SoundEffectId MapReturn = new(SoundEffectLibrary.Library1, 0x3c); // magic-number-audit: allow(AudioId) - named cartridge SFX identity
}

/// <summary>Named, proven sound sequences in cartridge SFX library two.</summary>
public static class SoundEffectLibrary2Sounds
{
    /// <summary>$93:80CF KillProjectileInner: shared regular/Super Missile impact, library two $07, Max6.</summary>
    public static readonly SoundEffectId MissileImpact = new(SoundEffectLibrary.Library2, 0x07); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>Library-two $56: Varia/Gravity transformation, queued with Max6 at $91:D593/$D66B by VariaSuitPickup/GravitySuitPickup.</summary>
    public static readonly SoundEffectId SuitTransformation = new(SoundEffectLibrary.Library2, 0x56); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>Stops/cancels every currently active library-two sound.</summary>
    public static readonly SoundEffectId CancelAll = new(SoundEffectLibrary.Library2, 0x71);

    /// <summary>Opens the standard horizontal or vertical room door actor.</summary>
    public static readonly SoundEffectId DoorOpening = new(SoundEffectLibrary.Library2, 0x57);

    /// <summary>Plays when Samus acquires a permanent equipment item.</summary>
    public static readonly SoundEffectId PermanentItemAcquisition = new(SoundEffectLibrary.Library2, 0x0a);

    /// <summary>
    /// Normal Morph Ball bomb explosion queued with Max6 by <c>Bomb_Func2</c> at
    /// <c>$90:C128</c> when the bomb fuse reaches zero.
    /// </summary>
    public static readonly SoundEffectId BombExplosion = new(SoundEffectLibrary.Library2, 0x08); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>
    /// Repeating lavaquake/Tourian-reveal rumble queued by
    /// <c>Handle_Earthquake_SoundEffect</c> at <c>$88:B21D</c>.
    /// </summary>
    public static readonly SoundEffectId Earthquake = new(SoundEffectLibrary.Library2, 0x46); // magic-number-audit: allow(AudioId) - named cartridge SFX identity
}

/// <summary>Named, proven sound sequences in cartridge SFX library three.</summary>
public static class SoundEffectLibrary3Sounds
{
    /// <summary>$A0:A8AE, normal enemy shot: library-three $0A when an enemy becomes frozen.</summary>
    public static readonly SoundEffectId EnemyFreeze = new(SoundEffectLibrary.Library3, 0x0a); // magic-number-audit: allow(AudioId) - named cartridge SFX identity
    /// <summary>$90:85A6 queues library-three $03 when Speed Booster reaches stage four.</summary>
    public static readonly SoundEffectId SpeedBoosterEcho = new(SoundEffectLibrary.Library3, 0x03); // magic-number-audit: allow(AudioId) - named cartridge SFX identity
    /// <summary>$A2:AB58 queues library-three $14 as the ship's entrance pad opens.</summary>
    public static readonly SoundEffectId GunshipEntrancePad = new(SoundEffectLibrary.Library3, 0x14); // magic-number-audit: allow(AudioId) - named cartridge SFX identity
    /// <summary>$A2:AB9D queues library-three $15 as the ship's entrance pad closes after exit.</summary>
    public static readonly SoundEffectId GunshipEntrancePadClosing = new(SoundEffectLibrary.Library3, 0x15); // magic-number-audit: allow(AudioId) - named cartridge SFX identity
    /// <summary>$82:A92B queues library-three $2A when the pause/map palette animation loops.</summary>
    public static readonly SoundEffectId MapPaletteLoop = new(SoundEffectLibrary.Library3, 0x2a); // magic-number-audit: allow(AudioId) - named cartridge SFX identity
    /// <summary>Stops/cancels every currently active library-three sound.</summary>
    public static readonly SoundEffectId CancelAll = new(SoundEffectLibrary.Library3, 0x01);

    /// <summary>Samus taking periodic damage from room heat, lava, or acid.</summary>
    public static readonly SoundEffectId EnvironmentalDamage = new(SoundEffectLibrary.Library3, 0x2d); // magic-number-audit: allow(AudioId) - named cartridge SFX identity
}
