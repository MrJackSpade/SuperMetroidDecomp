using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Enemies.EffectActors;

/// <summary>
/// The room's effect actors (#1591): animation-driven decorative effects (dust, splashes, explosions, shells) that enemies
/// and projectiles spawn, step and draw independently of enemies and enemy projectiles. Each spawn creates a fresh
/// object; the room steps and draws the live ones in the order they spawned, and forgets each once it ends. There is no
/// limit on how many are live at once.
/// </summary>
[RoomLifetime]
internal sealed class RoomEffectActors
{
    /// <summary>Publishes the objects from the room's enemy artwork.</summary>
    private readonly EnemyPresentation _presentation;

    /// <summary>The places the objects take, which Kraid's mouth pass reads after a dud flash.</summary>
    private readonly EffectActorPlaces _places;

    /// <summary>The objects spawned in this room, in the order they spawned, until a later spawn forgets the ended ones.</summary>
    private readonly List<RoomEffectActor> _objects = [];

    /// <summary>The place each object in <see cref="_objects"/> took, for those that took one.</summary>
    private readonly Dictionary<RoomEffectActor, int> _takenPlaces = [];

    /// <summary>Told of each object the draw reaches, which a Puyo's later ceiling strike reads.</summary>
    private readonly PuyoCeilingTurnFromLastDraw _puyoCeilingTurn;

    /// <summary>
    /// Connects decorative effect actors to the room's presentation bindings, their places and what a Puyo's ceiling
    /// strike reads of their draws.
    /// </summary>
    internal RoomEffectActors(EnemyPresentation presentation, EffectActorPlaces places, PuyoCeilingTurnFromLastDraw puyoCeilingTurn)
    {
        _presentation = presentation;
        _places = places;
        _puyoCeilingTurn = puyoCeilingTurn;
    }

    /// <summary>The live objects, in the order they spawned.</summary>
    internal IEnumerable<RoomEffectActor> Live => _objects.Where(static spawned => spawned.IsActive);

    /// <summary>Spawns an object of <paramref name="kind"/> at <paramref name="position"/>, after every live object.</summary>
    /// <returns>The object, which its spawner holds when it moves, recolours or ends it.</returns>
    internal RoomEffectActor SpawnRoomEffectActor(
        Position16<RoomPixels> position,
        RoomEffectActorKind kind,
        SpriteGraphics graphics)
    {
        ForgetEnded();
        var spawned = new RoomEffectActor(kind, position, graphics);
        if (_places.Take() is int place)
            _takenPlaces.Add(spawned, place);
        _objects.Add(spawned);
        return spawned;
    }

    /// <summary>The place <paramref name="spawned"/> took when it spawned, or null when every place was taken.</summary>
    /// <param name="spawned">The object most recently spawned.</param>
    internal int? PlaceOf(RoomEffectActor spawned) =>
        _takenPlaces.TryGetValue(Ensure.NotNull(spawned), out int place) ? place : null;

    /// <summary>Forgets every object that has ended, freeing the places they took.</summary>
    private void ForgetEnded()
    {
        foreach (RoomEffectActor ended in _objects)
        {
            if (ended.IsActive || !_takenPlaces.Remove(ended, out int place))
                continue;
            _places.Free(place);
        }
        _objects.RemoveAll(static spawned => !spawned.IsActive);
    }

    /// <summary>Steps every live object, in the order they spawned.</summary>
    internal void StepRoomEffectActors()
    {
        // No animation spawns another object, so the list does not change while it is stepped.
        foreach (RoomEffectActor spawned in _objects)
            spawned.Step();
    }

    /// <summary>Ends every live object, as Draygon's burial does before its Evirs spawn and once he has sunk.</summary>
    internal void EndAll()
    {
        foreach (RoomEffectActor spawned in _objects)
            spawned.End();
    }

    /// <summary>
    /// Draws every live object, in the order they spawned: each from its owner's spawned-object sheet, and a hit burst, one
    /// of Samus's weapon effects, from her projectile sheets as one of her draws (#1713).
    /// </summary>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="camera">Camera position in whole room pixels.</param>
    /// <param name="hitBursts">What a hit burst draws: Samus's projectile sheets and the beams she has equipped.</param>
    internal void DrawRoomEffectActors(SpriteFrameBuffer sprites, Position16<RoomPixels> camera, SamusHitBurstArtwork hitBursts)
    {
        foreach (RoomEffectActor spawned in Live)
        {
            // Every placement is published wherever it lies; the display clips it to the view (#1520, #1541).
            Offset16<ScreenPixels> screen = spawned.Position.Whole.RelativeToCamera(camera);
            // Whether this draw reaches the value a Puyo's ceiling strike reads depends on where the object lies
            // relative to the view, which the service decides.
            _puyoCeilingTurn.EffectActorDrawn(
                CameraPastRoomEdge.FromCamera(spawned.Position.Whole.X, camera.X),
                CameraPastRoomEdge.FromCamera(spawned.Position.Whole.Y, camera.Y));

            // A hit burst shows Samus's burst sprites, every other object a frame of its enemy sheet (#1817).
            _ = spawned.Frame.Select(
                shown =>
                {
                    // The empty frame, between a Metroid's crackles and its shell's flashes, draws nothing.
                    if (!CommonEnemyEmptyFrames.DrawsNothing(shown))
                        _presentation.PublishSpawnedFrame(sprites, new SpriteSource(SpriteSourceKind.RoomEffectActor),
                            EnemySheetDefinitions.EffectActorFrameName(shown), screen, spawned.Graphics);
                    return true;
                },
                burst =>
                {
                    hitBursts.Draw(sprites, burst, screen);
                    return true;
                });
        }
    }
}
