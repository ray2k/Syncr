using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Syncr.Tests
{
    public class FakeFileEntry : FileEntry
    {
        private Stream _stream;
        private bool _canWriteCreationTime = true;
        private bool _canWriteModificationTime = true;

        public FakeFileEntry(Stream contents)
        {
            _stream = contents;
        }

        public FakeFileEntry()
        {
        }

        public override System.IO.Stream Open()
        {
            return _stream;
        }

        public override bool CanWriteCreationTime
        {
            get { return _canWriteCreationTime; }
        }

        public override bool CanWriteModificationTime
        {
            get { return _canWriteModificationTime; }
        }

        public void SetCanWriteCreationTime(bool val)
        {
            _canWriteCreationTime = val;
        }

        public void SetCanWriteModificationTime(bool val)
        {
            _canWriteModificationTime = val;
        }
    }
}
