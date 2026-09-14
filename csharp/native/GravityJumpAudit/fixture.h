/* Original Japan/USA pause admission, fade and equipment identities. */
enum GravityJumpEntries {
  /* $90:EA45 accepts Start; $80:8924 advances INIDISP fade-out. */
  PauseAdmission = 0x90ea45, FadeOut = 0x808924,
  /* $82:AB47 selects from collected inventory; $82:B0C2 consumes suit-page input. */
  SelectInitialEquipment = 0x82ab47, SuitEquipmentInput = 0x82b0c2,
  /* $91:E633: movement-specific equipment reconciliation invoked by pause teardown. */
  ReconcileSamusEquipment = 0x91e633
};
