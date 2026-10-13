using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Profiling;
using UnityEditorInternal;
using UnityEngine;

namespace MiniFactory.Editor
{
    [InitializeOnLoad]
    public static class ProfilerDump
    {
        private static bool _dumped = false;

        static ProfilerDump()
        {
            EditorApplication.update += OnUpdate;
        }

        private static void OnUpdate()
        {
            if (_dumped) return;
            _dumped = true;
            EditorApplication.update -= OnUpdate;
            Dump();
        }

        [MenuItem("Tools/Dump Profiler Data")]
        public static void Dump()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== PROFILER DUMP ===");
            sb.AppendLine($"Time: {DateTime.Now}");

            try
            {
                string capturePath = "ProfilerCaptures/Mini Factory_2026-10-13_18-52-39.data";
                if (File.Exists(capturePath))
                {
                    ProfilerDriver.LoadProfile(capturePath, false);
                    sb.AppendLine($"Loaded capture: {capturePath}");
                }

                int firstFrame = ProfilerDriver.firstFrameIndex;
                int lastFrame = ProfilerDriver.lastFrameIndex;
                sb.AppendLine($"FirstFrame: {firstFrame}, LastFrame: {lastFrame}");

                // Sample several frames to get accurate averages
                int startFrame = Math.Max(firstFrame, lastFrame - 100);
                int count = 0;
                float totalFrameTime = 0;
                float totalMainThreadTime = 0;
                long totalGC = 0;
                int gcAllocCount = 0;

                for (int f = startFrame; f <= lastFrame; f++)
                {
                    using (var view = ProfilerDriver.GetHierarchyFrameDataView(
                        f, 0, HierarchyFrameDataView.ViewModes.Default, HierarchyFrameDataView.columnTotalTime, false))
                    {
                        if (view != null && view.valid)
                        {
                            count++;
                            totalFrameTime += view.frameTimeMs;

                            int root = view.GetRootItemID();
                            float rootTime = view.GetItemColumnDataAsFloat(root, HierarchyFrameDataView.columnTotalTime);
                            long rootGc = (long)view.GetItemColumnDataAsFloat(root, HierarchyFrameDataView.columnGcMemory);

                            totalGC += rootGc;
                            if (rootGc > 0) gcAllocCount++;
                            totalMainThreadTime += rootTime;
                        }
                    }
                }

                if (count > 0)
                {
                    float avgFrameTime = totalFrameTime / count;
                    float avgFps = avgFrameTime > 0 ? (1000f / avgFrameTime) : 0;
                    float avgMainThread = totalMainThreadTime / count;
                    sb.AppendLine($"--- Averaged over {count} frames ({startFrame} to {lastFrame}) ---");
                    sb.AppendLine($"Avg Frame Time: {avgFrameTime:F2} ms");
                    sb.AppendLine($"Avg FPS: {avgFps:F1} FPS");
                    sb.AppendLine($"Avg Main Thread Time: {avgMainThread:F2} ms");
                    sb.AppendLine($"Total GC in window: {totalGC} bytes");
                    sb.AppendLine($"Frames with GC Allocations: {gcAllocCount} / {count}");
                    sb.AppendLine($"Avg GC per frame: {(totalGC / (float)count):F1} B/frame");
                }

                // Dump single frame details (last frame)
                if (lastFrame >= 0)
                {
                    using (var view = ProfilerDriver.GetHierarchyFrameDataView(
                        lastFrame, 0, HierarchyFrameDataView.ViewModes.Default, HierarchyFrameDataView.columnTotalTime, false))
                    {
                        if (view != null && view.valid)
                        {
                            int root = view.GetRootItemID();
                            long rootGc = (long)view.GetItemColumnDataAsFloat(root, HierarchyFrameDataView.columnGcMemory);
                            sb.AppendLine($"--- Frame {lastFrame} Detail ---");
                            sb.AppendLine($"Frame Time: {view.frameTimeMs:F2} ms ({(1000f / view.frameTimeMs):F1} FPS)");
                            sb.AppendLine($"Frame GC Memory: {rootGc} bytes");
                            DumpHierarchy(view, root, sb, 0, 4);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"Exception in HierarchyFrameDataView: {ex.Message}\n{ex.StackTrace}");
            }

            try
            {
                sb.AppendLine("--- Memory Profiler ---");
                sb.AppendLine($"TotalAllocatedMemory: {UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / (1024 * 1024)} MB");
                sb.AppendLine($"TotalReservedMemory: {UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong() / (1024 * 1024)} MB");
                sb.AppendLine($"MonoHeapSize: {UnityEngine.Profiling.Profiler.GetMonoHeapSizeLong() / (1024 * 1024)} MB");
                sb.AppendLine($"MonoUsedSize: {UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong() / (1024 * 1024)} MB");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"Exception in Memory: {ex.Message}");
            }

            try
            {
                sb.AppendLine("--- UnityStats ---");
                sb.AppendLine($"Batches: {UnityStats.batches}");
                sb.AppendLine($"DrawCalls: {UnityStats.drawCalls}");
                sb.AppendLine($"SetPassCalls: {UnityStats.setPassCalls}");
                sb.AppendLine($"Triangles: {UnityStats.triangles}");
                sb.AppendLine($"Vertices: {UnityStats.vertices}");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"Exception in UnityStats: {ex.Message}");
            }

            Directory.CreateDirectory("Logs");
            string outPath = "Logs/profiler_dump.txt";
            File.WriteAllText(outPath, sb.ToString());
            Debug.Log($"[ProfilerDump] Data successfully dumped to {outPath}");
        }

        private static void DumpHierarchy(HierarchyFrameDataView view, int id, StringBuilder sb, int depth, int maxDepth)
        {
            if (depth > maxDepth) return;
            string name = view.GetItemName(id);
            float timeMs = view.GetItemColumnDataAsFloat(id, HierarchyFrameDataView.columnTotalTime);
            long gc = (long)view.GetItemColumnDataAsFloat(id, HierarchyFrameDataView.columnGcMemory);

            if (timeMs > 0.05f || gc > 0)
            {
                string indent = new string(' ', depth * 2);
                sb.AppendLine($"{indent}- {name}: {timeMs:F2} ms, GC: {gc} B");
            }

            if (!view.HasItemChildren(id)) return;
            var children = new List<int>();
            view.GetItemChildren(id, children);
            foreach (var ch in children)
            {
                DumpHierarchy(view, ch, sb, depth + 1, maxDepth);
            }
        }
    }
}
