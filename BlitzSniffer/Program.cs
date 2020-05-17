using System;

namespace BlitzSniffer
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            DemoApp.DemoApp _window = new DemoApp.DemoApp();
            System.Windows.Application _wpfApplication = new System.Windows.Application();
            _wpfApplication.Run(_window);
        }
    }
}
