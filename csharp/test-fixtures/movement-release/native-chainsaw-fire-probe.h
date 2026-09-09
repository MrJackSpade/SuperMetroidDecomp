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
    uint16 entry_y = g_snes->cpu->y;
    RunAsmCode(0x90aece, 0, 0, entry_y, 0);
    printf("CONTEXT entryY=%04X exitY=%04X window=%02X/%02X/%02X/%02X/%02X/%02X/%02X/%02X/%02X/%02X\n",
      entry_y, g_snes->cpu->y, g_ram[0x60], g_ram[0x61], g_ram[0x62], g_ram[0x63], g_ram[0x64],
      g_ram[0x65], g_ram[0x66], g_ram[0x67], g_ram[0x68], g_ram[0x69]);
    printf("STEP %d count=%u type=%04X damage=%u xy=%u/%u pre=%04X list=%04X timer=%u sprite=%04X radius=%u/%u dp60=%04X\n",
      frame, projectile_counter, projectile_type[0], projectile_damage[0], projectile_x_pos[0], projectile_y_pos[0],
      projectile_bomb_pre_instructions[0], projectile_bomb_instruction_ptr[0], projectile_bomb_instruction_timers[0],
      projectile_spritemap_pointers[0], projectile_x_radius[0], projectile_y_radius[0], *(uint16 *)(g_ram + 0x60));
  }
  }
  // Isolate the misaligned STY's address/value independently of the caller's Y.
  // The inactive room dimensions suppress subsequent terrain reads in this subprobe.
  for (int slot = 0; slot < 5; slot++) {
    memset(g_ram, 0, sizeof(g_ram));
    projectile_index = slot * 2;
    RunAsmCode(0x90b0ac, 0, slot * 2, 0x1234, 0);
    printf("WINDOW slot=%d bytes=", slot);
    for (int offset = 0x60; offset < 0x6a; offset++) printf("%02X", g_ram[offset]);
    printf("\n");
  }
  return 0;
}
