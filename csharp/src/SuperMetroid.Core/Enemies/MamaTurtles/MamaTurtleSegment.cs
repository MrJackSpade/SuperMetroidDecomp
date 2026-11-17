namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>
/// The Mama Turtle's dormant, shell and spin animations (#641, #2342): one segment per animation a spawn, branch or wake enters, in authored order. Their steps are
/// <see cref="MamaTurtleAnimations"/>.
/// </summary>
internal enum MamaTurtleSegment
{
    /// <summary>
    /// The Mama Turtle spinning in her shell with its sound, facing left, the viewer and right, repeating.
    /// </summary>
    Spinning,
    /// <summary>
    /// The Mama Turtle facing left withdrawing into her shell and rising to hover, held in her shell.
    /// </summary>
    EnterShellLeft,
    /// <summary>The dormant mama's animation.</summary>
    Asleep,
    /// <summary>The Mama Turtle facing left coming out of her shell and standing, before she withdraws again.</summary>
    LeaveShellLeft,
    /// <summary>
    /// The Mama Turtle facing right withdrawing into her shell and rising to hover, held in her shell.
    /// </summary>
    EnterShellRight,
    /// <summary>
    /// The Mama Turtle facing right coming out of her shell and standing, before she withdraws again.
    /// </summary>
    LeaveShellRight,
}
