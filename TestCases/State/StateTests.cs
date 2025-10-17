#region Example 1: Traffic Light
public interface ILightState
{
    void Handle(TrafficLightContext context);
}

public class RedState : ILightState
{
    public void Handle(TrafficLightContext context)
    {
        Console.WriteLine("Red → Switching to Green");
        context.SetState(new GreenState());
    }
}

public class GreenState : ILightState
{
    public void Handle(TrafficLightContext context)
    {
        Console.WriteLine("Green → Switching to Yellow");
        context.SetState(new YellowState());
    }
}

public class YellowState : ILightState
{
    public void Handle(TrafficLightContext context)
    {
        Console.WriteLine("Yellow → Switching to Red");
        context.SetState(new RedState());
    }
}

public class TrafficLightContext
{
    private ILightState _state;

    public TrafficLightContext(ILightState state) => _state = state;

    public void SetState(ILightState state) => _state = state;

    public void Request() => _state.Handle(this);
}
#endregion

#region Example 2: Media Player
public interface IPlayerState
{
    void Play(MediaPlayerContext context);
    void Pause(MediaPlayerContext context);
}

public class PlayingState : IPlayerState
{
    public void Play(MediaPlayerContext context) => Console.WriteLine("Already playing");
    public void Pause(MediaPlayerContext context)
    {
        Console.WriteLine("Pausing playback");
        context.SetState(new PausedState());
    }
}

public class PausedState : IPlayerState
{
    public void Play(MediaPlayerContext context)
    {
        Console.WriteLine("Resuming playback");
        context.SetState(new PlayingState());
    }
    public void Pause(MediaPlayerContext context) => Console.WriteLine("Already paused");
}

public class MediaPlayerContext
{
    private IPlayerState _state = new PausedState();

    public void SetState(IPlayerState state) => _state = state;

    public void Play() => _state.Play(this);
    public void Pause() => _state.Pause(this);
}
#endregion

#region Example 3: Document Workflow
public interface IDocumentState
{
    void Publish(DocumentContext context);
    void Reject(DocumentContext context);
}

public class DraftState : IDocumentState
{
    public void Publish(DocumentContext context)
    {
        Console.WriteLine("Draft → Submitted for Review");
        context.SetState(new ReviewState());
    }
    public void Reject(DocumentContext context) => Console.WriteLine("Draft cannot be rejected");
}

public class ReviewState : IDocumentState
{
    public void Publish(DocumentContext context)
    {
        Console.WriteLine("Review → Published");
        context.SetState(new PublishedState());
    }
    public void Reject(DocumentContext context)
    {
        Console.WriteLine("Review → Back to Draft");
        context.SetState(new DraftState());
    }
}

public class PublishedState : IDocumentState
{
    public void Publish(DocumentContext context) => Console.WriteLine("Already published");
    public void Reject(DocumentContext context) => Console.WriteLine("Published documents cannot be rejected");
}

public class DocumentContext
{
    private IDocumentState _state = new DraftState();

    public void SetState(IDocumentState state) => _state = state;

    public void Publish() => _state.Publish(this);
    public void Reject() => _state.Reject(this);
}
#endregion
