namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>What a cue of the turtles' animations does: crawl, turn and spin the baby, carry a riding Samus, enter or leave the shell, start the Mama Turtle's rise, or play the spinning sound.</summary>
internal enum MamaTurtleCueKind
{
    /// <summary>Moves the baby along its crawl and carries a riding Samus.</summary>
    Crawl,
    /// <summary>Hands the Mama Turtle's AI to entering its shell.</summary>
    EnterShell,
    /// <summary>Leaves the shell for the current direction while Samus rides the baby; otherwise runs on.</summary>
    LeaveShell,
    /// <summary>Restores the baby's crawl function and crawl for the current direction.</summary>
    LeftShell,
    /// <summary>Turns the baby around 48 pixels from its spawn and restarts its crawl for the current direction.</summary>
    LoopOrTurnAround,
    /// <summary>Plays the spinning sound.</summary>
    PlaySpinningSound,
    /// <summary>Starts the Mama Turtle's leftward rise and continues at its spin.</summary>
    RiseToHoverLeftwards,
    /// <summary>Starts the Mama Turtle's rightward rise and continues at its spin.</summary>
    RiseToHoverRightwards,
    /// <summary>Lets the baby's spin be stopped.</summary>
    SetSpinningStoppable,
}
