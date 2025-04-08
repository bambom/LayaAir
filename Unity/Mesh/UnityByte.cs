using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text;

public class UnityByte
{
    // 字节序常量
    public const string BIG_ENDIAN = "bigEndian";
    public const string LITTLE_ENDIAN = "littleEndian";

    // 内部数据
    private MemoryStream _stream;
    private BinaryReader _reader;
    private BinaryWriter _writer;
    private byte[] _data;
    private int _length;
    private bool _isLittleEndian;

    // 构造函数
    public UnityByte(byte[] data = null)
    {
        if (data != null)
        {
            _data = data;
            _length = data.Length;
            _stream = new MemoryStream(data);
            _reader = new BinaryReader(_stream);
            _writer = new BinaryWriter(_stream);
        }
        else
        {
            _data = new byte[8];
            _length = 8;
            _stream = new MemoryStream(_data);
            _reader = new BinaryReader(_stream);
            _writer = new BinaryWriter(_stream);
        }

        // 默认使用小端字节序
        _isLittleEndian = true;
    }

    // 属性
    public int Position
    {
        get { return (int)_stream.Position; }
        set { _stream.Position = value; }
    }

    public int Length
    {
        get { return _length; }
        set
        {
            if (value > _data.Length)
            {
                ResizeBuffer(Math.Max(value, _data.Length * 2));
            }
            _length = value;
        }
    }

    public string Endian
    {
        get { return _isLittleEndian ? LITTLE_ENDIAN : BIG_ENDIAN; }
        set { _isLittleEndian = (value == LITTLE_ENDIAN); }
    }

    // 调整缓冲区大小
    private void ResizeBuffer(int newSize)
    {
        byte[] newBuffer = new byte[newSize];
        if (_data != null)
        {
            Array.Copy(_data, newBuffer, Math.Min(_data.Length, newSize));
        }
        _data = newBuffer;
        
        // 重新创建流和读写器
        long position = _stream?.Position ?? 0;
        _stream?.Dispose();
        _reader?.Dispose();
        _writer?.Dispose();
        
        _stream = new MemoryStream(_data);
        _reader = new BinaryReader(_stream);
        _writer = new BinaryWriter(_stream);
        _stream.Position = position;
    }

    // 确保有足够的写入空间
    private void EnsureWrite(int lengthToEnsure)
    {
        if (_length < lengthToEnsure)
            _length = lengthToEnsure;
        if (_data.Length < lengthToEnsure)
            ResizeBuffer(lengthToEnsure);
    }

    // 读取方法
    public string ReadUTFString()
    {
        int len = ReadUInt16();
        return ReadUTFBytes(len);
    }

    public string ReadUTFBytes(int len)
    {
        if (len <= 0) return string.Empty;
        
        int bytesAvailable = _length - Position;
        if (len > bytesAvailable)
            throw new Exception("Out of range error");
            
        byte[] bytes = new byte[len];
        _stream.Read(bytes, 0, len);
        return Encoding.UTF8.GetString(bytes);
    }

    public float ReadFloat32()
    {
        if (Position + 4 > _length)
            throw new Exception("Out of range error");
            
        byte[] bytes = new byte[4];
        _stream.Read(bytes, 0, 4);
        
        if (_isLittleEndian != BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
            
        return BitConverter.ToSingle(bytes, 0);
    }

    public UInt16 ReadUInt16()
    {
        if (Position + 2 > _length)
            throw new Exception("Out of range error");
            
        byte[] bytes = new byte[2];
        _stream.Read(bytes, 0, 2);
        
        if (_isLittleEndian != BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
            
        return BitConverter.ToUInt16(bytes, 0);
    }

    public int ReadInt32()
    {
        if (Position + 4 > _length)
            throw new Exception("Out of range error");
            
        byte[] bytes = new byte[4];
        _stream.Read(bytes, 0, 4);
        
        if (_isLittleEndian != BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
            
        return BitConverter.ToInt32(bytes, 0);
    }

    public UInt32 ReadUInt32()
    {
        if (Position + 4 > _length)
            throw new Exception("Out of range error");
            
        byte[] bytes = new byte[4];
        _stream.Read(bytes, 0, 4);
        
        if (_isLittleEndian != BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
            
        return BitConverter.ToUInt32(bytes, 0);
    }

    public Int16 ReadInt16()
    {
        if (Position + 2 > _length)
            throw new Exception("Out of range error");
            
        byte[] bytes = new byte[2];
        _stream.Read(bytes, 0, 2);
        
        if (_isLittleEndian != BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
            
        return BitConverter.ToInt16(bytes, 0);
    }

    public byte ReadUInt8()
    {
        if (Position + 1 > _length)
            throw new Exception("Out of range error");
            
        return (byte)_stream.ReadByte();
    }

    // 获取缓冲区
    public byte[] GetBuffer()
    {
        byte[] result = new byte[_length];
        Array.Copy(_data, result, _length);
        return result;
    }
}
