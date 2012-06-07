using System;
using System.Collections.Generic;
using System.Linq;
using Bddify;
using Moq;
using Shouldly;
using Xunit;

namespace Syncr.Tests
{
    public abstract class FileDetectionSpec
    {
        public Mock<IFileSystem> MockSource;
        public Mock<IFileSystem> MockDestination;
        public List<FileSystemEntry> FakeSourceEntries;
        public List<FileSystemEntry> FakeDestinationEntries;

        public FileDetectionSpec()
        {
            MockSource = new Mock<IFileSystem>();
            MockDestination = new Mock<IFileSystem>();
            FakeSourceEntries = new List<FileSystemEntry>();
            FakeDestinationEntries = new List<FileSystemEntry>();
        }
    }

    public class Detecting_Changes_To_Destination : FileDetectionSpec
    {
        public class When_File_Is_On_Source_But_Not_Destination : FileDetectionSpec
        {
            IEnumerable<FileSystemChange> changeSet = null;

            public void Given_An_Unsynced_FileSystem()
            {
                FakeSourceEntries.Add(new FakeFileEntry() { RelativePath = "foo.txt", Size = 100, Name = "foo.txt" });
            }

            public void When_Changes_To_Destination_Are_Detected()
            {
                changeSet = new FileSystemChangeDetector().DetermineChangesToDestination(this.FakeSourceEntries, this.FakeDestinationEntries, ConflictBehavior.Skip);
            }

            public void Then_A_Change_For_A_Source_File_Not_On_Destination_Should_Be_Included()
            {
                changeSet.ShouldNotBe(null);
                changeSet.Count().ShouldBe(1);
                changeSet.First().ChangeType.ShouldBe(FileSystemChangeType.Create);
                changeSet.First().Entry.ShouldBeSameAs(FakeSourceEntries[0]);
            }

            [Fact]
            public void It_Should_Be_Included_In_Changeset()
            {
                this.Bddify();
            }
        }

        public class When_A_Conflicted_File_Is_On_Both_FileSystems_With_Skip : FileDetectionSpec
        {
            IEnumerable<FileSystemChange> changeSet = null;

            public void Given_A_Source_FileSystem_With_A_Conflicted_File_On_Destination()
            {
                FakeSourceEntries.Add(
                    new FakeFileEntry() { RelativePath = "foo.txt", Size = 100, Name = "foo.txt" });
                FakeDestinationEntries.Add(
                    new FakeFileEntry() { RelativePath = "foo.txt", Size = 200, Name = "foo.txt" });
            }

            public void When_Changes_Are_Detected_For_The_Destination_With_ConflictBehavior_Skip()
            {
                changeSet = new FileSystemChangeDetector().DetermineChangesToDestination(this.FakeSourceEntries, this.FakeDestinationEntries, ConflictBehavior.Skip);
            }

            public void Then_No_Change_For_The_Conflicting_File_Should_Be_In_The_Changeset()
            {
                changeSet.ShouldNotBe(null);
                changeSet.Count().ShouldBe(0);
            }

            [Fact]
            public void No_Change_For_The_Conflicting_File_Should_Be_In_The_Changeset()
            {
                this.Bddify();
            }
        }

        public class When_A_Conflicted_File_Is_On_Both_FileSystems_With_TakeSource : FileDetectionSpec
        {
            IEnumerable<FileSystemChange> changeSet = null;

            public void Given_A_Source_FIleSystem_With_Conflicted_File_On_Destination()
            {
                FakeSourceEntries.Add(
                    new FakeFileEntry() { RelativePath = "foo.txt", Size = 100, Name = "foo.txt" });
                FakeDestinationEntries.Add(
                    new FakeFileEntry() { RelativePath = "foo.txt", Size = 200, Name = "foo.txt" });
            }

            public void When_Changes_Are_Detected_For_Destination()
            {
                changeSet = new FileSystemChangeDetector().DetermineChangesToDestination(this.FakeSourceEntries, this.FakeDestinationEntries, ConflictBehavior.TakeSource);
            }

            public void Then_A_Change_For_OVerwriting_The_Destination_Should_Be_In_Changeset()
            {
                changeSet.ShouldNotBe(null);
                changeSet.Count().ShouldBe(1);
                changeSet.First().ChangeType.ShouldBe(FileSystemChangeType.Overwrite);
                changeSet.First().Entry.ShouldBeSameAs(FakeSourceEntries[0]);
            }

            [Fact]
            public void A_Change_For_OVerwriting_The_Destination_Should_Be_In_Changeset()
            {
                this.Bddify();
            }
        }

        public class When_A_Conflicted_File_Is_Newer_On_Destination_Than_Source_With_TakeNewest : FileDetectionSpec
        {
            IEnumerable<FileSystemChange> changeSet = null;

