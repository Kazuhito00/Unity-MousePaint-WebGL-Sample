using UnityEngine;
using System.Collections;
using System.IO;

public class MousePaint : MonoBehaviour
{
    Color[] drawBuffer;
    Texture2D drawTexture;
    
    Vector2 prevPoint;

    bool drawStartPointFlag = true;

    void Start()
    {
        // 描画データ保持用のバッファにテクスチャーをコピー
        Texture2D mainTexture = (Texture2D)GetComponent<Renderer>().material.mainTexture;
        Color[] pixels = mainTexture.GetPixels();
        drawBuffer = new Color[pixels.Length];
        pixels.CopyTo(drawBuffer, 0);

        // 描画用テクスチャ準備
        drawTexture = new Texture2D(mainTexture.width, mainTexture.height, TextureFormat.RGBA32, false);
        drawTexture.filterMode = FilterMode.Point;
    }

    void Update()
    {
        // 線の描画色
        int color_r = 243;
        int color_g = 134;
        int color_b = 48;
        Color color = new Color((float)(color_r/255f), (float)(color_g/255f), (float)(color_b/255f));

        // マウス左クリック時にオブジェクトの座標情報を取得
        bool raycastResult = false;
        var drawPoint = new Vector2(0, 0);
        if (Input.GetMouseButton(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            raycastResult = Physics.Raycast(ray, out hit, 100.0f);
            if (raycastResult) {
                drawPoint = new Vector2(hit.textureCoord.x * drawTexture.width, hit.textureCoord.y * drawTexture.height);
            }
        }
        // マウス軌跡を描画
        if (raycastResult) {
            // 開始点
            if (drawStartPointFlag) {
                DrawPoint(drawPoint, color);
            }
            // ドラッグ状態
            else {
                DrawLine(prevPoint, drawPoint, color);
            }

            drawStartPointFlag = false;
            prevPoint = drawPoint;
        } else {
            drawStartPointFlag = true;
        }
        // 描画バッファをテクスチャへ反映
        drawTexture.SetPixels(drawBuffer);
        drawTexture.Apply();
        GetComponent<Renderer>().material.mainTexture = drawTexture;
    }

    public void DrawPoint(Vector2 point, Color color, int brushSize = 3)
    {
        point.x = (int)point.x;
        point.y = (int)point.y;

        int start_x = Mathf.Max(0, (int)(point.x - (brushSize - 1)));
        int end_x = Mathf.Min(drawTexture.width, (int)(point.x + (brushSize + 1)));
        int start_y =  Mathf.Max(0, (int)(point.y - (brushSize - 1)));
        int end_y = Mathf.Min(drawTexture.height, (int)(point.y + (brushSize + 1)));

        for (int x = start_x; x < end_x; x++) {
            for (int y = start_y; y < end_y; y++) {
                double length = Mathf.Sqrt(Mathf.Pow(point.x - x, 2) + Mathf.Pow(point.y - y, 2));
                if (length < brushSize) {
                    drawBuffer.SetValue(color, x + drawTexture.width * y);
                }
            }
        }
    }

    public void DrawLine(Vector2 point1, Vector2 point2, Color color, int lerpNum = 100)
    {
        for(int i=0; i < lerpNum + 1; i++) {
            var point = Vector2.Lerp(point1, point2, i * (1.0f / lerpNum));
            DrawPoint(point, color);
        }
    }
}


