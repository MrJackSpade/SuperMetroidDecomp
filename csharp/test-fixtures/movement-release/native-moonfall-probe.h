// #468: bounded constructed shaft, real controller entry; no injected Moonfall state.
int DiagnosticMoonfall(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f, "left,enabled,medium,scenario,frame,input,x,y,pose,movement,anim,timer,base,extra,accel,facing,yspeed,ydir,bounce\n");
  for (int left = 0; left < 2; left++)
  for (int enabled = 0; enabled < 2; enabled++)
  for (int medium = 0; medium < 3; medium++)
  for (int scenario = 0; scenario < 13; scenario++) {
    int sequence = scenario >= 10 ? scenario - 10 : scenario;
    uint16 angle = scenario >= 10 ? 0x10 : 0x20;
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    room_width_in_blocks = 48; room_height_in_blocks = 144;
    room_width_in_scrolls = 3; room_height_in_scrolls = 9; room_size_in_blocks = 48 * 144 * 2;
    interactive_enemy_indexes[0] = 0xffff;
    for (int y = 0; y < 144; y++) for (int x = 0; x < 48; x++)
      if (y >= 138 || y == 16 && (left ? x <= 24 : x >= 23) || scenario == 7 && y == 125 || scenario == 9 && y == 126)
        level_data[y * 48 + x] = 0x8000;
    fx_y_pos = medium ? 8 : 0xffff; lava_acid_y_pos = 0xffff;
    fx_liquid_options = 0x80; fx_type = medium ? 6 : 0;
    equipped_items = medium == 2 ? 0x24 : 4;
    moonwalk_flag = enabled; samus_health = samus_max_health = 99;
    samus_x_pos = samus_prev_x_pos = scenario == 8 ? (left ? 403 : 364) : (left ? 400 : 367);
    samus_y_pos = samus_prev_y_pos = 235;
    samus_pose = samus_prev_pose = left ? 2 : 1;
    samus_pose_x_dir = samus_prev_pose_x_dir = left ? 4 : 8;
    samus_anim_frame_timer = 1; samus_x_speed_table_pointer = 0x9f55;
    samus_input_handler = 0xe913; samus_movement_handler = 0xa337; grapple_beam_function = 0xc4f0;
    button_config_run_b = 0x8000; button_config_jump_a = 0x80; button_config_shoot_x = 0x40;
    button_config_aim_up_R = 0x10; button_config_aim_down_L = 0x20;
    button_config_itemcancel_y = 0x4000; button_config_itemswitch = 0x2000;
    uint16 previous = sequence == 2 ? 0x80 : 0, forward = left ? 0x200 : 0x100, back = left ? 0x100 : 0x200;
    for (int frame = 0; frame < 240; frame++) {
      nmi_frame_counter_word = frame + 2; nmi_frame_counter_byte = frame + 2;
      uint16 input = 0x40 | angle;
      if (frame >= 10) input |= back;
      if (frame >= 11 || sequence == 2) input |= 0x80;
      if (sequence == 2 && frame >= 11) input &= ~0x40;
      if ((sequence == 1 || sequence == 4) && frame >= 12) input &= ~angle;
      if ((scenario == 3 || scenario == 4) && frame >= 60 && frame < 70) input = (input & ~back) | forward;
      if ((scenario == 5 || scenario == 6) && frame >= 60) input = frame < 69 ? ((frame % 3) != 2 ? 0x400 : 0) : back;
      if (scenario == 6 && frame == 90) input = 0x800;
      if (frame >= 200) input = 0;
      joypad1_lastkeys = input; joypad1_newkeys = input & ~previous; previous = input;
      RunAsmCode(0x90e695, 0, 0, 0, 0); RunAsmCode(0xa09785, 0, 0, 0, 0);
      samus_contact_damage_index = 0;
      RunAsmCode(0x900000 | samus_movement_handler, 0, 0, 0, 0);
      RunAsmCode(0x908000, 0, 0, 0, 0); RunAsmCode(0x90dde9, 0, 0, 0, 0);
      RunAsmCode(0x91e8b6, 0, 0, 0, 0); RunAsmCode(0x91eb88, 0, 0, 0, 0);
      RunAsmCode(0x90eab3, 0, 0, 0, 0); RunAsmCode(0x90e9ce, 0, 0, 0, 0);
      RunAsmCode(0xa09169, 0, 0, 0, 0);
      fprintf(f, "%d,%d,%d,%d,%d,%04X,%04X%04X,%04X%04X,%02X,%02X,%04X,%04X,%04X%04X,%04X%04X,%04X,%02X,%04X%04X,%04X,%04X\n",
        left,enabled,medium,scenario,frame,input,samus_x_pos,samus_x_subpos,samus_y_pos,samus_y_subpos,
        samus_pose,samus_movement_type,samus_anim_frame,samus_anim_frame_timer,samus_x_base_speed,samus_x_base_subspeed,
        samus_x_extra_run_speed,samus_x_extra_run_subspeed,samus_x_accel_mode,samus_pose_x_dir,
        samus_y_speed,samus_y_subspeed,samus_y_dir,used_for_ball_bounce_on_landing);
    }
  }
  fclose(f); return 0;
}
