using System.Text;
using UnityEngine;

public class FPSDisplayer : MonoBehaviour
{
    [SerializeField] private int sampleSize = 60; // Number of frames to average
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private int fontSize = 24;
    [SerializeField] private Vector2 position = new Vector2(10, 10);

    private float[] frameTimes;
    private int frameIndex;
    private GUIStyle style;
    private readonly StringBuilder sb = new StringBuilder();

    private void Awake()
    {
        frameTimes = new float[sampleSize];
        style = new GUIStyle
        {
            fontSize = fontSize,
            normal = { textColor = textColor }
        };
    }

    private void Update()
    {
        frameTimes[frameIndex] = Time.unscaledDeltaTime;
        frameIndex = (frameIndex + 1) % sampleSize;
    }

    private void OnGUI()
    {
        float avg = 0f;
        for (int i = 0; i < sampleSize; i++)
            avg += frameTimes[i];
        avg /= sampleSize;

        float fps = avg > 0 ? 1f / avg : 0f;

        sb.Clear();
        sb.Append("FPS: ");
        sb.Append(fps.ToString("F1"));
        sb.Append("  (avg of ");
        sb.Append(sampleSize);
        sb.Append(" frames)");

        GUI.Label(new Rect(position.x, position.y, 300, 50), sb.ToString(), style);
    }
}
