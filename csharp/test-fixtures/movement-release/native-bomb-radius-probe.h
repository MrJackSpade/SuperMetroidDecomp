// #485: isolate native normal-bomb placement/fuse/animation, with no SDL or saves.
// Include after native-release-probe.h; that loader restores unpatched ROM bytes.
int DiagnosticMetroidBomb(const char *rom) {
  int status = ProbeLoadRetailMovementRom(rom);
  if (status) return status;
  cpu_reset(g_snes->cpu);
  memset(g_ram, 0, sizeof(g_ram));
  g_snes->cpu->e = false;
  g_snes->cpu->sp = 0x1ff0;
  g_snes->cpu->dp = 0;
  room_width_in_blocks = room_height_in_blocks = 16;
  room_width_in_scrolls = room_height_in_scrolls = 1;
  interactive_enemy_indexes[0] = 0xffff;
  samus_x_pos = 128;
  samus_y_pos = 153;
  samus_x_radius = 7;
  samus_y_radius = 7;
  samus_pose = 0x1d;
  samus_pose_x_dir = 8;
  equipped_items = 0x1004;
  button_config_shoot_x = joypad1_lastkeys = joypad1_newkeys = 0x40;
  RunAsmCode(0x90bf9d, 0, 0, 0, 0);
  for (int frame = 0; frame < 80; frame++) {
    RunAsmCode(0x90aece, 0, 0, 0, 0);
    printf("BOMB frame=%d count=%u type=%04X timer=%u xy=%u/%u radius=%u/%u list=%04X instructionTimer=%u sprite=%04X\n",
      frame, bomb_counter, projectile_type[5], projectile_variables[5],
      projectile_x_pos[5], projectile_y_pos[5], projectile_x_radius[5], projectile_y_radius[5],
      projectile_bomb_instruction_ptr[5], projectile_bomb_instruction_timers[5], projectile_spritemap_pointers[5]);
  }
  return 0;
}
