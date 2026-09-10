#include "native-bounded-cpu.h"

int DiagnosticComboGrapple(const char *rom, const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  fprintf(f,"beam,cooldown,edge,previous,firing,count,pb,result_cooldown,end_x,end_y,types\n");
  const int beams[]={1,2,4,8};
  for(int b=0;b<4;b++) for(int c=0;c<2;c++) for(int edge=0;edge<2;edge++)
  for(int previous=0;previous<2;previous++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    samus_pose=1; samus_pose_x_dir=8; samus_x_pos=samus_y_pos=128;
    equipped_beams=0x1000|beams[b]; samus_power_bombs=2; hud_item_index=3;
    ProbeRunBounded(0x90ccc0);
    hud_item_index=4; cooldown_timer=c?2:0; grapple_beam_function=0xc4f0;
    button_config_shoot_x=0x40; joypad1_newkeys=edge?0x40:0;
    joypad1_newinput_samusfilter=previous?0x40:0;
    ProbeRunBounded(0x90dd3d);
    fprintf(f,"%d,%d,%d,%d,%d,%04X,%04X,%04X,%04X,%04X,",beams[b],c?2:0,edge,previous,
      grapple_beam_function==FUNC16(GrappleBeamFunc_Firing),projectile_counter,samus_power_bombs,
      cooldown_timer,grapple_beam_end_x_pos,grapple_beam_end_y_pos);
    for(int i=0;i<4;i++) fprintf(f,"%04X",projectile_type[i]);
    fprintf(f,"\n");
  }
  fclose(f); return 0;
}
