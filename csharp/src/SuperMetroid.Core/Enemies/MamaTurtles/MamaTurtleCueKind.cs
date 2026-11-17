namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>What a signalling cue of the Mama Turtle's animations does: enter her shell or play the spinning sound (#2342).</summary>
internal enum MamaTurtleCueKind
{
    /// <summary>Hands the Mama Turtle's AI to entering its shell.</summary>
    EnterShell,
    /// <summary>Plays the spinning sound.</summary>
    PlaySpinningSound,
}
