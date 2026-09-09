// #485: isolated retail CPU bomb-jump trajectory, not a full-room parity test.
// Include after native-release-probe.h in sm_rtl.c and dispatch before SDL starts.
int DiagnosticMetroidBombJump(const char *rom) {
  int status = ProbeLoadRetailMovementRom(rom);
  if (status) return status;
  cpu_reset(g_snes->cpu);
  memset(g_ram, 0, sizeof(g_ram));
  g_snes->cpu->e = false;
  g_snes->cpu->sp = 0x1ff0;
  g_snes->cpu->dp = 0;
  room_width_in_blocks = 16;
  room_height_in_blocks = 16;
  interactive_enemy_indexes[0] = 0xffff;
  for (int i = 192; i < 208; i++) level_data[i] = 0x8000;
  fx_y_pos = lava_acid_y_pos = 0xffff;
  samus_x_pos = 128;
  samus_y_pos = 185;
  samus_x_radius = 7;
  samus_y_radius = 7;
  samus_pose = samus_prev_pose = 0x1d;
  samus_pose_x_dir = 8;
  samus_movement_type = 4;
  samus_y_subaccel = 0x1c00;
  bomb_jump_dir = 0x0802;
  RunAsmCode(0x90e025, 0, 0, 0, 0);
  printf("BOMB START y=%04X.%04X speed=%04X.%04X\n", samus_y_pos,
    samus_y_subpos, samus_y_speed, samus_y_subspeed);
  for (int frame = 0; frame < 12; frame++) {
    RunAsmCode(0x90e032, 0, 0, 0, 0);
    printf("BOMB MOVE frame=%d y=%04X.%04X speed=%04X.%04X\n", frame,
      samus_y_pos, samus_y_subpos, samus_y_speed, samus_y_subspeed);
  }
  return 0;
}
