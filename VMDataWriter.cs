using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace VoxelMesh
{
    public static class VMDataWriter
    {
        public static void WriteObject_Color(string _path, VMObject<VMVoxelInfo_Color> _object)
        {
            if (string.IsNullOrEmpty(_path))
            {
                Debug.LogError("Path is empty");
                return;
            }
            
            if (!File.Exists(_path))
            {
                Debug.LogError("File not found");
                return;
            }

            if (_object.chunks.Count == 0)
            {
                Debug.LogError("Object has no chunks");
                return;
            }

            using (StreamWriter sw = new(_path))
            {
                sw.WriteLine(VMVersion.version);

                for (int i = 0; i < _object.chunks.Count; i++)
                {
                    string chunkJson = JsonUtility.ToJson(_object.chunks.ElementAt(i).Value);
                    
                    // Signal the start of a chunk with chunk properties
                    sw.WriteLine($"!{chunkJson}");
                    
                    VMData<VMVoxelInfo_Color> data = _object.chunks.ElementAt(i).Key.CreateCombinedData();
                    
                    foreach (KeyValuePair<ushort, VMVoxelInfo_Color> pair in data.infoByPalette)
                    {
                        string infoJson = JsonUtility.ToJson(pair.Value, false);
                        sw.WriteLine($"#{pair.Key},{infoJson}");
                    }

                    foreach (KeyValuePair<int3, ushort> pair in data.paletteByPosition)
                    {
                        sw.WriteLine($"{pair.Key.x},{pair.Key.y},{pair.Key.z},{pair.Value}");
                    }
                }
            }
        }
    }
} // VoxelMesh namespace
