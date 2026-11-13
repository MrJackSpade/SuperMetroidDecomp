using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// The session's sound and music queues as a runtime's gameplay requests reach them (#1620): each request goes straight
/// in as it is made. A sound request passes the queues' own test (<see cref="SoundQueueAdmission"/>), which turns it away
/// while an attract demo plays or a power bomb explodes; a music request is dropped while an attract demo plays.
/// </summary>
/// <remarks>
/// Evidence: the game's music queue routines test the game state (below the demo states) before queueing, and nothing
/// else; its sound queue routines make their own tests, which the session's queues model.
/// </remarks>
/// <param name="queues">The session's queues.</param>
/// <param name="demo">Whether an attract demo is driving the runtime.</param>
internal sealed class SessionGameplayAudioQueues(AudioQueues queues, IAttractDemoState demo) : IGameplayAudioQueues
{
    /// <summary>The session's queues.</summary>
    private readonly AudioQueues queues = Ensure.NotNull(queues);

    /// <summary>Whether an attract demo is driving the runtime.</summary>
    private readonly IAttractDemoState demo = Ensure.NotNull(demo);

    /// <inheritdoc/>
    public SoundQueueAdmission QueueSound(SoundRequest request) => queues.QueueSound(request);

    /// <inheritdoc/>
    public void QueueMusic(GameplayMusicRequest request)
    {
        Ensure.NotNull(request);
        if (demo.IsAttractDemo)
            return;
        switch (request)
        {
            case GameplayMusicRequest.Scheduled scheduled:
                queues.ScheduleMusic(scheduled.Request.Request, scheduled.Request.Delay);
                return;
            case GameplayMusicRequest.PermanentItemFanfare:
                queues.QueuePermanentItemFanfare();
                return;
            default:
                throw new InvalidOperationException($"Undefined gameplay music request {request}.");
        }
    }
}
