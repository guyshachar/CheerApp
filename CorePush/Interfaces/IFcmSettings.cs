﻿namespace CorePush.Interfaces.Google
{
    public interface IFcmSettings
    {
        string SenderId { get; }
        string ServerKey { get; }
    }
}