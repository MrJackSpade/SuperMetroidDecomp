namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The current stage of an ordinary door transition, in the order the transition runs them.</summary>
public abstract partial record DoorTransitionPhase
{

    /// <summary>No door transition has begun.</summary>
    public sealed partial record Inactive : DoorTransitionPhase;

    /// <summary>The transition is finished and ordinary gameplay may resume.</summary>
    public sealed partial record Complete : DoorTransitionPhase;

    /// <summary>The phases of a door transition under way, which only the transition itself holds.</summary>
    public abstract partial record Running : DoorTransitionPhase
    {

        /// <summary>Wait for all three sound-effect queues to drain.</summary>
        public sealed partial record WaitForSoundQueues : Running;

        /// <summary>Fade the source room out while the HUD keeps its colours.</summary>
        public sealed partial record FadeOutSourceRoom : Running;

        /// <summary>Stop the room liquid's wave and the statues' descent and give the screen to the door transition's display.</summary>
        public sealed partial record BeginDoorDisplay : Running;

        /// <summary>Move the source camera one pixel per update to the required screen boundary.</summary>
        public sealed partial record AlignSourceCamera : Running;

        /// <summary>Spend the destination-setup wait's updates (<see cref="DoorDestinationSetupWait"/>).</summary>
        public sealed partial record DestinationSetupWait : Running;

        /// <summary>Move Samus and the camera by the door direction's step to where the door's scroll starts.</summary>
        public sealed partial record MoveToDoorScrollStart : Running;

        /// <summary>Place Samus in the destination room's coordinates and take the scroll's first step.</summary>
        public sealed partial record PlaceSamusInDestination : Running;

        /// <summary>Load the destination room and begin its door-opening scroll.</summary>
        public sealed partial record LoadDestinationRoom : Running;

        /// <summary>Wait for the incremental destination door scroll to finish.</summary>
        public sealed partial record WaitForDoorOpeningScroll : Running;

        /// <summary>Wait one update after the destination's room objects run, then align Samus with the doorway.</summary>
        public sealed partial record AlignSamusWithDoorway : Running;

        /// <summary>Run the destination's block animations once after loading.</summary>
        public sealed partial record StepDestinationBlockAnimations : Running;

        /// <summary>Wait for the global music queue to drain before selecting room music.</summary>
        public sealed partial record WaitForMusicQueue : Running;

        /// <summary>Finish the opening scroll and return the screen to the destination room's display.</summary>
        public sealed partial record FinishDoorOpeningScroll : Running;

        /// <summary>Build the first destination actor frame before applying a fade step.</summary>
        public sealed partial record DrawFirstDestinationFrame : Running;

        /// <summary>Run destination actors while fading the loaded room in.</summary>
        public sealed partial record FadeInDestinationRoom : Running;
    }
}
