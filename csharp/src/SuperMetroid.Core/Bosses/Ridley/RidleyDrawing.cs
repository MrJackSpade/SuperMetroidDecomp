using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Draws both Ridleys' tail and wings before the common writer emits his body (#2229).</summary>
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

    /// <summary>Draws Ridley's tail, its tip first and then segments five to the base.</summary>
    /// <param name="sprites">The frame's sprites.</param>
    /// <param name="ridley">Ridley's encounter.</param>
    /// <param name="camera">The camera.</param>
    internal void DrawTail(SpriteFrameBuffer sprites, RidleyEncounterState ridley, Position16<RoomPixels> camera)
    {
        RidleyTailSegment[] segments = ridley.Tail.Segments;
        RidleyTailSegment tip = segments[^1];
        // The tip's pose follows its direction turned by the link before it.
        EnemyFrameId tipFrame = RidleySupplementalVisualDefinitions.TailTipFrames.At(
            tip.Angle.Direction.TurnedBy(segments[^2].Angle.Direction));
        DrawTailFrame(sprites, ridley.TailAndWingsShading, tip, tipFrame, camera);

        // The tail draw walks back from segment five to the base after emitting the
        // tip. Earlier draws win equal-priority overlap, so reversing this visually
        // changes the curl even though every individual piece is otherwise correct.
        for (int index = segments.Length - 2; index >= 0; index--)
            DrawTailFrame(sprites, ridley.TailAndWingsShading, segments[index], RidleySupplementalVisualDefinitions.LinkFrames[index], camera);
    }

    /// <summary>Draws Ridley's wings at his body for the way he faces; turning toward the screen he shows none.</summary>
    /// <param name="sprites">The frame's sprites.</param>
    /// <param name="place">Ridley's body.</param>
    /// <param name="ridley">Ridley's encounter.</param>
    /// <param name="camera">The camera.</param>
    internal void DrawWings(SpriteFrameBuffer sprites, RoomEnemyPlace place, RidleyEncounterState ridley, Position16<RoomPixels> camera)
    {
        RidleyFacing facing = ridley.Flight.FacingDirection;
        if (facing == RidleyFacing.Turning)
            return;
        IReadOnlyList<EnemyFrameId> wingBeat = facing == RidleyFacing.Left
            ? RidleySupplementalVisualDefinitions.WingBeatFacingLeft
            : RidleySupplementalVisualDefinitions.WingBeatFacingRight;
        EnemyFrameId wingFrame = wingBeat[ridley.Flight.WingBeatStep];
        _presentation.PublishEncounterFrame(sprites, new SpriteSource(SpriteSourceKind.RidleyWings), wingFrame,
            place.Position.Whole.RelativeToCamera(camera), ridley.TailAndWingsShading);
    }

    /// <summary>Publishes one tail segment's frame at its screen position for the current camera.</summary>
    private void DrawTailFrame(
        SpriteFrameBuffer sprites,
        PictureShading colouring,
        RidleyTailSegment segment,
        EnemyFrameId frame,
        Position16<RoomPixels> camera) =>
        _presentation.PublishEncounterFrame(sprites, new SpriteSource(SpriteSourceKind.RidleyTail), frame,
            segment.Position.RelativeToCamera(camera), colouring);
}
