using System;

namespace jp.co.stofu.RxPowerShell
{
    public interface Message<T>
    {
    }
    public class NextMessage<T> : Message<T>
    {
        private T body;
        public T Body
        {
            get { return this.body; }
            set { this.body = value; }
        }
        public NextMessage(T message)
        {
            this.body = message;
        }
    }
    public class ErrorMessage<T> : Message<T>
    {
        private Exception error;
        public Exception Error
        {
            get { return this.error; }
            set { this.error = value; }
        }
        public ErrorMessage(Exception error)
        {
            this.error = error;
        }
    }
    public class NullDisposable : IDisposable {
        public void Dispose() {
            //Nothing to do
        }
    }
}
