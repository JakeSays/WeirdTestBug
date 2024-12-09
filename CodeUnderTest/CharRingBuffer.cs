using System.Collections;

namespace Things;

public class CharRingBuffer : ICollection<char>, ICollection
{
    private readonly char[] _buffer;
    private long _readIndex;
    private long _writeIndex;
    private int _characterCount;
    private readonly bool _clearAfterRead;

    public CharRingBuffer(int size, bool clearAfterRead = false)
    {
        Size = size;
        _buffer = new char[Size];
        _clearAfterRead = clearAfterRead;
    }

    public int Size { get; }
    public bool CanRead => _characterCount > 0;
    public bool CanWrite => _characterCount < Size;

    public int WriteSpaceAvailable => Size - _characterCount;
    public int ReadCharsAvailable => _characterCount;
    
    public void Clear()
    {
        _readIndex = 0;
        _writeIndex = 0;
        _characterCount = 0;

        if (!_clearAfterRead)
        {
            return;
        }

        for (var index = 0; index < Size; index++)
        {
            _buffer[index] = default;
        }
    }
    
    public bool Write(string value)
    {
        if (WriteSpaceAvailable < value.Length)
        {
            return false;
        }

        foreach (var ch in value)
        {
            _buffer[_writeIndex++ % Size] = ch;
        }

        //NOTE: the bug happens here. after the following line is executed,
        //while running under:
        //  unit test: _characterCount == 0, should be 26
        //             _readIndex == 26, should be 0
        //  console:   _characterCount == 26
        //             _readIndex == 0
        //so the code runs correctly outside of the unit test framework.
        _characterCount += value.Length;
        
        return true;
    }
    
    public bool Write(char value)
    {
        if (!CanWrite)
        {
            return false;
        }
        
        _buffer[_writeIndex++ % Size] = value;
        _characterCount += 1;
        
        return true;
    }
    
    public char? Read()
    {
        if (!CanRead)
        {
            return null;
        }

        var index = _readIndex % Size;
        var c = _buffer[index];

        if (_clearAfterRead)
        {
            _buffer[index] = default;
        }

        _readIndex++;
        _characterCount -= 1;
        
        return c;
    }

    public string ReadAll()
    {
        if (!CanRead)
        {
            return "";
        }
        
        var text = ToString();
        
        Clear();
        
        return text;
    }

    public char[] ToArray()
    {
        if (!CanRead)
        {
            return [];
        }

        char[]? buffer = null;
        CopyToArray(ref buffer);
        
        return buffer!;
    }
    
    public override string ToString()
    {
        if (!CanRead)
        {
            return "";
        }

        var text = new string(ToArray());
        return text;
    }

    IEnumerator<char> IEnumerable<char>.GetEnumerator()
    {
        if (!CanRead)
        {
            yield break;
        }

        var savedReadIndex = _readIndex;
        
        while (CanRead)
        {
            yield return (char) Read()!;
        }
        
        _readIndex = savedReadIndex;
    }
    
    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable<char>) this).GetEnumerator();
    }

    private void CopyToArray(ref char[]? buffer, int index = 0)
    {
        buffer ??= new char[_characterCount];
        
        if (!CanRead)
        {
            return;
        }

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(buffer.Length, index);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(_readIndex, buffer.Length - index);

        while (CanRead)
        {
            buffer[index++] = (char) Read()!;
        }
    }
    
    void ICollection.CopyTo(Array array, int index)
    {
        if (array.GetType() != typeof(char[]))
        {
            throw new ArgumentException("The array must be of type char[]", nameof(array));
        }
        
        var destination = (char[]) array;
        CopyToArray(ref destination, index);        
    }

    int ICollection.Count => _characterCount;
    bool ICollection.IsSynchronized => false;
    object ICollection.SyncRoot => this;

    void ICollection<char>.Add(char item) => Write(item);

    bool ICollection<char>.Contains(char item) => this.Any(ch => ch == item);

    void ICollection<char>.CopyTo(char[] array, int arrayIndex)
    {
        CopyToArray(ref array!, arrayIndex);
    }

    bool ICollection<char>.Remove(char item) => throw new NotSupportedException();

    int ICollection<char>.Count => _characterCount;

    bool ICollection<char>.IsReadOnly => false;
}
