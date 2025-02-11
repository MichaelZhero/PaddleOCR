using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using System;
using System.Drawing;

using AForge.Video;
using AForge.Video.DirectShow;
using MathNet.Numerics.LinearAlgebra;

namespace PaddleOCR.TotalStation
{
    internal class LaserAlignment
    {
    }


    public class LaserAlignmentSystem : IDisposable
    {
        // 硬件控制参数
        private readonly SerialPort _laserController;
        private readonly IVideoSource _videoSource;

        // 控制参数
        private MathNet.Numerics.LinearAlgebra.Matrix<double> _calibrationMatrix;
        private PIDController _pidX = new PIDController(0.1, 0.01, 0.05);
        private PIDController _pidY = new PIDController(0.1, 0.01, 0.05);

        // 图像处理参数
        private const int TargetCenterX = 640;  // 图像中心X坐标
        private const int TargetCenterY = 360;  // 图像中心Y坐标
        private const int LaserThreshold = 200; // 激光点亮度阈值

        public LaserAlignmentSystem(string comPort, int baudRate)
        {
            // 初始化激光控制器
            _laserController = new SerialPort(comPort, baudRate);
            _laserController.Open();

            // 初始化摄像头
            var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            _videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
            _videoSource.NewFrame += VideoSource_NewFrame;
            _videoSource.Start();
        }

        // 图像帧处理
        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            using (var frame = (Bitmap)eventArgs.Frame.Clone())
            {
                var laserPos = DetectLaserPosition(frame);
                if (laserPos.HasValue)
                {
                    AdjustLaserPosition(laserPos.Value);
                }
            }
        }

        // 激光点检测
        private Point? DetectLaserPosition(Bitmap frame)
        {
            int sumX = 0, sumY = 0, count = 0;

            for (int y = 0; y < frame.Height; y++)
            {
                for (int x = 0; x < frame.Width; x++)
                {
                    var pixel = frame.GetPixel(x, y);
                    if (pixel.R > LaserThreshold &&
                        pixel.G < LaserThreshold / 2 &&
                        pixel.B < LaserThreshold / 2)
                    {
                        sumX += x;
                        sumY += y;
                        count++;
                    }
                }
            }

            return count > 0 ? new Point(sumX / count, sumY / count) : (Point?)null;
        }

        // 位置调整
        private void AdjustLaserPosition(Point currentPos)
        {
            // 计算偏差
            double errorX = TargetCenterX - currentPos.X;
            double errorY = TargetCenterY - currentPos.Y;

            // PID计算控制量
            double controlX = _pidX.Calculate(errorX);
            double controlY = _pidY.Calculate(errorY);

            // 转换为电机角度
            var controlVector = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.Dense(new[] { controlX, controlY });
            var angleVector = _calibrationMatrix * controlVector;

            // 发送控制命令
            SendLaserCommand(
                (float)angleVector[0],
                (float)angleVector[1]);
        }

        // 发送激光控制命令
        private void SendLaserCommand(float panAngle, float tiltAngle)
        {
            string command = $"PAN:{panAngle:0.00},TILT:{tiltAngle:0.00}\n";
            _laserController.Write(command);
        }

        // 校准程序
        public void PerformCalibration()
        {
            // 九点校准法生成转换矩阵
            var inputPoints = new List<MathNet.Numerics.LinearAlgebra.Vector<double>>();
            var outputPoints = new List<MathNet.Numerics.LinearAlgebra.Vector<double>>();

            // 在九个预设位置采集数据
            for (float pan = -30; pan <= 30; pan += 30)
            {
                for (float tilt = -30; tilt <= 30; tilt += 30)
                {
                    SendLaserCommand(pan, tilt);
                    System.Threading.Thread.Sleep(500); // 等待稳定

                    var pos = GetCurrentLaserPosition();
                    inputPoints.Add(MathNet.Numerics.LinearAlgebra.Vector<double>.Build.Dense(new[] { (double)pan, (double)tilt }));
                    outputPoints.Add(MathNet.Numerics.LinearAlgebra.Vector<double>.Build.Dense(new[] { (double)pos.X, (double)pos.Y }));
                }
            }

            // 计算最小二乘解
            var A = MathNet.Numerics.LinearAlgebra.Matrix<double>.Build.DenseOfRowVectors(inputPoints);
            var B = MathNet.Numerics.LinearAlgebra.Matrix<double>.Build.DenseOfRowVectors(outputPoints);
            _calibrationMatrix = (A.Transpose() * A).Inverse() * A.Transpose() * B;
        }

        private Point GetCurrentLaserPosition()
        {
            // 获取当前激光位置的实现
            // 这里假设返回一个Point对象
            return new Point(0, 0); // 示例返回值
        }

        public void Dispose()
        {
            _videoSource.SignalToStop();
            _laserController.Close();
        }

        // PID控制器类
        private class PIDController
        {
            private readonly double _kp, _ki, _kd;
            private double _integral, _lastError;

            public PIDController(double kp, double ki, double kd)
            {
                _kp = kp;
                _ki = ki;
                _kd = kd;
            }

            public double Calculate(double error)
            {
                _integral += error;
                double derivative = error - _lastError;
                _lastError = error;

                return _kp * error +
                       _ki * _integral +
                       _kd * derivative;
            }
        }
}

// 使用示例
//var alignmentSystem = new LaserAlignmentSystem("COM3", 9600);
//alignmentSystem.PerformCalibration();

// 保持运行
//Console.ReadLine();
//alignmentSystem.Dispose();
   
}
