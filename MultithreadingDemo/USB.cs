using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;

namespace MultithreadingDemo
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    /// 

    public struct USBControllerDevice
    {
        /// <summary>  
        /// USB控制器設備ID  
        /// </summary>  
        public String Antecedent;

        /// <summary>  
        /// USB即插即用設備ID  
        /// </summary>  
        public String Dependent;
    }

    /// <summary>  
    /// 監視USB插拔  
    /// </summary>  
    public partial class USB
    {
        /// <summary>  
        /// USB插入事件監視  
        /// </summary>  
        private ManagementEventWatcher insertWatcher = null;

        /// <summary>  
        /// USB拔出事件監視  
        /// </summary>  
        private ManagementEventWatcher removeWatcher = null;

        /// <summary>  
        /// 添加USB事件監視器  
        /// </summary>  
        /// <param name="usbInsertHandler">USB插入事件處理器</param>  
        /// <param name="usbRemoveHandler">USB拔出事件處理器</param>  
        /// <param name="withinInterval">發送通知允許的滯後時間</param>  
        public Boolean AddUSBEventWatcher(EventArrivedEventHandler usbInsertHandler, EventArrivedEventHandler usbRemoveHandler, TimeSpan withinInterval)
        {
            try
            {
                ManagementScope Scope = new ManagementScope("root\\CIMV2");
                Scope.Options.EnablePrivileges = true;

                // USB插入監視  
                if (usbInsertHandler != null)
                {
                    WqlEventQuery InsertQuery = new WqlEventQuery("__InstanceCreationEvent",
                        withinInterval,
                        "TargetInstance isa 'Win32_USBControllerDevice'");

                    insertWatcher = new ManagementEventWatcher(Scope, InsertQuery);
                    insertWatcher.EventArrived += usbInsertHandler;
                    insertWatcher.Start();
                }

                // USB拔出監視  
                if (usbRemoveHandler != null)
                {
                    WqlEventQuery RemoveQuery = new WqlEventQuery("__InstanceDeletionEvent",
                        withinInterval,
                        "TargetInstance isa 'Win32_USBControllerDevice'");

                    removeWatcher = new ManagementEventWatcher(Scope, RemoveQuery);
                    removeWatcher.EventArrived += usbRemoveHandler;
                    removeWatcher.Start();
                }

                return true;
            }

            catch (Exception)
            {
                RemoveUSBEventWatcher();
                return false;
            }
        }

        /// <summary>  
        /// 移去USB事件監視器  
        /// </summary>  
        public void RemoveUSBEventWatcher()
        {
            if (insertWatcher != null)
            {
                insertWatcher.Stop();
                insertWatcher = null;
            }

            if (removeWatcher != null)
            {
                removeWatcher.Stop();
                removeWatcher = null;
            }
        }

        /// <summary>  
        /// 定位發生插拔的USB設備  
        /// </summary>  
        /// <param name="e">USB插拔事件參數</param>  
        /// <returns>發生插拔現象的USB控制設備ID</returns>  
        public static USBControllerDevice[] WhoUSBControllerDevice(EventArrivedEventArgs e)
        {
            ManagementBaseObject mbo = e.NewEvent["TargetInstance"] as ManagementBaseObject;
            if (mbo != null && mbo.ClassPath.ClassName == "Win32_USBControllerDevice")
            {
                String Antecedent = (mbo["Antecedent"] as String).Replace("\"", String.Empty).Split(new Char[] { '=' })[1];
                String Dependent = (mbo["Dependent"] as String).Replace("\"", String.Empty).Split(new Char[] { '=' })[1];
                return new USBControllerDevice[1] { new USBControllerDevice { Antecedent = Antecedent, Dependent = Dependent } };
            }

            return null;
        }
    }
}
