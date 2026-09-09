namespace SuperMetroid.Core.Game;

/// <summary>Cartridge data used by the live $A9:AEE1-$B33C death/escape handoff.</summary>
public static class MotherBrainDeathRomData
{
    /// <summary>$AD:E9E8 pointer table, fourteen-color body fades followed by a null word.</summary>
    public const int BodyFadeTable = 0xade9e8;
    /// <summary>$AD:F107 pointer table, fifteen-color decapitated-head fades followed by null.</summary>
    public const int CorpseFadeTable = 0xadf107;
    /// <summary>Bank $AD contains both pointed-to death palette sequences.</summary>
    public const int PaletteBank = 0xad0000;
    /// <summary>$A9:AF77 copies the live brain colors beginning at CGRAM entry 145.</summary>
    public const int BrainColors = 145;
    /// <summary>$A9:AF7D copies those fifteen colors to sprite palette seven, skipping color zero.</summary>
    public const int CorpseColors = 241;
    /// <summary>$AD:E9CC background palette two's first opaque color.</summary>
    public const int BodyColors = 65;
    /// <summary>$AD:E9DD back-leg palette three's first opaque color.</summary>
    public const int LegColors = 177;
    /// <summary>$AD:E9C9 copies fourteen colors per body/leg segment.</summary>
    public const int BodyColorCount = 14;
    /// <summary>$AD:F102 copies fifteen colors into the corpse palette.</summary>
    public const int CorpseColorCount = 15;
    /// <summary>$A9:AFE0 last byte offset cleared in the enemy BG2 staging image, inclusively.</summary>
    public const ushort Bg2LastByte = 710;
    /// <summary>$A9:AFEC empty enemy BG2 tile written after the body fade terminates.</summary>
    public const ushort EmptyBodyTile = 0x0338;
    /// <summary>$7E:2000, native enemy BG2 staging image shared with the extended tilemap writer.</summary>
    public const int Bg2WorkAddress = 0x7e2000;
    /// <summary>$86:CB13, body-relative smoky/mixed death explosions.</summary>
    public const ushort ExplosionDefinition = 0xcb13;
    /// <summary>$86:C914, reattaches a death explosion to the current body position each frame.</summary>
    public const ushort ExplosionPreInstruction = 0xc914;
    /// <summary>$86:C929, animation pointers indexed by the explosion spawn parameter.</summary>
    public const int ExplosionLists = 0x86c929;
    /// <summary>$A9:B203 loads music data index $24 through the delayed queue.</summary>
    public const byte EscapeMusicData = 0x24;
    /// <summary>$A9:B280 starts track seven through the eight-frame-delay music queue.</summary>
    public const byte EscapeMusicTrack = 7;
    /// <summary>$A9:9534 fourteen colors installed for the exploded escape door.</summary>
    public const int DoorPalette = 0xa99534;
    /// <summary>$86:CB21, eight fragments emitted when the escape door opens.</summary>
    public const ushort DoorFragmentDefinition = 0xcb21;
    /// <summary>$86:C9D2, fragment drag, gravity, and thirty-three-call lifetime.</summary>
    public const ushort DoorFragmentPreInstruction = 0xc9d2;
    /// <summary>$86:C992 interleaved signed X/Y offsets for eight door fragments.</summary>
    public const int DoorFragmentOffsets = 0x86c992;
    /// <summary>$86:C9B2 interleaved 8.8 X/Y velocities for eight door fragments.</summary>
    public const int DoorFragmentVelocities = 0x86c9b2;
    /// <summary>$86:C98B seed for the signed-underflow fragment lifetime.</summary>
    public const ushort DoorFragmentLifetime = 32;
    /// <summary>$86:C9D7 drag magnitude subtracted from the 8.8 horizontal speed.</summary>
    public const int DoorFragmentDrag = 16;
    /// <summary>$86:C9ED gravity added to the 8.8 vertical speed.</summary>
    public const int DoorFragmentGravity = 32;
    /// <summary>$86:C96F/$C979 fragment spawn center.</summary>
    public const int DoorX = 16, DoorY = 128;
    /// <summary>$86:CA0D final dust is four pixels above the fragment.</summary>
    public const int DoorDustYOffset = 4;
    /// <summary>$86:CA1B dust-cloud spawn parameter on fragment expiry.</summary>
    public const ushort DoorDustParameter = 9;
    /// <summary>$86:CBBB optional Japanese time-bomb subtitle.</summary>
    public const ushort SubtitleDefinition = 0xcbbb;
    /// <summary>$86:CAFA pins the subtitle to physical screen coordinates.</summary>
    public const ushort SubtitlePreInstruction = 0xcafa;
    /// <summary>$86:CB03/$CB09 subtitle screen anchor.</summary>
    public const int SubtitleX = 128, SubtitleY = 192;
    /// <summary>$84:B5FB: door collision type with door-list index one.</summary>
    public const ushort EscapeDoorCollision = 0x9001;
    /// <summary>$84:B60A: vertical extension looking one block upward.</summary>
    public const ushort EscapeDoorExtension = 0xd0ff;
}
