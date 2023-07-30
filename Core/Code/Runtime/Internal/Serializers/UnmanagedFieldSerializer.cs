using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UFlow.Addon.Serialization.Core.Runtime;

namespace UFlow.Addon.SaveState.Core.Runtime {
    internal sealed class UnmanagedFieldSerializer<TObject, TField> : ISerializer<TObject> where TField : unmanaged {
        private readonly int m_offset;

        public UnmanagedFieldSerializer(in FieldInfo fieldInfo) => m_offset = (int)Marshal.OffsetOf<TObject>(fieldInfo.Name);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Serialize(in ByteBuffer buffer, ref TObject value) {
            var objPtr = Unsafe.AsPointer(ref value);
            var fieldPtr = (TField*)((byte*)objPtr + m_offset);
            var fieldValue = Unsafe.Read<TField>(fieldPtr);
            buffer.WriteUnsafe(fieldValue);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Deserialize(in ByteBuffer buffer, ref TObject value) {
            var objPtr = Unsafe.AsPointer(ref value);
            var fieldPtr = (TField*)((byte*)objPtr + m_offset);
            Unsafe.Write(fieldPtr, buffer.ReadUnsafe<TField>());
        }
    }
}