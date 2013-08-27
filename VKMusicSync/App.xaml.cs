using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace VKMusicSync
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public sealed partial class App : Application
    {
        private String appID = "3704292";
        private String scope = "audio";
        private String DBAppKey = "8e8e20nzzbzxyo0";
        private String DBAppSecret = "we0vhahk88vqny0";
        public String AppID
        {
            get { return appID; }
        }
        public String Scope
        {
            get { return scope; }
        }
        public VKAPI VK;
        public DropNet.DropNetClient DB;
        private void StartupHandler(object sender, System.Windows.StartupEventArgs e)
        {
            VK = new VKAPI(appID,scope);
            DB = new DropNet.DropNetClient(DBAppKey, DBAppSecret);
            Elysium.Manager.Apply(this, Elysium.Theme.Dark, System.Windows.Media.Brushes.Blue, System.Windows.Media.Brushes.White);
        }
    }
}
