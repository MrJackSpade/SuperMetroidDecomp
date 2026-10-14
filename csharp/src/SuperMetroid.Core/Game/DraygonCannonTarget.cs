namespace SuperMetroid.Core.Game;

/// <summary>One bank-$A5 cannon-control word paired with its wall projectile origin.</summary>
internal readonly record struct DraygonCannonTarget(
    DraygonCannonControlWord DisabledWord,
    Position16<RoomPixels> Origin);
