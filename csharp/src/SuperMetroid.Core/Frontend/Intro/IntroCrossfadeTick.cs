namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// What a running crossfade did on one update (#1980), for the actors whose ends it times: the scenes it fades between and
/// the updates it has left before its last.
/// </summary>
/// <param name="From">The scene the crossfade fades out.</param>
/// <param name="To">The scene the crossfade fades in.</param>
/// <param name="UpdatesLeft">
/// The updates the crossfade has left before its last: zero on the update before its last, and below zero on its last,
/// when it ends.
/// </param>
internal readonly record struct IntroCrossfadeTick(IntroCrossfadeScene From, IntroCrossfadeScene To, int UpdatesLeft)
{
    /// <summary>
    /// Whether the crossfade has counted down to its final update, which runs next: the actors it retires go on this
    /// update.
    /// </summary>
    internal bool Finishing => UpdatesLeft == 0;

    /// <summary>Whether the crossfade ended on this update, its last step shown.</summary>
    internal bool Ended => UpdatesLeft < 0;
}
