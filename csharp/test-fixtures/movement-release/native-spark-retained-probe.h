#include "native-bounded-cpu.h"

// Handler boundary evidence only. Retained echo-Y values are explicitly seeded;
// a later integrated sequence must prove which combos/echoes leave these words.
int DiagnosticSparkRetained(const char *rom, const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  const int retained[]={0,64,128,192};
  fprintf(f,"left,retained,frame,handler,index,travel,x0,y0,x1,y1\n");
  for(int left=0;left<2;left++) for(int seed=0;seed<4;seed++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    samus_x_pos=samus_y_pos=192; samus_pose_x_dir=left?4:8;
    samus_health=29; speed_echo_ypos[2]=retained[seed];
    ProbeRunBounded(0x90d2ba);
    for(int frame=0;frame<100;frame++) {
      if(samus_movement_handler==0xd40d) break;
      ProbeRunBounded(0x900000|samus_movement_handler);
      fprintf(f,"%d,%d,%d,%04X,%04X,%04X,%04X,%04X,%04X,%04X\n",
        left,retained[seed],frame,samus_movement_handler,speed_echoes_index,
        speed_echo_ypos[2],speed_echo_xpos[0],speed_echo_ypos[0],speed_echo_xpos[1],speed_echo_ypos[1]);
    }
  }
  fclose(f); return 0;
}
