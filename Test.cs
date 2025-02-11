using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp.Extensions;
using OpenCvSharp;
using Point = OpenCvSharp.Point;
using NVRCsharpDemo;

namespace PaddleOCR.TotalStation
{
    public partial class Test : Form
    {
        public Test()
        {
            InitializeComponent();
        }

        private bool m_bInitSDK = false;
        private void button1_Click(object sender, EventArgs e)
        {
            ModelLoader modelLoader = new ModelLoader();
            ImgEdit imgEdit = new ImgEdit();

            Point acenterPoint = new Point();

            int iWidth = 0, iHeight = 0;
            uint iActualSize = 0;
            uint nBufSize = (uint)(iWidth * iHeight) * 8;

            string imagePath = @"E://win_serial//Useful//1.bmp";
            byte[] pBitmap = LoadBitmapToByteArray(imagePath);
            Bitmap bmp = ConvertByteArrayToBitmap(pBitmap);
            var (resultBitmap, centerPoint) = modelLoader.ProcessAndDrawOnBitmap(bmp);
            if (resultBitmap != null & centerPoint != null)
            {
                MessageBox.Show(centerPoint.Value.X.ToString() + ":" + centerPoint.Value.Y.ToString());
                imgEdit.ShowBitmapWithOpenCV(resultBitmap);
            }

            acenterPoint = imgEdit.maxLoc(imagePath);

        }

        static byte[] LoadBitmapToByteArray(string path)
        {
            try
            {
                // 加载位图
                using (Bitmap bitmap = new Bitmap(path))
                {
                    // 使用 MemoryStream 将 Bitmap 转换为字节数组
                    using (MemoryStream ms = new MemoryStream())
                    {
                        bitmap.Save(ms, ImageFormat.Bmp);
                        return ms.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生错误: {ex.Message}");
                return null;
            }
        }

        public static Bitmap ConvertByteArrayToBitmap(byte[] imageBytes)
        {
            using (MemoryStream ms = new MemoryStream(imageBytes))
            {
                return new Bitmap(ms);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            xiangji x =new xiangji();
            m_bInitSDK = CHCNetSDK.NET_DVR_Init();

            if (m_bInitSDK == false)
            {
                MessageBox.Show("NET_DVR_Init error!");
                return;
            }
            x.Login();
            x.zuobiao();
            x.ini_zuobiao();

            TotalStation toatl = new TotalStation();
            cmd cmd = new cmd();
            string aa = cmd.Setcmd("ji");
            toatl.WriteLine(aa);
        }
    }
}
