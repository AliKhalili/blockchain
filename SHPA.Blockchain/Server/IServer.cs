﻿using System;
using System.Threading;

namespace SHPA.Blockchain.Server
{
    public interface IServer:IDisposable
    {
        void Start(CancellationToken cancellationToken);
        void Stop();
    }
}