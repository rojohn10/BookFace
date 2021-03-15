﻿using System;
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

namespace BookFace.EventListeners
{
    class TaskCompletionListeners : Java.Lang.Object, IOnSuccessListener, IOnFailureListener
    {

        //Event Handlers
        public event EventHandler<TaskSuccessEventArgs> Success;
        public event EventHandler<TaskCompletionFailureEventArgs> Failure;

        //Event types / EventArgs
        public class TaskCompletionFailureEventArgs : EventArgs
        {
            public string Cause { get; set; }
        }

        public class TaskSuccessEventArgs : EventArgs
        {
            public Java.Lang.Object Result { get; set; }
        }

        public void OnFailure(Java.Lang.Exception e)
        {
            Failure?.Invoke(this, new TaskCompletionFailureEventArgs { Cause = e.Message });
        }

        public void OnSuccess(Java.Lang.Object result)
        {
            Success?.Invoke(this, new TaskSuccessEventArgs { Result = result });
        }
    }
}