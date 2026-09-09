// #396: native uncharged firing in an empty synthetic room, no SDL or save writes.
// Include after native-release-probe.h; suppress explicit SDL Die/Warning dialogs.
int DiagnosticChainsawFire(const char *rom) {
  int status = ProbeLoadRetailMovementRom(rom);
  if (status) return status;
  for (int active = 0; active < 2; active++) {
  cpu_reset(g_snes->cpu);
  memset(g_ram, 0, sizeof(g_ram));
  g_snes->cpu->e = false;
  g_snes->cpu->sp = 0x1ff0;
  g_snes->cpu->dp = 0;
  room_width_in_blocks = room_height_in_blocks = 16;
  room_width_in_scrolls = room_height_in_scrolls = 1;
  interactive_enemy_indexes[0] = 0xffff;
  samus_x_pos = samus_y_pos = 128;
  samus_x_radius = 5;
  samus_y_radius = 21;
  samus_pose = 1;
  samus_pose_x_dir = 8;
  equipped_beams = 13;
  power_bomb_flag = active ? 0x8000 : 0;
  button_config_shoot_x = joypad1_lastkeys = joypad1_newkeys = 0x40;
  RunAsmCode(0x90b887, 0, 0, 0, 0);
  printf("CHAINSAW active=%d count=%u cooldown=%u type=%04X damage=%u dir=%u xy=%u/%u pre=%04X list=%04X radius=%u/%u speed=%04X/%04X\n",
    active, projectile_counter, cooldown_timer, projectile_type[0], projectile_damage[0], projectile_dir[0],
    projectile_x_pos[0], projectile_y_pos[0], projectile_bomb_pre_instructions[0],
    projectile_bomb_instruction_ptr[0], projectile_x_radius[0], projectile_y_radius[0],
    projectile_bomb_x_speed[0], projectile_bomb_y_speed[0]);
  for (int frame = 0; frame < 8; frame++) {
    RunAsmCode(0x90aece, 0, 0, 0, 0);
    printf("STEP %d count=%u type=%04X damage=%u xy=%u/%u pre=%04X list=%04X timer=%u sprite=%04X radius=%u/%u dp60=%04X\n",
      frame, projectile_counter, projectile_type[0], projectile_damage[0], projectile_x_pos[0], projectile_y_pos[0],
      projectile_bomb_pre_instructions[0], projectile_bomb_instruction_ptr[0], projectile_bomb_instruction_timers[0],
      projectile_spritemap_pointers[0], projectile_x_radius[0], projectile_y_radius[0], *(uint16 *)(g_ram + 0x60));
  }
  }
  return 0;
}
