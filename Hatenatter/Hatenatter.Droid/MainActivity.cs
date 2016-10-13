using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Xamarin.Auth;
using System.Json;
using System.Threading.Tasks;
using Android.Util;
using Acr.UserDialogs;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms;
using Android.Content;

namespace Hatenatter.Droid
{
    [
        Activity(
            Label = "Hatenatter",
            Icon = "@drawable/icon",
            Theme = "@style/MainTheme",
            MainLauncher = true,
            ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation
        )
    ]
    [
        IntentFilter(
            new[] { Intent.ActionView },
            Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
            DataScheme = "hatenatter",
            DataHost = "main"
        )
    ]
    //<intent-filter>
    //  <action android:name="android.intent.action.VIEW" />
    //  <category android:name="android.intent.category.DEFAULT" />
    //  <category android:name="android.intent.category.BROWSABLE"/>
    //  <data android:scheme="myapp" android:host="simple" />
    //</intent-filter>
    public class MainActivity : FormsAppCompatActivity
    {
        private Android.Widget.Button m_authButton;

        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            Forms.Init(this, bundle);

            UserDialogs.Init(this); // Dialogs初期化
            LoadApplication(new App());

            // Text="Auth" な Button を探す -> m_authButton
            var content = this.FindViewById<ViewGroup>(Android.Resource.Id.Content);
            scanChildren(content);
            if(m_authButton != null)
            {
                /*
                m_authButton.Click += delegate
                {
                    Android.Util.Log.Info("TEST", "===== Android Button Start.");
                    //LoginToFacebook(true);
                    Android.Util.Log.Info("TEST", "===== Android Button End.");
                };
                */
            }

            Log.Info("TEST", "============= Class.Package.Name = " + this.Class.Package.Name); // "md5342ced9ef53c8f94896788778571b378"
            Log.Info("TEST", "============= PackageResourcePath = " + this.PackageResourcePath); // "/data/app/jp.clockup.hatenatter-2.apk"
            Log.Info("TEST", "============= PackageName = " + this.PackageName); // "jp.clockup.hatenatter
            Log.Info("TEST", "============= Class.Name = " + this.Class.Name); // "md5342ced9ef53c8f94896788778571b378.MainActivity"

        }

        private void scanChildren(ViewGroup viewGroup)
        {
            for (int i = 0; i < viewGroup.ChildCount; i++)
            {
                Android.Views.View v = viewGroup.GetChildAt(i);
                if (v is Android.Widget.Button)
                {
                    var b = v as Android.Widget.Button;
                    Android.Util.Log.Info("TEST", "Button!!" + b.Text);
                    m_authButton = b;
                }
                else if(v is ViewGroup)
                {
                    scanChildren(v as ViewGroup);
                }
            }
        }

        private static readonly TaskScheduler UIScheduler = TaskScheduler.FromCurrentSynchronizationContext();

        void LoginToHatena(bool allowCancel)
        {
            var auth = new OAuth2Authenticator(
                clientId: "1775250636081148", // https://developers.facebook.com/apps
                scope: "",
                authorizeUrl: new Uri("https://m.facebook.com/dialog/oauth/"),
                redirectUrl: new Uri("http://www.facebook.com/connect/login_success.html"));

            auth.AllowCancel = allowCancel;

            // If authorization succeeds or is canceled, .Completed will be fired.
            auth.Completed += (s, ee) => {
                if (!ee.IsAuthenticated)
                {
                    var builder = new AlertDialog.Builder(this);
                    builder.SetMessage("Not Authenticated");
                    builder.SetPositiveButton("Ok", (o, e) => { });
                    builder.Create().Show();
                    return;
                }

                // Now that we're logged in, make a OAuth2 request to get the user's info.
                var request = new OAuth2Request("GET", new Uri("https://graph.facebook.com/me"), null, ee.Account);
                request.GetResponseAsync().ContinueWith(t => {
                    var builder = new AlertDialog.Builder(this);
                    if (t.IsFaulted)
                    {
                        builder.SetTitle("Error");
                        builder.SetMessage(t.Exception.Flatten().InnerException.ToString());
                    }
                    else if (t.IsCanceled)
                        builder.SetTitle("Task Canceled");
                    else
                    {
                        var obj = JsonValue.Parse(t.Result.GetResponseText());

                        builder.SetTitle("Logged in");
                        builder.SetMessage("Name: " + obj["name"]);
                    }

                    builder.SetPositiveButton("Ok", (o, e) => { });
                    builder.Create().Show();
                }, UIScheduler);
            };

            var intent = auth.GetUI(this);
            StartActivity(intent);
        }

        void LoginToFacebook(bool allowCancel)
        {
            var auth = new OAuth2Authenticator(
                clientId: "1775250636081148", // https://developers.facebook.com/apps
                scope: "",
                authorizeUrl: new Uri("https://m.facebook.com/dialog/oauth/"),
                redirectUrl: new Uri("http://www.facebook.com/connect/login_success.html"));

            auth.AllowCancel = allowCancel;

            // If authorization succeeds or is canceled, .Completed will be fired.
            auth.Completed += (s, ee) => {
                if (!ee.IsAuthenticated)
                {
                    var builder = new AlertDialog.Builder(this);
                    builder.SetMessage("Not Authenticated");
                    builder.SetPositiveButton("Ok", (o, e) => { });
                    builder.Create().Show();
                    return;
                }

                // Now that we're logged in, make a OAuth2 request to get the user's info.
                var request = new OAuth2Request("GET", new Uri("https://graph.facebook.com/me"), null, ee.Account);
                request.GetResponseAsync().ContinueWith(t => {
                    var builder = new AlertDialog.Builder(this);
                    if (t.IsFaulted)
                    {
                        builder.SetTitle("Error");
                        builder.SetMessage(t.Exception.Flatten().InnerException.ToString());
                    }
                    else if (t.IsCanceled)
                        builder.SetTitle("Task Canceled");
                    else
                    {
                        var obj = JsonValue.Parse(t.Result.GetResponseText());

                        builder.SetTitle("Logged in");
                        builder.SetMessage("Name: " + obj["name"]);
                    }

                    builder.SetPositiveButton("Ok", (o, e) => { });
                    builder.Create().Show();
                }, UIScheduler);
            };

            var intent = auth.GetUI(this);
            StartActivity(intent);
        }
    }
}

