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
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Java.Util;
using Android.Gms.Tasks;
using BookFace.EventListeners;
using BookFace.Helpers;
using BookFace.Fragments;

namespace BookFace.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class RegistrationActivity : AppCompatActivity
    {
        //Variables
        string fullname, email, password, confirmPass;

        Button registerButton;
        TextInputLayout fullnameText, emailText, passwordText, confirmPasswordText;

        //create instance of Firebase authentication
        FirebaseAuth mAuth;

        //create new instance of our firebase firestore
        FirebaseFirestore database;

        TextView clickHereToLogin;

        TaskCompletionListeners taskCompletionListener = new TaskCompletionListeners();
        ProgressDialogueFragment progressDialogue;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.register);

            fullnameText = (TextInputLayout)FindViewById(Resource.Id.fullNameRegText);
            emailText = (TextInputLayout)FindViewById(Resource.Id.emailRegText);
            passwordText = (TextInputLayout)FindViewById(Resource.Id.passwordRegText);
            confirmPasswordText = (TextInputLayout)FindViewById(Resource.Id.confirmPasswordRegText);
            clickHereToLogin = (TextView)FindViewById(Resource.Id.clickToLogin);

            clickHereToLogin.Click += ClickHereToLogin_Click;

            registerButton = (Button)FindViewById(Resource.Id.registerButton);
            registerButton.Click += RegisterButton_Click;

            //whenver we call the GetFirestore() method, it will return an instance of our firebase firestore.
            database = AppDataHelper.GetFirestore();

            //Instantiate firebase authentication
            mAuth = AppDataHelper.GetFirebaseAuth();
        }

        private void ClickHereToLogin_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(LoginActivity));
            Finish();
        }

        private void RegisterButton_Click(object sender, EventArgs e)
        {
            fullname = fullnameText.EditText.Text;
            email = emailText.EditText.Text;
            password = passwordText.EditText.Text;
            confirmPass = confirmPasswordText.EditText.Text;

            if (fullname.Length < 4)
            {
                Toast.MakeText(this, "Please enter a valid name.", ToastLength.Short).Show();
                return;
            }
            else if (!email.Contains('@'))
            {
                Toast.MakeText(this, "Please enter a valid email address.", ToastLength.Short).Show();
                return;
            }
            else if (password.Length < 8)
            {
                Toast.MakeText(this, "Password must be 8 characters long.", ToastLength.Short).Show();
                passwordText.EditText.Text = "";
                confirmPasswordText.EditText.Text = "";
                return;
            }
            else if (confirmPass != password)
            {
                Toast.MakeText(this, "Password does not match.", ToastLength.Short).Show();
                passwordText.EditText.Text = "";
                confirmPasswordText.EditText.Text = "";
                return;
            }


            //Create user to firebase/REGISTRATION
            ShowProgressDialogue("Registering...");
            mAuth.CreateUserWithEmailAndPassword(email, password)
                .AddOnSuccessListener(this, taskCompletionListener)
                .AddOnFailureListener(this, taskCompletionListener);


            //REGISTRATION SUCCESS CALLBACK
            taskCompletionListener.Success += (success, args) =>
            {
                //add the records of the user
                HashMap userMap = new HashMap();
                userMap.Put("email", email);
                userMap.Put("fullname", fullname);

                //Since the registration is successfull, the current user is not null.
                DocumentReference userReference = database.Collection("users").Document(mAuth.CurrentUser.Uid);

                //Save the usermap to the userReference
                userReference.Set(userMap);
                CloseProgressDialogue();
                StartActivity(typeof(MainActivity));
                Finish();
            };

            //REGISTRATION FAILURE CALLBACK
            taskCompletionListener.Failure += (failure, args) =>
            {
                //Show message why the registration failed
                CloseProgressDialogue();
                Toast.MakeText(this, "Registration failed : " + args.Cause, ToastLength.Short).Show();
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