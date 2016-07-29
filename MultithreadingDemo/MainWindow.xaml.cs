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
        enum ComPortStatus { Disconnect, Connected };
        ComPortStatus CPS_A = ComPortStatus.Disconnect;
        ComPortStatus CPS_B = ComPortStatus.Disconnect;
        ComPortStatus CPS_C = ComPortStatus.Disconnect;
        SerialPort SP_A = new SerialPort();
        SerialPort SP_B = new SerialPort();
        SerialPort SP_C = new SerialPort();
        BackgroundWorker bwCP = new BackgroundWorker();

        // 自動連線
        BackgroundWorker bwAuto = new BackgroundWorker();
        string SP_A_Name;
        string SP_B_Name;
        string SP_C_Name;
        bool TA_justDisconnect = false;
        bool TB_justDisconnect = false;
        bool TC_justDisconnect = false;

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
                    Console.WriteLine(com);
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
                        Console.WriteLine(com);
                        ComPortBox.Add(com);
                        CP_now = com;
                    }
                }

                // 看哪個 Thread 未連接, 就分派給他
                if (CPS_A == ComPortStatus.Disconnect && CP_now != "")
                {
                    if (!SP_A.IsOpen)
                    {
                        try
                        {
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
                if (CPS_B == ComPortStatus.Disconnect)
                {
                    
                }
                if (CPS_C == ComPortStatus.Disconnect)
                {
                    
                }
                else
                {
                    Console.WriteLine("SCAN ERROR " + CP_now);
                }
                Console.WriteLine("USB插入: " + CP_now);
            }
            else if (e.NewEvent.ClassPath.ClassName == "__InstanceDeletionEvent") // USB 拔出
            {
                Console.WriteLine(" USB拔出");
            }

            /*foreach (USBControllerDevice Device in USB.WhoUSBControllerDevice(e))
            {
                this.Dispatcher.BeginInvoke((Action)(delegate
                {
                    Console.WriteLine("\tAntecedent：" + Device.Antecedent + "\r\n");
                    Console.WriteLine("\tDependent：" + Device.Dependent + "\r\n");
                }));
            }*/
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

            bwCP.WorkerSupportsCancellation = true;
            bwCP.DoWork += new DoWorkEventHandler(bwCP_DoWork);
            bwCP.ProgressChanged += new ProgressChangedEventHandler(bwCP_ProgressChanged);

            bwAuto.WorkerSupportsCancellation = true;
            bwAuto.DoWork += BwAuto_DoWork;
            bwAuto.ProgressChanged += BwAuto_ProgressChanged;
        }

        private void BwAuto_DoWork(object sender, DoWorkEventArgs e)
        {
            while(true)
            {
                if (bwAuto.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    try
                    {
                        if (CPS_A == ComPortStatus.Disconnect) // Thread A 沒連線
                        {
                            if (!SP_A.IsOpen)
                            {
                                try
                                {
                                    if (TA_justDisconnect)
                                    {
                                        Thread.Sleep(5000);
                                        TA_justDisconnect = false;
                                    }
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

                                    bwAuto.ReportProgress(1);

                                    // 傳送連線指令
                                    SP_A.Write(@"z"); // 檢查是不是DHC的裝置
                                }
                                catch (Exception ex)
                                {
                                    //System.Windows.MessageBox.Show("通訊埠不存在，請檢查是否尚未開啟治具上的USB開關?\n\n如果已開啟的話''請重複一次USB關閉再開啟''\n\n\n" + ex.ToString());
                                    Thread.Sleep(1000);
                                }
                            }
                        }
                        if (CPS_B == ComPortStatus.Disconnect) // Thread B 沒連線
                        {
                            if (!SP_B.IsOpen)
                            {
                                try
                                {
                                    if (TB_justDisconnect)
                                    {
                                        Thread.Sleep(5000);
                                        TB_justDisconnect = false;
                                    }
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

                                    bwAuto.ReportProgress(2);

                                    // 傳送連線指令
                                    SP_B.Write(@"z"); // 檢查是不是DHC的裝置
                                }
                                catch (Exception ex)
                                {
                                    //System.Windows.MessageBox.Show("通訊埠不存在，請檢查是否尚未開啟治具上的USB開關?\n\n如果已開啟的話''請重複一次USB關閉再開啟''\n\n\n" + ex.ToString());
                                    Thread.Sleep(1000);
                                }
                            }
                        }
                        else
                        {
                            Thread.Sleep(1000);
                        }
                    }
                    catch
                    {
                        e.Cancel = true;
                        break;
                    }
                }
            }
        }

        private void BwAuto_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // 連線按鈕更改
            this.Dispatcher.BeginInvoke((Action)(delegate
            {
                if(e.ProgressPercentage == 1)
                {
                    Btn_TA_connect.IsEnabled = false;
                    Btn_TA_connect.Content = "連線中";
                }
                else if(e.ProgressPercentage == 2)
                {
                    Btn_TB_connect.IsEnabled = false;
                    Btn_TB_connect.Content = "連線中";
                }
            }));
        }

        #region BackgroundWorker : ComPort

        void bwCP_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke((Action)(delegate
            {
                ScanComPort();
            }));
        }

        void bwCP_DoWork(object sender, DoWorkEventArgs e) // 每1秒偵測1次, 只要ABC三條線其中一條沒連接, 就會替沒連上的線掛上新的ComPort
        {
            while (true)
            {
                if (bwCP.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    try
                    {
                        if(CPS_A == ComPortStatus.Disconnect || CPS_B == ComPortStatus.Disconnect || CPS_C == ComPortStatus.Disconnect)
                        {
                            bwCP.ReportProgress(0);
                        }
                        Thread.Sleep(1000);
                    }
                    catch
                    {
                        e.Cancel = true;
                        break;
                    }
                }
            }
        }

        void ScanComPort()
        {
            if(CPS_A == ComPortStatus.Disconnect)
            {
                foreach (var com in SerialPort.GetPortNames())
                {
                    if(!CB_TA.Items.Contains(com))
                    {
                        Console.WriteLine(com);
                        CB_TA.Items.Add(com);
                    }
                }
            }
            if (CPS_B == ComPortStatus.Disconnect)
            {
                foreach (var com in SerialPort.GetPortNames())
                {
                    if (!CB_TB.Items.Contains(com))
                    {
                        CB_TB.Items.Add(com);
                    }
                }
            }
            if (CPS_C == ComPortStatus.Disconnect)
            {
                foreach (var com in SerialPort.GetPortNames())
                {
                    if (!CB_TC.Items.Contains(com))
                    {
                        CB_TC.Items.Add(com);
                    }
                }
            }
        }

        #endregion

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
                    TA_justDisconnect = true;

                    if (SP_A.IsOpen)
                    {
                        try
                        {
                            // 移除 ComPortBox 內的 ComPort
                            ComPortBox.Remove(SP_A_Name);
                            Console.Write(SP_A_Name);

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
                    TB_justDisconnect = true;

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
                        if (i < 20 || i > 50) // 無須共用資源階段
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
                                        /*Console.WriteLine("Queue has:");
                                        foreach (var item in request)
                                        {
                                            Console.WriteLine(item);
                                        }*/
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
        }

        void bwC_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            PB_TC.Value = e.ProgressPercentage;
        }

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
            bwCP.CancelAsync();
            bwAuto.CancelAsync();

            bwA.WorkerReportsProgress = false;
            bwB.WorkerReportsProgress = false;
            bwC.WorkerReportsProgress = false;
            bwSR.WorkerReportsProgress = false;
            bwCP.WorkerReportsProgress = false;
            bwAuto.WorkerReportsProgress = false;

            bwA.Dispose();
            bwB.Dispose();
            bwC.Dispose();
            bwSR.Dispose();
            bwCP.Dispose();
            bwAuto.Dispose();
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
                bwC.WorkerReportsProgress = true; //**
                bwSR.WorkerReportsProgress = true;
                //bwCP.WorkerReportsProgress = true;
                //bwAuto.WorkerReportsProgress = true;
                
                bwC.RunWorkerAsync(); //**
                bwSR.RunWorkerAsync();
                //bwCP.RunWorkerAsync();
                //bwAuto.RunWorkerAsync();

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

                /*
                // 清空Queue, 共享資源狀態改成Free
                request.Clear();
                SR = SharedResources.Free;

                BW_Close();

                // Thread A斷線
                if (SP_A.IsOpen)
                {
                    try
                    {
                        // 關閉 ComPort
                        CPS_A = ComPortStatus.Disconnect;
                        Thread.Sleep(200);
                        SP_A.Close();

                        // UI 更新
                        PB_TA.Value = 0;
                        Btn_TA_connect.Content = "連線";
                        CB_TA.IsEnabled = true;
                        Btn_TA_connect.IsEnabled = true;
                    }
                    catch (IOException ex)
                    {
                        System.Windows.MessageBox.Show("通訊埠不存在，請檢查是否已先行關閉治具上的USB開關?\n\n\n" + ex.ToString());
                    }
                }

                // Thread B斷線
                if (SP_B.IsOpen)
                {
                    try
                    {
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

                // UI 更新為初始狀態
                this.Dispatcher.Invoke((Action)(delegate ()
                {
                    Btn_Start.Content = "Start";
                    SP_Mode.IsEnabled = true;
                    RB_Free.IsChecked = true;
                    LB_SharedResources.Content = "Free";

                    PB_TA.Value = 0;
                    PB_TB.Value = 0;
                    PB_TC.Value = 0;

                    // UI 更新, 關閉 BackgroundWorker 連線功能
                    CB_TA.Items.Clear();
                    CB_TB.Items.Clear();
                    CB_TC.Items.Clear();
                    CB_TA.IsEnabled = false;
                    CB_TB.IsEnabled = false;
                    CB_TC.IsEnabled = false;
                    Btn_TA_connect.IsEnabled = false;
                    Btn_TB_connect.IsEnabled = false;
                    Btn_TC_connect.IsEnabled = false;
                    Btn_TA_connect.Content = "連線";
                    Btn_TB_connect.Content = "連線";
                    Btn_TC_connect.Content = "連線";
                }));

                Console.Clear(); // 清空CMD畫面*/
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

        private void Btn_TA_connect_Click(object sender, RoutedEventArgs e)
        {
            SP_A_Name = CB_TA.Text;
            CB_TA.IsEnabled = false;
            Btn_TA_connect.IsEnabled = false;
        }

        private void Btn_TB_connect_Click(object sender, RoutedEventArgs e)
        {
            SP_B_Name = CB_TB.Text;
            CB_TB.IsEnabled = false;
            Btn_TB_connect.IsEnabled = false;
        }
    }
}
