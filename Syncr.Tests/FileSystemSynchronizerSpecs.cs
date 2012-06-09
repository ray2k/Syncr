using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Bddify;
using Moq;
using Xunit;

namespace Syncr.Tests
{
    public abstract class FileSystemSynchronizerSpec
    {
        public Mock<ISyncProvider> MockSource;
        public Mock<ISyncProvider> MockDestination;
        public Mock<IFileSystemChangeDetector> MockChangeDetector;
        public Mock<IFileSystemUpdater> MockUpdater;        

        public FileSystemSynchronizerSpec()
        {
            MockSource = new Mock<ISyncProvider>();
            MockDestination = new Mock<ISyncProvider>();
            MockChangeDetector = new Mock<IFileSystemChangeDetector>();
            MockUpdater = new Mock<IFileSystemUpdater>();
        }
    }

    public class Syncing_OneWay : FileSystemSynchronizerSpec
    {
        public void Given_Two_UnSynced_FileSystems()
        {
            MockSource.Setup(p => p.GetFileSystemEntries(SearchOption.AllDirectories)).Returns(new List<FileSystemEntry>());

            MockDestination.Setup(p => p.GetFileSystemEntries(SearchOption.AllDirectories)).Returns(new List<FileSystemEntry>());

            MockChangeDetector.Setup(p => p.DetermineChangesToDestination(
                It.IsAny<IEnumerable<FileSystemEntry>>(),
                It.IsAny<IEnumerable<FileSystemEntry>>(),
                ConflictBehavior.Skip)).Returns(new List<FileSystemChange>());

            MockUpdater.Setup(p => p.ApplyChangesWhile(
                MockDestination.Object,
                It.IsAny<IEnumerable<FileSystemChange>>(),
                It.IsAny<Func<bool>>()));
        }

        public void When_Synced_One_Way()
        {
            var instance = new FileSystemSynchronizer(MockChangeDetector.Object, MockUpdater.Object);

            bool iterationComplete = false;

            instance.IterationCompleted += delegate(object sender, EventArgs args)
            {
                iterationComplete = true;
            };

            instance.Start(MockSource.Object, MockDestination.Object,
                new SyncronizationOptions()
                {
                    ConflictBehavior = ConflictBehavior.Skip,
                    PollingInterval = TimeSpan.FromMilliseconds(10),
                    SyncDirection = SyncDirection.OneWay,
                    SearchOption = SearchOption.AllDirectories
                }
            );

            while (!iterationComplete)
                Thread.SpinWait(10);
        }

        public void Then_the_changes_to_the_destination_file_system_should_be_detected()
        {
            MockChangeDetector.VerifyAll();
        }

        public void AndThen_the_changes_to_the_destination_file_system_should_be_applied()
        {
            MockUpdater.VerifyAll();
        }

        [Fact]
        public void The_Destination_Should_Be_Updated_From_Detected_Changes()
        {
            this.Bddify();
        }
    }

    public class Syncing_TwoWay : FileSystemSynchronizerSpec
    {
        public void Given_Two_UnSynced_FileSystems()
        {
            MockSource.Setup(p => p.GetFileSystemEntries(SearchOption.AllDirectories)).Returns(new List<FileSystemEntry>());
            MockDestination.Setup(p => p.GetFileSystemEntries(SearchOption.AllDirectories)).Returns(new List<FileSystemEntry>());

            MockChangeDetector.Setup(p => p.DetermineChangesToDestination(
                It.IsAny<IEnumerable<FileSystemEntry>>(),
                It.IsAny<IEnumerable<FileSystemEntry>>(),
                ConflictBehavior.Skip)).Returns(new List<FileSystemChange>());

            MockChangeDetector.Setup(p => p.DetermineChangesToSource(
                It.IsAny<IEnumerable<FileSystemEntry>>(),
                It.IsAny<IEnumerable<FileSystemEntry>>(),
                ConflictBehavior.Skip)).Returns(new List<FileSystemChange>());

            MockUpdater.Setup(p => p.ApplyChangesWhile(
                MockDestination.Object,
                It.IsAny<IEnumerable<FileSystemChange>>(),
                It.IsAny<Func<bool>>()));

            MockUpdater.Setup(p => p.ApplyChangesWhile(
                MockSource.Object,
                It.IsAny<IEnumerable<FileSystemChange>>(),
                It.IsAny<Func<bool>>()));
        }

        public void When_Synced_One_Way()
        {
            var instance = new FileSystemSynchronizer(MockChangeDetector.Object, MockUpdater.Object);

            bool iterationCompleted = false;

            instance.IterationCompleted += delegate(object sender, EventArgs e)
            {
                iterationCompleted = true;
            };
            
            instance.Start(MockSource.Object, MockDestination.Object,
                new SyncronizationOptions()
                {
                    ConflictBehavior = ConflictBehavior.Skip,
                    PollingInterval = TimeSpan.FromMilliseconds(10),
                    SyncDirection = SyncDirection.TwoWay,
                    SearchOption = SearchOption.AllDirectories
                }
            );

            while (!iterationCompleted)
                Thread.SpinWait(20);
        }

        public void Then_the_changes_to_the_destination_file_system_should_be_detected()
        {
            MockChangeDetector.VerifyAll();
        }

        public void AndThen_the_changes_to_the_destination_file_system_should_be_applied()
        {
            MockUpdater.VerifyAll();
        }

        [Fact]
        public void The_Destination_Should_Be_Updated_From_Detected_Changes()
        {
            this.Bddify();
        }
    }
}
