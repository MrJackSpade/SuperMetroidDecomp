namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>What a cue of the n00b tube's crack, shards and bubbles animations does: give a bubble or shard its falling angle, place a shard, start a piece's next motion, or delete the projectile.</summary>
internal enum NoobTubeProjectileCueKind
{
    /// <summary>Gives a bubble a random falling angle.</summary>
    AssignBubbleFallingAngle,
    /// <summary>Gives a shard a random falling angle and its falling speed.</summary>
    AssignShardFallingAngle,
    /// <summary>Deletes the projectile.</summary>
    Delete,
    /// <summary>Places the shard at its column on odd frames and hides it on even ones.</summary>
    FlickerHidden,
    /// <summary>Places the shard at its column on odd frames and mirrored across the tube on even ones.</summary>
    FlickerReflected,
    /// <summary>Starts the crack flickering.</summary>
    StartCrackFlicker,
    /// <summary>Starts the detached crack falling.</summary>
    StartCrackFall,
    /// <summary>Starts a shard swaying down after its first flight.</summary>
    StartShardFall,
    /// <summary>Starts a released-air bubble rising.</summary>
    StartBubbleRise,
    /// <summary>Starts a released-air bubble swaying down.</summary>
    StartBubbleFall,
}
