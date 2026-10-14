namespace SuperMetroid.Core.Rooms;

/// <summary>The three bank-$84 elevator-platform draw lists, 24 bytes apart.</summary>
internal enum ElevatorPlatformDraw : ushort
{
    /// <summary>First elevator-platform draw list, $84:AA97.</summary>
    First = 0xaa97,
    /// <summary>Second elevator-platform draw list, $84:AAAF.</summary>
    Second = 0xaaaf,
    /// <summary>Third elevator-platform draw list, $84:AAC7.</summary>
    Third = 0xaac7,
}
