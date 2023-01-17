using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using System.Linq;

namespace Metamesh {

[System.Serializable]
public class Teapot
{
    public int Subdivision = 10;

    public void Generate(Mesh mesh)
    {
        var P = Patches;

        // Vertex array construction
        var vtx = new List<float3>();
        var nrm = new List<float3>();
        var uv0 = new List<float2>();
        for (var offs = 0; offs < P.Length; offs += 16)
        {
            for (var col = 0; col < Subdivision; col++)
            {
                var i = offs;
                var u = (float)col / (Subdivision - 1);
                var c0 = Evaluate(P[i++], P[i++], P[i++], P[i++], u);
                var c1 = Evaluate(P[i++], P[i++], P[i++], P[i++], u);
                var c2 = Evaluate(P[i++], P[i++], P[i++], P[i++], u);
                var c3 = Evaluate(P[i++], P[i++], P[i++], P[i++], u);
                for (var row = 0; row < Subdivision; row++)
                {
                    var v = (float)row / (Subdivision - 1);
                    var p = Evaluate(c0.p, c1.p, c2.p, c3.p, v);
                    var du = p.d;
                    var dv = Bezier(c0.d, c1.d, c2.d, c3.d, v);
                    if (math.length(dv) < math.FLT_MIN_NORMAL) dv = c1.d;
                    vtx.Add(p.p);
                    nrm.Add(math.normalize(math.cross(du, dv)));
                    uv0.Add(math.float2(u, v));
                }
            }
        }

        // Index array construction
        var idx = new List<int>();
        for (var offs = 0; offs < vtx.Count; offs += Subdivision * Subdivision)
        {
            for (var row = 0; row < Subdivision - 1; row++)
            {
                for (var col = 0; col < Subdivision - 1; col++)
                {
                    // Quad indices
                    var i0 = offs + row * Subdivision + col;
                    var i1 = i0 + 1;
                    var i2 = i0 + Subdivision;
                    var i3 = i0 + Subdivision + 1;
                    // First triangle
                    idx.Add(i0);
                    idx.Add(i1);
                    idx.Add(i2);
                    // Second triangle
                    idx.Add(i1);
                    idx.Add(i3);
                    idx.Add(i2);
                }
            }
        }

        // Mesh object construction
        mesh.SetVertices(vtx.Select(v => (Vector3)v).ToList());
        mesh.SetNormals(nrm.Select(n => (Vector3)n).ToList());
        mesh.SetUVs(0, uv0.Select(t => (Vector2)t).ToList());
        mesh.SetIndices(idx, MeshTopology.Triangles, 0);
    }

    // Surface evaluation function
    (float3 p, float3 d)
      Evaluate(float3 p0, float3 p1, float3 p2, float3 p3, float t)
        => (Bezier(p0, p1, p2, p3, t), BezierD(p0, p1, p2, p3, t));

    // Bezier curve function
    float3 Bezier(float3 p0, float3 p1, float3 p2, float3 p3, float t)
    {
        var mt = 1 - t;
        return     mt * mt * mt * p0 +
               3 * mt * mt *  t * p1 +
               3 * mt *  t *  t * p2 +
                    t *  t *  t * p3;
    }

    // Derivative of Bezier
    float3 BezierD(float3 p0, float3 p1, float3 p2, float3 p3, float t)
    {
        var mt = 1 - t;
        return 3 * mt * mt * (p1 - p0) +
               6 * mt *  t * (p2 - p1) +
               3 *  t *  t * (p3 - p2);
    }

