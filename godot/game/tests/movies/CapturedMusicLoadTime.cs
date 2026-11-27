using Game.Compatibility;

namespace Game.Tests.Movies;

/// <summary>
/// How long a set of music took to load on the console a movie was recorded on: the frames the recording's list of
/// updates says passed inside the update being played while the cartridge waited on its sound program, which the
/// list leaves out of its updates and the cartridge's count of frames kept.
/// </summary>
internal sealed class CapturedMusicLoadTime : IMusicLoadTime
{
    private int _frames;

    /// <summary>Takes the movie's updates and which of them is about to be played.</summary>
    public void Expect(IReadOnlyList<MovieUpdate> updates, int playing) =>
        _frames = updates[playing].WaitedFramesAfter - (playing > 0 ? updates[playing - 1].WaitedFramesAfter : 0);

    /// <inheritdoc/>
    public int UpdatesToLoad(string set) => _frames;
}
