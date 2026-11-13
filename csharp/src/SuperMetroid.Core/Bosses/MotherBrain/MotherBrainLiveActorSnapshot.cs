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

    /// <summary>How the body meets Samus and her shots; death makes it ignore Samus.</summary>
    public required SuperMetroid.Core.Enemies.EnemyContact BodyContact { get; init; }

    /// <summary>What the enemy passes run for the body; fade completion stops its animation.</summary>
    public required SuperMetroid.Core.Enemies.EnemyProcessing BodyProcessing { get; init; }

    /// <summary>Whether the body is drawn and still occupies its slot; fade completion hides it.</summary>
    public required SuperMetroid.Core.Enemies.EnemyPresence BodyPresence { get; init; }

    /// <summary>Whether the body collides by its frames' hitboxes; cleared when the faded body becomes non-interactive.</summary>
    public required bool BodyCollidesByFrameHitboxes { get; init; }

    /// <summary>Whether the body's animation just installed a frame; cleared when the faded body becomes non-interactive.</summary>
    public required bool BodyShowsNewFrame { get; init; }

    /// <summary>Mother Brain brain-slot room position used by the final beam's aim vector.</summary>
    public required Position16<RoomPixels> Brain { get; init; }

    /// <summary>Brain-slot health rewritten to 36,000 when corpse state one is published.</summary>
    public required int BrainHealth { get; init; }

    /// <summary>How the brain meets Samus and her shots; death makes it ignore Samus.</summary>
    public required SuperMetroid.Core.Enemies.EnemyContact BrainContact { get; init; }

    /// <summary>What the enemy passes run for the brain; the end of rotting stops its animation.</summary>
    public required SuperMetroid.Core.Enemies.EnemyProcessing BrainProcessing { get; init; }

    /// <summary>Whether the brain is drawn and still occupies its slot; the end of rotting hides it.</summary>
    public required SuperMetroid.Core.Enemies.EnemyPresence BrainPresence { get; init; }

    /// <summary>Whether the brain collides by its frames' hitboxes; cleared when the rot animation completes.</summary>
    public required bool BrainCollidesByFrameHitboxes { get; init; }

    /// <summary>Whether the brain's animation just installed a frame; cleared when the rot animation completes.</summary>
    public required bool BrainShowsNewFrame { get; init; }

    /// <summary>Lower neck angle updated by the brain-slot handler. The final beam has already lowered it to three sixteenths of a turn before the Baby interrupts the body.</summary>
    public required RotationAngle LowerNeckAngle { get; init; }

    /// <summary>Upper neck angle, correspondingly lowered to an eighth of a turn.</summary>
    public required RotationAngle UpperNeckAngle { get; init; }

    /// <summary>Neck movement enabled.</summary>
    public required bool NeckMovementEnabled { get; init; }

    /// <summary>The lower neck movement selected by the current actor phase.</summary>
    public required MotherBrainNeckMovement LowerNeckMovement { get; init; }

    /// <summary>The upper neck movement selected by the current actor phase.</summary>
    public required MotherBrainNeckMovement UpperNeckMovement { get; init; }

    /// <summary>Number of live Mother Brain bomb enemy projectiles.</summary>
    public required int BombCounter { get; init; }

    /// <summary>Shared Mother Brain hitbox-enable flag cleared at both death boundaries.</summary>
    public required bool HitboxesEnabled { get; init; }
}
