cbuffer MatrixBuffer :register(b0)
{
	float4x4 world;
	float4x4 view;
	float4x4 proj;
};

/*cbuffer MatrixBuffer :register(b1)
{
	int mapper[][]
};*/

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
PixelInputType TextureVertexShader(VertexInputType input)
{  
	PixelInputType output;
    input.position.w = 1.0f;

	int x = input.indexPos.x; 
	int y = input.indexPos.y; 
	int z = input.indexPos.z;

	int currentMapData;
	int currentByte;

	int currentIndex = x + (tinyChunkWidth * (y + (tinyChunkHeight * z)));
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
		if (totalTimes * theNumber >= currentIndex) // >=?? why not only >
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

	someOtherIndex = 7- (((someOtherIndex - (tinyChunkWidth * baser))*2)+someAdder);

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