using System;

namespace SC_WPF_VR
{
    public static class SC_Globals
    {
        public const int tinyChunkWidth = 4; // CANNOT BE CHANGED FOR THE MOMENT. VERTEX BINDING LIMITATION
        public const int tinyChunkHeight = 4; // CANNOT BE CHANGED FOR THE MOMENT. VERTEX BINDING LIMITATION
        public const int tinyChunkDepth = 4; // CANNOT BE CHANGED FOR THE MOMENT. VERTEX BINDING LIMITATION

        public const int numberOfInstancesPerObjectInWidth = 4; // CAN BE CHANGED
        public const int numberOfInstancesPerObjectInHeight = 4; // CAN BE CHANGED
        public const int numberOfInstancesPerObjectInDepth = 4; // CAN BE CHANGED

        public const int numberOfObjectInWidth = 4; // CAN BE CHANGED
        public const int numberOfObjectInHeight = 4; // CAN BE CHANGED
        public const int numberOfObjectInDepth = 4; // CAN BE CHANGED

        //THIS SETTING WORKS AT 1f and 0.1f. OTHERWISE FAILING THE PERLIN NOISE IN THE chunk.cs script.
        public const float planeSize = 0.1f;  

    }
}



