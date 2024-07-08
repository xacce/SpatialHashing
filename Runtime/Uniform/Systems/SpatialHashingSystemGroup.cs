using Unity.Entities;

namespace SpatialHashing.Uniform.Systems
{
    [UpdateBefore(typeof(SimulationSystemGroup))]
    public partial class SpatialHashingSystemGroup : ComponentSystemGroup
    {

    }
}