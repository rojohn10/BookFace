using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Gms.Tasks;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BookFace.DataModels;
using BookFace.Helpers;
using Firebase.Firestore;

namespace BookFace.EventListeners
{
    public class PostEventListener : Java.Lang.Object, IOnSuccessListener, IEventListener
    {
        public List<Post> ListOfPost = new List<Post>();

        public event EventHandler<PostEventArgs> OnPostRetrieved;

        public class PostEventArgs : EventArgs
        {
            public List<Post> Posts { get; set; }
        }
        public void FetchPost()
        {
            //Retrieve only once
            // AppDataHelper.GetFirestore().Collection("posts").Get()
            //    .AddOnSuccessListener(this);

            AppDataHelper.GetFirestore().Collection("posts").AddSnapshotListener(this);
        }

        public void RemoveListener()
        {
            var listener = AppDataHelper.GetFirestore().Collection("posts").AddSnapshotListener(this);
            listener.Remove();
        }


        public void OnSuccess(Java.Lang.Object result)
        {
            OrganizedData(result);
        }

        public void OnEvent(Java.Lang.Object value, FirebaseFirestoreException error)
        {
            OrganizedData(value);
        }

        void OrganizedData(Java.Lang.Object value)
        {
            var snapshot = (QuerySnapshot)value; 

            if (!snapshot.IsEmpty)
            {
                if (ListOfPost.Count > 0)
                {  
                    ListOfPost.Clear(); 
                }
                   
                foreach (DocumentSnapshot item in snapshot.Documents) 
                {  
                    Post post = new Post();
                    post.ID = item.Id;
                    post.PostBody = item.Get("post_body") != null ? item.Get("post_body").ToString() : "";
                    post.Author = item.Get("author") != null ? item.Get("author").ToString() : "";
                    post.ImageId = item.Get("image_id") != null ? item.Get("image_id").ToString() : "";
                    post.OwnerId = item.Get("owner_id") != null ? item.Get("owner_id").ToString() : "";
                    post.DownloadUrl = item.Get("download_url") != null ? item.Get("download_url").ToString() : "";

                    string datestring = item.Get("post_date") != null ? item.Get("post_date").ToString() : "";

                    post.PostDate =  DateTime.ParseExact(datestring, @"dd/MM/yyyy HH:mm:ss tt",
                        System.Globalization.CultureInfo.InvariantCulture);

                    var data = item.Get("likes") != null ? item.Get("likes") : null;

                    if (data != null)
                    {
                        var dictionaryFromHashMap = new Android.Runtime.JavaDictionary<string, string>(data.Handle, JniHandleOwnership.DoNotRegister);

                        //retrieve our own id
                        string uid = AppDataHelper.GetFirebaseAuth().CurrentUser.Uid;

                        //check the number of users liked the post
                        post.LikeCount = dictionaryFromHashMap.Count;

                        //verifies if we liked the post or not.
                        if (dictionaryFromHashMap.Contains(uid))
                        {
                            post.Liked = true;
                        }
                    }
                     
                    ListOfPost.Add(post);
                }

                OnPostRetrieved?.Invoke(this, new PostEventArgs { Posts = ListOfPost });
            }
        }
    }
}