#if UNITY_EDITOR
using UniformSpatialHashing;
using Unity.Entities;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace SpatialHashing.Uniform.Authoring
{
    public class UniformGridAuthoring : MonoBehaviour
    {
        // [SerializeField] private int subdivisions;
        [SerializeField] private float cellSize;
        [SerializeField] private int3 cellCount;
        [SerializeField] private int cellCapacity_s;
        [SerializeField] private float growFactor_s = 1;
        [SerializeField] private bool debug;

        [SerializeField] private float3 position;
        [SerializeField] private int3 index;

        private class UniformGridBaker : Baker<UniformGridAuthoring>
        {
            public override void Bake(UniformGridAuthoring authoring)
            {
                var e = GetEntity(TransformUsageFlags.None);
                var cells = AddBuffer<UniformSpatialCell>(e);
                var elements = AddBuffer<UniformSpatialElement>(e);
                var grid = new UniformGrid(authoring.cellSize, authoring.cellCount, authoring.growFactor_s);
                var db = new UniformSpatialDatabase();
                UniformSpatialDatabase.Initialize(ref db, ref elements, ref cells, authoring.cellCapacity_s, grid);
                AddComponent(e, db);
            }
        }

        private void OnDrawGizmosSelected()
        {
            var grid = new UniformGrid(cellSize, cellCount, growFactor_s);
            Gizmos.DrawWireCube(default,grid.extents*2);
            if (debug)
            {
                // Draw grid cells
                Color col = Color.cyan;
                float colMultiplier = 0.3f;
                col.r *= colMultiplier;
                col.g *= colMultiplier;
                col.b *= colMultiplier;
                Gizmos.color = col;

                int3 maxCoords = grid.cellCountPerD;
                float3 cellSize3 = new float3(grid.cellSize);
                float3 minCenter = grid.boundsMin + (grid.cellSize * 0.5f);

                for (int y = 0; y < maxCoords.y; y++)
                {
                    for (int z = 0; z < maxCoords.z; z++)
                    {
                        for (int x = 0; x < maxCoords.x; x++)
                        {
                            float3 cellCenter = minCenter + new float3
                            {
                                x = x * grid.cellSize,
                                y = y * grid.cellSize,
                                z = z * grid.cellSize,
                            };
                            // Handles.Label(cellCenter, $"{UniformGrid.GetCellIndex(grid, cellCenter)}");
                            Gizmos.DrawWireCube(cellCenter, cellSize3);
                        }
                    }
                }

            }
            // var cellIndex = UniformGrid.GetCellIndex(grid, position);
            // var cords = UniformGrid.GetCellCoordsFromPosition(grid, position);
            // var center = UniformGrid.GetCellCenter(grid.boundsMin, grid.cellSize, cords);
            // Debug.Log($"{cellIndex}:{center}");
            // Gizmos.color = Color.red;
            // // Gizmos.DrawWireCube((float3)UniformGrid.GetCellCoordsFromPosition(grid, position), new float3(grid.cellSize));
            // Gizmos.DrawWireCube(center, new float3(grid.cellSize));
            // Gizmos.color = Color.cyan;
            // Gizmos.DrawWireCube(default, new float3(grid.extents) * 2f);

        }
    }
}

#endif