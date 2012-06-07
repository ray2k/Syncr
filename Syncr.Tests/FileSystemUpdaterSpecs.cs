﻿using System;
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
        public Mock<IFileSystem> MockFileSystem;
        public List<FileSystemChange> FakeChanges;

        public FileSystemUpdaterSpec()
        {
            MockFileSystem = new Mock<IFileSystem>();
            FakeChanges = new List<FileSystemChange>();
        }
    }
    
    public class Updating_A_FileSystem_Requiring_File_Creations : FileSystemUpdaterSpec
    {
        Mock<FileEntry> mockEntry = new Mock<FileEntry>();
        bool modificationUpdated = false;
        bool creationUpdated = false;
        FileSystemUpdatedEventArgs raisedArgs = null;

        public void Given_An_Out_Of_Date_FileSystem()
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

        public void When_Changes_Are_Applied()
        {
            var instance = new FileSystemUpdater();
            instance.FileSystemUpdated += (sender, args) => raisedArgs = args;
            instance.ApplyChangesWhile(this.MockFileSystem.Object, FakeChanges, () => false);
        }

        public void Then_The_Entry_Pending_Creation_Should_Be_Created()
        {
            MockFileSystem.VerifyAll();
        }

        public void And_Then_Its_CreationTime_Should_Be_Set()
        {
            creationUpdated.ShouldBe(true);
        }

        public void And_Then_Its_Modification_Time_Should_Be_Set()
        {
            modificationUpdated.ShouldBe(true);
        }

        public void And_Then_The_FileSystemUpdated_Event_Should_Be_Raised()
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
        public void Entries_Pending_Creation_Should_Be_Created()
        {
            this.Bddify();
        }
    }

    public class Updating_A_File_System_Requiring_File_Deletions : FileSystemUpdaterSpec
    {
        public void Given_An_Out_Of_Date_FileSystem()
        {
            FakeChanges.Clear();
            FakeChanges.Add(FileSystemChange.ForDelete(new FakeFileEntry() { Created = DateTime.Now.Date, Modified = DateTime.Now.Date }));
            MockFileSystem.Setup(p => p.Delete(FakeChanges[0].Entry));
        }

        public void When_Changes_Are_Applied()
        {
            new FileSystemUpdater().ApplyChangesWhile(this.MockFileSystem.Object, FakeChanges, () => false);
        }

        public void Then_The_Entry_Pending_Deletion_Should_Be_Deleted()
        {
            MockFileSystem.VerifyAll();
        }

        [Fact]
        public void Entries_Pending_Deletion_Should_Be_Deleted()
        {
            this.Bddify();
        }
    }

    public class Updating_A_File_System_Requiring_Overwrites : FileSystemUpdaterSpec
    {
        public void Given_An_Out_Of_Date_FileSystem()
        {
            FakeChanges.Clear();
            FakeChanges.Add(FileSystemChange.ForOverwrite(new FakeFileEntry() { Created = DateTime.Now.Date, Modified = DateTime.Now.Date }));
            MockFileSystem.Setup(p => p.Delete(FakeChanges[0].Entry));
            MockFileSystem.Setup(p => p.Create(FakeChanges[0].Entry)).Returns(new FakeFileEntry());
        }

        public void When_Changes_Are_Applied()
        {
            new FileSystemUpdater().ApplyChangesWhile(this.MockFileSystem.Object, FakeChanges, () => false);
        }

        public void Then_The_Entry_Pending_Overwriting_Should_Be_Overwritten()
        {
            MockFileSystem.VerifyAll();
        }

        [Fact]
        public void Entries_Pending_Overwrite_Should_Be_Overwritten()
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