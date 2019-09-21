using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
namespace Spark
{
    public partial class MainPage : Page
    {
        public const string FEATURE_NAME = "步态测量";

        List<Scenario> scenarios = new List<Scenario>
        {
            new Scenario() { Title="扫描传感器", ClassType=typeof(Discovery) },
            new Scenario() { Title="连接传感器", ClassType=typeof(Client) },
            //new Scenario() { Title="Server: Publish foreground", ClassType=typeof(Scenario3_ServerForeground) },
        };

        public List<string> SelectedBleDeviceId = new List<string>();
        public List<string> SelectedBleDeviceName = new List<string>();
    }

    public class Scenario
    {
        public string Title { get; set; }
        public Type ClassType { get; set; }
    }
}
