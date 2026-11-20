using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.World;

/// <summary>
/// The enemies' draw phase: the queue of live enemies the enemy pass fills, the common draw-shading selection, and each
/// enemy's frame and supplemental sprites with the boss body frames the draw pass shows. Each draw shows at its frame's
/// depth; the display culls what lies off screen.
/// </summary>
[RoomLifetime]
internal sealed class EnemyDrawList
{
    /// <summary>The encounters' sprites drawn after every queued enemy, in their order (#2571).</summary>
    private readonly IReadOnlyList<IEnemyLateDraw> _lateDraws;
    /// <summary>Supplies the shared enemy frame counter that phases the hit flash.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Shows a reached enemy's boss body frame before its picture is drawn.</summary>
    private readonly EnemyAnimationPlayer _animationPlayer;
    /// <summary>Publishes each enemy's frame picture to the frame's sprites.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Supplies every installed enemy frame's display and validates active room ownership.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Told of each queued enemy the draw pass reaches, which a Puyo's later ceiling strike reads.</summary>
    private readonly PuyoCeilingTurnFromLastDraw _puyoCeilingTurn;

    /// <summary>Retains the behavior and presentation owners used by the enemy phase's draw pass.</summary>
    internal EnemyDrawList(EnemyDrawListDependencies dependencies)
    {
        _lateDraws = dependencies.LateDraws;
        _frameContext = dependencies.FrameContext;
        _animationPlayer = dependencies.AnimationPlayer;
        _presentation = dependencies.Presentation;
        _room = dependencies.Room;
        _puyoCeilingTurn = dependencies.PuyoCeilingTurn;
    }
    /// <summary>The live enemies the enemy pass queued to show, in <see cref="EnemyPlaceOrder"/>, each with whether the draw pass reaches it.</summary>
    private readonly List<QueuedEnemy> _queued = [];

    /// <summary>Empties the queue before the enemy pass rebuilds it.</summary>
    internal void Clear() => _queued.Clear();

    /// <summary>
    /// Queues <paramref name="enemy"/> to show after the enemies already queued; <paramref name="reached"/> tells whether
    /// the draw pass reaches it, which only a reached enemy's draw-pass side effects depend on.
    /// </summary>
    internal void Enqueue(RoomEnemyPlace enemy, bool reached) => _queued.Add(new QueuedEnemy(Ensure.NotNull(enemy), reached));

    /// <summary>The draws <see cref="Advance"/> prepared for <see cref="Draw"/> to publish next.</summary>
    private List<PreparedEnemyDraw>? _prepared;

    /// <summary>
    /// The simulation half of the enemies' draw, in queue order. For each enemy the draw pass reaches, the Puyo value its
    /// draw clears is cleared, its shake tick is consumed while its origin is formed, and its boss body frame is shown;
    /// these run whenever the game's draw pass runs, whether or not the frame is seen. Every queued enemy's picture is
    /// prepared for <see cref="Draw"/>.
    /// </summary>
    /// <param name="camera">Camera position (viewport top-left) in whole room pixels.</param>
    /// <exception cref="InvalidOperationException">No installed enemy frame displays are bound.</exception>
    /// <exception cref="InvalidDataException">A reached enemy shows no frame, or an enemy shows a frame that is not installed.</exception>
    public void Advance(Position16<RoomPixels> camera)
    {
        var prepared = new List<PreparedEnemyDraw>();
        foreach ((RoomEnemyPlace place, bool reached) in _queued)
        {
            // The draw of every reached enemy, a cleared or reserved one too, leaves the value a Puyo's ceiling
            // strike reads clear.
            if (reached)
                _puyoCeilingTurn.EnemyDrawn();
            // Power-bomb damage runs after queue construction and can clear a
            // queued actor or replace it with the inert respawn reservation.
            // Neither lifecycle state owns a display composition.
            if (place.DefinitionId == EnemyDefinitionId.None)
                continue;

            // The draw offset is zero for enemies the room places; the offset is added
            // before the camera is taken away, and the sum wraps as room positions do.
            // An enemy whose animation places its picture draws it there (#2210).
            var placed = place.Entity as IPlacedEnemyPicture;
            Offset16<ScreenPixels> origin = (place.Position.Whole + place.DrawOffset + (placed?.PicturePlacement ?? Offset16<RoomPixels>.Zero))
                .RelativeToCamera(camera);

            // The draw pass consumes one shake tick of an enemy it reaches while forming
            // its origin. The enemy's own update count chooses one pixel left or right; this is deliberately not the
            // room's background displacement. An earthquake that shakes the enemies, as Ceres's does
            // (EarthquakeShake.ShakesEnemies), restarts the shake for every active enemy, which is why its door sprites
            // visibly move with the station instead of remaining pinned to host screen space.
            if (reached && place.Shake.IsRunning)
            {
                origin += new Offset16<ScreenPixels>(1.Toward(EnemyDrawPhaseDefinitions.ShakeShiftsLeft.IsSetIn(place.FrameCounter)
                    ? HorizontalDirection.Left : HorizontalDirection.Right), 0);
                place.Shake.Count();
            }
            PictureShading drawShading = SelectCommonEnemyDrawShading(place);
            // An enemy the draw pass does not reach and that has shown no frame yet shows nothing; one it reaches has
            // always shown a frame.
            if (place.Frame is not { } shown)
            {
                if (reached)
                    throw new InvalidDataException($"Enemy {place.DefinitionId} draws before showing a frame.");
                continue;
            }
            // Resolve the sprite picture and the body frame through one presentation binding. Validate the
            // frame before the body is shown; physical selectors continue to belong to the enemy's animations
            // and collision definitions. The empty frame draws nothing and is on no sheet (#1513).
            EnemyFrameDisplays displays = _room.Session.FrameDisplays;
            if (!displays.TryGetDisplay(shown, out bool pictured))
                throw new InvalidDataException($"Enemy {place.DefinitionId} frame {shown} has no installed presentation.");
            // Only the draw pass shows a boss body frame; a frame without a body shows none.
            if (reached)
                _animationPlayer.DrawBossBodyFrame(place, displays.GetDisplayFrame(shown));
            prepared.Add(new PreparedEnemyDraw(place, origin, drawShading, shown, pictured, placed?.PictureDepth));
        }
        _prepared = prepared;
    }

