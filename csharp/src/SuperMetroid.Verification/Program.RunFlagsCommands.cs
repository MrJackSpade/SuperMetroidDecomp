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
/// <summary>Accepts verification command flags whose current handlers return success without work.</summary>
internal static class RunFlagsCommandsVerification
{

    /// <summary>Recognizes the gameplay-message definition flag and exits successfully without running a suite.</summary>
    [VerificationCommand("--gameplay-message-definitions")]
    internal static int GameplayMessageDefinitionsCommand()
    {
        return 0;
    }

    /// <summary>Recognizes the Android-host flag and exits successfully without running a suite.</summary>
    [VerificationCommand("--android-host")]
    internal static int AndroidHostCommand()
    {
        return 0;
    }
}
