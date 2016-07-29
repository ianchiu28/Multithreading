using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Threading;
using System.IO.Ports;
using System.IO;
using System.Diagnostics;
using System.Management;


/**
 * this.Dispatcher.BeginInvoke((Action)(delegate
            {
            }));
 */

namespace MultithreadingDemo
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>

    public partial class MainWindow : Window
    {
        // 三條thread
        private BackgroundWorker bwA = new BackgroundWorker();
        private BackgroundWorker bwB = new BackgroundWorker();
        private BackgroundWorker bwC = new BackgroundWorker();

        // 共享資源
        enum SharedResources { Free, A, B, C };
        SharedResources SR = SharedResources.Free;
        private BackgroundWorker bwSR = new BackgroundWorker();

        // 自動模式, 分派共享資源
        private bool IsAuto = false;
        Queue<SharedResources> request = new Queue<SharedResources>();

        // ComPort端
        enum ComPortStatus { Disconnect, Connected, Connecting};
        ComPortStatus CPS_A = ComPortStatus.Disconnect;
        ComPortStatus CPS_B = ComPortStatus.Disconnect;
        ComPortStatus CPS_C = ComPortStatus.Disconnect;
        SerialPort SP_A = new SerialPort();
        SerialPort SP_B = new SerialPort();
        SerialPort SP_C = new SerialPort();

        // 自動連線
        string SP_A_Name = "";
        string SP_B_Name = "";
        string SP_C_Name = "";

        //USB隨插即用
        USB ezUSB = new USB();
        List<string> ComPortBox = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
   
            BW_Initialize();
            ezUSB.AddUSBEventWatcher(USBEventHandler, USBEventHandler, new TimeSpan(0, 0, 3));
            foreach (var com in SerialPort.GetPortNames())
            {
                if (!ComPortBox.Contains(com))
                {
                    ComPortBox.Add(com);
                }
            }
        }

        private void USBEventHandler(Object sender, EventArrivedEventArgs e)
        {
            if (e.NewEvent.ClassPath.ClassName == "__InstanceCreationEvent") // USB 插入
            {
                // 偵測現在是哪個ComPort進來
                string CP_now = "";
                foreach (var com in SerialPort.GetPortNames())
                {
                    if (!ComPortBox.Contains(com))
                    {
                        ComPortBox.Add(com);
                        CP_now = com;
                    }
                }

                // 看哪個 Thread 未連接, 就分派給他
                if (CPS_A == ComPortStatus.Disconnect && CP_now != "")
                {
                    CPS_A = ComPortStatus.Connecting;
                    if (!SP_A.IsOpen)
                    {
                        try
                        {
                            //CPS_A = ComPortStatus.Connecting;
                            SP_A_Name = CP_now;
                            // 連接ComPort
                            SP_A.PortName = SP_A_Name;
                            SP_A.BaudRate = 9600;
                            SP_A.DataBits = 8;
                            SP_A.Parity = System.IO.Ports.Parity.None;
                            SP_A.StopBits = System.IO.Ports.StopBits.One;
                            SP_A.Encoding = Encoding.ASCII;//傳輸編碼方式
                            SP_A.Open();
                            SP_A.DtrEnable = true;

                            // 執行 BackgroundWorker
                            if (bwA.IsBusy != true)
                            {
                                bwA.WorkerReportsProgress = true;
                                bwA.RunWorkerAsync();
                            }

                            this.Dispatcher.BeginInvoke((Action)(delegate
                            {
                                Btn_TA_connect.IsEnabled = false;
                                Btn_TA_connect.Content = "連線中";
                                
                            }));

                            // 傳送連線指令
                            SP_A.Write(@"z"); // 檢查是不是DHC的裝置
                        }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show("通訊埠不存在，請檢查是否尚未開啟治具上的USB開關?\n\n如果已開啟的話''請重複一次USB關閉再開啟''\n\n\n" + ex.ToString());
                            //Thread.Sleep(1000);
                        }
                    }
                }
                else if (CPS_B == ComPortStatus.Disconnect && CP_now != "")
                {
                    CPS_B = ComPortStatus.Connecting;
                    if (!SP_B.IsOpen)
                    {
                        try
                        {
                            //CPS_B = ComPortStatus.Connecting;
                            SP_B_Name = CP_now;
                            // 連接ComPort
                            SP_B.PortName = SP_B_Name;
                            SP_B.BaudRate = 9600;
                            SP_B.DataBits = 8;
                            SP_B.Parity = System.IO.Ports.Parity.None;
                            SP_B.StopBits = System.IO.Ports.StopBits.One;
                            SP_B.Encoding = Encoding.ASCII;//傳輸編碼方式
                            SP_B.Open();
                            SP_B.DtrEnable = true;

                            // 執行 BackgroundWorker
                            if (bwB.IsBusy != true)
                            {
                                bwB.WorkerReportsProgress = true;
                                bwB.RunWorkerAsync(); 
                            }

                            this.Dispatcher.BeginInvoke((Action)(delegate
                            {
                                Btn_TB_connect.IsEnabled = false;
                                Btn_TB_connect.Content = "連線中";

                            }));

                            // 傳送連線指令
                            SP_B.Write(@"z"); // 檢查是不是DHC的裝置
                        }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show("通訊埠不存在，請檢查是否尚未開啟治具上的USB開關?\n\n如果已開啟的話''請重複一次USB關閉再開啟''\n\n\n" + ex.ToString());
                            //Thread.Sleep(1000);
                        }
                    }
                }
                else if (CPS_C == ComPortStatus.Disconnect && CP_now != "")
                {
                    CPS_C = ComPortStatus.Connecting;
                    if (!SP_C.IsOpen)
                    {
                        try
                        {
                            //CPS_C = ComPortStatus.Connecting;
                            SP_C_Name = CP_now;
                            // 連接ComPort
                            SP_C.PortName = SP_C_Name;
                            SP_C.BaudRate = 9600;
                            SP_C.DataBits = 8;
                            SP_C.Parity = System.IO.Ports.Parity.None;
                            SP_C.StopBits = System.IO.Ports.StopBits.One;
                            SP_C.Encoding = Encoding.ASCII;//傳輸編碼方式
                            SP_C.Open();
                            SP_C.DtrEnable = true;

                            // 執行 BackgroundWorker
                            if (bwC.IsBusy != true)
                            {
                                bwC.WorkerReportsProgress = true;
                                bwC.RunWorkerAsync();
                            }

                            this.Dispatcher.BeginInvoke((Action)(delegate
                            {
                                Btn_TC_connect.IsEnabled = false;
                                Btn_TC_connect.Content = "連線中";

                            }));

                            // 傳送連線指令
                            SP_C.Write(@"z"); // 檢查是不是DHC的裝置
                        }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show("通訊埠不存在，請檢查是否尚未開啟治具上的USB開關?\n\n如果已開啟的話''請重複一次USB關閉再開啟''\n\n\n" + ex.ToString());
                            //Thread.Sleep(1000);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("SCAN ERROR " + CP_now);
                }
                Console.WriteLine("USB插入: " + CP_now);
            }
            else if (e.NewEvent.ClassPath.ClassName == "__InstanceDeletionEvent") // USB 拔出
            {
                if(CPS_A == ComPortStatus.Disconnect && SP_A_Name != "")
                {
                    // 移除 ComPortBox 內的 ComPort
                    ComPortBox.Remove(SP_A_Name);
                    Console.WriteLine("USB拔出: " + SP_A_Name);
                    SP_A_Name = "";
                }
                else if(CPS_B == ComPortStatus.Disconnect && SP_B_Name != "")
                {
                    // 移除 ComPortBox 內的 ComPort
                    ComPortBox.Remove(SP_B_Name);
                    Console.WriteLine("USB拔出: "+SP_B_Name);
                    SP_B_Name = "";
                }
                else if (CPS_C == ComPortStatus.Disconnect && SP_C_Name != "")
                {
                    // 移除 ComPortBox 內的 ComPort
                    ComPortBox.Remove(SP_C_Name);
                    Console.WriteLine("USB拔出: " + SP_C_Name);
                    SP_C_Name = "";
                }
                else
                {
                    Console.WriteLine("ERROR拔出");
                }
            }
        }

        void BW_Initialize() // BackgroundWorker 設定
        {
            bwA.WorkerSupportsCancellation = true;
            bwA.DoWork += new DoWorkEventHandler(bwA_DoWork);
            bwA.ProgressChanged += new ProgressChangedEventHandler(bwA_ProgressChanged);

            bwB.WorkerSupportsCancellation = true;
            bwB.DoWork += new DoWorkEventHandler(bwB_DoWork);
            bwB.ProgressChanged += new ProgressChangedEventHandler(bwB_ProgressChanged);

            bwC.WorkerSupportsCancellation = true;
            bwC.DoWork += new DoWorkEventHandler(bwC_DoWork);
            bwC.ProgressChanged += new ProgressChangedEventHandler(bwC_ProgressChanged);

            bwSR.WorkerSupportsCancellation = true;
            bwSR.DoWork += new DoWorkEventHandler(bwSR_DoWork);
            bwSR.ProgressChanged += new ProgressChangedEventHandler(bwSR_ProgressChanged);
        }

        #region BackgroundWorker : A

        void bwA_DoWork(object sender, DoWorkEventArgs e)
        {
            int i = 0;
            //耗時作業
            while(i <= 100)
            {
                if(bwA.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    try
                    {
                        if(i < 20) // 連接ComPort階段
                        {
                            if (SP_A.BytesToRead != 0)
                            {
                                string msg = SP_A.ReadExisting();
                                if (!msg.Contains("@"))
                                {
                                    do
                                    {
                                        Thread.Sleep(100);
                                        msg += SP_A.ReadExisting();
                                    } while (!msg.Contains("@"));
                                }
                                Console.WriteLine("##############################");
                                Console.WriteLine("Thread A, GET MSG: {0}", msg);
                                Console.WriteLine("##############################");
                                CPS_A = ComPortStatus.Connected;

                                for (; i < 20; i++)
                                {
                                    Thread.Sleep(100);
                                    bwA.ReportProgress(i);
                                }
                            }
                            Thread.Sleep(100);
                        }
                        else if (i > 50) // 自行運算階段
                        {
                            i++;
                            Thread.Sleep(100);
                            bwA.ReportProgress(i);
                        }
                        else // 須共用資源階段
                        {
                            if (SR == SharedResources.A) // 得到共用資源
                            {
                                i++;
                                Thread.Sleep(100);
                                bwA.ReportProgress(i);

                                if (IsAuto) // 自動模式
                                {
                                    if (i > 50) // 共用資源階段結束, 釋放資源
                                    {
                                        SR = SharedResources.Free;
                                    }
                                }
                            }
                            else // 沒得到共用資源
                            {
                                if (IsAuto) // 自動模式
                                {
                                    if (!request.Contains(SharedResources.A)) // 如果還沒提出需求, 就提出需求
                                    {
                                        request.Enqueue(SharedResources.A);
                                        Console.WriteLine("Request! A");
                                    }
                                }
                                
                                Thread.Sleep(200);
                            }
                        }
                    }
                    catch
                    {
                        e.Cancel = true;
                        break;
                    }
                }
            }
            bwA.ReportProgress(999);
        }

        void bwA_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke((Action)(delegate
            {
                if (e.ProgressPercentage <= 100) // 更新進度條
                {
                    PB_TA.Value = e.ProgressPercentage;
                }
                if (CPS_A == ComPortStatus.Connected && CB_TA.IsEnabled == true) // 連線中, 關閉下拉式選單
                {
                    CB_TA.IsEnabled = false;
                }
                if (e.ProgressPercentage == 999) // 達成斷線條件
                {
                    Btn_TA_connect.IsEnabled = true;

                    if (SP_A.IsOpen)
                    {
                        try
                        {
                            // 關閉 BackgroundWorker
                            bwA.CancelAsync();
                            bwA.WorkerReportsProgress = false;
                            bwA.Dispose();

                            // 關閉 ComPort
                            CPS_A = ComPortStatus.Disconnect;
                            Thread.Sleep(200);
                            SP_A.Close();

                            // UI 更新
                            PB_TA.Value = 0;
                            Btn_TA_connect.Content = "連線";
                            CB_TA.IsEnabled = true;
                        }
                        catch (IOException ex)
                        {
                            System.Windows.MessageBox.Show("通訊埠不存在，請檢查是否已先行關閉治具上的USB開關?\n\n\n" + ex.ToString());
                        }
                    }
                }
            }));
        }

        #endregion

        #region BackgroundWorker : B

        void bwB_DoWork(object sender, DoWorkEventArgs e)
        {
            int i = 0;
            //耗時作業
            while (i <= 100)
            {
                if (bwB.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    try
                    {
                        if (i < 20) // 連接ComPort階段
                        {
                            if (SP_B.BytesToRead != 0)
                            {
                                string msg = SP_B.ReadExisting();
                                if (!msg.Contains("@"))
                                {
                                    do
                                    {
                                        Thread.Sleep(100);
                                        msg += SP_B.ReadExisting();
                                    } while (!msg.Contains("@"));
                                }
                                Console.WriteLine("##############################");
                                Console.WriteLine("Thread B, GET MSG: {0}", msg);
                                Console.WriteLine("##############################");
                                CPS_B = ComPortStatus.Connected;

                                for (; i < 20; i++)
                                {
                                    Thread.Sleep(100);
                                    bwB.ReportProgress(i);
                                }
                            }
                            Thread.Sleep(100);
                        }
                        else if (i > 50) // 自行運算階段
                        {
                            i++;
                            Thread.Sleep(100);
                            bwB.ReportProgress(i);
                        }
                        else // 須共用資源階段
                        {
                            if (SR == SharedResources.B) // 得到共用資源
                            {
                                i++;
                                Thread.Sleep(100);
                                bwB.ReportProgress(i);

                                if (IsAuto) // 自動模式
                                {
                                    if (i > 50) // 共用資源階段結束, 釋放資源
                                    {
                                        SR = SharedResources.Free;
                                    }
                                }
                            }
                            else // 沒得到共用資源
                            {
                                if (IsAuto) // 自動模式
                                {
                                    if (!request.Contains(SharedResources.B)) // 如果還沒提出需求, 就提出需求
                                    {
                                        request.Enqueue(SharedResources.B);
                                        Console.WriteLine("Request! B");
                                    }
                                }
                                Thread.Sleep(200);
                            }
                        }
                    }
                    catch
                    {
                        e.Cancel = true;
                        break;
                    }
                }
            }
            bwB.ReportProgress(999);
        }

        void bwB_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke((Action)(delegate
            {
                if (e.ProgressPercentage <= 100) // 更新進度條
                {
                    PB_TB.Value = e.ProgressPercentage;
                }
                if (CPS_B == ComPortStatus.Connected && CB_TB.IsEnabled == true) // 連線中, 關閉下拉式選單
                {
                    CB_TB.IsEnabled = false;
                }
                if (e.ProgressPercentage == 999) // 達成斷線條件
                {
                    Btn_TB_connect.IsEnabled = true;

                    if (SP_B.IsOpen)
                    {
                        try
                        {
                            // 關閉 BackgroundWorker
                            bwB.CancelAsync();
                            bwB.WorkerReportsProgress = false;
                            bwB.Dispose();

                            // 關閉 ComPort
                            CPS_B = ComPortStatus.Disconnect;
                            Thread.Sleep(200);
                            SP_B.Close();

                            // UI 更新
                            PB_TB.Value = 0;
                            Btn_TB_connect.Content = "連線";
                            CB_TB.IsEnabled = true;
                        }
                        catch (IOException ex)
                        {
                            System.Windows.MessageBox.Show("通訊埠不存在，請檢查是否已先行關閉治具上的USB開關?\n\n\n" + ex.ToString());
                        }
                    }
                }
            }));            
        }

        #endregion

        #region BackgroundWorker : C

        void bwC_DoWork(object sender, DoWorkEventArgs e)
        {
            int i = 0;
            //耗時作業
            while (i <= 100)
            {
                if (bwC.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    try
                    {
                        if (i < 20) // 連接ComPort階段
                        {
                            if (SP_C.BytesToRead != 0)
                            {
                                string msg = SP_C.ReadExisting();
                                if (!msg.Contains("@"))
                                {
                                    do
                                    {
                                        Thread.Sleep(100);
                                        msg += SP_C.ReadExisting();
                                    } while (!msg.Contains("@"));
                                }
                                Console.WriteLine("##############################");
                                Console.WriteLine("Thread C, GET MSG: {0}", msg);
                                Console.WriteLine("##############################");
                                CPS_C = ComPortStatus.Connected;

                                for (; i < 20; i++)
                                {
                                    Thread.Sleep(100);
                                    bwC.ReportProgress(i);
                                }
                            }
                            Thread.Sleep(100);
                        }
                        else if (i > 50) // 自行運算階段
                        {
                            i++;
                            Thread.Sleep(100);
                            bwC.ReportProgress(i);
                        }
                        else // 須共用資源階段
                        {
                            if (SR == SharedResources.C) // 得到共用資源
                            {
                                i++;
                                Thread.Sleep(100);
                                bwC.ReportProgress(i);

                                if (IsAuto) // 自動模式
                                {
                                    if (i > 50) // 共用資源階段結束, 釋放資源
                                    {
                                        SR = SharedResources.Free;
                                    }
                                }
                            }
                            else // 沒得到共用資源
                            {
                                if (IsAuto) // 自動模式
                                {
                                    if (!request.Contains(SharedResources.C)) // 如果還沒提出需求, 就提出需求
                                    {
                                        request.Enqueue(SharedResources.C);
                                        Console.WriteLine("Request! C");
                                    }
                                }
                                Thread.Sleep(200);
                            }
                        }
                    }
                    catch
                    {
                        e.Cancel = true;
                        break;
                    }
                }
            }
            bwC.ReportProgress(999);
        }

        void bwC_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke((Action)(delegate
            {
                if (e.ProgressPercentage <= 100) // 更新進度條
                {
                    PB_TC.Value = e.ProgressPercentage;
                }
                if (CPS_C == ComPortStatus.Connected && CB_TC.IsEnabled == true) // 連線中, 關閉下拉式選單
                {
                    CB_TC.IsEnabled = false;
                }
                if (e.ProgressPercentage == 999) // 達成斷線條件
                {
                    Btn_TC_connect.IsEnabled = true;

                    if (SP_C.IsOpen)
                    {
                        try
                        {
                            // 關閉 BackgroundWorker
                            bwC.CancelAsync();
                            bwC.WorkerReportsProgress = false;
                            bwC.Dispose();

                            // 關閉 ComPort
                            CPS_C = ComPortStatus.Disconnect;
                            Thread.Sleep(200);
                            SP_C.Close();

                            // UI 更新
                            PB_TC.Value = 0;
                            Btn_TC_connect.Content = "連線";
                            CB_TC.IsEnabled = true;
                        }
                        catch (IOException ex)
                        {
                            System.Windows.MessageBox.Show("通訊埠不存在，請檢查是否已先行關閉治具上的USB開關?\n\n\n" + ex.ToString());
                        }
                    }
                }
            }));
        }

        #endregion

        #region BackgroundWorker : 共享資源

        void bwSR_DoWork(object sender, DoWorkEventArgs e)
        {
            for (;;)
            {
                if(bwSR.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    try
                    {
                        if (IsAuto) // 自動模式
                        {
                            if(SR == SharedResources.Free && request.Count != 0) // 如果共用資源處在沒人用的狀態且有thread提出需求, 分配給第一個提出需求的thread
                            {
                                SR = request.Dequeue();
                                Console.WriteLine("\t\tDeQueue! {0}",SR);
                            }
                        }

                        bwSR.ReportProgress(0); // 更新共享資源位置
                        Thread.Sleep(200);
                    }
                    catch
                    {
                        e.Cancel = true;
                        break;
                    }
                }
            }
        }

        void bwSR_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if((String)LB_SharedResources.Content != SR.ToString())
            {
                LB_SharedResources.Content = SR.ToString();
            }
        }

        #endregion

        void BW_Close() // 關掉所有BackgroundWorker
        {
            bwA.CancelAsync();
            bwB.CancelAsync();
            bwC.CancelAsync();
            bwSR.CancelAsync();

            bwA.WorkerReportsProgress = false;
            bwB.WorkerReportsProgress = false;
            bwC.WorkerReportsProgress = false;
            bwSR.WorkerReportsProgress = false;

            bwA.Dispose();
            bwB.Dispose();
            bwC.Dispose();
            bwSR.Dispose();
        } 

        void Window_Closing(object sender, CancelEventArgs e)
        {
            BW_Close();
        }

        private void Btn_Start_Click(object sender, RoutedEventArgs e)
        {
            if((String)Btn_Start.Content == "Start") // 按下start
            {
                // 執行背景BackgroundWorker : 共享資源、ComPort偵測、自動連線
                bwSR.WorkerReportsProgress = true;
                
                bwSR.RunWorkerAsync();

                // UI 更新
                this.Dispatcher.Invoke((Action)(delegate ()
                {
                    Btn_Start.Content = "Reset";
                    SP_Mode.IsEnabled = false;

                    // 開啟各 BackgroundWorker 連線功能
                    CB_TA.IsEnabled = true;
                    CB_TB.IsEnabled = true;
                    CB_TC.IsEnabled = true;
                    Btn_TA_connect.IsEnabled = true;
                    Btn_TB_connect.IsEnabled = true;
                    Btn_TC_connect.IsEnabled = true;
                }));
            }
            else // 按下reset
            {
                // 重起再開
                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }
        }

        #region RadioButton : 共享資源

        private void RB_A_Checked(object sender, RoutedEventArgs e)
        {
            SR = SharedResources.A;
        }

        private void RB_B_Checked(object sender, RoutedEventArgs e)
        {
            SR = SharedResources.B;
        }

        private void RB_C_Checked(object sender, RoutedEventArgs e)
        {
            SR = SharedResources.C;
        }

        private void RB_Free_Checked(object sender, RoutedEventArgs e)
        {
            SR = SharedResources.Free;
        }

        #endregion

        #region RadioButton : Mode

        private void RB_Demo_Checked(object sender, RoutedEventArgs e)
        {
            IsAuto = false;
            this.Dispatcher.Invoke((Action)(delegate ()
            {
                SP_SR.IsEnabled = true; // 關閉共享資源的選擇
            }));
        }

        private void RB_Auto_Checked(object sender, RoutedEventArgs e)
        {
            IsAuto = true;
            this.Dispatcher.Invoke((Action)(delegate ()
            {
                SP_SR.IsEnabled = false; // 關閉共享資源的選擇
            }));
        }

        #endregion
    }
}
