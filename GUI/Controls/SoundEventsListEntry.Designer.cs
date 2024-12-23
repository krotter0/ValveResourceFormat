namespace GUI.Controls;

partial class SoundEventsListEntry
{
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        playButton = new System.Windows.Forms.Button();
        soundEventName = new System.Windows.Forms.TextBox();
        SuspendLayout();
        // 
        // playButton
        // 
        playButton.Font = new System.Drawing.Font("Segoe UI", 8F);
        playButton.Location = new System.Drawing.Point(3, 3);
        playButton.Margin = new System.Windows.Forms.Padding(1);
        playButton.Name = "playButton";
        playButton.Size = new System.Drawing.Size(38, 19);
        playButton.TabIndex = 0;
        playButton.Text = "Play";
        playButton.UseVisualStyleBackColor = true;
        playButton.Click += playButton_Click;
        // 
        // soundEventName
        // 
        soundEventName.BorderStyle = System.Windows.Forms.BorderStyle.None;
        soundEventName.Location = new System.Drawing.Point(45, 5);
        soundEventName.Name = "soundEventName";
        soundEventName.ReadOnly = true;
        soundEventName.Size = new System.Drawing.Size(336, 16);
        soundEventName.TabIndex = 2;
        soundEventName.TabStop = false;
        soundEventName.Text = "soundEventName";
        // 
        // SoundEventsListEntry
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        Controls.Add(soundEventName);
        Controls.Add(playButton);
        Name = "SoundEventsListEntry";
        Size = new System.Drawing.Size(504, 25);
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private System.Windows.Forms.Button playButton;
    private System.Windows.Forms.TextBox soundEventName;
}
