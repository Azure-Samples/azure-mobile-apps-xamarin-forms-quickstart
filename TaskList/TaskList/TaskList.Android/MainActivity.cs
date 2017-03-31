using Android.App;
using Android.Content.PM;
using Android.OS;

namespace TaskList.Droid
{
    [Activity (Label = "TaskList.Droid",
        Icon = "@drawable/icon",
        Theme="@android:style/Theme.Holo.Light",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate(bundle);

            // Initialize Azure Mobile Apps
            Microsoft.WindowsAzure.MobileServices.CurrentPlatform.Init();

            // Initialize Xamarin Forms
			global::Xamarin.Forms.Forms.Init(this, bundle);

            // Load the main Application
			LoadApplication(new TaskList.App());
		}
	}
}

