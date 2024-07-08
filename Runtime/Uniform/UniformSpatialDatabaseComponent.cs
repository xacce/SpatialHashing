using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace SpatialHashing.Uniform
{
    public partial struct UniformSpatialDatabase : IComponentData
    {
        public UniformGrid grid;

        public static void Initialize(
            ref UniformSpatialDatabase database,
            ref DynamicBuffer<UniformSpatialElement> elements,
            ref DynamicBuffer<UniformSpatialCell> cells,
            in int cellCapacity,
            in UniformGrid grid)
        {
            elements.Clear();
            cells.Clear();
            elements.Capacity = 0;
            cells.Capacity = 0;
            cells.Resize(grid.cellCount, NativeArrayOptions.ClearMemory);
            elements.Resize(grid.cellCount * cellCapacity, NativeArrayOptions.ClearMemory);
            for (int i = 0; i < cells.Length; i++)
            {
                var cell = cells[i];
                cell.start = i * cellCapacity;
                cell.capacity = cellCapacity;
                cells[i] = cell;
            }
            database.grid = grid;
        }

        public static void ClearAndResize(in UniformGrid grid, ref DynamicBuffer<UniformSpatialCell> cellsBuffer,
            ref DynamicBuffer<UniformSpatialElement> storageBuffer)
        {
            int totalDesiredStorage = 0;
            var growFactor = grid.growFactor;
            for (int i = 0; i < cellsBuffer.Length; i++)
            {
                UniformSpatialCell cell = cellsBuffer[i];
                cell.start = totalDesiredStorage;

                // Handle calculating an increased max storage for this cell
                cell.capacity = math.select(
                    cell.capacity,
                    (int)math.ceil((cell.capacity + cell.excess) * growFactor),
                    cell.excess > 0);
                totalDesiredStorage += cell.capacity;

                // Reset storage
                cell.length = 0;
                cell.excess = 0;

                cellsBuffer[i] = cell;
            }

            storageBuffer.Resize(totalDesiredStorage, NativeArrayOptions.ClearMemory);
        }

        public static void Clear(ref DynamicBuffer<UniformSpatialCell> cells, ref DynamicBuffer<UniformSpatialElement> bufferData)
        {
            for (int i = 0; i < cells.Length; i++)
            {
                var cell = cells[i];
                cell.length = 0;
                cells[i] = cell;
            }
        }

        public static void Add(in UniformSpatialDatabase db, in UniformSpatialElement element, ref UnsafeList<UniformSpatialElement> elements, ref UnsafeList<UniformSpatialCell> cells)
        {
            var cellI = UniformGrid.GetCellIndex(db.grid, element.position);
            if (cellI < 0) return;
            var cell = cells[cellI];
            if (cell.length + 1 >= cell.capacity)
            {
                cell.excess++;
            }
            else
            {
                elements[cell.start + cell.length] = element;
                cell.length++;
            }
            cells[cellI] = cell;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void QueryAABB<T>(in UniformSpatialDatabase spatialDatabase,
            in DynamicBuffer<UniformSpatialCell> cellsBuffer, in DynamicBuffer<UniformSpatialElement> elementsBuffer,
            float3 center, float3 halfExtents, ref T collector)
            where T : unmanaged, IUniformSpatialQueryCollector
        {
            UnsafeList<UniformSpatialCell> cells =
                new UnsafeList<UniformSpatialCell>(
                    (UniformSpatialCell*)cellsBuffer.GetUnsafeReadOnlyPtr(),
                    cellsBuffer.Length);
            UnsafeList<UniformSpatialElement> elements =
                new UnsafeList<UniformSpatialElement>(
                    (UniformSpatialElement*)elementsBuffer.GetUnsafeReadOnlyPtr(),
                    elementsBuffer.Length);
            QueryAABB(in spatialDatabase, in cells, in elements, center, halfExtents, ref collector);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void QueryAABB<T>(in UniformSpatialDatabase spatialDatabase,
            in UnsafeList<UniformSpatialCell> cellsBuffer, in UnsafeList<UniformSpatialElement> elementsBuffer,
            float3 center, float3 halfExtents, ref T collector)
            where T : unmanaged, IUniformSpatialQueryCollector
        {
            float3 aabbMin = center - halfExtents;
            float3 aabbMax = center + halfExtents;
            var grid = spatialDatabase.grid;
            if (UniformGrid.GetAABBMinMaxCoords(
                    in grid,
                    aabbMin,
                    aabbMax,
                    out int3 minCoords,
                    out int3 maxCoords))
            {
                for (int y = minCoords.y; y <= maxCoords.y; y++)
                {
                    for (int z = minCoords.z; z <= maxCoords.z; z++)
                    {
                        for (int x = minCoords.x; x <= maxCoords.x; x++)
                        {
                            int3 coords = new int3(x, y, z);
                            int cellIndex = UniformGrid.GetCellIndexFromCoords(in grid, coords);
                            UniformSpatialCell cell = cellsBuffer[cellIndex];
                            collector.OnVisit(
                                in cell,
                                in elementsBuffer,
                                out bool shouldEarlyExit);
                            if (shouldEarlyExit)
                            {
                                return;
                            }
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void QueryAABBCellProximityOrder<T>(in UniformSpatialDatabase spatialDatabase,
            in DynamicBuffer<UniformSpatialCell> cellsBuffer, in DynamicBuffer<UniformSpatialElement> elementsBuffer,
            float3 center, float3 halfExtents, ref T collector)
            where T : unmanaged, IUniformSpatialQueryCollector
        {
            UnsafeList<UniformSpatialCell> cells =
                new UnsafeList<UniformSpatialCell>(
                    (UniformSpatialCell*)cellsBuffer.GetUnsafeReadOnlyPtr(),
                    cellsBuffer.Length);
            UnsafeList<UniformSpatialElement> elements =
                new UnsafeList<UniformSpatialElement>(
                    (UniformSpatialElement*)elementsBuffer.GetUnsafeReadOnlyPtr(),
                    elementsBuffer.Length);
            QueryAABB(in spatialDatabase, in cells, in elements, center, halfExtents, ref collector);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void QueryAABBCellProximityOrder<T>(in UniformSpatialDatabase spatialDatabase,
            in UnsafeList<UniformSpatialCell> cellsBuffer, in UnsafeList<UniformSpatialElement> elementsBuffer,
            float3 center, float3 halfExtents, ref T collector)
            where T : unmanaged, IUniformSpatialQueryCollector
        {
            float3 aabbMin = center - halfExtents;
            float3 aabbMax = center + halfExtents;
            var grid = spatialDatabase.grid;
            if (UniformGrid.GetAABBMinMaxCoords(in grid, aabbMin, aabbMax, out int3 minCoords, out int3 maxCoords))
            {
                int3 sourceCoord = UniformGrid.GetCellCoordsFromPosition(in grid, center);
                int3 highestCoordDistances = math.max(maxCoords - sourceCoord, sourceCoord - minCoords);
                int maxLayer = math.max(
                    highestCoordDistances.x,
                    math.max(highestCoordDistances.y, highestCoordDistances.z));

                // Iterate layers of cells around the original cell
                for (int l = 0; l <= maxLayer; l++)
                {
                    int2 yRange = new int2(sourceCoord.y - l, sourceCoord.y + l);
                    int2 zRange = new int2(sourceCoord.z - l, sourceCoord.z + l);
                    int2 xRange = new int2(sourceCoord.x - l, sourceCoord.x + l);

                    for (int y = yRange.x; y <= yRange.y; y++)
                    {
                        int yDistToEdge = math.min(y - minCoords.y, maxCoords.y - y); // positive is inside

                        // Skip coords outside of query coords range
                        if (yDistToEdge < 0)
                        {
                            continue;
                        }

                        for (int z = zRange.x; z <= zRange.y; z++)
                        {
                            int zDistToEdge = math.min(z - minCoords.z, maxCoords.z - z); // positive is inside

                            // Skip coords outside of query coords range
                            if (zDistToEdge < 0)
                            {
                                continue;
                            }

                            for (int x = xRange.x; x <= xRange.y; x++)
                            {
                                int xDistToEdge = math.min(x - minCoords.x, maxCoords.x - x); // positive is inside

                                // Skip coords outside of query coords range
                                if (xDistToEdge < 0)
                                {
                                    continue;
                                }

                                int3 coords = new int3(x, y, z);
                                int3 coordDistToCenter = math.abs(coords - sourceCoord);
                                int maxCoordsDist = math.max(
                                    coordDistToCenter.x,
                                    math.max(coordDistToCenter.y, coordDistToCenter.z));

                                // Skip all inner coords not belonging to the external layer
                                if (maxCoordsDist != l)
                                {
                                    x = xRange.y - 1;
                                    continue;
                                }

                                int cellIndex = UniformGrid.GetCellIndexFromCoords(in grid, coords);
                                var cell = cellsBuffer[cellIndex];
                                collector.OnVisit(
                                    in cell,
                                    in elementsBuffer,
                                    out bool shouldEarlyExit);
                                if (shouldEarlyExit)
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}