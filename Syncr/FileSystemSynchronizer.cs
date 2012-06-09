using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;

namespace Syncr
{
    public class FileSystemSynchronizer : IFileSystemSynchronizer
    {
        private volatile bool _syncInProgress = false;
        private volatile bool _stopRequested = false;
        private System.Timers.Timer _pollingTimer;
        private static object _lockObject = new object();

        public FileSystemSynchronizer()
            : this(new FileSystemChangeDetector(), new FileSystemUpdater())
        {
        }

        public FileSystemSynchronizer(IFileSystemChangeDetector changeDetector, IFileSystemUpdater fileSystemUpdater)
        {
            this.ChangeDetector = changeDetector;
            this.FileSystemUpdater = fileSystemUpdater;

            _pollingTimer = new System.Timers.Timer();
            _pollingTimer.AutoReset = true;
            _pollingTimer.Elapsed += OnTimerTick;            
        }

        public void Start(ISyncProvider source, ISyncProvider destination, SyncronizationOptions syncOptions)
        {
            this.Source = source;
            this.Destination = destination;
            this.Options = syncOptions;
            
            _pollingTimer.Interval = syncOptions.PollingInterval.TotalMilliseconds;
            _pollingTimer.Start();
        }

        public void Stop()
        {
            _stopRequested = true;
            _pollingTimer.Stop();

            while (_syncInProgress)
            {
                Thread.SpinWait(20);
            }
        }

        public ISyncProvider Source { get; private set; }
        public ISyncProvider Destination { get; private set; }
        public IFileSystemChangeDetector ChangeDetector { get; private set; }
        public IFileSystemUpdater FileSystemUpdater { get; private set; }
        public SyncronizationOptions Options { get; private set; }

        public event EventHandler<FileSystemUpdatedEventArgs> FileSystemUpdated
        {
            add { this.FileSystemUpdater.FileSystemUpdated += value;  }
            remove { this.FileSystemUpdater.FileSystemUpdated -= value; }
        }

        private void OnTimerTick(object sender, ElapsedEventArgs e)
        {
            lock(_lockObject)
            {
                if (_syncInProgress)
                    return;

                _syncInProgress = true;
            }

            try
            {
                var sourceEntries = this.Source.GetFileSystemEntries(this.Options.SearchOption);
                var destinationEntries = this.Destination.GetFileSystemEntries(this.Options.SearchOption);

                if (this.Options.SyncDirection == SyncDirection.OneWay)
                {
                    var sourceToDestinationChangeSet = this.ChangeDetector.DetermineChangesToDestination(sourceEntries, destinationEntries, this.Options.ConflictBehavior);                    
                    this.FileSystemUpdater.ApplyChangesWhile(this.Destination, sourceToDestinationChangeSet, () => _stopRequested);
                }
                else if (this.Options.SyncDirection == SyncDirection.TwoWay)
                {
                    var sourceToDestinationChangeSet = this.ChangeDetector.DetermineChangesToDestination(sourceEntries, destinationEntries, this.Options.ConflictBehavior);
                    var destinationToSourceChangeSet = this.ChangeDetector.DetermineChangesToSource(sourceEntries, destinationEntries, this.Options.ConflictBehavior);

                    this.FileSystemUpdater.ApplyChangesWhile(this.Destination, sourceToDestinationChangeSet, () => _stopRequested);
                    this.FileSystemUpdater.ApplyChangesWhile(this.Source, destinationToSourceChangeSet, () => _stopRequested);
                }

                lock (_lockObject)
                {
                    _syncInProgress = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
