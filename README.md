2021-jan-02-
i wanted this to have the 60 days trial using the ab3d.dxengine.OculusWrap, right out of the box, for people to use the ab3d.dxengine.oculusWrap. I modified my repos so you won't be able to make them work out of the box anymore.

You will have to go clone the repository here https://github.com/ab4d/Ab3d.OculusWrap and build the dlls separately for yourselves. If the ab3d.dxengine.oculusWrap would be provided in the future with a nugget you won't have to do those steps. 

1. Clone the github repository here: https://github.com/ab4d/Ab3d.OculusWrap
2. Build the ab3d.OculusWrap solution first with the FrameWork 4.5 or 4.7.2 whatever.
3. Then build the solution ab3d.DXEngine.OculusWrap.
4. use both the ab3d.OculusWrap.dll and the ab3d.DXEngine.OculusWrap as references for my projects sc_core_systems and SCCoreSystems and the solution sccsv10 and this one sccsv11 and that one too sccsVD4VE. Those DLLs will make the projects work. after inserting those references, rebuild your projects and this should take care of restoring the nugget packages for the other dlls to load.

thank you for reading me,
steve chassé

2020-dec-25-
OCULUS RIFT CV1 ONLY
# voxel-chunk-example
voxel chunk with instanced "integer chunk" for an alternative to using an additional buffer for the shader to update the instanced vertex position.The idea is great, but the performance suffers of course when there are so many triangles and vertices drawn on the screen. And thats an old project. Its an old project. 

i have built this program by myself using sharpdx and the ab3d.oculusWrap. this is a wpf version supposedly but it aint using xaml.

I originally posted this on the stackoverflow website here:
https://gamedev.stackexchange.com/questions/169506/vertex-manipulation-in-instanced-shader-hlsl-sharpdx-4-2?noredirect=1#comment302452_169506

The original download link for the project is still active and is here:
https://drive.google.com/open?id=1-kum0phM_iBMc8xRxYShmCNRWRs6W0Aw

I took as reference Craig Perko's C# old Minecraft tutorial https://www.youtube.com/watch?v=YpHQ-Kykp_s&ab_channel=CraigPerko in order to build the triangles and faces and vertices for the chunks. The things i have added is a simple integer that starts with a 1 and then adding a 1 or a 0 if the chunk byte/face is transparent or not. I could 
have used floats or vectors or matrices to pass on the data to the shader but i read somewhere on the web that using integers in
c# isn't a bad thing. I was having strange results when using the "floor" or "ceil" in hlsl and i would never get the right chunk byte out of a float or from the vectors or matrices. so i came out with the idea to send a long integer that gets the byte 0 or 1 if a face is supposed to be shown. I will work towards trying and getting a version from multiple floats in the vertex bindings and out of a vector or a matrix to see if i can just put in much more chunk data, whenever i have the chance to code that.

thank you for reading me.
steve chassé

