namespace SuperMetroid.Core.Audio;

/// <summary>Named, proven sound sequences in cartridge SFX library one.</summary>
public static class SoundEffectLibrary1Sounds
{
    /// <summary>Library-one sound $09, the X-ray scope switching on.</summary>
    public static readonly SoundEffectId XrayStart = new(SoundEffectLibrary.Library1, 0x09);

    /// <summary>Library-one sound $0A, the X-ray scope switching off.</summary>
    public static readonly SoundEffectId XrayEnd = new(SoundEffectLibrary.Library1, 0x0a);

    /// <summary>$A0:A7CB / $A7:B0EB: library-one $3D, a shot absorbed without damage.</summary>
    public static readonly SoundEffectId DudShot = new(SoundEffectLibrary.Library1, 0x3d);

    /// <summary>$91:F624, SamusFunc_F468_SpinJump: library-one $31, initial ordinary spin.</summary>
    public static readonly SoundEffectId SpinJump = new(SoundEffectLibrary.Library1, 0x31);

    /// <summary>$90:F4C7 / $90:F576: library-one $41 resumes the charging-beam loop.</summary>
    public static readonly SoundEffectId ResumeChargingBeam = new(SoundEffectLibrary.Library1, 0x41);

    /// <summary>$90:F576, Samus_ShootCheck: library-one $32 stops a spin when movement leaves spin/wall jump.</summary>
    public static readonly SoundEffectId StopSpinJump = new(SoundEffectLibrary.Library1, 0x32);

    /// <summary>$91:F05D stops Screw Attack on landing through library-one $34.</summary>
    public static readonly SoundEffectId StopScrewAttack = new(SoundEffectLibrary.Library1, 0x34);

    /// <summary>$91:F624, SamusFunc_F468_SpinJump: library-one $33, initial Screw Attack spin.</summary>
    public static readonly SoundEffectId ScrewAttack = new(SoundEffectLibrary.Library1, 0x33);

    /// <summary>$91:F624, SamusFunc_F468_SpinJump: library-one $3E, Space Jump pose selection.</summary>
    public static readonly SoundEffectId SpaceJump = new(SoundEffectLibrary.Library1, 0x3e);

    /// <summary>$85:811B queues library-one $2E for the gunship saving wait.</summary>
    public static readonly SoundEffectId Saving = new(SoundEffectLibrary.Library1, 0x2e);
    /// <summary>
    /// Starts the Power Bomb explosion at <c>$88:8AA9-$8AAC</c> through the library-one
    /// max-fifteen queue entry.
    /// </summary>
    public static readonly SoundEffectId PowerBombExplosion =
        new(SoundEffectLibrary.Library1, 0x01);

    /// <summary>Stops/cancels every currently active library-one sound.</summary>
    public static readonly SoundEffectId CancelAll = new(SoundEffectLibrary.Library1, 0x02);

    /// <summary>
    /// Starts the sustained Charge Beam sound when <c>HandleChargingBeamGfxAudio</c> sees
    /// flare counter sixteen at <c>$90:BB45-$BB4B</c>.
    /// </summary>
    public static readonly SoundEffectId ChargeBeamStart =
        new(SoundEffectLibrary.Library1, 0x08);

    /// <summary>Moves a cursor among file, options, game-over, or pause-menu entries.</summary>
    public static readonly SoundEffectId MenuCursor = new(SoundEffectLibrary.Library1, 0x37);

    /// <summary>Library-one $39, HUD weapon-selection feedback (distinct from menu cursor $37).</summary>
    public static readonly SoundEffectId HudWeaponSelect = new(SoundEffectLibrary.Library1, 0x39);

    /// <summary>Accepts the currently highlighted menu or pause-screen entry.</summary>
    public static readonly SoundEffectId MenuConfirm = new(SoundEffectLibrary.Library1, 0x38);
    /// <summary>$81:A1C2 queues library-one $2A when accepting a file, before the helmet turns.</summary>
    public static readonly SoundEffectId FileSelectSwoosh = new(SoundEffectLibrary.Library1, 0x2a);
    /// <summary>$82:9299 completes an eight-tick map scroll.</summary>
    public static readonly SoundEffectId MapScroll = new(SoundEffectLibrary.Library1, 0x36);
    /// <summary>$81:AAAC begins the area-to-room expanding window.</summary>
    public static readonly SoundEffectId MapExpand = new(SoundEffectLibrary.Library1, 0x3b);
    /// <summary>$81:AD7F/$81:AFF6 return from the room map to the area map.</summary>
    public static readonly SoundEffectId MapReturn = new(SoundEffectLibrary.Library1, 0x3c);
}
