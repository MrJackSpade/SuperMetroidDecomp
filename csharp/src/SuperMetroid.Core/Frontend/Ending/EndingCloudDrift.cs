using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// An escape cloud's drift (#2489): the cloud waits until the turning floor's zoom releases it, then from its next update
/// on drifts by a fixed step every update without testing the zoom again.
/// </summary>
internal sealed class EndingCloudDrift : EndingActorMotion
{
    /// <summary>How far the cloud drifts each update once released.</summary>
    private readonly Offset16<ScreenPixels> step;

    /// <summary>Whether the floor's zoom releases the waiting cloud.</summary>
    private readonly Func<FloorZoom, bool> releases;

    /// <summary>Whether the floor's zoom has released the cloud.</summary>
    internal bool Drifting { get; set; }

    /// <summary>Creates a waiting cloud that drifts by <paramref name="step"/> once <paramref name="releases"/> says the zoom has released it.</summary>
    private EndingCloudDrift(Offset16<ScreenPixels> step, Func<FloorZoom, bool> releases)
    {
        this.step = step;
        this.releases = releases;
    }

    /// <summary>
    /// A side-cloud escape cloud, released once the zoom reaches <see cref="EndingMotionDefinitions.CloudMotionStartZoom"/>:
    /// the right clouds drift left and up, the left clouds right and down, two pixels down for one across.
    /// </summary>
    internal static EndingCloudDrift SideCloud(HorizontalDirection drift) => new(
        drift == HorizontalDirection.Left ? EndingMotionDefinitions.RightSideCloudDrift : EndingMotionDefinitions.LeftSideCloudDrift,
        zoom => zoom >= EndingMotionDefinitions.CloudMotionStartZoom);

    /// <summary>
    /// A top-and-bottom-cloud escape cloud, released while the zoom is below
    /// <see cref="EndingMotionDefinitions.TopAndBottomCloudZoomLimit"/>: the top clouds drift down and the bottom clouds up,
    /// a pixel an update.
    /// </summary>
    internal static EndingCloudDrift TopOrBottomCloud(VerticalDirection drift) => new(
        new Offset16<ScreenPixels>(0, 1.Toward(drift)),
        zoom => zoom < EndingMotionDefinitions.TopAndBottomCloudZoomLimit);

    /// <summary>Releases the waiting cloud when the zoom allows, its drift beginning next update, or drifts a released one.</summary>
    internal override void Step(ICinematicActor<EndingSceneCue> actor, IEndingActorScene scene)
    {
        Ensure.NotNull(actor);
        Ensure.NotNull(scene);
        if (!Drifting)
        {
            // Releasing a cloud does not run its drift immediately, and a released cloud no longer tests the zoom.
            Drifting = releases(scene.FloorZoom);
            return;
        }
        actor.ScreenPlace += step;
    }
}
