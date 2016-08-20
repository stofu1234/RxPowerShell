using System;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace jp.co.stofu.RxPowerShell
{
    public class FileEx
    {
        public static string DEFAULT_ENCODING = "shift_jis";

        public static IObservable<string> ReadTxtBuffer(string filePath)
        {
            return ReadTxtToBuffer(filePath, DEFAULT_ENCODING);
        }
        public static IObservable<string> ReadTxtToBuffer(string filePath, string encoding) {
            return ReadTxtToBuffer(filePath, encoding, BlockingSubject<string>.DEFAULT_QUEUE_LENGTH, 1);
        }
        public static IObservable<string> ReadTxtToBuffer(string filePath, int queueLength, int bufferSize) {
            return ReadTxtToBuffer(filePath, DEFAULT_ENCODING, BlockingSubject<string>.DEFAULT_QUEUE_LENGTH, 1);
        }
        public static IObservable<string> ReadTxtToBuffer(string filePath, string encoding,int queueLength,int bufferSize)
        {
            var subject = BlockingSubject<string>.Create(queueLength,bufferSize);
            Task.Run(() => {
                StreamReader reader = null;
                try
                {
                    reader = new StreamReader(filePath, Encoding.GetEncoding(encoding));
                    while (reader.Peek() > -1)
                    {
                        subject.OnNext(reader.ReadLine());
                    }
                }
                catch (Exception e)
                {
                    subject.OnError(e);
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                    }
                    subject.OnCompleted();
                }
            });
            return subject;
        }

    }
}
