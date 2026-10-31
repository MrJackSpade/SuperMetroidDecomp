namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>What a cue of the Magdollite's programs does: one per bank-$A8 instruction its lists use.</summary>
internal enum MagdolliteCueKind
{
    /// <summary>Clears the animation-busy flag (<c>Instruction_Magdollite_ResetWaitingFlag</c>, $A8:AE45).</summary>
    ClearAnimationBusy,
    /// <summary>Moves down two pixels (<c>Instruction_Magdollite_MoveDown2Pixels</c>, $A8:AE26).</summary>
    MoveDown2,
    /// <summary>Moves up two pixels (<c>Instruction_Magdollite_MoveUp2Pixels</c>, $A8:AE30).</summary>
    MoveUp2,
    /// <summary>Moves this part and the following one down a pixel (<c>Instruction_Magdollite_MoveBaseAndPillarDown1Pixel</c>, $A8:AE5A).</summary>
    MoveWithPillarDown1,
    /// <summary>Moves this part and the following one up a pixel (<c>Instruction_Magdollite_MoveBaseAndPillarUp1Pixel</c>, $A8:AE50).</summary>
    MoveWithPillarUp1,
    /// <summary>Records the operand's library-two sound while the Magdollite is on screen ($A8:AE12).</summary>
    PlaySoundIfOnScreen,
    /// <summary>Returns the head and body to the origin height (<c>Instruction_Magdollite_RestoreInitialYPositions</c>, $A8:AE88).</summary>
    RestoreOrigins,
    /// <summary>Marks the animation busy (<c>Instruction_Magdollite_SetWaitingFlag</c>, $A8:AE3A).</summary>
    SetAnimationBusy,
    /// <summary>Moves to 8 pixels left of and 4 above the throw origin (<c>Instruction_Magdollite_ShiftLeft8Pixels_Up4Pixels_FacingLeft</c>, $A8:AEE4).</summary>
    ShiftLeftUp4,
    /// <summary>Moves to 8 pixels left of and 4 above the throw origin (the second, identical left callback) (<c>Instruction_Magdollite_ShiftLeft8Pixels_Up4Pixels_Left_dup</c>, $A8:AF18).</summary>
    ShiftLeftUp4Again,
    /// <summary>Moves to 8 pixels right of and 4 above the throw origin (<c>Instruction_Magdollite_ShiftRight8Pixels_Up4Pixels_FaceRight</c>, $A8:AECA).</summary>
    ShiftRightUp4,
    /// <summary>Moves to 8 pixels right of and 8 above the throw origin (<c>Instruction_Magdollite_ShiftRight8Pixels_Up4Pixels_Right_dup</c>, $A8:AEFE).</summary>
    ShiftRightUp8,
    /// <summary>Puts the head and body 24 pixels below the origin and shows the body and overlay (<c>Instruction_Magdollite_MoveDownBy18Pixels_SetSlavesAsVisible</c>, $A8:AE64).</summary>
    SinkAndShowParts,
    /// <summary>Throws a lava projectile (<c>Instruction_Magdollite_SpawnLavaProjectile</c>, $A8:AEBA).</summary>
    SpawnLava,
    /// <summary>Starts the 256-frame attack cooldown (<c>Instruction_Magdollite_SetCooldownTimerTo100</c>, $A8:AF44).</summary>
    StartCooldown,
    /// <summary>Puts the head and body 4 pixels below the origin and hides the body and overlay (<c>Instruction_Magdollite_MoveDown4Pixels_SetSlavesAsInvisible</c>, $A8:AE96).</summary>
    SurfaceAndHideParts,
}
