namespace neuneu9.WindowPanel
{
    /// <summary>
    /// フェードイン・アウトのウインドウパネル
    /// </summary>
    public class FadeWindowPanel : WindowPanel
    {
        protected override void OpenAction(float progress)
        {
            _window.alpha = progress;
        }

        protected override void CloseAction(float progress)
        {
            _window.alpha = 1f - progress;
        }
    }
}
