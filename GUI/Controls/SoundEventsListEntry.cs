using System.Windows.Forms;

namespace GUI.Controls;
public partial class SoundEventsListEntry : UserControl
{
    public delegate void OnClickedDelegate(SoundEventsListEntry entry, string soundEventName);
    public event OnClickedDelegate PlayClicked;
    private bool isPlayingSound;

    public SoundEventsListEntry()
    {
        InitializeComponent();
    }

    public void SetSoundEventName(string soundEventName)
    {
        this.soundEventName.Text = soundEventName;
    }

    public string GetSoundEventName()
    {
        return this.soundEventName.Text;
    }

    public void SetPlayingSound(bool playing)
    {
        this.playButton.Text = playing ? "Stop" : "Play";
        isPlayingSound = playing;
    }

    public bool IsPlayingSound()
    {
        return isPlayingSound;
    }

    private void playButton_Click(object sender, EventArgs e)
    {
        PlayClicked?.Invoke(this, soundEventName.Text);
    }
}
