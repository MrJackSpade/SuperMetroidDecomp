using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Runs the two animated-tile objects spawned by Wrecked Ship entrance door setup. The door
/// chooses a direction; the object then waits for Phantoon's area-boss bit and afterwards
/// shows its four treadmill frames in a loop, one frame per pass, each selected as the frame its treadmill's tiles show
/// (#1406).
/// </summary>
[RoomLifetime]
public sealed class WreckedShipTreadmillAnimatedTilesState
{
    /// <summary>Passes left before the object next shows a frame or re-tests the boss wait.</summary>
    private int _frameHold;
    /// <summary>Whether the object is still waiting for Phantoon's area-boss bit before its first frame.</summary>
    private bool _waitingForAreaBoss;

    /// <summary>The selected direction's object definition, which names its frame art.</summary>
    private WreckedShipTreadmillLoop? _compiledMechanics;

    /// <summary>Whether one of the two door-spawned objects occupies its slot.</summary>
    public bool IsActive { get; private set; }

    /// <summary>Direction selected by the object definition passed when the object spawns.</summary>
    public WreckedShipTreadmillDirection Direction { get; private set; }

    /// <summary>Index of the next entry in the four-frame animation script.</summary>
    public int NextFrameIndex { get; private set; }

    /// <summary>
    /// Spawns the animated-tiles object for either treadmill, as the game does. Both scripts begin
    /// on the wait-for-area-boss step with a frame hold of one.
    /// </summary>
    public void Start(WreckedShipTreadmillDirection direction)
    {
        WreckedShipTreadmillLoop definition =
            WreckedShipTreadmills.ForDirection(direction);
        StartDefinition(direction, definition.AnimatedObject);
    }

    /// <summary>
    /// Starts one animated-tile object. Only the two treadmill objects are compiled, and each only
    /// for its own direction; any other object is rejected before it can run.
    /// </summary>
    internal void StartDefinition(
        WreckedShipTreadmillDirection direction,
        AnimatedTileObject animatedObject)
    {

        IsActive = true;
        Direction = direction;
        NextFrameIndex = 0;
        if (!WreckedShipTreadmills.TryResolve(animatedObject, out _compiledMechanics))
            throw new InvalidDataException($"Wrecked Ship treadmill {animatedObject} is not compiled.");
        if (_compiledMechanics.Direction != direction)
            throw new InvalidDataException(
                $"Wrecked Ship treadmill {animatedObject} belongs to {_compiledMechanics.Direction}, not {direction}.");
        _waitingForAreaBoss = true;
        _frameHold = 1;
    }

    /// <summary>
    /// Runs one animated-tile handler pass and publishes the source selected for the following
    /// frame boundary. While Phantoon's area-boss bit is clear, the wait holds the object for
    /// another pass and no source exists; once the bit is set, the four frames loop forever.
    /// </summary>
    public void Step(
        bool areaBossDefeated,
        ArtworkFrameSelections frames)
    {
        Ensure.NotNull(frames);
        if (!IsActive)
            return;

        if (--_frameHold != 0)
            return;

        if (_waitingForAreaBoss)
        {
            if (!areaBossDefeated)
            {
                _frameHold = 1;
                return;
            }
            _waitingForAreaBoss = false;
        }

        WreckedShipTreadmillLoop definition = _compiledMechanics ?? throw new InvalidOperationException(
            "Wrecked Ship treadmill lost its compiled mechanics.");
        _frameHold = WreckedShipTreadmillMechanicsDefinitions.FrameDuration;
        frames.Select(AnimatedTileFrames.Frame(definition.AnimatedObject, NextFrameIndex));
        NextFrameIndex = (NextFrameIndex + 1) % WreckedShipTreadmillMechanicsDefinitions.FrameCount;
    }
}
