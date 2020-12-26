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