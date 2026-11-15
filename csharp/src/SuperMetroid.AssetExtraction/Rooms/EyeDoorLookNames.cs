using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Names what an eye-door draw shows in its installed picture's identity, and finds it in the eye door's draw tables (#1994).</summary>
internal static class EyeDoorLookNames
{
    /// <summary>The eye's looks, the middle's poses and the bottom's poses, each in the order its draw table lists them.</summary>
    internal static IReadOnlyList<EyeDoorLook> TableOrder { get; } =
    [
        EyeDoorLook.EyeClosed, EyeDoorLook.EyeHalfOpen, EyeDoorLook.EyeOpen, EyeDoorLook.EyeFlinching, EyeDoorLook.EyeOpenSolid,
        EyeDoorLook.MiddleResting, EyeDoorLook.MiddleSwelling, EyeDoorLook.MiddleSwollen,
        EyeDoorLook.BottomResting, EyeDoorLook.BottomSwelling, EyeDoorLook.BottomSwollen,
    ];

    /// <summary>The look's words in an eye door's editable visual identity, after its side.</summary>
    internal static string VisualName(EyeDoorLook look) => look switch
    {
        EyeDoorLook.EyeClosed => "eye-closed",
        EyeDoorLook.EyeHalfOpen => "eye-half-open",
        EyeDoorLook.EyeOpen => "eye-open",
        EyeDoorLook.EyeFlinching => "eye-flinching",
        EyeDoorLook.EyeOpenSolid => "eye-open-solid",
        EyeDoorLook.MiddleResting => "middle-resting",
        EyeDoorLook.MiddleSwelling => "middle-swelling",
        EyeDoorLook.MiddleSwollen => "middle-swollen",
        EyeDoorLook.BottomResting => "bottom-resting",
        EyeDoorLook.BottomSwelling => "bottom-swelling",
        EyeDoorLook.BottomSwollen => "bottom-swollen",
        EyeDoorLook.Clear => throw new InvalidOperationException("The eye door's cleared opening is named by its shared clear's identity."),
        _ => throw new InvalidOperationException($"Undefined {nameof(EyeDoorLook)} {look}."),
    };

    /// <summary>The position of <paramref name="look"/> among its component's records in the cartridge's eye-door table.</summary>
    internal static int TableIndex(EyeDoorLook look) => look switch
    {
        EyeDoorLook.Clear or EyeDoorLook.EyeClosed or EyeDoorLook.MiddleResting or EyeDoorLook.BottomResting => 0,
        EyeDoorLook.EyeHalfOpen or EyeDoorLook.MiddleSwelling or EyeDoorLook.BottomSwelling => 1,
        EyeDoorLook.EyeOpen or EyeDoorLook.MiddleSwollen or EyeDoorLook.BottomSwollen => 2,
        EyeDoorLook.EyeFlinching => 3,
        EyeDoorLook.EyeOpenSolid => 4,
        _ => throw new InvalidDataException($"Undefined {nameof(EyeDoorLook)} {look}."),
    };
}
