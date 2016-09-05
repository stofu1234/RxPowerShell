using System;
using System.Management.Automation;
using System.Collections.ObjectModel;

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
    public class PowerShellResult
    {
        private object[]      objects;
        private string        output;
        private int           lastExitCode;
        private Boolean       success;
        private Exception     exception;
        private PSDataStreams streams;

        public object[] Objects
        {
            get { return this.objects; }
            set { this.objects = value; }
        }
        public string Output
        {
            get { return this.output; }
            set { this.output = value; }
        }
        public int LastExitCode
        {
            get { return this.lastExitCode; }
            set { this.lastExitCode = value; }
        }
        public Boolean Success
        {
            get { return this.success; }
            set { this.success = value; }
        }
        public Exception Exception
        {
            get { return this.exception; }
            set { this.exception = value; }
        }
        public PSDataStreams Streams
        {
            get { return this.streams; }
            set { this.streams = value; }
        }

        //public PowerShellResult(PSObject[] resultArray,PSDataStreams streams){
        public PowerShellResult(Collection<PSObject> resultArray, PSDataStreams streams)
        {
            objects      = resultArray[0] == null ? null  : resultArray[0].BaseObject as object[];
            output       = resultArray[1] == null ? null  : resultArray[1].BaseObject as string;
            lastExitCode = resultArray[2] == null ? 0     : (int)resultArray[2].BaseObject;
            success      = resultArray[3] == null ? false : (Boolean)resultArray[3].BaseObject;
            exception    = resultArray[4] == null ? null  : resultArray[4].BaseObject as Exception;
            this.streams = streams;
        }
    }
}
