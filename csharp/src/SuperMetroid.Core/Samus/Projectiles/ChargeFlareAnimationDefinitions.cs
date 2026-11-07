using System.Collections.Frozen;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>NTSC charge-flare cadence and loop commands, independent of sprite composition.</summary>
internal static class ChargeFlareAnimationDefinitions
{
    /// <summary>The main flare, the region's first program: thirty delays followed by rewind fourteen.</summary>
    internal const int MainFlare = 0;
    /// <summary>The flare's slow sparks: six delays followed by restart.</summary>
    internal const int SlowSparks = 32;
    /// <summary>The flare's fast sparks: six delays followed by restart.</summary>
    internal const int FastSparks = 39;
    /// <summary>The bytes the three programs fill, end to end.</summary>
    internal const int RegionByteCount = 46;
    /// <summary>The three words just before the programs, naming the main-flare, slow-spark and fast-spark programs.</summary>
    internal const int ProgramTableByteCount = 6;
    /// <summary>HandleGrappleBeamFlare: counter one force-selects main frame sixteen.</summary>
    internal const int GrappleInitialFrame = 16;
    /// <summary>HandleGrappleBeamFlare: counter one seeds three before the ordinary decrement.</summary>
    internal const int GrappleInitialDelay = 3;
    /// <summary>The stream byte that encodes <see cref="ChargeFlareDelayCommand.Rewind"/>.</summary>
    internal const int RewindByte = 254;

    /// <summary>The stream byte that encodes <see cref="ChargeFlareDelayCommand.Restart"/>.</summary>
    internal const int RestartByte = 255;

    /// <summary>The command each command byte of a delay stream encodes; every other stream byte is a delay.</summary>
    internal static readonly FrozenDictionary<int, ChargeFlareDelayCommand> CommandBytes =
        new Dictionary<int, ChargeFlareDelayCommand>
        {
            [RewindByte] = ChargeFlareDelayCommand.Rewind,
            [RestartByte] = ChargeFlareDelayCommand.Restart,
        }.ToFrozenDictionary();
}
