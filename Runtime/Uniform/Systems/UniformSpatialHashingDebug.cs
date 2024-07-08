using UniformSpatialHashing;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace SpatialHashing.Uniform.Systems
{
    public partial class UniformSpatialHashingDebug : SystemBase
    {
        protected override void OnUpdate()
        {
            // foreach (var (db, elements, cells) in SystemAPI.Query<RefRO<UniformSpatialDatabase>, DynamicBuffer<UniformSpatialElement>, DynamicBuffer<UniformSpatialCell>>())
            // {
            //     for (int i = 0; i < cells.Length; i++)
            //     {
            //         var cell = cells[i];
            //         for (int j = 0; j < cell.length; j++)
            //         {
            //             var coords = UniformGrid.GetCellCoordsFromPosition(in db.ValueRO.grid, elements[cell.start + j].position);
            //             Debug.DrawLine((float3)UniformGrid.GetCellCenter( db.ValueRO.grid.boundsMin, db.ValueRO.grid.cellSize, coords), elements[cell.start + j].position);
            //         }
            //     }
            // }
        }
    }
}