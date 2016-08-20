using System;
using System.Collections.Concurrent;
using System.Collections.Generic;


namespace jp.co.stofu.RxPowerShell
{
    public class BlockingSubject<T> : IObserver<T>, IObservable<T>
    {
        public static int DEFAULT_QUEUE_LENGTH = 1024;

        //factoryクラスなのでprotected
        protected BlockingSubject()
        {
            throw new NotImplementedException();
        }

        public static BlockingSubject<T> Create()
        {
            return new SimpleBlockingSubject<T>();
        }

        public static BlockingSubject<T> Create(int queueLength)
        {
            return new SimpleBlockingSubject<T>(queueLength);
        }

        public static BlockingSubject<T> Create(int queueLength, int bufferSize)
        {
            if (bufferSize > 1)
            {
                return new BufferedBlockingSubject<T>(queueLength, bufferSize);
            }
            else
            {
                return new SimpleBlockingSubject<T>(queueLength);
            }
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(T value)
        {
            throw new NotImplementedException();
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            throw new NotImplementedException();
        }
    }

    class SimpleBlockingSubject<T> : BlockingSubject<T>
    {
        BlockingCollection<Message<T>> queue = null;
        private int queueLength = DEFAULT_QUEUE_LENGTH;

        public SimpleBlockingSubject()
        {
            queue = new BlockingCollection<Message<T>>(queueLength);
        }
        public SimpleBlockingSubject(int queueLength)
        {
            queue = new BlockingCollection<Message<T>>(queueLength);
        }
        new public void OnCompleted()
        {
            queue.CompleteAdding();
        }
        new public void OnError(Exception ex)
        {
            queue.Add(new ErrorMessage<T>(ex));
        }
        new public void OnNext(T message)
        {
            queue.Add(new NextMessage<T>(message));
        }
        new public IDisposable Subscribe(IObserver<T> observer)
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

    class BufferedBlockingSubject<T> : BlockingSubject<T>
    {
        BlockingCollection<List<Message<T>>> queue = null;
        private int queueLength = DEFAULT_QUEUE_LENGTH;
        private int bufferSize = 1;
        private List<Message<T>> currentList;
        private int offset;

        public BufferedBlockingSubject(int queueLenth, int bufferSize)
        {
            this.bufferSize = bufferSize;
            currentList = new List<Message<T>>(bufferSize);
            offset = 0;
            queue = new BlockingCollection<List<Message<T>>>(queueLength);
        }
        new public void OnCompleted()
        {
            queue.Add(currentList);
            queue.CompleteAdding();
        }
        new public void OnError(Exception ex)
        {
            currentList.Add(new ErrorMessage<T>(ex));
            queue.Add(currentList);
        }
        new public void OnNext(T message)
        {
            if (offset < bufferSize)
            {
                currentList.Add(new NextMessage<T>(message));
                ++offset;
            }
            else if (offset >= bufferSize)
            {
                queue.Add(currentList);
                offset = 0;
                currentList = new List<Message<T>>(queueLength);
            }
        }
        new public IDisposable Subscribe(IObserver<T> observer)
        {
            try
            {
                while (true)
                {
                    var messages = queue.Take();
                    foreach (var message in messages)
                    {
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
