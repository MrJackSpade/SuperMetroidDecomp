namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>The Wrecked Ship entrance treadmill's row of blocks: where it lies and the blocks it writes.</summary>
internal static class WreckedShipTreadmillBlockDefinitions
{
    /// <summary>The treadmill row's first block column, the same in both doors' setup.</summary>
    public const byte BlockX = 4;

    /// <summary>The treadmill row's block row, the same in both doors' setup.</summary>
    public const byte BlockY = 9;

    /// <summary>The consecutive blocks in the row that setup and activation write.</summary>
    public const int BlockCount = 56;

    /// <summary>The exact blank-air level word setup writes while Phantoon lives.</summary>
    public const ushort BlankAirWord = 0x00ff;

    /// <summary>The exact treadmill level word written once Phantoon is dead.</summary>
    public const ushort ActiveTreadmillWord = 0x30ff;

    /// <summary>The block behaviour that carries Samus rightwards.</summary>
    public const byte RightwardsBehavior = 0x08;

    /// <summary>The block behaviour that carries Samus leftwards.</summary>
    public const byte LeftwardsBehavior = 0x09;
}
