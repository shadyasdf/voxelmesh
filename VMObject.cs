using System.Collections.Generic;
using UnityEngine.Events;

namespace VoxelMesh
{
    /// <summary>
    /// Objects house a collection of chunks
    /// </summary>
    public class VMObject<T> where T : VMVoxelInfo
    {
        public readonly UnityEvent<VMObject<T>> OnRegenerated = new();
        
        public Dictionary<VMChunk<T>, VMChunkProperties> chunks { get; private set; }
        
        
        public VMObject(Dictionary<VMChunk<T>, VMChunkProperties> _chunks)
        {
            chunks = _chunks;
        }
        
        public virtual void Regenerate()
        {
            OnRegenerated?.Invoke(this);
        }

        public virtual void RegenerateChunks()
        {
            foreach (VMChunk<T> chunk in chunks.Keys)
            {
                chunk.Regenerate();
            }
        }
    }
} // VoxelMesh namespace
