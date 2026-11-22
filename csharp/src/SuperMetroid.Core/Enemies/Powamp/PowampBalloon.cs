namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>A Powamp body's balloon half: its enemy place and the state holding its spawn point and typed animation.</summary>
/// <param name="Place">The balloon's enemy place, the literal place before its body.</param>
/// <param name="State">The balloon's state.</param>
internal readonly partial record struct PowampBalloon(RoomEnemyPlace Place, PowampBalloonState State);
