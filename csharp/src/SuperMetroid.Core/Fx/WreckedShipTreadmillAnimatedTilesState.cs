using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Fx.Artwork;

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
    private WrappingCountdown _instructionTimer;
    /// <summary>Whether the object is still waiting for Phantoon's area-boss bit before its first frame.</summary>
    private bool _waitingForAreaBoss;

    /// <summary>The selected direction's object definition, which names its frame art.</summary>
    private WreckedShipTreadmillObjectDefinition? _compiledMechanics;

    /// <summary>Whether one of the two door-spawned objects occupies its native slot.</summary>
    public bool IsActive { get; private set; }

    /// <summary>Direction selected by the object header passed to $87:8027.</summary>
    public WreckedShipTreadmillDirection Direction { get; private set; }

    /// <summary>Index of the next entry in the four-frame cartridge list.</summary>
    public int NextFrameIndex { get; private set; }

    /// <summary>The picture the most recent handler pass showed, if it showed one.</summary>
    public AnimatedTileImage? LastImage { get; private set; }


    /// <summary>
    /// Mirrors <c>Spawn_AnimatedTilesObject</c> for object $8275 or $827B. Both lists begin
    /// on the wait-for-area-boss instruction with an instruction timer of one.
    /// </summary>
    public void Start(WreckedShipTreadmillDirection direction)
    {
        WreckedShipTreadmillObjectDefinition definition =
            WreckedShipTreadmillMechanicsDefinitions.ForDirection(direction);
        StartDefinition(direction, definition.AnimatedObject);
    }

    /// <summary>
    /// Starts one bank-$87 object. Only the two treadmill objects are compiled, and each only
    /// for its own direction; any other object is rejected before it can run.
    /// </summary>
    internal void StartDefinition(
        WreckedShipTreadmillDirection direction,
        AnimatedTileObject animatedObject)
    {

        IsActive = true;
        Direction = direction;
        NextFrameIndex = 0;
        LastImage = null;
        if (!WreckedShipTreadmillMechanicsDefinitions.TryResolve(animatedObject, out _compiledMechanics))
            throw new InvalidDataException($"Wrecked Ship treadmill {animatedObject} is not compiled.");
        if (_compiledMechanics.Direction != direction)
            throw new InvalidDataException(
                $"Wrecked Ship treadmill {animatedObject} belongs to {_compiledMechanics.Direction}, not {direction}.");
        _waitingForAreaBoss = true;
        _instructionTimer = WrappingCountdown.Start(1);
    }

    /// <summary>
    /// Runs one bank-$87 handler pass and publishes the source selected for the following
    /// NMI. While Phantoon's area-boss bit is clear, the wait ($87:81BA) holds the object for
    /// another pass and no source exists; once the bit is set, the four frames loop forever.
    /// </summary>
    public void Step(
        bool areaBossDefeated,
        ArtworkFrameSelections frames)
    {
        Ensure.NotNull(frames);
        LastImage = null;
        if (!IsActive)
            return;

        _instructionTimer = _instructionTimer.Decrement().Remaining;
        if (!_instructionTimer.IsZero)
            return;

        if (_waitingForAreaBoss)
        {
            if (!areaBossDefeated)
            {
                _instructionTimer = WrappingCountdown.Start(1);
                return;
            }
            _waitingForAreaBoss = false;
        }

        WreckedShipTreadmillObjectDefinition definition = _compiledMechanics ?? throw new InvalidOperationException(
            "Wrecked Ship treadmill lost its compiled mechanics.");
        _instructionTimer = WrappingCountdown.Start(WreckedShipTreadmillMechanicsDefinitions.FrameDuration);
        AnimatedTileImage image = definition.Image(NextFrameIndex);
        LastImage = image;
        frames.Select(AnimatedTileFrames.Frame(definition.AnimatedObject, NextFrameIndex));
        NextFrameIndex = (NextFrameIndex + 1) % WreckedShipTreadmillMechanicsDefinitions.FrameCount;
    }
}
