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
