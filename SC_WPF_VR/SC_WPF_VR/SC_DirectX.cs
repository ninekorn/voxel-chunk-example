using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;
using System.ComponentModel;
using System.Runtime.InteropServices;

using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Threading;
using SharpDX.DirectInput;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using Matrix = SharpDX.Matrix;

using Ab3d.OculusWrap.DemoDX11;
using Ab3d.OculusWrap;
using Result = Ab3d.OculusWrap.Result;

using ovrSession = System.IntPtr;
using ovrTextureSwapChain = System.IntPtr;
using ovrMirrorTexture = System.IntPtr;

namespace SC_WPF_VR
{
    public class SC_DirectX
    {
        //OVR
        IntPtr sessionPtr;
        Texture2D mirrorTextureD3D = null;
        EyeTexture[] eyeTextures = null;
        DeviceContext immediateContext = null;
        DepthStencilState depthStencilState = null;
        DepthStencilView depthStencilView = null;
        Texture2D depthBuffer = null;
        RenderTargetView backBufferRenderTargetView = null;
        Texture2D backBuffer = null;
        SharpDX.DXGI.SwapChain swapChain = null;
        Factory factory = null;
        MirrorTexture mirrorTexture = null;
        Guid textureInterfaceId = new Guid("6f15aaf2-d208-4e89-9ab4-489535d34f9c"); // Interface ID of the Direct3D Texture2D interface.
        Result result;
        OvrWrap OVR;
        HmdDesc hmdDesc;
        LayerEyeFov layerEyeFov;
        EyeTexture eyeTexture;
        //OVR

        public static SharpDX.Matrix originRot = SharpDX.Matrix.Identity;
        public static SharpDX.Matrix rotatingMatrix = SharpDX.Matrix.Identity;

        public SC_VR_Chunk.DInstanceType[] instances { get; set; }

        SC_VR_Chunk[] arrayOfChunks;
        chunkData[] arrayOfChunkData;

        public static Device device;

        public static DeviceContext context;

        public int InstanceCount;
        public static SharpDX.DirectInput.Keyboard _Keyboard;

        Vector3 VRPos = new Vector3(0, 0, 0);

        public VertexShader VertexShader;
        public PixelShader PixelShader;

        public InputLayout Layout;
        //SC_ThreadPool threadPool;
        public static System.Windows.Forms.Control MainControl;

        public static float RotationY { get; set; }
        public static float RotationX { get; set; }
        public static float RotationZ { get; set; }

        Matrix _worldMatrix;
        Matrix _viewMatrix;
        Matrix _projectionMatrix;
        Vector3 originPos = new Vector3(0, 1, 0);
        Matrix _WorldMatrix = Matrix.Identity;
        DMatrixBuffer[] arrayOfMatrixBuff = new DMatrixBuffer[1];
        SC_VR_Chunk_Shader shaderOfChunk;

        DLightBuffer[] lightBuffer = new DLightBuffer[1];

        RasterizerState rasterState;
        DepthStencilState depthState;
        SamplerState samplerState;
        BlendState blendState;
        chunkTerrain chunkTerrain = new chunkTerrain();

        [StructLayout(LayoutKind.Explicit)]
        public struct DMatrixBuffer
        {
            [FieldOffset(0)]
            public Matrix world;
            [FieldOffset(64)]
            public Matrix view;
            [FieldOffset(128)]
            public Matrix proj;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct DLightBuffer
        {
            [FieldOffset(0)]
            public Vector4 ambientColor;
            [FieldOffset(16)]
            public Vector4 diffuseColor;
            [FieldOffset(32)]
            public Vector3 lightDirection;
            [FieldOffset(44)]
            public float padding; // Added extra padding so structure is a multiple of 16.
        }

