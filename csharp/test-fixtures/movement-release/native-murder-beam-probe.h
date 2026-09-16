// Bounded original-CPU probe for issue #399. Include after native-release-probe.h.
int DiagnosticMurderBeam(const char *rom) {
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
  samus_x_pos = samus_y_pos = 128;
  samus_x_radius = 5;
  samus_y_radius = 21;
  samus_pose = 2;
  samus_pose_x_dir = 4;
  equipped_beams = 0x100f;
  button_config_shoot_x = joypad1_lastkeys = 0x40;

  RunAsmCode(0x90b986, 0, 0, 0, 0);
  printf("MURDER fire count=%u cooldown=%u type=%04X damage=%u dir=%u xy=%u/%u pre=%04X list=%04X radius=%u/%u speed=%04X/%04X exit=%04X/%04X/%04X\n",
    projectile_counter, cooldown_timer, projectile_type[0], projectile_damage[0], projectile_dir[0],
    projectile_x_pos[0], projectile_y_pos[0], projectile_bomb_pre_instructions[0],
    projectile_bomb_instruction_ptr[0], projectile_x_radius[0], projectile_y_radius[0],
    projectile_bomb_x_speed[0], projectile_bomb_y_speed[0],
    g_snes->cpu->a, g_snes->cpu->x, g_snes->cpu->y);

  for (int frame = 0; frame < 16 && projectile_damage[0]; frame++) {
    uint16 entry_y = g_snes->cpu->y;
    RunAsmCode(0x90aece, 0, 0, entry_y, 0);
    printf("MURDER step frame=%d count=%u type=%04X damage=%u dir=%04X xy=%u/%u pre=%04X list=%04X timer=%u sprite=%04X radius=%u/%u speed=%04X/%04X var=%04X entryY=%04X exitY=%04X\n",
      frame, projectile_counter, projectile_type[0], projectile_damage[0], projectile_dir[0],
      projectile_x_pos[0], projectile_y_pos[0], projectile_bomb_pre_instructions[0],
      projectile_bomb_instruction_ptr[0], projectile_bomb_instruction_timers[0],
      projectile_spritemap_pointers[0], projectile_x_radius[0], projectile_y_radius[0],
      projectile_bomb_x_speed[0], projectile_bomb_y_speed[0], projectile_variables[0],
      entry_y, g_snes->cpu->y);
  }
  return 0;
}
