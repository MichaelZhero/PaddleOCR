using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NVRCsharpDemo;
using OpenCvSharp;

namespace PaddleOCR.TotalStation
{
    internal class xiangji
    {
        private bool m_bInitSDK = false;
        private bool m_bRecord = false;
        private uint iLastErr = 0;
        private Int32 m_lUserID = -1;
        private Int32 m_lRealHandle = -1;
        private string str1;
        private string str2;
        private Int32 i = 0;
        private Int32 m_lTree = 0;
        private string str;
        private long iSelIndex = 0;
        private uint dwAChanTotalNum = 0;
        private uint dwDChanTotalNum = 0;
        private Int32 m_lPort = -1;
        private IntPtr m_ptrRealHandle;
        private int[] iIPDevID = new int[96];
        private int[] iChannelNum = new int[96]; private Thread bmpCaptureThread;
        private bool captureRunning = false;
        private CHCNetSDK.REALDATACALLBACK RealData = null;
        public CHCNetSDK.NET_DVR_DEVICEINFO_V30 DeviceInfo;
        public CHCNetSDK.NET_DVR_IPPARACFG_V40 m_struIpParaCfgV40;
        public CHCNetSDK.NET_DVR_STREAM_MODE m_struStreamMode;
        public CHCNetSDK.NET_DVR_IPCHANINFO m_struChanInfo;
        public CHCNetSDK.NET_DVR_PU_STREAM_URL m_struStreamURL;
        public CHCNetSDK.NET_DVR_IPCHANINFO_V40 m_struChanInfoV40;
        private PlayCtrl.DECCBFUN m_fDisplayFun = null;
        public delegate void MyDebugInfo(string str);

        public int m_lChannel = 1;
        private bool bAuto = false;

        public int PreSetNo = 0;
        public CHCNetSDK.NET_DVR_PTZPOS m_struPtzCfg;
        public CHCNetSDK.NET_DVR_PTZSCOPE m_struPtzCfg1;
        public CHCNetSDK.NET_DVR_PRESET_NAME[] m_struPreSetCfg = new CHCNetSDK.NET_DVR_PRESET_NAME[300];


        public void Login()
        {
            if (m_lUserID < 0)
            {
                string DVRIPAddress = IpPort.IP; //设备IP地址或者域名 Device IP
                Int16 DVRPortNumber = Int16.Parse(IpPort.Port); //设备服务端口号 Device Port
                string DVRUserName = IpPort.User; //设备登录用户名 User name to login
                string DVRPassword = IpPort.Password; //设备登录密码 Password to login

                //登录设备 Login the device
                m_lUserID = CHCNetSDK.NET_DVR_Login_V30(DVRIPAddress, DVRPortNumber, DVRUserName, DVRPassword, ref DeviceInfo);
                if (m_lUserID < 0)
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    string str = "NET_DVR_Login_V30 failed, error code= " + iLastErr; //登录失败，输出错误号 Failed to login and output the error code
                    DebugInfo(str);
                    return;
                }
                else
                {
                    //登录成功
                    DebugInfo("NET_DVR_Login_V30 succ!");
                    dwAChanTotalNum = (uint)DeviceInfo.byChanNum;
                    dwDChanTotalNum = (uint)DeviceInfo.byIPChanNum + 256 * (uint)DeviceInfo.byHighDChanNum;
                    if (dwDChanTotalNum > 0)
                    {
                        //InfoIPChannel();
                    }
                    else
                    {
                        for (i = 0; i < dwAChanTotalNum; i++)
                        {
                            iChannelNum[i] = i + (int)DeviceInfo.byStartChan;
                        }
                        //comboBoxView.SelectedItem = 1;
                        // MessageBox.Show("This device has no IP channel!");
                    }
                }
            }
            else
            {
                //注销登录 Logout the device
                if (!CHCNetSDK.NET_DVR_Logout(m_lUserID))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    string str = "NET_DVR_Logout failed, error code= " + iLastErr;
                    DebugInfo(str);
                    return;
                }
                m_lUserID = -1;
            }
            return;
        }

        //日志输出
        public void DebugInfo(string str)
        {
            if (!string.IsNullOrWhiteSpace(str))
            {
                // 添加时间戳并确保在行末添加换行符
                string output = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {str}\r\n";


            }
        }

        public void zuobiao()
        {
            UInt32 dwReturn = 0;
            Int32 nSize = Marshal.SizeOf(m_struPtzCfg);
            IntPtr ptrPtzCfg = Marshal.AllocHGlobal(nSize);
            Marshal.StructureToPtr(m_struPtzCfg, ptrPtzCfg, false);
            //获取参数失败
            if (!CHCNetSDK.NET_DVR_GetDVRConfig(m_lUserID, CHCNetSDK.NET_DVR_GET_PTZPOS, -1, ptrPtzCfg, (UInt32)nSize, ref dwReturn))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                str = "NET_DVR_GetDVRConfig failed, error code= " + iLastErr;
                MessageBox.Show(str);
                return;
            }
            else
            {
                m_struPtzCfg = (CHCNetSDK.NET_DVR_PTZPOS)Marshal.PtrToStructure(ptrPtzCfg, typeof(CHCNetSDK.NET_DVR_PTZPOS));
                //成功获取显示ptz参数
                ushort wPanPos = Convert.ToUInt16(Convert.ToString(m_struPtzCfg.wPanPos, 16));
                float WPanPos = wPanPos * 0.1f;
                //MessageBox.Show(Convert.ToString(WPanPos));
                ushort wTiltPos = Convert.ToUInt16(Convert.ToString(m_struPtzCfg.wTiltPos, 16));
                float WTiltPos = wTiltPos * 0.1f;
                //MessageBox.Show(Convert.ToString(WTiltPos));
                ushort wZoomPos = Convert.ToUInt16(Convert.ToString(m_struPtzCfg.wZoomPos, 16));
                float WZoomPos = wZoomPos * 0.1f;
                //MessageBox.Show(Convert.ToString(WZoomPos));

            }
            return;
        }

        public void ini_zuobiao()
        {
            int flag = 0;

            

            m_struPtzCfg.wPanPos = (ushort)(Convert.ToUInt16("0", 16));
            m_struPtzCfg.wTiltPos = (ushort)(Convert.ToUInt16("0", 16));
            m_struPtzCfg.wZoomPos = (ushort)(Convert.ToUInt16("1", 16));

            while (flag == 0)
            {

                Int32 nSize = Marshal.SizeOf(m_struPtzCfg);
                IntPtr ptrPtzCfg = Marshal.AllocHGlobal(nSize);
                Marshal.StructureToPtr(m_struPtzCfg, ptrPtzCfg, false);

                if (!CHCNetSDK.NET_DVR_SetDVRConfig(m_lUserID, CHCNetSDK.NET_DVR_SET_PTZPOS, 1, ptrPtzCfg, (UInt32)nSize))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_SetDVRConfig failed, error code= " + iLastErr;
                    //设置POS参数失败
                    MessageBox.Show(str);
                    return;
                }
                else
                {
                    MessageBox.Show("设置成功!");
                    break;
                }

                // Marshal.FreeHGlobal(ptrPtzCfg);

            }
            return;

        }
    }
}