    /// <summary>
    /// Publishes the draws <see cref="Advance"/> prepared, in queue order, each at the depth its frame shows at. An
    /// occupant's supplemental sprites are written just before its body; Mother Brain's late parts and the dead Torizo's
    /// sand heap follow every queued enemy.
    /// </summary>
    /// <param name="sprites">The frame's sprite description; the occupants and the late draws write its sprites.</param>
    /// <param name="camera">Camera position (viewport top-left) in whole room pixels.</param>
    /// <param name="samus">Samus, whom Mother Brain's brain animation reads as her late parts advance it.</param>
    /// <exception cref="InvalidOperationException">The draws were not prepared by <see cref="Advance"/>.</exception>
    public void Draw(SpriteFrameBuffer sprites, Position16<RoomPixels> camera, SamusState samus)
    {
        Ensure.NotNull(sprites);
        if (_prepared is not { } prepared)
            throw new InvalidOperationException("The enemies were not advanced before drawing.");
        _prepared = null;

        foreach (PreparedEnemyDraw draw in prepared)
        {
            RoomEnemyPlace place = draw.Place;
            var source = new SpriteSource(SpriteSourceKind.Enemy);
            // An occupant may draw supplemental sprites before the common enemy drawing draws its body (Ridley's tail
            // and wings).
            if (place.Entity is IEnemyDrawHooks drawHooks)
                drawHooks.DrawBeforeBody(sprites, camera);
            // Every frame is one whole sheet picture placed at the enemy's origin. Collision still
            // uses the game's own frame hitboxes, never the picture.
            if (draw.Pictured)
                _presentation.PublishSheetFrame(sprites, source, new EnemySheetDraw(place.DefinitionId, EnemySheetDefinitions.FrameName(draw.Frame),
                    draw.Origin)
                {
                    Shading = draw.Shading,
                    RotRows = (place.Entity as IShowsCorpseRotRows)?.ShownRotRows,
                    Depth = draw.Depth,
                });
        }

        // The encounters' late draws follow every queued enemy's draw; Mother Brain's advances her brain, which reads the
        // room's random numbers.
        foreach (IEnemyLateDraw late in _lateDraws)
            late.DrawAfterEnemies(sprites, camera, samus);
    }

    /// <summary>One live enemy queued to show.</summary>
    /// <param name="Place">The enemy.</param>
    /// <param name="Reached">Whether the draw pass reaches it, running its draw's side effects.</param>
    private readonly record struct QueuedEnemy(RoomEnemyPlace Place, bool Reached);

    /// <summary>One queued enemy's draw, formed by <see cref="Advance"/>.</summary>
    /// <param name="Place">The enemy.</param>
    /// <param name="Origin">Its screen origin, shaken when the draw pass reaches it.</param>
    /// <param name="Shading">The shading the common draw selected for this frame (#1512).</param>
    /// <param name="Frame">The frame the enemy shows, which names its sheet picture.</param>
    /// <param name="Pictured">Whether the frame has a sheet picture: the empty frame, a frame shown only on a boss body and
    /// a frame whose pieces show nothing have none.</param>
    /// <param name="Depth">The depth the enemy places its picture at (#2210), or null for its picture's own.</param>
    private readonly record struct PreparedEnemyDraw(RoomEnemyPlace Place,
        Offset16<ScreenPixels> Origin, PictureShading Shading, EnemyFrameId Frame, bool Pictured, ActorDepth? Depth);

    /// <summary>
    /// Selects the shading an enemy is drawn in, immediately before
    /// the draw publishes its frame's picture.
    /// </summary>
    /// <remarks>
    /// A hit does not replace an enemy's own colouring. On alternating phases of the
    /// shared enemy frame counter, the draw shows the common hurt colours instead;
    /// the following draw shows the enemy's own again. A frozen enemy's flash
    /// shows the frozen colours the same way. Every enemy flashes through this one choice.
    /// Ridley makes the same choice himself, once for his body, wings and tail, so they flash on one phase.
    /// </remarks>
    private PictureShading SelectCommonEnemyDrawShading(RoomEnemyPlace place)
    {
        if (place.Entity is IEnemyDrawHooks drawHooks && drawHooks.CommonDrawShading() is { } owned)
            return owned;

        // A hit flash shows the common hurt colours on alternate phases. The flash shades the draw over the enemy's own
        // shading: the colours it changes show the flash, and the colours it leaves as they are keep showing what the
        // enemy's own effects laid there.
        if (place.HurtFlash.IsRunning && EnemyDrawPhaseDefinitions.HurtFlashShown.IsSetIn(_frameContext.EnemyPassCount))
            return place.Shading.With(EnemyShades.HurtFlash);

        if (place.Freeze.IsRunning &&
            (place.Freeze.UpdatesLeft >= EnemyDrawPhaseDefinitions.FrozenColoursSteadyUpdates ||
             EnemyDrawPhaseDefinitions.FrozenColoursShown.IsSetIn(place.Freeze.UpdatesLeft)))
        {
            return place.Shading.With(EnemyShades.Frozen);
        }

        return place.Shading;
    }
}
