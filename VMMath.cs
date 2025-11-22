using Unity.Mathematics;
using UnityEngine;

namespace VoxelMesh
{
    public static class VMMath
    {
        public static readonly Vector3[] cubeCornerVertices =
        {
            /*       b - - - - - - - a
                     | \      +y     | \
                     |   e - - - - - - - f
                     |   |           |   |
                     |   |   +z      |   |
                  -x |   |           |   |
                     |   |       -z  |   | +x
                     |   |           |   |
                     c - | - - - - - d   |
                       \ |     -y      \ |
            *///         h - - - - - - - g

            new( 1, 1, 1), // a
            new(-1, 1, 1), // b
            new(-1,-1, 1), // c
            new( 1,-1, 1), // d
            new(-1, 1,-1), // e
            new( 1, 1,-1), // f
            new( 1,-1,-1), // g
            new(-1,-1,-1)  // h
        };

        // Each row represents the 4 indices on the verts array above that map to vertex positions
        public static readonly int[][] cubeFaceTris =
        {
            new[] { 0, 1, 2, 3 }, // a, b, c, d   +Z
            new[] { 5, 0, 3, 6 }, // f, a, d, g   +X
            new[] { 4, 5, 6, 7 }, // e, f, g, h   -Z
            new[] { 1, 4, 7, 2 }, // b, e, h, c   -X
            new[] { 5, 4, 1, 0 }, // f, e, b, a   +Y
            new[] { 3, 2, 7, 6 }  // d, c, h, g   -Y
        };

        public static Vector3[] GetFaceVerts(VMDirection _direction, Vector3 _offset, float _scale)
        {
            Vector3[] fv = new Vector3[4]; // 4 corners on a face
            for (int i = 0; i < fv.Length; i++)
            {
                // Get the 4 vertices on a face
                // (Add the cube's offset so the vertices are offset locally and not from 0,0,0)
                fv[i] = (cubeCornerVertices[cubeFaceTris[(int)_direction][i]] * _scale) + _offset;
            }
            return fv;
        }
        
        public static readonly int3[] int3OffsetsByDirection =
        {
            new(0,  0,  1),
            new(1,  0,  0),
            new(0,  0, -1),
            new(-1, 0,  0),
            new(0,  1,  0),
            new(0, -1,  0)
        };

        public static int3 offset(this VMDirection _direction)
        {
            return int3OffsetsByDirection[(int)_direction];
        }
    }
} // VoxelMesh namespace
