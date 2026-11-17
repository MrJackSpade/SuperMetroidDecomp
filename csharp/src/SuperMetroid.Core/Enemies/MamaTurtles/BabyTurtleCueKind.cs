namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>What a cue of a baby turtle's animations does: crawl, turn, hide and spin, carrying a riding Samus (#2342).</summary>
internal enum BabyTurtleCueKind
{
    /// <summary>Moves the baby along its crawl and carries a riding Samus.</summary>
    Crawl,
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
