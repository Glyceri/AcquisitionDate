using AcquisitionDate.Core.Handlers;
using AcquisitionDate.LodestoneNetworking.Interfaces;
using AcquisitionDate.LodestoneNetworking.Structs;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AcquisitionDate.LodestoneNetworking;

internal class NetworkClient : INetworkClient
{
    private HttpClient HttpClient;
    private readonly CancellationTokenSource CancellationTokenSource;

    private string _sessionToken = string.Empty;

    public NetworkClient()
    {
        HttpClient              = new HttpClient();
        CancellationTokenSource = new CancellationTokenSource();
    }

    private void CreateNewHttpClient()
    {
        HttpClient.Dispose();

        HttpClient = new HttpClient();
    }

    public void RefreshHttpClient()
    {
        CreateNewHttpClient();
    }

    public void CancelPendingRequests()
    {
        try
        {
            HttpClient.CancelPendingRequests();
        }
        catch (Exception e)
        {
            PluginHandlers.PluginLog.Error(e, "Couldn't cancel pending requests.");
        }
    }

    public void SetSessionToken(string sessionToken)
    {
        _sessionToken = sessionToken;
        HttpClient.DefaultRequestHeaders.Remove("Cookie");
        HttpClient.DefaultRequestHeaders.Add("Cookie", $"ldst_sess={sessionToken}");
    }

    public string GetSessionToken()
    {
        return _sessionToken;
    }

    public NetworkClientRequest SendRequest(string URL, Action<HttpResponseMessage> onResponse, Action<Exception> onFailure)
    {
        NetworkClientRequest request = CreateRequest(URL);

        Task.Run(async () => await AsyncSendRequest(request.RequestMessage, onResponse, onFailure), CancellationTokenSource.Token).ConfigureAwait(false);

        return request;
    }

    private NetworkClientRequest CreateRequest(string URL)
    {
        NetworkClientRequest request = new NetworkClientRequest(URL);

        return request;
    }

    private async Task AsyncSendRequest(HttpRequestMessage request, Action<HttpResponseMessage> onResponse, Action<Exception> onFailure)
    {
        HttpResponseMessage? response = null;

        try
        {
            response = await HttpClient.SendAsync(request, CancellationTokenSource.Token).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            onFailure.Invoke(ex);
            return;
        }

        if (response == null)
        {
            onFailure.Invoke(new Exception("Response is NULL."));
            return;
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            onFailure.Invoke(new Exception("Response not found."));
            return;
        }

        onResponse.Invoke(response);
    }

    public void Dispose()
    {
        CancelPendingRequests();

        try
        {
            CancellationTokenSource.Cancel();
            CancellationTokenSource.Dispose();
        }
        catch (Exception e)
        {
            PluginHandlers.PluginLog.Error(e, "Couldn't dispose CancellationTokenSource.");
        }

        try
        {
            HttpClient.Dispose();
        }
        catch (Exception e)
        {
            PluginHandlers.PluginLog.Error(e, "Couldn't dispose HTTP client :c.");
        }
    }
}