            public void Given_A_Source_FileSystem_With_Conflicted_File()
            {
                FakeSourceEntries.Add(
                       new FakeFileEntry() { RelativePath = "foo.txt", Size = 100, Name = "foo.txt", Created = DateTime.Now });
                FakeDestinationEntries.Add(
                    new FakeFileEntry() { RelativePath = "foo.txt", Size = 200, Name = "foo.txt", Created = DateTime.Now.AddDays(1.0) });
            }

            public void When_Changes_Are_Detected_For_Destination_Using_TakeNewest()
            {
                changeSet = new FileSystemChangeDetector().DetermineChangesToDestination(this.FakeSourceEntries, this.FakeDestinationEntries, ConflictBehavior.TakeNewest);
            }

            public void Then_No_Change_Should_Be_Included_For_The_Conflicted_File()
            {
                changeSet.ShouldNotBe(null);
                changeSet.Count().ShouldBe(0);
            }

            [Fact]
            public void No_Change_Should_Be_Included_For_The_Conflicted_File()
            {
                this.Bddify();
            }
        }

        public class When_A_Conflicted_File_Is_Newer_On_Source_Than_Destination_With_TakeNewest : FileDetectionSpec
        {
            IEnumerable<FileSystemChange> changeSet = null;

            public void Given_A_Source_FileSystem_With_Conflicted_File_On_Destination()
            {
                FakeSourceEntries.Add(
                    new FakeFileEntry() { RelativePath = "foo.txt", Size = 100, Name = "foo.txt", Created = DateTime.Now });
                FakeDestinationEntries.Add(
                    new FakeFileEntry() { RelativePath = "foo.txt", Size = 200, Name = "foo.txt", Created = DateTime.Now.AddDays(-1.0) });
            }
            
            public void When_Changes_Are_Detected_For_Destination_Using_TakeNewest()
            {
                changeSet = new FileSystemChangeDetector().DetermineChangesToDestination(this.FakeSourceEntries, this.FakeDestinationEntries, ConflictBehavior.TakeNewest);
            }

            public void Then_A_Change_For_Overwriting_The_Conflicted_File_On_Destination_Should_Be_Included()
            {
                changeSet.ShouldNotBe(null);
                changeSet.Count().ShouldBe(1);
                changeSet.First().ChangeType.ShouldBe(FileSystemChangeType.Overwrite);
                changeSet.First().Entry.ShouldBeSameAs(FakeSourceEntries[0]);
            }

            [Fact]
            public void A_Change_For_Overwriting_The_Conflicted_File_On_Destination_Should_Be_Included()
            {
                this.Bddify();
            }
        }
    }

    public class Detecting_Changes_To_Source : FileDetectionSpec
    {
        public class When_File_Is_On_Destination_But_Not_Source : FileDetectionSpec
        {
            IEnumerable<FileSystemChange> changeSet = null;

            public void Given_An_Unsynced_FileSystem()
            {
                FakeSourceEntries.Add(new FakeFileEntry() { RelativePath = "foo.txt", Size = 100, Name = "foo.txt" });
            }

            public void When_Changes_To_Source_Are_Detected()
            {
                changeSet = new FileSystemChangeDetector().DetermineChangesToDestination(this.FakeSourceEntries, this.FakeDestinationEntries, ConflictBehavior.Skip);
            }

            public void Then_A_Change_For_A_Destination_File_Not_On_Source_Should_Be_Included()
            {
                changeSet.ShouldNotBe(null);
                changeSet.Count().ShouldBe(1);
                changeSet.First().ChangeType.ShouldBe(FileSystemChangeType.Create);
                changeSet.First().Entry.ShouldBeSameAs(FakeSourceEntries[0]);
            }

            [Fact]
            public void It_Should_Be_Included_In_Changeset()
            {
                this.Bddify();
            }
        }

        public class When_A_Conflicted_File_Is_On_Both_FileSystems_With_Skip : FileDetectionSpec
        {
            IEnumerable<FileSystemChange> changeSet = null;

            public void Given_A_Destination_FileSystem_With_A_Conflicted_File_On_Source()
            {
                FakeSourceEntries.Add(
                    new FakeFileEntry() { RelativePath = "foo.txt", Size = 100, Name = "foo.txt" });
                FakeDestinationEntries.Add(
                    new FakeFileEntry() { RelativePath = "foo.txt", Size = 200, Name = "foo.txt" });
            }

            public void When_Changes_Are_Detected_For_The_Source_With_ConflictBehavior_Skip()
            {
                changeSet = new FileSystemChangeDetector().DetermineChangesToDestination(this.FakeSourceEntries, this.FakeDestinationEntries, ConflictBehavior.Skip);
            }

