using System;

namespace UFlow.Addon.SaveState.Core.Runtime {
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class EcsSerializableAttribute : Attribute {
        public readonly string persistentKey;

        public EcsSerializableAttribute(string persistentKey) => this.persistentKey = persistentKey;
    }
}