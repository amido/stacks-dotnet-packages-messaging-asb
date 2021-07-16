namespace Amido.Stacks.Messaging.Handlers.TestDependency
{
    public interface ITestable<T>
    {
        void Complete(T operationContext);
    }
}
