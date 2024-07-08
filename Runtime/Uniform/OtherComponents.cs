using System;
using System.Runtime.CompilerServices;
using Core.Runtime;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;

namespace SpatialHashing.Uniform
{
    public interface IUniformSpatialQueryCollector
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnVisit(in UniformSpatialCell cell, in UnsafeList<UniformSpatialElement> elements, out bool shouldEarlyExit);
    }
    
    public partial struct UniformSpatialNode : IComponentData
    {
        public XaObjectType type;
    }


    [InternalBufferCapacity(0)]
    public partial struct UniformSpatialElement : IBufferElementData
    {
        public float3 position;
        public Entity entity;
        public XaObjectType type;
        // public byte subType;
    }

    [InternalBufferCapacity(0)]
    public partial struct UniformSpatialCell : IBufferElementData
    {
        public int start;
        public int capacity;
        public int length;
        public int excess;
    }

    [Serializable]
    public partial struct UniformGridBaseSettings : IComponentData
    {
        public int subdivisions;
        public float halfExtents;
    }

    

}