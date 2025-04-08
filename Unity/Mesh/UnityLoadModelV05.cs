using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Mesh
{
    public class UnityLoadModelV05
    {
        // 内部结构
        private class Block
        {
            public int count;
            public List<int> blockStarts = new List<int>();
            public List<int> blockLengths = new List<int>();
        }
        
        private class Data
        {
            public int offset;
            public int size;
        }
        
        // 静态字段
        private static List<string> _strings = new List<string>();
        private static UnityByte _readData;
        private static string _version;
        private static UnityEngine.Mesh _mesh;
        private static List<int[]> _subMeshIndices;
        private static Block _BLOCK = new Block();
        private static Data _DATA = new Data();
        
        // 解析模型
        public static void Parse(UnityByte readData, string version, UnityEngine.Mesh mesh, List<int[]> subMeshIndices)
        {
            _mesh = mesh;
            _subMeshIndices = subMeshIndices;
            _version = version;
            _readData = readData;
            
            ReadDATA();
            ReadBLOCK();
            ReadSTRINGS();
            
            for (int i = 0; i < _BLOCK.count; i++)
            {
                _readData.Position = _BLOCK.blockStarts[i];
                int index = _readData.ReadUInt16();
                string blockName = _strings[index];
                
                switch (blockName)
                {
                    case "MESH":
                        ReadMESH();
                        break;
                    case "SUBMESH":
                        ReadSUBMESH();
                        break;
                    case "MORPH":
                        // 暂不实现变形目标
                        break;
                    case "UVSIZE":
                        ReadUVSIZE();
                        break;
                    default:
                        Debug.LogWarning($"未知的模型块: {blockName}");
                        break;
                }
            }
            
            // 清理
            _strings.Clear();
            _readData = null;
            _version = null;
            _mesh = null;
            _subMeshIndices = null;
        }
        
        private static string ReadString()
        {
            return _strings[_readData.ReadUInt16()];
        }
        
        private static void ReadDATA()
        {
            _DATA.offset = _readData.ReadInt32();
            _DATA.size = _readData.ReadInt32();
        }
        
        private static void ReadBLOCK()
        {
            _BLOCK.count = _readData.ReadUInt16();
            _BLOCK.blockStarts.Clear();
            _BLOCK.blockLengths.Clear();
            
            for (int i = 0; i < _BLOCK.count; i++)
            {
                _BLOCK.blockStarts.Add(_readData.ReadInt32());
                _BLOCK.blockLengths.Add(_readData.ReadInt32());
            }
        }
        
        private static void ReadSTRINGS()
        {
            int offset = _readData.ReadInt32();
            int count = _readData.ReadUInt16();
            int prePos = _readData.Position;
            
            _readData.Position = offset + _DATA.offset;
            _strings.Clear();
            
            for (int i = 0; i < count; i++)
            {
                _strings.Add(_readData.ReadUTFString());
            }
            
            _readData.Position = prePos;
        }
        
        private static void ReadMESH()
        {
            string name = ReadString();
            Debug.Log($"读取网格: {name}");
            
            byte[] arrayBuffer = _readData.GetBuffer();
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector4> tangents = new List<Vector4>();
            List<Vector2> uv = new List<Vector2>();
            List<Vector2> uv2 = new List<Vector2>();
            List<Color> colors = new List<Color>();
            List<BoneWeight> boneWeights = new List<BoneWeight>();
            
            int vertexBufferCount = _readData.ReadInt16();
            int offset = _DATA.offset;
            
            // 读取顶点数据
            for (int i = 0; i < vertexBufferCount; i++)
            {
                int vbStart = offset + _readData.ReadInt32();
                int vertexCount = _readData.ReadInt32();
                string vertexFlag = ReadString();
                
                string[] subVertexFlags = vertexFlag.Split(',');
                bool hasPosition = false, hasNormal = false, hasColor = false, hasUV = false;
                bool hasUV1 = false, hasBlendWeight = false, hasBlendIndices = false, hasTangent = false;
                
                foreach (string flag in subVertexFlags)
                {
                    switch (flag)
                    {
                        case "POSITION": hasPosition = true; break;
                        case "NORMAL": hasNormal = true; break;
                        case "COLOR": hasColor = true; break;
                        case "UV": hasUV = true; break;
                        case "UV1": hasUV1 = true; break;
                        case "BLENDWEIGHT": hasBlendWeight = true; break;
                        case "BLENDINDICES": hasBlendIndices = true; break;
                        case "TANGENT": hasTangent = true; break;
                    }
                }
                
                // 初始化顶点数据
                for (int j = 0; j < vertexCount; j++)
                {
                    vertices.Add(Vector3.zero);
                    if (hasNormal) normals.Add(Vector3.zero);
                    if (hasColor) colors.Add(Color.white);
                    if (hasUV) uv.Add(Vector2.zero);
                    if (hasUV1) uv2.Add(Vector2.zero);
                    if (hasTangent) tangents.Add(Vector4.zero);
                    if (hasBlendWeight && hasBlendIndices) boneWeights.Add(new BoneWeight());
                }
                
                // 解析顶点数据 - 这部分需要根据实际的数据格式进行适配
                // 这里只是一个简化版，实际实现需要处理多种格式和压缩数据
                int lastPosition = _readData.Position;
                _readData.Position = vbStart;
                
                // 根据版本和压缩类型读取顶点数据
                if (_version == "LAYAMODEL:COMPRESSION_05" || _version == "LAYAMODEL:COMPRESSION_0501")
                {
                    // 读取压缩数据 - 这里需要实现HalfFloat转换等功能
                    Debug.LogWarning("压缩格式需要更复杂的实现");
                }
                else
                {
                    // 读取非压缩数据
                    for (int j = 0; j < vertexCount; j++)
                    {
                        int verOffset = 0;
                        for (int k = 0; k < subVertexFlags.Length; k++)
                        {
                            switch (subVertexFlags[k])
                            {
                                case "POSITION":
                                    vertices[j] = new Vector3(
                                        _readData.ReadFloat32(),
                                        _readData.ReadFloat32(),
                                        _readData.ReadFloat32()
                                    );
                                    verOffset += 12;
                                    break;
                                case "NORMAL":
                                    normals[j] = new Vector3(
                                        _readData.ReadFloat32(),
                                        _readData.ReadFloat32(),
                                        _readData.ReadFloat32()
                                    );
                                    verOffset += 12;
                                    break;
                                case "COLOR":
                                    colors[j] = new Color(
                                        _readData.ReadFloat32(),
                                        _readData.ReadFloat32(),
                                        _readData.ReadFloat32(),
                                        _readData.ReadFloat32()
                                    );
                                    verOffset += 16;
                                    break;
                                case "UV":
                                    uv[j] = new Vector2(
                                        _readData.ReadFloat32(),
                                        _readData.ReadFloat32()
                                    );
                                    verOffset += 8;
                                    break;
                                case "UV1":
                                    uv2[j] = new Vector2(
                                        _readData.ReadFloat32(),
                                        _readData.ReadFloat32()
                                    );
                                    verOffset += 8;
                                    break;
                                case "BLENDWEIGHT":
                                    BoneWeight bw = boneWeights[j];
                                    bw.weight0 = _readData.ReadFloat32();
                                    bw.weight1 = _readData.ReadFloat32();
                                    bw.weight2 = _readData.ReadFloat32();
                                    bw.weight3 = _readData.ReadFloat32();
                                    boneWeights[j] = bw;
                                    verOffset += 16;
                                    break;
                                case "BLENDINDICES":
                                    BoneWeight bi = boneWeights[j];
                                    bi.boneIndex0 = _readData.ReadUInt8();
                                    bi.boneIndex1 = _readData.ReadUInt8();
                                    bi.boneIndex2 = _readData.ReadUInt8();
                                    bi.boneIndex3 = _readData.ReadUInt8();
                                    boneWeights[j] = bi;
                                    verOffset += 4;
                                    break;
                                case "TANGENT":
                                    tangents[j] = new Vector4(
                                        _readData.ReadFloat32(),
                                        _readData.ReadFloat32(),
                                        _readData.ReadFloat32(),
                                        _readData.ReadFloat32()
                                    );
                                    verOffset += 16;
                                    break;
                            }
                        }
                    }
                }
                
                _readData.Position = lastPosition;
            }
            
            // 读取索引数据
            int ibStart = offset + _readData.ReadInt32();
            int ibLength = _readData.ReadInt32();
            
            int lastPos = _readData.Position;
            _readData.Position = ibStart;
            
            // 读取索引
            List<int> indices = new List<int>();
            bool isUint32 = false; // 根据文件格式判断
            
            for (int i = 0; i < ibLength / (isUint32 ? 4 : 2); i++)
            {
                indices.Add(isUint32 ? _readData.ReadInt32() : _readData.ReadUInt16());
            }
            
            _readData.Position = lastPos;
            
            // 设置网格数据
            _mesh.vertices = vertices.ToArray();
            if (normals.Count > 0) _mesh.normals = normals.ToArray();
            if (colors.Count > 0) _mesh.colors = colors.ToArray();
            if (uv.Count > 0) _mesh.uv = uv.ToArray();
            if (uv2.Count > 0) _mesh.uv2 = uv2.ToArray();
            if (tangents.Count > 0) _mesh.tangents = tangents.ToArray();
            if (boneWeights.Count > 0) _mesh.boneWeights = boneWeights.ToArray();
            
            // 设置边界
            if (_version == "LAYAMODEL:0501" || _version == "LAYAMODEL:COMPRESSION_0501" || _version == "LAYAMODEL:0502")
            {
                Vector3 min = new Vector3(
                    _readData.ReadFloat32(),
                    _readData.ReadFloat32(),
                    _readData.ReadFloat32()
                );
                Vector3 max = new Vector3(
                    _readData.ReadFloat32(),
                    _readData.ReadFloat32(),
                    _readData.ReadFloat32()
                );
                _mesh.bounds = new Bounds((min + max) * 0.5f, max - min);
            }
            
            // 存储索引数据，等待子网格处理
            _mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
            
            // 读取骨骼名称
            List<string> boneNames = new List<string>();
            int boneCount = _readData.ReadUInt16();
            
            for (int i = 0; i < boneCount; i++)
            {
                boneNames.Add(_strings[_readData.ReadUInt16()]);
            }
            
            // 读取绑定姿势矩阵 - Unity中需要转换为Matrix4x4
            int bindPoseDataStart = _readData.ReadInt32();
            int bindPoseDataLength = _readData.ReadInt32();
            
            // 这里需要根据实际需求读取和使用绑定姿势矩阵
        }
        
        private static void ReadSUBMESH()
        {
            _readData.ReadInt16(); // vbIndex
            int ibStart = _readData.ReadInt32();
            int ibCount = _readData.ReadInt32();
            
            // 创建子网格索引数组
            int[] indices = new int[ibCount];
            // 从主网格中获取完整索引并截取子网格部分
            // 这部分需要根据实际存储格式调整
            int[] allIndices = _mesh.GetIndices(0);
            for (int i = 0; i < ibCount; i++)
            {
                indices[i] = allIndices[ibStart + i];
            }
            
            _subMeshIndices.Add(indices);
            
            // 骨骼和权重信息 - 如果需要的话
            int drawCount = _readData.ReadUInt16();
            for (int i = 0; i < drawCount; i++)
            {
                int start = _readData.ReadInt32();
                int count = _readData.ReadInt32();
                int boneDicofs = _readData.ReadInt32();
                int boneDicCount = _readData.ReadInt32();
                
                // 读取骨骼索引 - 如果需要的话
            }
        }
        
        private static void ReadUVSIZE()
        {
            int width = _readData.ReadUInt16();
            int height = _readData.ReadUInt16();
            // 在Unity Mesh中可能不需要使用这些值
        }
    }
}