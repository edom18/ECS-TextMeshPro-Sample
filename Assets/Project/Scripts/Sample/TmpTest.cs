using TMPro;
using UnityEngine;
using UnityEngine.TextCore;

public class TmpTest : MonoBehaviour
{
    [SerializeField] private TMP_Text _targetText;
    [SerializeField] private Material _material;
    [SerializeField] private float _unitPerPixel = 100f;
    [SerializeField] private float _fontSize = 24f;

    private float FontSizeToUnit => _fontSize / _unitPerPixel;
    
    private int _index = -1;
    private GameObject _meshObj;
    private MeshFilter _meshFilter;

    private void Start()
    {
        CreateMeshObject();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ChangeCharacter();
        }
    }

    private void CreateMeshObject()
    {
        _meshObj = new GameObject("Mesh Object");
        MeshRenderer meshRenderer = _meshObj.AddComponent<MeshRenderer>();
        meshRenderer.material = _material;
        _meshFilter = _meshObj.AddComponent<MeshFilter>();
    }

    private void ChangeCharacter()
    {
        _index = (_index + 1) % _targetText.text.Length;
        Mesh mesh = CreateMeshAt(_index);
        mesh.MarkDynamic();
        _meshFilter.mesh = mesh;
    }

    private Mesh CreateMeshAt(int index)
    {
        TMP_FontAsset fontAsset = _targetText.font;
        char character = _targetText.text[index];
        if (!fontAsset.characterLookupTable.TryGetValue(character, out TMP_Character tmpCharacter))
        {
            throw new System.Exception("Character not found in font asset");
        }

        // グリフ情報を取得

        Glyph glyph = tmpCharacter.glyph;

        // グリフの幅と高さを計算

        float toUnit = 1f / fontAsset.faceInfo.pointSize * FontSizeToUnit;

        float glyphWidth = glyph.metrics.width * toUnit;
        float glyphHeight = glyph.metrics.height * toUnit;

        float x0 = -glyphWidth * 0.5f;
        float x1 = glyphWidth * 0.5f;
        float y0 = -glyphHeight * 0.5f;
        float y1 = glyphHeight * 0.5f;

        Vector3[] vertices = new[]
        {
            new Vector3(x0, y0, 0),
            new Vector3(x0, y1, 0),
            new Vector3(x1, y1, 0),
            new Vector3(x1, y0, 0),
        };

        // グリフのUVを計算

        float rectWidth = glyph.glyphRect.width;
        float rectHeight = glyph.glyphRect.height;
        float atlasWidth = fontAsset.atlasWidth;
        float atlasHeight = fontAsset.atlasHeight;
        float rx = glyph.glyphRect.x;
        float ry = glyph.glyphRect.y;

        Vector2 uv0 = new Vector2(rx / atlasWidth, ry / atlasHeight);
        Vector2 uv1 = new Vector2(uv0.x, (ry + rectHeight) / atlasHeight);
        Vector2 uv2 = new Vector2((rx + rectWidth) / atlasWidth, uv1.y);
        Vector2 uv3 = new Vector2(uv2.x, uv0.y);

        Vector2[] uv = new[]
        {
            uv0, uv1, uv2, uv3,
        };

        int[] triangles = new[]
        {
            0, 1, 2,
            0, 2, 3,
        };

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        return mesh;
    }
}