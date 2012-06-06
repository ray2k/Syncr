using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Bddify;
using Bddify.Scanners.ScenarioScanners;
using Xunit;

namespace Syncr.Tests
{
    public abstract class FileSystemSynchronizerSpec
    {
        public Mock<IFileSystem> MockSource;
        public Mock<IFileSystem> MockDestination;
        public Mock<IFileSystemChangeDetector> MockChangeDetector;
        public Mock<IFileSystemUpdater> MockUpdater;        

        public FileSystemSynchronizerSpec()
        {
            MockSource = new Mock<IFileSystem>();
            MockDestination = new Mock<IFileSystem>();
            MockChangeDetector = new Mock<IFileSystemChangeDetector>();
            MockUpdater = new Mock<IFileSystemUpdater>();
        }
    }

    public class When_Syncing_OneWay : FileSystemSynchronizerSpec
    {
        public void Given_two_unsynced_file_systems()
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

        public void When_a_one_way_sync_is_started()
        {
            new FileSystemSynchronizer(MockChangeDetector.Object, MockUpdater.Object)
                        .Start(MockSource.Object, MockDestination.Object,
                                new SyncronizationOptions()
                                {
                                    ConflictBehavior = ConflictBehavior.Skip,
                                    PollingInterval = TimeSpan.FromMilliseconds(10),
                                    SyncDirection = SyncDirection.OneWay,
                                    SearchOption = SearchOption.AllDirectories
                                }
                        );
        }

        public void Then_the_changes_to_the_destination_file_system_should_be_detected()
        {
            Thread.Sleep(200);
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
