using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour {

    public const float maxViewDistance = 450;
    public Transform viewer;
    public Material mapMaterial;

    public static Vector2 viewerPosition;
    static MapGenerator mapGenerator;

    private int chunkSize;
    private int chunksVisibleInViewDistance;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    void Start() {
        mapGenerator = FindObjectOfType<MapGenerator>();
        chunkSize = MapGenerator.mapChunkSize -1;
        chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);
    }

    void Update() {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks();
    }

    void UpdateVisibleChunks() {

        for (int i=0; i < terrainChunksVisibleLastUpdate.Count; i++ ) {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        int currentChunkCoordinateX = Mathf.RoundToInt(viewerPosition.x/chunkSize);
        int currentChunkCoordinateY = Mathf.RoundToInt(viewerPosition.y/chunkSize);

        for (int yOffSet = -chunksVisibleInViewDistance; yOffSet <= chunksVisibleInViewDistance; yOffSet++) {
            for (int xOffSet = -chunksVisibleInViewDistance; xOffSet <= chunksVisibleInViewDistance; xOffSet++) { 

                Vector2 viewedChunksCoordinate = new Vector2(currentChunkCoordinateX + xOffSet, currentChunkCoordinateY + yOffSet);

                if (terrainChunkDictionary.ContainsKey(viewedChunksCoordinate)) {
                    terrainChunkDictionary[viewedChunksCoordinate].UpdateTerrainChunk();
                    if (terrainChunkDictionary[viewedChunksCoordinate].IsVisible()) {
                        terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunksCoordinate]);
                    }
                } else {
                    terrainChunkDictionary.Add(viewedChunksCoordinate, new TerrainChunk(viewedChunksCoordinate, chunkSize, transform, mapMaterial));
                } 
            }
        }
    }


    public class TerrainChunk {

        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MapData mapData;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;

        public TerrainChunk(Vector2 coord, int size, Transform parent, Material material) {
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer.material = material;
            meshObject.transform.position = positionV3;
            meshObject.transform.parent = parent;
            SetVisible(false);

            mapGenerator.RequestMapData(OnMapDataReceived);
        }

        void OnMapDataReceived(MapData mapData) {
            mapGenerator.RequestMeshData(mapData, OnMeshDataReceived);
        }

        void OnMeshDataReceived(MeshData meshData) {
            meshFilter.mesh = meshData.CreateMesh();
        }

        public void UpdateTerrainChunk() {
            float viewerDisanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = viewerDisanceFromNearestEdge <= maxViewDistance;
            SetVisible(visible);
        }

        public void SetVisible(bool visible) {
            meshObject.SetActive(visible);
        }

        public bool IsVisible() {
            return meshObject.activeSelf;
        }
    }
}
