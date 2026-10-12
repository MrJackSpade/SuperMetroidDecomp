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
internal static class RunFlagsCommandsVerification
{

    [VerificationCommand("--gameplay-message-definitions")]
    internal static int GameplayMessageDefinitionsCommand()
    {
        return 0;
    }

    [VerificationCommand("--android-host")]
    internal static int AndroidHostCommand()
    {
        return 0;
    }
}
