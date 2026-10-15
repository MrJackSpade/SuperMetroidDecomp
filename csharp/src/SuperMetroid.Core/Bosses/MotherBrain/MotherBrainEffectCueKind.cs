namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What a cue of Mother Brain's onion rings, bombs, breath, drool, door fragments and subtitle programs does: one per bank-$86 command its lists use.</summary>
internal enum MotherBrainEffectCueKind
{
    /// <summary>Stops the projectile's movement (<c>Instruction_EnemyProjectile_ClearPreInstruction</c>, $86:816A).</summary>
    ClearPreInstruction,
    /// <summary>Deletes the projectile (<c>Instruction_EnemyProjectile_Delete</c>, $86:8154).</summary>
    Delete,
    /// <summary>Lowers the released drool twelve pixels (<c>Instruction_EnemyProj_MotherBrainsDrool_MoveDownCPixels</c>, $86:C8D0).</summary>
    DropDrool,
    /// <summary>Marks Mother Brain's small purple breath inactive (<c>Instruction_EnemyProjectile_MotherBrainPurpleBreath_Inactive</c>, $86:CAEE).</summary>
    EndSmallPurpleBreath,
    /// <summary>Installs a movement routine (<c>Instruction_EnemyProjectile_PreInstructionInY</c>, $86:8161).</summary>
    SetPreInstruction,
    /// <summary>Sets the projectile's packed X and Y collision radii (<c>Instruction_EnemyProjectile_XYRadiusInY</c>, $86:8298).</summary>
    SetRadius,
    /// <summary>Draws the projectile with palette zero (<c>Instruction_EnemyProjectile_UsePalette0_duplicate</c>, $86:C42E).</summary>
    UsePaletteZero,
}
