namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>One sample of a Choot's falling path: where it is from the falling origin, and for how many updates.</summary>
/// <param name="Offset">The Choot's offset from the falling origin, X rightward and Y downward.</param>
/// <param name="Updates">How many updates the Choot stays at the offset, one or more.</param>
internal readonly record struct ChootFallSample(Offset16<RoomPixels> Offset, int Updates);
