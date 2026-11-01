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

    /// <summary>Exact body enemy property set touched by the death and its fade. Keeping the raw flags avoids guessing names for engine-wide enemy-property bits before enemy processing is fully translated: death makes the body intangible, then fade completion hides it and clears its processing property with the game's literal OR/AND sequence.</summary>
    public required SuperMetroid.Core.Enemies.EnemyProperties BodyProperties { get; init; }

    /// <summary>Second body property word cleared when the faded body becomes non-interactive.</summary>
    public required SuperMetroid.Core.Enemies.EnemyExtraProperties BodyExtraProperties { get; init; }

    /// <summary>Mother Brain brain-slot room position used by the final beam's aim vector.</summary>
    public required Position16<RoomPixels> Brain { get; init; }

    /// <summary>Brain-slot health rewritten to 36,000 when corpse state one is published.</summary>
    public required int BrainHealth { get; init; }

    /// <summary>Exact brain enemy property set; death/rotting makes it intangible and hidden.</summary>
    public required SuperMetroid.Core.Enemies.EnemyProperties BrainProperties { get; init; }

    /// <summary>Second brain property word cleared when the rot animation completes.</summary>
    public required SuperMetroid.Core.Enemies.EnemyExtraProperties BrainExtraProperties { get; init; }

    /// <summary>Lower neck angle updated by the brain-slot handler. The final beam has already lowered it to three sixteenths of a turn before the Baby interrupts the body.</summary>
    public required RotationAngle LowerNeckAngle { get; init; }

    /// <summary>Upper neck angle, correspondingly lowered to an eighth of a turn.</summary>
    public required RotationAngle UpperNeckAngle { get; init; }

    /// <summary>Neck movement enabled.</summary>
    public required bool NeckMovementEnabled { get; init; }

    /// <summary>Lower neck movement index selected by the current actor phase.</summary>
    public required MotherBrainNeckMovement LowerNeckMovementIndex { get; init; }

    /// <summary>Upper neck movement index selected by the current actor phase.</summary>
    public required MotherBrainNeckMovement UpperNeckMovementIndex { get; init; }

    /// <summary>Number of live Mother Brain bomb enemy projectiles.</summary>
    public required int BombCounter { get; init; }

    /// <summary>Shared Mother Brain hitbox-enable word cleared at both death boundaries.</summary>
    public required bool HitboxesEnabled { get; init; }
}
