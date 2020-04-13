using System;
using System.Runtime.Serialization;

[Serializable]
internal class YourCustomException : Exception
{
    private string v;
    private object e;

    public YourCustomException()
    {
    }

    public YourCustomException(string message) : base(message)
    {
    }

    public YourCustomException(string v, object e)
    {
        this.v = v;
        this.e = e;
    }

    public YourCustomException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected YourCustomException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}