using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
/// <summary>Represents run flags commands verification.</summary>
internal static class RunFlagsCommandsVerification
{

    /// <summary>gameplay message definitions command.</summary>
    [VerificationCommand("--gameplay-message-definitions")]
    internal static int GameplayMessageDefinitionsCommand()
    {
        return 0;
    }

    /// <summary>android host command.</summary>
    [VerificationCommand("--android-host")]
    internal static int AndroidHostCommand()
    {
        return 0;
    }
}
