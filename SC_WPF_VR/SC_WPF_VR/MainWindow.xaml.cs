using System;
using System.Windows;
using System.Windows.Interop;
using System.ComponentModel;

namespace SC_WPF_VR
{
    public partial class MainWindow : Window
    {
        MainWindow currentWindow;
        SC_DirectX directXWindows;
        WindowInteropHelper wih;
        public static int switchOff = 0;
        HwndSource source;
        public MainWindow()
        {
            currentWindow = this;

            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += (object sender, DoWorkEventArgs args) =>
            {     
                for (int i = 0; i < 1; i++)
                {
                _threadLoop:
              
                    var someAction = new Action(delegate
                    {
                        source = (HwndSource)HwndSource.FromVisual(mainDXWindow);
                        //wih = new WindowInteropHelper(currentWindow);
                        if (source!= null)
                        {
                            if (source.Handle != IntPtr.Zero)
                            {
                                Console.WriteLine("Step 1 results: found main window handle");
                                switchOff = 1;
                            }
                            else
                            {
                                Console.WriteLine("Step 1 results: didn't find main window handle");
                                switchOff = 0;
                            }
                        }                       
                    });

                    if (switchOff == 1)
                    {
                        break;
                    }

                    System.Threading.Thread.Sleep(1);
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, someAction);

                    goto _threadLoop;
                }
            };

            backgroundWorker.RunWorkerAsync();

            backgroundWorker.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs args)
            {

                Console.WriteLine("Step 2 result: starting program");
                directXWindows = new SC_DirectX(source.Handle, currentWindow);
            };
        }
    }
}




