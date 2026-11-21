namespace SuperMetroid.Core.Audio;

/// <summary>One music request gameplay makes for the music queue through <see cref="GameplayAudioRequests"/>.</summary>
public abstract partial record GameplayMusicRequest
{
    /// <summary>Closes the hierarchy to the requests declared here.</summary>
    private GameplayMusicRequest()
    {
    }

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this gameplay music request, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract void React<TContext>(GameplayMusicRequestReactions<TContext> reactions, TContext context);


    /// <summary>Schedules <paramref name="Request"/>'s music to play after its delay (<see cref="AudioQueues.ScheduleMusic"/>).</summary>
    /// <param name="Request">The music and how long it waits on the queue.</param>
    public sealed partial record Scheduled(DelayedMusicRequest Request) : GameplayMusicRequest
    {
        /// <inheritdoc/>
        internal override void React<TContext>(GameplayMusicRequestReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).Scheduled(this, context);
    }

    /// <summary>Runs the permanent-item fanfare's music sequence (<see cref="AudioQueues.QueuePermanentItemFanfare"/>).</summary>
    public sealed partial record PermanentItemFanfare : GameplayMusicRequest
    {
        /// <inheritdoc/>
        internal override void React<TContext>(GameplayMusicRequestReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).PermanentItemFanfare(this, context);
    }
}

/// <summary>What the owner of a <see cref="GameplayMusicRequest"/> does on each of its cases (#2553).</summary>
/// <param name="Scheduled">Reacts to a <see cref="GameplayMusicRequest.Scheduled"/>.</param>
/// <param name="PermanentItemFanfare">Reacts to a <see cref="GameplayMusicRequest.PermanentItemFanfare"/>.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
public sealed record GameplayMusicRequestReactions<TContext>(Action<GameplayMusicRequest.Scheduled, TContext> Scheduled, Action<GameplayMusicRequest.PermanentItemFanfare, TContext> PermanentItemFanfare);
