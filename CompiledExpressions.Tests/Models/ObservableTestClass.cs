using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CompiledExpressions.Tests.Models;

public class ObservableTestClass : INotifyPropertyChanged
{
    private int _id;
    private string? _text;
    private TestClass? _nested;

    public int Id
    {
        get => _id;
        set => SetField(ref _id, value);
    }

    public string? Text
    {
        get => _text;
        set => SetField(ref _text, value);
    }

    public TestClass? Nested 
    {
        get => _nested;
        set => SetField(ref _nested, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
