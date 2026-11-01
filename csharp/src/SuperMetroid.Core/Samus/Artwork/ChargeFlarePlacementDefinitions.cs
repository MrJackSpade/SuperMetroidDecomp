
namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Visual flare placement resource contract, including low-nibble overread selections.</summary>
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
        /// <summary>Standing turn aiming up or diagonally up.</summary>
        Up,
        /// <summary>Standing turn with neutral aim.</summary>
        Neutral,
        /// <summary>Standing turn aiming down or diagonally down.</summary>
        Down,
    }
    /// <summary>The standing turn rows in table order after the named aiming directions.</summary>
    internal static readonly StandingTurnDirection[] StandingTurnRows =
        [StandingTurnDirection.Up, StandingTurnDirection.Neutral, StandingTurnDirection.Down];
    /// <summary>The standing rows have thirteen named origins; the running rows have ten.</summary>
    internal const int StandingDirectionCount = 13;
}
