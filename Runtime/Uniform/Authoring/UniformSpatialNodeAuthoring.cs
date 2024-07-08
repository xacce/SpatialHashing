#if UNITY_EDITOR
using Unity.Entities;
using UnityEngine;
using Core.Runtime;

namespace SpatialHashing.Uniform.Authoring
{
    public class UniformSpatialNodeAuthoring : MonoBehaviour
    {
        [SerializeField] private XaObjectType type_s;

        private class UniformSpatialNodeBaker : Baker<UniformSpatialNodeAuthoring>
        {
            public override void Bake(UniformSpatialNodeAuthoring authoring)
            {
                var e = GetEntity(authoring, TransformUsageFlags.WorldSpace);
                AddComponent(
                    e,
                    new UniformSpatialNode()
                    {
                        type = authoring.type_s,
                    });
            }
        }

        public XaObjectType type => type_s;
    }
}
#endif