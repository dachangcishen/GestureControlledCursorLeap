using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class VelocityDataExporter : MonoBehaviour
{
    [Header("Data Collection")]
    [SerializeField] private float sampleInterval = 0.1f; // 采样间隔（秒）
    private float lastSampleTime;

    private Vector3 lastPosition; // 上一次的位置
    private List<Vector2> velocityData = new List<Vector2>(); // 数据存储结构（时间戳，速度值）

    [Header("Export Settings")]
    [SerializeField] private string fileName;

    void Start()
    {
        if(File.Exists(Path.Combine(Application.dataPath,"SubjectName.txt")))
        {
            string[] lines = File.ReadAllLines(Path.Combine(Application.dataPath,"SubjectName.txt"));
            fileName = lines[0];
        }
        lastPosition = transform.position; // 初始化位置
    }

    void Update()
    {
        // 按间隔采样数据
        if (Time.time - lastSampleTime >= sampleInterval)
        {
            RecordDataPoint();
            lastSampleTime = Time.time;
        }
    }

    void RecordDataPoint()
    {
        // 计算速度：位移 / 时间
        float deltaTime = Time.time - lastSampleTime;
        Vector3 displacement = transform.position - lastPosition;
        float speed = displacement.magnitude / deltaTime;

        velocityData.Add(new Vector2(
            Time.timeSinceLevelLoad,
            speed
        ));

        lastPosition = transform.position; // 更新位置
    }

    // CSV导出方法
    public void ExportCSV()
    {
        if (velocityData.Count == 0)
        {
            Debug.LogWarning("没有可导出的数据");
            return;
        }

        string filePath = Path.Combine(
            Application.dataPath,
            $"{fileName}_{System.DateTime.Now:yyyyMMddHHmmss}.csv"
        );

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine("Time,Velocity"); // 表头

            foreach (var point in velocityData)
            {
                writer.WriteLine($"{point.x},{point.y}");
            }
        }

        Debug.Log($"数据已导出至：{filePath}");
    }

    // 游戏结束自动导出
    void OnApplicationQuit()
    {
        ExportCSV();
    }
}
