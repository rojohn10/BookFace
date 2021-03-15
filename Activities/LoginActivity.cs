using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using BookFace.EventListeners;
using BookFace.Fragments;
using BookFace.Helpers;
using Firebase.Auth;

namespace BookFace.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class LoginActivity : AppCompatActivity
    {
        TextInputLayout emailText, passwordText;
        Button loginButton;
        FirebaseAuth mAuth;

        TextView clickToRegister;
        ProgressDialogueFragment progressDialogue;
        TaskCompletionListeners taskCompletionListeners = new TaskCompletionListeners();
         
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.login);

            emailText = (TextInputLayout)FindViewById(Resource.Id.emailLoginText);
            passwordText = (TextInputLayout)FindViewById(Resource.Id.passwordLoginText);
            loginButton = (Button)FindViewById(Resource.Id.loginButton);

            clickToRegister = (TextView)FindViewById(Resource.Id.clickToRegister);

            clickToRegister.Click += ClickToRegister_Click;
            //Return an instance of our FirebaseAuth
            mAuth = AppDataHelper.GetFirebaseAuth();

            loginButton.Click += LoginButton_Click;
        }

        private void ClickToRegister_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(RegistrationActivity));
            Finish();
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            string email, password;
            email = emailText.EditText.Text;
            password = passwordText.EditText.Text;

            if (!email.Contains ('@'))
            {
                Toast.MakeText(this, "Please enter valid email.", ToastLength.Short).Show();
                 
            }
            else if (password.Length < 8)
            {
                Toast.MakeText(this, "Password must be 8 characters long.", ToastLength.Short).Show();
            }
             
            ShowProgressDialogue("Verifying user"); 
            mAuth.SignInWithEmailAndPassword(email, password)
                .AddOnSuccessListener(taskCompletionListeners)
                .AddOnFailureListener(taskCompletionListeners);

            taskCompletionListeners.Success += (success, args) =>
            {
                CloseProgressDialogue();
                StartActivity(typeof(MainActivity));
                Finish(); 
            };

            taskCompletionListeners.Failure += (failure, args) =>
            {
                CloseProgressDialogue();
                Toast.MakeText(this, "Login Failed!" + args.Cause, ToastLength.Short).Show();
            };
        }

        void ShowProgressDialogue(string status)
        {
            progressDialogue = new ProgressDialogueFragment(status);
            var trans = SupportFragmentManager.BeginTransaction();
            progressDialogue.Cancelable = false;
            progressDialogue.Show(trans, "Progress");
        }

        void CloseProgressDialogue()
        {
            if (progressDialogue != null)
            {
                progressDialogue.Dismiss();
                progressDialogue = null;
            }
            
        }
    }
}