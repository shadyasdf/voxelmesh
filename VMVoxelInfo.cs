using System;
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
    }
} // VoxelMesh namespace
