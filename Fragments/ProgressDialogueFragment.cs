﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace BookFace.Fragments
{
    public class ProgressDialogueFragment : Android.Support.V4.App.DialogFragment
    {
        string status;
        public ProgressDialogueFragment(string _status)
        {
            status = _status;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.progress, container, false);

            TextView statusTextview = (TextView)view.FindViewById(Resource.Id.progressStatus);
            statusTextview.Text = status;
            return view;
        }
    }
}