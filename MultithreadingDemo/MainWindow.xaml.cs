﻿using System;
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


        public MainWindow()
        {
            InitializeComponent();

            BW_Initialize();

            bwCP.WorkerReportsProgress = true;
            bwCP.RunWorkerAsync();
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
        }

        void bwCP_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ScanComPort();
        }

        void bwCP_DoWork(object sender, DoWorkEventArgs e)
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
                        Thread.Sleep(1000);
                        if(CPS_A == ComPortStatus.Disconnect || CPS_B == ComPortStatus.Disconnect || CPS_C == ComPortStatus.Disconnect)
                        {
                            bwCP.ReportProgress(0);
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

        void ScanComPort()
        {
            if(CPS_A == ComPortStatus.Disconnect)
            {
                foreach (var com in SerialPort.GetPortNames())
                {
                    if(!CB_TA.Items.Contains(com))
                    {
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
                        if(i < 20) // 無須共用資源階段, 連接ComPort
                        {
                            if (SP_A.BytesToRead != 0)
                            {
                                string msg = SP_A.ReadExisting();
                                Console.WriteLine("#####\nThread A, GET MSG: {0}\n#####", msg);
                                CPS_A = ComPortStatus.Connected;

                                for (; i < 20; i++)
                                {
                                    bwA.ReportProgress(i);
                                    Thread.Sleep(100);
                                }
                            }
                            Thread.Sleep(100);
                        }
                        else if (i > 50) // 無須共用資源階段
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
                                        /*Console.WriteLine("Queue has:");
                                        foreach(var item in request)
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
            bwA.ReportProgress(999);
        }

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
                        if (i < 20 || i > 50) // 無須共用資源階段
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

        void bwA_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if(e.ProgressPercentage <= 100)
            {
                PB_TA.Value = e.ProgressPercentage;
            }
            if (CPS_A == ComPortStatus.Connected && CB_TA.IsEnabled == true)
            {
                CB_TA.IsEnabled = false;
            }
            if(e.ProgressPercentage == 999) // 達成斷線條件
            {
                Btn_TA_connect.IsEnabled = true;
                Btn_TA_connect.Content = "斷線";
            }
        }

        void bwB_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            PB_TB.Value = e.ProgressPercentage;
        }

        void bwC_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            PB_TC.Value = e.ProgressPercentage;
        }

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
                                /*Console.WriteLine("Queue has:");
                                foreach (var item in request)
                                {
                                    Console.WriteLine(item);
                                }*/
                            }
                        }

                        bwSR.ReportProgress(0);
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

        void BW_Close()
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

            bwCP.CancelAsync();
            bwCP.WorkerReportsProgress = false;
            bwCP.Dispose();
        }

        private void Btn_Start_Click(object sender, RoutedEventArgs e)
        {
            if((String)Btn_Start.Content == "Start") // 按下start
            {
                //bwA.WorkerReportsProgress = true;
                bwB.WorkerReportsProgress = true;
                bwC.WorkerReportsProgress = true;
                bwSR.WorkerReportsProgress = true;

                //bwA.RunWorkerAsync();
                bwB.RunWorkerAsync();
                bwC.RunWorkerAsync();
                bwSR.RunWorkerAsync();

                this.Dispatcher.Invoke((Action)(delegate ()
                {
                    Btn_Start.Content = "Reset";
                    SP_Mode.IsEnabled = false;
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
                request.Clear();
                SR = SharedResources.Free;

                BW_Close();

                // 等同按下Thread A的斷線
                if (SP_A.IsOpen)
                {
                    try
                    {
                        // bw
                        bwA.CancelAsync();
                        bwA.WorkerReportsProgress = false;
                        bwA.Dispose();

                        // comport
                        CPS_A = ComPortStatus.Disconnect;
                        Thread.Sleep(200);
                        SP_A.Close();

                        // UI
                        PB_TA.Value = 0;
                        Btn_TA_connect.Content = "連線";
                        CB_TA.IsEnabled = true;
                        Btn_TA_connect.IsEnabled = true;

                        CB_TA.IsEnabled = false;
                        CB_TB.IsEnabled = false;
                        CB_TC.IsEnabled = false;
                        Btn_TA_connect.IsEnabled = false;
                        Btn_TB_connect.IsEnabled = false;
                        Btn_TC_connect.IsEnabled = false;
                    }
                    catch (IOException ex)
                    {
                        System.Windows.MessageBox.Show("通訊埠不存在，請檢查是否已先行關閉治具上的USB開關?\n\n\n" + ex.ToString());
                    }
                }


                this.Dispatcher.Invoke((Action)(delegate ()
                {
                    Btn_Start.Content = "Start";
                    SP_Mode.IsEnabled = true;
                    RB_Demo.IsChecked = true;
                    RB_Free.IsChecked = true;
                    LB_SharedResources.Content = "Free";

                    PB_TA.Value = 0;
                    PB_TB.Value = 0;
                    PB_TC.Value = 0;
                }));

                Console.Clear();
            }
        }

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

        private void RB_Demo_Checked(object sender, RoutedEventArgs e)
        {
            IsAuto = false;
            this.Dispatcher.Invoke((Action)(delegate ()
            {
                SP_SR.IsEnabled = true;
            }));
        }

        private void RB_Auto_Checked(object sender, RoutedEventArgs e)
        {
            IsAuto = true;
            this.Dispatcher.Invoke((Action)(delegate ()
            {
                SP_SR.IsEnabled = false;
            }));
        }

        private void Btn_TA_connect_Click(object sender, RoutedEventArgs e)
        {
            if((String)Btn_TA_connect.Content == "連線")
            {
                if (!SP_A.IsOpen)
                {
                    try
                    {
                        SP_A.PortName = CB_TA.Text;
                        SP_A.BaudRate = 9600;
                        SP_A.DataBits = 8;
                        SP_A.Parity = System.IO.Ports.Parity.None;
                        SP_A.StopBits = System.IO.Ports.StopBits.One;
                        SP_A.Encoding = Encoding.ASCII;//傳輸編碼方式

                        SP_A.Open();
                        SP_A.DtrEnable = true;

                        if (bwA.IsBusy != true)
                        {
                            bwA.WorkerReportsProgress = true;
                            bwA.RunWorkerAsync();
                        }
                        // 連線按鈕更改
                        Btn_TA_connect.IsEnabled = false;
                        Btn_TA_connect.Content = "連線中";

                        // 傳送連線指令
                        SP_A.Write(@"z"); // 檢查是不是DHC的裝置

                        //Thread.Sleep(100);
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show("通訊埠不存在，請檢查是否尚未開啟治具上的USB開關?\n\n如果已開啟的話''請重複一次USB關閉再開啟''\n\n\n" + ex.ToString());
                    }
                }
            }
            else if((String)Btn_TA_connect.Content == "斷線")
            {
                if(SP_A.IsOpen)
                {
                    try
                    {
                        // bw
                        bwA.CancelAsync();
                        bwA.WorkerReportsProgress = false;
                        bwA.Dispose();

                        // comport
                        CPS_A = ComPortStatus.Disconnect;
                        Thread.Sleep(200);
                        SP_A.Close();

                        // UI
                        PB_TA.Value = 0;
                        Btn_TA_connect.Content = "連線";
                        CB_TA.IsEnabled = true;
                    }
                    catch(IOException ex)
                    {
                        System.Windows.MessageBox.Show("通訊埠不存在，請檢查是否已先行關閉治具上的USB開關?\n\n\n" + ex.ToString());
                    }
                }
            }
        }
    }
}
