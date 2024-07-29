using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CompiledExpressions.Sample;

public class ExampleObservable : INotifyPropertyChanged
{
    private int id;
    private string? text;
    private ExampleObservable? nested;

    public int Id
    {
        get => id;
        set => SetField(ref id, value);
    }

    public string? Text
    {
        get => text;
        set => SetField(ref text, value);
    }
    
    public ExampleObservable? Nested
    {
        get => nested;
        set => SetField(ref nested, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
