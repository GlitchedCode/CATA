namespace Simulation;

using System.Text;

public class ConfigurationKey
{
    byte[] _bytes;
    public byte[] Bytes
    {
        get => _bytes;
        set
        {
            _bytes = value;
            var memoryStream = new MemoryStream(value);
            var sr = new StreamReader(memoryStream, Encoding.ASCII, false);
            _string = sr.ReadLine();
        }
    }

    string _string;
    public string String
    {
        get => _string;
        set
        {
            _string = value;
            _bytes = Encoding.ASCII.GetBytes(value);
        }
    }

    public ConfigurationKey(string keyString)
    {
        if (keyString == null) keyString = "";
        this.String = keyString;
    }

    public ConfigurationKey(byte[] bytes = null)
    {
        if (bytes == null) bytes = new byte[] { };
        this.Bytes = bytes;
    }

    public override int GetHashCode() => String.GetHashCode();

    public override bool Equals(object obj)
    {
        if (!(obj is ConfigurationKey)) return false;
        var other = (ConfigurationKey)obj;

        return String == other.String;
    }

    public override string ToString()
    {
        return String;
    }
}
