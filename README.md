RxPowerShell
====

## Overview
PowerShellを.NET環境からReactiveExtensionのObservableとして扱うライブラリです。
昨今流行りのC# Scriptingでコマンドレットが扱いやすくなるので、PowerShell上での速度最適化が限界に達した時に使ってみてください

## Description

## Demo
> rx.PowerShell.CreateAsObservable<string>("1..10 | % {Start-Sleep 1;\"count:${_} script:$(Get-Date)\"}")  
               .ForEach(l => Console.WriteLine(l + " out:" + DateTime.Now.ToString()));  
count:1 script:08/14/2016 11:40:23 out:2016/08/14 11:40:24  
count:2 script:08/14/2016 11:40:25 out:2016/08/14 11:40:25  
count:3 script:08/14/2016 11:40:26 out:2016/08/14 11:40:26  
count:4 script:08/14/2016 11:40:27 out:2016/08/14 11:40:27  
count:5 script:08/14/2016 11:40:28 out:2016/08/14 11:40:28  
count:6 script:08/14/2016 11:40:29 out:2016/08/14 11:40:29  
count:7 script:08/14/2016 11:40:30 out:2016/08/14 11:40:30  
count:8 script:08/14/2016 11:40:31 out:2016/08/14 11:40:31  
count:9 script:08/14/2016 11:40:32 out:2016/08/14 11:40:32  
count:10 script:08/14/2016 11:40:33 out:2016/08/14 11:40:33  
  
> FileEx.ReadTxtToBuffer(@"c:\tmp\Hoge.txt")  
.       .Subscribe(   l  => Console.WriteLine("out:" + DateTime.Now.ToString() + " " + l)  
.                   , e  => Console.WriteLine("out:" + DateTime.Now.ToString() + " " + e)  
.                   , () => Console.WriteLine("Complete!!")  
.                 );  
.   
out:2016/08/16 8:19:06 HogeHoge  
out:2016/08/16 8:19:06 FugeFuge  
out:2016/08/16 8:19:06 HigeHige  
Complete!!  


## VS. 

## Requirement
.NET4.5（3.5以上で動作すると思いますが、未検証です）
PowerShell5.0（2.0でも動作すると思いますが、未検証なのとSystem.Mangement.Automation.dllの参照パスを変更してください）
ReactiveExtension2.0(以前のバージョンは未検証)
Windows10(他Windowsでも動くと思いますが未検証です)

## Usage

## Install

## Contribution

## Licence
