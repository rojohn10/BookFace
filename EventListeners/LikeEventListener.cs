using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Gms.Tasks;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BookFace.Helpers;
using Firebase.Firestore;

namespace BookFace.EventListeners
{
    public class LikeEventListener : Java.Lang.Object, IOnSuccessListener
    {
        string postID;
        bool like;

        public LikeEventListener(string _postID)
        {
            postID = _postID;
        }

        public void LikePost()
        {
            like = true;
            AppDataHelper.GetFirestore().Collection("posts").Document(postID).Get()
                .AddOnSuccessListener(this);

        }

        public void UnlikePost()
        {
            like = false;
            AppDataHelper.GetFirestore().Collection("posts").Document(postID).Get()
                .AddOnSuccessListener(this);
        }

        public void OnSuccess(Java.Lang.Object result)
        {
            DocumentSnapshot snapshot = (DocumentSnapshot)result;

            if (!snapshot.Exists())
            {
                return;
            }
            
            DocumentReference likeReference = AppDataHelper.GetFirestore().Collection("posts").Document(postID);

            if (like)
            {
                
                likeReference.Update("likes." + AppDataHelper.GetFirebaseAuth().CurrentUser.Uid, true);
            }
            else
            {
                //check for null
                if(snapshot.Get("likes") == null)
                {
                    return;
                }

                var data = snapshot.Get("likes") != null ? snapshot.Get("likes") : null;

                if (data != null)
                {
                    var dictionaryFromHashMap = new Android.Runtime.JavaDictionary<string, string>(data.Handle, JniHandleOwnership.DoNotRegister);

                    //retrieve our own id
                    string uid = AppDataHelper.GetFirebaseAuth().CurrentUser.Uid;

                    //check if our user ID is contained inside the dictionary
                    if (dictionaryFromHashMap.Contains(uid))
                    {
                        //remove our user ID to unlike the post
                        dictionaryFromHashMap.Remove(uid);

                        //update the hashmap withour our userid included
                        likeReference.Update("likes", dictionaryFromHashMap);
                    }
                }
            }
        }
    }
}