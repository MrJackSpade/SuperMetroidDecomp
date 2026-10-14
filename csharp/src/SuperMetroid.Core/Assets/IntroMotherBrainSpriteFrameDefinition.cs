namespace SuperMetroid.Core.Assets;

/// <summary>Pairs an intro Mother Brain frame's bank-$8C OAM pointer with its editable artwork key.</summary>
internal readonly record struct IntroMotherBrainSpriteFrameDefinition(ushort Pointer, string Name);
