namespace Amido.Stacks.Messaging.Azure.ServiceBus.Exceptions
{
    public enum ExceptionIds
    {
        InvalidCast = 99900100,
        Serialization = 99900101,
        MissingEnclosedMessageType = 99900102,
        MissingHandlerFromIoC = 99900104,
        MissingHandlerFromAssembly = 99900105,
        MissingHandlerMethod = 99900106,
        MoreThanOneHandler = 9990107,
        InvalidMessageBody = 9990108,
        MessageBodyIsNull = 9990109,
        MessageRouteNotDefined = 9990110,
        MessageSenderNotDefined = 9990111,
        MissingQueueConfiguration = 9990112
    }
}
