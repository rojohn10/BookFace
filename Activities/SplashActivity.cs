using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using BookFace.Helpers;
using Firebase.Auth;

namespace BookFace.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/MyTheme.Splash", MainLauncher = true)]
    public class SplashActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
        }

        protected override void OnResume()
        {
            base.OnResume();

            //this will return an instance of firebase user
            FirebaseUser currentUser = AppDataHelper.GetFirebaseAuth().CurrentUser;

            //If current user is not null, the user is already logged in
            if (currentUser != null)
            {
                StartActivity(typeof(MainActivity));
                Finish();
            }
            else if (currentUser == null)
            {
                StartActivity(typeof(LoginActivity));
            }
        }
    }
}