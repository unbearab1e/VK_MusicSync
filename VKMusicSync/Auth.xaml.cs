using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;

namespace VKMusicSync
{

    public partial class FBAuthentication : Elysium.Controls.Window
    {
        public string AccessToken
        {
            set;
            get;
        }

        public string UserID
        {
            set;
            get;
        }

        public string DBUserToken
        {
            get;
            set;
        }
        public string DBUserSecret
        {
            get;
            set;
        }

        private Boolean isDropboxAuth = false;
        private Boolean exitCode = false;

        public Boolean ExitCode
        {
            get { return exitCode; }
        }

        public FBAuthentication(string AppID, string scope)
        {
            InitializeComponent();
            var navigationUrl = "https://oauth.vk.com/authorize?client_id=" + AppID + "&redirect_uri=http://oauth.vk.com/blank.html&display=popup&response_type=token&scope=" + scope;
            browser.ScriptErrorsSuppressed = true;
            browser.Navigate(new Uri(navigationUrl).AbsoluteUri);
        }

        public FBAuthentication(Uri DBUri)
        {
            InitializeComponent();
            browser.ScriptErrorsSuppressed = true;
            this.isDropboxAuth = true;
            browser.Navigate(DBUri);
        }

        private void browser_Navigated(object sender, System.Windows.Forms.WebBrowserNavigatedEventArgs e)
        {
            this.addressTextBox.Text = e.Url.ToString();

            if (this.isDropboxAuth == false)
            {
                if (this.addressTextBox.Text.StartsWith("http://oauth.vk.com/blank.html") || this.addressTextBox.Text.StartsWith("https://oauth.vk.com/blank.html"))
                {
                    string queryString = e.Url.Fragment;
                    string[] parameters = queryString.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string parameter in parameters)
                    {
                        List<string> parameterList = parameter.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                        if (parameterList.ElementAt(0).Equals("#access_token"))
                        {
                            AccessToken = parameterList.ElementAt(1);
                        }
                        else if (parameterList.ElementAt(0).Equals("user_id"))
                        {
                            UserID = parameterList.ElementAt(1);
                        }
                    }
                    exitCode = true;
                    this.Close();
                }
            }
            else
            {
                if (this.addressTextBox.Text=="http://www.dropbox.com/1/oauth/authorize" || this.addressTextBox.Text == "https://www.dropbox.com/1/oauth/authorize")
                {
                    exitCode = true;
                    this.Close();
                }
            }
        }
    }
}
