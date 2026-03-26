using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public interface IDataWriter
    {
        void Write(char character);

        void Write(string text);

        void Write(object data);

        void WriteLine();

        void WriteLine(char character);

        void WriteLine(string text);

        void WriteLine(object data);

        void Close();
    }
}
