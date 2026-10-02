namespace SuperMetroid.Core.Game;

/// <summary>Cartridge thresholds for aborting and recovering from a Norfair Ridley lunge.</summary>
public static class RidleyLungeDefinitions
{
    /// <summary>$A6:BA89, RidleyLungeMissedSamus: ten zero-health lunges end in the death roar.</summary>
    public const int DeathLungeLimit = 10;
    /// <summary>$A6:BAE2, Function_Ridley_Action_Lunge: abandon after passing Samus by 32 pixels.</summary>
    public const int PassDistance = 32;
    /// <summary>$A6:BAEB, Function_Ridley_Action_Lunge: claw height below the body origin.</summary>
    public const int ClawHeight = 35;
    /// <summary>$A6:BD5A, Function_Ridley_DodgingPowerBomb: target left of a right-side bomb.</summary>
    public const ushort DodgeLeftX = 80;
    /// <summary>$A6:BD65, Function_Ridley_DodgingPowerBomb: target right of a left-side bomb.</summary>
    public const ushort DodgeRightX = 192;
    /// <summary>$A6:BD60, Function_Ridley_DodgingPowerBomb: bomb X dividing line.</summary>
    public const int DodgeSplitX = 128;
    /// <summary>$A6:BD70, Function_Ridley_DodgingPowerBomb: bomb Y dividing line.</summary>
    public const int DodgeSplitY = 256;
    /// <summary>$A6:BD6A, Function_Ridley_DodgingPowerBomb: target above a low bomb.</summary>
    public const ushort DodgeUpperY = 192;
    /// <summary>$A6:BD75, Function_Ridley_DodgingPowerBomb: target below a high bomb.</summary>
    public const ushort DodgeLowerY = 384;
}
