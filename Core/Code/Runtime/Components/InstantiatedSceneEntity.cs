using Sirenix.OdinInspector;
using UFlow.Addon.ECS.Core.Runtime;

namespace UFlow.Addon.SaveState.Core.Runtime {
    [EcsSerializable("InstantiatedSceneEntity")]
    public struct InstantiatedSceneEntity : IEcsComponent {
        [ReadOnly] public SceneEntity sceneEntity;
        [ReadOnly] public string persistentKey;
    }
}