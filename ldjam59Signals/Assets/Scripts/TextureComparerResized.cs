using UnityEngine;

public class TextureComparerResized
{
    public static bool[,] Compare(Texture2D texA, Texture2D texB, bool[,] result, float threshold = 0.6f)
    {
        int width = texA.width;
        int height = texA.height;
        var targetWidth = result.GetLength(0);
        var targetHeight = result.GetLength(1);

        float blockWidth = (float)width / targetWidth;
        float blockHeight = (float)height / targetHeight;

        for (int x = 0; x < targetWidth; x++)
        {
            for (int y = 0; y < targetHeight; y++)
            {
                int startX = Mathf.FloorToInt(x * blockWidth);
                int endX = Mathf.FloorToInt((x + 1) * blockWidth);

                int startY = Mathf.FloorToInt(y * blockHeight);
                int endY = Mathf.FloorToInt((y + 1) * blockHeight);

                int total = 0;
                int matched = 0;

                for (int px = startX; px < endX; px++)
                {
                    for (int py = startY; py < endY; py++)
                    {
                        Color a = texA.GetPixel(px, py);
                        Color b = texB.GetPixel(px, py);

                        bool aWhite = a.grayscale > 0.5f;
                        bool bWhite = b.grayscale > 0.5f;

                        if (aWhite == bWhite)
                            matched++;

                        total++;
                    }
                }

                float percent = (float)matched / total;
                result[x, y] = percent >= threshold;
            }
        }

        return result;
    }
}