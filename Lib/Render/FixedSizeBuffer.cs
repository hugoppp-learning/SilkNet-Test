using System;
using System.Collections.Generic;

namespace Lib.Render
{

public class FixedSizeBuffer<T>
    where T : unmanaged
{
    private readonly T[] _arr;

    public FixedSizeBuffer(int size)
    {
        Count = 0;
        _arr = new T[size];
    }

    public int Count { get; private set; }

    public int Capacity => _arr.Length;
    public ReadOnlySpan<T> Items => _arr.AsSpan(0, Count);

    public T this[int index] => _arr[index];

    public bool TryAdd(in T item)
    {
        if (Count == _arr.Length)
            return false;

        _arr[Count++] = item;
        return true;
    }

    public Span<T> AddViaMap(int vertexMapCount)
    {
        int newCount = Count + vertexMapCount;
        if (newCount > _arr.Length)
            throw new InvalidOperationException();


        var t = new Span<T>(_arr, Count, vertexMapCount);
        Count = newCount;
        return t;
    }

    public void Add(ReadOnlySpan<T> item)
    {
        item.CopyTo(AddViaMap(item.Length));
    }

    public void Add(in T item)
    {
        if (Count == _arr.Length)
            throw new InvalidOperationException();

        _arr[Count++] = item;
    }

    public void Clear()
    {
    #if DEBUG
        Array.Clear(_arr, 0, _arr.Length);
    #endif
        Count = 0;
    }

    public int IndexOf(in T value)
    {
        return Array.IndexOf(_arr, value);
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < Count; i++)
            yield return _arr[i];
    }
}

}