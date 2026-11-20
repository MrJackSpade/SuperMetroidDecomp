using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Draws both Ridleys' tail and wings before the common enemy drawing draws his body (#2229).</summary>
[RoomLifetime]
internal sealed class RidleyDrawing
{
    /// <summary>Publishes his drawn parts' frames.</summary>
    private readonly EnemyPresentation _presentation;

    /// <summary>Builds the drawing over the room's enemy presentation.</summary>
    internal RidleyDrawing(EnemyPresentation presentation)
    {
        _presentation = presentation;
    }

    /// <summary>Draws Ridley's tail, tip first, and then his wings, while his movement and animation run.</summary>
    /// <param name="sprites">The frame's sprites.</param>
    /// <param name="place">Ridley's body.</param>
    /// <param name="ridley">Ridley's encounter.</param>
    /// <param name="camera">The camera.</param>
    internal void DrawTailAndWings(
        SpriteFrameBuffer sprites,
        RoomEnemyPlace place,
        RidleyEncounterState ridley,
        Position16<RoomPixels> camera)
    {
        if (!ridley.MovementAnimationEnabled)
            return;
        DrawTail(sprites, ridley, camera);
        DrawWings(sprites, place, ridley, camera);
    }

    /// <summary>Draws Ridley's tail, its tip first and then its links from the inner joint back to the base.</summary>
    /// <param name="sprites">The frame's sprites.</param>
    /// <param name="ridley">Ridley's encounter.</param>
    /// <param name="camera">The camera.</param>
    private void DrawTail(SpriteFrameBuffer sprites, RidleyEncounterState ridley, Position16<RoomPixels> camera)
    {
        RidleyTailLink tip = ridley.Tail.Tip;
        // The tip's pose follows the direction it points in, turned by the link before it.
        EnemyFrameId tipFrame = RidleySupplementalVisualDefinitions.TailTipFrames.At(tip.PointingDirection);
        DrawTailFrame(sprites, ridley.TailAndWingsShading, tip, tipFrame, camera);

        // The tail is drawn from the tip back from the inner joint to the base, so the links nearer the tip lie over
        // the others; drawn the other way, the curl would look different.
        IEnumerable<RidleyTailLinkPicture> links = ridley.Tail.Links.Zip(
            RidleySupplementalVisualDefinitions.LinkFrames, (link, frame) => new RidleyTailLinkPicture(link, frame));
        foreach (RidleyTailLinkPicture picture in links.Reverse())
            DrawTailFrame(sprites, ridley.TailAndWingsShading, picture.Link, picture.Frame, camera);
    }

    /// <summary>Draws Ridley's wings at his body for the way he faces; turning toward the screen he shows none.</summary>
    /// <param name="sprites">The frame's sprites.</param>
    /// <param name="place">Ridley's body.</param>
    /// <param name="ridley">Ridley's encounter.</param>
    /// <param name="camera">The camera.</param>
    private void DrawWings(SpriteFrameBuffer sprites, RoomEnemyPlace place, RidleyEncounterState ridley, Position16<RoomPixels> camera)
    {
        RidleyFacing facing = ridley.Flight.FacingDirection;
        if (facing == RidleyFacing.Turning)
            return;
        RidleyWingPose pose = ridley.Flight.Wings.Pose.Entry;
        EnemyFrameId wingFrame = facing == RidleyFacing.Left ? pose.FacingLeft : pose.FacingRight;
        _presentation.PublishEncounterFrame(sprites, new SpriteSource(SpriteSourceKind.RidleyWings), wingFrame,
            place.Position.Whole.RelativeToCamera(camera), ridley.TailAndWingsShading);
    }

    /// <summary>Publishes one tail link's frame at its screen position for the current camera.</summary>
    private void DrawTailFrame(
        SpriteFrameBuffer sprites,
        PictureShading colouring,
        RidleyTailLink link,
        EnemyFrameId frame,
        Position16<RoomPixels> camera) =>
        _presentation.PublishEncounterFrame(sprites, new SpriteSource(SpriteSourceKind.RidleyTail), frame,
            link.Position.RelativeToCamera(camera), colouring);
}
