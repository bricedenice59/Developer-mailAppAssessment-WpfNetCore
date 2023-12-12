using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeveloperTest.EmailDiscovery.ConnectionService;

public interface IConnectionPoolInstance
{
    List<IConnection> Connections { get; }
    IConnectionDescriptor ConnectionDescriptor { get; set; }
    void Enqueue(IConnection cnx);
    IConnection GetOneAvailable();
}

public class ConnectionPoolInstance : IConnectionPoolInstance
{
    public IConnectionDescriptor ConnectionDescriptor { get; set; }
    public List<IConnection> Connections { get; }
    private ConcurrentQueue<IConnection> _connectionsPool;
    private readonly object poolLock = new object();

    public ConnectionPoolInstance()
    {
        Connections = new List<IConnection>();
        _connectionsPool = new ConcurrentQueue<IConnection>();
    }

    public void Enqueue(IConnection cnx)
    {
        lock (poolLock)
        {
            SetIsBusy(cnx, false);
            _connectionsPool.Enqueue(cnx);
        }
    }

    public IConnection GetOneAvailable()
    {
        if (_connectionsPool.IsEmpty)
            return null;

        lock (poolLock)
        {
            if (_connectionsPool.TryDequeue(out var cnx))
            {
                SetIsBusy(cnx, true);
                return cnx;
            }
        }

        return null;
    }

    private void SetIsBusy(IConnection cnx, bool isBusy)
    {
        cnx.IsBusy = isBusy;
    }
}
