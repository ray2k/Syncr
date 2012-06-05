using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Shouldly;
using SubSpec;
using Moq;
using System.IO;
using System.Diagnostics;

namespace Syncr.Tests
{
    public class FileSystemChangeDetectorSpecs
    {
        public Mock<IFileSystem> MockSource;
        public Mock<IFileSystem> MockDestination;
        public List<FileSystemEntry> FakeSourceEntries;
        public List<FileSystemEntry> FakeDestinationEntries;

        public FileSystemChangeDetectorSpecs()
        {
            MockSource = new Mock<IFileSystem>();
            MockDestination = new Mock<IFileSystem>();
            FakeSourceEntries = new List<FileSystemEntry>();
            FakeDestinationEntries = new List<FileSystemEntry>();
        }

        [Specification]
        public void ChangeDetector_Should_Detect_Files_Missing_On_Destination()
        {
            "Given an unsynced source filesystem".Context(
                () =>
                {
                    FakeSourceEntries.Add(
                        new FakeFileEntry() { RelativePath = "foo.txt", Size = 100, Name = "foo.txt" });
                }
            );

            IEnumerable<FileSystemChange> changeSet = null;

            "When changes are detected for the destination filesystem".Do(
                () =>
                {
                    changeSet = new FileSystemChangeDetector().DetermineChangesToDestination(this.FakeSourceEntries, this.FakeDestinationEntries, ConflictBehavior.Skip);
                }
            );

            "Then a change for creating the missing file should be included".Assert(
                () =>
                {
                    changeSet.ShouldNotBe(null);
                    changeSet.Count().ShouldBe(1);
                    changeSet.First().ChangeType.ShouldBe(FileSystemChangeType.Create);
                    changeSet.First().Entry.ShouldBeSameAs(FakeSourceEntries[0]);
                }
            );
        }

        [Specification]
        public void ChangeDetector_Should_Not_Include_Changes_For_Skipped_Conflicts()
        {
            "Given a source filesystem with a conflicted file on the destination filesystem".Context(
                () =>
                {
                    FakeSourceEntries.Add(
                        new FakeFileEntry() { RelativePath = "foo.txt", Size = 100, Name = "foo.txt" });
                    FakeDestinationEntries.Add(
                        new FakeFileEntry() { RelativePath = "foo.txt", Size = 200, Name = "foo.txt" });
                }
            );

            IEnumerable<FileSystemChange> changeSet = null;

            "when changes are detected for the destination filesystem using conflict behavior 'skip'".Do(
                () =>
                {
                    changeSet = new FileSystemChangeDetector().DetermineChangesToDestination(this.FakeSourceEntries, this.FakeDestinationEntries, ConflictBehavior.Skip);
                }
            );

            "then no change for the conflicting file should be included".Assert(
                () =>
                {
                    changeSet.ShouldNotBe(null);
                    changeSet.Count().ShouldBe(0);
                }
            );
        }

        [Specification]
        public void ChangeDetector_Should_Take_Source_FileEntry_When_ConflictBehavior_Specified()
        {
            "Given a source filesystem with a conflicted file on the destination filesystem".Context(
                () =>
                {
                    FakeSourceEntries.Add(
                        new FakeFileEntry() { RelativePath = "foo.txt", Size = 100, Name = "foo.txt" });
                    FakeDestinationEntries.Add(
                        new FakeFileEntry() { RelativePath = "foo.txt", Size = 200, Name = "foo.txt" });
                }
            );

            IEnumerable<FileSystemChange> changeSet = null;

            "when changes are detected for the destination filesystem using conflict behavior 'take source'".Do(
                () =>
                {
                    changeSet = new FileSystemChangeDetector().DetermineChangesToDestination(this.FakeSourceEntries, this.FakeDestinationEntries, ConflictBehavior.TakeSource);
                }
            );

            "then a change for ovwriting the destination file should be included".Assert(
                () =>
                {
                    changeSet.ShouldNotBe(null);
                    changeSet.Count().ShouldBe(1);
                    changeSet.First().ChangeType.ShouldBe(FileSystemChangeType.Overwrite);
                    changeSet.First().Entry.ShouldBeSameAs(FakeSourceEntries[0]);
                }
            );
        }

        [Specification]
        public void ChangeDetector_Should_Not_Include_Change_Destination_Entry_When_Destination_Entry_Is_Newer()
        {
            "Given a source filesystem with a conflicted file on the destination filesystem".Context(
               () =>
               {
                   FakeSourceEntries.Add(
                       new FakeFileEntry() { RelativePath = "foo.txt", Size = 100, Name = "foo.txt", Created = DateTime.Now });
                   FakeDestinationEntries.Add(
                       new FakeFileEntry() { RelativePath = "foo.txt", Size = 200, Name = "foo.txt", Created = DateTime.Now.AddDays(1.0) });
               }
           );

            IEnumerable<FileSystemChange> changeSet = null;

            "when changes are detected for the destination filesystem using conflict behavior 'take newest'".Do(
                () =>
                {
                    changeSet = new FileSystemChangeDetector().DetermineChangesToDestination(this.FakeSourceEntries, this.FakeDestinationEntries, ConflictBehavior.TakeNewest);
                }
            );

            "then no change should be included for the conflicted file".Assert(
                () =>
                {
                    changeSet.ShouldNotBe(null);
                    changeSet.Count().ShouldBe(0);
                }
            );
        }

        [Specification]
        public void ChangeDetector_Should_Take_Source_FileEntry_When_Destination_Is_Older()
        {
            "Given a source filesystem with a conflicted file on the destination filesystem".Context(
               () =>
               {
                   FakeSourceEntries.Add(
                       new FakeFileEntry() { RelativePath = "foo.txt", Size = 100, Name = "foo.txt", Created = DateTime.Now });
                   FakeDestinationEntries.Add(
                       new FakeFileEntry() { RelativePath = "foo.txt", Size = 200, Name = "foo.txt", Created = DateTime.Now.AddDays(-1.0) });
               }
           );

            IEnumerable<FileSystemChange> changeSet = null;

            "when changes are detected for the destination filesystem when using conflict behavior 'take newest'".Do(
                () =>
                {
                    changeSet = new FileSystemChangeDetector().DetermineChangesToDestination(this.FakeSourceEntries, this.FakeDestinationEntries, ConflictBehavior.TakeNewest);
                }
            );

            "then a change for ovewriting the destionation file should be included".Assert(
                () =>
                {
                    changeSet.ShouldNotBe(null);
                    changeSet.Count().ShouldBe(1);
                    changeSet.First().ChangeType.ShouldBe(FileSystemChangeType.Overwrite);
                    changeSet.First().Entry.ShouldBeSameAs(FakeSourceEntries[0]);
                }
            );
        }
    }
}
