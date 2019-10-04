using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Security.Cryptography;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;


namespace Spark
{
   
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Client : Page
    {
        private MainPage rootPage = MainPage.Current;
        private List<BluetoothLEDevice> bluetoothLeDevice = new List<BluetoothLEDevice>();
        private GattCharacteristic[] registeredCharacteristic = new GattCharacteristic[3];        

        struct Data
        {
            public DateTime dateTime;
            public string data;
        }

        private List<Data>[] datas = { new List<Data>(), new List<Data>(), new List<Data>() };

        #region Error Codes       
        readonly int E_DEVICE_NOT_AVAILABLE = unchecked((int)0x800710df); // HRESULT_FROM_WIN32(ERROR_DEVICE_NOT_AVAILABLE)
        #endregion

        #region UI Code
        public Client()
        {
            this.InitializeComponent();           
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (rootPage.SelectedBleDeviceId != null )
            {
                SelectedDeviceRun.Text = rootPage.SelectedBleDeviceName.Count.ToString();
                try
                {
                    // BT_Code: BluetoothLEDevice.FromIdAsync must be called from a UI thread because it may prompt for consent.
                    foreach (var SelectedBleDevice in rootPage.SelectedBleDeviceId)
                    {
                        bluetoothLeDevice.Add(await BluetoothLEDevice.FromIdAsync(SelectedBleDevice));
                    }

                    if (bluetoothLeDevice == null)
                    {
                        rootPage.NotifyUser("连接到设备失败.", NotifyType.ErrorMessage);
                    }
                }
                catch (Exception ex) when (ex.HResult == E_DEVICE_NOT_AVAILABLE)
                {
                    rootPage.NotifyUser("蓝牙没有打开.", NotifyType.ErrorMessage);
                }

                switch (bluetoothLeDevice.Count)
                {
                    case 1:
                        ConnectButton1.IsEnabled = true;
                        ConnectButton2.IsEnabled = false;
                        ConnectButton3.IsEnabled = false;
                        break;
                    case 2:
                        ConnectButton1.IsEnabled = true;
                        ConnectButton2.IsEnabled = true;
                        ConnectButton3.IsEnabled = false;
                        break;
                    case 3:
                        ConnectButton1.IsEnabled = true;
                        ConnectButton2.IsEnabled = true;
                        ConnectButton3.IsEnabled = true;
                        break;
                    default:
                        ConnectButton1.IsEnabled = false;
                        ConnectButton2.IsEnabled = false;
                        ConnectButton3.IsEnabled = false;
                        break;

                }
            }
            else
            {
                rootPage.NotifyUser("没有选择蓝牙设备.", NotifyType.ErrorMessage);
                ConnectButton1.IsEnabled = false;
                ConnectButton2.IsEnabled = false;
                ConnectButton3.IsEnabled = false;
            }
        }

        protected async override void OnNavigatedFrom(NavigationEventArgs e)
        {
            var success = await ClearBluetoothLEDeviceAsync();
            registeredCharacteristic = null;
            datas = null;

            if (!success)
            {
                rootPage.NotifyUser("错误: 无法重置软件状态", NotifyType.ErrorMessage);
            }
        }
        #endregion

        #region Enumerating Services       

        private async void ConnectButton1_Click()
        {      
            ConnectButton1.IsEnabled = false;
            //subscribedForNotifications = false;

            if (bluetoothLeDevice != null)
            {
                // Note: BluetoothLEDevice.GattServices property will return an empty list for unpaired devices. For all uses we recommend using the GetGattServicesAsync method.
                // BT_Code: GetGattServicesAsync returns a list of all the supported services of the device (even if it's not paired to the system).
                // If the services supported by the device are expected to change during BT usage, subscribe to the GattServicesChanged event.

                var _bluetoothLeDevice = bluetoothLeDevice[0];
                {
                    List<GattDeviceServicesResult> result = new List<GattDeviceServicesResult>();
                    result.Add(await _bluetoothLeDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached));
                    foreach (var _result in result)
                    {

                        if (_result.Status == GattCommunicationStatus.Success)
                        {
                            var services = _result.Services;
                            rootPage.NotifyUser(String.Format("发现 {0} 服务", services.Count), NotifyType.StatusMessage);
                            ServiceList(services,0);
                        }
                        else
                        {
                            rootPage.NotifyUser("无法获取设备", NotifyType.ErrorMessage);
                        }
                    }
                }
                
            }

            ConnectButton1.IsEnabled = true;

        }

