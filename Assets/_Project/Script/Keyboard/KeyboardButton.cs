using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[DisallowMultipleComponent]
public class KeyboardButton : MonoBehaviour
{
    [SerializeField] private bool flipYInsideCell = true;

    [Header("Target")]
    [SerializeField] private Renderer targetRenderer;

    [Header("Shader property names")]
    [SerializeField] private string skinColorProp = "_SkinColor";
    [SerializeField] private string uvScaleProp = "_UVScale";
    [SerializeField] private string uvOffsetProp = "_UVOffset";

    [Header("Palette (до 10 цветов)")]
    [SerializeField] public Color[] palette = new Color[10];

    [Header("Atlas grid")]
    [Tooltip("Количество столбцов (X) и строк (Y) ячеек в атласе. Должно совпадать с реальной сеткой текстуры атласа.")]
    [SerializeField] private Vector2Int grid = new Vector2Int(8, 8);
    [Tooltip("От какого края атласа считается индекс строк: TopLeft — сверху, BottomLeft — снизу.")]
    [SerializeField] private Origin origin = Origin.TopLeft;
    [Tooltip("Уменьшение области выборки внутри ячейки (в долях ячейки), чтобы символ был по центру кнопки и не \"подтекал\" краями соседних ячеек. 0 = без полей, 0.1 = 10% внутрь.")]
    [SerializeField, Range(0f, 0.25f)] private float cellPadding = 0.05f;

    [Header("Current selection")]
    [SerializeField, Range(0, 9)] private int colorIndex = 0;
    [SerializeField] private int glyphIndex = 0;

    [SerializeField] private bool _notChangeView;

    private MaterialPropertyBlock _mpb;
    private int _skinColorId, _uvScaleId, _uvOffsetId;

    public enum Origin { BottomLeft, TopLeft }

    private void Reset()
    {
        targetRenderer = GetComponent<Renderer>();
    }

    private void Start()
    {
        Init();
        Apply();
    }

    private void Init()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        if (_mpb == null)
            _mpb = new MaterialPropertyBlock();

        _skinColorId = Shader.PropertyToID(skinColorProp);
        _uvScaleId = Shader.PropertyToID(uvScaleProp);
        _uvOffsetId = Shader.PropertyToID(uvOffsetProp);
    }

    public void Apply()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        if (targetRenderer == null)
            return;

        if (_mpb == null)
            _mpb = new MaterialPropertyBlock();

        targetRenderer.GetPropertyBlock(_mpb);

        Color c = (palette != null && palette.Length > 0)
            ? palette[Mathf.Clamp(colorIndex, 0, palette.Length - 1)]
            : Color.white;

        _mpb.SetColor(_skinColorId, c);

        int cols = Mathf.Max(1, grid.x);
        int rows = Mathf.Max(1, grid.y);

        float sx = 1f / cols;
        float sy = 1f / rows;

        int clampedGlyph = Mathf.FloorToInt(Mathf.Repeat(glyphIndex, cols * rows));
        int col = clampedGlyph % cols;
        int row = clampedGlyph / cols;

        // Центрированный выбор ячейки атласа (col, row) с небольшими полями,
        // чтобы символ был точно по центру кнопки и не "подтекал" краями соседних ячеек.
        // Шейдер сам делает: uv_final = uv * UVScale + UVOffset (UV меша 0..1 на весь атлас).
        float padX = sx * cellPadding;
        float padY = sy * cellPadding;

        // X: суб-прямоугольник внутри ячейки по X.
        float scaleX = sx - 2f * padX;                     // ширина области выборки
        float offsetX = col * sx + padX;                   // левый край области выборки

        // Y: верх (vTop) и низ (vBottom) ячейки в координатах V (0 = низ, 1 = верх).
        float vTop, vBottom;
        if (origin == Origin.TopLeft)
        {
            vTop    = 1f - row * sy;
            vBottom = 1f - (row + 1) * sy;
        }
        else // BottomLeft
        {
            vBottom = row * sy;
            vTop    = (row + 1) * sy;
        }

        // Учитываем поля внутрь ячейки.
        vTop    -= padY;
        vBottom += padY;

        float spanY = vTop - vBottom; // > 0 всегда

        float scaleY, offsetY;
        if (flipYInsideCell)
        {
            // Переворачиваем внутри ячейки: uv.y=0 показывает верх, uv.y=1 — низ.
            scaleY  = -spanY;
            offsetY = vTop;
        }
        else
        {
            scaleY  = spanY;
            offsetY = vBottom;
        }

        Vector2 scale = new Vector2(scaleX, scaleY);
        Vector2 offset = new Vector2(offsetX, offsetY);

        _mpb.SetVector(_uvScaleId, scale);
        _mpb.SetVector(_uvOffsetId, offset);

        targetRenderer.SetPropertyBlock(_mpb);
    }

    public void ApplyEditor()
    {
        Apply();

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        if (targetRenderer != null)
            EditorUtility.SetDirty(targetRenderer);
#endif
    }

    public void SetRandomAll()
    {
        SetRandomSymbolAndColor();
        ApplyEditor();
    }

    public void SetColorIndex(int idx)
    {
        colorIndex = Mathf.Clamp(idx, 0, Mathf.Max(0, palette.Length - 1));
        Apply();
    }

    public void SetGlyphIndex(int idx)
    {
        glyphIndex = idx;
        Apply();
    }

    public void NextGlyph()
    {
        glyphIndex = (glyphIndex + 1) % Mathf.Max(1, grid.x * grid.y);
        Apply();
    }

    public void PrevGlyph()
    {
        int total = Mathf.Max(1, grid.x * grid.y);
        glyphIndex = (glyphIndex - 1 + total) % total;
        Apply();
    }

    public void SetSymbolAndColor(int letterIndex, int colorIdx)
    {
        glyphIndex = Mathf.Clamp(letterIndex, 0, 25);
        colorIndex = Mathf.Clamp(colorIdx, 0, Mathf.Max(0, palette.Length - 1));
        Apply();
    }

    public void SetRandomColor()
    {
        if (palette == null || palette.Length == 0) return;
        colorIndex = Random.Range(0, palette.Length);
        Apply();
    }

    public void SetRandomSymbol()
    {
        glyphIndex = Random.Range(0, 26);
        Apply();
    }

    public void SetRandomSymbolAndColor()
    {
        SetRandomSymbol();
        SetRandomColor();
    }

    public void ResetUV()
    {
        if (targetRenderer == null) return;
        if (_mpb == null) _mpb = new MaterialPropertyBlock();

        targetRenderer.GetPropertyBlock(_mpb);
        _mpb.SetVector(_uvScaleId, Vector2.one);
        _mpb.SetVector(_uvOffsetId, Vector2.zero);
        targetRenderer.SetPropertyBlock(_mpb);
    }
}