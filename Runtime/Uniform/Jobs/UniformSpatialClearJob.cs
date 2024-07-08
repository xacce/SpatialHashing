using UniformSpatialHashing;
using Unity.Burst;
using Unity.Entities;

namespace SpatialHashing.Uniform.Jobs
{
    [BurstCompile]
    [WithAll(typeof(Simulate))]
    internal partial struct UniformSpatialClearJob : IJobEntity
    {
        public BufferLookup<UniformSpatialElement> uniformSpatialElementRw;
        public BufferLookup<UniformSpatialCell> uniformSpatialCellRw;
        
        [BurstCompile]
        public void Execute(Entity entity, UniformSpatialDatabase database)
        {
            if (uniformSpatialElementRw.TryGetBuffer(entity, out var elements) && uniformSpatialCellRw.TryGetBuffer(entity, out var cells))
            {
                UniformSpatialDatabase.ClearAndResize(in database.grid,ref cells,ref elements);
            }
        }
    }
}