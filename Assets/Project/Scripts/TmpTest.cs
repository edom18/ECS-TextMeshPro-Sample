using TMPro;
using UnityEngine;
using UnityEngine.TextCore;

public class TmpTest : MonoBehaviour
{
    [SerializeField] private TMP_Text _tmpText;
    [SerializeField] private Material _material;
    [SerializeField] private float _fontSize = 24f;

    private int _index = -1;
    private GameObject _meshObj;
    private Mesh _mesh;

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
        MeshFilter meshFilter = _meshObj.AddComponent<MeshFilter>();

        _mesh = new Mesh();
        _mesh.MarkDynamic();
        meshFilter.mesh = _mesh;
    }

    private void ChangeCharacter()
    {
        _index = (_index + 1) % _tmpText.text.Length;
        ShowCharacterAt(_index);
    }

    private void ShowCharacterAt(int index)
    {
        TMP_FontAsset fontAsset = _tmpText.font;
        char character = _tmpText.text[index];
        if (!fontAsset.characterLookupTable.TryGetValue(character, out TMP_Character tmpCharacter))
        {
            return;
        }

        Glyph glyph = tmpCharacter.glyph;

        float glyphWidth = glyph.metrics.width / fontAsset.faceInfo.pointSize * _fontSize;
        float glyphHeight = glyph.metrics.height / fontAsset.faceInfo.pointSize * _fontSize;

        float x0 = glyph.metrics.horizontalBearingX / fontAsset.faceInfo.pointSize * _fontSize;
        float x1 = x0 + glyphWidth;
        float y0 = -glyph.metrics.horizontalBearingY / fontAsset.faceInfo.pointSize * _fontSize;
        float y1 = y0 - glyphHeight;

        Vector3[] vertices = new[]
        {
            new Vector3(x0, y0, 0),
            new Vector3(x0, y1, 0),
            new Vector3(x1, y1, 0),
            new Vector3(x1, y0, 0),
        };

        Vector2 uv0 = new Vector2(glyph.glyphRect.x / (float)fontAsset.atlasWidth, glyph.glyphRect.y / (float)fontAsset.atlasHeight);
        Vector2 uv1 = new Vector2(uv0.x, (glyph.glyphRect.y + glyph.glyphRect.height) / (float)fontAsset.atlasHeight);
        Vector2 uv2 = new Vector2((glyph.glyphRect.x + glyph.glyphRect.width) / (float)fontAsset.atlasWidth, uv1.y);
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

        _mesh.vertices = vertices;
        _mesh.uv = uv;
        _mesh.triangles = triangles;
    }
}