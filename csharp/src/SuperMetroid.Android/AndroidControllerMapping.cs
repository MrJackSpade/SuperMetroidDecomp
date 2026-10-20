using System.Diagnostics.CodeAnalysis;
using Android.Views;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Android;

/// <summary>
/// Retroid Classic's Nintendo-layout key mapping, separate from cartridge bindings.
/// Player testing confirmed this device reports face-button names, not Xbox positions:
/// A is accept/jump, B is back/dash, X is fire, and Y is item cancel.
/// </summary>
internal static class AndroidControllerMapping
{
    /// <summary>Remappable gameplay keys; Back and Mode remain reserved for host tools.</summary>
    /// <remarks>
    /// Issues #625 and #951: this twelve-entry order is the controller-settings row order,
    /// indexed only by the dialog's selected row (0..11). It lists four directions, four
    /// Retroid face buttons, L1/R1, Start, and Select exactly once. The corresponding
    /// default button for each row is Map(key); this is one logical host policy with two
    /// views, not a cartridge table. Retain the explicit order because it is presentation
    /// and remapping policy, not a numerical sequence to generate.
    /// </remarks>
    public static readonly Keycode[] Keys =
    [
        Keycode.DpadLeft, Keycode.DpadRight, Keycode.DpadUp, Keycode.DpadDown,
        Keycode.ButtonA, Keycode.ButtonB, Keycode.ButtonX, Keycode.ButtonY,
        Keycode.ButtonL1, Keycode.ButtonR1, Keycode.ButtonStart, Keycode.ButtonSelect,
    ];

