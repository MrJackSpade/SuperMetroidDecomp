// #470: ordinary grounded ball movement into each collision bomb-block BTS.
// This seed isolates the contact predicate; it does not acquire Blue Suit itself.
int DiagnosticSpeedballBlockVariant(const char *rom, const char *output, int families) {
  int status = ProbeLoadRetailMovementRom(rom);
  if (status) return status;
  FILE *f = fopen(output, "wx");
  if (!f) return 4;
  fprintf(f, "left,bts,stage,x,y,base,extra,level,boost,plms\n");
  const uint16 stages[] = { 0, 0x300, 0x400 };
  const uint16 family_words[] = { 0x0123, 0x7123, 0x8123, 0xb123, 0xc123, 0xc123, 0xc123, 0xe123, 0xf123,
    0x7123, 0x7123, 0x7123, 0x7123, 0x7123, 0x7123, 0x7123, 0x7123, 0xf123, 0xb123 };
  const uint8 family_bts[] = { 0, 0, 0, 0x0e, 0, 8, 9, 0, 0, 1, 2, 3, 4, 5, 6, 7, 0x80, 0x80, 0x0f };
  for (int left = 0; left < 2; left++)
  for (int bts = 0; bts < (families ? 19 : 8); bts++)
  for (int stage = 0; stage < 3; stage++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    room_width_in_blocks = 16; room_height_in_blocks = 32;
    interactive_enemy_indexes[0] = 0xffff;
    for (int x = 0; x < 16; x++) level_data[16 * 16 + x] = 0x8000;
    int block = 15 * 16 + (left ? 7 : 8);
    level_data[block] = 0xf123; BTS[block] = bts;
    if (families) { level_data[block] = family_words[bts]; BTS[block] = family_bts[bts]; }
    fx_y_pos = lava_acid_y_pos = 0xffff; equipped_items = 0x2004; samus_health = 99;
    samus_x_pos = samus_prev_x_pos = left ? 132 : 124;
    samus_y_pos = samus_prev_y_pos = 249;
    samus_pose = samus_prev_pose = left ? 0x1f : 0x1e;
    samus_pose_x_dir = samus_prev_pose_x_dir = left ? 4 : 8;
    samus_movement_type = samus_prev_movement_type = samus_prev_movement_type2 = 4;
    samus_x_speed_table_pointer = 0x9f55;
    samus_x_base_speed = 3; samus_x_extra_run_speed = 2;
    samus_has_momentum_flag = 1; speed_boost_counter = stages[stage];
    RunAsmCode(0x90ec22, 0, 0, 0, 0);
    RunAsmCode(0x90a521, 0, 0, 0, 0);
    int active = 0;
    for (int i = 0; i < 40; i++) if (plm_header_ptr[i]) active++;
    fprintf(f, "%d,%d,%d,%04X%04X,%04X%04X,%04X%04X,%04X%04X,%04X,%04X,%d\n",
      left,bts,stage,samus_x_pos,samus_x_subpos,samus_y_pos,samus_y_subpos,
      samus_x_base_speed,samus_x_base_subspeed,samus_x_extra_run_speed,samus_x_extra_run_subspeed,
      level_data[block],speed_boost_counter,active);
  }
  fclose(f); return 0;
}
int DiagnosticSpeedballBlocks(const char *rom, const char *output) { return DiagnosticSpeedballBlockVariant(rom, output, 0); }
int DiagnosticSpeedballBlockFamilies(const char *rom, const char *output) { return DiagnosticSpeedballBlockVariant(rom, output, 1); }
