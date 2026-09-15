/* #433: original-CPU Reserve Mode / greyout ownership probe.
   The successful case is the exact shared-freeze-word state produced when automatic
   reserve recovery finishes under X-Ray. The adjacent control keeps the word set. */
static void reserve_mode_matrix(void) {
  enum { XrayFinish = 0x888a08, XrayPalette = 0x91dcb4 };
  printf("frozen_before,frozen_after,alpha,beta,hdma_mask,xray_phase,backdrop,palette_kind,palette_frame,palette_timer\n");
  for (int frozen = 0; frozen <= 1; frozen++) {
    memset(g_ram, 0, sizeof(g_ram));
    memset(dma_channel_registers, 0, sizeof(dma_channel_registers));
    samus_pose = samus_prev_pose = 1;
    samus_pose_x_dir = samus_prev_pose_x_dir = 8;
    samus_movement_type = samus_prev_movement_type = 0;
    samus_x_pos = samus_prev_x_pos = 256;
    samus_y_pos = samus_prev_y_pos = 256;
    frame_handler_alfa = 0xe695; frame_handler_beta = 0xe725;
    samus_input_handler = 0xe913; samus_movement_handler = 0xa337;
    time_is_frozen_flag = frozen;
    hdma_object_index = 0;
    hdma_object_channels_bitmask[0] = 4;
    demo_input_pre_instr = 5;
    samus_special_transgfx_index = 8;
    special_samus_palette_frame = 6;
    special_samus_palette_timer = 1;
    demo_timer_counter = 1;
    palette_buffer[0] = 3171;

    run(XrayFinish);
    run(XrayPalette);
    printf("%d,%04X,%04X,%04X,%02X,%04X,%04X,%04X,%04X,%04X\n",
      frozen,time_is_frozen_flag,frame_handler_alfa,frame_handler_beta,
      hdma_object_channels_bitmask[0],demo_input_pre_instr,palette_buffer[0],
      samus_special_transgfx_index,special_samus_palette_frame,special_samus_palette_timer);
  }
}
