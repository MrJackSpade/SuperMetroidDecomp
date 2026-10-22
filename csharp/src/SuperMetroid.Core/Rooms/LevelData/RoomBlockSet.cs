namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>A table of room block compositions, shared by the graphics sets that build their rooms from it.</summary>
public enum RoomBlockSet
{
    /// <summary>The blocks of upper Crateria and red Crateria.</summary>
    UpperCrateria,

    /// <summary>The blocks of lower Crateria and the old Tourian rooms.</summary>
    LowerCrateria,

    /// <summary>The blocks of the Wrecked Ship, powered or not.</summary>
    WreckedShip,

    /// <summary>The blocks of green and blue Brinstar.</summary>
    GreenBlueBrinstar,

    /// <summary>The blocks of red Brinstar, Kraid's lair and the statues hall.</summary>
    RedBrinstarKraidStatuesHall,

    /// <summary>The blocks of heated and unheated Norfair.</summary>
    Norfair,

    /// <summary>The blocks of Maridia without sand.</summary>
    SandlessMaridia,

    /// <summary>The blocks of sandy Maridia.</summary>
    SandyMaridia,

    /// <summary>The blocks of Tourian and Mother Brain's room.</summary>
    Tourian,

    /// <summary>The blocks of every Ceres Station room.</summary>
    Ceres,

    /// <summary>The blocks of map, refill and save rooms and the Tourian statue room.</summary>
    UtilityRoomStatues,

    /// <summary>The blocks of Kraid's room.</summary>
    Kraid,

    /// <summary>The blocks of Crocomire's room.</summary>
    Crocomire,

    /// <summary>The blocks of Draygon's room.</summary>
    Draygon,
}
