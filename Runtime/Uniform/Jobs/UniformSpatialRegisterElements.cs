using SpatialHashing.Uniform;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Entities;
using Unity.Transforms;

namespace UniformSpatialHashing.Jobs
{

    [BurstCompile]
    [WithAll(typeof(Simulate))]
    internal partial struct UniformSpatialRegisterElementsJob : IJobEntity, IJobEntityChunkBeginEnd
    {
        public UniformSpatialDatabaseBridge bridge;
        private UniformGrid _grid;

        [BurstCompile]
        private void Execute(in LocalToWorld ltw, in UniformSpatialNode node, in Entity entity)
        {
            var element = new UniformSpatialElement()
            {
                entity = entity,
                position = ltw.Position,
                type = node.type,
            };
            UniformSpatialDatabase.Add(bridge.database, element, ref bridge.elementsUnsafe, ref bridge.cellsUnsafe);
        }

        public bool OnChunkBegin(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            bridge.CreateBridge();
            _grid = bridge.database.grid;
            return true;
        }

        public void OnChunkEnd(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask, bool chunkWasExecuted)
        {
        }
    }
}