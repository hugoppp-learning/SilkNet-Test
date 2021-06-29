using System;
using System.Text;

namespace Lib
{

public class Utf8Buffer
{
    private byte[] _data;
    private int len;
    private int prefixLen;
    public int BytesLeft => _data.Length - 1 - len;

    public ReadOnlySpan<byte> ToSpan() => _data;

    public void Put(string s)
    {
        Encoding.UTF8.GetBytes(s, GetAppendSpan(s.Length));
        len += s.Length;
    }

    public readonly struct ClearOnDispose : IDisposable
    {
        private readonly Utf8Buffer _buf;
        public ClearOnDispose(Utf8Buffer buf) => _buf = buf;

        public void Dispose()
        {
            _buf.Clear();
        }
    }

    public ClearOnDispose Put(int i)
    {
        len += itoa(i, GetAppendSpan());
        return new ClearOnDispose(this);
    }


    public ClearOnDispose Put(ReadOnlySpan<byte> bytes)
    {
        bytes.CopyTo(GetAppendSpan(bytes.Length));
        len += bytes.Length;
        return new ClearOnDispose(this);
    }

    public void Clear()
    {
        Array.Clear(_data, prefixLen, _data.Length - prefixLen);
        len = prefixLen;
    }

    public Utf8Buffer(string prefix, int size)
    {
        int utf8ByteCount = Encoding.UTF8.GetByteCount(prefix);
        if (utf8ByteCount > size - 1) throw new ArgumentException();

        prefixLen = prefix.Length;
        len = prefixLen;

        _data = new byte[size];

        Encoding.UTF8.GetBytes(prefix, 0, prefix.Length, _data, 0);
    }

    public static int itoa(int n, Span<byte> result, int radix = 10)
    {
        if (0 == n)
        {
            result[0] = 48;
            return 1;
        }

        int index = 10;
        Span<char> buffer = stackalloc char[10];
        string xlat = "0123456789abcdefghijklmnopqrstuvwxyz";

        for (int r = Math.Abs(n), q; r > 0; r = q)
        {
            q = Math.DivRem(r, radix, out r);
            buffer[--index] = xlat[r];
        }

        if (n < 0)
        {
            buffer[--index] = '-';
        }

        buffer = buffer.Slice(index);
        if (result.Length <= buffer.Length) throw new ArgumentException("write buffer to small");

        Encoding.UTF8.GetBytes(buffer, result);
        return index;
    }

    public override string ToString()
    {
        return Encoding.UTF8.GetString(_data.AsSpan(0, len));
    }

    private Span<byte> GetAppendSpan() => new(_data, len, _data.Length - len);

    private Span<byte> GetAppendSpan(int size)
    {
    #if debug
        if (BytesLeft < size)
        {
            throw new ArgumentException($"Requested {size} bytes, but there were only {BytesLeft} left");
        }
    #endif

        return new(_data, len, size);
    }
}

}