        SC_VR_Chunk.DVertex[] originalArrayOfVerts;
        int[] originalArrayOfIndices;
        int[] originalMap;
        public SC_DirectX(IntPtr hwnd, MainWindow currentWindow)
        {
            Result result;

            OVR = OvrWrap.Create();

            InitParams initializationParameters = new InitParams();
            initializationParameters.Flags = InitFlags.Debug | InitFlags.RequestVersion;
            initializationParameters.RequestedMinorVersion = 17;

            string errorReason = null;
            try
            {
                result = OVR.Initialize(initializationParameters);

                if (result < Result.Success)
                    errorReason = result.ToString();
            }
            catch (Exception ex)
            {
                errorReason = ex.Message;
            }

            if (errorReason != null)
            {
                System.Windows.Forms.MessageBox.Show("Failed to initialize the Oculus runtime library:\r\n" + errorReason, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Use the head mounted display.
            sessionPtr = IntPtr.Zero;
            var graphicsLuid = new GraphicsLuid();
            result = OVR.Create(ref sessionPtr, ref graphicsLuid);
            if (result < Result.Success)
            {
                System.Windows.Forms.MessageBox.Show("The HMD is not enabled: " + result.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            hmdDesc = OVR.GetHmdDesc(sessionPtr);

            try
            {
                SwapChainDescription swapChainDescription = new SwapChainDescription();
                swapChainDescription.BufferCount = 1;
                swapChainDescription.IsWindowed = true;
                swapChainDescription.OutputHandle = hwnd;
                swapChainDescription.SampleDescription = new SampleDescription(1, 0);
                swapChainDescription.Usage = Usage.RenderTargetOutput | Usage.ShaderInput;
                swapChainDescription.SwapEffect = SwapEffect.Sequential;
                swapChainDescription.Flags = SwapChainFlags.AllowModeSwitch;
                swapChainDescription.ModeDescription.Width = (int)currentWindow.mainDXWindow.ActualWidth;
                swapChainDescription.ModeDescription.Height = (int)currentWindow.mainDXWindow.ActualHeight;
                swapChainDescription.ModeDescription.Format = Format.R8G8B8A8_UNorm;
                swapChainDescription.ModeDescription.RefreshRate.Numerator = 0;
                swapChainDescription.ModeDescription.RefreshRate.Denominator = 1;
                // Create a set of layers to submit.
                eyeTextures = new EyeTexture[2];

                // Create DirectX drawing device.
                //device = new Device(SharpDX.Direct3D.DriverType.Hardware, DeviceCreationFlags.Debug);
                Device.CreateWithSwapChain(SharpDX.Direct3D.DriverType.Hardware, DeviceCreationFlags.None, swapChainDescription, out device, out swapChain);

                //var prec = device.CheckShaderMinimumPrecisionSupport();
                //Console.WriteLine(prec.AllOtherShaderStagesMinPrecision);

                factory = new SharpDX.DXGI.Factory4();

                immediateContext = device.ImmediateContext;

                // Retrieve the back buffer of the swap chain.
                backBuffer = swapChain.GetBackBuffer<Texture2D>(0);
                backBufferRenderTargetView = new RenderTargetView(device, backBuffer);

                // Create a depth buffer, using the same width and height as the back buffer.
                Texture2DDescription depthBufferDescription = new Texture2DDescription();
                depthBufferDescription.Format = Format.D32_Float;
                depthBufferDescription.ArraySize = 1;
                depthBufferDescription.MipLevels = 1;
                depthBufferDescription.Width = (int)currentWindow.mainDXWindow.ActualWidth;
                depthBufferDescription.Height = (int)currentWindow.mainDXWindow.ActualHeight;
                depthBufferDescription.SampleDescription = new SampleDescription(1, 0);
                depthBufferDescription.Usage = ResourceUsage.Default;
                depthBufferDescription.BindFlags = BindFlags.DepthStencil;
                depthBufferDescription.CpuAccessFlags = CpuAccessFlags.None;
                depthBufferDescription.OptionFlags = ResourceOptionFlags.None;

                // Define how the depth buffer will be used to filter out objects, based on their distance from the viewer.
                DepthStencilStateDescription depthStencilStateDescription = new DepthStencilStateDescription();
                depthStencilStateDescription.IsDepthEnabled = true;
                depthStencilStateDescription.DepthComparison = Comparison.Less;
                depthStencilStateDescription.DepthWriteMask = DepthWriteMask.Zero;

                // Create the depth buffer.
                depthBuffer = new Texture2D(device, depthBufferDescription);
                depthStencilView = new DepthStencilView(device, depthBuffer);
                depthStencilState = new DepthStencilState(device, depthStencilStateDescription);

                var viewport = new Viewport(0, 0, hmdDesc.Resolution.Width, hmdDesc.Resolution.Height, 0.0f, 1.0f);

                immediateContext.OutputMerger.SetDepthStencilState(depthStencilState);
                immediateContext.OutputMerger.SetRenderTargets(depthStencilView, backBufferRenderTargetView);
                immediateContext.Rasterizer.SetViewport(viewport);

                // Retrieve the DXGI device, in order to set the maximum frame latency.
                using (SharpDX.DXGI.Device1 dxgiDevice = device.QueryInterface<SharpDX.DXGI.Device1>())
                {
                    dxgiDevice.MaximumFrameLatency = 1;
                }

                layerEyeFov = new LayerEyeFov();
                layerEyeFov.Header.Type = LayerType.EyeFov;
                layerEyeFov.Header.Flags = LayerFlags.None;

                for (int eyeIndex = 0; eyeIndex < 2; eyeIndex++)
                {
                    EyeType eye = (EyeType)eyeIndex;
                    eyeTexture = new EyeTexture();
                    eyeTextures[eyeIndex] = eyeTexture;

                    // Retrieve size and position of the texture for the current eye.
                    eyeTexture.FieldOfView = hmdDesc.DefaultEyeFov[eyeIndex];
                    eyeTexture.TextureSize = OVR.GetFovTextureSize(sessionPtr, eye, hmdDesc.DefaultEyeFov[eyeIndex], 1.0f);
                    eyeTexture.RenderDescription = OVR.GetRenderDesc(sessionPtr, eye, hmdDesc.DefaultEyeFov[eyeIndex]);
                    eyeTexture.HmdToEyeViewOffset = eyeTexture.RenderDescription.HmdToEyePose.Position;
                    eyeTexture.ViewportSize.Position = new Vector2i(0, 0);
                    eyeTexture.ViewportSize.Size = eyeTexture.TextureSize;
                    eyeTexture.Viewport = new Viewport(0, 0, eyeTexture.TextureSize.Width, eyeTexture.TextureSize.Height, 0.0f, 1.0f);

                    // Define a texture at the size recommended for the eye texture.
                    eyeTexture.Texture2DDescription = new Texture2DDescription();
                    eyeTexture.Texture2DDescription.Width = eyeTexture.TextureSize.Width;
                    eyeTexture.Texture2DDescription.Height = eyeTexture.TextureSize.Height;
                    eyeTexture.Texture2DDescription.ArraySize = 1;
                    eyeTexture.Texture2DDescription.MipLevels = 1;
                    eyeTexture.Texture2DDescription.Format = Format.R8G8B8A8_UNorm;
                    eyeTexture.Texture2DDescription.SampleDescription = new SampleDescription(1, 0);
                    eyeTexture.Texture2DDescription.Usage = ResourceUsage.Default;
                    eyeTexture.Texture2DDescription.CpuAccessFlags = CpuAccessFlags.None;
                    eyeTexture.Texture2DDescription.BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget;

                    // Convert the SharpDX texture description to the Oculus texture swap chain description.
                    TextureSwapChainDesc textureSwapChainDesc = SharpDXHelpers.CreateTextureSwapChainDescription(eyeTexture.Texture2DDescription);

                    // Create a texture swap chain, which will contain the textures to render to, for the current eye.
                    IntPtr textureSwapChainPtr;

                    result = OVR.CreateTextureSwapChainDX(sessionPtr, device.NativePointer, ref textureSwapChainDesc, out textureSwapChainPtr);
                    WriteErrorDetails(OVR, result, "Failed to create swap chain.");

                    eyeTexture.SwapTextureSet = new TextureSwapChain(OVR, sessionPtr, textureSwapChainPtr);


                    // Retrieve the number of buffers of the created swap chain.
                    int textureSwapChainBufferCount;
                    result = eyeTexture.SwapTextureSet.GetLength(out textureSwapChainBufferCount);
                    WriteErrorDetails(OVR, result, "Failed to retrieve the number of buffers of the created swap chain.");

                    // Create room for each DirectX texture in the SwapTextureSet.
                    eyeTexture.Textures = new Texture2D[textureSwapChainBufferCount];
                    eyeTexture.RenderTargetViews = new RenderTargetView[textureSwapChainBufferCount];

                    // Create a texture 2D and a render target view, for each unmanaged texture contained in the SwapTextureSet.
                    for (int textureIndex = 0; textureIndex < textureSwapChainBufferCount; textureIndex++)
                    {
                        // Retrieve the Direct3D texture contained in the Oculus TextureSwapChainBuffer.
                        IntPtr swapChainTextureComPtr = IntPtr.Zero;
                        result = eyeTexture.SwapTextureSet.GetBufferDX(textureIndex, textureInterfaceId, out swapChainTextureComPtr);
                        WriteErrorDetails(OVR, result, "Failed to retrieve a texture from the created swap chain.");

                        // Create a managed Texture2D, based on the unmanaged texture pointer.
                        eyeTexture.Textures[textureIndex] = new Texture2D(swapChainTextureComPtr);

                        // Create a render target view for the current Texture2D.
                        eyeTexture.RenderTargetViews[textureIndex] = new RenderTargetView(device, eyeTexture.Textures[textureIndex]);
                    }

                    // Define the depth buffer, at the size recommended for the eye texture.
                    eyeTexture.DepthBufferDescription = new Texture2DDescription();
                    eyeTexture.DepthBufferDescription.Format = Format.D32_Float;
                    eyeTexture.DepthBufferDescription.Width = eyeTexture.TextureSize.Width;
                    eyeTexture.DepthBufferDescription.Height = eyeTexture.TextureSize.Height;
                    eyeTexture.DepthBufferDescription.ArraySize = 1;
                    eyeTexture.DepthBufferDescription.MipLevels = 1;
                    eyeTexture.DepthBufferDescription.SampleDescription = new SampleDescription(1, 0);
                    eyeTexture.DepthBufferDescription.Usage = ResourceUsage.Default;
                    eyeTexture.DepthBufferDescription.BindFlags = BindFlags.DepthStencil;
                    eyeTexture.DepthBufferDescription.CpuAccessFlags = CpuAccessFlags.None;
                    eyeTexture.DepthBufferDescription.OptionFlags = ResourceOptionFlags.None;

                    // Create the depth buffer.
                    eyeTexture.DepthBuffer = new Texture2D(device, eyeTexture.DepthBufferDescription);
                    eyeTexture.DepthStencilView = new DepthStencilView(device, eyeTexture.DepthBuffer);

                    // Specify the texture to show on the HMD.
                    if (eyeIndex == 0)
                    {
                        layerEyeFov.ColorTextureLeft = eyeTexture.SwapTextureSet.TextureSwapChainPtr;
                        layerEyeFov.ViewportLeft.Position = new Vector2i(0, 0);
                        layerEyeFov.ViewportLeft.Size = eyeTexture.TextureSize;
                        layerEyeFov.FovLeft = eyeTexture.FieldOfView;
                    }
                    else
                    {
                        layerEyeFov.ColorTextureRight = eyeTexture.SwapTextureSet.TextureSwapChainPtr;
                        layerEyeFov.ViewportRight.Position = new Vector2i(0, 0);
                        layerEyeFov.ViewportRight.Size = eyeTexture.TextureSize;
                        layerEyeFov.FovRight = eyeTexture.FieldOfView;
                    }
                }

                MirrorTextureDesc mirrorTextureDescription = new MirrorTextureDesc();
                mirrorTextureDescription.Format = TextureFormat.R8G8B8A8_UNorm_SRgb;
                mirrorTextureDescription.Width = (int)currentWindow.mainDXWindow.ActualWidth;
                mirrorTextureDescription.Height = (int)currentWindow.mainDXWindow.ActualHeight;
                mirrorTextureDescription.MiscFlags = TextureMiscFlags.None;

                IntPtr mirrorTexturePtr;
                result = OVR.CreateMirrorTextureDX(sessionPtr, device.NativePointer, ref mirrorTextureDescription, out mirrorTexturePtr);
                WriteErrorDetails(OVR, result, "Failed to create mirror texture.");

                mirrorTexture = new MirrorTexture(OVR, sessionPtr, mirrorTexturePtr);

                IntPtr mirrorTextureComPtr = IntPtr.Zero;
                result = mirrorTexture.GetBufferDX(textureInterfaceId, out mirrorTextureComPtr);
                WriteErrorDetails(OVR, result, "Failed to retrieve the texture from the created mirror texture buffer.");

                mirrorTextureD3D = new Texture2D(mirrorTextureComPtr);

                int startOnce = 1;

                var backgroundWorker0 = new BackgroundWorker();
                backgroundWorker0.DoWork += (object senderer, DoWorkEventArgs argers) =>
                {
                    if (startOnce == 1)
                    {
                        arrayOfChunks = new SC_VR_Chunk[SC_Globals.numberOfObjectInWidth * SC_Globals.numberOfObjectInHeight * SC_Globals.numberOfObjectInDepth];

                        InstanceCount = SC_Globals.numberOfInstancesPerObjectInWidth * SC_Globals.numberOfInstancesPerObjectInHeight * SC_Globals.numberOfInstancesPerObjectInDepth;
                        instances = new SC_VR_Chunk.DInstanceType[SC_Globals.numberOfInstancesPerObjectInWidth * SC_Globals.numberOfInstancesPerObjectInHeight * SC_Globals.numberOfInstancesPerObjectInDepth];

                        var vsFileNameByteArray = SC_WPF_VR.Properties.Resources.textureTrigVS;
                        var psFileNameByteArray = SC_WPF_VR.Properties.Resources.textureTrigPS;

                        ShaderBytecode vertexShaderByteCode = ShaderBytecode.Compile(vsFileNameByteArray, "TextureVertexShader", "vs_5_0", ShaderFlags.None, SharpDX.D3DCompiler.EffectFlags.None);
                        ShaderBytecode pixelShaderByteCode = ShaderBytecode.Compile(psFileNameByteArray, "TexturePixelShader", "ps_5_0", ShaderFlags.None, SharpDX.D3DCompiler.EffectFlags.None);

                        VertexShader = new VertexShader(device, vertexShaderByteCode);
                        PixelShader = new PixelShader(device, pixelShaderByteCode);

                        InputElement[] inputElements = new InputElement[]
                        {
                            new InputElement()
                            {
                                SemanticName = "POSITION",
                                SemanticIndex = 0,
                                Format = SharpDX.DXGI.Format.R32G32B32A32_Float,
                                Slot = 0,
                                AlignedByteOffset = 0,
                                Classification = InputClassification.PerVertexData,
                                InstanceDataStepRate = 0
                            },
                            new InputElement()
                            {
                                SemanticName = "POSITION",
                                SemanticIndex = 1,
                                Format = SharpDX.DXGI.Format.R32G32B32A32_Float,
                                Slot = 0,
                                AlignedByteOffset = InputElement.AppendAligned,
                                Classification = InputClassification.PerVertexData,
                                InstanceDataStepRate = 0
                            },
                            new InputElement()
                            {
                                SemanticName = "COLOR",
                                SemanticIndex = 0,
                                Format = SharpDX.DXGI.Format.R32G32B32A32_Float,
                                Slot = 0,
                                AlignedByteOffset =InputElement.AppendAligned,
                                Classification = InputClassification.PerVertexData,
                                InstanceDataStepRate = 0
                            },
                            new InputElement()
                            {
                                SemanticName = "NORMAL",
                                SemanticIndex = 0,
                                Format = SharpDX.DXGI.Format.R32G32B32A32_Float,
                                Slot = 0,
                                AlignedByteOffset = InputElement.AppendAligned,
                                Classification =InputClassification.PerVertexData,
                                InstanceDataStepRate = 0
                            },
                            new InputElement()
                            {
                                SemanticName = "TEXCOORD",
                                SemanticIndex = 0,
                                Format = SharpDX.DXGI.Format.R32G32B32A32_Float,
                                Slot = 0,
                                AlignedByteOffset =InputElement.AppendAligned,
                                Classification =InputClassification.PerVertexData,
                                InstanceDataStepRate = 0
                            },


                            new InputElement()
                            {
                                SemanticName = "PSIZE",
                                SemanticIndex = 0,
                                Format = SharpDX.DXGI.Format.R32_SInt,
                                Slot = 1,
                                AlignedByteOffset = 0,
                                Classification = InputClassification.PerInstanceData,
                                InstanceDataStepRate = 1
                            },
                            new InputElement()
                            {
                                SemanticName = "PSIZE",
                                SemanticIndex = 1,
                                Format = SharpDX.DXGI.Format.R32_SInt,
                                Slot = 1,
                                AlignedByteOffset = InputElement.AppendAligned,
                                Classification = InputClassification.PerInstanceData,
                                InstanceDataStepRate = 1
                            },
                            new InputElement()
                            {
                                SemanticName = "PSIZE",
                                SemanticIndex = 2,
                                Format = SharpDX.DXGI.Format.R32_SInt,
                                Slot = 1,
                                AlignedByteOffset = InputElement.AppendAligned,
                                Classification = InputClassification.PerInstanceData,
                                InstanceDataStepRate = 1
                            },
                            new InputElement()
                            {
                                SemanticName = "PSIZE",
                                SemanticIndex = 3,
                                Format = SharpDX.DXGI.Format.R32_SInt,
                                Slot = 1,
                                AlignedByteOffset = InputElement.AppendAligned,
                                Classification = InputClassification.PerInstanceData,
                                InstanceDataStepRate = 1
                            },
                            new InputElement()
                            {
                                SemanticName = "PSIZE",
                                SemanticIndex = 4,
                                Format = SharpDX.DXGI.Format.R32_SInt,
                                Slot = 1,
                                AlignedByteOffset =  InputElement.AppendAligned,
                                Classification = InputClassification.PerInstanceData,
                                InstanceDataStepRate = 1
                            },
                            new InputElement()
                            {
                                SemanticName = "PSIZE",
                                SemanticIndex = 5,
                                Format = SharpDX.DXGI.Format.R32_SInt,
                                Slot = 1,
                                AlignedByteOffset = InputElement.AppendAligned,
                                Classification = InputClassification.PerInstanceData,
                                InstanceDataStepRate = 1
                            },
                            new InputElement()
                            {
                                SemanticName = "PSIZE",
                                SemanticIndex = 6,
                                Format = SharpDX.DXGI.Format.R32_SInt,
                                Slot = 1,
                                AlignedByteOffset = InputElement.AppendAligned,
                                Classification = InputClassification.PerInstanceData,
                                InstanceDataStepRate = 1
                            },
                            new InputElement()
                            {
                                SemanticName = "PSIZE",
                                SemanticIndex = 7,
                                Format = SharpDX.DXGI.Format.R32_SInt,
                                Slot = 1,
                                AlignedByteOffset = InputElement.AppendAligned,
                                Classification = InputClassification.PerInstanceData,
                                InstanceDataStepRate = 1
                            },



                            new InputElement()
                            {
                                SemanticName = "POSITION",
                                SemanticIndex = 2,
                                Format = SharpDX.DXGI.Format.R32G32B32A32_Float,
                                Slot = 1,
                                AlignedByteOffset =  InputElement.AppendAligned,
                                Classification = InputClassification.PerInstanceData,
                                InstanceDataStepRate = 1
                            },

                        };

                        Layout = new InputLayout(device, ShaderSignature.GetInputSignature(vertexShaderByteCode), inputElements);

                        var contantBuffer = new Buffer(device, Utilities.SizeOf<DMatrixBuffer>(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);

                        Vector4 ambientColor = new Vector4(0.15f, 0.15f, 0.15f, 1.0f);
                        Vector4 diffuseColour = new Vector4(1, 1, 1, 1);
                        Vector3 lightDirection = new Vector3(1, 0, 0);


                        lightBuffer[0] = new DLightBuffer()
                        {
                            ambientColor = ambientColor,
                            diffuseColor = diffuseColour,
                            lightDirection = lightDirection,
                            padding = 0
                        };


                        BufferDescription lightBufferDesc = new BufferDescription()
                        {
                            Usage = ResourceUsage.Dynamic,
                            SizeInBytes = Utilities.SizeOf<DLightBuffer>(),
                            BindFlags = BindFlags.ConstantBuffer,
                            CpuAccessFlags = CpuAccessFlags.Write,
                            OptionFlags = ResourceOptionFlags.None,
                            StructureByteStride = 0
                        };

                        var ConstantLightBuffer = new SharpDX.Direct3D11.Buffer(device, lightBufferDesc);

                        chunkTerrain.startBuildingArray(new Vector4(0, 0, 0, 1), out originalArrayOfVerts, out originalArrayOfIndices, out originalMap);

                        SC_VR_Chunk.DIndexType[] indexArray = new SC_VR_Chunk.DIndexType[originalArrayOfIndices.Length];
                        for (int i = 0; i < originalArrayOfIndices.Length; i++)
                        {
                            indexArray[i] = new SC_VR_Chunk.DIndexType()
                            {
                                indexPos = originalArrayOfIndices[i],
                            };
                        }

                        arrayOfChunkData = new chunkData[SC_Globals.numberOfObjectInWidth * SC_Globals.numberOfObjectInHeight * SC_Globals.numberOfObjectInDepth];

                        var matrixBufferDescriptionVertex00 = new BufferDescription()
                        {
                            Usage = ResourceUsage.Dynamic,
                            SizeInBytes = Marshal.SizeOf(typeof(SC_VR_Chunk.DInstanceType)) * instances.Length,
                            BindFlags = BindFlags.VertexBuffer,
                            CpuAccessFlags = CpuAccessFlags.Write,
                            OptionFlags = ResourceOptionFlags.None,
                            StructureByteStride = 0
                        };

                        var InstanceBuffer = new SharpDX.Direct3D11.Buffer(device, matrixBufferDescriptionVertex00);

                        var VertexBuffer = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.VertexBuffer, originalArrayOfVerts);

                        chunkData chunkDat;

                        for (int x = 0; x < SC_Globals.numberOfObjectInWidth; x++)
                        {
                            for (int y = 0; y < SC_Globals.numberOfObjectInHeight; y++)
                            {
                                for (int z = 0; z < SC_Globals.numberOfObjectInDepth; z++)
                                {
                                    Vector3 chunkPos = new Vector3(x, y, z);

                                    chunkPos.X *= (SC_Globals.numberOfInstancesPerObjectInWidth * SC_Globals.tinyChunkWidth * SC_Globals.planeSize);
                                    chunkPos.Y *= (SC_Globals.numberOfInstancesPerObjectInHeight * SC_Globals.tinyChunkHeight * SC_Globals.planeSize);
                                    chunkPos.Z *= (SC_Globals.numberOfInstancesPerObjectInDepth * SC_Globals.tinyChunkDepth * SC_Globals.planeSize);

                                    arrayOfChunks[x + SC_Globals.numberOfObjectInWidth * (y + SC_Globals.numberOfObjectInHeight * z)] = new SC_VR_Chunk(1f, 1f, 1f, new Vector4(0.1f, 0.1f, 0.1f, 1), SC_Globals.numberOfInstancesPerObjectInWidth, SC_Globals.numberOfInstancesPerObjectInHeight, SC_Globals.numberOfInstancesPerObjectInDepth, chunkPos); //, instances[x + SC_Globals.numberOfObjectInWidth * (y + SC_Globals.numberOfObjectInHeight * z)]

                                    chunkDat = new chunkData();
                                    arrayOfChunkData[x + SC_Globals.numberOfObjectInWidth * (y + SC_Globals.numberOfObjectInHeight * z)] = chunkDat;
                                    arrayOfChunkData[x + SC_Globals.numberOfObjectInWidth * (y + SC_Globals.numberOfObjectInHeight * z)].switchForRender = 1;
                                    arrayOfChunkData[x + SC_Globals.numberOfObjectInWidth * (y + SC_Globals.numberOfObjectInHeight * z)].instanceBuffer = InstanceBuffer;
                                    arrayOfChunkData[x + SC_Globals.numberOfObjectInWidth * (y + SC_Globals.numberOfObjectInHeight * z)].constantLightBuffer = ConstantLightBuffer;
                                    arrayOfChunkData[x + SC_Globals.numberOfObjectInWidth * (y + SC_Globals.numberOfObjectInHeight * z)].vertexBuffer = VertexBuffer;
                                    arrayOfChunkData[x + SC_Globals.numberOfObjectInWidth * (y + SC_Globals.numberOfObjectInHeight * z)].constantMatrixPosBuffer = contantBuffer;
                                }
                            }
                        }

                        var IndexBuffer = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.IndexBuffer, originalArrayOfIndices);

                        shaderOfChunk = new SC_VR_Chunk_Shader(device, contantBuffer, Layout, VertexShader, PixelShader, InstanceBuffer, ConstantLightBuffer, originalArrayOfVerts, indexArray, VertexBuffer, IndexBuffer);


                        var directInput = new DirectInput();

                        _Keyboard = new SharpDX.DirectInput.Keyboard(directInput);

                        _Keyboard.Properties.BufferSize = 128;
                        _Keyboard.Acquire();


                        var rasterDesc = new RasterizerStateDescription()
                        {
                            IsAntialiasedLineEnabled = false,
                            CullMode = CullMode.Front, //CullMode.Back
                            DepthBias = 0,
                            DepthBiasClamp = 0.0f,
                            IsDepthClipEnabled = true,
                            FillMode = SharpDX.Direct3D11.FillMode.Solid,
                            IsFrontCounterClockwise = false,
                            IsMultisampleEnabled = false,
                            IsScissorEnabled = false,
                            SlopeScaledDepthBias = 0.0f
                        };


                        rasterState = new RasterizerState(device, rasterDesc);

                        device.ImmediateContext.Rasterizer.State = rasterState;

                        BlendStateDescription description = BlendStateDescription.Default();
                        blendState = new BlendState(device, description);

                        DepthStencilStateDescription descriptioner = DepthStencilStateDescription.Default();
                        descriptioner.DepthComparison = Comparison.LessEqual;
                        descriptioner.IsDepthEnabled = true;

                        depthState = new DepthStencilState(device, descriptioner);

                        SamplerStateDescription descriptionator = SamplerStateDescription.Default();
                        descriptionator.Filter = Filter.MinMagMipLinear;
                        descriptionator.AddressU = TextureAddressMode.Wrap;
                        descriptionator.AddressV = TextureAddressMode.Wrap;
                        samplerState = new SamplerState(device, descriptionator);

                        originPos = new Vector3(0, 0, 0);

                        RotationX = 0;
                        RotationY = 0; //180
                        RotationZ = 0;

                        float pitch = RotationX * 0.0174532925f;
                        float yaw = RotationY * 0.0174532925f;
                        float roll = RotationZ * 0.0174532925f;

                        originRot = SharpDX.Matrix.RotationYawPitchRoll(yaw, pitch, roll);
                        //backgroundWorker0.RunWorkerAsync();
                        startOnce = 0;
                    }

                _threaLoop:

                    renderOculus();
                    Thread.Sleep(1);
                    goto _threaLoop;
                };

                backgroundWorker0.RunWorkerAsync();

                /*RenderLoop.Run(form, () =>
                {

                    Thread.Sleep(1);
                });*/

                /*if (immediateContext != null)
                {
                    immediateContext.ClearState();
                    immediateContext.Flush();
                }

                // Release all resources
                //Dispose(contantBuffer);
                Dispose(mirrorTextureD3D);
                Dispose(mirrorTexture);
                Dispose(eyeTextures[0]);
                Dispose(eyeTextures[1]);
                Dispose(immediateContext);
                Dispose(depthStencilState);
                Dispose(depthStencilView);
                Dispose(depthBuffer);
                Dispose(backBufferRenderTargetView);
                Dispose(backBuffer);
                Dispose(swapChain);
                Dispose(factory);
                // Disposing the device, before the hmd, will cause the hmd to fail when disposing.
                // Disposing the device, after the hmd, will cause the dispose of the device to fail.
                // It looks as if the hmd steals ownership of the device and destroys it, when it's shutting down.
                // device.Dispose();
                OVR.Destroy(sessionPtr);

                //    // Present!
                //    swapChain.Present(0, PresentFlags.None);
                //});

                // Release all resources
                //signature.Dispose();
                //vertexShaderByteCode.Dispose();
                VertexShader.Dispose();
                //pixelShaderByteCode.Dispose();
                PixelShader.Dispose();
                //vertices.Dispose();
                Layout.Dispose();
                //contantBuffer.Dispose();
                depthBuffer.Dispose();
                //depthView.Dispose();
                //renderView.Dispose();
                backBuffer.Dispose();
                context.ClearState();
                context.Flush();
                device.Dispose();
                context.Dispose();
                swapChain.Dispose();
                factory.Dispose();*/

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {

            }
        }

        public static int startOncer = 1;

        public void renderOculus()
        {
            try
            {
                device.ImmediateContext.Rasterizer.State = rasterState;
                device.ImmediateContext.OutputMerger.SetBlendState(blendState);
                device.ImmediateContext.OutputMerger.SetDepthStencilState(depthState);
                device.ImmediateContext.PixelShader.SetSampler(0, samplerState);

                Vector3f[] hmdToEyeViewOffsets = { eyeTextures[0].HmdToEyeViewOffset, eyeTextures[1].HmdToEyeViewOffset };
                double displayMidpoint = OVR.GetPredictedDisplayTime(sessionPtr, 0);
                TrackingState trackingState = OVR.GetTrackingState(sessionPtr, displayMidpoint, true);
                Posef[] eyePoses = new Posef[2];

                // Calculate the position and orientation of each eye.
                OVR.CalcEyePoses(trackingState.HeadPose.ThePose, hmdToEyeViewOffsets, ref eyePoses);

                for (int eyeIndex = 0; eyeIndex < 2; eyeIndex++)
                {
                    EyeType eye = (EyeType)eyeIndex;
                    EyeTexture eyeTexture = eyeTextures[eyeIndex];

                    if (eyeIndex == 0)
                        layerEyeFov.RenderPoseLeft = eyePoses[0];
                    else
                        layerEyeFov.RenderPoseRight = eyePoses[1];

                    // Update the render description at each frame, as the HmdToEyeOffset can change at runtime.
                    eyeTexture.RenderDescription = OVR.GetRenderDesc(sessionPtr, eye, hmdDesc.DefaultEyeFov[eyeIndex]);

                    // Retrieve the index of the active texture
                    int textureIndex;
                    result = eyeTexture.SwapTextureSet.GetCurrentIndex(out textureIndex);
                    WriteErrorDetails(OVR, result, "Failed to retrieve texture swap chain current index.");

                    immediateContext.OutputMerger.SetRenderTargets(eyeTexture.DepthStencilView, eyeTexture.RenderTargetViews[textureIndex]);
                    immediateContext.ClearRenderTargetView(eyeTexture.RenderTargetViews[textureIndex], SharpDX.Color.CornflowerBlue);
                    immediateContext.ClearDepthStencilView(eyeTexture.DepthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
                    immediateContext.Rasterizer.SetViewport(eyeTexture.Viewport);


                    SharpDX.Vector3 eyePos = new SharpDX.Vector3(eyePoses[eyeIndex].Position.X, eyePoses[eyeIndex].Position.Y, eyePoses[eyeIndex].Position.Z);
                    Vector3 pos = ((originPos + VRPos));

                    pos.X -= eyePos.X;
                    pos.Y += eyePos.Y;
                    pos.Z -= eyePos.Z;

                    var eyeQuaternionMatrix = SharpDX.Matrix.RotationQuaternion(new SharpDX.Quaternion(eyePoses[eyeIndex].Orientation.X,
                                                                                                       eyePoses[eyeIndex].Orientation.Y * -1,
                                                                                                      eyePoses[eyeIndex].Orientation.Z,
                                                                                                      eyePoses[eyeIndex].Orientation.W * -1));
                    Matrix rot = eyeQuaternionMatrix;
                    //rot.Invert();
                    Vector3 finalUp = Vector3.Transform(new Vector3(0, 1, 0), rot).ToVector3();
                    Vector3 finalForward = Vector3.Transform(new Vector3(0, 0, 1), rot).ToVector3();


                    Matrix viewMatrix = SharpDX.Matrix.LookAtRH(pos, (pos + finalForward), finalUp);
                    Matrix projectionMatrix = OVR.Matrix4f_Projection(eyeTexture.FieldOfView, 0.1f, 100.0f, ProjectionModifier.None).ToMatrix();
                    projectionMatrix.Transpose();


                    _worldMatrix = _WorldMatrix;
                    _viewMatrix = viewMatrix;
                    _projectionMatrix = projectionMatrix;

                    _worldMatrix.Transpose();
                    _viewMatrix.Transpose();
                    _projectionMatrix.Transpose();

                    arrayOfMatrixBuff[0] = new DMatrixBuffer()
                    {
                        world = _worldMatrix,
                        view = _viewMatrix,
                        proj = _projectionMatrix,
                    };

                    timeWatch.Stop();
                    timeWatch.Reset();
                    timeWatch.Start();

                    try
                    {
                           
                        if (startOncer == 1) //I was using this switch just to get results on 1 iteration only. It's not switching off at the moment.
                        {
                            Func<int> formatDelegate = () =>
                            {
                                for (int x = 0; x < SC_Globals.numberOfObjectInWidth; x++)
                                {
                                    for (int y = 0; y < SC_Globals.numberOfObjectInHeight; y++)
                                    {
                                        for (int z = 0; z < SC_Globals.numberOfObjectInDepth; z++)
                                        {

                                            int c = x + SC_Globals.numberOfObjectInWidth * (y + SC_Globals.numberOfObjectInHeight * z);

                                            if (arrayOfChunkData[c].switchForRender == 1)
                                            {
                                                var matrixBufferDescriptionVertex00 = new BufferDescription()
                                                {
                                                    Usage = ResourceUsage.Dynamic,
                                                    SizeInBytes = Marshal.SizeOf(typeof(SC_VR_Chunk.DInstanceType)) * instances.Length,
                                                    BindFlags = BindFlags.VertexBuffer,
                                                    CpuAccessFlags = CpuAccessFlags.Write,
                                                    OptionFlags = ResourceOptionFlags.None,
                                                    StructureByteStride = 0
                                                };

                                                var InstanceBuffer = new SharpDX.Direct3D11.Buffer(device, matrixBufferDescriptionVertex00);

                                                arrayOfChunkData[c].arrayOfInstance = arrayOfChunks[c].instances;
                                                arrayOfChunkData[c].worldMatrix = _worldMatrix;
                                                arrayOfChunkData[c].viewMatrix = _viewMatrix;
                                                arrayOfChunkData[c].projectionMatrix = _projectionMatrix;
                                                arrayOfChunkData[c].matrixBuffer = arrayOfMatrixBuff;
                                                arrayOfChunkData[c].lightBuffer = lightBuffer;
                                                arrayOfChunkData[c].instanceBuffer = InstanceBuffer;
                                                arrayOfChunkData[c].arrayOfSomeMap = arrayOfChunks[c].arrayOfSomeMap;

                                                arrayOfChunkData[c] = shaderOfChunk.Renderer(arrayOfChunkData[c], c);
                                            }
                                            else
                                            {
                                                arrayOfChunkData[c] = shaderOfChunk.Renderer(arrayOfChunkData[c], c);
                                            }

                                        }
                                    }
                                }


                                return 1;
                            };

                            var t2 = new Task<int>(formatDelegate);
                            t2.RunSynchronously();
                            t2.Dispose();
                            //startOncer = 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }

                    Console.WriteLine(timeWatch.Elapsed.Ticks);





                    result = eyeTexture.SwapTextureSet.Commit();
                    WriteErrorDetails(OVR, result, "Failed to commit the swap chain texture.");
                }

                float speed = 0.1f;

                ReadKeyboard();

                if (_KeyboardState != null && _KeyboardState.PressedKeys.Contains(Key.Up))
                {
                    VRPos.Z += speed;
                }
                else if (_KeyboardState != null && _KeyboardState.PressedKeys.Contains(Key.Down))
                {
                    VRPos.Z -= speed;
                }
                else if (_KeyboardState != null && _KeyboardState.PressedKeys.Contains(Key.Q))
                {
                    VRPos.Y += speed;
                }
                else if (_KeyboardState != null && _KeyboardState.PressedKeys.Contains(Key.Z))
                {
                    VRPos.Y -= speed;
                }
                else if (_KeyboardState != null && _KeyboardState.PressedKeys.Contains(Key.Left))
                {
                    VRPos.X += speed;
                }
                else if (_KeyboardState != null && _KeyboardState.PressedKeys.Contains(Key.Right))
                {
                    VRPos.X -= speed;
                }

                result = OVR.SubmitFrame(sessionPtr, 0L, IntPtr.Zero, ref layerEyeFov);
                WriteErrorDetails(OVR, result, "Failed to submit the frame of the current layers.");

                immediateContext.CopyResource(mirrorTextureD3D, backBuffer);
                swapChain.Present(0, PresentFlags.None);
            }
            catch
            {

            }
            finally
            {

            }
        }

        Stopwatch timeWatch = new Stopwatch();
        public class chunkData
        {
            public SC_VR_Chunk.DInstanceType[] arrayOfInstance;
            public Matrix worldMatrix;
            public Matrix viewMatrix;
            public Matrix projectionMatrix;
            public SC_VR_Chunk_Shader chunkShader;
            public DMatrixBuffer[] matrixBuffer;
            public DLightBuffer[] lightBuffer;
            public int switchForRender;
            public SC_VR_Chunk.DInstanceType[] instancesIndex;
            public SC_VR_Chunk.DInstanceType[] arrayOfDeVectorMapTemp;
            public SC_VR_Chunk.DInstanceTypeTwo[] arrayOfDeVectorMapTempTwo;
            public SharpDX.Direct3D11.Buffer instanceBuffer;
            public SharpDX.Direct3D11.Buffer constantLightBuffer;
            public SharpDX.Direct3D11.Buffer vertexBuffer;
            public SharpDX.Direct3D11.Buffer constantMatrixPosBuffer;
            public int[][] arrayOfSomeMap;
            public SharpDX.Direct3D11.Buffer mapBuffer;
        }

        KeyboardState _KeyboardState;
        private bool ReadKeyboard()
        {
            var resultCode = SharpDX.DirectInput.ResultCode.Ok;
            _KeyboardState = new KeyboardState();

            try
            {
                // Read the keyboard device.
                _Keyboard.GetCurrentState(ref _KeyboardState);
            }
            catch (SharpDX.SharpDXException ex)
            {
                resultCode = ex.Descriptor; // ex.ResultCode;
            }
            catch (Exception)
            {
                return false;
            }

            // If the mouse lost focus or was not acquired then try to get control back.
            if (resultCode == SharpDX.DirectInput.ResultCode.InputLost || resultCode == SharpDX.DirectInput.ResultCode.NotAcquired)
            {
                try
                {
                    _Keyboard.Acquire();
                }
                catch
                { }

                return true;
            }

            if (resultCode == SharpDX.DirectInput.ResultCode.Ok)
                return true;

            return false;
        }
        public static void WriteErrorDetails(OvrWrap OVR, Result result, string message)
        {
            if (result >= Result.Success)
                return;

            // Retrieve the error message from the last occurring error.
            ErrorInfo errorInformation = OVR.GetLastErrorInfo();

            string formattedMessage = string.Format("{0}. \nMessage: {1} (Error code={2})", message, errorInformation.ErrorString, errorInformation.Result);
            Trace.WriteLine(formattedMessage);
            System.Windows.Forms.MessageBox.Show(formattedMessage, message);

            throw new Exception(formattedMessage);
        }

        public static void Dispose(IDisposable disposable)
        {
            if (disposable != null)
                disposable.Dispose();
        }
    }
}