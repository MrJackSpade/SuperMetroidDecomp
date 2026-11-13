namespace SuperMetroid.Core.Audio;

/// <summary>One music request gameplay makes for the music queue, as <see cref="GameplayAudioRequests"/> records it.</summary>
public abstract record GameplayMusicRequest
{
    /// <summary>Closes the hierarchy to the requests declared here.</summary>
    private GameplayMusicRequest()
    {
    }

    /// <summary>Schedules <paramref name="Request"/>'s music to play after its delay (<see cref="AudioQueues.ScheduleMusic"/>).</summary>
    /// <param name="Request">The music and how long it waits on the queue.</param>
    public sealed record Scheduled(DelayedMusicRequest Request) : GameplayMusicRequest;

    /// <summary>Runs the permanent-item fanfare's music sequence (<see cref="AudioQueues.QueuePermanentItemFanfare"/>).</summary>
    public sealed record PermanentItemFanfare : GameplayMusicRequest;
}
