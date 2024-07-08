using UniformSpatialHashing;
using UniformSpatialHashing.Jobs;
using Unity.Burst;
using Unity.Entities;
using UniformSpatialClearJob = SpatialHashing.Uniform.Jobs.UniformSpatialClearJob;

namespace SpatialHashing.Uniform.Systems
{
    [BurstCompile]
    public partial struct SpatialHashingSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<UniformSpatialDatabase>();
            _lookups = new Lookups(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _lookups.Update(ref state);
            state.Dependency = new UniformSpatialClearJob()
            {
                uniformSpatialCellRw = _lookups.uniformSpatialCellRw,
                uniformSpatialElementRw = _lookups.uniformSpatialElementRw,
            }.Schedule(state.Dependency);

            state.Dependency = new UniformSpatialRegisterElementsJob()
            {
                bridge = new UniformSpatialDatabaseBridge()
                {
                    entity = SystemAPI.GetSingletonEntity<UniformSpatialDatabase>(),
                    uniformSpatialCellRw = _lookups.uniformSpatialCellRw,
                    uniformSpatialDatabaseRw = _lookups.uniformSpatialDatabaseRw,
                    uniformSpatialElementRw = _lookups.uniformSpatialElementRw,
                }
            }.Schedule(state.Dependency);
        }

        [BurstCompile]
        internal struct Lookups
        {
            public ComponentLookup<UniformSpatialDatabase> uniformSpatialDatabaseRw;

            public BufferLookup<UniformSpatialElement> uniformSpatialElementRw;

            public BufferLookup<UniformSpatialCell> uniformSpatialCellRw;

            public Lookups(ref SystemState state) : this()
            {
                uniformSpatialCellRw = state.GetBufferLookup<UniformSpatialCell>(false);
                uniformSpatialElementRw = state.GetBufferLookup<UniformSpatialElement>(false);
                uniformSpatialDatabaseRw = state.GetComponentLookup<UniformSpatialDatabase>(false);
            }

            [BurstCompile]
            public void Update(ref SystemState state)
            {
                uniformSpatialCellRw.Update(ref state);
                uniformSpatialElementRw.Update(ref state);
                uniformSpatialDatabaseRw.Update(ref state);
            }
        }

        private Lookups _lookups;
    }
}