using System;
using System.Net.Http;

namespace AcquisitionDate.LodestoneNetworking.Structs;

internal class NetworkClientRequest : IDisposable
{
    public readonly HttpRequestMessage RequestMessage;

    public NetworkClientRequest(string URL)
    {
        RequestMessage = new HttpRequestMessage(HttpMethod.Get, URL);
    }

    public void Dispose()
    {
        RequestMessage.Dispose();
    }
}
