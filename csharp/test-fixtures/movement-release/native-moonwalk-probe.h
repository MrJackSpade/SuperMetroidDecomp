// #467: actual alpha/beta with matched real-input Moonwalk entry/exit scenarios.
int DiagnosticMoonwalk(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom);
  if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f, "left,enabled,medium,scenario,frame,input,x,y,pose,movement,anim,timer,base,extra,accel,facing,yspeed,ydir,charge,health,invinc,knockback\n");
  for (int left = 0; left < 2; left++)
  for (int enabled = 0; enabled < 2; enabled++)
  for (int medium = 0; medium < 3; medium++)
  for (int scenario = 0; scenario < 14; scenario++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    room_width_in_blocks = 144; room_height_in_blocks = 80;
    room_width_in_scrolls = 9; room_height_in_scrolls = 5; room_size_in_blocks = 144 * 80 * 2;
    interactive_enemy_indexes[0] = 0xffff;
    for (int x = 0; x < 144; x++)
      if (!(scenario == 6 || scenario == 10 || scenario == 11) || (left ? x <= 64 : x >= 63)) level_data[16 * 144 + x] = 0x8000;
    if (scenario == 3) for (int y = 0; y < 16; y++) level_data[y * 144 + (left ? 62 : 65)] = 0x8000;
    if (scenario == 9) level_data[16 * 144 + (left ? 65 : 62)] = 0xa000;
    fx_y_pos = medium ? 8 : 0xffff; lava_acid_y_pos = 0xffff;
    fx_liquid_options = 0x80; fx_type = medium ? 6 : 0;
    equipped_items = medium == 2 ? 0x24 : 4; equipped_beams = 0x1000;
    moonwalk_flag = enabled; samus_health = samus_max_health = 99;
    samus_x_pos = samus_prev_x_pos = 1024; samus_y_pos = samus_prev_y_pos = 235;
    samus_pose = samus_prev_pose = left ? 2 : 1;
    samus_pose_x_dir = samus_prev_pose_x_dir = left ? 4 : 8;
    samus_anim_frame_timer = 1; samus_x_speed_table_pointer = 0x9f55;
    samus_input_handler = 0xe913; samus_movement_handler = 0xa337; grapple_beam_function = 0xc4f0;
    button_config_run_b = 0x8000; button_config_jump_a = 0x80; button_config_shoot_x = 0x40;
    button_config_aim_up_R = 0x10; button_config_aim_down_L = 0x20;
    button_config_itemcancel_y = 0x4000; button_config_itemswitch = 0x2000;
    uint16 previous = 0, forward = left ? 0x200 : 0x100, back = left ? 0x100 : 0x200;
    for (int frame = 0; frame < 120; frame++) {
      nmi_frame_counter_word = frame + 2; nmi_frame_counter_byte = frame + 2;
      uint16 input = 0x40;
      if (frame >= (scenario == 13 ? 70 : 40)) input |= back;
      if ((scenario == 1 || scenario == 3) && frame < 40) input |= forward;
      if (scenario == 2 && frame == 10) input |= 0x400;
      if (scenario == 4 && frame >= 41) input = 0x40 | forward;
      if (scenario == 5 && frame >= 50) input |= 0x80;
      if (scenario == 8 && frame >= 80) input |= 0x80;
      if (scenario == 12 && frame >= 40 || scenario == 13 && frame >= 70) input |= 0x80;
      if (scenario == 7 || scenario == 10) input |= 0x10;
      if (scenario == 11) input |= 0x20;
      if (frame >= 90) input = 0;
      joypad1_lastkeys = input; joypad1_newkeys = input & ~previous; previous = input;
      RunAsmCode(0x90e695, 0, 0, 0, 0); RunAsmCode(0xa09785, 0, 0, 0, 0);
      samus_contact_damage_index = 0;
      RunAsmCode(0x900000 | samus_movement_handler, 0, 0, 0, 0);
      RunAsmCode(0x908000, 0, 0, 0, 0); RunAsmCode(0x90dde9, 0, 0, 0, 0);
      RunAsmCode(0x91e8b6, 0, 0, 0, 0); RunAsmCode(0x91eb88, 0, 0, 0, 0);
      RunAsmCode(0x90eab3, 0, 0, 0, 0); RunAsmCode(0x90e9ce, 0, 0, 0, 0);
      RunAsmCode(0xa09169, 0, 0, 0, 0);
      fprintf(f, "%d,%d,%d,%d,%d,%04X,%04X%04X,%04X%04X,%02X,%02X,%04X,%04X,%04X%04X,%04X%04X,%04X,%02X,%04X%04X,%04X,%04X,%04X,%04X,%04X\n",
        left,enabled,medium,scenario,frame,input,samus_x_pos,samus_x_subpos,samus_y_pos,samus_y_subpos,
        samus_pose,samus_movement_type,samus_anim_frame,samus_anim_frame_timer,samus_x_base_speed,samus_x_base_subspeed,
        samus_x_extra_run_speed,samus_x_extra_run_subspeed,samus_x_accel_mode,samus_pose_x_dir,
        samus_y_speed,samus_y_subspeed,samus_y_dir,flare_counter,samus_health,samus_invincibility_timer,knockback_dir);
    }
  }
  fclose(f); return 0;
}
