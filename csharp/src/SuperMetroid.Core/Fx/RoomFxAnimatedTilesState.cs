using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// One looping animation of a room's animated blocks: a room effect's, the horizontal spikes' or the Maridia sand's.
/// </summary>
/// <remarks>
/// Lava, acid, rain, and spores do not show pictures of the room tileset's. Their effect starts one looping animation,
/// each with its own named frames and durations. Playing the whole loop matters: showing one convenient frame would make
/// the first still image look plausible while discarding the game's cadence and preventing replacement graphics from
/// following the same frames.
/// </remarks>
internal sealed class RoomFxAnimatedTilesState
{
    /// <summary>
    /// The frame shown when the timer next expires, an index into the loaded loop's
    /// <see cref="RoomFxAnimatedTileLoop.Frames"/>: the cursor only ever moves to the next frame, and from the last frame
    /// back to the first in the same pass.
    /// </summary>
    private int nextFrame;
    /// <summary>Remaining passes before the current frame gives way to the next.</summary>
    private int frameHold;
    /// <summary>The loaded loop, or null while no animation is loaded.</summary>
    private RoomFxAnimatedTileLoop? loop;

    /// <summary>The loaded loop; only an active animation has one.</summary>
    private RoomFxAnimatedTileLoop Loop =>
        loop ?? throw new InvalidOperationException("Room-FX animation requires a loaded loop.");

    /// <summary>Whether an animation is loaded.</summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Replaces the previous room's animation with the one the selected effect type shows.
    /// Non-animated types deliberately leave the owner empty.
    /// </summary>
    public void Load(RoomFxType type) => LoadDefinition(ObjectOf(type));

    /// <summary>The animation a room effect of <paramref name="type"/> animates the effect overlay with, or none.</summary>
    /// <exception cref="InvalidOperationException">The type is undefined.</exception>
    public static AnimatedTileObject ObjectOf(RoomFxType type) =>
        type switch
        {
            RoomFxType.Lava => AnimatedTileObject.Lava,
            RoomFxType.Acid => AnimatedTileObject.Acid,
            RoomFxType.Rain => AnimatedTileObject.Rain,
            RoomFxType.Spores => AnimatedTileObject.Spores,
            RoomFxType.None or RoomFxType.Water or RoomFxType.Fog or RoomFxType.ScrollingSky or
                RoomFxType.Fireflea or RoomFxType.TourianEntranceStatue or
                RoomFxType.CeresRidley or RoomFxType.CeresElevator or RoomFxType.CeresHaze => 0,
            _ => throw new InvalidOperationException($"Undefined RoomFxType {type}."),
        };

    /// <summary>Loads the looping animation <paramref name="definition"/>, independent of effect type.</summary>
    /// <exception cref="InvalidOperationException">This owner has already loaded an animation.</exception>
    /// <exception cref="InvalidDataException"><paramref name="definition"/> is not a looping animation.</exception>
    public void LoadDefinition(AnimatedTileObject definition)
    {
        // Each room builds its own animations, so an owner loads one animation, from its fresh state.
        if (loop is not null)
            throw new InvalidOperationException($"Room-FX animation already loaded {loop.AnimatedObject}.");
        if (definition == AnimatedTileObject.None)
            return;

        if (!RoomFxAnimatedTiles.TryResolve(definition, out loop))
            throw new InvalidDataException($"Room-FX animation {definition} is not a looping animation.");
        nextFrame = 0;
        frameHold = 1;
        IsActive = true;
    }

    /// <summary>
    /// Runs one animation pass (#1406). When the timer expires the next frame is shown for its duration,
    /// selected in <paramref name="frames"/> at this update, as this update's frame boundary shows it; the loop
    /// returns from the last frame to the first.
    /// </summary>
    public void Step(ArtworkFrameSelections frames)
    {
        Ensure.NotNull(frames);
        if (!IsActive)
            return;

        if (--frameHold != 0)
            return;

        IReadOnlyList<TimedArtworkFrame> loopFrames = Loop.Frames;
        TimedArtworkFrame shown = loopFrames[nextFrame];
        frameHold = shown.Duration;
        // After the last frame the loop returns to the first.
        nextFrame = nextFrame + 1 < loopFrames.Count ? nextFrame + 1 : 0;
        frames.Select(shown.Frame);
    }
}
