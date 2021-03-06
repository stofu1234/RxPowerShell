﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;


namespace jp.co.stofu.RxPowerShell
{
    /// <summary>
    /// このクラスはBlockingCollectionをラッピングした抽象Subjectです
    /// Subjectであるため、IObserverとIObservable両方のインタフェースを実装しています
    /// AbstractFactoryパターンで、Factoryで生成するクラスを切り替えます
    /// </summary>
    public abstract class BlockingSubject<T> : IObserver<T>, IObservable<T>
    {
        public static int DEFAULT_QUEUE_LENGTH = 1024;

        //factoryクラスなのでprotected
        protected BlockingSubject()
        {
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

        public abstract void OnCompleted();
        public abstract void OnError(Exception error);
        public abstract void OnNext(T value);
        public abstract IDisposable Subscribe(IObserver<T> observer);

        public void processMessage(IObserver<T> observer, Message<T> message)
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
        public IDisposable SubscribeLoop(IObserver<T> observer, Action inLoopProcess)
        {
            try
            {
                while (true)
                {
                    inLoopProcess.Invoke();
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
        public override void OnCompleted()
        {
            queue.CompleteAdding();
        }
        public override void OnError(Exception ex)
        {
            queue.Add(new ErrorMessage<T>(ex));
        }
        public override void OnNext(T message)
        {
            queue.Add(new NextMessage<T>(message));
        }
        public override IDisposable Subscribe(IObserver<T> observer)
        {
            return SubscribeLoop(observer, () =>
            {
                var message = queue.Take();
                processMessage(observer, message);
            });
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
        public override void OnCompleted()
        {
            queue.Add(currentList);
            queue.CompleteAdding();
        }
        public override void OnError(Exception ex)
        {
            currentList.Add(new ErrorMessage<T>(ex));
            queue.Add(currentList);
            //CompleteAddingすべきか、迷ってますが一応リスト初期化します
            offset = 0;
            currentList = new List<Message<T>>(queueLength);
        }
        public override void OnNext(T message)
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
        public override IDisposable Subscribe(IObserver<T> observer)
        {
            return SubscribeLoop(observer, () =>
            {
                var messages = queue.Take();
                foreach (var message in messages)
                {
                    processMessage(observer, message);
                }
            });
        }
    }
}
