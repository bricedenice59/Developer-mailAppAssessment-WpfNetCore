﻿using System;
using System.Windows.Threading;

namespace DeveloperTest.Utils.WPF;

//https://stackoverflow.com/questions/72283485/migrating-dispatcherhelper-from-mvvmlight-to-windows-community-toolkit
public static class DispatcherHelper
{
    public static Dispatcher UIDispatcher { get; private set; }

    private static void CheckDispatcher()
    {
        if (UIDispatcher == null)
            throw new InvalidOperationException("The DispatcherHelper is not initialized.\n" +
                                                "Call DispatcherHelper.Initialize() in the static App constructor.");
    }

    public static DispatcherOperation RunAsync(Action action)
    {
        CheckDispatcher();
        return UIDispatcher.BeginInvoke(action);
    }

    public static void Initialize()
    {
        if (UIDispatcher != null && UIDispatcher.Thread.IsAlive)
            return;

        UIDispatcher = Dispatcher.CurrentDispatcher;
    }

    public static void Reset()
    {
        UIDispatcher = null;
    }
}
