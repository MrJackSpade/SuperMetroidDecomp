/* Prepared shared-word boundaries, then the unmodified native gamma handler.
   This is arithmetic/input coverage, not a claim that all seeds arise in play. */
static void edge_matrix(void) {
  const uint16 seeds[] = {0,1,58,59,60,61,0x7fff,0x8000,0x803a,0x803b,0xfffe,0xffff};
  printf("right,locked,seed,previous,input,counter,last,active,pose,suppressed\n");
  for (int right = 0; right < 2; right++)
  for (int locked = 0; locked < 2; locked++)
  for (int seed = 0; seed < 12; seed++)
  for (int previous = 0; previous < 16; previous++)
  for (int input = 0; input < 16; input++) {
    memset(g_ram, 0, sizeof(g_ram));
    samus_pose = samus_prev_pose = right ? 1 : 2;
    samus_pose_x_dir = samus_prev_pose_x_dir = right ? 8 : 4;
    grab(right);
    substate = seeds[seed]; suit_pickup_light_beam_pos = previous << 8;
    joypad1_newkeys = (input << 8) | 0x40;
    grapple_beam_function = locked ? 0xc77e : 0xc4f0;
    samus_new_pose = 1; samus_momentum_routine_index = 1;
    run(0x900000 | GrabGamma);
    printf("%d,%d,%04X,%04X,%04X,%04X,%04X,%d,%02X,%d\n",right,locked,seeds[seed],previous<<8,
      joypad1_newkeys,substate,suit_pickup_light_beam_pos,frame_handler_gamma==GrabGamma,samus_pose,
      samus_new_pose==0xffff && samus_momentum_routine_index==0);
  }
}
