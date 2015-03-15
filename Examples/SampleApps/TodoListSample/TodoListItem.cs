using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tempo;

namespace MasterDetailSample
{
    public class TodoListItem : IRefCounted
    {
        public MemoryCell<string> text;
        public MemoryCell<bool> isDone;

        public TodoListItem()
        {
            this.text = new MemoryCell<string>("");
            this.isDone = new MemoryCell<bool>(false);
        }

        public void AddRef()
        {
            text.AddRef();
            isDone.AddRef();
        }

        public void Release()
        {
            text.Release();
            isDone.Release();
        }
    }
}
