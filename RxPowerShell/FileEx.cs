using System;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace jp.co.stofu.RxPowerShell
{
    public class FileEx
    {
        public static IObservable<string> ReadTxtToBuffer(string filePath, string encoding)
        {
            var subject = new BlockingSubject<string>();
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

        public static IObservable<string> ReadTxtBuffer(string filePath)
        {
            return ReadTxtToBuffer(filePath, "shift_jis");
        }
    }
}
