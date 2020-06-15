namespace Netx
{
    public interface IMethodController
    {
        INetxSClientBase? Current { get; set; }
    }

    public class MethodControllerBase : IMethodController
    {
        public INetxSClientBase? Current { get; set; }
    }
}
