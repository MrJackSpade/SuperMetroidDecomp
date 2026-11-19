namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// What an owner of cinematic actors of several families that publish one cue type moves and runs (#2197): where an actor
/// stands, whether it is alive, and its next update, whatever its family shows.
/// </summary>
/// <typeparam name="TCue">The cues the actor's animations publish.</typeparam>
internal interface ICinematicActor<TCue>
    where TCue : notnull
{
    /// <summary>Whether the actor is alive: cleared when its animation ends or its owner deletes it.</summary>
    bool IsActive { get; }

    /// <summary>Where the actor stands on the screen, to a fraction of a pixel, from the screen's top-left corner.</summary>
    FixedVector<ScreenPixels> Position { get; set; }

    /// <summary>The whole pixels of <see cref="Position"/>; assigning replaces them and keeps both fractions.</summary>
    Offset16<ScreenPixels> ScreenPlace { get; set; }

    /// <summary>Retires the actor and clears what it shows.</summary>
    void Delete();

    /// <summary>Runs the actor's animation for one update, handing each cue it publishes to <paramref name="ownerCue"/>.</summary>
    void Step(Action<TCue>? ownerCue = null);
}
