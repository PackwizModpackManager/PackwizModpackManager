using System.ComponentModel;

namespace PackwizModpackManager.Models;

public class Mod : INotifyPropertyChanged
{
    private string name;
    private int sideIndex;
    private string filePath;

    public string Name
    {
        get => name;
        set
        {
            if (name != value)
            {
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }

    public int SideIndex
    {
        get => sideIndex;
        set
        {
            if (sideIndex != value)
            {
                sideIndex = value;
                OnPropertyChanged(nameof(SideIndex));
                OnPropertyChanged(nameof(Side));
            }
        }
    }

    public string Side
    {
        get
        {
            return sideIndex switch
            {
                0 => "client",
                1 => "server",
                2 => "both",
                _ => "both"
            };
        }
        set
        {
            sideIndex = value switch
            {
                "client" => 0,
                "server" => 1,
                "both" => 2,
                _ => 2
            };
            OnPropertyChanged(nameof(SideIndex));
            OnPropertyChanged(nameof(Side));
        }
    }

    public string FilePath
    {
        get => filePath;
        set
        {
            if (filePath != value)
            {
                filePath = value;
                OnPropertyChanged(nameof(FilePath));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
