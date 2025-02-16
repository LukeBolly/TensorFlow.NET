﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tensorflow.NumPy
{
    public class NDArrayConverter
    {
        public unsafe static T Scalar<T>(NDArray nd) where T : unmanaged
            => nd.dtype switch
            {
                TF_DataType.TF_UINT8 => Scalar<T>(*(byte*)nd.data),
                TF_DataType.TF_FLOAT => Scalar<T>(*(float*)nd.data),
                TF_DataType.TF_INT64 => Scalar<T>(*(long*)nd.data),
                _ => throw new NotImplementedException("")
            };

        static T Scalar<T>(byte input)
            => Type.GetTypeCode(typeof(T)) switch
            {
                TypeCode.Byte => (T)Convert.ChangeType(input, TypeCode.Byte),
                TypeCode.Int32 => (T)Convert.ChangeType(input, TypeCode.Int32),
                TypeCode.Single => (T)Convert.ChangeType(input, TypeCode.Single),
                _ => throw new NotImplementedException("")
            };

        static T Scalar<T>(float input)
            => Type.GetTypeCode(typeof(T)) switch
            {
                TypeCode.Byte => (T)Convert.ChangeType(input, TypeCode.Byte),
                TypeCode.Int32 => (T)Convert.ChangeType(input, TypeCode.Int32),
                TypeCode.Single => (T)Convert.ChangeType(input, TypeCode.Single),
                _ => throw new NotImplementedException("")
            };

        static T Scalar<T>(long input)
            => Type.GetTypeCode(typeof(T)) switch
            {
                TypeCode.Byte => (T)Convert.ChangeType(input, TypeCode.Byte),
                TypeCode.Int32 => (T)Convert.ChangeType(input, TypeCode.Int32),
                TypeCode.Single => (T)Convert.ChangeType(input, TypeCode.Single),
                _ => throw new NotImplementedException("")
            };

        public static unsafe Array ToMultiDimArray<T>(NDArray nd) where T : unmanaged
        {
            var ret = Array.CreateInstance(typeof(T), nd.shape.as_int_list());

            var addr = ret switch
            {
                T[] array => Addr(array),
                T[,] array => Addr(array),
                T[,,] array => Addr(array),
                T[,,,] array => Addr(array),
                T[,,,,] array => Addr(array),
                T[,,,,,] array => Addr(array),
                _ => throw new NotImplementedException("")
            };

            System.Buffer.MemoryCopy(nd.data.ToPointer(), addr, nd.bytesize, nd.bytesize);
            return ret;
        }

        #region multiple array
        static unsafe T* Addr<T>(T[] array) where T : unmanaged
        {
            fixed (T* a = &array[0])
                return a;
        }

        static unsafe T* Addr<T>(T[,] array) where T : unmanaged
        {
            fixed (T* a = &array[0, 0])
                return a;
        }

        static unsafe T* Addr<T>(T[,,] array) where T : unmanaged
        {
            fixed (T* a = &array[0, 0, 0])
                return a;
        }

        static unsafe T* Addr<T>(T[,,,] array) where T : unmanaged
        {
            fixed (T* a = &array[0, 0, 0, 0])
                return a;
        }

        static unsafe T* Addr<T>(T[,,,,] array) where T : unmanaged
        {
            fixed (T* a = &array[0, 0, 0, 0, 0])
                return a;
        }

        static unsafe T* Addr<T>(T[,,,,,] array) where T : unmanaged
        {
            fixed (T* a = &array[0, 0, 0, 0, 0, 0])
                return a;
        }
        #endregion
    }
}
