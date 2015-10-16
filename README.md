## Brunch Task Runner extension

Adds support for the Brunch build tool in Visual Studio 2015's
Task Runner Explorer.

[![Build status](https://ci.appveyor.com/api/projects/status/3x24c3gbyv2g34l8?svg=true)](https://ci.appveyor.com/project/madskristensen/brunchtaskrunner)

Download the extension at the
[VS Gallery](https://visualstudiogallery.msdn.microsoft.com/3b329021-cd7a-4a01-86fc-714c2d05bb6c)
or get the
[nightly build](http://vsixgallery.com/extension/b4a4ad37-5a4b-4dfd-85fd-595cab6a26a9/)

### Execute tasks

Task Runner Explorer will show both _build_ and _watch_
tasks as well as any overrides file present in the working
directory.

![Task List](art/task-list.png)

Each task can be executed by double-clicking the task.

### Bindings

Task bindings make it possible to associate individual scripts
with Visual Studio events such as _Project Open_ etc.

![Bindings](art/bindings.png)



