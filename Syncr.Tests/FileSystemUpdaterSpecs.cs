using System;
using System.Collections.Generic;
using Bddify;
using Moq;
using Moq.Protected;
using Shouldly;
using Xunit;

namespace Syncr.Tests
{
    public abstract class FileSystemUpdaterSpec
    {
        public Mock<ISyncProvider> MockFileSystem;
        public List<FileSystemChange> FakeChanges;

        public FileSystemUpdaterSpec()
        {
            MockFileSystem = new Mock<ISyncProvider>();
            FakeChanges = new List<FileSystemChange>();
        }
    }
    
    public class Updating_A_FileSystem_Requiring_File_Creations : FileSystemUpdaterSpec
    {
        Mock<FileEntry> mockEntry = new Mock<FileEntry>();
        bool modificationUpdated = false;
        bool creationUpdated = false;
        FileSystemUpdatedEventArgs raisedArgs = null;

        public void Given_an_out_of_date_filesystem_with_pending_file_creation()
        {
            FakeChanges.Clear();
            FakeChanges.Add(FileSystemChange.ForCreate(new FakeFileEntry() { Created = DateTime.Now.Date, Modified = DateTime.Now.Date }));

            mockEntry.Setup(p => p.CanWriteCreationTime).Returns(true);
            mockEntry.Setup(p => p.CanWriteModificationTime).Returns(true);
            mockEntry.Protected().Setup("WriteCreationTime", DateTime.Now.Date).Callback(
                () => { creationUpdated = true; });
            mockEntry.Protected().Setup("WriteModificationTime", DateTime.Now.Date).Callback(
                () => { modificationUpdated = true; });

            MockFileSystem.Setup(p => p.Create(FakeChanges[0].Entry)).Returns(mockEntry.Object);
        }

        public void When_changes_are_applied_to_filesystem()
        {
            var instance = new FileSystemUpdater();
            instance.FileSystemUpdated += (sender, args) => raisedArgs = args;
            instance.ApplyChangesWhile(this.MockFileSystem.Object, FakeChanges, () => false);
        }

        public void Then_the_missing_file_entry_should_be_created()
        {
            MockFileSystem.VerifyAll();
        }

        public void And_Then_its_creationtime_should_be_set()
        {
            creationUpdated.ShouldBe(true);
        }

        public void And_Then_its_modification_time_should_be_set()
        {
            modificationUpdated.ShouldBe(true);
        }

        public void And_Then_the_filesystemupdated_event_should_be_raised()
        {
            raisedArgs.ShouldNotBe(null);
            raisedArgs.Entry.ShouldNotBe(null);
            raisedArgs.ChangeType.ShouldBe(FakeChanges[0].ChangeType);
            raisedArgs.FileSystem.ShouldNotBe(null);
            raisedArgs.FileSystem.ShouldBeSameAs(MockFileSystem.Object);
            raisedArgs.Entry.ShouldNotBe(null);
            raisedArgs.Entry.ShouldBeSameAs(FakeChanges[0].Entry);
        }

        [Fact]
        public void Missing_files_should_be_created_on_filesystem()
        {
            this.Bddify();
        }
    }

    public class Updating_A_File_System_Requiring_File_Deletions : FileSystemUpdaterSpec
    {
        public void Given_a_filesystem_requiring_file_deletions()
        {
            FakeChanges.Clear();
            FakeChanges.Add(FileSystemChange.ForDelete(new FakeFileEntry() { Created = DateTime.Now.Date, Modified = DateTime.Now.Date }));
            MockFileSystem.Setup(p => p.Delete(FakeChanges[0].Entry));
        }

        public void When_changeset_is_applied()
        {
            new FileSystemUpdater().ApplyChangesWhile(this.MockFileSystem.Object, FakeChanges, () => false);
        }

        public void Then_entries_pending_deletion_should_be_deleted()
        {
            MockFileSystem.VerifyAll();
        }

        [Fact]
        public void Entries_pending_deletion_should_be_deleted_from_filesystem()
        {
            this.Bddify();
        }
    }

    public class Updating_A_File_System_Requiring_Overwrites : FileSystemUpdaterSpec
    {
        public void Given_a_filesystem_with_pending_file_replacements()
        {
            FakeChanges.Clear();
            FakeChanges.Add(FileSystemChange.ForOverwrite(new FakeFileEntry() { Created = DateTime.Now.Date, Modified = DateTime.Now.Date }));
            MockFileSystem.Setup(p => p.Delete(FakeChanges[0].Entry));
            MockFileSystem.Setup(p => p.Create(FakeChanges[0].Entry)).Returns(new FakeFileEntry());
        }

        public void When_changeset_is_applied_to_filesystem()
        {
            new FileSystemUpdater().ApplyChangesWhile(this.MockFileSystem.Object, FakeChanges, () => false);
        }

        public void Then_files_requiring_replacement_should_be_overwritten()
        {
            MockFileSystem.VerifyAll();
        }

        [Fact]
        public void Files_requiring_replacement_should_be_overwritten()
        {
            this.Bddify();
        }
    }
    
    public class An_Exception_Is_Caught_Updating_A_File_System : FileSystemUpdaterSpec
    {
        bool eventRaised = false;

        public void Given_An_Out_Of_Date_FileSystem()
        {
            FakeChanges.Clear();
            FakeChanges.Add(FileSystemChange.ForOverwrite(new FakeFileEntry() { Created = DateTime.Now.Date, Modified = DateTime.Now.Date }));
            MockFileSystem.Setup(p => p.Create(FakeChanges[0].Entry)).Throws(new Exception("foobar"));
        }

        public void When_Changes_Are_Applied()
        {
            var instance = new FileSystemUpdater();
            instance.FileSystemUpdateFailed += (sender, args) =>
                {
                    eventRaised = true;
                    args.ShouldNotBe(null);
                    args.Exception.ShouldNotBe(null);
                    args.Exception.Message.ShouldBe("foobar");
                };

            instance.ApplyChangesWhile(this.MockFileSystem.Object, FakeChanges, () => false);
        }

        public void Then_The_FileSystemUpdateFailed_Event_Should_Be_Raised()
        {
            eventRaised.ShouldBe(true);
        }

        [Fact]
        public void The_FileSystemUpdateFailed_Event_Should_Be_Raised()
        {
            this.Bddify();
        }
    }
}