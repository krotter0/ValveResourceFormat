using System.Linq;
using System.Windows.Forms;
using GUI.Types.Audio;
using ValveResourceFormat.IO;
using ValveResourceFormat.ResourceTypes;

namespace GUI.Controls;
public partial class SoundEventsList : UserControl
{
    private WorldAudioPlayer audioPlayer;
    private ISoundEvent activeSoundEvent;
    private SoundEventsListEntry lastPlayedEntry;

    public SoundEventsList(BinaryKV3 soundEvents, IFileLoader fileLoader)
    {
        Dock = DockStyle.Fill;
        InitializeComponent();

        var soundEventNames = soundEvents.Data.Select(x => x.Key);
        AddEntries(soundEventNames);

        audioPlayer = new(fileLoader);
        audioPlayer.LoadSoundEventsFile(soundEvents.Data);
        audioPlayer.Init();
        audioPlayer.Play();
    }

    public void AddEntries(IEnumerable<string> soundEventNames)
    {
        soundEntriesPanel.SuspendLayout();
        try
        {
            foreach (var item in soundEventNames)
            {
                var entry = CreateEntry(item);
                soundEntriesPanel.Controls.Add(entry);
            }
        }
        finally
        {
            soundEntriesPanel.ResumeLayout(true);
        }
    }

    public SoundEventsListEntry CreateEntry(string soundEventName)
    {
        var entry = new SoundEventsListEntry();
        entry.SetSoundEventName(soundEventName);
        entry.PlayClicked += OnEntryPlayClicked;
        return entry;
    }

    private void OnEntryPlayClicked(SoundEventsListEntry entry, string soundEventName)
    {
        var soundData = new SoundData();
        soundData.Position = new Vector3(10, 0, 0);
        soundData.Volume = 1f;

        var wasPlayingSound = entry.IsPlayingSound();

        if (activeSoundEvent != null)
        {
            activeSoundEvent.StopImmediately();
            activeSoundEvent.SoundEventOver -= OnSoundOver;
            activeSoundEvent = null;
            lastPlayedEntry?.SetPlayingSound(false);
        }

        if (!wasPlayingSound && audioPlayer != null)
        {
            lastPlayedEntry = entry;
            entry?.SetPlayingSound(true);
            activeSoundEvent = audioPlayer.PlaySound(0, soundData, soundEventName);
            activeSoundEvent.SoundEventOver += OnSoundOver;
        }
    }

    private void OnSoundOver()
    {
        SoundEventsListEntry entry = lastPlayedEntry;
        lastPlayedEntry?.Invoke(() =>
        {
            entry.SetPlayingSound(false);
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
            audioPlayer?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void searchTextBox_TextChanged(object sender, EventArgs e)
    {
        var searchText = searchTextBox.Text;
        var searchEmpty = string.IsNullOrEmpty(searchText);
        soundEntriesPanel.SuspendLayout();
        try
        {
            foreach (var listControl in soundEntriesPanel.Controls)
            {
                var soundEventEntry = listControl as SoundEventsListEntry;
                var soundEventName = soundEventEntry.GetSoundEventName();
                soundEventEntry.Visible = searchEmpty || soundEventName.Contains(searchText, StringComparison.InvariantCultureIgnoreCase);
            }
        }
        finally
        {
            soundEntriesPanel.ResumeLayout(true);
        }
    }
}
