namespace SuperMetroid.Core.Rooms;

/// <summary>Cartridge values of <see cref="RoomHeaderPointers"/> that only verification reads.</summary>
internal static class RoomHeaderPointersConstants
{
    /// <summary>Blue Brinstar elevator room at $8F:97B5.</summary>
    public const ushort BlueBrinstarElevatorRoom = 0x97b5;
    /// <summary>RoomHeader_Botwoon at $8F:D95E; both room states share the grey-door population.</summary>
    public const ushort Botwoon = 0xd95e;
    /// <summary>RoomHeader_Hellway at $8F:A2F7, Brinstar room $23.</summary>
    public const ushort Hellway = 0xa2f7;
    /// <summary>RoomHeader_BotwoonHallway at $8F:D617; contains the Mochtroid pipe-clip setup.</summary>
    public const ushort BotwoonHallway = 0xd617;
    /// <summary>Bank-$8F:9CB3, Brinstar room $08; includes the Dachora Speed Booster floor and shaft.</summary>
    public const ushort BrinstarRoom08 = 0x9cb3;
    /// <summary>RoomHeader_Caterpillar at $8F:A322, Brinstar room $24.</summary>
    public const ushort Caterpillar = 0xa322;
    /// <summary>Crab Maze at $8F:957D; its ordinary Scisers cycle all four surface poses.</summary>
    public const ushort CrabMaze = 0x957d;
    /// <summary>Crateria room $1E at $8F:9A44; contains the Blue Brinstar face block.</summary>
    public const ushort CrateriaFaceBlockRoom = 0x9a44;
    /// <summary>Crateria room $1A at $8F:9969; its Kago population uses the shared three-frame cycle.</summary>
    public const ushort CrateriaKagoRoom = 0x9969;
    /// <summary>Crateria room $1F at $8F:9A90; contains the Morph Ball surveillance eye.</summary>
    public const ushort CrateriaMorphBallEyeRoom = 0x9a90;
    /// <summary>RoomHeader_Crocomire at $8F:A98D; includes the west Power Bomb route door.</summary>
    public const ushort Crocomire = 0xa98d;
    /// <summary>Dead-Torizo corpse room at $8F:DC65 in Tourian.</summary>
    public const ushort DeadTorizoCorpse = 0xdc65;
    /// <summary>RoomHeader_Draygon at $8F:DA60; the four-record Maridia boss encounter.</summary>
    public const ushort Draygon = 0xda60;
    /// <summary>RoomHeader_EastTunnel at $8F:CF80; Maridia room $03 with the frozen-enemy gate setup.</summary>
    public const ushort EastTunnel = 0xcf80;
    /// <summary>RoomHeader_FrogSpeedway at $8F:B106; Norfair room $30, eight screens wide.</summary>
    public const ushort FrogSpeedway = 0xb106;
    /// <summary>Gauntlet east at $8F:92B3; contains the Crateria Yapping Maws.</summary>
    public const ushort GauntletEast = 0x92b3;
    /// <summary>RoomHeader_GoldenTorizo at $8F:B283; lower-right Samus position triggers statue awakening.</summary>
    public const ushort GoldenTorizo = 0xb283;
    /// <summary>Green Brinstar main shaft at $8F:9AD9 (area $01, room $00).</summary>
    public const ushort GreenBrinstarMainShaft = 0x9ad9;
    /// <summary>RoomHeader_GreenHillZone at $8F:9E52; contains the blue-left gate used by Grapple and Speed Booster gate glitches.</summary>
    public const ushort GreenHillZone = 0x9e52;
    /// <summary>RoomHeader_Kraid at $8F:A59F; its live state relocates BG3 HUD characters to VRAM word $2000.</summary>
    public const ushort Kraid = 0xa59f;
    /// <summary>RoomHeader_KronicBoost at $8F:AE74; Norfair room $22 with the reported blue gate.</summary>
    public const ushort KronicBoost = 0xae74;
    /// <summary>RoomHeader_MainHall at $8F:B236, Lower Norfair's elevator destination.</summary>
    public const ushort LowerNorfairMainHall = 0xb236;
    /// <summary>Mother Brain's chamber at $8F:DD58; its live state loads population $A1:E321.</summary>
    public const ushort MotherBrainChamber = 0xdd58;
    /// <summary>Retail Norfair room $1E at $8F:ADAD; its lower floor includes half-height square slopes.</summary>
    public const ushort NorfairRoom1E = 0xadad;
    /// <summary>RoomHeader_Phantoon at $8F:CD13; the Wrecked Ship boss encounter.</summary>
    public const ushort Phantoon = 0xcd13;
    /// <summary>RoomHeader_PinkBrinstarHopper at $8F:A130; the high-speed right-facing gate-glitch room.</summary>
    public const ushort PinkBrinstarHopper = 0xa130;
    /// <summary>Pre-moat room at $8F:948C; contains the Crateria KiHunter pair.</summary>
    public const ushort PreMoat = 0x948c;
    /// <summary>RoomHeader_RedTower at $8F:A253, Brinstar room $20.</summary>
    public const ushort RedTower = 0xa253;
    /// <summary>Tourian room $01 at $8F:DAE1; its initial state has four ordinary Metroids.</summary>
    public const ushort TourianMetroidRoom = 0xdae1;
    /// <summary>West Ocean at $8F:93FE (area $00, room $05), using the ocean sky main routine.</summary>
    public const ushort WestOcean = 0x93fe;

