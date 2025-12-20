using AcquisitionDate.LodestoneNetworking.Structs;
using System;
using System.Net.Http;

namespace AcquisitionDate.LodestoneNetworking.Interfaces;

internal interface INetworkClient : IDisposable
{
    void CancelPendingRequests();

    void SetSessionToken(string sessionToken);
    string GetSessionToken();

    NetworkClientRequest SendRequest(string URL, Action<HttpResponseMessage> onResponse, Action<Exception> onFailure);

    void RefreshHttpClient();
}
