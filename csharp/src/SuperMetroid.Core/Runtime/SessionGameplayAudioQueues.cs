using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// The session's sound and music queues as a runtime's gameplay requests reach them (#1620): each request goes straight
/// in as it is made, unless an attract demo is playing, whose game state turns gameplay's audio away.
/// </summary>
/// <remarks>
/// While a demo plays, a sound request is still tested against its queue, so what the queue would have done with it stays
/// readable (the Speed Booster's echo reads it), but nothing joins the queue; a music request is dropped. Evidence: the
/// game's sound queue routines test the game state after the queue's occupancy, and its music queue routines test it
/// before queueing.
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
    public SoundQueueAdmission QueueSound(SoundRequest request) =>
        queues.QueueSound(request.SoundEffect, request.Priority, request.SoundSuppressed || demo.IsAttractDemo);

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
