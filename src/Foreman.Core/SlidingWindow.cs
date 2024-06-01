namespace Foreman.Core;

// TODO: should this be here?

public abstract class SlidingWindow<TElement, TProduces> {
    private int _position;
    private int _offset;
    private int _head => _position + _offset;
    
    public SlidingWindow() {
        _position = 0;
        _offset = 0;
    }

    public TElement Peek(int offset) {
        int absolutePosition = _head + offset;
        return Get(absolutePosition);
    }

    public bool IsOffset => _offset > 0;

    public TElement Current() => Peek(0);

    public bool ShiftLeft(int delta) {
        _offset -= delta;
        if (_offset < 0) {
            _offset = 0;
            return false;
        }

        return true;
    }

    public void ShiftRight(int delta) {
        _offset += delta;
    }
    public TProduces Consume(int delta = 0) {
        ShiftRight(delta);

        int startPosition = _position;
        int endPosition = _head;

        _position = _head + 1;
        _offset = 0;

        return Build(startPosition, endPosition);
    }

    public void ResetWindow(int position = 0) {
        _position = position;
        _offset = 0;
    }

    public void ResetOffset() {
        _offset = 0;
    }

    public abstract TElement Get(int absolutePosition);
    public abstract TProduces Build(int rangeStart, int rangeEnd);
}