    /// <summary>Returns the Retroid Classic default SNES button for an Android key.</summary>
    /// <remarks>
    /// Issues #625 and #951: the twelve Keys cases are one-to-one with same-named SNES
    /// directions, A/B/X/Y, L/R, Start, and Select. Every other Keycode maps to None;
    /// Back and Mode are reserved by MainActivity for host controls. Settings and key
    /// events both pass this value as the fallback to AndroidControllerPreferences.Resolve,
    /// so a saved per-key override takes priority. Device testing established the face
    /// names; no ROM revision or native address applies. An arithmetic enum cast would
    /// depend on unrelated Android/SNES numeric identities and lose the explicit default
    /// domain, so retain this small authored selector.
    /// </remarks>
    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility",
        Justification = "The switch names every Keycode so that it stays exhaustive; codes from later Android API levels are only compared as constant values, which is valid on every supported version.")]
    public static SnesButton Map(Keycode code) => code switch
    {
        Keycode.DpadLeft => SnesButton.Left,
        Keycode.DpadRight => SnesButton.Right,
        Keycode.DpadUp => SnesButton.Up,
        Keycode.DpadDown => SnesButton.Down,
        Keycode.ButtonA => SnesButton.A,
        Keycode.ButtonB => SnesButton.B,
        Keycode.ButtonX => SnesButton.X,
        Keycode.ButtonY => SnesButton.Y,
        Keycode.ButtonL1 => SnesButton.L,
        Keycode.ButtonR1 => SnesButton.R,
        Keycode.ButtonStart => SnesButton.Start,
        Keycode.ButtonSelect => SnesButton.Select,
        Keycode.Num0 or
            Keycode.Num1 or
            Keycode.K11 or
            Keycode.K12 or
            Keycode.Num2 or
            Keycode.Num3 or
            Keycode.ThreeDMode or
            Keycode.Num4 or
            Keycode.Num5 or
            Keycode.Num6 or
            Keycode.Num7 or
            Keycode.Num8 or
            Keycode.Num9 or
            Keycode.A or
            Keycode.AllApps or
            Keycode.AltLeft or
            Keycode.AltRight or
            Keycode.Apostrophe or
            Keycode.AppSwitch or
            Keycode.Assist or
            Keycode.At or
            Keycode.AvrInput or
            Keycode.AvrPower or
            Keycode.B or
            Keycode.Back or
            Keycode.Backslash or
            Keycode.Bookmark or
            Keycode.Break or
            Keycode.BrightnessDown or
            Keycode.BrightnessUp or
            Keycode.Button1 or
            Keycode.Button10 or
            Keycode.Button11 or
            Keycode.Button12 or
            Keycode.Button13 or
            Keycode.Button14 or
            Keycode.Button15 or
            Keycode.Button16 or
            Keycode.Button2 or
            Keycode.Button3 or
            Keycode.Button4 or
            Keycode.Button5 or
            Keycode.Button6 or
            Keycode.Button7 or
            Keycode.Button8 or
            Keycode.Button9 or
            Keycode.ButtonC or
            Keycode.ButtonL2 or
            Keycode.ButtonMode or
            Keycode.ButtonR2 or
            Keycode.ButtonThumbl or
            Keycode.ButtonThumbr or
            Keycode.ButtonZ or
            Keycode.C or
            Keycode.Calculator or
            Keycode.Calendar or
            Keycode.Call or
            Keycode.Camera or
            Keycode.CapsLock or
            Keycode.Captions or
            Keycode.ChannelDown or
            Keycode.ChannelUp or
            Keycode.Clear or
            Keycode.Close or
            Keycode.Comma or
            Keycode.Contacts or
            Keycode.Copy or
            Keycode.CtrlLeft or
            Keycode.CtrlRight or
            Keycode.Cut or
            Keycode.D or
            Keycode.Del or
            Keycode.DemoApp1 or
            Keycode.DemoApp2 or
            Keycode.DemoApp3 or
            Keycode.DemoApp4 or
            Keycode.Dictate or
            Keycode.DoNotDisturb or
            Keycode.DpadCenter or
            Keycode.DpadDownLeft or
            Keycode.DpadDownRight or
            Keycode.DpadUpLeft or
            Keycode.DpadUpRight or
            Keycode.Dvr or
            Keycode.E or
            Keycode.Eisu or
            Keycode.EmojiPicker or
            Keycode.Endcall or
            Keycode.Enter or
            Keycode.Envelope or
            Keycode.Equals or
            Keycode.Escape or
            Keycode.Explorer or
            Keycode.F or
            Keycode.F1 or
            Keycode.F10 or
            Keycode.F11 or
            Keycode.F12 or
            Keycode.F13 or
            Keycode.F14 or
            Keycode.F15 or
            Keycode.F16 or
            Keycode.F17 or
            Keycode.F18 or
            Keycode.F19 or
            Keycode.F2 or
            Keycode.F20 or
            Keycode.F21 or
            Keycode.F22 or
            Keycode.F23 or
            Keycode.F24 or
            Keycode.F3 or
            Keycode.F4 or
            Keycode.F5 or
            Keycode.F6 or
            Keycode.F7 or
            Keycode.F8 or
            Keycode.F9 or
            Keycode.FeaturedApp1 or
            Keycode.FeaturedApp2 or
            Keycode.FeaturedApp3 or
            Keycode.FeaturedApp4 or
            Keycode.Focus or
            Keycode.Forward or
            Keycode.ForwardDel or
            Keycode.Fullscreen or
            Keycode.Function or
            Keycode.G or
            Keycode.Grave or
            Keycode.Guide or
            Keycode.H or
            Keycode.Headsethook or
            Keycode.Help or
            Keycode.Henkan or
            Keycode.Home or
            Keycode.I or
            Keycode.Info or
            Keycode.Insert or
            Keycode.J or
            Keycode.K or
            Keycode.Kana or
            Keycode.KatakanaHiragana or
            Keycode.KeyboardBacklightDown or
            Keycode.KeyboardBacklightToggle or
            Keycode.KeyboardBacklightUp or
            Keycode.L or
            Keycode.LanguageSwitch or
            Keycode.LastChannel or
            Keycode.LeftBracket or
            Keycode.Lock or
            Keycode.M or
            Keycode.Macro1 or
            Keycode.Macro2 or
            Keycode.Macro3 or
            Keycode.Macro4 or
            Keycode.MannerMode or
            Keycode.MediaAudioTrack or
            Keycode.MediaClose or
            Keycode.MediaEject or
            Keycode.MediaFastForward or
            Keycode.MediaNext or
            Keycode.MediaPause or
            Keycode.MediaPlay or
            Keycode.MediaPlayPause or
            Keycode.MediaPrevious or
            Keycode.MediaRecord or
            Keycode.MediaRewind or
            Keycode.MediaSkipBackward or
            Keycode.MediaSkipForward or
            Keycode.MediaStepBackward or
            Keycode.MediaStepForward or
            Keycode.MediaStop or
            Keycode.MediaTopMenu or
            Keycode.Menu or
            Keycode.MetaLeft or
            Keycode.MetaRight or
            Keycode.Minus or
            Keycode.MoveEnd or
            Keycode.MoveHome or
            Keycode.Muhenkan or
            Keycode.Music or
            Keycode.Mute or
            Keycode.N or
            Keycode.NavigateIn or
            Keycode.NavigateNext or
            Keycode.NavigateOut or
            Keycode.NavigatePrevious or
            Keycode.New or
            Keycode.Notification or
            Keycode.Num or
            Keycode.NumLock or
            Keycode.Numpad0 or
            Keycode.Numpad1 or
            Keycode.Numpad2 or
            Keycode.Numpad3 or
            Keycode.Numpad4 or
            Keycode.Numpad5 or
            Keycode.Numpad6 or
            Keycode.Numpad7 or
            Keycode.Numpad8 or
            Keycode.Numpad9 or
            Keycode.NumpadAdd or
            Keycode.NumpadComma or
            Keycode.NumpadDivide or
            Keycode.NumpadDot or
            Keycode.NumpadEnter or
            Keycode.NumpadEquals or
            Keycode.NumpadLeftParen or
            Keycode.NumpadMultiply or
            Keycode.NumpadRightParen or
            Keycode.NumpadSubtract or
            Keycode.O or
            Keycode.P or
            Keycode.PageDown or
            Keycode.PageUp or
            Keycode.Pairing or
            Keycode.Paste or
            Keycode.Period or
            Keycode.Pictsymbols or
            Keycode.Plus or
            Keycode.Pound or
            Keycode.Power or
            Keycode.Print or
            Keycode.ProfileSwitch or
            Keycode.ProgBlue or
            Keycode.ProgGreen or
            Keycode.ProgRed or
            Keycode.ProgYellow or
            Keycode.Q or
            Keycode.R or
            Keycode.RecentApps or
            Keycode.Refresh or
            Keycode.RightBracket or
            Keycode.Ro or
            Keycode.S or
            Keycode.Screenshot or
            Keycode.ScrollLock or
            Keycode.Search or
            Keycode.Semicolon or
            Keycode.Settings or
            Keycode.ShiftLeft or
            Keycode.ShiftRight or
            Keycode.Slash or
            Keycode.Sleep or
            Keycode.SoftLeft or
            Keycode.SoftRight or
            Keycode.SoftSleep or
            Keycode.Space or
            Keycode.Star or
            Keycode.StbInput or
            Keycode.StbPower or
            Keycode.Stem1 or
            Keycode.Stem2 or
            Keycode.Stem3 or
            Keycode.StemPrimary or
            Keycode.StylusButtonPrimary or
            Keycode.StylusButtonSecondary or
            Keycode.StylusButtonTail or
            Keycode.StylusButtonTertiary or
            Keycode.SwitchCharset or
            Keycode.Sym or
            Keycode.Sysrq or
            Keycode.SystemNavigationDown or
            Keycode.SystemNavigationLeft or
            Keycode.SystemNavigationRight or
            Keycode.SystemNavigationUp or
            Keycode.T or
            Keycode.Tab or
            Keycode.ThumbsDown or
            Keycode.ThumbsUp or
            Keycode.Tv or
            Keycode.TvAntennaCable or
            Keycode.TvAudioDescription or
            Keycode.TvAudioDescriptionMixDown or
            Keycode.TvAudioDescriptionMixUp or
            Keycode.TvContentsMenu or
            Keycode.TvDataService or
            Keycode.TvInput or
            Keycode.TvInputComponent1 or
            Keycode.TvInputComponent2 or
            Keycode.TvInputComposite1 or
            Keycode.TvInputComposite2 or
            Keycode.TvInputHdmi1 or
            Keycode.TvInputHdmi2 or
            Keycode.TvInputHdmi3 or
            Keycode.TvInputHdmi4 or
            Keycode.TvInputVga1 or
            Keycode.TvMediaContextMenu or
            Keycode.TvNetwork or
            Keycode.TvNumberEntry or
            Keycode.TvPower or
            Keycode.TvRadioService or
            Keycode.TvSatellite or
            Keycode.TvSatelliteBs or
            Keycode.TvSatelliteCs or
            Keycode.TvSatelliteService or
            Keycode.TvTeletext or
            Keycode.TvTerrestrialAnalog or
            Keycode.TvTerrestrialDigital or
            Keycode.TvTimerProgramming or
            Keycode.TvZoomMode or
            Keycode.U or
            Keycode.Unknown or
            Keycode.V or
            Keycode.VideoApp1 or
            Keycode.VideoApp2 or
            Keycode.VideoApp3 or
            Keycode.VideoApp4 or
            Keycode.VideoApp5 or
            Keycode.VideoApp6 or
            Keycode.VideoApp7 or
            Keycode.VideoApp8 or
            Keycode.VoiceAssist or
            Keycode.VolumeDown or
            Keycode.VolumeMute or
            Keycode.VolumeUp or
            Keycode.W or
            Keycode.Wakeup or
            Keycode.Window or
            Keycode.X or
            Keycode.Y or
            Keycode.Yen or
            Keycode.Z or
            Keycode.ZenkakuHankaku or
            Keycode.ZoomIn or
            Keycode.ZoomOut => SnesButton.None,
        _ => SnesButton.None,
    };
}
