namespace SuperMetroid.Core.Audio;

/// <summary>One music request gameplay makes for the music queue through <see cref="GameplayAudioRequests"/>.</summary>
public abstract record GameplayMusicRequest
{
    /// <summary>Closes the hierarchy to the requests declared here.</summary>
    private GameplayMusicRequest()
    {
    }

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this gameplay music request, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    internal abstract void React(GameplayMusicRequestReactions reactions);


    /// <summary>Schedules <paramref name="Request"/>'s music to play after its delay (<see cref="AudioQueues.ScheduleMusic"/>).</summary>
    /// <param name="Request">The music and how long it waits on the queue.</param>
    public sealed record Scheduled(DelayedMusicRequest Request) : GameplayMusicRequest
    {
        /// <inheritdoc/>
        internal override void React(GameplayMusicRequestReactions reactions) => Ensure.NotNull(reactions).Scheduled(this);
    }

    /// <summary>Runs the permanent-item fanfare's music sequence (<see cref="AudioQueues.QueuePermanentItemFanfare"/>).</summary>
    public sealed record PermanentItemFanfare : GameplayMusicRequest
    {
        /// <inheritdoc/>
        internal override void React(GameplayMusicRequestReactions reactions) => Ensure.NotNull(reactions).PermanentItemFanfare(this);
    }
}

/// <summary>What the owner of a <see cref="GameplayMusicRequest"/> does on each of its cases (#2553).</summary>
/// <param name="Scheduled">Reacts to a <see cref="GameplayMusicRequest.Scheduled"/>.</param>
/// <param name="PermanentItemFanfare">Reacts to a <see cref="GameplayMusicRequest.PermanentItemFanfare"/>.</param>
public sealed record GameplayMusicRequestReactions(Action<GameplayMusicRequest.Scheduled> Scheduled, Action<GameplayMusicRequest.PermanentItemFanfare> PermanentItemFanfare);
