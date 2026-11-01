namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Thresholds for aborting and recovering from a Norfair Ridley lunge.</summary>
public static class RidleyLungeDefinitions
{
    /// <summary>RidleyLungeMissedSamus: ten zero-health lunges end in the death roar.</summary>
    public const int DeathLungeLimit = 10;
    /// <summary>Abandon after passing Samus by 32 pixels.</summary>
    public const int PassDistance = 32;
    /// <summary>Claw height below the body origin.</summary>
    public const int ClawHeight = 35;
    /// <summary>Target left of a right-side bomb.</summary>
    public const int DodgeLeftX = 80;
    /// <summary>Target right of a left-side bomb.</summary>
    public const int DodgeRightX = 192;
    /// <summary>Bomb X dividing line.</summary>
    public const int DodgeSplitX = 128;
    /// <summary>Bomb Y dividing line.</summary>
    public const int DodgeSplitY = 256;
    /// <summary>Target above a low bomb.</summary>
    public const int DodgeUpperY = 192;
    /// <summary>Target below a high bomb.</summary>
    public const int DodgeLowerY = 384;
}
