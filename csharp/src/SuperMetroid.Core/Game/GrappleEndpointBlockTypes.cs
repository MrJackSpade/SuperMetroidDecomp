namespace SuperMetroid.Core.Game;

/// <summary>
/// All sixteen high-nibble room block dispatch values as interpreted by the bank-$94
/// grapple endpoint and swing collision routines. Names describe verified grapple behavior
/// where the broader room-collision role is not yet translated.
/// </summary>
internal static class GrappleEndpointBlockTypes
{
    /// <summary>Ordinary air; endpoint and swing both pass through.</summary>
    public const byte Air = 0x0;
    /// <summary>Slope; grapple body and endpoint both collide.</summary>
    public const byte Slope = 0x1;
    /// <summary>Spike air; endpoint passes through while swing may apply damage.</summary>
    public const byte SpikeAir = 0x2;
    /// <summary>Type-three special air; both grapple dispatchers pass through.</summary>
    public const byte PassThroughSpecialAir = 0x3;
    /// <summary>Shootable air; endpoint may spawn its reaction PLM.</summary>
    public const byte ShootableAir = 0x4;
    /// <summary>Signed-BTS horizontal extension.</summary>
    public const byte HorizontalExtension = 0x5;
    /// <summary>Type-six block; both grapple dispatchers pass through.</summary>
    public const byte PassThroughTypeSix = 0x6;
    /// <summary>Bombable air; grapple cannot activate it and passes through.</summary>
    public const byte BombableAir = 0x7;
    /// <summary>Ordinary solid block.</summary>
    public const byte Solid = 0x8;
    /// <summary>Type-nine solid-family block in the grapple dispatchers.</summary>
    public const byte SolidTypeNine = 0x9;
    /// <summary>Spike solid; swing collision may apply BTS-selected damage.</summary>
    public const byte SpikeSolid = 0xa;
    /// <summary>Type-B solid-family block in the grapple dispatchers.</summary>
    public const byte SolidTypeB = 0xb;
    /// <summary>Shootable solid; endpoint may spawn its reaction PLM.</summary>
    public const byte ShootableSolid = 0xc;
    /// <summary>Signed-BTS vertical extension.</summary>
    public const byte VerticalExtension = 0xd;
    /// <summary>Grapple block with BTS-selected persistent/breakable reaction.</summary>
    public const byte Grapple = 0xe;
    /// <summary>Bombable solid; grapple cannot activate it but still collides.</summary>
    public const byte BombableSolid = 0xf;
}
