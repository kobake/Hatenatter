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
using Hatena;

namespace Hatenatter.Droid
{
    //<intent-filter>
    //  <action android:name="android.intent.action.VIEW" />
    //  <category android:name="android.intent.category.DEFAULT" />
    //  <category android:name="android.intent.category.BROWSABLE"/>
    //  <data android:scheme="myapp" android:host="simple" />
    //</intent-filter>


    [
        Activity(
            Label = "Hatenatter",
            Icon = "@drawable/icon",
            Theme = "@style/MainTheme",
            MainLauncher = true,
            ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
            LaunchMode = LaunchMode.SingleTask
        )
    ]
    public class MainActivity : FormsAppCompatActivity
    {
        protected override void OnResume()
        {
            base.OnResume();
            HatenaLogin.Cancel();
        }

        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            Forms.Init(this, bundle);

            UserDialogs.Init(this); // Dialogs初期化
            LoadApplication(new App());
        }
    }

    // verifier受け取り用アクティビティ
    [
        Activity(
            Label = "Hatenatter",
            Icon = "@drawable/icon",
            Theme = "@style/MainTheme",
            MainLauncher = false,
            ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
            LaunchMode = LaunchMode.SingleTop
        )
    ]
    [
        IntentFilter(
            new[] { Intent.ActionView },
            Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
            DataScheme = "hatenatter",
            DataHost = "main2"
        )
    ]
    public class VerifierActivity : Activity
    {
        protected override void OnResume()
        {
            base.OnResume();

            Log.Info("TEST", "================RESTORE=======================");
            string verifier = "";
            if (Intent.ActionView.Equals(Intent.Action))
            {
                Android.Net.Uri uri = Intent.Data;
                if (uri != null)
                {
                    verifier = uri.GetQueryParameter("oauth_verifier");
                }
            }
            if (string.IsNullOrEmpty(verifier))
            {
                HatenaLogin.Cancel();
            }
            else
            {
                HatenaLogin.OnGotVerifier(verifier);
            }
            Log.Info("TEST", "=============================================");

            var intent = new Intent(this, typeof(MainActivity));
            StartActivity(intent);

            this.Finish();
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
        }
    }
}