    // Utah teapot patches from CGA
    // http://www.holmes3d.net/graphics/teapot/
    static readonly float3[] Patches =
    {
        math.float3(1.4f, 2.4f, 0.0f),
        math.float3(1.4f, 2.4f, -0.784f),
        math.float3(0.784f, 2.4f, -1.4f),
        math.float3(0.0f, 2.4f, -1.4f),
        math.float3(1.3375f, 2.53125f, 0.0f),
        math.float3(1.3375f, 2.53125f, -0.749f),
        math.float3(0.749f, 2.53125f, -1.3375f),
        math.float3(0.0f, 2.53125f, -1.3375f),
        math.float3(1.4375f, 2.53125f, 0.0f),
        math.float3(1.4375f, 2.53125f, -0.805f),
        math.float3(0.805f, 2.53125f, -1.4375f),
        math.float3(0.0f, 2.53125f, -1.4375f),
        math.float3(1.5f, 2.4f, 0.0f),
        math.float3(1.5f, 2.4f, -0.84f),
        math.float3(0.84f, 2.4f, -1.5f),
        math.float3(0.0f, 2.4f, -1.5f),
        math.float3(0.0f, 2.4f, -1.4f),
        math.float3(-0.784f, 2.4f, -1.4f),
        math.float3(-1.4f, 2.4f, -0.784f),
        math.float3(-1.4f, 2.4f, 0.0f),
        math.float3(0.0f, 2.53125f, -1.3375f),
        math.float3(-0.749f, 2.53125f, -1.3375f),
        math.float3(-1.3375f, 2.53125f, -0.749f),
        math.float3(-1.3375f, 2.53125f, 0.0f),
        math.float3(0.0f, 2.53125f, -1.4375f),
        math.float3(-0.805f, 2.53125f, -1.4375f),
        math.float3(-1.4375f, 2.53125f, -0.805f),
        math.float3(-1.4375f, 2.53125f, 0.0f),
        math.float3(0.0f, 2.4f, -1.5f),
        math.float3(-0.84f, 2.4f, -1.5f),
        math.float3(-1.5f, 2.4f, -0.84f),
        math.float3(-1.5f, 2.4f, 0.0f),
        math.float3(-1.4f, 2.4f, 0.0f),
        math.float3(-1.4f, 2.4f, 0.784f),
        math.float3(-0.784f, 2.4f, 1.4f),
        math.float3(0.0f, 2.4f, 1.4f),
        math.float3(-1.3375f, 2.53125f, 0.0f),
        math.float3(-1.3375f, 2.53125f, 0.749f),
        math.float3(-0.749f, 2.53125f, 1.3375f),
        math.float3(0.0f, 2.53125f, 1.3375f),
        math.float3(-1.4375f, 2.53125f, 0.0f),
        math.float3(-1.4375f, 2.53125f, 0.805f),
        math.float3(-0.805f, 2.53125f, 1.4375f),
        math.float3(0.0f, 2.53125f, 1.4375f),
        math.float3(-1.5f, 2.4f, 0.0f),
        math.float3(-1.5f, 2.4f, 0.84f),
        math.float3(-0.84f, 2.4f, 1.5f),
        math.float3(0.0f, 2.4f, 1.5f),
        math.float3(0.0f, 2.4f, 1.4f),
        math.float3(0.784f, 2.4f, 1.4f),
        math.float3(1.4f, 2.4f, 0.784f),
        math.float3(1.4f, 2.4f, 0.0f),
        math.float3(0.0f, 2.53125f, 1.3375f),
        math.float3(0.749f, 2.53125f, 1.3375f),
        math.float3(1.3375f, 2.53125f, 0.749f),
        math.float3(1.3375f, 2.53125f, 0.0f),
        math.float3(0.0f, 2.53125f, 1.4375f),
        math.float3(0.805f, 2.53125f, 1.4375f),
        math.float3(1.4375f, 2.53125f, 0.805f),
        math.float3(1.4375f, 2.53125f, 0.0f),
        math.float3(0.0f, 2.4f, 1.5f),
        math.float3(0.84f, 2.4f, 1.5f),
        math.float3(1.5f, 2.4f, 0.84f),
        math.float3(1.5f, 2.4f, 0.0f),
        math.float3(1.5f, 2.4f, 0.0f),
        math.float3(1.5f, 2.4f, -0.84f),
        math.float3(0.84f, 2.4f, -1.5f),
        math.float3(0.0f, 2.4f, -1.5f),
        math.float3(1.75f, 1.875f, 0.0f),
        math.float3(1.75f, 1.875f, -0.98f),
        math.float3(0.98f, 1.875f, -1.75f),
        math.float3(0.0f, 1.875f, -1.75f),
        math.float3(2.0f, 1.35f, 0.0f),
        math.float3(2.0f, 1.35f, -1.12f),
        math.float3(1.12f, 1.35f, -2.0f),
        math.float3(0.0f, 1.35f, -2.0f),
        math.float3(2.0f, 0.9f, 0.0f),
        math.float3(2.0f, 0.9f, -1.12f),
        math.float3(1.12f, 0.9f, -2.0f),
        math.float3(0.0f, 0.9f, -2.0f),
        math.float3(0.0f, 2.4f, -1.5f),
        math.float3(-0.84f, 2.4f, -1.5f),
        math.float3(-1.5f, 2.4f, -0.84f),
        math.float3(-1.5f, 2.4f, 0.0f),
        math.float3(0.0f, 1.875f, -1.75f),
        math.float3(-0.98f, 1.875f, -1.75f),
        math.float3(-1.75f, 1.875f, -0.98f),
        math.float3(-1.75f, 1.875f, 0.0f),
        math.float3(0.0f, 1.35f, -2.0f),
        math.float3(-1.12f, 1.35f, -2.0f),
        math.float3(-2.0f, 1.35f, -1.12f),
        math.float3(-2.0f, 1.35f, 0.0f),
        math.float3(0.0f, 0.9f, -2.0f),
        math.float3(-1.12f, 0.9f, -2.0f),
        math.float3(-2.0f, 0.9f, -1.12f),
        math.float3(-2.0f, 0.9f, 0.0f),
        math.float3(-1.5f, 2.4f, 0.0f),
        math.float3(-1.5f, 2.4f, 0.84f),
        math.float3(-0.84f, 2.4f, 1.5f),
        math.float3(0.0f, 2.4f, 1.5f),
        math.float3(-1.75f, 1.875f, 0.0f),
        math.float3(-1.75f, 1.875f, 0.98f),
        math.float3(-0.98f, 1.875f, 1.75f),
        math.float3(0.0f, 1.875f, 1.75f),
        math.float3(-2.0f, 1.35f, 0.0f),
        math.float3(-2.0f, 1.35f, 1.12f),
        math.float3(-1.12f, 1.35f, 2.0f),
        math.float3(0.0f, 1.35f, 2.0f),
        math.float3(-2.0f, 0.9f, 0.0f),
        math.float3(-2.0f, 0.9f, 1.12f),
        math.float3(-1.12f, 0.9f, 2.0f),
        math.float3(0.0f, 0.9f, 2.0f),
        math.float3(0.0f, 2.4f, 1.5f),
        math.float3(0.84f, 2.4f, 1.5f),
        math.float3(1.5f, 2.4f, 0.84f),
        math.float3(1.5f, 2.4f, 0.0f),
        math.float3(0.0f, 1.875f, 1.75f),
        math.float3(0.98f, 1.875f, 1.75f),
        math.float3(1.75f, 1.875f, 0.98f),
        math.float3(1.75f, 1.875f, 0.0f),
        math.float3(0.0f, 1.35f, 2.0f),
        math.float3(1.12f, 1.35f, 2.0f),
        math.float3(2.0f, 1.35f, 1.12f),
        math.float3(2.0f, 1.35f, 0.0f),
        math.float3(0.0f, 0.9f, 2.0f),
        math.float3(1.12f, 0.9f, 2.0f),
        math.float3(2.0f, 0.9f, 1.12f),
        math.float3(2.0f, 0.9f, 0.0f),
        math.float3(2.0f, 0.9f, 0.0f),
        math.float3(2.0f, 0.9f, -1.12f),
        math.float3(1.12f, 0.9f, -2.0f),
        math.float3(0.0f, 0.9f, -2.0f),
        math.float3(2.0f, 0.45f, 0.0f),
        math.float3(2.0f, 0.45f, -1.12f),
        math.float3(1.12f, 0.45f, -2.0f),
        math.float3(0.0f, 0.45f, -2.0f),
        math.float3(1.5f, 0.225f, 0.0f),
        math.float3(1.5f, 0.225f, -0.84f),
        math.float3(0.84f, 0.225f, -1.5f),
        math.float3(0.0f, 0.225f, -1.5f),
        math.float3(1.5f, 0.15f, 0.0f),
        math.float3(1.5f, 0.15f, -0.84f),
        math.float3(0.84f, 0.15f, -1.5f),
        math.float3(0.0f, 0.15f, -1.5f),
        math.float3(0.0f, 0.9f, -2.0f),
        math.float3(-1.12f, 0.9f, -2.0f),
        math.float3(-2.0f, 0.9f, -1.12f),
        math.float3(-2.0f, 0.9f, 0.0f),
        math.float3(0.0f, 0.45f, -2.0f),
        math.float3(-1.12f, 0.45f, -2.0f),
        math.float3(-2.0f, 0.45f, -1.12f),
        math.float3(-2.0f, 0.45f, 0.0f),
        math.float3(0.0f, 0.225f, -1.5f),
        math.float3(-0.84f, 0.225f, -1.5f),
        math.float3(-1.5f, 0.225f, -0.84f),
        math.float3(-1.5f, 0.225f, 0.0f),
        math.float3(0.0f, 0.15f, -1.5f),
        math.float3(-0.84f, 0.15f, -1.5f),
        math.float3(-1.5f, 0.15f, -0.84f),
        math.float3(-1.5f, 0.15f, 0.0f),
        math.float3(-2.0f, 0.9f, 0.0f),
        math.float3(-2.0f, 0.9f, 1.12f),
        math.float3(-1.12f, 0.9f, 2.0f),
        math.float3(0.0f, 0.9f, 2.0f),
        math.float3(-2.0f, 0.45f, 0.0f),
        math.float3(-2.0f, 0.45f, 1.12f),
        math.float3(-1.12f, 0.45f, 2.0f),
        math.float3(0.0f, 0.45f, 2.0f),
        math.float3(-1.5f, 0.225f, 0.0f),
        math.float3(-1.5f, 0.225f, 0.84f),
        math.float3(-0.84f, 0.225f, 1.5f),
        math.float3(0.0f, 0.225f, 1.5f),
        math.float3(-1.5f, 0.15f, 0.0f),
        math.float3(-1.5f, 0.15f, 0.84f),
        math.float3(-0.84f, 0.15f, 1.5f),
        math.float3(0.0f, 0.15f, 1.5f),
        math.float3(0.0f, 0.9f, 2.0f),
        math.float3(1.12f, 0.9f, 2.0f),
        math.float3(2.0f, 0.9f, 1.12f),
        math.float3(2.0f, 0.9f, 0.0f),
        math.float3(0.0f, 0.45f, 2.0f),
        math.float3(1.12f, 0.45f, 2.0f),
        math.float3(2.0f, 0.45f, 1.12f),
        math.float3(2.0f, 0.45f, 0.0f),
        math.float3(0.0f, 0.225f, 1.5f),
        math.float3(0.84f, 0.225f, 1.5f),
        math.float3(1.5f, 0.225f, 0.84f),
        math.float3(1.5f, 0.225f, 0.0f),
        math.float3(0.0f, 0.15f, 1.5f),
        math.float3(0.84f, 0.15f, 1.5f),
        math.float3(1.5f, 0.15f, 0.84f),
        math.float3(1.5f, 0.15f, 0.0f),
        math.float3(-1.6f, 2.025f, 0.0f),
        math.float3(-1.6f, 2.025f, -0.3f),
        math.float3(-1.5f, 2.25f, -0.3f),
        math.float3(-1.5f, 2.25f, 0.0f),
        math.float3(-2.3f, 2.025f, 0.0f),
        math.float3(-2.3f, 2.025f, -0.3f),
        math.float3(-2.5f, 2.25f, -0.3f),
        math.float3(-2.5f, 2.25f, 0.0f),
        math.float3(-2.7f, 2.025f, 0.0f),
        math.float3(-2.7f, 2.025f, -0.3f),
        math.float3(-3.0f, 2.25f, -0.3f),
        math.float3(-3.0f, 2.25f, 0.0f),
        math.float3(-2.7f, 1.8f, 0.0f),
        math.float3(-2.7f, 1.8f, -0.3f),
        math.float3(-3.0f, 1.8f, -0.3f),
        math.float3(-3.0f, 1.8f, 0.0f),
        math.float3(-1.5f, 2.25f, 0.0f),
        math.float3(-1.5f, 2.25f, 0.3f),
        math.float3(-1.6f, 2.025f, 0.3f),
        math.float3(-1.6f, 2.025f, 0.0f),
        math.float3(-2.5f, 2.25f, 0.0f),
        math.float3(-2.5f, 2.25f, 0.3f),
        math.float3(-2.3f, 2.025f, 0.3f),
        math.float3(-2.3f, 2.025f, 0.0f),
        math.float3(-3.0f, 2.25f, 0.0f),
        math.float3(-3.0f, 2.25f, 0.3f),
        math.float3(-2.7f, 2.025f, 0.3f),
        math.float3(-2.7f, 2.025f, 0.0f),
        math.float3(-3.0f, 1.8f, 0.0f),
        math.float3(-3.0f, 1.8f, 0.3f),
        math.float3(-2.7f, 1.8f, 0.3f),
        math.float3(-2.7f, 1.8f, 0.0f),
        math.float3(-2.7f, 1.8f, 0.0f),
        math.float3(-2.7f, 1.8f, -0.3f),
        math.float3(-3.0f, 1.8f, -0.3f),
        math.float3(-3.0f, 1.8f, 0.0f),
        math.float3(-2.7f, 1.575f, 0.0f),
        math.float3(-2.7f, 1.575f, -0.3f),
        math.float3(-3.0f, 1.35f, -0.3f),
        math.float3(-3.0f, 1.35f, 0.0f),
        math.float3(-2.5f, 1.125f, 0.0f),
        math.float3(-2.5f, 1.125f, -0.3f),
        math.float3(-2.65f, 0.9375f, -0.3f),
        math.float3(-2.65f, 0.9375f, 0.0f),
        math.float3(-2.0f, 0.9f, 0.0f),
        math.float3(-2.0f, 0.9f, -0.3f),
        math.float3(-1.9f, 0.6f, -0.3f),
        math.float3(-1.9f, 0.6f, 0.0f),
        math.float3(-3.0f, 1.8f, 0.0f),
        math.float3(-3.0f, 1.8f, 0.3f),
        math.float3(-2.7f, 1.8f, 0.3f),
        math.float3(-2.7f, 1.8f, 0.0f),
        math.float3(-3.0f, 1.35f, 0.0f),
        math.float3(-3.0f, 1.35f, 0.3f),
        math.float3(-2.7f, 1.575f, 0.3f),
        math.float3(-2.7f, 1.575f, 0.0f),
        math.float3(-2.65f, 0.9375f, 0.0f),
        math.float3(-2.65f, 0.9375f, 0.3f),
        math.float3(-2.5f, 1.125f, 0.3f),
        math.float3(-2.5f, 1.125f, 0.0f),
        math.float3(-1.9f, 0.6f, 0.0f),
        math.float3(-1.9f, 0.6f, 0.3f),
        math.float3(-2.0f, 0.9f, 0.3f),
        math.float3(-2.0f, 0.9f, 0.0f),
        math.float3(1.7f, 1.425f, 0.0f),
        math.float3(1.7f, 1.425f, -0.66f),
        math.float3(1.7f, 0.6f, -0.66f),
        math.float3(1.7f, 0.6f, 0.0f),
        math.float3(2.6f, 1.425f, 0.0f),
        math.float3(2.6f, 1.425f, -0.66f),
        math.float3(3.1f, 0.825f, -0.66f),
        math.float3(3.1f, 0.825f, 0.0f),
        math.float3(2.3f, 2.1f, 0.0f),
        math.float3(2.3f, 2.1f, -0.25f),
        math.float3(2.4f, 2.025f, -0.25f),
        math.float3(2.4f, 2.025f, 0.0f),
        math.float3(2.7f, 2.4f, 0.0f),
        math.float3(2.7f, 2.4f, -0.25f),
        math.float3(3.3f, 2.4f, -0.25f),
        math.float3(3.3f, 2.4f, 0.0f),
        math.float3(1.7f, 0.6f, 0.0f),
        math.float3(1.7f, 0.6f, 0.66f),
        math.float3(1.7f, 1.425f, 0.66f),
        math.float3(1.7f, 1.425f, 0.0f),
        math.float3(3.1f, 0.825f, 0.0f),
        math.float3(3.1f, 0.825f, 0.66f),
        math.float3(2.6f, 1.425f, 0.66f),
        math.float3(2.6f, 1.425f, 0.0f),
        math.float3(2.4f, 2.025f, 0.0f),
        math.float3(2.4f, 2.025f, 0.25f),
        math.float3(2.3f, 2.1f, 0.25f),
        math.float3(2.3f, 2.1f, 0.0f),
        math.float3(3.3f, 2.4f, 0.0f),
        math.float3(3.3f, 2.4f, 0.25f),
        math.float3(2.7f, 2.4f, 0.25f),
        math.float3(2.7f, 2.4f, 0.0f),
        math.float3(2.7f, 2.4f, 0.0f),
        math.float3(2.7f, 2.4f, -0.25f),
        math.float3(3.3f, 2.4f, -0.25f),
        math.float3(3.3f, 2.4f, 0.0f),
        math.float3(2.8f, 2.475f, 0.0f),
        math.float3(2.8f, 2.475f, -0.25f),
        math.float3(3.525f, 2.49375f, -0.25f),
        math.float3(3.525f, 2.49375f, 0.0f),
        math.float3(2.9f, 2.475f, 0.0f),
        math.float3(2.9f, 2.475f, -0.15f),
        math.float3(3.45f, 2.5125f, -0.15f),
        math.float3(3.45f, 2.5125f, 0.0f),
        math.float3(2.8f, 2.4f, 0.0f),
        math.float3(2.8f, 2.4f, -0.15f),
        math.float3(3.2f, 2.4f, -0.15f),
        math.float3(3.2f, 2.4f, 0.0f),
        math.float3(3.3f, 2.4f, 0.0f),
        math.float3(3.3f, 2.4f, 0.25f),
        math.float3(2.7f, 2.4f, 0.25f),
        math.float3(2.7f, 2.4f, 0.0f),
        math.float3(3.525f, 2.49375f, 0.0f),
        math.float3(3.525f, 2.49375f, 0.25f),
        math.float3(2.8f, 2.475f, 0.25f),
        math.float3(2.8f, 2.475f, 0.0f),
        math.float3(3.45f, 2.5125f, 0.0f),
        math.float3(3.45f, 2.5125f, 0.15f),
        math.float3(2.9f, 2.475f, 0.15f),
        math.float3(2.9f, 2.475f, 0.0f),
        math.float3(3.2f, 2.4f, 0.0f),
        math.float3(3.2f, 2.4f, 0.15f),
        math.float3(2.8f, 2.4f, 0.15f),
        math.float3(2.8f, 2.4f, 0.0f),
        math.float3(0.0f, 3.15f, 0.0f),
        math.float3(0.0f, 3.15f, 0.0f),
        math.float3(0.0f, 3.15f, 0.0f),
        math.float3(0.0f, 3.15f, 0.0f),
        math.float3(0.8f, 3.15f, 0.0f),
        math.float3(0.8f, 3.15f, -0.45f),
        math.float3(0.45f, 3.15f, -0.8f),
        math.float3(0.0f, 3.15f, -0.8f),
        math.float3(0.0f, 2.85f, 0.0f),
        math.float3(0.0f, 2.85f, 0.0f),
        math.float3(0.0f, 2.85f, 0.0f),
        math.float3(0.0f, 2.85f, 0.0f),
        math.float3(0.2f, 2.7f, 0.0f),
        math.float3(0.2f, 2.7f, -0.112f),
        math.float3(0.112f, 2.7f, -0.2f),
        math.float3(0.0f, 2.7f, -0.2f),
        math.float3(0.0f, 3.15f, 0.0f),
        math.float3(0.0f, 3.15f, 0.0f),
        math.float3(0.0f, 3.15f, 0.0f),
        math.float3(0.0f, 3.15f, 0.0f),
        math.float3(0.0f, 3.15f, -0.8f),
        math.float3(-0.45f, 3.15f, -0.8f),
        math.float3(-0.8f, 3.15f, -0.45f),
        math.float3(-0.8f, 3.15f, 0.0f),
        math.float3(0.0f, 2.85f, 0.0f),
        math.float3(0.0f, 2.85f, 0.0f),
        math.float3(0.0f, 2.85f, 0.0f),
        math.float3(0.0f, 2.85f, 0.0f),
        math.float3(0.0f, 2.7f, -0.2f),
        math.float3(-0.112f, 2.7f, -0.2f),
        math.float3(-0.2f, 2.7f, -0.112f),
        math.float3(-0.2f, 2.7f, 0.0f),
        math.float3(0.0f, 3.15f, 0.0f),
        math.float3(0.0f, 3.15f, 0.0f),
        math.float3(0.0f, 3.15f, 0.0f),
        math.float3(0.0f, 3.15f, 0.0f),
        math.float3(-0.8f, 3.15f, 0.0f),
        math.float3(-0.8f, 3.15f, 0.45f),
        math.float3(-0.45f, 3.15f, 0.8f),
        math.float3(0.0f, 3.15f, 0.8f),
        math.float3(0.0f, 2.85f, 0.0f),
        math.float3(0.0f, 2.85f, 0.0f),
        math.float3(0.0f, 2.85f, 0.0f),
        math.float3(0.0f, 2.85f, 0.0f),
        math.float3(-0.2f, 2.7f, 0.0f),
        math.float3(-0.2f, 2.7f, 0.112f),
        math.float3(-0.112f, 2.7f, 0.2f),
        math.float3(0.0f, 2.7f, 0.2f),
        math.float3(0.0f, 3.15f, 0.0f),
        math.float3(0.0f, 3.15f, 0.0f),
        math.float3(0.0f, 3.15f, 0.0f),
        math.float3(0.0f, 3.15f, 0.0f),
        math.float3(0.0f, 3.15f, 0.8f),
        math.float3(0.45f, 3.15f, 0.8f),
        math.float3(0.8f, 3.15f, 0.45f),
        math.float3(0.8f, 3.15f, 0.0f),
        math.float3(0.0f, 2.85f, 0.0f),
        math.float3(0.0f, 2.85f, 0.0f),
        math.float3(0.0f, 2.85f, 0.0f),
        math.float3(0.0f, 2.85f, 0.0f),
        math.float3(0.0f, 2.7f, 0.2f),
        math.float3(0.112f, 2.7f, 0.2f),
        math.float3(0.2f, 2.7f, 0.112f),
        math.float3(0.2f, 2.7f, 0.0f),
        math.float3(0.2f, 2.7f, 0.0f),
        math.float3(0.2f, 2.7f, -0.112f),
        math.float3(0.112f, 2.7f, -0.2f),
        math.float3(0.0f, 2.7f, -0.2f),
        math.float3(0.4f, 2.55f, 0.0f),
        math.float3(0.4f, 2.55f, -0.224f),
        math.float3(0.224f, 2.55f, -0.4f),
        math.float3(0.0f, 2.55f, -0.4f),
        math.float3(1.3f, 2.55f, 0.0f),
        math.float3(1.3f, 2.55f, -0.728f),
        math.float3(0.728f, 2.55f, -1.3f),
        math.float3(0.0f, 2.55f, -1.3f),
        math.float3(1.3f, 2.4f, 0.0f),
        math.float3(1.3f, 2.4f, -0.728f),
        math.float3(0.728f, 2.4f, -1.3f),
        math.float3(0.0f, 2.4f, -1.3f),
        math.float3(0.0f, 2.7f, -0.2f),
        math.float3(-0.112f, 2.7f, -0.2f),
        math.float3(-0.2f, 2.7f, -0.112f),
        math.float3(-0.2f, 2.7f, 0.0f),
        math.float3(0.0f, 2.55f, -0.4f),
        math.float3(-0.224f, 2.55f, -0.4f),
        math.float3(-0.4f, 2.55f, -0.224f),
        math.float3(-0.4f, 2.55f, 0.0f),
        math.float3(0.0f, 2.55f, -1.3f),
        math.float3(-0.728f, 2.55f, -1.3f),
        math.float3(-1.3f, 2.55f, -0.728f),
        math.float3(-1.3f, 2.55f, 0.0f),
        math.float3(0.0f, 2.4f, -1.3f),
        math.float3(-0.728f, 2.4f, -1.3f),
        math.float3(-1.3f, 2.4f, -0.728f),
        math.float3(-1.3f, 2.4f, 0.0f),
        math.float3(-0.2f, 2.7f, 0.0f),
        math.float3(-0.2f, 2.7f, 0.112f),
        math.float3(-0.112f, 2.7f, 0.2f),
        math.float3(0.0f, 2.7f, 0.2f),
        math.float3(-0.4f, 2.55f, 0.0f),
        math.float3(-0.4f, 2.55f, 0.224f),
        math.float3(-0.224f, 2.55f, 0.4f),
        math.float3(0.0f, 2.55f, 0.4f),
        math.float3(-1.3f, 2.55f, 0.0f),
        math.float3(-1.3f, 2.55f, 0.728f),
        math.float3(-0.728f, 2.55f, 1.3f),
        math.float3(0.0f, 2.55f, 1.3f),
        math.float3(-1.3f, 2.4f, 0.0f),
        math.float3(-1.3f, 2.4f, 0.728f),
        math.float3(-0.728f, 2.4f, 1.3f),
        math.float3(0.0f, 2.4f, 1.3f),
        math.float3(0.0f, 2.7f, 0.2f),
        math.float3(0.112f, 2.7f, 0.2f),
        math.float3(0.2f, 2.7f, 0.112f),
        math.float3(0.2f, 2.7f, 0.0f),
        math.float3(0.0f, 2.55f, 0.4f),
        math.float3(0.224f, 2.55f, 0.4f),
        math.float3(0.4f, 2.55f, 0.224f),
        math.float3(0.4f, 2.55f, 0.0f),
        math.float3(0.0f, 2.55f, 1.3f),
        math.float3(0.728f, 2.55f, 1.3f),
        math.float3(1.3f, 2.55f, 0.728f),
        math.float3(1.3f, 2.55f, 0.0f),
        math.float3(0.0f, 2.4f, 1.3f),
        math.float3(0.728f, 2.4f, 1.3f),
        math.float3(1.3f, 2.4f, 0.728f),
        math.float3(1.3f, 2.4f, 0.0f),
        math.float3(0.0f, 0.0f, 0.0f),
        math.float3(0.0f, 0.0f, 0.0f),
        math.float3(0.0f, 0.0f, 0.0f),
        math.float3(0.0f, 0.0f, 0.0f),
        math.float3(1.425f, 0.0f, 0.0f),
        math.float3(1.425f, 0.0f, 0.798f),
        math.float3(0.798f, 0.0f, 1.425f),
        math.float3(0.0f, 0.0f, 1.425f),
        math.float3(1.5f, 0.075f, 0.0f),
        math.float3(1.5f, 0.075f, 0.84f),
        math.float3(0.84f, 0.075f, 1.5f),
        math.float3(0.0f, 0.075f, 1.5f),
        math.float3(1.5f, 0.15f, 0.0f),
        math.float3(1.5f, 0.15f, 0.84f),
        math.float3(0.84f, 0.15f, 1.5f),
        math.float3(0.0f, 0.15f, 1.5f),
        math.float3(0.0f, 0.0f, 0.0f),
        math.float3(0.0f, 0.0f, 0.0f),
        math.float3(0.0f, 0.0f, 0.0f),
        math.float3(0.0f, 0.0f, 0.0f),
        math.float3(0.0f, 0.0f, 1.425f),
        math.float3(-0.798f, 0.0f, 1.425f),
        math.float3(-1.425f, 0.0f, 0.798f),
        math.float3(-1.425f, 0.0f, 0.0f),
        math.float3(0.0f, 0.075f, 1.5f),
        math.float3(-0.84f, 0.075f, 1.5f),
        math.float3(-1.5f, 0.075f, 0.84f),
        math.float3(-1.5f, 0.075f, 0.0f),
        math.float3(0.0f, 0.15f, 1.5f),
        math.float3(-0.84f, 0.15f, 1.5f),
        math.float3(-1.5f, 0.15f, 0.84f),
        math.float3(-1.5f, 0.15f, 0.0f),
        math.float3(0.0f, 0.0f, 0.0f),
        math.float3(0.0f, 0.0f, 0.0f),
        math.float3(0.0f, 0.0f, 0.0f),
        math.float3(0.0f, 0.0f, 0.0f),
        math.float3(-1.425f, 0.0f, 0.0f),
        math.float3(-1.425f, 0.0f, -0.798f),
        math.float3(-0.798f, 0.0f, -1.425f),
        math.float3(0.0f, 0.0f, -1.425f),
        math.float3(-1.5f, 0.075f, 0.0f),
        math.float3(-1.5f, 0.075f, -0.84f),
        math.float3(-0.84f, 0.075f, -1.5f),
        math.float3(0.0f, 0.075f, -1.5f),
        math.float3(-1.5f, 0.15f, 0.0f),
        math.float3(-1.5f, 0.15f, -0.84f),
        math.float3(-0.84f, 0.15f, -1.5f),
        math.float3(0.0f, 0.15f, -1.5f),
        math.float3(0.0f, 0.0f, 0.0f),
        math.float3(0.0f, 0.0f, 0.0f),
        math.float3(0.0f, 0.0f, 0.0f),
        math.float3(0.0f, 0.0f, 0.0f),
        math.float3(0.0f, 0.0f, -1.425f),
        math.float3(0.798f, 0.0f, -1.425f),
        math.float3(1.425f, 0.0f, -0.798f),
        math.float3(1.425f, 0.0f, 0.0f),
        math.float3(0.0f, 0.075f, -1.5f),
        math.float3(0.84f, 0.075f, -1.5f),
        math.float3(1.5f, 0.075f, -0.84f),
        math.float3(1.5f, 0.075f, 0.0f),
        math.float3(0.0f, 0.15f, -1.5f),
        math.float3(0.84f, 0.15f, -1.5f),
        math.float3(1.5f, 0.15f, -0.84f),
        math.float3(1.5f, 0.15f, 0.0f),
    };
}

} // namespace Metamesh
