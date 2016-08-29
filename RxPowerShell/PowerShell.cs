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
            return CreateAsObservable<T>(script, null,BlockingSubject<T>.DEFAULT_QUEUE_LENGTH,1);
        }
        public static IObservable<T> CreateAsObservable<T>(string script, int queueLength, int bufferSize)
        {
            return CreateAsObservable<T>(script, null, queueLength, bufferSize);
        }
        public static IObservable<T> CreateAsObservable<T>(string script,Dictionary<string,Object> addParams,int queueLength,int bufferSize)
        {
            var subject = BlockingSubject<T>.Create(queueLength,bufferSize);
            var newLine = System.Environment.NewLine;
            Task.Run(() => {
                using (var runspace = RunspaceFactory.CreateRunspace())
                using (var powershell = auto.PowerShell.Create())
                {
                    runspace.ApartmentState = System.Threading.ApartmentState.MTA;
                    runspace.Open();
                    powershell.Runspace = runspace;

                    var addParamString = "";
                    if (addParams != null)
                    {
                        foreach (var addParam in addParams)
                        {
                            addParamString += ",$" + addParam.Key;
                        }
                    }
                    powershell.AddScript("param($subject" + addParamString + ")          " + newLine
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
                    if (addParams != null)
                    {
                        foreach (var addParam in addParams)
                        {
                            powershell.AddParameter(addParam.Key, addParam.Value);
                        }
                    }
                    powershell.AddCommand("Out-String");
                    var result = powershell.Invoke();
                }
            });
            return subject;
        }

        public static PowerShellResult Run(string script) {
            PowerShellResult ret;
            using (var runspace = RunspaceFactory.CreateRunspace())
            using (var powershell = auto.PowerShell.Create())
            {
                var newLine = System.Environment.NewLine;

                runspace.Open();
                powershell.Runspace = runspace;

                powershell.AddScript("$ErrorActionPreference = 'Stop'                  " + newLine
                                      + "$objects = & {                                   " + newLine
                                      + "   trap [Exception] {                            " + newLine
                                      + "      @($null,$null,$laststExitCode,$?,$error[0])" + newLine
                                      + "      break                                      " + newLine
                                      + "   }                                             " + newLine
                                      + "         " + script + "                          " + newLine
                                      + "}                                                " + newLine
                                      + "$output  = ($objects | % {$_.ToString()}) -Join [Environment]::NewLine" + newLine
                                      + "@($objects,$output,$laststExitCode,$?,$null)     ");

                var result = powershell.Invoke();
                ret= new PowerShellResult(result, powershell.Streams);
            }
            return ret;
        }
    }
}
