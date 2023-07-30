using System;
using System.Runtime.CompilerServices;

namespace UFlow.Addon.SaveState.Core.Runtime {
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SaveAttribute : Attribute {
        public readonly int order;

        public SaveAttribute([CallerLineNumber] int order = 0) => this.order = order;
    }
}