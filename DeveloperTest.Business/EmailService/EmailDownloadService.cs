using System.Collections.Concurrent;
using System.Diagnostics;
using DeveloperTest.EmailDiscovery.ConnectionService;
using DeveloperTest.EmailDiscovery.Factory;
using DeveloperTest.EmailDiscovery.Utils;
using DeveloperTest.Helpers.Events;
using DeveloperTest.Helpers.Models;
using Limilabs.Client.IMAP;
using Limilabs.Client.POP3;
using Limilabs.Mail;
using Microsoft.Extensions.Logging;
using static System.Net.Mime.MediaTypeNames;

namespace DeveloperTest.EmailDiscovery.EmailService;

public class EmailDownloadService : IEmailDownloadService
{
    public event EventHandler<ScanEmailsStatusChangedEventArgs> ScanEmailsStatusChanged;
    public event EventHandler<NewEmailDiscoveredEventArgs> NewEmailDiscovered;
    private readonly ILogger _logger;
    private readonly IEmailConnectionInteractions _connectionUtils;
    private readonly IConnectionPoolInstance _connectionPoolInstance;
    private readonly IEmailInboxFactory _emailInboxFactory;
    private IEmailInboxInteractions _inboxInteractions;
    private static object _lockEmailBodyProcess = new object();
    private int _nbProcessedHeaders;
    private int _nbProcessedBodies;
    private SemaphoreSlim _semaphoreSlim;

    //contains the list of body uids that have been already downloaded, this property is used to avoid processing body download more than once
    public ConcurrentBag<string> ProcessedBodies { get; set; }

    public EmailDownloadService(ILogger<EmailDownloadService> logger,
        IEmailConnectionInteractions connectionUtils,
        IConnectionPoolInstance connectionPoolInstance,
        IEmailInboxFactory emailInboxFactory)
    {
        _logger = logger;
        _connectionUtils = connectionUtils;
        _connectionPoolInstance = connectionPoolInstance;
        _emailInboxFactory = emailInboxFactory;
        ProcessedBodies = new ConcurrentBag<string>();
    }

    /// <summary>
    /// Download emails 
    /// </summary>
    /// <returns></returns>
    public async Task DownloadEmails()
    {
        _logger.LogInformation("Start Download headers...");
        var connections = _connectionPoolInstance.Connections;
        if (connections.Count == 0)
        {
            _logger.LogError("No connection exist");
            throw new Exception("Cannot start download headers as no connection exist!");
        }

        _inboxInteractions = _emailInboxFactory.GetInstance(connections[0].ProtocolType);

        //Select Inbox for all existing connections
        foreach (var connection in connections)
        {
            _logger.LogInformation($"Select Inbox for Connection id {connection.ConnectionId}...");
            if (await _inboxInteractions.SelectInboxAsync(connection))
            {
                // at that stage we have a valid connection ready for downloading emails
                //add it in the connection pool
                _connectionPoolInstance.Enqueue(connection);
            }
        }

        _logger.LogInformation($"Get emails uids");
        var uids = await _inboxInteractions.GetAllEmailsUIds(connections[0]);
        _logger.LogInformation($"Found {uids.Count} emails to download");

        ScanEmailsStatusChanged?.Invoke(this, new ScanEmailsStatusChangedEventArgs(ScanProgress.InProgress));
        _semaphoreSlim = new SemaphoreSlim(connections.Count);
        await ProcessDownloadHeadersAndBodies(uids);
        ScanEmailsStatusChanged?.Invoke(this, new ScanEmailsStatusChangedEventArgs(ScanProgress.Completed));

        _logger.LogInformation($"{_nbProcessedHeaders} email headers have been successfully downloaded");
        _logger.LogInformation($"{_nbProcessedBodies} email bodies have been successfully downloaded");
    }

    /// <summary>
    /// This is a method that downloads emails headers and bodies concurrently and by using the maximum connections available
    /// </summary>
    /// <param name="uids">list of emails ids to proceed with</param>
    /// <returns></returns>
    private async Task ProcessDownloadHeadersAndBodies(List<string> uids)
    {
        var tasksDownload = uids
            .Select(x => Task.Run(async () =>
            {
                await _semaphoreSlim.WaitAsync();

                var availableCnx = _connectionPoolInstance.GetOneAvailable();
                if (availableCnx == null)
                {
                    //that should never happen since the semaphore initial count = number of connections in pool
                    _logger.LogInformation("No connection available");
                    return;
                }

                Debug.WriteLine($"Connection id {availableCnx.ConnectionId} taken");

                var email = await _inboxInteractions.DownloadHeaderAsync(x, availableCnx);
                Interlocked.Increment(ref _nbProcessedHeaders);

                //send downloaded header to UI
                if (email != null)
                    NewEmailDiscovered?.Invoke(this, new NewEmailDiscoveredEventArgs(email));

                await DownloadAndSetBodyAsync(email!, availableCnx);
                Interlocked.Increment(ref _nbProcessedBodies);

                //free connection and re-enqueue it
                _connectionPoolInstance.Enqueue(availableCnx);

                _semaphoreSlim.Release();
            }))
            .ToArray();

        await Task.WhenAll(tasksDownload);
    }

    public async Task DownloadAndSetBodyAsync(EmailModel emailObj, IConnection connection)
    {
        //don't download email body if there is already a task that does the job
        //this happens if a request on downloading on demand has been raised and at the same time the automatic download is still running
        if (HasEmailBodyBeenProcessed(emailObj))
        {
            emailObj.SetBodyIsNowDownloaded();
            return;
        }

        if (_inboxInteractions == null)
            _inboxInteractions = _emailInboxFactory.GetInstance(connection.ProtocolType);

        AddProcessedBodyToBag(emailObj);

        (string html, string text) = await _inboxInteractions.DownloadBodyAsync(emailObj.Uid, connection);
        //some conversion work for later displaying html instead of text
        emailObj.Body = html ?? HtmlUtils.GetHtmlFromText(text);

        emailObj.SetBodyIsNowDownloaded();
    }

    private bool HasEmailBodyBeenProcessed(EmailModel emailObj)
    {
        lock (_lockEmailBodyProcess)
        {
            return ProcessedBodies.Any(x => x == emailObj.Uid);
        }
    }

    private void AddProcessedBodyToBag(EmailModel emailObj)
    {
        lock (_lockEmailBodyProcess)
        {
            ProcessedBodies.Add(emailObj.Uid);
        }
    }
}