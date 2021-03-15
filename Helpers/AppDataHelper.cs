using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;

namespace BookFace.Helpers
{
    public static class AppDataHelper
    {

        static ISharedPreferences preferences = Application.Context.GetSharedPreferences("userinfo", FileCreationMode.Private);
        static ISharedPreferencesEditor editor;

        //This will return an instance of our firestore database
        public static FirebaseFirestore GetFirestore()
        {

            FirebaseFirestore database;

            //Initialize firestore
            var app = FirebaseApp.InitializeApp(Application.Context);

            //to check if the firebase has been initialized
            if (app == null)
            {
                //create firbase options
                //we are going to use the builder to force the firebase to initialize
                var options = new FirebaseOptions.Builder()
                    .SetProjectId("bookface-f0b7d")
                    .SetApplicationId("bookface-f0b7d")
                    .SetApiKey("AIzaSyBVPmz1ACSUFGfqUuJYCtuqn5qm0rqMQOg")
                    .SetDatabaseUrl("https://bookface-f0b7d.firebaseio.com")
                    .SetStorageBucket("bookface-f0b7d.appspot.com")
                    .Build();

                //this will initialize our database
                app = FirebaseApp.InitializeApp(Application.Context, options);
                database = FirebaseFirestore.GetInstance(app);
            }
            else
            {
                //if the database was automatically initialized
                database = FirebaseFirestore.GetInstance(app);
            }

            return database;
        }

        //This will return an instance of our firestore database
        public static FirebaseAuth GetFirebaseAuth()
        {
            FirebaseAuth mAuth;

            //Initialize firestore
            var app = FirebaseApp.InitializeApp(Application.Context);

            //to check if the firebase has been initialized
            if (app == null)
            {
                //create firbase options
                //we are going to use the builder to force the firebase to initialize
                var options = new FirebaseOptions.Builder()
                    .SetProjectId("bookface-f0b7d")
                    .SetApplicationId("bookface-f0b7d")
                    .SetApiKey("AIzaSyBVPmz1ACSUFGfqUuJYCtuqn5qm0rqMQOg")
                    .SetDatabaseUrl("https://bookface-f0b7d.firebaseio.com")
                    .SetStorageBucket("bookface-f0b7d.appspot.com")
                    .Build();

                //this will initialize our database
                app = FirebaseApp.InitializeApp(Application.Context, options);
                mAuth = FirebaseAuth.Instance;
            }
            else
            {
                //if the database was automatically initialized
                mAuth = FirebaseAuth.Instance;
            }

            return mAuth;
        }

        //Save fullname to our shared preferences
        public static void SaveFullName(string fullname)
        {
            editor = preferences.Edit();
            editor.PutString("fullname", fullname);
            editor.Apply();
        }

        //Retrieve the fullname
        public static string GetFullname()
        {
            string fullname = "";
            fullname = preferences.GetString("fullname", "");
            return fullname;
        }
    }
}