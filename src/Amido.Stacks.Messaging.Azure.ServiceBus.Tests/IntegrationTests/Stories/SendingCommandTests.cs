using Amido.Stacks.Messaging.Azure.ServiceBus.Tests.IntegrationTests.Steps;
using AutoFixture.Xunit2;
using TestStack.BDDfy;
using Xunit;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Tests.IntegrationTests.Stories
{
    public class SendingCommandTests
    {
        [Trait("Type ", "ICommand on Azure Service Bus")]
        [Trait("Category", "Validated pass")]
        [Theory(Skip = "due to missing queue used by pipeline to run tests")]
        //[Theory]
        [AutoData]
        public void SendingAndHandlingMessage(SendingCommandFixtures fixture)
        {
            this.Given(_ => fixture.TheCorrectCommandIsSentToTheQueue())
                .And(_ => fixture.TheQueueSenderHealthCheckPass())
                .And(_ => fixture.TheHostIsRunning())
                .And(_ => fixture.WaitFor3Seconds())
                .Then(_ => fixture.TheMessageIsHandledInTheHandler())
                .BDDfy();
        }
    }
}
