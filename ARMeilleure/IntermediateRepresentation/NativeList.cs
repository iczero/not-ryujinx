﻿using System;
using System.Runtime.InteropServices;

namespace ARMeilleure.IntermediateRepresentation
{
    unsafe struct NativeList<T> where T : unmanaged
    {
        private ushort _count;
        private ushort _capacity;
        private T* _data;

        public int Count => _count;
        public int Capacity => _capacity;

        public ref T this[int index]
        {
            get
            {
                if ((uint)index >= _count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return ref _data[index];
            }
        }

        public Span<T> Span => new(_data, _count);

        public void Add(in T item)
        {
            int newCount = _count + 1;
            if (newCount > ushort.MaxValue)
            {
                throw new OverflowException();
            }

            if (newCount >= _capacity)
            {
                var oldData = _data;
                var oldSpan = Span;

                _capacity += 8;
                _data = (T*)Marshal.AllocHGlobal(sizeof(T) * _capacity);

                oldSpan.CopyTo(Span);
                Marshal.FreeHGlobal((IntPtr)oldData);
            }

            _data[_count++] = item;
        }

        public bool Remove(in T item)
        {
            int index = -1;
            var span = Span;

            for (int i = 0; i < span.Length; i++)
            {
                if (span[i].Equals(item))
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
            {
                return false;
            }

            RemoveAt(index);

            return true;
        }

        public void RemoveAt(int index)
        {
            if (index + 1 < _count)
            {
                var span = Span;

                span.Slice(index + 1).CopyTo(span.Slice(index));
            }

            _count--;
        }

        public void Clear()
        {
            _count = 0;
        }

        public T[] ToArray()
        {
            return Span.ToArray();
        }

        public static NativeList<T> New()
        {
            var result = new NativeList<T>();

            result._count = 0;
            result._capacity = 4;
            result._data = (T*)Marshal.AllocHGlobal(sizeof(T) * result._capacity);

            return result;
        }
    }
}
