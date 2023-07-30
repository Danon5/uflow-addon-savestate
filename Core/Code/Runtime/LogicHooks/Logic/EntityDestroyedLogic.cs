using UFlow.Addon.ECS.Core.Runtime;
using UFlow.Core.Runtime;

namespace UFlow.Addon.SaveState.Core.Runtime {
    public struct EntityDestroyedLogic : ILogic<EntityDestroyedHook> {
        public void Execute(in EntityDestroyedHook hook) {
            if (Stashes<InstantiatedSceneEntity>.TryGet(hook.worldId, out var stash) && stash.Has(hook.entity.id))
                stash.Get(hook.entity.id).sceneEntity.DestroyEntity();
        }
    }
}