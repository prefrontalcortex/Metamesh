using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;

namespace MetaMesh
{
    [ScriptedImporter(1, "metamesh")]
    public sealed class MetaMeshImporter : ScriptedImporter
    {
        #region ScriptedImporter implementation

        [SerializeField] Shape _shape = Shape.Box;
        [SerializeField] Plane _plane = null;
        [SerializeField] Box _box = null;

        public override void OnImportAsset(AssetImportContext context)
        {
            var gameObject = new GameObject();
            var mesh = ImportAsMesh(context.assetPath);

            var meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;

            var meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial =
                AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat");

            context.AddObjectToAsset("prefab", gameObject);
            if (mesh != null) context.AddObjectToAsset("mesh", mesh);

            context.SetMainObject(gameObject);
        }

        #endregion

        #region Reader implementation

        Mesh ImportAsMesh(string path)
        {
            var mesh = new Mesh();
            mesh.name = "Mesh";

            switch (_shape)
            {
                case Shape.Plane: _plane.Generate(mesh); break;
                case Shape.Box  : _box  .Generate(mesh); break;
            }

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.UploadMeshData(true);

            return mesh;
        }

        #endregion
    }
}
