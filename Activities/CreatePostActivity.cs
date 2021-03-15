using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using BookFace.EventListeners;
using BookFace.Helpers;
using Firebase.Firestore;
using Firebase.Storage;
using Java.Util;
using Plugin.Media;
using BookFace.Fragments;
using System.Globalization;

namespace BookFace.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class CreatePostActivity : AppCompatActivity
    {
        Android.Support.V7.Widget.Toolbar toolbar;
        ImageView postImage;
        Button submitButton;
        byte[] fileBytes;
        EditText postEditText;

        TaskCompletionListeners taskCompletionListeners = new TaskCompletionListeners();
        TaskCompletionListeners downloadUrlListener = new TaskCompletionListeners();
        ProgressDialogueFragment progressDialogue;

        readonly string[] permissionGroup =
        {
            Manifest.Permission.ReadExternalStorage,
            Manifest.Permission.WriteExternalStorage,
            Manifest.Permission.Camera
        };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.create_post);

            toolbar = (Android.Support.V7.Widget.Toolbar)FindViewById(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.Title = "Create Post";

            Android.Support.V7.App.ActionBar actionBar = SupportActionBar;
            actionBar.SetDisplayHomeAsUpEnabled(true);
            actionBar.SetHomeAsUpIndicator(Resource.Drawable.outline_arrowback);

            postEditText = (EditText)FindViewById(Resource.Id.postEditText);

            postImage = (ImageView)FindViewById(Resource.Id.postImage);
            postImage.Click += PostImage_Click;

            submitButton = (Button)FindViewById(Resource.Id.submitButton);
            submitButton.Click += SubmitButton_Click;

            RequestPermissions(permissionGroup, 0);
        }

        private void SubmitButton_Click(object sender, EventArgs e)
        {

            HashMap postMap = new HashMap();
            postMap.Put("author", AppDataHelper.GetFullname());
            postMap.Put("owner_id", AppDataHelper.GetFirebaseAuth().CurrentUser.Uid);

            postMap.Put("post_date", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt"));
            postMap.Put("post_body", postEditText.Text);

            //this will return an instance of our firestore
            //this will also generate an id
            DocumentReference newPostRef = AppDataHelper.GetFirestore().Collection("posts").Document();
            //retrieve the document ID created
            string postKey = newPostRef.Id;
            postMap.Put("image_id", postKey);

            ShowProgressDialogue("Posting..");

            // Save post image to firebase storage
            StorageReference storageReference = null;

            //check if the filebytes is not null
            if (fileBytes != null)
            {
                //This is the location where our images will be uploaded in the Firebase Storage
                //"postImages/" + "photo" is the location + imagename
                storageReference = FirebaseStorage.Instance.GetReference("postImages/" + postKey);

                //this code will save our image file to firebase storage
                storageReference.PutBytes(fileBytes)
                    .AddOnSuccessListener(taskCompletionListeners)
                    .AddOnFailureListener(taskCompletionListeners);
            }
            taskCompletionListeners.Success += (obj, EventArgs args) =>
            {
                //check if storageReference is null
                if (storageReference != null)
                {
                    storageReference.DownloadUrl.AddOnSuccessListener(downloadUrlListener);
                }

                //to get hold of the URL
                downloadUrlListener.Success += (obj, args) =>
                {
                    string downloadUrl = args.Result.ToString();
                    postMap.Put("download_url", downloadUrl);

                    //Save post to Firebase Firestore
                    newPostRef.Set(postMap);

                    CloseProgressDialogue(); 
                    Finish();
                };
            };

            taskCompletionListeners.Failure += (obj, args) =>
            {
                Toast.MakeText(this, "Upload failed!", ToastLength.Short).Show();
                CloseProgressDialogue();
            };
        }

        private void PostImage_Click(object sender, EventArgs e)
        {
            Android.Support.V7.App.AlertDialog.Builder photoAlert = new Android.Support.V7.App.AlertDialog.Builder(this);
            photoAlert.SetMessage("Change Photo");

            photoAlert.SetNegativeButton("Take Photo", (thisalert, args) =>
             {
                 //capture image
                 TakePhoto();
             });

            photoAlert.SetPositiveButton("Upload Photo", (thisAlert, args) =>
             {
                 //Choose image
                 SelectPhoto();
             });

            photoAlert.Show();
        }       

        async void TakePhoto()
        {
            await CrossMedia.Current.Initialize();
            var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                //Attributes of the photo to be captured
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium,
                CompressionQuality = 20,
                Directory = "Sample",
                Name = GenerateRandomString(6) + "rojohnPogi.jpg"
            });

            //check if an image was captured
            if (file == null)
            {
                return;
            }

            //Converts file.path to byte array and set the resulting bitmap to imageview 
            byte[] imageArray = System.IO.File.ReadAllBytes(file.Path);
            fileBytes = imageArray;

            //convert the byte array into a bitmap object
            Bitmap bitmap = BitmapFactory.DecodeByteArray(imageArray, 0, imageArray.Length);
            postImage.SetImageBitmap(bitmap);
        }
         
        async void SelectPhoto() 
        {
            await CrossMedia.Current.Initialize();

            //to check if the device is supported to pick photos
            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                Toast.MakeText(this, "Upload not supported", ToastLength.Short).Show();
                return;
            }

            var file = await CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
            {
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium,
                CompressionQuality = 30,
            });

            //to check if the user selected a photo in the device
            if (file == null)
            {
                return;
            }

            //Convert the image to byteArray and decode it to Bitmap image
            byte[] imageArray = System.IO.File.ReadAllBytes(file.Path);
            fileBytes = imageArray;

            Bitmap bitmap = BitmapFactory.DecodeByteArray(imageArray, 0, imageArray.Length);
            postImage.SetImageBitmap(bitmap);
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

        string GenerateRandomString(int length)
        {
            System.Random rand = new System.Random();
            char[] allchars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
            string sResult = "";

            for (int i = 0; i < length; i++)
            {
                sResult += allchars[rand.Next(0, allchars.Length)];
            }
            return sResult;
        }
        

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}