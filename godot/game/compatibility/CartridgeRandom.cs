using Engine.Shared;

namespace Game.Compatibility;

/// <summary>
/// The sequence of random numbers this game's outcomes follow: the cartridge's own, so that the same play gives the
/// same drops, hops and attacks. Each number is from 0 through 65535, and the next is five times the last plus 273,
/// worked a half at a time, with one more added when that sum passes 65535.
/// </summary>
public sealed class CartridgeRandom : IRandomSequence
{
    /// <summary>The number the sequence holds when the game is switched on.</summary>
    public const uint PowerOn = 97;

    private uint _number = PowerOn;

    /// <inheritdoc/>
    public uint Current => _number;

    /// <inheritdoc/>
    public void Restart(uint seed)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(seed, 65535u);
        _number = seed;
    }

    /// <inheritdoc/>
    public uint Next()
    {
        uint low = _number % 256, high = _number / 256;
        uint products = low * 5 + (high * 5 % 256) * 256;
        uint raised = products + 256;
        uint past = raised > 65535 ? 1u : 0u;
        _number = (raised + 17 + past) % 65536;
        return _number;
    }

    /// <summary>Swaps the two halves of the number the sequence holds, as the original does while a liquid's surface ripples.</summary>
    public void SwapHalves() => _number = (_number % 256) * 256 + _number / 256;
}
