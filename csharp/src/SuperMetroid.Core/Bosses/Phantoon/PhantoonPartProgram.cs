namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Phantoon's typed part programs (#641), in the authored order of their bank-$A7 lists.</summary>
internal enum PhantoonPartProgram
{
    /// <summary>The body with no hitbox ($A7:CC41).</summary>
    InvulnerableBody,
    /// <summary>The body with its full hitbox ($A7:CC47).</summary>
    FullHitboxBody,
    /// <summary>The body with only its eye hitbox ($A7:CC4D).</summary>
    EyeHitboxBody,
    /// <summary>The eye opening, then the materialization sound and eye tracking ($A7:CC53).</summary>
    EyeOpen,
    /// <summary>The closed eye ($A7:CC7B).</summary>
    EyeClosed,
    /// <summary>The eye closing, then picking the next pattern ($A7:CC81).</summary>
    EyeCloseAndPickNewPattern,
    /// <summary>The eye closing ($A7:CC91).</summary>
    EyeClose,
    /// <summary>The centered eyeball, then the materialization sound ($A7:CC9D).</summary>
    EyeballCentered,
    /// <summary>The eyeball looking up ($A7:CCA7).</summary>
    EyeLookingUp,
    /// <summary>The eyeball looking up and right ($A7:CCAD).</summary>
    EyeLookingUpRight,
    /// <summary>The eyeball looking right ($A7:CCB3).</summary>
    EyeLookingRight,
    /// <summary>The eyeball looking down and right ($A7:CCB9).</summary>
    EyeLookingDownRight,
    /// <summary>The eyeball looking down ($A7:CCBF).</summary>
    EyeLookingDown,
    /// <summary>The eyeball looking down and left ($A7:CCC5).</summary>
    EyeLookingDownLeft,
    /// <summary>The eyeball looking left ($A7:CCCB).</summary>
    EyeLookingLeft,
    /// <summary>The eyeball looking up and left ($A7:CCD1).</summary>
    EyeLookingUpLeft,
    /// <summary>The tentacles' looping sway ($A7:CCD7).</summary>
    Tentacles,
    /// <summary>The mouth opening to spawn a casual flame, running on into the initial mouth ($A7:CCEB).</summary>
    MouthSpawnFlame,
    /// <summary>The closed mouth ($A7:CCF7).</summary>
    MouthInitial,
}