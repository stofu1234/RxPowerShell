using System;
using System.Collections.Concurrent;

namespace jp.co.stofu.RxPowerShell
{
    public class BlockingSubject<T> : IObserver<T>, IObservable<T>
    {
        BlockingCollection<Message<T>> queue=null;
        private int queueLength = 1024;
        private int bufferSize = 1;

        public BlockingSubject()
        {
            queue = new BlockingCollection<Message<T>>(queueLength);
        }
        public void OnCompleted()
        {
            queue.CompleteAdding();
        }
        public void OnError(Exception ex)
        {
            queue.Add(new ErrorMessage<T>(ex));
        }
        public void OnNext(T message)
        {
            queue.Add(new NextMessage<T>(message));
        }
        public IDisposable Subscribe(IObserver<T> observer)
        {
            try
            {
                while (true)
                {
                    var message = queue.Take();
                    if (message.GetType().Equals(typeof(NextMessage<T>)))
                    {
                        var nextMessage = message as NextMessage<T>;
                        observer.OnNext(nextMessage.Body);
                    }
                    else if (message.GetType().Equals(typeof(ErrorMessage<T>)))
                    {
                        var errorMessage = message as ErrorMessage<T>;
                        observer.OnError(errorMessage.Error);
                    }
                }
            }
            catch (Exception e)
            {
                //dummy
            }
            finally
            {
                observer.OnCompleted();
            }
            return new NullDisposable();
        }
    }
}
