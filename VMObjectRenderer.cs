using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VoxelMesh
{
    /// <summary>
    /// Object renderers handle visuals for an object via a collection of child chunk renderers, each of which represents a chunk in the object
    /// </summary>
    public class VMObjectRenderer : MonoBehaviour
    {
        protected VMObject<VMVoxelInfo_Color> voxelObject { get; private set; }
        
        private List<VMChunkRenderer> chunkRenderers = new();
        

        public void SetVoxelObject(VMObject<VMVoxelInfo_Color> _voxelObject)
        {
            voxelObject?.OnRegenerated.RemoveListener(OnVoxelObjectRegenerated);
            voxelObject = _voxelObject;
            voxelObject?.OnRegenerated.AddListener(OnVoxelObjectRegenerated);
            OnVoxelObjectRegenerated(voxelObject);
        }
        
        protected virtual void OnVoxelObjectRegenerated(VMObject<VMVoxelInfo_Color> _voxelObject)
        {
            UpdateChunks();
        }

        protected void UpdateChunks()
        {
            int numRenderersOverChunkCount = chunkRenderers.Count - voxelObject.chunks.Count;
            
            // Delete any unused chunk renderers
            if (numRenderersOverChunkCount > 0)
            {
                for (int i = 0; i < numRenderersOverChunkCount; i++)
                {
                    DestroyChunk(chunkRenderers.Last());
                }
            }
            
            // Add more chunk renderers if needed
            else if (numRenderersOverChunkCount < 0)
            {
                for (int i = voxelObject.chunks.Count + numRenderersOverChunkCount; i < voxelObject.chunks.Count; i++)
                {
                    CreateChunk(voxelObject.chunks.ElementAt(i).Key, voxelObject.chunks.ElementAt(i).Value);
                }
            }

            // Update chunk renderers
            for (int i = 0; i < voxelObject.chunks.Count; i++)
            {
                if (!chunkRenderers[i])
                {
                    continue;
                }
                
                UpdateChunk(chunkRenderers[i], voxelObject.chunks.ElementAt(i).Key,  voxelObject.chunks.ElementAt(i).Value);
            }
        }

        protected void CreateChunk(VMChunk<VMVoxelInfo_Color> _chunk, VMChunkProperties _chunkProperties)
        {
            GameObject chunkRendererGO = new("Chunk");
            chunkRendererGO.transform.SetParent(transform);
            VMChunkRenderer chunkRenderer = chunkRendererGO.AddComponent<VMChunkRenderer>();
            chunkRenderers.Add(chunkRenderer);
        }
        
        protected void DestroyChunk(VMChunkRenderer _chunkRenderer)
        {
            if (chunkRenderers.Contains(_chunkRenderer))
            {
                chunkRenderers.Remove(_chunkRenderer);
                Destroy(_chunkRenderer.gameObject);
            }
        }

        protected void UpdateChunk(VMChunkRenderer _chunkRenderer, VMChunk<VMVoxelInfo_Color> _chunk, VMChunkProperties _chunkProperties)
        {
            _chunkRenderer.SetVoxelChunk(_chunk);

            _chunkRenderer.transform.localPosition = new Vector3(_chunkProperties.offset.x, _chunkProperties.offset.y, _chunkProperties.offset.z);
            _chunkRenderer.transform.localRotation = Quaternion.Euler(new Vector3(_chunkProperties.rotation.x, _chunkProperties.rotation.y, _chunkProperties.rotation.z));
        }
    }
}
