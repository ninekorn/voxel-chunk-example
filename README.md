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



