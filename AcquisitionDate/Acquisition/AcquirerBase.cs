﻿using AcquisitionDate.Acquisition.Interfaces;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.LodestoneNetworking.Interfaces;
using AcquisitionDate.LodestoneNetworking.Queue.Interfaces;
using AcquisitionDate.LodestoneRequests.Interfaces;
using System.Collections.Generic;

namespace AcquisitionDate.Acquisition;

internal abstract class AcquirerBase : IAcquirer
{
    public bool IsAcquiring { get; protected set; }
    public byte CompletionRate { get; protected set; }

    protected readonly ILodestoneNetworker Networker;

    protected IDatableData _currentUser;

    List<ILodestoneQueueElement> queueElements = new List<ILodestoneQueueElement>();

    public AcquirerBase(ILodestoneNetworker networker)
    {
        Networker = networker;
    }

    public void Acquire(IDatableData user)
    {
        _currentUser = user;
        ReadyAcquire();
        OnAcquire();
    }

    public void Cancel()
    {
        HandleCancel();
        OnCancel();
    }

    public void Dispose()
    {
        ResetAcquire();
        OnDispose();
    }

    protected abstract void OnAcquire();
    protected abstract void OnCancel();
    protected abstract void OnDispose();

    protected void Failed()
    {
        IsAcquiring = false;
    }

    protected void Success()
    {
        IsAcquiring = false;
        CompletionRate = byte.MaxValue;
    }

    protected void ResetAcquire()
    {
        InternalReset();
        HandleCancel();
    }

    protected void ReadyAcquire()
    {
        ResetAcquire();
        IsAcquiring = true;
    }

    protected void HandleCancel()
    {
        foreach (ILodestoneQueueElement request in queueElements)
        {
            if (request.DISPOSED) continue;

            request.Cancel();
        }

        queueElements.Clear();
        InternalReset();
    }

    void InternalReset()
    {
        IsAcquiring = false;
        CompletionRate = 0;
    }

    protected void SetPercentage(float percentage)
    {
        if (percentage <= 0) CompletionRate = 0;
        if (percentage >= 100) CompletionRate = byte.MaxValue;

        float percentageAmount = percentage * 0.01f;

        float percentageByte = byte.MaxValue * percentageAmount;

        CompletionRate = (byte)percentageByte;
    }

    protected void RegisterQueueElement(ILodestoneRequest request)
    {
        queueElements.Add(Networker.AddElementToQueue(request));
    }
}
