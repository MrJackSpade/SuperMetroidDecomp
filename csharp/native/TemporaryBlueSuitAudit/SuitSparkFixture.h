/* #427: suspend a controller-earned windup at the post-message suit boundary. */
enum {
  VariaPickupSetup = 0x91d4e4, GravityPickupSetup = 0x91d5ba,
  VariaPickupHdma = 0x88e026, GravityPickupHdma = 0x88e05c,
  SuitLockedAlpha = 0x90e713, SuitLockedBeta = 0xe8cd
};
enum { XrayRestoreFirst = 0x888934, XrayRestoreSecond = 0x8889ba, XrayRelease = 0x888a08 };
static uint16 suit_spark_input(int frame, int left, int mode);
static uint16 suit_release_input(int frame, int left, int mode) {
  if (frame < 330) return suit_spark_input(frame, left, mode);
  if (frame == 390) return 0x410;
  if (frame > 390 && frame < 400) return 0x10;
  if (frame == 400) return 0x80;
  if (frame >= 401) return 0x880;
  return 0;
}
static void step_suit_xray_release(int frame) {
  if (frame == 330) run(XrayRestoreFirst);
  if (frame == 331) run(XrayRestoreSecond);
  if (frame == 332) run(XrayRelease);
}
static void verify_suit_xray_release(int frame) {
  if (frame == 332 && (time_is_frozen_flag || samus_movement_handler != 0xa337 || speed_boost_counter != 0x400))
    Die("X-ray teardown must restore normal movement and retain boost");
  if (frame == 390 && samus_shine_timer != 179)
    Die("Retained suit boost must store a new charge without a run-up");
  if (frame == 403 && (samus_contact_damage_index != 2 || samus_health != 98 || samus_y_speed != 7 || samus_y_subspeed != 0x1c00))
    Die("Retained suit boost must launch a moving, damaging, energy-consuming spark");
}
static bool suit_pickup_active;
static const char *suit_spark_columns = ",suitactive,suitsubstate,items,winduptimer,frozen,windowhash,beam,widening,red,green,blue";
static void print_suit_spark_state(void) {
  uint32 hash = 2166136261u;
  for (int i = 0; i < 256; i++) {
    hash = (hash ^ (uint8)hdma_table_1[i]) * 16777619u;
    hash = (hash ^ (uint8)(hdma_table_1[i] >> 8)) * 16777619u;
  }
  printf(",%04X,%04X,%04X,%04X,%04X,%08X,%04X,%04X,%02X,%02X,%02X",
    suit_pickup_active,substate,equipped_items,timer_for_shinesparks_startstop,time_is_frozen_flag,
    hash,suit_pickup_light_beam_pos,suit_pickup_light_beam_widening_speed,
    suit_pickup_color_math_R,suit_pickup_color_math_G,suit_pickup_color_math_B);
}
static uint16 suit_spark_input(int frame, int left, int mode) {
  if (frame <= 151) return crystal_spark_input(frame, left, 0);
  if ((mode & 3) == 0) return 0;
  return 0x8000 | ((mode & 3) == 3 ? 0 : (mode & 3) == 1 ?
    (left ? 0x200 : 0x100) : (left ? 0x100 : 0x200));
}
static void begin_suit_spark(int mode) {
  if (samus_movement_handler != 0xd068) Die("Suit pickup must interrupt earned windup");
  equipped_items |= mode >= 4 ? 0x20 : 1;
  collected_items = equipped_items;
  layer1_x_pos = 0; layer1_y_pos = 355;
  hud_item_index = 5;
  run(mode >= 4 ? GravityPickupSetup : VariaPickupSetup);
  if (frame_handler_beta != SuitLockedBeta) Die("Suit did not install empty beta");
  suit_pickup_active = true;
}
static void verify_suit_spark(int frame, int mode) {
  if (frame == 152 && (!suit_pickup_active || timer_for_shinesparks_startstop != 30))
    Die("Suit did not suspend windup");
  if (frame == 322 && ((mode & 3) == 1 || (mode & 3) == 2) &&
      (samus_movement_handler != 0xe94f || !time_is_frozen_flag || speed_boost_counter != 0x400))
    Die("Suit release did not install X-ray while preserving boost");
}
static void step_suit_hdma(int mode) {
  if (!suit_pickup_active) return;
  bool finishing = substate == 6;
  run(mode >= 4 ? GravityPickupHdma : VariaPickupHdma);
  if (finishing) suit_pickup_active = false;
}
