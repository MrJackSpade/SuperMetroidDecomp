namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The Speed Booster escape's lavaquake (#1554). It starts watching on its second update. While it waits for the Speed
/// Booster, an update on which Samus lacks it stops the lava and the earthquake and removes the lavaquake, and one on
/// which the lava has no target only removes it; otherwise the lava starts rising and it waits, from the next update,
/// for Samus to run far enough left, which starts the lava moving. From the update after that, each update on which
/// Samus is at or left of the next rise's line raises the lava to that rise; once Samus has passed every rise, each
/// update marks the escape outrun.
/// </summary>
/// <param name="owner">The room's escape owner, which reaches Samus, the lava, the earthquake and the events.</param>
[RoomLifetime]
internal sealed class SpeedBoosterEscapeObject(SpeedBoosterEscapeObjects owner) : RoomObjectBehaviour
{
    /// <summary>The room's escape owner.</summary>
    private readonly SpeedBoosterEscapeObjects _owner = Ensure.NotNull(owner);

    /// <summary>Where the lavaquake is in its course.</summary>
    internal SpeedBoosterEscapePhase Phase { get; private set; } = SpeedBoosterEscapePhase.Starting;

    /// <summary>How many of the escape's rises Samus has triggered, 0 through the number of rises.</summary>
    internal int RisesPassed { get; private set; }

    /// <summary>Whether Samus has triggered every rise, which ends the escape.</summary>
    internal bool IsEscaped => RisesPassed == SpeedBoosterEscapeStageDefinitions.Stages.Length;

    /// <inheritdoc/>
    internal override void Update(RoomObjectSlot place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        switch (Phase)
        {
            case SpeedBoosterEscapePhase.Starting:
                Phase = SpeedBoosterEscapePhase.WaitingForSpeedBooster;
                return;
            case SpeedBoosterEscapePhase.WaitingForSpeedBooster:
                if (_owner.TryStartLavaquake())
                    Phase = SpeedBoosterEscapePhase.WaitingForSamusLeft;
                else
                    // A removed lavaquake leaves nothing at its place, as its setup's removal does.
                    place.Delete();
                return;
            case SpeedBoosterEscapePhase.WaitingForSamusLeft:
                if (_owner.TryStartLavaMoving())
                    Phase = SpeedBoosterEscapePhase.AdvancingLava;
                return;
            case SpeedBoosterEscapePhase.AdvancingLava when IsEscaped:
                _owner.MarkEscaped();
                return;
            case SpeedBoosterEscapePhase.AdvancingLava:
                if (_owner.TryRaiseLava(SpeedBoosterEscapeStageDefinitions.Stages[RisesPassed]))
                    RisesPassed++;
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(SpeedBoosterEscapePhase)} {Phase}.");
        }
    }
}