Here is what it looks like... It looks like a normal chunk, but its made of instanced chunks where the chunk integer sent inside of a vertex binding to the shader with 0s or 1s inside of a single integer of 9 digits which in turns decide of the triangles to be displayed or not from within the shader itself. It's of very poor performance as the vertexes and triangles that aren't visible on the screen are still drawn and currently since they are instanced chunks (c# reference- when the objects are instanced, you can change the position/rotation color, but the number of vertices for each instances are identical per object). I thought using a Geometry shader to see if it would make me able to remove the surplus vertices but, thats not really working... as if i modify the first instance vertices and try and remove them through a geometry shader, the whole thing breaks. 
I'm going to try and find an alternative.
<img src="https://i.ibb.co/F0VpVPS/Vertex-Binding-Chunk-Data.png" alt="Vertex-Binding-Chunk-Data" border="0">





The shader code:

      cbuffer MatrixBuffer :register(b0)
      {
        float4x4 world;
        float4x4 view;
        float4x4 proj;
      };

      struct VertexInputType
      {
          float4 position : POSITION0;
          float4 indexPos : POSITION1;
          float4 color : COLOR0;
          float3 normal : NORMAL0;
          float2 tex : TEXCOORD0;
          int one : PSIZE0; 
          int two : PSIZE1; 
          int three : PSIZE2;   
          int four : PSIZE3;    
          int oneTwo : PSIZE4;  
          int twoTwo : PSIZE5;  
          int threeTwo : PSIZE6;    
          int fourTwo : PSIZE7; 
          float4 instancePosition1 : POSITION2; 
      };

      //row_major matrix instancePosition1 : WORLD;

      struct PixelInputType
      { 
        float4 position : SV_POSITION;
        float4 color : COLOR0;
        float3 normal : NORMAL0;
        float2 tex : TEXCOORD0;
      };

      //float planeSize = 0.1f;

      static int mapWidth = 4;
      static int mapHeight = 4;
      static int mapDepth = 4;

      static int tinyChunkWidth = 4;
      static int tinyChunkHeight = 4;
      static int tinyChunkDepth = 4;

      //[maxvertexcount(96)] 
      PixelInputType TextureVertexShader(VertexInputType                     input)
      {  
        PixelInputType output;
          input.position.w = 1.0f;

        int x = input.indexPos.x; 
        int y = input.indexPos.y; 
        int z = input.indexPos.z;

        int currentMapData;
        int currentByte;

        int currentIndex = x + (tinyChunkWidth * (y+(tinyChunkHeight*z)));
        int someOtherIndex = currentIndex;

        int theNumber = tinyChunkWidth;
        int remainder = 0;
        int totalTimes = 0;

        for (int i = 0;i <= currentIndex; i++)
        {           
            if (remainder == theNumber)
            {
                remainder = 0;
                totalTimes++;
            }
            if (totalTimes * theNumber >= currentIndex)//>=?? why not only >
            {
                break;
            }
            remainder++;
        }

        int arrayIndex = int(floor(totalTimes *0.5));

        switch(arrayIndex)
        {
            case 0:
                currentMapData = input.one;
                break;
                    case 1:
                currentMapData = input.oneTwo;
                break;
            case 2:
                currentMapData = input.two;
                break;
            case 3:
                currentMapData = input.twoTwo;
                break;
            case 4:
                currentMapData = input.three;
                break;
              case 5:
                currentMapData = input.threeTwo;
                break;
              case 6:
                currentMapData = input.four;
                break;
              case 7:
                currentMapData = input.fourTwo;
                break;
          }

          //0-4-1-5-2-6-3-7
          //8-12-9-13-10-14-11-15
          //16-20-17-21-18-22-19-23
          //24-28-25-29-26-30-27-31
          //32-36-33-37-34-38-35-39
          //40-44-41-45-42-46-43-47
          //48-52-49-53-50-54-51-55
          //56-60-57-61-58-62-59-63

          int baser = totalTimes;

          int someAdder = totalTimes % 2;

          someOtherIndex=7-(((someOtherIndex-(tinyChunkWidth*baser))*2)+someAdder);

          int testera = 0;
          int substract = 0;
          int before0 = 0;

          if (someOtherIndex == 0)
          {
              testera = currentMapData >> 1 << 1;
              currentByte = currentMapData - testera;
          }
          else
          {
            float someData0 = currentMapData;

            for (int i = 0; i < someOtherIndex; i++)
            {
                someData0 = int(someData0 * 0.1f);
            }
              before0 = int(trunc(someData0));
            //https://stackoverflow.com/questions/46312893/how-do-you-use-bit-shift-operators-to-find-out-a-certain-digit-of-a-number-in-ba
              testera = before0 >> 1 << 1;
              currentByte = before0 - testera;
          }

          //currentByte = 1;

          if(currentByte == 1)
          { 
            input.position.x += input.instancePosition1.x;
            input.position.y += input.instancePosition1.y;
            input.position.z += input.instancePosition1.z;

            output.position = mul(input.position, world);
            output.position = mul(output.position, view);
            output.position = mul(output.position, proj);
            output.color = input.color;
          }
          else
          {
            input.position.x = input.instancePosition1.x;
            input.position.y = input.instancePosition1.y;
            input.position.z = input.instancePosition1.z;

            output.position = mul(input.position, world);
            output.position = mul(output.position, view);
            output.position = mul(output.position, proj);
            output.color = input.color * float4(0.5f,0.5f,0.5f,1);
          }

          output.tex = input.tex;

          output.normal = mul(input.normal, world);
          output.normal = normalize(output.normal);

          return output;
      }

      /*technique Test
      {
          pass pass0 //pass1
          {
              VertexShader = compile vs_5_0 TextureVertexShader();
              //PixelShader  = compile ps_5_0 TexturePixelShader();
          }
      }*/


The chunk noise Map is based on the craig perko youtube old minecraft tutorial. But i added a way to squeeze the chunk "byte/integer" map adding the bytes into a couple of integers that i use as vertex binding elements instead of updating an HLSL "cbuffer" every frame. It's a different way to update the shader and i thought it was giving better performance than updating a "cbuffer".



      using System;
      using SharpDX;

      namespace SC_WPF_VR
      {
          public class chunk
          {
              float staticPlaneSize;
              float alternateStaticPlaneSize;

              private int[] map;
              private int seed = 3420; // 3420

              private int _detailScale = 10; // 10
              private int _HeightScale = 200; //200

              public void startBuildingArray(Vector4 currentPosition,
                  out int oneInt, out int twoInt, out int threeInt, out int fourInt,
                  out int oneIntTwo, out int twoIntTwo, out int threeIntTwo, out int fourIntTwo, out int[] mapper)
              {

                  staticPlaneSize = SC_Globals.planeSize;

                  if (staticPlaneSize == 1)
                  {
                      staticPlaneSize = SC_Globals.planeSize * 0.1f;
                      alternateStaticPlaneSize = SC_Globals.planeSize * 0.1f;
                  }
                  else if (staticPlaneSize == 0.1f)
                  {
                      staticPlaneSize = SC_Globals.planeSize;
                      alternateStaticPlaneSize = SC_Globals.planeSize *10;
                  }
                  else if (staticPlaneSize == 0.01f)
                  {
                      staticPlaneSize = SC_Globals.planeSize;
                      alternateStaticPlaneSize = SC_Globals.planeSize*1000;
                  }

                  //float staticPlaneSize = SC_Globals.planeSize; //
                  //float alternateStaticPlaneSize = SC_Globals.planeSize * 10;

                  _detailScale = 10;
                  _HeightScale = 200;

                  map = new int[SC_Globals.tinyChunkWidth * SC_Globals.tinyChunkHeight * SC_Globals.tinyChunkDepth];

                  FastNoise fastNoise = new FastNoise();

                  for (int x = 0; x < SC_Globals.tinyChunkWidth; x++)
                  {
                      for (int y = 0; y < SC_Globals.tinyChunkHeight; y++)
                      {
                          for (int z = 0; z < SC_Globals.tinyChunkDepth; z++)
                          {
                              float noiseXZ = 20;

                              noiseXZ *= fastNoise.GetNoise((((x * staticPlaneSize) + (currentPosition.X * alternateStaticPlaneSize) + seed) / _detailScale) * _HeightScale, (((y * staticPlaneSize) + (currentPosition.Y * alternateStaticPlaneSize) + seed) / _detailScale) * _HeightScale, (((z * staticPlaneSize) + (currentPosition.Z * alternateStaticPlaneSize) + seed) / _detailScale) * _HeightScale);

                              //Console.WriteLine(noiseXZ);

                              if (noiseXZ >= 0.1f)
                              {
                                  map[x + SC_Globals.tinyChunkWidth * (y + SC_Globals.tinyChunkHeight * z)] = 1;
                              }
                              else if (y == 0 && currentPosition.Y == 0)
                              {
                                  map[x + SC_Globals.tinyChunkWidth * (y + SC_Globals.tinyChunkHeight * z)] = 1;
                              }
                              else
                              {
                                  map[x + SC_Globals.tinyChunkWidth * (y + SC_Globals.tinyChunkHeight * z)] = 0;
                              }
                              //map[x + SC_Globals.tinyChunkWidth * (y + SC_Globals.tinyChunkHeight * z)] = 1;
                          }
                      }
                  }

                  int DarrayOfDeVectorMapTempX = 1;
                  int DarrayOfDeVectorMapTempY = 1;
                  int DarrayOfDeVectorMapTempZ = 1;
                  int DarrayOfDeVectorMapTempW = 1;

                  int DarrayOfDeVectorMapTempTwoX = 1;
                  int DarrayOfDeVectorMapTempTwoY = 1;
                  int DarrayOfDeVectorMapTempTwoZ = 1;
                  int DarrayOfDeVectorMapTempTwoW = 1;

                  int total = SC_Globals.tinyChunkWidth * SC_Globals.tinyChunkHeight * SC_Globals.tinyChunkDepth;

                  int xx = 0;
                  int yy = 0;
                  int zz = 0;

                  int switchXX = 0;
                  int switchYY = 0;

                  for (int t = 0; t < total; t++)
                  {
                      int currentByte = map[xx + SC_Globals.tinyChunkWidth * (yy + SC_Globals.tinyChunkHeight * zz)];

                      int index = xx + (SC_Globals.tinyChunkWidth * (yy + (SC_Globals.tinyChunkHeight * zz)));

                      if (index >= 0 && index <= 7)
                      {
                          if (currentByte == 0)
                          {
                              DarrayOfDeVectorMapTempX = (DarrayOfDeVectorMapTempX * 10);
                          }
                          else
                          {
                              DarrayOfDeVectorMapTempX = (DarrayOfDeVectorMapTempX * 10) + 1;
                          }
                      }
                      else if (index >= 8 && index <= 15)
                      {
                          if (currentByte == 0)
                          {
                              DarrayOfDeVectorMapTempTwoX = (DarrayOfDeVectorMapTempTwoX * 10);
                          }
                          else
                          {
                              DarrayOfDeVectorMapTempTwoX = (DarrayOfDeVectorMapTempTwoX * 10) + 1;
                          }
                      }
                      else if (index >= 16 && index <= 23)
                      {
                          if (currentByte == 0)
                          {
                              DarrayOfDeVectorMapTempY = (DarrayOfDeVectorMapTempY * 10);
                          }
                          else
                          {
                              DarrayOfDeVectorMapTempY = (DarrayOfDeVectorMapTempY * 10) + 1;
                          }
                      }
                      else if (index >= 24 && index <= 31)
                      {
                          if (currentByte == 0)
                          {
                              DarrayOfDeVectorMapTempTwoY = (DarrayOfDeVectorMapTempTwoY * 10);
                          }
                          else
                          {
                              DarrayOfDeVectorMapTempTwoY = (DarrayOfDeVectorMapTempTwoY * 10) + 1;
                          }
                      }

                      else if (index >= 32 && index <= 39)
                      {
                          if (currentByte == 0)
                          {
                              DarrayOfDeVectorMapTempZ = (DarrayOfDeVectorMapTempZ * 10);
                          }
                          else
                          {
                              DarrayOfDeVectorMapTempZ = (DarrayOfDeVectorMapTempZ * 10) + 1;
                          }
                      }
                      else if (index >= 40 && index <= 47)
                      {

                          if (currentByte == 0)
                          {
                              DarrayOfDeVectorMapTempTwoZ = (DarrayOfDeVectorMapTempTwoZ * 10);
                          }
                          else
                          {
                              DarrayOfDeVectorMapTempTwoZ = (DarrayOfDeVectorMapTempTwoZ * 10) + 1;
                          }

                      }
                      else if (index >= 48 && index <= 55)
                      {

                          if (currentByte == 0)
                          {
                              DarrayOfDeVectorMapTempW = (DarrayOfDeVectorMapTempW * 10);
                          }
                          else
                          {
                              DarrayOfDeVectorMapTempW = (DarrayOfDeVectorMapTempW * 10) + 1;
                          }

                      }
                      else if (index >= 56 && index <= 63)
                      {

                          if (currentByte == 0)
                          {
                              DarrayOfDeVectorMapTempTwoW = (DarrayOfDeVectorMapTempTwoW * 10);
                          }
                          else
                          {
                              DarrayOfDeVectorMapTempTwoW = (DarrayOfDeVectorMapTempTwoW * 10) + 1;
                          }
                      }

                      zz++;
                      if (zz >= SC_Globals.tinyChunkDepth)
                      {
                          yy++;
                          zz = 0;
                          switchYY = 1;
                      }
                      if (yy >= SC_Globals.tinyChunkHeight && switchYY == 1)
                      {
                          xx++;
                          yy = 0;
                          switchYY = 0;
                          switchXX = 1;
                      }
                      if (xx >= SC_Globals.tinyChunkWidth && switchXX == 1)
                      {
                          xx = 0;
                          switchXX = 0;
                          break;
                      }
                  }

                  oneInt = DarrayOfDeVectorMapTempX;
                  twoInt = DarrayOfDeVectorMapTempY;
                  threeInt = DarrayOfDeVectorMapTempZ;
                  fourInt = DarrayOfDeVectorMapTempW;

                  oneIntTwo = DarrayOfDeVectorMapTempTwoX;
                  twoIntTwo = DarrayOfDeVectorMapTempTwoY;
                  threeIntTwo = DarrayOfDeVectorMapTempTwoZ;
                  fourIntTwo = DarrayOfDeVectorMapTempTwoW;

                  mapper = map;
              }
          }
      }










