using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

namespace VoxelMesh
{
    [Serializable]
    public struct VMChunkProperties
    {
        public int3 offset => o;
        [SerializeField] private int3 o;

        public int3 rotation => r;
        [SerializeField] private int3 r;


        public VMChunkProperties(int3 _offset, int3 _rotation)
        {
            o = _offset;
            r = _rotation;
        }
    }
    
    /// <summary>
    /// Chunks house voxel data and have the ability to dynamically override individual voxels
    /// </summary>
    public class VMChunk<T> where T : VMVoxelInfo
    {
        public readonly UnityEvent<VMChunk<T>> OnRegenerated = new();

        protected readonly VMData<T> template;

        protected Dictionary<ushort, T> overrideInfoByPalette { get; private set; }
        protected Dictionary<int3, ushort> overridePaletteByPosition { get; private set; }


        public VMChunk(VMData<T> _template)
        {
            template = _template;
        }
        
        public virtual void Regenerate()
        {
            OnRegenerated?.Invoke(this);
        }

        public ushort GetPalette(int3 _position)
        {
            if (overridePaletteByPosition != null
                && overridePaletteByPosition.TryGetValue(_position, out ushort overridePalette))
            {
                return overridePalette;
            }

            if (template.paletteByPosition.TryGetValue(_position, out ushort templatePalette))
            {
                return templatePalette;
            }

            return 0;
        }

        public T GetVoxelInfo(int3 _position)
        {
            return template.infoByPalette.GetValueOrDefault(GetPalette(_position));
        }
        
        public void SetOverride(int3 _position, T _info)
        {
            SetOverride_Internal(_position, _info, true);
        }
        
        public void SetOverrides(int3[] _positions, T _info)
        {
            foreach (int3 position in _positions)
            {
                SetOverride_Internal(position, _info, false);
            }
            Regenerate();
        }

        public void SetOverrides(Dictionary<int3, T> _overrides)
        {
            foreach (KeyValuePair<int3, T> pair in _overrides)
            {
                SetOverride_Internal(pair.Key, pair.Value, false);
            }
            Regenerate();
        }

        protected virtual void SetOverride_Internal(int3 _position, T _info, bool _broadcast)
        {
            ushort paletteForThisOverride = 0;
            if (!template.GetPaletteForInfo(_info, out ushort palette))
            {
                if (overrideInfoByPalette == null)
                {
                    overrideInfoByPalette = new Dictionary<ushort, T>();
                }
                
                paletteForThisOverride = GetUnusedPalette();
                overrideInfoByPalette.Add(paletteForThisOverride, _info);
            }
            else
            {
                paletteForThisOverride = palette;
            }
            
            if (overridePaletteByPosition == null)
            {
                overridePaletteByPosition = new Dictionary<int3, ushort>();
            }
            overridePaletteByPosition.Add(_position, paletteForThisOverride);

            if (_broadcast)
            {
                Regenerate();
            }
        }

        protected ushort GetUnusedPalette()
        {
            ushort palette = template.GetUnusedPalette();
            while (overrideInfoByPalette.ContainsKey(palette))
            {
                palette++;
            }
            return palette;
        }

        /// <summary>
        /// Creates a new data structure by using the template data combined with overrides
        /// </summary>
        public VMData<T> CreateCombinedData()
        {
            Dictionary<ushort, T> infoByPalette = new();
            Dictionary<int3, ushort> paletteByPosition = new();

            foreach (KeyValuePair<int3, ushort> pair in template.paletteByPosition)
            {
                if (overridePaletteByPosition != null
                    && overridePaletteByPosition.TryGetValue(pair.Key, out ushort overridePalette))
                {
                    paletteByPosition.Add(pair.Key, overridePalette);
                }
                else
                {
                    paletteByPosition.Add(pair.Key, pair.Value);
                }
            }

            foreach (KeyValuePair<ushort, T> pair in template.infoByPalette)
            {
                if (overrideInfoByPalette != null
                    && overrideInfoByPalette.TryGetValue(pair.Key, out T overrideInfo))
                {
                    infoByPalette.Add(pair.Key, overrideInfo);
                }
                else
                {
                    infoByPalette.Add(pair.Key, pair.Value);
                }
            }
            
            return new VMData<T>(infoByPalette, paletteByPosition);
        }
    }
} // VoxelMesh namespace
