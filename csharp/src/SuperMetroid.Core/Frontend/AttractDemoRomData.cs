namespace SuperMetroid.Core.Frontend;

/// <summary>Cartridge definition tables consumed by the title-demo loaders.</summary>
public static class AttractDemoRomData
{
    /// <summary>Bank-$91 input-object identities for the two grapple demonstrations.</summary>
    public static class InputObjects
    {
        /// <summary>$91:9EB2, DemoInputObjects_Title_GrappleBeam, ordinary grapple demonstration.</summary>
        public const ushort GrappleBeam = 0x9eb2;
        /// <summary>$91:9EC4, DemoInputObjects_Title_AdvancedGrappleBeam, advanced grapple demonstration.</summary>
        public const ushort AdvancedGrappleBeam = 0x9ec4;
    }
    /// <summary>$82:8548 waits ninety NMI calls on the completed demo's final image.</summary>
    public const int FinalImageHoldFrames = 90;
    /// <summary>$82:8000 demo enemy-graphics loop starts at six and includes zero.</summary>
    public const int EnemyTransferFrames = 7;
    /// <summary>$80:8261 enables three sets before the completed-game SRAM marker.</summary>
    public const int DefaultSetCount = 3;
    /// <summary>$70:1FE0, completion marker checked by VerifySRAM to unlock set four.</summary>
    public const int CompletionMarkerAddress = 0x701fe0;
    /// <summary>VerifySRAM compares twelve bytes, including the trailing NUL.</summary>
    public static ReadOnlySpan<byte> CompletionMarker => "supermetroid\0"u8;
    /// <summary>Literal state writes in the demo setup routines.</summary>
    public static class SetupValues
    {
        /// <summary>$91:8A43 sets current energy to twenty without changing maximum energy.</summary>
        public const ushort LowHealth = 20;
        /// <summary>$82:891A writes the scroll cell at byte offset $21.</summary>
        public const int ChargeBeamScrollIndex = 0x21;
        /// <summary>$82:892B writes sixty to Kraid's body function timer.</summary>
        public const ushort KraidFunctionTimer = 60;
    }
    /// <summary>Native bank-$91 Samus setup dispatcher identities.</summary>
    public static class SamusSetup
    {
        /// <summary>$91:8A33, DemoSetFunc_0: front-facing Landing Site actor.</summary>
        public const ushort LandingSite = 0x8a33;
        /// <summary>$91:8A3E, DemoSetFunc_3: left-facing grounded morph ball.</summary>
        public const ushort MorphLeft = 0x8a3e;
        /// <summary>$91:8A43, DemoSetFunc_7: twenty health, standing left.</summary>
        public const ushort LowHealthLeft = 0x8a43;
        /// <summary>$91:8A49, DemoSetFunc_2: standing left.</summary>
        public const ushort StandingLeft = 0x8a49;
        /// <summary>$91:8A4E, DemoSetFunc_4: falling left.</summary>
        public const ushort FallingLeft = 0x8a4e;
        /// <summary>$91:8A53, DemoSetFunc_1: standing right.</summary>
        public const ushort StandingRight = 0x8a53;
        /// <summary>$91:8A68, DemoSetFunc_5: immediate diagonal-right shinespark.</summary>
        public const ushort DiagonalShinespark = 0x8a68;
        /// <summary>$91:8A81, DemoSetFunc_6: immediate horizontal-left shinespark.</summary>
        public const ushort HorizontalShinespark = 0x8a81;
    }

    /// <summary>Native bank-$82 post-room-load demo callbacks.</summary>
    public static class RoomSetup
    {
        /// <summary>$82:891A, DemoRoom_ChargeBeamRoomScroll21: scroll cell $21 becomes red.</summary>
        public const ushort ChargeBeamScroll = 0x891a;
        /// <summary>$82:8924, nullsub_291: RTS.</summary>
        public const ushort NoOp = 0x8924;
        /// <summary>$82:8925, DemoRoom_SetBG2TilemapBase: Landing Site BG2SC=$4A.</summary>
        public const ushort LandingSiteSky = 0x8925;
        /// <summary>$82:892B, DemoRoom_SetKraidFunctionTimer: body variable F becomes sixty.</summary>
        public const ushort KraidTimer = 0x892b;
        /// <summary>$82:8932, DemoRoom_SetBrinstarBossBits: Brinstar boss byte becomes one.</summary>
        public const ushort DefeatedKraid = 0x8932;
    }
    /// <summary>$82:876C, DemoRoomData_pointers: four room-list pointers terminated by $FFFF.</summary>
    public const int RoomSetPointers = 0x82876c;
    /// <summary>$91:8885, DemoData_Pointers: four equipment/input-object list pointers.</summary>
    public const int EquipmentSetPointers = 0x918885;
    /// <summary>$91:89FD, DemoSamusSetup_Pointers: four Samus-initializer list pointers.</summary>
    public const int SamusSetupSetPointers = 0x9189fd;
    /// <summary>Number of physical entries in each retail demo-set pointer table.</summary>
    public const int SetCount = 4;
    /// <summary>LoadDemoRoomData's bank and eighteen-byte record stride.</summary>
    public const int RoomBank = 0x820000, RoomRecordBytes = 18;
    /// <summary>LoadDemoData's bank and sixteen-byte equipment record stride.</summary>
    public const int EquipmentBank = 0x910000, EquipmentRecordBytes = 16;
    /// <summary>CheckForNextDemo's room-list termination word.</summary>
    public const ushort EndOfSet = 0xffff;

    /// <summary>Word offsets in a DemoRoomData record at bank $82.</summary>
    public static class RoomFields
    {
        public const int Room = 0, Door = 2, DoorSlot = 4, CameraX = 6, CameraY = 8,
            SamusYFromTop = 10, SamusXFromCenter = 12, Duration = 14, Setup = 16;
    }

    /// <summary>Word offsets in a DemoSetDef record at bank $91.</summary>
    public static class EquipmentFields
    {
        public const int Items = 0, Missiles = 2, SuperMissiles = 4, PowerBombs = 6,
            Health = 8, CollectedBeams = 10, EquippedBeams = 12, InputObject = 14;
    }
}
