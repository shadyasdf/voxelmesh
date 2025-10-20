using System.Collections.Generic;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;

namespace VoxelMesh
{
    public struct VMData<T> where T : VMVoxelInfo
    {
        public readonly Dictionary<ushort, T> infoByPalette;
        public readonly Dictionary<int3, ushort> paletteByPosition;

        public static readonly VMData<T> Empty = new(new Dictionary<ushort, T>());
        

        public VMData(Dictionary<ushort, T> _infoByPalette, Dictionary<int3, ushort> _paletteByPosition = null)
        {
            infoByPalette = _infoByPalette;
            paletteByPosition = _paletteByPosition;
        }


        public bool IsValid()
        {
            return infoByPalette != null
                && paletteByPosition != null;
        }

        public readonly bool GetInfoForPalette(ushort _palette, out T _info)
        {
            if (infoByPalette.TryGetValue(_palette, out T info))
            {
                _info = info;
                return true;
            }

            _info = null;
            return false;
        }

        public readonly bool GetPaletteForInfo(T _info, out ushort _palette)
        {
            if (infoByPalette.ContainsValue(_info))
            {
                foreach (KeyValuePair<ushort, T> pair in infoByPalette)
                {
                    if (pair.Value.Equals(_info))
                    {
                        _palette = pair.Key;
                        return true;
                    }
                }
            }

            _palette = 0;
            return false;
        }

        public readonly ushort GetUnusedPalette()
        {
            ushort palette = 1;
            while (infoByPalette.ContainsKey(palette))
            {
                palette++;
            }
            return palette;
        }
    }
} // VoxelMesh namespace
