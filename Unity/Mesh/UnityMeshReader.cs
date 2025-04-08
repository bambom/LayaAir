using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Mesh;

public class UnityMeshReader
{
    public static Mesh Parse(byte[] data)
    {
        Mesh mesh = new Mesh();
        List<int[]> subMeshIndices = new List<int[]>();
        
        Read(data, mesh, subMeshIndices);
        
        // 设置子网格
        mesh.subMeshCount = subMeshIndices.Count;
        for (int i = 0; i < subMeshIndices.Count; i++)
        {
            mesh.SetIndices(subMeshIndices[i], MeshTopology.Triangles, i);
        }
        
        return mesh;
    }
    
    public static void Read(byte[] data, Mesh mesh, List<int[]> subMeshIndices)
    {
        UnityByte readData = new UnityByte(data);
        readData.Position = 0;
        
        string version = readData.ReadUTFString();
        Debug.Log("Mesh版本: " + version);
        
        switch (version)
        {
            case "LAYAMODEL:0301":
            case "LAYAMODEL:0400":
            case "LAYAMODEL:0401":
                Debug.LogWarning("不支持的老版本模型格式: " + version);
                break;
                
            case "LAYAMODEL:05":
            case "LAYAMODEL:COMPRESSION_05":
            case "LAYAMODEL:0501":
            case "LAYAMODEL:COMPRESSION_0501":
            case "LAYAMODEL:0502":
                UnityLoadModelV05.Parse(readData, version, mesh, subMeshIndices);
                break;
                
            default:
                throw new Exception("未知的模型版本: " + version);
        }
        
        // 兼容旧版本，如果需要的话计算边界
        if (version != "LAYAMODEL:0501" && version != "LAYAMODEL:COMPRESSION_0501" && version != "LAYAMODEL:0502")
        {
            mesh.RecalculateBounds();
        }
    }
}