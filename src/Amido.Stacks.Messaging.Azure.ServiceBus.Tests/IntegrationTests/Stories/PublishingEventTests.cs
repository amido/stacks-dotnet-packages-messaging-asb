using Amido.Stacks.Messaging.Azure.ServiceBus.Tests.IntegrationTests.Steps;
using AutoFixture.Xunit2;
using TestStack.BDDfy;
using Xunit;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Tests.IntegrationTests.Stories
{
    public class PublishingEventTests
    {
        [Trait("Type", "IApplicationEvent on Azure Service Bus")]
        [Trait("Category", "Validated pass")]
        [Theory(Skip = "due to missing queue used by pipeline to run tests")]
        //[Theory]
        [AutoData]
        public void PublishAndHandlingMessage(PublishEventFixtures fixture)
        {
            this.Given(_ => fixture.TheCorrectEventIsSentToTheTopic())
                .And(_ => fixture.TheTopicSenderHealthCheckPass())
                .And(_ => fixture.TheHostIsRunning())
                .And(_ => fixture.WaitFor3Seconds())
                .Then(_ => fixture.TheMessageIsHandledInTheHandler())
                .BDDfy();
        }
    }
}
