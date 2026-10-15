namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>What a cue of the turtles' programs does: one per bank-$A2 instruction its lists use.</summary>
internal enum MamaTurtleCueKind
{
    /// <summary>Moves the baby along its crawl and carries a riding Samus (<c>Instruction_BabyTurtle_Crawl</c>, $A2:9381).</summary>
    Crawl,
    /// <summary>Hands the Mama Turtle's AI to entering its shell (<c>Instruction_MamaTurtle_EnterShell</c>, $A2:9447).</summary>
    EnterShell,
    /// <summary>Leaves the shell for the current direction while Samus rides the baby; otherwise runs on (<c>Instruction_BabyTurtle_LeaveShell</c>, $A2:9485).</summary>
    LeaveShell,
    /// <summary>Restores the baby's crawl function and crawl for the current direction (<c>Instruction_BabyTurtle_LeftShell</c>, $A2:94A1).</summary>
    LeftShell,
    /// <summary>Turns the baby around 48 pixels from its spawn and restarts its crawl for the current direction (<c>Instruction_BabyTurtle_LoopOrTurnAroundIfMovedTooFar</c>, $A2:9412).</summary>
    LoopOrTurnAround,
    /// <summary>Plays the spinning sound (<c>Instruction_MamaTurtle_PlaySpinningSFX</c>, $A2:94D1).</summary>
    PlaySpinningSound,
    /// <summary>Starts the Mama Turtle's leftward rise and continues at its spin (<c>Instruction_MamaTurtle_RiseToHoverLeftwards</c>, $A2:946B).</summary>
    RiseToHoverLeftwards,
    /// <summary>Starts the Mama Turtle's rightward rise and continues at its spin (<c>Instruction_MamaTurtle_RiseToHoverRightwards</c>, $A2:9451).</summary>
    RiseToHoverRightwards,
    /// <summary>Lets the baby's spin be stopped (<c>Instruction_BabyTurtle_Set_Spinning_Stoppable</c>, $A2:94C7).</summary>
    SetSpinningStoppable,
}
