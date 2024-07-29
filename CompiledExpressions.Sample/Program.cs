using System;
using CompiledExpressions.Sample;

var source = new ExampleObservable();
var target = new ExampleObservable
{
    Nested = new ExampleObservable()
};

using var relay = ObservableRelay.OneWay(
    source,
    x => x.Id,
    target,
    x => x.Nested!.Id
);

source.Id = 42;

Console.WriteLine(target.Nested!.Id);
