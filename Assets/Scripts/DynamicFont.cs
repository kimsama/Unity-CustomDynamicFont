using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DynamicFont : MonoBehaviour 
{
    public Font dynamicFont;
    public Material dynamicFontMaterial;
    public int dynamicFontSize = 12;
    public FontStyle dynamicFontStyle;

    public string text;

    public bool UseDynamicFont { get { return (dynamicFont != null); } }

    int mSpacingX = 0;
    int mSpacingY = 0;

    List<Color> mColors = new List<Color>();

    List<Vector3> verts = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<Color32> cols = new List<Color32>();


    MeshFilter meshFilter = null;
    Mesh mesh = null;

    void Awake()
    {
        MeshRenderer render = GetComponent<MeshRenderer>();
        render.sharedMaterial.mainTexture = dynamicFont.material.mainTexture;

        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter.mesh == null)
            meshFilter.mesh = new Mesh();

        this.mesh = meshFilter.mesh;
        this.mesh.MarkDynamic();
    }

	// Use this for initialization
	void Start () 
    {
        Print(text, new Color(1f,1f, 1f, 1f), verts, uvs, cols);

        UpdateMesh();

	}

    void Print(string text, Color32 color, List<Vector3> verts, List<Vector2> uvs, List<Color32> cols)
    {
        if (text != null)
        {
            if (!UseDynamicFont)
                return;

            //RgisterFont(this);
            if (UseDynamicFont)
                dynamicFont.RequestCharactersInTexture(text, dynamicFontSize, dynamicFontStyle);

            mColors.Clear();
            mColors.Add(color);

            Vector2 scale = dynamicFontSize > 0 ? new Vector2(1f / dynamicFontSize, 1f / dynamicFontSize) : Vector2.one;

			int indexOffset = verts.Count;
			int maxX = 0;
			int x = 0;
			int y = 0;
			int prev = 0;
            int lineHeight = (dynamicFontSize + mSpacingY);
			Vector3 v0 = Vector3.zero, v1 = Vector3.zero;
			Vector2 u0 = Vector2.zero, u1 = Vector2.zero;
			//float invX = uvRect.width / mFont.texWidth;
			//float invY = mUVRect.height / mFont.texHeight;
			int textLength = text.Length;

            for (int i = 0; i < textLength; ++i)
            {
                char c = text[i];

                if (c == '\n')
                {
                    if (x > maxX) maxX = x;

                    x = 0;
                    y += lineHeight;
                    prev = 0;
                    continue;
                }

                if (c < ' ')
                {
                    prev = 0;
                    continue;
                }

                //v0 v1 are the two corners
                CharacterInfo charInfo;
                if (!dynamicFont.GetCharacterInfo(c, out charInfo, dynamicFontSize, dynamicFontStyle))
                {
                    Debug.LogError("character not found in font");
                    continue;
                }

                v0.x = scale.x * (x + charInfo.vert.xMin);
                v0.y = scale.x * (-y + charInfo.vert.yMax);
                v1.x = scale.y * (x + charInfo.vert.xMax);
                v1.y = scale.y * (-y + charInfo.vert.yMin);

                u0.x = charInfo.uv.xMin;
                u0.y = charInfo.uv.yMin;
                u1.x = charInfo.uv.xMax;
                u1.y = charInfo.uv.yMax;

                x += mSpacingX + (int)charInfo.width;

                for (int b = 0; b < 4; ++b) cols.Add(color);

                if (charInfo.flipped)
                {
                    //swap entries
                    uvs.Add(new Vector2(u0.x, u1.y));
                    uvs.Add(new Vector2(u0.x, u0.y));
                    uvs.Add(new Vector2(u1.x, u0.y));
                    uvs.Add(new Vector2(u1.x, u1.y));
                }
                else
                {
                    uvs.Add(new Vector2(u1.x, u0.y));
                    uvs.Add(new Vector2(u0.x, u0.y));
                    uvs.Add(new Vector2(u0.x, u1.y));
                    uvs.Add(new Vector2(u1.x, u1.y));
                }

                verts.Add(new Vector3(v1.x, v0.y));
                verts.Add(new Vector3(v0.x, v0.y));
                verts.Add(new Vector3(v0.x, v1.y));
                verts.Add(new Vector3(v1.x, v1.y));

            }// end of for each char in text
        }
    }

    void UpdateMesh()
    {
        this.mesh.vertices = verts.ToArray();
        this.mesh.uv = uvs.ToArray();

        List<int> faces = new List<int>();
        for (int vertIndex = 0; vertIndex < this.verts.Count; vertIndex += 4)
        {
            faces.Add(vertIndex + 0);
            faces.Add(vertIndex + 1);
            faces.Add(vertIndex + 2);

            faces.Add(vertIndex + 2);
            faces.Add(vertIndex + 3);
            faces.Add(vertIndex + 0);
        }
        this.mesh.triangles = faces.ToArray();
    }

}
