using UnityEngine;

[ExecuteAlways]                 // работает и в редакторе, и в Play
public class FaceAtlasSelector : MonoBehaviour
{
    [Header("Atlas grid")]
    public int cols = 8;
    public int rows = 8;

    [Header("Target")]
    public Renderer targetRenderer;      // SkinnedMeshRenderer / MeshRenderer
    [Range(0, 63)]
    public int faceIndex = 0;

    // Built-in: "_MainTex_ST", URP: "_BaseMap_ST"
    public string stPropertyName = "_MainTex_ST";

    MaterialPropertyBlock _mpb;

    void OnEnable()
    {
        if (!targetRenderer) targetRenderer = GetComponent<Renderer>();
        if (_mpb == null) _mpb = new MaterialPropertyBlock();
        Apply();
    }

    void OnValidate()
    {                   // вызывается при изменениях в инспекторе
        if (_mpb == null) _mpb = new MaterialPropertyBlock();
        Apply();
    }

    public void SetFace(int newIndex)
    {   // вызов из кода при необходимости
        faceIndex = Mathf.Clamp(newIndex, 0, cols * rows - 1);
        Apply();
    }

    void Apply()
    {
        if (!targetRenderer || cols <= 0 || rows <= 0) return;

        Vector2 tiling = new Vector2(1f / cols, 1f / rows);
        int c = faceIndex % cols;
        int r = faceIndex / cols;

        // если тайлы считаются «сверху-вниз»
        Vector2 offset = new Vector2(c * tiling.x, 1f - (r + 1) * tiling.y);

        const float eps = 0.001f;                 // защита от bleed
        tiling -= new Vector2(eps, eps);
        offset += new Vector2(eps * 0.5f, eps * 0.5f);

        targetRenderer.GetPropertyBlock(_mpb);
        _mpb.SetVector(stPropertyName, new Vector4(tiling.x, tiling.y, offset.x, offset.y));
        targetRenderer.SetPropertyBlock(_mpb);
    }
}
