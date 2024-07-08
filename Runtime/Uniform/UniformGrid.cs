using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace SpatialHashing.Uniform
{
    //Based on unity ecs example
    public struct UniformGrid
    {
        public float growFactor;
        public float3 extents;
        public int cellCount;
        public int3 cellCountPerD;
        public float cellSize;
        public float3 boundsMin;
        public float3 boundsMax;
        public int cellCountPerXy;

        public UniformGrid(float cellSize, int3 cellCount, float growFactor)
        {
            this.growFactor = growFactor;
            this.cellSize = cellSize;
            extents = ((float3)(cellCount) * cellSize) / 2;
            cellCountPerD = cellCount;
            cellCountPerXy = cellCountPerD.x * cellCountPerD.z;
            this.cellCount = (cellCountPerD.x * cellCountPerD.y * cellCountPerD.z);
            // cellCountPerDimension = (int)math.pow(2f, subdivisions);
            // cellCountPerPlane = cellCountPerDimension * cellCountPerDimension;
            // cellSize = (halfExtents * 2f) / (float)cellCountPerDimension;
            boundsMin = new float3(-extents);
            boundsMax = new float3(extents );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetSubdivisionLevelForMaxCellSize(float halfExtents, float maxCellSize, int maxSubdivisionLevel = 5)
        {
            for (int s = 1; s <= maxSubdivisionLevel; s++)
            {
                int cellCountPerDimension = (int)math.pow(2f, s);
                float cellSize = (halfExtents * 2f) / (float)cellCountPerDimension;
                if (cellSize < maxCellSize)
                {
                    return s;
                }
            }
            return maxSubdivisionLevel;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInBounds(in UniformGrid grid, float3 position)
        {
            return position.x > grid.boundsMin.x &&
                   position.x < grid.boundsMax.x &&
                   position.y > grid.boundsMin.y &&
                   position.y < grid.boundsMax.y &&
                   position.z > grid.boundsMin.z &&
                   position.z < grid.boundsMax.z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 GetCellCoordsFromPosition(in UniformGrid grid, float3 position)
        {
            float3 localPos = position - grid.boundsMin;
            int3 cellCoords = new int3
            {
                x = (int)math.floor(localPos.x / grid.cellSize),
                y = (int)math.floor(localPos.y / grid.cellSize),
                z = (int)math.floor(localPos.z / grid.cellSize),
            };
            cellCoords = math.clamp(cellCoords, int3.zero, grid.cellCountPerD - 1);
            return cellCoords;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetCellIndex(in UniformGrid grid, float3 position)
        {
            if (IsInBounds(in grid, position))
            {
                int3 cellCoords = GetCellCoordsFromPosition(in grid, position);
                return GetCellIndexFromCoords(in grid, cellCoords);
            }

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetCellIndexFromCoords(in UniformGrid grid, int3 coords)
        {
            return (coords.x) +
                   (coords.z * grid.cellCountPerD.x) +
                   (coords.y * grid.cellCountPerXy);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AABBIntersectAABB(float3 aabb1Min, float3 aabb1Max, float3 aabb2Min, float3 aabb2Max)
        {
            return (aabb1Min.x <= aabb2Max.x && aabb1Max.x >= aabb2Min.x) &&
                   (aabb1Min.y <= aabb2Max.y && aabb1Max.y >= aabb2Min.y) &&
                   (aabb1Min.z <= aabb2Max.z && aabb1Max.z >= aabb2Min.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GetCellCenter(float3 spatialDatabaseBoundsMin, float cellSize, int3 cellCoords)
        {
            float3 minCenter = spatialDatabaseBoundsMin + new float3(cellSize * 0.5f);
            return minCenter + ((float3)cellCoords * cellSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetDistanceSqAABBToPoint(float3 point, float3 aabbMin, float3 aabbMax)
        {
            float3 pointOnBounds = math.clamp(point, aabbMin, aabbMax);
            return math.lengthsq(pointOnBounds - point);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetAABBMinMaxCoords(in UniformGrid grid, float3 aabbMin, float3 aabbMax, out int3 minCoords, out int3 maxCoords)
        {
            if (AABBIntersectAABB(aabbMin, aabbMax, grid.boundsMin, grid.boundsMax))
            {
                // Clamp to bounds
                aabbMin = math.clamp(aabbMin, grid.boundsMin, grid.boundsMax);
                aabbMax = math.clamp(aabbMax, grid.boundsMin, grid.boundsMax);

                // Get min max coords
                minCoords = GetCellCoordsFromPosition(in grid, aabbMin);
                maxCoords = GetCellCoordsFromPosition(in grid, aabbMax);

                return true;
            }

            minCoords = new int3(-1);
            maxCoords = new int3(-1);
            return false;
        }
    }
}