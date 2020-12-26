using System;
using SharpDX;
using System.Runtime.InteropServices;

namespace SC_WPF_VR
{
    public class SC_VR_Chunk
    {
        public DVertex[] Vertices { get; set; }
        public int[] indices;

        private float _sizeX = 0;
        private float _sizeY = 0;
        private float _sizeZ = 0;

        public DInstanceType[] instances { get; set; }

        public DInstanceType[] instancesIndex { get; set; }

        public int[][] arrayOfSomeMap { get; set; }
        public Matrix[] totalArrayOfMatrixData { get; set; }

        [StructLayout(LayoutKind.Explicit, Size = 72)]
        public struct DVertex
        {
            [FieldOffset(0)]
            public Vector4 position;
            [FieldOffset(16)]
            public Vector4 indexPos;
            [FieldOffset(32)]
            public Vector4 color;
            [FieldOffset(48)]
            public Vector3 normal;
            [FieldOffset(64)]
            public Vector2 tex;
        }


        [StructLayout(LayoutKind.Explicit, Size = 48)] //, Pack = 48
        public struct DInstanceType
        {
            [FieldOffset(0)]
            public int one;
            [FieldOffset(4)]
            public int two;
            [FieldOffset(8)]
            public int three;
            [FieldOffset(12)]
            public int four;
            [FieldOffset(16)]
            public int oneTwo;
            [FieldOffset(20)]
            public int twoTwo;
            [FieldOffset(24)]
            public int threeTwo;
            [FieldOffset(28)]
            public int fourTwo;
            [FieldOffset(32)]
            public Vector4 instancePos;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 16)]
        public struct DInstanceTypeTwo
        {
            public Matrix matrix;
        };


        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct DIndexType
        {
            public int indexPos;
        };

        public SC_VR_Chunk(float xxi, float yyi, float zzi, Vector4 color, int width, int height, int depth, Vector3 pos)
        {
            this._color = color;
            this._sizeX = xxi;
            this._sizeY = yyi;
            this._sizeZ = zzi;

            this._chunkPos = pos;

            int[] theMap;

            instances = new DInstanceType[SC_Globals.numberOfInstancesPerObjectInWidth * SC_Globals.numberOfInstancesPerObjectInHeight * SC_Globals.numberOfInstancesPerObjectInDepth];
            instancesIndex = new DInstanceType[SC_Globals.numberOfInstancesPerObjectInWidth * SC_Globals.numberOfInstancesPerObjectInHeight * SC_Globals.numberOfInstancesPerObjectInDepth];

            arrayOfSomeMap = new int[SC_Globals.numberOfInstancesPerObjectInWidth * SC_Globals.numberOfInstancesPerObjectInHeight * SC_Globals.numberOfInstancesPerObjectInDepth][];

            Vector4 position;
            chunk newChunker;

            int oner;
            int twoer;
            int threer;
            int fourer;

            int onerTwo;
            int twoerTwo;
            int threerTwo;
            int fourerTwo;

            for (int x = 0; x < SC_Globals.numberOfInstancesPerObjectInWidth; x++)
            {
                for (int y = 0; y < SC_Globals.numberOfInstancesPerObjectInHeight; y++)
                {
                    for (int z = 0; z < SC_Globals.numberOfInstancesPerObjectInDepth; z++)
                    {
                        position = new Vector4(x, y, z, 1);
                        newChunker = new chunk();

                        position.X *= (SC_Globals.tinyChunkWidth);
                        position.Y *= (SC_Globals.tinyChunkHeight);
                        position.Z *= (SC_Globals.tinyChunkDepth);

                        position.X *= (SC_Globals.planeSize);
                        position.Y *= (SC_Globals.planeSize);
                        position.Z *= (SC_Globals.planeSize);

                        position.X += (_chunkPos.X);
                        position.Y += (_chunkPos.Y);
                        position.Z += (_chunkPos.Z);

                        newChunker.startBuildingArray(position, out oner, out twoer, out threer, out fourer, out onerTwo, out twoerTwo, out threerTwo, out fourerTwo, out theMap);

                        //_matrix = Matrix.Identity;

                        /*_matrix.M11 = position.X;
                        _matrix.M12 = position.Y;
                        _matrix.M13 = position.Z;
                        _matrix.M14 = position.W; // can be switched for something else as it's not even required in the shader.

                        _matrix.M21 = VectorTemp.X;
                        _matrix.M22 = VectorTemp.Y;
                        _matrix.M23 = VectorTemp.Z;
                        _matrix.M24 = VectorTemp.W;

                        _matrix.M31 = VectorTempTwo.X;
                        _matrix.M32 = VectorTempTwo.Y;
                        _matrix.M33 = VectorTempTwo.Z;
                        _matrix.M34 = VectorTempTwo.W;*/

                        //_matrix.M41 = X;
                        //_matrix.M42 = Y;
                        //_matrix.M43 = Z;
                        //_matrix.M44 = W;

                        //It is less convoluted when the data is inside of 1 Matrix isntead of 8 ints.

                        instances[x + SC_Globals.numberOfInstancesPerObjectInWidth * (y + SC_Globals.numberOfInstancesPerObjectInHeight * z)] = new DInstanceType()
                        {                        
                            one = oner,
                            two = twoer,
                            three = threer,
                            four = fourer,
                            oneTwo = onerTwo,
                            twoTwo = twoerTwo,
                            threeTwo = threerTwo,
                            fourTwo = fourerTwo,
                            instancePos = new Vector4(position.X, position.Y, position.Z, 1),
                        };

                        arrayOfSomeMap[x + SC_Globals.numberOfInstancesPerObjectInWidth * (y + SC_Globals.numberOfInstancesPerObjectInHeight * z)] = theMap;                      
                    }
                }
            }
        }

        public int InstanceCount = 0;

        public Vector4 _color;

        public int instanceCounter { get; set; }
        public byte[] map { get; set; }

        public Vector3 _chunkPos { get; set; }

    }
}