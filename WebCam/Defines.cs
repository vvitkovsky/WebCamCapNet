namespace WebCam
{
    [Flags]
    public enum CodecType
    {
        Video = 1 << 1,
        Audio = 1 << 2,
        Subtitle = 1 << 3,
        Data = 1 << 4,
    }
}
