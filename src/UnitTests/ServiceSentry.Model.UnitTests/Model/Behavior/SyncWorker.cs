// -----------------------------------------------------------------------
//  <copyright file="SyncWorker.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Generic;
using System.ComponentModel;
using ServiceSentry.Common;

#endregion

namespace ServiceSentry.Model.UnitTests.Model.Behavior
{
    internal sealed class SyncWorker : AsyncWorker
    {
        new public static SyncWorker Default
        {
            get { return new SyncWorker(); }
        }

        public override void Run(DoWorkEventHandler doWork, RunWorkerCompletedEventHandler onComplete,
                                 ProgressChangedEventHandler progressChanged, List<object> arguments = null)
        {
            Exception error = null;
            var args = new DoWorkEventArgs(arguments);
            try
            {
                doWork(this, args);
            }
            catch (Exception ex)
            {
                error = ex;
                throw;
            }
            finally
            {
                onComplete(this, new RunWorkerCompletedEventArgs(args.Result, error, args.Cancel));
            }
        }

        public override void ReportProgress(int percentProgress, object userState)
        {
            //
        }
    }
}