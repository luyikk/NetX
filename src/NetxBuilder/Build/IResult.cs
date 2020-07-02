using System;

namespace Netx
{
    public interface IResultValue
    {
        byte[] Data { get; }
        T Value<T>();
    }

    public interface IResult
    {
        IResultValue this[int index] { get; }
        int ErrorId { get; set; }
        string ErrorMsg { get; set; }
        IResultValue First { get; }
        long Id { get; set; }
        bool IsError { get; }
        bool IsHaveValue { get; }
        int? Length { get; }
        object As(Type type, int index);
        T As<T>();
        T As<T>(int index);
    }
}