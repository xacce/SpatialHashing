using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace SpatialHashing.Uniform
{
    public unsafe struct UniformSpatialDatabaseBridge
    {
        public Entity entity;
        public ComponentLookup<UniformSpatialDatabase> uniformSpatialDatabaseRw;
        public BufferLookup<UniformSpatialCell> uniformSpatialCellRw;
        public BufferLookup<UniformSpatialElement> uniformSpatialElementRw;
        public UniformSpatialDatabase database;
        public UnsafeList<UniformSpatialCell> cellsUnsafe;
        public UnsafeList<UniformSpatialElement> elementsUnsafe;

        public bool _created;

        public void CreateBridge()
        {
            if (!_created)
            {
                database = uniformSpatialDatabaseRw[entity];
                DynamicBuffer<UniformSpatialCell> cellsBuffer = uniformSpatialCellRw[entity];
                DynamicBuffer<UniformSpatialElement> elementsBuffer = uniformSpatialElementRw[entity];
                cellsUnsafe = new UnsafeList<UniformSpatialCell>((UniformSpatialCell*)cellsBuffer.GetUnsafePtr(), cellsBuffer.Length);
                elementsUnsafe = new UnsafeList<UniformSpatialElement>((UniformSpatialElement*)elementsBuffer.GetUnsafePtr(), elementsBuffer.Length);
                _created = true;
            }
        }
    }

    public unsafe struct UniformSpatialDatabaseReadonlyBridge
    {
        public Entity entity;
        [ReadOnly] public ComponentLookup<UniformSpatialDatabase> uniformSpatialDatabaseRo;
        [ReadOnly] public BufferLookup<UniformSpatialCell> uniformSpatialCellRo;
        [ReadOnly] public BufferLookup<UniformSpatialElement> uniformSpatialElementRo;
        public UniformSpatialDatabase database;
        public UnsafeList<UniformSpatialCell> cellsUnsafe;
        public UnsafeList<UniformSpatialElement> elementsUnsafe;

        public bool _created;

        public void CreateBridge()
        {
            if (!_created)
            {
                database = uniformSpatialDatabaseRo[entity];
                DynamicBuffer<UniformSpatialCell> cellsBuffer = uniformSpatialCellRo[entity];
                DynamicBuffer<UniformSpatialElement> elementsBuffer = uniformSpatialElementRo[entity];
                cellsUnsafe = new UnsafeList<UniformSpatialCell>((UniformSpatialCell*)cellsBuffer.GetUnsafeReadOnlyPtr(), cellsBuffer.Length);
                elementsUnsafe = new UnsafeList<UniformSpatialElement>((UniformSpatialElement*)elementsBuffer.GetUnsafeReadOnlyPtr(), elementsBuffer.Length);
                _created = true;
            }
        }
    }

}