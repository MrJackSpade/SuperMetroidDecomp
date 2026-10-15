
namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>Cartridge literals owned by setup $84:B04A and instructions $84:AD43/$AD58.</summary>
internal static class WreckedShipTreadmillPlmRomData
{
    /// <summary>Hardcoded PLM X block supplied by both bank-$8F door routines.</summary>
    public const byte BlockX = 4;

    /// <summary>Hardcoded PLM Y block supplied by both bank-$8F door routines.</summary>
    public const byte BlockY = 9;

    /// <summary>Number of consecutive row entries written by setup and activation.</summary>
    public const int BlockCount = 0x38;

    /// <summary>Exact blank-air word written synchronously by setup $84:B04A.</summary>
    public const ushort BlankAirWord = 0x00ff;

    /// <summary>Exact type-three treadmill word written after the Phantoon boss-bit test.</summary>
    public const ushort ActiveTreadmillWord = 0x30ff;

    /// <summary>Rightward treadmill BTS written by instruction $84:AD43.</summary>
    public const byte RightwardsBehavior = 0x08;

    /// <summary>Leftward treadmill BTS written by instruction $84:AD58.</summary>
    public const byte LeftwardsBehavior = 0x09;
}
