using System.Windows.Forms;
using GUI.Types.Audio;

namespace GUI.Controls;
partial class SoundEventsListEntry : UserControl
{
    public delegate void OnClickedDelegate(SoundEventsListEntry entry, string soundEventName);
    public event OnClickedDelegate PlayClicked;
    private bool isPlayingSound;

    public SoundEventsListEntry(string soundEventName, SoundEventCache soundEvents)
    {
        InitializeComponent();
        SetSoundEventName(soundEventName);
        var soundEvent = soundEvents.GetSoundEvent(soundEventName);
        if (soundEvent == null)
        {
            SetError("Soundevent was not loaded");
        }
        else if (!SoundEventCache.IsSoundEventTypeValid(soundEvent))
        {
            SetError("Soundevent type is not supported");
        }
    }

    public void SetSoundEventName(string soundEventName)
    {
        this.soundEventName.Text = soundEventName;
    }

    public void SetError(string errorText)
    {
        errorLabel.Text = errorText;
        playButton.Enabled = false;
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
