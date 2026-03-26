using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public sealed class EmptyWriter : IDataWriter
    {
        public void Write(char character)
        {
        }

        public void Write(string text)
        {
        }

        public void Write(object data)
        {
        }

        public void WriteLine()
        {
        }

        public void WriteLine(char character)
        {
        }

        public void WriteLine(string text)
        {
        }

        public void WriteLine(object data)
        {
        }

        public void Close()
        {
        }
    }
}
