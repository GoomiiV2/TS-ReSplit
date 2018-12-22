using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

// Used to store baked data that should be applyied to a scene after it has been loaded and genrated
// Lisghtmaps for example

[System.Serializable]
public class LevelBakedData
{
    public List<SectionBakedData> PerSectionData;
}

[System.Serializable]
public class SectionBakedData
{
    public LightmapBakedData Lightmap;

    public void Apply(GameObject GameObj)
    {
        var meshRender = GameObj.GetComponent<MeshRenderer>();

        Lightmap.Apply(meshRender);
    }

    public static SectionBakedData CreateFromGameObject(GameObject GameObj)
    {
        var meshRender = GameObj.GetComponent<MeshRenderer>();

        var data = new SectionBakedData()
        {
            Lightmap = LightmapBakedData.CreateFrom(meshRender)
        };

        return data;
    }
}

[System.Serializable]
public struct LightmapBakedData
{
    public int LightmapIndex;
    public int RealtimeLightmapIndex;
    public Vector4 LightmapScaleOffset;
    public Vector4 RealtimeLightmapScaleOffset;
    public LightProbeUsage LightProbeUsage;

    public void Apply(MeshRenderer MeshRender)
    {
        var meshRender = MeshRender.GetComponentInParent<MeshFilter>();

        MeshRender.lightmapIndex               = LightmapIndex;
        MeshRender.realtimeLightmapIndex = -1; //RealtimeLightmapIndex; // Unity seems to always say this is one for some reason and that breaks stuff so i'm disabling it, for now
        MeshRender.lightmapScaleOffset         = LightmapScaleOffset;
        MeshRender.realtimeLightmapScaleOffset = RealtimeLightmapScaleOffset;
        MeshRender.lightProbeUsage             = LightProbeUsage;
    }

    public static LightmapBakedData CreateFrom(MeshRenderer MeshRender)
    {
        var meshFilter = MeshRender.GetComponentInParent<MeshFilter>();

        var data = new LightmapBakedData()
        {
            LightmapIndex               = MeshRender.lightmapIndex,
            RealtimeLightmapIndex       = MeshRender.realtimeLightmapIndex,
            LightmapScaleOffset         = MeshRender.lightmapScaleOffset,
            RealtimeLightmapScaleOffset = MeshRender.realtimeLightmapScaleOffset,
            LightProbeUsage             = MeshRender.lightProbeUsage
        };

        return data;
    }
}
