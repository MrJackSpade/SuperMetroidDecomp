namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>Where a HUD ammo counter is drawn and how many digits it shows.</summary>
/// <param name="Anchor">The anchor.</param>
/// <param name="Digits">The digits.</param>
internal readonly record struct HudAmmoField(Position16<TilemapCells> Anchor, int Digits);
