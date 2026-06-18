namespace Spirantis.Extensions.Logging.Tests;

public sealed class LogEntryEventTests
{
    private sealed class SampleEvent : LogEntryEvent<string>
    {
        public SampleEvent(string key)
            : base(key) { }

        public SampleEvent(string key, LogEntryEventType eventType)
            : base(key, eventType) { }
    }

    private sealed class GenericEvent<T> : LogEntryEvent<string>
    {
        public GenericEvent(string key)
            : base(key) { }
    }

    [Fact]
    public void EventType_DefaultsToInternal()
    {
        Assert.Equal(LogEntryEventType.Internal, new SampleEvent("k").EventType);
    }

    [Fact]
    public void EventType_CanBeSetViaConstructor()
    {
        Assert.Equal(
            LogEntryEventType.Business,
            new SampleEvent("k", LogEntryEventType.Business).EventType
        );
    }

    [Fact]
    public void EventName_IsTheConcreteTypeName()
    {
        Assert.Equal("SampleEvent", new SampleEvent("k").EventName);
    }

    [Fact]
    public void EventName_StripsGenericArity()
    {
        Assert.Equal("GenericEvent", new GenericEvent<int>("k").EventName);
    }

    [Fact]
    public void Key_IsStored()
    {
        Assert.Equal("order-1", new SampleEvent("order-1").Key);
    }

    [Fact]
    public void Message_DefaultsToEmpty()
    {
        Assert.Equal(string.Empty, new SampleEvent("k").Message);
    }
}
