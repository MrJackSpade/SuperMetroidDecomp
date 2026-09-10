// Exploratory #445/#471: find a real-input far-side platform walljump with carried speed.
int DiagnosticContinuousWalljumpSearch(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f, "left,shift,jump,frame,x,y,base,extra,charge\n");
  for (int left = 0; left < 2; left++) for (int shift = -8; shift <= 8; shift++)
  for (int jump = 140; jump <= 150; jump++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    room_width_in_blocks = 144; room_height_in_blocks = 80;
    room_width_in_scrolls = 9; room_height_in_scrolls = 5; room_size_in_blocks = 144 * 80 * 2;
    interactive_enemy_indexes[0] = 0xffff;
    for (int y = 0; y <= 16; y++) for (int x = 0; x < 144; x++)
      if (y == 16 || x == 1 || x == 142 || x == (left ? 56 : 87) && y >= 11) level_data[y * 144 + x] = 0x8000;
    fx_y_pos = lava_acid_y_pos = 0xffff;
    equipped_items = 0x1004; equipped_beams = 0x1000;
    samus_health = samus_max_health = 99;
    samus_x_pos = samus_prev_x_pos = (left ? 1279 : 1024) + shift; samus_y_pos = samus_prev_y_pos = 235;
    samus_pose = samus_prev_pose = left ? 2 : 1;
    samus_pose_x_dir = samus_prev_pose_x_dir = left ? 4 : 8;
    samus_anim_frame_timer = 1; samus_x_speed_table_pointer = 0x9f55;
    samus_input_handler = 0xe913; samus_movement_handler = 0xa337; grapple_beam_function = 0xc4f0;
    button_config_run_b = 0x8000; button_config_jump_a = 0x80; button_config_shoot_x = 0x40;
    button_config_aim_up_R = 0x10; button_config_aim_down_L = 0x20;
    button_config_itemcancel_y = 0x4000; button_config_itemswitch = 0x2000;
    uint16 previous = 0;
    for (int frame = 0; frame < jump + 8; frame++) {
      nmi_frame_counter_word = frame + 2; nmi_frame_counter_byte = frame + 2;
      uint16 input = frame <= 70 ? 0x40 : 0;
      if (frame >= 30 && frame != jump - 2) input |= left ? 0x200 : 0x100;
      if (frame >= 30 && frame < 70) input |= 0x8000;
      if (frame >= 70 && frame < jump - 1 || frame >= jump) input |= 0x80;
      joypad1_lastkeys = input; joypad1_newkeys = input & ~previous; previous = input;
      RunAsmCode(0x90e695, 0, 0, 0, 0); RunAsmCode(0xa09785, 0, 0, 0, 0);
      samus_contact_damage_index = 0;
      RunAsmCode(0x900000 | samus_movement_handler, 0, 0, 0, 0);
      RunAsmCode(0x908000, 0, 0, 0, 0); RunAsmCode(0x90dde9, 0, 0, 0, 0);
      RunAsmCode(0x91e8b6, 0, 0, 0, 0); RunAsmCode(0x91eb88, 0, 0, 0, 0);
      RunAsmCode(0x90eab3, 0, 0, 0, 0); RunAsmCode(0x90e9ce, 0, 0, 0, 0);
      RunAsmCode(0xa09169, 0, 0, 0, 0);
      if (samus_movement_type == 20) {
        fprintf(f, "%d,%d,%d,%d,%04X%04X,%04X%04X,%04X%04X,%04X%04X,%04X\n",
          left,shift,jump,frame,samus_x_pos,samus_x_subpos,samus_y_pos,samus_y_subpos,
          samus_x_base_speed,samus_x_base_subspeed,samus_x_extra_run_speed,samus_x_extra_run_subspeed,flare_counter);
        break;
      }
    }
  }
  fclose(f); return 0;
}
