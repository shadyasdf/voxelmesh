using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace VoxelMesh
{
    // Inherit from this class with whatever data you want stored in your voxels
    public class VMVoxelInfo
    {
        public override string ToString()
        {
            return $"({nameof(VMVoxelInfo)}) {base.ToString()}";
        }
    }

    [Serializable]
    public class VMVoxelInfo_Color : VMVoxelInfo
    {
        public Color color => c;
        [SerializeField] private Color c;


        public VMVoxelInfo_Color(Color _color)
        {
            c = _color;
        }
        
        
        public override string ToString()
        {
            return $"({nameof(VMVoxelInfo_Color)}) {color.ToString()}";
        }
        
        
        static float UVbuffer = 0.25f; // Value from 0 to 1 for how far inside a pixel the UVs should be to prevent mipmapping artifacts. 0 is no buffer
        
        public static void GenerateVoxelMesh(VMData<VMVoxelInfo_Color> _data, out Mesh _outMesh, out Texture2D _outTexture)
        {
            List<Vector3> verts = new();
            List<int> tris = new();
            List<Vector2> uvs = new();
            List<Color> colors = new();
            
            // Generate the mesh
            foreach (KeyValuePair<int3, ushort> x in _data.paletteByPosition)
            {
                // Make a cube by creating each of the 6 faces
                for (int i = 0; i < 6; i++)
                {
                    // If there is a voxel at this position that this face is facing, don't bother adding anything there
                    if (_data.paletteByPosition.ContainsKey(x.Key + ((VMDirection)i).offset()))
                    {
                        continue;
                    }
                    
                    // Add new vertices for the face we are creating
                    Vector3[] faceVerts = VMMath.GetFaceVerts((VMDirection)i, new Vector3(x.Key.x, x.Key.y, x.Key.z), 0.5f);
                    verts.AddRange(faceVerts);

                    // This acts as an offset so the tris we add aren't overwriting ones that exist
                    int indexOfFaceVerts = verts.Count - 4; // We just added 4 face verts

                    // Each set of 3 ints being added to tris is going to create a single tri
                    // The integers are references to vertices in the verts array, 3 make a face
                    tris.Add(indexOfFaceVerts + 0);
                    tris.Add(indexOfFaceVerts + 1);
                    tris.Add(indexOfFaceVerts + 2);
                    // ---
                    tris.Add(indexOfFaceVerts + 0);
                    tris.Add(indexOfFaceVerts + 2);
                    tris.Add(indexOfFaceVerts + 3);
                }
            }

            // Keep track of colors
            foreach (KeyValuePair<ushort, VMVoxelInfo_Color> x in _data.infoByPalette)
            {
                if (!colors.Contains(x.Value.color))
                {
                    colors.Add(x.Value.color);
                }
            }

            // Create a new atlasTexture and assign it to the Material
            // Determine the dimensions of the texture atlas
            int textureWidth = 1;
            int textureHeight = 1;
            while (colors.Count > textureWidth * textureHeight)
            {
                if (textureWidth <= textureHeight)
                {
                    textureWidth *= 2;
                }
                else
                {
                    textureHeight *= 2;
                }
            }
            _outTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point
            };
            
            // Fill the texture left-to-right, top-to-bottom
            int index = 0;
            for (int y = 0; y < textureHeight; y++)
            {
                for (int x = 0; x < textureWidth; x++)
                {
                    if (index + 1 <= colors.Count)
                    {
                        _outTexture.SetPixel(x, y, colors[index]);
                    }
                    index++;
                }
            }
            _outTexture.Apply();

            // Determine UVs
            foreach (KeyValuePair<int3, ushort> x in _data.paletteByPosition)
            {
                if (!_data.infoByPalette.ContainsKey(x.Value))
                {
                    continue;
                }
                
                int indexOfColor = colors.IndexOf(_data.infoByPalette[x.Value].color);
                int column = indexOfColor % _outTexture.width;
                int row = (indexOfColor - column) / _outTexture.width;

                for (int i = 0; i < 6; i++)
                {
                    // If there is a voxel at this position that this face is facing, don't bother adding anything there
                    if (_data.paletteByPosition.ContainsKey(x.Key + ((VMDirection)i).offset()))
                    {
                        continue;
                    }
                    
                    // Set the UVs for this face
                    // Convert the pixel from the texture that we want for this voxel into actual UV values
                    Rect rect = new(
                        (float)column / _outTexture.width,
                        (float)row / _outTexture.height,
                        1f / _outTexture.width,
                        1f / _outTexture.height
                    );
                    
                    if (UVbuffer != 0f)
                    {
                        rect.x += UVbuffer / _outTexture.width;
                        rect.y += UVbuffer / _outTexture.height;
                        rect.width -= UVbuffer / _outTexture.width;
                        rect.height -= UVbuffer / _outTexture.height;
                    }
                    
                    uvs.AddRange(new Vector2[]
                    {
                        new(rect.x, rect.y + rect.height),              // Top-left
                        new(rect.x + rect.width, rect.y + rect.height), // Top-right
                        new(rect.x, rect.y),                            // Bottom-left
                        new(rect.x + rect.width, rect.y)                // Bottom-right
                    });
                }
            }

            // Set the mesh variables to actually generate the mesh
            _outMesh = new Mesh();
            _outMesh.Clear();
            _outMesh.vertices = verts.ToArray();
            _outMesh.triangles = tris.ToArray();
            _outMesh.uv = uvs.ToArray();

            _outMesh.RecalculateNormals();
        }
    }
} // VoxelMesh namespace
