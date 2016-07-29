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

namespace MultithreadingDemo
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>

    public partial class MainWindow : Window
    {
        private BackgroundWorker bwA = new BackgroundWorker();
        private BackgroundWorker bwB = new BackgroundWorker();
        private BackgroundWorker bwC = new BackgroundWorker();

        enum SharedResources { Free, A, B, C };
        SharedResources SR = SharedResources.Free;
        private BackgroundWorker bwSR = new BackgroundWorker();

        private bool IsAuto = false;
        Queue<SharedResources> request = new Queue<SharedResources>();

        public MainWindow()
        {
            InitializeComponent();

            BW_Initialize();
            request.Clear();
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
                        if (i < 20 || i > 50) // 無須共用資源階段
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
            PB_TA.Value = e.ProgressPercentage;
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

        void Window_Closing(object sender, CancelEventArgs e)
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

        private void Btn_Start_Click(object sender, RoutedEventArgs e)
        {
            if((String)Btn_Start.Content == "Start") // 按下start
            {
                bwA.WorkerReportsProgress = true;
                bwB.WorkerReportsProgress = true;
                bwC.WorkerReportsProgress = true;
                bwSR.WorkerReportsProgress = true;

                bwA.RunWorkerAsync();
                bwB.RunWorkerAsync();
                bwC.RunWorkerAsync();
                bwSR.RunWorkerAsync();

                this.Dispatcher.Invoke((Action)(delegate ()
                {
                    Btn_Start.Content = "Restart";
                    SP_Mode.IsEnabled = false;
                }));
            }
            else // 按下restart
            {
                request.Clear();
                SR = SharedResources.Free;

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
    }
}
