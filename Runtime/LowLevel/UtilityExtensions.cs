using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace MediaFramework.LowLevel
{
    public static class UtilityExtensions
    {
        public unsafe static ref T AsRef<T>(this NativeReference<T> native) where T : unmanaged
        {
            return ref UnsafeUtility.AsRef<T>(native.GetUnsafePtr());
        }
    }
}