        private async void ConnectButton2_Click()
        {

            ConnectButton2.IsEnabled = false;
            //subscribedForNotifications[2] = false;

            if (bluetoothLeDevice != null)
            {
                // Note: BluetoothLEDevice.GattServices property will return an empty list for unpaired devices. For all uses we recommend using the GetGattServicesAsync method.
                // BT_Code: GetGattServicesAsync returns a list of all the supported services of the device (even if it's not paired to the system).
                // If the services supported by the device are expected to change during BT usage, subscribe to the GattServicesChanged event.

                var _bluetoothLeDevice = bluetoothLeDevice[1];
                {
                    List<GattDeviceServicesResult> result = new List<GattDeviceServicesResult>();
                    result.Add(await _bluetoothLeDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached));
                    foreach (var _result in result)
                    {

                        if (_result.Status == GattCommunicationStatus.Success)
                        {
                            var services = _result.Services;
                            rootPage.NotifyUser(String.Format("发现 {0} 服务", services.Count), NotifyType.StatusMessage);
                            ServiceList(services,1);
                        }
                        else
                        {
                            rootPage.NotifyUser("无法获取设备", NotifyType.ErrorMessage);
                        }
                    }
                }

            }

            ConnectButton2.IsEnabled = true;

        }

        private async void ConnectButton3_Click()
        {
            ConnectButton3.IsEnabled = false;
            //subscribedForNotifications = false;

            if (bluetoothLeDevice != null)
            {
                // Note: BluetoothLEDevice.GattServices property will return an empty list for unpaired devices. For all uses we recommend using the GetGattServicesAsync method.
                // BT_Code: GetGattServicesAsync returns a list of all the supported services of the device (even if it's not paired to the system).
                // If the services supported by the device are expected to change during BT usage, subscribe to the GattServicesChanged event.

                var _bluetoothLeDevice = bluetoothLeDevice[2];
                {
                    List<GattDeviceServicesResult> result = new List<GattDeviceServicesResult>();
                    result.Add(await _bluetoothLeDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached));
                    foreach (var _result in result)
                    {

                        if (_result.Status == GattCommunicationStatus.Success)
                        {
                            var services = _result.Services;
                            rootPage.NotifyUser(String.Format("发现 {0} 服务", services.Count), NotifyType.StatusMessage);
                            ServiceList(services, 2);
                        }
                        else
                        {
                            rootPage.NotifyUser("无法获取设备", NotifyType.ErrorMessage);
                        }
                    }
                }

            }

            ConnectButton2.IsEnabled = true;

        }
        #endregion

        private async Task<bool> ClearBluetoothLEDeviceAsync()
        {

            for (int i = 0; i < subscribedForNotifications.Length; i++)
            {
                if (subscribedForNotifications[i])
                {
                    // Need to clear the CCCD from the remote device so we stop receiving notifications

                    var result = await registeredCharacteristic[i].WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
                    if (result != GattCommunicationStatus.Success)
                    {
                        return false;
                    }
                    else
                    {                        
                        switch (i)
                        {
                            case 0:
                                registeredCharacteristic[0].ValueChanged -= Characteristic_ValueChanged_0;
                                subscribedForNotifications[0] = false;
                                break;
                            case 1:
                                registeredCharacteristic[1].ValueChanged -= Characteristic_ValueChanged_1;
                                subscribedForNotifications[1] = false;
                                break;
                            case 2:
                                registeredCharacteristic[2].ValueChanged -= Characteristic_ValueChanged_2;
                                subscribedForNotifications[2] = false;
                                break;
                        }
                    }

                }
               
            }
            if (bluetoothLeDevice != null)
            {
                for (int j = 0; j < bluetoothLeDevice.Count; j++)
                {
                    bluetoothLeDevice[j]?.Dispose();
                    bluetoothLeDevice[j] = null;
                }
            }
            bluetoothLeDevice = null;
            rootPage.SelectedBleDeviceId = null;
            rootPage.SelectedBleDeviceName = null;
            ConnectButton1.Content = "连接设备1";
            ConnectButton1.IsEnabled = false;
            ConnectButton2.Content = "连接设备2";
            ConnectButton2.IsEnabled = false;
            ConnectButton3.Content = "连接设备3";
            ConnectButton3.IsEnabled = false;

            return true;
        }

        private void StopButton_Click()
        {


            try
            {

                RemoveValueChangedHandler(-1);

                rootPage.NotifyUser("停止读取传感器数据成功", NotifyType.StatusMessage);
                WriteToFile();

            }
            catch (UnauthorizedAccessException ex)
            {
                // This usually happens when a device reports that it support notify, but it actually doesn't.
                rootPage.NotifyUser(ex.Message, NotifyType.ErrorMessage);
            }
        }

        #region Enumerating Characteristics
        private async void ServiceList(IReadOnlyList<GattDeviceService> services,int index)
        {
            GattCharacteristic selectedCharacteristic;           

            var service = services[2];    
            IReadOnlyList<GattCharacteristic> characteristics = null;
            try
            {
                // Ensure we have access to the device.
                var accessStatus = await service.RequestAccessAsync();
                if (accessStatus == DeviceAccessStatus.Allowed)
                {
                    // BT_Code: Get all the child characteristics of a service. Use the cache mode to specify uncached characterstics only 
                    // and the new Async functions to get the characteristics of unpaired devices as well. 
                    var characteristicsResult = await service.GetCharacteristicsAsync(BluetoothCacheMode.Uncached);
                    if (characteristicsResult.Status == GattCommunicationStatus.Success)
                    {
                        characteristics = characteristicsResult.Characteristics;
                    }
                    else
                    {
                        rootPage.NotifyUser("获取服务信息失败.", NotifyType.ErrorMessage);

                        // On error, act as if there are no characteristics.
                        characteristics = new List<GattCharacteristic>();
                    }
                }
                else
                {
                    // Not granted access
                    rootPage.NotifyUser("获取服务信息失败.", NotifyType.ErrorMessage);

                    // On error, act as if there are no characteristics.
                    characteristics = new List<GattCharacteristic>();

                }
            }
            catch (Exception ex)
            {
                rootPage.NotifyUser("服务受限. 无法获得特征信息: " + ex.Message,
                    NotifyType.ErrorMessage);
                // On error, act as if there are no characteristics.
                characteristics = new List<GattCharacteristic>();
            }

            if (characteristics.Count > 0)
            {
                selectedCharacteristic = characteristics[0];
                ValueChangedSubscribe(selectedCharacteristic,index);
            }
            else
            {
                //EnableCharacteristicPanels(GattCharacteristicProperties.None);
                rootPage.NotifyUser("没有获得特征列表", NotifyType.ErrorMessage);
                return;
            }

            // Get all the child descriptors of a characteristics. Use the cache mode to specify uncached descriptors only 
            // and the new Async functions to get the descriptors of unpaired devices as well. 
            var result = await selectedCharacteristic.GetDescriptorsAsync(BluetoothCacheMode.Uncached);
            if (result.Status != GattCommunicationStatus.Success)
            {
                rootPage.NotifyUser("获取描述信息失败: " + result.Status.ToString(), NotifyType.ErrorMessage);
            }
            

        }
        #endregion

        private void AddValueChangedHandler(GattCharacteristic selectedCharacteristic,int index)
        {
            switch (index)
            {
                case 0:
                    if (!subscribedForNotifications[index])
                    {
                        ConnectButton1.Content = "断开设备1";
                        registeredCharacteristic[0]=selectedCharacteristic;
                        selectedCharacteristic.ValueChanged += Characteristic_ValueChanged_0;
                        subscribedForNotifications[index] = true;

                    }
                    break;
                case 1:
                    if (!subscribedForNotifications[index])
                    {
                        ConnectButton2.Content = "断开设备2";
                        registeredCharacteristic[1]= selectedCharacteristic;
                        selectedCharacteristic.ValueChanged += Characteristic_ValueChanged_1;
                        subscribedForNotifications[index] = true;
                    }
                    break;
                case 2:
                    if (!subscribedForNotifications[index])
                    {
                        ConnectButton3.Content = "断开设备3";
                        registeredCharacteristic[2]=selectedCharacteristic;
                        selectedCharacteristic.ValueChanged += Characteristic_ValueChanged_2;
                        subscribedForNotifications[index] = true;
                    }
                    break;
            }
        }

        private void RemoveValueChangedHandler(int index)
        {
            switch (index)
            {
                case 0:
                    if (registeredCharacteristic[0] != null && subscribedForNotifications[0])
                    {
                        ConnectButton1.Content = "连接设备1";
                        registeredCharacteristic[0].ValueChanged -= Characteristic_ValueChanged_0;
                        registeredCharacteristic[0] = null;
                        subscribedForNotifications[0] = false;
                    }
                    break;
                case 1:
                    if (registeredCharacteristic[1] != null && subscribedForNotifications[1])
                    {
                        ConnectButton2.Content = "连接设备2";
                        registeredCharacteristic[1].ValueChanged -= Characteristic_ValueChanged_1;
                        registeredCharacteristic[1] = null;
                        subscribedForNotifications[1] = false;
                    }
                    break;
                case 2:
                    if (registeredCharacteristic[2] != null && subscribedForNotifications[2])
                    {
                        ConnectButton3.Content = "连接设备3";
                        registeredCharacteristic[2].ValueChanged -= Characteristic_ValueChanged_2;
                        registeredCharacteristic[2] = null;
                        subscribedForNotifications[2] = false;
                    }
                    break;
                default:
                    if (registeredCharacteristic[0] != null && subscribedForNotifications[0])
                    {
                        ConnectButton1.Content = "连接设备1";
                        registeredCharacteristic[0].ValueChanged -= Characteristic_ValueChanged_0;
                        registeredCharacteristic[0] = null;
                        subscribedForNotifications[0] = false;
                    }
                    if (registeredCharacteristic[1] != null && subscribedForNotifications[1])
                    {
                        ConnectButton2.Content = "连接设备2";
                        registeredCharacteristic[1].ValueChanged -= Characteristic_ValueChanged_1;
                        registeredCharacteristic[1] = null;
                        subscribedForNotifications[1] = false;
                    }
                    if (registeredCharacteristic[2] != null && subscribedForNotifications[2])
                    {
                        ConnectButton3.Content = "连接设备3";
                        registeredCharacteristic[2].ValueChanged -= Characteristic_ValueChanged_2;
                        registeredCharacteristic[2] = null;
                        subscribedForNotifications[2] = false;
                    }
                    break;
            }

        }

        private bool[] subscribedForNotifications = { false,false,false };        
        private async void ValueChangedSubscribe(GattCharacteristic selectedCharacteristic ,int index)
        {
            if (!subscribedForNotifications[index])
            {
                // initialize status
                GattCommunicationStatus status = GattCommunicationStatus.Unreachable;
                var cccdValue = GattClientCharacteristicConfigurationDescriptorValue.None;
                if (selectedCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Indicate))
                {
                    cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Indicate;
                }

                else if (selectedCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
                {
                    cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Notify;
                }

                try
                {
                    // BT_Code: Must write the CCCD in order for server to send indications.
                    // We receive them in the ValueChanged event handler.
                    status = await selectedCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(cccdValue);

                    if (status == GattCommunicationStatus.Success)
                    {
                        AddValueChangedHandler(selectedCharacteristic,index);
                        rootPage.NotifyUser($"获得设备{index+1}数据成功", NotifyType.StatusMessage);
                    }
                    else
                    {
                        rootPage.NotifyUser($"获得设备{index+1}数据失败: {status}", NotifyType.ErrorMessage);
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    // This usually happens when a device reports that it support indicate, but it actually doesn't.
                    rootPage.NotifyUser(ex.Message, NotifyType.ErrorMessage);
                }
            }
            else
            {
                try
                {
                    // BT_Code: Must write the CCCD in order for server to send notifications.
                    // We receive them in the ValueChanged event handler.
                    // Note that this sample configures either Indicate or Notify, but not both.
                    var result = await
                            selectedCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                                GattClientCharacteristicConfigurationDescriptorValue.None);
                    if (result == GattCommunicationStatus.Success)
                    {
                        RemoveValueChangedHandler(index);
                        subscribedForNotifications[index] = false;                        
                        rootPage.NotifyUser($"停止读取设备{index+1}数据成功", NotifyType.StatusMessage);
                    }
                    else
                    {
                        rootPage.NotifyUser($"停止读取设备{index+1}数据失败: {result}", NotifyType.ErrorMessage);
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    // This usually happens when a device reports that it support notify, but it actually doesn't.
                    rootPage.NotifyUser(ex.Message, NotifyType.ErrorMessage);
                }
            }

        }

        private async void Characteristic_ValueChanged_0(GattCharacteristic sender, GattValueChangedEventArgs args)
        {            
            // BT_Code: An Indicate or Notify reported that the value has changed.
            // Display the new value with a timestamp.
            var newValue = FormatValueByPresentation(args.CharacteristicValue);
            Data data;
            data.dateTime = DateTime.Now;
            data.data = newValue;
            var message = $"设备:{rootPage.SelectedBleDeviceName[0]}     时间 {data.dateTime:hh:mm:ss.FFF}     数值:{data.data}";            
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => CharacteristicLatestValue.Text = message);
            datas[0].Add(data);
            Debug.WriteLine(sender.Uuid.ToString());
        }

        private async void Characteristic_ValueChanged_1(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            // BT_Code: An Indicate or Notify reported that the value has changed.
            // Display the new value with a timestamp.
            var newValue = FormatValueByPresentation(args.CharacteristicValue);
            Data data;
            data.dateTime = DateTime.Now;
            data.data = newValue;
            var message = $"设备:{rootPage.SelectedBleDeviceName[1]}     时间 {data.dateTime:hh:mm:ss.FFF}     数值:{data.data}";
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => CharacteristicLatestValue1.Text = message);
            datas[1].Add(data);
            Debug.WriteLine(sender.Uuid.ToString());
        }
        private async void Characteristic_ValueChanged_2(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            // BT_Code: An Indicate or Notify reported that the value has changed.
            // Display the new value with a timestamp.
            var newValue = FormatValueByPresentation(args.CharacteristicValue);
            Data data;
            data.dateTime = DateTime.Now;
            data.data = newValue;
            var message = $"设备:{rootPage.SelectedBleDeviceName[2]}     时间 {data.dateTime:hh:mm:ss.FFF}     数值:{data.data}";
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => CharacteristicLatestValue2.Text = message);
            datas[2].Add(data);
            Debug.WriteLine(sender.Uuid.ToString());
        }

        private string FormatValueByPresentation(IBuffer buffer)
        {
            // BT_Code: For the purpose of this sample, this function converts only UInt32 and
            // UTF-8 buffers to readable text. It can be extended to support other formats if your app needs them.
            byte[] data;
            CryptographicBuffer.CopyToByteArray(buffer, out data);

            if (data != null)
            {
                return BitConverter.ToString(data);
            }
            else
            {
                return "Unknown format";
            }
        }

        private async void WriteToFile()
        {

            StorageFolder folder = ApplicationData.Current.LocalFolder;
            CharacteristicLatestValue3.Text = folder.Path;
            RadioButton[,] radioButtons = new RadioButton[3,3] { { rbLL1, rbRL1, rbMI1 }, { rbLL2, rbRL2, rbMI2 }, { rbLL3, rbRL3, rbMI3 } };
            string[] position = new string[3];

            for(int i=0;i<3;i++)
            {
                for(int j=0;j<3;j++)
                {
                    if (radioButtons[i,j].IsChecked==true)
                    {
                        position[i] = radioButtons[i, j].Content.ToString();
                    }
                }
            }


            try
            {
                for (int i = 0; i < datas.Length; i++)
                {
                    if (i<rootPage.SelectedBleDeviceId.Count)
                    {
                        StorageFile sfile = await folder.CreateFileAsync($"{i+1}-{tbName.Text}-{position[i]}-传感器{rootPage.SelectedBleDeviceName[i]}数据.csv", CreationCollisionOption.GenerateUniqueName);
                        using (Stream file = await sfile.OpenStreamForWriteAsync())
                        {
                            using (StreamWriter writer = new StreamWriter(file))
                            {
                                writer.WriteLine("time" + "," + "data");
                                foreach (var dt in datas[i])
                                {
                                    writer.WriteLine(dt.dateTime.ToString("hh:mm:ss.FFF") + "," + dt.data);
                                }
                            }
                        }
                    }                   
                    
                }
                rootPage.NotifyUser("文件保存成功.", NotifyType.StatusMessage);
            }

            catch (Exception ex)
            {
                rootPage.NotifyUser("文件保存错误:" + ex.ToString(), NotifyType.ErrorMessage);
            }



        }

    }


    
}
