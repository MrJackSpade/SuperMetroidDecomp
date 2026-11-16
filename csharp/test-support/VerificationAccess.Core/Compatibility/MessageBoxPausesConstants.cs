using System.Collections.Immutable;

namespace SuperMetroid.Core.Compatibility;

/// <summary>Cartridge values and helpers of <see cref="MessageBoxPauses"/> that only verification reads.</summary>
internal static class MessageBoxPausesConstants
{
    /// <summary>The updates gameplay stays paused before an ordinary box's window opens, counting the gameplay update that opened it.</summary>
    public const int UpdatesBeforeOpening = 8;
}
