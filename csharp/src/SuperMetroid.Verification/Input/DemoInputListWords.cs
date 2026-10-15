namespace SuperMetroid.Verification.Input;

/// <summary>How the demo-input ROM oracle (#1300) reads a bank-$91 list.</summary>
internal static class DemoInputListWords
{
    /// <summary>The bank the demo-input objects and lists live in.</summary>
    internal const int ListBank = 0x910000;

    /// <summary>List words from here up are routine addresses; below it, input-record durations.</summary>
    internal const ushort RoutineWord = 0x8000;

    /// <summary>The bytes of one input record: its duration, held and newly pressed words.</summary>
    internal const int InputRecordBytes = 6;

    /// <summary><c>$91:8682</c>, the Baby Metroid discovery list's end routine.</summary>
    internal const ushort EndIntroBabyDiscoveryRoutine = 0x8682;

    /// <summary><c>$91:8739</c>, the Mother Brain flashback list's end routine.</summary>
    internal const ushort EndIntroMotherBrainFlashbackRoutine = 0x8739;

    /// <summary><c>$91:877E</c>, the intro Baby Metroid discovery's demo object.</summary>
    internal const ushort IntroBabyDiscoveryObject = 0x877e;

    /// <summary><c>$91:8784</c>, the intro Mother Brain flashback's demo object.</summary>
    internal const ushort IntroMotherBrainFlashbackObject = 0x8784;
}
