namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Cartridge values and helpers of <see cref="IntroCinematicRomData.Layers"/> that only verification reads.</summary>
internal static class IntroCinematicRomDataLayersConstants
{
        /// <summary>BG screen-base word used for the portrait tilemap.</summary>
        public const ushort PortraitTilemapWord = 0x4800;
        /// <summary>BG screen-base word used for narration text.</summary>
        public const ushort NarrationTilemapWord = 0x4c00;
        /// <summary>BG1 screen-base word used by cinematic scenes.</summary>
        public const ushort SceneBg1TilemapWord = 0x5000;
        /// <summary>BG2 screen-base word used by cinematic scenes.</summary>
        public const ushort SceneBg2TilemapWord = 0x5400;
        /// <summary>BG screen-base word used by the scientist scene's delivery page.</summary>
        public const ushort ScientistTilemapWord = 0x5800;
        /// <summary>BG screen-base word used by the scientist scene's examination page.</summary>
        public const ushort ScientistExaminationTilemapWord = 0x5c00;
        /// <summary>BG character-base word used by the cinematic font.</summary>
        public const ushort FontCharacterBaseWord = 0x4000;
}
