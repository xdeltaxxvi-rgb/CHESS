using UnityEngine;
using UnityEditor;
using System.IO;
using Chess.Core;
using Chess.Core.Board;
using Chess.Input;

namespace Chess.Editor
{
    /// <summary>
    /// One-click scene setup. Run via: Chess → Setup Game Scene.
    /// Safe to re-run — skips anything that already exists.
    /// </summary>
    public static class SceneSetup
    {
        [MenuItem("Chess/Setup Game Scene")]
        public static void SetupScene()
        {
            // ── Materials ────────────────────────────────────────────────
            EnsureFolder("Assets/Materials");
            Material lightMat = GetOrCreateMaterial(
                "Assets/Materials/TileLight.mat",
                new Color(0.941f, 0.851f, 0.710f));   // #F0D9B5
            Material darkMat = GetOrCreateMaterial(
                "Assets/Materials/TileDark.mat",
                new Color(0.710f, 0.533f, 0.388f));   // #B58863

            // ── GameObjects + components ─────────────────────────────────
            var boardGO  = GetOrCreate("Board");
            var visGO    = GetOrCreate("BoardVisualizer");
            var inputGO  = GetOrCreate("Input");
            var gmGO     = GetOrCreate("GameManager");

            var boardMgr = AddOrGet<BoardManager>(boardGO);
            var boardVis = AddOrGet<BoardVisualizer>(visGO);
            var tileSel  = AddOrGet<TileSelector>(inputGO);
            var moveSel  = AddOrGet<MoveSelector>(inputGO);
            var gameMgr  = AddOrGet<GameManager>(gmGO);

            // ── Wire BoardVisualizer ─────────────────────────────────────
            var soVis = new SerializedObject(boardVis);
            soVis.FindProperty("_boardManager").objectReferenceValue = boardMgr;
            soVis.FindProperty("_lightMaterial").objectReferenceValue = lightMat;
            soVis.FindProperty("_darkMaterial").objectReferenceValue = darkMat;
            soVis.ApplyModifiedProperties();

            // ── Wire TileSelector ────────────────────────────────────────
            var soTile = new SerializedObject(tileSel);
            soTile.FindProperty("_camera").objectReferenceValue = Camera.main;
            soTile.ApplyModifiedProperties();

            // ── Wire MoveSelector ────────────────────────────────────────
            var soMove = new SerializedObject(moveSel);
            soMove.FindProperty("_boardManager").objectReferenceValue = boardMgr;
            soMove.FindProperty("_boardVisualizer").objectReferenceValue = boardVis;
            soMove.FindProperty("_tileSelector").objectReferenceValue = tileSel;
            soMove.ApplyModifiedProperties();

            // ── Wire GameManager ─────────────────────────────────────────
            var soGM = new SerializedObject(gameMgr);
            soGM.FindProperty("_moveSelector").objectReferenceValue = moveSel;
            soGM.FindProperty("_boardManager").objectReferenceValue = boardMgr;
            soGM.FindProperty("_boardVisualizer").objectReferenceValue = boardVis;
            soGM.ApplyModifiedProperties();

            // ── Camera ───────────────────────────────────────────────────
            if (Camera.main != null)
            {
                Camera cam = Camera.main;
                cam.orthographic = true;
                cam.orthographicSize = 6f;
                // (-22, 18, -22) with (30°, 45°, 0°) looks exactly at world origin (0,0,0).
                cam.transform.position = new Vector3(-22f, 18f, -22f);
                cam.transform.rotation = Quaternion.Euler(30f, 45f, 0f);
                EditorUtility.SetDirty(cam);
            }

            // ── Save ─────────────────────────────────────────────────────
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

            Debug.Log("[SceneSetup] Done — press Play to start the game.");
        }

        // ── Helpers ──────────────────────────────────────────────────────

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = Path.GetDirectoryName(path).Replace('\\', '/');
                string folder = Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, folder);
            }
        }

        private static Material GetOrCreateMaterial(string assetPath, Color color)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                             ?? Shader.Find("Standard");
                mat = new Material(shader);
                mat.color = color;
                AssetDatabase.CreateAsset(mat, assetPath);
            }
            return mat;
        }

        private static GameObject GetOrCreate(string name)
        {
            GameObject go = GameObject.Find(name);
            if (go == null) go = new GameObject(name);
            return go;
        }

        private static T AddOrGet<T>(GameObject go) where T : Component =>
            go.GetComponent<T>() ?? go.AddComponent<T>();
    }
}
