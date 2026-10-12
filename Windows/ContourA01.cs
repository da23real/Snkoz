using System;
using System.Collections.Generic;
using System.Linq;
namespace Synkoz {
 static class ContourA01 {
  // Follow each external boundary on the 256 x 256 analysis grid. The longest
  // boundary is the visible principal body; propagate its arc coordinate once.
  static float[] Coordinates(byte[] mask){const int n=256;var inside=new bool[n*n];for(int i=0;i<inside.Length;i++)inside[i]=mask[i]>=128;var seen=new bool[n*n];var best=new List<int>();int[] dx={-1,-1,0,1,1,1,0,-1},dy={0,-1,-1,-1,0,1,1,1};
   for(int start=0;start<inside.Length;start++){if(!inside[start]||seen[start]||(start% n>0&&inside[start-1]))continue;var route=new List<int>();int current=start,back=0,firstNext=-1;for(int step=0;step<n*n*2;step++){route.Add(current);seen[current]=true;int x=current%n,y=current/n,next=-1,direction=0;for(int j=0;j<8;j++){int d=(back+j)%8,nx=x+dx[d],ny=y+dy[d];if(nx>=0&&nx<n&&ny>=0&&ny<n&&inside[ny*n+nx]){next=ny*n+nx;direction=d;break;}}if(next<0)break;if(firstNext<0)firstNext=next;else if(current==start&&next==firstNext){route.RemoveAt(route.Count-1);break;}int previous=(direction+7)%8,bx=x+dx[previous],by=y+dy[previous],tx=next%n,ty=next/n;back=0;for(int d=0;d<8;d++)if(tx+dx[d]==bx&&ty+dy[d]==by){back=d;break;}current=next;}
    if(route.Count>best.Count)best=route;
   }
   var result=Enumerable.Repeat(-1f,n*n).ToArray();if(best.Count==0)return result;var queue=new Queue<int>();for(int i=0;i<best.Count;i++){int p=best[i];if(result[p]>=0)continue;result[p]=i/(float)best.Count;queue.Enqueue(p);}while(queue.Count>0){int p=queue.Dequeue(),x=p%n,y=p/n;for(int d=0;d<8;d+=2){int nx=x+dx[d],ny=y+dy[d];if(nx<0||nx>=n||ny<0||ny>=n)continue;int q=ny*n+nx;if(result[q]>=0)continue;result[q]=result[p];queue.Enqueue(q);}}return result;
  }
  public static byte[] Shape(byte[] mask,EffectRun936 run,double time){var e=run.Fx;var copy=e.MaskCopyA01();double v=run.DriverA01(time);if(e.ValueTargetA01==ValueTargetA01.Position_contour)copy.ContourPositionA01=v;else if(e.ValueTargetA01==ValueTargetA01.Décalage_masque_X)copy.MaskX=Math.Max(-100,Math.Min(100,v));else if(e.ValueTargetA01==ValueTargetA01.Décalage_masque_Y)copy.MaskY=Math.Max(-100,Math.Min(100,v));else if(e.ValueTargetA01==ValueTargetA01.Profondeur_contour)copy.MaskDepth=Math.Max(0,Math.Min(100,v));var shaped=MaskR3.Shape(mask,copy);if(!copy.SegmentA01)return shaped;float[] coordinate=Coordinates(mask);int dx=(int)Math.Round(copy.MaskX*2.56),dy=(int)Math.Round(copy.MaskY*2.56);double position=((copy.ContourPositionA01/100)%1+1)%1,length=copy.ContourLengthA01/100;
   for(int y=0;y<256;y++)for(int x=0;x<256;x++){int sx=x-dx,sy=y-dy,i=y*256+x;if(sx<0||sx>=256||sy<0||sy>=256){shaped[i]=0;continue;}double c=coordinate[sy*256+sx];double distance=(c-position+1)%1;if(c<0||distance>length)shaped[i]=0;}return shaped;
  }
 }
}