            public void Then_No_Change_For_The_Conflicting_File_Should_Be_In_The_Changeset()
            {
                changeSet.ShouldNotBe(null);
                changeSet.Count().ShouldBe(0);
            }

            [Fact]
            public void No_Change_For_The_Conflicting_File_Should_Be_In_The_Changeset()
            {
                this.Bddify();
            }
        }

        public class When_A_Conflicted_File_Is_On_Both_FileSystems_With_TakeSource : FileDetectionSpec
        {
            IEnumerable<FileSystemChange> changeSet = null;

            public void Given_A_Destination_FIleSystem_With_Conflicted_File_On_Source()
            {
                FakeSourceEntries.Add(
                    new FakeFileEntry() { RelativePath = "foo.txt", Size = 100, Name = "foo.txt" });
                FakeDestinationEntries.Add(
                    new FakeFileEntry() { RelativePath = "foo.txt", Size = 200, Name = "foo.txt" });
            }

            public void When_Changes_Are_Detected_For_Source()
            {
                changeSet = new FileSystemChangeDetector().DetermineChangesToDestination(this.FakeSourceEntries, this.FakeDestinationEntries, ConflictBehavior.TakeSource);
            }

            public void Then_A_Change_For_OVerwriting_The_Source_Should_Be_In_Changeset()
            {
                changeSet.ShouldNotBe(null);
                changeSet.Count().ShouldBe(1);
                changeSet.First().ChangeType.ShouldBe(FileSystemChangeType.Overwrite);
                changeSet.First().Entry.ShouldBeSameAs(FakeSourceEntries[0]);
            }

            [Fact]
            public void A_Change_For_OVerwriting_The_Source_Should_Be_In_Changeset()
            {
                this.Bddify();
            }
        }

        public class When_A_Conflicted_File_Is_Newer_On_Source_Than_Destination_With_TakeNewest : FileDetectionSpec
        {
            IEnumerable<FileSystemChange> changeSet = null;

            public void Given_A_Destination_FileSystem_With_Conflicted_File()
            {
                FakeSourceEntries.Add(
                       new FakeFileEntry() { RelativePath = "foo.txt", Size = 100, Name = "foo.txt", Created = DateTime.Now });
                FakeDestinationEntries.Add(
                    new FakeFileEntry() { RelativePath = "foo.txt", Size = 200, Name = "foo.txt", Created = DateTime.Now.AddDays(1.0) });
            }

            public void When_Changes_Are_Detected_For_Source_Using_TakeNewest()
            {
                changeSet = new FileSystemChangeDetector().DetermineChangesToDestination(this.FakeSourceEntries, this.FakeDestinationEntries, ConflictBehavior.TakeNewest);
            }

            public void Then_No_Change_Should_Be_Included_For_The_Conflicted_File()
            {
                changeSet.ShouldNotBe(null);
                changeSet.Count().ShouldBe(0);
            }

            [Fact]
            public void No_Change_Should_Be_Included_For_The_Conflicted_File()
            {
                this.Bddify();
            }
        }

        public class When_A_Conflicted_File_Is_Newer_On_Destination_Than_Source_With_TakeNewest : FileDetectionSpec
        {
            IEnumerable<FileSystemChange> changeSet = null;

            public void Given_A_Destination_FileSystem_With_Conflicted_File_On_Source()
            {
                FakeSourceEntries.Add(
                    new FakeFileEntry() { RelativePath = "foo.txt", Size = 100, Name = "foo.txt", Created = DateTime.Now });
                FakeDestinationEntries.Add(
                    new FakeFileEntry() { RelativePath = "foo.txt", Size = 200, Name = "foo.txt", Created = DateTime.Now.AddDays(-1.0) });
            }
            
            public void When_Changes_Are_Detected_For_Source_Using_TakeNewest()
            {
                changeSet = new FileSystemChangeDetector().DetermineChangesToDestination(this.FakeSourceEntries, this.FakeDestinationEntries, ConflictBehavior.TakeNewest);
            }

            public void Then_A_Change_For_Overwriting_The_Conflicted_File_On_Source_Should_Be_Included()
            {
                changeSet.ShouldNotBe(null);
                changeSet.Count().ShouldBe(1);
                changeSet.First().ChangeType.ShouldBe(FileSystemChangeType.Overwrite);
                changeSet.First().Entry.ShouldBeSameAs(FakeSourceEntries[0]);
            }

            [Fact]
            public void A_Change_For_Overwriting_The_Conflicted_File_On_Source_Should_Be_Included()
            {
                this.Bddify();
            }
        }
    }
}
