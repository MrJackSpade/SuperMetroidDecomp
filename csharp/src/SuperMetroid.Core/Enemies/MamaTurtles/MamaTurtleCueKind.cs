namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>What a signalling cue of the turtles' animations does: crawl, turn and spin the baby, carry a riding Samus, enter or leave the shell, or play the spinning sound.</summary>
internal enum MamaTurtleCueKind
{
    /// <summary>Moves the baby along its crawl and carries a riding Samus.</summary>
    Crawl,
    /// <summary>Hands the Mama Turtle's AI to entering its shell.</summary>
    EnterShell,
    /// <summary>Leaves the shell for the current direction while Samus rides the baby; otherwise runs on.</summary>
    LeaveShell,
    /// <summary>Restores the baby's crawl action and crawl for the current direction.</summary>
    LeftShell,
    /// <summary>Turns the baby around 48 pixels from its spawn and restarts its crawl for the current direction.</summary>
    LoopOrTurnAround,
    /// <summary>Plays the spinning sound.</summary>
    PlaySpinningSound,
    /// <summary>Lets the baby's spin be stopped.</summary>
    SetSpinningStoppable,
}
