using System;
using UnityEngine;

namespace Assets.URP.Runtime.Bokeh
{
    public class Utils
    {
        public static float SmoothStep(float edge0, float edge1, float x)
        {
            x = Mathf.Clamp01((x - edge0) / (edge1 - edge0));
            return x * x * (3f - 2f * x);
        }
    }

    public class BokehFuncs
    {
        public static (float[], Vector2[]) ComputeCircleIntensities(int samples)
        {
            if (samples == 1) return (new float[] { 1.0f }, new Vector2[] { Vector2.zero });

            float[] intensities = new float[samples * samples];
            Vector2[] offsets = new Vector2[samples * samples];
            for (int i = 0; i < samples; i++)
            {
                for (int j = 0; j < samples; j++)
                {
                    var p = new Vector2(i, j) / (samples - 1) * 2.0f - Vector2.one;
                    intensities[i * samples + j] = Utils.SmoothStep(0.0f, 0.5f, 1.0f - p.magnitude);
                    offsets[i * samples + j] = p;
                }
            }
            return (intensities, offsets);
        }

        public static (float[], Vector2[]) ComputePolyIntensities(int samples, int sides, float rotation)
        {
            if (samples == 1) return (new float[] { 1.0f }, new Vector2[] { Vector2.zero });

            Vector2[] vertices = new Vector2[sides], offsets = new Vector2[samples * samples];
            float[] intensities = new float[samples * samples];
            float angleStep = 2.0f * Mathf.PI / sides;
            rotation = Mathf.Deg2Rad * rotation;

            ComputeVertices(rotation, angleStep, vertices);
            Logger.Log(vertices, "vertices", true, "vertices");

            for (int i = 0; i < samples; i++)
            {
                for (int j = 0; j < samples; j++)
                {
                    var p = new Vector2(i, j) / (samples - 1) * 2.0f - Vector2.one;
                    if (p == Vector2.zero)
                    {
                        intensities[i * samples + j] = 1.0f;
                        offsets[i * samples + j] = p;
                        continue;
                    }
                    intensities[i * samples + j] = IntensityPoly(p, rotation, angleStep, vertices);
                    offsets[i * samples + j] = p;
                }
            }
            return (intensities, offsets);
        }

        static void ComputeVertices(float rotation, float angleStep, Vector2[] vertices)
        {
            for (int i = 0; i < vertices.Length; ++i)
            {
                var angle = i * angleStep + rotation;
                vertices[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            }
        }

        static float IntensityPoly(Vector2 p, float rotation, float angleStep, Vector2[] vertices)
        {
            float pAngle = Mathf.Atan2(p.y, p.x);
            pAngle += 4.0f * Mathf.PI;
            
            int index = (int)Mathf.Floor((pAngle - rotation)  / angleStep) % vertices.Length;
            // Debug.Log($"pAngle {pAngle} index {index}");
            var v1 = vertices[index];
            var v2 = vertices[(index == vertices.Length - 1) ? 0 : index + 1];

            var dir = p.normalized;
            var edge = v2 - v1;
            float t = (v1.x * edge.y - v1.y * edge.x) / (dir.x * edge.y - dir.y * edge.x);
            // Debug.Log($"{p} intersects with {v1} {v2} at {t * dir}");
            return Utils.SmoothStep(0.0f, 0.5f, 1.0f - p.magnitude / t);
        }
    }
}
