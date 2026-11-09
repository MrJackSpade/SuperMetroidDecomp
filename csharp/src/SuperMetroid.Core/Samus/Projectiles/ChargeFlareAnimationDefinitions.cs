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
    /// <summary>The entries the three programs fill, end to end.</summary>
    internal const int RegionLength = 46;
    /// <summary>The places just before the programs, which name the main-flare, slow-spark and fast-spark programs.</summary>
    internal const int ProgramTableLength = 6;
    /// <summary>The grapple beam's flare: counter one force-selects main frame sixteen.</summary>
    internal const int GrappleInitialFrame = 16;
    /// <summary>The grapple beam's flare: counter one seeds three before the ordinary decrement.</summary>
    internal const int GrappleInitialDelay = 3;
    /// <summary>The stream entry that encodes <see cref="ChargeFlareDelayCommand.Rewind"/>.</summary>
    internal const int RewindCode = 254;

    /// <summary>The stream entry that encodes <see cref="ChargeFlareDelayCommand.Restart"/>.</summary>
    internal const int RestartCode = 255;

    /// <summary>The command each command entry of a delay stream encodes; every other stream entry is a delay.</summary>
    internal static readonly FrozenDictionary<int, ChargeFlareDelayCommand> CommandCodes =
        new Dictionary<int, ChargeFlareDelayCommand>
        {
            [RewindCode] = ChargeFlareDelayCommand.Rewind,
            [RestartCode] = ChargeFlareDelayCommand.Restart,
        }.ToFrozenDictionary();
}
