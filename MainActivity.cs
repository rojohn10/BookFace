using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Android.Views;
using Android.Support.V7.Widget;
using BookFace.Adapter;
using System.Collections.Generic;
using BookFace.DataModels;
using BookFace.Activities;
using BookFace.EventListeners;
using System.Linq;
using BookFace.Helpers;
using Firebase.Storage;
using BookFace.Fragments;

namespace BookFace
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class MainActivity : AppCompatActivity
    {
        Android.Support.V7.Widget.Toolbar toolbar;
        RecyclerView postRecyclerView;
        PostAdapter postAdapter; 
        List<Post> ListOfPost;
        RelativeLayout layStatus;
        PostEventListener postEventListener;

        ImageView camera;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);        
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            toolbar = (Android.Support.V7.Widget.Toolbar)FindViewById(Resource.Id.toolbar);
            //Setup the toolbar
            SetSupportActionBar(toolbar);
            postRecyclerView = (RecyclerView)FindViewById(Resource.Id.postRecyclerView);
            layStatus = (RelativeLayout)FindViewById(Resource.Id.layStatus);

            camera = (ImageView)FindViewById(Resource.Id.camera);
            camera.Click += LayStatus_Click;
            layStatus.Click += LayStatus_Click;

            //retrieve fullname upon login
            FullnameListener fullnameListener = new FullnameListener();
            fullnameListener.FetchUser();

            FetchPost();          
        }

        private void LayStatus_Click(object sender, System.EventArgs e)
        {
            StartActivity(typeof(CreatePostActivity));
        }

        void FetchPost()
        {
            postEventListener = new PostEventListener();
            postEventListener.FetchPost();
            postEventListener.OnPostRetrieved += PostEventListener_OnPostRetrieved;

        }
         
        private void PostEventListener_OnPostRetrieved(object sender, PostEventListener.PostEventArgs e)
        {
            ListOfPost = new List<Post>();
            ListOfPost = e.Posts;

            if (ListOfPost != null)
            {
                ListOfPost = ListOfPost.OrderByDescending(x => x.PostDate).ToList();
            }
            SetupRecyclerView();
        }

        void SetupRecyclerView()
        {
            postRecyclerView.SetLayoutManager(new Android.Support.V7.Widget.LinearLayoutManager(postRecyclerView.Context));
            postAdapter = new PostAdapter(ListOfPost);
            postRecyclerView.SetAdapter(postAdapter);
            postAdapter.ItemLongClick += PostAdapter_ItemLongClick;
            postAdapter.LikeClick += PostAdapter_LikeClick;
        }

        private void PostAdapter_LikeClick(object sender, PostAdapterClickEventArgs e)
        {
            //Object of the post that we will like or unlike
            Post post = ListOfPost[e.Position];
            LikeEventListener likeEventListener = new LikeEventListener(post.ID);

            if (!post.Liked)
            {
                likeEventListener.LikePost(); 
            }
            else
            {
                likeEventListener.UnlikePost();
            }
        }

        private void PostAdapter_ItemLongClick(object sender, PostAdapterClickEventArgs e)
        {
            string postID = ListOfPost[e.Position].ID;
            string ownerID = ListOfPost[e.Position].OwnerId; 

            //check if the current user id matches the owner ID of the post
            if (AppDataHelper.GetFirebaseAuth().CurrentUser.Uid == ownerID)
            {
                Android.Support.V7.App.AlertDialog.Builder alert = new Android.Support.V7.App.AlertDialog.Builder(this);
                alert.SetTitle("Edit or Delete post");
                alert.SetMessage("Are you sure?");

                //Edit post on Firestore.
                alert.SetNegativeButton("Edit post", (o, args) =>
                {
                    EditPostFragment editPostFragment = new EditPostFragment(ListOfPost[e.Position]);
                    var trans = SupportFragmentManager.BeginTransaction();
                    editPostFragment.Show(trans, "edit");
                });

                alert.SetPositiveButton("Delete", (o, args) =>
                 {
                     //delete post
                     AppDataHelper.GetFirestore().Collection("posts").Document(postID).Delete();

                     //delete image with the current post
                     StorageReference storageReference = FirebaseStorage.Instance.GetReference("postImages/" + postID);
                     storageReference.Delete();
                 });

                alert.Show();
            }
        }
         
        //This method enables us to inflate our menu  
        public override bool OnCreateOptionsMenu(IMenu menu)
        {

            MenuInflater.Inflate(Resource.Menu.feeds_menu, menu);
            return true;
        }

        //Whenever the user clicks the menu, this item will be called
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;

            if (id == Resource.Id.action_logout)
            {
                postEventListener.RemoveListener();
                AppDataHelper.GetFirebaseAuth().SignOut();
                StartActivity(typeof(LoginActivity)); 
                Finish();
            }

            if (id == Resource.Id.action_refresh)
            {
                Toast.MakeText(this, "Refresh was clicked", ToastLength.Short).Show();
            }

            return base.OnOptionsItemSelected(item);
        }
    }
} 