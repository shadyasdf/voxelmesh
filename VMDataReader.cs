using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace VoxelMesh
{
    public static class VMDataReader
    {
        
        public static VMObject<VMVoxelInfo_Color> ReadObject_Color(string _path) 
        {
            if (!File.Exists(_path))
            {
                Debug.LogError("File does not exist");
                return null;
            }

            string content = File.ReadAllText(_path);

            if (string.IsNullOrEmpty(content))
            {
                Debug.LogError("File is empty");
                return null;
            }
            
            // Parse version and upgrade data if needed
            {
                using (StreamReader sr = new(_path))
                {
                    string firstLine = sr.ReadLine();

                    if (string.IsNullOrEmpty(firstLine))
                    {
                        Debug.LogError("No version found in file");
                        return null;
                    }
                    
                    int.TryParse(firstLine, out int fileVersion);

                    if (fileVersion > VMVersion.version
                        || fileVersion < 0)
                    {
                        Debug.LogError("File version is out of range");
                        return null;
                    }
                    
                    // Upgrade data version
                    if (fileVersion == 0)
                    {
                        content = UpgradeDataFrom0To1(content);
                        fileVersion = 1;
                    }
                    // More version upgrades would go here
                }
            }

            // Parse the rest of the file's content (palette and voxel data)
            Dictionary<VMChunk<VMVoxelInfo_Color>, VMChunkProperties> chunks = new();
            using (StreamReader sr = new(_path))
            {
                VMChunkProperties chunkProperties = new();
                Dictionary<ushort, VMVoxelInfo_Color> infoByPalette = new();
                Dictionary<int3, ushort> paletteByPosition = new();

                void AddChunk()
                {
                    // If our data is empty, don't add a chunk for it
                    if (infoByPalette.Count == 0
                        && paletteByPosition.Count == 0)
                    {
                        return;
                    }
                        
                    VMData<VMVoxelInfo_Color> data = new(infoByPalette, paletteByPosition);
                    VMChunk<VMVoxelInfo_Color> chunk = new(data);
                    chunks.Add(chunk, chunkProperties);
                    
                    infoByPalette = new Dictionary<ushort, VMVoxelInfo_Color>();
                    paletteByPosition = new Dictionary<int3, ushort>();
                }
                
                string line = string.Empty;
                while ((line = sr.ReadLine()) != null)
                {
                    // If we've found the start of a new chunk, add what data we have
                    if (line.StartsWith("!"))
                    {
                        AddChunk();
                        
                        // Set the properties for this chunk after we've consumed the properties for the previous chunk
                        chunkProperties = JsonUtility.FromJson<VMChunkProperties>(line.Substring(1));

                        continue;
                    }
                    
                    // Parse palette
                    if (line.StartsWith("#"))
                    {
                        string[] split = line.Substring(1).Split(",", 2);
                        if (split.Length < 2)
                        {
                            continue;
                        }
                        
                        ushort.TryParse(split[0], out ushort palette);
                        
                        VMVoxelInfo_Color info = JsonUtility.FromJson<VMVoxelInfo_Color>(split[1]);
                        if (info == null)
                        {
                            continue;
                        }
                        
                        infoByPalette.Add(palette, info);
                    }
                    else // Parse voxel data
                    {
                        string[] split = line.Split(",", 4);
                        if (split.Length < 4)
                        {
                            continue;
                        }

                        int.TryParse(split[0], out int positionX);
                        int.TryParse(split[1], out int positionY);
                        int.TryParse(split[2], out int positionZ);
                        ushort.TryParse(split[3], out ushort palette);
                        
                        paletteByPosition.Add(new int3(positionX, positionY, positionZ), palette);
                    }
                }
                
                // Add the last chunk if we've finished reading all the lines in the file
                AddChunk();
            }

            VMObject<VMVoxelInfo_Color> obj = new(chunks);
            return obj;
        }

        private static string UpgradeDataFrom0To1(string _content)
        {
            // Example function for upgrading data
            
            return _content;
        }
    }
} // VoxelMesh namespace
