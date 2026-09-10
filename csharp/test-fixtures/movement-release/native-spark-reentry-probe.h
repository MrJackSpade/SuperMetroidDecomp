#include "native-spark-ownership-probe.h"

// Enter another crash after a combo overwrites the first departing projectile.
// Seed only the handler boundary; all echo/delta writes come from original code.
int DiagnosticSparkReentry(const char *rom, const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  const int beams[]={1,2,4,8};
  fprintf(f,"beam,left,stage,count,type3,pre3,draw3,x3,y3,type4,pre4,draw4,x4,y4\n");
  for(int beam=0;beam<4;beam++) for(int left=0;left<2;left++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    samus_x_pos=samus_y_pos=128; samus_pose=left?0xca:0xc9; samus_pose_x_dir=left?4:8;
    equipped_beams=0x1000|beams[beam]; samus_power_bombs=2; hud_item_index=3;
    ProbeRunBounded(0x90d40d);
    ProbeRunBounded(0x90ccc0);
    SparkOwnershipRow(f,beams[beam],left,0);
    samus_health=29;
    ProbeRunBounded(0x90d2ba);
    SparkOwnershipRow(f,beams[beam],left,1);
    for(int frame=0;frame<5;frame++) {
      ProbeRunBounded(0x900000|samus_movement_handler);
      SparkOwnershipRow(f,beams[beam],left,frame+2);
    }
  }
  fclose(f); return 0;
}
