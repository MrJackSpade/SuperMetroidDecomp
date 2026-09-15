/* The native cleanup owns exact-center admission, then replaces the spark
   handler. Control cases alter only bomb placement, resources or the chord. */
enum { PowerBombCleanup = 0x888b4e };
static uint16 crystal_spark_input(int frame, int left, int mode) {
  if (frame < 140) return 0x8000 | (left ? 0x200 : 0x100);
  if (frame == 140) return 0x410;
  if (frame < 150) return 0;
  if (frame == 150) return 0x80;
  if (frame == 151) return 0x800;
  if (frame == 152) return mode == 7 ? 0x4f0 : 0x470;
  /* Reuse only the successful Flash's persistent boost, without another run. */
  if (mode == 0 && frame == 460) return 0x410;
  if (mode == 0 && frame > 460 && frame < 470) return 0x10;
  if (mode == 0 && frame == 470) return 0x80;
  if (mode == 0 && frame >= 471) return 0x880;
  return 0;
}
static void crystal_spark_cleanup(int mode) {
  power_bomb_explosion_x_pos = samus_x_pos + (mode == 1 ? 1 : mode == 2 ? -1 : 0);
  power_bomb_explosion_y_pos = samus_y_pos + (mode == 3 ? 1 : mode == 4 ? -1 : 0);
  power_bomb_explosion_status = 0x8000;
  power_bomb_flag = 1;
  run(PowerBombCleanup);
}
/* Same earned charge, three pre-jump postures; never inject Y or a pose. */
static uint16 crystal_height_input(int frame, int left, int mode) {
  int posture = mode / 8;
  if (posture == 1 && frame >= 148 && frame <= 150)
    return frame == 150 ? 0x90 : 0x10;
  if (posture == 2 && frame >= 145 && frame < 150) return 0x800;
  return crystal_spark_input(frame, left, mode & 7);
}
static void verify_crystal_height(int frame, int mode) {
  static const uint16 centers[] = {482, 492, 490};
  if (frame == 151 && (samus_y_pos != centers[mode / 8] || samus_y_subpos != 0xffff || samus_movement_handler != 0xd068))
    Die("Crystal Spark posture must earn the expected windup center");
  if (frame == 152 && ((samus_pose == 0xd3 || samus_pose == 0xd4) != ((mode & 7) == 0)))
    Die("Crystal Spark exact-center/resource/chord admission changed");
  if ((mode & 7) != 0) return;
  if (frame == 459 && (speed_boost_counter != 0x400 || samus_health != 99 || samus_missiles || samus_super_missiles || samus_power_bombs))
    Die("Crystal Flash must leave reusable boost and consume ammunition");
  if (frame == 460 && samus_shine_timer != 179)
    Die("Crystal Spark must store a second charge without a run-up");
  if (frame == 473 && (samus_contact_damage_index != 2 || samus_health != 98))
    Die("Crystal Spark must launch a damaging, energy-consuming second spark");
}
