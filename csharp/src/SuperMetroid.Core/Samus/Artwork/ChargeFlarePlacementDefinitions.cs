
namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Visual flare placement resource contract, including native low-nibble overread selections.</summary>
public static class ChargeFlarePlacementDefinitions
{
    /// <summary>JSON resource filename for beam-charge flare drawing offsets; editing these signed pixel placements does not change physical projectile origins or charge timing.</summary>
    public const string FileName = "charge-flare-placement.json";
    /// <summary>Required revision of the shared beam/Grapple placement schema, containing all sixteen standing and sixteen running direction keys.</summary>
    public const int Version = 1;
    /// <summary>The renderer retains all four direction bits, including values beyond named directions.</summary>
    public const int DirectionCount = 16;
    /// <summary>The three standing turn rows that follow the ten named aiming directions.</summary>
    internal enum StandingTurnDirection
    {
        /// <summary>$90:C1BC/C1D6: standing turn aiming up or diagonally up.</summary>
        Up,
        /// <summary>$90:C1BE/C1D8: standing turn with neutral aim.</summary>
        Neutral,
        /// <summary>$90:C1C0/C1DA: standing turn aiming down or diagonally down.</summary>
        Down,
    }
    /// <summary>The standing turn rows in table order after the named aiming directions.</summary>
    internal static readonly StandingTurnDirection[] StandingTurnRows =
        [StandingTurnDirection.Up, StandingTurnDirection.Neutral, StandingTurnDirection.Down];
    /// <summary>$90:C1A8/C1C2 have thirteen named standing origins; running rows C1DC/C1F0 have ten.</summary>
    internal const int StandingDirectionCount = 13;
}