    /// <summary>Provides named native room headers used as anchors for room import and transition checks.</summary>
    extension(RoomHeaderPointers)
    {
        /// <inheritdoc cref="RoomHeaderPointersConstants.BlueBrinstarElevatorRoom"/>
        internal static ushort BlueBrinstarElevatorRoom => RoomHeaderPointersConstants.BlueBrinstarElevatorRoom;
        /// <inheritdoc cref="RoomHeaderPointersConstants.Botwoon"/>
        internal static ushort Botwoon => RoomHeaderPointersConstants.Botwoon;
        /// <inheritdoc cref="RoomHeaderPointersConstants.Hellway"/>
        internal static ushort Hellway => RoomHeaderPointersConstants.Hellway;
        /// <inheritdoc cref="RoomHeaderPointersConstants.BotwoonHallway"/>
        internal static ushort BotwoonHallway => RoomHeaderPointersConstants.BotwoonHallway;
        /// <inheritdoc cref="RoomHeaderPointersConstants.BrinstarRoom08"/>
        internal static ushort BrinstarRoom08 => RoomHeaderPointersConstants.BrinstarRoom08;
        /// <inheritdoc cref="RoomHeaderPointersConstants.Caterpillar"/>
        internal static ushort Caterpillar => RoomHeaderPointersConstants.Caterpillar;
        /// <inheritdoc cref="RoomHeaderPointersConstants.CrabMaze"/>
        internal static ushort CrabMaze => RoomHeaderPointersConstants.CrabMaze;
        /// <inheritdoc cref="RoomHeaderPointersConstants.CrateriaFaceBlockRoom"/>
        internal static ushort CrateriaFaceBlockRoom => RoomHeaderPointersConstants.CrateriaFaceBlockRoom;
        /// <inheritdoc cref="RoomHeaderPointersConstants.CrateriaKagoRoom"/>
        internal static ushort CrateriaKagoRoom => RoomHeaderPointersConstants.CrateriaKagoRoom;
        /// <inheritdoc cref="RoomHeaderPointersConstants.CrateriaMorphBallEyeRoom"/>
        internal static ushort CrateriaMorphBallEyeRoom => RoomHeaderPointersConstants.CrateriaMorphBallEyeRoom;
        /// <inheritdoc cref="RoomHeaderPointersConstants.Crocomire"/>
        internal static ushort Crocomire => RoomHeaderPointersConstants.Crocomire;
        /// <inheritdoc cref="RoomHeaderPointersConstants.DeadTorizoCorpse"/>
        internal static ushort DeadTorizoCorpse => RoomHeaderPointersConstants.DeadTorizoCorpse;
        /// <inheritdoc cref="RoomHeaderPointersConstants.Draygon"/>
        internal static ushort Draygon => RoomHeaderPointersConstants.Draygon;
        /// <inheritdoc cref="RoomHeaderPointersConstants.EastTunnel"/>
        internal static ushort EastTunnel => RoomHeaderPointersConstants.EastTunnel;
        /// <inheritdoc cref="RoomHeaderPointersConstants.FrogSpeedway"/>
        internal static ushort FrogSpeedway => RoomHeaderPointersConstants.FrogSpeedway;
        /// <inheritdoc cref="RoomHeaderPointersConstants.GauntletEast"/>
        internal static ushort GauntletEast => RoomHeaderPointersConstants.GauntletEast;
        /// <inheritdoc cref="RoomHeaderPointersConstants.GoldenTorizo"/>
        internal static ushort GoldenTorizo => RoomHeaderPointersConstants.GoldenTorizo;
        /// <inheritdoc cref="RoomHeaderPointersConstants.GreenBrinstarMainShaft"/>
        internal static ushort GreenBrinstarMainShaft => RoomHeaderPointersConstants.GreenBrinstarMainShaft;
        /// <inheritdoc cref="RoomHeaderPointersConstants.GreenHillZone"/>
        internal static ushort GreenHillZone => RoomHeaderPointersConstants.GreenHillZone;
        /// <inheritdoc cref="RoomHeaderPointersConstants.Kraid"/>
        internal static ushort Kraid => RoomHeaderPointersConstants.Kraid;
        /// <inheritdoc cref="RoomHeaderPointersConstants.KronicBoost"/>
        internal static ushort KronicBoost => RoomHeaderPointersConstants.KronicBoost;
        /// <inheritdoc cref="RoomHeaderPointersConstants.LowerNorfairMainHall"/>
        internal static ushort LowerNorfairMainHall => RoomHeaderPointersConstants.LowerNorfairMainHall;
        /// <inheritdoc cref="RoomHeaderPointersConstants.MotherBrainChamber"/>
        internal static ushort MotherBrainChamber => RoomHeaderPointersConstants.MotherBrainChamber;
        /// <inheritdoc cref="RoomHeaderPointersConstants.NorfairRoom1E"/>
        internal static ushort NorfairRoom1E => RoomHeaderPointersConstants.NorfairRoom1E;
        /// <inheritdoc cref="RoomHeaderPointersConstants.Phantoon"/>
        internal static ushort Phantoon => RoomHeaderPointersConstants.Phantoon;
        /// <inheritdoc cref="RoomHeaderPointersConstants.PinkBrinstarHopper"/>
        internal static ushort PinkBrinstarHopper => RoomHeaderPointersConstants.PinkBrinstarHopper;
        /// <inheritdoc cref="RoomHeaderPointersConstants.PreMoat"/>
        internal static ushort PreMoat => RoomHeaderPointersConstants.PreMoat;
        /// <inheritdoc cref="RoomHeaderPointersConstants.RedTower"/>
        internal static ushort RedTower => RoomHeaderPointersConstants.RedTower;
        /// <inheritdoc cref="RoomHeaderPointersConstants.TourianMetroidRoom"/>
        internal static ushort TourianMetroidRoom => RoomHeaderPointersConstants.TourianMetroidRoom;
        /// <inheritdoc cref="RoomHeaderPointersConstants.WestOcean"/>
        internal static ushort WestOcean => RoomHeaderPointersConstants.WestOcean;
    }
}
