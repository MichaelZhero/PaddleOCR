using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Wpf;
using MathNet.Filtering;

namespace PaddleOCR.TotalStation
{
    internal class get_depth_width
    {
    }



    public class GrooveMeasurement
    {
        // 输入：20个点的距离数据（单位：mm）
        public static (double Width, double Depth) MeasureGroove(double[] distances)
        {
            // 参数校验
            if (distances == null || distances.Length != 20)
                throw new ArgumentException("需要20个点的距离数据");

            // 数据预处理
            var smoothed = SmoothData(distances);
            var points = ConvertToPoints(smoothed);

            // 特征提取
            var edges = FindGrooveEdges(points);
            if (edges.Count < 2)
                throw new InvalidOperationException("未检测到凹槽边缘");

            // 计算宽度和深度
            double width = CalculateWidth(edges);
            double depth = CalculateDepth(points, edges);

            return (width, depth);

        }

        // 数据平滑处理（移动平均滤波）
        private static double[] SmoothData(double[] data)
        {
            const int windowSize = 3;
            var smoothed = new double[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                int start = Math.Max(0, i - windowSize / 2);
                int end = Math.Min(data.Length - 1, i + windowSize / 2);
                smoothed[i] = data.Skip(start).Take(end - start + 1).Average();
            }

            return smoothed;
        }

        // 转换为二维坐标点
        private static List<(double X, double Y)> ConvertToPoints(double[] distances)
        {
            var points = new List<(double X, double Y)>();
            for (int i = 0; i < distances.Length; i++)
            {
                points.Add((i * 2.0, distances[i])); // X坐标：2mm间隔
            }
            return points;
        }

        // 检测凹槽边缘
        private static List<int> FindGrooveEdges(List<(double X, double Y)> points)
        {
            const double edgeThreshold = 0.5; // 边缘检测阈值（mm）
            var edges = new List<int>();

            for (int i = 1; i < points.Count - 1; i++)
            {
                double diffPrev = points[i].Y - points[i - 1].Y;
                double diffNext = points[i + 1].Y - points[i].Y;

                // 检测下降沿或上升沿
                if (Math.Abs(diffPrev) > edgeThreshold || Math.Abs(diffNext) > edgeThreshold)
                {
                    edges.Add(i);
                }
            }

            return edges;
        }

        // 使用Savitzky-Golay滤波器
        private static double[] AdvancedSmooth(double[] data)
        {
            // 替换为MathNet.Numerics.Filtering.OnlineFilter.SavitzkyGolay
            var filter = MathNet.Numerics.Series.Filtering.OnlineFilter.CreateSavitzkyGolay(5, 2);
            return filter.ProcessSamples(data);
        }

        // 使用Canny边缘检测算法
        private static List<int> FindEdgesCanny(List<(double X, double Y)> points)
        {
            var gradients = points.Select((p, i) =>
                i > 0 ? points[i].Y - points[i - 1].Y : 0).ToArray();

            return Enumerable.Range(1, gradients.Length - 2)
                .Where(i => Math.Abs(gradients[i]) > 0.5 &&
                            gradients[i] * gradients[i + 1] <= 0)
                .ToList();
        }

        // 添加数据校验
        private static void ValidateData(double[] distances)
        {
            if (distances.Any(d => d < 0 || d > 100))
                throw new ArgumentException("距离数据超出合理范围");
        }

        // 使用OxyPlot绘制曲线
        public static void PlotData(List<(double X, double Y)> points)
        {
            var model = new PlotModel { Title = "铝型材轮廓" };
            var series = new LineSeries();

            foreach (var p in points)
            {
                series.Points.Add(new DataPoint(p.X, p.Y));
            }

            model.Series.Add(series);
            var plotView = new PlotView { Model = model };
            // 显示plotView
        }

        // 计算凹槽宽度
        private static double CalculateWidth(List<int> edges)
        {
            if (edges.Count < 2)
                return 0;

            // 取最外侧的两个边缘点
            int leftEdge = edges[0];
            int rightEdge = edges[^1]; // 使用C# 8.0的索引语法

            return (rightEdge - leftEdge) * 2.0; // 乘以2mm间隔
        }

        // 计算凹槽深度
        private static double CalculateDepth(List<(double X, double Y)> points, List<int> edges)
        {
            if (edges.Count < 2)
                return 0;

            // 计算基线（假设凹槽两侧是平的）
            double leftBase = points[edges[0]].Y;
            double rightBase = points[edges[^1]].Y;
            double baseLevel = (leftBase + rightBase) / 2;

            // 找到凹槽最低点
            double minDepth = points.Skip(edges[0]).Take(edges[^1] - edges[0] + 1)
                .Min(p => p.Y);

            return baseLevel - minDepth;
        }
    }

    // 使用示例
    public class Program
    {
        public static void Main()
        {
            // 模拟激光扫描数据（单位：mm）
            double[] distances = {
            10.0, 10.1, 10.0, 9.9, 9.8,  // 左侧平面
            9.5, 9.0, 8.5, 8.0, 7.5,      // 凹槽下降
            7.0, 7.0, 7.0, 7.0, 7.0,       // 凹槽底部
            7.5, 8.0, 8.5, 9.0, 9.5,       // 凹槽上升
            9.8, 9.9, 10.0, 10.1           // 右侧平面
        };

            try
            {
                var result = GrooveMeasurement.MeasureGroove(distances);
                Console.WriteLine($"凹槽宽度: {result.Width:F2} mm");
                Console.WriteLine($"凹槽深度: {result.Depth:F2} mm");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测量失败: {ex.Message}");
            }
        }
    }


}
