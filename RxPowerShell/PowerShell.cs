using System;
using System.Threading.Tasks;
using auto = System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Collections.Generic;

namespace jp.co.stofu.RxPowerShell
{
    public class PowerShell
    {
        public static IObservable<T> CreateAsObservable<T>(string script) {
            return CreateAsObservable<T>(script, null);
        }
        public static IObservable<T> CreateAsObservable<T>(string script,Dictionary<string,Object> addParams)
        {
            var subject = new BlockingSubject<T>();
            var newLine = System.Environment.NewLine;
            Task.Run(() => {
                using (var runspace = RunspaceFactory.CreateRunspace())
                {
                    var powershell = auto.PowerShell.Create();
                    runspace.ApartmentState = System.Threading.ApartmentState.MTA;
                    runspace.Open();
                    powershell.Runspace = runspace;

                    var addParamString = "";
                    if (addParams != null)
                    {
                        foreach (var addParam in addParams)
                        {
                            addParamString+=",$"+addParam.Key;
                        }
                    }
                    powershell.AddScript(   "param($subject" + addParamString + ")          " + newLine
                                          + "$ErrorActionPreference = 'Stop'                " + newLine
                                          + "& {                                            " + newLine
                                          + "   trap [Exception] {                          " + newLine
                                          + "      $subject.OnError($Error[0].Exception)    " + newLine
                                          + "      continue                                 " + newLine
                                          + "   }                                           " + newLine
                                          + "         " + script + "                        " + newLine
                                          + "  } | % { $subject.OnNext($_) }                " + newLine
                                          + "$subject.OnCompleted()                         ");

                    powershell.AddParameter("subject", subject);
                    if (addParams != null) {
                        foreach (var addParam in addParams) {
                            powershell.AddParameter(addParam.Key, addParam.Value);
                        }
                    }
                    powershell.AddCommand("Out-String");
                    var result = powershell.Invoke();
                }
            });
            return subject;
        }
    }
}
