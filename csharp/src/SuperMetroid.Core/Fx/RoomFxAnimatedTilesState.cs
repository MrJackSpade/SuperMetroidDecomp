using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The simple looping animated-tile object spawned directly by a room-FX type.
/// </summary>
/// <remarks>
/// Lava, acid, rain, and spores do not obtain their visible characters from the room tileset.
/// Their FX type initializer spawns one of the looping objects, each with its own timed frames,
/// transfer size, and destination. Playing the whole loop matters: copying one convenient
/// frame would make the first still image look plausible while discarding the game's cadence
/// and preventing replacement graphics from following the same data path.
/// </remarks>
internal sealed class RoomFxAnimatedTilesState
{
    /// <summary>The object chosen by the current room FX type, or <see cref="AnimatedTileObject.None"/>.</summary>
    private AnimatedTileObject animatedObject;
    /// <summary>
    /// The frame shown when the timer next expires, an index below the loaded object's
    /// <see cref="RoomFxAnimatedTileLoop.FrameCount"/>. It replaces a script position in
    /// the object's list: every supported list is its timed frames followed by a goto back to the first frame, so the
    /// cursor only ever moves to the next frame, and from the last frame back to the first in the same handler pass.
    /// </summary>
    private int nextFrame;
    /// <summary>Remaining handler passes before the current animated frame advances.</summary>
    private int frameHold;
    /// <summary>Frame schedule, transfer size and destination of the loaded object.</summary>
    private RoomFxAnimatedTileLoop? compiledMechanics;

    /// <summary>The loaded object's definition; only an active slot has one.</summary>
    private RoomFxAnimatedTileLoop Mechanics =>
        compiledMechanics ?? throw new InvalidOperationException("Room-FX animation requires compiled mechanics.");

    /// <summary>Whether a room-FX animated-tile object currently owns this slot.</summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Replaces the previous room's object with the one spawned by the selected FX type.
    /// Non-animated types deliberately leave the owner empty.
    /// </summary>
    public void Load(RoomFxType type) => LoadDefinition(ObjectOf(type));

    /// <summary>The animated-tile object a room effect of <paramref name="type"/> animates effect overlay's characters with, or none.</summary>
    /// <exception cref="InvalidOperationException">The type is undefined.</exception>
    public static AnimatedTileObject ObjectOf(RoomFxType type) =>
        type switch
        {
            RoomFxType.Lava => AnimatedTileObject.Lava,
            RoomFxType.Acid => AnimatedTileObject.Acid,
            RoomFxType.Rain => AnimatedTileObject.Rain,
            RoomFxType.Spores => AnimatedTileObject.Spores,
            RoomFxType.None or RoomFxType.Water or RoomFxType.Fog or RoomFxType.ScrollingSky or
                RoomFxType.UnusedScrollingSky or RoomFxType.Fireflea or RoomFxType.TourianEntranceStatue or
                RoomFxType.CeresRidley or RoomFxType.CeresElevator or RoomFxType.CeresHaze => 0,
            _ => throw new InvalidOperationException($"Undefined RoomFxType {type}."),
        };

    /// <summary>Loads a timed looping object selected by its header, independent of FX type.</summary>
    /// <exception cref="InvalidOperationException">This object has already loaded a definition.</exception>
    public void LoadDefinition(AnimatedTileObject definition)
    {
        // Each room builds its own animated-tile objects, so an object loads one definition, from its fresh state.
        if (animatedObject != AnimatedTileObject.None)
            throw new InvalidOperationException($"Room-FX animation already loaded {animatedObject}.");
        animatedObject = definition;
        if (animatedObject == AnimatedTileObject.None)
            return;

        if (!RoomFxAnimatedTiles.TryResolve(animatedObject, out compiledMechanics))
            throw new InvalidDataException($"Room-FX animation {animatedObject} is not compiled.");
        nextFrame = 0;
        frameHold = 1;
        IsActive = true;

        if (compiledMechanics.FrameArtworkSize == 0)
            throw new InvalidDataException($"Room-FX animated-tile object {animatedObject} has no character artwork.");
    }

    /// <summary>
    /// Runs one animated-tile pass (#1406). When the timer expires the next frame is shown for its duration,
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

        RoomFxAnimatedTileLoop mechanics = Mechanics;
        int shown = nextFrame;
        frameHold = mechanics.FrameDuration;
        // The list's goto after the last frame returns to the first frame.
        nextFrame = shown + 1 < mechanics.FrameCount ? shown + 1 : 0;
        frames.Select(AnimatedTileFrames.Frame(animatedObject, shown));
    }
}
