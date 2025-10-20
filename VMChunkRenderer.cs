using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace VoxelMesh
{
    /// <summary>
    /// Chunk renderers handle visuals for an individual chunk
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class VMChunkRenderer : MonoBehaviour
    {
        protected VMChunk<VMVoxelInfo_Color> voxelChunk { get; private set; }
        
        protected MeshFilter meshFilter { get; private set; }
        protected MeshRenderer meshRenderer { get; private set; }
        

        protected virtual void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
        }


        public void SetVoxelChunk(VMChunk<VMVoxelInfo_Color> _voxelChunk)
        {
            voxelChunk?.OnRegenerated.RemoveListener(OnVoxelChunkRegenerated);
            voxelChunk = _voxelChunk;
            voxelChunk?.OnRegenerated.AddListener(OnVoxelChunkRegenerated);
            OnVoxelChunkRegenerated(voxelChunk);
        }
        
        protected virtual void OnVoxelChunkRegenerated(VMChunk<VMVoxelInfo_Color> _voxelChunk)
        {
            UpdateMesh();
        }

        protected void UpdateMesh()
        {
            if (voxelChunk == null)
            {
                meshFilter.mesh = null;
                return;
            }

            VMData<VMVoxelInfo_Color> data = voxelChunk.CreateCombinedData();
            
            // TODO: Make mesh with data
            
            Debug.Log($"Rendering chunk with {data.paletteByPosition.Count} voxels (palette size of {data.infoByPalette.Count})");
            foreach (KeyValuePair<int3, ushort> pair in data.paletteByPosition)
            {
                Debug.Log($"Voxel: {pair.Key} = {data.infoByPalette[pair.Value].ToString()}");
            }
        }
    }
} // VoxelMesh namespace
