namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The shape and colour structure of Mother Brain's room colours: the fake-death room flash, the final room, phase two's
/// attack and rear-leg colours, the room-entry glass and tube colours and the phase-three room lights.
/// </summary>
public static class MotherBrainRoomColorDefinitions
{
    /// <summary>The colours in each of a room-flash image's two slices.</summary>
    public const int SliceColors = 12;

    /// <summary>The colours of phase two's attack and rear-leg images, after their transparent first colour.</summary>
    public const int PhaseTwoColors = 15;

    /// <summary>The colours of the room-entry glass-shard and tube-projectile images, after their transparent first colour.</summary>
    public const int InitialColors = 15;

    /// <summary>The phase-three room-light images, shown after the Baby Metroid cutscene.</summary>
    public const int RecoveryLightsFrames = 7;

    /// <summary>The colours in each of a room-light image's two slices.</summary>
    public const int RecoveryLightsSliceColors = 14;

    /// <summary>The attack colours' cyan ramp: its first colour and its three colours.</summary>
    public const int AttackCyanFirst = 0, AttackCyanCount = 3;

    /// <summary>The attack colours' yellow-to-red ramp: its first colour and its four colours.</summary>
    public const int AttackFlameFirst = 3, AttackFlameCount = 4;

    /// <summary>The attack colours' three repeated body tissue shades, and the body tissue colour they start from.</summary>
    public const int AttackTissueFirst = 7, AttackBodyTissueFirst = 9;

    /// <summary>The attack colours' four neutral shades, which the white colour interrupts.</summary>
    public const int AttackGrayFirst = 10, AttackGrayLast = 14;

    /// <summary>The attack colour that repeats the body's white colour.</summary>
    public const int WhiteColor = 13;

    /// <summary>The final room's foreground outline, repeated by the tube and the glass ramp's end.</summary>
    public const int RoomOutlineColor = 12;

    /// <summary>The final room's first grey shade, repeated by the first three glass colours.</summary>
    public const int RoomGrayFirst = 13;

    /// <summary>The final room's darkest grey, repeated by a glass colour.</summary>
    public const int RoomDarkGrayColor = 16;

    /// <summary>The final room's four-colour wall ramp.</summary>
    public const int RoomWallFirst = 0, RoomWallCount = 4;

    /// <summary>The final room's four-colour darker wall ramp.</summary>
    public const int RoomShadowFirst = 4, RoomShadowCount = 4;

    /// <summary>The final room's three neutral shades, from intensity twenty to eight; the middle one interpolates.</summary>
    public const int RoomGrayCount = 3;

    /// <summary>The final room's red and blue indicator pairs, which share an active-channel shade decrease.</summary>
    public const int RoomRedFirst = 17, RoomBlueFirst = 19;

    /// <summary>The final room colour that repeats its darkest grey.</summary>
    public const int RoomRepeatedDarkGrayColor = 21;

    /// <summary>The final room's warm accent.</summary>
    public const int RoomGlowColor = 22;

    /// <summary>The room-entry images' last colour, the body's black colour.</summary>
    public const int RoomEntryBlackColor = 14;

    /// <summary>The glass-shard colours' eight-colour shade ramp.</summary>
    public const int GlassRampFirst = 3, GlassRampCount = 8;

    /// <summary>The glass-shard colours' dark neutral.</summary>
    public const int GlassDarkGrayColor = 11;

    /// <summary>The tube colours that repeat the room lights' neutral highlight; the following twelve repeat final-room colours twelve to twenty-three.</summary>
    public const int TubeNeutralCount = 2;
}
