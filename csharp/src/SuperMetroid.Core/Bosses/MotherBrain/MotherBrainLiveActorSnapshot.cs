namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The live Mother Brain body, brain and neck state the rainbow-beam sequence mirrors (#633).</summary>
internal sealed record MotherBrainLiveActorSnapshot
{
    /// <summary>Body.</summary>
    public required Position16<RoomPixels> Body { get; init; }

    /// <summary>Body pose.</summary>
    public required MotherBrainBodyPose BodyPose { get; init; }

    /// <summary>The live encounter stage, which the sequence's shot reactions and footsteps branch on.</summary>
    public required MotherBrainForm Form { get; init; }

    /// <summary>Exact body enemy property word touched by `$A9:AEE1` and `$A9:AFF7`. Keeping the raw flags avoids guessing names for engine-wide enemy-property bits before bank `$A0` is fully translated: death sets `$0400`, then fade completion sets `$0100` and clears `$2000` with the cartridge's literal OR/AND sequence.</summary>
    public required SuperMetroid.Core.Enemies.EnemyPropertyWord BodyProperties { get; init; }

    /// <summary>Second body property word cleared when the faded body becomes non-interactive.</summary>
    public required SuperMetroid.Core.Enemies.EnemyExtraPropertyWord BodyExtraProperties { get; init; }

    /// <summary>Mother Brain brain-slot room position used by `$A9:BB82/$BB8F`'s aim vector.</summary>
    public required Position16<RoomPixels> Brain { get; init; }

    /// <summary>Brain-slot health rewritten to 36,000 when corpse state one is published.</summary>
    public required int BrainHealth { get; init; }

    /// <summary>Exact brain enemy property word; death/rotting sets raw bits `$0400/$0100`.</summary>
    public required SuperMetroid.Core.Enemies.EnemyPropertyWord BrainProperties { get; init; }

    /// <summary>Second brain property word cleared when the rot animation completes.</summary>
    public required SuperMetroid.Core.Enemies.EnemyExtraPropertyWord BrainExtraProperties { get; init; }

    /// <summary>Lower neck angle updated by the brain-slot handler at <c>$A9:9072</c>. The final beam has already lowered it to <c>$3000</c> before the Baby interrupts the body.</summary>
    public required RotationAngle LowerNeckAngle { get; init; }

    /// <summary>Upper neck angle, correspondingly lowered to <c>$2000</c>.</summary>
    public required RotationAngle UpperNeckAngle { get; init; }

    /// <summary>Neck movement enabled.</summary>
    public required bool NeckMovementEnabled { get; init; }

    /// <summary>Lower neck movement index selected by the current actor phase.</summary>
    public required MotherBrainNeckMovement LowerNeckMovementIndex { get; init; }

    /// <summary>Upper neck movement index selected by the current actor phase.</summary>
    public required MotherBrainNeckMovement UpperNeckMovementIndex { get; init; }

    /// <summary>Number of live Mother Brain bomb enemy projectiles at body WRAM <c>$7E:802A</c>.</summary>
    public required int BombCounter { get; init; }

    /// <summary>Shared Mother Brain hitbox-enable word cleared at both death boundaries.</summary>
    public required bool HitboxesEnabled { get; init; }
}
