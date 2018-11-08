using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ModelGridSpawner : MonoBehaviour
{
    public string PakFile;
    public float GridSpacing = 64;
    public int NumPerRow     = 10;

    private Vector3 LastPos      = Vector3.zero;
    private int NumModelsSpawned = 0;

    void Start()
    {
        LastPos = transform.position;

        SpawnModels();
    }

    private List<string> GetModelsInPak()
    {
        var fileList   = TSAssetManager.GetFileListForPak(PakFile);
        var modelPaths = fileList.Where(x => x.Contains("ob")).ToList();

        return modelPaths;
    }

    private void SpawnModels()
    {
        var modelPaths = GetModelsInPak();

        foreach (var modelPath in modelPaths)
        {
            SpawnModel(LastPos, $"{PakFile}/{modelPath}");

            if (NumModelsSpawned % NumPerRow == 0)
            {
                LastPos.x  = transform.position.x;
                LastPos.z += GridSpacing;
            }
            else
            {
                LastPos.x += GridSpacing;
            }

            NumModelsSpawned++;
        }
    }

    private void SpawnModel(Vector3 Positon, string ModelPath)
    {
        var gameObject = new GameObject(ModelPath);
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        var ts2AnimatedModel = gameObject.AddComponent<TSAnimatedModel>();

        ts2AnimatedModel.ModelPath    = ModelPath;
        gameObject.transform.position = Positon;

        //ts2AnimatedModel.LoadTs2Model(ModelPath);
    }
}
